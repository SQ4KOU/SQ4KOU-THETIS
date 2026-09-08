using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.Scripting;

public abstract class ScriptState
{
	private ImmutableArray<ScriptVariable> _lazyVariables;

	private IReadOnlyDictionary<string, int> _lazyVariableMap;

	public Script Script { get; }

	public Exception Exception { get; }

	internal ScriptExecutionState ExecutionState { get; }

	public object ReturnValue => GetReturnValue();

	public ImmutableArray<ScriptVariable> Variables
	{
		get
		{
			if (_lazyVariables == null)
			{
				ImmutableInterlocked.InterlockedInitialize(ref _lazyVariables, CreateVariables());
			}
			return _lazyVariables;
		}
	}

	internal ScriptState(ScriptExecutionState executionState, Script script, Exception exceptionOpt)
	{
		ExecutionState = executionState;
		Script = script;
		Exception = exceptionOpt;
	}

	internal abstract object GetReturnValue();

	public ScriptVariable GetVariable(string name)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (!GetVariableMap().TryGetValue(name, out var value))
		{
			return null;
		}
		return Variables[value];
	}

	private ImmutableArray<ScriptVariable> CreateVariables()
	{
		ArrayBuilder<ScriptVariable> instance = ArrayBuilder<ScriptVariable>.GetInstance();
		ScriptExecutionState executionState = ExecutionState;
		for (int i = 1; i < executionState.SubmissionStateCount; i++)
		{
			object submissionState = executionState.GetSubmissionState(i);
			foreach (FieldInfo declaredField in submissionState.GetType().GetTypeInfo().DeclaredFields)
			{
				if (declaredField.IsPublic && declaredField.Name.Length > 0 && (char.IsLetterOrDigit(declaredField.Name[0]) || declaredField.Name[0] == '_'))
				{
					instance.Add(new ScriptVariable(submissionState, declaredField));
				}
			}
		}
		return instance.ToImmutableAndFree();
	}

	private IReadOnlyDictionary<string, int> GetVariableMap()
	{
		if (_lazyVariableMap == null)
		{
			Dictionary<string, int> dictionary = new Dictionary<string, int>(Script.Compiler.IdentifierComparer);
			for (int i = 0; i < Variables.Length; i++)
			{
				dictionary[Variables[i].Name] = i;
			}
			_lazyVariableMap = dictionary;
		}
		return _lazyVariableMap;
	}

	public Task<ScriptState<object>> ContinueWithAsync(string code, ScriptOptions options, CancellationToken cancellationToken)
	{
		return ContinueWithAsync<object>(code, options, null, cancellationToken);
	}

	public Task<ScriptState<object>> ContinueWithAsync(string code, ScriptOptions options = null, Func<Exception, bool> catchException = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		return Script.ContinueWith<object>(code, options).RunFromAsync(this, catchException, cancellationToken);
	}

	public Task<ScriptState<TResult>> ContinueWithAsync<TResult>(string code, ScriptOptions options, CancellationToken cancellationToken)
	{
		return ContinueWithAsync<TResult>(code, options, null, cancellationToken);
	}

	public Task<ScriptState<TResult>> ContinueWithAsync<TResult>(string code, ScriptOptions options = null, Func<Exception, bool> catchException = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		return Script.ContinueWith<TResult>(code, options).RunFromAsync(this, catchException, cancellationToken);
	}
}
public sealed class ScriptState<T> : ScriptState
{
	public new T ReturnValue { get; }

	internal override object GetReturnValue()
	{
		return ReturnValue;
	}

	internal ScriptState(ScriptExecutionState executionState, Script script, T value, Exception exceptionOpt)
		: base(executionState, script, exceptionOpt)
	{
		ReturnValue = value;
	}
}
