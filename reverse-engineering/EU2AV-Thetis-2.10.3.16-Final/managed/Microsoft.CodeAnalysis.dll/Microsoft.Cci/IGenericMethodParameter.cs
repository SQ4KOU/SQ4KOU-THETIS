namespace Microsoft.Cci;

internal interface IGenericMethodParameter : IGenericParameter, IDefinition, IReference, IGenericParameterReference, ITypeReference, INamedEntity, IParameterListEntry, IGenericMethodParameterReference
{
	new IMethodDefinition DefiningMethod { get; }
}
