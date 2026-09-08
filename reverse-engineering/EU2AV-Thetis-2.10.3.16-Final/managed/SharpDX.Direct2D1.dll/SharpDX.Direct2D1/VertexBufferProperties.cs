using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct2D1;

public class VertexBufferProperties
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct __Native
	{
		public int InputCount;

		public VertexUsage Usage;

		public IntPtr DataPointer;

		public int SizeInBytes;
	}

	private DataStream data;

	public int InputCount;

	public VertexUsage Usage;

	internal IntPtr DataPointer;

	internal int SizeInBytes;

	public DataStream Data
	{
		get
		{
			return data;
		}
		set
		{
			data = value;
			if (data != null)
			{
				DataPointer = data.DataPointer;
				SizeInBytes = (int)data.Length;
			}
		}
	}

	public VertexBufferProperties()
	{
	}

	public VertexBufferProperties(int inputCount, VertexUsage usage, DataStream data)
	{
		InputCount = inputCount;
		Usage = usage;
		Data = data;
	}

	internal void __MarshalFree(ref __Native @ref)
	{
	}

	internal void __MarshalFrom(ref __Native @ref)
	{
		InputCount = @ref.InputCount;
		Usage = @ref.Usage;
		DataPointer = @ref.DataPointer;
		SizeInBytes = @ref.SizeInBytes;
	}

	internal void __MarshalTo(ref __Native @ref)
	{
		@ref.InputCount = InputCount;
		@ref.Usage = Usage;
		@ref.DataPointer = DataPointer;
		@ref.SizeInBytes = SizeInBytes;
	}
}
