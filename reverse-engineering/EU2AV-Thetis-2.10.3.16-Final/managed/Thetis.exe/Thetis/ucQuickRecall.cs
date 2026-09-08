using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Thetis;

[DefaultEvent("ButtonClicked")]
public class ucQuickRecall : UserControl
{
	private struct QuickInfo
	{
		public double dFrequency;

		public string sFormattedFrequency;

		public DSPMode mode;
	}

	private const int m_nWAITDELAY = 4000;

	private Console m_objConsole;

	private Timer m_objTimerVFOA;

	private Timer m_objTimerVFOAMode;

	private Timer m_objBackgroundColourPinger;

	private double m_dLastVFOAFreq;

	private DSPMode m_lastDSPMode = DSPMode.FIRST;

	private int m_nVFOASelectedIndex = -1;

	private List<QuickInfo> m_lstVFOAFrequencies;

	private frmQuickRecallPopupList m_frmPopupList;

	private ToolStripDropDown m_popup;

	private ToolStripControlHost m_host;

	private bool m_bMox;

	private IContainer components;

	private ButtonTS btnPrevious;

	private ButtonTS btnNext;

	private ButtonTS btnList;

	private LabelTS lblFlashColour;

	private ToolTip toolTip1;

	public Button NextButton => btnNext;

	public Button ListButton => btnList;

	public Button PreviousButton => btnPrevious;

	public Console console
	{
		get
		{
			return m_objConsole;
		}
		set
		{
			m_objConsole = value;
			if (m_objConsole != null)
			{
				Console objConsole = m_objConsole;
				objConsole.VFOAFrequencyChangeHandlers = (Console.VFOAFrequencyChanged)Delegate.Combine(objConsole.VFOAFrequencyChangeHandlers, new Console.VFOAFrequencyChanged(OnVFOAChange));
				Console objConsole2 = m_objConsole;
				objConsole2.ModeChangeHandlers = (Console.ModeChanged)Delegate.Combine(objConsole2.ModeChangeHandlers, new Console.ModeChanged(OnModeChanged));
				Console objConsole3 = m_objConsole;
				objConsole3.MoxChangeHandlers = (Console.MoxChanged)Delegate.Combine(objConsole3.MoxChangeHandlers, new Console.MoxChanged(OnMoxChanged));
			}
		}
	}

	public event EventHandler ButtonClicked;

	public ucQuickRecall()
	{
		InitializeComponent();
		base.Disposed += OnDispose;
		m_frmPopupList = new frmQuickRecallPopupList();
		m_frmPopupList.TopLevel = false;
		frmQuickRecallPopupList frmPopupList = m_frmPopupList;
		frmPopupList.EntrySelectedHandlers = (frmQuickRecallPopupList.EntrySelected)Delegate.Combine(frmPopupList.EntrySelectedHandlers, new frmQuickRecallPopupList.EntrySelected(OnEntrySelected));
		m_host = new ToolStripControlHost(m_frmPopupList);
		m_host.Margin = Padding.Empty;
		m_host.Padding = Padding.Empty;
		m_popup = new ToolStripDropDown();
		m_popup.Margin = Padding.Empty;
		m_popup.Padding = Padding.Empty;
		m_popup.Items.Add(m_host);
		m_popup.Closed += OnPopupClosed;
		m_lstVFOAFrequencies = new List<QuickInfo>();
		m_objTimerVFOA = new Timer();
		m_objTimerVFOA.Tick += OnVFOATick;
		m_objTimerVFOAMode = new Timer();
		m_objTimerVFOAMode.Tick += OnVFOAModeTick;
		m_objBackgroundColourPinger = new Timer();
		m_objBackgroundColourPinger.Tick += OnBackgroundColourPingerTick;
		SetStyle(ControlStyles.SupportsTransparentBackColor, value: true);
		BackColor = Color.Transparent;
		setButtonBackColour();
		resizeAndReposition();
		lblFlashColour.Visible = false;
	}

	private void resizeAndReposition()
	{
		base.Size = new Size(btnNext.Right + 1, btnNext.Bottom + 1);
		int num = btnList.Left + btnList.Width / 2;
		int num2 = btnList.Top + btnList.Height / 2;
		Point location = new Point(num - lblFlashColour.Width / 2, num2 - lblFlashColour.Height / 2);
		lblFlashColour.Location = location;
	}

	private void OnPopupClosed(object sender, ToolStripDropDownClosedEventArgs e)
	{
		btnList.Enabled = true;
	}

	private void OnMoxChanged(int rx, bool oldMox, bool newMox)
	{
		m_bMox = newMox;
		btnPrevious.Enabled = !m_bMox;
		btnList.Enabled = !m_bMox;
		btnNext.Enabled = !m_bMox;
	}

	private void OnModeChanged(int rx, DSPMode oldMode, DSPMode newMode, Band oldBand, Band newBand)
	{
		if (!m_bMox && rx == 1)
		{
			m_lastDSPMode = newMode;
			m_objTimerVFOAMode.Stop();
			m_objTimerVFOAMode.Interval = 4000;
			m_objTimerVFOAMode.Start();
		}
	}

	private void OnVFOAChange(Band oldBand, Band newBand, DSPMode oldMode, DSPMode newMode, Filter oldFilter, Filter newFilter, double oldFreq, double newFreq, double oldCentreF, double newCentreF, bool oldCTUN, bool newCTUN, int oldZoomSlider, int newZoomSlider, double offset, int rx)
	{
		if (!m_bMox)
		{
			m_dLastVFOAFreq = newFreq;
			m_objTimerVFOA.Stop();
			m_objTimerVFOA.Interval = 4000;
			m_objTimerVFOA.Start();
		}
	}

	private void buttonClicked(EventArgs e)
	{
		ButtonClicked?.Invoke(this, e);
	}

	private string formatFrequencyToString(double f)
	{
		string text = f.ToString("0.000000");
		string text2 = text.Substring(text.Length - 3, 3);
		return text.Substring(0, text.Length - 3) + "." + text2;
	}

	private int findExistingInVFOA(double f)
	{
		for (int i = 0; i < m_lstVFOAFrequencies.Count; i++)
		{
			if (m_lstVFOAFrequencies[i].dFrequency == f)
			{
				return i;
			}
		}
		return -1;
	}

	private void addVFOAEntry(QuickInfo qi)
	{
		m_lstVFOAFrequencies.Insert(0, qi);
		if (m_lstVFOAFrequencies.Count > 16)
		{
			m_lstVFOAFrequencies.RemoveAt(m_lstVFOAFrequencies.Count - 1);
		}
		m_nVFOASelectedIndex = 0;
	}

	private void OnBackgroundColourPingerTick(object sender, EventArgs e)
	{
		m_objBackgroundColourPinger.Stop();
		lblFlashColour.Visible = false;
	}

	private void OnVFOAModeTick(object sender, EventArgs e)
	{
		m_objTimerVFOAMode.Stop();
		if (!m_bMox)
		{
			int num = findExistingInVFOA(m_dLastVFOAFreq);
			if (num != -1 && m_lstVFOAFrequencies[num].mode != m_lastDSPMode)
			{
				QuickInfo value = m_lstVFOAFrequencies[num];
				value.mode = m_lastDSPMode;
				m_lstVFOAFrequencies[num] = value;
				lblFlashColour.BackColor = Color.Orange;
				lblFlashColour.Visible = true;
				m_objBackgroundColourPinger.Interval = 250;
				m_objBackgroundColourPinger.Start();
			}
		}
	}

	private void OnVFOATick(object sender, EventArgs e)
	{
		m_objTimerVFOA.Stop();
		if (!m_bMox)
		{
			int num = findExistingInVFOA(m_dLastVFOAFreq);
			if (num == -1)
			{
				addVFOAEntry(new QuickInfo
				{
					dFrequency = m_dLastVFOAFreq,
					sFormattedFrequency = formatFrequencyToString(m_dLastVFOAFreq),
					mode = m_lastDSPMode
				});
				lblFlashColour.BackColor = Color.LimeGreen;
				lblFlashColour.Visible = true;
				m_objBackgroundColourPinger.Interval = 250;
				m_objBackgroundColourPinger.Start();
			}
			else
			{
				m_nVFOASelectedIndex = num;
			}
			if (m_popup.Visible)
			{
				buildAndShowPopup();
			}
		}
	}

	private void OnDispose(object sender, EventArgs e)
	{
		m_popup.Closed -= OnPopupClosed;
		frmQuickRecallPopupList frmPopupList = m_frmPopupList;
		frmPopupList.EntrySelectedHandlers = (frmQuickRecallPopupList.EntrySelected)Delegate.Remove(frmPopupList.EntrySelectedHandlers, new frmQuickRecallPopupList.EntrySelected(OnEntrySelected));
		if (m_objConsole != null)
		{
			Console objConsole = m_objConsole;
			objConsole.VFOAFrequencyChangeHandlers = (Console.VFOAFrequencyChanged)Delegate.Remove(objConsole.VFOAFrequencyChangeHandlers, new Console.VFOAFrequencyChanged(OnVFOAChange));
			Console objConsole2 = m_objConsole;
			objConsole2.ModeChangeHandlers = (Console.ModeChanged)Delegate.Remove(objConsole2.ModeChangeHandlers, new Console.ModeChanged(OnModeChanged));
		}
	}

	private void OnEntrySelected(int index)
	{
		m_popup.Hide();
		if (index >= 0 && index <= m_lstVFOAFrequencies.Count - 1)
		{
			m_nVFOASelectedIndex = index;
			selectVFOAEntry(m_nVFOASelectedIndex);
		}
	}

	private void btnPrevious_Click(object sender, EventArgs e)
	{
		if (m_nVFOASelectedIndex != -1 && m_lstVFOAFrequencies.Count > 1)
		{
			m_nVFOASelectedIndex--;
			if (m_nVFOASelectedIndex < 0)
			{
				m_nVFOASelectedIndex = m_lstVFOAFrequencies.Count - 1;
			}
			selectVFOAEntry(m_nVFOASelectedIndex);
			buttonClicked(EventArgs.Empty);
		}
	}

	private void btnNext_Click(object sender, EventArgs e)
	{
		if (m_nVFOASelectedIndex != -1 && m_lstVFOAFrequencies.Count > 1)
		{
			m_nVFOASelectedIndex++;
			if (m_nVFOASelectedIndex > m_lstVFOAFrequencies.Count - 1)
			{
				m_nVFOASelectedIndex = 0;
			}
			selectVFOAEntry(m_nVFOASelectedIndex);
			buttonClicked(EventArgs.Empty);
		}
	}

	private void selectVFOAEntry(int index)
	{
		if (index >= 0 && index <= m_lstVFOAFrequencies.Count - 1)
		{
			console.VFOAFreq = m_lstVFOAFrequencies[index].dFrequency;
			if (m_lstVFOAFrequencies[index].mode != DSPMode.FIRST)
			{
				console.RX1DSPMode = m_lstVFOAFrequencies[index].mode;
			}
		}
	}

	private void btnList_Click(object sender, EventArgs e)
	{
		btnList.Enabled = false;
		buildAndShowPopup();
		buttonClicked(EventArgs.Empty);
	}

	private void buildAndShowPopup()
	{
		m_frmPopupList.ClearItems();
		for (int i = 0; i < m_lstVFOAFrequencies.Count; i++)
		{
			int num = m_frmPopupList.AddItem(Math.Round(m_lstVFOAFrequencies[i].dFrequency, 6));
			if (m_nVFOASelectedIndex == num)
			{
				m_frmPopupList.FreqList.SelectedIndex = num;
			}
		}
		m_host.Width = m_frmPopupList.Width;
		m_host.Height = 20 + m_lstVFOAFrequencies.Count * (m_frmPopupList.FontEntryHeight + 1);
		m_frmPopupList.Height = m_host.Height;
		m_popup.Show(this, new Point(btnList.Left + btnList.Width / 2 - m_frmPopupList.Width / 2, btnList.Top + btnList.Height));
	}

	private void ucQuickRecall_BackColorChanged(object sender, EventArgs e)
	{
		setButtonBackColour();
	}

	private void setButtonBackColour()
	{
		btnPrevious.BackColor = BackColor;
		btnList.BackColor = BackColor;
		btnNext.BackColor = BackColor;
	}

	private void ucQuickRecall_Resize(object sender, EventArgs e)
	{
		resizeAndReposition();
	}

	private void lblFlashColour_Click(object sender, EventArgs e)
	{
		if (btnList.Enabled)
		{
			btnList_Click(sender, e);
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
		this.btnList = new System.Windows.Forms.ButtonTS();
		this.btnNext = new System.Windows.Forms.ButtonTS();
		this.btnPrevious = new System.Windows.Forms.ButtonTS();
		this.lblFlashColour = new System.Windows.Forms.LabelTS();
		this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
		base.SuspendLayout();
		this.btnList.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
		this.btnList.FlatAppearance.BorderSize = 0;
		this.btnList.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnList.ForeColor = System.Drawing.SystemColors.ControlLightLight;
		this.btnList.Image = null;
		this.btnList.Location = new System.Drawing.Point(20, 0);
		this.btnList.Name = "btnList";
		this.btnList.Size = new System.Drawing.Size(20, 20);
		this.btnList.TabIndex = 2;
		this.btnList.TabStop = false;
		this.btnList.Text = "V";
		this.toolTip1.SetToolTip(this.btnList, "QuickRecall will store a frequency/mode if you stay on that frequency for 4 seconds. Click to show list.");
		this.btnList.UseVisualStyleBackColor = false;
		this.btnList.Click += new System.EventHandler(btnList_Click);
		this.btnNext.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
		this.btnNext.FlatAppearance.BorderSize = 0;
		this.btnNext.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnNext.ForeColor = System.Drawing.SystemColors.ControlLightLight;
		this.btnNext.Image = null;
		this.btnNext.Location = new System.Drawing.Point(40, 0);
		this.btnNext.Name = "btnNext";
		this.btnNext.Size = new System.Drawing.Size(20, 20);
		this.btnNext.TabIndex = 1;
		this.btnNext.TabStop = false;
		this.btnNext.Text = ">";
		this.toolTip1.SetToolTip(this.btnNext, "Next frequency in QuickRecall list");
		this.btnNext.UseVisualStyleBackColor = false;
		this.btnNext.Click += new System.EventHandler(btnNext_Click);
		this.btnPrevious.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
		this.btnPrevious.FlatAppearance.BorderSize = 0;
		this.btnPrevious.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnPrevious.ForeColor = System.Drawing.SystemColors.ControlLightLight;
		this.btnPrevious.Image = null;
		this.btnPrevious.Location = new System.Drawing.Point(0, 0);
		this.btnPrevious.Name = "btnPrevious";
		this.btnPrevious.Size = new System.Drawing.Size(20, 20);
		this.btnPrevious.TabIndex = 0;
		this.btnPrevious.TabStop = false;
		this.btnPrevious.Text = "<";
		this.toolTip1.SetToolTip(this.btnPrevious, "Previous frequency in QuickRecall list");
		this.btnPrevious.UseVisualStyleBackColor = false;
		this.btnPrevious.Click += new System.EventHandler(btnPrevious_Click);
		this.lblFlashColour.BackColor = System.Drawing.Color.Red;
		this.lblFlashColour.Image = null;
		this.lblFlashColour.Location = new System.Drawing.Point(25, 6);
		this.lblFlashColour.Name = "lblFlashColour";
		this.lblFlashColour.Size = new System.Drawing.Size(14, 14);
		this.lblFlashColour.TabIndex = 3;
		this.toolTip1.SetToolTip(this.lblFlashColour, "Green = just added, Orange = udated mode");
		this.lblFlashColour.Click += new System.EventHandler(lblFlashColour_Click);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackColor = System.Drawing.Color.Transparent;
		base.Controls.Add(this.lblFlashColour);
		base.Controls.Add(this.btnList);
		base.Controls.Add(this.btnNext);
		base.Controls.Add(this.btnPrevious);
		base.Name = "ucQuickRecall";
		base.Size = new System.Drawing.Size(89, 34);
		base.BackColorChanged += new System.EventHandler(ucQuickRecall_BackColorChanged);
		base.Resize += new System.EventHandler(ucQuickRecall_Resize);
		base.ResumeLayout(false);
	}
}
