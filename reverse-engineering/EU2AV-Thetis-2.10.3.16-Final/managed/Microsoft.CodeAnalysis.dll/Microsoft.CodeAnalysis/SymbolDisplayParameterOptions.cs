using System;
using System.ComponentModel;

namespace Microsoft.CodeAnalysis;

[Flags]
public enum SymbolDisplayParameterOptions
{
	None = 0,
	IncludeExtensionThis = 1,
	[EditorBrowsable(EditorBrowsableState.Never)]
	IncludeParamsRefOut = 2,
	IncludeModifiers = IncludeParamsRefOut,
	IncludeType = 4,
	IncludeName = 8,
	IncludeDefaultValue = 0x10,
	IncludeOptionalBrackets = 0x20
}
