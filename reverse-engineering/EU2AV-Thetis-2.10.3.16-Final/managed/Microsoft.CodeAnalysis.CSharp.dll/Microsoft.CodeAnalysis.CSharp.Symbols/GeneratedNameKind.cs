using System;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal enum GeneratedNameKind
{
	None = 0,
	ThisProxyField = 52,
	HoistedLocalField = 53,
	DisplayClassLocalOrField = 56,
	LambdaMethod = 98,
	LambdaDisplayClass = 99,
	StateMachineType = 100,
	LocalFunction = 103,
	AwaiterField = 117,
	HoistedSynthesizedLocalField = 115,
	StateMachineStateField = 49,
	IteratorCurrentBackingField = 50,
	StateMachineParameterProxyField = 51,
	ReusableHoistedLocalField = 55,
	LambdaCacheField = 57,
	FixedBufferField = 101,
	Extension = 69,
	FileType = 70,
	AnonymousType = 102,
	TransparentIdentifier = 104,
	AnonymousTypeField = 105,
	StateMachineStateIdField = 73,
	AnonymousTypeTypeParameter = 106,
	AutoPropertyBackingField = 107,
	IteratorCurrentThreadIdField = 108,
	IteratorFinallyMethod = 109,
	BaseMethodWrapper = 110,
	AsyncBuilderField = 116,
	DelegateCacheContainerType = 79,
	DynamicCallSiteContainerType = 111,
	PrimaryConstructorParameter = 80,
	DynamicCallSiteField = 112,
	AsyncIteratorPromiseOfValueOrEndBackingField = 118,
	DisposeModeField = 119,
	CombinedTokensField = 120,
	InlineArrayType = 121,
	ReadOnlyListType = 122,
	[Obsolete]
	Deprecated_OuterscopeLocals = 54,
	[Obsolete]
	Deprecated_IteratorInstance = 97,
	[Obsolete]
	Deprecated_InitializerLocal = LocalFunction,
	[Obsolete]
	Deprecated_DynamicDelegate = 113,
	[Obsolete]
	Deprecated_ComrefCallLocal = 114
}
