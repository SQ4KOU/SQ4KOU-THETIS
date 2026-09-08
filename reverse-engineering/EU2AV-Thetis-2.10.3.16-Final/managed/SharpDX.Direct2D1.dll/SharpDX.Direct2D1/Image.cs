using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct2D1;

[Guid("65019f75-8da2-497c-b32c-dfa34e48ede6")]
public class Image : Resource
{
	public Image(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Image(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new Image(nativePtr);
		}
		return null;
	}
}
