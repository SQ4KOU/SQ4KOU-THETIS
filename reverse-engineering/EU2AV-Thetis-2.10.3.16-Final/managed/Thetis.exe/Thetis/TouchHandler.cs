using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Thetis;

internal static class TouchHandler
{
	private class ControlTouchInfo
	{
		public IntPtr handler;

		public Action<int, int> TouchDown;

		public Action<int, int> TouchMove;

		public Action<int, int> TouchUp;

		public int touch_mask;
	}

	private delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

	private struct TOUCHINPUT
	{
		public int x;

		public int y;

		public IntPtr hSource;

		public int dwID;

		public int dwFlags;

		public int dwMask;

		public int dwTime;

		public IntPtr dwExtraInfo;

		public int cxContact;

		public int cyContact;
	}

	private struct POINT
	{
		public int x;

		public int y;
	}

	private const int WM_TOUCH = 576;

	public const int TOUCHEVENTF_UP = 4;

	public const int TOUCHEVENTF_DOWN = 2;

	public const int TOUCHEVENTF_MOVE = 1;

	private const uint MW_MOUSEFIRST = 512u;

	private const uint MW_MOUSELAST = 525u;

	private static DateTime _last_touch_time = DateTime.MinValue;

	private static bool _is_touch_active = false;

	private static readonly Dictionary<Guid, ControlTouchInfo> controlInfoMap = new Dictionary<Guid, ControlTouchInfo>();

	private static readonly Dictionary<IntPtr, IntPtr> originalWndProcs = new Dictionary<IntPtr, IntPtr>();

	private static readonly WndProcDelegate customWndProcDelegate = CustomWndProc;

	private static readonly object _locker = new object();

	[DllImport("user32.dll")]
	private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

	[DllImport("user32.dll")]
	private static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

	[DllImport("user32.dll")]
	private static extern bool GetTouchInputInfo(IntPtr hTouchInput, int cInputs, [Out] TOUCHINPUT[] pInputs, int cbSize);

	[DllImport("user32.dll")]
	private static extern void CloseTouchInputHandle(IntPtr lParam);

	[DllImport("user32.dll")]
	private static extern bool RegisterTouchWindow(IntPtr hWnd, uint ulFlags);

	[DllImport("user32.dll")]
	private static extern bool UnregisterTouchWindow(IntPtr hWnd);

	[DllImport("user32.dll")]
	private static extern bool ScreenToClient(IntPtr hWnd, ref POINT lpPoint);

	public static Guid EnableTouchSupport(Control control, Action<int, int> touchDown, Action<int, int> touchMove, Action<int, int> touchUp, int touch_mask, string id = "")
	{
		lock (_locker)
		{
			IntPtr handle = control.Handle;
			if (!originalWndProcs.ContainsKey(handle))
			{
				IntPtr functionPointerForDelegate = Marshal.GetFunctionPointerForDelegate(customWndProcDelegate);
				IntPtr value = SetWindowLongPtr(handle, -4, functionPointerForDelegate);
				originalWndProcs[handle] = value;
				RegisterTouchWindow(handle, 0u);
			}
			Guid result;
			if (string.IsNullOrEmpty(id))
			{
				result = Guid.NewGuid();
			}
			else
			{
				Guid.TryParse(id, out result);
			}
			controlInfoMap[result] = new ControlTouchInfo
			{
				handler = handle,
				TouchDown = touchDown,
				TouchMove = touchMove,
				TouchUp = touchUp,
				touch_mask = touch_mask
			};
			return result;
		}
	}

	public static void DisableTouchSupport(Guid id)
	{
		lock (_locker)
		{
			if (controlInfoMap.TryGetValue(id, out var value))
			{
				IntPtr handler = value.handler;
				if (originalWndProcs.ContainsKey(handler))
				{
					SetWindowLongPtr(handler, -4, originalWndProcs[handler]);
					originalWndProcs.Remove(handler);
				}
				UnregisterTouchWindow(handler);
				controlInfoMap.Remove(id);
			}
		}
	}

	private static IntPtr CustomWndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
	{
		switch (msg)
		{
		case 576u:
		{
			bool flag = false;
			int num = wParam.ToInt32() & 0xFFFF;
			TOUCHINPUT[] array = new TOUCHINPUT[num];
			if (GetTouchInputInfo(lParam, num, array, Marshal.SizeOf(typeof(TOUCHINPUT))))
			{
				flag = true;
			}
			CloseTouchInputHandle(lParam);
			bool flag2 = false;
			if (flag)
			{
				lock (_locker)
				{
					foreach (KeyValuePair<Guid, ControlTouchInfo> item in controlInfoMap)
					{
						if (!(item.Value.handler == hWnd))
						{
							continue;
						}
						ControlTouchInfo value = item.Value;
						for (int i = 0; i < array.Length; i++)
						{
							POINT lpPoint = new POINT
							{
								x = array[i].x / 100,
								y = array[i].y / 100
							};
							ScreenToClient(hWnd, ref lpPoint);
							int x = lpPoint.x;
							int y = lpPoint.y;
							if ((value.touch_mask & 2) != 0 && (array[i].dwFlags & 2) != 0 && value.TouchDown != null)
							{
								value.TouchDown(x, y);
								flag2 = true;
							}
							else if ((value.touch_mask & 1) != 0 && (array[i].dwFlags & 1) != 0 && value.TouchMove != null)
							{
								value.TouchMove(x, y);
								flag2 = true;
							}
							else if ((value.touch_mask & 4) != 0 && (array[i].dwFlags & 4) != 0 && value.TouchUp != null)
							{
								value.TouchUp(x, y);
								flag2 = true;
							}
						}
					}
				}
			}
			if (flag2)
			{
				_is_touch_active = true;
				_last_touch_time = DateTime.UtcNow;
				return new IntPtr(1);
			}
			break;
		}
		case 512u:
		case 513u:
		case 514u:
		case 515u:
		case 516u:
		case 517u:
		case 518u:
		case 519u:
		case 520u:
		case 521u:
		case 522u:
		case 523u:
		case 524u:
		case 525u:
			lock (_locker)
			{
				if (_is_touch_active && (DateTime.UtcNow - _last_touch_time).TotalMilliseconds <= 250.0)
				{
					return IntPtr.Zero;
				}
				_is_touch_active = false;
			}
			break;
		}
		if (originalWndProcs.ContainsKey(hWnd))
		{
			lock (_locker)
			{
				return CallWindowProc(originalWndProcs[hWnd], hWnd, msg, wParam, lParam);
			}
		}
		return IntPtr.Zero;
	}
}
