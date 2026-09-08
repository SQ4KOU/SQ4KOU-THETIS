using System;
using System.Runtime.InteropServices;

namespace Thetis;

public class RingBufferByte
{
	private byte[] buf;

	private int wptr;

	private int rptr;

	private int size;

	private int mask;

	public RingBufferByte(int sz2)
	{
		size = nblock2(sz2);
		buf = new byte[size];
		mask = size - 1;
		wptr = (rptr = 0);
	}

	public int npoof2(int n)
	{
		int num = 0;
		n--;
		while (n > 0)
		{
			n >>= 1;
			num++;
		}
		return num;
	}

	public int nblock2(int n)
	{
		return 1 << npoof2(n);
	}

	public int ReadSpace()
	{
		int num = wptr;
		int num2 = rptr;
		if (num > num2)
		{
			return num - num2;
		}
		return (size - num2 + num) & mask;
	}

	public int WriteSpace()
	{
		int num = wptr;
		int num2 = rptr;
		if (num > num2)
		{
			return ((size - num + num2) & mask) - 1;
		}
		if (num < num2)
		{
			return num2 - num - 1;
		}
		return size - 1;
	}

	public int Read(byte[] dest, int cnt)
	{
		int num = ReadSpace();
		if (num == 0)
		{
			return 0;
		}
		int num2 = ((cnt > num) ? num : cnt);
		int num3 = rptr + num2;
		int num4 = 0;
		int num5 = 0;
		if (num3 > size)
		{
			num4 = size - rptr;
			num5 = num3 & mask;
		}
		else
		{
			num4 = num2;
			num5 = 0;
		}
		Array.Copy(buf, rptr, dest, 0, num4);
		rptr = (rptr + num4) & mask;
		if (num5 != 0)
		{
			Array.Copy(buf, rptr, dest, num4, num5);
			rptr = (rptr + num5) & mask;
		}
		return num2;
	}

	public unsafe int ReadPtr(byte* dest, int cnt)
	{
		int num = ReadSpace();
		if (num == 0)
		{
			return 0;
		}
		int num2 = ((cnt > num) ? num : cnt);
		int num3 = rptr + num2;
		int num4 = 0;
		int num5 = 0;
		if (num3 > size)
		{
			num4 = size - rptr;
			num5 = num3 & mask;
		}
		else
		{
			num4 = num2;
			num5 = 0;
		}
		Marshal.Copy(buf, rptr, new IntPtr(dest), num4);
		rptr = (rptr + num4) & mask;
		if (num5 != 0)
		{
			Marshal.Copy(buf, rptr, new IntPtr(dest + num4), num5);
			rptr = (rptr + num5) & mask;
		}
		return num2;
	}

	public int Write(byte[] src, int cnt)
	{
		int num = WriteSpace();
		if (num == 0)
		{
			return 0;
		}
		int num2 = ((cnt > num) ? num : cnt);
		int num3 = wptr + num2;
		int num4 = 0;
		int num5 = 0;
		if (num3 > size)
		{
			num4 = size - wptr;
			num5 = num3 & mask;
		}
		else
		{
			num4 = num2;
			num5 = 0;
		}
		Array.Copy(src, 0, buf, wptr, num4);
		wptr = (wptr + num4) & mask;
		if (num5 != 0)
		{
			Array.Copy(src, num4, buf, wptr, num5);
			wptr = (wptr + num5) & mask;
		}
		return num2;
	}

	public unsafe int WritePtr(byte* src, int cnt)
	{
		int num = WriteSpace();
		if (num == 0)
		{
			return 0;
		}
		int num2 = ((cnt > num) ? num : cnt);
		int num3 = wptr + num2;
		int num4 = 0;
		int num5 = 0;
		if (num3 > size)
		{
			num4 = size - wptr;
			num5 = num3 & mask;
		}
		else
		{
			num4 = num2;
			num5 = 0;
		}
		Marshal.Copy(new IntPtr(src), buf, wptr, num4);
		wptr = (wptr + num4) & mask;
		if (num5 != 0)
		{
			Marshal.Copy(new IntPtr(src + num4), buf, wptr, num5);
			wptr = (wptr + num5) & mask;
		}
		return num2;
	}

	public void Reset()
	{
		rptr = 0;
		wptr = 0;
	}

	public void Clear(int nbytes)
	{
		byte[] array = new byte[nbytes];
		Array.Clear(array, 0, nbytes);
		Write(array, nbytes);
	}

	public void Restart(int nbytes)
	{
		Reset();
		Clear(nbytes);
	}

	public int Peek(byte[] dest, int cnt)
	{
		int num = ReadSpace();
		if (num == 0)
		{
			return 0;
		}
		int num2 = ((cnt > num) ? num : cnt);
		int num3 = rptr + num2;
		int num4 = 0;
		int num5 = 0;
		if (num3 > size)
		{
			num4 = size - rptr;
			num5 = num3 & mask;
		}
		else
		{
			num4 = num2;
			num5 = 0;
		}
		Array.Copy(buf, rptr, dest, 0, num4);
		if (num5 != 0)
		{
			Array.Copy(buf, 0, dest, num4, num5);
		}
		return num2;
	}

	public unsafe int Peek(byte* dest, int cnt)
	{
		int num = ReadSpace();
		if (num == 0)
		{
			return 0;
		}
		int num2 = ((cnt > num) ? num : cnt);
		int num3 = rptr + num2;
		int num4 = 0;
		int num5 = 0;
		if (num3 > size)
		{
			num4 = size - rptr;
			num5 = num3 & mask;
		}
		else
		{
			num4 = num2;
			num5 = 0;
		}
		Marshal.Copy(buf, rptr, new IntPtr(dest), num4);
		if (num5 != 0)
		{
			Marshal.Copy(buf, 0, new IntPtr(dest + num4), num5);
		}
		return num2;
	}

	public void ReadAdvance(int cnt)
	{
		rptr = (rptr + cnt) % mask;
	}

	public void WriteAdvance(int cnt)
	{
		wptr = (wptr + cnt) % mask;
	}
}
