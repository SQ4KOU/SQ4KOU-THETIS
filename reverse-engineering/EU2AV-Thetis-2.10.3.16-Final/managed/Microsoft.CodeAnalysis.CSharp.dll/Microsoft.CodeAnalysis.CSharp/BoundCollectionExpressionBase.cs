using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal abstract class BoundCollectionExpressionBase : BoundExpression
{
	public new bool IsParamsArrayOrCollection
	{
		get
		{
			return base.IsParamsArrayOrCollection;
		}
		init
		{
			base.IsParamsArrayOrCollection = value;
		}
	}

	public ImmutableArray<BoundNode> Elements { get; }

	internal bool HasSpreadElements(out int numberIncludingLastSpread, out bool hasKnownLength)
	{
		hasKnownLength = true;
		numberIncludingLastSpread = 0;
		for (int i = 0; i < Elements.Length; i++)
		{
			if (Elements[i] is BoundCollectionExpressionSpreadElement boundCollectionExpressionSpreadElement)
			{
				numberIncludingLastSpread = i + 1;
				if (boundCollectionExpressionSpreadElement.LengthOrCount == null)
				{
					hasKnownLength = false;
				}
			}
		}
		return numberIncludingLastSpread > 0;
	}

	protected BoundCollectionExpressionBase(BoundKind kind, SyntaxNode syntax, ImmutableArray<BoundNode> elements, TypeSymbol? type, bool hasErrors = false)
		: base(kind, syntax, type, hasErrors)
	{
		Elements = elements;
	}
}
