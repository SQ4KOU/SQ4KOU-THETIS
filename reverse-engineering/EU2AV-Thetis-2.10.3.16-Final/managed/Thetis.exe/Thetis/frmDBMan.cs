using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using Thetis.Properties;

namespace Thetis;

public class frmDBMan : Form
{
	private bool _allow_check_change;

	private bool _ignore_lstActiveDBs_selectectedindexchanged;

	private bool _restore;

	private IContainer components;

	private ButtonTS btnNewDB;

	private ButtonTS btnRemoveDB;

	private ButtonTS btnTakeBackupNow;

	private ButtonTS btnBackupOnStart;

	private ButtonTS btnBackupOnShutdown;

	private ButtonTS btnRemoveBackup;

	private ButtonTS btnMakeBackupAvailable;

	private ListView lstActiveDBs;

	private ListView lstBackups;

	private LabelTS lblDabaseBackups_active_selected;

	private LabelTS labelTS2;

	private ButtonTS btnMakeActive;

	private ButtonTS btnDuplicateDB;

	private ColumnHeader colDesc;

	private ColumnHeader colChanged;

	private ColumnHeader colAge;

	private ColumnHeader colSize;

	private ColumnHeader colFolder;

	private ColumnHeader colHardware;

	private ColumnHeader colBackupOnStartup;

	private ColumnHeader colBackupOnShutdown;

	private ToolTip toolTip1;

	private ColumnHeader colTimeDate;

	private ColumnHeader colBackupAge;

	private ColumnHeader colBackupFilename;

	private ColumnHeader colVersion;

	private ButtonTS btnImport;

	private ButtonTS btnExport;

	private ButtonTS btnRename;

	private ButtonTS btnExportBackup;

	private ButtonTS btnOpenFolder;

	private ButtonTS btnImport_to_available_list;

	private LabelTS labelTS3;

	private LabelTS labelTS4;

	private ColumnHeader colDescription;

	private ButtonTS btnRenameBackup;

	private CheckBoxTS chkPruneBackups;

	private PictureBox picTick;

	public bool PruneBackups
	{
		get
		{
			return chkPruneBackups.Checked;
		}
		set
		{
			chkPruneBackups.Checked = value;
		}
	}

	public frmDBMan()
	{
		_restore = false;
		_allow_check_change = true;
		_ignore_lstActiveDBs_selectectedindexchanged = false;
		InitializeComponent();
		base.TopMost = true;
		Common.DoubleBufferAll(this, enabled: true);
		Text = "Database Manager  [v" + Common.GetVerNum() + "]";
	}

	public void Restore()
	{
		_restore = true;
		Common.RestoreForm(this, "DBManForm", restore_size: true);
		_restore = false;
	}

	private string localDateTimeFormat(DateTime dateTime)
	{
		CultureInfo currentCulture = CultureInfo.CurrentCulture;
		try
		{
			CultureInfo installedUICulture = CultureInfo.InstalledUICulture;
			return dateTime.ToString("G", installedUICulture);
		}
		catch
		{
			return dateTime.ToString("G");
		}
		finally
		{
			CultureInfo.CurrentCulture = currentCulture;
		}
	}

	private string formatTimeSpanWithYears(TimeSpan difference)
	{
		int days = difference.Days;
		int num = days / 365;
		int num2 = days % 365;
		if (num > 0)
		{
			if (num2 > 0)
			{
				return string.Format("{0}y {1}d {2}:{3}:{4}", num, num2, difference.Hours.ToString("00"), difference.Minutes.ToString("00"), difference.Seconds.ToString("00"));
			}
			return string.Format("{0}y {1}:{2}:{3}", num, difference.Hours.ToString("00"), difference.Minutes.ToString("00"), difference.Seconds.ToString("00"));
		}
		if (num2 > 0)
		{
			return string.Format("{0}d {1}:{2}:{3}", num2, difference.Hours.ToString("00"), difference.Minutes.ToString("00"), difference.Seconds.ToString("00"));
		}
		return difference.Hours.ToString("00") + ":" + difference.Minutes.ToString("00") + ":" + difference.Seconds.ToString("00");
	}

	internal void InitBackups(List<DBMan.BackupFileInfo> backups)
	{
		lstBackups.Items.Clear();
		foreach (DBMan.BackupFileInfo backup in backups)
		{
			ListViewItem listViewItem = new ListViewItem(backup.Description);
			listViewItem.SubItems.Add(localDateTimeFormat(backup.DateTimeOfBackup));
			TimeSpan difference = DateTime.Now - backup.DateTimeOfBackup;
			string text = formatTimeSpanWithYears(difference);
			listViewItem.SubItems.Add(text);
			listViewItem.SubItems.Add(backup.FullFilePath);
			listViewItem.Tag = backup.FullFilePath;
			lstBackups.Items.Add(listViewItem);
		}
		lstBackups.ColumnWidthChanging -= lstBackups_ColumnWidthChanging;
		foreach (ColumnHeader column in lstBackups.Columns)
		{
			column.Width = -2;
		}
		lstBackups.ColumnWidthChanging += lstBackups_ColumnWidthChanging;
		lstBackups.Enabled = lstBackups.Items.Count > 0;
		lstBackups_SelectedIndexChanged(this, EventArgs.Empty);
	}

	internal void InitAvailableDBs(Dictionary<Guid, DBMan.DatabaseInfo> dbs, Guid active_guid, Guid reselect_guid)
	{
		bool allow_check_change = _allow_check_change;
		_allow_check_change = true;
		_ignore_lstActiveDBs_selectectedindexchanged = true;
		lstActiveDBs.Items.Clear();
		foreach (KeyValuePair<Guid, DBMan.DatabaseInfo> db in dbs)
		{
			DBMan.DatabaseInfo value = db.Value;
			ListViewItem listViewItem = new ListViewItem(value.Description);
			listViewItem.Checked = value.GUID == active_guid;
			listViewItem.SubItems.Add(HardwareSpecific.EnumModelToString(value.Model));
			listViewItem.SubItems.Add(localDateTimeFormat(value.LastChanged));
			TimeSpan difference = DateTime.Now - value.CreationTime;
			string text = formatTimeSpanWithYears(difference);
			listViewItem.SubItems.Add(text);
			listViewItem.SubItems.Add(getReadableFileSize(value.Size));
			listViewItem.SubItems.Add(value.BackupOnStartup ? "yes" : "no");
			listViewItem.SubItems.Add(value.BackupOnShutdown ? "yes" : "no");
			listViewItem.SubItems.Add(value.VersionNumber);
			listViewItem.SubItems.Add(value.FullPath);
			listViewItem.Tag = value.GUID.ToString();
			if (reselect_guid != Guid.Empty && value.GUID == reselect_guid)
			{
				listViewItem.Selected = true;
			}
			lstActiveDBs.Items.Add(listViewItem);
		}
		lstActiveDBs.ColumnWidthChanging -= lstActiveDBs_ColumnWidthChanging;
		foreach (ColumnHeader column in lstActiveDBs.Columns)
		{
			column.Width = -2;
		}
		lstActiveDBs.ColumnWidthChanging += lstActiveDBs_ColumnWidthChanging;
		_ignore_lstActiveDBs_selectectedindexchanged = false;
		lstActiveDBs_SelectedIndexChanged(this, EventArgs.Empty);
		_allow_check_change = allow_check_change;
	}

	private string getReadableFileSize(long byteSize)
	{
		if (byteSize >= 1048576)
		{
			return $"{(double)byteSize / 1048576.0:0.00}MB";
		}
		if (byteSize >= 1024)
		{
			return $"{(double)byteSize / 1024.0:0.00}KB";
		}
		return $"{byteSize}B";
	}

	private void lstActiveDBs_ItemCheck(object sender, ItemCheckEventArgs e)
	{
		if (!_allow_check_change)
		{
			e.NewValue = e.CurrentValue;
		}
	}

	private void frmDBMan_Shown(object sender, EventArgs e)
	{
		_allow_check_change = false;
	}

	private void frmDBMan_FormClosing(object sender, FormClosingEventArgs e)
	{
		if (e.CloseReason == CloseReason.UserClosing)
		{
			Common.SaveForm(this, "DBManForm");
			_allow_check_change = true;
		}
	}

	private void btnMakeActive_Click(object sender, EventArgs e)
	{
		if (lstActiveDBs.SelectedItems.Count == 1)
		{
			ListViewItem listViewItem = lstActiveDBs.SelectedItems[0];
			DBMan.MakeActiveDB(new Guid(listViewItem.Tag.ToString()));
		}
	}

	private void btnNewDB_Click(object sender, EventArgs e)
	{
		DBMan.NewDB();
	}

	private void btnDuplicateBD_Click(object sender, EventArgs e)
	{
		if (lstActiveDBs.SelectedItems.Count == 1)
		{
			ListViewItem listViewItem = lstActiveDBs.SelectedItems[0];
			DBMan.DuplicateDB(new Guid(listViewItem.Tag.ToString()));
		}
	}

	private void btnRemoveDB_Click(object sender, EventArgs e)
	{
		if (lstActiveDBs.SelectedItems.Count == 1)
		{
			ListViewItem listViewItem = lstActiveDBs.SelectedItems[0];
			DBMan.RemoveDB(new Guid(listViewItem.Tag.ToString()));
		}
	}

	private void lstActiveDBs_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (!_ignore_lstActiveDBs_selectectedindexchanged)
		{
			bool enabled = lstActiveDBs.SelectedItems.Count != 0;
			btnNewDB.Enabled = true;
			btnTakeBackupNow.Enabled = true;
			btnImport.Enabled = true;
			btnRename.Enabled = enabled;
			btnExport.Enabled = enabled;
			btnOpenFolder.Enabled = enabled;
			btnDuplicateDB.Enabled = enabled;
			btnBackupOnStart.Enabled = enabled;
			btnBackupOnShutdown.Enabled = enabled;
			if (lstActiveDBs.SelectedItems.Count == 1 && lstActiveDBs.SelectedItems[0].Checked)
			{
				enabled = false;
			}
			btnMakeActive.Enabled = enabled;
			btnRemoveDB.Enabled = enabled;
			Guid guid = Guid.Empty;
			if (lstActiveDBs.SelectedItems.Count == 1)
			{
				ListViewItem listViewItem = lstActiveDBs.SelectedItems[0];
				guid = new Guid(listViewItem.Tag.ToString());
				string text = "";
				text = ((!listViewItem.Checked) ? "SELECTED available" : (text = "currently ACTIVE"));
				lblDabaseBackups_active_selected.Text = "Database Backups for " + text + " database";
			}
			else
			{
				lblDabaseBackups_active_selected.Text = "Database Backups for currently ACTIVE database";
			}
			DBMan.SelectedAvailable(guid);
		}
	}

	private void btnTakeBackupNow_Click(object sender, EventArgs e)
	{
		Guid highlighted = Guid.Empty;
		if (lstActiveDBs.SelectedItems.Count == 1)
		{
			ListViewItem listViewItem = lstActiveDBs.SelectedItems[0];
			highlighted = new Guid(listViewItem.Tag.ToString());
		}
		DBMan.TakeBackup(highlighted);
	}

	private void lstBackups_SelectedIndexChanged(object sender, EventArgs e)
	{
		btnMakeBackupAvailable.Enabled = lstBackups.SelectedItems.Count == 1;
		btnExportBackup.Enabled = lstBackups.SelectedItems.Count == 1;
		btnRenameBackup.Enabled = lstBackups.SelectedItems.Count == 1;
		btnRemoveBackup.Enabled = lstBackups.SelectedItems.Count > 0;
	}

	private void btnMakeBackupAvailable_Click(object sender, EventArgs e)
	{
		if (lstBackups.SelectedItems.Count == 1)
		{
			DBMan.MakeBackupAvailable(lstBackups.SelectedItems[0].Tag.ToString());
		}
	}

	private void btnRemoveBackup_Click(object sender, EventArgs e)
	{
		if (lstBackups.SelectedItems.Count < 1)
		{
			return;
		}
		List<string> list = new List<string>();
		foreach (ListViewItem selectedItem in lstBackups.SelectedItems)
		{
			list.Add(selectedItem.Tag.ToString());
		}
		DBMan.RemoveBackupDB(list);
		Guid guid = Guid.Empty;
		if (lstActiveDBs.SelectedItems.Count == 1)
		{
			ListViewItem listViewItem2 = lstActiveDBs.SelectedItems[0];
			guid = new Guid(listViewItem2.Tag.ToString());
		}
		DBMan.SelectedAvailable(guid);
	}

	private void lstActiveDBs_ColumnWidthChanging(object sender, ColumnWidthChangingEventArgs e)
	{
		e.NewWidth = lstActiveDBs.Columns[e.ColumnIndex].Width;
		e.Cancel = true;
	}

	private void lstBackups_ColumnWidthChanging(object sender, ColumnWidthChangingEventArgs e)
	{
		e.NewWidth = lstBackups.Columns[e.ColumnIndex].Width;
		e.Cancel = true;
	}

	private void btnBackupOnStart_Click(object sender, EventArgs e)
	{
		if (lstActiveDBs.SelectedItems.Count == 1)
		{
			ListViewItem listViewItem = lstActiveDBs.SelectedItems[0];
			DBMan.BackupOnStartUpToggle(new Guid(listViewItem.Tag.ToString()));
		}
	}

	private void btnBackupOnShutdown_Click(object sender, EventArgs e)
	{
		if (lstActiveDBs.SelectedItems.Count == 1)
		{
			ListViewItem listViewItem = lstActiveDBs.SelectedItems[0];
			DBMan.BackupOnShutDownToggle(new Guid(listViewItem.Tag.ToString()));
		}
	}

	private void btnImport_Click(object sender, EventArgs e)
	{
		DBMan.Import();
	}

	private void btnExport_Click(object sender, EventArgs e)
	{
		if (lstActiveDBs.SelectedItems.Count == 1)
		{
			ListViewItem listViewItem = lstActiveDBs.SelectedItems[0];
			DBMan.Export(new Guid(listViewItem.Tag.ToString()));
		}
	}

	private void btnRename_Click(object sender, EventArgs e)
	{
		if (lstActiveDBs.SelectedItems.Count == 1)
		{
			ListViewItem listViewItem = lstActiveDBs.SelectedItems[0];
			DBMan.Rename(new Guid(listViewItem.Tag.ToString()));
		}
	}

	private void btnExportBackup_Click(object sender, EventArgs e)
	{
		if (lstBackups.SelectedItems.Count == 1)
		{
			ListViewItem listViewItem = lstBackups.SelectedItems[0];
			DBMan.ExportBackup(listViewItem.Text, listViewItem.Tag.ToString());
		}
	}

	private void btnOpenFolder_Click(object sender, EventArgs e)
	{
		if (lstActiveDBs.SelectedItems.Count == 1)
		{
			ListViewItem listViewItem = lstActiveDBs.SelectedItems[0];
			DBMan.OpenFolder(new Guid(listViewItem.Tag.ToString()));
		}
	}

	private void btnImport_to_available_list_Click(object sender, EventArgs e)
	{
		Guid selected = Guid.Empty;
		if (lstActiveDBs.SelectedItems.Count == 1)
		{
			ListViewItem listViewItem = lstActiveDBs.SelectedItems[0];
			selected = new Guid(listViewItem.Tag.ToString());
		}
		DBMan.ImportAsAvailable(selected);
	}

	private void btnRenameBackup_Click(object sender, EventArgs e)
	{
		if (lstBackups.SelectedItems.Count == 1)
		{
			Guid guid = Guid.Empty;
			ListViewItem listViewItem;
			if (lstActiveDBs.SelectedItems.Count == 1)
			{
				listViewItem = lstActiveDBs.SelectedItems[0];
				guid = new Guid(listViewItem.Tag.ToString());
			}
			listViewItem = lstBackups.SelectedItems[0];
			DBMan.RenameBackup(guid, listViewItem.Tag.ToString());
		}
	}

	private void chkPruneBackups_CheckedChanged(object sender, EventArgs e)
	{
		if (!_restore)
		{
			DBMan.PruneBackups = chkPruneBackups.Checked;
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
		System.Windows.Forms.ListViewItem listViewItem = new System.Windows.Forms.ListViewItem(new string[2] { "test", "sub1" }, -1);
		System.Windows.Forms.ListViewItem listViewItem2 = new System.Windows.Forms.ListViewItem("test");
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Thetis.frmDBMan));
		this.lstActiveDBs = new System.Windows.Forms.ListView();
		this.colDesc = new System.Windows.Forms.ColumnHeader();
		this.colHardware = new System.Windows.Forms.ColumnHeader();
		this.colChanged = new System.Windows.Forms.ColumnHeader();
		this.colAge = new System.Windows.Forms.ColumnHeader();
		this.colSize = new System.Windows.Forms.ColumnHeader();
		this.colBackupOnStartup = new System.Windows.Forms.ColumnHeader();
		this.colBackupOnShutdown = new System.Windows.Forms.ColumnHeader();
		this.colVersion = new System.Windows.Forms.ColumnHeader();
		this.colFolder = new System.Windows.Forms.ColumnHeader();
		this.lstBackups = new System.Windows.Forms.ListView();
		this.colDescription = new System.Windows.Forms.ColumnHeader();
		this.colTimeDate = new System.Windows.Forms.ColumnHeader();
		this.colBackupAge = new System.Windows.Forms.ColumnHeader();
		this.colBackupFilename = new System.Windows.Forms.ColumnHeader();
		this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
		this.btnRenameBackup = new System.Windows.Forms.ButtonTS();
		this.btnImport_to_available_list = new System.Windows.Forms.ButtonTS();
		this.btnOpenFolder = new System.Windows.Forms.ButtonTS();
		this.btnExportBackup = new System.Windows.Forms.ButtonTS();
		this.btnRename = new System.Windows.Forms.ButtonTS();
		this.btnExport = new System.Windows.Forms.ButtonTS();
		this.btnImport = new System.Windows.Forms.ButtonTS();
		this.btnDuplicateDB = new System.Windows.Forms.ButtonTS();
		this.btnMakeActive = new System.Windows.Forms.ButtonTS();
		this.btnMakeBackupAvailable = new System.Windows.Forms.ButtonTS();
		this.btnRemoveBackup = new System.Windows.Forms.ButtonTS();
		this.btnBackupOnShutdown = new System.Windows.Forms.ButtonTS();
		this.btnBackupOnStart = new System.Windows.Forms.ButtonTS();
		this.btnTakeBackupNow = new System.Windows.Forms.ButtonTS();
		this.btnRemoveDB = new System.Windows.Forms.ButtonTS();
		this.btnNewDB = new System.Windows.Forms.ButtonTS();
		this.chkPruneBackups = new System.Windows.Forms.CheckBoxTS();
		this.labelTS4 = new System.Windows.Forms.LabelTS();
		this.labelTS3 = new System.Windows.Forms.LabelTS();
		this.labelTS2 = new System.Windows.Forms.LabelTS();
		this.lblDabaseBackups_active_selected = new System.Windows.Forms.LabelTS();
		this.picTick = new System.Windows.Forms.PictureBox();
		((System.ComponentModel.ISupportInitialize)this.picTick).BeginInit();
		base.SuspendLayout();
		this.lstActiveDBs.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.lstActiveDBs.CheckBoxes = true;
		this.lstActiveDBs.Columns.AddRange(new System.Windows.Forms.ColumnHeader[9] { this.colDesc, this.colHardware, this.colChanged, this.colAge, this.colSize, this.colBackupOnStartup, this.colBackupOnShutdown, this.colVersion, this.colFolder });
		this.lstActiveDBs.FullRowSelect = true;
		this.lstActiveDBs.GridLines = true;
		this.lstActiveDBs.HideSelection = false;
		listViewItem.StateImageIndex = 0;
		this.lstActiveDBs.Items.AddRange(new System.Windows.Forms.ListViewItem[1] { listViewItem });
		this.lstActiveDBs.Location = new System.Drawing.Point(12, 22);
		this.lstActiveDBs.MultiSelect = false;
		this.lstActiveDBs.Name = "lstActiveDBs";
		this.lstActiveDBs.ShowItemToolTips = true;
		this.lstActiveDBs.Size = new System.Drawing.Size(524, 334);
		this.lstActiveDBs.TabIndex = 7;
		this.lstActiveDBs.UseCompatibleStateImageBehavior = false;
		this.lstActiveDBs.View = System.Windows.Forms.View.Details;
		this.lstActiveDBs.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(lstActiveDBs_ItemCheck);
		this.lstActiveDBs.SelectedIndexChanged += new System.EventHandler(lstActiveDBs_SelectedIndexChanged);
		this.colDesc.Text = "Description";
		this.colDesc.Width = 128;
		this.colHardware.Text = "Hardware";
		this.colHardware.Width = 66;
		this.colChanged.Text = "Last Changed";
		this.colChanged.Width = 70;
		this.colAge.Text = "Age";
		this.colSize.Text = "Size";
		this.colBackupOnStartup.Text = "Startup Backup";
		this.colBackupOnShutdown.Text = "Shutdown Backup";
		this.colVersion.Text = "Version";
		this.colFolder.Text = "Folder";
		this.lstBackups.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.lstBackups.Columns.AddRange(new System.Windows.Forms.ColumnHeader[4] { this.colDescription, this.colTimeDate, this.colBackupAge, this.colBackupFilename });
		this.lstBackups.FullRowSelect = true;
		this.lstBackups.GridLines = true;
		this.lstBackups.HideSelection = false;
		this.lstBackups.Items.AddRange(new System.Windows.Forms.ListViewItem[1] { listViewItem2 });
		this.lstBackups.Location = new System.Drawing.Point(12, 382);
		this.lstBackups.Name = "lstBackups";
		this.lstBackups.ShowItemToolTips = true;
		this.lstBackups.Size = new System.Drawing.Size(524, 207);
		this.lstBackups.TabIndex = 8;
		this.lstBackups.UseCompatibleStateImageBehavior = false;
		this.lstBackups.View = System.Windows.Forms.View.Details;
		this.lstBackups.SelectedIndexChanged += new System.EventHandler(lstBackups_SelectedIndexChanged);
		this.colDescription.Text = "Description";
		this.colDescription.Width = 90;
		this.colTimeDate.Text = "TimeDate";
		this.colBackupAge.Text = "Age";
		this.colBackupFilename.Text = "Filename";
		this.btnRenameBackup.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.btnRenameBackup.Image = Thetis.Properties.Resources.Data_Edit_32;
		this.btnRenameBackup.Location = new System.Drawing.Point(542, 478);
		this.btnRenameBackup.Name = "btnRenameBackup";
		this.btnRenameBackup.Selectable = true;
		this.btnRenameBackup.Size = new System.Drawing.Size(42, 42);
		this.btnRenameBackup.TabIndex = 30;
		this.toolTip1.SetToolTip(this.btnRenameBackup, "Change description for the selected database backup");
		this.btnRenameBackup.UseVisualStyleBackColor = true;
		this.btnRenameBackup.Click += new System.EventHandler(btnRenameBackup_Click);
		this.btnImport_to_available_list.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.btnImport_to_available_list.Image = Thetis.Properties.Resources.Data_Import_2_32;
		this.btnImport_to_available_list.Location = new System.Drawing.Point(590, 22);
		this.btnImport_to_available_list.Name = "btnImport_to_available_list";
		this.btnImport_to_available_list.Selectable = true;
		this.btnImport_to_available_list.Size = new System.Drawing.Size(42, 42);
		this.btnImport_to_available_list.TabIndex = 27;
		this.toolTip1.SetToolTip(this.btnImport_to_available_list, "Import into the available list");
		this.btnImport_to_available_list.UseVisualStyleBackColor = true;
		this.btnImport_to_available_list.Click += new System.EventHandler(btnImport_to_available_list_Click);
		this.btnOpenFolder.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.btnOpenFolder.Image = Thetis.Properties.Resources.Folder_Open_32;
		this.btnOpenFolder.Location = new System.Drawing.Point(590, 287);
		this.btnOpenFolder.Name = "btnOpenFolder";
		this.btnOpenFolder.Selectable = true;
		this.btnOpenFolder.Size = new System.Drawing.Size(42, 42);
		this.btnOpenFolder.TabIndex = 26;
		this.toolTip1.SetToolTip(this.btnOpenFolder, "Open the folder for the selected database");
		this.btnOpenFolder.UseVisualStyleBackColor = true;
		this.btnOpenFolder.Click += new System.EventHandler(btnOpenFolder_Click);
		this.btnExportBackup.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.btnExportBackup.Image = Thetis.Properties.Resources.Data_Export_32;
		this.btnExportBackup.Location = new System.Drawing.Point(542, 430);
		this.btnExportBackup.Name = "btnExportBackup";
		this.btnExportBackup.Selectable = true;
		this.btnExportBackup.Size = new System.Drawing.Size(42, 42);
		this.btnExportBackup.TabIndex = 25;
		this.toolTip1.SetToolTip(this.btnExportBackup, "Export the selected backup database");
		this.btnExportBackup.UseVisualStyleBackColor = true;
		this.btnExportBackup.Click += new System.EventHandler(btnExportBackup_Click);
		this.btnRename.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.btnRename.Image = Thetis.Properties.Resources.Data_Edit_32;
		this.btnRename.Location = new System.Drawing.Point(542, 287);
		this.btnRename.Name = "btnRename";
		this.btnRename.Selectable = true;
		this.btnRename.Size = new System.Drawing.Size(42, 42);
		this.btnRename.TabIndex = 24;
		this.toolTip1.SetToolTip(this.btnRename, "Change description for the selected database");
		this.btnRename.UseVisualStyleBackColor = true;
		this.btnRename.Click += new System.EventHandler(btnRename_Click);
		this.btnExport.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.btnExport.Image = Thetis.Properties.Resources.Data_Export_32;
		this.btnExport.Location = new System.Drawing.Point(638, 239);
		this.btnExport.Name = "btnExport";
		this.btnExport.Selectable = true;
		this.btnExport.Size = new System.Drawing.Size(42, 42);
		this.btnExport.TabIndex = 23;
		this.toolTip1.SetToolTip(this.btnExport, "Export the selected database");
		this.btnExport.UseVisualStyleBackColor = true;
		this.btnExport.Click += new System.EventHandler(btnExport_Click);
		this.btnImport.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.btnImport.Image = Thetis.Properties.Resources.Data_Import_32;
		this.btnImport.Location = new System.Drawing.Point(590, 104);
		this.btnImport.Name = "btnImport";
		this.btnImport.Selectable = true;
		this.btnImport.Size = new System.Drawing.Size(42, 42);
		this.btnImport.TabIndex = 22;
		this.toolTip1.SetToolTip(this.btnImport, "Import into the active database");
		this.btnImport.UseVisualStyleBackColor = true;
		this.btnImport.Click += new System.EventHandler(btnImport_Click);
		this.btnDuplicateDB.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.btnDuplicateDB.Image = Thetis.Properties.Resources.Data_Copy_32;
		this.btnDuplicateDB.Location = new System.Drawing.Point(638, 191);
		this.btnDuplicateDB.Name = "btnDuplicateDB";
		this.btnDuplicateDB.Selectable = true;
		this.btnDuplicateDB.Size = new System.Drawing.Size(42, 42);
		this.btnDuplicateDB.TabIndex = 21;
		this.toolTip1.SetToolTip(this.btnDuplicateDB, "Duplicate the database");
		this.btnDuplicateDB.UseVisualStyleBackColor = true;
		this.btnDuplicateDB.Click += new System.EventHandler(btnDuplicateBD_Click);
		this.btnMakeActive.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.btnMakeActive.Image = Thetis.Properties.Resources.Data_Active_32;
		this.btnMakeActive.Location = new System.Drawing.Point(542, 191);
		this.btnMakeActive.Name = "btnMakeActive";
		this.btnMakeActive.Selectable = true;
		this.btnMakeActive.Size = new System.Drawing.Size(42, 42);
		this.btnMakeActive.TabIndex = 14;
		this.toolTip1.SetToolTip(this.btnMakeActive, "Make the database active");
		this.btnMakeActive.UseVisualStyleBackColor = true;
		this.btnMakeActive.Click += new System.EventHandler(btnMakeActive_Click);
		this.btnMakeBackupAvailable.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.btnMakeBackupAvailable.Image = Thetis.Properties.Resources.Data_Refresh_32;
		this.btnMakeBackupAvailable.Location = new System.Drawing.Point(542, 382);
		this.btnMakeBackupAvailable.Name = "btnMakeBackupAvailable";
		this.btnMakeBackupAvailable.Selectable = true;
		this.btnMakeBackupAvailable.Size = new System.Drawing.Size(42, 42);
		this.btnMakeBackupAvailable.TabIndex = 6;
		this.toolTip1.SetToolTip(this.btnMakeBackupAvailable, "Make the selected backup available");
		this.btnMakeBackupAvailable.UseVisualStyleBackColor = true;
		this.btnMakeBackupAvailable.Click += new System.EventHandler(btnMakeBackupAvailable_Click);
		this.btnRemoveBackup.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.btnRemoveBackup.Image = Thetis.Properties.Resources.Data_Delete_32;
		this.btnRemoveBackup.Location = new System.Drawing.Point(542, 526);
		this.btnRemoveBackup.Name = "btnRemoveBackup";
		this.btnRemoveBackup.Selectable = true;
		this.btnRemoveBackup.Size = new System.Drawing.Size(42, 42);
		this.btnRemoveBackup.TabIndex = 5;
		this.toolTip1.SetToolTip(this.btnRemoveBackup, "Remove the selected backup");
		this.btnRemoveBackup.UseVisualStyleBackColor = true;
		this.btnRemoveBackup.Click += new System.EventHandler(btnRemoveBackup_Click);
		this.btnBackupOnShutdown.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.btnBackupOnShutdown.Image = Thetis.Properties.Resources.Data_Down_32;
		this.btnBackupOnShutdown.Location = new System.Drawing.Point(590, 239);
		this.btnBackupOnShutdown.Name = "btnBackupOnShutdown";
		this.btnBackupOnShutdown.Selectable = true;
		this.btnBackupOnShutdown.Size = new System.Drawing.Size(42, 42);
		this.btnBackupOnShutdown.TabIndex = 4;
		this.toolTip1.SetToolTip(this.btnBackupOnShutdown, "Toggle Backup On Shut-Down");
		this.btnBackupOnShutdown.UseVisualStyleBackColor = true;
		this.btnBackupOnShutdown.Click += new System.EventHandler(btnBackupOnShutdown_Click);
		this.btnBackupOnStart.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.btnBackupOnStart.Image = Thetis.Properties.Resources.Data_Up_32;
		this.btnBackupOnStart.Location = new System.Drawing.Point(542, 239);
		this.btnBackupOnStart.Name = "btnBackupOnStart";
		this.btnBackupOnStart.Selectable = true;
		this.btnBackupOnStart.Size = new System.Drawing.Size(42, 42);
		this.btnBackupOnStart.TabIndex = 3;
		this.toolTip1.SetToolTip(this.btnBackupOnStart, "Toggle Backup On Start-Up");
		this.btnBackupOnStart.UseVisualStyleBackColor = true;
		this.btnBackupOnStart.Click += new System.EventHandler(btnBackupOnStart_Click);
		this.btnTakeBackupNow.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.btnTakeBackupNow.Image = Thetis.Properties.Resources.Archive_32;
		this.btnTakeBackupNow.Location = new System.Drawing.Point(542, 104);
		this.btnTakeBackupNow.Name = "btnTakeBackupNow";
		this.btnTakeBackupNow.Selectable = true;
		this.btnTakeBackupNow.Size = new System.Drawing.Size(42, 42);
		this.btnTakeBackupNow.TabIndex = 2;
		this.toolTip1.SetToolTip(this.btnTakeBackupNow, "Take backup now of the active database");
		this.btnTakeBackupNow.UseVisualStyleBackColor = true;
		this.btnTakeBackupNow.Click += new System.EventHandler(btnTakeBackupNow_Click);
		this.btnRemoveDB.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.btnRemoveDB.Image = Thetis.Properties.Resources.Data_Delete_32;
		this.btnRemoveDB.Location = new System.Drawing.Point(590, 191);
		this.btnRemoveDB.Name = "btnRemoveDB";
		this.btnRemoveDB.Selectable = true;
		this.btnRemoveDB.Size = new System.Drawing.Size(42, 42);
		this.btnRemoveDB.TabIndex = 1;
		this.toolTip1.SetToolTip(this.btnRemoveDB, "Remove the database");
		this.btnRemoveDB.UseVisualStyleBackColor = true;
		this.btnRemoveDB.Click += new System.EventHandler(btnRemoveDB_Click);
		this.btnNewDB.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.btnNewDB.Image = Thetis.Properties.Resources.Data_Add_32;
		this.btnNewDB.Location = new System.Drawing.Point(542, 22);
		this.btnNewDB.Name = "btnNewDB";
		this.btnNewDB.Selectable = true;
		this.btnNewDB.Size = new System.Drawing.Size(42, 42);
		this.btnNewDB.TabIndex = 0;
		this.toolTip1.SetToolTip(this.btnNewDB, "New database");
		this.btnNewDB.UseVisualStyleBackColor = true;
		this.btnNewDB.Click += new System.EventHandler(btnNewDB_Click);
		this.chkPruneBackups.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.chkPruneBackups.AutoSize = true;
		this.chkPruneBackups.Image = null;
		this.chkPruneBackups.Location = new System.Drawing.Point(542, 574);
		this.chkPruneBackups.Name = "chkPruneBackups";
		this.chkPruneBackups.Size = new System.Drawing.Size(122, 17);
		this.chkPruneBackups.TabIndex = 31;
		this.chkPruneBackups.Text = "Prune auto backups";
		this.toolTip1.SetToolTip(this.chkPruneBackups, resources.GetString("chkPruneBackups.ToolTip"));
		this.chkPruneBackups.UseVisualStyleBackColor = true;
		this.chkPruneBackups.CheckedChanged += new System.EventHandler(chkPruneBackups_CheckedChanged);
		this.labelTS4.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.labelTS4.AutoSize = true;
		this.labelTS4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.labelTS4.Image = null;
		this.labelTS4.Location = new System.Drawing.Point(542, 175);
		this.labelTS4.Name = "labelTS4";
		this.labelTS4.Size = new System.Drawing.Size(136, 13);
		this.labelTS4.TabIndex = 29;
		this.labelTS4.Text = "apply to highlighted db";
		this.labelTS3.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.labelTS3.AutoSize = true;
		this.labelTS3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.labelTS3.Image = null;
		this.labelTS3.Location = new System.Drawing.Point(542, 88);
		this.labelTS3.Name = "labelTS3";
		this.labelTS3.Size = new System.Drawing.Size(109, 13);
		this.labelTS3.TabIndex = 28;
		this.labelTS3.Text = "apply to active db";
		this.labelTS2.AutoSize = true;
		this.labelTS2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.labelTS2.Image = null;
		this.labelTS2.Location = new System.Drawing.Point(12, 6);
		this.labelTS2.Name = "labelTS2";
		this.labelTS2.Size = new System.Drawing.Size(123, 13);
		this.labelTS2.TabIndex = 12;
		this.labelTS2.Text = "Available Databases";
		this.lblDabaseBackups_active_selected.AutoSize = true;
		this.lblDabaseBackups_active_selected.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.lblDabaseBackups_active_selected.Image = null;
		this.lblDabaseBackups_active_selected.Location = new System.Drawing.Point(12, 366);
		this.lblDabaseBackups_active_selected.Name = "lblDabaseBackups_active_selected";
		this.lblDabaseBackups_active_selected.Size = new System.Drawing.Size(114, 13);
		this.lblDabaseBackups_active_selected.TabIndex = 9;
		this.lblDabaseBackups_active_selected.Text = "Database Backups";
		this.picTick.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.picTick.BackColor = System.Drawing.Color.Transparent;
		this.picTick.Image = (System.Drawing.Image)resources.GetObject("picTick.Image");
		this.picTick.Location = new System.Drawing.Point(650, 86);
		this.picTick.Name = "picTick";
		this.picTick.Size = new System.Drawing.Size(18, 18);
		this.picTick.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
		this.picTick.TabIndex = 52;
		this.picTick.TabStop = false;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackColor = System.Drawing.SystemColors.ControlDark;
		base.ClientSize = new System.Drawing.Size(704, 601);
		base.Controls.Add(this.picTick);
		base.Controls.Add(this.chkPruneBackups);
		base.Controls.Add(this.btnRenameBackup);
		base.Controls.Add(this.labelTS4);
		base.Controls.Add(this.labelTS3);
		base.Controls.Add(this.btnImport_to_available_list);
		base.Controls.Add(this.btnOpenFolder);
		base.Controls.Add(this.btnExportBackup);
		base.Controls.Add(this.btnRename);
		base.Controls.Add(this.btnExport);
		base.Controls.Add(this.btnImport);
		base.Controls.Add(this.btnDuplicateDB);
		base.Controls.Add(this.btnMakeActive);
		base.Controls.Add(this.labelTS2);
		base.Controls.Add(this.lblDabaseBackups_active_selected);
		base.Controls.Add(this.lstBackups);
		base.Controls.Add(this.lstActiveDBs);
		base.Controls.Add(this.btnMakeBackupAvailable);
		base.Controls.Add(this.btnRemoveBackup);
		base.Controls.Add(this.btnBackupOnShutdown);
		base.Controls.Add(this.btnBackupOnStart);
		base.Controls.Add(this.btnTakeBackupNow);
		base.Controls.Add(this.btnRemoveDB);
		base.Controls.Add(this.btnNewDB);
		base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		this.MinimumSize = new System.Drawing.Size(720, 640);
		base.Name = "frmDBMan";
		this.Text = "Database Manager";
		base.TopMost = true;
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(frmDBMan_FormClosing);
		base.Shown += new System.EventHandler(frmDBMan_Shown);
		((System.ComponentModel.ISupportInitialize)this.picTick).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
