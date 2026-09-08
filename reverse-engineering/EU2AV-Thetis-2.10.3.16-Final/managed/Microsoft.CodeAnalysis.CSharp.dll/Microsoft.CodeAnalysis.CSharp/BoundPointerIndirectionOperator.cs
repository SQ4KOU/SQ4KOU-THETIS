using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundPointerIndirectionOperator : BoundExpression
{
	protected override ImmutableArray<BoundNode?> Children => ImmutableArray.Create((BoundNode)Operand);

	public new TypeSymbol Type => base.Type;

	public BoundExpression Operand { get; }

	public bool RefersToLocation { get; }

	public BoundPointerIndirectionOperator(SyntaxNode syntax, BoundExpression operand, bool refersToLocation, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.PointerIndirectionOperator, syntax, type, hasErrors || operand.HasErrors())
	{
		Operand = operand;
		RefersToLocation = refersToLocation;
	}

	[DebuggerStepThrough]
	public override BoundNode? Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitPointerIndirectionOperator(this);
	}

	public BoundPointerIndirectionOperator Update(BoundExpression operand, bool refersToLocation, TypeSymbol type)
	{
		if (operand != Operand || refersToLocation != RefersToLocation || !TypeSymbol.Equals(type, Type, TypeCompareKind.ConsiderEverything))
		{
			BoundPointerIndirectionOperator boundPointerIndirectionOperator = new BoundPointerIndirectionOperator(Syntax, operand, refersToLocation, type, base.HasErrors);
			boundPointerIndirectionOperator.CopyAttributes(this);
			return boundPointerIndirectionOperator;
		}
		return this;
	}
}
