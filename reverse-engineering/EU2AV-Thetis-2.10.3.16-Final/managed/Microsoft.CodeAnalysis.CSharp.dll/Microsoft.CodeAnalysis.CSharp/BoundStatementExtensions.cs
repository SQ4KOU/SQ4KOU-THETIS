using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal static class BoundStatementExtensions
{
	[Conditional("DEBUG")]
	internal static void AssertIsLabeledStatement(this BoundStatement node)
	{
		BoundKind kind = node.Kind;
		if (kind != BoundKind.LabelStatement && kind != BoundKind.LabeledStatement && kind != BoundKind.SwitchSection)
		{
			throw ExceptionUtilities.UnexpectedValue(node.Kind);
		}
	}

	[Conditional("DEBUG")]
	internal static void AssertIsLabeledStatementWithLabel(this BoundStatement node, LabelSymbol label)
	{
		switch (node.Kind)
		{
		case BoundKind.SwitchSection:
			foreach (BoundSwitchLabel switchLabel in ((BoundSwitchSection)node).SwitchLabels)
			{
				if (switchLabel.Label == label)
				{
					return;
				}
			}
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/BoundTree/BoundStatementExtensions.cs", 50);
		default:
			throw ExceptionUtilities.UnexpectedValue(node.Kind);
		case BoundKind.LabelStatement:
		case BoundKind.LabeledStatement:
			break;
		}
	}
}
