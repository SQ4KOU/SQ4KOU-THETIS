using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

namespace Thetis;

public static class LogTool
{
	private struct Entry
	{
		public string base_text;

		public ListViewItem item;

		public DateTime start_utc;

		public bool colour_warn;

		public bool completed;

		public long completed_ms;
	}

	private sealed class LogForm : Form
	{
		private sealed class NoSelectListView : ListView
		{
			private const int WM_UPDATEUISTATE = 296;

			private const int UIS_SET = 1;

			private const int UISF_HIDEFOCUS = 1;

			[DllImport("user32.dll")]
			private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

			protected override void OnCreateControl()
			{
				base.OnCreateControl();
				if (base.IsHandleCreated)
				{
					SendMessage(base.Handle, 296, (IntPtr)65537, IntPtr.Zero);
				}
			}

			protected override void OnItemSelectionChanged(ListViewItemSelectionChangedEventArgs e)
			{
				if (e.IsSelected)
				{
					e.Item.Selected = false;
				}
				base.OnItemSelectionChanged(e);
			}

			protected override void OnGotFocus(EventArgs e)
			{
				base.OnGotFocus(e);
				if (base.IsHandleCreated)
				{
					SendMessage(base.Handle, 296, (IntPtr)65537, IntPtr.Zero);
				}
				if (base.SelectedIndices.Count != 0)
				{
					base.SelectedIndices.Clear();
				}
				if (base.FocusedItem != null)
				{
					base.FocusedItem.Focused = false;
				}
			}

			protected override void OnMouseDown(MouseEventArgs e)
			{
				base.OnMouseDown(e);
				if (base.SelectedIndices.Count != 0)
				{
					base.SelectedIndices.Clear();
				}
				if (base.FocusedItem != null)
				{
					base.FocusedItem.Focused = false;
				}
			}

			protected override void OnKeyDown(KeyEventArgs e)
			{
				e.Handled = true;
				base.OnKeyDown(e);
			}
		}

		public readonly ListView list;

		public readonly Panel close_panel;

		public readonly Label time_label;

		private readonly Button close_button;

		public LogForm()
		{
			Text = "Thetis Startup Log";
			base.StartPosition = FormStartPosition.Manual;
			base.Size = new Size(400, 680);
			base.FormBorderStyle = FormBorderStyle.FixedToolWindow;
			base.ShowInTaskbar = false;
			BackColor = Color.Black;
			list = new NoSelectListView();
			list.View = View.Details;
			list.HeaderStyle = ColumnHeaderStyle.None;
			list.Columns.Add("", -2, HorizontalAlignment.Left);
			list.Dock = DockStyle.Fill;
			list.FullRowSelect = true;
			list.BackColor = Color.Black;
			list.ForeColor = Color.Lime;
			list.Font = new Font("Courier New", 10f, FontStyle.Regular);
			list.BorderStyle = BorderStyle.None;
			list.HandleCreated += delegate
			{
				hideHorizontalScrollBar(list);
			};
			list.SizeChanged += delegate
			{
				hideHorizontalScrollBar(list);
			};
			list.MultiSelect = false;
			list.HideSelection = true;
			close_panel = new Panel();
			close_panel.Dock = DockStyle.Bottom;
			close_panel.Height = 80;
			close_panel.BackColor = Color.Black;
			close_panel.Visible = false;
			time_label = new Label();
			time_label.AutoSize = true;
			time_label.Text = "Completed in 0.0s";
			time_label.ForeColor = Color.Lime;
			time_label.BackColor = Color.Black;
			time_label.Font = new Font("Courier New", 10f, FontStyle.Regular);
			time_label.Left = 6;
			time_label.Top = 6;
			close_button = new Button();
			close_button.Text = "Close";
			close_button.AutoSize = true;
			close_button.Anchor = AnchorStyles.None;
			close_button.FlatStyle = FlatStyle.System;
			close_button.Click += delegate
			{
				HideAndSave();
			};
			close_panel.Controls.Add(time_label);
			close_panel.Controls.Add(close_button);
			base.Controls.Add(list);
			base.Controls.Add(close_panel);
			base.Resize += delegate
			{
				layoutColumns();
				centerClose();
			};
			base.Shown += delegate
			{
				layoutColumns();
				centerClose();
			};
			close_panel.Resize += delegate
			{
				centerClose();
			};
			Common.DoubleBufferAll(this, enabled: true);
			Common.UseImmersiveDarkMode(base.Handle, enabled: true);
		}

		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			e.Cancel = true;
			HideAndSave();
		}

		private void layoutColumns()
		{
			if (list.Columns.Count != 0)
			{
				int num = list.ClientSize.Width - 4;
				if (num < 50)
				{
					num = 50;
				}
				list.Columns[0].Width = num;
				if (list.IsHandleCreated)
				{
					ShowScrollBar(list.Handle, 0, bShow: false);
				}
			}
		}

		private void centerClose()
		{
			Panel panel = close_panel;
			int num = (panel.ClientSize.Width - close_button.Width) / 2;
			int num2 = panel.ClientSize.Height - close_button.Height - 8;
			if (num < 0)
			{
				num = 0;
			}
			if (num2 < 0)
			{
				num2 = 0;
			}
			close_button.Left = num;
			close_button.Top = num2;
		}
	}

	private const int GWLP_HWNDPARENT = -8;

	private static LogForm _form;

	private static readonly object _sync = new object();

	private static readonly Dictionary<string, Entry> _entries = new Dictionary<string, Entry>();

	private static DateTime _total_start_utc;

	private static int _seq = 1;

	private static readonly string _reg_subkey = "Software\\OpenHPSDR\\Thetis-x64";

	[DllImport("user32.dll", EntryPoint = "SetWindowLong")]
	private static extern IntPtr SetWindowLong32(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

	[DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
	private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

	private static IntPtr setWindowLongAuto(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
	{
		if (Common.Is64Bit)
		{
			return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
		}
		return SetWindowLong32(hWnd, nIndex, dwNewLong);
	}

	[DllImport("user32.dll")]
	private static extern bool ShowScrollBar(IntPtr hWnd, int wBar, bool bShow);

	public static void ShowNewLog(IntPtr ownerHandle)
	{
		ensureForm();
		setOwner(ownerHandle);
		runOnUiThreadSync(delegate
		{
			_form.list.Items.Clear();
			lock (_sync)
			{
				_entries.Clear();
			}
			_form.close_panel.Visible = false;
			_total_start_utc = DateTime.UtcNow;
			_form.time_label.Text = "Completed in 0.0s";
			if (tryReadLocation(out var p))
			{
				_form.StartPosition = FormStartPosition.Manual;
				_form.Location = p;
				Common.ForceFormOnScreen(_form);
			}
			if (readRegistryShow(out var show) & show)
			{
				_form.Show();
				_form.BringToFront();
			}
			else
			{
				_form.Hide();
			}
		});
	}

	public static void ShowLog(IntPtr ownerHandle)
	{
		ensureForm();
		setOwner(ownerHandle);
		runOnUiThread(delegate
		{
			_form.Show();
			_form.BringToFront();
		});
	}

	private static void setOwner(IntPtr ownerHandle)
	{
		runOnUiThreadSync(delegate
		{
			if (_form.IsHandleCreated)
			{
				setWindowLongAuto(_form.Handle, -8, ownerHandle);
			}
		});
	}

	public static string AddLogEntry(string text)
	{
		return AddLogEntry(text, colour_warn: true);
	}

	public static string AddLogEntry(string text, bool colour_warn)
	{
		ensureForm();
		string text2;
		lock (_sync)
		{
			text2 = _seq.ToString();
			_seq++;
		}
		addCore(text, text2, colour_warn);
		return text2;
	}

	public static bool AddLogEntry(string text, string id, bool colour_warn = true)
	{
		ensureForm();
		if (string.IsNullOrEmpty(id))
		{
			return false;
		}
		lock (_sync)
		{
			if (_entries.ContainsKey(id))
			{
				return false;
			}
		}
		addCore(text, id, colour_warn);
		return true;
	}

	public static void Shutdown()
	{
		if (_form != null && !_form.IsDisposed)
		{
			runOnUiThread(delegate
			{
				writeLocation(_form.Location);
				_form.Close();
				_form.Dispose();
				_form = null;
			});
		}
	}

	public static void Completed(string id)
	{
		ensureForm();
		bool flag;
		lock (_sync)
		{
			flag = _entries.TryGetValue(id, out var value);
			if (flag)
			{
				TimeSpan timeSpan = DateTime.UtcNow - value.start_utc;
				long completed_ms = ((timeSpan.TotalMilliseconds > 0.0) ? ((long)timeSpan.TotalMilliseconds) : 0);
				value.completed = true;
				value.completed_ms = completed_ms;
				_entries[id] = value;
			}
		}
		if (!flag)
		{
			return;
		}
		runOnUiThread(delegate
		{
			if (_entries.TryGetValue(id, out var value2))
			{
				if (value2.item != null)
				{
					value2.item.Text = value2.base_text + " " + value2.completed_ms + "ms";
					if (value2.colour_warn)
					{
						if (value2.completed_ms > 8000)
						{
							value2.item.ForeColor = Color.Red;
						}
						else if (value2.completed_ms > 2000)
						{
							value2.item.ForeColor = Color.Orange;
						}
						else
						{
							value2.item.ForeColor = Color.Lime;
						}
					}
					else
					{
						value2.item.ForeColor = Color.Lime;
					}
				}
				double totalSeconds = (DateTime.UtcNow - _total_start_utc).TotalSeconds;
				_form.time_label.Text = "Completed in " + totalSeconds.ToString("0.0") + "s";
			}
		});
	}

	public static void Finish()
	{
		ensureForm();
		runOnUiThread(delegate
		{
			double totalSeconds = (DateTime.UtcNow - _total_start_utc).TotalSeconds;
			_form.time_label.Text = "Completed in " + totalSeconds.ToString("0.0") + "s";
			_form.close_panel.Visible = true;
		});
	}

	public static void HideAndSave()
	{
		ensureForm();
		runOnUiThread(delegate
		{
			writeLocation(_form.Location);
			_form.Hide();
		});
	}

	public static void SetRegistryToShow(bool show)
	{
		writeRegistryShow(show);
	}

	public static bool GetRegistryToShow(out bool show)
	{
		return readRegistryShow(out show);
	}

	public static void SetRegistryDpiAwareness(bool enabled)
	{
		writeRegistryDpiAwareness(enabled);
	}

	public static bool GetRegistryDpiAwareness()
	{
		return readRegistryDpiAwareness();
	}

	private static void addCore(string text, string id, bool colour_warn)
	{
		DateTime utcNow = DateTime.UtcNow;
		lock (_sync)
		{
			Entry value = new Entry
			{
				base_text = text,
				item = null,
				start_utc = utcNow,
				colour_warn = colour_warn,
				completed = false,
				completed_ms = 0L
			};
			_entries[id] = value;
		}
		runOnUiThread(delegate
		{
			if (_entries.TryGetValue(id, out var value2))
			{
				ListViewItem listViewItem = new ListViewItem("");
				_form.list.Items.Add(listViewItem);
				value2.item = listViewItem;
				if (value2.completed)
				{
					listViewItem.Text = value2.base_text + " " + value2.completed_ms + "ms";
					if (value2.colour_warn)
					{
						if (value2.completed_ms > 4000)
						{
							listViewItem.ForeColor = Color.Red;
						}
						else if (value2.completed_ms > 2000)
						{
							listViewItem.ForeColor = Color.Orange;
						}
						else
						{
							listViewItem.ForeColor = Color.Lime;
						}
					}
					else
					{
						listViewItem.ForeColor = Color.Lime;
					}
				}
				else
				{
					listViewItem.Text = value2.base_text;
					listViewItem.ForeColor = Color.Lime;
				}
				lock (_sync)
				{
					_entries[id] = value2;
				}
				if (_form.list.Items.Count > 0)
				{
					_form.list.EnsureVisible(_form.list.Items.Count - 1);
				}
				hideHorizontalScrollBar(_form.list);
			}
		});
	}

	private static void ensureForm()
	{
		if (_form != null && !_form.IsDisposed)
		{
			return;
		}
		ManualResetEvent created = new ManualResetEvent(initialState: false);
		Exception init_ex = null;
		Thread thread = new Thread((ThreadStart)delegate
		{
			try
			{
				_form = new LogForm();
				_form.CreateControl();
				_ = _form.Handle;
				created.Set();
				Application.Run();
			}
			catch (Exception ex)
			{
				init_ex = ex;
				created.Set();
			}
		});
		thread.IsBackground = true;
		thread.SetApartmentState(ApartmentState.STA);
		thread.Start();
		created.WaitOne();
		if (init_ex != null)
		{
			throw init_ex;
		}
	}

	private static void runOnUiThreadSync(MethodInvoker a)
	{
		if (_form != null && !_form.IsDisposed && _form.IsHandleCreated)
		{
			if (_form.InvokeRequired)
			{
				_form.Invoke(a);
			}
			else
			{
				a();
			}
		}
	}

	private static void runOnUiThread(MethodInvoker a)
	{
		if (_form != null && !_form.IsDisposed && _form.IsHandleCreated)
		{
			_form.BeginInvoke(a);
		}
	}

	private static bool readRegistryShow(out bool show)
	{
		bool result = false;
		show = false;
		try
		{
			RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(_reg_subkey, writable: false);
			if (registryKey != null)
			{
				object value = registryKey.GetValue("ShowLog");
				registryKey.Close();
				if (value != null)
				{
					if (value is int)
					{
						show = (int)value == 1;
						result = true;
					}
					else if (value is string)
					{
						show = string.Equals((string)value, "1", StringComparison.OrdinalIgnoreCase);
						result = true;
					}
				}
				else
				{
					show = false;
					result = true;
				}
			}
		}
		catch
		{
		}
		return result;
	}

	private static void writeRegistryShow(bool show)
	{
		RegistryKey registryKey = Registry.CurrentUser.CreateSubKey(_reg_subkey);
		registryKey.SetValue("ShowLog", show ? 1 : 0, RegistryValueKind.DWord);
		registryKey.Close();
	}

	private static bool readRegistryDpiAwareness()
	{
		bool result = false;
		try
		{
			RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(_reg_subkey, writable: false);
			if (registryKey != null)
			{
				object value = registryKey.GetValue("DpiAwareness");
				registryKey.Close();
				if (value is int)
				{
					result = (int)value == 1;
				}
				else if (value is string)
				{
					result = string.Equals((string)value, "1", StringComparison.OrdinalIgnoreCase);
				}
			}
		}
		catch
		{
		}
		return result;
	}

	private static void writeRegistryDpiAwareness(bool enabled)
	{
		RegistryKey registryKey = Registry.CurrentUser.CreateSubKey(_reg_subkey);
		registryKey.SetValue("DpiAwareness", enabled ? 1 : 0, RegistryValueKind.DWord);
		registryKey.Close();
	}

	private static bool tryReadLocation(out Point p)
	{
		p = new Point(0, 0);
		RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(_reg_subkey, writable: false);
		if (registryKey == null)
		{
			return false;
		}
		object value = registryKey.GetValue("LogLeft");
		object value2 = registryKey.GetValue("LogTop");
		registryKey.Close();
		if (value == null || value2 == null)
		{
			return false;
		}
		if (!int.TryParse(value.ToString(), out var result))
		{
			return false;
		}
		if (!int.TryParse(value2.ToString(), out var result2))
		{
			return false;
		}
		p = new Point(result, result2);
		return true;
	}

	private static void writeLocation(Point p)
	{
		RegistryKey registryKey = Registry.CurrentUser.CreateSubKey(_reg_subkey);
		registryKey.SetValue("LogLeft", p.X, RegistryValueKind.DWord);
		registryKey.SetValue("LogTop", p.Y, RegistryValueKind.DWord);
		registryKey.Close();
	}

	private static void hideHorizontalScrollBar(ListView lv)
	{
		if (lv.IsHandleCreated)
		{
			ShowScrollBar(lv.Handle, 0, bShow: false);
		}
	}
}
