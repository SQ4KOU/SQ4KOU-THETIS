using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Thetis;

public class Progress : Form
{
	private delegate void Invoker(float f);

	private float percent_done;

	private Panel panel1;

	private ButtonTS btnAbort;

	private Container components;

	private string digits = "f1";

	private int percent_digits = 1;

	private string percent_symbol = "%";

	public int PercentDigits
	{
		get
		{
			return percent_digits;
		}
		set
		{
			percent_digits = value;
			digits = "f" + percent_digits;
		}
	}

	public string PercentSymbol
	{
		get
		{
			return percent_symbol;
		}
		set
		{
			percent_symbol = value;
		}
	}

	public Progress(string s)
	{
		InitializeComponent();
		Common.DoubleBufferAll(this, enabled: true);
		Text = s;
		percent_done = 0f;
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Thetis.Progress));
		this.panel1 = new System.Windows.Forms.Panel();
		this.btnAbort = new System.Windows.Forms.ButtonTS();
		base.SuspendLayout();
		this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.panel1.Location = new System.Drawing.Point(16, 16);
		this.panel1.Name = "panel1";
		this.panel1.Size = new System.Drawing.Size(208, 24);
		this.panel1.TabIndex = 0;
		this.panel1.Paint += new System.Windows.Forms.PaintEventHandler(panel1_Paint);
		this.btnAbort.Image = null;
		this.btnAbort.Location = new System.Drawing.Point(240, 16);
		this.btnAbort.Name = "btnAbort";
		this.btnAbort.Size = new System.Drawing.Size(75, 23);
		this.btnAbort.TabIndex = 1;
		this.btnAbort.Text = "Abort";
		this.btnAbort.Click += new System.EventHandler(btnAbort_Click);
		base.ClientSize = new System.Drawing.Size(330, 56);
		base.Controls.Add(this.btnAbort);
		base.Controls.Add(this.panel1);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
		base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		base.Name = "Progress";
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "progress";
		base.Closing += new System.ComponentModel.CancelEventHandler(Progress_Closing);
		base.ResumeLayout(false);
	}

	public void SetPercent(float f)
	{
		percent_done = f * 100f;
		panel1.Invalidate();
	}

	private void btnAbort_Click(object sender, EventArgs e)
	{
		Hide();
		base.Visible = false;
	}

	private void panel1_Paint(object sender, PaintEventArgs e)
	{
		int num = (int)Math.Floor((float)panel1.Width * percent_done / 100f);
		if (num != 0)
		{
			Graphics graphics = e.Graphics;
			graphics.SmoothingMode = SmoothingMode.HighQuality;
			LinearGradientBrush linearGradientBrush = new LinearGradientBrush(new Rectangle(0, 0, num, panel1.Height), Color.Green, Color.Lime, LinearGradientMode.Horizontal);
			graphics.FillRectangle(linearGradientBrush, 0, 0, num, panel1.Height);
			string s = percent_done.ToString(digits) + percent_symbol;
			SolidBrush solidBrush = new SolidBrush(Color.Black);
			Font font = new Font("Microsoft Sans Serif", 10f);
			StringFormat stringFormat = new StringFormat
			{
				Alignment = StringAlignment.Center,
				LineAlignment = StringAlignment.Center
			};
			graphics.DrawString(s, font, solidBrush, new RectangleF(0f, 0f, panel1.Width, panel1.Height), stringFormat);
			linearGradientBrush.Dispose();
			solidBrush.Dispose();
			font.Dispose();
			stringFormat.Dispose();
		}
	}

	private void Progress_Closing(object sender, CancelEventArgs e)
	{
		Hide();
		e.Cancel = true;
	}
}
