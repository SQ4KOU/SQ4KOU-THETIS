using System.Threading;

namespace Thetis;

internal class RecordWave
{
	private int id;

	private bool run;

	private int condx;

	private bool rxpre;

	private bool txpre = true;

	private int busy;

	private float[] left = new float[2048];

	private float[] right = new float[2048];

	private float[] ltemp = new float[2048];

	private float[] rtemp = new float[2048];

	public int ID
	{
		get
		{
			return id;
		}
		set
		{
			id = value;
		}
	}

	public bool Run
	{
		get
		{
			return run;
		}
		set
		{
			run = value;
		}
	}

	public int Condx
	{
		get
		{
			return condx;
		}
		set
		{
			condx = value;
		}
	}

	public bool RxPre
	{
		get
		{
			return rxpre;
		}
		set
		{
			rxpre = value;
		}
	}

	public bool TxPre
	{
		get
		{
			return txpre;
		}
		set
		{
			txpre = value;
		}
	}

	public unsafe void wrecord(int state, int pos, double* data)
	{
		if (!run || condx != state || Interlocked.Exchange(ref busy, 1) == 1)
		{
			return;
		}
		try
		{
			fixed (float* ptr = &left[0])
			{
				fixed (float* ptr2 = &right[0])
				{
					fixed (float* output = &ltemp[0])
					{
						fixed (float* output2 = &rtemp[0])
						{
							if (pos == 0)
							{
								if (state == 0 && rxpre)
								{
									int inputRate = cmaster.GetInputRate(0, id);
									int buffSize = cmaster.GetBuffSize(inputRate);
									deswizzle(buffSize, data, ptr, ptr2);
									if (WaveThing.wave_file_writer[id].BaseRate != inputRate)
									{
										int nsamps = default(int);
										WDSP.xresampleFV(ptr, output, buffSize, &nsamps, WaveThing.wave_file_writer[id].RcvrResampL);
										WDSP.xresampleFV(ptr2, output2, buffSize, &nsamps, WaveThing.wave_file_writer[id].RcvrResampR);
										WaveThing.wave_file_writer[id].AddWriteBuffer(output, output2, nsamps);
									}
									else
									{
										WaveThing.wave_file_writer[id].AddWriteBuffer(ptr, ptr2, buffSize);
									}
								}
								if (state == 1 && txpre)
								{
									int inputRate2 = cmaster.GetInputRate(1, 0);
									int buffSize2 = cmaster.GetBuffSize(inputRate2);
									deswizzle(buffSize2, data, ptr, ptr2);
									if (WaveThing.wave_file_writer[id].BaseRate != inputRate2)
									{
										int nsamps2 = default(int);
										WDSP.xresampleFV(ptr, output, buffSize2, &nsamps2, WaveThing.wave_file_writer[id].XmtrResampL);
										WDSP.xresampleFV(ptr2, output2, buffSize2, &nsamps2, WaveThing.wave_file_writer[id].XmtrResampR);
										WaveThing.wave_file_writer[id].AddWriteBuffer(output, output2, nsamps2);
									}
									else
									{
										WaveThing.wave_file_writer[id].AddWriteBuffer(ptr, ptr2, buffSize2);
									}
								}
							}
							if (pos != 1)
							{
								return;
							}
							if (state == 0 && !rxpre)
							{
								int channelOutputRate = cmaster.GetChannelOutputRate(0, id);
								int buffSize3 = cmaster.GetBuffSize(channelOutputRate);
								deswizzle(buffSize3, data, ptr, ptr2);
								if (WaveThing.wave_file_writer[id].BaseRate != channelOutputRate)
								{
									int nsamps3 = default(int);
									WDSP.xresampleFV(ptr, output, buffSize3, &nsamps3, WaveThing.wave_file_writer[id].RcvrResampL);
									WDSP.xresampleFV(ptr2, output2, buffSize3, &nsamps3, WaveThing.wave_file_writer[id].RcvrResampR);
									WaveThing.wave_file_writer[id].AddWriteBuffer(output, output2, nsamps3);
								}
								else
								{
									WaveThing.wave_file_writer[id].AddWriteBuffer(ptr, ptr2, buffSize3);
								}
							}
							if (state == 1 && !txpre)
							{
								int channelOutputRate2 = cmaster.GetChannelOutputRate(1, 0);
								int buffSize4 = cmaster.GetBuffSize(channelOutputRate2);
								deswizzle(buffSize4, data, ptr, ptr2);
								if (WaveThing.wave_file_writer[id].BaseRate != channelOutputRate2)
								{
									int nsamps4 = default(int);
									WDSP.xresampleFV(ptr, output, buffSize4, &nsamps4, WaveThing.wave_file_writer[id].XmtrResampL);
									WDSP.xresampleFV(ptr2, output2, buffSize4, &nsamps4, WaveThing.wave_file_writer[id].XmtrResampR);
									WaveThing.wave_file_writer[id].AddWriteBuffer(output, output2, nsamps4);
								}
								else
								{
									WaveThing.wave_file_writer[id].AddWriteBuffer(ptr, ptr2, buffSize4);
								}
							}
						}
					}
				}
			}
		}
		catch
		{
		}
		finally
		{
			Interlocked.Exchange(ref busy, 0);
		}
	}

	private unsafe static void deswizzle(int n, double* C, float* I, float* Q)
	{
		for (int i = 0; i < n; i++)
		{
			I[i] = (float)C[2 * i];
			Q[i] = (float)C[2 * i + 1];
		}
	}
}
