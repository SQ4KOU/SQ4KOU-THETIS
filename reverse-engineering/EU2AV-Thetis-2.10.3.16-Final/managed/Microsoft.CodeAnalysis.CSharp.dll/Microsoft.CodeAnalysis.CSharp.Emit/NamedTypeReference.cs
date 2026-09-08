using System.Collections.Generic;
using System.Reflection.Metadata;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Symbols;

namespace Microsoft.CodeAnalysis.CSharp.Emit;

internal abstract class NamedTypeReference : INamedTypeReference, ITypeReference, IReference, INamedEntity
{
	protected readonly NamedTypeSymbol UnderlyingNamedType;

	ushort INamedTypeReference.GenericParameterCount => (ushort)UnderlyingNamedType.Arity;

	bool INamedTypeReference.MangleName => UnderlyingNamedType.MangleName;

	string? INamedTypeReference.AssociatedFileIdentifier => UnderlyingNamedType.GetFileLocalTypeMetadataNamePrefix();

	string INamedEntity.Name => UnderlyingNamedType.MetadataName;

	bool ITypeReference.IsEnum => UnderlyingNamedType.IsEnumType();

	bool ITypeReference.IsValueType => UnderlyingNamedType.IsValueType;

	Microsoft.Cci.PrimitiveTypeCode ITypeReference.TypeCode => Microsoft.Cci.PrimitiveTypeCode.NotPrimitive;

	TypeDefinitionHandle ITypeReference.TypeDef => default(TypeDefinitionHandle);

	IGenericMethodParameterReference ITypeReference.AsGenericMethodParameterReference => null;

	public abstract IGenericTypeInstanceReference AsGenericTypeInstanceReference { get; }

	IGenericTypeParameterReference ITypeReference.AsGenericTypeParameterReference => null;

	public abstract INamespaceTypeReference AsNamespaceTypeReference { get; }

	public abstract INestedTypeReference AsNestedTypeReference { get; }

	public abstract ISpecializedNestedTypeReference AsSpecializedNestedTypeReference { get; }

	public NamedTypeReference(NamedTypeSymbol underlyingNamedType)
	{
		UnderlyingNamedType = underlyingNamedType;
	}

	ITypeDefinition ITypeReference.GetResolvedType(EmitContext context)
	{
		return null;
	}

	INamespaceTypeDefinition ITypeReference.AsNamespaceTypeDefinition(EmitContext context)
	{
		return null;
	}

	INestedTypeDefinition ITypeReference.AsNestedTypeDefinition(EmitContext context)
	{
		return null;
	}

	ITypeDefinition ITypeReference.AsTypeDefinition(EmitContext context)
	{
		return null;
	}

	public override string ToString()
	{
		return UnderlyingNamedType.ToDisplayString(SymbolDisplayFormat.ILVisualizationFormat);
	}

	IEnumerable<ICustomAttribute> IReference.GetAttributes(EmitContext context)
	{
		return SpecializedCollections.EmptyEnumerable<ICustomAttribute>();
	}

	public abstract void Dispatch(MetadataVisitor visitor);

	IDefinition IReference.AsDefinition(EmitContext context)
	{
		return null;
	}

	ISymbolInternal IReference.GetInternalSymbol()
	{
		return UnderlyingNamedType;
	}

	public sealed override bool Equals(object obj)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Emitter/Model/NamedTypeReference.cs", 172);
	}

	public sealed override int GetHashCode()
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Emitter/Model/NamedTypeReference.cs", 178);
	}
}
