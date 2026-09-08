using System;
using System.Drawing;
using System.Windows.Forms;

namespace Thetis;

internal class QuickRecallListBox : ListBox
{
	private int m_nHighlighted = -1;

	private int m_nFrequencyMHzWidth = -1;

	private int m_nFontHeight = -1;

	public int FontEntryHeight => m_nFontHeight;

	public QuickRecallListBox()
	{
		SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.OptimizedDoubleBuffer, value: true);
		DrawMode = DrawMode.OwnerDrawFixed;
	}

	public void ClearItems()
	{
		m_nFrequencyMHzWidth = -1;
		base.Items.Clear();
		Invalidate();
	}

	public int AddItem(double dFreq)
	{
		dFreq = Math.Round(dFreq, 6);
		int num = TextRenderer.MeasureText(((int)dFreq).ToString(), Font).Width;
		if (num > m_nFrequencyMHzWidth)
		{
			m_nFrequencyMHzWidth = num;
		}
		return base.Items.Add(dFreq);
	}

	protected override void OnDrawItem(DrawItemEventArgs e)
	{
		if (base.Items.Count > 0 && e.Index >= 0)
		{
			e.DrawBackground();
			if (double.TryParse(base.Items[e.Index].ToString(), out var result))
			{
				SolidBrush solidBrush = new SolidBrush(ForeColor);
				int num = (int)result;
				string obj = (result - (double)num).ToString("f6");
				string text = obj.Substring(2, 3);
				string text2 = obj.Substring(5, 3);
				Size size = TextRenderer.MeasureText(num.ToString(), Font);
				int num2 = m_nFrequencyMHzWidth - size.Width;
				Rectangle rectangle = new Rectangle(num2, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height);
				e.Graphics.DrawString(num.ToString(), e.Font, solidBrush, rectangle, StringFormat.GenericDefault);
				rectangle.X = m_nFrequencyMHzWidth - 8;
				e.Graphics.DrawString("." + text + "." + text2, e.Font, solidBrush, rectangle, StringFormat.GenericDefault);
				solidBrush.Dispose();
			}
		}
		base.OnDrawItem(e);
	}

	protected override void OnFontChanged(EventArgs e)
	{
		Size size = TextRenderer.MeasureText("0", Font);
		ItemHeight = size.Height;
		m_nFontHeight = size.Height;
		base.OnFontChanged(e);
	}

	protected override void OnSelectedIndexChanged(EventArgs e)
	{
		Invalidate();
		base.OnSelectedIndexChanged(e);
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
