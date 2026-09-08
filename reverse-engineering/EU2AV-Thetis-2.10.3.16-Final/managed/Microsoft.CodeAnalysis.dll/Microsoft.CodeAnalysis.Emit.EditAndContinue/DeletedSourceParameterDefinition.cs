using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CodeGen;

namespace Microsoft.CodeAnalysis.Emit.EditAndContinue;

internal sealed class DeletedSourceParameterDefinition : DeletedSourceDefinition<IParameterDefinition>, IParameterDefinition, IDefinition, IReference, INamedEntity, IParameterTypeInformation, IParameterListEntry
{
	public bool HasDefaultValue => OldDefinition.HasDefaultValue;

	public bool IsIn => OldDefinition.IsIn;

	public bool IsMarshalledExplicitly => OldDefinition.IsMarshalledExplicitly;

	public bool IsOptional => OldDefinition.IsOptional;

	public bool IsOut => OldDefinition.IsOut;

	public IMarshallingInformation? MarshallingInformation => OldDefinition.MarshallingInformation;

	public ImmutableArray<byte> MarshallingDescriptor => OldDefinition.MarshallingDescriptor;

	public string? Name => OldDefinition.Name;

	public ImmutableArray<ICustomModifier> CustomModifiers => OldDefinition.CustomModifiers;

	public ImmutableArray<ICustomModifier> RefCustomModifiers => OldDefinition.RefCustomModifiers;

	public bool IsByReference => OldDefinition.IsByReference;

	public ushort Index => OldDefinition.Index;

	public DeletedSourceParameterDefinition(IParameterDefinition oldParameter, Dictionary<ITypeDefinition, DeletedSourceTypeDefinition> typesUsedByDeletedMembers)
		: base(oldParameter, typesUsedByDeletedMembers, (ICustomAttribute?)null)
	{
	}

	public MetadataConstant? GetDefaultValue(EmitContext context)
	{
		return OldDefinition.GetDefaultValue(context);
	}

	public ITypeReference GetType(EmitContext context)
	{
		return WrapType(OldDefinition.GetType(context));
	}

	public override void Dispatch(MetadataVisitor visitor)
	{
		visitor.Visit(this);
	}
}
