using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundCollectionExpressionSpreadExpressionPlaceholder : BoundValuePlaceholderBase
{
	public sealed override bool IsEquivalentToThisReference => false;

	public BoundCollectionExpressionSpreadExpressionPlaceholder(SyntaxNode syntax, TypeSymbol? type, bool hasErrors)
		: base(BoundKind.CollectionExpressionSpreadExpressionPlaceholder, syntax, type, hasErrors)
	{
	}

	public BoundCollectionExpressionSpreadExpressionPlaceholder(SyntaxNode syntax, TypeSymbol? type)
		: base(BoundKind.CollectionExpressionSpreadExpressionPlaceholder, syntax, type)
	{
	}

	[DebuggerStepThrough]
	public override BoundNode? Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitCollectionExpressionSpreadExpressionPlaceholder(this);
	}

	public BoundCollectionExpressionSpreadExpressionPlaceholder Update(TypeSymbol? type)
	{
		if (!TypeSymbol.Equals(type, base.Type, TypeCompareKind.ConsiderEverything))
		{
			BoundCollectionExpressionSpreadExpressionPlaceholder boundCollectionExpressionSpreadExpressionPlaceholder = new BoundCollectionExpressionSpreadExpressionPlaceholder(Syntax, type, base.HasErrors);
			boundCollectionExpressionSpreadExpressionPlaceholder.CopyAttributes(this);
			return boundCollectionExpressionSpreadExpressionPlaceholder;
		}
		return this;
	}
}
