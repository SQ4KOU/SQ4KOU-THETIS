using System;
using System.Runtime.InteropServices;

namespace SharpDX.DirectWrite;

public struct Panose
{
	[StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode)]
	internal struct __Native
	{
		[FieldOffset(0)]
		public byte Values;

		[FieldOffset(1)]
		public byte __Values1;

		[FieldOffset(2)]
		public byte __Values2;

		[FieldOffset(3)]
		public byte __Values3;

		[FieldOffset(4)]
		public byte __Values4;

		[FieldOffset(5)]
		public byte __Values5;

		[FieldOffset(6)]
		public byte __Values6;

		[FieldOffset(7)]
		public byte __Values7;

		[FieldOffset(8)]
		public byte __Values8;

		[FieldOffset(9)]
		public byte __Values9;

		[FieldOffset(0)]
		public byte FamilyKind;

		[FieldOffset(0)]
		public PanoseText Text;

		[FieldOffset(0)]
		public PanoseScript Script;

		[FieldOffset(0)]
		public PanoseDecorative Decorative;

		[FieldOffset(0)]
		public PanoseSymbol Symbol;
	}

	internal byte[] _Values;

	public byte FamilyKind;

	public PanoseText Text;

	public PanoseScript Script;

	public PanoseDecorative Decorative;

	public PanoseSymbol Symbol;

	public byte[] Values
	{
		get
		{
			return _Values ?? (_Values = new byte[10]);
		}
		private set
		{
			_Values = value;
		}
	}

	internal void __MarshalFree(ref __Native @ref)
	{
	}

	internal unsafe void __MarshalFrom(ref __Native @ref)
	{
		fixed (byte* ptr = &Values[0])
		{
			void* ptr2 = ptr;
			fixed (byte* values = &@ref.Values)
			{
				void* ptr3 = values;
				Utilities.CopyMemory((IntPtr)ptr2, (IntPtr)ptr3, 10);
			}
		}
		FamilyKind = @ref.FamilyKind;
		Text = @ref.Text;
		Script = @ref.Script;
		Decorative = @ref.Decorative;
		Symbol = @ref.Symbol;
	}

	internal unsafe void __MarshalTo(ref __Native @ref)
	{
		fixed (byte* ptr = &Values[0])
		{
			void* ptr2 = ptr;
			fixed (byte* values = &@ref.Values)
			{
				void* ptr3 = values;
				Utilities.CopyMemory((IntPtr)ptr3, (IntPtr)ptr2, 10);
			}
		}
		@ref.FamilyKind = FamilyKind;
		@ref.Text = Text;
		@ref.Script = Script;
		@ref.Decorative = Decorative;
		@ref.Symbol = Symbol;
	}
}
