using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Markdig;

namespace Thetis;

public class frmReleaseNotes : Form
{
	private string _releaseNotesPath;

	private IContainer components;

	private ButtonTS btnClose;

	private WebBrowser webBrowser1;

	public frmReleaseNotes()
	{
		InitializeComponent();
		webBrowser1.Navigating += WebBrowser1_Navigating;
	}

	private void btnClose_Click(object sender, EventArgs e)
	{
		Close();
	}

	public void InitPath(string directoryPath)
	{
		_releaseNotesPath = directoryPath;
	}

	public void ShowReleaseNotes()
	{
		try
		{
			if (!base.Visible)
			{
				base.Opacity = 0.0;
				string path = Path.Combine(_releaseNotesPath, "ReleaseNotes.txt");
				if (File.Exists(path))
				{
					string text = Markdown.ToHtml(File.ReadAllText(path));
					string documentText = "<html><head><style>body{font-family: Arial, sans-serif; background-color: black; color: white;}</style></head><body>" + text + "</body></html>";
					webBrowser1.DocumentText = documentText;
					Show();
					Common.FadeIn(this);
				}
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show("Issue showing Release Notes", ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1, (MessageBoxOptions)262144);
		}
	}

	private void WebBrowser1_Navigating(object sender, WebBrowserNavigatingEventArgs e)
	{
		if (!(e.Url.ToString().ToLower() == "about:blank"))
		{
			e.Cancel = true;
			Common.OpenUri(e.Url.ToString());
		}
	}

	private void frmReleaseNotes_FormClosing(object sender, FormClosingEventArgs e)
	{
		if (e.CloseReason == CloseReason.UserClosing)
		{
			Hide();
			e.Cancel = true;
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
		this.btnClose = new System.Windows.Forms.ButtonTS();
		this.webBrowser1 = new System.Windows.Forms.WebBrowser();
		base.SuspendLayout();
		this.btnClose.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.btnClose.Image = null;
		this.btnClose.Location = new System.Drawing.Point(500, 408);
		this.btnClose.Name = "btnClose";
		this.btnClose.Selectable = true;
		this.btnClose.Size = new System.Drawing.Size(112, 30);
		this.btnClose.TabIndex = 0;
		this.btnClose.Text = "&Close";
		this.btnClose.UseVisualStyleBackColor = true;
		this.btnClose.Click += new System.EventHandler(btnClose_Click);
		this.webBrowser1.AllowWebBrowserDrop = false;
		this.webBrowser1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.webBrowser1.IsWebBrowserContextMenuEnabled = false;
		this.webBrowser1.Location = new System.Drawing.Point(0, 0);
		this.webBrowser1.MinimumSize = new System.Drawing.Size(320, 240);
		this.webBrowser1.Name = "webBrowser1";
		this.webBrowser1.ScriptErrorsSuppressed = true;
		this.webBrowser1.Size = new System.Drawing.Size(624, 402);
		this.webBrowser1.TabIndex = 1;
		this.webBrowser1.WebBrowserShortcutsEnabled = false;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(624, 441);
		base.Controls.Add(this.webBrowser1);
		base.Controls.Add(this.btnClose);
		base.MinimizeBox = false;
		base.Name = "frmReleaseNotes";
		base.ShowIcon = false;
		base.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "Release Notes";
		base.TopMost = true;
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(frmReleaseNotes_FormClosing);
		base.ResumeLayout(false);
	}
}
