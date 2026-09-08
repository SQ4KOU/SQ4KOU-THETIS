using System;
using System.ComponentModel;

namespace Microsoft.CodeAnalysis;

[Flags]
public enum SymbolDisplayLocalOptions
{
	None = 0,
	IncludeType = 1,
	IncludeConstantValue = 2,
	[EditorBrowsable(EditorBrowsableState.Never)]
	IncludeRef = 4,
	IncludeModifiers = IncludeRef
}
