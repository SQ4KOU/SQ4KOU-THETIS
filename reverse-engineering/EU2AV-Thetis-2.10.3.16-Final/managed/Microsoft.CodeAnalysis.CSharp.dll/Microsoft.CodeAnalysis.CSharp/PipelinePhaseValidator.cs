using System.Diagnostics;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class PipelinePhaseValidator
{
	[Conditional("DEBUG")]
	public static void AssertAfterInitialBinding(BoundNode node)
	{
	}

	[Conditional("DEBUG")]
	public static void AssertAfterLocalRewriting(BoundNode node)
	{
	}

	[Conditional("DEBUG")]
	public static void AssertAfterSpilling(BoundNode node)
	{
	}

	[Conditional("DEBUG")]
	public static void AssertAfterClosureConversion(BoundNode node)
	{
	}

	[Conditional("DEBUG")]
	public static void AssertAfterStateMachineRewriting(BoundNode node)
	{
	}
}
