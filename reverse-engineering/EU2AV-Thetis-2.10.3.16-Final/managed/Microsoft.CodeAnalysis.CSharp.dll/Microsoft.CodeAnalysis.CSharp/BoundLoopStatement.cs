using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal abstract class BoundLoopStatement : BoundStatement
{
	public LabelSymbol BreakLabel { get; }

	public LabelSymbol ContinueLabel { get; }

	protected BoundLoopStatement(BoundKind kind, SyntaxNode syntax, LabelSymbol breakLabel, LabelSymbol continueLabel, bool hasErrors)
		: base(kind, syntax, hasErrors)
	{
		BreakLabel = breakLabel;
		ContinueLabel = continueLabel;
	}

	protected BoundLoopStatement(BoundKind kind, SyntaxNode syntax, LabelSymbol breakLabel, LabelSymbol continueLabel)
		: base(kind, syntax)
	{
		BreakLabel = breakLabel;
		ContinueLabel = continueLabel;
	}
}
