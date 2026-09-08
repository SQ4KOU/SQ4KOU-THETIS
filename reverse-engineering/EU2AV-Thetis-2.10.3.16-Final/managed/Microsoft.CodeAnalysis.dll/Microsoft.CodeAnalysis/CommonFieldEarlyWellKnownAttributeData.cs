namespace Microsoft.CodeAnalysis;

internal class CommonFieldEarlyWellKnownAttributeData : EarlyWellKnownAttributeData
{
	private ObsoleteAttributeData _obsoleteAttributeData = ObsoleteAttributeData.Uninitialized;

	public ObsoleteAttributeData ObsoleteAttributeData
	{
		get
		{
			if (!_obsoleteAttributeData.IsUninitialized)
			{
				return _obsoleteAttributeData;
			}
			return null;
		}
		set
		{
			if (!PEModule.IsMoreImportantObsoleteKind(_obsoleteAttributeData.Kind, value.Kind))
			{
				_obsoleteAttributeData = value;
			}
		}
	}
}
