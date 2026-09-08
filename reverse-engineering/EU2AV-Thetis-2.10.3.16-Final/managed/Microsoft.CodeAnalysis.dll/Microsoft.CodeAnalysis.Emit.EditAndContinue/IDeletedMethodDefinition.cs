using System.Reflection.Metadata;
using Microsoft.Cci;

namespace Microsoft.CodeAnalysis.Emit.EditAndContinue;

internal interface IDeletedMethodDefinition : IMethodDefinition, ITypeDefinitionMember, ITypeMemberReference, IReference, INamedEntity, IDefinition, IMethodReference, ISignature
{
	MethodDefinitionHandle MetadataHandle { get; }
}
