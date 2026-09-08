using System;

namespace Microsoft.CodeAnalysis.CSharp;

[Flags]
internal enum EmbeddableAttributes
{
	IsReadOnlyAttribute = 1,
	IsByRefLikeAttribute = 2,
	IsUnmanagedAttribute = 4,
	NullableAttribute = 8,
	NullableContextAttribute = 0x10,
	NullablePublicOnlyAttribute = 0x20,
	NativeIntegerAttribute = 0x40,
	ScopedRefAttribute = 0x80,
	RefSafetyRulesAttribute = 0x100,
	RequiresLocationAttribute = 0x200,
	ParamCollectionAttribute = 0x400,
	ExtensionMarkerAttribute = 0x800
}
