using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace Thetis;

public class ucVARGrapher : UserControl
{
	private const double m_MIN = 5E-06;

	private List<double> m_dData;

	private int m_nMaxPoints;

	private double m_dPlusMinusSwing = 0.04;

	private double m_dAutoSwing = 0.04;

	private bool m_bAutoSwing;

	private double m_dRingBufferPerc;

	private string m_sCaption = "";

	private IContainer components;

	public string Caption
	{
		get
		{
			return m_sCaption;
		}
		set
		{
			m_sCaption = value;
			Invalidate();
		}
	}

	public double RingBufferPerc
	{
		get
		{
			return m_dRingBufferPerc;
		}
		set
		{
			m_dRingBufferPerc = value;
			Invalidate();
		}
	}

	public bool AutoSwing
	{
		get
		{
			return m_bAutoSwing;
		}
		set
		{
			m_bAutoSwing = value;
			Invalidate();
		}
	}

	public double PlusMinusSwing
	{
		get
		{
			return m_dPlusMinusSwing;
		}
		set
		{
			m_dPlusMinusSwing = value;
			Invalidate();
		}
	}

	public int MaxPoints
	{
		get
		{
			return m_nMaxPoints;
		}
		set
		{
			if (m_dData != null)
			{
				m_nMaxPoints = value;
				if (m_dData.Count > m_nMaxPoints)
				{
					m_dData.RemoveRange(0, m_dData.Count - m_nMaxPoints - 1);
					Invalidate();
				}
			}
		}
	}

	public ucVARGrapher()
	{
		m_dData = new List<double>();
		InitializeComponent();
		Common.DoubleBufferAll(this, enabled: true);
		MaxPoints = 100;
	}

	public void AddDataPoint(double dataPoint)
	{
		if (m_dData != null)
		{
			m_dData.Add(Math.Round(dataPoint, 6));
			if (m_dData.Count > m_nMaxPoints)
			{
				m_dData.RemoveAt(0);
			}
			double val = Math.Abs(m_dData.Min());
			double val2 = Math.Abs(m_dData.Max());
			m_dAutoSwing = Math.Max(val, val2);
			Invalidate();
		}
	}

	private void VARGraph_Paint(object sender, PaintEventArgs e)
	{
		if (m_dData != null && m_dData.Count >= 2)
		{
			e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
			int num = base.Height / 2;
			e.Graphics.DrawLine(Pens.Gray, 0, num, base.Width, num);
			double num2 = (m_bAutoSwing ? m_dAutoSwing : m_dPlusMinusSwing);
			if (num2 < 5E-06)
			{
				num2 = 5E-06;
			}
			string text = num2.ToString("F6");
			using (StringFormat stringFormat = new StringFormat())
			{
				stringFormat.Alignment = StringAlignment.Far;
				SizeF sizeF = e.Graphics.MeasureString("±" + text, Font);
				e.Graphics.DrawString("±" + text, Font, Brushes.White, new PointF(base.Width, 0f), stringFormat);
				e.Graphics.DrawString(m_sCaption, Font, Brushes.White, new PointF(base.Width, (float)base.Height - sizeF.Height - 1f), stringFormat);
			}
			double num3 = (double)base.Height / (num2 * 2.0);
			Point[] array = new Point[m_dData.Count];
			for (int i = 0; i < m_dData.Count; i++)
			{
				int num4 = (int)(m_dData[i] * num3) + num;
				array[i] = new Point(i, base.Height - 1 - num4);
			}
			e.Graphics.DrawLines(Pens.Red, array);
			int num5 = base.Height - 1 - (int)((double)(base.Height - 1) * (m_dRingBufferPerc / 100.0));
			e.Graphics.DrawLine(Pens.Yellow, 0, num5, 10, num5);
		}
	}

	private void VARGraph_Resize(object sender, EventArgs e)
	{
		MaxPoints = base.Width;
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
		this.BackColor = System.Drawing.SystemColors.Control;
		this.DoubleBuffered = true;
		base.Name = "ucVARGrapher";
		base.Paint += new System.Windows.Forms.PaintEventHandler(VARGraph_Paint);
		base.Resize += new System.EventHandler(VARGraph_Resize);
		base.ResumeLayout(false);
	}
}
