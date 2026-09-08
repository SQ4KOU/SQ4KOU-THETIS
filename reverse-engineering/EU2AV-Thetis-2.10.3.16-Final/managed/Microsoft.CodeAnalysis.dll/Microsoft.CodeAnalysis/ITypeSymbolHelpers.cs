using System.Diagnostics.CodeAnalysis;

namespace Microsoft.CodeAnalysis;

internal static class ITypeSymbolHelpers
{
	internal static bool IsNullableType([NotNullWhen(true)] ITypeSymbol? typeOpt)
	{
		if (typeOpt == null)
		{
			return false;
		}
		return typeOpt.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T;
	}

	internal static bool IsNullableOfBoolean([NotNullWhen(true)] ITypeSymbol? type)
	{
		if (IsNullableType(type))
		{
			return IsBooleanType(GetNullableUnderlyingType(type));
		}
		return false;
	}

	internal static ITypeSymbol GetNullableUnderlyingType(ITypeSymbol type)
	{
		return ((INamedTypeSymbol)type).TypeArguments[0];
	}

	internal static bool IsBooleanType([NotNullWhen(true)] ITypeSymbol? type)
	{
		if (type == null)
		{
			return false;
		}
		return type.SpecialType == SpecialType.System_Boolean;
	}

	internal static bool IsObjectType([NotNullWhen(true)] ITypeSymbol? type)
	{
		if (type == null)
		{
			return false;
		}
		return type.SpecialType == SpecialType.System_Object;
	}

	internal static bool IsSignedIntegralType([NotNullWhen(true)] ITypeSymbol? type)
	{
		return type?.SpecialType.IsSignedIntegralType() ?? false;
	}

	internal static bool IsUnsignedIntegralType([NotNullWhen(true)] ITypeSymbol? type)
	{
		return type?.SpecialType.IsUnsignedIntegralType() ?? false;
	}

	internal static bool IsNumericType([NotNullWhen(true)] ITypeSymbol? type)
	{
		return type?.SpecialType.IsNumericType() ?? false;
	}

	internal static ITypeSymbol? GetEnumUnderlyingType(ITypeSymbol? type)
	{
		return (type as INamedTypeSymbol)?.EnumUnderlyingType;
	}

	[return: NotNullIfNotNull("type")]
	internal static ITypeSymbol? GetEnumUnderlyingTypeOrSelf(ITypeSymbol? type)
	{
		return GetEnumUnderlyingType(type) ?? type;
	}

	internal static bool IsDynamicType([NotNullWhen(true)] ITypeSymbol? type)
	{
		if (type == null)
		{
			return false;
		}
		return type.Kind == SymbolKind.DynamicType;
	}
}
