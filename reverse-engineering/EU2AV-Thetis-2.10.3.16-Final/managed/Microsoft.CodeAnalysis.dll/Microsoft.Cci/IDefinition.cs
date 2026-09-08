namespace Microsoft.Cci;

internal interface IDefinition : IReference
{
	bool IsEncDeleted { get; }
}
