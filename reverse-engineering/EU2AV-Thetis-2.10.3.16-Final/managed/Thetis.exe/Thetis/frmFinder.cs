using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

namespace Thetis;

public class frmFinder : Form
{
	private class SearchData
	{
		public Control Control { get; set; }

		public string Name { get; set; }

		public string FullName { get; set; }

		public string Text { get; set; }

		public string ToolTip { get; set; }

		public string ShortName { get; set; }

		public string XMLReplacement { get; set; }
	}

	private class CatStructEntry
	{
		public string Code { get; set; }

		public string Description { get; set; }
	}

	private Dictionary<string, SearchData> _searchData;

	private object _objLocker;

	private object _objWTLocker;

	private Dictionary<string, Thread> _workerThreads;

	private bool _fullDetails;

	private StringFormat _stringFormat;

	private Dictionary<string, string> _xmlData = new Dictionary<string, string>();

	private bool _fullName;

	private bool _highlightResults;

	private bool _keywords;

	private static readonly HashSet<Type> target_types = new HashSet<Type>
	{
		typeof(CheckBoxTS),
		typeof(CheckBox),
		typeof(ComboBoxTS),
		typeof(ComboBox),
		typeof(NumericUpDownTS),
		typeof(NumericUpDown),
		typeof(RadioButtonTS),
		typeof(RadioButton),
		typeof(TextBoxTS),
		typeof(TextBox),
		typeof(TrackBarTS),
		typeof(TrackBar),
		typeof(ColorButton),
		typeof(ucLGPicker),
		typeof(RichTextBox),
		typeof(LabelTS),
		typeof(Label),
		typeof(ButtonTS)
	};

	private bool _ignoreUpdateToList;

	private SearchData _oldSelectedSearchResult;

	private IContainer components;

	private TextBoxTS txtSearch;

	private ListBox lstResults;

	private CheckBoxTS chkFullDetails;

	private CheckBoxTS chkHighlight;

	private ToolTip toolTip1;

	private CheckBoxTS chkKeywords;

	public frmFinder()
	{
		_objLocker = new object();
		_objWTLocker = new object();
		_searchData = new Dictionary<string, SearchData>();
		_workerThreads = new Dictionary<string, Thread>();
		_fullName = false;
		_xmlData = new Dictionary<string, string>();
		_stringFormat = new StringFormat(StringFormat.GenericTypographic);
		_stringFormat.FormatFlags = StringFormatFlags.MeasureTrailingSpaces | StringFormatFlags.NoWrap | StringFormatFlags.NoClip;
		InitializeComponent();
		_highlightResults = chkHighlight.Checked;
		_fullDetails = chkFullDetails.Checked;
		_keywords = chkKeywords.Checked;
	}

	public void GatherSearchData(Form frm, ToolTip tt)
	{
		if (!_workerThreads.ContainsKey(frm.Name))
		{
			Thread thread = new Thread((ThreadStart)delegate
			{
				gatherSearchDataThread(frm, tt);
			})
			{
				Name = "Finder Worker Thread for " + frm.Name,
				Priority = ThreadPriority.BelowNormal,
				IsBackground = true
			};
			lock (_objWTLocker)
			{
				_workerThreads.Add(frm.Name, thread);
			}
			thread.Start();
		}
	}

	public void GatherCATStructData(string file_path)
	{
		string text = "CATSTRUCT_" + file_path;
		if (!_workerThreads.ContainsKey(text))
		{
			Thread thread = new Thread((ThreadStart)delegate
			{
				gatherCATStructSearchDataThread(file_path);
			})
			{
				Name = "Finder Worker Thread for " + text,
				Priority = ThreadPriority.Highest,
				IsBackground = true
			};
			lock (_objWTLocker)
			{
				_workerThreads.Add(text, thread);
			}
			thread.Start();
		}
	}

	private void gatherCATStructSearchDataThread(string file_path)
	{
		try
		{
			XElement xElement = XDocument.Load(file_path).Element("catstructs");
			new List<CatStructEntry>();
			foreach (XElement item in xElement.Elements("catstruct"))
			{
				XAttribute xAttribute = item.Attribute("code");
				XElement xElement2 = item.Element("desc");
				item.Element("active");
				XElement xElement3 = item.Element("nsetparms");
				XElement xElement4 = item.Element("ngetparms");
				XElement xElement5 = item.Element("nansparms");
				if (xAttribute != null && xElement2 != null)
				{
					string value = xAttribute.Value;
					string value2 = xElement2.Value;
					int result = -1;
					int.TryParse(xElement3.Value, out result);
					int result2 = -1;
					int.TryParse(xElement4.Value, out result2);
					int result3 = -1;
					int.TryParse(xElement5.Value, out result3);
					string text = "";
					if (result > 0)
					{
						text += $" set[{result}]";
					}
					if (result2 > 0)
					{
						text += $" get[{result2}]";
					}
					if (result3 > 0)
					{
						text += $" ans[{result3}]";
					}
					text = text.Trim();
					SearchData searchData = new SearchData();
					searchData.Control = null;
					searchData.Name = "CATcommand";
					searchData.ShortName = "";
					searchData.Text = value + " : " + value2 + "   " + text;
					searchData.ToolTip = value + " : " + value2;
					searchData.XMLReplacement = "";
					searchData.FullName = value + " : " + value2;
					SearchData value3 = searchData;
					lock (_objLocker)
					{
						_searchData.Add(Guid.NewGuid().ToString(), value3);
					}
				}
			}
		}
		catch
		{
		}
		string key = "CATSTRUCT_" + file_path;
		lock (_objWTLocker)
		{
			_workerThreads.Remove(key);
		}
	}

	private void gatherSearchDataThread(Control frm, ToolTip tt)
	{
		getControlList(frm, tt);
		lock (_objWTLocker)
		{
			_workerThreads.Remove(frm.Name);
		}
	}

	private static string stripPrefix(string name)
	{
		string[] array = new string[12]
		{
			"clrbtn", "combo", "label", "text", "nud", "lbl", "chk", "rad", "txt", "tb",
			"ud", "btn"
		};
		foreach (string text in array)
		{
			if (name.StartsWith(text, StringComparison.OrdinalIgnoreCase))
			{
				return name.Substring(text.Length);
			}
		}
		return name;
	}

	private void getControlList(Control root, ToolTip tt)
	{
		HashSet<string> hashSet;
		lock (_objLocker)
		{
			hashSet = new HashSet<string>(_searchData.Keys);
		}
		List<SearchData> list = new List<SearchData>();
		Stack<Control> stack = new Stack<Control>();
		stack.Push(root);
		while (stack.Count > 0)
		{
			Control control = stack.Pop();
			for (int i = 0; i < control.Controls.Count; i++)
			{
				stack.Push(control.Controls[i]);
			}
			if (!target_types.Contains(control.GetType()))
			{
				continue;
			}
			string fullName = control.GetFullName();
			if (hashSet.Contains(fullName))
			{
				continue;
			}
			string toolTip = "";
			if (tt != null)
			{
				string toolTip2 = tt.GetToolTip(control);
				if (!string.IsNullOrEmpty(toolTip2))
				{
					toolTip = toolTip2.Replace("\n", " ");
				}
			}
			string shortName = stripPrefix(control.Name);
			string text = control.Text;
			if (!string.IsNullOrEmpty(text) && text.IndexOf('\n') >= 0)
			{
				text = text.Replace("\n", " ");
			}
			string xMLReplacement = "";
			if (_xmlData.ContainsKey(fullName))
			{
				xMLReplacement = _xmlData[fullName];
			}
			SearchData item = new SearchData
			{
				Control = control,
				Name = control.Name,
				ShortName = shortName,
				Text = (string.IsNullOrEmpty(text) ? "" : text),
				ToolTip = toolTip,
				XMLReplacement = xMLReplacement,
				FullName = fullName
			};
			list.Add(item);
			hashSet.Add(fullName);
		}
		if (list.Count == 0)
		{
			return;
		}
		lock (_objLocker)
		{
			for (int j = 0; j < list.Count; j++)
			{
				SearchData searchData = list[j];
				if (!_searchData.ContainsKey(searchData.FullName))
				{
					_searchData.Add(searchData.FullName, searchData);
				}
			}
		}
	}

	private void txtSearch_TextChanged(object sender, EventArgs e)
	{
		if (string.IsNullOrEmpty(txtSearch.Text))
		{
			lstResults.DataSource = null;
			lstResults.Items.Clear();
			return;
		}
		lock (_objLocker)
		{
			List<SearchData> dataSource;
			if (!_keywords)
			{
				string sSearch = txtSearch.Text.ToLower();
				dataSource = _searchData.Where((KeyValuePair<string, SearchData> kv) => kv.Value.Name.Contains(sSearch, StringComparison.OrdinalIgnoreCase) || kv.Value.Text.Contains(sSearch, StringComparison.OrdinalIgnoreCase) || kv.Value.ToolTip.Contains(sSearch, StringComparison.OrdinalIgnoreCase) || kv.Value.XMLReplacement.Contains(sSearch, StringComparison.OrdinalIgnoreCase)).ToDictionary((KeyValuePair<string, SearchData> kv) => kv.Key, (KeyValuePair<string, SearchData> kv) => kv.Value).Values.ToList();
			}
			else
			{
				string[] search = (from s in txtSearch.Text.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
					select s.ToLower()).ToArray();
				dataSource = _searchData.Where((KeyValuePair<string, SearchData> kv) => search.All((string term) => kv.Value.Name.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0 || kv.Value.Text.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0 || kv.Value.ToolTip.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0 || kv.Value.XMLReplacement.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0)).ToDictionary((KeyValuePair<string, SearchData> kv) => kv.Key, (KeyValuePair<string, SearchData> kv) => kv.Value).Values.ToList();
			}
			_ignoreUpdateToList = true;
			lstResults.DataSource = null;
			lstResults.Items.Clear();
			lstResults.DisplayMember = "Text";
			lstResults.DataSource = dataSource;
			lstResults.ClearSelected();
			_ignoreUpdateToList = false;
		}
	}

	private void lstResults_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (_oldSelectedSearchResult != null && _oldSelectedSearchResult.Control != null)
		{
			Common.HightlightControl(_oldSelectedSearchResult.Control, bHighlight: false, bFromFinder: true);
			_oldSelectedSearchResult = null;
		}
		if (lstResults.SelectedItems.Count != 0 && !_ignoreUpdateToList && lstResults.SelectedItem is SearchData { Control: not null } searchData)
		{
			showControl(searchData.Control);
			Common.HightlightControl(searchData.Control, bHighlight: true, bFromFinder: true);
			_oldSelectedSearchResult = searchData;
		}
	}

	private void lstResults_DrawItem(object sender, DrawItemEventArgs e)
	{
		if (e.Index < 0)
		{
			return;
		}
		ListBox listBox = (ListBox)sender;
		SearchData searchData = listBox.Items[e.Index] as SearchData;
		Graphics graphics = e.Graphics;
		Color color = (e.State.HasFlag(DrawItemState.Selected) ? SystemColors.Highlight : SystemColors.Window);
		Color color2 = (e.State.HasFlag(DrawItemState.Selected) ? SystemColors.HighlightText : SystemColors.ControlText);
		if (!e.State.HasFlag(DrawItemState.Selected))
		{
			color = ((e.Index % 2 == 0) ? color : applyTint(color, Color.LightGray));
		}
		graphics.FillRectangle(new SolidBrush(color), e.Bounds);
		int num = e.Bounds.Y;
		if (_fullDetails)
		{
			string sSearchText = txtSearch.Text.ToLower();
			if (!string.IsNullOrEmpty(searchData.Text))
			{
				highlight(sSearchText, searchData.Text, listBox, e.Bounds.X, num, graphics);
				graphics.DrawString(searchData.Text, listBox.Font, new SolidBrush(color2), e.Bounds.X, num, _stringFormat);
				num += 20;
			}
			if (_xmlData.ContainsKey(searchData.FullName))
			{
				string text = _xmlData[searchData.FullName];
				text = text.Replace("\n", "").Replace("\r", "").Replace("\t", "");
				highlight(sSearchText, text, listBox, e.Bounds.X, num, graphics);
				graphics.DrawString(text, listBox.Font, new SolidBrush(color2), e.Bounds.X, num, _stringFormat);
				num += 20;
			}
			else if (!string.IsNullOrEmpty(searchData.ToolTip))
			{
				highlight(sSearchText, searchData.ToolTip, listBox, e.Bounds.X, num, graphics);
				graphics.DrawString(searchData.ToolTip, listBox.Font, new SolidBrush(color2), e.Bounds.X, num, _stringFormat);
				num += 20;
			}
			highlight(sSearchText, _fullName ? searchData.FullName : searchData.Name, listBox, e.Bounds.X, num, graphics);
			graphics.DrawString(_fullName ? searchData.FullName : searchData.Name, listBox.Font, new SolidBrush(color2), e.Bounds.X, num, _stringFormat);
			return;
		}
		string text2;
		if (_xmlData.ContainsKey(searchData.FullName))
		{
			text2 = _xmlData[searchData.FullName];
			text2 = text2.Replace("\n", "").Replace("\r", "").Replace("\t", "");
		}
		else if (!string.IsNullOrEmpty(searchData.ToolTip))
		{
			text2 = searchData.ToolTip;
		}
		else
		{
			string text3 = "";
			if (!string.IsNullOrEmpty(searchData.Text))
			{
				text3 = " [" + searchData.Text + "]";
			}
			text2 = searchData.ShortName + text3;
		}
		highlight(txtSearch.Text.ToLower(), text2, listBox, e.Bounds.X, e.Bounds.Y, graphics);
		graphics.DrawString(text2, listBox.Font, new SolidBrush(color2), e.Bounds.X, num, _stringFormat);
	}

	private void highlight(string sSearchText, string sLineText, ListBox listBox, int xPos, int yPos, Graphics g)
	{
		if (!_highlightResults)
		{
			return;
		}
		string[] array = (_keywords ? sSearchText.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries) : new string[1] { sSearchText });
		foreach (string text in array)
		{
			foreach (Tuple<int, int> item in findSubstringOccurrences(sLineText.ToLower(), text.ToLower()))
			{
				float num = g.MeasureString(sLineText.Substring(0, item.Item1), listBox.Font, int.MaxValue, _stringFormat).Width;
				float num2 = g.MeasureString(sLineText.Substring(item.Item1, text.Length), listBox.Font, int.MaxValue, _stringFormat).Width;
				Rectangle rect = new Rectangle(xPos + (int)num, yPos, (int)num2, 20);
				CompositingMode compositingMode = g.CompositingMode;
				g.CompositingMode = CompositingMode.SourceOver;
				using (SolidBrush brush = new SolidBrush(Color.FromArgb(102, Color.Yellow)))
				{
					g.FillRectangle(brush, rect);
				}
				g.CompositingMode = compositingMode;
			}
		}
	}

	private List<Tuple<int, int>> findSubstringOccurrences(string inputString, string searchString)
	{
		List<Tuple<int, int>> list = new List<Tuple<int, int>>();
		int num;
		for (num = 0; num < inputString.Length; num += searchString.Length)
		{
			num = inputString.IndexOf(searchString, num, StringComparison.Ordinal);
			if (num == -1)
			{
				break;
			}
			int item = num;
			int item2 = num + searchString.Length - 1;
			list.Add(new Tuple<int, int>(item, item2));
		}
		return list;
	}

	private Color applyTint(Color baseColor, Color tintColor)
	{
		int red = (baseColor.R + tintColor.R) / 2;
		int green = (baseColor.G + tintColor.G) / 2;
		int blue = (baseColor.B + tintColor.B) / 2;
		return Color.FromArgb(red, green, blue);
	}

	private void frmFinder_FormClosing(object sender, FormClosingEventArgs e)
	{
		if (e.CloseReason == CloseReason.UserClosing)
		{
			Hide();
			e.Cancel = true;
		}
		txtSearch.Text = "";
		Common.SaveForm(this, base.Name);
	}

	public new void Show()
	{
		Common.RestoreForm(this, base.Name, restore_size: true);
		if (string.IsNullOrEmpty(txtSearch.Text))
		{
			txtSearch.Text = "Search";
		}
		base.Show();
		txtSearch.Focus();
	}

	private void lstResults_MeasureItem(object sender, MeasureItemEventArgs e)
	{
		if (e.Index < 0)
		{
			return;
		}
		SearchData searchData = ((ListBox)sender).Items[e.Index] as SearchData;
		int num = 20;
		if (searchData != null && _fullDetails)
		{
			if (!string.IsNullOrEmpty(searchData.Text))
			{
				num += 20;
			}
			if (!string.IsNullOrEmpty(searchData.ToolTip))
			{
				num += 20;
			}
		}
		e.ItemHeight = num;
	}

	private void showControl(Control c)
	{
		if (c == null)
		{
			return;
		}
		Form form = c.FindForm();
		if (form != null)
		{
			if (!form.Visible)
			{
				form.Show();
			}
			form.BringToFront();
			selectRequiredTabs(form, c);
			SuspendLayout();
			BringToFront();
			lstResults.Focus();
			ResumeLayout();
		}
	}

	private void selectRequiredTabs(Control parentControl, Control targetControl)
	{
		if (parentControl == null || targetControl == null)
		{
			return;
		}
		List<TabPage> list = new List<TabPage>();
		HashSet<TabPage> hashSet = new HashSet<TabPage>();
		Control control = targetControl;
		while (control != null && control != parentControl)
		{
			if (control is TabPage item && !hashSet.Contains(item))
			{
				list.Add(item);
				hashSet.Add(item);
			}
			control = control.Parent;
		}
		if (control != parentControl)
		{
			return;
		}
		for (int num = list.Count - 1; num >= 0; num--)
		{
			TabPage tabPage = list[num];
			if (tabPage.Parent is TabControl tabControl && tabControl.TabPages.Contains(tabPage))
			{
				tabControl.SelectedTab = tabPage;
			}
		}
		parentControl.PerformLayout();
		List<ScrollableControl> list2 = new List<ScrollableControl>();
		Control control2 = targetControl.Parent;
		while (control2 != null && control2 != parentControl)
		{
			if (control2 is ScrollableControl { AutoScroll: not false } scrollableControl)
			{
				list2.Add(scrollableControl);
			}
			control2 = control2.Parent;
		}
		if (parentControl is ScrollableControl { AutoScroll: not false } scrollableControl2)
		{
			list2.Add(scrollableControl2);
		}
		for (int i = 0; i < list2.Count; i++)
		{
			ScrollableControl scrollableControl3 = list2[i];
			scrollableControl3.ScrollControlIntoView(targetControl);
			scrollableControl3.Update();
		}
	}

	private void chkFullDetails_CheckedChanged(object sender, EventArgs e)
	{
		lock (_objLocker)
		{
			SearchData searchData = lstResults.SelectedItem as SearchData;
			_fullDetails = chkFullDetails.Checked;
			txtSearch_TextChanged(this, EventArgs.Empty);
			if (searchData != null)
			{
				lstResults.SelectedItem = searchData;
			}
		}
	}

	public void ReadXmlFinderFile(string directoryPath)
	{
		string text = Path.Combine(directoryPath, "Finder.xml");
		_xmlData.Clear();
		if (!File.Exists(text))
		{
			return;
		}
		try
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(text);
			foreach (XmlNode item in xmlDocument.SelectNodes("//element"))
			{
				string text2 = item.SelectSingleNode("control")?.InnerText;
				string value = item.SelectSingleNode("text")?.InnerText;
				if (!string.IsNullOrEmpty(text2) && !string.IsNullOrEmpty(value))
				{
					_xmlData.Add(text2, value);
				}
			}
		}
		catch
		{
		}
	}

	public void WriteXmlFinderFile(string directoryPath)
	{
		string text = Path.Combine(directoryPath, "Finder.xml");
		try
		{
			if (File.Exists(text))
			{
				return;
			}
			int num = 0;
			bool flag = false;
			lock (_objWTLocker)
			{
				flag = _workerThreads.Count > 0;
			}
			while (flag)
			{
				Thread.Sleep(50);
				lock (_objWTLocker)
				{
					flag = _workerThreads.Count > 0;
				}
				num++;
				if (num > 200)
				{
					return;
				}
			}
			XmlDocument xmlDocument = new XmlDocument();
			XmlDeclaration newChild = xmlDocument.CreateXmlDeclaration("1.0", "UTF-8", null);
			xmlDocument.AppendChild(newChild);
			XmlElement xmlElement = xmlDocument.CreateElement("root");
			xmlDocument.AppendChild(xmlElement);
			lock (_objLocker)
			{
				foreach (KeyValuePair<string, SearchData> searchDatum in _searchData)
				{
					if (searchDatum.Value.Control != null)
					{
						XmlElement xmlElement2 = xmlDocument.CreateElement("element");
						XmlElement xmlElement3 = xmlDocument.CreateElement("control");
						xmlElement3.InnerText = searchDatum.Key;
						xmlElement2.AppendChild(xmlElement3);
						XmlElement xmlElement4 = xmlDocument.CreateElement("text");
						xmlElement4.InnerText = null;
						xmlElement2.AppendChild(xmlElement4);
						xmlElement.AppendChild(xmlElement2);
					}
				}
			}
			xmlDocument.Save(text);
		}
		catch
		{
		}
	}

	private void frmFinder_KeyDown(object sender, KeyEventArgs e)
	{
		if (e.Alt && _fullDetails)
		{
			lock (_objLocker)
			{
				_fullName = !_fullName;
				SearchData searchData = lstResults.SelectedItem as SearchData;
				txtSearch_TextChanged(this, EventArgs.Empty);
				if (searchData != null)
				{
					lstResults.SelectedItem = searchData;
				}
			}
			e.Handled = true;
		}
		else if (e.Control && e.KeyCode == Keys.C && lstResults.SelectedItem is SearchData searchData2)
		{
			try
			{
				Clipboard.SetText(searchData2.FullName);
				e.Handled = true;
			}
			catch
			{
			}
		}
	}

	private void chkHighlight_CheckedChanged(object sender, EventArgs e)
	{
		lock (_objLocker)
		{
			SearchData searchData = lstResults.SelectedItem as SearchData;
			_highlightResults = chkHighlight.Checked;
			txtSearch_TextChanged(this, EventArgs.Empty);
			if (searchData != null)
			{
				lstResults.SelectedItem = searchData;
			}
		}
	}

	private void chkKeywords_CheckedChanged(object sender, EventArgs e)
	{
		lock (_objLocker)
		{
			SearchData searchData = lstResults.SelectedItem as SearchData;
			_keywords = chkKeywords.Checked;
			txtSearch_TextChanged(this, EventArgs.Empty);
			if (searchData != null)
			{
				lstResults.SelectedItem = searchData;
			}
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
		this.lstResults = new System.Windows.Forms.ListBox();
		this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
		this.chkKeywords = new System.Windows.Forms.CheckBoxTS();
		this.chkHighlight = new System.Windows.Forms.CheckBoxTS();
		this.chkFullDetails = new System.Windows.Forms.CheckBoxTS();
		this.txtSearch = new System.Windows.Forms.TextBoxTS();
		base.SuspendLayout();
		this.lstResults.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.lstResults.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lstResults.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
		this.lstResults.Font = new System.Drawing.Font("Microsoft Sans Serif", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.lstResults.FormattingEnabled = true;
		this.lstResults.IntegralHeight = false;
		this.lstResults.ItemHeight = 60;
		this.lstResults.Location = new System.Drawing.Point(12, 66);
		this.lstResults.Name = "lstResults";
		this.lstResults.Size = new System.Drawing.Size(280, 304);
		this.lstResults.TabIndex = 0;
		this.lstResults.DrawItem += new System.Windows.Forms.DrawItemEventHandler(lstResults_DrawItem);
		this.lstResults.MeasureItem += new System.Windows.Forms.MeasureItemEventHandler(lstResults_MeasureItem);
		this.lstResults.SelectedIndexChanged += new System.EventHandler(lstResults_SelectedIndexChanged);
		this.chkKeywords.AutoSize = true;
		this.chkKeywords.Image = null;
		this.chkKeywords.Location = new System.Drawing.Point(220, 8);
		this.chkKeywords.Name = "chkKeywords";
		this.chkKeywords.Size = new System.Drawing.Size(72, 17);
		this.chkKeywords.TabIndex = 3;
		this.chkKeywords.Text = "Keywords";
		this.toolTip1.SetToolTip(this.chkKeywords, "Treat each separated word with a space as a keyword to search on");
		this.chkKeywords.UseVisualStyleBackColor = true;
		this.chkKeywords.CheckedChanged += new System.EventHandler(chkKeywords_CheckedChanged);
		this.chkHighlight.AutoSize = true;
		this.chkHighlight.Checked = true;
		this.chkHighlight.CheckState = System.Windows.Forms.CheckState.Checked;
		this.chkHighlight.Image = null;
		this.chkHighlight.Location = new System.Drawing.Point(102, 8);
		this.chkHighlight.Name = "chkHighlight";
		this.chkHighlight.Size = new System.Drawing.Size(105, 17);
		this.chkHighlight.TabIndex = 2;
		this.chkHighlight.Text = "Highlight Results";
		this.toolTip1.SetToolTip(this.chkHighlight, "Highlight part of the search that matched. It may be hidden if Full Details is not enabled.");
		this.chkHighlight.UseVisualStyleBackColor = true;
		this.chkHighlight.CheckedChanged += new System.EventHandler(chkHighlight_CheckedChanged);
		this.chkFullDetails.AutoSize = true;
		this.chkFullDetails.Image = null;
		this.chkFullDetails.Location = new System.Drawing.Point(12, 8);
		this.chkFullDetails.Name = "chkFullDetails";
		this.chkFullDetails.Size = new System.Drawing.Size(75, 17);
		this.chkFullDetails.TabIndex = 1;
		this.chkFullDetails.Text = "Full details";
		this.toolTip1.SetToolTip(this.chkFullDetails, "Shows full details. Text, Tooltip and Control name. Use ALT to toglle full control name for use in Finder.xml");
		this.chkFullDetails.UseVisualStyleBackColor = true;
		this.chkFullDetails.CheckedChanged += new System.EventHandler(chkFullDetails_CheckedChanged);
		this.txtSearch.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.txtSearch.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.txtSearch.Location = new System.Drawing.Point(12, 31);
		this.txtSearch.Name = "txtSearch";
		this.txtSearch.Size = new System.Drawing.Size(280, 29);
		this.txtSearch.TabIndex = 0;
		this.txtSearch.Text = "Search";
		this.toolTip1.SetToolTip(this.txtSearch, "Type something to search");
		this.txtSearch.TextChanged += new System.EventHandler(txtSearch_TextChanged);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(304, 382);
		base.Controls.Add(this.chkKeywords);
		base.Controls.Add(this.chkHighlight);
		base.Controls.Add(this.chkFullDetails);
		base.Controls.Add(this.lstResults);
		base.Controls.Add(this.txtSearch);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
		base.KeyPreview = true;
		this.MinimumSize = new System.Drawing.Size(320, 400);
		base.Name = "frmFinder";
		base.ShowIcon = false;
		base.ShowInTaskbar = false;
		this.Text = "Finder";
		base.TopMost = true;
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(frmFinder_FormClosing);
		base.KeyDown += new System.Windows.Forms.KeyEventHandler(frmFinder_KeyDown);
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
