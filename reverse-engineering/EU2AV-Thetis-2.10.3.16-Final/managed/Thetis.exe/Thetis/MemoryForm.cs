using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Thetis;

public class MemoryForm : Form
{
	public class AutoClosingMessageBox
	{
		private System.Threading.Timer _timeoutTimer;

		private string _caption;

		private const int WM_CLOSE = 16;

		private AutoClosingMessageBox(string text, string caption, int timeout)
		{
			_caption = caption;
			_timeoutTimer = new System.Threading.Timer(OnTimerElapsed, null, timeout, -1);
			using (_timeoutTimer)
			{
				MessageBox.Show(text, caption);
			}
		}

		public static void Show(string text, string caption, int timeout)
		{
			new AutoClosingMessageBox(text, caption, timeout);
		}

		private void OnTimerElapsed(object state)
		{
			IntPtr intPtr = FindWindow("#32770", _caption);
			if (intPtr != IntPtr.Zero)
			{
				SendMessage(intPtr, 16u, IntPtr.Zero, IntPtr.Zero);
			}
			_timeoutTimer.Dispose();
		}

		[DllImport("user32.dll", SetLastError = true)]
		private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
	}

	private Console console;

	public static string URLTEXT;

	public string droppedUrl;

	public static string[] filename;

	public static int RIndex = 0;

	public static int CIndex = 0;

	private const string _asciiUrlDataFormatName = "UniformResourceLocator";

	private static readonly Encoding _asciiUrlEncoding = Encoding.ASCII;

	private const string _unicodeUrlDataFormatName = "UniformResourceLocatorW";

	private static readonly Encoding _unicodeUrlEncoding = Encoding.Unicode;

	public static DateTime UTCD = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local);

	public static string FD = UTCD.ToString("HHmm");

	public static int UTCNEW = Convert.ToInt16(FD);

	public static int LASTUTC = 0;

	public static int DurationCount = 0;

	private int daycheck;

	private int ScheduleOnce;

	private int poweroff;

	private string wave_folder = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) + "\\Thetis";

	private IContainer components;

	private DataGridView dataGridView1;

	private ButtonTS btnMemoryRecordCopy;

	private ButtonTS btnMemoryRecordDelete;

	private ToolTip toolTip1;

	private ButtonTS btnSelect;

	private CheckBoxTS chkMemoryFormClose;

	public ButtonTS MemoryRecordAdd;

	private TextBox textBox1;

	private TextBox MemComments;

	private TextBox textBox3;

	private TextBox textBox4;

	private TextBox textBox6;

	private TextBox textBox8;

	private CheckBoxTS ScheduleRepeat;

	private CheckBoxTS ScheduleRecord;

	private NumericUpDownTS ScheduleDurationTime;

	private CheckBoxTS chkAlwaysOnTop;

	private DateTimePicker ScheduleStartDate;

	private DateTimePicker ScheduleStartTime;

	private CheckBoxTS ScheduleOn;

	private TextBox ScheduleRemain;

	private TextBox textBox2;

	private TextBox textBox5;

	private TextBox textBox7;

	private TextBox MemFreq;

	private TextBox MemGroup;

	private TextBox MemName;

	private CheckBoxTS ScheduleRepeatm;

	private NumericUpDownTS ScheduleExtra;

	private ButtonTS buttonTS1;

	private OpenFileDialog openFileDialog1;

	private System.Windows.Forms.Timer timer1;

	public MemoryForm(Console c)
	{
		InitializeComponent();
		console = c;
		Common.RestoreForm(this, "MemoryForm", restore_size: true);
		dataGridView1.RowHeadersVisible = true;
		dataGridView1.DataSource = console.MemoryList.List;
		dataGridView1.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
		dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
		dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
		dataGridView1.AllowUserToAddRows = false;
		dataGridView1.AllowUserToDeleteRows = false;
		dataGridView1.AutoGenerateColumns = false;
		DataGridViewComboBoxColumn dataGridViewComboBoxColumn = new DataGridViewComboBoxColumn
		{
			DataPropertyName = "DSPMode",
			Name = "DSPMode",
			HeaderText = "DSP Mode",
			ValueType = typeof(DSPMode)
		};
		DataGridViewComboBoxColumn dataGridViewComboBoxColumn2 = new DataGridViewComboBoxColumn
		{
			DataPropertyName = "TuneStep",
			Name = "TuneStep",
			HeaderText = "Tune Step",
			ValueType = typeof(string)
		};
		DataGridViewComboBoxColumn dataGridViewComboBoxColumn3 = new DataGridViewComboBoxColumn
		{
			DataPropertyName = "RPTR",
			Name = "RPTR",
			HeaderText = "RPTR",
			ValueType = typeof(FMTXMode)
		};
		DataGridViewComboBoxColumn dataGridViewComboBoxColumn4 = new DataGridViewComboBoxColumn
		{
			DataPropertyName = "CTCSSFreq",
			Name = "CTCSSFreq",
			HeaderText = "CTCSS Freq",
			ValueType = typeof(double)
		};
		DataGridViewComboBoxColumn dataGridViewComboBoxColumn5 = new DataGridViewComboBoxColumn
		{
			DataPropertyName = "Deviation",
			Name = "Deviation",
			HeaderText = "Deviation",
			ValueType = typeof(double)
		};
		DataGridViewComboBoxColumn dataGridViewComboBoxColumn6 = new DataGridViewComboBoxColumn
		{
			DataPropertyName = "RXFilter",
			Name = "RXFilter",
			HeaderText = "RXFilter",
			ValueType = typeof(Filter)
		};
		DataGridViewComboBoxColumn dataGridViewComboBoxColumn7 = new DataGridViewComboBoxColumn
		{
			DataPropertyName = "AGCMode",
			Name = "AGCMode",
			HeaderText = "AGC Mode",
			ValueType = typeof(AGCMode)
		};
		dataGridViewComboBoxColumn.Items.Add(DSPMode.LSB);
		dataGridViewComboBoxColumn.Items.Add(DSPMode.USB);
		dataGridViewComboBoxColumn.Items.Add(DSPMode.DSB);
		dataGridViewComboBoxColumn.Items.Add(DSPMode.CWL);
		dataGridViewComboBoxColumn.Items.Add(DSPMode.CWU);
		dataGridViewComboBoxColumn.Items.Add(DSPMode.FM);
		dataGridViewComboBoxColumn.Items.Add(DSPMode.AM);
		dataGridViewComboBoxColumn.Items.Add(DSPMode.SAM);
		dataGridViewComboBoxColumn.Items.Add(DSPMode.SPEC);
		dataGridViewComboBoxColumn.Items.Add(DSPMode.DIGL);
		dataGridViewComboBoxColumn.Items.Add(DSPMode.DIGU);
		dataGridViewComboBoxColumn.Items.Add(DSPMode.DRM);
		for (int i = 0; i < console.TuneStepList.Count; i++)
		{
			dataGridViewComboBoxColumn2.Items.Add(console.TuneStepList[i].Name);
		}
		dataGridViewComboBoxColumn3.Items.Add(FMTXMode.High);
		dataGridViewComboBoxColumn3.Items.Add(FMTXMode.Simplex);
		dataGridViewComboBoxColumn3.Items.Add(FMTXMode.Low);
		for (int j = 0; j < console.CTCSS_array.Length; j++)
		{
			dataGridViewComboBoxColumn4.Items.Add(console.CTCSS_array[j]);
		}
		for (int k = 0; k < console.FM_deviation_array.Length; k++)
		{
			dataGridViewComboBoxColumn5.Items.Add((int)console.FM_deviation_array[k]);
		}
		for (int l = 0; l < 13; l++)
		{
			dataGridViewComboBoxColumn6.Items.Add((Filter)l);
		}
		for (int m = 0; m < 6; m++)
		{
			dataGridViewComboBoxColumn7.Items.Add((AGCMode)m);
		}
		int index = dataGridView1.Columns["DSPMode"].Index;
		dataGridView1.Columns.Remove("DSPMode");
		dataGridView1.Columns.Insert(index, dataGridViewComboBoxColumn);
		index = dataGridView1.Columns["TuneStep"].Index;
		dataGridView1.Columns.Remove("TuneStep");
		dataGridView1.Columns.Insert(index, dataGridViewComboBoxColumn2);
		index = dataGridView1.Columns["RPTR"].Index;
		dataGridView1.Columns.Remove("RPTR");
		dataGridView1.Columns.Insert(index, dataGridViewComboBoxColumn3);
		index = dataGridView1.Columns["CTCSSFreq"].Index;
		dataGridView1.Columns.Remove("CTCSSFreq");
		dataGridView1.Columns.Insert(index, dataGridViewComboBoxColumn4);
		index = dataGridView1.Columns["Deviation"].Index;
		dataGridView1.Columns.Remove("Deviation");
		dataGridView1.Columns.Insert(index, dataGridViewComboBoxColumn5);
		index = dataGridView1.Columns["RXFilter"].Index;
		dataGridView1.Columns.Remove("RXFilter");
		dataGridView1.Columns.Insert(index, dataGridViewComboBoxColumn6);
		index = dataGridView1.Columns["AGCMode"].Index;
		dataGridView1.Columns.Remove("AGCMode");
		dataGridView1.Columns.Insert(index, dataGridViewComboBoxColumn7);
		dataGridView1.Columns["RXFreq"].HeaderText = "RX Freq";
		dataGridView1.Columns["RPTROffset"].HeaderText = "RPTR Offset";
		dataGridView1.Columns["CTCSSOn"].HeaderText = "CTCSS";
		dataGridView1.Columns["TXFreq"].HeaderText = "TX Freq";
		dataGridView1.Columns["RXFilterLow"].HeaderText = "RX Filter Low";
		dataGridView1.Columns["RXFilterHigh"].HeaderText = "RX Filter High";
		dataGridView1.Columns["AGCT"].HeaderText = "AGC-T";
		dataGridView1.Columns["StartDate"].HeaderText = "Schedule Start";
		dataGridView1.Columns["Repeating"].HeaderText = "Weekly";
		dataGridView1.Columns["Repeatingm"].HeaderText = "Monthly";
		dataGridView1.Columns["RXFreq"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
		dataGridView1.Columns["RXFreq"].DefaultCellStyle.Format = "f6";
		dataGridView1.Columns["TXFreq"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
		dataGridView1.Columns["TXFreq"].DefaultCellStyle.Format = "f6";
		dataGridView1.Columns["RPTROffset"].DefaultCellStyle.Format = "f";
		dataGridView1.Columns["Scan"].Visible = false;
		dataGridView1.CellValidating += dataGridView1_CellValidating;
		ScheduleUpdate();
		Thread thread = new Thread(SCHEDULER);
		thread.CurrentCulture = CultureInfo.CreateSpecificCulture("en-US");
		thread.CurrentUICulture = CultureInfo.CreateSpecificCulture("en-US");
		thread.Name = "Scheduler Thread";
		thread.IsBackground = true;
		thread.Priority = ThreadPriority.BelowNormal;
		thread.Start();
	}

	private void dataGridView1_DragEnter(object sender, DragEventArgs e)
	{
		if (e.Data.GetDataPresent(DataFormats.FileDrop))
		{
			e.Effect = DragDropEffects.Copy;
			e.Effect = DragDropEffects.All;
			filename = (string[])e.Data.GetData(DataFormats.FileDrop);
			URLTEXT = filename[0];
			return;
		}
		droppedUrl = ReadURL(e.Data);
		if (droppedUrl != null && droppedUrl.Trim().Length != 0)
		{
			URLTEXT = droppedUrl;
			e.Effect = DragDropEffects.Link;
		}
		else
		{
			e.Effect = DragDropEffects.None;
		}
	}

	private void dataGridView1_DragDrop(object sender, DragEventArgs e)
	{
		dataGridView1["comments", RIndex].Value = URLTEXT;
	}

	private void dataGridView1_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
	{
		if (e.Button == MouseButtons.Right)
		{
			try
			{
				Process.Start((string)dataGridView1["comments", RIndex].Value);
			}
			catch
			{
			}
		}
		else
		{
			_ = e.Button;
			_ = 1048576;
		}
		ScheduleUpdate();
	}

	private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
	{
		RIndex = e.RowIndex;
		ScheduleUpdate();
	}

	private void dataGridView1_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
	{
		if (dataGridView1.Columns[e.ColumnIndex].Name == "RXFreq" || dataGridView1.Columns[e.ColumnIndex].Name == "TXFreq" || dataGridView1.Columns[e.ColumnIndex].Name == "RPTROffset")
		{
			if (!double.TryParse((string)e.FormattedValue, out var _))
			{
				dataGridView1[e.ColumnIndex, e.RowIndex].Value = 0.0;
			}
		}
		else if (dataGridView1.Columns[e.ColumnIndex].Name == "Power" || dataGridView1.Columns[e.ColumnIndex].Name == "FilterLow" || dataGridView1.Columns[e.ColumnIndex].Name == "FilterHigh" || dataGridView1.Columns[e.ColumnIndex].Name == "AGCT")
		{
			if (!int.TryParse((string)e.FormattedValue, out var _))
			{
				dataGridView1[e.ColumnIndex, e.RowIndex].Value = 0;
			}
		}
		else
		{
			ScheduleUpdate();
		}
	}

	private void MemoryRecordAdd_DragEnter(object sender, DragEventArgs e)
	{
		if (e.Data.GetDataPresent(DataFormats.FileDrop))
		{
			e.Effect = DragDropEffects.Copy;
			e.Effect = DragDropEffects.All;
			filename = (string[])e.Data.GetData(DataFormats.FileDrop);
			URLTEXT = filename[0];
			return;
		}
		droppedUrl = ReadURL(e.Data);
		if (droppedUrl != null && droppedUrl.Trim().Length != 0)
		{
			URLTEXT = droppedUrl;
			e.Effect = DragDropEffects.Link;
		}
		else
		{
			e.Effect = DragDropEffects.None;
		}
		ScheduleUpdate();
	}

	private void MemoryRecordAdd_DragDrop(object sender, DragEventArgs e)
	{
		string name = Convert.ToString(console.VFOAFreq);
		console.MemoryList.List.Add(new MemoryRecord("", console.VFOAFreq, name, console.RX1DSPMode, _scan: true, console.TuneStepList[console.TuneStepIndex].Name, console.CurrentFMTXMode, console.FMTXOffsetMHz, console.radio.GetDSPTX(0).CTCSSFlag, console.radio.GetDSPTX(0).CTCSSFreqHz, console.PWR, (int)console.radio.GetDSPTX(0).TXFMDeviation, console.VFOSplit, console.TXFreq, console.RX1Filter, console.RX1FilterLow, console.RX1FilterHigh, URLTEXT, console.radio.GetDSPRX(0, 0).RXAGCMode, console.RF, DateTime.Now, ScheduleOn.Checked, (int)ScheduleDurationTime.Value, ScheduleRepeat.Checked, ScheduleRecord.Checked, ScheduleRepeatm.Checked, (int)ScheduleExtra.Value));
		Common.SaveForm(this, "MemoryForm");
		console.MemoryList.Save();
		ScheduleUpdate();
	}

	public void MemoryRecordAdd_Click(object sender, EventArgs e)
	{
		string name = Convert.ToString(console.VFOAFreq);
		if (Console.ALTM)
		{
			console.MemoryList.List.Add(new MemoryRecord("New Spot", console.VFOAFreq, name, console.RX1DSPMode, _scan: true, console.TuneStepList[console.TuneStepIndex].Name, console.CurrentFMTXMode, console.FMTXOffsetMHz, console.radio.GetDSPTX(0).CTCSSFlag, console.radio.GetDSPTX(0).CTCSSFreqHz, console.PWR, (int)console.radio.GetDSPTX(0).TXFMDeviation, console.VFOSplit, console.TXFreq, console.RX1Filter, console.RX1FilterLow, console.RX1FilterHigh, "", console.radio.GetDSPRX(0, 0).RXAGCMode, console.RF, DateTime.Now, ScheduleOn.Checked, (int)ScheduleDurationTime.Value, ScheduleRepeat.Checked, ScheduleRecord.Checked, ScheduleRepeatm.Checked, (int)ScheduleExtra.Value));
		}
		else
		{
			console.MemoryList.List.Add(new MemoryRecord("", console.VFOAFreq, name, console.RX1DSPMode, _scan: true, console.TuneStepList[console.TuneStepIndex].Name, console.CurrentFMTXMode, console.FMTXOffsetMHz, console.radio.GetDSPTX(0).CTCSSFlag, console.radio.GetDSPTX(0).CTCSSFreqHz, console.PWR, (int)console.radio.GetDSPTX(0).TXFMDeviation, console.VFOSplit, console.TXFreq, console.RX1Filter, console.RX1FilterLow, console.RX1FilterHigh, "", console.radio.GetDSPRX(0, 0).RXAGCMode, console.RF, DateTime.Now, ScheduleOn.Checked, (int)ScheduleDurationTime.Value, ScheduleRepeat.Checked, ScheduleRecord.Checked, ScheduleRepeatm.Checked, (int)ScheduleExtra.Value));
		}
		ScheduleUpdate();
		Console.ALTM = false;
		Common.SaveForm(this, "MemoryForm");
		console.MemoryList.Save();
	}

	private void btnMemoryRecordCopy_Click(object sender, EventArgs e)
	{
		if (console.MemoryList.List.Count != 0)
		{
			console.MemoryList.List.Add(new MemoryRecord(console.MemoryList.List[dataGridView1.CurrentCell.RowIndex]));
			Common.SaveForm(this, "MemoryForm");
			console.MemoryList.Save();
		}
	}

	private void btnMemoryRecordDelete_Click(object sender, EventArgs e)
	{
		if (console.MemoryList.List.Count == 0 || (dataGridView1.SelectedRows.Count == 0 && (dataGridView1.CurrentCell.RowIndex < 0 || dataGridView1.CurrentCell.RowIndex > console.MemoryList.List.Count - 1)) || MessageBox.Show("Are you sure you want to remove the selected row(s)?", "Remove Row(s)?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
		{
			return;
		}
		if (dataGridView1.SelectedRows.Count > 0)
		{
			int num = 0;
			while (num < dataGridView1.SelectedRows.Count)
			{
				console.MemoryList.List.Remove(console.MemoryList.List[dataGridView1.SelectedRows[num].Index]);
			}
		}
		else
		{
			console.MemoryList.List.Remove(console.MemoryList.List[dataGridView1.CurrentCell.RowIndex]);
		}
		Common.SaveForm(this, "MemoryForm");
		console.MemoryList.Save();
	}

	private void btnSelect_Click(object sender, EventArgs e)
	{
		if (console.MemoryList.List.Count == 0)
		{
			return;
		}
		int rowIndex = dataGridView1.CurrentCell.RowIndex;
		if (rowIndex >= 0 && rowIndex <= console.MemoryList.List.Count - 1)
		{
			console.changeComboFMMemory(rowIndex);
			if (chkMemoryFormClose.Checked)
			{
				Common.SaveForm(this, "MemoryForm");
				console.MemoryList.Save();
				Close();
			}
		}
	}

	private void MemoryForm_FormClosing(object sender, FormClosingEventArgs e)
	{
		Hide();
		e.Cancel = true;
		Common.SaveForm(this, "MemoryForm");
		console.MemoryList.Save();
	}

	private string ReadURL(IDataObject data)
	{
		string text = Readurl(data, "UniformResourceLocatorW", _unicodeUrlEncoding);
		if (text != null)
		{
			return text;
		}
		return Readurl(data, "UniformResourceLocator", _asciiUrlEncoding);
	}

	private string Readurl(IDataObject data, string urlDataFormatName, Encoding urlEncoding)
	{
		if (!DoesDragDropDataContainUrl1(data, urlDataFormatName))
		{
			return null;
		}
		string text;
		using (Stream stream = (Stream)data.GetData(urlDataFormatName))
		{
			using TextReader textReader = new StreamReader(stream, urlEncoding);
			text = textReader.ReadToEnd();
		}
		return text.TrimEnd(default(char));
	}

	private static bool DoesDragDropDataContainUrl1(IDataObject data, string urlDataFormatName)
	{
		return data?.GetDataPresent(urlDataFormatName) ?? false;
	}

	private void textBox1_TextChanged(object sender, EventArgs e)
	{
	}

	private void MemoryForm_Load(object sender, EventArgs e)
	{
	}

	private void ScheduleDurationTime_ValueChanged(object sender, EventArgs e)
	{
		try
		{
			LASTUTC = 0;
			dataGridView1["Duration", RIndex].Value = ScheduleDurationTime.Value;
			console.MemoryList.Save();
		}
		catch (Exception)
		{
		}
	}

	private void ScheduleRepeat_CheckedChanged(object sender, EventArgs e)
	{
		if (ScheduleRepeat.Checked)
		{
			ScheduleRepeatm.Checked = false;
		}
		poweroff = 0;
		try
		{
			dataGridView1["Repeating", RIndex].Value = ScheduleRepeat.Checked;
			if (!ScheduleRepeat.Checked && !ScheduleRepeatm.Checked && DurationCount > 1)
			{
				LASTUTC = UTCNEW;
				DurationCount = 1;
			}
			LASTUTC = 0;
			console.MemoryList.Save();
		}
		catch (Exception)
		{
		}
	}

	private void ScheduleRepeatm_CheckedChanged(object sender, EventArgs e)
	{
		if (ScheduleRepeatm.Checked)
		{
			ScheduleRepeat.Checked = false;
		}
		poweroff = 0;
		try
		{
			dataGridView1["Repeatingm", RIndex].Value = ScheduleRepeatm.Checked;
			if (!ScheduleRepeat.Checked && !ScheduleRepeatm.Checked && DurationCount > 1)
			{
				LASTUTC = UTCNEW;
				DurationCount = 1;
			}
			LASTUTC = 0;
			console.MemoryList.Save();
		}
		catch (Exception)
		{
		}
	}

	private void ScheduleRecord_CheckedChanged(object sender, EventArgs e)
	{
		try
		{
			LASTUTC = 0;
			dataGridView1["Recording", RIndex].Value = ScheduleRecord.Checked;
			console.MemoryList.Save();
		}
		catch (Exception)
		{
		}
	}

	private void ScheduleOn_CheckedChanged(object sender, EventArgs e)
	{
		try
		{
			if (!ScheduleOn.Checked && DurationCount > 1)
			{
				LASTUTC = UTCNEW;
				DurationCount = 1;
			}
			LASTUTC = 0;
			dataGridView1["ScheduleOn", RIndex].Value = ScheduleOn.Checked;
			console.MemoryList.Save();
		}
		catch (Exception)
		{
		}
	}

	private void ScheduleStartDate_ValueChanged(object sender, EventArgs e)
	{
		ScheduleStartDate.Value = ScheduleStartDate.Value.Date + ScheduleStartTime.Value.TimeOfDay;
		try
		{
			LASTUTC = 0;
			dataGridView1["StartDate", RIndex].Value = ScheduleStartDate.Value;
			console.MemoryList.Save();
		}
		catch (Exception)
		{
		}
	}

	public void ScheduleUpdate()
	{
		if (dataGridView1.Rows.Count < 1 || RIndex < 0 || RIndex >= dataGridView1.Rows.Count)
		{
			return;
		}
		MemComments.Text = (string)dataGridView1["comments", RIndex].Value;
		MemGroup.Text = (string)dataGridView1["Group", RIndex].Value;
		MemName.Text = (string)dataGridView1["Name", RIndex].Value;
		double num = (double)dataGridView1["RXFreq", RIndex].Value;
		MemFreq.Text = num.ToString("F6");
		try
		{
			ScheduleStartDate.ValueChanged -= ScheduleStartDate_ValueChanged;
			ScheduleStartTime.ValueChanged -= ScheduleStartDate_ValueChanged;
			ScheduleStartDate.Value = (DateTime)dataGridView1["StartDate", RIndex].Value;
			ScheduleStartTime.Value = (DateTime)dataGridView1["StartDate", RIndex].Value;
			ScheduleStartDate.ValueChanged += ScheduleStartDate_ValueChanged;
			ScheduleStartTime.ValueChanged += ScheduleStartDate_ValueChanged;
			ScheduleOn.Checked = (bool)dataGridView1["ScheduleOn", RIndex].Value;
			if ((int)dataGridView1["Duration", RIndex].Value > 120)
			{
				dataGridView1["Duration", RIndex].Value = 120;
			}
			else if ((int)dataGridView1["Duration", RIndex].Value < 0)
			{
				dataGridView1["Duration", RIndex].Value = 0;
			}
			ScheduleDurationTime.Value = (int)dataGridView1["Duration", RIndex].Value;
			ScheduleRepeat.Checked = (bool)dataGridView1["Repeating", RIndex].Value;
			ScheduleRecord.Checked = (bool)dataGridView1["Recording", RIndex].Value;
			ScheduleRepeatm.Checked = (bool)dataGridView1["Repeatingm", RIndex].Value;
		}
		catch (Exception)
		{
			ScheduleStartDate.Value = DateTime.Now;
			ScheduleStartTime.Value = DateTime.Now;
			ScheduleOn.Checked = false;
			ScheduleDurationTime.Value = 0m;
			ScheduleRepeat.Checked = false;
			ScheduleRecord.Checked = false;
			ScheduleRepeatm.Checked = false;
		}
	}

	private void chkAlwaysOnTop_CheckedChanged(object sender, EventArgs e)
	{
		base.TopMost = chkAlwaysOnTop.Checked;
	}

	private void SCHEDULER()
	{
		while (true)
		{
			UTCD = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
			FD = UTCD.ToString("HHmm");
			UTCNEW = Convert.ToInt16(FD);
			try
			{
				if (UTCNEW != LASTUTC && dataGridView1.Rows.Count > 0)
				{
					LASTUTC = UTCNEW;
					if (DurationCount > 1)
					{
						DurationCount--;
						ScheduleRemain.Text = DurationCount.ToString();
						continue;
					}
					if (DurationCount == 1)
					{
						console.RECPOST = false;
						ScheduleRecord.ForeColor = Color.Black;
						ScheduleRemain.ForeColor = Color.Black;
						DurationCount = 0;
						ScheduleRemain.Text = DurationCount.ToString();
						console.REC1 = false;
						console.SCHED1 = false;
						console.RECPOST1 = true;
						Thread thread = new Thread(TOMP3);
						thread.CurrentCulture = CultureInfo.CreateSpecificCulture("en-US");
						thread.CurrentUICulture = CultureInfo.CreateSpecificCulture("en-US");
						thread.Name = "mp3 Thread";
						thread.IsBackground = true;
						thread.Priority = ThreadPriority.Normal;
						thread.Start();
						if (poweroff == 1)
						{
							console.PowerOn = false;
							poweroff = 0;
						}
					}
					if (ScheduleOnce == UTCNEW)
					{
						continue;
					}
					for (int i = 0; i < dataGridView1.Rows.Count; i++)
					{
						daycheck = 0;
						if (!(bool)dataGridView1["Repeating", i].Value && !(bool)dataGridView1["Repeatingm", i].Value)
						{
							continue;
						}
						DateTime dateTime = (DateTime)dataGridView1["StartDate", i].Value;
						if ((bool)dataGridView1["Repeating", i].Value && DateTime.Now.DayOfWeek == dateTime.DayOfWeek)
						{
							daycheck = 1;
						}
						if ((bool)dataGridView1["Repeatingm", i].Value && DateTime.Now.DayOfWeek == dateTime.DayOfWeek)
						{
							DateTime dateTime2 = dateTime;
							int num = 0;
							int num2 = 0;
							for (int j = 1; j < 32; j++)
							{
								try
								{
									dateTime2 = new DateTime(dateTime.Year, dateTime.Month, j);
									if (dateTime2.DayOfWeek == dateTime.DayOfWeek)
									{
										num++;
										if (dateTime2.Day == dateTime.Day)
										{
											num2 = num;
										}
									}
								}
								catch (Exception)
								{
									break;
								}
							}
							if (num2 == num)
							{
								num2 = 10;
							}
							int num3 = 0;
							int[] array = new int[6];
							for (int k = 1; k < 32; k++)
							{
								try
								{
									if (new DateTime(DateTime.Now.Year, DateTime.Now.Month, k).DayOfWeek == dateTime.DayOfWeek)
									{
										num3++;
										array[num3] = k;
									}
								}
								catch (Exception)
								{
									break;
								}
							}
							switch (num2)
							{
							case 10:
								if (num3 == 3)
								{
									if (array[3] == DateTime.Now.Day)
									{
										daycheck = 1;
									}
								}
								else if (array[4] == DateTime.Now.Day)
								{
									daycheck = 1;
								}
								break;
							case 1:
								if (array[1] == DateTime.Now.Day)
								{
									daycheck = 1;
								}
								break;
							case 2:
								if (array[2] == DateTime.Now.Day)
								{
									daycheck = 1;
								}
								break;
							case 3:
								if (array[3] == DateTime.Now.Day)
								{
									daycheck = 1;
								}
								break;
							case 4:
								if (array[4] == DateTime.Now.Day)
								{
									daycheck = 1;
								}
								break;
							}
						}
						if ((!(dateTime.Date == DateTime.Now.Date) && daycheck != 1) || dateTime.TimeOfDay.Hours != DateTime.Now.TimeOfDay.Hours || dateTime.TimeOfDay.Minutes != DateTime.Now.TimeOfDay.Minutes)
						{
							continue;
						}
						if (!console.PowerOn)
						{
							console.PowerOn = true;
							poweroff = 1;
						}
						else
						{
							poweroff = 0;
						}
						if (!console.MOX)
						{
							ScheduleOnce = UTCNEW;
							int num4 = i;
							if (num4 >= 0 && num4 <= console.MemoryList.List.Count - 1)
							{
								console.changeComboFMMemory(num4);
								if (chkMemoryFormClose.Checked)
								{
									Common.SaveForm(this, "MemoryForm");
									console.MemoryList.Save();
									Close();
								}
								console.SCHED1 = true;
								DurationCount = (int)dataGridView1["Duration", i].Value;
								ScheduleRemain.Text = DurationCount.ToString();
								if ((bool)dataGridView1["Recording", i].Value)
								{
									AutoClosingMessageBox.Show("A Scheduled Recording has been started.\nYou can end the recording early by Checking OFF both Weekly & Monthly boxes. ", "Scheduled Recording Started", 4000);
									console.RECPOST = true;
									ScheduleRecord.ForeColor = Color.Red;
									ScheduleRemain.ForeColor = Color.Red;
									console.REC1 = true;
								}
								else
								{
									AutoClosingMessageBox.Show("Scheduled Frequency Change has occured\n", "Scheduled Frequency change", 4000);
								}
							}
						}
						else
						{
							AutoClosingMessageBox.Show("Scheduled Frequency change and/or Recording could not take place while Transmitting", "Scheduled Memory Event", 4000);
						}
					}
				}
			}
			catch (Exception)
			{
				ScheduleRecord.ForeColor = Color.Black;
				ScheduleRemain.ForeColor = Color.Black;
				console.RECPOST = false;
				console.REC1 = false;
				console.SCHED1 = false;
			}
			Thread.Sleep(50);
		}
	}

	private void buttonTS1_Click(object sender, EventArgs e)
	{
		string text = Path.Combine(console.ARP.AudioFolder, "scheduled");
		try
		{
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
		}
		catch
		{
		}
		try
		{
			if (Directory.Exists(text))
			{
				Process.Start("explorer.exe", text);
			}
		}
		catch
		{
		}
	}

	public static void WaveToMP3(string waveFileName, string mp3FileName, int bitRate = 128)
	{
	}

	public void TOMP3()
	{
	}

	public static void ConvertWavStreamToMp3File(ref MemoryStream ms, string savetofilename)
	{
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Thetis.MemoryForm));
		this.dataGridView1 = new System.Windows.Forms.DataGridView();
		this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
		this.textBox4 = new System.Windows.Forms.TextBox();
		this.textBox6 = new System.Windows.Forms.TextBox();
		this.textBox8 = new System.Windows.Forms.TextBox();
		this.ScheduleStartDate = new System.Windows.Forms.DateTimePicker();
		this.ScheduleStartTime = new System.Windows.Forms.DateTimePicker();
		this.MemComments = new System.Windows.Forms.TextBox();
		this.ScheduleRemain = new System.Windows.Forms.TextBox();
		this.MemFreq = new System.Windows.Forms.TextBox();
		this.MemGroup = new System.Windows.Forms.TextBox();
		this.MemName = new System.Windows.Forms.TextBox();
		this.buttonTS1 = new System.Windows.Forms.ButtonTS();
		this.ScheduleRepeatm = new System.Windows.Forms.CheckBoxTS();
		this.ScheduleOn = new System.Windows.Forms.CheckBoxTS();
		this.ScheduleDurationTime = new System.Windows.Forms.NumericUpDownTS();
		this.ScheduleRecord = new System.Windows.Forms.CheckBoxTS();
		this.ScheduleRepeat = new System.Windows.Forms.CheckBoxTS();
		this.chkMemoryFormClose = new System.Windows.Forms.CheckBoxTS();
		this.btnSelect = new System.Windows.Forms.ButtonTS();
		this.btnMemoryRecordDelete = new System.Windows.Forms.ButtonTS();
		this.btnMemoryRecordCopy = new System.Windows.Forms.ButtonTS();
		this.MemoryRecordAdd = new System.Windows.Forms.ButtonTS();
		this.textBox1 = new System.Windows.Forms.TextBox();
		this.textBox3 = new System.Windows.Forms.TextBox();
		this.textBox2 = new System.Windows.Forms.TextBox();
		this.textBox5 = new System.Windows.Forms.TextBox();
		this.textBox7 = new System.Windows.Forms.TextBox();
		this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
		this.timer1 = new System.Windows.Forms.Timer(this.components);
		this.ScheduleExtra = new System.Windows.Forms.NumericUpDownTS();
		this.chkAlwaysOnTop = new System.Windows.Forms.CheckBoxTS();
		((System.ComponentModel.ISupportInitialize)this.dataGridView1).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.ScheduleDurationTime).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.ScheduleExtra).BeginInit();
		base.SuspendLayout();
		this.dataGridView1.AllowDrop = true;
		this.dataGridView1.AllowUserToAddRows = false;
		this.dataGridView1.AllowUserToDeleteRows = false;
		this.dataGridView1.AllowUserToResizeColumns = false;
		this.dataGridView1.AllowUserToResizeRows = false;
		this.dataGridView1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
		this.dataGridView1.Location = new System.Drawing.Point(12, 0);
		this.dataGridView1.Name = "dataGridView1";
		this.dataGridView1.Size = new System.Drawing.Size(951, 389);
		this.dataGridView1.TabIndex = 1;
		this.toolTip1.SetToolTip(this.dataGridView1, resources.GetString("dataGridView1.ToolTip"));
		this.dataGridView1.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(dataGridView1_CellClick);
		this.dataGridView1.CellMouseDown += new System.Windows.Forms.DataGridViewCellMouseEventHandler(dataGridView1_CellMouseDown);
		this.dataGridView1.DragDrop += new System.Windows.Forms.DragEventHandler(dataGridView1_DragDrop);
		this.dataGridView1.DragEnter += new System.Windows.Forms.DragEventHandler(dataGridView1_DragEnter);
		this.dataGridView1.DoubleClick += new System.EventHandler(btnSelect_Click);
		this.textBox4.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.textBox4.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.textBox4.Location = new System.Drawing.Point(12, 395);
		this.textBox4.Name = "textBox4";
		this.textBox4.Size = new System.Drawing.Size(219, 13);
		this.textBox4.TabIndex = 17;
		this.textBox4.Text = "Schedule Start Date for selected Memory\r\n";
		this.toolTip1.SetToolTip(this.textBox4, "Schedule Start Date to change Frequency and optionally record");
		this.textBox6.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.textBox6.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.textBox6.Location = new System.Drawing.Point(237, 395);
		this.textBox6.Name = "textBox6";
		this.textBox6.Size = new System.Drawing.Size(90, 13);
		this.textBox6.TabIndex = 19;
		this.textBox6.Text = "Start Time (local)";
		this.toolTip1.SetToolTip(this.textBox6, "Schedule Start Time to change Frequency and optionally record");
		this.textBox8.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.textBox8.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.textBox8.Location = new System.Drawing.Point(347, 395);
		this.textBox8.Name = "textBox8";
		this.textBox8.Size = new System.Drawing.Size(147, 13);
		this.textBox8.TabIndex = 21;
		this.textBox8.Text = "Set <- Duration ->Remaining";
		this.toolTip1.SetToolTip(this.textBox8, "Duration of Scheduled recording (if Enabled)");
		this.ScheduleStartDate.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.ScheduleStartDate.Location = new System.Drawing.Point(12, 411);
		this.ScheduleStartDate.Name = "ScheduleStartDate";
		this.ScheduleStartDate.Size = new System.Drawing.Size(219, 20);
		this.ScheduleStartDate.TabIndex = 60;
		this.toolTip1.SetToolTip(this.ScheduleStartDate, "Initial Date of Schedule for this Selected Memory\r\n\r\nCheck boxes below determine if Schedule Event is turned ON\r\nand if its Weekly or Monthly");
		this.ScheduleStartDate.ValueChanged += new System.EventHandler(ScheduleStartDate_ValueChanged);
		this.ScheduleStartTime.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.ScheduleStartTime.Format = System.Windows.Forms.DateTimePickerFormat.Time;
		this.ScheduleStartTime.Location = new System.Drawing.Point(237, 412);
		this.ScheduleStartTime.Name = "ScheduleStartTime";
		this.ScheduleStartTime.ShowUpDown = true;
		this.ScheduleStartTime.Size = new System.Drawing.Size(90, 20);
		this.ScheduleStartTime.TabIndex = 61;
		this.toolTip1.SetToolTip(this.ScheduleStartTime, "Initial Time of Schedule for this Selected Memory.\r\nIgnores the seconds.");
		this.ScheduleStartTime.ValueChanged += new System.EventHandler(ScheduleStartDate_ValueChanged);
		this.MemComments.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.MemComments.Location = new System.Drawing.Point(516, 454);
		this.MemComments.Name = "MemComments";
		this.MemComments.Size = new System.Drawing.Size(435, 20);
		this.MemComments.TabIndex = 14;
		this.toolTip1.SetToolTip(this.MemComments, "Comments of currently selected Memory. Including any Hyperlinks");
		this.ScheduleRemain.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.ScheduleRemain.Location = new System.Drawing.Point(447, 410);
		this.ScheduleRemain.Name = "ScheduleRemain";
		this.ScheduleRemain.Size = new System.Drawing.Size(47, 20);
		this.ScheduleRemain.TabIndex = 63;
		this.toolTip1.SetToolTip(this.ScheduleRemain, "Time Remaining in Recording");
		this.MemFreq.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.MemFreq.Location = new System.Drawing.Point(516, 411);
		this.MemFreq.Name = "MemFreq";
		this.MemFreq.Size = new System.Drawing.Size(133, 20);
		this.MemFreq.TabIndex = 67;
		this.toolTip1.SetToolTip(this.MemFreq, "Frequency of currently selected Memory");
		this.MemGroup.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.MemGroup.Location = new System.Drawing.Point(655, 411);
		this.MemGroup.Name = "MemGroup";
		this.MemGroup.Size = new System.Drawing.Size(163, 20);
		this.MemGroup.TabIndex = 68;
		this.toolTip1.SetToolTip(this.MemGroup, "Group name of currently selected Memory");
		this.MemName.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.MemName.Location = new System.Drawing.Point(824, 411);
		this.MemName.Name = "MemName";
		this.MemName.Size = new System.Drawing.Size(127, 20);
		this.MemName.TabIndex = 69;
		this.toolTip1.SetToolTip(this.MemName, "Name of currently selected Memory");
		this.buttonTS1.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.buttonTS1.Image = null;
		this.buttonTS1.Location = new System.Drawing.Point(347, 465);
		this.buttonTS1.Name = "buttonTS1";
		this.buttonTS1.Selectable = true;
		this.buttonTS1.Size = new System.Drawing.Size(131, 23);
		this.buttonTS1.TabIndex = 72;
		this.buttonTS1.Text = "Open Rec Folder";
		this.toolTip1.SetToolTip(this.buttonTS1, "Make the selected memory active ");
		this.buttonTS1.UseVisualStyleBackColor = true;
		this.buttonTS1.Click += new System.EventHandler(buttonTS1_Click);
		this.ScheduleRepeatm.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.ScheduleRepeatm.Image = null;
		this.ScheduleRepeatm.Location = new System.Drawing.Point(133, 436);
		this.ScheduleRepeatm.Name = "ScheduleRepeatm";
		this.ScheduleRepeatm.Size = new System.Drawing.Size(116, 23);
		this.ScheduleRepeatm.TabIndex = 70;
		this.ScheduleRepeatm.Text = "Schedule Monthly";
		this.toolTip1.SetToolTip(this.ScheduleRepeatm, "Check to Schedule every Month. \r\nWill auto check for Last Week of the month. \r\n\r\nTurn Both off to turn of Memory Schedule.");
		this.ScheduleRepeatm.CheckedChanged += new System.EventHandler(ScheduleRepeatm_CheckedChanged);
		this.ScheduleOn.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.ScheduleOn.Image = null;
		this.ScheduleOn.Location = new System.Drawing.Point(237, 454);
		this.ScheduleOn.Name = "ScheduleOn";
		this.ScheduleOn.Size = new System.Drawing.Size(101, 23);
		this.ScheduleOn.TabIndex = 62;
		this.ScheduleOn.Text = "Schedule On";
		this.toolTip1.SetToolTip(this.ScheduleOn, "Check box to turn of Scheduler for this Selected Memory.");
		this.ScheduleOn.UseCompatibleTextRendering = true;
		this.ScheduleOn.Visible = false;
		this.ScheduleOn.CheckedChanged += new System.EventHandler(ScheduleOn_CheckedChanged);
		this.ScheduleDurationTime.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.ScheduleDurationTime.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.ScheduleDurationTime.Location = new System.Drawing.Point(347, 411);
		this.ScheduleDurationTime.Maximum = new decimal(new int[4] { 120, 0, 0, 0 });
		this.ScheduleDurationTime.Minimum = new decimal(new int[4]);
		this.ScheduleDurationTime.Name = "ScheduleDurationTime";
		this.ScheduleDurationTime.Size = new System.Drawing.Size(56, 20);
		this.ScheduleDurationTime.TabIndex = 24;
		this.ScheduleDurationTime.TinyStep = false;
		this.toolTip1.SetToolTip(this.ScheduleDurationTime, "Duration of Scheduled recording (if Enabled)");
		this.ScheduleDurationTime.Value = new decimal(new int[4] { 30, 0, 0, 0 });
		this.ScheduleDurationTime.ValueChanged += new System.EventHandler(ScheduleDurationTime_ValueChanged);
		this.ScheduleRecord.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.ScheduleRecord.Image = null;
		this.ScheduleRecord.Location = new System.Drawing.Point(363, 437);
		this.ScheduleRecord.Name = "ScheduleRecord";
		this.ScheduleRecord.Size = new System.Drawing.Size(127, 21);
		this.ScheduleRecord.TabIndex = 23;
		this.ScheduleRecord.Text = "Record on Schedule";
		this.toolTip1.SetToolTip(this.ScheduleRecord, "Check to record audio at scheduled time for the set Duration");
		this.ScheduleRecord.CheckedChanged += new System.EventHandler(ScheduleRecord_CheckedChanged);
		this.ScheduleRepeat.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.ScheduleRepeat.Image = null;
		this.ScheduleRepeat.Location = new System.Drawing.Point(12, 437);
		this.ScheduleRepeat.Name = "ScheduleRepeat";
		this.ScheduleRepeat.Size = new System.Drawing.Size(116, 23);
		this.ScheduleRepeat.TabIndex = 22;
		this.ScheduleRepeat.Text = "Schedule Weekly";
		this.toolTip1.SetToolTip(this.ScheduleRepeat, "Check to Schedule every Week.\r\nTurn Both off to turn of Memory Schedule.");
		this.ScheduleRepeat.CheckedChanged += new System.EventHandler(ScheduleRepeat_CheckedChanged);
		this.chkMemoryFormClose.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.chkMemoryFormClose.Image = null;
		this.chkMemoryFormClose.Location = new System.Drawing.Point(432, 518);
		this.chkMemoryFormClose.Name = "chkMemoryFormClose";
		this.chkMemoryFormClose.Size = new System.Drawing.Size(89, 32);
		this.chkMemoryFormClose.TabIndex = 12;
		this.chkMemoryFormClose.Text = "Close after selection";
		this.toolTip1.SetToolTip(this.chkMemoryFormClose, "Check to close the Memory window after an entry has been selected");
		this.btnSelect.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.btnSelect.Image = null;
		this.btnSelect.Location = new System.Drawing.Point(266, 520);
		this.btnSelect.Name = "btnSelect";
		this.btnSelect.Selectable = true;
		this.btnSelect.Size = new System.Drawing.Size(75, 23);
		this.btnSelect.TabIndex = 5;
		this.btnSelect.Text = "Select";
		this.toolTip1.SetToolTip(this.btnSelect, "Make the selected memory active ");
		this.btnSelect.UseVisualStyleBackColor = true;
		this.btnSelect.Click += new System.EventHandler(btnSelect_Click);
		this.btnMemoryRecordDelete.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.btnMemoryRecordDelete.Image = null;
		this.btnMemoryRecordDelete.Location = new System.Drawing.Point(174, 520);
		this.btnMemoryRecordDelete.Name = "btnMemoryRecordDelete";
		this.btnMemoryRecordDelete.Selectable = true;
		this.btnMemoryRecordDelete.Size = new System.Drawing.Size(75, 23);
		this.btnMemoryRecordDelete.TabIndex = 4;
		this.btnMemoryRecordDelete.Text = "Delete";
		this.toolTip1.SetToolTip(this.btnMemoryRecordDelete, "Delete the current row");
		this.btnMemoryRecordDelete.UseVisualStyleBackColor = true;
		this.btnMemoryRecordDelete.Click += new System.EventHandler(btnMemoryRecordDelete_Click);
		this.btnMemoryRecordCopy.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.btnMemoryRecordCopy.Image = null;
		this.btnMemoryRecordCopy.Location = new System.Drawing.Point(93, 520);
		this.btnMemoryRecordCopy.Name = "btnMemoryRecordCopy";
		this.btnMemoryRecordCopy.Selectable = true;
		this.btnMemoryRecordCopy.Size = new System.Drawing.Size(75, 23);
		this.btnMemoryRecordCopy.TabIndex = 3;
		this.btnMemoryRecordCopy.Text = "Copy";
		this.toolTip1.SetToolTip(this.btnMemoryRecordCopy, "Create a new row with the same values as the currently selected row");
		this.btnMemoryRecordCopy.UseVisualStyleBackColor = true;
		this.btnMemoryRecordCopy.Click += new System.EventHandler(btnMemoryRecordCopy_Click);
		this.MemoryRecordAdd.AllowDrop = true;
		this.MemoryRecordAdd.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.MemoryRecordAdd.Image = null;
		this.MemoryRecordAdd.Location = new System.Drawing.Point(12, 520);
		this.MemoryRecordAdd.Name = "MemoryRecordAdd";
		this.MemoryRecordAdd.Selectable = true;
		this.MemoryRecordAdd.Size = new System.Drawing.Size(75, 23);
		this.MemoryRecordAdd.TabIndex = 2;
		this.MemoryRecordAdd.Text = "Add";
		this.toolTip1.SetToolTip(this.MemoryRecordAdd, resources.GetString("MemoryRecordAdd.ToolTip"));
		this.MemoryRecordAdd.UseVisualStyleBackColor = true;
		this.MemoryRecordAdd.Click += new System.EventHandler(MemoryRecordAdd_Click);
		this.MemoryRecordAdd.DragDrop += new System.Windows.Forms.DragEventHandler(MemoryRecordAdd_DragDrop);
		this.MemoryRecordAdd.DragEnter += new System.Windows.Forms.DragEventHandler(MemoryRecordAdd_DragEnter);
		this.textBox1.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.textBox1.Location = new System.Drawing.Point(516, 480);
		this.textBox1.Multiline = true;
		this.textBox1.Name = "textBox1";
		this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
		this.textBox1.Size = new System.Drawing.Size(435, 68);
		this.textBox1.TabIndex = 13;
		this.textBox1.Text = resources.GetString("textBox1.Text");
		this.textBox3.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.textBox3.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.textBox3.Location = new System.Drawing.Point(516, 440);
		this.textBox3.Name = "textBox3";
		this.textBox3.Size = new System.Drawing.Size(435, 13);
		this.textBox3.TabIndex = 15;
		this.textBox3.Text = "Comments:";
		this.textBox2.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.textBox2.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.textBox2.Location = new System.Drawing.Point(516, 395);
		this.textBox2.Name = "textBox2";
		this.textBox2.Size = new System.Drawing.Size(133, 13);
		this.textBox2.TabIndex = 64;
		this.textBox2.Text = "Frequency: (mhz)";
		this.textBox5.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.textBox5.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.textBox5.Location = new System.Drawing.Point(655, 395);
		this.textBox5.Name = "textBox5";
		this.textBox5.Size = new System.Drawing.Size(163, 13);
		this.textBox5.TabIndex = 65;
		this.textBox5.Text = "Group:";
		this.textBox7.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.textBox7.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.textBox7.Location = new System.Drawing.Point(824, 395);
		this.textBox7.Name = "textBox7";
		this.textBox7.Size = new System.Drawing.Size(127, 13);
		this.textBox7.TabIndex = 66;
		this.textBox7.Text = "Name:";
		this.openFileDialog1.Filter = "WAV files (*.wav)|*.wav|All files (*.*)|*.*";
		this.openFileDialog1.Multiselect = true;
		this.ScheduleExtra.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.ScheduleExtra.Increment = new decimal(new int[4] { 5, 0, 0, 0 });
		this.ScheduleExtra.Location = new System.Drawing.Point(237, 466);
		this.ScheduleExtra.Maximum = new decimal(new int[4] { 120, 0, 0, 0 });
		this.ScheduleExtra.Minimum = new decimal(new int[4]);
		this.ScheduleExtra.Name = "ScheduleExtra";
		this.ScheduleExtra.Size = new System.Drawing.Size(56, 20);
		this.ScheduleExtra.TabIndex = 71;
		this.ScheduleExtra.TinyStep = false;
		this.ScheduleExtra.Value = new decimal(new int[4] { 30, 0, 0, 0 });
		this.ScheduleExtra.Visible = false;
		this.chkAlwaysOnTop.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.chkAlwaysOnTop.Image = null;
		this.chkAlwaysOnTop.Location = new System.Drawing.Point(347, 516);
		this.chkAlwaysOnTop.Name = "chkAlwaysOnTop";
		this.chkAlwaysOnTop.Size = new System.Drawing.Size(79, 36);
		this.chkAlwaysOnTop.TabIndex = 59;
		this.chkAlwaysOnTop.Text = "Always On Top";
		this.chkAlwaysOnTop.CheckedChanged += new System.EventHandler(chkAlwaysOnTop_CheckedChanged);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(975, 558);
		base.Controls.Add(this.buttonTS1);
		base.Controls.Add(this.ScheduleExtra);
		base.Controls.Add(this.ScheduleRepeatm);
		base.Controls.Add(this.MemName);
		base.Controls.Add(this.MemGroup);
		base.Controls.Add(this.MemFreq);
		base.Controls.Add(this.textBox7);
		base.Controls.Add(this.textBox5);
		base.Controls.Add(this.textBox2);
		base.Controls.Add(this.ScheduleRemain);
		base.Controls.Add(this.ScheduleOn);
		base.Controls.Add(this.ScheduleStartTime);
		base.Controls.Add(this.ScheduleStartDate);
		base.Controls.Add(this.chkAlwaysOnTop);
		base.Controls.Add(this.ScheduleDurationTime);
		base.Controls.Add(this.ScheduleRecord);
		base.Controls.Add(this.ScheduleRepeat);
		base.Controls.Add(this.textBox8);
		base.Controls.Add(this.textBox6);
		base.Controls.Add(this.textBox4);
		base.Controls.Add(this.textBox3);
		base.Controls.Add(this.MemComments);
		base.Controls.Add(this.textBox1);
		base.Controls.Add(this.chkMemoryFormClose);
		base.Controls.Add(this.btnSelect);
		base.Controls.Add(this.btnMemoryRecordDelete);
		base.Controls.Add(this.btnMemoryRecordCopy);
		base.Controls.Add(this.MemoryRecordAdd);
		base.Controls.Add(this.dataGridView1);
		base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		this.MinimumSize = new System.Drawing.Size(526, 367);
		base.Name = "MemoryForm";
		this.Text = "Memory Interface";
		this.toolTip1.SetToolTip(this, resources.GetString("$this.ToolTip"));
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(MemoryForm_FormClosing);
		base.Load += new System.EventHandler(MemoryForm_Load);
		((System.ComponentModel.ISupportInitialize)this.dataGridView1).EndInit();
		((System.ComponentModel.ISupportInitialize)this.ScheduleDurationTime).EndInit();
		((System.ComponentModel.ISupportInitialize)this.ScheduleExtra).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
