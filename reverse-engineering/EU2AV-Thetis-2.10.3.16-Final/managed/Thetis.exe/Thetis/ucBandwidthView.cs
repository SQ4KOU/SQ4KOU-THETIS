using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Thetis;

public class ucBandwidthView : UserControl
{
	public enum BandwidthUnits
	{
		KBps,
		Mbitps
	}

	private BandwidthUnits _display_units;

	private bool _show_grid;

	private int _history_seconds;

	private double[] _in_bps;

	private double[] _out_bps;

	private double[] _tot_bps;

	private int _head;

	private int _count;

	private double _scale_max_display;

	private int _scale_hold_ticks;

	private double _last_in_bps;

	private double _last_out_bps;

	private double _last_tot_bps;

	private bool _enable_smoothing;

	private double _smoothing_factor;

	private double _smooth_in;

	private double _smooth_out;

	private double _smooth_tot;

	private bool _smoothing_ready;

	private IContainer components;

	public BandwidthUnits DisplayUnits
	{
		get
		{
			return _display_units;
		}
		set
		{
			if (_display_units != value)
			{
				_display_units = value;
				_scale_max_display = 1.0;
				_scale_hold_ticks = 0;
				updateScale();
				Invalidate();
			}
		}
	}

	public bool ShowGrid
	{
		get
		{
			return _show_grid;
		}
		set
		{
			if (_show_grid != value)
			{
				_show_grid = value;
				Invalidate();
			}
		}
	}

	public int HistorySeconds
	{
		get
		{
			return _history_seconds;
		}
		set
		{
			int num = value;
			if (num < 10)
			{
				num = 10;
			}
			if (num > 600)
			{
				num = 600;
			}
			if (_history_seconds != num)
			{
				_history_seconds = num;
				resizeBuffers(_history_seconds);
				Invalidate();
			}
		}
	}

	public bool EnableSmoothing
	{
		get
		{
			return _enable_smoothing;
		}
		set
		{
			if (_enable_smoothing != value)
			{
				_enable_smoothing = value;
				resetSmoothing();
				Invalidate();
			}
		}
	}

	public double SmoothingFactor
	{
		get
		{
			return _smoothing_factor;
		}
		set
		{
			double num = value;
			if (num < 0.01)
			{
				num = 0.01;
			}
			if (num > 1.0)
			{
				num = 1.0;
			}
			if (!(Math.Abs(_smoothing_factor - num) < 1E-07))
			{
				_smoothing_factor = num;
				resetSmoothing();
				Invalidate();
			}
		}
	}

	public ucBandwidthView()
	{
		SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, value: true);
		BackColor = Color.Black;
		ForeColor = Color.White;
		Font = new Font("Segoe UI", 9f, FontStyle.Bold, GraphicsUnit.Point);
		_display_units = BandwidthUnits.Mbitps;
		_history_seconds = 60;
		_show_grid = true;
		_enable_smoothing = false;
		_smoothing_factor = 0.5;
		resizeBuffers(_history_seconds);
		resetSmoothing();
	}

	public void Reset()
	{
		Array.Clear(_in_bps, 0, _in_bps.Length);
		Array.Clear(_out_bps, 0, _out_bps.Length);
		Array.Clear(_tot_bps, 0, _tot_bps.Length);
		_head = 0;
		_count = 0;
		_scale_max_display = 1.0;
		_scale_hold_ticks = 0;
		_last_in_bps = 0.0;
		_last_out_bps = 0.0;
		_last_tot_bps = 0.0;
		resetSmoothing();
		updateScale();
		Invalidate();
	}

	public void PushSample(double inbound_bps, double outbound_bps)
	{
		if (inbound_bps < 0.0)
		{
			inbound_bps = 0.0;
		}
		if (outbound_bps < 0.0)
		{
			outbound_bps = 0.0;
		}
		double num = inbound_bps + outbound_bps;
		if (_enable_smoothing)
		{
			if (!_smoothing_ready)
			{
				_smooth_in = inbound_bps;
				_smooth_out = outbound_bps;
				_smooth_tot = num;
				_smoothing_ready = true;
			}
			else
			{
				double smoothing_factor = _smoothing_factor;
				_smooth_in = _smooth_in * (1.0 - smoothing_factor) + inbound_bps * smoothing_factor;
				_smooth_out = _smooth_out * (1.0 - smoothing_factor) + outbound_bps * smoothing_factor;
				_smooth_tot = _smooth_tot * (1.0 - smoothing_factor) + num * smoothing_factor;
			}
			inbound_bps = _smooth_in;
			outbound_bps = _smooth_out;
			num = _smooth_tot;
		}
		_in_bps[_head] = inbound_bps;
		_out_bps[_head] = outbound_bps;
		_tot_bps[_head] = num;
		_last_in_bps = inbound_bps;
		_last_out_bps = outbound_bps;
		_last_tot_bps = num;
		_head++;
		if (_head >= _in_bps.Length)
		{
			_head = 0;
		}
		if (_count < _in_bps.Length)
		{
			_count++;
		}
		updateScale();
		Invalidate();
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		Graphics graphics = e.Graphics;
		graphics.SmoothingMode = SmoothingMode.AntiAlias;
		graphics.Clear(BackColor);
		Rectangle clientRectangle = base.ClientRectangle;
		if (clientRectangle.Width < 10 || clientRectangle.Height < 10)
		{
			return;
		}
		int num = 68;
		int num2 = 10;
		int num3 = 8;
		int num4 = 18;
		Rectangle plot = new Rectangle(clientRectangle.Left + num, clientRectangle.Top + num3, clientRectangle.Width - num - num2, clientRectangle.Height - num3 - num4);
		if (plot.Width > 5 && plot.Height > 5)
		{
			double num5 = _scale_max_display;
			if (num5 < 1E-06)
			{
				num5 = 1.0;
			}
			if (_show_grid)
			{
				drawGrid(graphics, plot);
			}
			drawAxisLeft(graphics, plot, num5);
			drawLine(graphics, plot, _in_bps, _count, _head, num5, Color.DeepSkyBlue, 1.6f);
			drawLine(graphics, plot, _out_bps, _count, _head, num5, Color.Orange, 1.6f);
			drawLine(graphics, plot, _tot_bps, _count, _head, num5, Color.LimeGreen, 2f);
			drawOverlay(graphics, plot);
		}
	}

	private void drawGrid(Graphics g, Rectangle plot)
	{
		using (Pen pen = new Pen(Color.FromArgb(35, 255, 255, 255), 1f))
		{
			pen.DashStyle = DashStyle.Solid;
			int num = 6;
			int num2 = 4;
			for (int i = 1; i < num; i++)
			{
				float num3 = (float)plot.Left + (float)(plot.Width * i) / (float)num;
				g.DrawLine(pen, num3, plot.Top, num3, plot.Bottom);
			}
			for (int j = 1; j < num2; j++)
			{
				float num4 = (float)plot.Top + (float)(plot.Height * j) / (float)num2;
				g.DrawLine(pen, plot.Left, num4, plot.Right, num4);
			}
		}
		using Pen pen2 = new Pen(Color.FromArgb(90, 255, 255, 255), 1f);
		g.DrawRectangle(pen2, plot);
	}

	private void drawAxisLeft(Graphics g, Rectangle plot, double max_display)
	{
		using Brush brush = new SolidBrush(ForeColor);
		using StringFormat stringFormat = new StringFormat();
		stringFormat.Alignment = StringAlignment.Far;
		stringFormat.LineAlignment = StringAlignment.Center;
		int num = 4;
		for (int i = 0; i <= num; i++)
		{
			double value_display = max_display * (double)(num - i) / (double)num;
			string s = formatAxisValue(value_display);
			float num2 = (float)plot.Top + (float)(plot.Height * i) / (float)num;
			g.DrawString(layoutRectangle: new RectangleF(0f, num2 - 10f, plot.Left - 6, 20f), s: s, font: Font, brush: brush, format: stringFormat);
		}
		using Brush brush2 = new SolidBrush(Color.FromArgb(200, 255, 255, 255));
		string s2 = ((_display_units == BandwidthUnits.KBps) ? "kB/s" : "Mbit/s");
		GraphicsState gstate = g.Save();
		float dx = 12f;
		float dy = (float)plot.Top + (float)plot.Height * 0.5f;
		g.TranslateTransform(dx, dy);
		g.RotateTransform(-90f);
		SizeF sizeF = g.MeasureString(s2, Font);
		g.DrawString(s2, Font, brush2, (0f - sizeF.Width) * 0.5f, (0f - sizeF.Height) * 0.5f);
		g.Restore(gstate);
	}

	private void drawLine(Graphics g, Rectangle plot, double[] buf, int count, int head, double max_display, Color color, float width)
	{
		if (count < 2)
		{
			return;
		}
		int num = Math.Min(count, buf.Length);
		float num2 = (float)plot.Width / (float)(buf.Length - 1);
		PointF[] array = new PointF[num];
		int num3 = head - num;
		if (num3 < 0)
		{
			num3 += buf.Length;
		}
		for (int i = 0; i < num; i++)
		{
			int num4 = num3 + i;
			if (num4 >= buf.Length)
			{
				num4 -= buf.Length;
			}
			double num5 = toDisplayUnits(buf[num4]);
			if (num5 < 0.0)
			{
				num5 = 0.0;
			}
			float num6 = (float)plot.Left + num2 * (float)i;
			float num7 = (float)plot.Bottom - (float)(num5 / max_display * (double)plot.Height);
			if (num7 < (float)plot.Top)
			{
				num7 = plot.Top;
			}
			if (num7 > (float)plot.Bottom)
			{
				num7 = plot.Bottom;
			}
			array[i] = new PointF(num6, num7);
		}
		using Pen pen = new Pen(color, width);
		pen.LineJoin = LineJoin.Round;
		pen.StartCap = LineCap.Round;
		pen.EndCap = LineCap.Round;
		g.DrawLines(pen, array);
	}

	private void drawOverlay(Graphics g, Rectangle plot)
	{
		using (new SolidBrush(Color.FromArgb(230, 255, 255, 255)))
		{
			using Brush brush = new SolidBrush(Color.DeepSkyBlue);
			using Brush brush2 = new SolidBrush(Color.Orange);
			using Brush brush3 = new SolidBrush(Color.LimeGreen);
			double value_display = toDisplayUnits(_last_in_bps);
			double value_display2 = toDisplayUnits(_last_out_bps);
			double value_display3 = toDisplayUnits(_last_tot_bps);
			string unit = ((_display_units == BandwidthUnits.KBps) ? "kB/s" : "Mbit/s");
			string s = formatOverlayLine("From radio:", value_display, unit);
			string s2 = formatOverlayLine("To radio:", value_display2, unit);
			string s3 = formatOverlayLine("Total:", value_display3, unit);
			SizeF sizeF = g.MeasureString(s, Font);
			SizeF sizeF2 = g.MeasureString(s2, Font);
			g.MeasureString(s3, Font);
			float num = plot.Left + 8;
			float num2 = plot.Top + 6;
			g.DrawString(s, Font, brush, num, num2);
			g.DrawString(s2, Font, brush2, num, num2 + sizeF.Height);
			g.DrawString(s3, Font, brush3, num, num2 + sizeF.Height + sizeF2.Height);
		}
	}

	private string formatOverlayLine(string prefix, double value_display, string unit)
	{
		if (_display_units == BandwidthUnits.KBps)
		{
			int num = (int)Math.Ceiling(value_display);
			return prefix + " " + num + " " + unit;
		}
		double num2 = Math.Ceiling(value_display * 10.0) / 10.0;
		return prefix + " " + num2.ToString("F1") + " " + unit;
	}

	private string formatAxisValue(double value_display)
	{
		if (_display_units == BandwidthUnits.KBps)
		{
			return ((int)Math.Ceiling(value_display)).ToString();
		}
		return (Math.Ceiling(value_display * 10.0) / 10.0).ToString("F1");
	}

	private double toDisplayUnits(double bytes_per_second)
	{
		if (_display_units == BandwidthUnits.KBps)
		{
			return bytes_per_second / 1024.0;
		}
		return bytes_per_second * 8.0 / 1000000.0;
	}

	private void updateScale()
	{
		if (_count <= 0)
		{
			return;
		}
		double num = 0.0;
		int num2 = Math.Min(_count, _tot_bps.Length);
		for (int i = 0; i < num2; i++)
		{
			double num3 = _tot_bps[i];
			if (num3 > num)
			{
				num = num3;
			}
			num3 = _in_bps[i];
			if (num3 > num)
			{
				num = num3;
			}
			num3 = _out_bps[i];
			if (num3 > num)
			{
				num = num3;
			}
		}
		double num4 = toDisplayUnits(num);
		if (num4 < 1E-06)
		{
			num4 = 1.0;
		}
		double num5 = num4 * 1.2;
		if (num5 < 1.0)
		{
			num5 = 1.0;
		}
		if (num5 > _scale_max_display)
		{
			_scale_max_display = num5;
			_scale_hold_ticks = 6;
			return;
		}
		if (_scale_hold_ticks > 0)
		{
			_scale_hold_ticks--;
			return;
		}
		double num6 = _scale_max_display * 0.92;
		if (num6 < num5)
		{
			num6 = num5;
		}
		if (num6 < 1.0)
		{
			num6 = 1.0;
		}
		_scale_max_display = num6;
	}

	private void resizeBuffers(int seconds)
	{
		_in_bps = new double[seconds];
		_out_bps = new double[seconds];
		_tot_bps = new double[seconds];
		_head = 0;
		_count = 0;
		_scale_max_display = 1.0;
		_scale_hold_ticks = 0;
		_last_in_bps = 0.0;
		_last_out_bps = 0.0;
		_last_tot_bps = 0.0;
		resetSmoothing();
	}

	private void resetSmoothing()
	{
		_smooth_in = 0.0;
		_smooth_out = 0.0;
		_smooth_tot = 0.0;
		_smoothing_ready = false;
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
		this.components = new System.ComponentModel.Container();
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
	}
}
