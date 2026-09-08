using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Thetis;

public class ucOCLedStrip : UserControl
{
	private bool m_bTX;

	private int m_nBits;

	private IContainer components;

	public bool TX
	{
		get
		{
			return m_bTX;
		}
		set
		{
			m_bTX = value;
			Invalidate();
		}
	}

	public int Bits
	{
		get
		{
			return m_nBits;
		}
		set
		{
			m_nBits = value;
			Invalidate();
		}
	}

	public ucOCLedStrip()
	{
		InitializeComponent();
	}

	private void usOCLedStrip_Paint(object sender, PaintEventArgs e)
	{
		Graphics graphics = e.Graphics;
		for (int i = 0; i < 7; i++)
		{
			int num = i * 16;
			Rectangle rect = new Rectangle(num, 0, 15, base.Height - 1);
			Brush brush = (((m_nBits & (1 << i)) == 0) ? Brushes.Gray : ((!m_bTX) ? Brushes.GreenYellow : Brushes.OrangeRed));
			graphics.FillRectangle(brush, rect);
			graphics.DrawRectangle(Pens.Black, rect);
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		base.SuspendLayout();
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.DoubleBuffered = true;
		base.Name = "ucOCLedStrip";
		base.Size = new System.Drawing.Size(333, 38);
		base.Paint += new System.Windows.Forms.PaintEventHandler(usOCLedStrip_Paint);
		base.ResumeLayout(false);
	}
}
