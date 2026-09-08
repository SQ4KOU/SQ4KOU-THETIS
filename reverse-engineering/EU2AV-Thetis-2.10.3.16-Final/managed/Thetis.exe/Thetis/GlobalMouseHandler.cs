using System.Windows.Forms;

namespace Thetis;

public class GlobalMouseHandler : IMessageFilter
{
	private const int WM_MOUSEMOVE = 512;

	private const int WM_LBUTTONUP = 514;

	public event MouseMovedEvent MouseUp;

	public event MouseEventHandler MouseMove;

	public bool PreFilterMessage(ref Message m)
	{
		if (m.Msg == 512)
		{
			MouseMove?.Invoke(null, new MouseEventArgs(MouseButtons.None, 0, Control.MousePosition.X, Control.MousePosition.Y, 0));
		}
		if (m.Msg == 514 && MouseUp != null)
		{
			MouseUp();
		}
		return false;
	}
}
