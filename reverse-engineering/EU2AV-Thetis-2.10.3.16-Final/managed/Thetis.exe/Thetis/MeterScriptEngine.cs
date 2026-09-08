using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Scripting;

namespace Thetis;

public static class MeterScriptEngine
{
	public sealed class BankVars
	{
		private readonly Dictionary<string, object> _bank;

		private readonly Dictionary<string, object> _common;

		public dynamic this[string k]
		{
			get
			{
				if (_bank != null && _bank.ContainsKey(k))
				{
					return _bank[k];
				}
				if (_common != null && _common.ContainsKey(k))
				{
					return _common[k];
				}
				throw new KeyNotFoundException("Variable '" + k + "' not found");
			}
		}

		public BankVars(Dictionary<string, object> bank, Dictionary<string, object> common)
		{
			_bank = bank;
			_common = common;
		}
	}

	public sealed class Snapshot
	{
		public Dictionary<string, object> Common;

		public Dictionary<string, object>[] Banks;
	}

	public class Globals
	{
		public BankVars[] Variables;
	}

	private sealed class CompileResult
	{
		public ScriptRunner<bool> Runner;

		public string Diagnostics;

		public bool Success;
	}

	private struct EvalResult
	{
		public int Index;

		public bool Value;

		public bool Error;

		public string Diagnostic;
	}

	private static readonly object _lock = new object();

	private static readonly ScriptOptions _script_options = ScriptOptions.Default.AddReferences(typeof(object).Assembly).AddReferences(typeof(Dictionary<string, object>).Assembly).AddReferences(typeof(Binder).Assembly)
		.AddReferences(typeof(Globals).Assembly)
		.AddImports("System")
		.AddImports("System.Collections.Generic")
		.AddImports("System.Linq")
		.AddImports("Microsoft.CSharp");

	private static List<string> _conditions = new List<string>();

	private static List<bool> _occupied = new List<bool>();

	private static List<int> _update_intervals_ms = new List<int>();

	private static List<long> _next_due_ticks = new List<long>();

	private static List<bool> _results = new List<bool>();

	private static List<bool> _errors = new List<bool>();

	private static List<string> _diagnostics = new List<string>();

	private static Queue<int> _free_indices = new Queue<int>();

	private static List<ScriptRunner<bool>> _delegates = new List<ScriptRunner<bool>>();

	private static List<bool> _needs_compile = new List<bool>();

	private static Dictionary<string, ScriptRunner<bool>> _delegate_cache = new Dictionary<string, ScriptRunner<bool>>();

	private static bool _needs_recompile = false;

	private static Timer _timer = null;

	private static int _default_interval_ms = 100;

	private static int _loop_interval_ms = 100;

	private static int _bank_count = 1;

	private static Func<Snapshot> _variable_provider_banked = null;

	private static readonly long _ticks_per_millisecond = 10000L;

	private static int _batch_depth = 0;

	private static bool _loop_interval_dirty = false;

	private static Thread _compile_thread = null;

	private static AutoResetEvent _compile_event = new AutoResetEvent(initialState: false);

	private static bool _stopping = false;

	private static int _compile_debounce_ms = 25;

	private static int _tick_in_progress = 0;

	public static bool IsInBatch
	{
		get
		{
			lock (_lock)
			{
				return _batch_depth > 0;
			}
		}
	}

	public static void Start(Func<Snapshot> variable_provider_banked, int default_interval_ms, int bank_count)
	{
		lock (_lock)
		{
			_variable_provider_banked = variable_provider_banked;
			_bank_count = ((bank_count < 1) ? 1 : bank_count);
			_default_interval_ms = ((default_interval_ms < 1) ? 1 : default_interval_ms);
			_loop_interval_ms = _default_interval_ms;
			_stopping = false;
			if (_timer == null)
			{
				_timer = new Timer(delegate
				{
					tick();
				}, null, _loop_interval_ms, _loop_interval_ms);
			}
			else
			{
				_timer.Change(_loop_interval_ms, _loop_interval_ms);
			}
			if (_compile_thread == null || !_compile_thread.IsAlive)
			{
				_compile_thread = new Thread(compile_worker_entry);
				_compile_thread.IsBackground = true;
				_compile_thread.Priority = ThreadPriority.BelowNormal;
				_compile_thread.Name = "MeterScriptEngine-Compiler";
				_compile_thread.Start();
			}
		}
		Thread thread = new Thread(warmup_roslyn_entry);
		thread.IsBackground = true;
		thread.Priority = ThreadPriority.BelowNormal;
		thread.Name = "MeterScriptEngine-Warmup";
		thread.Start();
	}

	public static void Stop()
	{
		Timer timer = null;
		lock (_lock)
		{
			_stopping = true;
			_compile_event.Set();
			timer = _timer;
			_timer = null;
			timer?.Change(-1, -1);
		}
		timer?.Dispose();
	}

	public static void BeginBatch()
	{
		lock (_lock)
		{
			_batch_depth++;
		}
	}

	public static void EndBatch()
	{
		bool flag = false;
		bool flag2 = false;
		lock (_lock)
		{
			if (_batch_depth > 0)
			{
				_batch_depth--;
			}
			if (_batch_depth == 0)
			{
				if (_needs_recompile)
				{
					flag2 = true;
				}
				if (_loop_interval_dirty)
				{
					flag = true;
				}
				_loop_interval_dirty = false;
			}
		}
		if (flag)
		{
			lock (_lock)
			{
				recompute_loop_interval_nolock();
			}
		}
		if (flag2)
		{
			lock (_lock)
			{
				schedule_compile_if_needed_nolock();
			}
		}
	}

	public static int RegisterLed()
	{
		lock (_lock)
		{
			int num;
			if (_free_indices.Count > 0)
			{
				num = _free_indices.Dequeue();
				_occupied[num] = true;
				_conditions[num] = string.Empty;
				_update_intervals_ms[num] = _default_interval_ms;
				_next_due_ticks[num] = 0L;
				_results[num] = false;
				_errors[num] = false;
				_diagnostics[num] = string.Empty;
				_delegates[num] = null;
				_needs_compile[num] = false;
			}
			else
			{
				num = _conditions.Count;
				_conditions.Add(string.Empty);
				_occupied.Add(item: true);
				_update_intervals_ms.Add(_default_interval_ms);
				_next_due_ticks.Add(0L);
				_results.Add(item: false);
				_errors.Add(item: false);
				_diagnostics.Add(string.Empty);
				_delegates.Add(null);
				_needs_compile.Add(item: false);
			}
			_loop_interval_dirty = true;
			if (_batch_depth == 0)
			{
				recompute_loop_interval_nolock();
			}
			return num;
		}
	}

	public static void UnregisterLed(int index)
	{
		lock (_lock)
		{
			if (index >= 0 && index < _occupied.Count && _occupied[index])
			{
				_occupied[index] = false;
				_conditions[index] = string.Empty;
				_results[index] = false;
				_errors[index] = false;
				_diagnostics[index] = string.Empty;
				_update_intervals_ms[index] = _default_interval_ms;
				_next_due_ticks[index] = 0L;
				_delegates[index] = null;
				_needs_compile[index] = false;
				_free_indices.Enqueue(index);
				_loop_interval_dirty = true;
				if (_batch_depth == 0)
				{
					recompute_loop_interval_nolock();
				}
			}
		}
	}

	public static bool SetCondition(int index, string condition)
	{
		if (index < 0 || index >= _occupied.Count)
		{
			return false;
		}
		if (!_occupied[index])
		{
			return false;
		}
		string text = condition ?? string.Empty;
		string text2 = text.Trim();
		if (text2.Length == 0)
		{
			lock (_lock)
			{
				_conditions[index] = string.Empty;
				_delegates[index] = null;
				_needs_compile[index] = false;
				_errors[index] = false;
				_diagnostics[index] = string.Empty;
			}
			return true;
		}
		ExpressionSyntax expressionSyntax = SyntaxFactory.ParseExpression(text2);
		bool flag = false;
		StringBuilder stringBuilder = new StringBuilder();
		foreach (Diagnostic diagnostic in expressionSyntax.GetDiagnostics())
		{
			if (diagnostic.Severity == DiagnosticSeverity.Error)
			{
				flag = true;
				stringBuilder.AppendLine(diagnostic.ToString());
			}
		}
		if (flag)
		{
			lock (_lock)
			{
				_conditions[index] = text;
				_delegates[index] = null;
				_needs_compile[index] = false;
				_errors[index] = true;
				_diagnostics[index] = stringBuilder.ToString();
			}
			return false;
		}
		lock (_lock)
		{
			_conditions[index] = text;
			_diagnostics[index] = string.Empty;
			_errors[index] = false;
			_needs_compile[index] = true;
			_needs_recompile = true;
			if (_batch_depth == 0)
			{
				schedule_compile_if_needed_nolock();
			}
		}
		return true;
	}

	public static void SetUpdateInterval(int index, int milliseconds)
	{
		lock (_lock)
		{
			if (index >= 0 && index < _occupied.Count && _occupied[index])
			{
				int value = ((milliseconds < 1) ? 1 : milliseconds);
				_update_intervals_ms[index] = value;
				_loop_interval_dirty = true;
				if (_batch_depth == 0)
				{
					recompute_loop_interval_nolock();
				}
			}
		}
	}

	public static bool ReadResult(int index)
	{
		lock (_lock)
		{
			if (index < 0 || index >= _occupied.Count)
			{
				return false;
			}
			if (!_occupied[index])
			{
				return false;
			}
			return _results[index];
		}
	}

	public static bool ReadError(int index)
	{
		lock (_lock)
		{
			if (index < 0 || index >= _occupied.Count)
			{
				return true;
			}
			if (!_occupied[index])
			{
				return true;
			}
			return _errors[index];
		}
	}

	public static string ReadDiagnostic(int index)
	{
		lock (_lock)
		{
			if (index < 0 || index >= _occupied.Count)
			{
				return string.Empty;
			}
			if (!_occupied[index])
			{
				return string.Empty;
			}
			return _diagnostics[index] ?? string.Empty;
		}
	}

	public static void SetLoopIntervalFloor(int milliseconds)
	{
		lock (_lock)
		{
			int num = ((milliseconds < 1) ? 1 : milliseconds);
			if (num > _loop_interval_ms)
			{
				_loop_interval_ms = num;
			}
			if (_timer != null)
			{
				_timer.Change(_loop_interval_ms, _loop_interval_ms);
			}
		}
	}

	private static void tick()
	{
		if (Interlocked.CompareExchange(ref _tick_in_progress, 1, 0) != 0)
		{
			return;
		}
		try
		{
			if (Volatile.Read(ref _stopping))
			{
				return;
			}
			List<int> list = null;
			List<ScriptRunner<bool>> list2 = null;
			lock (_lock)
			{
				if (_needs_recompile && _batch_depth == 0)
				{
					schedule_compile_if_needed_nolock();
				}
				list = get_due_indices_nolock();
				int count = list.Count;
				list2 = new List<ScriptRunner<bool>>(count);
				for (int i = 0; i < count; i++)
				{
					int index = list[i];
					ScriptRunner<bool> item = _delegates[index];
					list2.Add(item);
				}
			}
			if (list == null || list.Count == 0)
			{
				return;
			}
			Globals globals = build_globals_once();
			List<EvalResult> list3 = new List<EvalResult>(list.Count);
			for (int j = 0; j < list.Count; j++)
			{
				int index2 = list[j];
				ScriptRunner<bool> scriptRunner = list2[j];
				if (scriptRunner == null)
				{
					list3.Add(new EvalResult
					{
						Index = index2,
						Value = false,
						Error = false,
						Diagnostic = string.Empty
					});
					continue;
				}
				try
				{
					bool result = scriptRunner(globals).GetAwaiter().GetResult();
					list3.Add(new EvalResult
					{
						Index = index2,
						Value = result,
						Error = false,
						Diagnostic = string.Empty
					});
				}
				catch (Exception ex)
				{
					list3.Add(new EvalResult
					{
						Index = index2,
						Value = false,
						Error = true,
						Diagnostic = (ex.Message ?? string.Empty)
					});
				}
			}
			long ticks = DateTime.UtcNow.Ticks;
			lock (_lock)
			{
				for (int k = 0; k < list3.Count; k++)
				{
					EvalResult evalResult = list3[k];
					if (evalResult.Index >= 0 && evalResult.Index < _occupied.Count && _occupied[evalResult.Index])
					{
						_results[evalResult.Index] = evalResult.Value;
						if (evalResult.Error)
						{
							_errors[evalResult.Index] = true;
							_diagnostics[evalResult.Index] = evalResult.Diagnostic;
						}
						long num = _update_intervals_ms[evalResult.Index] * _ticks_per_millisecond;
						_next_due_ticks[evalResult.Index] = ticks + num;
					}
				}
			}
		}
		finally
		{
			Interlocked.Exchange(ref _tick_in_progress, 0);
		}
	}

	private static Globals build_globals_once()
	{
		Snapshot snapshot = ((_variable_provider_banked != null) ? _variable_provider_banked() : null);
		if (snapshot == null)
		{
			snapshot = new Snapshot
			{
				Common = new Dictionary<string, object>(),
				Banks = new Dictionary<string, object>[_bank_count]
			};
		}
		if (snapshot.Common == null)
		{
			snapshot.Common = new Dictionary<string, object>();
		}
		if (snapshot.Banks == null || snapshot.Banks.Length != _bank_count)
		{
			snapshot.Banks = new Dictionary<string, object>[_bank_count];
		}
		BankVars[] array = new BankVars[_bank_count];
		for (int i = 0; i < _bank_count; i++)
		{
			Dictionary<string, object> bank = snapshot.Banks[i] ?? new Dictionary<string, object>();
			array[i] = new BankVars(bank, snapshot.Common);
		}
		return new Globals
		{
			Variables = array
		};
	}

	private static List<int> get_due_indices_nolock()
	{
		long ticks = DateTime.UtcNow.Ticks;
		int count = _occupied.Count;
		List<int> list = new List<int>(count);
		for (int i = 0; i < count; i++)
		{
			if (_occupied[i] && _next_due_ticks[i] <= ticks)
			{
				list.Add(i);
			}
		}
		return list;
	}

	private static void recompute_loop_interval_nolock()
	{
		int num = int.MaxValue;
		int count = _occupied.Count;
		for (int i = 0; i < count; i++)
		{
			if (_occupied[i])
			{
				int num2 = _update_intervals_ms[i];
				if (num2 < num)
				{
					num = num2;
				}
			}
		}
		if (num == int.MaxValue)
		{
			num = _default_interval_ms;
		}
		if (num < 1)
		{
			num = 1;
		}
		_loop_interval_ms = num;
		if (_timer != null)
		{
			_timer.Change(_loop_interval_ms, _loop_interval_ms);
		}
	}

	private static void schedule_compile_if_needed_nolock()
	{
		if (!_stopping && _batch_depth <= 0)
		{
			_compile_event.Set();
		}
	}

	private static void compile_worker_entry()
	{
		while (true)
		{
			_compile_event.WaitOne();
			if (_stopping)
			{
				break;
			}
			if (_compile_debounce_ms > 0)
			{
				Thread.Sleep(_compile_debounce_ms);
			}
			while (true)
			{
				int[] array;
				string[] array2;
				lock (_lock)
				{
					if (_stopping)
					{
						return;
					}
					if (_batch_depth > 0)
					{
						break;
					}
					List<int> list = new List<int>();
					List<string> list2 = new List<string>();
					int count = _conditions.Count;
					for (int i = 0; i < count; i++)
					{
						if (_occupied[i] && _needs_compile[i])
						{
							list.Add(i);
							list2.Add(_conditions[i]);
							_needs_compile[i] = false;
						}
					}
					if (list.Count == 0)
					{
						_needs_recompile = false;
						break;
					}
					array = list.ToArray();
					array2 = list2.ToArray();
					goto IL_00ed;
				}
				IL_00ed:
				for (int j = 0; j < array.Length; j++)
				{
					int num = array[j];
					string text = array2[j] ?? string.Empty;
					string text2 = text.Trim();
					if (text2.Length == 0)
					{
						lock (_lock)
						{
							if (num >= 0 && num < _delegates.Count && _occupied[num] && _conditions[num] == text)
							{
								_delegates[num] = null;
								_errors[num] = false;
								_diagnostics[num] = string.Empty;
							}
						}
						continue;
					}
					bool flag = false;
					ScriptRunner<bool> value;
					lock (_lock)
					{
						flag = _delegate_cache.TryGetValue(text2, out value);
					}
					if (flag && value != null)
					{
						lock (_lock)
						{
							if (_occupied[num] && _conditions[num] == text)
							{
								_delegates[num] = value;
								_errors[num] = false;
								_diagnostics[num] = string.Empty;
							}
						}
						continue;
					}
					CompileResult compileResult = compile_one(text2);
					lock (_lock)
					{
						if (!_occupied[num])
						{
							continue;
						}
						if (_conditions[num] != text)
						{
							_needs_compile[num] = true;
							_needs_recompile = true;
						}
						else if (compileResult.Success && compileResult.Runner != null)
						{
							_delegates[num] = compileResult.Runner;
							if (!_delegate_cache.ContainsKey(text2))
							{
								_delegate_cache[text2] = compileResult.Runner;
							}
							_errors[num] = false;
							_diagnostics[num] = string.Empty;
						}
						else
						{
							_errors[num] = true;
							_diagnostics[num] = compileResult.Diagnostics ?? string.Empty;
						}
					}
				}
			}
		}
	}

	private static CompileResult compile_one(string expr_trimmed)
	{
		Script<bool> script = CSharpScript.Create<bool>("return (bool)(" + expr_trimmed + ");", _script_options, typeof(Globals));
		ImmutableArray<Diagnostic> immutableArray = script.Compile();
		bool flag = false;
		StringBuilder stringBuilder = new StringBuilder();
		foreach (Diagnostic item in immutableArray)
		{
			if (item.Severity == DiagnosticSeverity.Error)
			{
				flag = true;
				stringBuilder.AppendLine(item.ToString());
			}
		}
		if (flag)
		{
			return new CompileResult
			{
				Runner = null,
				Diagnostics = stringBuilder.ToString(),
				Success = false
			};
		}
		try
		{
			ScriptRunner<bool> runner = script.CreateDelegate();
			return new CompileResult
			{
				Runner = runner,
				Diagnostics = string.Empty,
				Success = true
			};
		}
		catch (CompilationErrorException ex)
		{
			string diagnostics = string.Join(Environment.NewLine, ex.Diagnostics.Select((Diagnostic x) => x.ToString()));
			return new CompileResult
			{
				Runner = null,
				Diagnostics = diagnostics,
				Success = false
			};
		}
	}

	private static void warmup_roslyn_entry()
	{
		try
		{
			Script<bool> script = CSharpScript.Create<bool>("return true;", _script_options, typeof(Globals));
			script.Compile();
			script.CreateDelegate()(new Globals
			{
				Variables = new BankVars[0]
			}).GetAwaiter().GetResult();
		}
		catch
		{
		}
	}
}
