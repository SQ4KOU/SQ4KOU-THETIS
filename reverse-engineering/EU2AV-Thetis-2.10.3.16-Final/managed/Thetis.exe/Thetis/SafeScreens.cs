using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Thetis;

internal static class SafeScreens
{
	private delegate bool MonitorEnumProc(IntPtr hMonitor, IntPtr hdcMonitor, IntPtr lprcMonitor, IntPtr dwData);

	private struct RECT
	{
		public int left;

		public int top;

		public int right;

		public int bottom;

		public override string ToString()
		{
			return "{" + $"X={left},Y={top},Width={right - left},Height={bottom - top}" + "}";
		}
	}

	private enum MONITOR_DPI_TYPE
	{
		MDT_EFFECTIVE_DPI = 0,
		MDT_ANGULAR_DPI = 1,
		MDT_RAW_DPI = 2,
		MDT_DEFAULT = MDT_EFFECTIVE_DPI
	}

	private struct MONITORINFO
	{
		public uint cbSize;

		public RECT rcMonitor;

		public RECT rcWork;

		public uint dwFlags;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
	private struct MONITORINFOEX
	{
		public uint cbSize;

		public RECT rcMonitor;

		public RECT rcWork;

		public uint dwFlags;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
		public string szDevice;
	}

	private struct monitor_info
	{
		public Rectangle rect_monitor;

		public Rectangle rect_work;

		public int scale_percent;

		public int index;

		public int display_number;
	}

	private const int DWMWA_EXTENDED_FRAME_BOUNDS = 9;

	private static readonly IntPtr DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2 = new IntPtr(-4);

	[DllImport("Shcore.dll")]
	private static extern int GetDpiForMonitor(IntPtr hmonitor, MONITOR_DPI_TYPE dpiType, out uint dpiX, out uint dpiY);

	[DllImport("user32.dll")]
	private static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, MonitorEnumProc lpfnEnum, IntPtr dwData);

	[DllImport("user32.dll", CharSet = CharSet.Auto)]
	private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

	[DllImport("user32.dll", CharSet = CharSet.Auto, EntryPoint = "GetMonitorInfo")]
	private static extern bool GetMonitorInfoEx(IntPtr hMonitor, ref MONITORINFOEX lpmi);

	[DllImport("user32.dll")]
	private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

	[DllImport("user32.dll")]
	private static extern IntPtr SetThreadDpiAwarenessContext(IntPtr dpiContext);

	[DllImport("dwmapi.dll")]
	private static extern int DwmGetWindowAttribute(IntPtr hwnd, int dwAttribute, out RECT pvAttribute, int cbAttribute);

	public static (Rectangle adjusted, bool resized, bool repositioned) EnsureRectangleWithinNearestScreen(Rectangle? rect = null, Form form = null, bool keep_on_screen = false, bool use_working_area = false)
	{
		if (!rect.HasValue && form == null)
		{
			return (adjusted: Rectangle.Empty, resized: false, repositioned: false);
		}
		if (form != null && form.WindowState == FormWindowState.Maximized)
		{
			if (tryGetExtendedFrameBounds(form, out var rect2))
			{
				return (adjusted: rect2, resized: false, repositioned: false);
			}
			return (adjusted: use_working_area ? Screen.FromHandle(form.Handle).WorkingArea : Screen.FromHandle(form.Handle).Bounds, resized: false, repositioned: false);
		}
		List<Rectangle> monitorRects = getMonitorRects(use_working_area);
		if (monitorRects.Count == 0)
		{
			for (int i = 0; i < Screen.AllScreens.Length; i++)
			{
				monitorRects.Add(use_working_area ? Screen.AllScreens[i].WorkingArea : Screen.AllScreens[i].Bounds);
			}
		}
		Rectangle rectangle = form?.Bounds ?? rect.Value;
		int left = 0;
		int top = 0;
		int right = 0;
		int bottom = 0;
		if (form != null)
		{
			getDwmShadowMargins(form, out left, out top, out right, out bottom);
		}
		bool num = isFullyOnMonitors(rectangle, monitorRects);
		bool flag = isContainedByAnyScreen(rectangle, monitorRects);
		bool flag2 = !num || (keep_on_screen && !flag);
		Rectangle rectangle2 = chooseTargetMonitor(rectangle, monitorRects, keep_on_screen);
		bool item = false;
		bool item2 = false;
		Rectangle item3 = rectangle;
		if (flag2)
		{
			int num2 = rectangle.X;
			int num3 = rectangle.Y;
			int num4 = rectangle.Width;
			int num5 = rectangle.Height;
			int num6 = rectangle2.Left - left;
			int num7 = rectangle2.Top - top;
			int num8 = rectangle2.Right + right;
			int num9 = rectangle2.Bottom + bottom;
			int num10 = Math.Max(1, num8 - num6);
			int num11 = Math.Max(1, num9 - num7);
			if (num4 > num10)
			{
				num4 = num10;
				item = true;
			}
			if (num5 > num11)
			{
				num5 = num11;
				item = true;
			}
			if (num2 < num6)
			{
				num2 = num6;
			}
			if (num3 < num7)
			{
				num3 = num7;
			}
			if (num2 + num4 > num8)
			{
				num2 = num8 - num4;
			}
			if (num3 + num5 > num9)
			{
				num3 = num9 - num5;
			}
			if (num2 != rectangle.X || num3 != rectangle.Y)
			{
				item2 = true;
			}
			item3 = new Rectangle(num2, num3, num4, num5);
		}
		if ((form != null) & flag2)
		{
			if (form.WindowState != FormWindowState.Normal)
			{
				form.WindowState = FormWindowState.Normal;
			}
			form.SetBounds(item3.X, item3.Y, item3.Width, item3.Height);
		}
		return (adjusted: item3, resized: item, repositioned: item2);
	}

	private static List<Rectangle> getMonitorRects(bool use_working_area)
	{
		List<Rectangle> list = new List<Rectangle>();
		for (int i = 0; i < Screen.AllScreens.Length; i++)
		{
			list.Add(use_working_area ? Screen.AllScreens[i].WorkingArea : Screen.AllScreens[i].Bounds);
		}
		return list;
	}

	private static Rectangle chooseTargetMonitor(Rectangle r, List<Rectangle> monitors, bool keep_on_screen)
	{
		if (keep_on_screen)
		{
			Point position = Cursor.Position;
			for (int i = 0; i < monitors.Count; i++)
			{
				if (monitors[i].Contains(position))
				{
					return monitors[i];
				}
			}
		}
		int num = -1;
		long num2 = -1L;
		for (int j = 0; j < monitors.Count; j++)
		{
			Rectangle rectangle = Rectangle.Intersect(r, monitors[j]);
			long num3 = ((rectangle.Width > 0 && rectangle.Height > 0) ? ((long)rectangle.Width * (long)rectangle.Height) : 0);
			if (num3 > num2)
			{
				num2 = num3;
				num = j;
			}
		}
		if (num >= 0)
		{
			return monitors[num];
		}
		long num4 = long.MaxValue;
		int index = 0;
		for (int k = 0; k < monitors.Count; k++)
		{
			Rectangle rectangle2 = monitors[k];
			int num5 = clamp(r.X + r.Width / 2, rectangle2.Left, rectangle2.Right);
			int num6 = clamp(r.Y + r.Height / 2, rectangle2.Top, rectangle2.Bottom);
			long num7 = (long)(r.X + r.Width / 2) - (long)num5;
			long num8 = (long)(r.Y + r.Height / 2) - (long)num6;
			long num9 = num7 * num7 + num8 * num8;
			if (num9 < num4)
			{
				num4 = num9;
				index = k;
			}
		}
		return monitors[index];
	}

	private static bool isFullyOnMonitors(Rectangle r, List<Rectangle> monitors)
	{
		long num = (long)r.Width * (long)r.Height;
		long num2 = 0L;
		for (int i = 0; i < monitors.Count; i++)
		{
			Rectangle rectangle = Rectangle.Intersect(r, monitors[i]);
			if (rectangle.Width > 0 && rectangle.Height > 0)
			{
				num2 += (long)rectangle.Width * (long)rectangle.Height;
			}
		}
		return num2 >= num;
	}

	private static bool isContainedByAnyScreen(Rectangle r, List<Rectangle> monitors)
	{
		for (int i = 0; i < monitors.Count; i++)
		{
			if (monitors[i].Contains(r))
			{
				return true;
			}
		}
		return false;
	}

	private static Rectangle getUnion(List<Rectangle> rects)
	{
		if (rects.Count == 0)
		{
			return new Rectangle(0, 0, 1, 1);
		}
		Rectangle result = rects[0];
		for (int i = 1; i < rects.Count; i++)
		{
			Rectangle rectangle = rects[i];
			int left = Math.Min(result.Left, rectangle.Left);
			int top = Math.Min(result.Top, rectangle.Top);
			int right = Math.Max(result.Right, rectangle.Right);
			int bottom = Math.Max(result.Bottom, rectangle.Bottom);
			result = Rectangle.FromLTRB(left, top, right, bottom);
		}
		return result;
	}

	private static int clamp(int v, int a, int b)
	{
		if (v < a)
		{
			return a;
		}
		if (v > b)
		{
			return b;
		}
		return v;
	}

	private static Color colorFromHue(double hue_deg, double saturation, double value)
	{
		double num = value * saturation;
		double num2 = num * (1.0 - Math.Abs(hue_deg / 60.0 % 2.0 - 1.0));
		double num3 = value - num;
		double num4 = 0.0;
		double num5 = 0.0;
		double num6 = 0.0;
		if (hue_deg < 60.0)
		{
			num4 = num;
			num5 = num2;
			num6 = 0.0;
		}
		else if (hue_deg < 120.0)
		{
			num4 = num2;
			num5 = num;
			num6 = 0.0;
		}
		else if (hue_deg < 180.0)
		{
			num4 = 0.0;
			num5 = num;
			num6 = num2;
		}
		else if (hue_deg < 240.0)
		{
			num4 = 0.0;
			num5 = num2;
			num6 = num;
		}
		else if (hue_deg < 300.0)
		{
			num4 = num2;
			num5 = 0.0;
			num6 = num;
		}
		else
		{
			num4 = num;
			num5 = 0.0;
			num6 = num2;
		}
		int num7 = (int)Math.Round((num4 + num3) * 255.0);
		int num8 = (int)Math.Round((num5 + num3) * 255.0);
		int num9 = (int)Math.Round((num6 + num3) * 255.0);
		if (num7 < 0)
		{
			num7 = 0;
		}
		if (num7 > 255)
		{
			num7 = 255;
		}
		if (num8 < 0)
		{
			num8 = 0;
		}
		if (num8 > 255)
		{
			num8 = 255;
		}
		if (num9 < 0)
		{
			num9 = 0;
		}
		if (num9 > 255)
		{
			num9 = 255;
		}
		return Color.FromArgb(num7, num8, num9);
	}

	private static bool tryGetExtendedFrameBounds(Form form, out Rectangle rect)
	{
		rect = Rectangle.Empty;
		if (form == null)
		{
			return false;
		}
		if (DwmGetWindowAttribute(form.Handle, 9, out var pvAttribute, Marshal.SizeOf(typeof(RECT))) != 0)
		{
			return false;
		}
		rect = Rectangle.FromLTRB(pvAttribute.left, pvAttribute.top, pvAttribute.right, pvAttribute.bottom);
		return true;
	}

	private static void getDwmShadowMargins(Form form, out int left, out int top, out int right, out int bottom)
	{
		left = 0;
		top = 0;
		right = 0;
		bottom = 0;
		if (form != null && tryGetExtendedFrameBounds(form, out var rect) && (tryGetWindowRectPhysical(form.Handle, out var rect2) || GetWindowRect(form.Handle, out rect2)))
		{
			Rectangle rectangle = Rectangle.FromLTRB(rect2.left, rect2.top, rect2.right, rect2.bottom);
			int num = rect.Left - rectangle.Left;
			int num2 = rect.Top - rectangle.Top;
			int num3 = rectangle.Right - rect.Right;
			int num4 = rectangle.Bottom - rect.Bottom;
			double num5 = 1.0;
			int width = form.Bounds.Width;
			if (width > 0)
			{
				num5 = (double)rectangle.Width / (double)width;
			}
			if (num5 <= 0.01)
			{
				num5 = 1.0;
			}
			left = (int)Math.Round((double)num / num5);
			top = (int)Math.Round((double)num2 / num5);
			right = (int)Math.Round((double)num3 / num5);
			bottom = (int)Math.Round((double)num4 / num5);
			if (left < 0)
			{
				left = 0;
			}
			if (top < 0)
			{
				top = 0;
			}
			if (right < 0)
			{
				right = 0;
			}
			if (bottom < 0)
			{
				bottom = 0;
			}
		}
	}

	private static bool tryGetWindowRectPhysical(IntPtr hwnd, out RECT rect)
	{
		rect = default(RECT);
		IntPtr intPtr = IntPtr.Zero;
		bool flag = false;
		try
		{
			intPtr = SetThreadDpiAwarenessContext(DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2);
			flag = intPtr != IntPtr.Zero;
		}
		catch
		{
			flag = false;
		}
		bool windowRect = GetWindowRect(hwnd, out rect);
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
		return windowRect;
	}

	public static Bitmap createScreensBitmap(Size target_size, IEnumerable<Form> forms, bool use_working_area = false)
	{
		if (target_size.Width <= 0 || target_size.Height <= 0)
		{
			return new Bitmap(1, 1);
		}
		List<monitor_info> monitorInfosPhysical = getMonitorInfosPhysical(use_working_area);
		if (monitorInfosPhysical.Count == 0)
		{
			for (int i = 0; i < Screen.AllScreens.Length; i++)
			{
				monitor_info item = default(monitor_info);
				Rectangle bounds = Screen.AllScreens[i].Bounds;
				Rectangle workingArea = Screen.AllScreens[i].WorkingArea;
				item.rect_monitor = bounds;
				item.rect_work = workingArea;
				item.scale_percent = 100;
				item.index = i + 1;
				item.display_number = item.index;
				monitorInfosPhysical.Add(item);
			}
		}
		List<Rectangle> list = new List<Rectangle>();
		for (int j = 0; j < monitorInfosPhysical.Count; j++)
		{
			list.Add(monitorInfosPhysical[j].rect_monitor);
		}
		Rectangle union = getUnion(list);
		float num = (float)(target_size.Width - 1) / (float)union.Width;
		float num2 = (float)(target_size.Height - 1) / (float)union.Height;
		float num3 = ((num < num2) ? num : num2);
		int num4 = (int)Math.Round((float)union.Width * num3);
		int num5 = (int)Math.Round((float)union.Height * num3);
		int num6 = (target_size.Width - num4) / 2;
		int num7 = (target_size.Height - num5) / 2;
		Bitmap bitmap = new Bitmap(target_size.Width, target_size.Height);
		Graphics graphics = Graphics.FromImage(bitmap);
		graphics.Clear(Color.White);
		int count = monitorInfosPhysical.Count;
		for (int k = 0; k < count; k++)
		{
			Rectangle rect_monitor = monitorInfosPhysical[k].rect_monitor;
			int x = num6 + (int)Math.Round((float)(rect_monitor.Left - union.Left) * num3);
			int y = num7 + (int)Math.Round((float)(rect_monitor.Top - union.Top) * num3);
			int width = Math.Max(1, (int)Math.Round((float)rect_monitor.Width * num3));
			int height = Math.Max(1, (int)Math.Round((float)rect_monitor.Height * num3));
			Rectangle rect = new Rectangle(x, y, width, height);
			using (SolidBrush brush = new SolidBrush(colorFromHue((double)k * 360.0 / (double)Math.Max(1, count) % 360.0, 0.55, 0.95)))
			{
				using Pen pen = new Pen(Color.Black, 1f);
				pen.Alignment = PenAlignment.Inset;
				graphics.FillRectangle(brush, rect);
				graphics.DrawRectangle(pen, rect);
			}
			if (use_working_area)
			{
				Rectangle rect_work = monitorInfosPhysical[k].rect_work;
				int num8 = num6 + (int)Math.Round((float)(rect_work.Left - union.Left) * num3);
				int num9 = num7 + (int)Math.Round((float)(rect_work.Top - union.Top) * num3);
				int num10 = Math.Max(1, (int)Math.Round((float)rect_work.Width * num3));
				int num11 = Math.Max(1, (int)Math.Round((float)rect_work.Height * num3));
				using Pen pen2 = new Pen(Color.Black, 1f);
				pen2.DashStyle = DashStyle.Dash;
				if (rect_work.Left > rect_monitor.Left)
				{
					graphics.DrawLine(pen2, num8, num9, num8, num9 + num11);
				}
				if (rect_work.Top > rect_monitor.Top)
				{
					graphics.DrawLine(pen2, num8, num9, num8 + num10, num9);
				}
				if (rect_work.Right < rect_monitor.Right)
				{
					graphics.DrawLine(pen2, num8 + num10, num9, num8 + num10, num9 + num11);
				}
				if (rect_work.Bottom < rect_monitor.Bottom)
				{
					graphics.DrawLine(pen2, num8, num9 + num11, num8 + num10, num9 + num11);
				}
			}
			string text = ((monitorInfosPhysical[k].display_number > 0) ? monitorInfosPhysical[k].display_number : monitorInfosPhysical[k].index).ToString();
			string text2 = monitorInfosPhysical[k].scale_percent + "%";
			using StringFormat stringFormat = new StringFormat();
			stringFormat.Alignment = StringAlignment.Center;
			stringFormat.LineAlignment = StringAlignment.Near;
			float num12 = (float)rect.Left + (float)rect.Width / 2f;
			float num13 = (float)rect.Top + (float)rect.Height / 2f;
			using Font font = new Font(SystemFonts.DefaultFont.FontFamily, Math.Max(8f, (float)rect.Height * 0.2f), FontStyle.Bold, GraphicsUnit.Pixel);
			using Font font2 = new Font(SystemFonts.DefaultFont.FontFamily, Math.Max(6f, (float)rect.Height * 0.1f), FontStyle.Regular, GraphicsUnit.Pixel);
			SizeF sizeF = graphics.MeasureString(text, font);
			SizeF sizeF2 = graphics.MeasureString(text2, font2);
			float num14 = num13 - (sizeF.Height + sizeF2.Height) / 2f;
			RectangleF layoutRectangle = new RectangleF(num12 - sizeF.Width / 2f, num14, sizeF.Width, sizeF.Height);
			RectangleF layoutRectangle2 = new RectangleF(num12 - sizeF2.Width / 2f, num14 + sizeF.Height, sizeF2.Width, sizeF2.Height);
			using SolidBrush brush2 = new SolidBrush(Color.Black);
			graphics.DrawString(text, font, brush2, layoutRectangle, stringFormat);
			graphics.DrawString(text2, font2, brush2, layoutRectangle2, stringFormat);
		}
		if (forms != null)
		{
			foreach (Form form in forms)
			{
				if (form == null)
				{
					continue;
				}
				if (!tryGetExtendedFrameBounds(form, out var rect2))
				{
					rect2 = ((!tryGetWindowRectPhysical(form.Handle, out var rect3)) ? form.Bounds : Rectangle.FromLTRB(rect3.left, rect3.top, rect3.right, rect3.bottom));
				}
				int x2 = num6 + (int)Math.Round((float)(rect2.Left - union.Left) * num3);
				int y2 = num7 + (int)Math.Round((float)(rect2.Top - union.Top) * num3);
				int width2 = Math.Max(1, (int)Math.Round((float)rect2.Width * num3));
				int height2 = Math.Max(1, (int)Math.Round((float)rect2.Height * num3));
				Rectangle rect4 = new Rectangle(x2, y2, width2, height2);
				using SolidBrush brush3 = new SolidBrush(Color.FromArgb(128, 0, 200, 0));
				using Pen pen3 = new Pen(Color.Black, 2f);
				pen3.Alignment = PenAlignment.Inset;
				graphics.FillRectangle(brush3, rect4);
				graphics.DrawRectangle(pen3, rect4);
			}
		}
		graphics.Dispose();
		return bitmap;
	}

	public static void RenderScreensToPictureBox(PictureBox picture_box, IEnumerable<Form> forms = null, bool use_working_area = false)
	{
		if (picture_box != null)
		{
			Bitmap image = createScreensBitmap(picture_box.ClientSize, forms, use_working_area);
			Image image2 = picture_box.Image;
			picture_box.Image = image;
			image2?.Dispose();
		}
	}

	public static void RenderScreensToPictureBox(PictureBox picture_box, Form form = null, bool use_working_area = false)
	{
		if (picture_box != null)
		{
			if (form == null)
			{
				RenderScreensToPictureBox(picture_box, (IEnumerable<Form>)null, use_working_area);
				return;
			}
			List<Form> list = new List<Form>();
			list.Add(form);
			RenderScreensToPictureBox(picture_box, list, use_working_area);
		}
	}

	private static List<monitor_info> getMonitorInfosPhysical(bool use_working_area)
	{
		List<monitor_info> list = new List<monitor_info>();
		IntPtr intPtr = IntPtr.Zero;
		bool flag = false;
		try
		{
			intPtr = SetThreadDpiAwarenessContext(DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2);
			if (intPtr != IntPtr.Zero)
			{
				flag = true;
			}
		}
		catch
		{
			flag = false;
		}
		try
		{
			int idx = 1;
			EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, delegate(IntPtr hMon, IntPtr hdc, IntPtr lprc, IntPtr data)
			{
				MONITORINFOEX lpmi = new MONITORINFOEX
				{
					cbSize = (uint)Marshal.SizeOf(typeof(MONITORINFOEX))
				};
				if (GetMonitorInfoEx(hMon, ref lpmi))
				{
					Rectangle rect_monitor = Rectangle.FromLTRB(lpmi.rcMonitor.left, lpmi.rcMonitor.top, lpmi.rcMonitor.right, lpmi.rcMonitor.bottom);
					Rectangle rect_work = Rectangle.FromLTRB(lpmi.rcWork.left, lpmi.rcWork.top, lpmi.rcWork.right, lpmi.rcWork.bottom);
					uint dpiX = 96u;
					uint dpiY = 96u;
					int scale_percent = 100;
					try
					{
						if (GetDpiForMonitor(hMon, MONITOR_DPI_TYPE.MDT_EFFECTIVE_DPI, out dpiX, out dpiY) == 0)
						{
							scale_percent = (int)Math.Round((double)dpiX / 96.0 * 100.0);
						}
					}
					catch
					{
						scale_percent = 100;
					}
					int num = parseDisplayNumber((lpmi.szDevice == null) ? "" : lpmi.szDevice.TrimEnd(default(char)));
					if (num <= 0)
					{
						num = idx;
					}
					monitor_info item = new monitor_info
					{
						rect_monitor = rect_monitor,
						rect_work = rect_work,
						scale_percent = scale_percent,
						index = idx,
						display_number = num
					};
					list.Add(item);
					idx++;
				}
				return true;
			}, IntPtr.Zero);
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
		return list;
	}

	private static int parseDisplayNumber(string device)
	{
		if (string.IsNullOrEmpty(device))
		{
			return -1;
		}
		int num = device.Length - 1;
		while (num >= 0 && char.IsDigit(device[num]))
		{
			num--;
		}
		if (num == device.Length - 1)
		{
			return -1;
		}
		if (int.TryParse(device.Substring(num + 1), out var result))
		{
			return result;
		}
		return -1;
	}
}
