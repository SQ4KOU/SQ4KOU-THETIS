using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Thetis;

public class ucUnderOverFlowWarningViewer : UserControl
{
	private bool[] _hasHadIssues;

	private Color _OutOverflowsColour = Color.Transparent;

	private Color _OutUnderflowsColour = Color.Transparent;

	private Color _InOverflowsColour = Color.Transparent;

	private Color _InUnderflowsColour = Color.Transparent;

	private bool _noFade;

	private IContainer components;

	private ToolTip toolTip1;

	private Timer tmrFade;

	public bool OutOverflow
	{
		set
		{
			_hasHadIssues[0] = true;
			setColours();
		}
	}

	public bool OutUnderflow
	{
		set
		{
			_hasHadIssues[1] = true;
			setColours();
		}
	}

	public bool InOverflow
	{
		set
		{
			_hasHadIssues[2] = true;
			setColours();
		}
	}

	public bool InUnderflow
	{
		set
		{
			_hasHadIssues[3] = true;
			setColours();
		}
	}

	public bool NoFade
	{
		get
		{
			return _noFade;
		}
		set
		{
			_noFade = value;
		}
	}

	public event EventHandler ClearIssuesClick;

	public ucUnderOverFlowWarningViewer()
	{
		InitializeComponent();
		_hasHadIssues = new bool[4];
	}

	private void UnderOverFlowWarningViewer_Load(object sender, EventArgs e)
	{
		BackColor = Color.Transparent;
		tmrFade.Enabled = false;
		clearIssues();
	}

	private void setColours(bool forceUpdate = false)
	{
		bool flag = false;
		for (int i = 0; i < _hasHadIssues.Length; i++)
		{
			switch (i)
			{
			case 0:
				if (_hasHadIssues[i])
				{
					_OutOverflowsColour = Color.Red;
					flag = true;
				}
				break;
			case 1:
				if (_hasHadIssues[i])
				{
					_OutUnderflowsColour = Color.Red;
					flag = true;
				}
				break;
			case 2:
				if (_hasHadIssues[i])
				{
					_InOverflowsColour = Color.Lime;
					flag = true;
				}
				break;
			case 3:
				if (_hasHadIssues[i])
				{
					_InUnderflowsColour = Color.Lime;
					flag = true;
				}
				break;
			}
			_hasHadIssues[i] = false;
		}
		if (flag | forceUpdate)
		{
			tmrFade.Enabled = !_noFade;
			Invalidate();
		}
	}

	private void clearIssues()
	{
		tmrFade.Enabled = false;
		for (int i = 0; i < _hasHadIssues.Length; i++)
		{
			_hasHadIssues[i] = false;
		}
		_OutOverflowsColour = Color.Transparent;
		_OutUnderflowsColour = Color.Transparent;
		_InOverflowsColour = Color.Transparent;
		_InUnderflowsColour = Color.Transparent;
		setColours(forceUpdate: true);
	}

	private Color fadeBackground(Color c)
	{
		if (c == Color.Transparent)
		{
			return Color.Transparent;
		}
		float num = (int)c.A;
		num *= 0.9f;
		if (num < 16f)
		{
			return Color.Transparent;
		}
		return Color.FromArgb((int)num, c.R, c.G, c.B);
	}

	private void tmrFade_Tick(object sender, EventArgs e)
	{
		_OutOverflowsColour = fadeBackground(_OutOverflowsColour);
		_OutUnderflowsColour = fadeBackground(_OutUnderflowsColour);
		_InOverflowsColour = fadeBackground(_InOverflowsColour);
		_InUnderflowsColour = fadeBackground(_InUnderflowsColour);
		if (_OutOverflowsColour == Color.Transparent && _OutUnderflowsColour == Color.Transparent && _InOverflowsColour == Color.Transparent && _InUnderflowsColour == Color.Transparent)
		{
			tmrFade.Enabled = false;
			for (int i = 0; i < _hasHadIssues.Length; i++)
			{
				_hasHadIssues[i] = false;
			}
		}
		Invalidate();
	}

	private void UnderOverFlowWarningViewer_Paint(object sender, PaintEventArgs e)
	{
		Graphics graphics = e.Graphics;
		SolidBrush solidBrush = new SolidBrush(_OutOverflowsColour);
		SolidBrush solidBrush2 = new SolidBrush(_OutUnderflowsColour);
		SolidBrush solidBrush3 = new SolidBrush(_InOverflowsColour);
		SolidBrush solidBrush4 = new SolidBrush(_InUnderflowsColour);
		using (Pen pen = new Pen(Color.FromArgb(Math.Max(Math.Max((int)Math.Max(solidBrush.Color.A, solidBrush2.Color.A), (int)solidBrush3.Color.A), solidBrush4.Color.A), 32, 32, 32), 1f))
		{
			graphics.DrawLine(pen, new Point(0, 0), new Point(0, 14));
			graphics.DrawLine(pen, new Point(7, 0), new Point(7, 14));
			graphics.DrawLine(pen, new Point(14, 0), new Point(14, 14));
			graphics.DrawLine(pen, new Point(0, 0), new Point(14, 0));
			graphics.DrawLine(pen, new Point(0, 7), new Point(14, 7));
			graphics.DrawLine(pen, new Point(0, 14), new Point(14, 14));
		}
		Rectangle rect = new Rectangle(1, 1, 6, 6);
		graphics.FillRectangle(solidBrush, rect);
		rect.Location = new Point(1, 8);
		graphics.FillRectangle(solidBrush2, rect);
		rect.Location = new Point(8, 1);
		graphics.FillRectangle(solidBrush3, rect);
		rect.Location = new Point(8, 8);
		graphics.FillRectangle(solidBrush4, rect);
		solidBrush.Dispose();
		solidBrush2.Dispose();
		solidBrush3.Dispose();
		solidBrush4.Dispose();
	}

	private void UnderOverFlowWarningViewer_Click(object sender, EventArgs e)
	{
		if (_noFade)
		{
			clearIssues();
			ClearIssuesClick?.Invoke(sender, e);
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
		this.components = new System.ComponentModel.Container();
		this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
		this.tmrFade = new System.Windows.Forms.Timer(this.components);
		base.SuspendLayout();
		this.tmrFade.Tick += new System.EventHandler(tmrFade_Tick);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackColor = System.Drawing.Color.White;
		this.DoubleBuffered = true;
		this.ForeColor = System.Drawing.Color.Transparent;
		this.MaximumSize = new System.Drawing.Size(15, 15);
		this.MinimumSize = new System.Drawing.Size(15, 15);
		base.Name = "UnderOverFlowWarningViewer";
		base.Size = new System.Drawing.Size(15, 15);
		base.Load += new System.EventHandler(UnderOverFlowWarningViewer_Load);
		base.Click += new System.EventHandler(UnderOverFlowWarningViewer_Click);
		base.Paint += new System.Windows.Forms.PaintEventHandler(UnderOverFlowWarningViewer_Paint);
		base.ResumeLayout(false);
	}
}
