namespace Microsoft.Cci;

internal interface INestedTypeReference : INamedTypeReference, ITypeReference, IReference, INamedEntity, ITypeMemberReference
{
	bool InheritsEnclosingTypeTypeParameters { get; }
}
