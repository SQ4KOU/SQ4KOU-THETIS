using System.Diagnostics.CodeAnalysis;

namespace Microsoft.CodeAnalysis;

internal class CommonPropertyEarlyWellKnownAttributeData : EarlyWellKnownAttributeData
{
	private ObsoleteAttributeData _obsoleteAttributeData = Microsoft.CodeAnalysis.ObsoleteAttributeData.Uninitialized;

	private int _overloadResolutionPriority;

	public ObsoleteAttributeData? ObsoleteAttributeData
	{
		get
		{
			if (!_obsoleteAttributeData.IsUninitialized)
			{
				return _obsoleteAttributeData;
			}
			return null;
		}
		[param: DisallowNull]
		set
		{
			if (!PEModule.IsMoreImportantObsoleteKind(_obsoleteAttributeData.Kind, value.Kind))
			{
				_obsoleteAttributeData = value;
			}
		}
	}

	public int OverloadResolutionPriority
	{
		get
		{
			return _overloadResolutionPriority;
		}
		[param: DisallowNull]
		set
		{
			_overloadResolutionPriority = value;
		}
	}
}
