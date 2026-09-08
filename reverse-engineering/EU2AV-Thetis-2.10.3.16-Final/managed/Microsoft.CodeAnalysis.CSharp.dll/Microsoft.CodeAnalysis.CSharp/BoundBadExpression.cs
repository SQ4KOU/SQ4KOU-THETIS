using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundBadExpression : BoundExpression, IBoundInvalidNode
{
	protected override ImmutableArray<BoundNode?> Children => StaticCast<BoundNode>.From(ChildBoundNodes);

	ImmutableArray<BoundNode> IBoundInvalidNode.InvalidNodeChildren => StaticCast<BoundNode>.From(ChildBoundNodes);

	public override LookupResultKind ResultKind { get; }

	public ImmutableArray<Symbol?> Symbols { get; }

	public ImmutableArray<BoundExpression> ChildBoundNodes { get; }

	public BoundBadExpression(SyntaxNode syntax, LookupResultKind resultKind, ImmutableArray<Symbol?> symbols, ImmutableArray<BoundExpression> childBoundNodes, TypeSymbol type)
		: this(syntax, resultKind, symbols, childBoundNodes, type, hasErrors: true)
	{
	}

	public BoundBadExpression(SyntaxNode syntax, LookupResultKind resultKind, ImmutableArray<Symbol?> symbols, ImmutableArray<BoundExpression> childBoundNodes, TypeSymbol? type, bool hasErrors = false)
		: base(BoundKind.BadExpression, syntax, type, hasErrors || childBoundNodes.HasErrors())
	{
		ResultKind = resultKind;
		Symbols = symbols;
		ChildBoundNodes = childBoundNodes;
	}

	[Conditional("DEBUG")]
	private void Validate()
	{
	}

	[DebuggerStepThrough]
	public override BoundNode? Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitBadExpression(this);
	}

	public BoundBadExpression Update(LookupResultKind resultKind, ImmutableArray<Symbol?> symbols, ImmutableArray<BoundExpression> childBoundNodes, TypeSymbol? type)
	{
		if (resultKind != ResultKind || symbols != Symbols || childBoundNodes != ChildBoundNodes || !TypeSymbol.Equals(type, base.Type, TypeCompareKind.ConsiderEverything))
		{
			BoundBadExpression boundBadExpression = new BoundBadExpression(Syntax, resultKind, symbols, childBoundNodes, type, base.HasErrors);
			boundBadExpression.CopyAttributes(this);
			return boundBadExpression;
		}
		return this;
	}
}
