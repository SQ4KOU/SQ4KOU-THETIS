using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.Symbols;

namespace Microsoft.CodeAnalysis.Emit.EditAndContinue;

internal abstract class DeletedSourceDefinition<T> : IDefinition, IReference where T : IDefinition
{
	public readonly T OldDefinition;

	private readonly Dictionary<ITypeDefinition, DeletedSourceTypeDefinition> _typesUsedByDeletedMembers;

	private readonly IEnumerable<ICustomAttribute> _attributes;

	public bool IsEncDeleted => true;

	protected DeletedSourceDefinition(T oldDefinition, Dictionary<ITypeDefinition, DeletedSourceTypeDefinition> typesUsedByDeletedMembers, ICustomAttribute? deletedAttribute)
	{
		OldDefinition = oldDefinition;
		_typesUsedByDeletedMembers = typesUsedByDeletedMembers;
		IEnumerable<ICustomAttribute> attributes;
		if (deletedAttribute == null)
		{
			IEnumerable<ICustomAttribute> enumerable = Array.Empty<ICustomAttribute>();
			attributes = enumerable;
		}
		else
		{
			IEnumerable<ICustomAttribute> enumerable = new _003C_003Ez__ReadOnlySingleElementList<ICustomAttribute>(deletedAttribute);
			attributes = enumerable;
		}
		_attributes = attributes;
	}

	public IEnumerable<ICustomAttribute> GetAttributes(EmitContext context)
	{
		return _attributes;
	}

	public ISymbolInternal? GetInternalSymbol()
	{
		return OldDefinition.GetInternalSymbol();
	}

	public abstract void Dispatch(MetadataVisitor visitor);

	public IDefinition? AsDefinition(EmitContext context)
	{
		return this;
	}

	protected ImmutableArray<DeletedSourceParameterDefinition> WrapParameters(ImmutableArray<IParameterDefinition> parameters)
	{
		return parameters.SelectAsArray((IParameterDefinition p) => new DeletedSourceParameterDefinition(p, _typesUsedByDeletedMembers));
	}

	[return: NotNullIfNotNull("typeReference")]
	protected ITypeReference? WrapType(ITypeReference? typeReference)
	{
		if (typeReference is ITypeDefinition typeDefinition)
		{
			if (!_typesUsedByDeletedMembers.TryGetValue(typeDefinition, out DeletedSourceTypeDefinition value))
			{
				value = new DeletedSourceTypeDefinition(typeDefinition, _typesUsedByDeletedMembers);
				_typesUsedByDeletedMembers.Add(typeDefinition, value);
			}
			return value;
		}
		return typeReference;
	}
}
