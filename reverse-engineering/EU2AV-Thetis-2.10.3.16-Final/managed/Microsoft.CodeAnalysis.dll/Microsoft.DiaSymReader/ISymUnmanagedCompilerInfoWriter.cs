using System.Runtime.InteropServices;

namespace Microsoft.DiaSymReader;

[ComImport]
[Guid("2ae6a06a-92ba-4c2d-a64e-7e9fa421a330")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[ComVisible(false)]
internal interface ISymUnmanagedCompilerInfoWriter
{
	[PreserveSig]
	int AddCompilerInfo(ushort major, ushort minor, ushort build, ushort revision, [MarshalAs(UnmanagedType.LPWStr)] string name);
}
