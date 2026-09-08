using System.Threading;

namespace Thetis;

internal class PlayWave
{
	private int id;

	private bool run;

	private int condx;

	private int _busy;

	private float[] left = new float[2048];

	private float[] right = new float[2048];

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

	public unsafe void wplay(int state, double* data)
	{
		if (!run || condx != state || Interlocked.Exchange(ref _busy, 1) == 1)
		{
			return;
		}
		try
		{
			int n = 2048;
			fixed (float* i = &left[0])
			{
				fixed (float* q = &right[0])
				{
					WaveThing.wave_file_reader[id].GetPlayBuffer(i, q);
					swizzle(n, i, q, data);
				}
			}
		}
		catch
		{
		}
		finally
		{
			Interlocked.Exchange(ref _busy, 0);
		}
	}

	private unsafe static void swizzle(int n, float* I, float* Q, double* C)
	{
		for (int i = 0; i < n; i++)
		{
			C[2 * i] = I[i];
			C[2 * i + 1] = Q[i];
		}
	}
}
