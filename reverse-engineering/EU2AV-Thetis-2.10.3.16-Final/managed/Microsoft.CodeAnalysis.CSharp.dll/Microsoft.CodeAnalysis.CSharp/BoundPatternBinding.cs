using System.Diagnostics;

namespace Microsoft.CodeAnalysis.CSharp;

[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
internal readonly struct BoundPatternBinding(BoundExpression variableAccess, BoundDagTemp tempContainingValue)
{
	public readonly BoundExpression VariableAccess = variableAccess;

	public readonly BoundDagTemp TempContainingValue = tempContainingValue;

	public override string ToString()
	{
		return GetDebuggerDisplay();
	}

	internal string GetDebuggerDisplay()
	{
		return "(" + VariableAccess.GetDebuggerDisplay() + " = " + TempContainingValue.GetDebuggerDisplay() + ")";
	}
}
