using System;

namespace Microsoft.CodeAnalysis;

[Flags]
internal enum SymbolDisplayCompilerInternalOptions
{
	None = 0,
	UseMetadataMemberNames = 1,
	UseArityForGenericTypes = 2,
	FlagMissingMetadataTypes = 4,
	IncludeScriptType = 8,
	IncludeCustomModifiers = 0x10,
	ReverseArrayRankSpecifiers = 0x20,
	UseNativeIntegerUnderlyingType = 0x40,
	UsePlusForNestedTypes = 0x80,
	IncludeContainingFileForFileTypes = 0x100,
	ExcludeParameterNameIfStandalone = 0x200,
	IncludeFileLocalTypesPrefix = 0x400
}
