using System.Drawing;
using System.Windows.Forms;

namespace Thetis;

public class UI
{
	public delegate void SetCtrlDel(Control c, object val);

	public static void SetPanel(Control c, object val)
	{
		Panel obj = (Panel)c;
		Color backColor = (Color)val;
		obj.BackColor = backColor;
	}
}
