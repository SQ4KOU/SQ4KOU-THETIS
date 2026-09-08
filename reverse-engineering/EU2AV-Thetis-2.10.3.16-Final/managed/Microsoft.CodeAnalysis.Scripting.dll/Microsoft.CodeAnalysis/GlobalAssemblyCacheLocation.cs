using System;
using System.Collections.Immutable;
using System.Runtime.InteropServices;

namespace Microsoft.CodeAnalysis;

internal static class GlobalAssemblyCacheLocation
{
	internal enum ASM_CACHE
	{
		ZAP = 1,
		GAC = 2,
		DOWNLOAD = 4,
		ROOT = 8,
		GAC_MSIL = 0x10,
		GAC_32 = 0x20,
		GAC_64 = 0x40,
		ROOT_EX = 0x80
	}

	public static ImmutableArray<string> s_rootLocations;

	public static ImmutableArray<string> RootLocations
	{
		get
		{
			if (s_rootLocations.IsDefault)
			{
				s_rootLocations = ImmutableArray.Create(GetLocation(ASM_CACHE.ROOT), GetLocation(ASM_CACHE.ROOT_EX));
			}
			return s_rootLocations;
		}
	}

	[DllImport("clr")]
	private unsafe static extern int GetCachePath(ASM_CACHE id, byte* path, ref int length);

	private unsafe static string GetLocation(ASM_CACHE gacId)
	{
		int length = 0;
		int cachePath = GetCachePath(gacId, null, ref length);
		if (cachePath != -2147024774)
		{
			throw Marshal.GetExceptionForHR(cachePath);
		}
		fixed (byte* ptr = new byte[(length + 1) * 2])
		{
			cachePath = GetCachePath(gacId, ptr, ref length);
			if (cachePath != 0)
			{
				throw Marshal.GetExceptionForHR(cachePath);
			}
			return Marshal.PtrToStringUni((IntPtr)ptr);
		}
	}
}
