using System;
using System.Runtime.InteropServices;

namespace NAudio.Dmo;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public struct DmoOutputDataBuffer(int maxBufferSize) : IDisposable
{
	[MarshalAs(UnmanagedType.Interface)]
	private IMediaBuffer pBuffer = new MediaBuffer(maxBufferSize);

	private DmoOutputDataBufferFlags dwStatus = DmoOutputDataBufferFlags.None;

	private long rtTimestamp = 0L;

	private long referenceTimeDuration = 0L;

	public IMediaBuffer MediaBuffer
	{
		get
		{
			return pBuffer;
		}
		internal set
		{
			pBuffer = value;
		}
	}

	public int Length => ((MediaBuffer)pBuffer).Length;

	public DmoOutputDataBufferFlags StatusFlags
	{
		get
		{
			return dwStatus;
		}
		internal set
		{
			dwStatus = value;
		}
	}

	public long Timestamp
	{
		get
		{
			return rtTimestamp;
		}
		internal set
		{
			rtTimestamp = value;
		}
	}

	public long Duration
	{
		get
		{
			return referenceTimeDuration;
		}
		internal set
		{
			referenceTimeDuration = value;
		}
	}

	public bool MoreDataAvailable => (StatusFlags & DmoOutputDataBufferFlags.Incomplete) == DmoOutputDataBufferFlags.Incomplete;

	public void Dispose()
	{
		if (pBuffer != null)
		{
			((MediaBuffer)pBuffer).Dispose();
			pBuffer = null;
			GC.SuppressFinalize(this);
		}
	}

	public void RetrieveData(byte[] data, int offset)
	{
		((MediaBuffer)pBuffer).RetrieveData(data, offset);
	}
}
