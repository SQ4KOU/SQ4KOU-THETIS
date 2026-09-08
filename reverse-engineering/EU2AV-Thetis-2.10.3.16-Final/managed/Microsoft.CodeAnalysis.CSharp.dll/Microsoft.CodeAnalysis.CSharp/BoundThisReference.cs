using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundThisReference : BoundExpression
{
	public sealed override bool IsEquivalentToThisReference => true;

	public new TypeSymbol Type => base.Type;

	public BoundThisReference(SyntaxNode syntax, TypeSymbol type, bool hasErrors)
		: base(BoundKind.ThisReference, syntax, type, hasErrors)
	{
	}

	public BoundThisReference(SyntaxNode syntax, TypeSymbol type)
		: base(BoundKind.ThisReference, syntax, type)
	{
	}

	[DebuggerStepThrough]
	public override BoundNode? Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitThisReference(this);
	}

	public BoundThisReference Update(TypeSymbol type)
	{
		if (!TypeSymbol.Equals(type, Type, TypeCompareKind.ConsiderEverything))
		{
			BoundThisReference boundThisReference = new BoundThisReference(Syntax, type, base.HasErrors);
			boundThisReference.CopyAttributes(this);
			return boundThisReference;
		}
		return this;
	}
}
