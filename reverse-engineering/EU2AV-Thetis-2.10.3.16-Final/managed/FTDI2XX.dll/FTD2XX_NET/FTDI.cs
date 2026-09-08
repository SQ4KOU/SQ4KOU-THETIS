using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;

namespace FTD2XX_NET;

public class FTDI
{
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	private delegate FT_STATUS tFT_CreateDeviceInfoList(ref uint numdevs);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	private delegate FT_STATUS tFT_GetDeviceInfoDetail(uint index, ref uint flags, ref FT_DEVICE chiptype, ref uint id, ref uint locid, byte[] serialnumber, byte[] description, ref IntPtr ftHandle);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	private delegate FT_STATUS tFT_Open(uint index, ref IntPtr ftHandle);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	private delegate FT_STATUS tFT_OpenEx(string devstring, uint dwFlags, ref IntPtr ftHandle);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	private delegate FT_STATUS tFT_OpenExLoc(uint devloc, uint dwFlags, ref IntPtr ftHandle);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	private delegate FT_STATUS tFT_Close(IntPtr ftHandle);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	private delegate FT_STATUS tFT_Read(IntPtr ftHandle, byte[] lpBuffer, uint dwBytesToRead, ref uint lpdwBytesReturned);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	private delegate FT_STATUS tFT_Write(IntPtr ftHandle, byte[] lpBuffer, uint dwBytesToWrite, ref uint lpdwBytesWritten);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	private delegate FT_STATUS tFT_GetQueueStatus(IntPtr ftHandle, ref uint lpdwAmountInRxQueue);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	private delegate FT_STATUS tFT_GetModemStatus(IntPtr ftHandle, ref uint lpdwModemStatus);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	private delegate FT_STATUS tFT_GetStatus(IntPtr ftHandle, ref uint lpdwAmountInRxQueue, ref uint lpdwAmountInTxQueue, ref uint lpdwEventStatus);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	private delegate FT_STATUS tFT_SetBaudRate(IntPtr ftHandle, uint dwBaudRate);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	private delegate FT_STATUS tFT_SetDataCharacteristics(IntPtr ftHandle, byte uWordLength, byte uStopBits, byte uParity);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	private delegate FT_STATUS tFT_SetFlowControl(IntPtr ftHandle, ushort usFlowControl, byte uXon, byte uXoff);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	private delegate FT_STATUS tFT_SetDtr(IntPtr ftHandle);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	private delegate FT_STATUS tFT_ClrDtr(IntPtr ftHandle);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	private delegate FT_STATUS tFT_SetRts(IntPtr ftHandle);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	private delegate FT_STATUS tFT_ClrRts(IntPtr ftHandle);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	private delegate FT_STATUS tFT_ResetDevice(IntPtr ftHandle);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	private delegate FT_STATUS tFT_ResetPort(IntPtr ftHandle);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	private delegate FT_STATUS tFT_CyclePort(IntPtr ftHandle);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	private delegate FT_STATUS tFT_Rescan();

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	private delegate FT_STATUS tFT_Reload(ushort wVID, ushort wPID);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	private delegate FT_STATUS tFT_Purge(IntPtr ftHandle, uint dwMask);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	private delegate FT_STATUS tFT_SetTimeouts(IntPtr ftHandle, uint dwReadTimeout, uint dwWriteTimeout);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	private delegate FT_STATUS tFT_SetBreakOn(IntPtr ftHandle);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	private delegate FT_STATUS tFT_SetBreakOff(IntPtr ftHandle);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	private delegate FT_STATUS tFT_GetDeviceInfo(IntPtr ftHandle, ref FT_DEVICE pftType, ref uint lpdwID, byte[] pcSerialNumber, byte[] pcDescription, IntPtr pvDummy);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	private delegate FT_STATUS tFT_SetResetPipeRetryCount(IntPtr ftHandle, uint dwCount);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	private delegate FT_STATUS tFT_StopInTask(IntPtr ftHandle);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	private delegate FT_STATUS tFT_RestartInTask(IntPtr ftHandle);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	private delegate FT_STATUS tFT_GetDriverVersion(IntPtr ftHandle, ref uint lpdwDriverVersion);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	private delegate FT_STATUS tFT_GetLibraryVersion(ref uint lpdwLibraryVersion);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	private delegate FT_STATUS tFT_SetDeadmanTimeout(IntPtr ftHandle, uint dwDeadmanTimeout);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	private delegate FT_STATUS tFT_SetChars(IntPtr ftHandle, byte uEventCh, byte uEventChEn, byte uErrorCh, byte uErrorChEn);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	private delegate FT_STATUS tFT_SetEventNotification(IntPtr ftHandle, uint dwEventMask, SafeHandle hEvent);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	private delegate FT_STATUS tFT_GetComPortNumber(IntPtr ftHandle, ref int dwComPortNumber);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	private delegate FT_STATUS tFT_SetLatencyTimer(IntPtr ftHandle, byte ucLatency);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	private delegate FT_STATUS tFT_GetLatencyTimer(IntPtr ftHandle, ref byte ucLatency);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	private delegate FT_STATUS tFT_SetBitMode(IntPtr ftHandle, byte ucMask, byte ucMode);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	private delegate FT_STATUS tFT_GetBitMode(IntPtr ftHandle, ref byte ucMode);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	private delegate FT_STATUS tFT_SetUSBParameters(IntPtr ftHandle, uint dwInTransferSize, uint dwOutTransferSize);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	private delegate FT_STATUS tFT_ReadEE(IntPtr ftHandle, uint dwWordOffset, ref ushort lpwValue);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	private delegate FT_STATUS tFT_WriteEE(IntPtr ftHandle, uint dwWordOffset, ushort wValue);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	private delegate FT_STATUS tFT_EraseEE(IntPtr ftHandle);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	private delegate FT_STATUS tFT_EE_UASize(IntPtr ftHandle, ref uint dwSize);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	private delegate FT_STATUS tFT_EE_UARead(IntPtr ftHandle, byte[] pucData, int dwDataLen, ref uint lpdwDataRead);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	private delegate FT_STATUS tFT_EE_UAWrite(IntPtr ftHandle, byte[] pucData, int dwDataLen);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	private delegate FT_STATUS tFT_EE_Read(IntPtr ftHandle, FT_PROGRAM_DATA pData);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	private delegate FT_STATUS tFT_EE_Program(IntPtr ftHandle, FT_PROGRAM_DATA pData);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	private delegate FT_STATUS tFT_EEPROM_Read(IntPtr ftHandle, IntPtr eepromData, uint eepromDataSize, byte[] manufacturer, byte[] manufacturerID, byte[] description, byte[] serialnumber);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	private delegate FT_STATUS tFT_EEPROM_Program(IntPtr ftHandle, IntPtr eepromData, uint eepromDataSize, byte[] manufacturer, byte[] manufacturerID, byte[] description, byte[] serialnumber);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	private delegate FT_STATUS tFT_VendorCmdGet(IntPtr ftHandle, ushort request, byte[] buf, ushort len);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	private delegate FT_STATUS tFT_VendorCmdSet(IntPtr ftHandle, ushort request, byte[] buf, ushort len);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	private delegate FT_STATUS tFT_VendorCmdSetX(IntPtr ftHandle, ushort request, byte[] buf, ushort len);

	public enum FT_STATUS
	{
		FT_OK,
		FT_INVALID_HANDLE,
		FT_DEVICE_NOT_FOUND,
		FT_DEVICE_NOT_OPENED,
		FT_IO_ERROR,
		FT_INSUFFICIENT_RESOURCES,
		FT_INVALID_PARAMETER,
		FT_INVALID_BAUD_RATE,
		FT_DEVICE_NOT_OPENED_FOR_ERASE,
		FT_DEVICE_NOT_OPENED_FOR_WRITE,
		FT_FAILED_TO_WRITE_DEVICE,
		FT_EEPROM_READ_FAILED,
		FT_EEPROM_WRITE_FAILED,
		FT_EEPROM_ERASE_FAILED,
		FT_EEPROM_NOT_PRESENT,
		FT_EEPROM_NOT_PROGRAMMED,
		FT_INVALID_ARGS,
		FT_OTHER_ERROR
	}

	private enum FT_ERROR
	{
		FT_NO_ERROR,
		FT_INCORRECT_DEVICE,
		FT_INVALID_BITMODE,
		FT_BUFFER_SIZE
	}

	public class FT_DATA_BITS
	{
		public const byte FT_BITS_8 = 8;

		public const byte FT_BITS_7 = 7;
	}

	public class FT_STOP_BITS
	{
		public const byte FT_STOP_BITS_1 = 0;

		public const byte FT_STOP_BITS_2 = 2;
	}

	public class FT_PARITY
	{
		public const byte FT_PARITY_NONE = 0;

		public const byte FT_PARITY_ODD = 1;

		public const byte FT_PARITY_EVEN = 2;

		public const byte FT_PARITY_MARK = 3;

		public const byte FT_PARITY_SPACE = 4;
	}

	public class FT_FLOW_CONTROL
	{
		public const ushort FT_FLOW_NONE = 0;

		public const ushort FT_FLOW_RTS_CTS = 256;

		public const ushort FT_FLOW_DTR_DSR = 512;

		public const ushort FT_FLOW_XON_XOFF = 1024;
	}

	public class FT_PURGE
	{
		public const byte FT_PURGE_RX = 1;

		public const byte FT_PURGE_TX = 2;
	}

	public class FT_MODEM_STATUS
	{
		public const byte FT_CTS = 16;

		public const byte FT_DSR = 32;

		public const byte FT_RI = 64;

		public const byte FT_DCD = 128;
	}

	public class FT_LINE_STATUS
	{
		public const byte FT_OE = 2;

		public const byte FT_PE = 4;

		public const byte FT_FE = 8;

		public const byte FT_BI = 16;
	}

	public class FT_EVENTS
	{
		public const uint FT_EVENT_RXCHAR = 1u;

		public const uint FT_EVENT_MODEM_STATUS = 2u;

		public const uint FT_EVENT_LINE_STATUS = 4u;
	}

	public class FT_BIT_MODES
	{
		public const byte FT_BIT_MODE_RESET = 0;

		public const byte FT_BIT_MODE_ASYNC_BITBANG = 1;

		public const byte FT_BIT_MODE_MPSSE = 2;

		public const byte FT_BIT_MODE_SYNC_BITBANG = 4;

		public const byte FT_BIT_MODE_MCU_HOST = 8;

		public const byte FT_BIT_MODE_FAST_SERIAL = 16;

		public const byte FT_BIT_MODE_CBUS_BITBANG = 32;

		public const byte FT_BIT_MODE_SYNC_FIFO = 64;
	}

	public class FT_CBUS_OPTIONS
	{
		public const byte FT_CBUS_TXDEN = 0;

		public const byte FT_CBUS_PWRON = 1;

		public const byte FT_CBUS_RXLED = 2;

		public const byte FT_CBUS_TXLED = 3;

		public const byte FT_CBUS_TXRXLED = 4;

		public const byte FT_CBUS_SLEEP = 5;

		public const byte FT_CBUS_CLK48 = 6;

		public const byte FT_CBUS_CLK24 = 7;

		public const byte FT_CBUS_CLK12 = 8;

		public const byte FT_CBUS_CLK6 = 9;

		public const byte FT_CBUS_IOMODE = 10;

		public const byte FT_CBUS_BITBANG_WR = 11;

		public const byte FT_CBUS_BITBANG_RD = 12;
	}

	public class FT_232H_CBUS_OPTIONS
	{
		public const byte FT_CBUS_TRISTATE = 0;

		public const byte FT_CBUS_RXLED = 1;

		public const byte FT_CBUS_TXLED = 2;

		public const byte FT_CBUS_TXRXLED = 3;

		public const byte FT_CBUS_PWREN = 4;

		public const byte FT_CBUS_SLEEP = 5;

		public const byte FT_CBUS_DRIVE_0 = 6;

		public const byte FT_CBUS_DRIVE_1 = 7;

		public const byte FT_CBUS_IOMODE = 8;

		public const byte FT_CBUS_TXDEN = 9;

		public const byte FT_CBUS_CLK30 = 10;

		public const byte FT_CBUS_CLK15 = 11;

		public const byte FT_CBUS_CLK7_5 = 12;
	}

	public class FT_XSERIES_CBUS_OPTIONS
	{
		public const byte FT_CBUS_TRISTATE = 0;

		public const byte FT_CBUS_RXLED = 1;

		public const byte FT_CBUS_TXLED = 2;

		public const byte FT_CBUS_TXRXLED = 3;

		public const byte FT_CBUS_PWREN = 4;

		public const byte FT_CBUS_SLEEP = 5;

		public const byte FT_CBUS_Drive_0 = 6;

		public const byte FT_CBUS_Drive_1 = 7;

		public const byte FT_CBUS_GPIO = 8;

		public const byte FT_CBUS_TXDEN = 9;

		public const byte FT_CBUS_CLK24MHz = 10;

		public const byte FT_CBUS_CLK12MHz = 11;

		public const byte FT_CBUS_CLK6MHz = 12;

		public const byte FT_CBUS_BCD_Charger = 13;

		public const byte FT_CBUS_BCD_Charger_N = 14;

		public const byte FT_CBUS_I2C_TXE = 15;

		public const byte FT_CBUS_I2C_RXF = 16;

		public const byte FT_CBUS_VBUS_Sense = 17;

		public const byte FT_CBUS_BitBang_WR = 18;

		public const byte FT_CBUS_BitBang_RD = 19;

		public const byte FT_CBUS_Time_Stamp = 20;

		public const byte FT_CBUS_Keep_Awake = 21;
	}

	public class FT_FLAGS
	{
		public const uint FT_FLAGS_OPENED = 1u;

		public const uint FT_FLAGS_HISPEED = 2u;
	}

	public class FT_DRIVE_CURRENT
	{
		public const byte FT_DRIVE_CURRENT_4MA = 4;

		public const byte FT_DRIVE_CURRENT_8MA = 8;

		public const byte FT_DRIVE_CURRENT_12MA = 12;

		public const byte FT_DRIVE_CURRENT_16MA = 16;
	}

	public enum FT_DEVICE
	{
		FT_DEVICE_BM,
		FT_DEVICE_AM,
		FT_DEVICE_100AX,
		FT_DEVICE_UNKNOWN,
		FT_DEVICE_2232,
		FT_DEVICE_232R,
		FT_DEVICE_2232H,
		FT_DEVICE_4232H,
		FT_DEVICE_232H,
		FT_DEVICE_X_SERIES,
		FT_DEVICE_4222H_0,
		FT_DEVICE_4222H_1_2,
		FT_DEVICE_4222H_3,
		FT_DEVICE_4222_PROG,
		FT_DEVICE_FT900,
		FT_DEVICE_FT930,
		FT_DEVICE_UMFTPD3A,
		FT_DEVICE_2233HP,
		FT_DEVICE_4233HP,
		FT_DEVICE_2232HP,
		FT_DEVICE_4232HP,
		FT_DEVICE_233HP,
		FT_DEVICE_232HP,
		FT_DEVICE_2232HA,
		FT_DEVICE_4232HA
	}

	public class FT_DEVICE_INFO_NODE
	{
		public uint Flags;

		public FT_DEVICE Type;

		public uint ID;

		public uint LocId;

		public string SerialNumber;

		public string Description;

		public IntPtr ftHandle;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	private class FT_PROGRAM_DATA
	{
		public uint Signature1;

		public uint Signature2;

		public uint Version;

		public ushort VendorID;

		public ushort ProductID;

		public IntPtr Manufacturer;

		public IntPtr ManufacturerID;

		public IntPtr Description;

		public IntPtr SerialNumber;

		public ushort MaxPower;

		public ushort PnP;

		public ushort SelfPowered;

		public ushort RemoteWakeup;

		public byte Rev4;

		public byte IsoIn;

		public byte IsoOut;

		public byte PullDownEnable;

		public byte SerNumEnable;

		public byte USBVersionEnable;

		public ushort USBVersion;

		public byte Rev5;

		public byte IsoInA;

		public byte IsoInB;

		public byte IsoOutA;

		public byte IsoOutB;

		public byte PullDownEnable5;

		public byte SerNumEnable5;

		public byte USBVersionEnable5;

		public ushort USBVersion5;

		public byte AIsHighCurrent;

		public byte BIsHighCurrent;

		public byte IFAIsFifo;

		public byte IFAIsFifoTar;

		public byte IFAIsFastSer;

		public byte AIsVCP;

		public byte IFBIsFifo;

		public byte IFBIsFifoTar;

		public byte IFBIsFastSer;

		public byte BIsVCP;

		public byte UseExtOsc;

		public byte HighDriveIOs;

		public byte EndpointSize;

		public byte PullDownEnableR;

		public byte SerNumEnableR;

		public byte InvertTXD;

		public byte InvertRXD;

		public byte InvertRTS;

		public byte InvertCTS;

		public byte InvertDTR;

		public byte InvertDSR;

		public byte InvertDCD;

		public byte InvertRI;

		public byte Cbus0;

		public byte Cbus1;

		public byte Cbus2;

		public byte Cbus3;

		public byte Cbus4;

		public byte RIsD2XX;

		public byte PullDownEnable7;

		public byte SerNumEnable7;

		public byte ALSlowSlew;

		public byte ALSchmittInput;

		public byte ALDriveCurrent;

		public byte AHSlowSlew;

		public byte AHSchmittInput;

		public byte AHDriveCurrent;

		public byte BLSlowSlew;

		public byte BLSchmittInput;

		public byte BLDriveCurrent;

		public byte BHSlowSlew;

		public byte BHSchmittInput;

		public byte BHDriveCurrent;

		public byte IFAIsFifo7;

		public byte IFAIsFifoTar7;

		public byte IFAIsFastSer7;

		public byte AIsVCP7;

		public byte IFBIsFifo7;

		public byte IFBIsFifoTar7;

		public byte IFBIsFastSer7;

		public byte BIsVCP7;

		public byte PowerSaveEnable;

		public byte PullDownEnable8;

		public byte SerNumEnable8;

		public byte ASlowSlew;

		public byte ASchmittInput;

		public byte ADriveCurrent;

		public byte BSlowSlew;

		public byte BSchmittInput;

		public byte BDriveCurrent;

		public byte CSlowSlew;

		public byte CSchmittInput;

		public byte CDriveCurrent;

		public byte DSlowSlew;

		public byte DSchmittInput;

		public byte DDriveCurrent;

		public byte ARIIsTXDEN;

		public byte BRIIsTXDEN;

		public byte CRIIsTXDEN;

		public byte DRIIsTXDEN;

		public byte AIsVCP8;

		public byte BIsVCP8;

		public byte CIsVCP8;

		public byte DIsVCP8;

		public byte PullDownEnableH;

		public byte SerNumEnableH;

		public byte ACSlowSlewH;

		public byte ACSchmittInputH;

		public byte ACDriveCurrentH;

		public byte ADSlowSlewH;

		public byte ADSchmittInputH;

		public byte ADDriveCurrentH;

		public byte Cbus0H;

		public byte Cbus1H;

		public byte Cbus2H;

		public byte Cbus3H;

		public byte Cbus4H;

		public byte Cbus5H;

		public byte Cbus6H;

		public byte Cbus7H;

		public byte Cbus8H;

		public byte Cbus9H;

		public byte IsFifoH;

		public byte IsFifoTarH;

		public byte IsFastSerH;

		public byte IsFT1248H;

		public byte FT1248CpolH;

		public byte FT1248LsbH;

		public byte FT1248FlowControlH;

		public byte IsVCPH;

		public byte PowerSaveEnableH;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	private struct FT_EEPROM_HEADER
	{
		public uint deviceType;

		public ushort VendorId;

		public ushort ProductId;

		public byte SerNumEnable;

		public ushort MaxPower;

		public byte SelfPowered;

		public byte RemoteWakeup;

		public byte PullDownEnable;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	private struct FT_XSERIES_DATA
	{
		public FT_EEPROM_HEADER common;

		public byte ACSlowSlew;

		public byte ACSchmittInput;

		public byte ACDriveCurrent;

		public byte ADSlowSlew;

		public byte ADSchmittInput;

		public byte ADDriveCurrent;

		public byte Cbus0;

		public byte Cbus1;

		public byte Cbus2;

		public byte Cbus3;

		public byte Cbus4;

		public byte Cbus5;

		public byte Cbus6;

		public byte InvertTXD;

		public byte InvertRXD;

		public byte InvertRTS;

		public byte InvertCTS;

		public byte InvertDTR;

		public byte InvertDSR;

		public byte InvertDCD;

		public byte InvertRI;

		public byte BCDEnable;

		public byte BCDForceCbusPWREN;

		public byte BCDDisableSleep;

		public ushort I2CSlaveAddress;

		public uint I2CDeviceId;

		public byte I2CDisableSchmitt;

		public byte FT1248Cpol;

		public byte FT1248Lsb;

		public byte FT1248FlowControl;

		public byte RS485EchoSuppress;

		public byte PowerSaveEnable;

		public byte DriverType;
	}

	public class FT_EEPROM_DATA
	{
		public ushort VendorID = 1027;

		public ushort ProductID = 24577;

		public string Manufacturer = "FTDI";

		public string ManufacturerID = "FT";

		public string Description = "USB-Serial Converter";

		public string SerialNumber = "";

		public ushort MaxPower = 144;

		public bool SelfPowered;

		public bool RemoteWakeup;
	}

	public class FT232B_EEPROM_STRUCTURE : FT_EEPROM_DATA
	{
		public bool PullDownEnable;

		public bool SerNumEnable = true;

		public bool USBVersionEnable = true;

		public ushort USBVersion = 512;
	}

	public class FT2232_EEPROM_STRUCTURE : FT_EEPROM_DATA
	{
		public bool PullDownEnable;

		public bool SerNumEnable = true;

		public bool USBVersionEnable = true;

		public ushort USBVersion = 512;

		public bool AIsHighCurrent;

		public bool BIsHighCurrent;

		public bool IFAIsFifo;

		public bool IFAIsFifoTar;

		public bool IFAIsFastSer;

		public bool AIsVCP = true;

		public bool IFBIsFifo;

		public bool IFBIsFifoTar;

		public bool IFBIsFastSer;

		public bool BIsVCP = true;
	}

	public class FT232R_EEPROM_STRUCTURE : FT_EEPROM_DATA
	{
		public bool UseExtOsc;

		public bool HighDriveIOs;

		public byte EndpointSize = 64;

		public bool PullDownEnable;

		public bool SerNumEnable = true;

		public bool InvertTXD;

		public bool InvertRXD;

		public bool InvertRTS;

		public bool InvertCTS;

		public bool InvertDTR;

		public bool InvertDSR;

		public bool InvertDCD;

		public bool InvertRI;

		public byte Cbus0 = 5;

		public byte Cbus1 = 5;

		public byte Cbus2 = 5;

		public byte Cbus3 = 5;

		public byte Cbus4 = 5;

		public bool RIsD2XX;
	}

	public class FT2232H_EEPROM_STRUCTURE : FT_EEPROM_DATA
	{
		public bool PullDownEnable;

		public bool SerNumEnable = true;

		public bool ALSlowSlew;

		public bool ALSchmittInput;

		public byte ALDriveCurrent = 4;

		public bool AHSlowSlew;

		public bool AHSchmittInput;

		public byte AHDriveCurrent = 4;

		public bool BLSlowSlew;

		public bool BLSchmittInput;

		public byte BLDriveCurrent = 4;

		public bool BHSlowSlew;

		public bool BHSchmittInput;

		public byte BHDriveCurrent = 4;

		public bool IFAIsFifo;

		public bool IFAIsFifoTar;

		public bool IFAIsFastSer;

		public bool AIsVCP = true;

		public bool IFBIsFifo;

		public bool IFBIsFifoTar;

		public bool IFBIsFastSer;

		public bool BIsVCP = true;

		public bool PowerSaveEnable;
	}

	public class FT4232H_EEPROM_STRUCTURE : FT_EEPROM_DATA
	{
		public bool PullDownEnable;

		public bool SerNumEnable = true;

		public bool ASlowSlew;

		public bool ASchmittInput;

		public byte ADriveCurrent = 4;

		public bool BSlowSlew;

		public bool BSchmittInput;

		public byte BDriveCurrent = 4;

		public bool CSlowSlew;

		public bool CSchmittInput;

		public byte CDriveCurrent = 4;

		public bool DSlowSlew;

		public bool DSchmittInput;

		public byte DDriveCurrent = 4;

		public bool ARIIsTXDEN;

		public bool BRIIsTXDEN;

		public bool CRIIsTXDEN;

		public bool DRIIsTXDEN;

		public bool AIsVCP = true;

		public bool BIsVCP = true;

		public bool CIsVCP = true;

		public bool DIsVCP = true;
	}

	public class FT232H_EEPROM_STRUCTURE : FT_EEPROM_DATA
	{
		public bool PullDownEnable;

		public bool SerNumEnable = true;

		public bool ACSlowSlew;

		public bool ACSchmittInput;

		public byte ACDriveCurrent = 4;

		public bool ADSlowSlew;

		public bool ADSchmittInput;

		public byte ADDriveCurrent = 4;

		public byte Cbus0;

		public byte Cbus1;

		public byte Cbus2;

		public byte Cbus3;

		public byte Cbus4;

		public byte Cbus5;

		public byte Cbus6;

		public byte Cbus7;

		public byte Cbus8;

		public byte Cbus9;

		public bool IsFifo;

		public bool IsFifoTar;

		public bool IsFastSer;

		public bool IsFT1248;

		public bool FT1248Cpol;

		public bool FT1248Lsb;

		public bool FT1248FlowControl;

		public bool IsVCP = true;

		public bool PowerSaveEnable;
	}

	public class FT_XSERIES_EEPROM_STRUCTURE : FT_EEPROM_DATA
	{
		public bool PullDownEnable;

		public bool SerNumEnable = true;

		public bool USBVersionEnable = true;

		public ushort USBVersion = 512;

		public byte ACSlowSlew;

		public byte ACSchmittInput;

		public byte ACDriveCurrent;

		public byte ADSlowSlew;

		public byte ADSchmittInput;

		public byte ADDriveCurrent;

		public byte Cbus0;

		public byte Cbus1;

		public byte Cbus2;

		public byte Cbus3;

		public byte Cbus4;

		public byte Cbus5;

		public byte Cbus6;

		public byte InvertTXD;

		public byte InvertRXD;

		public byte InvertRTS;

		public byte InvertCTS;

		public byte InvertDTR;

		public byte InvertDSR;

		public byte InvertDCD;

		public byte InvertRI;

		public byte BCDEnable;

		public byte BCDForceCbusPWREN;

		public byte BCDDisableSleep;

		public ushort I2CSlaveAddress;

		public uint I2CDeviceId;

		public byte I2CDisableSchmitt;

		public byte FT1248Cpol;

		public byte FT1248Lsb;

		public byte FT1248FlowControl;

		public byte RS485EchoSuppress;

		public byte PowerSaveEnable;

		public byte IsVCP;
	}

	[Serializable]
	public class FT_EXCEPTION : Exception
	{
		public FT_EXCEPTION()
		{
		}

		public FT_EXCEPTION(string message)
			: base(message)
		{
		}

		public FT_EXCEPTION(string message, Exception inner)
			: base(message, inner)
		{
		}

		protected FT_EXCEPTION(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}

	private const uint FT_OPEN_BY_SERIAL_NUMBER = 1u;

	private const uint FT_OPEN_BY_DESCRIPTION = 2u;

	private const uint FT_OPEN_BY_LOCATION = 4u;

	private const uint FT_DEFAULT_BAUD_RATE = 9600u;

	private const uint FT_DEFAULT_DEADMAN_TIMEOUT = 5000u;

	private const int FT_COM_PORT_NOT_ASSIGNED = -1;

	private const uint FT_DEFAULT_IN_TRANSFER_SIZE = 4096u;

	private const uint FT_DEFAULT_OUT_TRANSFER_SIZE = 4096u;

	private const byte FT_DEFAULT_LATENCY = 16;

	private const uint FT_DEFAULT_DEVICE_ID = 67330049u;

	private IntPtr ftHandle = IntPtr.Zero;

	private IntPtr hFTD2XXDLL = IntPtr.Zero;

	private IntPtr pFT_CreateDeviceInfoList = IntPtr.Zero;

	private IntPtr pFT_GetDeviceInfoDetail = IntPtr.Zero;

	private IntPtr pFT_Open = IntPtr.Zero;

	private IntPtr pFT_OpenEx = IntPtr.Zero;

	private IntPtr pFT_Close = IntPtr.Zero;

	private IntPtr pFT_Read = IntPtr.Zero;

	private IntPtr pFT_Write = IntPtr.Zero;

	private IntPtr pFT_GetQueueStatus = IntPtr.Zero;

	private IntPtr pFT_GetModemStatus = IntPtr.Zero;

	private IntPtr pFT_GetStatus = IntPtr.Zero;

	private IntPtr pFT_SetBaudRate = IntPtr.Zero;

	private IntPtr pFT_SetDataCharacteristics = IntPtr.Zero;

	private IntPtr pFT_SetFlowControl = IntPtr.Zero;

	private IntPtr pFT_SetDtr = IntPtr.Zero;

	private IntPtr pFT_ClrDtr = IntPtr.Zero;

	private IntPtr pFT_SetRts = IntPtr.Zero;

	private IntPtr pFT_ClrRts = IntPtr.Zero;

	private IntPtr pFT_ResetDevice = IntPtr.Zero;

	private IntPtr pFT_ResetPort = IntPtr.Zero;

	private IntPtr pFT_CyclePort = IntPtr.Zero;

	private IntPtr pFT_Rescan = IntPtr.Zero;

	private IntPtr pFT_Reload = IntPtr.Zero;

	private IntPtr pFT_Purge = IntPtr.Zero;

	private IntPtr pFT_SetTimeouts = IntPtr.Zero;

	private IntPtr pFT_SetBreakOn = IntPtr.Zero;

	private IntPtr pFT_SetBreakOff = IntPtr.Zero;

	private IntPtr pFT_GetDeviceInfo = IntPtr.Zero;

	private IntPtr pFT_SetResetPipeRetryCount = IntPtr.Zero;

	private IntPtr pFT_StopInTask = IntPtr.Zero;

	private IntPtr pFT_RestartInTask = IntPtr.Zero;

	private IntPtr pFT_GetDriverVersion = IntPtr.Zero;

	private IntPtr pFT_GetLibraryVersion = IntPtr.Zero;

	private IntPtr pFT_SetDeadmanTimeout = IntPtr.Zero;

	private IntPtr pFT_SetChars = IntPtr.Zero;

	private IntPtr pFT_SetEventNotification = IntPtr.Zero;

	private IntPtr pFT_GetComPortNumber = IntPtr.Zero;

	private IntPtr pFT_SetLatencyTimer = IntPtr.Zero;

	private IntPtr pFT_GetLatencyTimer = IntPtr.Zero;

	private IntPtr pFT_SetBitMode = IntPtr.Zero;

	private IntPtr pFT_GetBitMode = IntPtr.Zero;

	private IntPtr pFT_SetUSBParameters = IntPtr.Zero;

	private IntPtr pFT_ReadEE = IntPtr.Zero;

	private IntPtr pFT_WriteEE = IntPtr.Zero;

	private IntPtr pFT_EraseEE = IntPtr.Zero;

	private IntPtr pFT_EE_UASize = IntPtr.Zero;

	private IntPtr pFT_EE_UARead = IntPtr.Zero;

	private IntPtr pFT_EE_UAWrite = IntPtr.Zero;

	private IntPtr pFT_EE_Read = IntPtr.Zero;

	private IntPtr pFT_EE_Program = IntPtr.Zero;

	private IntPtr pFT_EEPROM_Read = IntPtr.Zero;

	private IntPtr pFT_EEPROM_Program = IntPtr.Zero;

	private IntPtr pFT_VendorCmdGet = IntPtr.Zero;

	private IntPtr pFT_VendorCmdSet = IntPtr.Zero;

	private IntPtr pFT_VendorCmdSetX = IntPtr.Zero;

	public bool IsOpen
	{
		get
		{
			if (ftHandle == IntPtr.Zero)
			{
				return false;
			}
			return true;
		}
	}

	private string InterfaceIdentifier
	{
		get
		{
			string empty = string.Empty;
			if (IsOpen)
			{
				FT_DEVICE DeviceType = FT_DEVICE.FT_DEVICE_BM;
				GetDeviceType(ref DeviceType);
				if (DeviceType == FT_DEVICE.FT_DEVICE_2232H || DeviceType == FT_DEVICE.FT_DEVICE_4232H || DeviceType == FT_DEVICE.FT_DEVICE_2233HP || DeviceType == FT_DEVICE.FT_DEVICE_4233HP || DeviceType == FT_DEVICE.FT_DEVICE_2232HP || DeviceType == FT_DEVICE.FT_DEVICE_4232HP || DeviceType == FT_DEVICE.FT_DEVICE_2232HA || DeviceType == FT_DEVICE.FT_DEVICE_4232HA || DeviceType == FT_DEVICE.FT_DEVICE_2232)
				{
					GetDescription(out var Description);
					return Description.Substring(Description.Length - 1);
				}
			}
			return empty;
		}
	}

	public FTDI()
	{
		if (hFTD2XXDLL == IntPtr.Zero)
		{
			hFTD2XXDLL = LoadLibrary("FTD2XX.DLL");
			if (hFTD2XXDLL == IntPtr.Zero)
			{
				Console.WriteLine("Attempting to load FTD2XX.DLL from:\n" + Path.GetDirectoryName(GetType().Assembly.Location));
				hFTD2XXDLL = LoadLibrary(Path.GetDirectoryName(GetType().Assembly.Location) + "\\FTD2XX.DLL");
			}
		}
		if (hFTD2XXDLL != IntPtr.Zero)
		{
			FindFunctionPointers();
		}
		else
		{
			Console.WriteLine("Failed to load FTD2XX.DLL.  Are the FTDI drivers installed?");
		}
	}

	public FTDI(string path)
	{
		if (path == "")
		{
			return;
		}
		if (hFTD2XXDLL == IntPtr.Zero)
		{
			hFTD2XXDLL = LoadLibrary(path);
			if (hFTD2XXDLL == IntPtr.Zero)
			{
				Console.WriteLine("Attempting to load FTD2XX.DLL from:\n" + Path.GetDirectoryName(GetType().Assembly.Location));
			}
		}
		if (hFTD2XXDLL != IntPtr.Zero)
		{
			FindFunctionPointers();
		}
		else
		{
			Console.WriteLine("Failed to load FTD2XX.DLL.  Are the FTDI drivers installed?");
		}
	}

	private void FindFunctionPointers()
	{
		pFT_CreateDeviceInfoList = GetProcAddress(hFTD2XXDLL, "FT_CreateDeviceInfoList");
		pFT_GetDeviceInfoDetail = GetProcAddress(hFTD2XXDLL, "FT_GetDeviceInfoDetail");
		pFT_Open = GetProcAddress(hFTD2XXDLL, "FT_Open");
		pFT_OpenEx = GetProcAddress(hFTD2XXDLL, "FT_OpenEx");
		pFT_Close = GetProcAddress(hFTD2XXDLL, "FT_Close");
		pFT_Read = GetProcAddress(hFTD2XXDLL, "FT_Read");
		pFT_Write = GetProcAddress(hFTD2XXDLL, "FT_Write");
		pFT_GetQueueStatus = GetProcAddress(hFTD2XXDLL, "FT_GetQueueStatus");
		pFT_GetModemStatus = GetProcAddress(hFTD2XXDLL, "FT_GetModemStatus");
		pFT_GetStatus = GetProcAddress(hFTD2XXDLL, "FT_GetStatus");
		pFT_SetBaudRate = GetProcAddress(hFTD2XXDLL, "FT_SetBaudRate");
		pFT_SetDataCharacteristics = GetProcAddress(hFTD2XXDLL, "FT_SetDataCharacteristics");
		pFT_SetFlowControl = GetProcAddress(hFTD2XXDLL, "FT_SetFlowControl");
		pFT_SetDtr = GetProcAddress(hFTD2XXDLL, "FT_SetDtr");
		pFT_ClrDtr = GetProcAddress(hFTD2XXDLL, "FT_ClrDtr");
		pFT_SetRts = GetProcAddress(hFTD2XXDLL, "FT_SetRts");
		pFT_ClrRts = GetProcAddress(hFTD2XXDLL, "FT_ClrRts");
		pFT_ResetDevice = GetProcAddress(hFTD2XXDLL, "FT_ResetDevice");
		pFT_ResetPort = GetProcAddress(hFTD2XXDLL, "FT_ResetPort");
		pFT_CyclePort = GetProcAddress(hFTD2XXDLL, "FT_CyclePort");
		pFT_Rescan = GetProcAddress(hFTD2XXDLL, "FT_Rescan");
		pFT_Reload = GetProcAddress(hFTD2XXDLL, "FT_Reload");
		pFT_Purge = GetProcAddress(hFTD2XXDLL, "FT_Purge");
		pFT_SetTimeouts = GetProcAddress(hFTD2XXDLL, "FT_SetTimeouts");
		pFT_SetBreakOn = GetProcAddress(hFTD2XXDLL, "FT_SetBreakOn");
		pFT_SetBreakOff = GetProcAddress(hFTD2XXDLL, "FT_SetBreakOff");
		pFT_GetDeviceInfo = GetProcAddress(hFTD2XXDLL, "FT_GetDeviceInfo");
		pFT_SetResetPipeRetryCount = GetProcAddress(hFTD2XXDLL, "FT_SetResetPipeRetryCount");
		pFT_StopInTask = GetProcAddress(hFTD2XXDLL, "FT_StopInTask");
		pFT_RestartInTask = GetProcAddress(hFTD2XXDLL, "FT_RestartInTask");
		pFT_GetDriverVersion = GetProcAddress(hFTD2XXDLL, "FT_GetDriverVersion");
		pFT_GetLibraryVersion = GetProcAddress(hFTD2XXDLL, "FT_GetLibraryVersion");
		pFT_SetDeadmanTimeout = GetProcAddress(hFTD2XXDLL, "FT_SetDeadmanTimeout");
		pFT_SetChars = GetProcAddress(hFTD2XXDLL, "FT_SetChars");
		pFT_SetEventNotification = GetProcAddress(hFTD2XXDLL, "FT_SetEventNotification");
		pFT_GetComPortNumber = GetProcAddress(hFTD2XXDLL, "FT_GetComPortNumber");
		pFT_SetLatencyTimer = GetProcAddress(hFTD2XXDLL, "FT_SetLatencyTimer");
		pFT_GetLatencyTimer = GetProcAddress(hFTD2XXDLL, "FT_GetLatencyTimer");
		pFT_SetBitMode = GetProcAddress(hFTD2XXDLL, "FT_SetBitMode");
		pFT_GetBitMode = GetProcAddress(hFTD2XXDLL, "FT_GetBitMode");
		pFT_SetUSBParameters = GetProcAddress(hFTD2XXDLL, "FT_SetUSBParameters");
		pFT_ReadEE = GetProcAddress(hFTD2XXDLL, "FT_ReadEE");
		pFT_WriteEE = GetProcAddress(hFTD2XXDLL, "FT_WriteEE");
		pFT_EraseEE = GetProcAddress(hFTD2XXDLL, "FT_EraseEE");
		pFT_EE_UASize = GetProcAddress(hFTD2XXDLL, "FT_EE_UASize");
		pFT_EE_UARead = GetProcAddress(hFTD2XXDLL, "FT_EE_UARead");
		pFT_EE_UAWrite = GetProcAddress(hFTD2XXDLL, "FT_EE_UAWrite");
		pFT_EE_Read = GetProcAddress(hFTD2XXDLL, "FT_EE_Read");
		pFT_EE_Program = GetProcAddress(hFTD2XXDLL, "FT_EE_Program");
		pFT_EEPROM_Read = GetProcAddress(hFTD2XXDLL, "FT_EEPROM_Read");
		pFT_EEPROM_Program = GetProcAddress(hFTD2XXDLL, "FT_EEPROM_Program");
		pFT_VendorCmdGet = GetProcAddress(hFTD2XXDLL, "FT_VendorCmdGet");
		pFT_VendorCmdSet = GetProcAddress(hFTD2XXDLL, "FT_VendorCmdSet");
		pFT_VendorCmdSetX = GetProcAddress(hFTD2XXDLL, "FT_VendorCmdSetX");
	}

	~FTDI()
	{
		FreeLibrary(hFTD2XXDLL);
		hFTD2XXDLL = IntPtr.Zero;
	}

	[DllImport("kernel32.dll")]
	private static extern IntPtr LoadLibrary(string dllToLoad);

	[DllImport("kernel32.dll")]
	private static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

	[DllImport("kernel32.dll")]
	private static extern bool FreeLibrary(IntPtr hModule);

	public FT_STATUS GetNumberOfDevices(ref uint devcount)
	{
		FT_STATUS result = FT_STATUS.FT_OTHER_ERROR;
		if (hFTD2XXDLL == IntPtr.Zero)
		{
			return result;
		}
		if (pFT_CreateDeviceInfoList != IntPtr.Zero)
		{
			result = ((tFT_CreateDeviceInfoList)Marshal.GetDelegateForFunctionPointer(pFT_CreateDeviceInfoList, typeof(tFT_CreateDeviceInfoList)))(ref devcount);
		}
		else
		{
			Console.WriteLine("Failed to load function FT_CreateDeviceInfoList.");
		}
		return result;
	}

	public FT_STATUS GetDeviceList(FT_DEVICE_INFO_NODE[] devicelist)
	{
		FT_STATUS fT_STATUS = FT_STATUS.FT_OTHER_ERROR;
		FT_ERROR fT_ERROR = FT_ERROR.FT_NO_ERROR;
		int num = 0;
		if (hFTD2XXDLL == IntPtr.Zero)
		{
			return fT_STATUS;
		}
		if ((pFT_CreateDeviceInfoList != IntPtr.Zero) & (pFT_GetDeviceInfoDetail != IntPtr.Zero))
		{
			uint numdevs = 0u;
			tFT_CreateDeviceInfoList obj = (tFT_CreateDeviceInfoList)Marshal.GetDelegateForFunctionPointer(pFT_CreateDeviceInfoList, typeof(tFT_CreateDeviceInfoList));
			tFT_GetDeviceInfoDetail tFT_GetDeviceInfoDetail2 = (tFT_GetDeviceInfoDetail)Marshal.GetDelegateForFunctionPointer(pFT_GetDeviceInfoDetail, typeof(tFT_GetDeviceInfoDetail));
			fT_STATUS = obj(ref numdevs);
			byte[] array = new byte[16];
			byte[] array2 = new byte[64];
			if (numdevs != 0)
			{
				if (devicelist.Length < numdevs)
				{
					fT_ERROR = FT_ERROR.FT_BUFFER_SIZE;
					ErrorHandler(fT_STATUS, fT_ERROR);
				}
				for (uint num2 = 0u; num2 < numdevs; num2++)
				{
					devicelist[num2] = new FT_DEVICE_INFO_NODE();
					fT_STATUS = tFT_GetDeviceInfoDetail2(num2, ref devicelist[num2].Flags, ref devicelist[num2].Type, ref devicelist[num2].ID, ref devicelist[num2].LocId, array, array2, ref devicelist[num2].ftHandle);
					devicelist[num2].SerialNumber = Encoding.ASCII.GetString(array);
					devicelist[num2].Description = Encoding.ASCII.GetString(array2);
					num = devicelist[num2].SerialNumber.IndexOf('\0');
					if (num != -1)
					{
						devicelist[num2].SerialNumber = devicelist[num2].SerialNumber.Substring(0, num);
					}
					num = devicelist[num2].Description.IndexOf('\0');
					if (num != -1)
					{
						devicelist[num2].Description = devicelist[num2].Description.Substring(0, num);
					}
				}
			}
		}
		else
		{
			if (pFT_CreateDeviceInfoList == IntPtr.Zero)
			{
				Console.WriteLine("Failed to load function FT_CreateDeviceInfoList.");
			}
			if (pFT_GetDeviceInfoDetail == IntPtr.Zero)
			{
				Console.WriteLine("Failed to load function FT_GetDeviceInfoListDetail.");
			}
		}
		return fT_STATUS;
	}

	public FT_STATUS OpenByIndex(uint index)
	{
		FT_STATUS fT_STATUS = FT_STATUS.FT_OTHER_ERROR;
		if (hFTD2XXDLL == IntPtr.Zero)
		{
			return fT_STATUS;
		}
		if ((pFT_Open != IntPtr.Zero) & (pFT_SetDataCharacteristics != IntPtr.Zero) & (pFT_SetFlowControl != IntPtr.Zero) & (pFT_SetBaudRate != IntPtr.Zero))
		{
			tFT_Open obj = (tFT_Open)Marshal.GetDelegateForFunctionPointer(pFT_Open, typeof(tFT_Open));
			tFT_SetDataCharacteristics tFT_SetDataCharacteristics2 = (tFT_SetDataCharacteristics)Marshal.GetDelegateForFunctionPointer(pFT_SetDataCharacteristics, typeof(tFT_SetDataCharacteristics));
			tFT_SetFlowControl tFT_SetFlowControl2 = (tFT_SetFlowControl)Marshal.GetDelegateForFunctionPointer(pFT_SetFlowControl, typeof(tFT_SetFlowControl));
			tFT_SetBaudRate tFT_SetBaudRate2 = (tFT_SetBaudRate)Marshal.GetDelegateForFunctionPointer(pFT_SetBaudRate, typeof(tFT_SetBaudRate));
			fT_STATUS = obj(index, ref ftHandle);
			if (fT_STATUS != FT_STATUS.FT_OK)
			{
				ftHandle = IntPtr.Zero;
			}
			if (ftHandle != IntPtr.Zero)
			{
				byte uWordLength = 8;
				byte uStopBits = 0;
				byte uParity = 0;
				fT_STATUS = tFT_SetDataCharacteristics2(ftHandle, uWordLength, uStopBits, uParity);
				ushort usFlowControl = 0;
				byte uXon = 17;
				byte uXoff = 19;
				fT_STATUS = tFT_SetFlowControl2(ftHandle, usFlowControl, uXon, uXoff);
				uint dwBaudRate = 9600u;
				fT_STATUS = tFT_SetBaudRate2(ftHandle, dwBaudRate);
			}
		}
		else
		{
			if (pFT_Open == IntPtr.Zero)
			{
				Console.WriteLine("Failed to load function FT_Open.");
			}
			if (pFT_SetDataCharacteristics == IntPtr.Zero)
			{
				Console.WriteLine("Failed to load function FT_SetDataCharacteristics.");
			}
			if (pFT_SetFlowControl == IntPtr.Zero)
			{
				Console.WriteLine("Failed to load function FT_SetFlowControl.");
			}
			if (pFT_SetBaudRate == IntPtr.Zero)
			{
				Console.WriteLine("Failed to load function FT_SetBaudRate.");
			}
		}
		return fT_STATUS;
	}

	public FT_STATUS OpenBySerialNumber(string serialnumber)
	{
		FT_STATUS fT_STATUS = FT_STATUS.FT_OTHER_ERROR;
		if (hFTD2XXDLL == IntPtr.Zero)
		{
			return fT_STATUS;
		}
		if ((pFT_OpenEx != IntPtr.Zero) & (pFT_SetDataCharacteristics != IntPtr.Zero) & (pFT_SetFlowControl != IntPtr.Zero) & (pFT_SetBaudRate != IntPtr.Zero))
		{
			tFT_OpenEx obj = (tFT_OpenEx)Marshal.GetDelegateForFunctionPointer(pFT_OpenEx, typeof(tFT_OpenEx));
			tFT_SetDataCharacteristics tFT_SetDataCharacteristics2 = (tFT_SetDataCharacteristics)Marshal.GetDelegateForFunctionPointer(pFT_SetDataCharacteristics, typeof(tFT_SetDataCharacteristics));
			tFT_SetFlowControl tFT_SetFlowControl2 = (tFT_SetFlowControl)Marshal.GetDelegateForFunctionPointer(pFT_SetFlowControl, typeof(tFT_SetFlowControl));
			tFT_SetBaudRate tFT_SetBaudRate2 = (tFT_SetBaudRate)Marshal.GetDelegateForFunctionPointer(pFT_SetBaudRate, typeof(tFT_SetBaudRate));
			fT_STATUS = obj(serialnumber, 1u, ref ftHandle);
			if (fT_STATUS != FT_STATUS.FT_OK)
			{
				ftHandle = IntPtr.Zero;
			}
			if (ftHandle != IntPtr.Zero)
			{
				byte uWordLength = 8;
				byte uStopBits = 0;
				byte uParity = 0;
				fT_STATUS = tFT_SetDataCharacteristics2(ftHandle, uWordLength, uStopBits, uParity);
				ushort usFlowControl = 0;
				byte uXon = 17;
				byte uXoff = 19;
				fT_STATUS = tFT_SetFlowControl2(ftHandle, usFlowControl, uXon, uXoff);
				uint dwBaudRate = 9600u;
				fT_STATUS = tFT_SetBaudRate2(ftHandle, dwBaudRate);
			}
		}
		else
		{
			if (pFT_OpenEx == IntPtr.Zero)
			{
				Console.WriteLine("Failed to load function FT_OpenEx.");
			}
			if (pFT_SetDataCharacteristics == IntPtr.Zero)
			{
				Console.WriteLine("Failed to load function FT_SetDataCharacteristics.");
			}
			if (pFT_SetFlowControl == IntPtr.Zero)
			{
				Console.WriteLine("Failed to load function FT_SetFlowControl.");
			}
			if (pFT_SetBaudRate == IntPtr.Zero)
			{
				Console.WriteLine("Failed to load function FT_SetBaudRate.");
			}
		}
		return fT_STATUS;
	}

	public FT_STATUS OpenByDescription(string description)
	{
		FT_STATUS fT_STATUS = FT_STATUS.FT_OTHER_ERROR;
		if (hFTD2XXDLL == IntPtr.Zero)
		{
			return fT_STATUS;
		}
		if ((pFT_OpenEx != IntPtr.Zero) & (pFT_SetDataCharacteristics != IntPtr.Zero) & (pFT_SetFlowControl != IntPtr.Zero) & (pFT_SetBaudRate != IntPtr.Zero))
		{
			tFT_OpenEx obj = (tFT_OpenEx)Marshal.GetDelegateForFunctionPointer(pFT_OpenEx, typeof(tFT_OpenEx));
			tFT_SetDataCharacteristics tFT_SetDataCharacteristics2 = (tFT_SetDataCharacteristics)Marshal.GetDelegateForFunctionPointer(pFT_SetDataCharacteristics, typeof(tFT_SetDataCharacteristics));
			tFT_SetFlowControl tFT_SetFlowControl2 = (tFT_SetFlowControl)Marshal.GetDelegateForFunctionPointer(pFT_SetFlowControl, typeof(tFT_SetFlowControl));
			tFT_SetBaudRate tFT_SetBaudRate2 = (tFT_SetBaudRate)Marshal.GetDelegateForFunctionPointer(pFT_SetBaudRate, typeof(tFT_SetBaudRate));
			fT_STATUS = obj(description, 2u, ref ftHandle);
			if (fT_STATUS != FT_STATUS.FT_OK)
			{
				ftHandle = IntPtr.Zero;
			}
			if (ftHandle != IntPtr.Zero)
			{
				byte uWordLength = 8;
				byte uStopBits = 0;
				byte uParity = 0;
				fT_STATUS = tFT_SetDataCharacteristics2(ftHandle, uWordLength, uStopBits, uParity);
				ushort usFlowControl = 0;
				byte uXon = 17;
				byte uXoff = 19;
				fT_STATUS = tFT_SetFlowControl2(ftHandle, usFlowControl, uXon, uXoff);
				uint dwBaudRate = 9600u;
				fT_STATUS = tFT_SetBaudRate2(ftHandle, dwBaudRate);
			}
		}
		else
		{
			if (pFT_OpenEx == IntPtr.Zero)
			{
				Console.WriteLine("Failed to load function FT_OpenEx.");
			}
			if (pFT_SetDataCharacteristics == IntPtr.Zero)
			{
				Console.WriteLine("Failed to load function FT_SetDataCharacteristics.");
			}
			if (pFT_SetFlowControl == IntPtr.Zero)
			{
				Console.WriteLine("Failed to load function FT_SetFlowControl.");
			}
			if (pFT_SetBaudRate == IntPtr.Zero)
			{
				Console.WriteLine("Failed to load function FT_SetBaudRate.");
			}
		}
		return fT_STATUS;
	}

	public FT_STATUS OpenByLocation(uint location)
	{
		FT_STATUS fT_STATUS = FT_STATUS.FT_OTHER_ERROR;
		if (hFTD2XXDLL == IntPtr.Zero)
		{
			return fT_STATUS;
		}
		if ((pFT_OpenEx != IntPtr.Zero) & (pFT_SetDataCharacteristics != IntPtr.Zero) & (pFT_SetFlowControl != IntPtr.Zero) & (pFT_SetBaudRate != IntPtr.Zero))
		{
			tFT_OpenExLoc obj = (tFT_OpenExLoc)Marshal.GetDelegateForFunctionPointer(pFT_OpenEx, typeof(tFT_OpenExLoc));
			tFT_SetDataCharacteristics tFT_SetDataCharacteristics2 = (tFT_SetDataCharacteristics)Marshal.GetDelegateForFunctionPointer(pFT_SetDataCharacteristics, typeof(tFT_SetDataCharacteristics));
			tFT_SetFlowControl tFT_SetFlowControl2 = (tFT_SetFlowControl)Marshal.GetDelegateForFunctionPointer(pFT_SetFlowControl, typeof(tFT_SetFlowControl));
			tFT_SetBaudRate tFT_SetBaudRate2 = (tFT_SetBaudRate)Marshal.GetDelegateForFunctionPointer(pFT_SetBaudRate, typeof(tFT_SetBaudRate));
			fT_STATUS = obj(location, 4u, ref ftHandle);
			if (fT_STATUS != FT_STATUS.FT_OK)
			{
				ftHandle = IntPtr.Zero;
			}
			if (ftHandle != IntPtr.Zero)
			{
				byte uWordLength = 8;
				byte uStopBits = 0;
				byte uParity = 0;
				tFT_SetDataCharacteristics2(ftHandle, uWordLength, uStopBits, uParity);
				ushort usFlowControl = 0;
				byte uXon = 17;
				byte uXoff = 19;
				tFT_SetFlowControl2(ftHandle, usFlowControl, uXon, uXoff);
				uint dwBaudRate = 9600u;
				tFT_SetBaudRate2(ftHandle, dwBaudRate);
			}
		}
		else
		{
			if (pFT_OpenEx == IntPtr.Zero)
			{
				Console.WriteLine("Failed to load function FT_OpenEx.");
			}
			if (pFT_SetDataCharacteristics == IntPtr.Zero)
			{
				Console.WriteLine("Failed to load function FT_SetDataCharacteristics.");
			}
			if (pFT_SetFlowControl == IntPtr.Zero)
			{
				Console.WriteLine("Failed to load function FT_SetFlowControl.");
			}
			if (pFT_SetBaudRate == IntPtr.Zero)
			{
				Console.WriteLine("Failed to load function FT_SetBaudRate.");
			}
		}
		return fT_STATUS;
	}

	public FT_STATUS Close()
	{
		FT_STATUS fT_STATUS = FT_STATUS.FT_OTHER_ERROR;
		if (hFTD2XXDLL == IntPtr.Zero)
		{
			return fT_STATUS;
		}
		if (pFT_Close != IntPtr.Zero)
		{
			fT_STATUS = ((tFT_Close)Marshal.GetDelegateForFunctionPointer(pFT_Close, typeof(tFT_Close)))(ftHandle);
			if (fT_STATUS == FT_STATUS.FT_OK)
			{
				ftHandle = IntPtr.Zero;
			}
		}
		else if (pFT_Close == IntPtr.Zero)
		{
			Console.WriteLine("Failed to load function FT_Close.");
		}
		return fT_STATUS;
	}

	public FT_STATUS Read(byte[] dataBuffer, uint numBytesToRead, ref uint numBytesRead)
	{
		FT_STATUS result = FT_STATUS.FT_OTHER_ERROR;
		if (hFTD2XXDLL == IntPtr.Zero)
		{
			return result;
		}
		if (pFT_Read != IntPtr.Zero)
		{
			tFT_Read tFT_Read2 = (tFT_Read)Marshal.GetDelegateForFunctionPointer(pFT_Read, typeof(tFT_Read));
			if (dataBuffer.Length < numBytesToRead)
			{
				numBytesToRead = (uint)dataBuffer.Length;
			}
			if (ftHandle != IntPtr.Zero)
			{
				result = tFT_Read2(ftHandle, dataBuffer, numBytesToRead, ref numBytesRead);
			}
		}
		else if (pFT_Read == IntPtr.Zero)
		{
			Console.WriteLine("Failed to load function FT_Read.");
		}
		return result;
	}

	public FT_STATUS Read(out string dataBuffer, uint numBytesToRead, ref uint numBytesRead)
	{
		FT_STATUS result = FT_STATUS.FT_OTHER_ERROR;
		dataBuffer = string.Empty;
		if (hFTD2XXDLL == IntPtr.Zero)
		{
			return result;
		}
		if (pFT_Read != IntPtr.Zero)
		{
			tFT_Read tFT_Read2 = (tFT_Read)Marshal.GetDelegateForFunctionPointer(pFT_Read, typeof(tFT_Read));
			byte[] array = new byte[numBytesToRead];
			if (ftHandle != IntPtr.Zero)
			{
				result = tFT_Read2(ftHandle, array, numBytesToRead, ref numBytesRead);
				dataBuffer = Encoding.ASCII.GetString(array);
				dataBuffer = dataBuffer.Substring(0, (int)numBytesRead);
			}
		}
		else if (pFT_Read == IntPtr.Zero)
		{
			Console.WriteLine("Failed to load function FT_Read.");
		}
		return result;
	}

	public FT_STATUS Write(byte[] dataBuffer, int numBytesToWrite, ref uint numBytesWritten)
	{
		FT_STATUS result = FT_STATUS.FT_OTHER_ERROR;
		if (hFTD2XXDLL == IntPtr.Zero)
		{
			return result;
		}
		if (pFT_Write != IntPtr.Zero)
		{
			tFT_Write tFT_Write2 = (tFT_Write)Marshal.GetDelegateForFunctionPointer(pFT_Write, typeof(tFT_Write));
			if (ftHandle != IntPtr.Zero)
			{
				result = tFT_Write2(ftHandle, dataBuffer, (uint)numBytesToWrite, ref numBytesWritten);
			}
		}
		else if (pFT_Write == IntPtr.Zero)
		{
			Console.WriteLine("Failed to load function FT_Write.");
		}
		return result;
	}

	public FT_STATUS Write(byte[] dataBuffer, uint numBytesToWrite, ref uint numBytesWritten)
	{
		FT_STATUS result = FT_STATUS.FT_OTHER_ERROR;
		if (hFTD2XXDLL == IntPtr.Zero)
		{
			return result;
		}
		if (pFT_Write != IntPtr.Zero)
		{
			tFT_Write tFT_Write2 = (tFT_Write)Marshal.GetDelegateForFunctionPointer(pFT_Write, typeof(tFT_Write));
			if (ftHandle != IntPtr.Zero)
			{
				result = tFT_Write2(ftHandle, dataBuffer, numBytesToWrite, ref numBytesWritten);
			}
		}
		else if (pFT_Write == IntPtr.Zero)
		{
			Console.WriteLine("Failed to load function FT_Write.");
		}
		return result;
	}

	public FT_STATUS Write(string dataBuffer, int numBytesToWrite, ref uint numBytesWritten)
	{
		FT_STATUS result = FT_STATUS.FT_OTHER_ERROR;
		if (hFTD2XXDLL == IntPtr.Zero)
		{
			return result;
		}
		if (pFT_Write != IntPtr.Zero)
		{
			tFT_Write tFT_Write2 = (tFT_Write)Marshal.GetDelegateForFunctionPointer(pFT_Write, typeof(tFT_Write));
			byte[] bytes = Encoding.ASCII.GetBytes(dataBuffer);
			if (ftHandle != IntPtr.Zero)
			{
				result = tFT_Write2(ftHandle, bytes, (uint)numBytesToWrite, ref numBytesWritten);
			}
		}
		else if (pFT_Write == IntPtr.Zero)
		{
			Console.WriteLine("Failed to load function FT_Write.");
		}
		return result;
	}

	public FT_STATUS Write(string dataBuffer, uint numBytesToWrite, ref uint numBytesWritten)
	{
		FT_STATUS result = FT_STATUS.FT_OTHER_ERROR;
		if (hFTD2XXDLL == IntPtr.Zero)
		{
			return result;
		}
		if (pFT_Write != IntPtr.Zero)
		{
			tFT_Write tFT_Write2 = (tFT_Write)Marshal.GetDelegateForFunctionPointer(pFT_Write, typeof(tFT_Write));
			byte[] bytes = Encoding.ASCII.GetBytes(dataBuffer);
			if (ftHandle != IntPtr.Zero)
			{
				result = tFT_Write2(ftHandle, bytes, numBytesToWrite, ref numBytesWritten);
			}
		}
		else if (pFT_Write == IntPtr.Zero)
		{
			Console.WriteLine("Failed to load function FT_Write.");
		}
		return result;
	}

	public FT_STATUS ResetDevice()
	{
		FT_STATUS result = FT_STATUS.FT_OTHER_ERROR;
		if (hFTD2XXDLL == IntPtr.Zero)
		{
			return result;
		}
		if (pFT_ResetDevice != IntPtr.Zero)
		{
			tFT_ResetDevice tFT_ResetDevice2 = (tFT_ResetDevice)Marshal.GetDelegateForFunctionPointer(pFT_ResetDevice, typeof(tFT_ResetDevice));
			if (ftHandle != IntPtr.Zero)
			{
				result = tFT_ResetDevice2(ftHandle);
			}
		}
		else if (pFT_ResetDevice == IntPtr.Zero)
		{
			Console.WriteLine("Failed to load function FT_ResetDevice.");
		}
		return result;
	}

	public FT_STATUS Purge(uint purgemask)
	{
		FT_STATUS result = FT_STATUS.FT_OTHER_ERROR;
		if (hFTD2XXDLL == IntPtr.Zero)
		{
			return result;
		}
		if (pFT_Purge != IntPtr.Zero)
		{
			tFT_Purge tFT_Purge2 = (tFT_Purge)Marshal.GetDelegateForFunctionPointer(pFT_Purge, typeof(tFT_Purge));
			if (ftHandle != IntPtr.Zero)
			{
				result = tFT_Purge2(ftHandle, purgemask);
			}
		}
		else if (pFT_Purge == IntPtr.Zero)
		{
			Console.WriteLine("Failed to load function FT_Purge.");
		}
		return result;
	}

	public FT_STATUS SetEventNotification(uint eventmask, EventWaitHandle eventhandle)
	{
		FT_STATUS result = FT_STATUS.FT_OTHER_ERROR;
		if (hFTD2XXDLL == IntPtr.Zero)
		{
			return result;
		}
		if (pFT_SetEventNotification != IntPtr.Zero)
		{
			tFT_SetEventNotification tFT_SetEventNotification2 = (tFT_SetEventNotification)Marshal.GetDelegateForFunctionPointer(pFT_SetEventNotification, typeof(tFT_SetEventNotification));
			if (ftHandle != IntPtr.Zero)
			{
				result = tFT_SetEventNotification2(ftHandle, eventmask, eventhandle.SafeWaitHandle);
			}
		}
		else if (pFT_SetEventNotification == IntPtr.Zero)
		{
			Console.WriteLine("Failed to load function FT_SetEventNotification.");
		}
		return result;
	}

	public FT_STATUS StopInTask()
	{
		FT_STATUS result = FT_STATUS.FT_OTHER_ERROR;
		if (hFTD2XXDLL == IntPtr.Zero)
		{
			return result;
		}
		if (pFT_StopInTask != IntPtr.Zero)
		{
			tFT_StopInTask tFT_StopInTask2 = (tFT_StopInTask)Marshal.GetDelegateForFunctionPointer(pFT_StopInTask, typeof(tFT_StopInTask));
			if (ftHandle != IntPtr.Zero)
			{
				result = tFT_StopInTask2(ftHandle);
			}
		}
		else if (pFT_StopInTask == IntPtr.Zero)
		{
			Console.WriteLine("Failed to load function FT_StopInTask.");
		}
		return result;
	}

	public FT_STATUS RestartInTask()
	{
		FT_STATUS result = FT_STATUS.FT_OTHER_ERROR;
		if (hFTD2XXDLL == IntPtr.Zero)
		{
			return result;
		}
		if (pFT_RestartInTask != IntPtr.Zero)
		{
			tFT_RestartInTask tFT_RestartInTask2 = (tFT_RestartInTask)Marshal.GetDelegateForFunctionPointer(pFT_RestartInTask, typeof(tFT_RestartInTask));
			if (ftHandle != IntPtr.Zero)
			{
				result = tFT_RestartInTask2(ftHandle);
			}
		}
		else if (pFT_RestartInTask == IntPtr.Zero)
		{
			Console.WriteLine("Failed to load function FT_RestartInTask.");
		}
		return result;
	}

	public FT_STATUS ResetPort()
	{
		FT_STATUS result = FT_STATUS.FT_OTHER_ERROR;
		if (hFTD2XXDLL == IntPtr.Zero)
		{
			return result;
		}
		if (pFT_ResetPort != IntPtr.Zero)
		{
			tFT_ResetPort tFT_ResetPort2 = (tFT_ResetPort)Marshal.GetDelegateForFunctionPointer(pFT_ResetPort, typeof(tFT_ResetPort));
			if (ftHandle != IntPtr.Zero)
			{
				result = tFT_ResetPort2(ftHandle);
			}
		}
		else if (pFT_ResetPort == IntPtr.Zero)
		{
			Console.WriteLine("Failed to load function FT_ResetPort.");
		}
		return result;
	}

	public FT_STATUS CyclePort()
	{
		FT_STATUS fT_STATUS = FT_STATUS.FT_OTHER_ERROR;
		if (hFTD2XXDLL == IntPtr.Zero)
		{
			return fT_STATUS;
		}
		if ((pFT_CyclePort != IntPtr.Zero) & (pFT_Close != IntPtr.Zero))
		{
			tFT_CyclePort tFT_CyclePort2 = (tFT_CyclePort)Marshal.GetDelegateForFunctionPointer(pFT_CyclePort, typeof(tFT_CyclePort));
			tFT_Close tFT_Close2 = (tFT_Close)Marshal.GetDelegateForFunctionPointer(pFT_Close, typeof(tFT_Close));
			if (ftHandle != IntPtr.Zero)
			{
				fT_STATUS = tFT_CyclePort2(ftHandle);
				if (fT_STATUS == FT_STATUS.FT_OK)
				{
					fT_STATUS = tFT_Close2(ftHandle);
					if (fT_STATUS == FT_STATUS.FT_OK)
					{
						ftHandle = IntPtr.Zero;
					}
				}
			}
		}
		else
		{
			if (pFT_CyclePort == IntPtr.Zero)
			{
				Console.WriteLine("Failed to load function FT_CyclePort.");
			}
			if (pFT_Close == IntPtr.Zero)
			{
				Console.WriteLine("Failed to load function FT_Close.");
			}
		}
		return fT_STATUS;
	}

	public FT_STATUS Rescan()
	{
		FT_STATUS result = FT_STATUS.FT_OTHER_ERROR;
		if (hFTD2XXDLL == IntPtr.Zero)
		{
			return result;
		}
		if (pFT_Rescan != IntPtr.Zero)
		{
			result = ((tFT_Rescan)Marshal.GetDelegateForFunctionPointer(pFT_Rescan, typeof(tFT_Rescan)))();
		}
		else if (pFT_Rescan == IntPtr.Zero)
		{
			Console.WriteLine("Failed to load function FT_Rescan.");
		}
		return result;
	}

	public FT_STATUS Reload(ushort VendorID, ushort ProductID)
	{
		FT_STATUS result = FT_STATUS.FT_OTHER_ERROR;
		if (hFTD2XXDLL == IntPtr.Zero)
		{
			return result;
		}
		if (pFT_Reload != IntPtr.Zero)
		{
			result = ((tFT_Reload)Marshal.GetDelegateForFunctionPointer(pFT_Reload, typeof(tFT_Reload)))(VendorID, ProductID);
		}
		else if (pFT_Reload == IntPtr.Zero)
		{
			Console.WriteLine("Failed to load function FT_Reload.");
		}
		return result;
	}

	public FT_STATUS SetBitMode(byte Mask, byte BitMode)
	{
		FT_STATUS fT_STATUS = FT_STATUS.FT_OTHER_ERROR;
		FT_ERROR fT_ERROR = FT_ERROR.FT_NO_ERROR;
		if (hFTD2XXDLL == IntPtr.Zero)
		{
			return fT_STATUS;
		}
		if (pFT_SetBitMode != IntPtr.Zero)
		{
			tFT_SetBitMode tFT_SetBitMode2 = (tFT_SetBitMode)Marshal.GetDelegateForFunctionPointer(pFT_SetBitMode, typeof(tFT_SetBitMode));
			if (ftHandle != IntPtr.Zero)
			{
				FT_DEVICE DeviceType = FT_DEVICE.FT_DEVICE_UNKNOWN;
				GetDeviceType(ref DeviceType);
				if (DeviceType == FT_DEVICE.FT_DEVICE_AM)
				{
					fT_ERROR = FT_ERROR.FT_INVALID_BITMODE;
					ErrorHandler(fT_STATUS, fT_ERROR);
				}
				else if (DeviceType == FT_DEVICE.FT_DEVICE_100AX)
				{
					fT_ERROR = FT_ERROR.FT_INVALID_BITMODE;
					ErrorHandler(fT_STATUS, fT_ERROR);
				}
				else if (DeviceType == FT_DEVICE.FT_DEVICE_BM && BitMode != 0)
				{
					if ((BitMode & 1) == 0)
					{
						fT_ERROR = FT_ERROR.FT_INVALID_BITMODE;
						ErrorHandler(fT_STATUS, fT_ERROR);
					}
				}
				else if (DeviceType == FT_DEVICE.FT_DEVICE_2232 && BitMode != 0)
				{
					if ((BitMode & 0x1F) == 0)
					{
						fT_ERROR = FT_ERROR.FT_INVALID_BITMODE;
						ErrorHandler(fT_STATUS, fT_ERROR);
					}
					if ((BitMode == 2) & (InterfaceIdentifier != "A"))
					{
						fT_ERROR = FT_ERROR.FT_INVALID_BITMODE;
						ErrorHandler(fT_STATUS, fT_ERROR);
					}
				}
				else if (DeviceType == FT_DEVICE.FT_DEVICE_232R && BitMode != 0)
				{
					if ((BitMode & 0x25) == 0)
					{
						fT_ERROR = FT_ERROR.FT_INVALID_BITMODE;
						ErrorHandler(fT_STATUS, fT_ERROR);
					}
				}
				else if ((DeviceType == FT_DEVICE.FT_DEVICE_2232H || DeviceType == FT_DEVICE.FT_DEVICE_2232HP || DeviceType == FT_DEVICE.FT_DEVICE_2233HP || DeviceType == FT_DEVICE.FT_DEVICE_2232HA) && BitMode != 0)
				{
					if ((BitMode & 0x5F) == 0)
					{
						fT_ERROR = FT_ERROR.FT_INVALID_BITMODE;
						ErrorHandler(fT_STATUS, fT_ERROR);
					}
					if (((BitMode == 8) | (BitMode == 64)) & (InterfaceIdentifier != "A"))
					{
						fT_ERROR = FT_ERROR.FT_INVALID_BITMODE;
						ErrorHandler(fT_STATUS, fT_ERROR);
					}
				}
				else if ((DeviceType == FT_DEVICE.FT_DEVICE_4232H || DeviceType == FT_DEVICE.FT_DEVICE_4232HP || DeviceType == FT_DEVICE.FT_DEVICE_4233HP || DeviceType == FT_DEVICE.FT_DEVICE_4232HA) && BitMode != 0)
				{
					if ((BitMode & 7) == 0)
					{
						fT_ERROR = FT_ERROR.FT_INVALID_BITMODE;
						ErrorHandler(fT_STATUS, fT_ERROR);
					}
					if ((BitMode == 2) & ((InterfaceIdentifier != "A") & (InterfaceIdentifier != "B")))
					{
						fT_ERROR = FT_ERROR.FT_INVALID_BITMODE;
						ErrorHandler(fT_STATUS, fT_ERROR);
					}
				}
				else if ((DeviceType == FT_DEVICE.FT_DEVICE_232H || DeviceType == FT_DEVICE.FT_DEVICE_232HP || DeviceType == FT_DEVICE.FT_DEVICE_233HP) && BitMode != 0 && BitMode > 64)
				{
					fT_ERROR = FT_ERROR.FT_INVALID_BITMODE;
					ErrorHandler(fT_STATUS, fT_ERROR);
				}
				fT_STATUS = tFT_SetBitMode2(ftHandle, Mask, BitMode);
			}
		}
		else if (pFT_SetBitMode == IntPtr.Zero)
		{
			Console.WriteLine("Failed to load function FT_SetBitMode.");
		}
		return fT_STATUS;
	}

	public FT_STATUS GetPinStates(ref byte BitMode)
	{
		FT_STATUS result = FT_STATUS.FT_OTHER_ERROR;
		if (hFTD2XXDLL == IntPtr.Zero)
		{
			return result;
		}
		if (pFT_GetBitMode != IntPtr.Zero)
		{
			tFT_GetBitMode tFT_GetBitMode2 = (tFT_GetBitMode)Marshal.GetDelegateForFunctionPointer(pFT_GetBitMode, typeof(tFT_GetBitMode));
			if (ftHandle != IntPtr.Zero)
			{
				result = tFT_GetBitMode2(ftHandle, ref BitMode);
			}
		}
		else if (pFT_GetBitMode == IntPtr.Zero)
		{
			Console.WriteLine("Failed to load function FT_GetBitMode.");
		}
		return result;
	}

	public FT_STATUS ReadEEPROMLocation(uint Address, ref ushort EEValue)
	{
		FT_STATUS result = FT_STATUS.FT_OTHER_ERROR;
		if (hFTD2XXDLL == IntPtr.Zero)
		{
			return result;
		}
		if (pFT_ReadEE != IntPtr.Zero)
		{
			tFT_ReadEE tFT_ReadEE2 = (tFT_ReadEE)Marshal.GetDelegateForFunctionPointer(pFT_ReadEE, typeof(tFT_ReadEE));
			if (ftHandle != IntPtr.Zero)
			{
				result = tFT_ReadEE2(ftHandle, Address, ref EEValue);
			}
		}
		else if (pFT_ReadEE == IntPtr.Zero)
		{
			Console.WriteLine("Failed to load function FT_ReadEE.");
		}
		return result;
	}

	public FT_STATUS WriteEEPROMLocation(uint Address, ushort EEValue)
	{
		FT_STATUS result = FT_STATUS.FT_OTHER_ERROR;
		if (hFTD2XXDLL == IntPtr.Zero)
		{
			return result;
		}
		if (pFT_WriteEE != IntPtr.Zero)
		{
			tFT_WriteEE tFT_WriteEE2 = (tFT_WriteEE)Marshal.GetDelegateForFunctionPointer(pFT_WriteEE, typeof(tFT_WriteEE));
			if (ftHandle != IntPtr.Zero)
			{
				result = tFT_WriteEE2(ftHandle, Address, EEValue);
			}
		}
		else if (pFT_WriteEE == IntPtr.Zero)
		{
			Console.WriteLine("Failed to load function FT_WriteEE.");
		}
		return result;
	}

	public FT_STATUS EraseEEPROM()
	{
		FT_STATUS fT_STATUS = FT_STATUS.FT_OTHER_ERROR;
		FT_ERROR fT_ERROR = FT_ERROR.FT_NO_ERROR;
		if (hFTD2XXDLL == IntPtr.Zero)
		{
			return fT_STATUS;
		}
		if (pFT_EraseEE != IntPtr.Zero)
		{
			tFT_EraseEE tFT_EraseEE2 = (tFT_EraseEE)Marshal.GetDelegateForFunctionPointer(pFT_EraseEE, typeof(tFT_EraseEE));
			if (ftHandle != IntPtr.Zero)
			{
				FT_DEVICE DeviceType = FT_DEVICE.FT_DEVICE_UNKNOWN;
				GetDeviceType(ref DeviceType);
				if (DeviceType == FT_DEVICE.FT_DEVICE_232R)
				{
					fT_ERROR = FT_ERROR.FT_INCORRECT_DEVICE;
					ErrorHandler(fT_STATUS, fT_ERROR);
				}
				fT_STATUS = tFT_EraseEE2(ftHandle);
			}
		}
		else if (pFT_EraseEE == IntPtr.Zero)
		{
			Console.WriteLine("Failed to load function FT_EraseEE.");
		}
		return fT_STATUS;
	}

	public FT_STATUS ReadFT232BEEPROM(FT232B_EEPROM_STRUCTURE ee232b)
	{
		FT_STATUS fT_STATUS = FT_STATUS.FT_OTHER_ERROR;
		FT_ERROR fT_ERROR = FT_ERROR.FT_NO_ERROR;
		if (hFTD2XXDLL == IntPtr.Zero)
		{
			return fT_STATUS;
		}
		if (pFT_EE_Read != IntPtr.Zero)
		{
			tFT_EE_Read tFT_EE_Read2 = (tFT_EE_Read)Marshal.GetDelegateForFunctionPointer(pFT_EE_Read, typeof(tFT_EE_Read));
			if (ftHandle != IntPtr.Zero)
			{
				FT_DEVICE DeviceType = FT_DEVICE.FT_DEVICE_UNKNOWN;
				GetDeviceType(ref DeviceType);
				if (DeviceType != FT_DEVICE.FT_DEVICE_BM)
				{
					fT_ERROR = FT_ERROR.FT_INCORRECT_DEVICE;
					ErrorHandler(fT_STATUS, fT_ERROR);
				}
				FT_PROGRAM_DATA fT_PROGRAM_DATA = new FT_PROGRAM_DATA();
				fT_PROGRAM_DATA.Signature1 = 0u;
				fT_PROGRAM_DATA.Signature2 = uint.MaxValue;
				fT_PROGRAM_DATA.Version = 2u;
				fT_PROGRAM_DATA.Manufacturer = Marshal.AllocHGlobal(32);
				fT_PROGRAM_DATA.ManufacturerID = Marshal.AllocHGlobal(16);
				fT_PROGRAM_DATA.Description = Marshal.AllocHGlobal(64);
				fT_PROGRAM_DATA.SerialNumber = Marshal.AllocHGlobal(16);
				fT_STATUS = tFT_EE_Read2(ftHandle, fT_PROGRAM_DATA);
				ee232b.Manufacturer = Marshal.PtrToStringAnsi(fT_PROGRAM_DATA.Manufacturer);
				ee232b.ManufacturerID = Marshal.PtrToStringAnsi(fT_PROGRAM_DATA.ManufacturerID);
				ee232b.Description = Marshal.PtrToStringAnsi(fT_PROGRAM_DATA.Description);
				ee232b.SerialNumber = Marshal.PtrToStringAnsi(fT_PROGRAM_DATA.SerialNumber);
				Marshal.FreeHGlobal(fT_PROGRAM_DATA.Manufacturer);
				Marshal.FreeHGlobal(fT_PROGRAM_DATA.ManufacturerID);
				Marshal.FreeHGlobal(fT_PROGRAM_DATA.Description);
				Marshal.FreeHGlobal(fT_PROGRAM_DATA.SerialNumber);
				ee232b.VendorID = fT_PROGRAM_DATA.VendorID;
				ee232b.ProductID = fT_PROGRAM_DATA.ProductID;
				ee232b.MaxPower = fT_PROGRAM_DATA.MaxPower;
				ee232b.SelfPowered = Convert.ToBoolean(fT_PROGRAM_DATA.SelfPowered);
				ee232b.RemoteWakeup = Convert.ToBoolean(fT_PROGRAM_DATA.RemoteWakeup);
				ee232b.PullDownEnable = Convert.ToBoolean(fT_PROGRAM_DATA.PullDownEnable);
				ee232b.SerNumEnable = Convert.ToBoolean(fT_PROGRAM_DATA.SerNumEnable);
				ee232b.USBVersionEnable = Convert.ToBoolean(fT_PROGRAM_DATA.USBVersionEnable);
				ee232b.USBVersion = fT_PROGRAM_DATA.USBVersion;
			}
		}
		else if (pFT_EE_Read == IntPtr.Zero)
		{
			Console.WriteLine("Failed to load function FT_EE_Read.");
		}
		return fT_STATUS;
	}

	public FT_STATUS ReadFT2232EEPROM(FT2232_EEPROM_STRUCTURE ee2232)
	{
		FT_STATUS fT_STATUS = FT_STATUS.FT_OTHER_ERROR;
		FT_ERROR fT_ERROR = FT_ERROR.FT_NO_ERROR;
		if (hFTD2XXDLL == IntPtr.Zero)
		{
			return fT_STATUS;
		}
		if (pFT_EE_Read != IntPtr.Zero)
		{
			tFT_EE_Read tFT_EE_Read2 = (tFT_EE_Read)Marshal.GetDelegateForFunctionPointer(pFT_EE_Read, typeof(tFT_EE_Read));
			if (ftHandle != IntPtr.Zero)
			{
				FT_DEVICE DeviceType = FT_DEVICE.FT_DEVICE_UNKNOWN;
				GetDeviceType(ref DeviceType);
				if (DeviceType != FT_DEVICE.FT_DEVICE_2232)
				{
					fT_ERROR = FT_ERROR.FT_INCORRECT_DEVICE;
					ErrorHandler(fT_STATUS, fT_ERROR);
				}
				FT_PROGRAM_DATA fT_PROGRAM_DATA = new FT_PROGRAM_DATA();
				fT_PROGRAM_DATA.Signature1 = 0u;
				fT_PROGRAM_DATA.Signature2 = uint.MaxValue;
				fT_PROGRAM_DATA.Version = 2u;
				fT_PROGRAM_DATA.Manufacturer = Marshal.AllocHGlobal(32);
				fT_PROGRAM_DATA.ManufacturerID = Marshal.AllocHGlobal(16);
				fT_PROGRAM_DATA.Description = Marshal.AllocHGlobal(64);
				fT_PROGRAM_DATA.SerialNumber = Marshal.AllocHGlobal(16);
				fT_STATUS = tFT_EE_Read2(ftHandle, fT_PROGRAM_DATA);
				ee2232.Manufacturer = Marshal.PtrToStringAnsi(fT_PROGRAM_DATA.Manufacturer);
				ee2232.ManufacturerID = Marshal.PtrToStringAnsi(fT_PROGRAM_DATA.ManufacturerID);
				ee2232.Description = Marshal.PtrToStringAnsi(fT_PROGRAM_DATA.Description);
				ee2232.SerialNumber = Marshal.PtrToStringAnsi(fT_PROGRAM_DATA.SerialNumber);
				Marshal.FreeHGlobal(fT_PROGRAM_DATA.Manufacturer);
				Marshal.FreeHGlobal(fT_PROGRAM_DATA.ManufacturerID);
				Marshal.FreeHGlobal(fT_PROGRAM_DATA.Description);
				Marshal.FreeHGlobal(fT_PROGRAM_DATA.SerialNumber);
				ee2232.VendorID = fT_PROGRAM_DATA.VendorID;
				ee2232.ProductID = fT_PROGRAM_DATA.ProductID;
				ee2232.MaxPower = fT_PROGRAM_DATA.MaxPower;
				ee2232.SelfPowered = Convert.ToBoolean(fT_PROGRAM_DATA.SelfPowered);
				ee2232.RemoteWakeup = Convert.ToBoolean(fT_PROGRAM_DATA.RemoteWakeup);
				ee2232.PullDownEnable = Convert.ToBoolean(fT_PROGRAM_DATA.PullDownEnable5);
				ee2232.SerNumEnable = Convert.ToBoolean(fT_PROGRAM_DATA.SerNumEnable5);
				ee2232.USBVersionEnable = Convert.ToBoolean(fT_PROGRAM_DATA.USBVersionEnable5);
				ee2232.USBVersion = fT_PROGRAM_DATA.USBVersion5;
				ee2232.AIsHighCurrent = Convert.ToBoolean(fT_PROGRAM_DATA.AIsHighCurrent);
				ee2232.BIsHighCurrent = Convert.ToBoolean(fT_PROGRAM_DATA.BIsHighCurrent);
				ee2232.IFAIsFifo = Convert.ToBoolean(fT_PROGRAM_DATA.IFAIsFifo);
				ee2232.IFAIsFifoTar = Convert.ToBoolean(fT_PROGRAM_DATA.IFAIsFifoTar);
				ee2232.IFAIsFastSer = Convert.ToBoolean(fT_PROGRAM_DATA.IFAIsFastSer);
				ee2232.AIsVCP = Convert.ToBoolean(fT_PROGRAM_DATA.AIsVCP);
				ee2232.IFBIsFifo = Convert.ToBoolean(fT_PROGRAM_DATA.IFBIsFifo);
				ee2232.IFBIsFifoTar = Convert.ToBoolean(fT_PROGRAM_DATA.IFBIsFifoTar);
				ee2232.IFBIsFastSer = Convert.ToBoolean(fT_PROGRAM_DATA.IFBIsFastSer);
				ee2232.BIsVCP = Convert.ToBoolean(fT_PROGRAM_DATA.BIsVCP);
			}
		}
		else if (pFT_EE_Read == IntPtr.Zero)
		{
			Console.WriteLine("Failed to load function FT_EE_Read.");
		}
		return fT_STATUS;
	}

	public FT_STATUS ReadFT232REEPROM(FT232R_EEPROM_STRUCTURE ee232r)
	{
		FT_STATUS fT_STATUS = FT_STATUS.FT_OTHER_ERROR;
		FT_ERROR fT_ERROR = FT_ERROR.FT_NO_ERROR;
		if (hFTD2XXDLL == IntPtr.Zero)
		{
			return fT_STATUS;
		}
		if (pFT_EE_Read != IntPtr.Zero)
		{
			tFT_EE_Read tFT_EE_Read2 = (tFT_EE_Read)Marshal.GetDelegateForFunctionPointer(pFT_EE_Read, typeof(tFT_EE_Read));
			if (ftHandle != IntPtr.Zero)
			{
				FT_DEVICE DeviceType = FT_DEVICE.FT_DEVICE_UNKNOWN;
				GetDeviceType(ref DeviceType);
				if (DeviceType != FT_DEVICE.FT_DEVICE_232R)
				{
					fT_ERROR = FT_ERROR.FT_INCORRECT_DEVICE;
					ErrorHandler(fT_STATUS, fT_ERROR);
				}
				FT_PROGRAM_DATA fT_PROGRAM_DATA = new FT_PROGRAM_DATA();
				fT_PROGRAM_DATA.Signature1 = 0u;
				fT_PROGRAM_DATA.Signature2 = uint.MaxValue;
				fT_PROGRAM_DATA.Version = 2u;
				fT_PROGRAM_DATA.Manufacturer = Marshal.AllocHGlobal(32);
				fT_PROGRAM_DATA.ManufacturerID = Marshal.AllocHGlobal(16);
				fT_PROGRAM_DATA.Description = Marshal.AllocHGlobal(64);
				fT_PROGRAM_DATA.SerialNumber = Marshal.AllocHGlobal(16);
				fT_STATUS = tFT_EE_Read2(ftHandle, fT_PROGRAM_DATA);
				ee232r.Manufacturer = Marshal.PtrToStringAnsi(fT_PROGRAM_DATA.Manufacturer);
				ee232r.ManufacturerID = Marshal.PtrToStringAnsi(fT_PROGRAM_DATA.ManufacturerID);
				ee232r.Description = Marshal.PtrToStringAnsi(fT_PROGRAM_DATA.Description);
				ee232r.SerialNumber = Marshal.PtrToStringAnsi(fT_PROGRAM_DATA.SerialNumber);
				Marshal.FreeHGlobal(fT_PROGRAM_DATA.Manufacturer);
				Marshal.FreeHGlobal(fT_PROGRAM_DATA.ManufacturerID);
				Marshal.FreeHGlobal(fT_PROGRAM_DATA.Description);
				Marshal.FreeHGlobal(fT_PROGRAM_DATA.SerialNumber);
				ee232r.VendorID = fT_PROGRAM_DATA.VendorID;
				ee232r.ProductID = fT_PROGRAM_DATA.ProductID;
				ee232r.MaxPower = fT_PROGRAM_DATA.MaxPower;
				ee232r.SelfPowered = Convert.ToBoolean(fT_PROGRAM_DATA.SelfPowered);
				ee232r.RemoteWakeup = Convert.ToBoolean(fT_PROGRAM_DATA.RemoteWakeup);
				ee232r.UseExtOsc = Convert.ToBoolean(fT_PROGRAM_DATA.UseExtOsc);
				ee232r.HighDriveIOs = Convert.ToBoolean(fT_PROGRAM_DATA.HighDriveIOs);
				ee232r.EndpointSize = fT_PROGRAM_DATA.EndpointSize;
				ee232r.PullDownEnable = Convert.ToBoolean(fT_PROGRAM_DATA.PullDownEnableR);
				ee232r.SerNumEnable = Convert.ToBoolean(fT_PROGRAM_DATA.SerNumEnableR);
				ee232r.InvertTXD = Convert.ToBoolean(fT_PROGRAM_DATA.InvertTXD);
				ee232r.InvertRXD = Convert.ToBoolean(fT_PROGRAM_DATA.InvertRXD);
				ee232r.InvertRTS = Convert.ToBoolean(fT_PROGRAM_DATA.InvertRTS);
				ee232r.InvertCTS = Convert.ToBoolean(fT_PROGRAM_DATA.InvertCTS);
				ee232r.InvertDTR = Convert.ToBoolean(fT_PROGRAM_DATA.InvertDTR);
				ee232r.InvertDSR = Convert.ToBoolean(fT_PROGRAM_DATA.InvertDSR);
				ee232r.InvertDCD = Convert.ToBoolean(fT_PROGRAM_DATA.InvertDCD);
				ee232r.InvertRI = Convert.ToBoolean(fT_PROGRAM_DATA.InvertRI);
				ee232r.Cbus0 = fT_PROGRAM_DATA.Cbus0;
				ee232r.Cbus1 = fT_PROGRAM_DATA.Cbus1;
				ee232r.Cbus2 = fT_PROGRAM_DATA.Cbus2;
				ee232r.Cbus3 = fT_PROGRAM_DATA.Cbus3;
				ee232r.Cbus4 = fT_PROGRAM_DATA.Cbus4;
				ee232r.RIsD2XX = Convert.ToBoolean(fT_PROGRAM_DATA.RIsD2XX);
			}
		}
		else if (pFT_EE_Read == IntPtr.Zero)
		{
			Console.WriteLine("Failed to load function FT_EE_Read.");
		}
		return fT_STATUS;
	}

	public FT_STATUS ReadFT2232HEEPROM(FT2232H_EEPROM_STRUCTURE ee2232h)
	{
		FT_STATUS fT_STATUS = FT_STATUS.FT_OTHER_ERROR;
		FT_ERROR fT_ERROR = FT_ERROR.FT_NO_ERROR;
		if (hFTD2XXDLL == IntPtr.Zero)
		{
			return fT_STATUS;
		}
		if (pFT_EE_Read != IntPtr.Zero)
		{
			tFT_EE_Read tFT_EE_Read2 = (tFT_EE_Read)Marshal.GetDelegateForFunctionPointer(pFT_EE_Read, typeof(tFT_EE_Read));
			if (ftHandle != IntPtr.Zero)
			{
				FT_DEVICE DeviceType = FT_DEVICE.FT_DEVICE_UNKNOWN;
				GetDeviceType(ref DeviceType);
				if (DeviceType != FT_DEVICE.FT_DEVICE_2232H)
				{
					fT_ERROR = FT_ERROR.FT_INCORRECT_DEVICE;
					ErrorHandler(fT_STATUS, fT_ERROR);
				}
				FT_PROGRAM_DATA fT_PROGRAM_DATA = new FT_PROGRAM_DATA();
				fT_PROGRAM_DATA.Signature1 = 0u;
				fT_PROGRAM_DATA.Signature2 = uint.MaxValue;
				fT_PROGRAM_DATA.Version = 3u;
				fT_PROGRAM_DATA.Manufacturer = Marshal.AllocHGlobal(32);
				fT_PROGRAM_DATA.ManufacturerID = Marshal.AllocHGlobal(16);
				fT_PROGRAM_DATA.Description = Marshal.AllocHGlobal(64);
				fT_PROGRAM_DATA.SerialNumber = Marshal.AllocHGlobal(16);
				fT_STATUS = tFT_EE_Read2(ftHandle, fT_PROGRAM_DATA);
				ee2232h.Manufacturer = Marshal.PtrToStringAnsi(fT_PROGRAM_DATA.Manufacturer);
				ee2232h.ManufacturerID = Marshal.PtrToStringAnsi(fT_PROGRAM_DATA.ManufacturerID);
				ee2232h.Description = Marshal.PtrToStringAnsi(fT_PROGRAM_DATA.Description);
				ee2232h.SerialNumber = Marshal.PtrToStringAnsi(fT_PROGRAM_DATA.SerialNumber);
				Marshal.FreeHGlobal(fT_PROGRAM_DATA.Manufacturer);
				Marshal.FreeHGlobal(fT_PROGRAM_DATA.ManufacturerID);
				Marshal.FreeHGlobal(fT_PROGRAM_DATA.Description);
				Marshal.FreeHGlobal(fT_PROGRAM_DATA.SerialNumber);
				ee2232h.VendorID = fT_PROGRAM_DATA.VendorID;
				ee2232h.ProductID = fT_PROGRAM_DATA.ProductID;
				ee2232h.MaxPower = fT_PROGRAM_DATA.MaxPower;
				ee2232h.SelfPowered = Convert.ToBoolean(fT_PROGRAM_DATA.SelfPowered);
				ee2232h.RemoteWakeup = Convert.ToBoolean(fT_PROGRAM_DATA.RemoteWakeup);
				ee2232h.PullDownEnable = Convert.ToBoolean(fT_PROGRAM_DATA.PullDownEnable7);
				ee2232h.SerNumEnable = Convert.ToBoolean(fT_PROGRAM_DATA.SerNumEnable7);
				ee2232h.ALSlowSlew = Convert.ToBoolean(fT_PROGRAM_DATA.ALSlowSlew);
				ee2232h.ALSchmittInput = Convert.ToBoolean(fT_PROGRAM_DATA.ALSchmittInput);
				ee2232h.ALDriveCurrent = fT_PROGRAM_DATA.ALDriveCurrent;
				ee2232h.AHSlowSlew = Convert.ToBoolean(fT_PROGRAM_DATA.AHSlowSlew);
				ee2232h.AHSchmittInput = Convert.ToBoolean(fT_PROGRAM_DATA.AHSchmittInput);
				ee2232h.AHDriveCurrent = fT_PROGRAM_DATA.AHDriveCurrent;
				ee2232h.BLSlowSlew = Convert.ToBoolean(fT_PROGRAM_DATA.BLSlowSlew);
				ee2232h.BLSchmittInput = Convert.ToBoolean(fT_PROGRAM_DATA.BLSchmittInput);
				ee2232h.BLDriveCurrent = fT_PROGRAM_DATA.BLDriveCurrent;
				ee2232h.BHSlowSlew = Convert.ToBoolean(fT_PROGRAM_DATA.BHSlowSlew);
				ee2232h.BHSchmittInput = Convert.ToBoolean(fT_PROGRAM_DATA.BHSchmittInput);
				ee2232h.BHDriveCurrent = fT_PROGRAM_DATA.BHDriveCurrent;
				ee2232h.IFAIsFifo = Convert.ToBoolean(fT_PROGRAM_DATA.IFAIsFifo7);
				ee2232h.IFAIsFifoTar = Convert.ToBoolean(fT_PROGRAM_DATA.IFAIsFifoTar7);
				ee2232h.IFAIsFastSer = Convert.ToBoolean(fT_PROGRAM_DATA.IFAIsFastSer7);
				ee2232h.AIsVCP = Convert.ToBoolean(fT_PROGRAM_DATA.AIsVCP7);
				ee2232h.IFBIsFifo = Convert.ToBoolean(fT_PROGRAM_DATA.IFBIsFifo7);
				ee2232h.IFBIsFifoTar = Convert.ToBoolean(fT_PROGRAM_DATA.IFBIsFifoTar7);
				ee2232h.IFBIsFastSer = Convert.ToBoolean(fT_PROGRAM_DATA.IFBIsFastSer7);
				ee2232h.BIsVCP = Convert.ToBoolean(fT_PROGRAM_DATA.BIsVCP7);
				ee2232h.PowerSaveEnable = Convert.ToBoolean(fT_PROGRAM_DATA.PowerSaveEnable);
			}
		}
		else if (pFT_EE_Read == IntPtr.Zero)
		{
			Console.WriteLine("Failed to load function FT_EE_Read.");
		}
		return fT_STATUS;
	}

	public FT_STATUS ReadFT4232HEEPROM(FT4232H_EEPROM_STRUCTURE ee4232h)
	{
		FT_STATUS fT_STATUS = FT_STATUS.FT_OTHER_ERROR;
		FT_ERROR fT_ERROR = FT_ERROR.FT_NO_ERROR;
		if (hFTD2XXDLL == IntPtr.Zero)
		{
			return fT_STATUS;
		}
		if (pFT_EE_Read != IntPtr.Zero)
		{
			tFT_EE_Read tFT_EE_Read2 = (tFT_EE_Read)Marshal.GetDelegateForFunctionPointer(pFT_EE_Read, typeof(tFT_EE_Read));
			if (ftHandle != IntPtr.Zero)
			{
				FT_DEVICE DeviceType = FT_DEVICE.FT_DEVICE_UNKNOWN;
				GetDeviceType(ref DeviceType);
				if (DeviceType != FT_DEVICE.FT_DEVICE_4232H)
				{
					fT_ERROR = FT_ERROR.FT_INCORRECT_DEVICE;
					ErrorHandler(fT_STATUS, fT_ERROR);
				}
				FT_PROGRAM_DATA fT_PROGRAM_DATA = new FT_PROGRAM_DATA();
				fT_PROGRAM_DATA.Signature1 = 0u;
				fT_PROGRAM_DATA.Signature2 = uint.MaxValue;
				fT_PROGRAM_DATA.Version = 4u;
				fT_PROGRAM_DATA.Manufacturer = Marshal.AllocHGlobal(32);
				fT_PROGRAM_DATA.ManufacturerID = Marshal.AllocHGlobal(16);
				fT_PROGRAM_DATA.Description = Marshal.AllocHGlobal(64);
				fT_PROGRAM_DATA.SerialNumber = Marshal.AllocHGlobal(16);
				fT_STATUS = tFT_EE_Read2(ftHandle, fT_PROGRAM_DATA);
				ee4232h.Manufacturer = Marshal.PtrToStringAnsi(fT_PROGRAM_DATA.Manufacturer);
				ee4232h.ManufacturerID = Marshal.PtrToStringAnsi(fT_PROGRAM_DATA.ManufacturerID);
				ee4232h.Description = Marshal.PtrToStringAnsi(fT_PROGRAM_DATA.Description);
				ee4232h.SerialNumber = Marshal.PtrToStringAnsi(fT_PROGRAM_DATA.SerialNumber);
				Marshal.FreeHGlobal(fT_PROGRAM_DATA.Manufacturer);
				Marshal.FreeHGlobal(fT_PROGRAM_DATA.ManufacturerID);
				Marshal.FreeHGlobal(fT_PROGRAM_DATA.Description);
				Marshal.FreeHGlobal(fT_PROGRAM_DATA.SerialNumber);
				ee4232h.VendorID = fT_PROGRAM_DATA.VendorID;
				ee4232h.ProductID = fT_PROGRAM_DATA.ProductID;
				ee4232h.MaxPower = fT_PROGRAM_DATA.MaxPower;
				ee4232h.SelfPowered = Convert.ToBoolean(fT_PROGRAM_DATA.SelfPowered);
				ee4232h.RemoteWakeup = Convert.ToBoolean(fT_PROGRAM_DATA.RemoteWakeup);
				ee4232h.PullDownEnable = Convert.ToBoolean(fT_PROGRAM_DATA.PullDownEnable8);
				ee4232h.SerNumEnable = Convert.ToBoolean(fT_PROGRAM_DATA.SerNumEnable8);
				ee4232h.ASlowSlew = Convert.ToBoolean(fT_PROGRAM_DATA.ASlowSlew);
				ee4232h.ASchmittInput = Convert.ToBoolean(fT_PROGRAM_DATA.ASchmittInput);
				ee4232h.ADriveCurrent = fT_PROGRAM_DATA.ADriveCurrent;
				ee4232h.BSlowSlew = Convert.ToBoolean(fT_PROGRAM_DATA.BSlowSlew);
				ee4232h.BSchmittInput = Convert.ToBoolean(fT_PROGRAM_DATA.BSchmittInput);
				ee4232h.BDriveCurrent = fT_PROGRAM_DATA.BDriveCurrent;
				ee4232h.CSlowSlew = Convert.ToBoolean(fT_PROGRAM_DATA.CSlowSlew);
				ee4232h.CSchmittInput = Convert.ToBoolean(fT_PROGRAM_DATA.CSchmittInput);
				ee4232h.CDriveCurrent = fT_PROGRAM_DATA.CDriveCurrent;
				ee4232h.DSlowSlew = Convert.ToBoolean(fT_PROGRAM_DATA.DSlowSlew);
				ee4232h.DSchmittInput = Convert.ToBoolean(fT_PROGRAM_DATA.DSchmittInput);
				ee4232h.DDriveCurrent = fT_PROGRAM_DATA.DDriveCurrent;
				ee4232h.ARIIsTXDEN = Convert.ToBoolean(fT_PROGRAM_DATA.ARIIsTXDEN);
				ee4232h.BRIIsTXDEN = Convert.ToBoolean(fT_PROGRAM_DATA.BRIIsTXDEN);
				ee4232h.CRIIsTXDEN = Convert.ToBoolean(fT_PROGRAM_DATA.CRIIsTXDEN);
				ee4232h.DRIIsTXDEN = Convert.ToBoolean(fT_PROGRAM_DATA.DRIIsTXDEN);
				ee4232h.AIsVCP = Convert.ToBoolean(fT_PROGRAM_DATA.AIsVCP8);
				ee4232h.BIsVCP = Convert.ToBoolean(fT_PROGRAM_DATA.BIsVCP8);
				ee4232h.CIsVCP = Convert.ToBoolean(fT_PROGRAM_DATA.CIsVCP8);
				ee4232h.DIsVCP = Convert.ToBoolean(fT_PROGRAM_DATA.DIsVCP8);
			}
		}
		else if (pFT_EE_Read == IntPtr.Zero)
		{
			Console.WriteLine("Failed to load function FT_EE_Read.");
		}
		return fT_STATUS;
	}

	public FT_STATUS ReadFT232HEEPROM(FT232H_EEPROM_STRUCTURE ee232h)
	{
		FT_STATUS fT_STATUS = FT_STATUS.FT_OTHER_ERROR;
		FT_ERROR fT_ERROR = FT_ERROR.FT_NO_ERROR;
		if (hFTD2XXDLL == IntPtr.Zero)
		{
			return fT_STATUS;
		}
		if (pFT_EE_Read != IntPtr.Zero)
		{
			tFT_EE_Read tFT_EE_Read2 = (tFT_EE_Read)Marshal.GetDelegateForFunctionPointer(pFT_EE_Read, typeof(tFT_EE_Read));
			if (ftHandle != IntPtr.Zero)
			{
				FT_DEVICE DeviceType = FT_DEVICE.FT_DEVICE_UNKNOWN;
				GetDeviceType(ref DeviceType);
				if (DeviceType != FT_DEVICE.FT_DEVICE_232H && DeviceType != FT_DEVICE.FT_DEVICE_232HP && DeviceType != FT_DEVICE.FT_DEVICE_233HP)
				{
					fT_ERROR = FT_ERROR.FT_INCORRECT_DEVICE;
					ErrorHandler(fT_STATUS, fT_ERROR);
				}
				FT_PROGRAM_DATA fT_PROGRAM_DATA = new FT_PROGRAM_DATA();
				fT_PROGRAM_DATA.Signature1 = 0u;
				fT_PROGRAM_DATA.Signature2 = uint.MaxValue;
				fT_PROGRAM_DATA.Version = 5u;
				fT_PROGRAM_DATA.Manufacturer = Marshal.AllocHGlobal(32);
				fT_PROGRAM_DATA.ManufacturerID = Marshal.AllocHGlobal(16);
				fT_PROGRAM_DATA.Description = Marshal.AllocHGlobal(64);
				fT_PROGRAM_DATA.SerialNumber = Marshal.AllocHGlobal(16);
				fT_STATUS = tFT_EE_Read2(ftHandle, fT_PROGRAM_DATA);
				ee232h.Manufacturer = Marshal.PtrToStringAnsi(fT_PROGRAM_DATA.Manufacturer);
				ee232h.ManufacturerID = Marshal.PtrToStringAnsi(fT_PROGRAM_DATA.ManufacturerID);
				ee232h.Description = Marshal.PtrToStringAnsi(fT_PROGRAM_DATA.Description);
				ee232h.SerialNumber = Marshal.PtrToStringAnsi(fT_PROGRAM_DATA.SerialNumber);
				Marshal.FreeHGlobal(fT_PROGRAM_DATA.Manufacturer);
				Marshal.FreeHGlobal(fT_PROGRAM_DATA.ManufacturerID);
				Marshal.FreeHGlobal(fT_PROGRAM_DATA.Description);
				Marshal.FreeHGlobal(fT_PROGRAM_DATA.SerialNumber);
				ee232h.VendorID = fT_PROGRAM_DATA.VendorID;
				ee232h.ProductID = fT_PROGRAM_DATA.ProductID;
				ee232h.MaxPower = fT_PROGRAM_DATA.MaxPower;
				ee232h.SelfPowered = Convert.ToBoolean(fT_PROGRAM_DATA.SelfPowered);
				ee232h.RemoteWakeup = Convert.ToBoolean(fT_PROGRAM_DATA.RemoteWakeup);
				ee232h.PullDownEnable = Convert.ToBoolean(fT_PROGRAM_DATA.PullDownEnableH);
				ee232h.SerNumEnable = Convert.ToBoolean(fT_PROGRAM_DATA.SerNumEnableH);
				ee232h.ACSlowSlew = Convert.ToBoolean(fT_PROGRAM_DATA.ACSlowSlewH);
				ee232h.ACSchmittInput = Convert.ToBoolean(fT_PROGRAM_DATA.ACSchmittInputH);
				ee232h.ACDriveCurrent = fT_PROGRAM_DATA.ACDriveCurrentH;
				ee232h.ADSlowSlew = Convert.ToBoolean(fT_PROGRAM_DATA.ADSlowSlewH);
				ee232h.ADSchmittInput = Convert.ToBoolean(fT_PROGRAM_DATA.ADSchmittInputH);
				ee232h.ADDriveCurrent = fT_PROGRAM_DATA.ADDriveCurrentH;
				ee232h.Cbus0 = fT_PROGRAM_DATA.Cbus0H;
				ee232h.Cbus1 = fT_PROGRAM_DATA.Cbus1H;
				ee232h.Cbus2 = fT_PROGRAM_DATA.Cbus2H;
				ee232h.Cbus3 = fT_PROGRAM_DATA.Cbus3H;
				ee232h.Cbus4 = fT_PROGRAM_DATA.Cbus4H;
				ee232h.Cbus5 = fT_PROGRAM_DATA.Cbus5H;
				ee232h.Cbus6 = fT_PROGRAM_DATA.Cbus6H;
				ee232h.Cbus7 = fT_PROGRAM_DATA.Cbus7H;
				ee232h.Cbus8 = fT_PROGRAM_DATA.Cbus8H;
				ee232h.Cbus9 = fT_PROGRAM_DATA.Cbus9H;
				ee232h.IsFifo = Convert.ToBoolean(fT_PROGRAM_DATA.IsFifoH);
				ee232h.IsFifoTar = Convert.ToBoolean(fT_PROGRAM_DATA.IsFifoTarH);
				ee232h.IsFastSer = Convert.ToBoolean(fT_PROGRAM_DATA.IsFastSerH);
				ee232h.IsFT1248 = Convert.ToBoolean(fT_PROGRAM_DATA.IsFT1248H);
				ee232h.FT1248Cpol = Convert.ToBoolean(fT_PROGRAM_DATA.FT1248CpolH);
				ee232h.FT1248Lsb = Convert.ToBoolean(fT_PROGRAM_DATA.FT1248LsbH);
				ee232h.FT1248FlowControl = Convert.ToBoolean(fT_PROGRAM_DATA.FT1248FlowControlH);
				ee232h.IsVCP = Convert.ToBoolean(fT_PROGRAM_DATA.IsVCPH);
				ee232h.PowerSaveEnable = Convert.ToBoolean(fT_PROGRAM_DATA.PowerSaveEnableH);
			}
		}
		else if (pFT_EE_Read == IntPtr.Zero)
		{
			Console.WriteLine("Failed to load function FT_EE_Read.");
		}
		return fT_STATUS;
	}

	public FT_STATUS ReadXSeriesEEPROM(FT_XSERIES_EEPROM_STRUCTURE eeX)
	{
		FT_STATUS fT_STATUS = FT_STATUS.FT_OTHER_ERROR;
		FT_ERROR fT_ERROR = FT_ERROR.FT_NO_ERROR;
		if (hFTD2XXDLL == IntPtr.Zero)
		{
			return fT_STATUS;
		}
		if (pFT_EEPROM_Read != IntPtr.Zero)
		{
			tFT_EEPROM_Read tFT_EEPROM_Read2 = (tFT_EEPROM_Read)Marshal.GetDelegateForFunctionPointer(pFT_EEPROM_Read, typeof(tFT_EEPROM_Read));
			if (ftHandle != IntPtr.Zero)
			{
				FT_DEVICE DeviceType = FT_DEVICE.FT_DEVICE_UNKNOWN;
				GetDeviceType(ref DeviceType);
				if (DeviceType != FT_DEVICE.FT_DEVICE_X_SERIES)
				{
					fT_ERROR = FT_ERROR.FT_INCORRECT_DEVICE;
					ErrorHandler(fT_STATUS, fT_ERROR);
				}
				FT_XSERIES_DATA fT_XSERIES_DATA = default(FT_XSERIES_DATA);
				FT_EEPROM_HEADER common = default(FT_EEPROM_HEADER);
				byte[] array = new byte[32];
				byte[] array2 = new byte[16];
				byte[] array3 = new byte[64];
				byte[] array4 = new byte[16];
				common.deviceType = 9u;
				fT_XSERIES_DATA.common = common;
				int num = Marshal.SizeOf((object)fT_XSERIES_DATA);
				IntPtr intPtr = Marshal.AllocHGlobal(num);
				Marshal.StructureToPtr((object)fT_XSERIES_DATA, intPtr, false);
				fT_STATUS = tFT_EEPROM_Read2(ftHandle, intPtr, (uint)num, array, array2, array3, array4);
				if (fT_STATUS == FT_STATUS.FT_OK)
				{
					fT_XSERIES_DATA = (FT_XSERIES_DATA)Marshal.PtrToStructure(intPtr, typeof(FT_XSERIES_DATA));
					UTF8Encoding uTF8Encoding = new UTF8Encoding();
					eeX.Manufacturer = uTF8Encoding.GetString(array);
					eeX.ManufacturerID = uTF8Encoding.GetString(array2);
					eeX.Description = uTF8Encoding.GetString(array3);
					eeX.SerialNumber = uTF8Encoding.GetString(array4);
					eeX.VendorID = fT_XSERIES_DATA.common.VendorId;
					eeX.ProductID = fT_XSERIES_DATA.common.ProductId;
					eeX.MaxPower = fT_XSERIES_DATA.common.MaxPower;
					eeX.SelfPowered = Convert.ToBoolean(fT_XSERIES_DATA.common.SelfPowered);
					eeX.RemoteWakeup = Convert.ToBoolean(fT_XSERIES_DATA.common.RemoteWakeup);
					eeX.SerNumEnable = Convert.ToBoolean(fT_XSERIES_DATA.common.SerNumEnable);
					eeX.PullDownEnable = Convert.ToBoolean(fT_XSERIES_DATA.common.PullDownEnable);
					eeX.Cbus0 = fT_XSERIES_DATA.Cbus0;
					eeX.Cbus1 = fT_XSERIES_DATA.Cbus1;
					eeX.Cbus2 = fT_XSERIES_DATA.Cbus2;
					eeX.Cbus3 = fT_XSERIES_DATA.Cbus3;
					eeX.Cbus4 = fT_XSERIES_DATA.Cbus4;
					eeX.Cbus5 = fT_XSERIES_DATA.Cbus5;
					eeX.Cbus6 = fT_XSERIES_DATA.Cbus6;
					eeX.ACDriveCurrent = fT_XSERIES_DATA.ACDriveCurrent;
					eeX.ACSchmittInput = fT_XSERIES_DATA.ACSchmittInput;
					eeX.ACSlowSlew = fT_XSERIES_DATA.ACSlowSlew;
					eeX.ADDriveCurrent = fT_XSERIES_DATA.ADDriveCurrent;
					eeX.ADSchmittInput = fT_XSERIES_DATA.ADSchmittInput;
					eeX.ADSlowSlew = fT_XSERIES_DATA.ADSlowSlew;
					eeX.BCDDisableSleep = fT_XSERIES_DATA.BCDDisableSleep;
					eeX.BCDEnable = fT_XSERIES_DATA.BCDEnable;
					eeX.BCDForceCbusPWREN = fT_XSERIES_DATA.BCDForceCbusPWREN;
					eeX.FT1248Cpol = fT_XSERIES_DATA.FT1248Cpol;
					eeX.FT1248FlowControl = fT_XSERIES_DATA.FT1248FlowControl;
					eeX.FT1248Lsb = fT_XSERIES_DATA.FT1248Lsb;
					eeX.I2CDeviceId = fT_XSERIES_DATA.I2CDeviceId;
					eeX.I2CDisableSchmitt = fT_XSERIES_DATA.I2CDisableSchmitt;
					eeX.I2CSlaveAddress = fT_XSERIES_DATA.I2CSlaveAddress;
					eeX.InvertCTS = fT_XSERIES_DATA.InvertCTS;
					eeX.InvertDCD = fT_XSERIES_DATA.InvertDCD;
					eeX.InvertDSR = fT_XSERIES_DATA.InvertDSR;
					eeX.InvertDTR = fT_XSERIES_DATA.InvertDTR;
					eeX.InvertRI = fT_XSERIES_DATA.InvertRI;
					eeX.InvertRTS = fT_XSERIES_DATA.InvertRTS;
					eeX.InvertRXD = fT_XSERIES_DATA.InvertRXD;
					eeX.InvertTXD = fT_XSERIES_DATA.InvertTXD;
					eeX.PowerSaveEnable = fT_XSERIES_DATA.PowerSaveEnable;
					eeX.RS485EchoSuppress = fT_XSERIES_DATA.RS485EchoSuppress;
					eeX.IsVCP = fT_XSERIES_DATA.DriverType;
				}
			}
		}
		else if (pFT_EE_Read == IntPtr.Zero)
		{
			Console.WriteLine("Failed to load function FT_EE_Read.");
		}
		return fT_STATUS;
	}

	public FT_STATUS WriteFT232BEEPROM(FT232B_EEPROM_STRUCTURE ee232b)
	{
		FT_STATUS fT_STATUS = FT_STATUS.FT_OTHER_ERROR;
		FT_ERROR fT_ERROR = FT_ERROR.FT_NO_ERROR;
		if (hFTD2XXDLL == IntPtr.Zero)
		{
			return fT_STATUS;
		}
		if (pFT_EE_Program != IntPtr.Zero)
		{
			tFT_EE_Program tFT_EE_Program2 = (tFT_EE_Program)Marshal.GetDelegateForFunctionPointer(pFT_EE_Program, typeof(tFT_EE_Program));
			if (ftHandle != IntPtr.Zero)
			{
				FT_DEVICE DeviceType = FT_DEVICE.FT_DEVICE_UNKNOWN;
				GetDeviceType(ref DeviceType);
				if (DeviceType != FT_DEVICE.FT_DEVICE_BM)
				{
					fT_ERROR = FT_ERROR.FT_INCORRECT_DEVICE;
					ErrorHandler(fT_STATUS, fT_ERROR);
				}
				if ((ee232b.VendorID == 0) | (ee232b.ProductID == 0))
				{
					return FT_STATUS.FT_INVALID_PARAMETER;
				}
				FT_PROGRAM_DATA fT_PROGRAM_DATA = new FT_PROGRAM_DATA();
				fT_PROGRAM_DATA.Signature1 = 0u;
				fT_PROGRAM_DATA.Signature2 = uint.MaxValue;
				fT_PROGRAM_DATA.Version = 2u;
				fT_PROGRAM_DATA.Manufacturer = Marshal.AllocHGlobal(32);
				fT_PROGRAM_DATA.ManufacturerID = Marshal.AllocHGlobal(16);
				fT_PROGRAM_DATA.Description = Marshal.AllocHGlobal(64);
				fT_PROGRAM_DATA.SerialNumber = Marshal.AllocHGlobal(16);
				if (ee232b.Manufacturer.Length > 32)
				{
					ee232b.Manufacturer = ee232b.Manufacturer.Substring(0, 32);
				}
				if (ee232b.ManufacturerID.Length > 16)
				{
					ee232b.ManufacturerID = ee232b.ManufacturerID.Substring(0, 16);
				}
				if (ee232b.Description.Length > 64)
				{
					ee232b.Description = ee232b.Description.Substring(0, 64);
				}
				if (ee232b.SerialNumber.Length > 16)
				{
					ee232b.SerialNumber = ee232b.SerialNumber.Substring(0, 16);
				}
				fT_PROGRAM_DATA.Manufacturer = Marshal.StringToHGlobalAnsi(ee232b.Manufacturer);
				fT_PROGRAM_DATA.ManufacturerID = Marshal.StringToHGlobalAnsi(ee232b.ManufacturerID);
				fT_PROGRAM_DATA.Description = Marshal.StringToHGlobalAnsi(ee232b.Description);
				fT_PROGRAM_DATA.SerialNumber = Marshal.StringToHGlobalAnsi(ee232b.SerialNumber);
				fT_PROGRAM_DATA.VendorID = ee232b.VendorID;
				fT_PROGRAM_DATA.ProductID = ee232b.ProductID;
				fT_PROGRAM_DATA.MaxPower = ee232b.MaxPower;
				fT_PROGRAM_DATA.SelfPowered = Convert.ToUInt16(ee232b.SelfPowered);
				fT_PROGRAM_DATA.RemoteWakeup = Convert.ToUInt16(ee232b.RemoteWakeup);
				fT_PROGRAM_DATA.Rev4 = Convert.ToByte(value: true);
				fT_PROGRAM_DATA.PullDownEnable = Convert.ToByte(ee232b.PullDownEnable);
				fT_PROGRAM_DATA.SerNumEnable = Convert.ToByte(ee232b.SerNumEnable);
				fT_PROGRAM_DATA.USBVersionEnable = Convert.ToByte(ee232b.USBVersionEnable);
				fT_PROGRAM_DATA.USBVersion = ee232b.USBVersion;
				fT_STATUS = tFT_EE_Program2(ftHandle, fT_PROGRAM_DATA);
				Marshal.FreeHGlobal(fT_PROGRAM_DATA.Manufacturer);
				Marshal.FreeHGlobal(fT_PROGRAM_DATA.ManufacturerID);
				Marshal.FreeHGlobal(fT_PROGRAM_DATA.Description);
				Marshal.FreeHGlobal(fT_PROGRAM_DATA.SerialNumber);
			}
		}
		else if (pFT_EE_Program == IntPtr.Zero)
		{
			Console.WriteLine("Failed to load function FT_EE_Program.");
		}
		return fT_STATUS;
	}

	public FT_STATUS WriteFT2232EEPROM(FT2232_EEPROM_STRUCTURE ee2232)
	{
		FT_STATUS fT_STATUS = FT_STATUS.FT_OTHER_ERROR;
		FT_ERROR fT_ERROR = FT_ERROR.FT_NO_ERROR;
		if (hFTD2XXDLL == IntPtr.Zero)
		{
			return fT_STATUS;
		}
		if (pFT_EE_Program != IntPtr.Zero)
		{
			tFT_EE_Program tFT_EE_Program2 = (tFT_EE_Program)Marshal.GetDelegateForFunctionPointer(pFT_EE_Program, typeof(tFT_EE_Program));
			if (ftHandle != IntPtr.Zero)
			{
				FT_DEVICE DeviceType = FT_DEVICE.FT_DEVICE_UNKNOWN;
				GetDeviceType(ref DeviceType);
				if (DeviceType != FT_DEVICE.FT_DEVICE_2232)
				{
					fT_ERROR = FT_ERROR.FT_INCORRECT_DEVICE;
					ErrorHandler(fT_STATUS, fT_ERROR);
				}
				if ((ee2232.VendorID == 0) | (ee2232.ProductID == 0))
				{
					return FT_STATUS.FT_INVALID_PARAMETER;
				}
				FT_PROGRAM_DATA fT_PROGRAM_DATA = new FT_PROGRAM_DATA();
				fT_PROGRAM_DATA.Signature1 = 0u;
				fT_PROGRAM_DATA.Signature2 = uint.MaxValue;
				fT_PROGRAM_DATA.Version = 2u;
				fT_PROGRAM_DATA.Manufacturer = Marshal.AllocHGlobal(32);
				fT_PROGRAM_DATA.ManufacturerID = Marshal.AllocHGlobal(16);
				fT_PROGRAM_DATA.Description = Marshal.AllocHGlobal(64);
				fT_PROGRAM_DATA.SerialNumber = Marshal.AllocHGlobal(16);
				if (ee2232.Manufacturer.Length > 32)
				{
					ee2232.Manufacturer = ee2232.Manufacturer.Substring(0, 32);
				}
				if (ee2232.ManufacturerID.Length > 16)
				{
					ee2232.ManufacturerID = ee2232.ManufacturerID.Substring(0, 16);
				}
				if (ee2232.Description.Length > 64)
				{
					ee2232.Description = ee2232.Description.Substring(0, 64);
				}
				if (ee2232.SerialNumber.Length > 16)
				{
					ee2232.SerialNumber = ee2232.SerialNumber.Substring(0, 16);
				}
				fT_PROGRAM_DATA.Manufacturer = Marshal.StringToHGlobalAnsi(ee2232.Manufacturer);
				fT_PROGRAM_DATA.ManufacturerID = Marshal.StringToHGlobalAnsi(ee2232.ManufacturerID);
				fT_PROGRAM_DATA.Description = Marshal.StringToHGlobalAnsi(ee2232.Description);
				fT_PROGRAM_DATA.SerialNumber = Marshal.StringToHGlobalAnsi(ee2232.SerialNumber);
				fT_PROGRAM_DATA.VendorID = ee2232.VendorID;
				fT_PROGRAM_DATA.ProductID = ee2232.ProductID;
				fT_PROGRAM_DATA.MaxPower = ee2232.MaxPower;
				fT_PROGRAM_DATA.SelfPowered = Convert.ToUInt16(ee2232.SelfPowered);
				fT_PROGRAM_DATA.RemoteWakeup = Convert.ToUInt16(ee2232.RemoteWakeup);
				fT_PROGRAM_DATA.Rev5 = Convert.ToByte(value: true);
				fT_PROGRAM_DATA.PullDownEnable5 = Convert.ToByte(ee2232.PullDownEnable);
				fT_PROGRAM_DATA.SerNumEnable5 = Convert.ToByte(ee2232.SerNumEnable);
				fT_PROGRAM_DATA.USBVersionEnable5 = Convert.ToByte(ee2232.USBVersionEnable);
				fT_PROGRAM_DATA.USBVersion5 = ee2232.USBVersion;
				fT_PROGRAM_DATA.AIsHighCurrent = Convert.ToByte(ee2232.AIsHighCurrent);
				fT_PROGRAM_DATA.BIsHighCurrent = Convert.ToByte(ee2232.BIsHighCurrent);
				fT_PROGRAM_DATA.IFAIsFifo = Convert.ToByte(ee2232.IFAIsFifo);
				fT_PROGRAM_DATA.IFAIsFifoTar = Convert.ToByte(ee2232.IFAIsFifoTar);
				fT_PROGRAM_DATA.IFAIsFastSer = Convert.ToByte(ee2232.IFAIsFastSer);
				fT_PROGRAM_DATA.AIsVCP = Convert.ToByte(ee2232.AIsVCP);
				fT_PROGRAM_DATA.IFBIsFifo = Convert.ToByte(ee2232.IFBIsFifo);
				fT_PROGRAM_DATA.IFBIsFifoTar = Convert.ToByte(ee2232.IFBIsFifoTar);
				fT_PROGRAM_DATA.IFBIsFastSer = Convert.ToByte(ee2232.IFBIsFastSer);
				fT_PROGRAM_DATA.BIsVCP = Convert.ToByte(ee2232.BIsVCP);
				fT_STATUS = tFT_EE_Program2(ftHandle, fT_PROGRAM_DATA);
				Marshal.FreeHGlobal(fT_PROGRAM_DATA.Manufacturer);
				Marshal.FreeHGlobal(fT_PROGRAM_DATA.ManufacturerID);
				Marshal.FreeHGlobal(fT_PROGRAM_DATA.Description);
				Marshal.FreeHGlobal(fT_PROGRAM_DATA.SerialNumber);
			}
		}
		else if (pFT_EE_Program == IntPtr.Zero)
		{
			Console.WriteLine("Failed to load function FT_EE_Program.");
		}
		return fT_STATUS;
	}

	public FT_STATUS WriteFT232REEPROM(FT232R_EEPROM_STRUCTURE ee232r)
	{
		FT_STATUS fT_STATUS = FT_STATUS.FT_OTHER_ERROR;
		FT_ERROR fT_ERROR = FT_ERROR.FT_NO_ERROR;
		if (hFTD2XXDLL == IntPtr.Zero)
		{
			return fT_STATUS;
		}
		if (pFT_EE_Program != IntPtr.Zero)
		{
			tFT_EE_Program tFT_EE_Program2 = (tFT_EE_Program)Marshal.GetDelegateForFunctionPointer(pFT_EE_Program, typeof(tFT_EE_Program));
			if (ftHandle != IntPtr.Zero)
			{
				FT_DEVICE DeviceType = FT_DEVICE.FT_DEVICE_UNKNOWN;
				GetDeviceType(ref DeviceType);
				if (DeviceType != FT_DEVICE.FT_DEVICE_232R)
				{
					fT_ERROR = FT_ERROR.FT_INCORRECT_DEVICE;
					ErrorHandler(fT_STATUS, fT_ERROR);
				}
				if ((ee232r.VendorID == 0) | (ee232r.ProductID == 0))
				{
					return FT_STATUS.FT_INVALID_PARAMETER;
				}
				FT_PROGRAM_DATA fT_PROGRAM_DATA = new FT_PROGRAM_DATA();
				fT_PROGRAM_DATA.Signature1 = 0u;
				fT_PROGRAM_DATA.Signature2 = uint.MaxValue;
				fT_PROGRAM_DATA.Version = 2u;
				fT_PROGRAM_DATA.Manufacturer = Marshal.AllocHGlobal(32);
				fT_PROGRAM_DATA.ManufacturerID = Marshal.AllocHGlobal(16);
				fT_PROGRAM_DATA.Description = Marshal.AllocHGlobal(64);
				fT_PROGRAM_DATA.SerialNumber = Marshal.AllocHGlobal(16);
				if (ee232r.Manufacturer.Length > 32)
				{
					ee232r.Manufacturer = ee232r.Manufacturer.Substring(0, 32);
				}
				if (ee232r.ManufacturerID.Length > 16)
				{
					ee232r.ManufacturerID = ee232r.ManufacturerID.Substring(0, 16);
				}
				if (ee232r.Description.Length > 64)
				{
					ee232r.Description = ee232r.Description.Substring(0, 64);
				}
				if (ee232r.SerialNumber.Length > 16)
				{
					ee232r.SerialNumber = ee232r.SerialNumber.Substring(0, 16);
				}
				fT_PROGRAM_DATA.Manufacturer = Marshal.StringToHGlobalAnsi(ee232r.Manufacturer);
				fT_PROGRAM_DATA.ManufacturerID = Marshal.StringToHGlobalAnsi(ee232r.ManufacturerID);
				fT_PROGRAM_DATA.Description = Marshal.StringToHGlobalAnsi(ee232r.Description);
				fT_PROGRAM_DATA.SerialNumber = Marshal.StringToHGlobalAnsi(ee232r.SerialNumber);
				fT_PROGRAM_DATA.VendorID = ee232r.VendorID;
				fT_PROGRAM_DATA.ProductID = ee232r.ProductID;
				fT_PROGRAM_DATA.MaxPower = ee232r.MaxPower;
				fT_PROGRAM_DATA.SelfPowered = Convert.ToUInt16(ee232r.SelfPowered);
				fT_PROGRAM_DATA.RemoteWakeup = Convert.ToUInt16(ee232r.RemoteWakeup);
				fT_PROGRAM_DATA.PullDownEnableR = Convert.ToByte(ee232r.PullDownEnable);
				fT_PROGRAM_DATA.SerNumEnableR = Convert.ToByte(ee232r.SerNumEnable);
				fT_PROGRAM_DATA.UseExtOsc = Convert.ToByte(ee232r.UseExtOsc);
				fT_PROGRAM_DATA.HighDriveIOs = Convert.ToByte(ee232r.HighDriveIOs);
				fT_PROGRAM_DATA.EndpointSize = 64;
				fT_PROGRAM_DATA.PullDownEnableR = Convert.ToByte(ee232r.PullDownEnable);
				fT_PROGRAM_DATA.SerNumEnableR = Convert.ToByte(ee232r.SerNumEnable);
				fT_PROGRAM_DATA.InvertTXD = Convert.ToByte(ee232r.InvertTXD);
				fT_PROGRAM_DATA.InvertRXD = Convert.ToByte(ee232r.InvertRXD);
				fT_PROGRAM_DATA.InvertRTS = Convert.ToByte(ee232r.InvertRTS);
				fT_PROGRAM_DATA.InvertCTS = Convert.ToByte(ee232r.InvertCTS);
				fT_PROGRAM_DATA.InvertDTR = Convert.ToByte(ee232r.InvertDTR);
				fT_PROGRAM_DATA.InvertDSR = Convert.ToByte(ee232r.InvertDSR);
				fT_PROGRAM_DATA.InvertDCD = Convert.ToByte(ee232r.InvertDCD);
				fT_PROGRAM_DATA.InvertRI = Convert.ToByte(ee232r.InvertRI);
				fT_PROGRAM_DATA.Cbus0 = ee232r.Cbus0;
				fT_PROGRAM_DATA.Cbus1 = ee232r.Cbus1;
				fT_PROGRAM_DATA.Cbus2 = ee232r.Cbus2;
				fT_PROGRAM_DATA.Cbus3 = ee232r.Cbus3;
				fT_PROGRAM_DATA.Cbus4 = ee232r.Cbus4;
				fT_PROGRAM_DATA.RIsD2XX = Convert.ToByte(ee232r.RIsD2XX);
				fT_STATUS = tFT_EE_Program2(ftHandle, fT_PROGRAM_DATA);
				Marshal.FreeHGlobal(fT_PROGRAM_DATA.Manufacturer);
				Marshal.FreeHGlobal(fT_PROGRAM_DATA.ManufacturerID);
				Marshal.FreeHGlobal(fT_PROGRAM_DATA.Description);
				Marshal.FreeHGlobal(fT_PROGRAM_DATA.SerialNumber);
			}
		}
		else if (pFT_EE_Program == IntPtr.Zero)
		{
			Console.WriteLine("Failed to load function FT_EE_Program.");
		}
		return fT_STATUS;
	}

	public FT_STATUS WriteFT2232HEEPROM(FT2232H_EEPROM_STRUCTURE ee2232h)
	{
		FT_STATUS fT_STATUS = FT_STATUS.FT_OTHER_ERROR;
		FT_ERROR fT_ERROR = FT_ERROR.FT_NO_ERROR;
		if (hFTD2XXDLL == IntPtr.Zero)
		{
			return fT_STATUS;
		}
		if (pFT_EE_Program != IntPtr.Zero)
		{
			tFT_EE_Program tFT_EE_Program2 = (tFT_EE_Program)Marshal.GetDelegateForFunctionPointer(pFT_EE_Program, typeof(tFT_EE_Program));
			if (ftHandle != IntPtr.Zero)
			{
				FT_DEVICE DeviceType = FT_DEVICE.FT_DEVICE_UNKNOWN;
				GetDeviceType(ref DeviceType);
				if (DeviceType != FT_DEVICE.FT_DEVICE_2232H)
				{
					fT_ERROR = FT_ERROR.FT_INCORRECT_DEVICE;
					ErrorHandler(fT_STATUS, fT_ERROR);
				}
				if ((ee2232h.VendorID == 0) | (ee2232h.ProductID == 0))
				{
					return FT_STATUS.FT_INVALID_PARAMETER;
				}
				FT_PROGRAM_DATA fT_PROGRAM_DATA = new FT_PROGRAM_DATA();
				fT_PROGRAM_DATA.Signature1 = 0u;
				fT_PROGRAM_DATA.Signature2 = uint.MaxValue;
				fT_PROGRAM_DATA.Version = 3u;
				fT_PROGRAM_DATA.Manufacturer = Marshal.AllocHGlobal(32);
				fT_PROGRAM_DATA.ManufacturerID = Marshal.AllocHGlobal(16);
				fT_PROGRAM_DATA.Description = Marshal.AllocHGlobal(64);
				fT_PROGRAM_DATA.SerialNumber = Marshal.AllocHGlobal(16);
				if (ee2232h.Manufacturer.Length > 32)
				{
					ee2232h.Manufacturer = ee2232h.Manufacturer.Substring(0, 32);
				}
				if (ee2232h.ManufacturerID.Length > 16)
				{
					ee2232h.ManufacturerID = ee2232h.ManufacturerID.Substring(0, 16);
				}
				if (ee2232h.Description.Length > 64)
				{
					ee2232h.Description = ee2232h.Description.Substring(0, 64);
				}
				if (ee2232h.SerialNumber.Length > 16)
				{
					ee2232h.SerialNumber = ee2232h.SerialNumber.Substring(0, 16);
				}
				fT_PROGRAM_DATA.Manufacturer = Marshal.StringToHGlobalAnsi(ee2232h.Manufacturer);
				fT_PROGRAM_DATA.ManufacturerID = Marshal.StringToHGlobalAnsi(ee2232h.ManufacturerID);
				fT_PROGRAM_DATA.Description = Marshal.StringToHGlobalAnsi(ee2232h.Description);
				fT_PROGRAM_DATA.SerialNumber = Marshal.StringToHGlobalAnsi(ee2232h.SerialNumber);
				fT_PROGRAM_DATA.VendorID = ee2232h.VendorID;
				fT_PROGRAM_DATA.ProductID = ee2232h.ProductID;
				fT_PROGRAM_DATA.MaxPower = ee2232h.MaxPower;
				fT_PROGRAM_DATA.SelfPowered = Convert.ToUInt16(ee2232h.SelfPowered);
				fT_PROGRAM_DATA.RemoteWakeup = Convert.ToUInt16(ee2232h.RemoteWakeup);
				fT_PROGRAM_DATA.PullDownEnable7 = Convert.ToByte(ee2232h.PullDownEnable);
				fT_PROGRAM_DATA.SerNumEnable7 = Convert.ToByte(ee2232h.SerNumEnable);
				fT_PROGRAM_DATA.ALSlowSlew = Convert.ToByte(ee2232h.ALSlowSlew);
				fT_PROGRAM_DATA.ALSchmittInput = Convert.ToByte(ee2232h.ALSchmittInput);
				fT_PROGRAM_DATA.ALDriveCurrent = ee2232h.ALDriveCurrent;
				fT_PROGRAM_DATA.AHSlowSlew = Convert.ToByte(ee2232h.AHSlowSlew);
				fT_PROGRAM_DATA.AHSchmittInput = Convert.ToByte(ee2232h.AHSchmittInput);
				fT_PROGRAM_DATA.AHDriveCurrent = ee2232h.AHDriveCurrent;
				fT_PROGRAM_DATA.BLSlowSlew = Convert.ToByte(ee2232h.BLSlowSlew);
				fT_PROGRAM_DATA.BLSchmittInput = Convert.ToByte(ee2232h.BLSchmittInput);
				fT_PROGRAM_DATA.BLDriveCurrent = ee2232h.BLDriveCurrent;
				fT_PROGRAM_DATA.BHSlowSlew = Convert.ToByte(ee2232h.BHSlowSlew);
				fT_PROGRAM_DATA.BHSchmittInput = Convert.ToByte(ee2232h.BHSchmittInput);
				fT_PROGRAM_DATA.BHDriveCurrent = ee2232h.BHDriveCurrent;
				fT_PROGRAM_DATA.IFAIsFifo7 = Convert.ToByte(ee2232h.IFAIsFifo);
				fT_PROGRAM_DATA.IFAIsFifoTar7 = Convert.ToByte(ee2232h.IFAIsFifoTar);
				fT_PROGRAM_DATA.IFAIsFastSer7 = Convert.ToByte(ee2232h.IFAIsFastSer);
				fT_PROGRAM_DATA.AIsVCP7 = Convert.ToByte(ee2232h.AIsVCP);
				fT_PROGRAM_DATA.IFBIsFifo7 = Convert.ToByte(ee2232h.IFBIsFifo);
				fT_PROGRAM_DATA.IFBIsFifoTar7 = Convert.ToByte(ee2232h.IFBIsFifoTar);
				fT_PROGRAM_DATA.IFBIsFastSer7 = Convert.ToByte(ee2232h.IFBIsFastSer);
				fT_PROGRAM_DATA.BIsVCP7 = Convert.ToByte(ee2232h.BIsVCP);
				fT_PROGRAM_DATA.PowerSaveEnable = Convert.ToByte(ee2232h.PowerSaveEnable);
				fT_STATUS = tFT_EE_Program2(ftHandle, fT_PROGRAM_DATA);
				Marshal.FreeHGlobal(fT_PROGRAM_DATA.Manufacturer);
				Marshal.FreeHGlobal(fT_PROGRAM_DATA.ManufacturerID);
				Marshal.FreeHGlobal(fT_PROGRAM_DATA.Description);
				Marshal.FreeHGlobal(fT_PROGRAM_DATA.SerialNumber);
			}
		}
		else if (pFT_EE_Program == IntPtr.Zero)
		{
			Console.WriteLine("Failed to load function FT_EE_Program.");
		}
		return fT_STATUS;
	}

	public FT_STATUS WriteFT4232HEEPROM(FT4232H_EEPROM_STRUCTURE ee4232h)
	{
		FT_STATUS fT_STATUS = FT_STATUS.FT_OTHER_ERROR;
		FT_ERROR fT_ERROR = FT_ERROR.FT_NO_ERROR;
		if (hFTD2XXDLL == IntPtr.Zero)
		{
			return fT_STATUS;
		}
		if (pFT_EE_Program != IntPtr.Zero)
		{
			tFT_EE_Program tFT_EE_Program2 = (tFT_EE_Program)Marshal.GetDelegateForFunctionPointer(pFT_EE_Program, typeof(tFT_EE_Program));
			if (ftHandle != IntPtr.Zero)
			{
				FT_DEVICE DeviceType = FT_DEVICE.FT_DEVICE_UNKNOWN;
				GetDeviceType(ref DeviceType);
				if (DeviceType != FT_DEVICE.FT_DEVICE_4232H)
				{
					fT_ERROR = FT_ERROR.FT_INCORRECT_DEVICE;
					ErrorHandler(fT_STATUS, fT_ERROR);
				}
				if ((ee4232h.VendorID == 0) | (ee4232h.ProductID == 0))
				{
					return FT_STATUS.FT_INVALID_PARAMETER;
				}
				FT_PROGRAM_DATA fT_PROGRAM_DATA = new FT_PROGRAM_DATA();
				fT_PROGRAM_DATA.Signature1 = 0u;
				fT_PROGRAM_DATA.Signature2 = uint.MaxValue;
				fT_PROGRAM_DATA.Version = 4u;
				fT_PROGRAM_DATA.Manufacturer = Marshal.AllocHGlobal(32);
				fT_PROGRAM_DATA.ManufacturerID = Marshal.AllocHGlobal(16);
				fT_PROGRAM_DATA.Description = Marshal.AllocHGlobal(64);
				fT_PROGRAM_DATA.SerialNumber = Marshal.AllocHGlobal(16);
				if (ee4232h.Manufacturer.Length > 32)
				{
					ee4232h.Manufacturer = ee4232h.Manufacturer.Substring(0, 32);
				}
				if (ee4232h.ManufacturerID.Length > 16)
				{
					ee4232h.ManufacturerID = ee4232h.ManufacturerID.Substring(0, 16);
				}
				if (ee4232h.Description.Length > 64)
				{
					ee4232h.Description = ee4232h.Description.Substring(0, 64);
				}
				if (ee4232h.SerialNumber.Length > 16)
				{
					ee4232h.SerialNumber = ee4232h.SerialNumber.Substring(0, 16);
				}
				fT_PROGRAM_DATA.Manufacturer = Marshal.StringToHGlobalAnsi(ee4232h.Manufacturer);
				fT_PROGRAM_DATA.ManufacturerID = Marshal.StringToHGlobalAnsi(ee4232h.ManufacturerID);
				fT_PROGRAM_DATA.Description = Marshal.StringToHGlobalAnsi(ee4232h.Description);
				fT_PROGRAM_DATA.SerialNumber = Marshal.StringToHGlobalAnsi(ee4232h.SerialNumber);
				fT_PROGRAM_DATA.VendorID = ee4232h.VendorID;
				fT_PROGRAM_DATA.ProductID = ee4232h.ProductID;
				fT_PROGRAM_DATA.MaxPower = ee4232h.MaxPower;
				fT_PROGRAM_DATA.SelfPowered = Convert.ToUInt16(ee4232h.SelfPowered);
				fT_PROGRAM_DATA.RemoteWakeup = Convert.ToUInt16(ee4232h.RemoteWakeup);
				fT_PROGRAM_DATA.PullDownEnable8 = Convert.ToByte(ee4232h.PullDownEnable);
				fT_PROGRAM_DATA.SerNumEnable8 = Convert.ToByte(ee4232h.SerNumEnable);
				fT_PROGRAM_DATA.ASlowSlew = Convert.ToByte(ee4232h.ASlowSlew);
				fT_PROGRAM_DATA.ASchmittInput = Convert.ToByte(ee4232h.ASchmittInput);
				fT_PROGRAM_DATA.ADriveCurrent = ee4232h.ADriveCurrent;
				fT_PROGRAM_DATA.BSlowSlew = Convert.ToByte(ee4232h.BSlowSlew);
				fT_PROGRAM_DATA.BSchmittInput = Convert.ToByte(ee4232h.BSchmittInput);
				fT_PROGRAM_DATA.BDriveCurrent = ee4232h.BDriveCurrent;
				fT_PROGRAM_DATA.CSlowSlew = Convert.ToByte(ee4232h.CSlowSlew);
				fT_PROGRAM_DATA.CSchmittInput = Convert.ToByte(ee4232h.CSchmittInput);
				fT_PROGRAM_DATA.CDriveCurrent = ee4232h.CDriveCurrent;
				fT_PROGRAM_DATA.DSlowSlew = Convert.ToByte(ee4232h.DSlowSlew);
				fT_PROGRAM_DATA.DSchmittInput = Convert.ToByte(ee4232h.DSchmittInput);
				fT_PROGRAM_DATA.DDriveCurrent = ee4232h.DDriveCurrent;
				fT_PROGRAM_DATA.ARIIsTXDEN = Convert.ToByte(ee4232h.ARIIsTXDEN);
				fT_PROGRAM_DATA.BRIIsTXDEN = Convert.ToByte(ee4232h.BRIIsTXDEN);
				fT_PROGRAM_DATA.CRIIsTXDEN = Convert.ToByte(ee4232h.CRIIsTXDEN);
				fT_PROGRAM_DATA.DRIIsTXDEN = Convert.ToByte(ee4232h.DRIIsTXDEN);
				fT_PROGRAM_DATA.AIsVCP8 = Convert.ToByte(ee4232h.AIsVCP);
				fT_PROGRAM_DATA.BIsVCP8 = Convert.ToByte(ee4232h.BIsVCP);
				fT_PROGRAM_DATA.CIsVCP8 = Convert.ToByte(ee4232h.CIsVCP);
				fT_PROGRAM_DATA.DIsVCP8 = Convert.ToByte(ee4232h.DIsVCP);
				fT_STATUS = tFT_EE_Program2(ftHandle, fT_PROGRAM_DATA);
				Marshal.FreeHGlobal(fT_PROGRAM_DATA.Manufacturer);
				Marshal.FreeHGlobal(fT_PROGRAM_DATA.ManufacturerID);
				Marshal.FreeHGlobal(fT_PROGRAM_DATA.Description);
				Marshal.FreeHGlobal(fT_PROGRAM_DATA.SerialNumber);
			}
		}
		else if (pFT_EE_Program == IntPtr.Zero)
		{
			Console.WriteLine("Failed to load function FT_EE_Program.");
		}
		return fT_STATUS;
	}

	public FT_STATUS WriteFT232HEEPROM(FT232H_EEPROM_STRUCTURE ee232h)
	{
		FT_STATUS fT_STATUS = FT_STATUS.FT_OTHER_ERROR;
		FT_ERROR fT_ERROR = FT_ERROR.FT_NO_ERROR;
		if (hFTD2XXDLL == IntPtr.Zero)
		{
			return fT_STATUS;
		}
		if (pFT_EE_Program != IntPtr.Zero)
		{
			tFT_EE_Program tFT_EE_Program2 = (tFT_EE_Program)Marshal.GetDelegateForFunctionPointer(pFT_EE_Program, typeof(tFT_EE_Program));
			if (ftHandle != IntPtr.Zero)
			{
				FT_DEVICE DeviceType = FT_DEVICE.FT_DEVICE_UNKNOWN;
				GetDeviceType(ref DeviceType);
				if (DeviceType != FT_DEVICE.FT_DEVICE_232H)
				{
					fT_ERROR = FT_ERROR.FT_INCORRECT_DEVICE;
					ErrorHandler(fT_STATUS, fT_ERROR);
				}
				if ((ee232h.VendorID == 0) | (ee232h.ProductID == 0))
				{
					return FT_STATUS.FT_INVALID_PARAMETER;
				}
				FT_PROGRAM_DATA fT_PROGRAM_DATA = new FT_PROGRAM_DATA();
				fT_PROGRAM_DATA.Signature1 = 0u;
				fT_PROGRAM_DATA.Signature2 = uint.MaxValue;
				fT_PROGRAM_DATA.Version = 5u;
				fT_PROGRAM_DATA.Manufacturer = Marshal.AllocHGlobal(32);
				fT_PROGRAM_DATA.ManufacturerID = Marshal.AllocHGlobal(16);
				fT_PROGRAM_DATA.Description = Marshal.AllocHGlobal(64);
				fT_PROGRAM_DATA.SerialNumber = Marshal.AllocHGlobal(16);
				if (ee232h.Manufacturer.Length > 32)
				{
					ee232h.Manufacturer = ee232h.Manufacturer.Substring(0, 32);
				}
				if (ee232h.ManufacturerID.Length > 16)
				{
					ee232h.ManufacturerID = ee232h.ManufacturerID.Substring(0, 16);
				}
				if (ee232h.Description.Length > 64)
				{
					ee232h.Description = ee232h.Description.Substring(0, 64);
				}
				if (ee232h.SerialNumber.Length > 16)
				{
					ee232h.SerialNumber = ee232h.SerialNumber.Substring(0, 16);
				}
				fT_PROGRAM_DATA.Manufacturer = Marshal.StringToHGlobalAnsi(ee232h.Manufacturer);
				fT_PROGRAM_DATA.ManufacturerID = Marshal.StringToHGlobalAnsi(ee232h.ManufacturerID);
				fT_PROGRAM_DATA.Description = Marshal.StringToHGlobalAnsi(ee232h.Description);
				fT_PROGRAM_DATA.SerialNumber = Marshal.StringToHGlobalAnsi(ee232h.SerialNumber);
				fT_PROGRAM_DATA.VendorID = ee232h.VendorID;
				fT_PROGRAM_DATA.ProductID = ee232h.ProductID;
				fT_PROGRAM_DATA.MaxPower = ee232h.MaxPower;
				fT_PROGRAM_DATA.SelfPowered = Convert.ToUInt16(ee232h.SelfPowered);
				fT_PROGRAM_DATA.RemoteWakeup = Convert.ToUInt16(ee232h.RemoteWakeup);
				fT_PROGRAM_DATA.PullDownEnableH = Convert.ToByte(ee232h.PullDownEnable);
				fT_PROGRAM_DATA.SerNumEnableH = Convert.ToByte(ee232h.SerNumEnable);
				fT_PROGRAM_DATA.ACSlowSlewH = Convert.ToByte(ee232h.ACSlowSlew);
				fT_PROGRAM_DATA.ACSchmittInputH = Convert.ToByte(ee232h.ACSchmittInput);
				fT_PROGRAM_DATA.ACDriveCurrentH = Convert.ToByte(ee232h.ACDriveCurrent);
				fT_PROGRAM_DATA.ADSlowSlewH = Convert.ToByte(ee232h.ADSlowSlew);
				fT_PROGRAM_DATA.ADSchmittInputH = Convert.ToByte(ee232h.ADSchmittInput);
				fT_PROGRAM_DATA.ADDriveCurrentH = Convert.ToByte(ee232h.ADDriveCurrent);
				fT_PROGRAM_DATA.Cbus0H = Convert.ToByte(ee232h.Cbus0);
				fT_PROGRAM_DATA.Cbus1H = Convert.ToByte(ee232h.Cbus1);
				fT_PROGRAM_DATA.Cbus2H = Convert.ToByte(ee232h.Cbus2);
				fT_PROGRAM_DATA.Cbus3H = Convert.ToByte(ee232h.Cbus3);
				fT_PROGRAM_DATA.Cbus4H = Convert.ToByte(ee232h.Cbus4);
				fT_PROGRAM_DATA.Cbus5H = Convert.ToByte(ee232h.Cbus5);
				fT_PROGRAM_DATA.Cbus6H = Convert.ToByte(ee232h.Cbus6);
				fT_PROGRAM_DATA.Cbus7H = Convert.ToByte(ee232h.Cbus7);
				fT_PROGRAM_DATA.Cbus8H = Convert.ToByte(ee232h.Cbus8);
				fT_PROGRAM_DATA.Cbus9H = Convert.ToByte(ee232h.Cbus9);
				fT_PROGRAM_DATA.IsFifoH = Convert.ToByte(ee232h.IsFifo);
				fT_PROGRAM_DATA.IsFifoTarH = Convert.ToByte(ee232h.IsFifoTar);
				fT_PROGRAM_DATA.IsFastSerH = Convert.ToByte(ee232h.IsFastSer);
				fT_PROGRAM_DATA.IsFT1248H = Convert.ToByte(ee232h.IsFT1248);
				fT_PROGRAM_DATA.FT1248CpolH = Convert.ToByte(ee232h.FT1248Cpol);
				fT_PROGRAM_DATA.FT1248LsbH = Convert.ToByte(ee232h.FT1248Lsb);
				fT_PROGRAM_DATA.FT1248FlowControlH = Convert.ToByte(ee232h.FT1248FlowControl);
				fT_PROGRAM_DATA.IsVCPH = Convert.ToByte(ee232h.IsVCP);
				fT_PROGRAM_DATA.PowerSaveEnableH = Convert.ToByte(ee232h.PowerSaveEnable);
				fT_STATUS = tFT_EE_Program2(ftHandle, fT_PROGRAM_DATA);
				Marshal.FreeHGlobal(fT_PROGRAM_DATA.Manufacturer);
				Marshal.FreeHGlobal(fT_PROGRAM_DATA.ManufacturerID);
				Marshal.FreeHGlobal(fT_PROGRAM_DATA.Description);
				Marshal.FreeHGlobal(fT_PROGRAM_DATA.SerialNumber);
			}
		}
		else if (pFT_EE_Program == IntPtr.Zero)
		{
			Console.WriteLine("Failed to load function FT_EE_Program.");
		}
		return fT_STATUS;
	}

	public FT_STATUS WriteXSeriesEEPROM(FT_XSERIES_EEPROM_STRUCTURE eeX)
	{
		FT_STATUS fT_STATUS = FT_STATUS.FT_OTHER_ERROR;
		FT_ERROR fT_ERROR = FT_ERROR.FT_NO_ERROR;
		if (hFTD2XXDLL == IntPtr.Zero)
		{
			return fT_STATUS;
		}
		if (pFT_EEPROM_Program != IntPtr.Zero)
		{
			tFT_EEPROM_Program tFT_EEPROM_Program2 = (tFT_EEPROM_Program)Marshal.GetDelegateForFunctionPointer(pFT_EEPROM_Program, typeof(tFT_EEPROM_Program));
			if (ftHandle != IntPtr.Zero)
			{
				FT_DEVICE DeviceType = FT_DEVICE.FT_DEVICE_UNKNOWN;
				GetDeviceType(ref DeviceType);
				if (DeviceType != FT_DEVICE.FT_DEVICE_X_SERIES)
				{
					fT_ERROR = FT_ERROR.FT_INCORRECT_DEVICE;
					ErrorHandler(fT_STATUS, fT_ERROR);
				}
				if ((eeX.VendorID == 0) | (eeX.ProductID == 0))
				{
					return FT_STATUS.FT_INVALID_PARAMETER;
				}
				FT_XSERIES_DATA fT_XSERIES_DATA = default(FT_XSERIES_DATA);
				byte[] array = new byte[32];
				byte[] array2 = new byte[16];
				byte[] array3 = new byte[64];
				byte[] array4 = new byte[16];
				if (eeX.Manufacturer.Length > 32)
				{
					eeX.Manufacturer = eeX.Manufacturer.Substring(0, 32);
				}
				if (eeX.ManufacturerID.Length > 16)
				{
					eeX.ManufacturerID = eeX.ManufacturerID.Substring(0, 16);
				}
				if (eeX.Description.Length > 64)
				{
					eeX.Description = eeX.Description.Substring(0, 64);
				}
				if (eeX.SerialNumber.Length > 16)
				{
					eeX.SerialNumber = eeX.SerialNumber.Substring(0, 16);
				}
				UTF8Encoding uTF8Encoding = new UTF8Encoding();
				array = uTF8Encoding.GetBytes(eeX.Manufacturer);
				array2 = uTF8Encoding.GetBytes(eeX.ManufacturerID);
				array3 = uTF8Encoding.GetBytes(eeX.Description);
				array4 = uTF8Encoding.GetBytes(eeX.SerialNumber);
				fT_XSERIES_DATA.common.deviceType = 9u;
				fT_XSERIES_DATA.common.VendorId = eeX.VendorID;
				fT_XSERIES_DATA.common.ProductId = eeX.ProductID;
				fT_XSERIES_DATA.common.MaxPower = eeX.MaxPower;
				fT_XSERIES_DATA.common.SelfPowered = Convert.ToByte(eeX.SelfPowered);
				fT_XSERIES_DATA.common.RemoteWakeup = Convert.ToByte(eeX.RemoteWakeup);
				fT_XSERIES_DATA.common.SerNumEnable = Convert.ToByte(eeX.SerNumEnable);
				fT_XSERIES_DATA.common.PullDownEnable = Convert.ToByte(eeX.PullDownEnable);
				fT_XSERIES_DATA.Cbus0 = eeX.Cbus0;
				fT_XSERIES_DATA.Cbus1 = eeX.Cbus1;
				fT_XSERIES_DATA.Cbus2 = eeX.Cbus2;
				fT_XSERIES_DATA.Cbus3 = eeX.Cbus3;
				fT_XSERIES_DATA.Cbus4 = eeX.Cbus4;
				fT_XSERIES_DATA.Cbus5 = eeX.Cbus5;
				fT_XSERIES_DATA.Cbus6 = eeX.Cbus6;
				fT_XSERIES_DATA.ACDriveCurrent = eeX.ACDriveCurrent;
				fT_XSERIES_DATA.ACSchmittInput = eeX.ACSchmittInput;
				fT_XSERIES_DATA.ACSlowSlew = eeX.ACSlowSlew;
				fT_XSERIES_DATA.ADDriveCurrent = eeX.ADDriveCurrent;
				fT_XSERIES_DATA.ADSchmittInput = eeX.ADSchmittInput;
				fT_XSERIES_DATA.ADSlowSlew = eeX.ADSlowSlew;
				fT_XSERIES_DATA.BCDDisableSleep = eeX.BCDDisableSleep;
				fT_XSERIES_DATA.BCDEnable = eeX.BCDEnable;
				fT_XSERIES_DATA.BCDForceCbusPWREN = eeX.BCDForceCbusPWREN;
				fT_XSERIES_DATA.FT1248Cpol = eeX.FT1248Cpol;
				fT_XSERIES_DATA.FT1248FlowControl = eeX.FT1248FlowControl;
				fT_XSERIES_DATA.FT1248Lsb = eeX.FT1248Lsb;
				fT_XSERIES_DATA.I2CDeviceId = eeX.I2CDeviceId;
				fT_XSERIES_DATA.I2CDisableSchmitt = eeX.I2CDisableSchmitt;
				fT_XSERIES_DATA.I2CSlaveAddress = eeX.I2CSlaveAddress;
				fT_XSERIES_DATA.InvertCTS = eeX.InvertCTS;
				fT_XSERIES_DATA.InvertDCD = eeX.InvertDCD;
				fT_XSERIES_DATA.InvertDSR = eeX.InvertDSR;
				fT_XSERIES_DATA.InvertDTR = eeX.InvertDTR;
				fT_XSERIES_DATA.InvertRI = eeX.InvertRI;
				fT_XSERIES_DATA.InvertRTS = eeX.InvertRTS;
				fT_XSERIES_DATA.InvertRXD = eeX.InvertRXD;
				fT_XSERIES_DATA.InvertTXD = eeX.InvertTXD;
				fT_XSERIES_DATA.PowerSaveEnable = eeX.PowerSaveEnable;
				fT_XSERIES_DATA.RS485EchoSuppress = eeX.RS485EchoSuppress;
				fT_XSERIES_DATA.DriverType = eeX.IsVCP;
				int num = Marshal.SizeOf((object)fT_XSERIES_DATA);
				IntPtr intPtr = Marshal.AllocHGlobal(num);
				Marshal.StructureToPtr((object)fT_XSERIES_DATA, intPtr, false);
				fT_STATUS = tFT_EEPROM_Program2(ftHandle, intPtr, (uint)num, array, array2, array3, array4);
			}
		}
		return fT_STATUS;
	}

	public FT_STATUS EEReadUserArea(byte[] UserAreaDataBuffer, ref uint numBytesRead)
	{
		FT_STATUS result = FT_STATUS.FT_OTHER_ERROR;
		if (hFTD2XXDLL == IntPtr.Zero)
		{
			return result;
		}
		if ((pFT_EE_UASize != IntPtr.Zero) & (pFT_EE_UARead != IntPtr.Zero))
		{
			tFT_EE_UASize tFT_EE_UASize2 = (tFT_EE_UASize)Marshal.GetDelegateForFunctionPointer(pFT_EE_UASize, typeof(tFT_EE_UASize));
			tFT_EE_UARead tFT_EE_UARead2 = (tFT_EE_UARead)Marshal.GetDelegateForFunctionPointer(pFT_EE_UARead, typeof(tFT_EE_UARead));
			if (ftHandle != IntPtr.Zero)
			{
				uint dwSize = 0u;
				result = tFT_EE_UASize2(ftHandle, ref dwSize);
				if (UserAreaDataBuffer.Length >= dwSize)
				{
					result = tFT_EE_UARead2(ftHandle, UserAreaDataBuffer, UserAreaDataBuffer.Length, ref numBytesRead);
				}
			}
		}
		else
		{
			if (pFT_EE_UASize == IntPtr.Zero)
			{
				Console.WriteLine("Failed to load function FT_EE_UASize.");
			}
			if (pFT_EE_UARead == IntPtr.Zero)
			{
				Console.WriteLine("Failed to load function FT_EE_UARead.");
			}
		}
		return result;
	}

	public FT_STATUS EEWriteUserArea(byte[] UserAreaDataBuffer)
	{
		FT_STATUS result = FT_STATUS.FT_OTHER_ERROR;
		if (hFTD2XXDLL == IntPtr.Zero)
		{
			return result;
		}
		if ((pFT_EE_UASize != IntPtr.Zero) & (pFT_EE_UAWrite != IntPtr.Zero))
		{
			tFT_EE_UASize tFT_EE_UASize2 = (tFT_EE_UASize)Marshal.GetDelegateForFunctionPointer(pFT_EE_UASize, typeof(tFT_EE_UASize));
			tFT_EE_UAWrite tFT_EE_UAWrite2 = (tFT_EE_UAWrite)Marshal.GetDelegateForFunctionPointer(pFT_EE_UAWrite, typeof(tFT_EE_UAWrite));
			if (ftHandle != IntPtr.Zero)
			{
				uint dwSize = 0u;
				result = tFT_EE_UASize2(ftHandle, ref dwSize);
				if (UserAreaDataBuffer.Length <= dwSize)
				{
					result = tFT_EE_UAWrite2(ftHandle, UserAreaDataBuffer, UserAreaDataBuffer.Length);
				}
			}
		}
		else
		{
			if (pFT_EE_UASize == IntPtr.Zero)
			{
				Console.WriteLine("Failed to load function FT_EE_UASize.");
			}
			if (pFT_EE_UAWrite == IntPtr.Zero)
			{
				Console.WriteLine("Failed to load function FT_EE_UAWrite.");
			}
		}
		return result;
	}

	public FT_STATUS GetDeviceType(ref FT_DEVICE DeviceType)
	{
		FT_STATUS result = FT_STATUS.FT_OTHER_ERROR;
		if (hFTD2XXDLL == IntPtr.Zero)
		{
			return result;
		}
		if (pFT_GetDeviceInfo != IntPtr.Zero)
		{
			tFT_GetDeviceInfo tFT_GetDeviceInfo2 = (tFT_GetDeviceInfo)Marshal.GetDelegateForFunctionPointer(pFT_GetDeviceInfo, typeof(tFT_GetDeviceInfo));
			uint lpdwID = 0u;
			byte[] pcSerialNumber = new byte[16];
			byte[] pcDescription = new byte[64];
			DeviceType = FT_DEVICE.FT_DEVICE_UNKNOWN;
			if (ftHandle != IntPtr.Zero)
			{
				result = tFT_GetDeviceInfo2(ftHandle, ref DeviceType, ref lpdwID, pcSerialNumber, pcDescription, IntPtr.Zero);
			}
		}
		else if (pFT_GetDeviceInfo == IntPtr.Zero)
		{
			Console.WriteLine("Failed to load function FT_GetDeviceInfo.");
		}
		return result;
	}

	public FT_STATUS GetDeviceID(ref uint DeviceID)
	{
		FT_STATUS result = FT_STATUS.FT_OTHER_ERROR;
		if (hFTD2XXDLL == IntPtr.Zero)
		{
			return result;
		}
		if (pFT_GetDeviceInfo != IntPtr.Zero)
		{
			tFT_GetDeviceInfo tFT_GetDeviceInfo2 = (tFT_GetDeviceInfo)Marshal.GetDelegateForFunctionPointer(pFT_GetDeviceInfo, typeof(tFT_GetDeviceInfo));
			FT_DEVICE pftType = FT_DEVICE.FT_DEVICE_UNKNOWN;
			byte[] pcSerialNumber = new byte[16];
			byte[] pcDescription = new byte[64];
			if (ftHandle != IntPtr.Zero)
			{
				result = tFT_GetDeviceInfo2(ftHandle, ref pftType, ref DeviceID, pcSerialNumber, pcDescription, IntPtr.Zero);
			}
		}
		else if (pFT_GetDeviceInfo == IntPtr.Zero)
		{
			Console.WriteLine("Failed to load function FT_GetDeviceInfo.");
		}
		return result;
	}

	public FT_STATUS GetDescription(out string Description)
	{
		FT_STATUS result = FT_STATUS.FT_OTHER_ERROR;
		Description = string.Empty;
		if (hFTD2XXDLL == IntPtr.Zero)
		{
			return result;
		}
		if (pFT_GetDeviceInfo != IntPtr.Zero)
		{
			tFT_GetDeviceInfo tFT_GetDeviceInfo2 = (tFT_GetDeviceInfo)Marshal.GetDelegateForFunctionPointer(pFT_GetDeviceInfo, typeof(tFT_GetDeviceInfo));
			uint lpdwID = 0u;
			FT_DEVICE pftType = FT_DEVICE.FT_DEVICE_UNKNOWN;
			byte[] pcSerialNumber = new byte[16];
			byte[] array = new byte[64];
			if (ftHandle != IntPtr.Zero)
			{
				result = tFT_GetDeviceInfo2(ftHandle, ref pftType, ref lpdwID, pcSerialNumber, array, IntPtr.Zero);
				Description = Encoding.ASCII.GetString(array);
				Description = Description.Substring(0, Description.IndexOf('\0'));
			}
		}
		else if (pFT_GetDeviceInfo == IntPtr.Zero)
		{
			Console.WriteLine("Failed to load function FT_GetDeviceInfo.");
		}
		return result;
	}

	public FT_STATUS GetSerialNumber(out string SerialNumber)
	{
		FT_STATUS result = FT_STATUS.FT_OTHER_ERROR;
		SerialNumber = string.Empty;
		if (hFTD2XXDLL == IntPtr.Zero)
		{
			return result;
		}
		if (pFT_GetDeviceInfo != IntPtr.Zero)
		{
			tFT_GetDeviceInfo tFT_GetDeviceInfo2 = (tFT_GetDeviceInfo)Marshal.GetDelegateForFunctionPointer(pFT_GetDeviceInfo, typeof(tFT_GetDeviceInfo));
			uint lpdwID = 0u;
			FT_DEVICE pftType = FT_DEVICE.FT_DEVICE_UNKNOWN;
			byte[] array = new byte[16];
			byte[] pcDescription = new byte[64];
			if (ftHandle != IntPtr.Zero)
			{
				result = tFT_GetDeviceInfo2(ftHandle, ref pftType, ref lpdwID, array, pcDescription, IntPtr.Zero);
				SerialNumber = Encoding.ASCII.GetString(array);
				SerialNumber = SerialNumber.Substring(0, SerialNumber.IndexOf('\0'));
			}
		}
		else if (pFT_GetDeviceInfo == IntPtr.Zero)
		{
			Console.WriteLine("Failed to load function FT_GetDeviceInfo.");
		}
		return result;
	}

	public FT_STATUS GetRxBytesAvailable(ref uint RxQueue)
	{
		FT_STATUS result = FT_STATUS.FT_OTHER_ERROR;
		if (hFTD2XXDLL == IntPtr.Zero)
		{
			return result;
		}
		if (pFT_GetQueueStatus != IntPtr.Zero)
		{
			tFT_GetQueueStatus tFT_GetQueueStatus2 = (tFT_GetQueueStatus)Marshal.GetDelegateForFunctionPointer(pFT_GetQueueStatus, typeof(tFT_GetQueueStatus));
			if (ftHandle != IntPtr.Zero)
			{
				result = tFT_GetQueueStatus2(ftHandle, ref RxQueue);
			}
		}
		else if (pFT_GetQueueStatus == IntPtr.Zero)
		{
			Console.WriteLine("Failed to load function FT_GetQueueStatus.");
		}
		return result;
	}

	public FT_STATUS GetTxBytesWaiting(ref uint TxQueue)
	{
		FT_STATUS result = FT_STATUS.FT_OTHER_ERROR;
		if (hFTD2XXDLL == IntPtr.Zero)
		{
			return result;
		}
		if (pFT_GetStatus != IntPtr.Zero)
		{
			tFT_GetStatus tFT_GetStatus2 = (tFT_GetStatus)Marshal.GetDelegateForFunctionPointer(pFT_GetStatus, typeof(tFT_GetStatus));
			uint lpdwAmountInRxQueue = 0u;
			uint lpdwEventStatus = 0u;
			if (ftHandle != IntPtr.Zero)
			{
				result = tFT_GetStatus2(ftHandle, ref lpdwAmountInRxQueue, ref TxQueue, ref lpdwEventStatus);
			}
		}
		else if (pFT_GetStatus == IntPtr.Zero)
		{
			Console.WriteLine("Failed to load function FT_GetStatus.");
		}
		return result;
	}

	public FT_STATUS GetEventType(ref uint EventType)
	{
		FT_STATUS result = FT_STATUS.FT_OTHER_ERROR;
		if (hFTD2XXDLL == IntPtr.Zero)
		{
			return result;
		}
		if (pFT_GetStatus != IntPtr.Zero)
		{
			tFT_GetStatus tFT_GetStatus2 = (tFT_GetStatus)Marshal.GetDelegateForFunctionPointer(pFT_GetStatus, typeof(tFT_GetStatus));
			uint lpdwAmountInRxQueue = 0u;
			uint lpdwAmountInTxQueue = 0u;
			if (ftHandle != IntPtr.Zero)
			{
				result = tFT_GetStatus2(ftHandle, ref lpdwAmountInRxQueue, ref lpdwAmountInTxQueue, ref EventType);
			}
		}
		else if (pFT_GetStatus == IntPtr.Zero)
		{
			Console.WriteLine("Failed to load function FT_GetStatus.");
		}
		return result;
	}

	public FT_STATUS GetModemStatus(ref byte ModemStatus)
	{
		FT_STATUS result = FT_STATUS.FT_OTHER_ERROR;
		if (hFTD2XXDLL == IntPtr.Zero)
		{
			return result;
		}
		if (pFT_GetModemStatus != IntPtr.Zero)
		{
			tFT_GetModemStatus tFT_GetModemStatus2 = (tFT_GetModemStatus)Marshal.GetDelegateForFunctionPointer(pFT_GetModemStatus, typeof(tFT_GetModemStatus));
			uint lpdwModemStatus = 0u;
			if (ftHandle != IntPtr.Zero)
			{
				result = tFT_GetModemStatus2(ftHandle, ref lpdwModemStatus);
			}
			ModemStatus = Convert.ToByte(lpdwModemStatus & 0xFF);
		}
		else if (pFT_GetModemStatus == IntPtr.Zero)
		{
			Console.WriteLine("Failed to load function FT_GetModemStatus.");
		}
		return result;
	}

	public FT_STATUS GetLineStatus(ref byte LineStatus)
	{
		FT_STATUS result = FT_STATUS.FT_OTHER_ERROR;
		if (hFTD2XXDLL == IntPtr.Zero)
		{
			return result;
		}
		if (pFT_GetModemStatus != IntPtr.Zero)
		{
			tFT_GetModemStatus tFT_GetModemStatus2 = (tFT_GetModemStatus)Marshal.GetDelegateForFunctionPointer(pFT_GetModemStatus, typeof(tFT_GetModemStatus));
			uint lpdwModemStatus = 0u;
			if (ftHandle != IntPtr.Zero)
			{
				result = tFT_GetModemStatus2(ftHandle, ref lpdwModemStatus);
			}
			LineStatus = Convert.ToByte((lpdwModemStatus >> 8) & 0xFF);
		}
		else if (pFT_GetModemStatus == IntPtr.Zero)
		{
			Console.WriteLine("Failed to load function FT_GetModemStatus.");
		}
		return result;
	}

	public FT_STATUS SetBaudRate(uint BaudRate)
	{
		FT_STATUS result = FT_STATUS.FT_OTHER_ERROR;
		if (hFTD2XXDLL == IntPtr.Zero)
		{
			return result;
		}
		if (pFT_SetBaudRate != IntPtr.Zero)
		{
			tFT_SetBaudRate tFT_SetBaudRate2 = (tFT_SetBaudRate)Marshal.GetDelegateForFunctionPointer(pFT_SetBaudRate, typeof(tFT_SetBaudRate));
			if (ftHandle != IntPtr.Zero)
			{
				result = tFT_SetBaudRate2(ftHandle, BaudRate);
			}
		}
		else if (pFT_SetBaudRate == IntPtr.Zero)
		{
			Console.WriteLine("Failed to load function FT_SetBaudRate.");
		}
		return result;
	}

	public FT_STATUS SetDataCharacteristics(byte DataBits, byte StopBits, byte Parity)
	{
		FT_STATUS result = FT_STATUS.FT_OTHER_ERROR;
		if (hFTD2XXDLL == IntPtr.Zero)
		{
			return result;
		}
		if (pFT_SetDataCharacteristics != IntPtr.Zero)
		{
			tFT_SetDataCharacteristics tFT_SetDataCharacteristics2 = (tFT_SetDataCharacteristics)Marshal.GetDelegateForFunctionPointer(pFT_SetDataCharacteristics, typeof(tFT_SetDataCharacteristics));
			if (ftHandle != IntPtr.Zero)
			{
				result = tFT_SetDataCharacteristics2(ftHandle, DataBits, StopBits, Parity);
			}
		}
		else if (pFT_SetDataCharacteristics == IntPtr.Zero)
		{
			Console.WriteLine("Failed to load function FT_SetDataCharacteristics.");
		}
		return result;
	}

	public FT_STATUS SetFlowControl(ushort FlowControl, byte Xon, byte Xoff)
	{
		FT_STATUS result = FT_STATUS.FT_OTHER_ERROR;
		if (hFTD2XXDLL == IntPtr.Zero)
		{
			return result;
		}
		if (pFT_SetFlowControl != IntPtr.Zero)
		{
			tFT_SetFlowControl tFT_SetFlowControl2 = (tFT_SetFlowControl)Marshal.GetDelegateForFunctionPointer(pFT_SetFlowControl, typeof(tFT_SetFlowControl));
			if (ftHandle != IntPtr.Zero)
			{
				result = tFT_SetFlowControl2(ftHandle, FlowControl, Xon, Xoff);
			}
		}
		else if (pFT_SetFlowControl == IntPtr.Zero)
		{
			Console.WriteLine("Failed to load function FT_SetFlowControl.");
		}
		return result;
	}

	public FT_STATUS SetRTS(bool Enable)
	{
		FT_STATUS result = FT_STATUS.FT_OTHER_ERROR;
		if (hFTD2XXDLL == IntPtr.Zero)
		{
			return result;
		}
		if ((pFT_SetRts != IntPtr.Zero) & (pFT_ClrRts != IntPtr.Zero))
		{
			tFT_SetRts tFT_SetRts2 = (tFT_SetRts)Marshal.GetDelegateForFunctionPointer(pFT_SetRts, typeof(tFT_SetRts));
			tFT_ClrRts tFT_ClrRts2 = (tFT_ClrRts)Marshal.GetDelegateForFunctionPointer(pFT_ClrRts, typeof(tFT_ClrRts));
			if (ftHandle != IntPtr.Zero)
			{
				result = ((!Enable) ? tFT_ClrRts2(ftHandle) : tFT_SetRts2(ftHandle));
			}
		}
		else
		{
			if (pFT_SetRts == IntPtr.Zero)
			{
				Console.WriteLine("Failed to load function FT_SetRts.");
			}
			if (pFT_ClrRts == IntPtr.Zero)
			{
				Console.WriteLine("Failed to load function FT_ClrRts.");
			}
		}
		return result;
	}

	public FT_STATUS SetDTR(bool Enable)
	{
		FT_STATUS result = FT_STATUS.FT_OTHER_ERROR;
		if (hFTD2XXDLL == IntPtr.Zero)
		{
			return result;
		}
		if ((pFT_SetDtr != IntPtr.Zero) & (pFT_ClrDtr != IntPtr.Zero))
		{
			tFT_SetDtr tFT_SetDtr2 = (tFT_SetDtr)Marshal.GetDelegateForFunctionPointer(pFT_SetDtr, typeof(tFT_SetDtr));
			tFT_ClrDtr tFT_ClrDtr2 = (tFT_ClrDtr)Marshal.GetDelegateForFunctionPointer(pFT_ClrDtr, typeof(tFT_ClrDtr));
			if (ftHandle != IntPtr.Zero)
			{
				result = ((!Enable) ? tFT_ClrDtr2(ftHandle) : tFT_SetDtr2(ftHandle));
			}
		}
		else
		{
			if (pFT_SetDtr == IntPtr.Zero)
			{
				Console.WriteLine("Failed to load function FT_SetDtr.");
			}
			if (pFT_ClrDtr == IntPtr.Zero)
			{
				Console.WriteLine("Failed to load function FT_ClrDtr.");
			}
		}
		return result;
	}

	public FT_STATUS SetTimeouts(uint ReadTimeout, uint WriteTimeout)
	{
		FT_STATUS result = FT_STATUS.FT_OTHER_ERROR;
		if (hFTD2XXDLL == IntPtr.Zero)
		{
			return result;
		}
		if (pFT_SetTimeouts != IntPtr.Zero)
		{
			tFT_SetTimeouts tFT_SetTimeouts2 = (tFT_SetTimeouts)Marshal.GetDelegateForFunctionPointer(pFT_SetTimeouts, typeof(tFT_SetTimeouts));
			if (ftHandle != IntPtr.Zero)
			{
				result = tFT_SetTimeouts2(ftHandle, ReadTimeout, WriteTimeout);
			}
		}
		else if (pFT_SetTimeouts == IntPtr.Zero)
		{
			Console.WriteLine("Failed to load function FT_SetTimeouts.");
		}
		return result;
	}

	public FT_STATUS SetBreak(bool Enable)
	{
		FT_STATUS result = FT_STATUS.FT_OTHER_ERROR;
		if (hFTD2XXDLL == IntPtr.Zero)
		{
			return result;
		}
		if ((pFT_SetBreakOn != IntPtr.Zero) & (pFT_SetBreakOff != IntPtr.Zero))
		{
			tFT_SetBreakOn tFT_SetBreakOn2 = (tFT_SetBreakOn)Marshal.GetDelegateForFunctionPointer(pFT_SetBreakOn, typeof(tFT_SetBreakOn));
			tFT_SetBreakOff tFT_SetBreakOff2 = (tFT_SetBreakOff)Marshal.GetDelegateForFunctionPointer(pFT_SetBreakOff, typeof(tFT_SetBreakOff));
			if (ftHandle != IntPtr.Zero)
			{
				result = ((!Enable) ? tFT_SetBreakOff2(ftHandle) : tFT_SetBreakOn2(ftHandle));
			}
		}
		else
		{
			if (pFT_SetBreakOn == IntPtr.Zero)
			{
				Console.WriteLine("Failed to load function FT_SetBreakOn.");
			}
			if (pFT_SetBreakOff == IntPtr.Zero)
			{
				Console.WriteLine("Failed to load function FT_SetBreakOff.");
			}
		}
		return result;
	}

	public FT_STATUS SetResetPipeRetryCount(uint ResetPipeRetryCount)
	{
		FT_STATUS result = FT_STATUS.FT_OTHER_ERROR;
		if (hFTD2XXDLL == IntPtr.Zero)
		{
			return result;
		}
		if (pFT_SetResetPipeRetryCount != IntPtr.Zero)
		{
			tFT_SetResetPipeRetryCount tFT_SetResetPipeRetryCount2 = (tFT_SetResetPipeRetryCount)Marshal.GetDelegateForFunctionPointer(pFT_SetResetPipeRetryCount, typeof(tFT_SetResetPipeRetryCount));
			if (ftHandle != IntPtr.Zero)
			{
				result = tFT_SetResetPipeRetryCount2(ftHandle, ResetPipeRetryCount);
			}
		}
		else if (pFT_SetResetPipeRetryCount == IntPtr.Zero)
		{
			Console.WriteLine("Failed to load function FT_SetResetPipeRetryCount.");
		}
		return result;
	}

	public FT_STATUS GetDriverVersion(ref uint DriverVersion)
	{
		FT_STATUS result = FT_STATUS.FT_OTHER_ERROR;
		if (hFTD2XXDLL == IntPtr.Zero)
		{
			return result;
		}
		if (pFT_GetDriverVersion != IntPtr.Zero)
		{
			tFT_GetDriverVersion tFT_GetDriverVersion2 = (tFT_GetDriverVersion)Marshal.GetDelegateForFunctionPointer(pFT_GetDriverVersion, typeof(tFT_GetDriverVersion));
			if (ftHandle != IntPtr.Zero)
			{
				result = tFT_GetDriverVersion2(ftHandle, ref DriverVersion);
			}
		}
		else if (pFT_GetDriverVersion == IntPtr.Zero)
		{
			Console.WriteLine("Failed to load function FT_GetDriverVersion.");
		}
		return result;
	}

	public FT_STATUS GetLibraryVersion(ref uint LibraryVersion)
	{
		FT_STATUS result = FT_STATUS.FT_OTHER_ERROR;
		if (hFTD2XXDLL == IntPtr.Zero)
		{
			return result;
		}
		if (pFT_GetLibraryVersion != IntPtr.Zero)
		{
			result = ((tFT_GetLibraryVersion)Marshal.GetDelegateForFunctionPointer(pFT_GetLibraryVersion, typeof(tFT_GetLibraryVersion)))(ref LibraryVersion);
		}
		else if (pFT_GetLibraryVersion == IntPtr.Zero)
		{
			Console.WriteLine("Failed to load function FT_GetLibraryVersion.");
		}
		return result;
	}

	public FT_STATUS SetDeadmanTimeout(uint DeadmanTimeout)
	{
		FT_STATUS result = FT_STATUS.FT_OTHER_ERROR;
		if (hFTD2XXDLL == IntPtr.Zero)
		{
			return result;
		}
		if (pFT_SetDeadmanTimeout != IntPtr.Zero)
		{
			tFT_SetDeadmanTimeout tFT_SetDeadmanTimeout2 = (tFT_SetDeadmanTimeout)Marshal.GetDelegateForFunctionPointer(pFT_SetDeadmanTimeout, typeof(tFT_SetDeadmanTimeout));
			if (ftHandle != IntPtr.Zero)
			{
				result = tFT_SetDeadmanTimeout2(ftHandle, DeadmanTimeout);
			}
		}
		else if (pFT_SetDeadmanTimeout == IntPtr.Zero)
		{
			Console.WriteLine("Failed to load function FT_SetDeadmanTimeout.");
		}
		return result;
	}

	public FT_STATUS SetLatency(byte Latency)
	{
		FT_STATUS result = FT_STATUS.FT_OTHER_ERROR;
		if (hFTD2XXDLL == IntPtr.Zero)
		{
			return result;
		}
		if (pFT_SetLatencyTimer != IntPtr.Zero)
		{
			tFT_SetLatencyTimer tFT_SetLatencyTimer2 = (tFT_SetLatencyTimer)Marshal.GetDelegateForFunctionPointer(pFT_SetLatencyTimer, typeof(tFT_SetLatencyTimer));
			if (ftHandle != IntPtr.Zero)
			{
				FT_DEVICE DeviceType = FT_DEVICE.FT_DEVICE_UNKNOWN;
				GetDeviceType(ref DeviceType);
				if ((DeviceType == FT_DEVICE.FT_DEVICE_BM || DeviceType == FT_DEVICE.FT_DEVICE_2232) && Latency < 2)
				{
					Latency = 2;
				}
				result = tFT_SetLatencyTimer2(ftHandle, Latency);
			}
		}
		else if (pFT_SetLatencyTimer == IntPtr.Zero)
		{
			Console.WriteLine("Failed to load function FT_SetLatencyTimer.");
		}
		return result;
	}

	public FT_STATUS GetLatency(ref byte Latency)
	{
		FT_STATUS result = FT_STATUS.FT_OTHER_ERROR;
		if (hFTD2XXDLL == IntPtr.Zero)
		{
			return result;
		}
		if (pFT_GetLatencyTimer != IntPtr.Zero)
		{
			tFT_GetLatencyTimer tFT_GetLatencyTimer2 = (tFT_GetLatencyTimer)Marshal.GetDelegateForFunctionPointer(pFT_GetLatencyTimer, typeof(tFT_GetLatencyTimer));
			if (ftHandle != IntPtr.Zero)
			{
				result = tFT_GetLatencyTimer2(ftHandle, ref Latency);
			}
		}
		else if (pFT_GetLatencyTimer == IntPtr.Zero)
		{
			Console.WriteLine("Failed to load function FT_GetLatencyTimer.");
		}
		return result;
	}

	public FT_STATUS InTransferSize(uint InTransferSize)
	{
		FT_STATUS result = FT_STATUS.FT_OTHER_ERROR;
		if (hFTD2XXDLL == IntPtr.Zero)
		{
			return result;
		}
		if (pFT_SetUSBParameters != IntPtr.Zero)
		{
			tFT_SetUSBParameters tFT_SetUSBParameters2 = (tFT_SetUSBParameters)Marshal.GetDelegateForFunctionPointer(pFT_SetUSBParameters, typeof(tFT_SetUSBParameters));
			if (ftHandle != IntPtr.Zero)
			{
				result = tFT_SetUSBParameters2(ftHandle, InTransferSize, InTransferSize);
			}
		}
		else if (pFT_SetUSBParameters == IntPtr.Zero)
		{
			Console.WriteLine("Failed to load function FT_SetUSBParameters.");
		}
		return result;
	}

	public FT_STATUS SetCharacters(byte EventChar, bool EventCharEnable, byte ErrorChar, bool ErrorCharEnable)
	{
		FT_STATUS result = FT_STATUS.FT_OTHER_ERROR;
		if (hFTD2XXDLL == IntPtr.Zero)
		{
			return result;
		}
		if (pFT_SetChars != IntPtr.Zero)
		{
			tFT_SetChars tFT_SetChars2 = (tFT_SetChars)Marshal.GetDelegateForFunctionPointer(pFT_SetChars, typeof(tFT_SetChars));
			if (ftHandle != IntPtr.Zero)
			{
				result = tFT_SetChars2(ftHandle, EventChar, Convert.ToByte(EventCharEnable), ErrorChar, Convert.ToByte(ErrorCharEnable));
			}
		}
		else if (pFT_SetChars == IntPtr.Zero)
		{
			Console.WriteLine("Failed to load function FT_SetChars.");
		}
		return result;
	}

	public FT_STATUS EEUserAreaSize(ref uint UASize)
	{
		FT_STATUS result = FT_STATUS.FT_OTHER_ERROR;
		if (hFTD2XXDLL == IntPtr.Zero)
		{
			return result;
		}
		if (pFT_EE_UASize != IntPtr.Zero)
		{
			tFT_EE_UASize tFT_EE_UASize2 = (tFT_EE_UASize)Marshal.GetDelegateForFunctionPointer(pFT_EE_UASize, typeof(tFT_EE_UASize));
			if (ftHandle != IntPtr.Zero)
			{
				result = tFT_EE_UASize2(ftHandle, ref UASize);
			}
		}
		else if (pFT_EE_UASize == IntPtr.Zero)
		{
			Console.WriteLine("Failed to load function FT_EE_UASize.");
		}
		return result;
	}

	public FT_STATUS GetCOMPort(out string ComPortName)
	{
		FT_STATUS result = FT_STATUS.FT_OTHER_ERROR;
		ComPortName = string.Empty;
		if (hFTD2XXDLL == IntPtr.Zero)
		{
			return result;
		}
		if (pFT_GetComPortNumber != IntPtr.Zero)
		{
			tFT_GetComPortNumber tFT_GetComPortNumber2 = (tFT_GetComPortNumber)Marshal.GetDelegateForFunctionPointer(pFT_GetComPortNumber, typeof(tFT_GetComPortNumber));
			int dwComPortNumber = -1;
			if (ftHandle != IntPtr.Zero)
			{
				result = tFT_GetComPortNumber2(ftHandle, ref dwComPortNumber);
			}
			if (dwComPortNumber == -1)
			{
				ComPortName = string.Empty;
			}
			else
			{
				ComPortName = "COM" + dwComPortNumber;
			}
		}
		else if (pFT_GetComPortNumber == IntPtr.Zero)
		{
			Console.WriteLine("Failed to load function FT_GetComPortNumber.");
		}
		return result;
	}

	public FT_STATUS VendorCmdGet(ushort request, byte[] buf, ushort len)
	{
		FT_STATUS result = FT_STATUS.FT_OTHER_ERROR;
		if (hFTD2XXDLL == IntPtr.Zero)
		{
			return result;
		}
		if (pFT_VendorCmdGet != IntPtr.Zero)
		{
			tFT_VendorCmdGet tFT_VendorCmdGet2 = (tFT_VendorCmdGet)Marshal.GetDelegateForFunctionPointer(pFT_VendorCmdGet, typeof(tFT_VendorCmdGet));
			if (ftHandle != IntPtr.Zero)
			{
				result = tFT_VendorCmdGet2(ftHandle, request, buf, len);
			}
		}
		else if (pFT_VendorCmdGet == IntPtr.Zero)
		{
			Console.WriteLine("Failed to load function FT_VendorCmdGet.");
		}
		return result;
	}

	public FT_STATUS VendorCmdSet(ushort request, byte[] buf, ushort len)
	{
		FT_STATUS result = FT_STATUS.FT_OTHER_ERROR;
		if (hFTD2XXDLL == IntPtr.Zero)
		{
			return result;
		}
		if (pFT_VendorCmdSet != IntPtr.Zero)
		{
			tFT_VendorCmdSet tFT_VendorCmdSet2 = (tFT_VendorCmdSet)Marshal.GetDelegateForFunctionPointer(pFT_VendorCmdSet, typeof(tFT_VendorCmdSet));
			if (ftHandle != IntPtr.Zero)
			{
				result = tFT_VendorCmdSet2(ftHandle, request, buf, len);
			}
		}
		else if (pFT_VendorCmdSet == IntPtr.Zero)
		{
			Console.WriteLine("Failed to load function FT_VendorCmdSet.");
		}
		return result;
	}

	private void ErrorHandler(FT_STATUS ftStatus, FT_ERROR ftErrorCondition)
	{
		switch (ftStatus)
		{
		case FT_STATUS.FT_DEVICE_NOT_FOUND:
			throw new FT_EXCEPTION("FTDI device not found.");
		case FT_STATUS.FT_DEVICE_NOT_OPENED:
			throw new FT_EXCEPTION("FTDI device not opened.");
		case FT_STATUS.FT_DEVICE_NOT_OPENED_FOR_ERASE:
			throw new FT_EXCEPTION("FTDI device not opened for erase.");
		case FT_STATUS.FT_DEVICE_NOT_OPENED_FOR_WRITE:
			throw new FT_EXCEPTION("FTDI device not opened for write.");
		case FT_STATUS.FT_EEPROM_ERASE_FAILED:
			throw new FT_EXCEPTION("Failed to erase FTDI device EEPROM.");
		case FT_STATUS.FT_EEPROM_NOT_PRESENT:
			throw new FT_EXCEPTION("No EEPROM fitted to FTDI device.");
		case FT_STATUS.FT_EEPROM_NOT_PROGRAMMED:
			throw new FT_EXCEPTION("FTDI device EEPROM not programmed.");
		case FT_STATUS.FT_EEPROM_READ_FAILED:
			throw new FT_EXCEPTION("Failed to read FTDI device EEPROM.");
		case FT_STATUS.FT_EEPROM_WRITE_FAILED:
			throw new FT_EXCEPTION("Failed to write FTDI device EEPROM.");
		case FT_STATUS.FT_FAILED_TO_WRITE_DEVICE:
			throw new FT_EXCEPTION("Failed to write to FTDI device.");
		case FT_STATUS.FT_INSUFFICIENT_RESOURCES:
			throw new FT_EXCEPTION("Insufficient resources.");
		case FT_STATUS.FT_INVALID_ARGS:
			throw new FT_EXCEPTION("Invalid arguments for FTD2XX function call.");
		case FT_STATUS.FT_INVALID_BAUD_RATE:
			throw new FT_EXCEPTION("Invalid Baud rate for FTDI device.");
		case FT_STATUS.FT_INVALID_HANDLE:
			throw new FT_EXCEPTION("Invalid handle for FTDI device.");
		case FT_STATUS.FT_INVALID_PARAMETER:
			throw new FT_EXCEPTION("Invalid parameter for FTD2XX function call.");
		case FT_STATUS.FT_IO_ERROR:
			throw new FT_EXCEPTION("FTDI device IO error.");
		case FT_STATUS.FT_OTHER_ERROR:
			throw new FT_EXCEPTION("An unexpected error has occurred when trying to communicate with the FTDI device.");
		}
		switch (ftErrorCondition)
		{
		case FT_ERROR.FT_INCORRECT_DEVICE:
			throw new FT_EXCEPTION("The current device type does not match the EEPROM structure.");
		case FT_ERROR.FT_INVALID_BITMODE:
			throw new FT_EXCEPTION("The requested bit mode is not valid for the current device.");
		case FT_ERROR.FT_BUFFER_SIZE:
			throw new FT_EXCEPTION("The supplied buffer is not big enough.");
		}
	}
}
