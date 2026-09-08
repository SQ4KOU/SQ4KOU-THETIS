using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Thetis;

public class frmQuickRecallPopupList : Form
{
	public delegate void EntrySelected(int index);

	public EntrySelected EntrySelectedHandlers;

	private IContainer components;

	private QuickRecallListBox lstboxFrequencies;

	public ListBox FreqList => lstboxFrequencies;

	public int FontEntryHeight => lstboxFrequencies.FontEntryHeight;

	public frmQuickRecallPopupList()
	{
		InitializeComponent();
	}

	public int AddItem(double dFreq)
	{
		return lstboxFrequencies.AddItem(dFreq);
	}

	public void ClearItems()
	{
		lstboxFrequencies.ClearItems();
	}

	private void lstboxFrequencies_MouseClick(object sender, MouseEventArgs e)
	{
		int num = lstboxFrequencies.IndexFromPoint(e.Location);
		if (lstboxFrequencies.SelectedIndex >= 0 && num >= 0)
		{
			EntrySelectedHandlers?.Invoke(lstboxFrequencies.SelectedIndex);
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
		this.lstboxFrequencies = new Thetis.QuickRecallListBox();
		base.SuspendLayout();
		this.lstboxFrequencies.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.lstboxFrequencies.BackColor = System.Drawing.SystemColors.ControlDark;
		this.lstboxFrequencies.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lstboxFrequencies.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
		this.lstboxFrequencies.Font = new System.Drawing.Font("Calibri", 12f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.lstboxFrequencies.ForeColor = System.Drawing.SystemColors.ControlText;
		this.lstboxFrequencies.FormattingEnabled = true;
		this.lstboxFrequencies.ItemHeight = 20;
		this.lstboxFrequencies.Location = new System.Drawing.Point(8, 5);
		this.lstboxFrequencies.Name = "lstboxFrequencies";
		this.lstboxFrequencies.Size = new System.Drawing.Size(105, 320);
		this.lstboxFrequencies.TabIndex = 0;
		this.lstboxFrequencies.MouseClick += new System.Windows.Forms.MouseEventHandler(lstboxFrequencies_MouseClick);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackColor = System.Drawing.SystemColors.ControlDark;
		base.ClientSize = new System.Drawing.Size(120, 335);
		base.Controls.Add(this.lstboxFrequencies);
		this.DoubleBuffered = true;
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
		base.Name = "frmQuickRecallPopupList";
		this.Text = "frmQuickRecallPopupList";
		base.ResumeLayout(false);
	}
}
