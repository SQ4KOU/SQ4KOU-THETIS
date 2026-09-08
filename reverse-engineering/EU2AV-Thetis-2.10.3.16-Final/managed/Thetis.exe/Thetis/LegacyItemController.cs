namespace Thetis;

internal static class LegacyItemController
{
	private static Console _console;

	private static bool _update_on_property_change;

	private static bool _hide_bands;

	private static bool _hide_modes;

	private static bool _hide_filters;

	private static bool _hide_meters;

	private static bool _expand_spectrum_to_right;

	private static bool _hide_vfoA;

	private static bool _hide_vfoB;

	private static bool _hide_vfo_sync;

	private static bool _expand_spectrum_to_top;

	private static bool _hide_power_rx;

	private static bool _hide_mon_tune;

	private static bool _hide_splt_rit_vac;

	private static bool _hide_noise_mnf;

	private static bool _hide_mic_comp;

	private static bool _hide_display_controls;

	public static bool HideSplitRitVac
	{
		get
		{
			if (_console != null)
			{
				if (_hide_splt_rit_vac)
				{
					return _console.IsExpandedView;
				}
				return false;
			}
			return _hide_splt_rit_vac;
		}
		set
		{
			_hide_splt_rit_vac = value;
			if (_update_on_property_change)
			{
				Update();
			}
		}
	}

	public static bool HideNoiseMnf
	{
		get
		{
			if (_console != null)
			{
				if (_hide_noise_mnf)
				{
					return _console.IsExpandedView;
				}
				return false;
			}
			return _hide_noise_mnf;
		}
		set
		{
			_hide_noise_mnf = value;
			if (_update_on_property_change)
			{
				Update();
			}
		}
	}

	public static bool HideMicCompVox
	{
		get
		{
			if (_console != null)
			{
				if (_hide_mic_comp)
				{
					return _console.IsExpandedView;
				}
				return false;
			}
			return _hide_mic_comp;
		}
		set
		{
			_hide_mic_comp = value;
			if (_update_on_property_change)
			{
				Update();
			}
		}
	}

	public static bool HideDisplayControls
	{
		get
		{
			if (_console != null)
			{
				if (_hide_display_controls)
				{
					return _console.IsExpandedView;
				}
				return false;
			}
			return _hide_display_controls;
		}
		set
		{
			_hide_display_controls = value;
			if (_update_on_property_change)
			{
				Update();
			}
		}
	}

	public static bool HidePowerRx
	{
		get
		{
			if (_console != null)
			{
				if (_hide_power_rx)
				{
					return _console.IsExpandedView;
				}
				return false;
			}
			return _hide_power_rx;
		}
		set
		{
			_hide_power_rx = value;
			if (_update_on_property_change)
			{
				Update();
			}
		}
	}

	public static bool HideMonTune
	{
		get
		{
			if (_console != null)
			{
				if (_hide_mon_tune)
				{
					return _console.IsExpandedView;
				}
				return false;
			}
			return _hide_mon_tune;
		}
		set
		{
			_hide_mon_tune = value;
			if (_update_on_property_change)
			{
				Update();
			}
		}
	}

	public static bool HideMeters
	{
		get
		{
			if (_console != null)
			{
				if (_hide_meters)
				{
					return _console.IsExpandedView;
				}
				return false;
			}
			return _hide_meters;
		}
		set
		{
			_hide_meters = value;
			if (_update_on_property_change)
			{
				Update();
			}
		}
	}

	public static bool HideBands
	{
		get
		{
			if (_console != null)
			{
				if (_hide_bands)
				{
					return _console.IsExpandedView;
				}
				return false;
			}
			return _hide_bands;
		}
		set
		{
			_hide_bands = value;
			if (_update_on_property_change)
			{
				Update();
			}
		}
	}

	public static bool HideModes
	{
		get
		{
			if (_console != null)
			{
				if (_hide_modes)
				{
					return _console.IsExpandedView;
				}
				return false;
			}
			return _hide_modes;
		}
		set
		{
			_hide_modes = value;
			if (_update_on_property_change)
			{
				Update();
			}
		}
	}

	public static bool HideFilters
	{
		get
		{
			if (_console != null)
			{
				if (_hide_filters)
				{
					return _console.IsExpandedView;
				}
				return false;
			}
			return _hide_filters;
		}
		set
		{
			_hide_filters = value;
			if (_update_on_property_change)
			{
				Update();
			}
		}
	}

	public static bool HideVFOA
	{
		get
		{
			return _hide_vfoA;
		}
		set
		{
			_hide_vfoA = value;
			if (_update_on_property_change)
			{
				Update();
			}
		}
	}

	public static bool HideVFOB
	{
		get
		{
			return _hide_vfoB;
		}
		set
		{
			_hide_vfoB = value;
			if (_update_on_property_change)
			{
				Update();
			}
		}
	}

	public static bool HideVFOSync
	{
		get
		{
			return _hide_vfo_sync;
		}
		set
		{
			_hide_vfo_sync = value;
			if (_update_on_property_change)
			{
				Update();
			}
		}
	}

	public static bool ExpandSpectrumToRight
	{
		get
		{
			if (_expand_spectrum_to_right && _hide_bands && _hide_filters)
			{
				return _hide_modes & _hide_meters;
			}
			return false;
		}
		set
		{
			_expand_spectrum_to_right = value;
			if (_update_on_property_change)
			{
				Update();
			}
		}
	}

	public static bool ExpandSpectrumToTop
	{
		get
		{
			if (_expand_spectrum_to_top && _hide_vfoA && _hide_vfoB)
			{
				return _hide_vfo_sync & _hide_meters;
			}
			return false;
		}
		set
		{
			_expand_spectrum_to_top = value;
			if (_update_on_property_change)
			{
				Update();
			}
		}
	}

	static LegacyItemController()
	{
		_console = null;
		_hide_bands = false;
		_hide_modes = false;
		_hide_filters = false;
		_hide_meters = false;
		_expand_spectrum_to_right = false;
		_update_on_property_change = false;
		_hide_vfoA = false;
		_hide_vfoB = false;
		_hide_vfo_sync = false;
		_expand_spectrum_to_right = false;
		_hide_power_rx = false;
		_hide_mon_tune = false;
		_hide_splt_rit_vac = false;
		_hide_noise_mnf = false;
		_hide_mic_comp = false;
		_hide_display_controls = false;
	}

	public static void Init(Console c)
	{
		_console = c;
		_update_on_property_change = true;
	}

	public static void Update()
	{
		if (_console == null || _console.IsSetupFormNull)
		{
			return;
		}
		if (_console.IsCollapsedView && !_console.IsExpandedView)
		{
			_console.BandPanelVisible(!_console.SetupForm.chkShowBandControls.Checked || _console.SetupForm.chkShowAndromedaBar.Checked);
			_console.ModePanelVisible(_console.SetupForm.chkShowModeControls.Checked && !_console.SetupForm.chkShowAndromedaBar.Checked);
			_console.FilterPanelVisible(visible: false);
			_console.ExtendPanelDisplaySizeRight(expand: false);
			_console.VFOAVisible(_console.ShowRX1 || _console.ShowAndromedaTopControls);
			_console.VFOBVisible(_console.ShowRX2 || _console.ShowAndromedaTopControls);
			_console.PowerRxPanelVisible(visible: false);
			_console.MonTunePanelVisible(visible: false);
		}
		else if (_console.IsExpandedView && !_console.IsCollapsedView)
		{
			_console.BandPanelVisible();
			_console.ModePanelVisible(!_hide_modes);
			_console.FilterPanelVisible(!_hide_filters);
			if (_expand_spectrum_to_right && _hide_bands && _hide_filters && (_hide_modes & _hide_meters))
			{
				_console.ExtendPanelDisplaySizeRight(expand: true);
			}
			else
			{
				_console.ExtendPanelDisplaySizeRight(expand: false);
			}
			if (_expand_spectrum_to_top && _hide_vfoA && _hide_vfoB && (_hide_vfo_sync & _hide_meters))
			{
				_console.ExtendPanelDisplaySizeTop(expand: true);
			}
			else
			{
				_console.ExtendPanelDisplaySizeTop(expand: false);
			}
			_console.VFOAVisible(!_hide_vfoA);
			_console.VFOBVisible(!_hide_vfoB);
			_console.VFOSyncVisible(!_hide_vfo_sync);
			_console.PowerRxPanelVisible(!_hide_power_rx);
			_console.MonTunePanelVisible(!_hide_mon_tune);
			_console.SplitRitVacPanelVisible(!_hide_splt_rit_vac);
			_console.NoiseMnfPanelVisible(!_hide_noise_mnf);
			_console.MicCompVoxPanelVisible(!_hide_mic_comp);
			_console.DisplayControlsPanelVisible(!_hide_display_controls);
		}
	}
}
