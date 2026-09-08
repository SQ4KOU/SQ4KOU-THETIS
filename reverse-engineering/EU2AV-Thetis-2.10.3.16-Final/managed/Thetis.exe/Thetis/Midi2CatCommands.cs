using System;
using Midi2Cat;
using Midi2Cat.Data;
using Midi2Cat.IO;

namespace Thetis;

public class Midi2CatCommands
{
	private CATParser parser;

	private CATCommands commands;

	private MidiMessageManager midiManager;

	private Console console;

	private int _old_rit = -100;

	private int msgs_since_reversal;

	private int current_tuning_direction;

	private bool _swapVFOWheels;

	private int _old_mic_gain = -100;

	private int _old_cpdr_level = -100;

	public string Midi2CatDbFile
	{
		get
		{
			if (midiManager != null)
			{
				return midiManager.DbFile;
			}
			return null;
		}
	}

	public bool SwapVFOWheelsProperty
	{
		get
		{
			return _swapVFOWheels;
		}
		set
		{
			_swapVFOWheels = value;
		}
	}

	public Midi2CatCommands(Console c)
	{
		console = c;
		midiManager = new MidiMessageManager(this, c.AppDataPath + "midi2cat.xml");
		parser = new CATParser(c);
		commands = new CATCommands(c, parser);
		OpenMidi2Cat();
	}

	public void OpenMidi2Cat()
	{
		midiManager.Open();
	}

	public void CloseMidi2Cat()
	{
		midiManager.Close();
	}

	public void SendUpdateToMidi(CatCmd cmd, double pct)
	{
		midiManager.SendUpdateToMidi(cmd, pct);
	}

	public CmdState MultiRxOnOff(int msg, MidiDevice device)
	{
		parser.nSet = 1;
		parser.nGet = 0;
		if (msg == 127)
		{
			switch ((int)Convert.ToInt16(commands.ZZMU("")))
			{
			case 0:
				commands.ZZMU("1");
				return CmdState.On;
			case 1:
				commands.ZZMU("0");
				return CmdState.Off;
			}
		}
		return CmdState.NoChange;
	}

	public void Rx1ModeNext(int msg, MidiDevice device)
	{
		parser.nSet = 2;
		parser.nGet = 0;
		int num = Convert.ToInt16(commands.ZZMD(""));
		if (num < 11 && msg == 127)
		{
			commands.ZZMD((num + 1).ToString("00"));
		}
	}

	public void Rx1ModePrev(int msg, MidiDevice device)
	{
		parser.nSet = 2;
		parser.nGet = 0;
		int num = Convert.ToInt16(commands.ZZMD(""));
		if (num > 0 && msg == 127)
		{
			commands.ZZMD((num - 1).ToString("00"));
		}
	}

	public void Rx1FilterWider(int msg, MidiDevice device)
	{
		parser.nSet = 2;
		parser.nGet = 0;
		int num = Convert.ToInt16(commands.ZZFI(""));
		if (num > 0 && msg == 127)
		{
			commands.ZZFI((num - 1).ToString("00"));
		}
	}

	public void Rx1FilterNarrower(int msg, MidiDevice device)
	{
		parser.nSet = 2;
		parser.nGet = 0;
		int num = Convert.ToInt16(commands.ZZFI(""));
		if (num < 14 && msg == 127)
		{
			commands.ZZFI((num + 1).ToString("00"));
		}
	}

	public void VfoAtoB(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			console.CATVFOAtoB();
		}
	}

	public void VfoBtoA(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			console.CATVFOBtoA();
		}
	}

	public void VfoSwap(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			console.CATVFOABSwap();
		}
	}

	public void XIT(int msg, MidiDevice device)
	{
		parser.nSet = 5;
		parser.nGet = 0;
		if ((msg < 64) & (msg >= 0))
		{
			commands.ZZXF((-1280 + msg * 20).ToString("0000"));
		}
		if ((msg >= 64) & (msg <= 127))
		{
			commands.ZZXF("+" + ((msg - 64) * 20).ToString("0000"));
		}
	}

	public void RIT(int msg, MidiDevice device)
	{
		parser.nSet = 5;
		parser.nGet = 0;
		int num = (-64 + msg) * (Common.ShiftKeyDown ? 4 : 20);
		if (num != _old_rit)
		{
			commands.ZZRF(((num >= 0) ? "+" : "") + num.ToString("0000"));
			_old_rit = num;
		}
	}

	private bool IsBehringerCMD(MidiDevice device)
	{
		if (device.GetDeviceName().Contains("CMD"))
		{
			return true;
		}
		return false;
	}

	public void RIT_inc(int msg, MidiDevice device)
	{
		parser.nSet = 2;
		parser.nGet = 0;
		device.GetDeviceName();
		if (IsBehringerCMD(device))
		{
			if (msg == 127 || msg <= 1)
			{
				commands.ZZRC();
			}
			else if (msg < 64)
			{
				commands.ZZRD("");
			}
			else if (msg > 64)
			{
				commands.ZZRU("");
			}
		}
		else
		{
			if (msg == 127)
			{
				commands.ZZRD("");
			}
			if (msg == 1)
			{
				commands.ZZRU("");
			}
		}
	}

	public void XIT_inc(int msg, MidiDevice device)
	{
		parser.nSet = 5;
		parser.nGet = 0;
		parser.nAns = 5;
		long num = Convert.ToInt32(commands.ZZXF(""));
		Convert.ToInt16(commands.ZZMD(""));
		device.GetDeviceName();
		if (IsBehringerCMD(device))
		{
			if (msg == 127 || msg <= 1)
			{
				commands.ZZXC();
				return;
			}
			if (msg < 64 && num > -99995)
			{
				num -= 10;
				if (num < 0)
				{
					commands.ZZXF(num.ToString("D4"));
				}
				if (num >= 0)
				{
					commands.ZZXF("+" + num.ToString("D4"));
				}
			}
			if (msg > 64 && num < 99995)
			{
				num += 10;
				if (num < 0)
				{
					commands.ZZXF(num.ToString("D4"));
				}
				if (num >= 0)
				{
					commands.ZZXF("+" + num.ToString("D4"));
				}
			}
			return;
		}
		if (msg == 127 && num > -99995)
		{
			num -= 10;
			if (num < 0)
			{
				commands.ZZXF(num.ToString("D4"));
			}
			if (num >= 0)
			{
				commands.ZZXF("+" + num.ToString("D4"));
			}
		}
		if (msg == 1 && num < 99995)
		{
			num += 10;
			if (num < 0)
			{
				commands.ZZXF(num.ToString("D4"));
			}
			if (num >= 0)
			{
				commands.ZZXF("+" + num.ToString("D4"));
			}
		}
	}

	public void RIT_clear(int msg, MidiDevice device)
	{
		parser.nSet = 0;
		parser.nGet = 0;
		if (msg == 127)
		{
			commands.ZZRC();
		}
	}

	public void XIT_clear(int msg, MidiDevice device)
	{
		parser.nSet = 0;
		parser.nGet = 0;
		if (msg == 127)
		{
			commands.ZZXC();
		}
	}

	public void TuningStepUp(int msg, MidiDevice device)
	{
		parser.nSet = 2;
		parser.nGet = 0;
		if (Convert.ToInt16(commands.ZZAC("")) < 26 && msg == 127)
		{
			commands.ZZSU();
		}
	}

	public void TuningStepDown(int msg, MidiDevice device)
	{
		parser.nSet = 2;
		parser.nGet = 0;
		if (Convert.ToInt16(commands.ZZAC("")) > 0 && msg == 127)
		{
			commands.ZZSD();
		}
	}

	public void VolumeVfoA(int msg, MidiDevice device)
	{
		parser.nSet = 3;
		parser.nGet = 0;
		try
		{
			commands.ZZLA(((double)msg * 0.787).ToString("000"));
		}
		catch
		{
		}
	}

	public void VolumeVfoA_inc(int msg, MidiDevice device)
	{
		parser.nSet = 3;
		parser.nGet = 0;
		try
		{
			int num = int.Parse(commands.ZZLA(""));
			if (msg != 127 && msg != 0)
			{
				commands.ZZLA((num + (msg - 64)).ToString("000"));
			}
		}
		catch
		{
		}
	}

	public void VolumeVfoB(int msg, MidiDevice device)
	{
		parser.nSet = 3;
		parser.nGet = 0;
		try
		{
			commands.ZZLC(((double)msg * 0.787).ToString("000"));
		}
		catch
		{
		}
	}

	public void VolumeVfoB_inc(int msg, MidiDevice device)
	{
		parser.nSet = 3;
		parser.nGet = 0;
		try
		{
			int num = int.Parse(commands.ZZLE(""));
			if (msg != 127 && msg != 0)
			{
				commands.ZZLE((num + (msg - 64)).ToString("000"));
			}
		}
		catch
		{
		}
	}

	public void RX2Volume(int msg, MidiDevice device)
	{
		parser.nSet = 3;
		parser.nGet = 0;
		try
		{
			commands.ZZLE(((double)msg * 0.787).ToString("000"));
		}
		catch
		{
		}
	}

	public void RX2Pan(int msg, MidiDevice device)
	{
		parser.nSet = 3;
		parser.nGet = 0;
		try
		{
			commands.ZZLF(((double)msg * 0.787).ToString("000"));
		}
		catch
		{
		}
	}

	public void FilterBandwidth(int msg, MidiDevice device)
	{
		parser.nSet = 5;
		parser.nGet = 0;
		int num = Convert.ToInt16(commands.ZZIS(""));
		switch (msg)
		{
		case 1:
			commands.ZZIS((num + 50).ToString("00000"));
			break;
		case 127:
			commands.ZZIS((num - 50).ToString("00000"));
			break;
		}
	}

	public void FilterShift(int msg, MidiDevice device)
	{
		int num = (int)(((double)msg / 1.27 - 50.0) * 20.0);
		parser.nSet = 5;
		parser.nGet = 0;
		if (num < 0)
		{
			commands.ZZIT(num.ToString("0000"));
		}
		if (num >= 0)
		{
			commands.ZZIT("+" + num.ToString("0000"));
		}
	}

	public void RatioMainSubRx(int msg, MidiDevice device)
	{
		parser.nSet = 3;
		parser.nGet = 0;
		try
		{
			double num = (double)msg * 0.787;
			commands.ZZLB(num.ToString("000"));
			commands.ZZLD((100.0 - num).ToString("000"));
		}
		catch
		{
		}
	}

	public CmdState AutoNotchOnOff(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 1;
			switch ((int)Convert.ToInt16(commands.ZZNT("")))
			{
			case 0:
				commands.ZZNT("1");
				return CmdState.On;
			case 1:
				commands.ZZNT("0");
				return CmdState.Off;
			}
		}
		return CmdState.NoChange;
	}

	public CmdState Rx1NoiseBlanker1OnOff(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 1;
			switch ((int)Convert.ToInt16(commands.ZZNA("")))
			{
			case 0:
				commands.ZZNA("1");
				return CmdState.On;
			case 1:
				commands.ZZNA("0");
				return CmdState.Off;
			}
		}
		return CmdState.NoChange;
	}

	public CmdState Rx2NoiseBlanker1OnOff(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 1;
			try
			{
				switch ((int)Convert.ToInt16(commands.ZZNC("")))
				{
				case 0:
					commands.ZZNC("1");
					return CmdState.On;
				case 1:
					commands.ZZNC("0");
					return CmdState.Off;
				}
			}
			catch
			{
				return CmdState.NoChange;
			}
		}
		return CmdState.NoChange;
	}

	public CmdState Rx1Noiseblanker2OnOff(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 1;
			try
			{
				switch ((int)Convert.ToInt16(commands.ZZNB("")))
				{
				case 0:
					commands.ZZNB("1");
					return CmdState.On;
				case 1:
					commands.ZZNB("0");
					return CmdState.Off;
				}
			}
			catch
			{
				return CmdState.NoChange;
			}
		}
		return CmdState.NoChange;
	}

	public CmdState Rx2Noiseblanker2OnOff(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 1;
			try
			{
				switch ((int)Convert.ToInt16(commands.ZZND("")))
				{
				case 0:
					commands.ZZND("1");
					return CmdState.On;
				case 1:
					commands.ZZND("0");
					return CmdState.Off;
				}
			}
			catch
			{
				return CmdState.NoChange;
			}
		}
		return CmdState.NoChange;
	}

	public CmdState LockVFOOnOff(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 1;
			switch ((int)Convert.ToInt16(commands.ZZVL("")))
			{
			case 0:
				commands.ZZVL("1");
				return CmdState.On;
			case 1:
				commands.ZZVL("0");
				return CmdState.Off;
			}
		}
		return CmdState.NoChange;
	}

	public CmdState LockVFOAOnOff(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 1;
			switch ((int)Convert.ToInt16(commands.ZZUX("")))
			{
			case 0:
				commands.ZZUX("1");
				return CmdState.On;
			case 1:
				commands.ZZUX("0");
				return CmdState.Off;
			}
		}
		return CmdState.NoChange;
	}

	public CmdState LockVFOBOnOff(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 1;
			switch ((int)Convert.ToInt16(commands.ZZUY("")))
			{
			case 0:
				commands.ZZUY("1");
				return CmdState.On;
			case 1:
				commands.ZZUY("0");
				return CmdState.Off;
			}
		}
		return CmdState.NoChange;
	}

	public CmdState RitOnOff(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 1;
			switch ((int)Convert.ToInt16(commands.ZZRT("")))
			{
			case 0:
				commands.ZZRT("1");
				return CmdState.On;
			case 1:
				commands.ZZRT("0");
				return CmdState.Off;
			}
		}
		return CmdState.NoChange;
	}

	public CmdState XitOnOff(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 1;
			switch ((int)Convert.ToInt16(commands.ZZXS("")))
			{
			case 0:
				commands.ZZXS("1");
				return CmdState.On;
			case 1:
				commands.ZZXS("0");
				return CmdState.Off;
			}
		}
		return CmdState.NoChange;
	}

	public void SetAFGain(int msg, MidiDevice device)
	{
		int num = (int)((double)msg / 1.27);
		parser.nSet = 3;
		commands.ZZAG(num.ToString("000"));
	}

	public CmdState DiversityFormOpen(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 1;
			switch ((int)Convert.ToInt16(commands.ZZDF("")))
			{
			case 0:
				commands.ZZDF("1");
				return CmdState.On;
			case 1:
				commands.ZZDF("0");
				return CmdState.Off;
			}
		}
		return CmdState.NoChange;
	}

	public CmdState DiversityEnable(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 1;
			switch ((int)Convert.ToInt16(commands.ZZDE("")))
			{
			case 0:
				commands.ZZDE("1");
				return CmdState.On;
			case 1:
				commands.ZZDE("0");
				return CmdState.Off;
			}
		}
		return CmdState.NoChange;
	}

	public void DiversityPhase(int msg, MidiDevice device)
	{
		parser.nGet = 0;
		parser.nSet = 6;
		parser.nAns = 6;
		try
		{
			double num = 18000.0;
			double num2 = -18000.0;
			int num3 = Convert.ToInt32(commands.ZZDD(""));
			if ((msg < 64) & (msg >= 0))
			{
				if ((double)num3 > num2)
				{
					num3 -= 500;
				}
			}
			else if (((msg >= 64) & (msg <= 127)) && (double)num3 < num)
			{
				num3 += 500;
			}
			if (num3 >= 18000)
			{
				num3 -= 36000;
			}
			else if (num3 <= -18000)
			{
				num3 += 36000;
			}
			commands.ZZDD(num3.ToString("+00000;-00000;000000"));
		}
		catch
		{
		}
	}

	public void DiversityGain(int msg, MidiDevice device)
	{
		parser.nGet = 0;
		parser.nSet = 4;
		parser.nAns = 4;
		try
		{
			double num = 1000.0;
			double num2 = 0.0;
			int num3 = Convert.ToInt32(commands.ZZDG(""));
			if (num3 > 1000)
			{
				num3 = 1000;
			}
			if (num3 < 0)
			{
				num3 = 0;
			}
			if ((msg < 64) & (msg >= 0))
			{
				if ((double)num3 > num2)
				{
					num3 -= 10;
				}
			}
			else if (((msg >= 64) & (msg <= 127)) && (double)num3 < num)
			{
				num3 += 10;
			}
			commands.ZZDG(num3.ToString("0000"));
		}
		catch
		{
		}
	}

	public CmdState DiversityReference(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 1;
			switch ((int)Convert.ToInt16(commands.ZZDB("")))
			{
			case 0:
				commands.ZZDB("1");
				return CmdState.On;
			case 1:
				commands.ZZDB("0");
				return CmdState.Off;
			}
		}
		return CmdState.NoChange;
	}

	public CmdState DiversitySource(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 1;
			switch ((int)Convert.ToInt16(commands.ZZDH("")))
			{
			case 0:
				commands.ZZDH("1");
				return CmdState.On;
			case 1:
				commands.ZZDH("2");
				return CmdState.Off;
			case 2:
				commands.ZZDH("0");
				return CmdState.On;
			}
		}
		return CmdState.NoChange;
	}

	public int StringToFreq(string s)
	{
		int result = 0;
		switch ((int)Convert.ToInt16(s))
		{
		case 0:
			result = 1;
			break;
		case 1:
			result = 2;
			break;
		case 2:
			result = 10;
			break;
		case 3:
			result = 25;
			break;
		case 4:
			result = 50;
			break;
		case 5:
			result = 100;
			break;
		case 6:
			result = 250;
			break;
		case 7:
			result = 500;
			break;
		case 8:
			result = 1000;
			break;
		case 9:
			result = 2000;
			break;
		case 10:
			result = 2500;
			break;
		case 11:
			result = 5000;
			break;
		case 12:
			result = 6250;
			break;
		case 13:
			result = 9000;
			break;
		case 14:
			result = 10000;
			break;
		case 15:
			result = 12500;
			break;
		case 16:
			result = 15000;
			break;
		case 17:
			result = 20000;
			break;
		case 18:
			result = 25000;
			break;
		case 19:
			result = 30000;
			break;
		case 20:
			result = 50000;
			break;
		case 21:
			result = 100000;
			break;
		case 22:
			result = 250000;
			break;
		case 23:
			result = 500000;
			break;
		case 24:
			result = 1000000;
			break;
		case 25:
			result = 10000000;
			break;
		}
		return result;
	}

	public void MidiMessagesPerTuneStepUp(int msg, MidiDevice device)
	{
		if (msg >= 126)
		{
			console.CATMidiMessagesPerTuneStepUp();
		}
	}

	public void MidiMessagesPerTuneStepDown(int msg, MidiDevice device)
	{
		if (msg >= 126)
		{
			console.CATMidiMessagesPerTuneStepDown();
		}
	}

	public CmdState MidiMessagesPerTuneStepToggle(int msg, MidiDevice device)
	{
		if (msg >= 126)
		{
			CmdState cmdState = CmdState.Off;
			int midiMessagesPerTuneStep = console.MidiMessagesPerTuneStep;
			if (midiMessagesPerTuneStep == console.MinMIDIMessagesPerTuneStep)
			{
				cmdState = CmdState.On;
			}
			else
			{
				if (midiMessagesPerTuneStep != console.MaxMIDIMessagesPerTuneStep)
				{
					console.MidiMessagesPerTuneStep = console.MinMIDIMessagesPerTuneStep;
					return CmdState.Off;
				}
				cmdState = CmdState.Off;
			}
			console.CATMidiMessagesPerTuneStepToggle();
			return cmdState;
		}
		return CmdState.NoChange;
	}

	private void ProcessStdMIDIWheelAsVFO(int direction, int step, bool round_to_step_size, long freq, int mode, bool is_vfo_a)
	{
		Func<Func<string>, string> func = (Func<string> f) => (!console.InvokeRequired) ? f() : ((string)console.Invoke((Func<string>)(() => f())));
		Action<string> send_frequency_raw = (is_vfo_a ? ((Action<string>)delegate(string s)
		{
			commands.ZZFA(s);
		}) : ((Action<string>)delegate(string s)
		{
			commands.ZZFB(s);
		}));
		Action<string> action = delegate(string s)
		{
			if (console.InvokeRequired)
			{
				console.BeginInvoke((Action)delegate
				{
					send_frequency_raw(s);
				});
			}
			else
			{
				send_frequency_raw(s);
			}
		};
		string text = func(() => (!is_vfo_a) ? commands.ZZRB(string.Empty) : commands.ZZRA(string.Empty));
		bool flag = text.Length > 0 && text[0] == '1';
		int midiMessagesPerTuneStep = console.MidiMessagesPerTuneStep;
		if (current_tuning_direction == 0)
		{
			current_tuning_direction = ((direction > 125) ? 1 : (-1));
			msgs_since_reversal = 0;
		}
		bool flag2 = direction > 125;
		bool flag3 = direction < 3;
		if (!flag2 && !flag3)
		{
			return;
		}
		if ((flag2 && current_tuning_direction == 1) || (flag3 && current_tuning_direction == -1))
		{
			if (++msgs_since_reversal < midiMessagesPerTuneStep)
			{
				return;
			}
			msgs_since_reversal = 0;
		}
		else
		{
			current_tuning_direction = -current_tuning_direction;
			msgs_since_reversal = 1;
			if (msgs_since_reversal < midiMessagesPerTuneStep)
			{
				return;
			}
		}
		int num_steps = ((!flag2) ? 1 : (-1));
		long num = freq;
		int result = 0;
		int num2 = 0;
		if (flag && (mode == 7 || mode == 9))
		{
			if (!int.TryParse((mode == 7) ? func(() => commands.ZZRH(string.Empty)) : func(() => commands.ZZRL(string.Empty)), out result))
			{
				return;
			}
			num2 = ((mode != 7) ? 1 : (-1));
			num += num2 * result;
		}
		long num3 = SnapTune(num, step, num_steps, round_to_step_size);
		action((num3 - num2 * result).ToString("D11"));
	}

	private void ProcessBehringerMainWheelAsVFO(int direction, int step, bool RoundToStepSize, long freq, int mode, string vfo, string deviceName)
	{
		int num = 1;
		if (current_tuning_direction == 0)
		{
			msgs_since_reversal = 0;
			if (direction > 64)
			{
				current_tuning_direction = 1;
			}
			else
			{
				current_tuning_direction = -1;
			}
		}
		if ((direction > 64 && current_tuning_direction == 1) || (direction < 64 && current_tuning_direction == -1))
		{
			msgs_since_reversal++;
			int midiMessagesPerTuneStep = console.MidiMessagesPerTuneStep;
			if (msgs_since_reversal < midiMessagesPerTuneStep)
			{
				return;
			}
			msgs_since_reversal = 0;
		}
		else
		{
			msgs_since_reversal = 1;
			current_tuning_direction = -current_tuning_direction;
			int midiMessagesPerTuneStep2 = console.MidiMessagesPerTuneStep;
			if (msgs_since_reversal < midiMessagesPerTuneStep2)
			{
				return;
			}
		}
		if (deviceName == "CMD PL-1")
		{
			if ((direction <= 58 && direction >= 10) || (direction >= 70 && direction <= 117))
			{
				num = 3;
			}
			if ((direction <= 57 && direction >= 10) || (direction >= 71 && direction <= 117))
			{
				num = 7;
			}
			if ((direction <= 56 && direction >= 10) || (direction >= 72 && direction <= 117))
			{
				num = 11;
			}
			if ((direction <= 52 && direction >= 10) || (direction >= 76 && direction <= 117))
			{
				num = 200;
			}
		}
		else if (deviceName == "CMD Micro")
		{
			if ((direction <= 62 && direction >= 10) || (direction >= 66 && direction <= 117))
			{
				num = 3;
			}
			if ((direction <= 61 && direction >= 10) || (direction >= 67 && direction <= 117))
			{
				num = 7;
			}
			if ((direction <= 60 && direction >= 10) || (direction >= 68 && direction <= 117))
			{
				num = 11;
			}
			if ((direction <= 58 && direction >= 10) || (direction >= 70 && direction <= 117))
			{
				num = 200;
			}
		}
		else
		{
			if (!deviceName.Contains("CMD"))
			{
				return;
			}
			if ((direction <= 62 && direction >= 10) || (direction >= 66 && direction <= 117))
			{
				num = 3;
			}
			if ((direction <= 61 && direction >= 10) || (direction >= 67 && direction <= 117))
			{
				num = 7;
			}
			if ((direction <= 60 && direction >= 10) || (direction >= 68 && direction <= 117))
			{
				num = 11;
			}
			if ((direction <= 58 && direction >= 10) || (direction >= 70 && direction <= 117))
			{
				num = 200;
			}
		}
		int num2 = ((!(vfo == "a")) ? Convert.ToInt16(commands.ZZRB("")) : Convert.ToInt16(commands.ZZRA("")));
		switch (mode)
		{
		case 7:
			if (num2 == 1)
			{
				int num3 = Convert.ToInt16(commands.ZZRH(""));
				if (direction < 64 && direction >= 25)
				{
					freq -= num3;
					long num4 = SnapTune(freq, step, -1 * num, RoundToStepSize) + num3;
					if (vfo == "a")
					{
						commands.ZZFA(num4.ToString("D11"));
					}
					else
					{
						commands.ZZFB(num4.ToString("D11"));
					}
				}
				if (direction > 64 && direction <= 105)
				{
					freq -= num3;
					long num5 = SnapTune(freq, step, num, RoundToStepSize) + num3;
					if (vfo == "a")
					{
						commands.ZZFA(num5.ToString("D11"));
					}
					else
					{
						commands.ZZFB(num5.ToString("D11"));
					}
				}
				return;
			}
			if (direction < 64 && direction >= 25)
			{
				if (vfo == "a")
				{
					commands.ZZFA(SnapTune(freq, step, -1 * num, RoundToStepSize).ToString("D11"));
				}
				else
				{
					commands.ZZFB(SnapTune(freq, step, -1 * num, RoundToStepSize).ToString("D11"));
				}
			}
			if (direction > 64 && direction <= 105)
			{
				if (vfo == "a")
				{
					commands.ZZFA(SnapTune(freq, step, num, RoundToStepSize).ToString("D11"));
				}
				else
				{
					commands.ZZFB(SnapTune(freq, step, num, RoundToStepSize).ToString("D11"));
				}
			}
			return;
		case 9:
			if (num2 == 1)
			{
				int num6 = Convert.ToInt16(commands.ZZRL(""));
				if (direction < 64 && direction >= 25)
				{
					freq += num6;
					long num7 = SnapTune(freq, step, -1 * num, RoundToStepSize) - num6;
					if (vfo == "a")
					{
						commands.ZZFA(num7.ToString("D11"));
					}
					else
					{
						commands.ZZFB(num7.ToString("D11"));
					}
				}
				if (direction > 64 && direction <= 105)
				{
					freq += num6;
					long num8 = SnapTune(freq, step, num, RoundToStepSize) - num6;
					if (vfo == "a")
					{
						commands.ZZFA(num8.ToString("D11"));
					}
					else
					{
						commands.ZZFB(num8.ToString("D11"));
					}
				}
				return;
			}
			if (direction < 64 && direction >= 25)
			{
				if (vfo == "a")
				{
					commands.ZZFA(SnapTune(freq, step, -1 * num, RoundToStepSize).ToString("D11"));
				}
				else
				{
					commands.ZZFB(SnapTune(freq, step, -1 * num, RoundToStepSize).ToString("D11"));
				}
			}
			if (direction > 64 && direction <= 105)
			{
				if (vfo == "a")
				{
					commands.ZZFA(SnapTune(freq, step, num, RoundToStepSize).ToString("D11"));
				}
				else
				{
					commands.ZZFB(SnapTune(freq, step, num, RoundToStepSize).ToString("D11"));
				}
			}
			return;
		}
		if (direction < 64 && direction >= 25)
		{
			if (vfo == "a")
			{
				commands.ZZFA(SnapTune(freq, step, -1 * num, RoundToStepSize).ToString("D11"));
			}
			else
			{
				commands.ZZFB(SnapTune(freq, step, -1 * num, RoundToStepSize).ToString("D11"));
			}
		}
		if (direction > 64 && direction <= 105)
		{
			if (vfo == "a")
			{
				commands.ZZFA(SnapTune(freq, step, num, RoundToStepSize).ToString("D11"));
			}
			else
			{
				commands.ZZFB(SnapTune(freq, step, num, RoundToStepSize).ToString("D11"));
			}
		}
	}

	public void ChangeFreqVfoA(int msg, MidiDevice device)
	{
		parser.nSet = 2;
		parser.nGet = 0;
		int step = StringToFreq(commands.ZZAC(""));
		ChangeFreqVfoA(msg, step, RoundToStepSize: true, device);
	}

	private void ChangeFreqVfoA(int direction, int step, bool RoundToStepSize, MidiDevice device)
	{
		parser.nGet = 0;
		parser.nSet = 11;
		bool flag;
		long freq;
		if (_swapVFOWheels)
		{
			flag = false;
			freq = Convert.ToInt64(commands.ZZFB(""));
		}
		else
		{
			flag = true;
			freq = Convert.ToInt64(commands.ZZFA(""));
		}
		int mode = Convert.ToInt16(commands.ZZMD(""));
		commands.isMidi = true;
		string deviceName = device.GetDeviceName();
		if (!string.IsNullOrEmpty(deviceName) && deviceName.Contains("CMD", StringComparison.Ordinal))
		{
			string vfo = (flag ? "a" : "b");
			if (deviceName == "CMD PL-1" || deviceName == "CMD Micro")
			{
				ProcessBehringerMainWheelAsVFO(direction, step, RoundToStepSize, freq, mode, vfo, deviceName);
			}
			else
			{
				ProcessBehringerMainWheelAsVFO(direction, step, RoundToStepSize, freq, mode, vfo, "CMD");
			}
		}
		else
		{
			ProcessStdMIDIWheelAsVFO(direction, step, RoundToStepSize, freq, mode, flag);
		}
		commands.isMidi = false;
	}

	public long SnapTune(long freq, int step, int num_steps, bool RoundToStepSize)
	{
		if (step == 0)
		{
			return freq;
		}
		if (RoundToStepSize)
		{
			long num;
			try
			{
				num = freq / step;
			}
			catch
			{
				return freq;
			}
			if (num_steps < 0 && freq % step != 0L)
			{
				num_steps++;
			}
			num += num_steps;
			freq = num * step;
			return freq;
		}
		if (num_steps > 0)
		{
			return freq + step;
		}
		return freq - step;
	}

	public void ChangeFreqVfoB(int msg, MidiDevice device)
	{
		bool flag = true;
		parser.nGet = 0;
		parser.nSet = 2;
		if (!int.TryParse(commands.ZZMD(""), out var result))
		{
			return;
		}
		int step = StringToFreq(commands.ZZAC(""));
		parser.nSet = 11;
		bool flag2;
		long freq;
		if (_swapVFOWheels)
		{
			flag2 = true;
			freq = Convert.ToInt64(commands.ZZFA(""));
		}
		else
		{
			flag2 = false;
			freq = Convert.ToInt64(commands.ZZFB(""));
		}
		commands.isMidi = true;
		string deviceName = device.GetDeviceName();
		if (!string.IsNullOrEmpty(deviceName) && deviceName.Contains("CMD", StringComparison.Ordinal))
		{
			string vfo = (flag2 ? "a" : "b");
			if (deviceName == "CMD PL-1" || deviceName == "CMD Micro")
			{
				ProcessBehringerMainWheelAsVFO(msg, step, flag, freq, result, vfo, deviceName);
			}
			else
			{
				ProcessBehringerMainWheelAsVFO(msg, step, flag, freq, result, vfo, "CMD");
			}
		}
		else
		{
			ProcessStdMIDIWheelAsVFO(msg, step, flag, freq, result, flag2);
		}
		commands.isMidi2 = false;
	}

	public CmdState BinauralOnOff(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 1;
			switch ((int)Convert.ToInt16(commands.ZZBI("")))
			{
			case 0:
				commands.ZZBI("1");
				return CmdState.On;
			case 1:
				commands.ZZBI("0");
				return CmdState.Off;
			}
		}
		return CmdState.NoChange;
	}

	public CmdState MuteOnOff(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 1;
			switch ((int)Convert.ToInt16(commands.ZZMA("")))
			{
			case 0:
				commands.ZZMA("1");
				return CmdState.On;
			case 1:
				commands.ZZMA("0");
				return CmdState.Off;
			}
		}
		return CmdState.NoChange;
	}

	public CmdState SpurReductionOnOff(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 1;
			switch ((int)Convert.ToInt16(commands.ZZSR("")))
			{
			case 0:
				commands.ZZSR("1");
				return CmdState.On;
			case 1:
				commands.ZZSR("0");
				return CmdState.Off;
			}
		}
		return CmdState.NoChange;
	}

	public void NoiseReduction4Amount(int msg, MidiDevice device)
	{
		parser.nSet = 3;
		parser.nGet = 0;
		try
		{
			commands.ZZNG(((double)((float)msg * 0.78740156f)).ToString("000"));
		}
		catch
		{
		}
	}

	public CmdState NoiseReductionOnOff(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 1;
			int num = Convert.ToInt16(commands.ZZNE(""));
			if (num != 1)
			{
				commands.ZZNR("1");
				return CmdState.On;
			}
			if (num == 1)
			{
				commands.ZZNR("0");
				return CmdState.Off;
			}
		}
		return CmdState.NoChange;
	}

	public CmdState NoiseReduction2OnOff(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 1;
			int num = Convert.ToInt16(commands.ZZNE(""));
			if (num != 2)
			{
				commands.ZZNS("1");
				return CmdState.On;
			}
			if (num == 2)
			{
				commands.ZZNS("0");
				return CmdState.Off;
			}
		}
		return CmdState.NoChange;
	}

	public CmdState NoiseReduction3OnOff(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 1;
			int num = Convert.ToInt16(commands.ZZNE(""));
			if (num != 3)
			{
				commands.ZZNE("3");
				return CmdState.On;
			}
			if (num == 3)
			{
				commands.ZZNE("0");
				return CmdState.Off;
			}
		}
		return CmdState.NoChange;
	}

	public CmdState NoiseReduction4OnOff(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 1;
			int num = Convert.ToInt16(commands.ZZNE(""));
			if (num != 4)
			{
				commands.ZZNE("4");
				return CmdState.On;
			}
			if (num == 4)
			{
				commands.ZZNE("0");
				return CmdState.Off;
			}
		}
		return CmdState.NoChange;
	}

	public void Rx2NoiseReduction4Amount(int msg, MidiDevice device)
	{
		parser.nSet = 3;
		parser.nGet = 0;
		try
		{
			commands.ZZNH(((double)((float)msg * 0.78740156f)).ToString("000"));
		}
		catch
		{
		}
	}

	public CmdState Rx2NoiseReductionOnOff(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 1;
			int num = Convert.ToInt16(commands.ZZNF(""));
			if (num != 1)
			{
				commands.ZZNV("1");
				return CmdState.On;
			}
			if (num == 1)
			{
				commands.ZZNV("0");
				return CmdState.Off;
			}
		}
		return CmdState.NoChange;
	}

	public CmdState Rx2NoiseReduction2OnOff(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 1;
			int num = Convert.ToInt16(commands.ZZNF(""));
			if (num != 2)
			{
				commands.ZZNW("1");
				return CmdState.On;
			}
			if (num == 2)
			{
				commands.ZZNW("0");
				return CmdState.Off;
			}
		}
		return CmdState.NoChange;
	}

	public CmdState Rx2NoiseReduction3OnOff(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 1;
			int num = Convert.ToInt16(commands.ZZNF(""));
			if (num != 3)
			{
				commands.ZZNF("3");
				return CmdState.On;
			}
			if (num == 3)
			{
				commands.ZZNF("0");
				return CmdState.Off;
			}
		}
		return CmdState.NoChange;
	}

	public CmdState Rx2NoiseReduction4OnOff(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 1;
			int num = Convert.ToInt16(commands.ZZNF(""));
			if (num != 4)
			{
				commands.ZZNF("4");
				return CmdState.On;
			}
			if (num == 4)
			{
				commands.ZZNF("0");
				return CmdState.Off;
			}
		}
		return CmdState.NoChange;
	}

	public CmdState Rx2PreAmpOnOff(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 1;
			try
			{
				switch ((int)Convert.ToInt16(commands.ZZPB("")))
				{
				case 0:
					commands.ZZPB("1");
					return CmdState.On;
				case 1:
					commands.ZZPB("0");
					return CmdState.Off;
				}
			}
			catch
			{
				return CmdState.NoChange;
			}
		}
		return CmdState.NoChange;
	}

	public CmdState VfoSyncOnOff(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 1;
			switch ((int)Convert.ToInt16(commands.ZZSY("")))
			{
			case 0:
				commands.ZZSY("1");
				return CmdState.On;
			case 1:
				commands.ZZSY("0");
				return CmdState.Off;
			}
		}
		return CmdState.NoChange;
	}

	public CmdState SplitOnOff(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 1;
			switch ((int)Convert.ToInt16(commands.ZZSP("")))
			{
			case 0:
				commands.ZZSP("1");
				return CmdState.On;
			case 1:
				commands.ZZSP("0");
				return CmdState.Off;
			}
		}
		return CmdState.NoChange;
	}

	public CmdState MOXOnOff(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 1;
			switch ((int)Convert.ToInt16(commands.ZZTX("")))
			{
			case 0:
				commands.ZZTX("1");
				return CmdState.On;
			case 1:
				commands.ZZTX("0");
				return CmdState.Off;
			}
		}
		return CmdState.NoChange;
	}

	public CmdState VOXOnOff(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 1;
			switch ((int)Convert.ToInt16(commands.ZZVE("")))
			{
			case 0:
				commands.ZZVE("1");
				return CmdState.On;
			case 1:
				commands.ZZVE("0");
				return CmdState.Off;
			}
		}
		return CmdState.NoChange;
	}

	public CmdState CompanderOnOff(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 1;
			switch ((int)Convert.ToInt16(commands.ZZCP("")))
			{
			case 0:
				commands.ZZCP("1");
				return CmdState.On;
			case 1:
				commands.ZZCP("0");
				return CmdState.Off;
			}
		}
		return CmdState.NoChange;
	}

	public CmdState StereoDiversityOnOff(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 1;
			switch ((int)Convert.ToInt16(commands.ZZDX("")))
			{
			case 0:
				commands.ZZDX("1");
				return CmdState.On;
			case 1:
				commands.ZZDX("0");
				return CmdState.Off;
			}
		}
		return CmdState.NoChange;
	}

	public CmdState DEXPOnOff(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 1;
			switch ((int)Convert.ToInt16(commands.ZZGE("")))
			{
			case 0:
				commands.ZZGE("1");
				return CmdState.On;
			case 1:
				commands.ZZGE("0");
				return CmdState.Off;
			}
		}
		return CmdState.NoChange;
	}

	public CmdState RX2OnOff(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 1;
			try
			{
				switch ((int)Convert.ToInt16(commands.ZZRS("")))
				{
				case 0:
					commands.ZZRS("1");
					return CmdState.On;
				case 1:
					commands.ZZRS("0");
					return CmdState.Off;
				}
			}
			catch
			{
				return CmdState.NoChange;
			}
		}
		return CmdState.NoChange;
	}

	public CmdState StartOnOff(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 2;
			try
			{
				int num = Convert.ToInt16(commands.ZZPS(""));
				parser.nSet = 1;
				switch (num)
				{
				case 0:
					commands.ZZPS("1");
					return CmdState.On;
				case 1:
					commands.ZZPS("0");
					return CmdState.Off;
				}
			}
			catch
			{
				return CmdState.NoChange;
			}
		}
		return CmdState.NoChange;
	}

	public CmdState TunerOnOff(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 1;
			try
			{
				switch ((int)Convert.ToInt16(commands.ZZOV("")))
				{
				case 0:
					commands.ZZOV("1");
					return CmdState.On;
				case 1:
					commands.ZZOV("0");
					return CmdState.Off;
				}
			}
			catch
			{
				return CmdState.NoChange;
			}
		}
		return CmdState.NoChange;
	}

	public CmdState TunOnOff(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 1;
			try
			{
				switch ((int)Convert.ToInt16(commands.ZZTU("")))
				{
				case 0:
					commands.ZZTU("1");
					return CmdState.On;
				case 1:
					commands.ZZTU("0");
					return CmdState.Off;
				}
			}
			catch
			{
				return CmdState.NoChange;
			}
		}
		return CmdState.NoChange;
	}

	public CmdState TwoToneOnOff(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 1;
			try
			{
				switch ((int)Convert.ToInt16(commands.ZZUT("")))
				{
				case 0:
					commands.ZZUT("1");
					return CmdState.On;
				case 1:
					commands.ZZUT("0");
					return CmdState.Off;
				}
			}
			catch
			{
				return CmdState.NoChange;
			}
		}
		return CmdState.NoChange;
	}

	public CmdState TunerBypassOnOff(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 1;
			try
			{
				switch ((int)Convert.ToInt16(commands.ZZOW("")))
				{
				case 0:
					commands.ZZOW("1");
					return CmdState.On;
				case 1:
					commands.ZZOW("0");
					return CmdState.Off;
				}
			}
			catch
			{
				return CmdState.NoChange;
			}
		}
		return CmdState.NoChange;
	}

	public void ZeroBeatPress(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 1;
			commands.ZZZB();
		}
	}

	public void BandUp(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 0;
			try
			{
				commands.ZZBU();
			}
			catch
			{
			}
		}
	}

	public void BandDown(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 0;
			try
			{
				commands.ZZBD();
			}
			catch
			{
			}
		}
	}

	public void Rx2BandUp(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 0;
			try
			{
				commands.ZZBA();
			}
			catch
			{
			}
		}
	}

	public void Rx2BandDown(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 0;
			try
			{
				commands.ZZBB();
			}
			catch
			{
			}
		}
	}

	public void PreAmpSettingsKnob(int msg, MidiDevice device)
	{
		parser.nGet = 0;
		parser.nSet = 1;
		try
		{
			switch (Convert.ToInt16(commands.ZZFM()))
			{
			case 0:
				if (msg >= 0 && msg < 64)
				{
					commands.ZZPA("1");
				}
				else if (msg >= 64 && msg < 128)
				{
					commands.ZZPA("0");
				}
				break;
			case 1:
				if (msg >= 0 && msg < 16)
				{
					commands.ZZPA("1");
				}
				else if (msg >= 16 && msg < 32)
				{
					commands.ZZPA("0");
				}
				else if (msg >= 32 && msg < 48)
				{
					commands.ZZPA("2");
				}
				else if (msg >= 48 && msg < 64)
				{
					commands.ZZPA("3");
				}
				else if (msg >= 64 && msg < 80)
				{
					commands.ZZPA("4");
				}
				else if (msg >= 80 && msg < 96)
				{
					commands.ZZPA("5");
				}
				else if (msg >= 112 && msg < 128)
				{
					commands.ZZPA("6");
				}
				break;
			case 2:
				if (msg >= 0 && msg < 32)
				{
					commands.ZZPA("0");
				}
				else if (msg >= 32 && msg < 64)
				{
					commands.ZZPA("1");
				}
				else if (msg >= 64 && msg < 96)
				{
					commands.ZZPA("2");
				}
				else if (msg >= 96 && msg < 128)
				{
					commands.ZZPA("3");
				}
				break;
			case 3:
				if (msg >= 0 && msg < 25)
				{
					commands.ZZPA("0");
				}
				else if (msg >= 25 && msg < 51)
				{
					commands.ZZPA("1");
				}
				else if (msg >= 51 && msg < 77)
				{
					commands.ZZPA("2");
				}
				else if (msg >= 77 && msg < 102)
				{
					commands.ZZPA("3");
				}
				else if (msg >= 102 && msg < 128)
				{
					commands.ZZPA("4");
				}
				break;
			}
		}
		catch
		{
		}
	}

	public CmdState CWBreakIn(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 1;
			switch ((int)Convert.ToInt16(commands.ZZCB("")))
			{
			case 0:
				commands.ZZCB("1");
				return CmdState.On;
			case 1:
				commands.ZZCB("0");
				return CmdState.Off;
			}
		}
		return CmdState.NoChange;
	}

	public CmdState CWQSK(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 1;
			switch ((int)Convert.ToInt16(commands.ZZQK("")))
			{
			case 0:
				commands.ZZQK("1");
				return CmdState.On;
			case 1:
				commands.ZZQK("0");
				return CmdState.Off;
			}
		}
		return CmdState.NoChange;
	}

	public void CWSpeed(int msg, MidiDevice device)
	{
		parser.nGet = 0;
		parser.nSet = 2;
		try
		{
			if (msg != 0)
			{
				double num = (double)msg / 2.1333 + 1.0;
				num.ToString("00");
				commands.ZZCS(num.ToString("00"));
			}
		}
		catch
		{
		}
	}

	public void CWSpeed_inc(int msg, MidiDevice device)
	{
		parser.nGet = 0;
		parser.nSet = 2;
		try
		{
			if (msg == 63 || msg == 65)
			{
				int num = Convert.ToInt32(commands.ZZCS(""));
				if (msg == 65 && num < 60)
				{
					num++;
				}
				if (msg == 63 && num > 1)
				{
					num--;
				}
				commands.ZZCS(num.ToString("00"));
			}
		}
		catch
		{
		}
	}

	public void APFFreq(int msg, MidiDevice device)
	{
		parser.nGet = 0;
		parser.nSet = 1;
		try
		{
			double num = (double)msg * 3.937 - 250.0;
			if (num >= 0.0)
			{
				commands.ZZAT(num.ToString("000"));
			}
			if (num < 0.0)
			{
				commands.ZZAT(num.ToString("000"));
			}
		}
		catch
		{
		}
	}

	public void APFBandwidth(int msg, MidiDevice device)
	{
		parser.nGet = 0;
		parser.nSet = 3;
		try
		{
			commands.ZZAB(((double)msg * 1.1023 + 10.0).ToString("000"));
		}
		catch
		{
		}
	}

	public void APFGain(int msg, MidiDevice device)
	{
		parser.nGet = 0;
		parser.nSet = 4;
		try
		{
			commands.ZZAA("+" + ((double)msg * 0.7874 + 0.0).ToString("000"));
		}
		catch
		{
		}
	}

	public void AGCLevel(int msg, MidiDevice device)
	{
		parser.nGet = 0;
		parser.nSet = 4;
		try
		{
			device.GetDeviceName();
			double num = ((!IsBehringerCMD(device)) ? ((double)msg * 1.099 - 20.0) : ((double)(127 - msg) * 1.099 - 20.0));
			if (num >= 0.0)
			{
				commands.ZZAR("+" + num.ToString("000"));
			}
			if (num < 0.0)
			{
				commands.ZZAR(num.ToString("000"));
			}
		}
		catch
		{
		}
	}

	public void AGCLevel_inc(int msg, MidiDevice device)
	{
		parser.nGet = 0;
		parser.nSet = 4;
		parser.nAns = 4;
		try
		{
			double num = 120.0;
			double num2 = -20.0;
			int num3 = Convert.ToInt32(commands.ZZAR(""));
			if (msg == 127 || msg == 0)
			{
				return;
			}
			if (msg < 64)
			{
				if ((double)num3 > num2)
				{
					num3--;
				}
			}
			else if (msg > 64 && (double)num3 < num)
			{
				num3++;
			}
			commands.ZZAR("+" + num3.ToString("000"));
			double num4 = Convert.ToDouble(num3);
			int num5 = Convert.ToInt32(15.0 * (num4 - num2) / (num - num2));
			if (num5 < 1)
			{
				num5 = 1;
			}
			if (num5 > 15)
			{
				num5 = 15;
			}
		}
		catch
		{
		}
	}

	public void RX2AGCLevel(int msg, MidiDevice device)
	{
		parser.nGet = 0;
		parser.nSet = 4;
		try
		{
			device.GetDeviceName();
			double num = ((!IsBehringerCMD(device)) ? ((double)msg * 1.099 - 20.0) : ((double)(127 - msg) * 1.099 - 20.0));
			if (num >= 0.0)
			{
				commands.ZZAS("+" + num.ToString("000"));
			}
			if (num < 0.0)
			{
				commands.ZZAS(num.ToString("000"));
			}
		}
		catch
		{
		}
	}

	public void RX2AGCLevel_inc(int msg, MidiDevice device)
	{
		parser.nGet = 0;
		parser.nSet = 4;
		parser.nAns = 4;
		try
		{
			double num = 120.0;
			double num2 = -20.0;
			int num3 = Convert.ToInt32(commands.ZZAS(""));
			if (msg == 127 || msg == 0)
			{
				return;
			}
			if (msg < 64)
			{
				if ((double)num3 > num2)
				{
					num3--;
				}
			}
			else if (msg > 64 && (double)num3 < num)
			{
				num3++;
			}
			commands.ZZAS("+" + num3.ToString("000"));
			double num4 = Convert.ToDouble(num3);
			int num5 = Convert.ToInt32(15.0 * (num4 - num2) / (num - num2));
			if (num5 < 1)
			{
				num5 = 1;
			}
			if (num5 > 15)
			{
				num5 = 15;
			}
		}
		catch
		{
		}
	}

	public void MicGain(int msg, MidiDevice device)
	{
		parser.nGet = 0;
		parser.nSet = 3;
		parser.nAns = 3;
		try
		{
			int micGainMin = console.MicGainMin;
			int num = console.MicGainMax - micGainMin;
			float num2 = (float)msg / 127f;
			int num3 = micGainMin + (int)((float)num * num2);
			if (num3 != _old_mic_gain)
			{
				commands.ZZMG(num3.ToString("000"));
				_old_mic_gain = num3;
			}
		}
		catch
		{
		}
	}

	public void SquelchControl(int msg, MidiDevice device)
	{
		parser.nSet = 0;
		parser.nSet = 3;
		try
		{
			commands.ZZSQ((160.0 - (double)msg * 1.26).ToString("000"));
		}
		catch
		{
		}
	}

	public void CPDRLevel(int msg, MidiDevice device)
	{
		parser.nSet = 0;
		parser.nSet = 2;
		try
		{
			int cPDRMin = console.CPDRMin;
			int num = console.CPDRMax - cPDRMin;
			float num2 = (float)msg / 127f;
			int num3 = cPDRMin + (int)((float)num * num2);
			if (num3 != _old_cpdr_level)
			{
				commands.ZZCT(num3.ToString("00"));
				_old_cpdr_level = num3;
			}
		}
		catch
		{
		}
	}

	public void VOXGain(int msg, MidiDevice device)
	{
		parser.nSet = 0;
		parser.nSet = 4;
		try
		{
			commands.ZZVG(((double)msg * 7.89).ToString("0000"));
		}
		catch
		{
		}
	}

	public void DEXPThreshold(int msg, MidiDevice device)
	{
		parser.nSet = 0;
		parser.nSet = 4;
		try
		{
			double num = -160.0 + (double)msg * 1.26;
			if (num < 0.0)
			{
				commands.ZZGL(num.ToString("000"));
			}
			else
			{
				commands.ZZGL(num.ToString("0000"));
			}
		}
		catch
		{
		}
	}

	public void TXAFMonitor(int msg, MidiDevice device)
	{
		parser.nSet = 3;
		parser.nGet = 0;
		try
		{
			commands.ZZTM(((double)msg * 0.787).ToString("000"));
		}
		catch
		{
		}
	}

	public void DriveLevel(int msg, MidiDevice device)
	{
		parser.nSet = 3;
		parser.nGet = 0;
		try
		{
			commands.ZZPC(((double)msg * 0.787).ToString("000"));
		}
		catch
		{
		}
	}

	public void DriveLevel_inc(int msg, MidiDevice device)
	{
		parser.nGet = 0;
		parser.nSet = 3;
		parser.nAns = 4;
		try
		{
			double num = 100.0;
			double num2 = 0.0;
			int num3 = Convert.ToInt32(commands.ZZPC(""));
			if (msg == 127 || msg == 0)
			{
				return;
			}
			if (msg < 64)
			{
				if ((double)num3 > num2)
				{
					num3--;
				}
			}
			else if (msg > 64 && (double)num3 < num)
			{
				num3++;
			}
			commands.ZZPC(num3.ToString("000"));
			double num4 = Convert.ToDouble(num3);
			int num5 = Convert.ToInt32(15.0 * (num4 - num2) / (num - num2));
			if (num5 < 1)
			{
				num5 = 1;
			}
			if (num5 > 15)
			{
				num5 = 15;
			}
		}
		catch
		{
		}
	}

	public CmdState RXEQOnOff(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 1;
			switch ((int)Convert.ToInt16(commands.ZZER("")))
			{
			case 0:
				commands.ZZER("1");
				return CmdState.On;
			case 1:
				commands.ZZER("0");
				return CmdState.Off;
			}
		}
		return CmdState.NoChange;
	}

	public CmdState TXEQOnOff(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 1;
			switch ((int)Convert.ToInt16(commands.ZZET("")))
			{
			case 0:
				commands.ZZET("1");
				return CmdState.On;
			case 1:
				commands.ZZET("0");
				return CmdState.Off;
			}
		}
		return CmdState.NoChange;
	}

	public CmdState SquelchOnOff(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 1;
			try
			{
				switch ((int)Convert.ToInt16(commands.ZZSO("")))
				{
				case 0:
					commands.ZZSO("1");
					return CmdState.On;
				case 1:
					commands.ZZSO("0");
					return CmdState.Off;
				}
			}
			catch
			{
				return CmdState.NoChange;
			}
		}
		return CmdState.NoChange;
	}

	public void AGCModeKnob(int msg, MidiDevice device)
	{
		parser.nGet = 0;
		parser.nSet = 1;
		if (msg >= 0 && msg < 22)
		{
			commands.ZZGT("0");
		}
		else if (msg >= 22 && msg < 43)
		{
			commands.ZZGT("1");
		}
		else if (msg >= 43 && msg < 64)
		{
			commands.ZZGT("2");
		}
		else if (msg >= 64 && msg < 85)
		{
			commands.ZZGT("3");
		}
		else if (msg >= 85 && msg < 106)
		{
			commands.ZZGT("4");
		}
		else if (msg >= 106 && msg < 128)
		{
			commands.ZZGT("5");
		}
	}

	public void AGCModeUp(int msg, MidiDevice device)
	{
		parser.nGet = 0;
		parser.nSet = 1;
		if (msg != 127)
		{
			return;
		}
		try
		{
			int num = Convert.ToInt16(commands.ZZGT(""));
			if (num > 0 && num <= 5)
			{
				commands.ZZGT((num - 1).ToString("0"));
			}
		}
		catch
		{
		}
	}

	public void AGCModeDown(int msg, MidiDevice device)
	{
		parser.nGet = 0;
		parser.nSet = 1;
		if (msg != 127)
		{
			return;
		}
		try
		{
			int num = Convert.ToInt16(commands.ZZGT(""));
			if (num >= 0 && num < 5)
			{
				commands.ZZGT((num + 1).ToString("0"));
			}
		}
		catch
		{
		}
	}

	public void PreampFlex5000(int msg, MidiDevice device)
	{
		parser.nGet = 0;
		parser.nSet = 1;
		if (msg != 127)
		{
			return;
		}
		try
		{
			if (commands.ZZFM() == "1")
			{
				if (commands.ZZPA("") == "0")
				{
					commands.ZZPA("1");
				}
				else if (commands.ZZPA("") == "1")
				{
					commands.ZZPA("0");
				}
			}
		}
		catch
		{
		}
	}

	public void DisplayAverage(int msg, MidiDevice device)
	{
		if (msg != 127)
		{
			return;
		}
		parser.nGet = 0;
		parser.nSet = 1;
		try
		{
			switch ((int)Convert.ToInt16(commands.ZZDA("")))
			{
			case 0:
				commands.ZZDA("1");
				break;
			case 1:
				commands.ZZDA("0");
				break;
			}
		}
		catch
		{
		}
	}

	public void DisplayPeak(int msg, MidiDevice device)
	{
		if (msg != 127)
		{
			return;
		}
		parser.nGet = 0;
		parser.nSet = 1;
		try
		{
			switch ((int)Convert.ToInt16(commands.ZZPO("")))
			{
			case 0:
				commands.ZZPO("1");
				break;
			case 1:
				commands.ZZPO("0");
				break;
			}
		}
		catch
		{
		}
	}

	public void DisplayTxFilter(int msg, MidiDevice device)
	{
		if (msg != 127)
		{
			return;
		}
		parser.nGet = 0;
		parser.nSet = 1;
		try
		{
			switch ((int)Convert.ToInt16(commands.ZZTF("")))
			{
			case 0:
				commands.ZZTF("1");
				break;
			case 1:
				commands.ZZTF("0");
				break;
			}
		}
		catch
		{
		}
	}

	public CmdState VACOnOff(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 1;
			try
			{
				switch ((int)Convert.ToInt16(commands.ZZVA("")))
				{
				case 0:
					commands.ZZVA("1");
					return CmdState.On;
				case 1:
					commands.ZZVA("0");
					return CmdState.Off;
				}
			}
			catch
			{
				return CmdState.NoChange;
			}
		}
		return CmdState.NoChange;
	}

	public CmdState VAC2OnOff(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 1;
			try
			{
				switch ((int)Convert.ToInt16(commands.ZZVK("")))
				{
				case 0:
					commands.ZZVK("1");
					return CmdState.On;
				case 1:
					commands.ZZVK("0");
					return CmdState.Off;
				}
			}
			catch
			{
				return CmdState.NoChange;
			}
		}
		return CmdState.NoChange;
	}

	public void IQtoVAC(int msg, MidiDevice device)
	{
		if (msg != 127)
		{
			return;
		}
		parser.nGet = 0;
		parser.nSet = 1;
		try
		{
			switch ((int)Convert.ToInt16(commands.ZZVH("")))
			{
			case 0:
				commands.ZZVH("1");
				break;
			case 1:
				commands.ZZVH("0");
				break;
			}
		}
		catch
		{
		}
	}

	public void IQtoVACRX2(int msg, MidiDevice device)
	{
		if (msg != 127)
		{
			return;
		}
		parser.nGet = 0;
		parser.nSet = 1;
		try
		{
			int num = Convert.ToInt16(commands.ZZVJ(""));
			Convert.ToInt16(commands.ZZVH(""));
			switch (num)
			{
			case 0:
				commands.ZZVH("1");
				commands.ZZVJ("1");
				break;
			case 1:
				commands.ZZVJ("0");
				break;
			}
		}
		catch
		{
		}
	}

	public void DisplayModePrev(int msg, MidiDevice device)
	{
		if (msg != 127)
		{
			return;
		}
		parser.nGet = 0;
		parser.nSet = 1;
		try
		{
			int num = Convert.ToInt16(commands.ZZDM(""));
			if (num > 0 && num <= 7)
			{
				commands.ZZDM((num - 1).ToString("0"));
			}
			else if (num == 9)
			{
				commands.ZZDM(6.ToString("0"));
			}
		}
		catch
		{
		}
	}

	public void DisplayModeNext(int msg, MidiDevice device)
	{
		if (msg != 127)
		{
			return;
		}
		parser.nGet = 0;
		parser.nSet = 1;
		try
		{
			int num = Convert.ToInt16(commands.ZZDM(""));
			if (num >= 0 && num < 7)
			{
				commands.ZZDM((num + 1).ToString("0"));
			}
		}
		catch
		{
		}
	}

	public void ZoomDec(int msg, MidiDevice device)
	{
		if (msg != 127)
		{
			return;
		}
		parser.nGet = 0;
		parser.nSet = 1;
		try
		{
			Convert.ToInt16(commands.ZZPZ(""));
			int num = Convert.ToInt16(commands.ZZPY(""));
			if (num >= 51 && num <= 150)
			{
				commands.ZZPZ("0");
			}
			else if (num >= 151 && num <= 200)
			{
				commands.ZZPZ("1");
			}
			else if (num >= 201 && num <= 225)
			{
				commands.ZZPZ("2");
			}
			else if (num >= 226 && num <= 240)
			{
				commands.ZZPZ("3");
			}
		}
		catch
		{
		}
	}

	public void ZoomInc(int msg, MidiDevice device)
	{
		if (msg != 127)
		{
			return;
		}
		parser.nGet = 0;
		parser.nSet = 1;
		try
		{
			Convert.ToInt16(commands.ZZPZ(""));
			int num = Convert.ToInt16(commands.ZZPY(""));
			if (num >= 10 && num <= 49)
			{
				commands.ZZPZ("0");
			}
			else if (num >= 50 && num <= 149)
			{
				commands.ZZPZ("1");
			}
			else if (num >= 150 && num <= 199)
			{
				commands.ZZPZ("2");
			}
			else if (num >= 200 && num <= 225)
			{
				commands.ZZPZ("3");
			}
		}
		catch
		{
		}
	}

	public void ZoomSliderInc(int msg, MidiDevice device)
	{
		parser.nGet = 0;
		parser.nSet = 3;
		try
		{
			int num = Convert.ToInt16(commands.ZZPY(""));
			if (msg == 127 && num >= 15)
			{
				commands.ZZPY((num - 5).ToString("000"));
			}
			else if (msg == 1 && num <= 235)
			{
				commands.ZZPY((num + 5).ToString("000"));
			}
		}
		catch
		{
		}
	}

	public void PanSliderInc(int msg, MidiDevice device)
	{
		parser.nGet = 0;
		parser.nSet = 4;
		try
		{
			int num = Convert.ToInt16(commands.ZZPE(""));
			if (msg == 127 && num >= 50)
			{
				commands.ZZPE((num - 25).ToString("0000"));
			}
			else if (msg == 1 && num <= 235)
			{
				commands.ZZPE((num + 25).ToString("0000"));
			}
		}
		catch
		{
		}
	}

	public void PanSlider(int msg, MidiDevice device)
	{
		try
		{
			parser.nSet = 4;
			parser.nGet = 0;
			commands.ZZPE(((double)msg * 7.87).ToString("0000"));
		}
		catch
		{
		}
	}

	public CmdState SpectralNoiseBlankerOnOff(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 1;
			switch ((int)Convert.ToInt16(commands.ZZNN("")))
			{
			case 0:
				commands.ZZNN("1");
				return CmdState.On;
			case 1:
				commands.ZZNN("0");
				return CmdState.Off;
			}
		}
		return CmdState.NoChange;
	}

	public CmdState SpectralNoiseBlankerRx2OnOff(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 1;
			switch ((int)Convert.ToInt16(commands.ZZNO("")))
			{
			case 0:
				commands.ZZNO("1");
				return CmdState.On;
			case 1:
				commands.ZZNO("0");
				return CmdState.Off;
			}
		}
		return CmdState.NoChange;
	}

	public void QuickModeSave(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 1;
			try
			{
				commands.ZZQS();
			}
			catch
			{
			}
		}
	}

	public void CWXMacro1(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 1;
			try
			{
				commands.ZZKM("1");
			}
			catch
			{
			}
		}
	}

	public void CWXMacro2(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 1;
			try
			{
				commands.ZZKM("2");
			}
			catch
			{
			}
		}
	}

	public void CWXMacro3(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 1;
			try
			{
				commands.ZZKM("3");
			}
			catch
			{
			}
		}
	}

	public void CWXMacro4(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 1;
			try
			{
				commands.ZZKM("4");
			}
			catch
			{
			}
		}
	}

	public void CWXMacro5(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 1;
			try
			{
				commands.ZZKM("5");
			}
			catch
			{
			}
		}
	}

	public void CWXMacro6(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 1;
			try
			{
				commands.ZZKM("6");
			}
			catch
			{
			}
		}
	}

	public void CWXMacro7(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 1;
			try
			{
				commands.ZZKM("7");
			}
			catch
			{
			}
		}
	}

	public void CWXMacro8(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 1;
			try
			{
				commands.ZZKM("8");
			}
			catch
			{
			}
		}
	}

	public void CWXMacro9(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 1;
			try
			{
				commands.ZZKM("9");
			}
			catch
			{
			}
		}
	}

	public void CWXStop(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 1;
			try
			{
				commands.ZZSS();
			}
			catch
			{
			}
		}
	}

	public CmdState MONOnOff(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 1;
			switch ((int)Convert.ToInt16(commands.ZZMO("")))
			{
			case 0:
				commands.ZZMO("1");
				return CmdState.On;
			case 1:
				commands.ZZMO("0");
				return CmdState.Off;
			}
		}
		return CmdState.NoChange;
	}

	public void PanCenter(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 1;
			try
			{
				commands.ZZPD();
			}
			catch
			{
			}
		}
	}

	public void QuickModeRestore(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 1;
			try
			{
				commands.ZZQR();
			}
			catch
			{
			}
		}
	}

	public void ZoomSliderFix(int msg, MidiDevice device)
	{
		try
		{
			parser.nSet = 3;
			parser.nGet = 0;
			commands.ZZPY(((double)msg * 1.88).ToString("000"));
		}
		catch
		{
		}
	}

	public void FilterHigh(int msg, MidiDevice device)
	{
		int num = 20;
		device.GetDeviceName();
		if (IsBehringerCMD(device))
		{
			if (msg == 63)
			{
				msg = 127;
			}
			if (msg == 65)
			{
				msg = 1;
			}
		}
		try
		{
			parser.nGet = 0;
			parser.nSet = 2;
			int num2 = Convert.ToInt32(commands.ZZMD(""));
			num = ((num2 != 3 && num2 != 4 && num2 != 7 && num2 != 8 && num2 != 9) ? 50 : 20);
		}
		catch
		{
			return;
		}
		try
		{
			parser.nSet = 5;
			parser.nGet = 0;
			parser.nAns = 5;
			int num3 = Convert.ToInt32(commands.ZZFH(""));
			if (msg == 1 && num3 >= 0)
			{
				commands.ZZFH((num3 + num).ToString("00000"));
			}
			else if (msg == 1 && num3 < 0)
			{
				if (num3 > -num - 1)
				{
					commands.ZZFH((num3 + num).ToString("00000"));
				}
				else
				{
					commands.ZZFH((num3 + num).ToString("0000"));
				}
			}
			else if (msg == 127 && num3 >= 0)
			{
				if (num3 < num)
				{
					commands.ZZFH((num3 - num).ToString("0000"));
				}
				else
				{
					commands.ZZFH((num3 - num).ToString("00000"));
				}
			}
			else if (msg == 127 && num3 < 0)
			{
				commands.ZZFH((num3 - num).ToString("0000"));
			}
		}
		catch
		{
		}
	}

	public void FilterLow(int msg, MidiDevice device)
	{
		int num = 20;
		device.GetDeviceName();
		if (IsBehringerCMD(device))
		{
			if (msg == 63)
			{
				msg = 127;
			}
			if (msg == 65)
			{
				msg = 1;
			}
		}
		try
		{
			parser.nGet = 0;
			parser.nSet = 2;
			int num2 = Convert.ToInt32(commands.ZZMD(""));
			if (num2 != 3 && num2 != 4 && num2 != 7 && num2 != 8 && num2 != 9)
			{
				num = 50;
			}
		}
		catch
		{
			return;
		}
		try
		{
			parser.nSet = 5;
			parser.nGet = 0;
			parser.nAns = 5;
			int num3 = Convert.ToInt32(commands.ZZFL(""));
			if (msg == 1 && num3 >= 0)
			{
				commands.ZZFL((num3 + num).ToString("00000"));
			}
			else if (msg == 1 && num3 < 0)
			{
				if (num3 > -num - 1)
				{
					commands.ZZFL((num3 + num).ToString("00000"));
				}
				else
				{
					commands.ZZFL((num3 + num).ToString("0000"));
				}
			}
			else if (msg == 127 && num3 >= 0)
			{
				if (num3 < num)
				{
					commands.ZZFL((num3 - num).ToString("0000"));
				}
				else
				{
					commands.ZZFL((num3 - num).ToString("00000"));
				}
			}
			else if (msg == 127 && num3 < 0)
			{
				commands.ZZFL((num3 - num).ToString("0000"));
			}
		}
		catch
		{
		}
	}

	public void VACGainRX(int msg, MidiDevice device)
	{
		parser.nGet = 0;
		parser.nSet = 3;
		try
		{
			commands.ZZVB(((int)((double)(msg - 63) * 0.64)).ToString("000;-00;000"));
		}
		catch
		{
		}
	}

	public void VACGainTX(int msg, MidiDevice device)
	{
		parser.nGet = 0;
		parser.nSet = 3;
		try
		{
			commands.ZZVC(((int)((double)(msg - 63) * 0.64)).ToString("000;-00;000"));
		}
		catch
		{
		}
	}

	public void VAC2GainRX(int msg, MidiDevice device)
	{
		parser.nGet = 0;
		parser.nSet = 3;
		try
		{
			commands.ZZVW(((int)((double)(msg - 63) * 0.64)).ToString("000;-00;000"));
		}
		catch
		{
		}
	}

	public void VAC2GainTX(int msg, MidiDevice device)
	{
		parser.nGet = 0;
		parser.nSet = 3;
		try
		{
			commands.ZZVX(((int)((double)(msg - 63) * 0.64)).ToString("000;-00;000"));
		}
		catch
		{
		}
	}

	public CmdState CTunOnOff(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 1;
			switch ((int)Convert.ToInt16(commands.ZZCN("")))
			{
			case 0:
				commands.ZZCN("1");
				return CmdState.On;
			case 1:
				commands.ZZCN("0");
				return CmdState.Off;
			}
		}
		return CmdState.NoChange;
	}

	public CmdState ESCFormOnOff(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 1;
			switch ((int)Convert.ToInt16(commands.ZZDF("")))
			{
			case 0:
				commands.ZZDF("1");
				return CmdState.On;
			case 1:
				commands.ZZDF("0");
				return CmdState.Off;
			}
		}
		return CmdState.NoChange;
	}

	public void WaterfallLowLimit(int msg, MidiDevice device)
	{
		parser.nGet = 0;
		parser.nSet = 4;
		try
		{
			int num = (int)((float)(msg - 63) * 3.125f);
			commands.ZZDN(num.ToString("0000;-000;0000"));
			commands.ZZDQ(num.ToString("0000;-000;0000"));
		}
		catch
		{
		}
	}

	public void WaterfallHighLimit(int msg, MidiDevice device)
	{
		parser.nGet = 0;
		parser.nSet = 4;
		try
		{
			int num = (int)((float)(msg - 63) * 3.125f);
			commands.ZZDO(num.ToString("0000;-000;0000"));
			commands.ZZDP(num.ToString("0000;-000;0000"));
		}
		catch
		{
		}
	}

	public CmdState MuteRX2OnOff(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 1;
			switch ((int)Convert.ToInt16(commands.ZZMB("")))
			{
			case 0:
				commands.ZZMB("1");
				return CmdState.On;
			case 1:
				commands.ZZMB("0");
				return CmdState.Off;
			}
		}
		return CmdState.NoChange;
	}

	public void Band160m(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 3;
			try
			{
				commands.ZZBS("160");
			}
			catch
			{
			}
		}
	}

	public void Band80m(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 3;
			try
			{
				commands.ZZBS("080");
			}
			catch
			{
			}
		}
	}

	public void Band60m(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 3;
			try
			{
				commands.ZZBS("060");
			}
			catch
			{
			}
		}
	}

	public void Band40m(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 3;
			try
			{
				commands.ZZBS("040");
			}
			catch
			{
			}
		}
	}

	public void Band30m(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 3;
			try
			{
				commands.ZZBS("030");
			}
			catch
			{
			}
		}
	}

	public void Band20m(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 3;
			try
			{
				commands.ZZBS("020");
			}
			catch
			{
			}
		}
	}

	public void Band17m(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 3;
			try
			{
				commands.ZZBS("017");
			}
			catch
			{
			}
		}
	}

	public void Band15m(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 3;
			try
			{
				commands.ZZBS("015");
			}
			catch
			{
			}
		}
	}

	public void Band12m(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 3;
			try
			{
				commands.ZZBS("012");
			}
			catch
			{
			}
		}
	}

	public void Band10m(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 3;
			try
			{
				commands.ZZBS("010");
			}
			catch
			{
			}
		}
	}

	public void Band6m(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 3;
			try
			{
				commands.ZZBS("006");
			}
			catch
			{
			}
		}
	}

	public void Band2m(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 3;
			try
			{
				commands.ZZBS("002");
			}
			catch
			{
			}
		}
	}

	public void Band160mRX2(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 3;
			try
			{
				commands.ZZBT("160");
			}
			catch
			{
			}
		}
	}

	public void Band80mRX2(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 3;
			try
			{
				commands.ZZBT("080");
			}
			catch
			{
			}
		}
	}

	public void Band60mRX2(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 3;
			try
			{
				commands.ZZBT("060");
			}
			catch
			{
			}
		}
	}

	public void Band40mRX2(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 3;
			try
			{
				commands.ZZBT("040");
			}
			catch
			{
			}
		}
	}

	public void Band30mRX2(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 3;
			try
			{
				commands.ZZBT("030");
			}
			catch
			{
			}
		}
	}

	public void Band20mRX2(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 3;
			try
			{
				commands.ZZBT("020");
			}
			catch
			{
			}
		}
	}

	public void Band17mRX2(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 3;
			try
			{
				commands.ZZBT("017");
			}
			catch
			{
			}
		}
	}

	public void Band15mRX2(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 3;
			try
			{
				commands.ZZBT("015");
			}
			catch
			{
			}
		}
	}

	public void Band12mRX2(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 3;
			try
			{
				commands.ZZBT("012");
			}
			catch
			{
			}
		}
	}

	public void Band10mRX2(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 3;
			try
			{
				commands.ZZBT("010");
			}
			catch
			{
			}
		}
	}

	public void Band6mRX2(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 3;
			try
			{
				commands.ZZBT("006");
			}
			catch
			{
			}
		}
	}

	public void Band2mRX2(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 3;
			try
			{
				commands.ZZBT("002");
			}
			catch
			{
			}
		}
	}

	public void ModeSSB(int msg, MidiDevice device)
	{
		if (msg != 127)
		{
			return;
		}
		parser.nGet = 0;
		parser.nSet = 2;
		parser.nAns = 3;
		int num = 0;
		try
		{
			num = Convert.ToInt16(commands.ZZBS(""));
		}
		catch
		{
			num = 0;
		}
		parser.nGet = 0;
		parser.nSet = 2;
		parser.nAns = 2;
		try
		{
			if (num >= 40)
			{
				commands.ZZMD("00");
			}
			if (num < 40)
			{
				commands.ZZMD("01");
			}
		}
		catch
		{
		}
	}

	public void ModeLSB(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 2;
			try
			{
				commands.ZZMD("00");
			}
			catch
			{
			}
		}
	}

	public void ModeUSB(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 2;
			try
			{
				commands.ZZMD("01");
			}
			catch
			{
			}
		}
	}

	public void ModeDSB(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 2;
			try
			{
				commands.ZZMD("02");
			}
			catch
			{
			}
		}
	}

	public void ModeCW(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 2;
			try
			{
				commands.ZZMD("01");
			}
			catch
			{
			}
		}
	}

	public void ModeCWL(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 2;
			try
			{
				commands.ZZMD("03");
			}
			catch
			{
			}
		}
	}

	public void ModeCWU(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 2;
			try
			{
				commands.ZZMD("04");
			}
			catch
			{
			}
		}
	}

	public void ModeFM(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 2;
			try
			{
				commands.ZZMD("05");
			}
			catch
			{
			}
		}
	}

	public void ModeAM(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 2;
			try
			{
				commands.ZZMD("06");
			}
			catch
			{
			}
		}
	}

	public void ModeDIGU(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 2;
			try
			{
				commands.ZZMD("07");
			}
			catch
			{
			}
		}
	}

	public void ModeSPEC(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 2;
			try
			{
				commands.ZZMD("08");
			}
			catch
			{
			}
		}
	}

	public void ModeDIGL(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 2;
			try
			{
				commands.ZZMD("09");
			}
			catch
			{
			}
		}
	}

	public void ModeSAM(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 2;
			try
			{
				commands.ZZMD("10");
			}
			catch
			{
			}
		}
	}

	public void ModeDRM(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 2;
			try
			{
				commands.ZZMD("11");
			}
			catch
			{
			}
		}
	}

	public void MoveVFOADown100Khz(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 2;
			try
			{
				commands.ZZAD("11");
			}
			catch
			{
			}
		}
	}

	public void MoveVFOAUp100Khz(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 2;
			try
			{
				commands.ZZAU("11");
			}
			catch
			{
			}
		}
	}

	public CmdState APF_OnOff(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 1;
			switch ((int)Convert.ToInt16(commands.ZZAP("")))
			{
			case 0:
				commands.ZZAP("1");
				return CmdState.On;
			case 1:
				commands.ZZAP("0");
				return CmdState.Off;
			}
		}
		return CmdState.NoChange;
	}

	public void ToggleVFOWheel(int msg, MidiDevice device)
	{
		if (MidiDevice.VFOSelect == 0)
		{
			MidiDevice.VFOSelect = 1;
		}
		else if (MidiDevice.VFOSelect == 1)
		{
			MidiDevice.VFOSelect = 2;
			device.SetPL1ButtonLight(0);
		}
		else if (MidiDevice.VFOSelect == 2)
		{
			MidiDevice.VFOSelect = 3;
		}
		else
		{
			MidiDevice.VFOSelect = 0;
			device.SetPL1ButtonLight(1);
		}
	}

	public void Rx2ModeNext(int msg, MidiDevice device)
	{
		parser.nSet = 2;
		parser.nGet = 0;
		int num = Convert.ToInt16(commands.ZZME(""));
		if (num < 11 && msg == 127)
		{
			commands.ZZME((num + 1).ToString("00"));
		}
	}

	public void Rx2ModePrev(int msg, MidiDevice device)
	{
		parser.nSet = 2;
		parser.nGet = 0;
		int num = Convert.ToInt16(commands.ZZME(""));
		if (num > 0 && msg == 127)
		{
			commands.ZZME((num - 1).ToString("00"));
		}
	}

	public void Rx2FilterWider(int msg, MidiDevice device)
	{
		parser.nSet = 2;
		parser.nGet = 0;
		int num = Convert.ToInt16(commands.ZZFJ(""));
		if (num > 0 && msg == 127)
		{
			commands.ZZFJ((num - 1).ToString("00"));
		}
	}

	public void Rx2FilterNarrower(int msg, MidiDevice device)
	{
		parser.nSet = 2;
		parser.nGet = 0;
		int num = Convert.ToInt16(commands.ZZFJ(""));
		if (num < 14 && msg == 127)
		{
			commands.ZZFJ((num + 1).ToString("00"));
		}
	}

	public CmdState RX2AutoNotchOnOff(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 1;
			switch ((int)Convert.ToInt16(commands.ZZNU("")))
			{
			case 0:
				commands.ZZNU("1");
				return CmdState.On;
			case 1:
				commands.ZZNU("0");
				return CmdState.Off;
			}
		}
		return CmdState.NoChange;
	}

	public CmdState ToggleTX(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 1;
			switch ((int)Convert.ToInt16(commands.ZZSW("")))
			{
			case 0:
				commands.ZZSW("1");
				return CmdState.On;
			case 1:
				commands.ZZSW("0");
				return CmdState.Off;
			}
		}
		return CmdState.NoChange;
	}

	public void TUNPowerLevel(int msg, MidiDevice device)
	{
		parser.nSet = 3;
		parser.nGet = 0;
		try
		{
			commands.ZZTO(((double)msg * 0.787).ToString("000"));
		}
		catch
		{
		}
	}

	public void RX2AGCModeKnob(int msg, MidiDevice device)
	{
		parser.nGet = 0;
		parser.nSet = 1;
		if (msg >= 0 && msg < 22)
		{
			commands.ZZGU("0");
		}
		else if (msg >= 22 && msg < 43)
		{
			commands.ZZGU("1");
		}
		else if (msg >= 43 && msg < 64)
		{
			commands.ZZGU("2");
		}
		else if (msg >= 64 && msg < 85)
		{
			commands.ZZGU("3");
		}
		else if (msg >= 85 && msg < 106)
		{
			commands.ZZGU("4");
		}
		else if (msg >= 106 && msg < 128)
		{
			commands.ZZGU("5");
		}
	}

	public void RX2AGCModeUp(int msg, MidiDevice device)
	{
		parser.nGet = 0;
		parser.nSet = 1;
		if (msg != 127)
		{
			return;
		}
		try
		{
			int num = Convert.ToInt16(commands.ZZGU(""));
			if (num > 0 && num <= 5)
			{
				commands.ZZGU((num - 1).ToString("0"));
			}
		}
		catch
		{
		}
	}

	public void RX2AGCModeDown(int msg, MidiDevice device)
	{
		parser.nGet = 0;
		parser.nSet = 1;
		if (msg != 127)
		{
			return;
		}
		try
		{
			int num = Convert.ToInt16(commands.ZZGU(""));
			if (num >= 0 && num < 5)
			{
				commands.ZZGU((num + 1).ToString("0"));
			}
		}
		catch
		{
		}
	}

	public CmdState RX2CTunOnOff(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 1;
			switch ((int)Convert.ToInt16(commands.ZZCO("")))
			{
			case 0:
				commands.ZZCO("1");
				return CmdState.On;
			case 1:
				commands.ZZCO("0");
				return CmdState.Off;
			}
		}
		return CmdState.NoChange;
	}

	public CmdState PSOnOff(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 1;
			switch ((int)Convert.ToInt16(commands.ZZLI("")))
			{
			case 0:
				commands.ZZLI("1");
				return CmdState.On;
			case 1:
				commands.ZZLI("0");
				return CmdState.Off;
			}
		}
		return CmdState.NoChange;
	}

	public void RX2ModeSSB(int msg, MidiDevice device)
	{
		if (msg != 127)
		{
			return;
		}
		parser.nGet = 0;
		parser.nSet = 2;
		parser.nAns = 3;
		int num = 0;
		try
		{
			num = Convert.ToInt16(commands.ZZBT(""));
		}
		catch
		{
			num = 0;
		}
		parser.nGet = 0;
		parser.nSet = 2;
		parser.nAns = 2;
		try
		{
			if (num >= 40)
			{
				commands.ZZME("00");
			}
			if (num < 40)
			{
				commands.ZZME("01");
			}
		}
		catch
		{
		}
	}

	public void RX2ModeLSB(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 2;
			try
			{
				commands.ZZME("00");
			}
			catch
			{
			}
		}
	}

	public void RX2ModeUSB(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 2;
			try
			{
				commands.ZZME("01");
			}
			catch
			{
			}
		}
	}

	public void RX2ModeDSB(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 2;
			try
			{
				commands.ZZME("02");
			}
			catch
			{
			}
		}
	}

	public void RX2ModeCW(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 2;
			try
			{
				commands.ZZME("01");
			}
			catch
			{
			}
		}
	}

	public void RX2ModeCWL(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 2;
			try
			{
				commands.ZZME("03");
			}
			catch
			{
			}
		}
	}

	public void RX2ModeCWU(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 2;
			try
			{
				commands.ZZME("04");
			}
			catch
			{
			}
		}
	}

	public void RX2ModeFM(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 2;
			try
			{
				commands.ZZME("05");
			}
			catch
			{
			}
		}
	}

	public void RX2ModeAM(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 2;
			try
			{
				commands.ZZME("06");
			}
			catch
			{
			}
		}
	}

	public void RX2ModeDIGU(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 2;
			try
			{
				commands.ZZME("07");
			}
			catch
			{
			}
		}
	}

	public void RX2ModeSPEC(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 2;
			try
			{
				commands.ZZME("08");
			}
			catch
			{
			}
		}
	}

	public void RX2ModeDIGL(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 2;
			try
			{
				commands.ZZME("09");
			}
			catch
			{
			}
		}
	}

	public void RX2ModeSAM(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 2;
			try
			{
				commands.ZZME("10");
			}
			catch
			{
			}
		}
	}

	public void RX2ModeDRM(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 2;
			try
			{
				commands.ZZME("11");
			}
			catch
			{
			}
		}
	}

	public void MoveVFOBDown100Khz(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 2;
			try
			{
				commands.ZZBM("11");
			}
			catch
			{
			}
		}
	}

	public void MoveVFOBUp100Khz(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 2;
			try
			{
				commands.ZZBP("11");
			}
			catch
			{
			}
		}
	}

	public void CloseConsole(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 1;
			try
			{
				commands.ZZBY();
			}
			catch
			{
			}
		}
	}

	public CmdState RX2SquelchOnOff(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 1;
			try
			{
				switch ((int)Convert.ToInt16(commands.ZZSV("")))
				{
				case 0:
					commands.ZZSV("1");
					return CmdState.On;
				case 1:
					commands.ZZSV("0");
					return CmdState.Off;
				}
			}
			catch
			{
				return CmdState.NoChange;
			}
		}
		return CmdState.NoChange;
	}

	public void RX2SquelchControl(int msg, MidiDevice device)
	{
		parser.nSet = 0;
		parser.nSet = 3;
		try
		{
			commands.ZZSX((160.0 - (double)msg * 1.26).ToString("000"));
		}
		catch
		{
		}
	}

	public void TXFilterHigh(int msg, MidiDevice device)
	{
		int num = 20;
		device.GetDeviceName();
		if (IsBehringerCMD(device))
		{
			if (msg == 63)
			{
				msg = 127;
			}
			if (msg == 65)
			{
				msg = 1;
			}
		}
		try
		{
			parser.nGet = 0;
			parser.nSet = 2;
			int num2 = Convert.ToInt32(commands.ZZMD(""));
			num = ((num2 != 3 && num2 != 4 && num2 != 7 && num2 != 8 && num2 != 9) ? 50 : 20);
		}
		catch
		{
			return;
		}
		try
		{
			parser.nSet = 5;
			parser.nGet = 0;
			parser.nAns = 5;
			int num3 = Convert.ToInt32(commands.ZZTH(""));
			if (msg == 1 && num3 >= 0)
			{
				commands.ZZTH((num3 + num).ToString("00000"));
			}
			else if (msg == 1 && num3 < 0)
			{
				_ = -num - 1;
				commands.ZZTH((num3 + num).ToString("00000"));
			}
			else if (msg == 127 && num3 >= 0)
			{
				commands.ZZTH((num3 - num).ToString("00000"));
			}
			else if (msg == 127 && num3 < 0)
			{
				commands.ZZTH((num3 - num).ToString("00000"));
			}
		}
		catch
		{
		}
	}

	public void TXFilterLow(int msg, MidiDevice device)
	{
		int num = 20;
		device.GetDeviceName();
		if (IsBehringerCMD(device))
		{
			if (msg == 63)
			{
				msg = 127;
			}
			if (msg == 65)
			{
				msg = 1;
			}
		}
		try
		{
			parser.nGet = 0;
			parser.nSet = 2;
			int num2 = Convert.ToInt32(commands.ZZMD(""));
			if (num2 != 3 && num2 != 4 && num2 != 7 && num2 != 8 && num2 != 9)
			{
				num = 50;
			}
		}
		catch
		{
			return;
		}
		try
		{
			parser.nSet = 5;
			parser.nGet = 0;
			parser.nAns = 5;
			int num3 = Convert.ToInt32(commands.ZZTL(""));
			if (msg == 1 && num3 >= 0)
			{
				commands.ZZTL((num3 + num).ToString("00000"));
			}
			else if (msg == 1 && num3 < 0)
			{
				_ = -num - 1;
				commands.ZZTL((num3 + num).ToString("00000"));
			}
			else if (msg == 127 && num3 >= 0)
			{
				commands.ZZTL((num3 - num).ToString("00000"));
			}
			else if (msg == 127 && num3 < 0)
			{
				commands.ZZTL((num3 - num).ToString("00000"));
			}
		}
		catch
		{
		}
	}

	public CmdState ExternalPAOnOff(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 1;
			switch ((int)Convert.ToInt16(commands.ZZUP("")))
			{
			case 0:
				commands.ZZUP("1");
				return CmdState.On;
			case 1:
				commands.ZZUP("0");
				return CmdState.Off;
			}
		}
		return CmdState.NoChange;
	}

	public void ZoomToBandRecall(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 1;
			try
			{
				commands.ZZZT("0");
			}
			catch
			{
			}
		}
	}

	public void ZoomToBandStore(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 1;
			try
			{
				commands.ZZZT("1");
			}
			catch
			{
			}
		}
	}

	public CmdState RX1AutoAGC(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 1;
			switch ((int)Convert.ToInt16(commands.ZZZQ("")))
			{
			case 0:
				commands.ZZZQ("1");
				return CmdState.On;
			case 1:
				commands.ZZZQ("0");
				return CmdState.Off;
			}
		}
		return CmdState.NoChange;
	}

	public CmdState RX2AutoAGC(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 1;
			switch ((int)Convert.ToInt16(commands.ZZZR("")))
			{
			case 0:
				commands.ZZZR("1");
				return CmdState.On;
			case 1:
				commands.ZZZR("0");
				return CmdState.Off;
			}
		}
		return CmdState.NoChange;
	}

	public CmdState SwapVFOWheels(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 1;
			_swapVFOWheels = !_swapVFOWheels;
			if (_swapVFOWheels)
			{
				return CmdState.On;
			}
			return CmdState.Off;
		}
		return CmdState.NoChange;
	}

	public CmdState QuickSplitOnOff(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 1;
			switch ((int)Convert.ToInt16(commands.ZZZN("")))
			{
			case 0:
				commands.ZZZN("1");
				return CmdState.On;
			case 1:
				commands.ZZZN("0");
				return CmdState.Off;
			}
		}
		return CmdState.NoChange;
	}

	public CmdState QuickSplitOnOffandSplitOnOff(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 1;
			int num = Convert.ToInt16(commands.ZZZN(""));
			int num2 = Convert.ToInt16(commands.ZZSP(""));
			bool flag = num == 1 && num2 == 1;
			if (num == 0 || num2 == 0)
			{
				commands.ZZZN("1");
				commands.ZZSP("1");
			}
			else if (num == 1 || num2 == 1)
			{
				commands.ZZZN("0");
				commands.ZZSP("0");
			}
			num = Convert.ToInt16(commands.ZZZN(""));
			num2 = Convert.ToInt16(commands.ZZSP(""));
			bool flag2 = num == 1 && num2 == 1;
			if (flag2 != flag)
			{
				if (!flag2)
				{
					return CmdState.Off;
				}
				return CmdState.On;
			}
			return CmdState.NoChange;
		}
		return CmdState.NoChange;
	}

	public CmdState QuickPlayOnOff(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 1;
			try
			{
				switch ((int)Convert.ToInt16(commands.ZZQA("")))
				{
				case 0:
					commands.ZZQA("1");
					return CmdState.On;
				case 1:
					commands.ZZQA("0");
					return CmdState.Off;
				}
			}
			catch
			{
				return CmdState.NoChange;
			}
		}
		return CmdState.NoChange;
	}

	public CmdState QuickRecOnOff(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 1;
			try
			{
				switch ((int)Convert.ToInt16(commands.ZZQB("")))
				{
				case 0:
					commands.ZZQB("1");
					return CmdState.On;
				case 1:
					commands.ZZQB("0");
					return CmdState.Off;
				}
			}
			catch
			{
				return CmdState.NoChange;
			}
		}
		return CmdState.NoChange;
	}

	public CmdState AudioAmpOnOff(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 1;
			switch ((int)Convert.ToInt16(commands.ZZXA("")))
			{
			case 0:
				commands.ZZXA("1");
				return CmdState.On;
			case 1:
				commands.ZZXA("0");
				return CmdState.Off;
			}
		}
		return CmdState.NoChange;
	}

	public void APFType_doublepole(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 1;
			try
			{
				commands.ZZAY("0");
			}
			catch
			{
			}
		}
	}

	public void APFType_matched(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 1;
			try
			{
				commands.ZZAY("1");
			}
			catch
			{
			}
		}
	}

	public void APFType_gaussian(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 1;
			try
			{
				commands.ZZAY("2");
			}
			catch
			{
			}
		}
	}

	public void APFType_biquad(int msg, MidiDevice device)
	{
		if (msg == 127)
		{
			parser.nGet = 0;
			parser.nSet = 1;
			try
			{
				commands.ZZAY("3");
			}
			catch
			{
			}
		}
	}
}
