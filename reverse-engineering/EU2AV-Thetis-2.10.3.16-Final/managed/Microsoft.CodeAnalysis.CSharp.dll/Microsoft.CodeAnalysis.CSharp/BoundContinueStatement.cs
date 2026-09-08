using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundContinueStatement : BoundStatement
{
	public LabelSymbol Label { get; }

	public BoundContinueStatement(SyntaxNode syntax, LabelSymbol label, bool hasErrors)
		: base(BoundKind.ContinueStatement, syntax, hasErrors)
	{
		Label = label;
	}

	public BoundContinueStatement(SyntaxNode syntax, LabelSymbol label)
		: base(BoundKind.ContinueStatement, syntax)
	{
		Label = label;
	}

	[DebuggerStepThrough]
	public override BoundNode? Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitContinueStatement(this);
	}

	public BoundContinueStatement Update(LabelSymbol label)
	{
		if (!Microsoft.CodeAnalysis.CSharp.Symbols.SymbolEqualityComparer.ConsiderEverything.Equals(label, Label))
		{
			BoundContinueStatement boundContinueStatement = new BoundContinueStatement(Syntax, label, base.HasErrors);
			boundContinueStatement.CopyAttributes(this);
			return boundContinueStatement;
		}
		return this;
	}
}
