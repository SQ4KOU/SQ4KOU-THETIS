using System;

namespace Microsoft.CodeAnalysis;

[Flags]
internal enum TypeCompareKind
{
	ConsiderEverything = 0,
	ConsiderEverything2 = 0,
	IgnoreCustomModifiersAndArraySizesAndLowerBounds = 1,
	IgnoreDynamic = 2,
	IgnoreTupleNames = 4,
	IgnoreDynamicAndTupleNames = IgnoreDynamic | IgnoreTupleNames,
	IgnoreNullableModifiersForReferenceTypes = 8,
	ObliviousNullableModifierMatchesAny = 0x10,
	IgnoreNativeIntegers = 0x20,
	FunctionPointerRefOutInRefReadonlyMatch = 0x40,
	AllNullableIgnoreOptions = IgnoreNullableModifiersForReferenceTypes | ObliviousNullableModifierMatchesAny,
	AllIgnoreOptions = IgnoreDynamicAndTupleNames | AllNullableIgnoreOptions | IgnoreCustomModifiersAndArraySizesAndLowerBounds | IgnoreNativeIntegers,
	AllIgnoreOptionsForVB = 5,
	CLRSignatureCompareOptions = 0x3E,
	AllIgnoreOptionsPlusNullableWithObliviousMatchesAny = 0x37
}
