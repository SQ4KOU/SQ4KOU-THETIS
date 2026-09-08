using System.Threading;

namespace Thetis;

public class Radio
{
	private const int NUM_RX_THREADS = 2;

	private const int NUM_RX_PER_THREAD = 2;

	private RadioDSPRX[][] dsp_rx;

	private RadioDSPTX[] dsp_tx;

	public Radio(string datapath)
	{
		RadioDSP.AppDataPath = datapath;
		RadioDSP.CreateDSP();
		Thread.Sleep(100);
		dsp_rx = new RadioDSPRX[2][];
		for (int i = 0; i < 2; i++)
		{
			dsp_rx[i] = new RadioDSPRX[2];
			for (int j = 0; j < 2; j++)
			{
				dsp_rx[i][j] = new RadioDSPRX((uint)(i * 2), (uint)j);
			}
		}
		dsp_tx = new RadioDSPTX[1];
		dsp_tx[0] = new RadioDSPTX(1u);
		dsp_rx[0][0].Active = true;
	}

	public void Shutdown()
	{
		RadioDSP.DestroyDSP();
	}

	public RadioDSPRX GetDSPRX(int thread, int subrx)
	{
		return dsp_rx[thread][subrx];
	}

	public RadioDSPTX GetDSPTX(int thread)
	{
		return dsp_tx[thread];
	}
}
