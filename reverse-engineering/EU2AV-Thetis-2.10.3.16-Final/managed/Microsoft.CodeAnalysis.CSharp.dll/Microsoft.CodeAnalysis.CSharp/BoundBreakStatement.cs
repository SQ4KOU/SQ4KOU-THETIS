using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundBreakStatement : BoundStatement
{
	public LabelSymbol Label { get; }

	public BoundBreakStatement(SyntaxNode syntax, LabelSymbol label, bool hasErrors)
		: base(BoundKind.BreakStatement, syntax, hasErrors)
	{
		Label = label;
	}

	public BoundBreakStatement(SyntaxNode syntax, LabelSymbol label)
		: base(BoundKind.BreakStatement, syntax)
	{
		Label = label;
	}

	[DebuggerStepThrough]
	public override BoundNode? Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitBreakStatement(this);
	}

	public BoundBreakStatement Update(LabelSymbol label)
	{
		if (!Microsoft.CodeAnalysis.CSharp.Symbols.SymbolEqualityComparer.ConsiderEverything.Equals(label, Label))
		{
			BoundBreakStatement boundBreakStatement = new BoundBreakStatement(Syntax, label, base.HasErrors);
			boundBreakStatement.CopyAttributes(this);
			return boundBreakStatement;
		}
		return this;
	}
}
