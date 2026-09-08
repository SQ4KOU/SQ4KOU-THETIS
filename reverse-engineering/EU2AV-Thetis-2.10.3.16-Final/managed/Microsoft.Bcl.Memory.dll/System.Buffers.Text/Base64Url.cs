using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Buffers.Text;

public static class Base64Url
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	private readonly struct Base64UrlDecoderByte : System.Buffers.Text.Base64Helper.IBase64Decoder<byte>
	{
		public ReadOnlySpan<sbyte> DecodingMap => new sbyte[256]
		{
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, 62, -1, -1, 52, 53,
			54, 55, 56, 57, 58, 59, 60, 61, -1, -1,
			-1, -1, -1, -1, -1, 0, 1, 2, 3, 4,
			5, 6, 7, 8, 9, 10, 11, 12, 13, 14,
			15, 16, 17, 18, 19, 20, 21, 22, 23, 24,
			25, -1, -1, -1, -1, 63, -1, 26, 27, 28,
			29, 30, 31, 32, 33, 34, 35, 36, 37, 38,
			39, 40, 41, 42, 43, 44, 45, 46, 47, 48,
			49, 50, 51, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1
		};

		public ReadOnlySpan<uint> VbmiLookup0 => new ReadOnlySpan<uint>(new uint[16]
		{
			2155905152u, 2155905152u, 2155905152u, 2155905152u, 2155905152u, 2155905152u, 2155905152u, 2155905152u, 2155905152u, 2155905152u,
			2155905152u, 2155888256u, 926299444u, 993671480u, 2155887932u, 2155905152u
		});

		public ReadOnlySpan<uint> VbmiLookup1 => new ReadOnlySpan<uint>(new uint[16]
		{
			33620096u, 100992003u, 168364039u, 235736075u, 303108111u, 370480147u, 2149128215u, 1065386112u, 471538304u, 538910237u,
			606282273u, 673654309u, 741026345u, 808398381u, 2150838833u, 2155905152u
		});

		public ReadOnlySpan<sbyte> Avx2LutHigh => new sbyte[32]
		{
			0, 0, 45, 57, 79, 90, 111, 122, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 45, 57,
			79, 90, 111, 122, 0, 0, 0, 0, 0, 0,
			0, 0
		};

		public ReadOnlySpan<sbyte> Avx2LutLow => new sbyte[32]
		{
			1, 1, 45, 48, 65, 80, 97, 112, 1, 1,
			1, 1, 1, 1, 1, 1, 1, 1, 45, 48,
			65, 80, 97, 112, 1, 1, 1, 1, 1, 1,
			1, 1
		};

		public ReadOnlySpan<sbyte> Avx2LutShift => new sbyte[32]
		{
			0, 0, 17, 4, -65, -65, -71, -71, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 17, 4,
			-65, -65, -71, -71, 0, 0, 0, 0, 0, 0,
			0, 0
		};

		public byte MaskSlashOrUnderscore => 95;

		public ReadOnlySpan<int> Vector128LutHigh => new ReadOnlySpan<int>(new int[4] { 959250432, 2054117967, 0, 0 });

		public ReadOnlySpan<int> Vector128LutLow => new ReadOnlySpan<int>(new int[4] { 808255745, 1885425729, 16843009, 16843009 });

		public ReadOnlySpan<uint> Vector128LutShift => new ReadOnlySpan<uint>(new uint[4] { 68222976u, 3115958207u, 0u, 0u });

		public ReadOnlySpan<uint> AdvSimdLutOne3 => new ReadOnlySpan<uint>(new uint[4] { 4294967295u, 4294967295u, 4294967295u, 4294917887u });

		public uint AdvSimdLutTwo3Uint1 => 454754111u;

		public int GetMaxDecodedLength(int sourceLength)
		{
			return Base64Url.GetMaxDecodedLength(sourceLength);
		}

		public bool IsInvalidLength(int bufferLength)
		{
			return (bufferLength & 3) == 1;
		}

		public bool IsValidPadding(uint padChar)
		{
			if (padChar == 37 || padChar == 61)
			{
				return true;
			}
			return false;
		}

		public int SrcLength(bool isFinalBlock, int sourceLength)
		{
			if (!isFinalBlock)
			{
				return sourceLength & -4;
			}
			return sourceLength;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe int DecodeFourElements(byte* source, ref sbyte decodingMap)
		{
			return default(System.Buffers.Text.Base64Helper.Base64DecoderByte).DecodeFourElements(source, ref decodingMap);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe int DecodeRemaining(byte* srcEnd, ref sbyte decodingMap, long remaining, out uint t2, out uint t3)
		{
			return default(System.Buffers.Text.Base64Helper.Base64DecoderByte).DecodeRemaining(srcEnd, ref decodingMap, remaining, out t2, out t3);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int IndexOfAnyExceptWhiteSpace(ReadOnlySpan<byte> span)
		{
			return default(System.Buffers.Text.Base64Helper.Base64DecoderByte).IndexOfAnyExceptWhiteSpace(span);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public OperationStatus DecodeWithWhiteSpaceBlockwiseWrapper<TBase64Decoder>(TBase64Decoder decoder, ReadOnlySpan<byte> utf8, Span<byte> bytes, ref int bytesConsumed, ref int bytesWritten, bool isFinalBlock = true) where TBase64Decoder : System.Buffers.Text.Base64Helper.IBase64Decoder<byte>
		{
			return System.Buffers.Text.Base64Helper.DecodeWithWhiteSpaceBlockwise(decoder, utf8, bytes, ref bytesConsumed, ref bytesWritten, isFinalBlock);
		}
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	private readonly struct Base64UrlDecoderChar : System.Buffers.Text.Base64Helper.IBase64Decoder<ushort>
	{
		public ReadOnlySpan<sbyte> DecodingMap => default(Base64UrlDecoderByte).DecodingMap;

		public ReadOnlySpan<uint> VbmiLookup0 => default(Base64UrlDecoderByte).VbmiLookup0;

		public ReadOnlySpan<uint> VbmiLookup1 => default(Base64UrlDecoderByte).VbmiLookup1;

		public ReadOnlySpan<sbyte> Avx2LutHigh => default(Base64UrlDecoderByte).Avx2LutHigh;

		public ReadOnlySpan<sbyte> Avx2LutLow => default(Base64UrlDecoderByte).Avx2LutLow;

		public ReadOnlySpan<sbyte> Avx2LutShift => default(Base64UrlDecoderByte).Avx2LutShift;

		public byte MaskSlashOrUnderscore => default(Base64UrlDecoderByte).MaskSlashOrUnderscore;

		public ReadOnlySpan<int> Vector128LutHigh => default(Base64UrlDecoderByte).Vector128LutHigh;

		public ReadOnlySpan<int> Vector128LutLow => default(Base64UrlDecoderByte).Vector128LutLow;

		public ReadOnlySpan<uint> Vector128LutShift => default(Base64UrlDecoderByte).Vector128LutShift;

		public ReadOnlySpan<uint> AdvSimdLutOne3 => default(Base64UrlDecoderByte).AdvSimdLutOne3;

		public uint AdvSimdLutTwo3Uint1 => default(Base64UrlDecoderByte).AdvSimdLutTwo3Uint1;

		public int GetMaxDecodedLength(int sourceLength)
		{
			return default(Base64UrlDecoderByte).GetMaxDecodedLength(sourceLength);
		}

		public bool IsInvalidLength(int bufferLength)
		{
			return default(Base64UrlDecoderByte).IsInvalidLength(bufferLength);
		}

		public bool IsValidPadding(uint padChar)
		{
			return default(Base64UrlDecoderByte).IsValidPadding(padChar);
		}

		public int SrcLength(bool isFinalBlock, int sourceLength)
		{
			return default(Base64UrlDecoderByte).SrcLength(isFinalBlock, sourceLength);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe int DecodeFourElements(ushort* source, ref sbyte decodingMap)
		{
			uint num = *source;
			uint num2 = source[1];
			uint num3 = source[2];
			uint num4 = source[3];
			if (((num | num2 | num3 | num4) & 0xFFFFFF00u) != 0)
			{
				return -1;
			}
			sbyte num5 = Unsafe.Add(ref decodingMap, (int)num);
			int num6 = Unsafe.Add(ref decodingMap, (int)num2);
			int num7 = Unsafe.Add(ref decodingMap, (int)num3);
			int num8 = Unsafe.Add(ref decodingMap, (int)num4);
			int num9 = num5 << 18;
			num6 <<= 12;
			num7 <<= 6;
			int num10 = num9 | num8;
			num6 |= num7;
			return num10 | num6;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe int DecodeRemaining(ushort* srcEnd, ref sbyte decodingMap, long remaining, out uint t2, out uint t3)
		{
			t2 = 61u;
			t3 = 61u;
			long num = remaining - 2;
			if ((ulong)num <= 2uL)
			{
				uint num2;
				uint num3;
				sbyte num4;
				int num6;
				int num5;
				switch ((int)num)
				{
				case 0:
					num2 = srcEnd[-2];
					num3 = srcEnd[-1];
					goto IL_0086;
				case 1:
					num2 = srcEnd[-3];
					num3 = srcEnd[-2];
					t2 = srcEnd[-1];
					goto IL_0086;
				case 2:
					{
						num2 = srcEnd[-4];
						num3 = srcEnd[-3];
						t2 = srcEnd[-2];
						t3 = srcEnd[-1];
						goto IL_0086;
					}
					IL_0086:
					if (((num2 | num3 | t2 | t3) & 0xFFFFFF00u) != 0)
					{
						return -1;
					}
					num4 = Unsafe.Add(ref decodingMap, (IntPtr)num2);
					num5 = Unsafe.Add(ref decodingMap, (IntPtr)num3);
					num6 = num4 << 18;
					num5 <<= 12;
					return num6 | num5;
				}
			}
			return -1;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int IndexOfAnyExceptWhiteSpace(ReadOnlySpan<ushort> span)
		{
			for (int i = 0; i < span.Length; i++)
			{
				if (!System.Buffers.Text.Base64Helper.IsWhiteSpace(span[i]))
				{
					return i;
				}
			}
			return -1;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public OperationStatus DecodeWithWhiteSpaceBlockwiseWrapper<TBase64Decoder>(TBase64Decoder decoder, ReadOnlySpan<ushort> source, Span<byte> bytes, ref int bytesConsumed, ref int bytesWritten, bool isFinalBlock = true) where TBase64Decoder : System.Buffers.Text.Base64Helper.IBase64Decoder<ushort>
		{
			return DecodeWithWhiteSpaceBlockwise(decoder, source, bytes, ref bytesConsumed, ref bytesWritten, isFinalBlock);
		}
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	private readonly struct Base64UrlEncoderByte : System.Buffers.Text.Base64Helper.IBase64Encoder<byte>
	{
		public ReadOnlySpan<byte> EncodingMap => "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_"u8;

		public sbyte Avx2LutChar62 => -17;

		public sbyte Avx2LutChar63 => 32;

		public ReadOnlySpan<byte> AdvSimdLut4 => "wxyz0123456789-_"u8;

		public uint Ssse3AdvSimdLutE3 => 8431u;

		public int IncrementPadTwo => 2;

		public int IncrementPadOne => 3;

		public int GetMaxSrcLength(int srcLength, int destLength)
		{
			if (srcLength > 1610612733 || destLength < GetEncodedLength(srcLength))
			{
				return GetMaxDecodedLength(destLength);
			}
			return srcLength;
		}

		public uint GetInPlaceDestinationLength(int encodedLength, int leftOver)
		{
			if (leftOver <= 0)
			{
				return (uint)(encodedLength - 4);
			}
			return (uint)(encodedLength - leftOver - 1);
		}

		public int GetMaxEncodedLength(int srcLength)
		{
			return GetEncodedLength(srcLength);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe void EncodeOneOptionallyPadTwo(byte* oneByte, byte* dest, ref byte encodingMap)
		{
			uint num = (uint)(*oneByte << 8);
			byte b = Unsafe.Add(ref encodingMap, (IntPtr)(num >> 10));
			byte b2 = Unsafe.Add(ref encodingMap, (IntPtr)((num >> 4) & 0x3F));
			ushort value = ((!BitConverter.IsLittleEndian) ? ((ushort)((b << 8) | b2)) : ((ushort)(b | (b2 << 8))));
			Unsafe.WriteUnaligned(dest, value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe void EncodeTwoOptionallyPadOne(byte* twoBytes, byte* dest, ref byte encodingMap)
		{
			byte num = *twoBytes;
			uint num2 = twoBytes[1];
			uint num3 = (uint)(num << 16) | (num2 << 8);
			byte b = Unsafe.Add(ref encodingMap, (IntPtr)(num3 >> 18));
			byte b2 = Unsafe.Add(ref encodingMap, (IntPtr)((num3 >> 12) & 0x3F));
			byte b3 = Unsafe.Add(ref encodingMap, (IntPtr)((num3 >> 6) & 0x3F));
			*dest = b;
			dest[1] = b2;
			dest[2] = b3;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe void EncodeThreeAndWrite(byte* threeBytes, byte* destination, ref byte encodingMap)
		{
			default(System.Buffers.Text.Base64Helper.Base64EncoderByte).EncodeThreeAndWrite(threeBytes, destination, ref encodingMap);
		}
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	private readonly struct Base64UrlEncoderChar : System.Buffers.Text.Base64Helper.IBase64Encoder<ushort>
	{
		public ReadOnlySpan<byte> EncodingMap => default(Base64UrlEncoderByte).EncodingMap;

		public sbyte Avx2LutChar62 => default(Base64UrlEncoderByte).Avx2LutChar62;

		public sbyte Avx2LutChar63 => default(Base64UrlEncoderByte).Avx2LutChar63;

		public ReadOnlySpan<byte> AdvSimdLut4 => default(Base64UrlEncoderByte).AdvSimdLut4;

		public uint Ssse3AdvSimdLutE3 => default(Base64UrlEncoderByte).Ssse3AdvSimdLutE3;

		public int IncrementPadTwo => default(Base64UrlEncoderByte).IncrementPadTwo;

		public int IncrementPadOne => default(Base64UrlEncoderByte).IncrementPadOne;

		public int GetMaxSrcLength(int srcLength, int destLength)
		{
			return default(Base64UrlEncoderByte).GetMaxSrcLength(srcLength, destLength);
		}

		public uint GetInPlaceDestinationLength(int encodedLength, int _)
		{
			return 0u;
		}

		public int GetMaxEncodedLength(int _)
		{
			return 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe void EncodeOneOptionallyPadTwo(byte* oneByte, ushort* dest, ref byte encodingMap)
		{
			uint num = (uint)(*oneByte << 8);
			uint num2 = Unsafe.Add(ref encodingMap, (IntPtr)(num >> 10));
			uint num3 = Unsafe.Add(ref encodingMap, (IntPtr)((num >> 4) & 0x3F));
			uint value = ((!BitConverter.IsLittleEndian) ? ((num2 << 16) | num3) : (num2 | (num3 << 16)));
			Unsafe.WriteUnaligned(dest, value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe void EncodeTwoOptionallyPadOne(byte* twoBytes, ushort* dest, ref byte encodingMap)
		{
			byte num = *twoBytes;
			uint num2 = twoBytes[1];
			uint num3 = (uint)(num << 16) | (num2 << 8);
			ushort num4 = Unsafe.Add(ref encodingMap, (IntPtr)(num3 >> 18));
			ushort num5 = Unsafe.Add(ref encodingMap, (IntPtr)((num3 >> 12) & 0x3F));
			ushort num6 = Unsafe.Add(ref encodingMap, (IntPtr)((num3 >> 6) & 0x3F));
			*dest = num4;
			dest[1] = num5;
			dest[2] = num6;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe void EncodeThreeAndWrite(byte* threeBytes, ushort* destination, ref byte encodingMap)
		{
			byte num = *threeBytes;
			uint num2 = threeBytes[1];
			uint num3 = threeBytes[2];
			uint num4 = (uint)(num << 16) | (num2 << 8) | num3;
			ulong num5 = Unsafe.Add(ref encodingMap, (IntPtr)(num4 >> 18));
			ulong num6 = Unsafe.Add(ref encodingMap, (IntPtr)((num4 >> 12) & 0x3F));
			ulong num7 = Unsafe.Add(ref encodingMap, (IntPtr)((num4 >> 6) & 0x3F));
			ulong num8 = Unsafe.Add(ref encodingMap, (IntPtr)(num4 & 0x3F));
			ulong value = ((!BitConverter.IsLittleEndian) ? ((num5 << 48) | (num6 << 32) | (num7 << 16) | num8) : (num5 | (num6 << 16) | (num7 << 32) | (num8 << 48)));
			Unsafe.WriteUnaligned(destination, value);
		}
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	private readonly struct Base64UrlCharValidatable : System.Buffers.Text.Base64Helper.IBase64Validatable<char>
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int DecodeValue(char value)
		{
			if (value > 'ÿ')
			{
				return -2;
			}
			return default(Base64UrlDecoderByte).DecodingMap[value];
		}

		public bool IsWhiteSpace(char value)
		{
			return System.Buffers.Text.Base64Helper.IsWhiteSpace(value);
		}

		public bool IsEncodingPad(char value)
		{
			if (value != '=')
			{
				return value == '%';
			}
			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool ValidateAndDecodeLength(char lastChar, int length, int paddingCount, out int decodedLength)
		{
			return default(Base64UrlByteValidatable).ValidateAndDecodeLength((byte)lastChar, length, paddingCount, out decodedLength);
		}
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	private readonly struct Base64UrlByteValidatable : System.Buffers.Text.Base64Helper.IBase64Validatable<byte>
	{
		public int DecodeValue(byte value)
		{
			return default(Base64UrlDecoderByte).DecodingMap[value];
		}

		public bool IsWhiteSpace(byte value)
		{
			return System.Buffers.Text.Base64Helper.IsWhiteSpace(value);
		}

		public bool IsEncodingPad(byte value)
		{
			if (value != 61)
			{
				return value == 37;
			}
			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool ValidateAndDecodeLength(byte lastChar, int length, int paddingCount, out int decodedLength)
		{
			int num = (int)((uint)length % 4u);
			if (paddingCount != 0)
			{
				length -= paddingCount;
				num = (int)((uint)length % 4u);
				if (num == 0 || num + paddingCount > 4)
				{
					decodedLength = 0;
					return false;
				}
			}
			decodedLength = (length >> 2) * 3 + ((num > 0) ? (num - 1) : 0);
			if (num > 0)
			{
				int num2 = default(Base64UrlDecoderByte).DecodingMap[lastChar];
				switch (num)
				{
				case 1:
					return false;
				case 2:
					return (num2 & 0xF) == 0;
				case 3:
					return (num2 & 3) == 0;
				}
			}
			return true;
		}
	}

	private const int MaxStackallocThreshold = 256;

	private const uint UrlEncodingPad = 37u;

	public static int GetMaxDecodedLength(int base64Length)
	{
		if (base64Length < 0)
		{
			throw new ArgumentOutOfRangeException("base64Length");
		}
		int num = (int)((uint)base64Length % 4u);
		return (base64Length >> 2) * 3 + ((num > 0) ? (num - 1) : 0);
	}

	public static OperationStatus DecodeFromUtf8(ReadOnlySpan<byte> source, Span<byte> destination, out int bytesConsumed, out int bytesWritten, bool isFinalBlock = true)
	{
		return System.Buffers.Text.Base64Helper.DecodeFrom(default(Base64UrlDecoderByte), source, destination, out bytesConsumed, out bytesWritten, isFinalBlock, ignoreWhiteSpace: true);
	}

	public static int DecodeFromUtf8InPlace(Span<byte> buffer)
	{
		if (System.Buffers.Text.Base64Helper.DecodeFromUtf8InPlace(default(Base64UrlDecoderByte), buffer, out var bytesWritten, ignoreWhiteSpace: true) == OperationStatus.InvalidData)
		{
			throw new FormatException(System.SR.Format_BadBase64Char);
		}
		return bytesWritten;
	}

	public static int DecodeFromUtf8(ReadOnlySpan<byte> source, Span<byte> destination)
	{
		int bytesConsumed;
		int bytesWritten;
		return DecodeFromUtf8(source, destination, out bytesConsumed, out bytesWritten) switch
		{
			OperationStatus.Done => bytesWritten, 
			OperationStatus.DestinationTooSmall => throw new ArgumentException(System.SR.Argument_DestinationTooShort, "destination"), 
			_ => throw new FormatException(System.SR.Format_BadBase64Char), 
		};
	}

	public static bool TryDecodeFromUtf8(ReadOnlySpan<byte> source, Span<byte> destination, out int bytesWritten)
	{
		OperationStatus num = DecodeFromUtf8(source, destination, out var _, out bytesWritten);
		if (num == OperationStatus.InvalidData)
		{
			throw new FormatException(System.SR.Format_BadBase64Char);
		}
		return num == OperationStatus.Done;
	}

	public static byte[] DecodeFromUtf8(ReadOnlySpan<byte> source)
	{
		int maxDecodedLength = GetMaxDecodedLength(source.Length);
		byte[] array = null;
		Span<byte> span = (((uint)maxDecodedLength > 256u) ? ((Span<byte>)(array = ArrayPool<byte>.Shared.Rent(maxDecodedLength))) : stackalloc byte[256]);
		Span<byte> destination = span;
		OperationStatus num = DecodeFromUtf8(source, destination, out var _, out var bytesWritten);
		byte[] result = destination.Slice(0, bytesWritten).ToArray();
		if (array != null)
		{
			ArrayPool<byte>.Shared.Return(array);
		}
		if (num != OperationStatus.Done)
		{
			throw new FormatException(System.SR.Format_BadBase64Char);
		}
		return result;
	}

	public static OperationStatus DecodeFromChars(ReadOnlySpan<char> source, Span<byte> destination, out int charsConsumed, out int bytesWritten, bool isFinalBlock = true)
	{
		return System.Buffers.Text.Base64Helper.DecodeFrom(default(Base64UrlDecoderChar), MemoryMarshal.Cast<char, ushort>(source), destination, out charsConsumed, out bytesWritten, isFinalBlock, ignoreWhiteSpace: true);
	}

	private static OperationStatus DecodeWithWhiteSpaceBlockwise<TBase64Decoder>(TBase64Decoder decoder, ReadOnlySpan<ushort> source, Span<byte> bytes, ref int bytesConsumed, ref int bytesWritten, bool isFinalBlock = true) where TBase64Decoder : System.Buffers.Text.Base64Helper.IBase64Decoder<ushort>
	{
		Span<ushort> span = stackalloc ushort[4];
		OperationStatus operationStatus = OperationStatus.Done;
		while (!source.IsEmpty)
		{
			int i = 0;
			int num = 0;
			int num2 = 0;
			for (; i < source.Length; i++)
			{
				if ((uint)num >= (uint)span.Length)
				{
					break;
				}
				if (System.Buffers.Text.Base64Helper.IsWhiteSpace(source[i]))
				{
					num2++;
					continue;
				}
				span[num] = source[i];
				num++;
			}
			source = source.Slice(i);
			bytesConsumed += num2;
			if (num == 0)
			{
				continue;
			}
			bool flag = ((!(decoder is System.Buffers.Text.Base64Helper.Base64DecoderByte)) ? (source.Length > 1) : (source.Length >= 4));
			bool flag2 = !flag;
			if (flag && GetPaddingCount(decoder, ref span[3]) > 0)
			{
				flag = false;
				flag2 = true;
			}
			if (flag2 && !isFinalBlock)
			{
				flag2 = false;
			}
			operationStatus = System.Buffers.Text.Base64Helper.DecodeFrom(decoder, span.Slice(0, num), bytes, out var bytesConsumed2, out var bytesWritten2, flag2, ignoreWhiteSpace: false);
			bytesConsumed += bytesConsumed2;
			bytesWritten += bytesWritten2;
			if (operationStatus != OperationStatus.Done)
			{
				return operationStatus;
			}
			if (!flag)
			{
				for (int j = 0; j < source.Length; j++)
				{
					if (!System.Buffers.Text.Base64Helper.IsWhiteSpace(source[j]))
					{
						bytesConsumed -= bytesConsumed2;
						bytesWritten -= bytesWritten2;
						return OperationStatus.InvalidData;
					}
					bytesConsumed++;
				}
				break;
			}
			bytes = bytes.Slice(bytesWritten2);
		}
		return operationStatus;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static int GetPaddingCount<TBase64Decoder>(TBase64Decoder decoder, ref ushort ptrToLastElement) where TBase64Decoder : System.Buffers.Text.Base64Helper.IBase64Decoder<ushort>
	{
		int num = 0;
		ushort padChar = ptrToLastElement;
		if (decoder.IsValidPadding(padChar))
		{
			num++;
		}
		if (decoder.IsValidPadding(Unsafe.Subtract(ref ptrToLastElement, 1)))
		{
			num++;
		}
		return num;
	}

	public static int DecodeFromChars(ReadOnlySpan<char> source, Span<byte> destination)
	{
		int charsConsumed;
		int bytesWritten;
		return DecodeFromChars(source, destination, out charsConsumed, out bytesWritten) switch
		{
			OperationStatus.Done => bytesWritten, 
			OperationStatus.DestinationTooSmall => throw new ArgumentException(System.SR.Argument_DestinationTooShort, "destination"), 
			_ => throw new FormatException(System.SR.Format_BadBase64Char), 
		};
	}

	public static bool TryDecodeFromChars(ReadOnlySpan<char> source, Span<byte> destination, out int bytesWritten)
	{
		OperationStatus num = DecodeFromChars(source, destination, out var _, out bytesWritten);
		if (num == OperationStatus.InvalidData)
		{
			throw new FormatException(System.SR.Format_BadBase64Char);
		}
		return num == OperationStatus.Done;
	}

	public static byte[] DecodeFromChars(ReadOnlySpan<char> source)
	{
		int maxDecodedLength = GetMaxDecodedLength(source.Length);
		byte[] array = null;
		Span<byte> span = (((uint)maxDecodedLength > 256u) ? ((Span<byte>)(array = ArrayPool<byte>.Shared.Rent(maxDecodedLength))) : stackalloc byte[256]);
		Span<byte> destination = span;
		OperationStatus num = DecodeFromChars(source, destination, out var _, out var bytesWritten);
		byte[] result = destination.Slice(0, bytesWritten).ToArray();
		if (array != null)
		{
			ArrayPool<byte>.Shared.Return(array);
		}
		if (num != OperationStatus.Done)
		{
			throw new FormatException(System.SR.Format_BadBase64Char);
		}
		return result;
	}

	public static OperationStatus EncodeToUtf8(ReadOnlySpan<byte> source, Span<byte> destination, out int bytesConsumed, out int bytesWritten, bool isFinalBlock = true)
	{
		return System.Buffers.Text.Base64Helper.EncodeTo(default(Base64UrlEncoderByte), source, destination, out bytesConsumed, out bytesWritten, isFinalBlock);
	}

	public static int GetEncodedLength(int bytesLength)
	{
		if ((uint)bytesLength > 1610612733u)
		{
			throw new ArgumentOutOfRangeException("bytesLength");
		}
		int num = (int)((uint)bytesLength % 3u);
		return bytesLength / 3 * 4 + ((num > 0) ? (num + 1) : 0);
	}

	public static int EncodeToUtf8(ReadOnlySpan<byte> source, Span<byte> destination)
	{
		if (EncodeToUtf8(source, destination, out var _, out var bytesWritten) == OperationStatus.Done)
		{
			return bytesWritten;
		}
		throw new ArgumentException(System.SR.Argument_DestinationTooShort, "destination");
	}

	public static byte[] EncodeToUtf8(ReadOnlySpan<byte> source)
	{
		byte[] array = new byte[GetEncodedLength(source.Length)];
		EncodeToUtf8(source, array, out var _, out var _);
		return array;
	}

	public static OperationStatus EncodeToChars(ReadOnlySpan<byte> source, Span<char> destination, out int bytesConsumed, out int charsWritten, bool isFinalBlock = true)
	{
		return System.Buffers.Text.Base64Helper.EncodeTo(default(Base64UrlEncoderChar), source, MemoryMarshal.Cast<char, ushort>(destination), out bytesConsumed, out charsWritten, isFinalBlock);
	}

	public static int EncodeToChars(ReadOnlySpan<byte> source, Span<char> destination)
	{
		if (EncodeToChars(source, destination, out var _, out var charsWritten) == OperationStatus.Done)
		{
			return charsWritten;
		}
		throw new ArgumentException(System.SR.Argument_DestinationTooShort, "destination");
	}

	public static char[] EncodeToChars(ReadOnlySpan<byte> source)
	{
		char[] array = new char[GetEncodedLength(source.Length)];
		EncodeToChars(source, array, out var _, out var _);
		return array;
	}

	public static string EncodeToString(ReadOnlySpan<byte> source)
	{
		char[] array = new char[GetEncodedLength(source.Length)];
		EncodeToChars(source, array, out var _, out var _);
		return new string(array);
	}

	public static bool TryEncodeToChars(ReadOnlySpan<byte> source, Span<char> destination, out int charsWritten)
	{
		int bytesConsumed;
		return EncodeToChars(source, destination, out bytesConsumed, out charsWritten) == OperationStatus.Done;
	}

	public static bool TryEncodeToUtf8(ReadOnlySpan<byte> source, Span<byte> destination, out int bytesWritten)
	{
		int bytesConsumed;
		return EncodeToUtf8(source, destination, out bytesConsumed, out bytesWritten) == OperationStatus.Done;
	}

	public static bool TryEncodeToUtf8InPlace(Span<byte> buffer, int dataLength, out int bytesWritten)
	{
		return System.Buffers.Text.Base64Helper.EncodeToUtf8InPlace(default(Base64UrlEncoderByte), buffer, dataLength, out bytesWritten) == OperationStatus.Done;
	}

	public static bool IsValid(ReadOnlySpan<char> base64UrlText)
	{
		int decodedLength;
		return System.Buffers.Text.Base64Helper.IsValid(default(Base64UrlCharValidatable), base64UrlText, out decodedLength);
	}

	public static bool IsValid(ReadOnlySpan<char> base64UrlText, out int decodedLength)
	{
		return System.Buffers.Text.Base64Helper.IsValid(default(Base64UrlCharValidatable), base64UrlText, out decodedLength);
	}

	public static bool IsValid(ReadOnlySpan<byte> utf8Base64UrlText)
	{
		int decodedLength;
		return System.Buffers.Text.Base64Helper.IsValid(default(Base64UrlByteValidatable), utf8Base64UrlText, out decodedLength);
	}

	public static bool IsValid(ReadOnlySpan<byte> utf8Base64UrlText, out int decodedLength)
	{
		return System.Buffers.Text.Base64Helper.IsValid(default(Base64UrlByteValidatable), utf8Base64UrlText, out decodedLength);
	}
}
