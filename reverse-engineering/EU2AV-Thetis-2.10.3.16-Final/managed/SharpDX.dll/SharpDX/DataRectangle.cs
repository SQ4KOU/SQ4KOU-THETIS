using System;

namespace SharpDX;

public struct DataRectangle(IntPtr dataPointer, int pitch)
{
	public IntPtr DataPointer = dataPointer;

	public int Pitch = pitch;
}
