using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundCollectionExpression : BoundCollectionExpressionBase
{
	public new TypeSymbol Type => base.Type;

	public CollectionExpressionTypeKind CollectionTypeKind { get; }

	public BoundObjectOrCollectionValuePlaceholder? Placeholder { get; }

	public BoundExpression? CollectionCreation { get; }

	public MethodSymbol? CollectionBuilderMethod { get; }

	public BoundValuePlaceholder? CollectionBuilderInvocationPlaceholder { get; }

	public BoundExpression? CollectionBuilderInvocationConversion { get; }

	public bool WasTargetTyped { get; }

	public BoundUnconvertedCollectionExpression UnconvertedCollectionExpression { get; }

	public BoundCollectionExpression(SyntaxNode syntax, CollectionExpressionTypeKind collectionTypeKind, BoundObjectOrCollectionValuePlaceholder? placeholder, BoundExpression? collectionCreation, MethodSymbol? collectionBuilderMethod, BoundValuePlaceholder? collectionBuilderInvocationPlaceholder, BoundExpression? collectionBuilderInvocationConversion, bool wasTargetTyped, BoundUnconvertedCollectionExpression unconvertedCollectionExpression, ImmutableArray<BoundNode> elements, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.CollectionExpression, syntax, elements, type, hasErrors || placeholder.HasErrors() || collectionCreation.HasErrors() || collectionBuilderInvocationPlaceholder.HasErrors() || collectionBuilderInvocationConversion.HasErrors() || unconvertedCollectionExpression.HasErrors() || elements.HasErrors())
	{
		CollectionTypeKind = collectionTypeKind;
		Placeholder = placeholder;
		CollectionCreation = collectionCreation;
		CollectionBuilderMethod = collectionBuilderMethod;
		CollectionBuilderInvocationPlaceholder = collectionBuilderInvocationPlaceholder;
		CollectionBuilderInvocationConversion = collectionBuilderInvocationConversion;
		WasTargetTyped = wasTargetTyped;
		UnconvertedCollectionExpression = unconvertedCollectionExpression;
	}

	[DebuggerStepThrough]
	public override BoundNode? Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitCollectionExpression(this);
	}

	public BoundCollectionExpression Update(CollectionExpressionTypeKind collectionTypeKind, BoundObjectOrCollectionValuePlaceholder? placeholder, BoundExpression? collectionCreation, MethodSymbol? collectionBuilderMethod, BoundValuePlaceholder? collectionBuilderInvocationPlaceholder, BoundExpression? collectionBuilderInvocationConversion, bool wasTargetTyped, BoundUnconvertedCollectionExpression unconvertedCollectionExpression, ImmutableArray<BoundNode> elements, TypeSymbol type)
	{
		if (collectionTypeKind != CollectionTypeKind || placeholder != Placeholder || collectionCreation != CollectionCreation || !Microsoft.CodeAnalysis.CSharp.Symbols.SymbolEqualityComparer.ConsiderEverything.Equals(collectionBuilderMethod, CollectionBuilderMethod) || collectionBuilderInvocationPlaceholder != CollectionBuilderInvocationPlaceholder || collectionBuilderInvocationConversion != CollectionBuilderInvocationConversion || wasTargetTyped != WasTargetTyped || unconvertedCollectionExpression != UnconvertedCollectionExpression || elements != base.Elements || !TypeSymbol.Equals(type, Type, TypeCompareKind.ConsiderEverything))
		{
			BoundCollectionExpression boundCollectionExpression = new BoundCollectionExpression(Syntax, collectionTypeKind, placeholder, collectionCreation, collectionBuilderMethod, collectionBuilderInvocationPlaceholder, collectionBuilderInvocationConversion, wasTargetTyped, unconvertedCollectionExpression, elements, type, base.HasErrors);
			boundCollectionExpression.CopyAttributes(this);
			return boundCollectionExpression;
		}
		return this;
	}
}
