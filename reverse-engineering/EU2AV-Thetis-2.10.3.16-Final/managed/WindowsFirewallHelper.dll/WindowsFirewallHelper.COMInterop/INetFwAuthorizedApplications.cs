using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace WindowsFirewallHelper.COMInterop;

[ComImport]
[Guid("644EFD52-CCF9-486C-97A2-39F352570B30")]
internal interface INetFwAuthorizedApplications : IEnumerable
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
	void Add([In][MarshalAs(UnmanagedType.Interface)] INetFwAuthorizedApplication app);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(3)]
	void Remove([In][MarshalAs(UnmanagedType.BStr)] string imageFileName);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(4)]
	[return: MarshalAs(UnmanagedType.Interface)]
	INetFwAuthorizedApplication Item([In][MarshalAs(UnmanagedType.BStr)] string imageFileName);

	[DispId(-4)]
	IEnumVARIANT GetEnumeratorVariant();
}
