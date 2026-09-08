using System.Runtime.InteropServices;

namespace System.IO;

internal sealed class PinnedBufferMemoryStream : UnmanagedMemoryStream
{
	private readonly byte[] _array;

	private GCHandle _pinningHandle;

	internal unsafe PinnedBufferMemoryStream(byte[] array)
	{
		_array = array;
		_pinningHandle = GCHandle.Alloc(array, GCHandleType.Pinned);
		int num = array.Length;
		fixed (byte* reference = &MemoryMarshal.GetReference((Span<byte>)array))
		{
			Initialize(reference, num, num, FileAccess.Read);
		}
	}

	~PinnedBufferMemoryStream()
	{
		Dispose(disposing: false);
	}

	protected override void Dispose(bool disposing)
	{
		if (_pinningHandle.IsAllocated)
		{
			_pinningHandle.Free();
		}
		base.Dispose(disposing);
	}
}
