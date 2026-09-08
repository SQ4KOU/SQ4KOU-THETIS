using System;
using System.Runtime.InteropServices;

namespace SkiaSharp;

public class GRVkBackendContext : IDisposable
{
	private GRVkGetProcedureAddressDelegate getProc;

	private GCHandle getProcHandle;

	private unsafe void* getProcContext;

	public IntPtr VkInstance { get; set; }

	public IntPtr VkPhysicalDevice { get; set; }

	public IntPtr VkDevice { get; set; }

	public IntPtr VkQueue { get; set; }

	public uint GraphicsQueueIndex { get; set; }

	public uint MaxAPIVersion { get; set; }

	public GRVkExtensions Extensions { get; set; }

	public IntPtr VkPhysicalDeviceFeatures { get; set; }

	public IntPtr VkPhysicalDeviceFeatures2 { get; set; }

	public unsafe GRVkGetProcedureAddressDelegate GetProcedureAddress
	{
		get
		{
			return getProc;
		}
		set
		{
			getProc = value;
			if (getProcHandle.IsAllocated)
			{
				getProcHandle.Free();
			}
			getProcHandle = default(GCHandle);
			getProcContext = null;
			if (value != null)
			{
				DelegateProxies.Create(value, out var gch, out var contextPtr);
				getProcHandle = gch;
				getProcContext = (void*)contextPtr;
			}
		}
	}

	public bool ProtectedContext { get; set; }

	protected virtual void Dispose(bool disposing)
	{
		if (disposing && getProcHandle.IsAllocated)
		{
			getProcHandle.Free();
			getProcHandle = default(GCHandle);
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	internal unsafe GRVkBackendContextNative ToNative()
	{
		return new GRVkBackendContextNative
		{
			fInstance = VkInstance,
			fDevice = VkDevice,
			fPhysicalDevice = VkPhysicalDevice,
			fQueue = VkQueue,
			fGraphicsQueueIndex = GraphicsQueueIndex,
			fMaxAPIVersion = MaxAPIVersion,
			fVkExtensions = (Extensions?.Handle ?? IntPtr.Zero),
			fDeviceFeatures = VkPhysicalDeviceFeatures,
			fDeviceFeatures2 = VkPhysicalDeviceFeatures2,
			fGetProcUserData = getProcContext,
			fGetProc = ((getProcContext != null) ? DelegateProxies.GRVkGetProcProxy : null),
			fProtectedContext = (ProtectedContext ? ((byte)1) : ((byte)0))
		};
	}
}
