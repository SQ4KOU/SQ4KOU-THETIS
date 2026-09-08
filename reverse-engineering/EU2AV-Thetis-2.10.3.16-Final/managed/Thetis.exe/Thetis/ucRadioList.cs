using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Net;
using System.Net.NetworkInformation;
using System.Windows.Forms;
using Newtonsoft.Json;
using Thetis.Properties;

namespace Thetis;

public sealed class ucRadioList : UserControl
{
	private sealed class RowItem
	{
		public string Key;

		public string NicId;

		public string NicName;

		public string NicDescription;

		public string NicType;

		public string NicIp;

		public string NicMask;

		public NetworkInterfaceType NicInterfaceType;

		public bool NicIsEthernet;

		public bool NicIsWireless;

		public long NicSpeedBitsPerSecond;

		public string NicMacAddress;

		public bool NicIsApipaLocal;

		public bool NicIsLoopbackLocal;

		public bool NicIsDhcpEnabled;

		public OperationalStatus NicStatus;

		public int NicMtu;

		public string RadioModel;

		public string RadioIp;

		public int RadioPort;

		public RadioDiscoveryRadioProtocol RadioProtocol;

		public string RadioVersionText;

		public string RadioMac;

		public bool RadioIsCustom;

		public string RadioGuid;

		public byte RadioCodeVersion;

		public byte RadioBetaVersion;

		public byte RadioProtocol2Supported;

		public byte RadioNumRxs;

		public byte RadioMercuryVersion0;

		public byte RadioMercuryVersion1;

		public byte RadioMercuryVersion2;

		public byte RadioMercuryVersion3;

		public byte RadioPennyVersion;

		public byte RadioMetisVersion;

		public byte RadioHwRev;

		public HPSDRHW RadioDeviceType = HPSDRHW.Unknown;

		public bool RadioIsBusy;

		public int RadioDiscoveryPortBase;

		public int RadioPortCount;

		public bool RadioIsApipaRadio;

		public bool IsConnected;

		public bool PllLocked;
	}

	private sealed class PersistModel
	{
		public int Version;

		public string SelectedKey;

		public List<PersistRow> Items;
	}

	private sealed class PersistRow
	{
		public string Key;

		public string NicId;

		public string NicName;

		public string NicDescription;

		public string NicType;

		public string NicIp;

		public string NicMask;

		public string RadioModel;

		public string RadioIp;

		public int RadioPort;

		public string RadioProtocolEnum;

		public string RadioVersionText;

		public string RadioMac;

		public bool RadioIsCustom;

		public string RadioGuid;

		public byte RadioProtocol2Supported;

		public byte RadioHwRev;

		public string RadioDeviceType;
	}

	private sealed class HitTestResult
	{
		public int RowIndex;

		public bool IsTrash;
	}

	private static readonly NumberFormatInfo _nfi = NumberFormatInfo.InvariantInfo;

	private const string _auto_key = "auto:first_radio";

	private const string _auto_text = "Use the first radio found using the settings above";

	private readonly List<RowItem> _items;

	private readonly VScrollBar _scroll;

	private string _selected_key;

	private int _hover_index;

	private bool _hover_trash;

	private int _trash_down_index;

	private bool _trash_down;

	private bool _init_done;

	private IContainer components;

	public int Count => _items.Count;

	public string SelectedKey => _selected_key;

	public bool IsFirstRadioFoundSelected => isAutoKey(_selected_key);

	public string SelectedRadioIp => getSelectedItem()?.RadioIp;

	public int SelectedRadioPort => getSelectedItem()?.RadioPort ?? 0;

	public string SelectedRadioMac => getSelectedItem()?.RadioMac;

	public RadioDiscoveryRadioProtocol SelectedRadioProtocol => getSelectedItem()?.RadioProtocol ?? RadioDiscoveryRadioProtocol.P1;

	public string SelectedNicIp => getSelectedItem()?.NicIp;

	public string SelectedNicMask => getSelectedItem()?.NicMask;

	public NicRadioScanResult SelectedNICDetails
	{
		get
		{
			RowItem selectedItem = getSelectedItem();
			if (selectedItem == null)
			{
				return null;
			}
			RadioInfo selectedRadioDetails = SelectedRadioDetails;
			bool num = !string.IsNullOrWhiteSpace(selectedItem.NicId) || !string.IsNullOrWhiteSpace(selectedItem.NicName) || !string.IsNullOrWhiteSpace(selectedItem.NicDescription) || !string.IsNullOrWhiteSpace(selectedItem.NicIp) || !string.IsNullOrWhiteSpace(selectedItem.NicMask) || !string.IsNullOrWhiteSpace(selectedItem.NicMacAddress);
			bool flag = selectedRadioDetails != null;
			if (!num && !flag)
			{
				return null;
			}
			NicRadioScanResult nicRadioScanResult = new NicRadioScanResult();
			nicRadioScanResult.NicId = safe(selectedItem.NicId);
			nicRadioScanResult.NicName = safe(selectedItem.NicName);
			nicRadioScanResult.NicDescription = safe(selectedItem.NicDescription);
			nicRadioScanResult.NicSpeedBitsPerSecond = selectedItem.NicSpeedBitsPerSecond;
			nicRadioScanResult.NicInterfaceType = selectedItem.NicInterfaceType;
			nicRadioScanResult.IsEthernet = selectedItem.NicIsEthernet;
			nicRadioScanResult.IsWireless = selectedItem.NicIsWireless;
			IPAddress address = null;
			IPAddress address2 = null;
			if (!string.IsNullOrWhiteSpace(selectedItem.NicIp))
			{
				IPAddress.TryParse(selectedItem.NicIp, out address);
			}
			if (!string.IsNullOrWhiteSpace(selectedItem.NicMask))
			{
				IPAddress.TryParse(selectedItem.NicMask, out address2);
			}
			nicRadioScanResult.LocalIPv4 = address;
			nicRadioScanResult.LocalMaskIPv4 = address2;
			nicRadioScanResult.NicMacAddress = safe(selectedItem.NicMacAddress);
			nicRadioScanResult.IsApipaLocal = selectedItem.NicIsApipaLocal;
			nicRadioScanResult.IsLoopbackLocal = selectedItem.NicIsLoopbackLocal;
			nicRadioScanResult.IsDhcpEnabled = selectedItem.NicIsDhcpEnabled;
			nicRadioScanResult.NicStatus = selectedItem.NicStatus;
			nicRadioScanResult.Mtu = selectedItem.NicMtu;
			if (selectedRadioDetails != null)
			{
				nicRadioScanResult.Radios.Add(selectedRadioDetails);
			}
			return nicRadioScanResult;
		}
	}

	public RadioInfo SelectedRadioDetails
	{
		get
		{
			RowItem selectedItem = getSelectedItem();
			if (selectedItem == null)
			{
				return null;
			}
			if (string.IsNullOrWhiteSpace(selectedItem.RadioIp) && string.IsNullOrWhiteSpace(selectedItem.RadioMac) && string.IsNullOrWhiteSpace(selectedItem.RadioModel))
			{
				return null;
			}
			IPAddress address = null;
			if (!string.IsNullOrWhiteSpace(selectedItem.RadioIp))
			{
				IPAddress.TryParse(selectedItem.RadioIp, out address);
			}
			RadioInfo radioInfo = new RadioInfo();
			radioInfo.Protocol = selectedItem.RadioProtocol;
			radioInfo.IpAddress = address;
			radioInfo.IsCustom = selectedItem.RadioIsCustom;
			radioInfo.CustomGuid = safe(selectedItem.RadioGuid);
			radioInfo.MacAddress = (selectedItem.RadioIsCustom ? "" : safe(selectedItem.RadioMac));
			HPSDRHW hPSDRHW = selectedItem.RadioDeviceType;
			string value = safe(selectedItem.RadioModel);
			if (hPSDRHW == HPSDRHW.Unknown)
			{
				hPSDRHW = HPSDRHW.Atlas;
				if (!string.IsNullOrWhiteSpace(value))
				{
					try
					{
						hPSDRHW = (HPSDRHW)Enum.Parse(typeof(HPSDRHW), value, ignoreCase: true);
					}
					catch
					{
						hPSDRHW = HPSDRHW.Atlas;
					}
				}
			}
			radioInfo.DeviceType = hPSDRHW;
			radioInfo.HwRev = selectedItem.RadioHwRev;
			radioInfo.CodeVersion = selectedItem.RadioCodeVersion;
			radioInfo.BetaVersion = selectedItem.RadioBetaVersion;
			radioInfo.Protocol2Supported = selectedItem.RadioProtocol2Supported;
			radioInfo.NumRxs = selectedItem.RadioNumRxs;
			radioInfo.MercuryVersion0 = selectedItem.RadioMercuryVersion0;
			radioInfo.MercuryVersion1 = selectedItem.RadioMercuryVersion1;
			radioInfo.MercuryVersion2 = selectedItem.RadioMercuryVersion2;
			radioInfo.MercuryVersion3 = selectedItem.RadioMercuryVersion3;
			radioInfo.PennyVersion = selectedItem.RadioPennyVersion;
			radioInfo.MetisVersion = selectedItem.RadioMetisVersion;
			radioInfo.IsBusy = selectedItem.RadioIsBusy;
			radioInfo.DiscoveryPortBase = ((selectedItem.RadioDiscoveryPortBase > 0) ? selectedItem.RadioDiscoveryPortBase : selectedItem.RadioPort);
			radioInfo.PortCount = selectedItem.RadioPortCount;
			radioInfo.IsApipaRadio = selectedItem.RadioIsApipaRadio;
			return radioInfo;
		}
	}

	public event EventHandler SelectedRadioChanged;

	public event EventHandler RadioListChanged;

	public ucRadioList()
	{
		_init_done = false;
		InitializeComponent();
		Font = new Font("Consolas", 9f, FontStyle.Regular, GraphicsUnit.Point);
		_items = new List<RowItem>();
		_scroll = new VScrollBar();
		_hover_index = -1;
		_trash_down_index = -1;
		_trash_down = false;
		SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
		SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
		SetStyle(ControlStyles.ResizeRedraw, value: true);
		SetStyle(ControlStyles.UserPaint, value: true);
		SetStyle(ControlStyles.Selectable, value: true);
		base.TabStop = true;
		_scroll.Dock = DockStyle.Right;
		_scroll.ValueChanged += scroll_ValueChanged;
		base.Controls.Add(_scroll);
		BackColor = SystemColors.Window;
		ForeColor = SystemColors.WindowText;
		base.SizeChanged += ucRadioList_SizeChanged;
		_init_done = true;
		ensureAutoEntry(selectAutoIfMissingSelection: true);
		updateScroll();
		clampScrollValue();
		Invalidate();
	}

	public bool DoesRadioExist(string radioKey)
	{
		if (string.IsNullOrWhiteSpace(radioKey))
		{
			return false;
		}
		for (int i = 0; i < _items.Count; i++)
		{
			if (string.Equals(_items[i].Key, radioKey, StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
		}
		return false;
	}

	public void RadioConnected(string radioKey)
	{
		int num = indexOfKey(radioKey);
		if (num < 0)
		{
			return;
		}
		bool flag = false;
		for (int i = 0; i < _items.Count; i++)
		{
			bool flag2 = i == num;
			if (_items[i].IsConnected != flag2)
			{
				_items[i].IsConnected = flag2;
				flag = true;
			}
		}
		if (flag)
		{
			Invalidate();
		}
	}

	public void RadioDisconnected(string radioKey)
	{
		int num = indexOfKey(radioKey);
		if (num >= 0)
		{
			bool flag = false;
			if (isAutoKey(radioKey))
			{
				resetAutoItem(_items[num]);
				flag = true;
			}
			if (_items[num].IsConnected)
			{
				_items[num].IsConnected = false;
				flag = true;
			}
			if (flag)
			{
				updateScroll();
				clampScrollValue();
				Invalidate();
			}
		}
	}

	public void RadioConnected()
	{
		if (!string.IsNullOrWhiteSpace(_selected_key))
		{
			RadioConnected(_selected_key);
		}
	}

	public void RadioDisconnected()
	{
		if (!string.IsNullOrWhiteSpace(_selected_key))
		{
			RadioDisconnected(_selected_key);
		}
	}

	public void DisconnectAll()
	{
		bool flag = false;
		bool flag2 = true;
		for (int i = 0; i < _items.Count; i++)
		{
			if (flag2 && isAutoKey(_items[i].Key))
			{
				flag2 = false;
				resetAutoItem(_items[i]);
				flag = true;
			}
			if (_items[i].IsConnected)
			{
				_items[i].IsConnected = false;
				flag = true;
			}
		}
		if (flag)
		{
			updateScroll();
			clampScrollValue();
			Invalidate();
		}
	}

	public void PLLLocked(string radioKey, bool locked)
	{
		int num = indexOfKey(radioKey);
		if (num < 0)
		{
			return;
		}
		RowItem rowItem = _items[num];
		if (rowItem.RadioProtocol != RadioDiscoveryRadioProtocol.P2)
		{
			if (rowItem.PllLocked)
			{
				rowItem.PllLocked = false;
				Invalidate();
			}
		}
		else if (rowItem.PllLocked != locked)
		{
			rowItem.PllLocked = locked;
			Invalidate();
		}
	}

	public void PLLLocked(bool locked)
	{
		if (!string.IsNullOrWhiteSpace(_selected_key))
		{
			PLLLocked(_selected_key, locked);
		}
	}

	public void UpdateSelectedDetails(NicRadioScanResult nic, RadioInfo radio)
	{
		if (nic != null && radio != null)
		{
			RowItem selectedItem = getSelectedItem();
			if (selectedItem != null)
			{
				fillItemFromInfo(selectedItem, nic, radio);
				updateScroll();
				clampScrollValue();
				Invalidate();
				raiseSelectedChanged();
				raiseListChanged();
			}
		}
	}

	public string AddRadio(NicRadioScanResult nic, RadioInfo radio)
	{
		if (nic == null)
		{
			return null;
		}
		if (radio == null)
		{
			return null;
		}
		IPAddress ipAddress = radio.IpAddress;
		IPAddress localIPv = nic.LocalIPv4;
		IPAddress localMaskIPv = nic.LocalMaskIPv4;
		int num = radio.DiscoveryPortBase;
		if (num < 1)
		{
			num = 1024;
		}
		string text = safe(radio.MacAddress);
		bool flag = radio.IsCustom || string.IsNullOrWhiteSpace(text);
		if (flag)
		{
			string b = ((ipAddress != null) ? ipAddress.ToString() : "");
			for (int i = 0; i < _items.Count; i++)
			{
				RowItem rowItem = _items[i];
				if (rowItem != null && !isAutoKey(rowItem.Key) && rowItem.RadioIsCustom && string.Equals(rowItem.RadioIp, b, StringComparison.OrdinalIgnoreCase) && rowItem.RadioPort == num && rowItem.RadioProtocol == radio.Protocol)
				{
					return rowItem.Key;
				}
			}
		}
		string text2 = "";
		if (flag)
		{
			text2 = (radio.CustomGuid = ensureGuidString(radio.CustomGuid));
			radio.IsCustom = true;
			text = "";
		}
		string text4 = buildKey(text, text2, flag, ipAddress, num, nic.NicId, radio.Protocol);
		if (DoesRadioExist(text4))
		{
			return text4;
		}
		string nicType = (nic.IsEthernet ? "Ethernet" : ((!nic.IsWireless) ? nic.NicInterfaceType.ToString() : "WiFi"));
		string radioVersionText = buildVersionText(radio);
		RowItem rowItem2 = new RowItem();
		rowItem2.Key = text4;
		rowItem2.NicId = safe(nic.NicId);
		rowItem2.NicName = safe(nic.NicName);
		rowItem2.NicDescription = safe(nic.NicDescription);
		rowItem2.NicType = nicType;
		rowItem2.NicIp = ((localIPv != null) ? localIPv.ToString() : "");
		rowItem2.NicMask = ((localMaskIPv != null) ? localMaskIPv.ToString() : "");
		rowItem2.NicInterfaceType = nic.NicInterfaceType;
		rowItem2.NicIsEthernet = nic.IsEthernet;
		rowItem2.NicIsWireless = nic.IsWireless;
		rowItem2.NicSpeedBitsPerSecond = nic.NicSpeedBitsPerSecond;
		rowItem2.NicMacAddress = safe(nic.NicMacAddress);
		rowItem2.NicIsApipaLocal = nic.IsApipaLocal;
		rowItem2.NicIsLoopbackLocal = nic.IsLoopbackLocal;
		rowItem2.NicIsDhcpEnabled = nic.IsDhcpEnabled;
		rowItem2.NicStatus = nic.NicStatus;
		rowItem2.NicMtu = nic.Mtu;
		rowItem2.RadioModel = (((radio.HwRev & 0x10) != 0) ? "Anvelina PRO3 (24bit)" : radio.DeviceType.ToString());
		rowItem2.RadioDeviceType = radio.DeviceType;
		rowItem2.RadioIp = ((ipAddress != null) ? ipAddress.ToString() : "");
		rowItem2.RadioPort = num;
		rowItem2.RadioProtocol = radio.Protocol;
		rowItem2.RadioVersionText = radioVersionText;
		rowItem2.RadioMac = text;
		rowItem2.RadioIsCustom = flag;
		rowItem2.RadioGuid = (flag ? text2 : "");
		rowItem2.RadioCodeVersion = radio.CodeVersion;
		rowItem2.RadioBetaVersion = radio.BetaVersion;
		rowItem2.RadioProtocol2Supported = radio.Protocol2Supported;
		rowItem2.RadioNumRxs = radio.NumRxs;
		rowItem2.RadioMercuryVersion0 = radio.MercuryVersion0;
		rowItem2.RadioMercuryVersion1 = radio.MercuryVersion1;
		rowItem2.RadioMercuryVersion2 = radio.MercuryVersion2;
		rowItem2.RadioMercuryVersion3 = radio.MercuryVersion3;
		rowItem2.RadioPennyVersion = radio.PennyVersion;
		rowItem2.RadioMetisVersion = radio.MetisVersion;
		rowItem2.RadioHwRev = radio.HwRev;
		rowItem2.RadioIsBusy = radio.IsBusy;
		rowItem2.RadioDiscoveryPortBase = radio.DiscoveryPortBase;
		rowItem2.RadioPortCount = radio.PortCount;
		rowItem2.RadioIsApipaRadio = radio.IsApipaRadio;
		rowItem2.IsConnected = false;
		rowItem2.PllLocked = false;
		ensureAutoEntry(selectAutoIfMissingSelection: false);
		_items.Add(rowItem2);
		updateScroll();
		Invalidate();
		raiseListChanged();
		return text4;
	}

	public bool RemoveRadio(string radioKey)
	{
		_trash_down = false;
		_trash_down_index = -1;
		if (string.IsNullOrWhiteSpace(radioKey))
		{
			return false;
		}
		if (isAutoKey(radioKey))
		{
			return false;
		}
		int num = indexOfKey(radioKey);
		if (num < 0)
		{
			return false;
		}
		if (num == 0)
		{
			return false;
		}
		bool num2 = string.Equals(_selected_key, _items[num].Key, StringComparison.OrdinalIgnoreCase);
		_items.RemoveAt(num);
		if (num2)
		{
			_selected_key = "auto:first_radio";
			raiseSelectedChanged();
		}
		ensureAutoEntry(selectAutoIfMissingSelection: true);
		updateScroll();
		clampScrollValue();
		Invalidate();
		raiseListChanged();
		return true;
	}

	public void ClearRadios()
	{
		_hover_index = -1;
		_hover_trash = false;
		_trash_down = false;
		_trash_down_index = -1;
		_items.Clear();
		_selected_key = null;
		ensureAutoEntry(selectAutoIfMissingSelection: true);
		updateScroll();
		setScrollValue(0);
		Invalidate();
		raiseSelectedChanged();
		raiseListChanged();
	}

	public bool MakeRadioVisible(string radioKey)
	{
		int num = indexOfKey(radioKey);
		if (num < 0)
		{
			return false;
		}
		Rectangle viewportRect = getViewportRect();
		int num2 = rowTopForIndex(num);
		int num3 = num2 + rowHeightForIndex(num);
		int value = _scroll.Value;
		int num4 = value + viewportRect.Height;
		if (num2 < value)
		{
			setScrollValue(num2);
			return true;
		}
		if (num3 > num4)
		{
			int scrollValue = num3 - viewportRect.Height;
			setScrollValue(scrollValue);
			return true;
		}
		return true;
	}

	public string SaveToJson()
	{
		PersistModel persistModel = new PersistModel();
		persistModel.Version = 5;
		persistModel.SelectedKey = (isAutoKey(_selected_key) ? "" : enc(_selected_key));
		persistModel.Items = new List<PersistRow>();
		for (int i = 0; i < _items.Count; i++)
		{
			RowItem rowItem = _items[i];
			if (!isAutoKey(rowItem.Key))
			{
				PersistRow persistRow = new PersistRow();
				persistRow.Key = enc(rowItem.Key);
				persistRow.NicId = enc(rowItem.NicId);
				persistRow.NicName = enc(rowItem.NicName);
				persistRow.NicDescription = enc(rowItem.NicDescription);
				persistRow.NicType = enc(rowItem.NicType);
				persistRow.NicIp = enc(rowItem.NicIp);
				persistRow.NicMask = enc(rowItem.NicMask);
				persistRow.RadioModel = enc(rowItem.RadioModel);
				persistRow.RadioIp = enc(rowItem.RadioIp);
				persistRow.RadioPort = rowItem.RadioPort;
				persistRow.RadioProtocolEnum = enc(rowItem.RadioProtocol.ToString());
				persistRow.RadioVersionText = enc(rowItem.RadioVersionText);
				persistRow.RadioMac = enc(rowItem.RadioMac);
				persistRow.RadioIsCustom = rowItem.RadioIsCustom;
				persistRow.RadioGuid = enc(rowItem.RadioGuid);
				persistRow.RadioProtocol2Supported = rowItem.RadioProtocol2Supported;
				persistRow.RadioHwRev = rowItem.RadioHwRev;
				persistRow.RadioDeviceType = enc(rowItem.RadioDeviceType.ToString());
				persistModel.Items.Add(persistRow);
			}
		}
		return JsonConvert.SerializeObject(persistModel, Formatting.Indented, new JsonSerializerSettings
		{
			Culture = CultureInfo.InvariantCulture
		});
	}

	public void LoadFromJson(string json)
	{
		ClearRadios();
		if (string.IsNullOrWhiteSpace(json))
		{
			return;
		}
		PersistModel persistModel = null;
		try
		{
			persistModel = JsonConvert.DeserializeObject<PersistModel>(json);
		}
		catch
		{
			return;
		}
		if (persistModel == null || persistModel.Items == null)
		{
			return;
		}
		string text = dec(persistModel.SelectedKey);
		for (int i = 0; i < persistModel.Items.Count; i++)
		{
			PersistRow persistRow = persistModel.Items[i];
			if (persistRow == null)
			{
				continue;
			}
			RowItem rowItem = new RowItem();
			rowItem.Key = dec(persistRow.Key);
			if (isAutoKey(rowItem.Key))
			{
				continue;
			}
			rowItem.NicId = dec(persistRow.NicId);
			rowItem.NicName = dec(persistRow.NicName);
			rowItem.NicDescription = dec(persistRow.NicDescription);
			rowItem.NicType = dec(persistRow.NicType);
			rowItem.NicIp = dec(persistRow.NicIp);
			rowItem.NicMask = dec(persistRow.NicMask);
			rowItem.RadioModel = dec(persistRow.RadioModel);
			rowItem.RadioIp = dec(persistRow.RadioIp);
			rowItem.RadioPort = persistRow.RadioPort;
			rowItem.RadioVersionText = dec(persistRow.RadioVersionText);
			rowItem.RadioMac = dec(persistRow.RadioMac);
			rowItem.RadioIsCustom = persistRow.RadioIsCustom;
			rowItem.RadioGuid = dec(persistRow.RadioGuid);
			RadioDiscoveryRadioProtocol radioDiscoveryRadioProtocol = RadioDiscoveryRadioProtocol.P1;
			string value = dec(persistRow.RadioProtocolEnum);
			if (!string.IsNullOrWhiteSpace(value))
			{
				try
				{
					radioDiscoveryRadioProtocol = (RadioDiscoveryRadioProtocol)Enum.Parse(typeof(RadioDiscoveryRadioProtocol), value, ignoreCase: true);
				}
				catch
				{
					radioDiscoveryRadioProtocol = RadioDiscoveryRadioProtocol.P1;
				}
			}
			rowItem.RadioProtocol = radioDiscoveryRadioProtocol;
			rowItem.RadioProtocol2Supported = persistRow.RadioProtocol2Supported;
			rowItem.RadioHwRev = persistRow.RadioHwRev;
			if (!string.IsNullOrWhiteSpace(persistRow.RadioDeviceType))
			{
				try
				{
					rowItem.RadioDeviceType = (HPSDRHW)Enum.Parse(typeof(HPSDRHW), dec(persistRow.RadioDeviceType), ignoreCase: true);
				}
				catch
				{
					rowItem.RadioDeviceType = HPSDRHW.Unknown;
				}
			}
			rowItem.IsConnected = false;
			rowItem.PllLocked = false;
			string key = rowItem.Key;
			IPAddress address = null;
			if (!string.IsNullOrWhiteSpace(rowItem.RadioIp))
			{
				IPAddress.TryParse(rowItem.RadioIp, out address);
			}
			if (rowItem.RadioIsCustom = rowItem.RadioIsCustom || string.IsNullOrWhiteSpace(rowItem.RadioMac))
			{
				rowItem.RadioGuid = ensureGuidString(rowItem.RadioGuid);
				rowItem.RadioMac = "";
			}
			else
			{
				rowItem.RadioGuid = "";
			}
			rowItem.Key = buildKey(rowItem.RadioMac, rowItem.RadioGuid, rowItem.RadioIsCustom, address, rowItem.RadioPort, rowItem.NicId, radioDiscoveryRadioProtocol);
			if (!string.IsNullOrWhiteSpace(text) && string.Equals(text, key, StringComparison.OrdinalIgnoreCase))
			{
				text = rowItem.Key;
			}
			if (!DoesRadioExist(rowItem.Key))
			{
				ensureAutoEntry(selectAutoIfMissingSelection: false);
				_items.Add(rowItem);
			}
		}
		_selected_key = "auto:first_radio";
		if (!string.IsNullOrWhiteSpace(text) && DoesRadioExist(text))
		{
			_selected_key = text;
		}
		ensureAutoEntry(selectAutoIfMissingSelection: true);
		updateScroll();
		clampScrollValue();
		Invalidate();
		raiseSelectedChanged();
		raiseListChanged();
	}

	protected override bool IsInputKey(Keys keyData)
	{
		Keys keys = keyData & Keys.KeyCode;
		if (keys == Keys.Up || keys == Keys.Down || keys == Keys.Delete)
		{
			return true;
		}
		return base.IsInputKey(keyData);
	}

	protected override void OnKeyDown(KeyEventArgs e)
	{
		base.OnKeyDown(e);
		if (_items.Count == 0)
		{
			return;
		}
		if (e.KeyCode == Keys.Up)
		{
			int num = selectedIndex();
			if (num > 0)
			{
				setSelectedByIndex(num - 1, ensureVisible: true);
			}
			e.Handled = true;
		}
		else if (e.KeyCode == Keys.Down)
		{
			int num2 = selectedIndex();
			if (num2 < 0)
			{
				num2 = 0;
			}
			if (num2 < _items.Count - 1)
			{
				setSelectedByIndex(num2 + 1, ensureVisible: true);
			}
			e.Handled = true;
		}
		else if (e.KeyCode == Keys.Delete)
		{
			int num3 = selectedIndex();
			if (num3 > 0 && num3 < _items.Count)
			{
				string key = _items[num3].Key;
				RemoveRadio(key);
			}
			e.Handled = true;
		}
	}

	protected override void OnMouseWheel(MouseEventArgs e)
	{
		base.OnMouseWheel(e);
		int num = ((e.Delta > 0) ? (-normalRowHeight()) : ((e.Delta < 0) ? normalRowHeight() : 0));
		if (num != 0)
		{
			setScrollValue(_scroll.Value + num);
		}
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		base.OnMouseMove(e);
		HitTestResult hitTestResult = hitTest(e.Location);
		int rowIndex = hitTestResult.RowIndex;
		bool isTrash = hitTestResult.IsTrash;
		if (rowIndex != _hover_index || isTrash != _hover_trash)
		{
			_hover_index = rowIndex;
			_hover_trash = isTrash;
			Invalidate();
		}
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		base.OnMouseLeave(e);
		if (_hover_index != -1 || _hover_trash)
		{
			_hover_index = -1;
			_hover_trash = false;
			Invalidate();
		}
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		base.OnMouseDown(e);
		Focus();
		if (e.Button != MouseButtons.Left)
		{
			return;
		}
		HitTestResult hitTestResult = hitTest(e.Location);
		if (hitTestResult.RowIndex >= 0)
		{
			if (hitTestResult.IsTrash)
			{
				_trash_down = true;
				_trash_down_index = hitTestResult.RowIndex;
				base.Capture = true;
				Invalidate();
			}
			else
			{
				setSelectedByIndex(hitTestResult.RowIndex, ensureVisible: false);
			}
		}
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		base.OnMouseUp(e);
		if (e.Button != MouseButtons.Left)
		{
			return;
		}
		bool trash_down = _trash_down;
		int trash_down_index = _trash_down_index;
		_trash_down = false;
		_trash_down_index = -1;
		if (base.Capture)
		{
			base.Capture = false;
		}
		if (!trash_down || trash_down_index < 0)
		{
			Invalidate();
			return;
		}
		HitTestResult hitTestResult = hitTest(e.Location);
		if (hitTestResult.IsTrash && hitTestResult.RowIndex == trash_down_index && trash_down_index < _items.Count)
		{
			string key = _items[trash_down_index].Key;
			RemoveRadio(key);
		}
		else
		{
			Invalidate();
		}
	}

	protected override void OnFontChanged(EventArgs e)
	{
		base.OnFontChanged(e);
		if (_init_done)
		{
			updateScroll();
			clampScrollValue();
			Invalidate();
		}
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		base.OnPaint(e);
		Graphics graphics = e.Graphics;
		graphics.SmoothingMode = SmoothingMode.AntiAlias;
		Rectangle viewportRect = getViewportRect();
		using (SolidBrush brush = new SolidBrush(BackColor))
		{
			graphics.FillRectangle(brush, viewportRect);
		}
		if (_items.Count == 0)
		{
			return;
		}
		int value = _scroll.Value;
		int num = indexFromContentY(value);
		if (num < 0)
		{
			return;
		}
		int num2 = rowTopForIndex(num);
		int num3 = -(value - num2);
		int num4 = viewportRect.Top + num3;
		for (int i = num; i < _items.Count; i++)
		{
			int num5 = rowHeightForIndex(i);
			Rectangle rowRect = new Rectangle(viewportRect.Left, num4, viewportRect.Width, num5);
			if (rowRect.Bottom >= viewportRect.Top && rowRect.Top <= viewportRect.Bottom)
			{
				bool selected = !string.IsNullOrWhiteSpace(_selected_key) && string.Equals(_selected_key, _items[i].Key, StringComparison.OrdinalIgnoreCase);
				bool flag = i == _hover_index;
				bool flag2 = !isAutoKey(_items[i].Key);
				bool hoverTrash = flag2 && ((flag && _hover_trash) || (_trash_down && _trash_down_index == i));
				bool compact = i == 0 && autoRowIsCompact(_items[i]);
				drawRow(graphics, rowRect, _items[i], selected, flag, hoverTrash, flag2, compact);
			}
			num4 += num5;
			if (num4 > viewportRect.Bottom)
			{
				break;
			}
		}
		using Pen pen = new Pen(Color.FromArgb(210, 210, 210));
		graphics.DrawRectangle(pen, new Rectangle(viewportRect.Left, viewportRect.Top, viewportRect.Width - 1, viewportRect.Height - 1));
	}

	private void drawRow(Graphics g, Rectangle rowRect, RowItem item, bool selected, bool hovered, bool hoverTrash, bool canRemove, bool compact)
	{
		Color backColor = BackColor;
		Color color = Color.FromArgb(240, 247, 255);
		Color color2 = Color.FromArgb(225, 240, 255);
		Color color3 = Color.FromArgb(235, 248, 235);
		Color color4 = Color.FromArgb(222, 246, 230);
		Color color5 = backColor;
		if (selected && item.IsConnected)
		{
			color5 = color4;
		}
		else if (selected)
		{
			color5 = color2;
		}
		else if (hovered)
		{
			color5 = color;
		}
		else if (item.IsConnected)
		{
			color5 = color3;
		}
		using (SolidBrush brush = new SolidBrush(color5))
		{
			g.FillRectangle(brush, rowRect);
		}
		using (Pen pen = new Pen(Color.FromArgb(225, 225, 225)))
		{
			g.DrawLine(pen, rowRect.Left, rowRect.Bottom - 1, rowRect.Right, rowRect.Bottom - 1);
		}
		int num = scale(10);
		int num2 = scale(16);
		int num3 = scale(28);
		Rectangle rect = new Rectangle(rowRect.Left + num, rowRect.Top + (rowRect.Height - num2) / 2, num2, num2);
		Rectangle rect2 = new Rectangle(rowRect.Right - num - num3, rowRect.Top + (rowRect.Height - num3) / 2, num3, num3);
		drawRadioGlyph(g, rect, selected);
		int num4 = rect.Right + scale(10);
		int num5;
		if (canRemove)
		{
			drawTrashGlyph(g, rect2, hoverTrash);
			num5 = rect2.Left - scale(10);
		}
		else
		{
			num5 = rowRect.Right - num;
		}
		if (num5 < num4)
		{
			return;
		}
		Rectangle rectangle = new Rectangle(num4, rowRect.Top + scale(6), num5 - num4, rowRect.Height - scale(12));
		string s = buildLine1(item);
		using (StringFormat stringFormat = new StringFormat())
		{
			stringFormat.Alignment = StringAlignment.Near;
			stringFormat.LineAlignment = StringAlignment.Center;
			stringFormat.Trimming = StringTrimming.EllipsisCharacter;
			stringFormat.FormatFlags = StringFormatFlags.NoWrap;
			if (compact)
			{
				using (Font font = new Font(Font, FontStyle.Bold))
				{
					using SolidBrush brush2 = new SolidBrush(ForeColor);
					g.DrawString(s, font, brush2, rectangle, stringFormat);
					return;
				}
			}
		}
		string s2 = buildLine2(item);
		string s3 = buildNicLine3(item);
		string s4 = buildNicLine4(item);
		int num6 = rectangle.Height;
		int num7 = (int)Math.Round((double)num6 * 0.28);
		int num8 = (int)Math.Round((double)num6 * 0.22);
		int num9 = (int)Math.Round((double)num6 * 0.25);
		int num10 = num6 - num7 - num8 - num9;
		if (num7 < 1)
		{
			num7 = 1;
		}
		if (num8 < 1)
		{
			num8 = 1;
		}
		if (num9 < 1)
		{
			num9 = 1;
		}
		if (num10 < 1)
		{
			num10 = 1;
		}
		Rectangle rectangle2 = new Rectangle(rectangle.Left, rectangle.Top, rectangle.Width, num7);
		Rectangle rectangle3 = new Rectangle(rectangle.Left, rectangle.Top + num7, rectangle.Width, num8);
		Rectangle rectangle4 = new Rectangle(rectangle.Left, rectangle.Top + num7 + num8, rectangle.Width, num9);
		Rectangle rectangle5 = new Rectangle(rectangle.Left, rectangle.Top + num7 + num8 + num9, rectangle.Width, num10);
		using StringFormat stringFormat2 = new StringFormat();
		stringFormat2.Alignment = StringAlignment.Near;
		stringFormat2.LineAlignment = StringAlignment.Center;
		stringFormat2.Trimming = StringTrimming.EllipsisCharacter;
		stringFormat2.FormatFlags = StringFormatFlags.NoWrap;
		using (Font font2 = new Font(Font, FontStyle.Bold))
		{
			using SolidBrush brush3 = new SolidBrush(ForeColor);
			g.DrawString(s, font2, brush3, rectangle2, stringFormat2);
		}
		using (SolidBrush brush4 = new SolidBrush(Color.FromArgb(70, 70, 70)))
		{
			g.DrawString(s2, Font, brush4, rectangle3, stringFormat2);
		}
		using (SolidBrush brush5 = new SolidBrush(Color.FromArgb(60, 60, 60)))
		{
			g.DrawString(s3, Font, brush5, rectangle4, stringFormat2);
		}
		using SolidBrush brush6 = new SolidBrush(Color.FromArgb(110, 110, 110));
		g.DrawString(s4, Font, brush6, rectangle5, stringFormat2);
	}

	private void drawRadioGlyph(Graphics g, Rectangle rect, bool selected)
	{
		using (Pen pen = new Pen(Color.FromArgb(110, 110, 110)))
		{
			g.DrawEllipse(pen, rect);
		}
		if (selected)
		{
			Rectangle rect2 = rect;
			int num = Math.Max(2, rect.Width / 4);
			rect2.Inflate(-num, -num);
			using SolidBrush brush = new SolidBrush(Color.FromArgb(40, 120, 200));
			g.FillEllipse(brush, rect2);
		}
	}

	private void drawTrashGlyph(Graphics g, Rectangle rect, bool hot)
	{
		Image trash_black = Resources.trash_black;
		if (trash_black == null)
		{
			return;
		}
		if (hot)
		{
			using SolidBrush brush = new SolidBrush(Color.FromArgb(64, 240, 70, 70));
			g.FillEllipse(brush, rect);
		}
		int num = trash_black.Width;
		int num2 = trash_black.Height;
		int num3 = rect.Left + (rect.Width - num) / 2;
		int num4 = rect.Top + (rect.Height - num2) / 2;
		g.DrawImage(trash_black, new Rectangle(num3, num4, num, num2));
	}

	private string buildLine1(RowItem item)
	{
		if (isAutoKey(item.Key))
		{
			bool num = !string.IsNullOrWhiteSpace(item.RadioIp) || !string.IsNullOrWhiteSpace(item.RadioMac) || !string.IsNullOrWhiteSpace(item.RadioModel);
			bool flag = !string.IsNullOrWhiteSpace(item.NicDescription) || !string.IsNullOrWhiteSpace(item.NicName) || !string.IsNullOrWhiteSpace(item.NicIp) || !string.IsNullOrWhiteSpace(item.NicMask);
			if (!num && !flag)
			{
				return "Use the first radio found using the settings above";
			}
		}
		string text = safe(item.RadioModel);
		string text2 = safe(item.RadioIp);
		string text3 = (item.RadioIsCustom ? "Custom" : safe(item.RadioMac));
		string text4 = ((item.RadioPort > 0) ? item.RadioPort.ToString() : "");
		string text5 = (string.IsNullOrWhiteSpace(text4) ? text2 : (text2 + ":" + text4));
		string text6 = "";
		if (!string.IsNullOrWhiteSpace(text))
		{
			text6 = text;
		}
		if (!string.IsNullOrWhiteSpace(text5))
		{
			if (text6.Length > 0)
			{
				text6 += "  ";
			}
			text6 += text5;
		}
		if (!string.IsNullOrWhiteSpace(text3))
		{
			if (text6.Length > 0)
			{
				text6 += "  ";
			}
			text6 += text3;
		}
		return text6;
	}

	private string buildLine2(RowItem item)
	{
		if (isAutoKey(item.Key))
		{
			bool num = !string.IsNullOrWhiteSpace(item.RadioIp) || !string.IsNullOrWhiteSpace(item.RadioMac) || !string.IsNullOrWhiteSpace(item.RadioModel);
			bool flag = !string.IsNullOrWhiteSpace(item.NicDescription) || !string.IsNullOrWhiteSpace(item.NicName) || !string.IsNullOrWhiteSpace(item.NicIp) || !string.IsNullOrWhiteSpace(item.NicMask);
			if (!num && !flag)
			{
				return "";
			}
		}
		string text;
		if (item.RadioProtocol == RadioDiscoveryRadioProtocol.P1)
		{
			text = "Protocol-1";
		}
		else if (item.RadioProtocol == RadioDiscoveryRadioProtocol.P2)
		{
			text = "Protocol-2";
			if ((item.RadioHwRev & 0x10) != 0)
			{
				text += " (24bit-rx/tx)";
			}
			else if (item.RadioProtocol2Supported > 0)
			{
				string text2 = ((float)(int)item.RadioProtocol2Supported / 10f).ToString("0.0", CultureInfo.InvariantCulture);
				text = text + " (v" + text2 + ")";
			}
		}
		else
		{
			text = item.RadioProtocol.ToString();
		}
		string text3 = safe(item.RadioVersionText);
		string text4 = ((!string.IsNullOrWhiteSpace(text) && !string.IsNullOrWhiteSpace(text3)) ? (text + "     Version " + text3) : (string.IsNullOrWhiteSpace(text) ? ("Version " + text3) : text));
		if (item.RadioProtocol != RadioDiscoveryRadioProtocol.P2)
		{
			return text4;
		}
		if (!item.IsConnected)
		{
			return text4 + "     PLL Unknown";
		}
		return text4 + (item.PllLocked ? "     PLL Locked" : "     PLL Not Locked");
	}

	private string buildNicLine3(RowItem item)
	{
		if (isAutoKey(item.Key))
		{
			bool num = !string.IsNullOrWhiteSpace(item.RadioIp) || !string.IsNullOrWhiteSpace(item.RadioMac) || !string.IsNullOrWhiteSpace(item.RadioModel);
			bool flag = !string.IsNullOrWhiteSpace(item.NicDescription) || !string.IsNullOrWhiteSpace(item.NicName) || !string.IsNullOrWhiteSpace(item.NicIp) || !string.IsNullOrWhiteSpace(item.NicMask);
			if (!num && !flag)
			{
				return "";
			}
		}
		string text = safe(item.NicDescription);
		if (!string.IsNullOrWhiteSpace(text))
		{
			return text;
		}
		return safe(item.NicName);
	}

	private string buildNicLine4(RowItem item)
	{
		if (isAutoKey(item.Key))
		{
			bool num = !string.IsNullOrWhiteSpace(item.RadioIp) || !string.IsNullOrWhiteSpace(item.RadioMac) || !string.IsNullOrWhiteSpace(item.RadioModel);
			bool flag = !string.IsNullOrWhiteSpace(item.NicDescription) || !string.IsNullOrWhiteSpace(item.NicName) || !string.IsNullOrWhiteSpace(item.NicIp) || !string.IsNullOrWhiteSpace(item.NicMask);
			if (!num && !flag)
			{
				return "";
			}
		}
		string text = safe(item.NicIp);
		string text2 = safe(item.NicMask);
		string text3 = safe(item.NicType);
		string text4 = "";
		if (!string.IsNullOrWhiteSpace(text) && !string.IsNullOrWhiteSpace(text2))
		{
			text4 = text + " / " + text2;
		}
		else if (!string.IsNullOrWhiteSpace(text))
		{
			text4 = text;
		}
		else if (!string.IsNullOrWhiteSpace(text2))
		{
			text4 = text2;
		}
		if (!string.IsNullOrWhiteSpace(text4) && !string.IsNullOrWhiteSpace(text3))
		{
			return text4 + "  " + text3;
		}
		if (!string.IsNullOrWhiteSpace(text4))
		{
			return text4;
		}
		return text3;
	}

	private HitTestResult hitTest(Point p)
	{
		HitTestResult hitTestResult = new HitTestResult();
		hitTestResult.RowIndex = -1;
		hitTestResult.IsTrash = false;
		Rectangle viewportRect = getViewportRect();
		if (!viewportRect.Contains(p))
		{
			return hitTestResult;
		}
		int num = p.Y - viewportRect.Top + _scroll.Value;
		int num2 = indexFromContentY(num);
		if (num2 < 0 || num2 >= _items.Count)
		{
			return hitTestResult;
		}
		int num3 = viewportRect.Top + rowTopForIndex(num2) - _scroll.Value;
		int num4 = rowHeightForIndex(num2);
		Rectangle rectangle = new Rectangle(viewportRect.Left, num3, viewportRect.Width, num4);
		hitTestResult.RowIndex = num2;
		if (num2 == 0)
		{
			return hitTestResult;
		}
		int num5 = scale(10);
		int num6 = scale(28);
		hitTestResult.IsTrash = new Rectangle(rectangle.Right - num5 - num6, rectangle.Top + (rectangle.Height - num6) / 2, num6, num6).Contains(p);
		return hitTestResult;
	}

	private void setSelectedByIndex(int idx, bool ensureVisible)
	{
		if (idx >= 0 && idx < _items.Count)
		{
			string key = _items[idx].Key;
			if (!string.Equals(_selected_key, key, StringComparison.OrdinalIgnoreCase))
			{
				_selected_key = key;
				raiseSelectedChanged();
				Invalidate();
			}
			if (ensureVisible)
			{
				MakeRadioVisible(key);
			}
		}
	}

	private int selectedIndex()
	{
		if (string.IsNullOrWhiteSpace(_selected_key))
		{
			return -1;
		}
		for (int i = 0; i < _items.Count; i++)
		{
			if (string.Equals(_items[i].Key, _selected_key, StringComparison.OrdinalIgnoreCase))
			{
				return i;
			}
		}
		return -1;
	}

	private RowItem getSelectedItem()
	{
		if (string.IsNullOrWhiteSpace(_selected_key))
		{
			return null;
		}
		for (int i = 0; i < _items.Count; i++)
		{
			if (string.Equals(_items[i].Key, _selected_key, StringComparison.OrdinalIgnoreCase))
			{
				return _items[i];
			}
		}
		return null;
	}

	private int indexOfKey(string key)
	{
		if (string.IsNullOrWhiteSpace(key))
		{
			return -1;
		}
		for (int i = 0; i < _items.Count; i++)
		{
			if (string.Equals(_items[i].Key, key, StringComparison.OrdinalIgnoreCase))
			{
				return i;
			}
		}
		return -1;
	}

	private Rectangle getViewportRect()
	{
		Rectangle clientRectangle = base.ClientRectangle;
		if (_scroll.Visible)
		{
			clientRectangle.Width = Math.Max(0, clientRectangle.Width - _scroll.Width);
		}
		return clientRectangle;
	}

	private int normalRowHeight()
	{
		int num = scale(78);
		int num2 = scale(62);
		if (num < num2)
		{
			num = num2;
		}
		return num;
	}

	private int compactRowHeight()
	{
		int num = scale(30);
		int num2 = scale(26);
		if (num < num2)
		{
			num = num2;
		}
		return num;
	}

	private bool autoRowIsCompact(RowItem item)
	{
		if (item == null)
		{
			return false;
		}
		if (!isAutoKey(item.Key))
		{
			return false;
		}
		bool num = !string.IsNullOrWhiteSpace(item.RadioIp) || !string.IsNullOrWhiteSpace(item.RadioMac) || !string.IsNullOrWhiteSpace(item.RadioModel);
		bool flag = !string.IsNullOrWhiteSpace(item.NicDescription) || !string.IsNullOrWhiteSpace(item.NicName) || !string.IsNullOrWhiteSpace(item.NicIp) || !string.IsNullOrWhiteSpace(item.NicMask);
		if (!num)
		{
			return !flag;
		}
		return false;
	}

	private int rowHeightForIndex(int idx)
	{
		if (idx == 0 && _items.Count > 0)
		{
			if (!autoRowIsCompact(_items[0]))
			{
				return normalRowHeight();
			}
			return compactRowHeight();
		}
		return normalRowHeight();
	}

	private int contentHeight()
	{
		int num = 0;
		for (int i = 0; i < _items.Count; i++)
		{
			num += rowHeightForIndex(i);
		}
		return num;
	}

	private int rowTopForIndex(int idx)
	{
		if (idx <= 0)
		{
			return 0;
		}
		int num = 0;
		for (int i = 0; i < idx && i < _items.Count; i++)
		{
			num += rowHeightForIndex(i);
		}
		return num;
	}

	private int indexFromContentY(int y)
	{
		if (y < 0)
		{
			return -1;
		}
		int num = 0;
		for (int i = 0; i < _items.Count; i++)
		{
			int num2 = rowHeightForIndex(i);
			if (y < num + num2)
			{
				return i;
			}
			num += num2;
		}
		return -1;
	}

	private int scale(int px)
	{
		int num;
		try
		{
			num = base.DeviceDpi;
		}
		catch
		{
			num = 96;
		}
		int num2 = (int)Math.Round((double)px * (double)num / 96.0);
		if (num2 < 1)
		{
			num2 = 1;
		}
		return num2;
	}

	private void updateScroll()
	{
		Rectangle viewportRect = getViewportRect();
		int num = contentHeight();
		int num2 = viewportRect.Height;
		if (num2 < 1)
		{
			num2 = 1;
		}
		_scroll.Minimum = 0;
		_scroll.LargeChange = num2;
		int num3 = normalRowHeight();
		if (num3 < 1)
		{
			num3 = 1;
		}
		_scroll.SmallChange = num3;
		int num4 = num - viewportRect.Height;
		if (num4 < 0)
		{
			num4 = 0;
		}
		_scroll.Maximum = Math.Max(0, num - 1);
		bool flag = num4 > 0;
		if (_scroll.Visible != flag)
		{
			_scroll.Visible = flag;
		}
		clampScrollValue();
	}

	private void clampScrollValue()
	{
		int num = _scroll.Maximum - _scroll.LargeChange + 1;
		if (num < 0)
		{
			num = 0;
		}
		int num2 = _scroll.Value;
		if (num2 < 0)
		{
			num2 = 0;
		}
		if (num2 > num)
		{
			num2 = num;
		}
		if (_scroll.Value != num2)
		{
			_scroll.Value = num2;
		}
	}

	private void setScrollValue(int value)
	{
		int num = _scroll.Maximum - _scroll.LargeChange + 1;
		if (num < 0)
		{
			num = 0;
		}
		int num2 = value;
		if (num2 < 0)
		{
			num2 = 0;
		}
		if (num2 > num)
		{
			num2 = num;
		}
		if (_scroll.Value != num2)
		{
			_scroll.Value = num2;
		}
		else
		{
			Invalidate();
		}
	}

	private void scroll_ValueChanged(object sender, EventArgs e)
	{
		Invalidate();
	}

	private void ucRadioList_SizeChanged(object sender, EventArgs e)
	{
		updateScroll();
		clampScrollValue();
		Invalidate();
	}

	private void raiseSelectedChanged()
	{
		SelectedRadioChanged?.Invoke(this, EventArgs.Empty);
	}

	private void raiseListChanged()
	{
		RadioListChanged?.Invoke(this, EventArgs.Empty);
	}

	private string ensureGuidString(string guid)
	{
		string text = safe(guid).Trim();
		if (text.Length > 0 && Guid.TryParse(text, out var result))
		{
			return result.ToString("D").ToUpperInvariant();
		}
		return Guid.NewGuid().ToString("D").ToUpperInvariant();
	}

	private string buildKey(string mac, string guid, bool isCustom, IPAddress ip, int port, string nicId, RadioDiscoveryRadioProtocol proto)
	{
		if (isCustom)
		{
			string text = ensureGuidString(guid);
			return "guid:" + text;
		}
		string text2 = safe(mac).Trim().ToUpperInvariant();
		string text3 = ((ip != null) ? ip.ToString() : "");
		string text4 = port.ToString();
		string text5 = safe(nicId);
		string text6 = proto.ToString().ToString();
		if (!string.IsNullOrWhiteSpace(text2))
		{
			return "mac:" + text2 + "|ip:" + text3 + "|port:" + text4 + "|nic:" + text5 + "|proto:" + text6;
		}
		return "ip:" + text3 + "|port:" + text4 + "|nic:" + text5 + "|proto:" + text6;
	}

	private bool isLegacyMacOnlyKey(string key)
	{
		string text = safe(key);
		if (text.Length == 0)
		{
			return false;
		}
		if (text.IndexOf("|", StringComparison.Ordinal) >= 0)
		{
			return false;
		}
		return text.StartsWith("mac:", StringComparison.OrdinalIgnoreCase);
	}

	private bool isAutoKey(string key)
	{
		return string.Equals(safe(key), "auto:first_radio", StringComparison.OrdinalIgnoreCase);
	}

	private void ensureAutoEntry(bool selectAutoIfMissingSelection)
	{
		if (_items.Count == 0)
		{
			RowItem rowItem = new RowItem();
			rowItem.Key = "auto:first_radio";
			resetAutoItem(rowItem);
			_items.Add(rowItem);
		}
		else if (!isAutoKey(_items[0].Key))
		{
			RowItem rowItem2 = new RowItem();
			rowItem2.Key = "auto:first_radio";
			resetAutoItem(rowItem2);
			_items.Insert(0, rowItem2);
		}
		if (selectAutoIfMissingSelection && (string.IsNullOrWhiteSpace(_selected_key) || !DoesRadioExist(_selected_key)))
		{
			_selected_key = "auto:first_radio";
		}
	}

	private void resetAutoItem(RowItem item)
	{
		item.NicId = "";
		item.NicName = "";
		item.NicDescription = "";
		item.NicType = "";
		item.NicIp = "";
		item.NicMask = "";
		item.NicInterfaceType = NetworkInterfaceType.Unknown;
		item.NicIsEthernet = false;
		item.NicIsWireless = false;
		item.NicSpeedBitsPerSecond = 0L;
		item.NicMacAddress = "";
		item.NicIsApipaLocal = false;
		item.NicIsLoopbackLocal = false;
		item.NicIsDhcpEnabled = false;
		item.NicStatus = OperationalStatus.Unknown;
		item.NicMtu = 0;
		item.RadioModel = "";
		item.RadioIp = "";
		item.RadioPort = 0;
		item.RadioProtocol = RadioDiscoveryRadioProtocol.P1;
		item.RadioVersionText = "";
		item.RadioMac = "";
		item.RadioIsCustom = false;
		item.RadioGuid = "";
		item.RadioCodeVersion = 0;
		item.RadioBetaVersion = 0;
		item.RadioProtocol2Supported = 0;
		item.RadioNumRxs = 0;
		item.RadioMercuryVersion0 = 0;
		item.RadioMercuryVersion1 = 0;
		item.RadioMercuryVersion2 = 0;
		item.RadioMercuryVersion3 = 0;
		item.RadioPennyVersion = 0;
		item.RadioMetisVersion = 0;
		item.RadioIsBusy = false;
		item.RadioDiscoveryPortBase = 0;
		item.RadioPortCount = 0;
		item.RadioIsApipaRadio = false;
		item.IsConnected = false;
		item.PllLocked = false;
	}

	private void fillItemFromInfo(RowItem item, NicRadioScanResult nic, RadioInfo radio)
	{
		IPAddress ipAddress = radio.IpAddress;
		IPAddress localIPv = nic.LocalIPv4;
		IPAddress localMaskIPv = nic.LocalMaskIPv4;
		int num = radio.DiscoveryPortBase;
		if (num < 1)
		{
			num = 1024;
		}
		string text = safe(radio.MacAddress);
		bool flag = radio.IsCustom || string.IsNullOrWhiteSpace(text);
		string text2 = "";
		if (flag)
		{
			text2 = (radio.CustomGuid = ensureGuidString(radio.CustomGuid));
			radio.IsCustom = true;
			text = "";
		}
		string nicType = (nic.IsEthernet ? "Ethernet" : ((!nic.IsWireless) ? nic.NicInterfaceTypeString : "WiFi"));
		string radioVersionText = buildVersionText(radio);
		item.NicId = safe(nic.NicId);
		item.NicName = safe(nic.NicName);
		item.NicDescription = safe(nic.NicDescription);
		item.NicType = nicType;
		item.NicIp = ((localIPv != null) ? localIPv.ToString() : "");
		item.NicMask = ((localMaskIPv != null) ? localMaskIPv.ToString() : "");
		item.NicInterfaceType = nic.NicInterfaceType;
		item.NicIsEthernet = nic.IsEthernet;
		item.NicIsWireless = nic.IsWireless;
		item.NicSpeedBitsPerSecond = nic.NicSpeedBitsPerSecond;
		item.NicMacAddress = safe(nic.NicMacAddress);
		item.NicIsApipaLocal = nic.IsApipaLocal;
		item.NicIsLoopbackLocal = nic.IsLoopbackLocal;
		item.NicIsDhcpEnabled = nic.IsDhcpEnabled;
		item.NicStatus = nic.NicStatus;
		item.NicMtu = nic.Mtu;
		item.RadioModel = (((radio.HwRev & 0x10) != 0) ? "Anvelina PRO3 (24bit)" : radio.DeviceType.ToString());
		item.RadioDeviceType = radio.DeviceType;
		item.RadioIp = ((ipAddress != null) ? ipAddress.ToString() : "");
		item.RadioPort = num;
		item.RadioProtocol = radio.Protocol;
		item.RadioVersionText = radioVersionText;
		item.RadioMac = text;
		item.RadioIsCustom = flag;
		item.RadioGuid = (flag ? text2 : "");
		item.RadioCodeVersion = radio.CodeVersion;
		item.RadioBetaVersion = radio.BetaVersion;
		item.RadioProtocol2Supported = radio.Protocol2Supported;
		item.RadioNumRxs = radio.NumRxs;
		item.RadioMercuryVersion0 = radio.MercuryVersion0;
		item.RadioMercuryVersion1 = radio.MercuryVersion1;
		item.RadioMercuryVersion2 = radio.MercuryVersion2;
		item.RadioMercuryVersion3 = radio.MercuryVersion3;
		item.RadioPennyVersion = radio.PennyVersion;
		item.RadioMetisVersion = radio.MetisVersion;
		item.RadioHwRev = radio.HwRev;
		item.RadioIsBusy = radio.IsBusy;
		item.RadioDiscoveryPortBase = radio.DiscoveryPortBase;
		item.RadioPortCount = radio.PortCount;
		item.RadioIsApipaRadio = radio.IsApipaRadio;
	}

	private string buildVersionText(RadioInfo radio)
	{
		string text;
		if (radio.DeviceType == HPSDRHW.Saturn)
		{
			text = "fpga=" + radio.CodeVersion;
			if (radio.BetaVersion >= 39)
			{
				text = text + " p2app=" + radio.BetaVersion;
			}
		}
		else
		{
			text = ((float)(int)radio.CodeVersion / 10f).ToString("F1", _nfi);
			if (radio.Protocol == RadioDiscoveryRadioProtocol.P2 && radio.BetaVersion > 0)
			{
				text = text + "." + radio.BetaVersion;
			}
		}
		return text;
	}

	private string safe(string s)
	{
		if (s == null)
		{
			return "";
		}
		return s.Trim();
	}

	private string enc(string s)
	{
		string text = safe(s);
		if (text.Length == 0)
		{
			return "";
		}
		return text.Replace("/", "%2F");
	}

	private string dec(string s)
	{
		string text = safe(s);
		if (text.Length == 0)
		{
			return "";
		}
		return text.Replace("%2F", "/");
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
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
	}
}
