using System;
using FTD2XX_NET;

namespace Thetis;

internal class UsbBCDCable
{
	private const int BaudRate = 921600;

	private const byte BitMask = byte.MaxValue;

	private const byte BitMode = 4;

	private const int RelayCount = 8;

	private FTDI _device;

	private FTDI.FT_STATUS _status;

	private byte _values;

	private byte[] _writeBuffer = new byte[2];

	private byte[] _readBuffer = new byte[2];

	public byte Values => _values;

	public UsbBCDCable(string serialNumber)
	{
		_device = new FTDI();
		_status = _device.OpenBySerialNumber(serialNumber);
		if (_status != FTDI.FT_STATUS.FT_OK)
		{
			throw new UsbRelayDeviceNotFoundException();
		}
		_status = _device.SetBaudRate(921600u);
		if (_status != FTDI.FT_STATUS.FT_OK)
		{
			throw new UsbRelayConfigurationException();
		}
		_status = _device.SetBitMode(byte.MaxValue, 4);
		if (_status != FTDI.FT_STATUS.FT_OK)
		{
			throw new UsbRelayConfigurationException();
		}
		_values = 0;
		SetRelays(_values);
	}

	public byte GetRelayValues()
	{
		return _values;
	}

	public bool GetRelayValue(int relay)
	{
		byte relayValues = GetRelayValues();
		if (((byte)(~(byte)(1 << relay - 1)) & relayValues) > 0)
		{
			return true;
		}
		return false;
	}

	public void SetRelay(int relay, bool value)
	{
		if (relay < 0 || relay > 7)
		{
			throw new UsbRelayInvalidRelayException();
		}
		byte relayValues = GetRelayValues();
		byte b = (byte)Math.Pow(2.0, relay);
		relayValues = ((!value) ? ((byte)(relayValues & (byte)(~b))) : ((byte)(relayValues | b)));
		_values = relayValues;
		SetRelays(_values);
	}

	public void SetRelays(byte values)
	{
		_values = values;
		_writeBuffer[0] = values;
		uint numBytesWritten = 0u;
		_status = _device.Write(_writeBuffer, 1, ref numBytesWritten);
		if (_status != FTDI.FT_STATUS.FT_OK || numBytesWritten != 1)
		{
			throw new UsbRelayWriteException();
		}
	}

	public void CloseDevice()
	{
		if (_device.IsOpen)
		{
			_status = _device.Close();
		}
		_device = null;
	}
}
