using System;

namespace SkiaSharp;

public class GRMtlBackendContext : IDisposable
{
	private IntPtr _deviceHandle;

	private IntPtr _queueHandle;

	public IntPtr DeviceHandle
	{
		get
		{
			return _deviceHandle;
		}
		set
		{
			_deviceHandle = value;
		}
	}

	public IntPtr QueueHandle
	{
		get
		{
			return _queueHandle;
		}
		set
		{
			_queueHandle = value;
		}
	}

	protected virtual void Dispose(bool disposing)
	{
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
