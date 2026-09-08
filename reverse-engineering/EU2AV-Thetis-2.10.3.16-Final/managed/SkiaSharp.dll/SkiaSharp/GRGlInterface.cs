using System;
using System.Runtime.InteropServices;
using SkiaSharp.Internals;

namespace SkiaSharp;

public class GRGlInterface : SKObject, ISKReferenceCounted, ISKSkipObjectRegistration
{
	private static class AngleLoader
	{
		private static readonly IntPtr libEGL;

		private static readonly IntPtr libGLESv2;

		public static bool IsValid
		{
			get
			{
				if (libEGL != IntPtr.Zero)
				{
					return libGLESv2 != IntPtr.Zero;
				}
				return false;
			}
		}

		[DllImport("Kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
		private static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)] string lpFileName);

		[DllImport("Kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
		private static extern IntPtr GetProcAddress(IntPtr hModule, [MarshalAs(UnmanagedType.LPStr)] string lpProcName);

		[DllImport("libEGL.dll")]
		private static extern IntPtr eglGetProcAddress([MarshalAs(UnmanagedType.LPStr)] string procname);

		static AngleLoader()
		{
			if (PlatformConfiguration.IsWindows)
			{
				libEGL = LoadLibrary("libEGL.dll");
				libGLESv2 = LoadLibrary("libGLESv2.dll");
			}
		}

		public static IntPtr GetProc(string name)
		{
			if (!PlatformConfiguration.IsWindows)
			{
				return IntPtr.Zero;
			}
			if (!IsValid)
			{
				return IntPtr.Zero;
			}
			IntPtr intPtr = GetProcAddress(libGLESv2, name);
			if (intPtr == IntPtr.Zero)
			{
				intPtr = GetProcAddress(libEGL, name);
			}
			if (intPtr == IntPtr.Zero)
			{
				intPtr = eglGetProcAddress(name);
			}
			return intPtr;
		}
	}

	internal GRGlInterface(IntPtr h, bool owns)
		: base(h, owns)
	{
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
	}

	public static GRGlInterface Create()
	{
		return CreateGl() ?? CreateAngle();
	}

	private static GRGlInterface CreateGl()
	{
		return GetObject(SkiaApi.gr_glinterface_create_native_interface());
	}

	public static GRGlInterface CreateAngle()
	{
		if (PlatformConfiguration.IsWindows)
		{
			return CreateAngle(AngleLoader.GetProc);
		}
		return null;
	}

	public unsafe static GRGlInterface Create(GRGlGetProcedureAddressDelegate get)
	{
		DelegateProxies.Create(get, out var gch, out var contextPtr);
		GRGlGetProcProxyDelegate get2 = ((get != null) ? DelegateProxies.GRGlGetProcProxy : null);
		try
		{
			return GetObject(SkiaApi.gr_glinterface_assemble_interface((void*)contextPtr, get2));
		}
		finally
		{
			gch.Free();
		}
	}

	public static GRGlInterface CreateAngle(GRGlGetProcedureAddressDelegate get)
	{
		return CreateGles(get);
	}

	public unsafe static GRGlInterface CreateOpenGl(GRGlGetProcedureAddressDelegate get)
	{
		DelegateProxies.Create(get, out var gch, out var contextPtr);
		GRGlGetProcProxyDelegate get2 = ((get != null) ? DelegateProxies.GRGlGetProcProxy : null);
		try
		{
			return GetObject(SkiaApi.gr_glinterface_assemble_gl_interface((void*)contextPtr, get2));
		}
		finally
		{
			gch.Free();
		}
	}

	public unsafe static GRGlInterface CreateGles(GRGlGetProcedureAddressDelegate get)
	{
		DelegateProxies.Create(get, out var gch, out var contextPtr);
		GRGlGetProcProxyDelegate get2 = ((get != null) ? DelegateProxies.GRGlGetProcProxy : null);
		try
		{
			return GetObject(SkiaApi.gr_glinterface_assemble_gles_interface((void*)contextPtr, get2));
		}
		finally
		{
			gch.Free();
		}
	}

	public unsafe static GRGlInterface CreateWebGl(GRGlGetProcedureAddressDelegate get)
	{
		DelegateProxies.Create(get, out var gch, out var contextPtr);
		GRGlGetProcProxyDelegate get2 = ((get != null) ? DelegateProxies.GRGlGetProcProxy : null);
		try
		{
			return GetObject(SkiaApi.gr_glinterface_assemble_webgl_interface((void*)contextPtr, get2));
		}
		finally
		{
			gch.Free();
		}
	}

	public static GRGlInterface CreateEvas(IntPtr evas)
	{
		return null;
	}

	public bool Validate()
	{
		return SkiaApi.gr_glinterface_validate(Handle);
	}

	public bool HasExtension(string extension)
	{
		return SkiaApi.gr_glinterface_has_extension(Handle, extension);
	}

	internal static GRGlInterface GetObject(IntPtr handle)
	{
		if (!(handle == IntPtr.Zero))
		{
			return new GRGlInterface(handle, owns: true);
		}
		return null;
	}
}
