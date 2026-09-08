using Microsoft.Cci;
using Microsoft.CodeAnalysis.Emit;

namespace Microsoft.CodeAnalysis.CodeGen;

internal abstract class NestedTypeDefinition : DefaultTypeDef, INestedTypeDefinition, INamedTypeDefinition, ITypeDefinition, IDefinition, IReference, ITypeReference, INamedTypeReference, INamedEntity, ITypeDefinitionMember, ITypeMemberReference, INestedTypeReference
{
	public abstract string Name { get; }

	public abstract ITypeDefinition ContainingTypeDefinition { get; }

	public abstract TypeMemberVisibility Visibility { get; }

	public sealed override INestedTypeReference AsNestedTypeReference => this;

	bool INestedTypeReference.InheritsEnclosingTypeTypeParameters => true;

	public sealed override string ToString()
	{
		return ContainingTypeDefinition.ToString() + "." + Name;
	}

	public sealed override void Dispatch(MetadataVisitor visitor)
	{
		visitor.Visit(this);
	}

	public ITypeReference GetContainingType(EmitContext context)
	{
		return ContainingTypeDefinition;
	}

	public sealed override INestedTypeDefinition AsNestedTypeDefinition(EmitContext context)
	{
		return this;
	}
}
