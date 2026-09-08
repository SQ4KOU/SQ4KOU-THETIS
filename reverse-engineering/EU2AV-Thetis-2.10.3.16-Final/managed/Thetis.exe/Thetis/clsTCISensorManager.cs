namespace Thetis;

public sealed class clsTCISensorManager
{
	private sealed class clsRxReadingState
	{
		public double Signal = -200.0;

		public double AvgSignal = -200.0;

		public double PeakBinSignal = -200.0;

		public bool Updated;
	}

	private sealed class clsTxReadingState
	{
		public double MicLevelDbm = -200.0;

		public double PowerWatts;

		public double PeakPowerWatts;

		public double Swr = 1.0;

		public bool Updated;
	}

	private readonly object _lock = new object();

	private readonly clsRxReadingState[,] _rxChannelReadings = new clsRxReadingState[2, 2];

	private readonly clsTxReadingState _txReadings = new clsTxReadingState();

	private bool _rxSensorsEnabled;

	private bool _txSensorsEnabled;

	private int _rxIntervalMs = 200;

	private int _txIntervalMs = 200;

	public int RxIntervalMs
	{
		get
		{
			lock (_lock)
			{
				return _rxIntervalMs;
			}
		}
	}

	public int TxIntervalMs
	{
		get
		{
			lock (_lock)
			{
				return _txIntervalMs;
			}
		}
	}

	public bool RxSensorsEnabled
	{
		get
		{
			lock (_lock)
			{
				return _rxSensorsEnabled;
			}
		}
	}

	public bool TxSensorsEnabled
	{
		get
		{
			lock (_lock)
			{
				return _txSensorsEnabled;
			}
		}
	}

	public clsTCISensorManager()
	{
		for (int i = 0; i < 2; i++)
		{
			for (int j = 0; j < 2; j++)
			{
				_rxChannelReadings[i, j] = new clsRxReadingState();
			}
		}
	}

	private static int clampIntervalMs(int intervalMs)
	{
		if (intervalMs < 30)
		{
			return 30;
		}
		if (intervalMs > 1000)
		{
			return 1000;
		}
		return intervalMs;
	}

	public void ConfigureRxSensors(bool enabled, int intervalMs)
	{
		lock (_lock)
		{
			_rxSensorsEnabled = enabled;
			_rxIntervalMs = clampIntervalMs(intervalMs);
			for (int i = 0; i < 2; i++)
			{
				for (int j = 0; j < 2; j++)
				{
					_rxChannelReadings[i, j].Updated = false;
				}
			}
		}
	}

	public void ConfigureTxSensors(bool enabled, int intervalMs)
	{
		lock (_lock)
		{
			_txSensorsEnabled = enabled;
			_txIntervalMs = clampIntervalMs(intervalMs);
			_txReadings.Updated = false;
		}
	}

	public bool RequiresRxChannelUpdate(int receiver, int channel)
	{
		lock (_lock)
		{
			if (!_rxSensorsEnabled)
			{
				return false;
			}
			if (receiver < 0 || receiver > 1 || channel < 0 || channel > 1)
			{
				return false;
			}
			return !_rxChannelReadings[receiver, channel].Updated;
		}
	}

	public bool RequiresTxUpdate()
	{
		lock (_lock)
		{
			return _txSensorsEnabled && !_txReadings.Updated;
		}
	}

	public bool SensorRequiresUpdate(int receiver, Reading reading)
	{
		lock (_lock)
		{
			switch (reading)
			{
			case Reading.SIGNAL_STRENGTH:
			case Reading.AVG_SIGNAL_STRENGTH:
			case Reading.SIGNAL_MAX_BIN:
			{
				int num = receiver - 1;
				if (num < 0 || num > 1)
				{
					return false;
				}
				clsRxReadingState clsRxReadingState2 = _rxChannelReadings[num, 0];
				return _rxSensorsEnabled && !clsRxReadingState2.Updated;
			}
			case Reading.MIC:
			case Reading.PWR:
			case Reading.SWR:
				return _txSensorsEnabled && !_txReadings.Updated;
			default:
				return false;
			}
		}
	}

	public void SetRxChannelReading(int receiver, int channel, double signal, double avg_signal, double peak_bin_signal)
	{
		lock (_lock)
		{
			if (receiver >= 0 && receiver <= 1 && channel >= 0 && channel <= 1)
			{
				clsRxReadingState obj = _rxChannelReadings[receiver, channel];
				obj.Signal = signal;
				obj.AvgSignal = avg_signal;
				obj.PeakBinSignal = peak_bin_signal;
				obj.Updated = true;
			}
		}
	}

	public void SetTxReadings(double micLevelDbm, double powerWatts, double peakPowerWatts, double swr)
	{
		lock (_lock)
		{
			_txReadings.MicLevelDbm = micLevelDbm;
			_txReadings.PowerWatts = powerWatts;
			_txReadings.PeakPowerWatts = peakPowerWatts;
			_txReadings.Swr = swr;
			_txReadings.Updated = true;
		}
	}

	public bool TryGetRxChannelReadingForSend(int receiver, int channel, out double signal, out double avg_signal, out double peak_bin_signal)
	{
		lock (_lock)
		{
			signal = -200.0;
			avg_signal = -200.0;
			peak_bin_signal = -200.0;
			if (!_rxSensorsEnabled)
			{
				return false;
			}
			if (receiver < 0 || receiver > 1 || channel < 0 || channel > 1)
			{
				return false;
			}
			clsRxReadingState clsRxReadingState2 = _rxChannelReadings[receiver, channel];
			if (!clsRxReadingState2.Updated)
			{
				return false;
			}
			signal = clsRxReadingState2.Signal;
			avg_signal = clsRxReadingState2.AvgSignal;
			peak_bin_signal = clsRxReadingState2.PeakBinSignal;
			return true;
		}
	}

	public void ConsumeRxChannelReading(int receiver, int channel)
	{
		lock (_lock)
		{
			if (receiver >= 0 && receiver <= 1 && channel >= 0 && channel <= 1)
			{
				_rxChannelReadings[receiver, channel].Updated = false;
			}
		}
	}

	public bool TryGetTxReadingsForSend(out double micLevelDbm, out double powerWatts, out double peakPowerWatts, out double swr)
	{
		lock (_lock)
		{
			micLevelDbm = -200.0;
			powerWatts = 0.0;
			peakPowerWatts = 0.0;
			swr = 1.0;
			if (!_txSensorsEnabled || !_txReadings.Updated)
			{
				return false;
			}
			micLevelDbm = _txReadings.MicLevelDbm;
			powerWatts = _txReadings.PowerWatts;
			peakPowerWatts = _txReadings.PeakPowerWatts;
			swr = _txReadings.Swr;
			return true;
		}
	}

	public void ConsumeTxReadings()
	{
		lock (_lock)
		{
			_txReadings.Updated = false;
		}
	}
}
