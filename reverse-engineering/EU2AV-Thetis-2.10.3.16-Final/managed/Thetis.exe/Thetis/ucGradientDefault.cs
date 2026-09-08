using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Thetis;

public class ucGradientDefault : UserControl
{
	public const string DEFAULT_GRADIENT_PANADAPTOR = "9|1|0.000|-2147418368|1|0.494|-2130771968|1|0.341|-2147418368|1|0.432|-2130745856|0|0.669|-1493237760|0|0.159|-1|0|0.881|-65536|0|0.125|-32704|1|1.000|-2130771968|";

	public const string DEFAULT_GRADIENT_WATERFALL = "9|1|0.000|-16777216|0|0.678|-65536|1|0.545|-16711936|1|0.746|-39424|1|0.223|-13395610|0|0.159|-1|0|0.881|-65536|0|0.125|-32704|1|1.000|-65536|";

	public const string DEFAULT_GRADIENT_SPECTRAL_SERVER = "9|1|0.000|-16777216|1|0.494|-65536|1|0.341|-16711936|1|0.432|-39424|1|0.159|-16777216|0|0.159|-1|0|0.881|-65536|0|0.125|-32704|1|1.000|-65536|";

	private bool _is_panadaptor;

	private readonly (string name, string gradient, bool is_panadaptor)[] _gradients = new(string, string, bool)[10]
	{
		("Graphite", "9|1|0.000|-16777216|1|0.181|-8421505|0|0.644|-256|0|0.144|-16777216|0|0.669|-1493237760|0|0.159|-1|0|0.881|-65536|0|0.125|-32704|1|1.000|-1|", true),
		("Lemon", "9|1|0.000|-16777216|1|0.181|-8421632|1|0.644|-256|0|0.144|-16777216|0|0.669|-1493237760|0|0.159|-1|0|0.881|-65536|0|0.125|-32704|1|1.000|-1|", true),
		("Ice", "9|1|0.000|-16777216|1|0.262|-13408513|1|0.877|-1|1|0.458|-16724737|0|0.669|-1493237760|0|0.159|-1|0|0.881|-65536|0|0.125|-32704|1|1.000|-1|", true),
		("Fire", "9|1|0.000|-16777216|1|0.332|-39424|1|0.539|-52480|0|0.569|-19841|0|0.669|-1493237760|0|0.159|-1|0|0.881|-65536|0|0.125|-32704|1|1.000|-256|", true),
		("Rainbow", "9|1|0.000|-16777216|1|0.419|-16711681|1|0.168|-5279256|1|0.712|-256|1|0.859|-39424|1|0.558|-16711936|1|0.288|-6697729|1|0.097|-16777216|1|1.000|-65536|", true),
		("Graphite", "9|1|0.000|-16777216|1|0.181|-8421505|0|0.644|-256|0|0.144|-16777216|0|0.669|-1493237760|0|0.159|-1|0|0.881|-65536|0|0.125|-32704|1|1.000|-1|", false),
		("Lemon", "9|1|0.000|-16777216|1|0.181|-8421632|1|0.644|-256|0|0.144|-16777216|0|0.669|-1493237760|0|0.159|-1|0|0.881|-65536|0|0.125|-32704|1|1.000|-1|", false),
		("Ice", "9|1|0.000|-16777216|1|0.262|-13408513|1|0.877|-1|1|0.458|-16724737|0|0.669|-1493237760|0|0.159|-1|0|0.881|-65536|0|0.125|-32704|1|1.000|-1|", false),
		("Fire", "9|1|0.000|-16777216|1|0.332|-39424|1|0.539|-52480|0|0.569|-19841|0|0.669|-1493237760|0|0.159|-1|0|0.881|-65536|0|0.125|-32704|1|1.000|-256|", false),
		("Rainbow", "9|1|0.000|-16777216|1|0.419|-16711681|1|0.168|-5279256|1|0.712|-256|1|0.859|-39424|1|0.558|-16711936|1|0.288|-6697729|1|0.097|-16777216|1|1.000|-65536|", false)
	};

	private IContainer components;

	private ComboBoxTS comboGradient;

	private ButtonTS btnSet;

	[Category("Appearance")]
	[Description("Determines if the gradient is for a panadaptor or waterfall.")]
	public bool IsPanadaptor
	{
		get
		{
			return _is_panadaptor;
		}
		set
		{
			_is_panadaptor = value;
			populateGradientList();
		}
	}

	public event Action<bool, string> SetGradient;

	public ucGradientDefault()
	{
		InitializeComponent();
		populateGradientList();
	}

	private void populateGradientList()
	{
		comboGradient.Items.Clear();
		foreach (var item in _gradients.Where(((string name, string gradient, bool is_panadaptor) g) => g.is_panadaptor == _is_panadaptor))
		{
			comboGradient.Items.Add(item.name);
		}
		if (comboGradient.Items.Count > 0)
		{
			comboGradient.SelectedIndex = 0;
		}
	}

	private void btnSet_Click(object sender, EventArgs e)
	{
		if (comboGradient.SelectedIndex >= 0)
		{
			string arg = (from g in _gradients
				where g.name.Equals(comboGradient.Text, StringComparison.OrdinalIgnoreCase)
				select g.gradient).FirstOrDefault() ?? (_is_panadaptor ? "9|1|0.000|-2147418368|1|0.494|-2130771968|1|0.341|-2147418368|1|0.432|-2130745856|0|0.669|-1493237760|0|0.159|-1|0|0.881|-65536|0|0.125|-32704|1|1.000|-2130771968|" : "9|1|0.000|-16777216|0|0.678|-65536|1|0.545|-16711936|1|0.746|-39424|1|0.223|-13395610|0|0.159|-1|0|0.881|-65536|0|0.125|-32704|1|1.000|-65536|");
			SetGradient?.Invoke(_is_panadaptor, arg);
		}
	}

	protected override void OnEnabledChanged(EventArgs e)
	{
		base.OnEnabledChanged(e);
		setControlState(this, base.Enabled);
	}

	private void setControlState(Control parent, bool enabled)
	{
		foreach (Control control in parent.Controls)
		{
			control.Enabled = enabled;
			if (!enabled)
			{
				control.ForeColor = SystemColors.GrayText;
			}
			else
			{
				control.ForeColor = SystemColors.ControlText;
			}
			setControlState(control, enabled);
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
		this.comboGradient = new System.Windows.Forms.ComboBoxTS();
		this.btnSet = new System.Windows.Forms.ButtonTS();
		base.SuspendLayout();
		this.comboGradient.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.comboGradient.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.comboGradient.FormattingEnabled = true;
		this.comboGradient.Items.AddRange(new object[5] { "graphite", "lemon", "ice", "fire", "rainbow" });
		this.comboGradient.Location = new System.Drawing.Point(0, 0);
		this.comboGradient.Name = "comboGradient";
		this.comboGradient.Size = new System.Drawing.Size(161, 21);
		this.comboGradient.TabIndex = 0;
		this.btnSet.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.btnSet.Image = null;
		this.btnSet.Location = new System.Drawing.Point(167, 0);
		this.btnSet.Name = "btnSet";
		this.btnSet.Selectable = true;
		this.btnSet.Size = new System.Drawing.Size(64, 21);
		this.btnSet.TabIndex = 1;
		this.btnSet.Text = "Set";
		this.btnSet.UseVisualStyleBackColor = true;
		this.btnSet.Click += new System.EventHandler(btnSet_Click);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.btnSet);
		base.Controls.Add(this.comboGradient);
		base.Name = "ucGradientDefault";
		base.Size = new System.Drawing.Size(231, 21);
		base.ResumeLayout(false);
	}
}
