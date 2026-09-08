using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using WindowsFirewallHelper.InternalHelpers;

namespace WindowsFirewallHelper.COMInterop;

[ComImport]
[Guid("F7898AF5-CAC4-4632-A2EC-DA06E5111AF2")]
[ComClassProgId("HNetCfg.FwMgr")]
internal interface INetFwMgr
{
	[DispId(1)]
	INetFwPolicy LocalPolicy
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1)]
		[return: MarshalAs(UnmanagedType.Interface)]
		get;
	}

	[DispId(2)]
	NetFwProfileType CurrentProfileType
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(2)]
		get;
	}

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(3)]
	void RestoreDefaults();

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(4)]
	void IsPortAllowed([In][MarshalAs(UnmanagedType.BStr)] string imageFileName, [In] NetFwIPVersion ipVersion, [In] int portNumber, [In][MarshalAs(UnmanagedType.BStr)] string localAddress, [In] NetFwIPProtocol ipProtocol, [MarshalAs(UnmanagedType.Struct)] out object allowed, [MarshalAs(UnmanagedType.Struct)] out object restricted);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(5)]
	void IsIcmpTypeAllowed([In] NetFwIPVersion ipVersion, [In][MarshalAs(UnmanagedType.BStr)] string localAddress, [In] byte type, [MarshalAs(UnmanagedType.Struct)] out object allowed, [MarshalAs(UnmanagedType.Struct)] out object restricted);
}
