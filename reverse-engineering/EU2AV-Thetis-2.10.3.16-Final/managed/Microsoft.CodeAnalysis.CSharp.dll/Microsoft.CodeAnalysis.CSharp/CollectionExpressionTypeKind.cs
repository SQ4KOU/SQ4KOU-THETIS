namespace Microsoft.CodeAnalysis.CSharp;

internal enum CollectionExpressionTypeKind
{
	None,
	Array,
	Span,
	ReadOnlySpan,
	CollectionBuilder,
	ImplementsIEnumerable,
	ArrayInterface
}
