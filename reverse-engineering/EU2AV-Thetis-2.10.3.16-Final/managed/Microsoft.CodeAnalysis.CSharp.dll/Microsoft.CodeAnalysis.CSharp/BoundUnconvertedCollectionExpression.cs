using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundUnconvertedCollectionExpression : BoundCollectionExpressionBase
{
	public override object Display
	{
		get
		{
			if ((object)Type != null)
			{
				return base.Display;
			}
			return MessageID.IDS_CollectionExpression.Localize();
		}
	}

	public new TypeSymbol? Type => base.Type;

	public BoundUnconvertedCollectionExpression(SyntaxNode syntax, ImmutableArray<BoundNode> elements, bool hasErrors = false)
		: base(BoundKind.UnconvertedCollectionExpression, syntax, elements, null, hasErrors || elements.HasErrors())
	{
	}

	[DebuggerStepThrough]
	public override BoundNode? Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitUnconvertedCollectionExpression(this);
	}

	public BoundUnconvertedCollectionExpression Update(ImmutableArray<BoundNode> elements)
	{
		if (elements != base.Elements)
		{
			BoundUnconvertedCollectionExpression boundUnconvertedCollectionExpression = new BoundUnconvertedCollectionExpression(Syntax, elements, base.HasErrors);
			boundUnconvertedCollectionExpression.CopyAttributes(this);
			return boundUnconvertedCollectionExpression;
		}
		return this;
	}
}
