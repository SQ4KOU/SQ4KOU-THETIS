using System;
using System.Drawing;
using System.Windows.Forms;

namespace Thetis;

internal class BandStackListBox : ListBox
{
	private int m_nHighlighted = -1;

	private int m_nNumberWidth = -1;

	private int m_nFrequencyMHzWidth = -1;

	private Font m_SmallModeFont;

	private Image m_LockLockedImage;

	private Image m_LockUnlockedImage;

	private Image m_MemoryImage;

	private Image m_SpecificReturnImage;

	public int m_specficReturnIndex = -1;

	public Image LockImageLocked
	{
		get
		{
			return m_LockLockedImage;
		}
		set
		{
			m_LockLockedImage = value;
		}
	}

	public Image LockImageUnLocked
	{
		get
		{
			return m_LockUnlockedImage;
		}
		set
		{
			m_LockUnlockedImage = value;
		}
	}

	public Image Memory
	{
		get
		{
			return m_MemoryImage;
		}
		set
		{
			m_MemoryImage = value;
		}
	}

	public Image SpecificReturnImage
	{
		get
		{
			return m_SpecificReturnImage;
		}
		set
		{
			m_SpecificReturnImage = value;
		}
	}

	public int SpecificReturnIndex
	{
		get
		{
			return m_specficReturnIndex;
		}
		set
		{
			m_specficReturnIndex = value;
		}
	}

	public BandStackListBox()
	{
		SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.OptimizedDoubleBuffer, value: true);
		DrawMode = DrawMode.OwnerDrawFixed;
	}

	public int AddItem(BandStackEntry bse)
	{
		m_nNumberWidth = TextRenderer.MeasureText(base.Items.Count + ") ", m_SmallModeFont).Width;
		int num = TextRenderer.MeasureText(((int)bse.Frequency).ToString(), Font).Width;
		if (num > m_nFrequencyMHzWidth)
		{
			m_nFrequencyMHzWidth = num;
		}
		return base.Items.Add(bse);
	}

	public void ClearItems()
	{
		m_nFrequencyMHzWidth = -1;
		m_nNumberWidth = -1;
		base.Items.Clear();
		Invalidate();
	}

	protected override void OnFontChanged(EventArgs e)
	{
		ItemHeight = TextRenderer.MeasureText("0", Font).Height;
		m_SmallModeFont = new Font(Font.FontFamily, Font.Size * 0.6f, FontStyle.Regular);
		base.OnFontChanged(e);
	}

	protected override void OnSelectedIndexChanged(EventArgs e)
	{
		Invalidate();
		base.OnSelectedIndexChanged(e);
	}

	protected override void OnDrawItem(DrawItemEventArgs e)
	{
		e.DrawBackground();
		if (base.Items.Count > 0 && e.Index >= 0)
		{
			if (base.Items[e.Index] is BandStackEntry bandStackEntry)
			{
				if (bandStackEntry.Locked)
				{
					e.Graphics.DrawImage(m_LockLockedImage, new Point(e.Bounds.Width - 20, e.Bounds.Y + e.Bounds.Height / 2 - m_LockLockedImage.Height / 2));
				}
				else
				{
					e.Graphics.DrawImage(m_LockUnlockedImage, new Point(e.Bounds.Width - 20, e.Bounds.Y + e.Bounds.Height / 2 - m_LockLockedImage.Height / 2));
				}
				if (e.Index == m_specficReturnIndex)
				{
					e.Graphics.DrawImage(m_SpecificReturnImage, new Point(e.Bounds.Width - 30, e.Bounds.Y + e.Bounds.Height / 2 - m_SpecificReturnImage.Height / 2));
				}
				SolidBrush solidBrush = new SolidBrush(ForeColor);
				if (m_SmallModeFont != null)
				{
					string s = e.Index + 1 + ") ";
					Size size = TextRenderer.MeasureText(s, m_SmallModeFont);
					e.Graphics.DrawString(s, m_SmallModeFont, solidBrush, 0f, e.Bounds.Y + e.Bounds.Height / 2 - size.Height / 2, StringFormat.GenericDefault);
				}
				int num = (int)bandStackEntry.Frequency;
				string obj = (bandStackEntry.Frequency - (double)num).ToString("f6");
				string text = obj.Substring(2, 3);
				string text2 = obj.Substring(5, 3);
				Size size2 = TextRenderer.MeasureText(num.ToString(), Font);
				int num2 = m_nNumberWidth + m_nFrequencyMHzWidth - size2.Width;
				Rectangle rectangle = new Rectangle(num2, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height);
				e.Graphics.DrawString(num.ToString(), e.Font, solidBrush, rectangle, StringFormat.GenericDefault);
				rectangle.X = m_nNumberWidth + m_nFrequencyMHzWidth - 8;
				e.Graphics.DrawString("." + text + "." + text2, e.Font, solidBrush, rectangle, StringFormat.GenericDefault);
				if (m_SmallModeFont != null)
				{
					Size size3 = TextRenderer.MeasureText(bandStackEntry.Mode.ToString(), m_SmallModeFont);
					e.Graphics.DrawString(bandStackEntry.Mode.ToString(), m_SmallModeFont, solidBrush, e.Bounds.Width - size3.Width - 24, e.Bounds.Y + (e.Bounds.Height / 2 - size3.Height / 2), StringFormat.GenericDefault);
				}
				solidBrush.Dispose();
			}
			else
			{
				e.DrawBackground();
			}
		}
		base.OnDrawItem(e);
	}

	protected override void OnMouseEnter(EventArgs e)
	{
		base.OnMouseEnter(e);
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		m_nHighlighted = -1;
		Invalidate();
		base.OnMouseLeave(e);
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		Point p = new Point(e.X, e.Y);
		int num = IndexFromPoint(p);
		if (num != -1 && num != m_nHighlighted)
		{
			m_nHighlighted = num;
			Invalidate();
		}
		base.OnMouseMove(e);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		base.OnPaint(e);
		Region region = new Region(e.ClipRectangle);
		SolidBrush brush = new SolidBrush(BackColor);
		e.Graphics.FillRegion(brush, region);
		if (base.Items.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < base.Items.Count; i++)
		{
			Rectangle itemRectangle = GetItemRectangle(i);
			if (e.ClipRectangle.IntersectsWith(itemRectangle))
			{
				if ((SelectionMode == SelectionMode.One && SelectedIndex == i) || (SelectionMode == SelectionMode.MultiSimple && base.SelectedIndices.Contains(i)) || (SelectionMode == SelectionMode.MultiExtended && base.SelectedIndices.Contains(i)))
				{
					OnDrawItem(new DrawItemEventArgs(e.Graphics, Font, itemRectangle, i, DrawItemState.Selected, ForeColor, BackColor));
				}
				else if (m_nHighlighted == i)
				{
					OnDrawItem(new DrawItemEventArgs(e.Graphics, Font, itemRectangle, i, DrawItemState.HotLight, ForeColor, Color.Silver));
				}
				else
				{
					OnDrawItem(new DrawItemEventArgs(e.Graphics, Font, itemRectangle, i, DrawItemState.Default, ForeColor, BackColor));
				}
				region.Complement(itemRectangle);
			}
		}
	}
}
