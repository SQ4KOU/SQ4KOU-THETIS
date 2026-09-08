using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundDefaultLiteral : BoundExpression
{
	public override ConstantValue? ConstantValueOpt => null;

	public override object Display => ((object)Type) ?? ((object)"default");

	public new TypeSymbol? Type => base.Type;

	public BoundDefaultLiteral(SyntaxNode syntax, bool hasErrors)
		: base(BoundKind.DefaultLiteral, syntax, null, hasErrors)
	{
	}

	public BoundDefaultLiteral(SyntaxNode syntax)
		: base(BoundKind.DefaultLiteral, syntax, null)
	{
	}

	[DebuggerStepThrough]
	public override BoundNode? Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitDefaultLiteral(this);
	}

	public BoundDefaultLiteral Update()
	{
		return this;
	}
}
