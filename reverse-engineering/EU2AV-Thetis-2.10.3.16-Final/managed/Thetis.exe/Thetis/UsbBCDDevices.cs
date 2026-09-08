using System.Collections.Generic;
using System.Linq;
using FTD2XX_NET;

namespace Thetis;

internal class UsbBCDDevices
{
	private FTDI _device;

	private FTDI.FT_STATUS _status;

	private FTDI.FT_DEVICE_INFO_NODE[] _deviceList;

	private Dictionary<string, UsbBCDCable> _relays;

	private List<string> _serialNumbers;

	private List<uint> _locationIds;

	private List<uint> _ids;

	public bool HasDevices
	{
		get
		{
			try
			{
				return GetDeviceCount() != 0;
			}
			catch (UsbRelayStatusException)
			{
				return false;
			}
		}
	}

	public int DeviceCount => GetDeviceCount();

	public FTDI.FT_DEVICE_INFO_NODE[] Devices
	{
		get
		{
			PopulateDeviceList();
			return _deviceList;
		}
	}

	public List<string> DeviceSerialNumbers
	{
		get
		{
			PopulateDeviceList();
			return _serialNumbers;
		}
	}

	public List<uint> DeviceLocationIDs
	{
		get
		{
			PopulateDeviceList();
			return _locationIds;
		}
	}

	public List<uint> DeviceIDs
	{
		get
		{
			PopulateDeviceList();
			return _ids;
		}
	}

	public UsbBCDDevices()
	{
		_device = new FTDI();
		_relays = new Dictionary<string, UsbBCDCable>();
	}

	public void OpenDevice(string serialNumber)
	{
		UsbBCDCable value = new UsbBCDCable(serialNumber);
		_relays.Add(serialNumber, value);
	}

	public void SetRelay(string serialNumber, int relay, bool value)
	{
		_relays[serialNumber].SetRelay(relay, value);
	}

	public void SetRelays(string serialNumber, byte values)
	{
		_relays[serialNumber].SetRelays(values);
	}

	public void SetBCDbyBand(string serialNumber, Band b)
	{
		byte values = 0;
		switch (b)
		{
		case Band.GEN:
		case Band.B60M:
		case Band.B2M:
		case Band.WWV:
			values = 0;
			break;
		case Band.B160M:
			values = 1;
			break;
		case Band.B80M:
			values = 2;
			break;
		case Band.B40M:
			values = 3;
			break;
		case Band.B30M:
			values = 4;
			break;
		case Band.B20M:
			values = 5;
			break;
		case Band.B17M:
			values = 6;
			break;
		case Band.B15M:
			values = 7;
			break;
		case Band.B12M:
			values = 8;
			break;
		case Band.B10M:
			values = 9;
			break;
		case Band.B6M:
			values = 10;
			break;
		case Band.VHF0:
		case Band.VHF1:
		case Band.VHF2:
		case Band.VHF3:
		case Band.VHF4:
		case Band.VHF5:
		case Band.VHF6:
		case Band.VHF7:
		case Band.VHF8:
		case Band.VHF9:
		case Band.VHF10:
		case Band.VHF11:
		case Band.VHF12:
		case Band.VHF13:
			values = 0;
			break;
		case Band.BLMF:
		case Band.B120M:
		case Band.B90M:
		case Band.B61M:
		case Band.B49M:
		case Band.B41M:
		case Band.B31M:
		case Band.B25M:
		case Band.B22M:
		case Band.B19M:
		case Band.B16M:
		case Band.B14M:
		case Band.B13M:
		case Band.B11M:
			values = 0;
			break;
		}
		SetRelays(serialNumber, values);
	}

	public bool GetRelay(string serialNumber, int relay)
	{
		return _relays[serialNumber].GetRelayValue(relay);
	}

	public byte GetRelays(string serialNumber)
	{
		return _relays[serialNumber].GetRelayValues();
	}

	public void CloseDevice(string serialNumber)
	{
		if (_device.IsOpen)
		{
			_status = _device.Close();
		}
		if (serialNumber != null && _relays.Count() != 0)
		{
			_relays[serialNumber].CloseDevice();
		}
	}

	public void ResetDevice()
	{
		if (_device.IsOpen)
		{
			_status = _device.ResetDevice();
		}
	}

	private int GetDeviceCount()
	{
		uint devcount = 0u;
		_status = _device.GetNumberOfDevices(ref devcount);
		if (_status != FTDI.FT_STATUS.FT_OK)
		{
			throw new UsbRelayStatusException();
		}
		return (int)devcount;
	}

	private void PopulateDeviceList()
	{
		int deviceCount = GetDeviceCount();
		_deviceList = new FTDI.FT_DEVICE_INFO_NODE[deviceCount];
		_status = _device.GetDeviceList(_deviceList);
		if (_status != FTDI.FT_STATUS.FT_OK)
		{
			throw new UsbRelayStatusException();
		}
		_serialNumbers = new List<string>();
		_locationIds = new List<uint>();
		_ids = new List<uint>();
		FTDI.FT_DEVICE_INFO_NODE[] deviceList = _deviceList;
		foreach (FTDI.FT_DEVICE_INFO_NODE fT_DEVICE_INFO_NODE in deviceList)
		{
			_serialNumbers.Add(fT_DEVICE_INFO_NODE.SerialNumber);
			_locationIds.Add(fT_DEVICE_INFO_NODE.LocId);
			_ids.Add(fT_DEVICE_INFO_NODE.ID);
		}
	}
}
