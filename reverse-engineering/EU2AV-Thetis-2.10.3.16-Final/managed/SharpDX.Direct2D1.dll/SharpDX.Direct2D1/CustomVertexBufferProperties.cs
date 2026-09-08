using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct2D1;

public class CustomVertexBufferProperties
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct __Native
	{
		public IntPtr ShaderBufferWithInputSignature;

		public int ShaderBufferSize;

		public IntPtr InputElementsPointer;

		public int ElementCount;

		public int Stride;
	}

	internal IntPtr ShaderBufferWithInputSignature;

	internal int ShaderBufferSize;

	internal IntPtr InputElementsPointer;

	internal int ElementCount;

	public int Stride;

	public byte[] InputSignature { get; set; }

	public InputElement[] InputElements { get; set; }

	public CustomVertexBufferProperties()
	{
	}

	public CustomVertexBufferProperties(byte[] inputSignature, InputElement[] inputElements, int stride)
	{
		InputSignature = inputSignature;
		InputElements = inputElements;
		Stride = stride;
	}

	internal void __MarshalFree(ref __Native @ref)
	{
	}

	internal void __MarshalFrom(ref __Native @ref)
	{
		ShaderBufferWithInputSignature = @ref.ShaderBufferWithInputSignature;
		ShaderBufferSize = @ref.ShaderBufferSize;
		InputElementsPointer = @ref.InputElementsPointer;
		ElementCount = @ref.ElementCount;
		Stride = @ref.Stride;
	}

	internal void __MarshalTo(ref __Native @ref)
	{
		@ref.ShaderBufferWithInputSignature = ShaderBufferWithInputSignature;
		@ref.ShaderBufferSize = ShaderBufferSize;
		@ref.InputElementsPointer = InputElementsPointer;
		@ref.ElementCount = ElementCount;
		@ref.Stride = Stride;
	}
}
