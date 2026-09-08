using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace WindowsFirewallHelper.COMInterop;

[ComImport]
[Guid("C0E9D7FA-E07E-430A-B19A-090CE82D92E2")]
internal interface INetFwOpenPorts : IEnumerable
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
	void Add([In][MarshalAs(UnmanagedType.Interface)] INetFwOpenPort port);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(3)]
	void Remove([In] int portNumber, [In] NetFwIPProtocol ipProtocol);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(4)]
	[return: MarshalAs(UnmanagedType.Interface)]
	INetFwOpenPort Item([In] int portNumber, [In] NetFwIPProtocol ipProtocol);

	[DispId(-4)]
	IEnumVARIANT GetEnumeratorVariant();
}
