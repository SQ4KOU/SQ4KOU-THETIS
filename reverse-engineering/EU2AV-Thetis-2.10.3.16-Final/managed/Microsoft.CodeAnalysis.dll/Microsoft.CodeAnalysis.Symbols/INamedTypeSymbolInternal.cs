using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Symbols;

internal interface INamedTypeSymbolInternal : ITypeSymbolInternal, INamespaceOrTypeSymbolInternal, ISymbolInternal
{
	internal static class Helpers
	{
		public static (ThreeState isManaged, bool hasGenerics) IsManagedTypeHelper(INamedTypeSymbolInternal type)
		{
			if (type.TypeKind == TypeKind.Enum)
			{
				type = type.EnumUnderlyingType;
			}
			switch (type.SpecialType)
			{
			case SpecialType.System_Void:
			case SpecialType.System_Boolean:
			case SpecialType.System_Char:
			case SpecialType.System_SByte:
			case SpecialType.System_Byte:
			case SpecialType.System_Int16:
			case SpecialType.System_UInt16:
			case SpecialType.System_Int32:
			case SpecialType.System_UInt32:
			case SpecialType.System_Int64:
			case SpecialType.System_UInt64:
			case SpecialType.System_Decimal:
			case SpecialType.System_Single:
			case SpecialType.System_Double:
			case SpecialType.System_IntPtr:
			case SpecialType.System_UIntPtr:
			case SpecialType.System_ArgIterator:
			case SpecialType.System_RuntimeArgumentHandle:
				return (isManaged: ThreeState.False, hasGenerics: false);
			case SpecialType.System_TypedReference:
				return (isManaged: ThreeState.True, hasGenerics: false);
			default:
			{
				bool isGenericType = type.IsGenericType;
				return type.TypeKind switch
				{
					TypeKind.Enum => (isManaged: ThreeState.False, hasGenerics: isGenericType), 
					TypeKind.Struct => (isManaged: ThreeState.Unknown, hasGenerics: isGenericType), 
					_ => (isManaged: ThreeState.True, hasGenerics: isGenericType), 
				};
			}
			}
		}
	}

	INamedTypeSymbolInternal? EnumUnderlyingType { get; }

	bool IsGenericType { get; }

	ImmutableArray<ISymbolInternal> GetMembers();

	ImmutableArray<ISymbolInternal> GetMembers(string name);
}
