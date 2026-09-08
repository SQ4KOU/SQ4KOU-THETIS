using System;
using System.Runtime.InteropServices;

namespace WindowsFirewallHelper;

public class FirewallProductRegistrationHandle : IDisposable
{
	private object _handle;

	public bool IsInvalid => _handle == null;

	internal FirewallProductRegistrationHandle(object handle)
	{
		if (handle == null || !handle.GetType().IsCOMObject)
		{
			throw new ArgumentException("Handle is empty or invalid.", "handle");
		}
		_handle = handle;
	}

	public void Dispose()
	{
		Release();
		GC.SuppressFinalize(this);
	}

	public void Release()
	{
		lock (this)
		{
			if (!IsInvalid)
			{
				Marshal.ReleaseComObject(_handle);
				_handle = null;
			}
		}
	}

	~FirewallProductRegistrationHandle()
	{
		Release();
	}
}
