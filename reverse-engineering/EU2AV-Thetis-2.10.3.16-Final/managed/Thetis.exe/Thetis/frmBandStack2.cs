using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Thetis.Properties;

namespace Thetis;

public class frmBandStack2 : Form
{
	public delegate void EntrySelected(BandStackFilter bsf, BandStackEntry bse, bool updateLastVisited = true, bool obeyHide = true);

	public delegate void EntryAdd(BandStackFilter bsf);

	public delegate void EntryUpdate(BandStackFilter bsf, BandStackEntry bse);

	public delegate void EntryDelete(BandStackFilter bsf, BandStackEntry bse);

	public delegate void IgnoreDupes(bool ignore);

	public delegate void HideOnSelect(bool hideOnSelect);

	public delegate void ShowInSpectrum(bool show);

	public EntrySelected EntrySelectedHandlers;

	public EntryAdd EntryAddHandlers;

	public EntryUpdate EntryUpdateHandlers;

	public EntryDelete EntryDeleteHandlers;

	public IgnoreDupes IgnoreDupeHandlers;

	public HideOnSelect HideOnSelectHandlers;

	public HideOnSelect ShowInSpectrumHandlers;

	private BandStackFilter m_bsf;

	private bool _is_popup;

	private bool _is_popup_on_top;

	private Point _location;

	private bool m_bIgnoreIndexChanged;

	private Console _console;

	private bool _mox;

	private IContainer components;

	private BandStackListBox bandStackListBox;

	private LabelTS lblFilterNameCaption;

	private ToolTip toolTip1;

	private RadioButtonTS radioLastUsedEntry;

	private RadioButtonTS radioSpecific;

	private ButtonTS btnSetSpecific;

	private RadioButtonTS radioLastUsed;

	private ButtonTS btnOptions;

	private ButtonTS btnLockSelected;

	private ButtonTS btnDeleteSelected;

	private ButtonTS btnShowBandStackFilterManager;

	private LabelTS lblFilterName;

	private ButtonTS btnAddStackEntry;

	private CheckBoxTS chkAlwaysOnTop;

	private ButtonTS btnHideSelected;

	private CheckBoxTS chkShowHidden;

	private ButtonTS btnUpdateEntry;

	private CheckBoxTS chkIgnoreDuplicates;

	private CheckBoxTS chkHideOnSelect;

	private CheckBoxTS chkShowInSpectrum;

	public ToolTip ToolTip => toolTip1;

	public frmBandStack2()
	{
		InitializeComponent();
		_console = null;
	}

	public void InitForm(Console console)
	{
		_console = console;
		_mox = _console.MOX;
		Console console2 = _console;
		console2.MoxChangeHandlers = (Console.MoxChanged)Delegate.Combine(console2.MoxChangeHandlers, new Console.MoxChanged(OnMox));
		_is_popup = false;
		_is_popup_on_top = false;
		m_bIgnoreIndexChanged = false;
		bandStackListBox.Items.Clear();
		Common.RestoreForm(this, "BandStack2Form", restore_size: true);
		base.Width = 256;
		btnOptions.Text = "Options >>";
		btnLockSelected.Enabled = false;
		btnDeleteSelected.Enabled = false;
		btnSetSpecific.Enabled = false;
		btnUpdateEntry.Enabled = false;
		Common.ForceFormOnScreen(this);
		_location = base.Location;
	}

	private void OnMox(int rx, bool oldMox, bool newMox)
	{
		if (rx == 1)
		{
			_mox = newMox;
		}
	}

	public void RemoveDelegates()
	{
		if (_console != null)
		{
			Console console = _console;
			console.MoxChangeHandlers = (Console.MoxChanged)Delegate.Remove(console.MoxChangeHandlers, new Console.MoxChanged(OnMox));
		}
	}

	public void InitBandStackFilter(BandStackFilter bsf, bool select = true)
	{
		m_bIgnoreIndexChanged = true;
		bandStackListBox.BeginUpdate();
		bandStackListBox.ClearItems();
		bandStackListBox.SelectedIndex = -1;
		m_bsf = bsf;
		if (m_bsf == null)
		{
			bandStackListBox.EndUpdate();
			return;
		}
		string text = bsf.FilterName;
		if (!bsf.UserDefined && (text.StartsWith("B") || text.StartsWith("b")))
		{
			text = text.Substring(1);
		}
		lblFilterName.Text = text;
		if (!bsf.UserDefined)
		{
			lblFilterName.ForeColor = BandStackManager.BandToColour(BandStackManager.StringToBand(bsf.FilterName));
		}
		else
		{
			lblFilterName.ForeColor = Color.White;
		}
		foreach (BandStackEntry entry in m_bsf.Entries)
		{
			int num = bandStackListBox.AddItem(entry);
			if (select && num == m_bsf.IndexOfCurrent)
			{
				bandStackListBox.SelectedIndex = num;
			}
		}
		bandStackListBox.EndUpdate();
		setupSelectedButtons();
		setupRadioButtons();
		m_bIgnoreIndexChanged = false;
	}

	public void UpdateSelected()
	{
		if (m_bsf == null || m_bsf.Current() == null)
		{
			return;
		}
		m_bIgnoreIndexChanged = true;
		bandStackListBox.BeginUpdate();
		string gUID = m_bsf.Current().GUID;
		for (int i = 0; i < bandStackListBox.Items.Count; i++)
		{
			BandStackEntry bandStackEntry = bandStackListBox.Items[i] as BandStackEntry;
			if (gUID == bandStackEntry.GUID)
			{
				bandStackListBox.SelectedIndex = i;
				break;
			}
		}
		bandStackListBox.EndUpdate();
		setupSelectedButtons();
		m_bIgnoreIndexChanged = false;
	}

	private void setupSelectedButtons()
	{
		if (bandStackListBox.SelectedIndex < 0)
		{
			btnDeleteSelected.Enabled = false;
			btnUpdateEntry.Enabled = false;
			btnLockSelected.Enabled = false;
			if (radioSpecific.Checked)
			{
				btnSetSpecific.Enabled = false;
			}
			return;
		}
		BandStackEntry bandStackEntry = bandStackListBox.Items[bandStackListBox.SelectedIndex] as BandStackEntry;
		btnDeleteSelected.Enabled = !bandStackEntry.Locked;
		btnUpdateEntry.Enabled = !bandStackEntry.Locked;
		btnLockSelected.Text = (bandStackEntry.Locked ? "Unlock Selected" : "Lock Selected");
		btnLockSelected.Enabled = true;
		if (radioSpecific.Checked)
		{
			btnSetSpecific.Enabled = true;
		}
	}

	private void setupRadioButtons(bool bCheck = true)
	{
		if (m_bsf == null)
		{
			return;
		}
		switch (m_bsf.ReturnMode)
		{
		case BandStackFilter.FilterReturnMode.Current:
			if (bCheck)
			{
				radioLastUsedEntry.Checked = true;
			}
			btnSetSpecific.Enabled = false;
			bandStackListBox.SpecificReturnIndex = -1;
			break;
		case BandStackFilter.FilterReturnMode.LastVisited:
			if (bCheck)
			{
				radioLastUsed.Checked = true;
			}
			btnSetSpecific.Enabled = false;
			bandStackListBox.SpecificReturnIndex = -1;
			break;
		case BandStackFilter.FilterReturnMode.Specific:
		{
			if (bCheck)
			{
				radioSpecific.Checked = true;
			}
			btnSetSpecific.Enabled = bandStackListBox.Items.Count > 0;
			bandStackListBox.SpecificReturnIndex = -1;
			for (int i = 0; i < bandStackListBox.Items.Count; i++)
			{
				if ((bandStackListBox.Items[i] as BandStackEntry).GUID == m_bsf.ReturnGUID)
				{
					bandStackListBox.SpecificReturnIndex = i;
					break;
				}
			}
			break;
		}
		}
	}

	private void btnOptions_Click(object sender, EventArgs e)
	{
		if (base.Width > 256)
		{
			base.Width = 256;
			btnOptions.Text = "Options >>";
		}
		else
		{
			base.Width = 512;
			btnOptions.Text = "Options <<";
		}
	}

	private void radioLastUsedEntry_CheckedChanged(object sender, EventArgs e)
	{
		if (m_bsf != null && m_bsf.ReturnMode != BandStackFilter.FilterReturnMode.Current)
		{
			m_bsf.ReturnMode = BandStackFilter.FilterReturnMode.Current;
			setupRadioButtons(bCheck: false);
			bandStackListBox.Invalidate();
		}
	}

	private void radioSpecific_CheckedChanged(object sender, EventArgs e)
	{
		if (m_bsf != null && m_bsf.ReturnMode != BandStackFilter.FilterReturnMode.Specific)
		{
			m_bsf.ReturnMode = BandStackFilter.FilterReturnMode.Specific;
			setupRadioButtons(bCheck: false);
			bandStackListBox.Invalidate();
		}
	}

	private void radioLastUsed_CheckedChanged(object sender, EventArgs e)
	{
		if (m_bsf != null && m_bsf.ReturnMode != BandStackFilter.FilterReturnMode.LastVisited)
		{
			m_bsf.ReturnMode = BandStackFilter.FilterReturnMode.LastVisited;
			setupRadioButtons(bCheck: false);
			bandStackListBox.Invalidate();
		}
	}

	private void btnSetSpecific_Click(object sender, EventArgs e)
	{
		if (m_bsf != null && bandStackListBox.SelectedIndex >= 0)
		{
			BandStackEntry bandStackEntry = bandStackListBox.Items[bandStackListBox.SelectedIndex] as BandStackEntry;
			m_bsf.ReturnGUID = bandStackEntry.GUID;
			bandStackListBox.SpecificReturnIndex = bandStackListBox.SelectedIndex;
			bandStackListBox.Invalidate();
		}
	}

	private void btnLockSelected_Click(object sender, EventArgs e)
	{
		if (m_bsf != null && bandStackListBox.SelectedIndex >= 0)
		{
			BandStackEntry bandStackEntry = bandStackListBox.Items[bandStackListBox.SelectedIndex] as BandStackEntry;
			bandStackEntry.Locked = !bandStackEntry.Locked;
			btnDeleteSelected.Enabled = !bandStackEntry.Locked;
			btnUpdateEntry.Enabled = !bandStackEntry.Locked;
			btnLockSelected.Text = (bandStackEntry.Locked ? "Unlock Selected" : "Lock Selected");
			m_bsf.UpdateEntry(bandStackEntry);
			bandStackListBox.Invalidate();
		}
	}

	private void btnDeleteSelected_Click(object sender, EventArgs e)
	{
		if (m_bsf != null && bandStackListBox.SelectedIndex >= 0 && bandStackListBox.Items.Count != 0)
		{
			BandStackEntry bse = bandStackListBox.Items[bandStackListBox.SelectedIndex] as BandStackEntry;
			EntryDeleteHandlers?.Invoke(m_bsf, bse);
		}
	}

	private void bandStackListBox_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (!m_bIgnoreIndexChanged && !_mox && bandStackListBox.SelectedIndex >= 0)
		{
			BandStackEntry bandStackEntry = bandStackListBox.Items[bandStackListBox.SelectedIndex] as BandStackEntry;
			btnLockSelected.Text = (bandStackEntry.Locked ? "Unlock Selected" : "Lock Selected");
			if (m_bsf.ReturnMode == BandStackFilter.FilterReturnMode.Specific)
			{
				btnSetSpecific.Enabled = bandStackListBox.Items.Count > 0;
			}
			btnLockSelected.Enabled = bandStackListBox.SelectedIndex >= 0;
			btnDeleteSelected.Enabled = bandStackListBox.SelectedIndex >= 0;
			btnUpdateEntry.Enabled = bandStackListBox.SelectedIndex >= 0;
			EntrySelectedHandlers?.Invoke(m_bsf, bandStackEntry);
		}
	}

	private void btnAddStackEntry_Click(object sender, EventArgs e)
	{
		EntryAddHandlers?.Invoke(m_bsf);
	}

	private void chkAlwaysOnTop_CheckedChanged(object sender, EventArgs e)
	{
		base.TopMost = chkAlwaysOnTop.Checked;
	}

	public void HideClose()
	{
		Hide();
		Store();
		if (_is_popup && _is_popup_on_top && !chkAlwaysOnTop.Checked)
		{
			base.TopMost = false;
		}
		_is_popup = false;
		_is_popup_on_top = false;
	}

	private void frmBandStack2_FormClosing(object sender, FormClosingEventArgs e)
	{
		e.Cancel = true;
		HideClose();
	}

	public void Show(bool is_popup = false, Point? popup_location = null, bool on_top = false)
	{
		_is_popup = is_popup;
		_is_popup_on_top = on_top;
		InitBandStackFilter(m_bsf);
		BringToFront();
		if (_is_popup)
		{
			if (on_top)
			{
				base.TopMost = true;
			}
			base.Location = popup_location ?? Point.Empty;
			Common.ForceFormOnScreen(this, shrink_to_fit: false, keep_on_screen: true);
		}
		else
		{
			base.Location = _location;
			Common.ForceFormOnScreen(this);
		}
		base.Show();
	}

	public void Store()
	{
		if (!_is_popup)
		{
			Common.SaveForm(this, "BandStack2Form");
		}
	}

	private void btnUpdateEntry_Click(object sender, EventArgs e)
	{
		if (m_bsf != null && bandStackListBox.SelectedIndex >= 0 && bandStackListBox.Items.Count != 0)
		{
			BandStackEntry bse = bandStackListBox.Items[bandStackListBox.SelectedIndex] as BandStackEntry;
			EntryUpdateHandlers?.Invoke(m_bsf, bse);
		}
	}

	private void chkIgnoreDuplicates_CheckedChanged(object sender, EventArgs e)
	{
		IgnoreDupeHandlers?.Invoke(chkIgnoreDuplicates.Checked);
	}

	private void chkHideOnSelect_CheckedChanged(object sender, EventArgs e)
	{
		HideOnSelectHandlers?.Invoke(chkHideOnSelect.Checked);
	}

	private void chkShowInSpectrum_CheckedChanged(object sender, EventArgs e)
	{
		ShowInSpectrumHandlers?.Invoke(chkShowInSpectrum.Checked);
	}

	private void frmBandStack2_LocationChanged(object sender, EventArgs e)
	{
		if (!_is_popup)
		{
			_location = base.Location;
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
		this.btnHideSelected = new System.Windows.Forms.ButtonTS();
		this.chkAlwaysOnTop = new System.Windows.Forms.CheckBoxTS();
		this.btnAddStackEntry = new System.Windows.Forms.ButtonTS();
		this.lblFilterName = new System.Windows.Forms.LabelTS();
		this.btnShowBandStackFilterManager = new System.Windows.Forms.ButtonTS();
		this.btnDeleteSelected = new System.Windows.Forms.ButtonTS();
		this.btnLockSelected = new System.Windows.Forms.ButtonTS();
		this.btnOptions = new System.Windows.Forms.ButtonTS();
		this.radioLastUsed = new System.Windows.Forms.RadioButtonTS();
		this.btnSetSpecific = new System.Windows.Forms.ButtonTS();
		this.radioSpecific = new System.Windows.Forms.RadioButtonTS();
		this.radioLastUsedEntry = new System.Windows.Forms.RadioButtonTS();
		this.btnUpdateEntry = new System.Windows.Forms.ButtonTS();
		this.chkShowHidden = new System.Windows.Forms.CheckBoxTS();
		this.chkIgnoreDuplicates = new System.Windows.Forms.CheckBoxTS();
		this.chkHideOnSelect = new System.Windows.Forms.CheckBoxTS();
		this.chkShowInSpectrum = new System.Windows.Forms.CheckBoxTS();
		this.lblFilterNameCaption = new System.Windows.Forms.LabelTS();
		this.bandStackListBox = new Thetis.BandStackListBox();
		base.SuspendLayout();
		this.btnHideSelected.Enabled = false;
		this.btnHideSelected.Image = null;
		this.btnHideSelected.Location = new System.Drawing.Point(246, 318);
		this.btnHideSelected.Name = "btnHideSelected";
		this.btnHideSelected.Selectable = true;
		this.btnHideSelected.Size = new System.Drawing.Size(117, 31);
		this.btnHideSelected.TabIndex = 13;
		this.btnHideSelected.Text = "Hide Selected (wip)";
		this.toolTip1.SetToolTip(this.btnHideSelected, "Hides this entry from the results of this filter ONLY");
		this.btnHideSelected.UseVisualStyleBackColor = true;
		this.btnHideSelected.Visible = false;
		this.chkAlwaysOnTop.AutoSize = true;
		this.chkAlwaysOnTop.Image = null;
		this.chkAlwaysOnTop.Location = new System.Drawing.Point(386, 12);
		this.chkAlwaysOnTop.Name = "chkAlwaysOnTop";
		this.chkAlwaysOnTop.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
		this.chkAlwaysOnTop.Size = new System.Drawing.Size(98, 17);
		this.chkAlwaysOnTop.TabIndex = 12;
		this.chkAlwaysOnTop.Text = "Always On Top";
		this.toolTip1.SetToolTip(this.chkAlwaysOnTop, "This window is on top of all others always");
		this.chkAlwaysOnTop.UseVisualStyleBackColor = true;
		this.chkAlwaysOnTop.CheckedChanged += new System.EventHandler(chkAlwaysOnTop_CheckedChanged);
		this.btnAddStackEntry.Image = null;
		this.btnAddStackEntry.Location = new System.Drawing.Point(246, 157);
		this.btnAddStackEntry.Name = "btnAddStackEntry";
		this.btnAddStackEntry.Selectable = true;
		this.btnAddStackEntry.Size = new System.Drawing.Size(117, 64);
		this.btnAddStackEntry.TabIndex = 11;
		this.btnAddStackEntry.Text = "Add New Entry";
		this.toolTip1.SetToolTip(this.btnAddStackEntry, "Adds a new entry");
		this.btnAddStackEntry.UseVisualStyleBackColor = true;
		this.btnAddStackEntry.Click += new System.EventHandler(btnAddStackEntry_Click);
		this.lblFilterName.Font = new System.Drawing.Font("Microsoft Sans Serif", 12f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.lblFilterName.Image = null;
		this.lblFilterName.Location = new System.Drawing.Point(61, 12);
		this.lblFilterName.Name = "lblFilterName";
		this.lblFilterName.Size = new System.Drawing.Size(86, 16);
		this.lblFilterName.TabIndex = 10;
		this.lblFilterName.Text = "xxxxxx";
		this.toolTip1.SetToolTip(this.lblFilterName, "The current filter or description if one has been given");
		this.btnShowBandStackFilterManager.Enabled = false;
		this.btnShowBandStackFilterManager.Image = null;
		this.btnShowBandStackFilterManager.Location = new System.Drawing.Point(369, 355);
		this.btnShowBandStackFilterManager.Name = "btnShowBandStackFilterManager";
		this.btnShowBandStackFilterManager.Selectable = true;
		this.btnShowBandStackFilterManager.Size = new System.Drawing.Size(117, 31);
		this.btnShowBandStackFilterManager.TabIndex = 9;
		this.btnShowBandStackFilterManager.Text = "Filter Manager (wip)";
		this.toolTip1.SetToolTip(this.btnShowBandStackFilterManager, "Show the filter manager");
		this.btnShowBandStackFilterManager.UseVisualStyleBackColor = true;
		this.btnShowBandStackFilterManager.Visible = false;
		this.btnDeleteSelected.Image = null;
		this.btnDeleteSelected.Location = new System.Drawing.Point(246, 355);
		this.btnDeleteSelected.Name = "btnDeleteSelected";
		this.btnDeleteSelected.Selectable = true;
		this.btnDeleteSelected.Size = new System.Drawing.Size(117, 31);
		this.btnDeleteSelected.TabIndex = 8;
		this.btnDeleteSelected.Text = "Delete Selected";
		this.toolTip1.SetToolTip(this.btnDeleteSelected, "Removes this entry. It can not appear in any filter results.");
		this.btnDeleteSelected.UseVisualStyleBackColor = true;
		this.btnDeleteSelected.Click += new System.EventHandler(btnDeleteSelected_Click);
		this.btnLockSelected.Image = null;
		this.btnLockSelected.Location = new System.Drawing.Point(246, 120);
		this.btnLockSelected.Name = "btnLockSelected";
		this.btnLockSelected.Selectable = true;
		this.btnLockSelected.Size = new System.Drawing.Size(117, 31);
		this.btnLockSelected.TabIndex = 7;
		this.btnLockSelected.Text = "Lock Selected";
		this.toolTip1.SetToolTip(this.btnLockSelected, "Locks the currently selected entry so that changes to it are prevented");
		this.btnLockSelected.UseVisualStyleBackColor = true;
		this.btnLockSelected.Click += new System.EventHandler(btnLockSelected_Click);
		this.btnOptions.Image = null;
		this.btnOptions.Location = new System.Drawing.Point(153, 5);
		this.btnOptions.Name = "btnOptions";
		this.btnOptions.Selectable = true;
		this.btnOptions.Size = new System.Drawing.Size(75, 31);
		this.btnOptions.TabIndex = 6;
		this.btnOptions.Text = "Options >>";
		this.toolTip1.SetToolTip(this.btnOptions, "Expand/shrink the options");
		this.btnOptions.UseVisualStyleBackColor = true;
		this.btnOptions.Click += new System.EventHandler(btnOptions_Click);
		this.radioLastUsed.AutoSize = true;
		this.radioLastUsed.Image = null;
		this.radioLastUsed.Location = new System.Drawing.Point(246, 88);
		this.radioLastUsed.Name = "radioLastUsed";
		this.radioLastUsed.Size = new System.Drawing.Size(164, 17);
		this.radioLastUsed.TabIndex = 5;
		this.radioLastUsed.TabStop = true;
		this.radioLastUsed.Text = "Return to last used frequency";
		this.toolTip1.SetToolTip(this.radioLastUsed, "You will return to the last used frequency.");
		this.radioLastUsed.UseVisualStyleBackColor = true;
		this.radioLastUsed.CheckedChanged += new System.EventHandler(radioLastUsed_CheckedChanged);
		this.btnSetSpecific.Image = null;
		this.btnSetSpecific.Location = new System.Drawing.Point(386, 61);
		this.btnSetSpecific.Name = "btnSetSpecific";
		this.btnSetSpecific.Selectable = true;
		this.btnSetSpecific.Size = new System.Drawing.Size(45, 24);
		this.btnSetSpecific.TabIndex = 4;
		this.btnSetSpecific.Text = "Set";
		this.toolTip1.SetToolTip(this.btnSetSpecific, "Set the specific selected entry");
		this.btnSetSpecific.UseVisualStyleBackColor = true;
		this.btnSetSpecific.Click += new System.EventHandler(btnSetSpecific_Click);
		this.radioSpecific.AutoSize = true;
		this.radioSpecific.Image = null;
		this.radioSpecific.Location = new System.Drawing.Point(246, 65);
		this.radioSpecific.Name = "radioSpecific";
		this.radioSpecific.Size = new System.Drawing.Size(134, 17);
		this.radioSpecific.TabIndex = 3;
		this.radioSpecific.TabStop = true;
		this.radioSpecific.Text = "Return to specific entry";
		this.toolTip1.SetToolTip(this.radioSpecific, "You will return to the specific entry. If it has been removed you will return to last use frequency.");
		this.radioSpecific.UseVisualStyleBackColor = true;
		this.radioSpecific.CheckedChanged += new System.EventHandler(radioSpecific_CheckedChanged);
		this.radioLastUsedEntry.AutoSize = true;
		this.radioLastUsedEntry.Image = null;
		this.radioLastUsedEntry.Location = new System.Drawing.Point(246, 42);
		this.radioLastUsedEntry.Name = "radioLastUsedEntry";
		this.radioLastUsedEntry.Size = new System.Drawing.Size(140, 17);
		this.radioLastUsedEntry.TabIndex = 2;
		this.radioLastUsedEntry.TabStop = true;
		this.radioLastUsedEntry.Text = "Return to last used entry";
		this.toolTip1.SetToolTip(this.radioLastUsedEntry, "You will return to the currently selected entry if you leave and come back. If the entry has been remove you will return to last used frequency.");
		this.radioLastUsedEntry.UseVisualStyleBackColor = true;
		this.radioLastUsedEntry.CheckedChanged += new System.EventHandler(radioLastUsedEntry_CheckedChanged);
		this.btnUpdateEntry.Image = null;
		this.btnUpdateEntry.Location = new System.Drawing.Point(369, 157);
		this.btnUpdateEntry.Name = "btnUpdateEntry";
		this.btnUpdateEntry.Selectable = true;
		this.btnUpdateEntry.Size = new System.Drawing.Size(62, 64);
		this.btnUpdateEntry.TabIndex = 15;
		this.btnUpdateEntry.Text = "Update Entry";
		this.toolTip1.SetToolTip(this.btnUpdateEntry, "Update an existing unlocked entry");
		this.btnUpdateEntry.UseVisualStyleBackColor = true;
		this.btnUpdateEntry.Click += new System.EventHandler(btnUpdateEntry_Click);
		this.chkShowHidden.AutoSize = true;
		this.chkShowHidden.Enabled = false;
		this.chkShowHidden.Image = null;
		this.chkShowHidden.Location = new System.Drawing.Point(369, 326);
		this.chkShowHidden.Name = "chkShowHidden";
		this.chkShowHidden.Size = new System.Drawing.Size(88, 17);
		this.chkShowHidden.TabIndex = 14;
		this.chkShowHidden.Text = "Show hidden";
		this.toolTip1.SetToolTip(this.chkShowHidden, "Show any that have been hidden for this filter.");
		this.chkShowHidden.UseVisualStyleBackColor = true;
		this.chkShowHidden.Visible = false;
		this.chkIgnoreDuplicates.AutoSize = true;
		this.chkIgnoreDuplicates.Image = null;
		this.chkIgnoreDuplicates.Location = new System.Drawing.Point(369, 236);
		this.chkIgnoreDuplicates.Name = "chkIgnoreDuplicates";
		this.chkIgnoreDuplicates.Size = new System.Drawing.Size(88, 17);
		this.chkIgnoreDuplicates.TabIndex = 16;
		this.chkIgnoreDuplicates.Text = "Ignore dupes";
		this.toolTip1.SetToolTip(this.chkIgnoreDuplicates, "Do not update if the current VFO frequency already exists");
		this.chkIgnoreDuplicates.UseVisualStyleBackColor = true;
		this.chkIgnoreDuplicates.CheckedChanged += new System.EventHandler(chkIgnoreDuplicates_CheckedChanged);
		this.chkHideOnSelect.AutoSize = true;
		this.chkHideOnSelect.Image = null;
		this.chkHideOnSelect.Location = new System.Drawing.Point(369, 259);
		this.chkHideOnSelect.Name = "chkHideOnSelect";
		this.chkHideOnSelect.Size = new System.Drawing.Size(94, 17);
		this.chkHideOnSelect.TabIndex = 17;
		this.chkHideOnSelect.Text = "Hide on select";
		this.toolTip1.SetToolTip(this.chkHideOnSelect, "Close the band stack window when an item is selected");
		this.chkHideOnSelect.UseVisualStyleBackColor = true;
		this.chkHideOnSelect.CheckedChanged += new System.EventHandler(chkHideOnSelect_CheckedChanged);
		this.chkShowInSpectrum.AutoSize = true;
		this.chkShowInSpectrum.Image = null;
		this.chkShowInSpectrum.Location = new System.Drawing.Point(369, 282);
		this.chkShowInSpectrum.Name = "chkShowInSpectrum";
		this.chkShowInSpectrum.Size = new System.Drawing.Size(126, 17);
		this.chkShowInSpectrum.TabIndex = 18;
		this.chkShowInSpectrum.Text = "Show on Panadapter";
		this.toolTip1.SetToolTip(this.chkShowInSpectrum, "Show the entries as overlays on the panadapter. Click them to select.");
		this.chkShowInSpectrum.UseVisualStyleBackColor = true;
		this.chkShowInSpectrum.CheckedChanged += new System.EventHandler(chkShowInSpectrum_CheckedChanged);
		this.lblFilterNameCaption.AutoSize = true;
		this.lblFilterNameCaption.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.lblFilterNameCaption.Image = null;
		this.lblFilterNameCaption.Location = new System.Drawing.Point(12, 15);
		this.lblFilterNameCaption.Name = "lblFilterNameCaption";
		this.lblFilterNameCaption.Size = new System.Drawing.Size(42, 16);
		this.lblFilterNameCaption.TabIndex = 1;
		this.lblFilterNameCaption.Text = "Filter :";
		this.lblFilterNameCaption.TextAlign = System.Drawing.ContentAlignment.TopRight;
		this.bandStackListBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.bandStackListBox.BackColor = System.Drawing.Color.DarkGray;
		this.bandStackListBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.bandStackListBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
		this.bandStackListBox.Font = new System.Drawing.Font("Calibri", 12f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.bandStackListBox.FormattingEnabled = true;
		this.bandStackListBox.ItemHeight = 19;
		this.bandStackListBox.Location = new System.Drawing.Point(12, 42);
		this.bandStackListBox.LockImageLocked = Thetis.Properties.Resources.lock_locked_red;
		this.bandStackListBox.LockImageUnLocked = Thetis.Properties.Resources.lock_unlocked_grey;
		this.bandStackListBox.Memory = null;
		this.bandStackListBox.Name = "bandStackListBox";
		this.bandStackListBox.Size = new System.Drawing.Size(216, 344);
		this.bandStackListBox.SpecificReturnImage = Thetis.Properties.Resources.return_green;
		this.bandStackListBox.SpecificReturnIndex = -1;
		this.bandStackListBox.TabIndex = 0;
		this.bandStackListBox.SelectedIndexChanged += new System.EventHandler(bandStackListBox_SelectedIndexChanged);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackColor = System.Drawing.SystemColors.ControlDark;
		base.ClientSize = new System.Drawing.Size(496, 401);
		base.Controls.Add(this.chkShowInSpectrum);
		base.Controls.Add(this.chkHideOnSelect);
		base.Controls.Add(this.chkIgnoreDuplicates);
		base.Controls.Add(this.btnUpdateEntry);
		base.Controls.Add(this.chkShowHidden);
		base.Controls.Add(this.btnHideSelected);
		base.Controls.Add(this.chkAlwaysOnTop);
		base.Controls.Add(this.btnAddStackEntry);
		base.Controls.Add(this.lblFilterName);
		base.Controls.Add(this.btnShowBandStackFilterManager);
		base.Controls.Add(this.btnDeleteSelected);
		base.Controls.Add(this.btnLockSelected);
		base.Controls.Add(this.btnOptions);
		base.Controls.Add(this.radioLastUsed);
		base.Controls.Add(this.btnSetSpecific);
		base.Controls.Add(this.radioSpecific);
		base.Controls.Add(this.radioLastUsedEntry);
		base.Controls.Add(this.lblFilterNameCaption);
		base.Controls.Add(this.bandStackListBox);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
		base.Name = "frmBandStack2";
		this.RightToLeft = System.Windows.Forms.RightToLeft.No;
		base.ShowInTaskbar = false;
		this.Text = "Band Stack 2";
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(frmBandStack2_FormClosing);
		base.LocationChanged += new System.EventHandler(frmBandStack2_LocationChanged);
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
