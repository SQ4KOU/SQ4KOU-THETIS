using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace WindowsFirewallHelper.COMInterop;

[ComImport]
[Guid("79649BB4-903E-421B-94C9-79848E79F6EE")]
internal interface INetFwServices : IEnumerable
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
	[return: MarshalAs(UnmanagedType.Interface)]
	INetFwService Item([In] NetFwServiceType svcType);

	[DispId(-4)]
	IEnumVARIANT GetEnumeratorVariant();
}
