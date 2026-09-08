namespace Microsoft.CodeAnalysis.CSharp.Symbols.Retargeting;

internal sealed class RetargetingExtensionReceiverParameterSymbol : RetargetingParameterSymbol
{
	private readonly RetargetingNamedTypeSymbol _retargetingType;

	protected override RetargetingModuleSymbol RetargetingModule => (RetargetingModuleSymbol)_retargetingType.ContainingModule;

	public override Symbol ContainingSymbol => _retargetingType;

	public RetargetingExtensionReceiverParameterSymbol(RetargetingNamedTypeSymbol retargetingType, ParameterSymbol underlyingParameter)
		: base(underlyingParameter)
	{
		_retargetingType = retargetingType;
	}
}
