using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis;

internal readonly struct FieldInfo<TypeSymbol> where TypeSymbol : class
{
	internal readonly bool IsByRef;

	internal readonly ImmutableArray<ModifierInfo<TypeSymbol>> RefCustomModifiers;

	internal readonly TypeSymbol Type;

	internal readonly ImmutableArray<ModifierInfo<TypeSymbol>> CustomModifiers;

	internal FieldInfo(bool isByRef, ImmutableArray<ModifierInfo<TypeSymbol>> refCustomModifiers, TypeSymbol type, ImmutableArray<ModifierInfo<TypeSymbol>> customModifiers)
	{
		IsByRef = isByRef;
		RefCustomModifiers = refCustomModifiers;
		Type = type;
		CustomModifiers = customModifiers;
	}

	internal FieldInfo(TypeSymbol type)
		: this(isByRef: false, default(ImmutableArray<ModifierInfo<TypeSymbol>>), type, default(ImmutableArray<ModifierInfo<TypeSymbol>>))
	{
	}
}
