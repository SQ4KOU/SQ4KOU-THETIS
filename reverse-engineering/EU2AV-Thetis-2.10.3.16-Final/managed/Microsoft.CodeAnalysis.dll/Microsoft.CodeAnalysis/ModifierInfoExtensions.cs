using System.Collections.Immutable;
using System.Linq;

namespace Microsoft.CodeAnalysis;

internal static class ModifierInfoExtensions
{
	internal static bool AnyRequired<TypeSymbol>(this ImmutableArray<ModifierInfo<TypeSymbol>> modifiers) where TypeSymbol : class
	{
		if (!modifiers.IsDefaultOrEmpty)
		{
			return modifiers.Any((ModifierInfo<TypeSymbol> m) => !m.IsOptional);
		}
		return false;
	}
}
