namespace Microsoft.Cci;

internal interface IGenericTypeParameter : IGenericParameter, IDefinition, IReference, IGenericParameterReference, ITypeReference, INamedEntity, IParameterListEntry, IGenericTypeParameterReference
{
	new ITypeDefinition DefiningType { get; }
}
