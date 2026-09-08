using System;
using System.Runtime.InteropServices;
using NAudio.CoreAudioApi.Interfaces;

namespace NAudio.CoreAudioApi;

public class MMDevice : IDisposable
{
	private readonly IMMDevice deviceInterface;

	private PropertyStore propertyStore;

	private AudioMeterInformation audioMeterInformation;

	private AudioEndpointVolume audioEndpointVolume;

	private AudioSessionManager audioSessionManager;

	private DeviceTopology deviceTopology;

	private static readonly Guid IID_IAudioMeterInformation = new Guid("C02216F6-8C67-4B5B-9D00-D008E73E0064");

	private static readonly Guid IID_IAudioEndpointVolume = new Guid("5CDF2C82-841E-4546-9722-0CF74078229A");

	private static readonly Guid IID_IAudioClient = new Guid("1CB9AD4C-DBFA-4c32-B178-C2F568A703B2");

	private static readonly Guid IDD_IAudioSessionManager = new Guid("BFA971F1-4D5E-40BB-935E-967039BFBEE4");

	private static readonly Guid IDD_IDeviceTopology = new Guid("2A07407E-6497-4A18-9787-32F79BD0D98F");

	public AudioClient AudioClient => GetAudioClient();

	public AudioMeterInformation AudioMeterInformation
	{
		get
		{
			if (audioMeterInformation == null)
			{
				GetAudioMeterInformation();
			}
			return audioMeterInformation;
		}
	}

	public AudioEndpointVolume AudioEndpointVolume
	{
		get
		{
			if (audioEndpointVolume == null)
			{
				GetAudioEndpointVolume();
			}
			return audioEndpointVolume;
		}
	}

	public AudioSessionManager AudioSessionManager
	{
		get
		{
			if (audioSessionManager == null)
			{
				GetAudioSessionManager();
			}
			return audioSessionManager;
		}
	}

	public DeviceTopology DeviceTopology
	{
		get
		{
			if (deviceTopology == null)
			{
				GetDeviceTopology();
			}
			return deviceTopology;
		}
	}

	public PropertyStore Properties
	{
		get
		{
			EnsurePropertyStoreExists();
			return propertyStore;
		}
	}

	public string FriendlyName
	{
		get
		{
			EnsurePropertyStoreExists();
			if (!propertyStore.TryGetValue<string>(PropertyKeys.PKEY_Device_FriendlyName, out var obj))
			{
				return "Unknown";
			}
			return obj;
		}
	}

	public string DeviceFriendlyName
	{
		get
		{
			EnsurePropertyStoreExists();
			if (!propertyStore.TryGetValue<string>(PropertyKeys.PKEY_DeviceInterface_FriendlyName, out var obj))
			{
				return "Unknown";
			}
			return obj;
		}
	}

	public string IconPath
	{
		get
		{
			EnsurePropertyStoreExists();
			if (!propertyStore.TryGetValue<string>(PropertyKeys.PKEY_Device_IconPath, out var obj))
			{
				return "Unknown";
			}
			return obj;
		}
	}

	public string InstanceId
	{
		get
		{
			EnsurePropertyStoreExists();
			if (!propertyStore.TryGetValue<string>(PropertyKeys.PKEY_Device_InstanceId, out var obj))
			{
				return "Unknown";
			}
			return obj;
		}
	}

	public string ID
	{
		get
		{
			Marshal.ThrowExceptionForHR(deviceInterface.GetId(out var id));
			return id;
		}
	}

	public DataFlow DataFlow
	{
		get
		{
			(deviceInterface as IMMEndpoint).GetDataFlow(out var dataFlow);
			return dataFlow;
		}
	}

	public DeviceState State
	{
		get
		{
			Marshal.ThrowExceptionForHR(deviceInterface.GetState(out var state));
			return state;
		}
	}

	public void GetPropertyInformation(StorageAccessMode stgmAccess = StorageAccessMode.Read)
	{
		Marshal.ThrowExceptionForHR(deviceInterface.OpenPropertyStore(stgmAccess, out var properties));
		propertyStore = new PropertyStore(properties);
	}

	private AudioClient GetAudioClient()
	{
		Marshal.ThrowExceptionForHR(deviceInterface.Activate(in IID_IAudioClient, ClsCtx.ALL, IntPtr.Zero, out var interfacePointer));
		return new AudioClient(interfacePointer as IAudioClient);
	}

	private void GetAudioMeterInformation()
	{
		Marshal.ThrowExceptionForHR(deviceInterface.Activate(in IID_IAudioMeterInformation, ClsCtx.ALL, IntPtr.Zero, out var interfacePointer));
		audioMeterInformation = new AudioMeterInformation(interfacePointer as IAudioMeterInformation);
	}

	private void GetAudioEndpointVolume()
	{
		Marshal.ThrowExceptionForHR(deviceInterface.Activate(in IID_IAudioEndpointVolume, ClsCtx.ALL, IntPtr.Zero, out var interfacePointer));
		audioEndpointVolume = new AudioEndpointVolume(interfacePointer as IAudioEndpointVolume);
	}

	private void GetAudioSessionManager()
	{
		Marshal.ThrowExceptionForHR(deviceInterface.Activate(in IDD_IAudioSessionManager, ClsCtx.ALL, IntPtr.Zero, out var interfacePointer));
		audioSessionManager = new AudioSessionManager(interfacePointer as IAudioSessionManager);
	}

	private void GetDeviceTopology()
	{
		Marshal.ThrowExceptionForHR(deviceInterface.Activate(in IDD_IDeviceTopology, ClsCtx.ALL, IntPtr.Zero, out var interfacePointer));
		deviceTopology = new DeviceTopology(interfacePointer as IDeviceTopology);
	}

	private void EnsurePropertyStoreExists()
	{
		if (propertyStore == null)
		{
			GetPropertyInformation();
		}
	}

	internal MMDevice(IMMDevice realDevice)
	{
		deviceInterface = realDevice;
	}

	public override string ToString()
	{
		return FriendlyName;
	}

	public void Dispose()
	{
		audioEndpointVolume?.Dispose();
		audioSessionManager?.Dispose();
		GC.SuppressFinalize(this);
	}

	~MMDevice()
	{
		Dispose();
	}
}
