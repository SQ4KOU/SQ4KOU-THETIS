using System.Collections.Generic;
using System.Reflection.Metadata;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Symbols;

namespace Microsoft.Cci;

internal class InheritedTypeParameter : IGenericTypeParameter, IGenericParameter, IDefinition, IReference, IGenericParameterReference, ITypeReference, INamedEntity, IParameterListEntry, IGenericTypeParameterReference
{
	private readonly ushort _index;

	private readonly ITypeDefinition _inheritingType;

	private readonly IGenericTypeParameter _parentParameter;

	public ITypeDefinition DefiningType => _inheritingType;

	public bool MustBeReferenceType => _parentParameter.MustBeReferenceType;

	public bool MustBeValueType => _parentParameter.MustBeValueType;

	public bool AllowsRefLikeType => _parentParameter.AllowsRefLikeType;

	public bool MustHaveDefaultConstructor => _parentParameter.MustHaveDefaultConstructor;

	public TypeParameterVariance Variance
	{
		get
		{
			if (!_inheritingType.IsInterface && !_inheritingType.IsDelegate)
			{
				return TypeParameterVariance.NonVariant;
			}
			return _parentParameter.Variance;
		}
	}

	public bool IsEncDeleted => false;

	public ushort Alignment => 0;

	public bool HasDeclarativeSecurity => false;

	public bool IsEnum => false;

	public IArrayTypeReference? AsArrayTypeReference => this as IArrayTypeReference;

	public IGenericMethodParameter? AsGenericMethodParameter => this as IGenericMethodParameter;

	public IGenericMethodParameterReference? AsGenericMethodParameterReference => this as IGenericMethodParameterReference;

	public IGenericTypeInstanceReference? AsGenericTypeInstanceReference => this as IGenericTypeInstanceReference;

	public IGenericTypeParameter? AsGenericTypeParameter => this;

	public IGenericTypeParameterReference? AsGenericTypeParameterReference => this;

	public INamespaceTypeReference? AsNamespaceTypeReference => this as INamespaceTypeReference;

	public INestedTypeReference? AsNestedTypeReference => this as INestedTypeReference;

	public ISpecializedNestedTypeReference? AsSpecializedNestedTypeReference => this as ISpecializedNestedTypeReference;

	public IModifiedTypeReference? AsModifiedTypeReference => this as IModifiedTypeReference;

	public IPointerTypeReference? AsPointerTypeReference => this as IPointerTypeReference;

	public TypeDefinitionHandle TypeDef => default(TypeDefinitionHandle);

	public bool IsAlias => false;

	public bool IsValueType => false;

	public PrimitiveTypeCode TypeCode => PrimitiveTypeCode.NotPrimitive;

	public ushort Index => _index;

	public virtual string? Name => _parentParameter.Name;

	ITypeReference IGenericTypeParameterReference.DefiningType => _inheritingType;

	public bool MangleName => false;

	public bool IsNested
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/PEWriter/InheritedTypeParameter.cs", 283);
		}
	}

	public bool IsSpecializedNested
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/PEWriter/InheritedTypeParameter.cs", 288);
		}
	}

	public ITypeReference UnspecializedVersion
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/PEWriter/InheritedTypeParameter.cs", 293);
		}
	}

	public bool IsNamespaceTypeReference
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/PEWriter/InheritedTypeParameter.cs", 298);
		}
	}

	public bool IsGenericTypeInstance
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/PEWriter/InheritedTypeParameter.cs", 303);
		}
	}

	internal InheritedTypeParameter(ushort index, ITypeDefinition inheritingType, IGenericTypeParameter parentParameter)
	{
		_index = index;
		_inheritingType = inheritingType;
		_parentParameter = parentParameter;
	}

	public virtual IEnumerable<TypeReferenceWithAttributes> GetConstraints(EmitContext context)
	{
		return _parentParameter.GetConstraints(context);
	}

	public INamespaceTypeDefinition? AsNamespaceTypeDefinition(EmitContext context)
	{
		return this as INamespaceTypeDefinition;
	}

	public INestedTypeDefinition? AsNestedTypeDefinition(EmitContext context)
	{
		return this as INestedTypeDefinition;
	}

	public ITypeDefinition? AsTypeDefinition(EmitContext context)
	{
		return this as ITypeDefinition;
	}

	public IDefinition? AsDefinition(EmitContext context)
	{
		return this;
	}

	ISymbolInternal? IReference.GetInternalSymbol()
	{
		return null;
	}

	public virtual IEnumerable<ICustomAttribute> GetAttributes(EmitContext context)
	{
		return _parentParameter.GetAttributes(context);
	}

	public void Dispatch(MetadataVisitor visitor)
	{
	}

	public ITypeDefinition GetResolvedType(EmitContext context)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/PEWriter/InheritedTypeParameter.cs", 235);
	}

	public sealed override bool Equals(object? obj)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/PEWriter/InheritedTypeParameter.cs", 309);
	}

	public sealed override int GetHashCode()
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/PEWriter/InheritedTypeParameter.cs", 315);
	}
}
