using System.Collections.Generic;
using System.Reflection.Metadata;
using Microsoft.Cci;

namespace Microsoft.CodeAnalysis.Emit.EditAndContinue;

internal sealed class DeletedSourceGenericParameter : DeletedSourceDefinition<IGenericMethodParameter>, IGenericMethodParameter, IGenericParameter, IDefinition, IReference, IGenericParameterReference, ITypeReference, INamedEntity, IParameterListEntry, IGenericMethodParameterReference
{
	private readonly DeletedSourceMethodDefinition _method;

	public IMethodDefinition DefiningMethod => _method;

	public bool MustBeReferenceType => OldDefinition.MustBeReferenceType;

	public bool MustBeValueType => OldDefinition.MustBeValueType;

	public bool AllowsRefLikeType => OldDefinition.AllowsRefLikeType;

	public bool MustHaveDefaultConstructor => OldDefinition.MustHaveDefaultConstructor;

	public TypeParameterVariance Variance => OldDefinition.Variance;

	public IGenericMethodParameter? AsGenericMethodParameter => OldDefinition.AsGenericMethodParameter;

	public IGenericTypeParameter? AsGenericTypeParameter => OldDefinition.AsGenericTypeParameter;

	public bool IsEnum => OldDefinition.IsEnum;

	public bool IsValueType => OldDefinition.IsValueType;

	public Microsoft.Cci.PrimitiveTypeCode TypeCode => OldDefinition.TypeCode;

	public TypeDefinitionHandle TypeDef => OldDefinition.TypeDef;

	public IGenericMethodParameterReference? AsGenericMethodParameterReference => OldDefinition.AsGenericMethodParameterReference;

	public IGenericTypeInstanceReference? AsGenericTypeInstanceReference => OldDefinition.AsGenericTypeInstanceReference;

	public IGenericTypeParameterReference? AsGenericTypeParameterReference => OldDefinition.AsGenericTypeParameterReference;

	public INamespaceTypeReference? AsNamespaceTypeReference => OldDefinition.AsNamespaceTypeReference;

	public INestedTypeReference? AsNestedTypeReference => OldDefinition.AsNestedTypeReference;

	public ISpecializedNestedTypeReference? AsSpecializedNestedTypeReference => OldDefinition.AsSpecializedNestedTypeReference;

	public string? Name => OldDefinition.Name;

	public ushort Index => OldDefinition.Index;

	IMethodReference IGenericMethodParameterReference.DefiningMethod => ((IGenericMethodParameterReference)OldDefinition).DefiningMethod;

	public DeletedSourceGenericParameter(IGenericMethodParameter oldParameter, DeletedSourceMethodDefinition method, Dictionary<ITypeDefinition, DeletedSourceTypeDefinition> typesUsedByDeletedMembers)
		: base(oldParameter, typesUsedByDeletedMembers, (ICustomAttribute?)null)
	{
		_method = method;
	}

	public INamespaceTypeDefinition? AsNamespaceTypeDefinition(EmitContext context)
	{
		return OldDefinition.AsNamespaceTypeDefinition(context);
	}

	public INestedTypeDefinition? AsNestedTypeDefinition(EmitContext context)
	{
		return OldDefinition.AsNestedTypeDefinition(context);
	}

	public ITypeDefinition? AsTypeDefinition(EmitContext context)
	{
		return OldDefinition.AsTypeDefinition(context);
	}

	public override void Dispatch(MetadataVisitor visitor)
	{
		OldDefinition.Dispatch(visitor);
	}

	public IEnumerable<TypeReferenceWithAttributes> GetConstraints(EmitContext context)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/Emit/EditAndContinue/DeletedSourceGenericParameter.cs", 86);
	}

	public ITypeDefinition? GetResolvedType(EmitContext context)
	{
		return (ITypeDefinition)WrapType(OldDefinition.GetResolvedType(context));
	}
}
