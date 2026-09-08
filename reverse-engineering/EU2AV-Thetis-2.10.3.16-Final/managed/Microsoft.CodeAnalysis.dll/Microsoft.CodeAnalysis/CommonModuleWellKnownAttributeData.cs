using System.Runtime.InteropServices;

namespace Microsoft.CodeAnalysis;

internal class CommonModuleWellKnownAttributeData : WellKnownAttributeData
{
	private bool _hasDebuggableAttribute;

	private byte _defaultCharacterSet;

	private ObsoleteAttributeData _experimentalAttributeData = ObsoleteAttributeData.Uninitialized;

	public bool HasDebuggableAttribute
	{
		get
		{
			return _hasDebuggableAttribute;
		}
		set
		{
			_hasDebuggableAttribute = value;
		}
	}

	internal CharSet DefaultCharacterSet
	{
		get
		{
			return (CharSet)_defaultCharacterSet;
		}
		set
		{
			_defaultCharacterSet = (byte)value;
		}
	}

	internal bool HasDefaultCharSetAttribute => _defaultCharacterSet != 0;

	public ObsoleteAttributeData ExperimentalAttributeData
	{
		get
		{
			if (!_experimentalAttributeData.IsUninitialized)
			{
				return _experimentalAttributeData;
			}
			return null;
		}
		set
		{
			_experimentalAttributeData = value;
		}
	}

	internal static bool IsValidCharSet(CharSet value)
	{
		if (value >= CharSet.None)
		{
			return value <= CharSet.Auto;
		}
		return false;
	}
}
