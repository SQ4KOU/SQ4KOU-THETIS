using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using System.Windows.Forms;

namespace Thetis;

public sealed class clsDiscoveredRadioPicker
{
	private sealed class RowRef
	{
		public NicRadioScanResult Nic;

		public RadioInfo Radio;
	}

	public List<NicRadioScanResult> PickRadios(IWin32Window owner, List<NicRadioScanResult> discovered)
	{
		if (discovered == null || discovered.Count == 0)
		{
			return new List<NicRadioScanResult>();
		}
		List<NicRadioScanResult> picked = null;
		Form f = new Form();
		try
		{
			DataGridView grid = new DataGridView();
			try
			{
				Button btnAdd = new Button();
				try
				{
					Button btnCancel = new Button();
					try
					{
						Panel bottom = new Panel();
						try
						{
							f.Text = "Discovered radios";
							f.FormBorderStyle = FormBorderStyle.FixedDialog;
							f.MaximizeBox = false;
							f.MinimizeBox = false;
							f.ShowInTaskbar = false;
							f.StartPosition = ((owner == null) ? FormStartPosition.CenterScreen : FormStartPosition.CenterParent);
							f.ClientSize = new Size(720, 330);
							grid.Dock = DockStyle.Fill;
							grid.AllowUserToAddRows = false;
							grid.AllowUserToDeleteRows = false;
							grid.AllowUserToResizeRows = false;
							grid.AllowUserToResizeColumns = false;
							grid.MultiSelect = false;
							grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
							grid.RowHeadersVisible = false;
							grid.AutoGenerateColumns = false;
							grid.ScrollBars = ScrollBars.Vertical;
							grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
							grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
							grid.EnableHeadersVisualStyles = false;
							grid.ColumnHeadersDefaultCellStyle.BackColor = SystemColors.Control;
							grid.ColumnHeadersDefaultCellStyle.ForeColor = SystemColors.ControlText;
							grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = SystemColors.Control;
							grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = SystemColors.ControlText;
							DataGridViewCheckBoxColumn dataGridViewCheckBoxColumn = new DataGridViewCheckBoxColumn();
							dataGridViewCheckBoxColumn.Name = "Pick";
							dataGridViewCheckBoxColumn.HeaderText = "";
							dataGridViewCheckBoxColumn.Width = 42;
							dataGridViewCheckBoxColumn.FillWeight = 10f;
							dataGridViewCheckBoxColumn.FalseValue = false;
							dataGridViewCheckBoxColumn.TrueValue = true;
							dataGridViewCheckBoxColumn.IndeterminateValue = false;
							dataGridViewCheckBoxColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
							grid.Columns.Add(dataGridViewCheckBoxColumn);
							DataGridViewTextBoxColumn dataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
							dataGridViewTextBoxColumn.Name = "Hardware";
							dataGridViewTextBoxColumn.HeaderText = "Hardware";
							dataGridViewTextBoxColumn.ReadOnly = true;
							dataGridViewTextBoxColumn.FillWeight = 15f;
							dataGridViewTextBoxColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
							grid.Columns.Add(dataGridViewTextBoxColumn);
							DataGridViewTextBoxColumn dataGridViewTextBoxColumn2 = new DataGridViewTextBoxColumn();
							dataGridViewTextBoxColumn2.Name = "IP";
							dataGridViewTextBoxColumn2.HeaderText = "IP";
							dataGridViewTextBoxColumn2.ReadOnly = true;
							dataGridViewTextBoxColumn2.FillWeight = 15f;
							dataGridViewTextBoxColumn2.SortMode = DataGridViewColumnSortMode.NotSortable;
							grid.Columns.Add(dataGridViewTextBoxColumn2);
							DataGridViewTextBoxColumn dataGridViewTextBoxColumn3 = new DataGridViewTextBoxColumn();
							dataGridViewTextBoxColumn3.Name = "Port";
							dataGridViewTextBoxColumn3.HeaderText = "Base Port";
							dataGridViewTextBoxColumn3.ReadOnly = true;
							dataGridViewTextBoxColumn3.FillWeight = 15f;
							dataGridViewTextBoxColumn3.SortMode = DataGridViewColumnSortMode.NotSortable;
							grid.Columns.Add(dataGridViewTextBoxColumn3);
							DataGridViewTextBoxColumn dataGridViewTextBoxColumn4 = new DataGridViewTextBoxColumn();
							dataGridViewTextBoxColumn4.Name = "Mac";
							dataGridViewTextBoxColumn4.HeaderText = "Mac Address";
							dataGridViewTextBoxColumn4.ReadOnly = true;
							dataGridViewTextBoxColumn4.FillWeight = 20f;
							dataGridViewTextBoxColumn4.SortMode = DataGridViewColumnSortMode.NotSortable;
							grid.Columns.Add(dataGridViewTextBoxColumn4);
							DataGridViewTextBoxColumn dataGridViewTextBoxColumn5 = new DataGridViewTextBoxColumn();
							dataGridViewTextBoxColumn5.Name = "Protocol";
							dataGridViewTextBoxColumn5.HeaderText = "Protocol";
							dataGridViewTextBoxColumn5.ReadOnly = true;
							dataGridViewTextBoxColumn5.FillWeight = 10f;
							dataGridViewTextBoxColumn5.SortMode = DataGridViewColumnSortMode.NotSortable;
							grid.Columns.Add(dataGridViewTextBoxColumn5);
							DataGridViewTextBoxColumn dataGridViewTextBoxColumn6 = new DataGridViewTextBoxColumn();
							dataGridViewTextBoxColumn6.Name = "Version";
							dataGridViewTextBoxColumn6.HeaderText = "Version";
							dataGridViewTextBoxColumn6.ReadOnly = true;
							dataGridViewTextBoxColumn6.FillWeight = 15f;
							dataGridViewTextBoxColumn6.SortMode = DataGridViewColumnSortMode.NotSortable;
							grid.Columns.Add(dataGridViewTextBoxColumn6);
							bottom.Dock = DockStyle.Bottom;
							bottom.Height = 42;
							btnAdd.Text = "Add";
							btnAdd.Width = 90;
							btnAdd.Height = 26;
							btnAdd.Anchor = AnchorStyles.Top | AnchorStyles.Right;
							btnAdd.Enabled = false;
							btnCancel.Text = "Cancel";
							btnCancel.Width = 90;
							btnCancel.Height = 26;
							btnCancel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
							btnCancel.DialogResult = DialogResult.Cancel;
							bottom.Controls.Add(btnAdd);
							bottom.Controls.Add(btnCancel);
							f.Controls.Add(grid);
							f.Controls.Add(bottom);
							f.AcceptButton = btnAdd;
							f.CancelButton = btnCancel;
							btnCancel.Location = new Point(bottom.Width - btnCancel.Width - 12, 8);
							btnAdd.Location = new Point(btnCancel.Left - btnAdd.Width - 8, 8);
							bottom.Resize += delegate
							{
								btnCancel.Location = new Point(bottom.Width - btnCancel.Width - 12, 8);
								btnAdd.Location = new Point(btnCancel.Left - btnAdd.Width - 8, 8);
							};
							Action updateAddEnabled = delegate
							{
								bool enabled = false;
								for (int i = 0; i < grid.Rows.Count; i++)
								{
									DataGridViewRow dataGridViewRow = grid.Rows[i];
									if (dataGridViewRow.Tag is RowRef && dataGridViewRow.Cells[0].Value is int num4 && num4 != 0)
									{
										enabled = true;
										break;
									}
								}
								btnAdd.Enabled = enabled;
							};
							Func<DataGridViewRow, bool> isHeaderRow = (DataGridViewRow row) => row != null && row.Tag is NicRadioScanResult;
							Func<NicRadioScanResult, string> getHeaderText = delegate(NicRadioScanResult nic)
							{
								string text3 = ((nic != null) ? (nic.NicDescription ?? "") : "");
								string text4 = ((nic != null && nic.LocalIPv4 != null) ? nic.LocalIPv4.ToString() : "");
								string text5 = ((nic != null && nic.LocalMaskIPv4 != null) ? nic.LocalMaskIPv4.ToString() : "");
								string text6 = ((nic == null) ? "" : (nic.IsEthernet ? "Ethernet" : (nic.IsWireless ? "WiFi" : nic.NicInterfaceTypeString)));
								text6 = nic.NicInterfaceTypeString;
								string text7 = ((nic != null && nic.IsApipaLocal) ? " APIPA" : "");
								if (text4.Length == 0)
								{
									return "NIC: " + text3 + " [" + text6 + text7 + "]";
								}
								return (text5.Length == 0) ? ("NIC: " + text3 + " [" + text6 + text7 + "]  " + text4) : ("NIC: " + text3 + " [" + text6 + text7 + "]  " + text4 + " / " + text5);
							};
							Action<DataGridViewRow, NicRadioScanResult> action = delegate(DataGridViewRow row, NicRadioScanResult nic)
							{
								row.Tag = nic;
								row.ReadOnly = true;
								row.Height = 24;
								row.DefaultCellStyle.BackColor = SystemColors.ControlLight;
								row.DefaultCellStyle.ForeColor = SystemColors.ControlText;
								row.DefaultCellStyle.Font = new Font(grid.Font, FontStyle.Bold);
								row.DefaultCellStyle.SelectionBackColor = row.DefaultCellStyle.BackColor;
								row.DefaultCellStyle.SelectionForeColor = row.DefaultCellStyle.ForeColor;
								DataGridViewCell dataGridViewCell = row.Cells[0];
								DataGridViewTextBoxCell value = new DataGridViewTextBoxCell
								{
									Style = dataGridViewCell.Style
								};
								row.Cells[0] = value;
								row.Cells[0].Value = "";
								row.Cells[1].Value = "";
								row.Cells[2].Value = "";
								row.Cells[3].Value = "";
								row.Cells[4].Value = "";
								row.Cells[5].Value = "";
								row.Cells[6].Value = "";
								row.Cells[0].Tag = getHeaderText(nic);
							};
							for (int num = 0; num < discovered.Count; num++)
							{
								NicRadioScanResult nicRadioScanResult = discovered[num];
								if (nicRadioScanResult == null || nicRadioScanResult.Radios == null || nicRadioScanResult.Radios.Count == 0)
								{
									continue;
								}
								int index = grid.Rows.Add();
								DataGridViewRow arg = grid.Rows[index];
								action(arg, nicRadioScanResult);
								for (int num2 = 0; num2 < nicRadioScanResult.Radios.Count; num2++)
								{
									RadioInfo radioInfo = nicRadioScanResult.Radios[num2];
									if (radioInfo == null || radioInfo.IpAddress == null)
									{
										continue;
									}
									int num3 = radioInfo.DiscoveryPortBase;
									if (num3 < 1)
									{
										num3 = 1024;
									}
									string text;
									if (radioInfo.DeviceType == HPSDRHW.Saturn)
									{
										text = "fpga=" + radioInfo.CodeVersion;
										if (radioInfo.BetaVersion >= 39)
										{
											text = text + " p2app=" + radioInfo.BetaVersion;
										}
									}
									else
									{
										text = ((float)(int)radioInfo.CodeVersion / 10f).ToString("F1");
										if (radioInfo.Protocol == RadioDiscoveryRadioProtocol.P2 && radioInfo.BetaVersion > 0)
										{
											text = text + "." + radioInfo.BetaVersion;
										}
									}
									string text2 = "?";
									if (radioInfo.Protocol == RadioDiscoveryRadioProtocol.P1)
									{
										text2 = "1";
									}
									else if (radioInfo.Protocol == RadioDiscoveryRadioProtocol.P2)
									{
										text2 = "2";
									}
									int index2 = grid.Rows.Add(false, radioInfo.DeviceType.ToString(), radioInfo.IpAddress.ToString(), num3.ToString(), radioInfo.IsCustom ? "Custom" : radioInfo.MacAddress, text2, text);
									grid.Rows[index2].Tag = new RowRef
									{
										Nic = nicRadioScanResult,
										Radio = radioInfo
									};
								}
							}
							if (grid.Rows.Count == 0)
							{
								return new List<NicRadioScanResult>();
							}
							grid.CellPainting += delegate(object s, DataGridViewCellPaintingEventArgs e)
							{
								if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
								{
									DataGridViewRow dataGridViewRow = grid.Rows[e.RowIndex];
									if (isHeaderRow(dataGridViewRow))
									{
										e.Handled = true;
										if (e.ColumnIndex == 0)
										{
											int left = grid.GetCellDisplayRectangle(0, e.RowIndex, cutOverflow: true).Left;
											int top = e.CellBounds.Top;
											int height = e.CellBounds.Height;
											int num4 = 0;
											for (int i = 0; i <= 6; i++)
											{
												num4 += grid.GetCellDisplayRectangle(i, e.RowIndex, cutOverflow: true).Width;
											}
											Rectangle rect = new Rectangle(left, top, num4, height);
											using (SolidBrush brush = new SolidBrush(dataGridViewRow.DefaultCellStyle.BackColor))
											{
												e.Graphics.FillRectangle(brush, rect);
											}
											using (Pen pen = new Pen(grid.GridColor))
											{
												e.Graphics.DrawRectangle(pen, new Rectangle(rect.Left, rect.Top, rect.Width - 1, rect.Height - 1));
											}
											string text3 = (dataGridViewRow.Cells[0].Tag as string) ?? "";
											TextRenderer.DrawText(bounds: new Rectangle(rect.Left + 6, rect.Top + 2, rect.Width - 12, rect.Height - 4), dc: e.Graphics, text: text3, font: dataGridViewRow.DefaultCellStyle.Font ?? grid.Font, foreColor: dataGridViewRow.DefaultCellStyle.ForeColor, flags: TextFormatFlags.EndEllipsis | TextFormatFlags.VerticalCenter);
										}
									}
								}
							};
							bool suppressSelectionChanged = false;
							grid.SelectionChanged += delegate
							{
								if (!suppressSelectionChanged)
								{
									DataGridViewRow currentRow = grid.CurrentRow;
									if (currentRow != null && isHeaderRow(currentRow))
									{
										int index3 = currentRow.Index;
										int i;
										for (i = index3 + 1; i < grid.Rows.Count && isHeaderRow(grid.Rows[i]); i++)
										{
										}
										if (i < grid.Rows.Count)
										{
											suppressSelectionChanged = true;
											try
											{
												grid.CurrentCell = grid.Rows[i].Cells[1];
												return;
											}
											finally
											{
												suppressSelectionChanged = false;
											}
										}
										int num4 = index3 - 1;
										while (num4 >= 0 && isHeaderRow(grid.Rows[num4]))
										{
											num4--;
										}
										if (num4 >= 0)
										{
											suppressSelectionChanged = true;
											try
											{
												grid.CurrentCell = grid.Rows[num4].Cells[1];
											}
											finally
											{
												suppressSelectionChanged = false;
											}
										}
									}
								}
							};
							grid.CurrentCellDirtyStateChanged += delegate
							{
								if (grid.IsCurrentCellDirty)
								{
									grid.CommitEdit(DataGridViewDataErrorContexts.Commit);
								}
							};
							grid.CellValueChanged += delegate(object s, DataGridViewCellEventArgs e)
							{
								if (e.RowIndex >= 0 && e.ColumnIndex == 0)
								{
									updateAddEnabled();
								}
							};
							grid.CellDoubleClick += delegate(object s, DataGridViewCellEventArgs e)
							{
								if (e.RowIndex >= 0)
								{
									DataGridViewRow dataGridViewRow = grid.Rows[e.RowIndex];
									if (dataGridViewRow.Tag is RowRef)
									{
										object value = dataGridViewRow.Cells[0].Value;
										bool flag = value is bool && (bool)value;
										dataGridViewRow.Cells[0].Value = !flag;
										updateAddEnabled();
									}
								}
							};
							btnAdd.Click += delegate
							{
								Dictionary<string, NicRadioScanResult> dictionary = new Dictionary<string, NicRadioScanResult>(StringComparer.OrdinalIgnoreCase);
								for (int i = 0; i < grid.Rows.Count; i++)
								{
									DataGridViewRow dataGridViewRow = grid.Rows[i];
									if (dataGridViewRow.Tag is RowRef { Nic: not null, Radio: not null } rowRef && dataGridViewRow.Cells[0].Value is int num4 && num4 != 0)
									{
										string key = buildNicKey(rowRef.Nic);
										if (!dictionary.TryGetValue(key, out var value))
										{
											value = (dictionary[key] = cloneNicWithoutRadios(rowRef.Nic));
										}
										value.Radios.Add(rowRef.Radio);
									}
								}
								picked = new List<NicRadioScanResult>(dictionary.Values);
								f.DialogResult = DialogResult.OK;
								f.Close();
							};
							updateAddEnabled();
							if (((owner != null) ? f.ShowDialog(owner) : f.ShowDialog()) != DialogResult.OK)
							{
								return null;
							}
							if (picked == null)
							{
								return new List<NicRadioScanResult>();
							}
							return picked;
						}
						finally
						{
							if (bottom != null)
							{
								((IDisposable)bottom).Dispose();
							}
						}
					}
					finally
					{
						if (btnCancel != null)
						{
							((IDisposable)btnCancel).Dispose();
						}
					}
				}
				finally
				{
					if (btnAdd != null)
					{
						((IDisposable)btnAdd).Dispose();
					}
				}
			}
			finally
			{
				if (grid != null)
				{
					((IDisposable)grid).Dispose();
				}
			}
		}
		finally
		{
			if (f != null)
			{
				((IDisposable)f).Dispose();
			}
		}
	}

	private string buildNicKey(NicRadioScanResult nic)
	{
		string obj = nic.NicId ?? "";
		string text = ((nic.LocalIPv4 != null) ? nic.LocalIPv4.ToString() : "");
		return obj + "|" + text;
	}

	private NicRadioScanResult cloneNicWithoutRadios(NicRadioScanResult src)
	{
		NicRadioScanResult nicRadioScanResult = new NicRadioScanResult();
		nicRadioScanResult.NicId = src.NicId;
		nicRadioScanResult.NicName = src.NicName;
		nicRadioScanResult.NicDescription = src.NicDescription;
		nicRadioScanResult.NicSpeedBitsPerSecond = src.NicSpeedBitsPerSecond;
		nicRadioScanResult.NicInterfaceType = src.NicInterfaceType;
		nicRadioScanResult.IsEthernet = src.IsEthernet;
		nicRadioScanResult.IsWireless = src.IsWireless;
		nicRadioScanResult.IsApipaLocal = src.IsApipaLocal;
		nicRadioScanResult.LocalIPv4 = src.LocalIPv4;
		nicRadioScanResult.LocalMaskIPv4 = src.LocalMaskIPv4;
		nicRadioScanResult.NicMacAddress = src.NicMacAddress;
		nicRadioScanResult.IsApipaLocal = src.IsApipaLocal;
		nicRadioScanResult.IsLoopbackLocal = src.IsLoopbackLocal;
		nicRadioScanResult.GatewayIPv4 = src.GatewayIPv4;
		if (src.DnsServersIPv4 != null)
		{
			nicRadioScanResult.DnsServersIPv4 = new List<IPAddress>(src.DnsServersIPv4);
		}
		else
		{
			nicRadioScanResult.DnsServersIPv4 = new List<IPAddress>();
		}
		nicRadioScanResult.IsDhcpEnabled = src.IsDhcpEnabled;
		nicRadioScanResult.NicStatus = src.NicStatus;
		nicRadioScanResult.Mtu = src.Mtu;
		nicRadioScanResult.Diagnostics = src.Diagnostics;
		nicRadioScanResult.Radios = new List<RadioInfo>();
		return nicRadioScanResult;
	}
}
