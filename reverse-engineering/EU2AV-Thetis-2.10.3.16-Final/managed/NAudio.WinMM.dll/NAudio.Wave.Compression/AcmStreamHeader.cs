using System;
using System.Runtime.InteropServices;

namespace NAudio.Wave.Compression;

internal class AcmStreamHeader : IDisposable
{
	private AcmStreamHeaderStruct streamHeader;

	private GCHandle hSourceBuffer;

	private GCHandle hDestBuffer;

	private IntPtr streamHandle;

	private bool firstTime;

	private bool disposed;

	public byte[] SourceBuffer { get; private set; }

	public byte[] DestBuffer { get; private set; }

	public AcmStreamHeader(IntPtr streamHandle, int sourceBufferLength, int destBufferLength)
	{
		streamHeader = new AcmStreamHeaderStruct();
		SourceBuffer = new byte[sourceBufferLength];
		hSourceBuffer = GCHandle.Alloc(SourceBuffer, GCHandleType.Pinned);
		DestBuffer = new byte[destBufferLength];
		hDestBuffer = GCHandle.Alloc(DestBuffer, GCHandleType.Pinned);
		this.streamHandle = streamHandle;
		firstTime = true;
	}

	private void Prepare()
	{
		streamHeader.cbStruct = Marshal.SizeOf(streamHeader);
		streamHeader.sourceBufferLength = SourceBuffer.Length;
		streamHeader.sourceBufferPointer = hSourceBuffer.AddrOfPinnedObject();
		streamHeader.destBufferLength = DestBuffer.Length;
		streamHeader.destBufferPointer = hDestBuffer.AddrOfPinnedObject();
		MmException.Try(AcmInterop.acmStreamPrepareHeader(streamHandle, streamHeader, 0), "acmStreamPrepareHeader");
	}

	private void Unprepare()
	{
		streamHeader.sourceBufferLength = SourceBuffer.Length;
		streamHeader.sourceBufferPointer = hSourceBuffer.AddrOfPinnedObject();
		streamHeader.destBufferLength = DestBuffer.Length;
		streamHeader.destBufferPointer = hDestBuffer.AddrOfPinnedObject();
		MmResult mmResult = AcmInterop.acmStreamUnprepareHeader(streamHandle, streamHeader, 0);
		if (mmResult != MmResult.NoError)
		{
			throw new MmException(mmResult, "acmStreamUnprepareHeader");
		}
	}

	public void Reposition()
	{
		firstTime = true;
	}

	public int Convert(int bytesToConvert, out int sourceBytesConverted)
	{
		Prepare();
		try
		{
			streamHeader.sourceBufferLength = bytesToConvert;
			streamHeader.sourceBufferLengthUsed = bytesToConvert;
			AcmStreamConvertFlags streamConvertFlags = (firstTime ? (AcmStreamConvertFlags.BlockAlign | AcmStreamConvertFlags.Start) : AcmStreamConvertFlags.BlockAlign);
			MmException.Try(AcmInterop.acmStreamConvert(streamHandle, streamHeader, streamConvertFlags), "acmStreamConvert");
			firstTime = false;
			sourceBytesConverted = streamHeader.sourceBufferLengthUsed;
		}
		finally
		{
			Unprepare();
		}
		return streamHeader.destBufferLengthUsed;
	}

	public void Dispose()
	{
		GC.SuppressFinalize(this);
		Dispose(disposing: true);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!disposed)
		{
			SourceBuffer = null;
			DestBuffer = null;
			if (hSourceBuffer.IsAllocated)
			{
				hSourceBuffer.Free();
			}
			if (hDestBuffer.IsAllocated)
			{
				hDestBuffer.Free();
			}
		}
		disposed = true;
	}

	~AcmStreamHeader()
	{
		Dispose(disposing: false);
	}
}
