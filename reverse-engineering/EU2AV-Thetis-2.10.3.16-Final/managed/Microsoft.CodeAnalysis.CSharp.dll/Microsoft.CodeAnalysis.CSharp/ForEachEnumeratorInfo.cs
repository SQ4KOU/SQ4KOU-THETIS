using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class ForEachEnumeratorInfo
{
	internal struct Builder
	{
		public TypeSymbol CollectionType;

		public bool ViaExtensionMethod;

		public WellKnownType InlineArraySpanType;

		public bool InlineArrayUsedAsValue;

		public TypeWithAnnotations ElementTypeWithAnnotations;

		public MethodArgumentInfo? GetEnumeratorInfo;

		public MethodSymbol CurrentPropertyGetter;

		public MethodArgumentInfo? MoveNextInfo;

		public BoundAwaitableInfo? MoveNextAwaitableInfo;

		public bool IsAsync;

		public bool NeedsDisposal;

		public BoundAwaitableInfo? DisposeAwaitableInfo;

		public MethodArgumentInfo? PatternDisposeInfo;

		public BoundValuePlaceholder? CurrentPlaceholder;

		public BoundExpression? CurrentConversion;

		public TypeSymbol ElementType => ElementTypeWithAnnotations.Type;

		public bool IsIncomplete
		{
			get
			{
				if (GetEnumeratorInfo != null && MoveNextInfo != null)
				{
					return (object)CurrentPropertyGetter == null;
				}
				return true;
			}
		}

		public ForEachEnumeratorInfo Build(BinderFlags location)
		{
			return new ForEachEnumeratorInfo(CollectionType, InlineArraySpanType, InlineArrayUsedAsValue, ElementTypeWithAnnotations, GetEnumeratorInfo, CurrentPropertyGetter, MoveNextInfo, MoveNextAwaitableInfo, IsAsync, NeedsDisposal, DisposeAwaitableInfo, PatternDisposeInfo, CurrentPlaceholder, CurrentConversion, location);
		}
	}

	public readonly TypeSymbol CollectionType;

	public readonly WellKnownType InlineArraySpanType;

	public readonly bool InlineArrayUsedAsValue;

	public readonly TypeWithAnnotations ElementTypeWithAnnotations;

	public readonly MethodArgumentInfo GetEnumeratorInfo;

	public readonly MethodSymbol CurrentPropertyGetter;

	public readonly MethodArgumentInfo MoveNextInfo;

	public readonly BoundAwaitableInfo? MoveNextAwaitableInfo;

	public readonly bool NeedsDisposal;

	public readonly bool IsAsync;

	public readonly BoundAwaitableInfo? DisposeAwaitableInfo;

	public readonly MethodArgumentInfo? PatternDisposeInfo;

	public readonly BoundValuePlaceholder? CurrentPlaceholder;

	public readonly BoundExpression? CurrentConversion;

	public readonly BinderFlags Location;

	public TypeSymbol ElementType => ElementTypeWithAnnotations.Type;

	private ForEachEnumeratorInfo(TypeSymbol collectionType, WellKnownType inlineArraySpanType, bool inlineArrayUsedAsValue, TypeWithAnnotations elementType, MethodArgumentInfo getEnumeratorInfo, MethodSymbol currentPropertyGetter, MethodArgumentInfo moveNextInfo, BoundAwaitableInfo? moveNextAwaitableInfo, bool isAsync, bool needsDisposal, BoundAwaitableInfo? disposeAwaitableInfo, MethodArgumentInfo? patternDisposeInfo, BoundValuePlaceholder? currentPlaceholder, BoundExpression? currentConversion, BinderFlags location)
	{
		CollectionType = collectionType;
		InlineArraySpanType = inlineArraySpanType;
		InlineArrayUsedAsValue = inlineArrayUsedAsValue;
		ElementTypeWithAnnotations = elementType;
		GetEnumeratorInfo = getEnumeratorInfo;
		CurrentPropertyGetter = currentPropertyGetter;
		MoveNextInfo = moveNextInfo;
		MoveNextAwaitableInfo = moveNextAwaitableInfo;
		IsAsync = isAsync;
		NeedsDisposal = needsDisposal;
		DisposeAwaitableInfo = disposeAwaitableInfo;
		PatternDisposeInfo = patternDisposeInfo;
		CurrentPlaceholder = currentPlaceholder;
		CurrentConversion = currentConversion;
		Location = location;
	}
}
