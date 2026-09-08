using System.Reflection.Metadata;
using Microsoft.Cci;

namespace Microsoft.CodeAnalysis.Emit.EditAndContinue;

internal interface IDeletedEventDefinition : IEventDefinition, ITypeDefinitionMember, ITypeMemberReference, IReference, INamedEntity, IDefinition
{
	EventDefinitionHandle MetadataHandle { get; }
}
