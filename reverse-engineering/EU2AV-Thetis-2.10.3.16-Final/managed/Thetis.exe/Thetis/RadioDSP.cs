using System;
using System.IO;
using System.Windows.Forms;

namespace Thetis;

public class RadioDSP
{
	private static bool _cache_impulse = true;

	private static bool _cache_impulse_save_restore = true;

	private static DSPMode rx1_dsp_mode = DSPMode.FIRST;

	private static DSPMode rx2_dsp_mode = DSPMode.FIRST;

	private static int rx1_dsp_rate = 48000;

	private static int rx2_dsp_rate = 48000;

	private static string app_data_path = "";

	public static bool CacheImpulse
	{
		get
		{
			return _cache_impulse;
		}
		set
		{
			_cache_impulse = value;
			WDSP.use_impulse_cache(_cache_impulse ? 1 : 0);
		}
	}

	public static bool CacheImpulseSaveRestore
	{
		get
		{
			return _cache_impulse_save_restore;
		}
		set
		{
			_cache_impulse_save_restore = value;
		}
	}

	public static DSPMode RX1DSPMode
	{
		get
		{
			return rx1_dsp_mode;
		}
		set
		{
			rx1_dsp_mode = value;
		}
	}

	public static DSPMode RX2DSPMode
	{
		get
		{
			return rx2_dsp_mode;
		}
		set
		{
			rx2_dsp_mode = value;
		}
	}

	public static double SampleRate
	{
		set
		{
			if (rx1_dsp_mode == DSPMode.FM)
			{
				rx1_dsp_rate = 192000;
			}
			else
			{
				rx1_dsp_rate = 48000;
			}
			if (rx2_dsp_mode == DSPMode.FM)
			{
				rx2_dsp_rate = 192000;
			}
			else
			{
				rx2_dsp_rate = 48000;
			}
			WDSP.SetDSPSamplerate(WDSP.id(0u, 0u), rx1_dsp_rate);
			WDSP.SetDSPSamplerate(WDSP.id(0u, 1u), rx1_dsp_rate);
			WDSP.SetDSPSamplerate(WDSP.id(2u, 0u), rx2_dsp_rate);
			WDSP.SetDSPSamplerate(WDSP.id(2u, 1u), rx2_dsp_rate);
		}
	}

	public static string AppDataPath
	{
		set
		{
			app_data_path = value;
		}
	}

	public static void CreateDSP()
	{
		string text = Path.Combine(Path.GetDirectoryName(app_data_path), "wdspWisdom01");
		string text2 = Path.Combine(Path.GetDirectoryName(app_data_path), "wdspWisdom00");
		if (File.Exists(text2))
		{
			try
			{
				if (!File.Exists(text))
				{
					File.Move(text2, text);
				}
				else
				{
					File.Delete(text2);
				}
			}
			catch
			{
			}
		}
		if (File.Exists(text))
		{
			if (File.GetLastWriteTime(text) < DateTime.Now.AddMonths(-3))
			{
				if (MessageBox.Show("The fft wisdom file is older than 3 months.\n\nIt can yield performance improvements if rebuilt, especially if the Thetis version/install has changed.\n\nThis process can take upwards of 5 minutes or more depending upon your system.\n\nYou will be notified when complete. Do you want to rebuild it?\n\nnote: you will not be asked again for another 3 months", "Wisdom File", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2, (MessageBoxOptions)262144) == DialogResult.No)
				{
					File.SetLastWriteTime(text, DateTime.Now);
				}
				else
				{
					try
					{
						File.Delete(text);
					}
					catch
					{
					}
				}
			}
		}
		else
		{
			MessageBox.Show("The fft wisdom file is missing and needs to be built.\n\nThis process can take upwards of 5 minutes or more depending upon your system.\n\nYou will be notified when complete.", "Wisdom File", MessageBoxButtons.OK, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1, (MessageBoxOptions)262144);
		}
		bool flag = WDSP.WDSPwisdom(app_data_path) == 1;
		if (flag)
		{
			try
			{
				string path = Path.Combine(app_data_path, "impulse_cache.dat");
				if (File.Exists(path))
				{
					File.Delete(path);
				}
			}
			catch
			{
			}
			MessageBox.Show("The fft wisdom file has been rebuilt.\n\nIt is now safe to close the output console window.", "Wisdom File", MessageBoxButtons.OK, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1, (MessageBoxOptions)262144);
		}
		WDSP.init_impulse_cache(_cache_impulse ? 1 : 0);
		if (_cache_impulse_save_restore && !flag)
		{
			WDSP.read_impulse_cache(Path.Combine(app_data_path, "impulse_cache.dat"));
		}
		cmaster.CMCreateCMaster();
	}

	public static void DestroyDSP()
	{
		if (_cache_impulse && _cache_impulse_save_restore)
		{
			WDSP.save_impulse_cache(Path.Combine(app_data_path, "impulse_cache.dat"));
		}
		else
		{
			try
			{
				string path = Path.Combine(app_data_path, "impulse_cache.dat");
				if (File.Exists(path))
				{
					File.Delete(path);
				}
			}
			catch
			{
			}
		}
		WDSP.destroy_impulse_cache();
		WDSP.RNNRloadModel("");
		cmaster.StopTCIStreamThreads();
		cmaster.DestroyRadio();
	}
}
