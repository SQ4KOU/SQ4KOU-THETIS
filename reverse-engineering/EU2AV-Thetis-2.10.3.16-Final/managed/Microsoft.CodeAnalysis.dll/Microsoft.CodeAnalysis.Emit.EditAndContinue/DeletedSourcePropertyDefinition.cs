using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection.Metadata;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CodeGen;

namespace Microsoft.CodeAnalysis.Emit.EditAndContinue;

internal sealed class DeletedSourcePropertyDefinition : DeletedSourceDefinition<IPropertyDefinition>, IDeletedPropertyDefinition, IPropertyDefinition, ISignature, ITypeDefinitionMember, ITypeMemberReference, IReference, INamedEntity, IDefinition
{
	private readonly PropertyDefinitionHandle _handle;

	private readonly ImmutableArray<DeletedSourceParameterDefinition> _parameters;

	public PropertyDefinitionHandle MetadataHandle => _handle;

	public bool IsRuntimeSpecial => OldDefinition.IsRuntimeSpecial;

	public bool IsSpecialName => OldDefinition.IsSpecialName;

	public ImmutableArray<IParameterDefinition> Parameters => StaticCast<IParameterDefinition>.From(_parameters);

	public TypeMemberVisibility Visibility => OldDefinition.Visibility;

	public CallingConvention CallingConvention => OldDefinition.CallingConvention;

	public ushort ParameterCount => (ushort)_parameters.Length;

	public ImmutableArray<ICustomModifier> ReturnValueCustomModifiers => OldDefinition.ReturnValueCustomModifiers;

	public ImmutableArray<ICustomModifier> RefCustomModifiers => OldDefinition.RefCustomModifiers;

	public bool ReturnValueIsByRef => OldDefinition.ReturnValueIsByRef;

	public string? Name => OldDefinition.Name;

	public MetadataConstant? DefaultValue => OldDefinition.DefaultValue;

	public bool HasDefaultValue => OldDefinition.HasDefaultValue;

	public ITypeDefinition ContainingTypeDefinition
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/Emit/EditAndContinue/DeletedSourcePropertyDefinition.cs", 53);
		}
	}

	public IMethodReference? Getter
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/Emit/EditAndContinue/DeletedSourcePropertyDefinition.cs", 67);
		}
	}

	public IMethodReference? Setter
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/Emit/EditAndContinue/DeletedSourcePropertyDefinition.cs", 70);
		}
	}

	public DeletedSourcePropertyDefinition(IPropertyDefinition oldProperty, PropertyDefinitionHandle handle, Dictionary<ITypeDefinition, DeletedSourceTypeDefinition> typesUsedByDeletedMembers, ICustomAttribute? deletedAttribute)
		: base(oldProperty, typesUsedByDeletedMembers, deletedAttribute)
	{
		_handle = handle;
		_parameters = WrapParameters(oldProperty.Parameters);
	}

	public override void Dispatch(MetadataVisitor visitor)
	{
		visitor.Visit(this);
	}

	public ITypeReference GetType(EmitContext context)
	{
		return WrapType(OldDefinition.GetType(context));
	}

	public IEnumerable<IMethodReference> GetAccessors(EmitContext context)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/Emit/EditAndContinue/DeletedSourcePropertyDefinition.cs", 73);
	}

	public ImmutableArray<IParameterTypeInformation> GetParameters(EmitContext context)
	{
		return StaticCast<IParameterTypeInformation>.From(_parameters);
	}

	public ITypeReference GetContainingType(EmitContext context)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/Emit/EditAndContinue/DeletedSourcePropertyDefinition.cs", 79);
	}

	public sealed override bool Equals(object? obj)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/Emit/EditAndContinue/DeletedSourcePropertyDefinition.cs", 84);
	}

	public sealed override int GetHashCode()
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/Emit/EditAndContinue/DeletedSourcePropertyDefinition.cs", 90);
	}
}
