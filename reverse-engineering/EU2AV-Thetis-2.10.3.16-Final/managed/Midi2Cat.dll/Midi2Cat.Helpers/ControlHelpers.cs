using System;
using System.ComponentModel;

namespace Midi2Cat.Helpers;

public static class ControlHelpers
{
	public static void InvokeIfRequired<T>(this T control, Action<T> action) where T : ISynchronizeInvoke
	{
		if (control.InvokeRequired)
		{
			control.Invoke((Action)delegate
			{
				action(control);
			}, null);
		}
		else
		{
			action(control);
		}
	}
}
