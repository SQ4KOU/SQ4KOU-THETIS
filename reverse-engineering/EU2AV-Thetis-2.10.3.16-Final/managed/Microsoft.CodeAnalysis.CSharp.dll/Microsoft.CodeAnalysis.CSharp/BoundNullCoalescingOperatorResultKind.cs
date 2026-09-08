namespace Microsoft.CodeAnalysis.CSharp;

internal enum BoundNullCoalescingOperatorResultKind
{
	NoCommonType,
	LeftType,
	LeftUnwrappedType,
	RightType,
	LeftUnwrappedRightType,
	RightDynamicType
}
