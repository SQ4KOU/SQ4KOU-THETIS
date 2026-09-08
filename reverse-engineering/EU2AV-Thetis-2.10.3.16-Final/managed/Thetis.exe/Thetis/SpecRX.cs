namespace Thetis;

public class SpecRX
{
	private const int NUM_RX_DISP = 8;

	private SpecHPSDR[] spec_rx;

	public SpecRX()
	{
		spec_rx = new SpecHPSDR[8];
		spec_rx[cmaster.inid(0, 0)] = new SpecHPSDR(cmaster.inid(0, 0));
		spec_rx[cmaster.inid(0, 1)] = new SpecHPSDR(cmaster.inid(0, 1));
		spec_rx[cmaster.inid(1, 0)] = new SpecHPSDR(cmaster.inid(1, 0));
	}

	public SpecHPSDR GetSpecRX(int disp)
	{
		return spec_rx[disp];
	}
}
