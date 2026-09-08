using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Microsoft.CodeAnalysis;

internal static class StackGuard
{
	public const int MaxUncheckedRecursionDepth = 20;

	[DebuggerStepThrough]
	public static void EnsureSufficientExecutionStack(int recursionDepth)
	{
		if (recursionDepth > 20)
		{
			RuntimeHelpers.EnsureSufficientExecutionStack();
		}
	}
}
