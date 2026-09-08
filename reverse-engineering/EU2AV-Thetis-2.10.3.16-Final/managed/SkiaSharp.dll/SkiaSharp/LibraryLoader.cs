using System;
using System.IO;
using System.Runtime.InteropServices;
using SkiaSharp.Internals;

namespace SkiaSharp;

internal static class LibraryLoader
{
	private static class Mac
	{
		private const string SystemLibrary = "/usr/lib/libSystem.dylib";

		private const int RTLD_LAZY = 1;

		private const int RTLD_NOW = 2;

		public static IntPtr dlopen(string path, bool lazy = true)
		{
			return dlopen(path, lazy ? 1 : 2);
		}

		[DllImport("/usr/lib/libSystem.dylib")]
		public static extern IntPtr dlopen(string path, int mode);

		[DllImport("/usr/lib/libSystem.dylib")]
		public static extern IntPtr dlsym(IntPtr handle, string symbol);

		[DllImport("/usr/lib/libSystem.dylib")]
		public static extern void dlclose(IntPtr handle);
	}

	private static class Linux
	{
		private const string SystemLibrary = "libdl.so";

		private const string SystemLibrary2 = "libdl.so.2";

		private const int RTLD_LAZY = 1;

		private const int RTLD_NOW = 2;

		private const int RTLD_DEEPBIND = 8;

		private static bool UseSystemLibrary2 = true;

		public static IntPtr dlopen(string path, bool lazy = true)
		{
			try
			{
				return dlopen2(path, (lazy ? 1 : 2) | 8);
			}
			catch (DllNotFoundException)
			{
				UseSystemLibrary2 = false;
				return dlopen1(path, (lazy ? 1 : 2) | 8);
			}
		}

		public static IntPtr dlsym(IntPtr handle, string symbol)
		{
			if (!UseSystemLibrary2)
			{
				return dlsym1(handle, symbol);
			}
			return dlsym2(handle, symbol);
		}

		public static void dlclose(IntPtr handle)
		{
			if (UseSystemLibrary2)
			{
				dlclose2(handle);
			}
			else
			{
				dlclose1(handle);
			}
		}

		[DllImport("libdl.so", EntryPoint = "dlopen")]
		private static extern IntPtr dlopen1(string path, int mode);

		[DllImport("libdl.so", EntryPoint = "dlsym")]
		private static extern IntPtr dlsym1(IntPtr handle, string symbol);

		[DllImport("libdl.so", EntryPoint = "dlclose")]
		private static extern void dlclose1(IntPtr handle);

		[DllImport("libdl.so.2", EntryPoint = "dlopen")]
		private static extern IntPtr dlopen2(string path, int mode);

		[DllImport("libdl.so.2", EntryPoint = "dlsym")]
		private static extern IntPtr dlsym2(IntPtr handle, string symbol);

		[DllImport("libdl.so.2", EntryPoint = "dlclose")]
		private static extern void dlclose2(IntPtr handle);
	}

	private static class Win32
	{
		private const string SystemLibrary = "Kernel32.dll";

		[DllImport("Kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
		public static extern IntPtr LoadLibrary(string lpFileName);

		[DllImport("Kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
		public static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

		[DllImport("Kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
		public static extern void FreeLibrary(IntPtr hModule);
	}

	public static string Extension { get; }

	static LibraryLoader()
	{
		if (PlatformConfiguration.IsWindows)
		{
			Extension = ".dll";
		}
		else if (PlatformConfiguration.IsMac)
		{
			Extension = ".dylib";
		}
		else
		{
			Extension = ".so";
		}
	}

	public static IntPtr LoadLocalLibrary<T>(string libraryName)
	{
		string libraryName2 = GetLibraryPath(libraryName);
		IntPtr intPtr = LoadLibrary(libraryName2);
		if (intPtr == IntPtr.Zero)
		{
			throw new DllNotFoundException("Unable to load library '" + libraryName + "'.");
		}
		return intPtr;
		static bool CheckLibraryPath(string root, string arch, string libWithExt, out string foundPath)
		{
			if (!string.IsNullOrEmpty(root))
			{
				if (!string.IsNullOrEmpty(PlatformConfiguration.LinuxFlavor))
				{
					string text = Path.Combine(root, PlatformConfiguration.LinuxFlavor + "-" + arch, libWithExt);
					if (File.Exists(text))
					{
						foundPath = text;
						return true;
					}
				}
				string text2 = Path.Combine(root, arch, libWithExt);
				if (File.Exists(text2))
				{
					foundPath = text2;
					return true;
				}
				text2 = Path.Combine(root, libWithExt);
				if (File.Exists(text2))
				{
					foundPath = text2;
					return true;
				}
			}
			foundPath = null;
			return false;
		}
		static string GetLibraryPath(string text2)
		{
			string arch = ((!PlatformConfiguration.Is64Bit) ? (PlatformConfiguration.IsArm ? "arm" : "x86") : (PlatformConfiguration.IsArm ? "arm64" : "x64"));
			string text = text2;
			if (!text2.EndsWith(Extension, StringComparison.OrdinalIgnoreCase))
			{
				text += Extension;
			}
			string location = typeof(T).Assembly.Location;
			if (!string.IsNullOrEmpty(location))
			{
				location = Path.GetDirectoryName(location);
				if (CheckLibraryPath(location, arch, text, out var foundPath))
				{
					return foundPath;
				}
			}
			if (CheckLibraryPath(Directory.GetCurrentDirectory(), arch, text, out var foundPath2))
			{
				return foundPath2;
			}
			try
			{
				AppDomain currentDomain = AppDomain.CurrentDomain;
				if (currentDomain != null)
				{
					if (CheckLibraryPath(currentDomain.RelativeSearchPath, arch, text, out foundPath2))
					{
						return foundPath2;
					}
					if (CheckLibraryPath(currentDomain.BaseDirectory, arch, text, out foundPath2))
					{
						return foundPath2;
					}
				}
			}
			catch
			{
			}
			return text;
		}
	}

	public static T GetSymbolDelegate<T>(IntPtr library, string name) where T : Delegate
	{
		IntPtr symbol = GetSymbol(library, name);
		if (symbol == IntPtr.Zero)
		{
			throw new EntryPointNotFoundException("Unable to load symbol '" + name + "'.");
		}
		return Marshal.GetDelegateForFunctionPointer<T>(symbol);
	}

	public static IntPtr LoadLibrary(string libraryName)
	{
		if (string.IsNullOrEmpty(libraryName))
		{
			throw new ArgumentNullException("libraryName");
		}
		if (PlatformConfiguration.IsWindows)
		{
			return Win32.LoadLibrary(libraryName);
		}
		if (PlatformConfiguration.IsLinux)
		{
			return Linux.dlopen(libraryName);
		}
		if (PlatformConfiguration.IsMac)
		{
			return Mac.dlopen(libraryName);
		}
		throw new PlatformNotSupportedException("Current platform is unknown, unable to load library '" + libraryName + "'.");
	}

	public static IntPtr GetSymbol(IntPtr library, string symbolName)
	{
		if (string.IsNullOrEmpty(symbolName))
		{
			throw new ArgumentNullException("symbolName");
		}
		if (PlatformConfiguration.IsWindows)
		{
			return Win32.GetProcAddress(library, symbolName);
		}
		if (PlatformConfiguration.IsLinux)
		{
			return Linux.dlsym(library, symbolName);
		}
		if (PlatformConfiguration.IsMac)
		{
			return Mac.dlsym(library, symbolName);
		}
		throw new PlatformNotSupportedException($"Current platform is unknown, unable to load symbol '{symbolName}' from library {library}.");
	}

	public static void FreeLibrary(IntPtr library)
	{
		if (library == IntPtr.Zero)
		{
			return;
		}
		if (PlatformConfiguration.IsWindows)
		{
			Win32.FreeLibrary(library);
			return;
		}
		if (PlatformConfiguration.IsLinux)
		{
			Linux.dlclose(library);
			return;
		}
		if (PlatformConfiguration.IsMac)
		{
			Mac.dlclose(library);
			return;
		}
		throw new PlatformNotSupportedException($"Current platform is unknown, unable to close library '{library}'.");
	}
}
