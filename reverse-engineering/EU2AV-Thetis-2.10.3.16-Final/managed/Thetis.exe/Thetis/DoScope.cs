using System.Threading;

namespace Thetis;

internal class DoScope
{
	private float[] left = new float[2048];

	private float[] right = new float[2048];

	private int busy;

	private bool run;

	private int condx;

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

	public unsafe void xscope(int state, double* data)
	{
		if (!run || condx != state || Interlocked.Exchange(ref busy, 1) == 1)
		{
			return;
		}
		try
		{
			int num = (Audio.MOX ? Audio.OutCountTX : Audio.OutCount);
			fixed (float* ptr = &left[0])
			{
				fixed (float* ptr2 = &right[0])
				{
					deswizzle(num, data, ptr, ptr2);
					Audio.DoScope(ptr, num);
					Audio.DoScope2(ptr2, num);
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
