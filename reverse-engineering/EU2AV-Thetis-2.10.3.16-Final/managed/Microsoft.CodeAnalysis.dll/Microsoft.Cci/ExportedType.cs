namespace Microsoft.Cci;

internal readonly struct ExportedType(ITypeReference type, int parentIndex, bool isForwarder)
{
	public readonly ITypeReference Type = type;

	public readonly bool IsForwarder = isForwarder;

	public readonly int ParentIndex = parentIndex;
}
