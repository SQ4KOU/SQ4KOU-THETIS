using System;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace Thetis;

public static class ControlExtentions
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string GetFullName(this Control control)
	{
		if (control == null)
		{
			throw new ArgumentNullException("control");
		}
		if (control.Parent == null)
		{
			return control.Name;
		}
		return control.Parent.GetFullName() + "." + control.Name;
	}
}
