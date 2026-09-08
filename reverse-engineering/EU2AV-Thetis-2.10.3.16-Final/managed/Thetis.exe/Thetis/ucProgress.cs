using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Thetis;

public class ucProgress : UserControl
{
	private int _value;

	private int _min;

	private int _max = 100;

	private int _vertical_line_value = -1;

	private int _vertical_line_width = 1;

	private Color _bar_color = Color.Green;

	private Color _veritical_line_color = Color.Red;

	private IContainer components;

	[Browsable(true)]
	[DefaultValue(0)]
	public int Value
	{
		get
		{
			return _value;
		}
		set
		{
			_value = Math.Max(_min, Math.Min(_max, value));
			Refresh();
		}
	}

	[Browsable(true)]
	[DefaultValue(100)]
	public int Maximum
	{
		get
		{
			return _max;
		}
		set
		{
			_max = value;
			_value = Math.Max(_min, Math.Min(_max, _value));
			Refresh();
		}
	}

	[Browsable(true)]
	[DefaultValue(0)]
	public int Minimum
	{
		get
		{
			return _min;
		}
		set
		{
			_min = value;
			_value = Math.Max(_min, Math.Min(_max, _value));
			Refresh();
		}
	}

	[Browsable(true)]
	[DefaultValue(-1)]
	public int VerticalLineValue
	{
		get
		{
			return _vertical_line_value;
		}
		set
		{
			_vertical_line_value = value;
			Refresh();
		}
	}

	[Browsable(true)]
	[DefaultValue(1)]
	public int VerticalLineWidth
	{
		get
		{
			return _vertical_line_width;
		}
		set
		{
			_vertical_line_width = Math.Max(1, value);
			Refresh();
		}
	}

	[Browsable(true)]
	[DefaultValue(typeof(Color), "Green")]
	public Color BarColor
	{
		get
		{
			return _bar_color;
		}
		set
		{
			_bar_color = value;
			Refresh();
		}
	}

	[Browsable(true)]
	[DefaultValue(typeof(Color), "Red")]
	public Color VeriticalLineColor
	{
		get
		{
			return _veritical_line_color;
		}
		set
		{
			_veritical_line_color = value;
			Refresh();
		}
	}

	public ucProgress()
	{
		InitializeComponent();
		DoubleBuffered = true;
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		base.OnPaint(e);
		int num = _max - _min;
		using Pen pen = new Pen(Color.Black, 1f);
		if (num <= 0 || base.Width <= 0 || base.Height <= 0)
		{
			e.Graphics.DrawRectangle(pen, 0, 0, base.Width - 1, base.Height - 1);
			return;
		}
		double num2 = (double)(_value - _min) / (double)num;
		int num3 = (int)Math.Round((double)base.Width * num2);
		if (num3 < 0)
		{
			num3 = 0;
		}
		if (num3 > base.Width)
		{
			num3 = base.Width;
		}
		using (Brush brush = new SolidBrush(_bar_color))
		{
			e.Graphics.FillRectangle(brush, 0, 0, num3, base.Height);
		}
		if (_vertical_line_value >= _min && _vertical_line_value <= _max)
		{
			double num4 = (double)(_vertical_line_value - _min) / (double)num;
			int num5 = (int)Math.Round((double)(base.Width - 1) * num4);
			if (num5 < 0)
			{
				num5 = 0;
			}
			if (num5 > base.Width - 1)
			{
				num5 = base.Width - 1;
			}
			using Pen pen2 = new Pen(_veritical_line_color, _vertical_line_width);
			e.Graphics.DrawLine(pen2, num5, 0, num5, base.Height - 1);
		}
		e.Graphics.DrawRectangle(pen, 0, 0, base.Width - 1, base.Height - 1);
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
		base.Name = "ucProgress";
		base.Size = new System.Drawing.Size(341, 87);
		base.ResumeLayout(false);
	}
}
