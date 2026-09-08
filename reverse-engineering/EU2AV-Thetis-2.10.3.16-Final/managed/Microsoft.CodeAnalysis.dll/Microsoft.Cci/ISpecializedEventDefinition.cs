using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Cci;

internal interface ISpecializedEventDefinition : IEventDefinition, ITypeDefinitionMember, ITypeMemberReference, IReference, INamedEntity, IDefinition
{
	IEventDefinition UnspecializedVersion
	{
		[return: NotNull]
		get;
	}
}
