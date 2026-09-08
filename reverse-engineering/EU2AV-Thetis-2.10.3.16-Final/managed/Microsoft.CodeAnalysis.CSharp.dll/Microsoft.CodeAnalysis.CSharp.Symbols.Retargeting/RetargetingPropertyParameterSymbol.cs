namespace Microsoft.CodeAnalysis.CSharp.Symbols.Retargeting;

internal sealed class RetargetingPropertyParameterSymbol : RetargetingParameterSymbol
{
	private readonly RetargetingPropertySymbol _retargetingProperty;

	protected override RetargetingModuleSymbol RetargetingModule => _retargetingProperty.RetargetingModule;

	public override Symbol ContainingSymbol => _retargetingProperty;

	public RetargetingPropertyParameterSymbol(RetargetingPropertySymbol retargetingProperty, ParameterSymbol underlyingParameter)
		: base(underlyingParameter)
	{
		_retargetingProperty = retargetingProperty;
	}
}
