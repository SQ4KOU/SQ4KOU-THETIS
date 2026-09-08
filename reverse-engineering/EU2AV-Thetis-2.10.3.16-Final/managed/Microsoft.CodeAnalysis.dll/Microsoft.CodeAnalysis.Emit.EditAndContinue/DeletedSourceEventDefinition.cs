using System.Collections.Generic;
using System.Reflection.Metadata;
using Microsoft.Cci;

namespace Microsoft.CodeAnalysis.Emit.EditAndContinue;

internal sealed class DeletedSourceEventDefinition : DeletedSourceDefinition<IEventDefinition>, IDeletedEventDefinition, IEventDefinition, ITypeDefinitionMember, ITypeMemberReference, IReference, INamedEntity, IDefinition
{
	private readonly EventDefinitionHandle _handle;

	public EventDefinitionHandle MetadataHandle => _handle;

	public bool IsRuntimeSpecial => OldDefinition.IsRuntimeSpecial;

	public bool IsSpecialName => OldDefinition.IsSpecialName;

	public TypeMemberVisibility Visibility => OldDefinition.Visibility;

	public string? Name => OldDefinition.Name;

	public ITypeDefinition ContainingTypeDefinition
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/Emit/EditAndContinue/DeletedSourceEventDefinition.cs", 34);
		}
	}

	public IMethodReference Adder
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/Emit/EditAndContinue/DeletedSourceEventDefinition.cs", 47);
		}
	}

	public IMethodReference? Caller
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/Emit/EditAndContinue/DeletedSourceEventDefinition.cs", 48);
		}
	}

	public IMethodReference Remover
	{
		get
		{
			throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/Emit/EditAndContinue/DeletedSourceEventDefinition.cs", 49);
		}
	}

	public DeletedSourceEventDefinition(IEventDefinition oldEvent, EventDefinitionHandle handle, Dictionary<ITypeDefinition, DeletedSourceTypeDefinition> typesUsedByDeletedMembers, ICustomAttribute? deletedAttribute)
		: base(oldEvent, typesUsedByDeletedMembers, deletedAttribute)
	{
		_handle = handle;
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
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/Emit/EditAndContinue/DeletedSourceEventDefinition.cs", 52);
	}

	public ITypeReference GetContainingType(EmitContext context)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/Emit/EditAndContinue/DeletedSourceEventDefinition.cs", 55);
	}

	public sealed override bool Equals(object? obj)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/Emit/EditAndContinue/DeletedSourceEventDefinition.cs", 60);
	}

	public sealed override int GetHashCode()
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/Core/Portable/Emit/EditAndContinue/DeletedSourceEventDefinition.cs", 66);
	}
}
