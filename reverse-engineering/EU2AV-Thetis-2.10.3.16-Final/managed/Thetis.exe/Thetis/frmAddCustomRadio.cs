using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Thetis;

public class frmAddCustomRadio : Form
{
	private IContainer components;

	private LabelTS labelTS1;

	private ButtonTS btnCancel;

	private ButtonTS btnOK;

	private LabelTS labelTS2;

	private ComboBoxTS comboNICS;

	private TextBoxTS txtSpecificRadio;

	private LabelTS labelTS3;

	private ComboBoxTS comboProtocol;

	private LabelTS labelTS4;

	private LabelTS labelTS5;

	private TextBoxTS txtBoard;

	public ComboBoxTS Nics => comboNICS;

	public int Protocol
	{
		get
		{
			if (comboProtocol.SelectedIndex < 0)
			{
				return 0;
			}
			return comboProtocol.SelectedIndex;
		}
	}

	public string RadioIPPort => txtSpecificRadio.Text;

	public string Board
	{
		get
		{
			return "";
		}
		set
		{
			txtBoard.Text = value;
		}
	}

	public frmAddCustomRadio()
	{
		InitializeComponent();
		comboProtocol.SelectedIndex = 0;
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Thetis.frmAddCustomRadio));
		this.labelTS1 = new System.Windows.Forms.LabelTS();
		this.btnCancel = new System.Windows.Forms.ButtonTS();
		this.btnOK = new System.Windows.Forms.ButtonTS();
		this.labelTS2 = new System.Windows.Forms.LabelTS();
		this.comboNICS = new System.Windows.Forms.ComboBoxTS();
		this.txtSpecificRadio = new System.Windows.Forms.TextBoxTS();
		this.labelTS3 = new System.Windows.Forms.LabelTS();
		this.comboProtocol = new System.Windows.Forms.ComboBoxTS();
		this.labelTS4 = new System.Windows.Forms.LabelTS();
		this.labelTS5 = new System.Windows.Forms.LabelTS();
		this.txtBoard = new System.Windows.Forms.TextBoxTS();
		base.SuspendLayout();
		this.labelTS1.AutoSize = true;
		this.labelTS1.Font = new System.Drawing.Font("Courier New", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.labelTS1.Image = null;
		this.labelTS1.Location = new System.Drawing.Point(12, 9);
		this.labelTS1.Name = "labelTS1";
		this.labelTS1.Size = new System.Drawing.Size(463, 240);
		this.labelTS1.TabIndex = 0;
		this.labelTS1.Text = resources.GetString("labelTS1.Text");
		this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.btnCancel.Image = null;
		this.btnCancel.Location = new System.Drawing.Point(408, 358);
		this.btnCancel.Name = "btnCancel";
		this.btnCancel.Selectable = true;
		this.btnCancel.Size = new System.Drawing.Size(86, 27);
		this.btnCancel.TabIndex = 1;
		this.btnCancel.Text = "Cancel";
		this.btnCancel.UseVisualStyleBackColor = true;
		this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
		this.btnOK.Image = null;
		this.btnOK.Location = new System.Drawing.Point(316, 358);
		this.btnOK.Name = "btnOK";
		this.btnOK.Selectable = true;
		this.btnOK.Size = new System.Drawing.Size(86, 27);
		this.btnOK.TabIndex = 2;
		this.btnOK.Text = "Add";
		this.btnOK.UseVisualStyleBackColor = true;
		this.labelTS2.AutoSize = true;
		this.labelTS2.Image = null;
		this.labelTS2.Location = new System.Drawing.Point(34, 268);
		this.labelTS2.Name = "labelTS2";
		this.labelTS2.Size = new System.Drawing.Size(46, 13);
		this.labelTS2.TabIndex = 3;
		this.labelTS2.Text = "Via NIC:";
		this.comboNICS.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.comboNICS.FormattingEnabled = true;
		this.comboNICS.Location = new System.Drawing.Point(86, 265);
		this.comboNICS.Name = "comboNICS";
		this.comboNICS.Size = new System.Drawing.Size(408, 21);
		this.comboNICS.TabIndex = 129;
		this.txtSpecificRadio.Location = new System.Drawing.Point(86, 292);
		this.txtSpecificRadio.Name = "txtSpecificRadio";
		this.txtSpecificRadio.Size = new System.Drawing.Size(242, 20);
		this.txtSpecificRadio.TabIndex = 130;
		this.txtSpecificRadio.Text = "192.168.0.155:1024";
		this.labelTS3.AutoSize = true;
		this.labelTS3.Image = null;
		this.labelTS3.Location = new System.Drawing.Point(10, 295);
		this.labelTS3.Name = "labelTS3";
		this.labelTS3.Size = new System.Drawing.Size(70, 13);
		this.labelTS3.TabIndex = 131;
		this.labelTS3.Text = "Radio IP:Port";
		this.comboProtocol.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.comboProtocol.FormattingEnabled = true;
		this.comboProtocol.Items.AddRange(new object[2] { "Protocol 1", "Protocol 2" });
		this.comboProtocol.Location = new System.Drawing.Point(86, 318);
		this.comboProtocol.Name = "comboProtocol";
		this.comboProtocol.Size = new System.Drawing.Size(150, 21);
		this.comboProtocol.TabIndex = 133;
		this.labelTS4.AutoSize = true;
		this.labelTS4.Image = null;
		this.labelTS4.Location = new System.Drawing.Point(31, 321);
		this.labelTS4.Name = "labelTS4";
		this.labelTS4.Size = new System.Drawing.Size(49, 13);
		this.labelTS4.TabIndex = 132;
		this.labelTS4.Text = "Protocol:";
		this.labelTS5.AutoSize = true;
		this.labelTS5.Image = null;
		this.labelTS5.Location = new System.Drawing.Point(42, 349);
		this.labelTS5.Name = "labelTS5";
		this.labelTS5.Size = new System.Drawing.Size(38, 13);
		this.labelTS5.TabIndex = 134;
		this.labelTS5.Text = "Board:";
		this.txtBoard.Location = new System.Drawing.Point(86, 345);
		this.txtBoard.Name = "txtBoard";
		this.txtBoard.ReadOnly = true;
		this.txtBoard.Size = new System.Drawing.Size(150, 20);
		this.txtBoard.TabIndex = 135;
		base.AcceptButton = this.btnOK;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.CancelButton = this.btnCancel;
		base.ClientSize = new System.Drawing.Size(506, 397);
		base.Controls.Add(this.txtBoard);
		base.Controls.Add(this.labelTS5);
		base.Controls.Add(this.comboProtocol);
		base.Controls.Add(this.labelTS4);
		base.Controls.Add(this.labelTS3);
		base.Controls.Add(this.txtSpecificRadio);
		base.Controls.Add(this.comboNICS);
		base.Controls.Add(this.labelTS2);
		base.Controls.Add(this.btnOK);
		base.Controls.Add(this.btnCancel);
		base.Controls.Add(this.labelTS1);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "frmAddCustomRadio";
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
		this.Text = "Add Custom Radio";
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
