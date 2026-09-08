using System;
using System.Runtime.InteropServices;

namespace SkiaSharp.Internals;

public static class PlatformConfiguration
{
	private const string LibCLibrary = "libc";

	private static string linuxFlavor;

	private static readonly Lazy<bool> isGlibcLazy = new Lazy<bool>(IsGlibcImplementation);

	public static bool IsUnix
	{
		get
		{
			if (!IsMac)
			{
				return IsLinux;
			}
			return true;
		}
	}

	public static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

	public static bool IsMac => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

	public static bool IsLinux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

	public static bool IsArm
	{
		get
		{
			Architecture processArchitecture = RuntimeInformation.ProcessArchitecture;
			if ((uint)(processArchitecture - 2) <= 1u)
			{
				return true;
			}
			return false;
		}
	}

	public static bool Is64Bit => IntPtr.Size == 8;

	public static string LinuxFlavor
	{
		get
		{
			if (!IsLinux)
			{
				return null;
			}
			if (!string.IsNullOrEmpty(linuxFlavor))
			{
				return linuxFlavor;
			}
			if (!IsGlibc)
			{
				return "musl";
			}
			return null;
		}
		set
		{
			linuxFlavor = value;
		}
	}

	public static bool IsGlibc
	{
		get
		{
			if (IsLinux)
			{
				return isGlibcLazy.Value;
			}
			return false;
		}
	}

	private static bool IsGlibcImplementation()
	{
		try
		{
			gnu_get_libc_version();
			return true;
		}
		catch (TypeLoadException)
		{
			return false;
		}
	}

	[DllImport("libc", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
	private static extern IntPtr gnu_get_libc_version();
}
