using System.Runtime.InteropServices;

namespace Microsoft.CodeAnalysis;

[StructLayout(LayoutKind.Auto)]
internal readonly struct ModifierInfo<TypeSymbol>(bool isOptional, TypeSymbol modifier) where TypeSymbol : class
{
	internal readonly bool IsOptional = isOptional;

	internal readonly TypeSymbol Modifier = modifier;
}
