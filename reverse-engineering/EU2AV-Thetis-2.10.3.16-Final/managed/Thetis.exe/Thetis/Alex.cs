using System;

namespace Thetis;

public class Alex
{
	private static Alex theSingleton;

	private byte[] TxAnt = new byte[12];

	private byte[] RxAnt = new byte[12];

	private byte[] RxOnlyAnt = new byte[12];

	private bool LimitTXRXAntenna;

	public static bool RxOutOnTx;

	public static bool Ext1OutOnTx;

	public static bool Ext2OutOnTx;

	public static bool init_update;

	public static bool rx_out_override;

	public static bool TRxAnt;

	private int m_nOld_rx_only_ant = -99;

	private int m_nOld_trx_ant = -99;

	private int m_nOld_tx_ant = -99;

	private int m_nOld_rx_out = -99;

	private bool m_bOld_tx;

	private bool m_bOld_alex_enabled;

	public static bool trx_ant_different { get; set; }

	public static Alex getAlex()
	{
		lock (typeof(Alex))
		{
			if (theSingleton == null)
			{
				theSingleton = new Alex();
			}
		}
		return theSingleton;
	}

	private Alex()
	{
		for (int i = 0; i < 12; i++)
		{
			TxAnt[i] = 1;
			RxAnt[i] = 1;
			RxOnlyAnt[i] = 0;
		}
	}

	public void SetAntennasTo1(bool IsSetTo1)
	{
		LimitTXRXAntenna = IsSetTo1;
	}

	public void setRxAnt(Band band, byte ant)
	{
		if (ant > 3)
		{
			ant = 1;
		}
		int num = (int)(band - 1);
		RxAnt[num] = ant;
	}

	public void setRxOnlyAnt(Band band, byte ant)
	{
		_ = 3;
		int num = (int)(band - 1);
		RxOnlyAnt[num] = ant;
	}

	public void setTxAnt(Band band, byte ant)
	{
		if (ant > 3)
		{
			ant = 1;
		}
		int num = (int)(band - 1);
		TxAnt[num] = ant;
	}

	public static Band AntBandFromFreq(double freq)
	{
		if (freq >= 12.075)
		{
			if (freq >= 23.17)
			{
				if (freq >= 26.465)
				{
					return (freq >= 39.85) ? Band.B6M : Band.B10M;
				}
				return Band.B12M;
			}
			if (freq >= 16.209)
			{
				return (freq >= 19.584) ? Band.B15M : Band.B17M;
			}
			return Band.B20M;
		}
		if (freq >= 6.20175)
		{
			return (freq >= 8.7) ? Band.B30M : Band.B40M;
		}
		if (freq >= 4.66525)
		{
			return Band.B60M;
		}
		return (!(freq >= 2.75)) ? Band.B160M : Band.B80M;
	}

	public static Band AntBandFromFreq(bool tx)
	{
		Console console = Console.getConsole();
		if (console == null)
		{
			System.Console.WriteLine("no console");
			return Band.LAST;
		}
		double freq = ((!tx) ? console.VFOAFreq : ((!console.VFOATX) ? console.VFOBFreq : console.VFOAFreq));
		if (console.RX1XVTRIndex >= 0)
		{
			freq = console.XVTRForm.TranslateFreq(freq);
		}
		System.Console.WriteLine("Freq is: " + freq);
		Band result = AntBandFromFreq(freq);
		System.Console.WriteLine("Band is: " + result);
		return result;
	}

	public static Band AntBandFromFreqB()
	{
		Console console = Console.getConsole();
		if (console == null)
		{
			System.Console.WriteLine("no console");
			return Band.LAST;
		}
		double vFOBFreq = Console.getConsole().VFOBFreq;
		vFOBFreq = ((console.RX2XVTRIndex < 0) ? Console.getConsole().VFOBFreq : console.XVTRForm.TranslateFreq(vFOBFreq));
		System.Console.WriteLine("Freq is: " + vFOBFreq);
		if (vFOBFreq >= 12.075)
		{
			if (vFOBFreq >= 23.17)
			{
				if (vFOBFreq >= 26.465)
				{
					return (vFOBFreq >= 39.85) ? Band.B6M : Band.B10M;
				}
				return Band.B12M;
			}
			if (vFOBFreq >= 16.209)
			{
				return (vFOBFreq >= 19.584) ? Band.B15M : Band.B17M;
			}
			return Band.B20M;
		}
		if (vFOBFreq >= 6.20175)
		{
			return (vFOBFreq >= 8.7) ? Band.B30M : Band.B40M;
		}
		if (vFOBFreq >= 4.66525)
		{
			return Band.B60M;
		}
		return (!(vFOBFreq >= 2.75)) ? Band.B160M : Band.B80M;
	}

	public void UpdateAlexAntSelection(Band band, bool tx, bool xvtr)
	{
		UpdateAlexAntSelection(band, tx, alex_enabled: true, xvtr);
	}

	public void UpdateAlexAntSelection(Band band, bool tx, bool alex_enabled, bool xvtr)
	{
		if (!alex_enabled)
		{
			NetworkIO.SetAntBits(0, 0, 0, 0, tx: false);
			m_bOld_alex_enabled = alex_enabled;
			return;
		}
		int num = (int)(band - 1);
		if (num < 0 || num > 11)
		{
			band = AntBandFromFreq(tx);
			num = (int)(band - 1);
			if (num < 0 || num > 11)
			{
				System.Console.WriteLine("No good ant!");
				return;
			}
		}
		int num2 = TxAnt[num];
		int num3;
		int num4;
		int num5;
		if (tx)
		{
			num3 = (Ext2OutOnTx ? 1 : (Ext1OutOnTx ? 2 : 0));
			num4 = ((RxOutOnTx || Ext1OutOnTx || Ext2OutOnTx) ? 1 : 0);
			num5 = TxAnt[num];
		}
		else
		{
			num3 = RxOnlyAnt[num];
			if (xvtr)
			{
				num3 = ((num3 >= 3) ? 3 : 0);
			}
			else if (num3 >= 3)
			{
				num3 -= 3;
			}
			num4 = ((num3 != 0) ? 1 : 0);
			num5 = ((!TRxAnt) ? RxAnt[num] : TxAnt[num]);
			trx_ant_different = RxAnt[num] != TxAnt[num];
		}
		if (rx_out_override && num4 == 1)
		{
			if (!tx)
			{
				num5 = 4;
			}
			num4 = (tx ? ((RxOutOnTx || Ext1OutOnTx || Ext2OutOnTx) ? 1 : 0) : 0);
		}
		if (num5 != 4 && LimitTXRXAntenna)
		{
			num5 = 1;
		}
		if (m_nOld_rx_only_ant != num3 || m_nOld_trx_ant != num5 || m_nOld_tx_ant != num2 || m_nOld_rx_out != num4 || m_bOld_tx != tx || m_bOld_alex_enabled != alex_enabled)
		{
			NetworkIO.SetAntBits(num3, num5, num2, num4, tx);
			System.Console.WriteLine("Ant idx: " + num + "(" + ((Band)(num + 1)/*cast due to constrained. prefix*/).ToString() + ")");
			System.Console.WriteLine("Ant Rx Only {0} , TRx Ant {1}, Tx Ant {2}, Rx Out {3}, TX {4}", num3.ToString(), num5.ToString(), num2.ToString(), num4.ToString(), tx.ToString());
			m_nOld_rx_only_ant = num3;
			m_nOld_trx_ant = num5;
			m_nOld_tx_ant = num2;
			m_nOld_rx_out = num4;
			m_bOld_tx = tx;
			m_bOld_alex_enabled = alex_enabled;
		}
	}
}
