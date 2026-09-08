using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.ErrorReporting;

namespace Roslyn.Utilities;

internal static class RoslynParallel
{
	internal static readonly ParallelOptions DefaultParallelOptions = new ParallelOptions
	{
		MaxDegreeOfParallelism = Environment.ProcessorCount
	};

	public static ParallelLoopResult For(int fromInclusive, int toExclusive, Action<int> body, CancellationToken cancellationToken)
	{
		ParallelOptions parallelOptions = (cancellationToken.CanBeCanceled ? new ParallelOptions
		{
			CancellationToken = cancellationToken,
			MaxDegreeOfParallelism = Environment.ProcessorCount
		} : DefaultParallelOptions);
		return Parallel.For(fromInclusive, toExclusive, parallelOptions, errorHandlingBody);
		void errorHandlingBody(int i)
		{
			try
			{
				body(i);
			}
			catch (Exception exception) when (FatalError.ReportAndPropagateUnlessCanceled(exception, cancellationToken))
			{
				throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/InternalUtilities/RoslynParallel.cs", 35);
			}
			catch (OperationCanceledException ex) when (cancellationToken.IsCancellationRequested && ex.CancellationToken != cancellationToken)
			{
				cancellationToken.ThrowIfCancellationRequested();
				throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/InternalUtilities/RoslynParallel.cs", 42);
			}
		}
	}
}
