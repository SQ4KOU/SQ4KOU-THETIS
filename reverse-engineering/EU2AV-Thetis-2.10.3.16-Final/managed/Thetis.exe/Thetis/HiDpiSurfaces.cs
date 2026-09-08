using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Thetis;

internal static class HiDpiSurfaces
{
	private struct RECT
	{
		public int left;

		public int top;

		public int right;

		public int bottom;
	}

	private sealed class MouseFilter : IMessageFilter
	{
		public bool PreFilterMessage(ref Message m)
		{
			Entry value;
			lock (_entries)
			{
				if (!_entries.TryGetValue(m.HWnd, out value))
				{
					return false;
				}
			}
			if (m.Msg == 736)
			{
				try
				{
					if (value.OnDpiChanged != null)
					{
						value.OnDpiChanged();
					}
				}
				catch
				{
				}
				return false;
			}
			if (m.Msg < 512 || m.Msg > 525)
			{
				return false;
			}
			Control ctl = value.Ctl;
			if (ctl == null || ctl.IsDisposed || ctl.Width <= 0)
			{
				return false;
			}
			Size physicalClientSize = GetPhysicalClientSize(m.HWnd);
			if (physicalClientSize.Width <= 0)
			{
				return false;
			}
			float num = (float)physicalClientSize.Width / (float)ctl.Width;
			float num2 = (float)physicalClientSize.Height / (float)Math.Max(1, ctl.Height);
			if (num <= 1.01f && num2 <= 1.01f)
			{
				return false;
			}
			int num3 = m.LParam.ToInt32();
			short num4 = (short)(num3 & 0xFFFF);
			short num5 = (short)((num3 >> 16) & 0xFFFF);
			num4 = (short)((float)num4 / num);
			num5 = (short)((float)num5 / num2);
			m.LParam = (IntPtr)((num5 << 16) | (num4 & 0xFFFF));
			return false;
		}
	}

	private sealed class Entry
	{
		public Control Ctl;

		public Action OnDpiChanged;
	}

	private static readonly IntPtr CTX_PER_MONITOR_V2 = new IntPtr(-4);

	private static readonly IntPtr CTX_UNAWARE = new IntPtr(-1);

	private const int WM_DPICHANGED = 736;

	private static readonly Dictionary<IntPtr, Entry> _entries = new Dictionary<IntPtr, Entry>();

	private static MouseFilter _filter;

	[DllImport("user32.dll")]
	private static extern IntPtr SetThreadDpiAwarenessContext(IntPtr dpiContext);

	[DllImport("user32.dll")]
	private static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

	[DllImport("user32.dll")]
	private static extern uint GetDpiForWindow(IntPtr hwnd);

	public static Size GetPhysicalClientSize(IntPtr hwnd)
	{
		IntPtr intPtr = IntPtr.Zero;
		bool flag = false;
		try
		{
			intPtr = SetThreadDpiAwarenessContext(CTX_PER_MONITOR_V2);
			flag = intPtr != IntPtr.Zero;
		}
		catch
		{
		}
		bool clientRect = GetClientRect(hwnd, out var lpRect);
		if (flag)
		{
			try
			{
				SetThreadDpiAwarenessContext(intPtr);
			}
			catch
			{
			}
		}
		if (!clientRect)
		{
			return Size.Empty;
		}
		return new Size(lpRect.right - lpRect.left, lpRect.bottom - lpRect.top);
	}

	public static float GetWindowScale(IntPtr hwnd)
	{
		if (hwnd == IntPtr.Zero)
		{
			return 1f;
		}
		IntPtr intPtr = IntPtr.Zero;
		bool flag = false;
		try
		{
			intPtr = SetThreadDpiAwarenessContext(CTX_PER_MONITOR_V2);
			flag = intPtr != IntPtr.Zero;
		}
		catch
		{
		}
		uint num = 96u;
		try
		{
			num = GetDpiForWindow(hwnd);
		}
		catch
		{
		}
		if (flag)
		{
			try
			{
				SetThreadDpiAwarenessContext(intPtr);
			}
			catch
			{
			}
		}
		if (num == 0)
		{
			num = 96u;
		}
		return (float)num / 96f;
	}

	public static void RecreateHandleDpiAware(Control c, bool perMonitorV2)
	{
		if (c == null || c.IsDisposed || !c.IsHandleCreated)
		{
			return;
		}
		IntPtr intPtr = IntPtr.Zero;
		bool flag = false;
		try
		{
			intPtr = SetThreadDpiAwarenessContext(perMonitorV2 ? CTX_PER_MONITOR_V2 : CTX_UNAWARE);
			flag = intPtr != IntPtr.Zero;
		}
		catch
		{
		}
		try
		{
			MethodInfo method = typeof(Control).GetMethod("RecreateHandle", BindingFlags.Instance | BindingFlags.NonPublic);
			if (method != null)
			{
				method.Invoke(c, null);
			}
		}
		finally
		{
			if (flag)
			{
				try
				{
					SetThreadDpiAwarenessContext(intPtr);
				}
				catch
				{
				}
			}
		}
	}

	public static float GetScale(Control c)
	{
		if (c == null || !c.IsHandleCreated || c.Width <= 0)
		{
			return 1f;
		}
		Size physicalClientSize = GetPhysicalClientSize(c.Handle);
		if (physicalClientSize.Width <= 0)
		{
			return 1f;
		}
		return (float)physicalClientSize.Width / (float)c.Width;
	}

	public static void Enable(Control c, Action onDpiChanged)
	{
		if (c == null || c.IsDisposed || !c.IsHandleCreated)
		{
			return;
		}
		if (_filter == null)
		{
			_filter = new MouseFilter();
			Application.AddMessageFilter(_filter);
		}
		RecreateHandleDpiAware(c, perMonitorV2: true);
		lock (_entries)
		{
			_entries[c.Handle] = new Entry
			{
				Ctl = c,
				OnDpiChanged = onDpiChanged
			};
		}
	}

	public static void Disable(Control c)
	{
		if (c != null && !c.IsDisposed && c.IsHandleCreated)
		{
			lock (_entries)
			{
				_entries.Remove(c.Handle);
			}
			RecreateHandleDpiAware(c, perMonitorV2: false);
		}
	}
}
