using System.Windows.Forms;

namespace RawInput_dll;

public class PreMessageFilter : IMessageFilter
{
	private bool m_bIgnoreNextWheelEvent;

	public bool IgnoreNextWheelEvent
	{
		get
		{
			return m_bIgnoreNextWheelEvent;
		}
		set
		{
			m_bIgnoreNextWheelEvent = value;
		}
	}

	public bool PreFilterMessage(ref Message m)
	{
		if (m.Msg == 522 && m_bIgnoreNextWheelEvent)
		{
			m_bIgnoreNextWheelEvent = false;
			return true;
		}
		return false;
	}
}
