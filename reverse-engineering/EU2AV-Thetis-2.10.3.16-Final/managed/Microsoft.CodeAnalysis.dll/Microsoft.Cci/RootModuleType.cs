using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Symbols;

namespace Microsoft.Cci;

internal class RootModuleType : INamespaceTypeDefinition, INamedTypeDefinition, ITypeDefinition, IDefinition, IReference, ITypeReference, INamedTypeReference, INamedEntity, INamespaceTypeReference
{
	private readonly IUnit _unit;

	private IReadOnlyList<IMethodDefinition>? _methods;

	public TypeDefinitionHandle TypeDef => default(TypeDefinitionHandle);

	public ITypeDefinition ResolvedType => this;

	public bool MangleName => false;

	public string? AssociatedFileIdentifier => null;

	public string Name => "<Module>";

	public ushort Alignment => 0;

	public bool HasDeclarativeSecurity => false;

	bool IDefinition.IsEncDeleted => false;

	public bool IsAbstract => false;

	public bool IsBeforeFieldInit => false;

	public bool IsComObject => false;

	public bool IsGeneric => false;

	public bool IsInterface => false;

	public bool IsDelegate => false;

	public bool IsRuntimeSpecial => false;

	public bool IsSerializable => false;

	public bool IsSpecialName => false;

	public bool IsWindowsRuntimeImport => false;

	public bool IsSealed => false;

	public LayoutKind Layout => LayoutKind.Auto;

	public uint SizeOf => 0u;

	public CharSet StringFormat => CharSet.Ansi;

	public bool IsPublic => false;

	public bool IsNested => false;

	IEnumerable<IGenericTypeParameter> ITypeDefinition.GenericParameters
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/PEWriter/RootModuleType.cs", 202);
		}
	}

	ushort ITypeDefinition.GenericParameterCount => 0;

	IEnumerable<SecurityAttribute> ITypeDefinition.SecurityAttributes
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/PEWriter/RootModuleType.cs", 215);
		}
	}

	bool ITypeReference.IsEnum
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/PEWriter/RootModuleType.cs", 225);
		}
	}

	bool ITypeReference.IsValueType
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/PEWriter/RootModuleType.cs", 230);
		}
	}

	PrimitiveTypeCode ITypeReference.TypeCode
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/PEWriter/RootModuleType.cs", 240);
		}
	}

	ushort INamedTypeReference.GenericParameterCount
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/PEWriter/RootModuleType.cs", 245);
		}
	}

	string INamespaceTypeReference.NamespaceName => string.Empty;

	IGenericMethodParameterReference? ITypeReference.AsGenericMethodParameterReference => null;

	IGenericTypeInstanceReference? ITypeReference.AsGenericTypeInstanceReference => null;

	IGenericTypeParameterReference? ITypeReference.AsGenericTypeParameterReference => null;

	INamespaceTypeReference ITypeReference.AsNamespaceTypeReference => this;

	INestedTypeReference? ITypeReference.AsNestedTypeReference => null;

	ISpecializedNestedTypeReference? ITypeReference.AsSpecializedNestedTypeReference => null;

	public RootModuleType(IUnit unit)
	{
		_unit = unit;
	}

	public void SetStaticConstructorBody(ImmutableArray<byte> il)
	{
		_methods = SpecializedCollections.SingletonReadOnlyList(new StaticConstructor(this, 0, il));
	}

	public IEnumerable<IMethodDefinition> GetMethods(EmitContext context)
	{
		return _methods ?? (_methods = SpecializedCollections.EmptyReadOnlyList<IMethodDefinition>());
	}

	public IEnumerable<ICustomAttribute> GetAttributes(EmitContext context)
	{
		return SpecializedCollections.EmptyEnumerable<ICustomAttribute>();
	}

	public ITypeReference? GetBaseClass(EmitContext context)
	{
		return null;
	}

	public IEnumerable<IEventDefinition> GetEvents(EmitContext context)
	{
		return SpecializedCollections.EmptyEnumerable<IEventDefinition>();
	}

	public IEnumerable<MethodImplementation> GetExplicitImplementationOverrides(EmitContext context)
	{
		return SpecializedCollections.EmptyEnumerable<MethodImplementation>();
	}

	public IEnumerable<IFieldDefinition> GetFields(EmitContext context)
	{
		return SpecializedCollections.EmptyEnumerable<IFieldDefinition>();
	}

	public IEnumerable<TypeReferenceWithAttributes> Interfaces(EmitContext context)
	{
		return SpecializedCollections.EmptyEnumerable<TypeReferenceWithAttributes>();
	}

	public IEnumerable<INestedTypeDefinition> GetNestedTypes(EmitContext context)
	{
		return SpecializedCollections.EmptyEnumerable<INestedTypeDefinition>();
	}

	public IEnumerable<IPropertyDefinition> GetProperties(EmitContext context)
	{
		return SpecializedCollections.EmptyEnumerable<IPropertyDefinition>();
	}

	void IReference.Dispatch(MetadataVisitor visitor)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/PEWriter/RootModuleType.cs", 220);
	}

	ITypeDefinition ITypeReference.GetResolvedType(EmitContext context)
	{
		return this;
	}

	IUnitReference INamespaceTypeReference.GetUnit(EmitContext context)
	{
		return _unit;
	}

	INamespaceTypeDefinition ITypeReference.AsNamespaceTypeDefinition(EmitContext context)
	{
		return this;
	}

	INestedTypeDefinition? ITypeReference.AsNestedTypeDefinition(EmitContext context)
	{
		return null;
	}

	ITypeDefinition ITypeReference.AsTypeDefinition(EmitContext context)
	{
		return this;
	}

	IDefinition IReference.AsDefinition(EmitContext context)
	{
		return this;
	}

	ISymbolInternal? IReference.GetInternalSymbol()
	{
		return null;
	}

	public sealed override bool Equals(object? obj)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/PEWriter/RootModuleType.cs", 334);
	}

	public sealed override int GetHashCode()
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/PEWriter/RootModuleType.cs", 340);
	}
}
