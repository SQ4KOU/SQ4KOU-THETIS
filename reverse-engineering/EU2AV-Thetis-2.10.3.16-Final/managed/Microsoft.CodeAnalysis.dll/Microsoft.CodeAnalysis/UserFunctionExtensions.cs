using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

internal static class UserFunctionExtensions
{
	internal static Func<TInput, CancellationToken, TOutput> WrapUserFunction<TInput, TOutput>(this Func<TInput, CancellationToken, TOutput> userFunction, bool catchAnalyzerExceptions)
	{
		return delegate(TInput input, CancellationToken token)
		{
			try
			{
				return userFunction(input, token);
			}
			catch (Exception ex) when (catchAnalyzerExceptions && !ExceptionUtilities.IsCurrentOperationBeingCancelled(ex, token))
			{
				throw new UserFunctionException(ex);
			}
		};
	}

	internal static Func<TInput, CancellationToken, ImmutableArray<TOutput>> WrapUserFunctionAsImmutableArray<TInput, TOutput>(this Func<TInput, CancellationToken, IEnumerable<TOutput>> userFunction, bool catchAnalyzerExceptions)
	{
		return (TInput input, CancellationToken token) => userFunction.WrapUserFunction(catchAnalyzerExceptions)(input, token).ToImmutableArrayOrEmpty();
	}

	internal static Action<TInput, CancellationToken> WrapUserAction<TInput>(this Action<TInput> userAction, bool catchAnalyzerExceptions)
	{
		return delegate(TInput input, CancellationToken token)
		{
			try
			{
				userAction(input);
			}
			catch (Exception ex) when (catchAnalyzerExceptions && !ExceptionUtilities.IsCurrentOperationBeingCancelled(ex, token))
			{
				throw new UserFunctionException(ex);
			}
		};
	}

	internal static Action<TInput1, TInput2, CancellationToken> WrapUserAction<TInput1, TInput2>(this Action<TInput1, TInput2> userAction, bool catchAnalyzerExceptions)
	{
		return delegate(TInput1 input1, TInput2 input2, CancellationToken token)
		{
			try
			{
				userAction(input1, input2);
			}
			catch (Exception ex) when (catchAnalyzerExceptions && !ExceptionUtilities.IsCurrentOperationBeingCancelled(ex, token))
			{
				throw new UserFunctionException(ex);
			}
		};
	}

	internal static IEqualityComparer<T> WrapUserComparer<T>(this IEqualityComparer<T> comparer, bool catchAnalyzerExceptions)
	{
		if (!catchAnalyzerExceptions)
		{
			return comparer;
		}
		return new WrappedUserComparer<T>(comparer);
	}
}
