namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class MethodEarlyWellKnownAttributeData : CommonMethodEarlyWellKnownAttributeData
{
	private bool _unmanagedCallersOnlyAttributePresent;

	public bool UnmanagedCallersOnlyAttributePresent
	{
		get
		{
			return _unmanagedCallersOnlyAttributePresent;
		}
		set
		{
			_unmanagedCallersOnlyAttributePresent = value;
		}
	}
}
