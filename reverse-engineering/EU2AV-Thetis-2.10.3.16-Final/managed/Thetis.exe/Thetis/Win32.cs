using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace Thetis;

internal class Win32
{
	public struct WSAData
	{
		public short version;

		public short highVersion;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 257)]
		public string description;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 129)]
		public string systemStatus;

		public short maxSockets;

		public short maxUdpDg;

		public IntPtr vendorInfo;
	}

	[DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
	public unsafe static extern void memcpy(void* destptr, void* srcptr, int n);

	[DllImport("kernel32.dll")]
	public unsafe static extern void EnterCriticalSection(void* cs_ptr);

	[DllImport("kernel32.dll")]
	public unsafe static extern void LeaveCriticalSection(void* cs_ptr);

	[DllImport("kernel32.dll")]
	public unsafe static extern void InitializeCriticalSection(void* cs_ptr);

	[DllImport("kernel32.dll")]
	public unsafe static extern int InitializeCriticalSectionAndSpinCount(void* cs_ptr, uint spincount);

	[DllImport("kernel32.dll")]
	public unsafe static extern byte DeleteCriticalSection(void* cs_ptr);

	[DllImport("kernel32.dll")]
	public static extern IntPtr GetCurrentThread();

	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern int SetThreadAffinityMask(IntPtr hThread, IntPtr dwThreadAffinityMask);

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public unsafe static extern void* NewCriticalSection();

	[DllImport("wdsp.dll", CallingConvention = CallingConvention.Cdecl)]
	public unsafe static extern void DestroyCriticalSection(void* cs_ptr);

	[DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
	public unsafe static extern void memset(void* addr, byte val, int n);

	[DllImport("user32.dll")]
	public static extern int SetWindowPos(int hwnd, int hWndInsertAfter, int x, int y, int cx, int cy, int wFlags);

	[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
	public static extern void keybd_event(byte vk, byte scan, int flags, int extrainfo);

	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern bool AllocConsole();

	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern bool FreeConsole();

	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern bool AttachConsole(int dwProcessId);

	[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
	public static extern int GetWindowTextW(IntPtr hwnd, StringBuilder lpString, int maxcount);

	[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
	public static extern int GetWindowTextLengthW(IntPtr hwnd);

	[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
	public static extern bool IsWindow(IntPtr hWnd);

	[DllImport("user32.dll", SetLastError = true)]
	public static extern IntPtr GetForegroundWindow();

	[DllImport("user32.dll", SetLastError = true)]
	public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

	[DllImport("gdi32.dll")]
	public static extern IntPtr AddFontMemResourceEx(byte[] pbFont, int cbFont, IntPtr pdv, out uint pcFonts);

	[DllImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool ShowWindowAsync(HandleRef hWnd, int nCmdShow);

	[DllImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool SetForegroundWindow(IntPtr hWnd);

	[DllImport("ws2_32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	public static extern int WSAStartup(short wVersionRequested, out WSAData wsaData);

	[DllImport("ws2_32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	public static extern int WSACleanup();

	[DllImport("winmm.dll", EntryPoint = "timeBeginPeriod", SetLastError = true)]
	[SuppressUnmanagedCodeSecurity]
	public static extern uint TimeBeginPeriod(uint uMilliseconds);

	[DllImport("winmm.dll", EntryPoint = "timeEndPeriod", SetLastError = true)]
	[SuppressUnmanagedCodeSecurity]
	public static extern uint TimeEndPeriod(uint uMilliseconds);
}
