namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class ParameterEarlyWellKnownAttributeData : CommonParameterEarlyWellKnownAttributeData
{
	private bool _hasUnscopedRefAttribute;

	public bool HasUnscopedRefAttribute
	{
		get
		{
			return _hasUnscopedRefAttribute;
		}
		set
		{
			_hasUnscopedRefAttribute = value;
		}
	}
}
