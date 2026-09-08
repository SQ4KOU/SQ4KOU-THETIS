using System.Runtime.InteropServices;
using NAudio.CoreAudioApi.Interfaces;

namespace NAudio.CoreAudioApi;

public class PropertyStore
{
	private readonly IPropertyStore storeInterface;

	public int Count
	{
		get
		{
			Marshal.ThrowExceptionForHR(storeInterface.GetCount(out var propCount));
			return propCount;
		}
	}

	public PropertyStoreProperty this[int index]
	{
		get
		{
			PropertyKey key = Get(index);
			Marshal.ThrowExceptionForHR(storeInterface.GetValue(ref key, out var value));
			return new PropertyStoreProperty(key, value);
		}
	}

	public PropertyStoreProperty this[PropertyKey key]
	{
		get
		{
			Marshal.ThrowExceptionForHR(storeInterface.GetValue(ref key, out var value));
			return new PropertyStoreProperty(key, value);
		}
	}

	public bool Contains(PropertyKey key)
	{
		if (storeInterface.GetValue(ref key, out var value) >= 0)
		{
			return value.vt != 0;
		}
		return false;
	}

	public bool TryGetValue<T>(PropertyKey key, out T obj)
	{
		obj = default(T);
		if (storeInterface.GetValue(ref key, out var value) < 0 || value.vt == 0)
		{
			return false;
		}
		obj = (T)value.Value;
		return true;
	}

	public PropertyKey Get(int index)
	{
		Marshal.ThrowExceptionForHR(storeInterface.GetAt(index, out var key));
		return key;
	}

	public PropVariant GetValue(int index)
	{
		PropertyKey key = Get(index);
		Marshal.ThrowExceptionForHR(storeInterface.GetValue(ref key, out var value));
		return value;
	}

	public void SetValue(PropertyKey key, PropVariant value)
	{
		Marshal.ThrowExceptionForHR(storeInterface.SetValue(ref key, ref value));
	}

	public void Commit()
	{
		Marshal.ThrowExceptionForHR(storeInterface.Commit());
	}

	internal PropertyStore(IPropertyStore store)
	{
		storeInterface = store;
	}
}
