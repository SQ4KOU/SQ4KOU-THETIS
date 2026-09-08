using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Thetis.Properties;

namespace Thetis;

public class Splash : Form
{
	private class StartParams
	{
		public string vers;

		public string splash_folder;
	}

	private static Splash ms_frmSplash = null;

	private static Thread ms_oThread = null;

	private double m_dblOpacityIncrement = 0.07;

	private double m_dblOpacityDecrement = 0.035;

	private const int TIMER_INTERVAL = 50;

	private string m_sStatus;

	private double m_dblCompletionFraction;

	private Rectangle m_rProgress;

	private double m_dblLastCompletionFraction;

	private double m_dblPBIncrementPerTimerInterval = 0.015;

	private bool m_bFirstLaunch;

	private DateTime m_dtStart;

	private bool m_bDTSet;

	private int m_iIndex = 1;

	private int m_iActualTicks;

	private ArrayList m_alPreviousCompletionFraction;

	private ArrayList m_alActualTimes = new ArrayList();

	private const string REG_KEY_INITIALIZATION = "Initialization";

	private const string REGVALUE_PB_MILISECOND_INCREMENT = "Increment";

	private const string REGVALUE_PB_PERCENTS = "Percents";

	private LabelTS lblTimeRemaining;

	private System.Windows.Forms.Timer timer1;

	private LabelTS lblStatus;

	private Panel pnlStatus;

	private LabelTS lblVersion;

	private Panel panel1;

	private IContainer components;

	private static StartParams _start_params = new StartParams();

	public static Splash SplashForm => ms_frmSplash;

	[DllImport("user32.dll")]
	private static extern bool InvalidateRect(IntPtr hWnd, IntPtr lpRect, bool bErase);

	public Splash()
	{
		InitializeComponent();
		Common.DoubleBufferAll(this, enabled: true);
		base.Opacity = 0.0;
		timer1.Interval = 50;
		timer1.Start();
		base.ClientSize = BackgroundImage.Size;
		base.ShowInTaskbar = false;
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Thetis.Splash));
		this.pnlStatus = new System.Windows.Forms.Panel();
		this.timer1 = new System.Windows.Forms.Timer(this.components);
		this.panel1 = new System.Windows.Forms.Panel();
		this.lblVersion = new System.Windows.Forms.LabelTS();
		this.lblTimeRemaining = new System.Windows.Forms.LabelTS();
		this.lblStatus = new System.Windows.Forms.LabelTS();
		base.SuspendLayout();
		this.pnlStatus.BackColor = System.Drawing.Color.SkyBlue;
		this.pnlStatus.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
		this.pnlStatus.Location = new System.Drawing.Point(59, 270);
		this.pnlStatus.Name = "pnlStatus";
		this.pnlStatus.Size = new System.Drawing.Size(289, 17);
		this.pnlStatus.TabIndex = 2;
		this.pnlStatus.Paint += new System.Windows.Forms.PaintEventHandler(pnlStatus_Paint);
		this.timer1.Enabled = true;
		this.timer1.Tick += new System.EventHandler(timer1_Tick);
		this.panel1.BackColor = System.Drawing.Color.FromArgb(192, 64, 64, 64);
		this.panel1.Location = new System.Drawing.Point(50, 249);
		this.panel1.Name = "panel1";
		this.panel1.Size = new System.Drawing.Size(306, 59);
		this.panel1.TabIndex = 4;
		this.lblVersion.AutoSize = true;
		this.lblVersion.BackColor = System.Drawing.Color.FromArgb(192, 64, 64, 64);
		this.lblVersion.ForeColor = System.Drawing.Color.White;
		this.lblVersion.Image = null;
		this.lblVersion.Location = new System.Drawing.Point(56, 290);
		this.lblVersion.Name = "lblVersion";
		this.lblVersion.Size = new System.Drawing.Size(205, 13);
		this.lblVersion.TabIndex = 3;
		this.lblVersion.Text = "WWWWWWWWWWWWWWWWWW";
		this.lblTimeRemaining.BackColor = System.Drawing.Color.FromArgb(192, 64, 64, 64);
		this.lblTimeRemaining.ForeColor = System.Drawing.Color.White;
		this.lblTimeRemaining.Image = null;
		this.lblTimeRemaining.Location = new System.Drawing.Point(256, 290);
		this.lblTimeRemaining.Name = "lblTimeRemaining";
		this.lblTimeRemaining.Size = new System.Drawing.Size(92, 16);
		this.lblTimeRemaining.TabIndex = 1;
		this.lblTimeRemaining.Text = "Time";
		this.lblTimeRemaining.TextAlign = System.Drawing.ContentAlignment.TopRight;
		this.lblStatus.BackColor = System.Drawing.Color.FromArgb(192, 64, 64, 64);
		this.lblStatus.ForeColor = System.Drawing.Color.White;
		this.lblStatus.Image = null;
		this.lblStatus.Location = new System.Drawing.Point(50, 252);
		this.lblStatus.Name = "lblStatus";
		this.lblStatus.Size = new System.Drawing.Size(306, 16);
		this.lblStatus.TabIndex = 0;
		this.lblStatus.Text = "Status";
		this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.BackgroundImage = (System.Drawing.Image)resources.GetObject("$this.BackgroundImage");
		base.ClientSize = new System.Drawing.Size(400, 320);
		base.Controls.Add(this.lblVersion);
		base.Controls.Add(this.pnlStatus);
		base.Controls.Add(this.lblTimeRemaining);
		base.Controls.Add(this.lblStatus);
		base.Controls.Add(this.panel1);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
		base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		base.Name = "Splash";
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "Splash";
		base.Load += new System.EventHandler(Splash_Load);
		base.ResumeLayout(false);
		base.PerformLayout();
	}

	public static void ShowSplashScreen(string version, string splash_screen_folder = "")
	{
		if (ms_frmSplash == null)
		{
			ms_oThread = new Thread(ShowForm)
			{
				IsBackground = true,
				Name = "Splash Screen Thread"
			};
			_start_params.vers = version;
			_start_params.splash_folder = splash_screen_folder;
			ms_oThread.Start();
		}
	}

	private static void ShowForm()
	{
		ms_frmSplash = new Splash();
		ms_frmSplash.setVersion(_start_params.vers);
		ms_frmSplash.setBackground(_start_params.splash_folder);
		Control.CheckForIllegalCrossThreadCalls = false;
		Application.Run(ms_frmSplash);
	}

	public static void CloseForm()
	{
		if (ms_frmSplash != null && !ms_frmSplash.IsDisposed)
		{
			ms_frmSplash.m_dblOpacityIncrement = 0.0 - ms_frmSplash.m_dblOpacityDecrement;
		}
		ms_oThread = null;
		ms_frmSplash = null;
	}

	public static void HideForm()
	{
		if (ms_frmSplash != null && !ms_frmSplash.IsDisposed)
		{
			ms_frmSplash.Hide();
		}
	}

	public static void UnHideForm()
	{
		if (ms_frmSplash != null && !ms_frmSplash.IsDisposed)
		{
			ms_frmSplash.Show();
		}
	}

	public static void SetStatus(string newStatus)
	{
		SetStatus(newStatus, setReference: true);
	}

	public static void SetStatus(string newStatus, bool setReference)
	{
		if (ms_frmSplash != null)
		{
			ms_frmSplash.m_sStatus = newStatus;
			if (setReference)
			{
				ms_frmSplash.SetReferenceInternal();
			}
		}
	}

	public static void SetReferencePoint()
	{
		if (ms_frmSplash != null)
		{
			ms_frmSplash.SetReferenceInternal();
		}
	}

	private void setVersion(string version)
	{
		lblStatus.Text = "Extended version from EU2AV";
	}

	private void setBackground(string splash_screen_folder)
	{
		if (string.IsNullOrEmpty(splash_screen_folder))
		{
			return;
		}
		int width = 400;
		int height = 320;
		string[] array = Directory.GetFiles(splash_screen_folder, "*.png").Where(delegate(string file)
		{
			using Image image = Image.FromFile(file);
			return image.Width == width && image.Height == height;
		}).ToArray();
		if (array.Length == 0)
		{
			return;
		}
		int num = new Random().Next(array.Length);
		try
		{
			if (File.Exists(array[num]))
			{
				BackgroundImage = Image.FromFile(array[num]);
			}
		}
		catch
		{
			BackgroundImage = Resources.thetis_logo2;
		}
	}

	private void SetReferenceInternal()
	{
		if (!m_bDTSet)
		{
			m_bDTSet = true;
			m_dtStart = DateTime.Now;
			ReadIncrements();
		}
		double num = ElapsedMilliSeconds();
		m_alActualTimes.Add(num);
		m_dblLastCompletionFraction = m_dblCompletionFraction;
		if (m_alPreviousCompletionFraction != null && m_iIndex < m_alPreviousCompletionFraction.Count)
		{
			m_dblCompletionFraction = (double)m_alPreviousCompletionFraction[m_iIndex++];
		}
		else
		{
			m_dblCompletionFraction = ((m_iIndex > 0) ? 1 : 0);
		}
	}

	private double ElapsedMilliSeconds()
	{
		return (DateTime.Now - m_dtStart).TotalMilliseconds;
	}

	private void ReadIncrements()
	{
		if (double.TryParse(RegistryAccess.GetStringRegistryValue("Increment", "0.0015"), NumberStyles.Float, NumberFormatInfo.InvariantInfo, out var result))
		{
			m_dblPBIncrementPerTimerInterval = result;
		}
		else
		{
			m_dblPBIncrementPerTimerInterval = 0.0015;
		}
		string stringRegistryValue = RegistryAccess.GetStringRegistryValue("Percents", "");
		if (stringRegistryValue != "")
		{
			string[] array = stringRegistryValue.Split(null);
			m_alPreviousCompletionFraction = new ArrayList();
			for (int i = 0; i < array.Length; i++)
			{
				if (double.TryParse(array[i], NumberStyles.Float, NumberFormatInfo.InvariantInfo, out var result2))
				{
					m_alPreviousCompletionFraction.Add(result2);
				}
				else
				{
					m_alPreviousCompletionFraction.Add(1.0);
				}
			}
		}
		else
		{
			m_bFirstLaunch = true;
			lblTimeRemaining.Text = "";
		}
	}

	private void StoreIncrements()
	{
		string text = "";
		double num = ElapsedMilliSeconds();
		for (int i = 0; i < m_alActualTimes.Count; i++)
		{
			text = text + ((double)m_alActualTimes[i] / num).ToString("0.####", NumberFormatInfo.InvariantInfo) + " ";
		}
		RegistryAccess.SetStringRegistryValue("Percents", text);
		m_dblPBIncrementPerTimerInterval = 1.0 / (double)m_iActualTicks;
		RegistryAccess.SetStringRegistryValue("Increment", m_dblPBIncrementPerTimerInterval.ToString("#.000000", NumberFormatInfo.InvariantInfo));
	}

	private void timer1_Tick(object sender, EventArgs e)
	{
		lblVersion.Text = m_sStatus;
		if (m_dblOpacityIncrement > 0.0)
		{
			m_iActualTicks++;
			if (base.Opacity < 1.0)
			{
				base.Opacity += m_dblOpacityIncrement;
			}
		}
		else if (base.Opacity > 0.0)
		{
			base.Opacity += m_dblOpacityIncrement;
		}
		else
		{
			StoreIncrements();
			Close();
		}
		if (m_bFirstLaunch || !(m_dblLastCompletionFraction < m_dblCompletionFraction))
		{
			return;
		}
		m_dblLastCompletionFraction += m_dblPBIncrementPerTimerInterval;
		int num = (int)Math.Floor((double)pnlStatus.ClientRectangle.Width * m_dblLastCompletionFraction);
		int num2 = pnlStatus.ClientRectangle.Height;
		int num3 = pnlStatus.ClientRectangle.X;
		int num4 = pnlStatus.ClientRectangle.Y;
		if (num > 0 && num2 > 0)
		{
			m_rProgress = new Rectangle(num3, num4, num, num2);
			if (pnlStatus != null && !pnlStatus.IsDisposed)
			{
				InvalidateRect(pnlStatus.Handle, IntPtr.Zero, bErase: false);
			}
			int num5 = 1 + (int)(50.0 * ((1.0 - m_dblLastCompletionFraction) / m_dblPBIncrementPerTimerInterval)) / 1000;
			if (num5 == 1)
			{
				lblTimeRemaining.Text = $"1 second";
			}
			else
			{
				lblTimeRemaining.Text = $"{num5} seconds";
			}
		}
	}

	private void pnlStatus_Paint(object sender, PaintEventArgs e)
	{
		if (!m_bFirstLaunch && e.ClipRectangle.Width > 0 && m_iActualTicks > 1)
		{
			LinearGradientBrush brush = new LinearGradientBrush(m_rProgress, Color.FromArgb(100, 100, 100), Color.FromArgb(130, 255, 130), LinearGradientMode.Horizontal);
			e.Graphics.FillRectangle(brush, m_rProgress);
		}
	}

	private void SplashScreen_DoubleClick(object sender, EventArgs e)
	{
		CloseForm();
	}

	private void Splash_Load(object sender, EventArgs e)
	{
		lblStatus.Text = "Extended version from EU2AV";
		lblVersion.Text = "";
		lblTimeRemaining.Text = "";
	}
}
