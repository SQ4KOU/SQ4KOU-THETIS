using System;
using System.Collections.Immutable;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.CodeAnalysis.Scripting;

internal sealed class ScriptExecutionState
{
	private object[] _submissionStates;

	private int _frozen;

	public int SubmissionStateCount { get; private set; }

	private ScriptExecutionState(object[] submissionStates, int count)
	{
		_submissionStates = submissionStates;
		SubmissionStateCount = count;
	}

	public static ScriptExecutionState Create(object globals)
	{
		return new ScriptExecutionState(new object[2] { globals, null }, 1);
	}

	public ScriptExecutionState FreezeAndClone()
	{
		if (Interlocked.CompareExchange(ref _frozen, 1, 0) == 1)
		{
			object[] array = new object[SubmissionStateCount];
			Array.Copy(_submissionStates, array, SubmissionStateCount);
			return new ScriptExecutionState(array, SubmissionStateCount);
		}
		return new ScriptExecutionState(_submissionStates, SubmissionStateCount);
	}

	public object GetSubmissionState(int index)
	{
		return _submissionStates[index];
	}

	internal async Task<TResult> RunSubmissionsAsync<TResult>(ImmutableArray<Func<object[], Task>> precedingExecutors, Func<object[], Task> currentExecutor, StrongBox<Exception> exceptionHolderOpt, Func<Exception, bool> catchExceptionOpt, CancellationToken cancellationToken)
	{
		int executorIndex = 0;
		try
		{
			while (executorIndex < precedingExecutors.Length)
			{
				cancellationToken.ThrowIfCancellationRequested();
				EnsureStateCapacity();
				try
				{
					await precedingExecutors[executorIndex++](_submissionStates).ConfigureAwait(continueOnCapturedContext: false);
				}
				finally
				{
					AdvanceStateCounter();
				}
			}
			cancellationToken.ThrowIfCancellationRequested();
			EnsureStateCapacity();
			TResult result;
			try
			{
				executorIndex++;
				result = await ((Task<TResult>)currentExecutor(_submissionStates)).ConfigureAwait(continueOnCapturedContext: false);
			}
			finally
			{
				AdvanceStateCounter();
			}
			return result;
		}
		catch (Exception ex) when (catchExceptionOpt?.Invoke(ex) ?? false)
		{
			object[] array = new object[1];
			while (executorIndex < precedingExecutors.Length)
			{
				EnsureStateCapacity();
				array[0] = _submissionStates;
				Activator.CreateInstance(precedingExecutors[executorIndex++].GetMethodInfo().DeclaringType, array);
				AdvanceStateCounter();
			}
			if (executorIndex == precedingExecutors.Length)
			{
				EnsureStateCapacity();
				array[0] = _submissionStates;
				Activator.CreateInstance(currentExecutor.GetMethodInfo().DeclaringType, array);
				AdvanceStateCounter();
			}
			exceptionHolderOpt.Value = ex;
			return default(TResult);
		}
	}

	private void EnsureStateCapacity()
	{
		if (SubmissionStateCount >= _submissionStates.Length)
		{
			Array.Resize(ref _submissionStates, Math.Max(SubmissionStateCount, _submissionStates.Length * 2));
		}
	}

	private void AdvanceStateCounter()
	{
		if (_submissionStates[SubmissionStateCount] != null)
		{
			SubmissionStateCount++;
		}
	}
}
