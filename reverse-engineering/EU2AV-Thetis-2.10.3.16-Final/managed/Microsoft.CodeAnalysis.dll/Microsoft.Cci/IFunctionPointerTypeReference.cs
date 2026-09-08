namespace Microsoft.Cci;

internal interface IFunctionPointerTypeReference : ITypeReference, IReference
{
	ISignature Signature { get; }
}
