using System;
using System.Runtime.InteropServices;
using SharpDX.IO;
using SharpDX.Mathematics.Interop;
using SharpDX.Win32;

namespace SharpDX;

internal class Win32Native
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct TextMetric
	{
		public int tmHeight;

		public int tmAscent;

		public int tmDescent;

		public int tmInternalLeading;

		public int tmExternalLeading;

		public int tmAveCharWidth;

		public int tmMaxCharWidth;

		public int tmWeight;

		public int tmOverhang;

		public int tmDigitizedAspectX;

		public int tmDigitizedAspectY;

		public char tmFirstChar;

		public char tmLastChar;

		public char tmDefaultChar;

		public char tmBreakChar;

		public byte tmItalic;

		public byte tmUnderlined;

		public byte tmStruckOut;

		public byte tmPitchAndFamily;

		public byte tmCharSet;
	}

	public enum WindowLongType
	{
		WndProc = -4,
		HInstance = -6,
		HwndParent = -8,
		Style = -16,
		ExtendedStyle = -20,
		UserData = -21,
		Id = -12
	}

	public delegate IntPtr WndProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

	[DllImport("kernel32.dll", CharSet = CharSet.Unicode, EntryPoint = "CreateFile", SetLastError = true)]
	internal static extern IntPtr Create(string fileName, NativeFileAccess desiredAccess, NativeFileShare shareMode, IntPtr securityAttributes, NativeFileMode mode, NativeFileOptions flagsAndOptions, IntPtr templateFile);

	[DllImport("user32.dll")]
	public static extern int PeekMessage(out NativeMessage lpMsg, IntPtr hWnd, int wMsgFilterMin, int wMsgFilterMax, int wRemoveMsg);

	[DllImport("user32.dll")]
	public static extern int GetMessage(out NativeMessage lpMsg, IntPtr hWnd, int wMsgFilterMin, int wMsgFilterMax);

	[DllImport("user32.dll")]
	public static extern int TranslateMessage(ref NativeMessage lpMsg);

	[DllImport("user32.dll")]
	public static extern int DispatchMessage(ref NativeMessage lpMsg);

	public static IntPtr GetWindowLong(IntPtr hWnd, WindowLongType index)
	{
		if (IntPtr.Size == 4)
		{
			return GetWindowLong32(hWnd, index);
		}
		return GetWindowLong64(hWnd, index);
	}

	[DllImport("user32.dll", CharSet = CharSet.Unicode)]
	public static extern IntPtr GetFocus();

	[DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "GetWindowLong")]
	private static extern IntPtr GetWindowLong32(IntPtr hwnd, WindowLongType index);

	[DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "GetWindowLongPtr")]
	private static extern IntPtr GetWindowLong64(IntPtr hwnd, WindowLongType index);

	public static IntPtr SetWindowLong(IntPtr hwnd, WindowLongType index, IntPtr wndProcPtr)
	{
		if (IntPtr.Size == 4)
		{
			return SetWindowLong32(hwnd, index, wndProcPtr);
		}
		return SetWindowLongPtr64(hwnd, index, wndProcPtr);
	}

	[DllImport("user32.dll", CharSet = CharSet.Unicode)]
	public static extern IntPtr SetParent(IntPtr hWnd, IntPtr hWndParent);

	[DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "SetWindowLong")]
	private static extern IntPtr SetWindowLong32(IntPtr hwnd, WindowLongType index, IntPtr wndProc);

	public static bool ShowWindow(IntPtr hWnd, bool windowVisible)
	{
		return ShowWindow(hWnd, windowVisible ? 1 : 0);
	}

	[DllImport("user32.dll", CharSet = CharSet.Unicode)]
	private static extern bool ShowWindow(IntPtr hWnd, int mCmdShow);

	[DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "SetWindowLongPtr")]
	private static extern IntPtr SetWindowLongPtr64(IntPtr hwnd, WindowLongType index, IntPtr wndProc);

	[DllImport("user32.dll", CharSet = CharSet.Unicode)]
	public static extern IntPtr CallWindowProc(IntPtr wndProc, IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam);

	[DllImport("user32.dll")]
	public static extern bool GetClientRect(IntPtr hWnd, out RawRectangle lpRect);

	[DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
	public static extern IntPtr GetModuleHandle(string lpModuleName);
}
