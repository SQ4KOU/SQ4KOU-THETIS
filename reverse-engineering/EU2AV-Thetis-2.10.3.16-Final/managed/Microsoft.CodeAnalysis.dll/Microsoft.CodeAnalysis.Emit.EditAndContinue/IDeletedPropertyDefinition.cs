using System.Reflection.Metadata;
using Microsoft.Cci;

namespace Microsoft.CodeAnalysis.Emit.EditAndContinue;

internal interface IDeletedPropertyDefinition : IPropertyDefinition, ISignature, ITypeDefinitionMember, ITypeMemberReference, IReference, INamedEntity, IDefinition
{
	PropertyDefinitionHandle MetadataHandle { get; }
}
