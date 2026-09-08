using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Buffers.Text;

internal static class Base64Helper
{
	internal interface IBase64Encoder<T> where T : unmanaged
	{
		ReadOnlySpan<byte> EncodingMap { get; }

		sbyte Avx2LutChar62 { get; }

		sbyte Avx2LutChar63 { get; }

		ReadOnlySpan<byte> AdvSimdLut4 { get; }

		uint Ssse3AdvSimdLutE3 { get; }

		int IncrementPadTwo { get; }

		int IncrementPadOne { get; }

		int GetMaxSrcLength(int srcLength, int destLength);

		int GetMaxEncodedLength(int srcLength);

		uint GetInPlaceDestinationLength(int encodedLength, int leftOver);

		unsafe void EncodeOneOptionallyPadTwo(byte* oneByte, T* dest, ref byte encodingMap);

		unsafe void EncodeTwoOptionallyPadOne(byte* oneByte, T* dest, ref byte encodingMap);

		unsafe void EncodeThreeAndWrite(byte* threeBytes, T* destination, ref byte encodingMap);
	}

	internal interface IBase64Decoder<T> where T : unmanaged
	{
		ReadOnlySpan<sbyte> DecodingMap { get; }

		ReadOnlySpan<uint> VbmiLookup0 { get; }

		ReadOnlySpan<uint> VbmiLookup1 { get; }

		ReadOnlySpan<sbyte> Avx2LutHigh { get; }

		ReadOnlySpan<sbyte> Avx2LutLow { get; }

		ReadOnlySpan<sbyte> Avx2LutShift { get; }

		byte MaskSlashOrUnderscore { get; }

		ReadOnlySpan<int> Vector128LutHigh { get; }

		ReadOnlySpan<int> Vector128LutLow { get; }

		ReadOnlySpan<uint> Vector128LutShift { get; }

		ReadOnlySpan<uint> AdvSimdLutOne3 { get; }

		uint AdvSimdLutTwo3Uint1 { get; }

		int SrcLength(bool isFinalBlock, int sourceLength);

		int GetMaxDecodedLength(int sourceLength);

		bool IsInvalidLength(int bufferLength);

		bool IsValidPadding(uint padChar);

		unsafe int DecodeFourElements(T* source, ref sbyte decodingMap);

		unsafe int DecodeRemaining(T* srcEnd, ref sbyte decodingMap, long remaining, out uint t2, out uint t3);

		int IndexOfAnyExceptWhiteSpace(ReadOnlySpan<T> span);

		OperationStatus DecodeWithWhiteSpaceBlockwiseWrapper<TTBase64Decoder>(TTBase64Decoder decoder, ReadOnlySpan<T> source, Span<byte> bytes, ref int bytesConsumed, ref int bytesWritten, bool isFinalBlock = true) where TTBase64Decoder : IBase64Decoder<T>;
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	internal readonly struct Base64DecoderByte : IBase64Decoder<byte>
	{
		public ReadOnlySpan<sbyte> DecodingMap => new sbyte[256]
		{
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
			-1, -1, -1, 62, -1, -1, -1, 63, 52, 53,
			54, 55, 56, 57, 58, 59, 60, 61, -1, -1,
			-1, -1, -1, -1, -1, 0, 1, 2, 3, 4,
			5, 6, 7, 8, 9, 10, 11, 12, 13, 14,
			15, 16, 17, 18, 19, 20, 21, 22, 23, 24,
			25, -1, -1, -1, -1, -1, -1, 26, 27, 28,
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
			1048608896u, 1065386112u, 926299444u, 993671480u, 2155887932u, 2155905152u
		});

		public ReadOnlySpan<uint> VbmiLookup1 => new ReadOnlySpan<uint>(new uint[16]
		{
			33620096u, 100992003u, 168364039u, 235736075u, 303108111u, 370480147u, 2149128215u, 2155905152u, 471538304u, 538910237u,
			606282273u, 673654309u, 741026345u, 808398381u, 2150838833u, 2155905152u
		});

		public ReadOnlySpan<sbyte> Avx2LutHigh => new sbyte[32]
		{
			16, 16, 1, 2, 4, 8, 4, 8, 16, 16,
			16, 16, 16, 16, 16, 16, 16, 16, 1, 2,
			4, 8, 4, 8, 16, 16, 16, 16, 16, 16,
			16, 16
		};

		public ReadOnlySpan<sbyte> Avx2LutLow => new sbyte[32]
		{
			21, 17, 17, 17, 17, 17, 17, 17, 17, 17,
			19, 26, 27, 27, 27, 26, 21, 17, 17, 17,
			17, 17, 17, 17, 17, 17, 19, 26, 27, 27,
			27, 26
		};

		public ReadOnlySpan<sbyte> Avx2LutShift => new sbyte[32]
		{
			0, 16, 19, 4, -65, -65, -71, -71, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 16, 19, 4,
			-65, -65, -71, -71, 0, 0, 0, 0, 0, 0,
			0, 0
		};

		public byte MaskSlashOrUnderscore => 47;

		public ReadOnlySpan<int> Vector128LutHigh => new ReadOnlySpan<int>(new int[4] { 33624080, 134481924, 269488144, 269488144 });

		public ReadOnlySpan<int> Vector128LutLow => new ReadOnlySpan<int>(new int[4] { 286331157, 286331153, 437457169, 437984027 });

		public ReadOnlySpan<uint> Vector128LutShift => new ReadOnlySpan<uint>(new uint[4] { 68358144u, 3115958207u, 0u, 0u });

		public ReadOnlySpan<uint> AdvSimdLutOne3 => new ReadOnlySpan<uint>(new uint[4] { 4294967295u, 4294967295u, 1056964607u, 1073741823u });

		public uint AdvSimdLutTwo3Uint1 => 454754303u;

		public int GetMaxDecodedLength(int utf8Length)
		{
			return Base64.GetMaxDecodedFromUtf8Length(utf8Length);
		}

		public bool IsInvalidLength(int bufferLength)
		{
			return bufferLength % 4 != 0;
		}

		public bool IsValidPadding(uint padChar)
		{
			return padChar == 61;
		}

		public int SrcLength(bool _, int utf8Length)
		{
			return utf8Length & -4;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe int DecodeFourElements(byte* source, ref sbyte decodingMap)
		{
			uint elementOffset = *source;
			uint elementOffset2 = source[1];
			uint elementOffset3 = source[2];
			uint elementOffset4 = source[3];
			sbyte num = Unsafe.Add(ref decodingMap, (int)elementOffset);
			int num2 = Unsafe.Add(ref decodingMap, (int)elementOffset2);
			int num3 = Unsafe.Add(ref decodingMap, (int)elementOffset3);
			int num4 = Unsafe.Add(ref decodingMap, (int)elementOffset4);
			int num5 = num << 18;
			num2 <<= 12;
			num3 <<= 6;
			int num6 = num5 | num4;
			num2 |= num3;
			return num6 | num2;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe int DecodeRemaining(byte* srcEnd, ref sbyte decodingMap, long remaining, out uint t2, out uint t3)
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
					goto IL_006b;
				case 1:
					num2 = srcEnd[-3];
					num3 = srcEnd[-2];
					t2 = srcEnd[-1];
					goto IL_006b;
				case 2:
					{
						num2 = srcEnd[-4];
						num3 = srcEnd[-3];
						t2 = srcEnd[-2];
						t3 = srcEnd[-1];
						goto IL_006b;
					}
					IL_006b:
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
		public int IndexOfAnyExceptWhiteSpace(ReadOnlySpan<byte> span)
		{
			for (int i = 0; i < span.Length; i++)
			{
				if (!IsWhiteSpace(span[i]))
				{
					return i;
				}
			}
			return -1;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public OperationStatus DecodeWithWhiteSpaceBlockwiseWrapper<TBase64Decoder>(TBase64Decoder decoder, ReadOnlySpan<byte> utf8, Span<byte> bytes, ref int bytesConsumed, ref int bytesWritten, bool isFinalBlock = true) where TBase64Decoder : IBase64Decoder<byte>
		{
			return DecodeWithWhiteSpaceBlockwise(decoder, utf8, bytes, ref bytesConsumed, ref bytesWritten, isFinalBlock);
		}
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	internal readonly struct Base64EncoderByte : IBase64Encoder<byte>
	{
		public ReadOnlySpan<byte> EncodingMap => "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/"u8;

		public sbyte Avx2LutChar62 => -19;

		public sbyte Avx2LutChar63 => -16;

		public ReadOnlySpan<byte> AdvSimdLut4 => "wxyz0123456789+/"u8;

		public uint Ssse3AdvSimdLutE3 => 61677u;

		public int IncrementPadTwo => 4;

		public int IncrementPadOne => 4;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int GetMaxSrcLength(int srcLength, int destLength)
		{
			if (srcLength > 1610612733 || destLength < Base64.GetMaxEncodedToUtf8Length(srcLength))
			{
				return (destLength >> 2) * 3;
			}
			return srcLength;
		}

		public uint GetInPlaceDestinationLength(int encodedLength, int _)
		{
			return (uint)(encodedLength - 4);
		}

		public int GetMaxEncodedLength(int srcLength)
		{
			return Base64.GetMaxEncodedToUtf8Length(srcLength);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe void EncodeOneOptionallyPadTwo(byte* oneByte, byte* dest, ref byte encodingMap)
		{
			uint num = (uint)(*oneByte << 8);
			byte i = Unsafe.Add(ref encodingMap, (IntPtr)(num >> 10));
			uint i2 = Unsafe.Add(ref encodingMap, (IntPtr)((num >> 4) & 0x3F));
			uint value = ConstructResult(i, i2, 61u, 61u);
			Unsafe.WriteUnaligned(dest, value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe void EncodeTwoOptionallyPadOne(byte* twoBytes, byte* dest, ref byte encodingMap)
		{
			byte num = *twoBytes;
			uint num2 = twoBytes[1];
			uint num3 = (uint)(num << 16) | (num2 << 8);
			byte i = Unsafe.Add(ref encodingMap, (IntPtr)(num3 >> 18));
			uint i2 = Unsafe.Add(ref encodingMap, (IntPtr)((num3 >> 12) & 0x3F));
			uint i3 = Unsafe.Add(ref encodingMap, (IntPtr)((num3 >> 6) & 0x3F));
			uint value = ConstructResult(i, i2, i3, 61u);
			Unsafe.WriteUnaligned(dest, value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe void EncodeThreeAndWrite(byte* threeBytes, byte* destination, ref byte encodingMap)
		{
			uint value = Encode(threeBytes, ref encodingMap);
			Unsafe.WriteUnaligned(destination, value);
		}
	}

	internal interface IBase64Validatable<T>
	{
		int DecodeValue(T value);

		bool IsWhiteSpace(T value);

		bool IsEncodingPad(T value);

		bool ValidateAndDecodeLength(T lastChar, int length, int paddingCount, out int decodedLength);
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	internal readonly struct Base64CharValidatable : IBase64Validatable<char>
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int DecodeValue(char value)
		{
			if (value > 'ÿ')
			{
				return -2;
			}
			return default(Base64DecoderByte).DecodingMap[value];
		}

		public bool IsWhiteSpace(char value)
		{
			return System.Buffers.Text.Base64Helper.IsWhiteSpace((int)value);
		}

		public bool IsEncodingPad(char value)
		{
			return value == '=';
		}

		public bool ValidateAndDecodeLength(char lastChar, int length, int paddingCount, out int decodedLength)
		{
			return default(Base64ByteValidatable).ValidateAndDecodeLength((byte)lastChar, length, paddingCount, out decodedLength);
		}
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	internal readonly struct Base64ByteValidatable : IBase64Validatable<byte>
	{
		public int DecodeValue(byte value)
		{
			return default(Base64DecoderByte).DecodingMap[value];
		}

		public bool IsWhiteSpace(byte value)
		{
			return System.Buffers.Text.Base64Helper.IsWhiteSpace((int)value);
		}

		public bool IsEncodingPad(byte value)
		{
			return value == 61;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool ValidateAndDecodeLength(byte lastChar, int length, int paddingCount, out int decodedLength)
		{
			if (length % 4 == 0)
			{
				int num = default(Base64DecoderByte).DecodingMap[lastChar];
				if ((paddingCount == 1 && (num & 3) != 0) || (paddingCount == 2 && (num & 0xF) != 0))
				{
					decodedLength = 0;
					return false;
				}
				decodedLength = (int)((uint)length / 4u * 3) - paddingCount;
				return true;
			}
			decodedLength = 0;
			return false;
		}
	}

	internal const uint EncodingPad = 61u;

	internal const int MaximumEncodeLength = 1610612733;

	[Conditional("DEBUG")]
	internal unsafe static void AssertRead<TVector>(byte* src, byte* srcStart, int srcLength)
	{
		int num = Unsafe.SizeOf<TVector>();
		byte* num2 = src + num;
		byte* ptr = srcStart + srcLength;
		if (num2 > ptr)
		{
			_ = src - srcStart;
		}
	}

	[Conditional("DEBUG")]
	internal unsafe static void AssertWrite<TVector>(byte* dest, byte* destStart, int destLength)
	{
		int num = Unsafe.SizeOf<TVector>();
		byte* num2 = dest + num;
		byte* ptr = destStart + destLength;
		if (num2 > ptr)
		{
			_ = dest - destStart;
		}
	}

	[Conditional("DEBUG")]
	internal unsafe static void AssertRead<TVector>(ushort* src, ushort* srcStart, int srcLength)
	{
		int num = Unsafe.SizeOf<TVector>();
		ushort* num2 = src + num;
		ushort* ptr = srcStart + srcLength;
		if (num2 > ptr)
		{
			_ = src - srcStart;
		}
	}

	[Conditional("DEBUG")]
	internal unsafe static void AssertWrite<TVector>(ushort* dest, ushort* destStart, int destLength)
	{
		int num = Unsafe.SizeOf<TVector>();
		ushort* num2 = dest + num;
		ushort* ptr = destStart + destLength;
		if (num2 > ptr)
		{
			_ = dest - destStart;
		}
	}

	[DoesNotReturn]
	internal static void ThrowUnreachableException()
	{
		throw new Exception("Unreachable");
	}

	internal unsafe static OperationStatus DecodeFrom<TBase64Decoder, T>(TBase64Decoder decoder, ReadOnlySpan<T> source, Span<byte> bytes, out int bytesConsumed, out int bytesWritten, bool isFinalBlock, bool ignoreWhiteSpace) where TBase64Decoder : IBase64Decoder<T> where T : unmanaged
	{
		if (source.IsEmpty)
		{
			bytesConsumed = 0;
			bytesWritten = 0;
			return OperationStatus.Done;
		}
		fixed (T* reference = &MemoryMarshal.GetReference(source))
		{
			fixed (byte* reference2 = &MemoryMarshal.GetReference(bytes))
			{
				int num = decoder.SrcLength(isFinalBlock, source.Length);
				int length = bytes.Length;
				int num2 = num;
				int maxDecodedLength = decoder.GetMaxDecodedLength(num);
				if (length < maxDecodedLength - 2)
				{
					num2 = length / 3 * 4;
				}
				T* ptr = reference;
				byte* ptr2 = reference2;
				T* ptr3 = reference + (uint)num;
				T* ptr4 = reference + (uint)num2;
				int num3 = (isFinalBlock ? 4 : 0);
				if (length >= maxDecodedLength)
				{
					num2 = num - num3;
				}
				else
				{
					num2 = length / 3 * 4;
					int num4 = (int)((uint)length % 3u);
					if (isFinalBlock && num4 > 0)
					{
						num &= -4;
					}
				}
				ref sbyte reference3 = ref MemoryMarshal.GetReference(decoder.DecodingMap);
				ptr4 = reference + num2;
				while (true)
				{
					if (ptr < ptr4)
					{
						int num5 = decoder.DecodeFourElements(ptr, ref reference3);
						if (num5 < 0)
						{
							break;
						}
						WriteThreeLowOrderBytes(ptr2, num5);
						ptr += 4;
						ptr2 += 3;
						continue;
					}
					if (num2 == num - num3)
					{
						if (ptr == ptr3)
						{
							if (isFinalBlock)
							{
								break;
							}
							if (ptr == reference + source.Length)
							{
								goto IL_02e6;
							}
							bytesConsumed = (int)(ptr - reference);
							bytesWritten = (int)(ptr2 - reference2);
							return OperationStatus.NeedMoreData;
						}
						long num6 = ptr3 - ptr;
						int num7 = decoder.DecodeRemaining(ptr3, ref reference3, num6, out var t, out var t2);
						if (num7 < 0)
						{
							break;
						}
						byte* ptr5 = reference2 + (uint)length;
						uint padChar = t2;
						if (!decoder.IsValidPadding(padChar))
						{
							int num8 = Unsafe.Add(ref reference3, (IntPtr)t);
							int num9 = Unsafe.Add(ref reference3, (IntPtr)t2);
							num8 <<= 6;
							num7 |= num9;
							num7 |= num8;
							if (num7 < 0)
							{
								break;
							}
							if (ptr2 + 3 <= ptr5)
							{
								WriteThreeLowOrderBytes(ptr2, num7);
								ptr2 += 3;
								ptr += 4;
								goto IL_02db;
							}
						}
						else
						{
							uint padChar2 = t;
							if (!decoder.IsValidPadding(padChar2))
							{
								int num10 = Unsafe.Add(ref reference3, (IntPtr)t);
								num10 <<= 6;
								num7 |= num10;
								if ((num7 & 0x800000C0u) != 0L)
								{
									break;
								}
								if (ptr2 + 2 <= ptr5)
								{
									*ptr2 = (byte)(num7 >> 16);
									ptr2[1] = (byte)(num7 >> 8);
									ptr2 += 2;
									ptr += num6;
									goto IL_02db;
								}
							}
							else
							{
								if ((num7 & 0x8000F000u) != 0L)
								{
									break;
								}
								if (ptr2 + 1 <= ptr5)
								{
									*ptr2 = (byte)(num7 >> 16);
									ptr2++;
									ptr += num6;
									goto IL_02db;
								}
							}
						}
					}
					if ((num != source.Length) & isFinalBlock)
					{
						break;
					}
					bytesConsumed = (int)(ptr - reference);
					bytesWritten = (int)(ptr2 - reference2);
					return OperationStatus.DestinationTooSmall;
					IL_02db:
					if (num != source.Length)
					{
						break;
					}
					goto IL_02e6;
					IL_02e6:
					bytesConsumed = (int)(ptr - reference);
					bytesWritten = (int)(ptr2 - reference2);
					return OperationStatus.Done;
				}
				bytesConsumed = (int)(ptr - reference);
				bytesWritten = (int)(ptr2 - reference2);
				if (!ignoreWhiteSpace)
				{
					return OperationStatus.InvalidData;
				}
				return InvalidDataFallback(decoder, source, bytes, ref bytesConsumed, ref bytesWritten, isFinalBlock);
			}
		}
		static OperationStatus InvalidDataFallback(TBase64Decoder val, ReadOnlySpan<T> readOnlySpan, Span<byte> span, ref int reference4, ref int reference5, bool isFinalBlock2)
		{
			readOnlySpan = readOnlySpan.Slice(reference4);
			span = span.Slice(reference5);
			OperationStatus operationStatus;
			do
			{
				ReadOnlySpan<T> span2 = readOnlySpan;
				int bytesConsumed2 = val.IndexOfAnyExceptWhiteSpace(span2);
				if (bytesConsumed2 < 0)
				{
					reference4 += readOnlySpan.Length;
					operationStatus = OperationStatus.Done;
					break;
				}
				if (bytesConsumed2 == 0)
				{
					TBase64Decoder decoder2 = val;
					ReadOnlySpan<T> source2 = readOnlySpan;
					Span<byte> bytes2 = span;
					return val.DecodeWithWhiteSpaceBlockwiseWrapper(decoder2, source2, bytes2, ref reference4, ref reference5, isFinalBlock2);
				}
				reference4 += bytesConsumed2;
				readOnlySpan = readOnlySpan.Slice(bytesConsumed2);
				operationStatus = DecodeFrom(val, readOnlySpan, span, out bytesConsumed2, out var bytesWritten2, isFinalBlock2, ignoreWhiteSpace: false);
				reference4 += bytesConsumed2;
				reference5 += bytesWritten2;
				if (operationStatus != OperationStatus.InvalidData)
				{
					break;
				}
				readOnlySpan = readOnlySpan.Slice(bytesConsumed2);
				span = span.Slice(bytesWritten2);
			}
			while (!readOnlySpan.IsEmpty);
			return operationStatus;
		}
	}

	internal unsafe static OperationStatus DecodeFromUtf8InPlace<TBase64Decoder>(TBase64Decoder decoder, Span<byte> buffer, out int bytesWritten, bool ignoreWhiteSpace) where TBase64Decoder : IBase64Decoder<byte>
	{
		if (buffer.IsEmpty)
		{
			bytesWritten = 0;
			return OperationStatus.Done;
		}
		fixed (byte* reference = &MemoryMarshal.GetReference(buffer))
		{
			uint length = (uint)buffer.Length;
			uint num = 0u;
			uint num2 = 0u;
			if (!decoder.IsInvalidLength(buffer.Length))
			{
				ref sbyte reference2 = ref MemoryMarshal.GetReference(decoder.DecodingMap);
				if (length > 4)
				{
					while (num < length - 4)
					{
						int num3 = decoder.DecodeFourElements(reference + num, ref reference2);
						if (num3 >= 0)
						{
							WriteThreeLowOrderBytes(reference + num2, num3);
							num2 += 3;
							num += 4;
							continue;
						}
						goto IL_022c;
					}
				}
				uint elementOffset;
				uint elementOffset2;
				uint num4;
				uint num5;
				int num7;
				int num6;
				switch (length - num)
				{
				case 2u:
					elementOffset = reference[length - 2];
					elementOffset2 = reference[length - 1];
					num4 = 61u;
					num5 = 61u;
					goto IL_012b;
				case 3u:
					elementOffset = reference[length - 3];
					elementOffset2 = reference[length - 2];
					num4 = reference[length - 1];
					num5 = 61u;
					goto IL_012b;
				case 4u:
					{
						elementOffset = reference[length - 4];
						elementOffset2 = reference[length - 3];
						num4 = reference[length - 2];
						num5 = reference[length - 1];
						goto IL_012b;
					}
					IL_012b:
					num6 = Unsafe.Add(ref reference2, (int)elementOffset);
					num7 = Unsafe.Add(ref reference2, (int)elementOffset2);
					num6 <<= 18;
					num7 <<= 12;
					num6 |= num7;
					if (!decoder.IsValidPadding(num5))
					{
						int num8 = Unsafe.Add(ref reference2, (int)num4);
						int num9 = Unsafe.Add(ref reference2, (int)num5);
						num8 <<= 6;
						num6 |= num9;
						num6 |= num8;
						if (num6 < 0)
						{
							break;
						}
						WriteThreeLowOrderBytes(reference + num2, num6);
						num2 += 3;
					}
					else if (!decoder.IsValidPadding(num4))
					{
						int num10 = Unsafe.Add(ref reference2, (int)num4);
						num10 <<= 6;
						num6 |= num10;
						if ((num6 & 0x800000C0u) != 0L)
						{
							break;
						}
						reference[num2] = (byte)(num6 >> 16);
						reference[num2 + 1] = (byte)(num6 >> 8);
						num2 += 2;
					}
					else
					{
						if ((num6 & 0x8000F000u) != 0L)
						{
							break;
						}
						reference[num2] = (byte)(num6 >> 16);
						num2++;
					}
					bytesWritten = (int)num2;
					return OperationStatus.Done;
				}
			}
			goto IL_022c;
			IL_022c:
			bytesWritten = (int)num2;
			if (!ignoreWhiteSpace)
			{
				return OperationStatus.InvalidData;
			}
			return DecodeWithWhiteSpaceFromUtf8InPlace(decoder, buffer, ref bytesWritten, num);
		}
	}

	internal static OperationStatus DecodeWithWhiteSpaceBlockwise<TBase64Decoder>(TBase64Decoder decoder, ReadOnlySpan<byte> source, Span<byte> bytes, ref int bytesConsumed, ref int bytesWritten, bool isFinalBlock = true) where TBase64Decoder : IBase64Decoder<byte>
	{
		Span<byte> span = stackalloc byte[4];
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
				if (IsWhiteSpace(source[i]))
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
			bool flag = ((!(typeof(TBase64Decoder) == typeof(Base64DecoderByte))) ? (source.Length > 1) : (source.Length >= 4));
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
			operationStatus = DecodeFrom(decoder, span.Slice(0, num), bytes, out var bytesConsumed2, out var bytesWritten2, flag2, ignoreWhiteSpace: false);
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
					if (!IsWhiteSpace(source[j]))
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
	private static int GetPaddingCount<TBase64Decoder>(TBase64Decoder decoder, ref byte ptrToLastElement) where TBase64Decoder : IBase64Decoder<byte>
	{
		int num = 0;
		byte padChar = ptrToLastElement;
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

	private static OperationStatus DecodeWithWhiteSpaceFromUtf8InPlace<TBase64Decoder>(TBase64Decoder decoder, Span<byte> source, ref int destIndex, uint sourceIndex) where TBase64Decoder : IBase64Decoder<byte>
	{
		int num = Math.Min(source.Length - (int)sourceIndex, 4);
		Span<byte> buffer = stackalloc byte[num];
		OperationStatus operationStatus = OperationStatus.Done;
		int num2 = destIndex;
		bool flag = false;
		int bytesWritten = 0;
		while (sourceIndex < (uint)source.Length)
		{
			int num3 = 0;
			while (num3 < num && sourceIndex < (uint)source.Length)
			{
				if (!IsWhiteSpace(source[(int)sourceIndex]))
				{
					buffer[num3] = source[(int)sourceIndex];
					num3++;
				}
				sourceIndex++;
			}
			if (num3 == 0)
			{
				continue;
			}
			if (num3 != 4)
			{
				if (decoder is Base64DecoderByte || num3 == 1)
				{
					operationStatus = OperationStatus.InvalidData;
					break;
				}
				while (num3 < num)
				{
					buffer[num3++] = 61;
				}
			}
			if (flag)
			{
				num2 -= bytesWritten;
				operationStatus = OperationStatus.InvalidData;
				break;
			}
			operationStatus = DecodeFromUtf8InPlace(decoder, buffer, out bytesWritten, ignoreWhiteSpace: false);
			num2 += bytesWritten;
			flag = bytesWritten < 3;
			if (operationStatus != OperationStatus.Done)
			{
				break;
			}
			for (int i = 0; i < bytesWritten; i++)
			{
				source[num2 - bytesWritten + i] = buffer[i];
			}
		}
		destIndex = num2;
		return operationStatus;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private unsafe static void WriteThreeLowOrderBytes(byte* destination, int value)
	{
		*destination = (byte)(value >> 16);
		destination[1] = (byte)(value >> 8);
		destination[2] = (byte)value;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static bool IsWhiteSpace(int value)
	{
		uint num;
		return (int)((uint)(-939523840 << (int)(short)(num = (ushort)(value - 9))) & (num - 32)) < 0;
	}

	internal unsafe static OperationStatus EncodeTo<TBase64Encoder, T>(TBase64Encoder encoder, ReadOnlySpan<byte> source, Span<T> destination, out int bytesConsumed, out int bytesWritten, bool isFinalBlock = true) where TBase64Encoder : IBase64Encoder<T> where T : unmanaged
	{
		if (source.IsEmpty)
		{
			bytesConsumed = 0;
			bytesWritten = 0;
			return OperationStatus.Done;
		}
		fixed (byte* reference = &MemoryMarshal.GetReference(source))
		{
			fixed (T* reference2 = &MemoryMarshal.GetReference(destination))
			{
				int length = source.Length;
				int length2 = destination.Length;
				int maxSrcLength = encoder.GetMaxSrcLength(length, length2);
				byte* ptr = reference;
				T* ptr2 = reference2;
				byte* ptr3 = reference + (uint)length;
				byte* ptr4 = reference + (uint)maxSrcLength;
				ref byte reference3 = ref MemoryMarshal.GetReference(encoder.EncodingMap);
				ptr4 -= 2;
				while (ptr < ptr4)
				{
					encoder.EncodeThreeAndWrite(ptr, ptr2, ref reference3);
					ptr += 3;
					ptr2 += 4;
				}
				if (ptr4 + 2 == ptr3)
				{
					if (!isFinalBlock)
					{
						if (ptr != ptr3)
						{
							bytesConsumed = (int)(ptr - reference);
							bytesWritten = (int)(ptr2 - reference2);
							return OperationStatus.NeedMoreData;
						}
					}
					else if (ptr + 1 == ptr3)
					{
						encoder.EncodeOneOptionallyPadTwo(ptr, ptr2, ref reference3);
						ptr++;
						ptr2 += encoder.IncrementPadTwo;
					}
					else if (ptr + 2 == ptr3)
					{
						encoder.EncodeTwoOptionallyPadOne(ptr, ptr2, ref reference3);
						ptr += 2;
						ptr2 += encoder.IncrementPadOne;
					}
					bytesConsumed = (int)(ptr - reference);
					bytesWritten = (int)(ptr2 - reference2);
					return OperationStatus.Done;
				}
				bytesConsumed = (int)(ptr - reference);
				bytesWritten = (int)(ptr2 - reference2);
				return OperationStatus.DestinationTooSmall;
			}
		}
	}

	internal unsafe static OperationStatus EncodeToUtf8InPlace<TBase64Encoder>(TBase64Encoder encoder, Span<byte> buffer, int dataLength, out int bytesWritten) where TBase64Encoder : IBase64Encoder<byte>
	{
		if (buffer.IsEmpty)
		{
			bytesWritten = 0;
			return OperationStatus.Done;
		}
		fixed (byte* reference = &MemoryMarshal.GetReference(buffer))
		{
			int maxEncodedLength = encoder.GetMaxEncodedLength(dataLength);
			if (buffer.Length < maxEncodedLength)
			{
				bytesWritten = 0;
				return OperationStatus.DestinationTooSmall;
			}
			int num = (int)((uint)dataLength % 3u);
			uint num2 = encoder.GetInPlaceDestinationLength(maxEncodedLength, num);
			uint num3 = (uint)(dataLength - num);
			ref byte reference2 = ref MemoryMarshal.GetReference(encoder.EncodingMap);
			if (num != 0)
			{
				if (num == 1)
				{
					encoder.EncodeOneOptionallyPadTwo(reference + num3, reference + num2, ref reference2);
				}
				else
				{
					encoder.EncodeTwoOptionallyPadOne(reference + num3, reference + num2, ref reference2);
				}
				num2 -= 4;
			}
			num3 -= 3;
			while ((int)num3 >= 0)
			{
				uint value = Encode(reference + num3, ref reference2);
				Unsafe.WriteUnaligned(reference + num2, value);
				num2 -= 4;
				num3 -= 3;
			}
			bytesWritten = maxEncodedLength;
			return OperationStatus.Done;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private unsafe static uint Encode(byte* threeBytes, ref byte encodingMap)
	{
		byte num = *threeBytes;
		uint num2 = threeBytes[1];
		uint num3 = threeBytes[2];
		uint num4 = (uint)(num << 16) | (num2 << 8) | num3;
		byte i = Unsafe.Add(ref encodingMap, (IntPtr)(num4 >> 18));
		uint i2 = Unsafe.Add(ref encodingMap, (IntPtr)((num4 >> 12) & 0x3F));
		uint i3 = Unsafe.Add(ref encodingMap, (IntPtr)((num4 >> 6) & 0x3F));
		uint i4 = Unsafe.Add(ref encodingMap, (IntPtr)(num4 & 0x3F));
		return ConstructResult(i, i2, i3, i4);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static uint ConstructResult(uint i0, uint i1, uint i2, uint i3)
	{
		if (BitConverter.IsLittleEndian)
		{
			return i0 | (i1 << 8) | (i2 << 16) | (i3 << 24);
		}
		return (i0 << 24) | (i1 << 16) | (i2 << 8) | i3;
	}

	internal static bool IsValid<T, TBase64Validatable>(TBase64Validatable validatable, ReadOnlySpan<T> base64Text, out int decodedLength) where T : struct where TBase64Validatable : IBase64Validatable<T>
	{
		int num = 0;
		int num2 = 0;
		T lastChar = default(T);
		if (!base64Text.IsEmpty)
		{
			int num3 = 0;
			while (true)
			{
				if (num3 < base64Text.Length)
				{
					T val = base64Text[num3];
					int num4 = validatable.DecodeValue(val);
					if (num4 == -2)
					{
						break;
					}
					if (num4 >= 0)
					{
						num++;
						lastChar = val;
						goto IL_00d2;
					}
					if (validatable.IsWhiteSpace(val))
					{
						goto IL_00d2;
					}
					if (!validatable.IsEncodingPad(val))
					{
						break;
					}
					num2 = 1;
					for (num3++; num3 < base64Text.Length; num3++)
					{
						T value = base64Text[num3];
						if (validatable.IsEncodingPad(value))
						{
							if (num2 >= 2)
							{
								goto end_IL_00d6;
							}
							num2++;
						}
						else if (!validatable.IsWhiteSpace(value))
						{
							goto end_IL_00d6;
						}
					}
					num += num2;
				}
				if (!validatable.ValidateAndDecodeLength(lastChar, num, num2, out decodedLength))
				{
					break;
				}
				return true;
				IL_00d2:
				num3++;
				continue;
				end_IL_00d6:
				break;
			}
			decodedLength = 0;
			return false;
		}
		decodedLength = 0;
		return true;
	}
}
