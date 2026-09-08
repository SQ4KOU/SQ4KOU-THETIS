using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis;

[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
[DebuggerTypeProxy(typeof(DebuggerProxy))]
internal class DiagnosticBag
{
	internal sealed class DebuggerProxy
	{
		private readonly DiagnosticBag _bag;

		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public object[] Diagnostics
		{
			get
			{
				ConcurrentQueue<Diagnostic> lazyBag = _bag._lazyBag;
				if (lazyBag != null)
				{
					return lazyBag.ToArray();
				}
				return Array.Empty<object>();
			}
		}

		public DebuggerProxy(DiagnosticBag bag)
		{
			_bag = bag;
		}
	}

	private ConcurrentQueue<Diagnostic>? _lazyBag;

	private static readonly ObjectPool<DiagnosticBag> s_poolInstance = CreatePool(128);

	public bool IsEmptyWithoutResolution => _lazyBag?.IsEmpty ?? true;

	internal int Count => _lazyBag?.Count ?? 0;

	private ConcurrentQueue<Diagnostic> Bag
	{
		get
		{
			ConcurrentQueue<Diagnostic> lazyBag = _lazyBag;
			if (lazyBag != null)
			{
				return lazyBag;
			}
			ConcurrentQueue<Diagnostic> concurrentQueue = new ConcurrentQueue<Diagnostic>();
			return Interlocked.CompareExchange(ref _lazyBag, concurrentQueue, null) ?? concurrentQueue;
		}
	}

	public bool HasAnyErrors()
	{
		if (IsEmptyWithoutResolution)
		{
			return false;
		}
		foreach (Diagnostic item in Bag)
		{
			if (item.DefaultSeverity == DiagnosticSeverity.Error)
			{
				return true;
			}
		}
		return false;
	}

	internal bool HasAnyResolvedErrors()
	{
		if (IsEmptyWithoutResolution)
		{
			return false;
		}
		foreach (Diagnostic item in Bag)
		{
			DiagnosticWithInfo obj = item as DiagnosticWithInfo;
			if ((obj == null || !obj.HasLazyInfo) && item.DefaultSeverity == DiagnosticSeverity.Error)
			{
				return true;
			}
		}
		return false;
	}

	public void Add(Diagnostic diag)
	{
		Bag.Enqueue(diag);
	}

	public void AddRange<T>(ImmutableArray<T> diagnostics) where T : Diagnostic
	{
		if (!diagnostics.IsDefaultOrEmpty)
		{
			ConcurrentQueue<Diagnostic> bag = Bag;
			for (int i = 0; i < diagnostics.Length; i++)
			{
				bag.Enqueue(diagnostics[i]);
			}
		}
	}

	public void AddRange(IEnumerable<Diagnostic> diagnostics)
	{
		foreach (Diagnostic diagnostic in diagnostics)
		{
			Bag.Enqueue(diagnostic);
		}
	}

	public void AddRange(DiagnosticBag bag)
	{
		if (!bag.IsEmptyWithoutResolution)
		{
			AddRange(bag.Bag);
		}
	}

	public void AddRangeAndFree(DiagnosticBag bag)
	{
		AddRange(bag);
		bag.Free();
	}

	public ImmutableArray<TDiagnostic> ToReadOnlyAndFree<TDiagnostic>(bool forceResolution = true) where TDiagnostic : Diagnostic
	{
		ConcurrentQueue<Diagnostic>? lazyBag = _lazyBag;
		Free();
		return ToReadOnlyCore<TDiagnostic>(lazyBag, forceResolution);
	}

	public ImmutableArray<Diagnostic> ToReadOnlyAndFree(bool forceResolution = true)
	{
		return ToReadOnlyAndFree<Diagnostic>(forceResolution);
	}

	public ImmutableArray<TDiagnostic> ToReadOnly<TDiagnostic>(bool forceResolution = true) where TDiagnostic : Diagnostic
	{
		return ToReadOnlyCore<TDiagnostic>(_lazyBag, forceResolution);
	}

	public ImmutableArray<Diagnostic> ToReadOnly(bool forceResolution = true)
	{
		return ToReadOnly<Diagnostic>(forceResolution);
	}

	private static ImmutableArray<TDiagnostic> ToReadOnlyCore<TDiagnostic>(ConcurrentQueue<Diagnostic>? oldBag, bool forceResolution) where TDiagnostic : Diagnostic
	{
		if (oldBag == null)
		{
			return ImmutableArray<TDiagnostic>.Empty;
		}
		ArrayBuilder<TDiagnostic> instance = ArrayBuilder<TDiagnostic>.GetInstance();
		foreach (TDiagnostic item in oldBag)
		{
			if (forceResolution)
			{
				if (item.Severity != (DiagnosticSeverity)(-2))
				{
					instance.Add(item);
				}
			}
			else
			{
				instance.Add(item);
			}
		}
		return instance.ToImmutableAndFree();
	}

	public IEnumerable<Diagnostic> AsEnumerable()
	{
		ConcurrentQueue<Diagnostic> bag = Bag;
		bool flag = false;
		foreach (Diagnostic item in bag)
		{
			if (item.Severity == (DiagnosticSeverity)(-2))
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			return bag;
		}
		return AsEnumerableFiltered();
	}

	private IEnumerable<Diagnostic> AsEnumerableFiltered()
	{
		foreach (Diagnostic item in Bag)
		{
			if (item.Severity != (DiagnosticSeverity)(-2))
			{
				yield return item;
			}
		}
	}

	internal IEnumerable<Diagnostic> AsEnumerableWithoutResolution()
	{
		IEnumerable<Diagnostic> lazyBag = _lazyBag;
		return lazyBag ?? SpecializedCollections.EmptyEnumerable<Diagnostic>();
	}

	public override string ToString()
	{
		if (IsEmptyWithoutResolution)
		{
			return "<no errors>";
		}
		StringBuilder stringBuilder = new StringBuilder();
		foreach (Diagnostic item in Bag)
		{
			stringBuilder.AppendLine(item.ToString());
		}
		return stringBuilder.ToString();
	}

	internal void Clear()
	{
		if (_lazyBag != null)
		{
			_lazyBag = null;
		}
	}

	internal static DiagnosticBag GetInstance()
	{
		return s_poolInstance.Allocate();
	}

	internal void Free()
	{
		Clear();
		s_poolInstance.Free(this);
	}

	private static ObjectPool<DiagnosticBag> CreatePool(int size)
	{
		return new ObjectPool<DiagnosticBag>(() => new DiagnosticBag(), size);
	}

	private string GetDebuggerDisplay()
	{
		return "Count = " + (_lazyBag?.Count ?? 0);
	}
}
