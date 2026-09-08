using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

public struct StreamOutputElement(int streamIndex, string semanticName, int semanticIndex, byte startComponent, byte componentCount, byte outputSlot)
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct __Native
	{
		public int Stream;

		public IntPtr SemanticName;

		public int SemanticIndex;

		public byte StartComponent;

		public byte ComponentCount;

		public byte OutputSlot;
	}

	public int Stream = streamIndex;

	public string SemanticName = semanticName;

	public int SemanticIndex = semanticIndex;

	public byte StartComponent = startComponent;

	public byte ComponentCount = componentCount;

	public byte OutputSlot = outputSlot;

	internal void __MarshalFree(ref __Native @ref)
	{
		Marshal.FreeHGlobal(@ref.SemanticName);
	}

	internal void __MarshalFrom(ref __Native @ref)
	{
		Stream = @ref.Stream;
		SemanticName = Marshal.PtrToStringAnsi(@ref.SemanticName);
		SemanticIndex = @ref.SemanticIndex;
		StartComponent = @ref.StartComponent;
		ComponentCount = @ref.ComponentCount;
		OutputSlot = @ref.OutputSlot;
	}

	internal void __MarshalTo(ref __Native @ref)
	{
		@ref.Stream = Stream;
		@ref.SemanticName = Marshal.StringToHGlobalAnsi(SemanticName);
		@ref.SemanticIndex = SemanticIndex;
		@ref.StartComponent = StartComponent;
		@ref.ComponentCount = ComponentCount;
		@ref.OutputSlot = OutputSlot;
	}
}
