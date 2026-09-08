using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundPointerElementAccess : BoundExpression
{
	protected override ImmutableArray<BoundNode?> Children => ImmutableArray.Create((BoundNode)Expression, (BoundNode)Index);

	public new TypeSymbol Type => base.Type;

	public BoundExpression Expression { get; }

	public BoundExpression Index { get; }

	public bool Checked { get; }

	public bool RefersToLocation { get; }

	public BoundPointerElementAccess(SyntaxNode syntax, BoundExpression expression, BoundExpression index, bool @checked, bool refersToLocation, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.PointerElementAccess, syntax, type, hasErrors || expression.HasErrors() || index.HasErrors())
	{
		Expression = expression;
		Index = index;
		Checked = @checked;
		RefersToLocation = refersToLocation;
	}

	[DebuggerStepThrough]
	public override BoundNode? Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitPointerElementAccess(this);
	}

	public BoundPointerElementAccess Update(BoundExpression expression, BoundExpression index, bool @checked, bool refersToLocation, TypeSymbol type)
	{
		if (expression != Expression || index != Index || @checked != Checked || refersToLocation != RefersToLocation || !TypeSymbol.Equals(type, Type, TypeCompareKind.ConsiderEverything))
		{
			BoundPointerElementAccess boundPointerElementAccess = new BoundPointerElementAccess(Syntax, expression, index, @checked, refersToLocation, type, base.HasErrors);
			boundPointerElementAccess.CopyAttributes(this);
			return boundPointerElementAccess;
		}
		return this;
	}
}
