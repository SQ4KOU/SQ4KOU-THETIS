using System;
using System.Runtime.InteropServices;

namespace SharpDX.DirectWrite;

public struct FontProperty
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct __Native
	{
		public FontPropertyId PropertyId;

		public IntPtr PropertyValue;

		public IntPtr LocaleName;
	}

	public FontPropertyId PropertyId;

	public string PropertyValue;

	public string LocaleName;

	internal void __MarshalFree(ref __Native @ref)
	{
		Marshal.FreeHGlobal(@ref.PropertyValue);
		Marshal.FreeHGlobal(@ref.LocaleName);
	}

	internal void __MarshalFrom(ref __Native @ref)
	{
		PropertyId = @ref.PropertyId;
		PropertyValue = Marshal.PtrToStringUni(@ref.PropertyValue);
		LocaleName = Marshal.PtrToStringUni(@ref.LocaleName);
	}

	internal void __MarshalTo(ref __Native @ref)
	{
		@ref.PropertyId = PropertyId;
		@ref.PropertyValue = Marshal.StringToHGlobalUni(PropertyValue);
		@ref.LocaleName = Marshal.StringToHGlobalUni(LocaleName);
	}
}
