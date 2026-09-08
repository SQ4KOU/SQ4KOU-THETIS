using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using WindowsFirewallHelper.InternalHelpers;

namespace WindowsFirewallHelper.COMInterop;

[ComImport]
[Guid("39EB36E0-2097-40BD-8AF2-63A13B525362")]
[ComClassProgId("HNetCfg.FwProducts")]
internal interface INetFwProducts : IEnumerable
{
	[DispId(1)]
	int Count
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1)]
		get;
	}

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(2)]
	[return: MarshalAs(UnmanagedType.IUnknown)]
	object Register([In][MarshalAs(UnmanagedType.Interface)] INetFwProduct product);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(3)]
	[return: MarshalAs(UnmanagedType.Interface)]
	INetFwProduct Item([In] int index);

	[DispId(-4)]
	IEnumVARIANT GetEnumeratorVariant();
}
