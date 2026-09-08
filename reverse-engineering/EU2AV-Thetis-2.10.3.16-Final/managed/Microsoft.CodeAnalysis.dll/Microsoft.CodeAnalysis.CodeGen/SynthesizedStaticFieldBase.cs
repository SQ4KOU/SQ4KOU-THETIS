using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Symbols;

namespace Microsoft.CodeAnalysis.CodeGen;

internal abstract class SynthesizedStaticFieldBase : IFieldDefinition, ITypeDefinitionMember, ITypeMemberReference, IReference, INamedEntity, IDefinition, IFieldReference
{
	private readonly INamedTypeDefinition _containingType;

	private readonly string _name;

	public abstract ImmutableArray<byte> MappedData { get; }

	public bool IsEncDeleted => false;

	public bool IsCompileTimeConstant => false;

	public bool IsNotSerialized => false;

	public abstract bool IsReadOnly { get; }

	public bool IsRuntimeSpecial => false;

	public bool IsSpecialName => false;

	public bool IsStatic => true;

	public bool IsMarshalledExplicitly => false;

	public IMarshallingInformation? MarshallingInformation => null;

	public ImmutableArray<byte> MarshallingDescriptor => default(ImmutableArray<byte>);

	public int Offset
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/CodeGen/PrivateImplementationDetails.cs", 829);
		}
	}

	public ITypeDefinition ContainingTypeDefinition => _containingType;

	public TypeMemberVisibility Visibility => TypeMemberVisibility.Assembly;

	public string Name => _name;

	public bool IsContextualNamedEntity => false;

	public ImmutableArray<ICustomModifier> RefCustomModifiers => ImmutableArray<ICustomModifier>.Empty;

	public bool IsByReference => false;

	public ISpecializedFieldReference? AsSpecializedFieldReference => null;

	public MetadataConstant Constant
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/CodeGen/PrivateImplementationDetails.cs", 869);
		}
	}

	internal SynthesizedStaticFieldBase(string name, INamedTypeDefinition containingType)
	{
		_containingType = containingType;
		_name = name;
	}

	public MetadataConstant? GetCompileTimeValue(EmitContext context)
	{
		return null;
	}

	public ITypeReference GetContainingType(EmitContext context)
	{
		return _containingType;
	}

	public IEnumerable<ICustomAttribute> GetAttributes(EmitContext context)
	{
		return SpecializedCollections.EmptyEnumerable<ICustomAttribute>();
	}

	public void Dispatch(MetadataVisitor visitor)
	{
		visitor.Visit(this);
	}

	public IDefinition AsDefinition(EmitContext context)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/CodeGen/PrivateImplementationDetails.cs", 848);
	}

	ISymbolInternal? IReference.GetInternalSymbol()
	{
		return null;
	}

	public abstract ITypeReference GetType(EmitContext context);

	public IFieldDefinition GetResolvedField(EmitContext context)
	{
		return this;
	}

	public sealed override bool Equals(object? obj)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/CodeGen/PrivateImplementationDetails.cs", 875);
	}

	public sealed override int GetHashCode()
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/CodeGen/PrivateImplementationDetails.cs", 881);
	}
}
