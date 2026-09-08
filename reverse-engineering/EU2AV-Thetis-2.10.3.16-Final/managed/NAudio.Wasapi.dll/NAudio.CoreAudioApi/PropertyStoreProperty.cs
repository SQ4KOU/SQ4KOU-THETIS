using NAudio.CoreAudioApi.Interfaces;

namespace NAudio.CoreAudioApi;

public struct PropertyStoreProperty
{
	private PropVariant propertyValue;

	public PropertyKey Key { get; }

	public object Value => propertyValue.Value;

	internal PropertyStoreProperty(PropertyKey key, PropVariant value)
	{
		Key = key;
		propertyValue = value;
	}
}
