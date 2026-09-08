namespace Thetis;

internal static class HardwareSpecific
{
	private static HPSDRModel _model;

	private static HPSDRModel _old_model;

	private static HPSDRHW _hardware;

	private static HPSDRHW _old_hardware;

	public static HPSDRModel Model
	{
		get
		{
			return _model;
		}
		set
		{
			_old_model = _model;
			_model = value;
			NetworkIO.FWVersionsChecked = false;
			switch (_model)
			{
			case HPSDRModel.HERMES:
				NetworkIO.SetRxADC(1);
				NetworkIO.SetMKIIBPF(0);
				cmaster.SetADCSupply(0, 33);
				NetworkIO.LRAudioSwap(1);
				Hardware = HPSDRHW.Hermes;
				break;
			case HPSDRModel.ANAN10:
				NetworkIO.SetRxADC(1);
				NetworkIO.SetMKIIBPF(0);
				cmaster.SetADCSupply(0, 33);
				NetworkIO.LRAudioSwap(1);
				Hardware = HPSDRHW.Hermes;
				break;
			case HPSDRModel.ANAN10E:
				NetworkIO.SetRxADC(1);
				NetworkIO.SetMKIIBPF(0);
				cmaster.SetADCSupply(0, 33);
				NetworkIO.LRAudioSwap(1);
				Hardware = HPSDRHW.HermesII;
				break;
			case HPSDRModel.ANAN100:
				NetworkIO.SetRxADC(1);
				NetworkIO.SetMKIIBPF(0);
				cmaster.SetADCSupply(0, 33);
				NetworkIO.LRAudioSwap(1);
				Hardware = HPSDRHW.Hermes;
				break;
			case HPSDRModel.ANAN100B:
				NetworkIO.SetRxADC(1);
				NetworkIO.SetMKIIBPF(0);
				cmaster.SetADCSupply(0, 33);
				NetworkIO.LRAudioSwap(1);
				Hardware = HPSDRHW.HermesII;
				break;
			case HPSDRModel.ANAN100D:
				NetworkIO.SetRxADC(2);
				NetworkIO.SetMKIIBPF(0);
				cmaster.SetADCSupply(0, 33);
				NetworkIO.LRAudioSwap(0);
				Hardware = HPSDRHW.Angelia;
				break;
			case HPSDRModel.ANAN_G2E:
				NetworkIO.SetRxADC(1);
				NetworkIO.SetMKIIBPF(1);
				cmaster.SetADCSupply(0, 33);
				NetworkIO.LRAudioSwap(0);
				Hardware = HPSDRHW.HermesC10;
				break;
			case HPSDRModel.ANAN200D:
				NetworkIO.SetRxADC(2);
				NetworkIO.SetMKIIBPF(0);
				cmaster.SetADCSupply(0, 50);
				NetworkIO.LRAudioSwap(0);
				Hardware = HPSDRHW.Orion;
				break;
			case HPSDRModel.ORIONMKII:
				NetworkIO.SetRxADC(2);
				NetworkIO.SetMKIIBPF(1);
				cmaster.SetADCSupply(0, 50);
				NetworkIO.LRAudioSwap(0);
				Hardware = HPSDRHW.OrionMKII;
				break;
			case HPSDRModel.ANAN7000D:
				NetworkIO.SetRxADC(2);
				NetworkIO.SetMKIIBPF(1);
				cmaster.SetADCSupply(0, 50);
				NetworkIO.LRAudioSwap(0);
				Hardware = HPSDRHW.OrionMKII;
				break;
			case HPSDRModel.ANAN8000D:
				NetworkIO.SetRxADC(2);
				NetworkIO.SetMKIIBPF(1);
				cmaster.SetADCSupply(0, 50);
				NetworkIO.LRAudioSwap(0);
				Hardware = HPSDRHW.OrionMKII;
				break;
			case HPSDRModel.ANAN_G2:
				NetworkIO.SetRxADC(2);
				NetworkIO.SetMKIIBPF(1);
				cmaster.SetADCSupply(0, 50);
				NetworkIO.LRAudioSwap(0);
				Hardware = HPSDRHW.Saturn;
				break;
			case HPSDRModel.ANAN_G2_1K:
				NetworkIO.SetRxADC(2);
				NetworkIO.SetMKIIBPF(1);
				cmaster.SetADCSupply(0, 50);
				NetworkIO.LRAudioSwap(0);
				Hardware = HPSDRHW.Saturn;
				break;
			case HPSDRModel.ANVELINAPRO3:
				NetworkIO.SetRxADC(2);
				NetworkIO.SetMKIIBPF(1);
				cmaster.SetADCSupply(0, 50);
				NetworkIO.LRAudioSwap(0);
				Hardware = HPSDRHW.OrionMKII;
				break;
			case HPSDRModel.REDPITAYA:
				NetworkIO.SetRxADC(2);
				NetworkIO.SetMKIIBPF(0);
				cmaster.SetADCSupply(0, 50);
				NetworkIO.LRAudioSwap(0);
				Hardware = HPSDRHW.OrionMKII;
				break;
			case HPSDRModel.HERMESLITE:
				break;
			}
		}
	}

	public static HPSDRModel OldModel => _old_model;

	public static int ModelInt => (int)_model;

	public static int OldModelInt => (int)_old_model;

	public static HPSDRHW Hardware
	{
		get
		{
			return _hardware;
		}
		set
		{
			_old_hardware = _hardware;
			_hardware = value;
		}
	}

	public static HPSDRHW OldHardware => _old_hardware;

	public static bool HasVolts
	{
		get
		{
			if (_model != HPSDRModel.ANAN7000D && _model != HPSDRModel.ANAN8000D && _model != HPSDRModel.ANVELINAPRO3 && _model != HPSDRModel.ANAN_G2E && _model != HPSDRModel.ANAN_G2 && _model != HPSDRModel.ANAN_G2_1K)
			{
				return _model == HPSDRModel.REDPITAYA;
			}
			return true;
		}
	}

	public static bool HasAmps
	{
		get
		{
			if (_model != HPSDRModel.ANAN7000D && _model != HPSDRModel.ANAN8000D && _model != HPSDRModel.ANVELINAPRO3 && _model != HPSDRModel.ANAN_G2E && _model != HPSDRModel.ANAN_G2 && _model != HPSDRModel.ANAN_G2_1K)
			{
				return _model == HPSDRModel.REDPITAYA;
			}
			return true;
		}
	}

	public static double PSDefaultPeak
	{
		get
		{
			if (NetworkIO.CurrentRadioProtocol == RadioProtocol.USB)
			{
				_ = _hardware;
				return 0.4072;
			}
			if (_hardware == HPSDRHW.Saturn)
			{
				return 0.6121;
			}
			return 0.2899;
		}
	}

	public static int PSTargetFeedbackLevel => 0;

	public static bool PSOutlierEnableDefault
	{
		get
		{
			HPSDRModel model = _model;
			if ((uint)(model - 9) <= 1u || model == HPSDRModel.ANVELINAPRO3)
			{
				return true;
			}
			return false;
		}
	}

	public static double PSOutlierSigmaDefault
	{
		get
		{
			HPSDRModel model = _model;
			if ((uint)(model - 9) <= 1u || model == HPSDRModel.ANVELINAPRO3)
			{
				return 5.0;
			}
			return 2.5;
		}
	}

	public static string ModelString => EnumModelToString(_model);

	public static float RXMeterCalbrationOffset => RXMeterCalbrationOffsetDefaults(_model);

	public static float RXDisplayCalbrationOffset => RXDisplayCalbrationOffsetDefauls(_model);

	public static bool HasAudioAmplifier
	{
		get
		{
			if (NetworkIO.CurrentRadioProtocol == RadioProtocol.ETH)
			{
				if (_model != HPSDRModel.ANAN7000D && _model != HPSDRModel.ANAN8000D && _model != HPSDRModel.ANVELINAPRO3 && _model != HPSDRModel.ANAN_G2 && _model != HPSDRModel.ANAN_G2_1K)
				{
					return _model == HPSDRModel.REDPITAYA;
				}
				return true;
			}
			return false;
		}
	}

	public static bool SupportsPathIllustrator
	{
		get
		{
			if (_model != HPSDRModel.ORIONMKII && _model != HPSDRModel.ANAN7000D && _model != HPSDRModel.ANAN8000D && _model != HPSDRModel.ANAN_G2 && _model != HPSDRModel.ANAN_G2_1K && _model != HPSDRModel.ANVELINAPRO3)
			{
				return _model != HPSDRModel.REDPITAYA;
			}
			return false;
		}
	}

	static HardwareSpecific()
	{
		_model = HPSDRModel.FIRST;
		_old_model = HPSDRModel.FIRST;
		_hardware = HPSDRHW.Unknown;
		_old_hardware = HPSDRHW.Unknown;
	}

	public static (float, float) GetDefaultVoltCalibration()
	{
		float item;
		float item2;
		switch (_model)
		{
		case HPSDRModel.ANAN7000D:
		case HPSDRModel.ANVELINAPRO3:
		case HPSDRModel.REDPITAYA:
			item = 340f;
			item2 = 88f;
			break;
		case HPSDRModel.ANAN_G2:
			item = 0.001f;
			item2 = 66.23f;
			break;
		case HPSDRModel.ANAN_G2_1K:
			item = 0.001f;
			item2 = 66.23f;
			break;
		default:
			item = 360f;
			item2 = 120f;
			break;
		}
		return (item, item2);
	}

	public static HPSDRModel StringModelToEnum(string sModel)
	{
		return sModel.ToUpper() switch
		{
			"HERMES" => HPSDRModel.HERMES, 
			"ANAN-10" => HPSDRModel.ANAN10, 
			"ANAN-10E" => HPSDRModel.ANAN10E, 
			"ANAN-100" => HPSDRModel.ANAN100, 
			"ANAN-100B" => HPSDRModel.ANAN100B, 
			"ANAN-100D" => HPSDRModel.ANAN100D, 
			"ANAN-200D" => HPSDRModel.ANAN200D, 
			"ANAN-7000DLE" => HPSDRModel.ANAN7000D, 
			"ANAN-8000DLE" => HPSDRModel.ANAN8000D, 
			"ANAN-G2" => HPSDRModel.ANAN_G2, 
			"ANAN-G2-1K" => HPSDRModel.ANAN_G2_1K, 
			"ANVELINA-PRO3" => HPSDRModel.ANVELINAPRO3, 
			"HERMESLITE" => HPSDRModel.HERMESLITE, 
			"RED-PITAYA" => HPSDRModel.REDPITAYA, 
			"ANAN-G2E" => HPSDRModel.ANAN_G2E, 
			_ => HPSDRModel.HERMES, 
		};
	}

	public static string EnumModelToString(HPSDRModel model)
	{
		return model switch
		{
			HPSDRModel.HERMES => "HERMES", 
			HPSDRModel.ANAN10 => "ANAN-10", 
			HPSDRModel.ANAN10E => "ANAN-10E", 
			HPSDRModel.ANAN100 => "ANAN-100", 
			HPSDRModel.ANAN100B => "ANAN-100B", 
			HPSDRModel.ANAN100D => "ANAN-100D", 
			HPSDRModel.ANAN200D => "ANAN-200D", 
			HPSDRModel.ANAN7000D => "ANAN-7000DLE", 
			HPSDRModel.ANAN8000D => "ANAN-8000DLE", 
			HPSDRModel.ANAN_G2E => "ANAN-G2E", 
			HPSDRModel.ANAN_G2 => "ANAN-G2", 
			HPSDRModel.ANAN_G2_1K => "ANAN-G2-1K", 
			HPSDRModel.ANVELINAPRO3 => "ANVELINA-PRO3", 
			HPSDRModel.HERMESLITE => "HERMES-LITE", 
			HPSDRModel.REDPITAYA => "RED-PITAYA", 
			_ => "HERMES", 
		};
	}

	public static float RXMeterCalbrationOffsetDefaults(HPSDRModel model)
	{
		switch (model)
		{
		case HPSDRModel.ORIONMKII:
		case HPSDRModel.ANAN7000D:
		case HPSDRModel.ANAN8000D:
		case HPSDRModel.ANVELINAPRO3:
		case HPSDRModel.REDPITAYA:
			return 4.841644f;
		case HPSDRModel.ANAN_G2:
		case HPSDRModel.ANAN_G2_1K:
			return -4.476f;
		default:
			return 0.98f;
		}
	}

	public static float RXDisplayCalbrationOffsetDefauls(HPSDRModel model)
	{
		switch (model)
		{
		case HPSDRModel.ORIONMKII:
		case HPSDRModel.ANAN7000D:
		case HPSDRModel.ANAN8000D:
		case HPSDRModel.ANVELINAPRO3:
		case HPSDRModel.REDPITAYA:
			return 5.259f;
		case HPSDRModel.ANAN_G2:
		case HPSDRModel.ANAN_G2_1K:
			return -4.4005f;
		default:
			return -2.1f;
		}
	}

	public static float[] DefaultPAGainsForBands(HPSDRModel model)
	{
		float[] array = new float[42];
		for (int i = 0; i < 42; i++)
		{
			array[i] = 100f;
		}
		switch (model)
		{
		case HPSDRModel.FIRST:
		case HPSDRModel.HPSDR:
		case HPSDRModel.HERMES:
		case HPSDRModel.ORIONMKII:
			array[1] = 41f;
			array[2] = 41.2f;
			array[3] = 41.3f;
			array[4] = 41.3f;
			array[5] = 41f;
			array[6] = 40.5f;
			array[7] = 39.9f;
			array[8] = 38.8f;
			array[9] = 38.8f;
			array[10] = 38.8f;
			array[11] = 38.8f;
			array[14] = 56.2f;
			array[15] = 56.2f;
			array[16] = 56.2f;
			array[17] = 56.2f;
			array[18] = 56.2f;
			array[19] = 56.2f;
			array[20] = 56.2f;
			array[21] = 56.2f;
			array[22] = 56.2f;
			array[23] = 56.2f;
			array[24] = 56.2f;
			array[25] = 56.2f;
			array[26] = 56.2f;
			array[27] = 56.2f;
			return array;
		case HPSDRModel.ANAN10:
		case HPSDRModel.ANAN10E:
			array[1] = 41f;
			array[2] = 41.2f;
			array[3] = 41.3f;
			array[4] = 41.3f;
			array[5] = 41f;
			array[6] = 40.5f;
			array[7] = 39.9f;
			array[8] = 38.8f;
			array[9] = 38.8f;
			array[10] = 38.8f;
			array[11] = 38.8f;
			array[14] = 56.2f;
			array[15] = 56.2f;
			array[16] = 56.2f;
			array[17] = 56.2f;
			array[18] = 56.2f;
			array[19] = 56.2f;
			array[20] = 56.2f;
			array[21] = 56.2f;
			array[22] = 56.2f;
			array[23] = 56.2f;
			array[24] = 56.2f;
			array[25] = 56.2f;
			array[26] = 56.2f;
			array[27] = 56.2f;
			return array;
		case HPSDRModel.ANAN100:
			array[1] = 50f;
			array[2] = 50.5f;
			array[3] = 50.5f;
			array[4] = 50f;
			array[5] = 49.5f;
			array[6] = 48.5f;
			array[7] = 48f;
			array[8] = 47.5f;
			array[9] = 46.5f;
			array[10] = 42f;
			array[11] = 43f;
			array[14] = 56.2f;
			array[15] = 56.2f;
			array[16] = 56.2f;
			array[17] = 56.2f;
			array[18] = 56.2f;
			array[19] = 56.2f;
			array[20] = 56.2f;
			array[21] = 56.2f;
			array[22] = 56.2f;
			array[23] = 56.2f;
			array[24] = 56.2f;
			array[25] = 56.2f;
			array[26] = 56.2f;
			array[27] = 56.2f;
			return array;
		case HPSDRModel.ANAN100B:
			array[1] = 50f;
			array[2] = 50.5f;
			array[3] = 50.5f;
			array[4] = 50f;
			array[5] = 49.5f;
			array[6] = 48.5f;
			array[7] = 48f;
			array[8] = 47.5f;
			array[9] = 46.5f;
			array[10] = 42f;
			array[11] = 43f;
			array[14] = 56.2f;
			array[15] = 56.2f;
			array[16] = 56.2f;
			array[17] = 56.2f;
			array[18] = 56.2f;
			array[19] = 56.2f;
			array[20] = 56.2f;
			array[21] = 56.2f;
			array[22] = 56.2f;
			array[23] = 56.2f;
			array[24] = 56.2f;
			array[25] = 56.2f;
			array[26] = 56.2f;
			array[27] = 56.2f;
			return array;
		case HPSDRModel.ANAN100D:
			array[1] = 49.5f;
			array[2] = 50.5f;
			array[3] = 50.5f;
			array[4] = 50f;
			array[5] = 49f;
			array[6] = 48f;
			array[7] = 47f;
			array[8] = 46.5f;
			array[9] = 46f;
			array[10] = 43.5f;
			array[11] = 43f;
			array[14] = 56.2f;
			array[15] = 56.2f;
			array[16] = 56.2f;
			array[17] = 56.2f;
			array[18] = 56.2f;
			array[19] = 56.2f;
			array[20] = 56.2f;
			array[21] = 56.2f;
			array[22] = 56.2f;
			array[23] = 56.2f;
			array[24] = 56.2f;
			array[25] = 56.2f;
			array[26] = 56.2f;
			array[27] = 56.2f;
			return array;
		case HPSDRModel.ANAN200D:
			array[1] = 49.5f;
			array[2] = 50.5f;
			array[3] = 50.5f;
			array[4] = 50f;
			array[5] = 49f;
			array[6] = 48f;
			array[7] = 47f;
			array[8] = 46.5f;
			array[9] = 46f;
			array[10] = 43.5f;
			array[11] = 43f;
			array[14] = 56.2f;
			array[15] = 56.2f;
			array[16] = 56.2f;
			array[17] = 56.2f;
			array[18] = 56.2f;
			array[19] = 56.2f;
			array[20] = 56.2f;
			array[21] = 56.2f;
			array[22] = 56.2f;
			array[23] = 56.2f;
			array[24] = 56.2f;
			array[25] = 56.2f;
			array[26] = 56.2f;
			array[27] = 56.2f;
			return array;
		case HPSDRModel.ANAN8000D:
			array[1] = 50f;
			array[2] = 50.5f;
			array[3] = 50.5f;
			array[4] = 50f;
			array[5] = 49.5f;
			array[6] = 48.5f;
			array[7] = 48f;
			array[8] = 47.5f;
			array[9] = 46.5f;
			array[10] = 42f;
			array[11] = 43f;
			array[14] = 56.2f;
			array[15] = 56.2f;
			array[16] = 56.2f;
			array[17] = 56.2f;
			array[18] = 56.2f;
			array[19] = 56.2f;
			array[20] = 56.2f;
			array[21] = 56.2f;
			array[22] = 56.2f;
			array[23] = 56.2f;
			array[24] = 56.2f;
			array[25] = 56.2f;
			array[26] = 56.2f;
			array[27] = 56.2f;
			return array;
		case HPSDRModel.ANAN7000D:
		case HPSDRModel.ANAN_G2:
		case HPSDRModel.ANVELINAPRO3:
		case HPSDRModel.REDPITAYA:
		case HPSDRModel.ANAN_G2E:
			array[1] = 47.9f;
			array[2] = 50.5f;
			array[3] = 50.8f;
			array[4] = 50.8f;
			array[5] = 50.9f;
			array[6] = 50.9f;
			array[7] = 50.5f;
			array[8] = 47f;
			array[9] = 47.9f;
			array[10] = 46.5f;
			array[11] = 44.6f;
			array[14] = 63.1f;
			array[15] = 63.1f;
			array[16] = 63.1f;
			array[17] = 63.1f;
			array[18] = 63.1f;
			array[19] = 63.1f;
			array[20] = 63.1f;
			array[21] = 63.1f;
			array[22] = 63.1f;
			array[23] = 63.1f;
			array[24] = 63.1f;
			array[25] = 63.1f;
			array[26] = 63.1f;
			array[27] = 63.1f;
			return array;
		case HPSDRModel.ANAN_G2_1K:
			array[1] = 47.9f;
			array[2] = 50.5f;
			array[3] = 50.8f;
			array[4] = 50.8f;
			array[5] = 50.9f;
			array[6] = 50.9f;
			array[7] = 50.5f;
			array[8] = 47f;
			array[9] = 47.9f;
			array[10] = 46.5f;
			array[11] = 44.6f;
			array[14] = 63.1f;
			array[15] = 63.1f;
			array[16] = 63.1f;
			array[17] = 63.1f;
			array[18] = 63.1f;
			array[19] = 63.1f;
			array[20] = 63.1f;
			array[21] = 63.1f;
			array[22] = 63.1f;
			array[23] = 63.1f;
			array[24] = 63.1f;
			array[25] = 63.1f;
			array[26] = 63.1f;
			array[27] = 63.1f;
			return array;
		default:
			return array;
		}
	}

	public static float[] DefaultPAGainsForBands()
	{
		return DefaultPAGainsForBands(_model);
	}

	public static bool HasSteppedAttenuation(int rx)
	{
		switch (rx)
		{
		default:
			return false;
		case 1:
			return true;
		case 2:
		{
			HPSDRModel model = _model;
			if ((uint)(model - 1) <= 4u || model == HPSDRModel.ANAN_G2E)
			{
				return false;
			}
			return true;
		}
		}
	}
}
