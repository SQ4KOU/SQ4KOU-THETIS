using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Cci;

internal interface ISpecializedPropertyDefinition : IPropertyDefinition, ISignature, ITypeDefinitionMember, ITypeMemberReference, IReference, INamedEntity, IDefinition
{
	IPropertyDefinition UnspecializedVersion
	{
		[return: NotNull]
		get;
	}
}
