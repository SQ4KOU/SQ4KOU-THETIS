using System;

namespace Thetis;

public class Penny
{
	private static Penny theSingleton = null;

	private static object m_objLock = new object();

	private byte[] TXABitMasks = new byte[41];

	private byte[] RXABitMasks = new byte[41];

	private byte[] TXBBitMasks = new byte[41];

	private byte[] RXBBitMasks = new byte[41];

	private TXPinActions[,] TXPinAction = new TXPinActions[3, 7];

	private bool[,] TXPinPA = new bool[3, 7];

	private bool[,] RXPinPA = new bool[3, 7];

	public int RxABitMask = 15;

	public bool SplitPins;

	public bool VFOBTX;

	private int m_nOldBits = -1;

	public static Penny getPenny()
	{
		lock (m_objLock)
		{
			if (theSingleton == null)
			{
				theSingleton = new Penny();
			}
		}
		return theSingleton;
	}

	private Penny()
	{
		for (int i = 0; i < 7; i++)
		{
			for (int j = 0; j < 3; j++)
			{
				TXPinAction[j, i] = TXPinActions.MOX_TUNE_TWOTONE;
				TXPinPA[j, i] = false;
				RXPinPA[j, i] = false;
			}
		}
	}

	public void setRXPinPA(int group, int pin, bool pa)
	{
		if (group >= 0 && group <= 2 && pin >= 1 && pin <= 7)
		{
			RXPinPA[group, pin - 1] = pa;
		}
	}

	public void setTXPinPA(int group, int pin, bool pa)
	{
		if (group >= 0 && group <= 2 && pin >= 1 && pin <= 7)
		{
			TXPinPA[group, pin - 1] = pa;
		}
	}

	public void setTXPinAction(int group, int pin, TXPinActions action)
	{
		if (group >= 0 && group <= 2 && pin >= 1 && pin <= 7 && action > TXPinActions.FIRST && action < TXPinActions.LAST)
		{
			TXPinAction[group, pin - 1] = action;
		}
	}

	public void setBandABitMask(Band band, byte mask, bool tx)
	{
		int num = (int)(band - 1);
		if (tx)
		{
			TXABitMasks[num] = mask;
		}
		else
		{
			RXABitMasks[num] = mask;
		}
	}

	public void setBandBBitMask(Band band, byte mask, bool tx)
	{
		int num = (int)(band - 1);
		if (tx)
		{
			TXBBitMasks[num] = mask;
		}
		else
		{
			RXBBitMasks[num] = mask;
		}
	}

	public int ExtCtrlEnable(Band band, Band bandb, bool tx, bool enable, bool tune, bool twoTone, bool pa)
	{
		if (!enable)
		{
			NetworkIO.SetOCBits(0);
			return 0;
		}
		return UpdateExtCtrl(band, bandb, tx, tune, twoTone, pa);
	}

	public int UpdateExtCtrl(Band band, Band bandb, bool tx, bool tune, bool twoTone, bool pa)
	{
		int num = (int)(band - 1);
		int num2 = (int)(bandb - 1);
		int bits = ((num >= 0 && num <= 40 && (!SplitPins || num2 >= 0) && (!SplitPins || num2 <= 40)) ? (SplitPins ? (tx ? ((TXABitMasks[num] & RxABitMask) | TXBBitMasks[num2]) : ((RXABitMasks[num] & RxABitMask) | RXBBitMasks[num2])) : ((tx && VFOBTX) ? TXABitMasks[num2] : ((!tx) ? RXABitMasks[num] : TXABitMasks[num]))) : 0);
		bits = ((!tx) ? adjustForRX(band, bandb, bits, tx, tune, twoTone, pa) : adjustForTXAction(band, bandb, bits, tx, tune, twoTone, pa));
		if (bits != m_nOldBits)
		{
			string text = Convert.ToString(bits, 2).PadLeft(8, '0');
			string[] obj = new string[14]
			{
				"Bits: ",
				bits.ToString(),
				" (",
				text,
				") Band: ",
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null
			};
			int num3 = (int)band;
			obj[5] = num3.ToString();
			obj[6] = " BandB: ";
			num3 = (int)bandb;
			obj[7] = num3.ToString();
			obj[8] = " tx: ";
			obj[9] = tx.ToString();
			obj[10] = " tune: ";
			obj[11] = tune.ToString();
			obj[12] = " 2ton: ";
			obj[13] = twoTone.ToString();
			System.Console.WriteLine(string.Concat(obj));
			NetworkIO.SetOCBits(bits);
			m_nOldBits = bits;
		}
		return bits;
	}

	private int getGroup(Band b)
	{
		int result = -1;
		switch (b)
		{
		case Band.GEN:
		case Band.B160M:
		case Band.B80M:
		case Band.B60M:
		case Band.B40M:
		case Band.B30M:
		case Band.B20M:
		case Band.B17M:
		case Band.B15M:
		case Band.B12M:
		case Band.B10M:
		case Band.B6M:
		case Band.B2M:
		case Band.WWV:
			result = 0;
			break;
		case Band.VHF0:
		case Band.VHF1:
		case Band.VHF2:
		case Band.VHF3:
		case Band.VHF4:
		case Band.VHF5:
		case Band.VHF6:
		case Band.VHF7:
		case Band.VHF8:
		case Band.VHF9:
		case Band.VHF10:
		case Band.VHF11:
		case Band.VHF12:
		case Band.VHF13:
			result = 1;
			break;
		case Band.BLMF:
		case Band.B120M:
		case Band.B90M:
		case Band.B61M:
		case Band.B49M:
		case Band.B41M:
		case Band.B31M:
		case Band.B25M:
		case Band.B22M:
		case Band.B19M:
		case Band.B16M:
		case Band.B14M:
		case Band.B13M:
		case Band.B11M:
			result = 2;
			break;
		}
		return result;
	}

	private int adjustForRX(Band band, Band bandb, int bits, bool tx, bool tune, bool twoTone, bool pa)
	{
		int num = 0;
		for (int i = 0; i < 7; i++)
		{
			int num2 = (SplitPins ? ((((1 << i) & RxABitMask) == 0) ? getGroup(bandb) : getGroup(band)) : ((!VFOBTX) ? getGroup(band) : getGroup(bandb)));
			if (num2 >= 0)
			{
				bool flag = RXPinPA[num2, i];
				int num3 = ((!flag || (flag & pa)) ? 1 : 0);
				num |= num3 << i;
			}
		}
		return bits & num;
	}

	private int adjustForTXAction(Band band, Band bandb, int bits, bool tx, bool tune, bool twoTone, bool pa)
	{
		int num = 0;
		for (int i = 0; i < 7; i++)
		{
			int num2 = (SplitPins ? ((((1 << i) & RxABitMask) == 0) ? getGroup(bandb) : getGroup(band)) : ((!VFOBTX) ? getGroup(band) : getGroup(bandb)));
			if (num2 < 0)
			{
				continue;
			}
			TXPinActions tXPinActions = TXPinAction[num2, i];
			bool flag = TXPinPA[num2, i];
			int num3 = 0;
			if (!flag || (flag & pa))
			{
				switch (tXPinActions)
				{
				case TXPinActions.MOX:
					num3 = ((tx && !tune && !twoTone) ? 1 : 0);
					break;
				case TXPinActions.TUNE:
					num3 = (tune ? 1 : 0);
					break;
				case TXPinActions.TWOTONE:
					num3 = (twoTone ? 1 : 0);
					break;
				case TXPinActions.MOX_TUNE:
					num3 = (((tx | tune) && !twoTone) ? 1 : 0);
					break;
				case TXPinActions.MOX_TWOTONE:
					num3 = (((tx | twoTone) & !tune) ? 1 : 0);
					break;
				case TXPinActions.TUNE_TWOTONE:
					num3 = ((tune | twoTone) ? 1 : 0);
					break;
				case TXPinActions.MOX_TUNE_TWOTONE:
					num3 = ((tx | tune | twoTone) ? 1 : 0);
					break;
				}
			}
			num |= num3 << i;
		}
		return bits & num;
	}
}
