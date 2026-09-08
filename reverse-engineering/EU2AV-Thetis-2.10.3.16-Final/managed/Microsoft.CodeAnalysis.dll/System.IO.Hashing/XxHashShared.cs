using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.IO.Hashing;

internal static class XxHashShared
{
	[StructLayout(LayoutKind.Auto)]
	public struct State
	{
		internal unsafe fixed ulong Accumulators[8];

		internal unsafe fixed byte Secret[192];

		internal unsafe fixed byte Buffer[256];

		internal uint BufferedCount;

		internal ulong StripesProcessedInCurrentBlock;

		internal ulong TotalLength;

		internal ulong Seed;
	}

	public const int StripeLengthBytes = 64;

	public const int SecretLengthBytes = 192;

	public const int SecretSizeMin = 136;

	public const int SecretLastAccStartBytes = 7;

	public const int SecretConsumeRateBytes = 8;

	public const int SecretMergeAccsStartBytes = 11;

	public const int NumStripesPerBlock = 16;

	public const int AccumulatorCount = 8;

	public const int MidSizeMaxBytes = 240;

	public const int InternalBufferStripes = 4;

	public const int InternalBufferLengthBytes = 256;

	public const ulong DefaultSecretUInt64_0 = 13712233961653862072uL;

	public const ulong DefaultSecretUInt64_1 = 2066345149520216444uL;

	public const ulong DefaultSecretUInt64_2 = 15823274712020931806uL;

	public const ulong DefaultSecretUInt64_3 = 2262974939099578482uL;

	public const ulong DefaultSecretUInt64_4 = 8711581037947681227uL;

	public const ulong DefaultSecretUInt64_5 = 2410270004345854594uL;

	public const ulong DefaultSecretUInt64_6 = 10242386182634080440uL;

	public const ulong DefaultSecretUInt64_7 = 5487137525590930912uL;

	public const ulong DefaultSecretUInt64_8 = 14627906620379768892uL;

	public const ulong DefaultSecretUInt64_9 = 11758427054878871688uL;

	public const ulong DefaultSecretUInt64_10 = 5690594596133299313uL;

	public const ulong DefaultSecretUInt64_11 = 15613098826807580984uL;

	public const ulong DefaultSecretUInt64_12 = 4554437623014685352uL;

	public const ulong DefaultSecretUInt64_13 = 2111919702937427193uL;

	public const ulong DefaultSecretUInt64_14 = 3556072174620004746uL;

	public const ulong DefaultSecretUInt64_15 = 7238261902898274248uL;

	public const ulong DefaultSecret3UInt64_0 = 9295848262624092985uL;

	public const ulong DefaultSecret3UInt64_1 = 7914194659941938988uL;

	public const ulong DefaultSecret3UInt64_2 = 11835586108195898345uL;

	public const ulong DefaultSecret3UInt64_3 = 16607528436649670564uL;

	public const ulong DefaultSecret3UInt64_4 = 15013455763555273806uL;

	public const ulong DefaultSecret3UInt64_5 = 5046485836271438973uL;

	public const ulong DefaultSecret3UInt64_6 = 10391458616325699444uL;

	public const ulong DefaultSecret3UInt64_7 = 5920048007935066598uL;

	public const ulong DefaultSecret3UInt64_8 = 7336514198459093435uL;

	public const ulong DefaultSecret3UInt64_9 = 5216419214072683403uL;

	public const ulong DefaultSecret3UInt64_10 = 17228863761319568023uL;

	public const ulong DefaultSecret3UInt64_11 = 8573350489219836230uL;

	public const ulong DefaultSecret3UInt64_12 = 13536968629829821247uL;

	public const ulong DefaultSecret3UInt64_13 = 16163852396094277575uL;

	public const ulong Prime64_1 = 11400714785074694791uL;

	public const ulong Prime64_2 = 14029467366897019727uL;

	public const ulong Prime64_3 = 1609587929392839161uL;

	public const ulong Prime64_4 = 9650029242287828579uL;

	public const ulong Prime64_5 = 2870177450012600261uL;

	public const uint Prime32_1 = 2654435761u;

	public const uint Prime32_2 = 2246822519u;

	public const uint Prime32_3 = 3266489917u;

	public const uint Prime32_4 = 668265263u;

	public const uint Prime32_5 = 374761393u;

	public static ReadOnlySpan<byte> DefaultSecret => new byte[192]
	{
		184, 254, 108, 57, 35, 164, 75, 190, 124, 1,
		129, 44, 247, 33, 173, 28, 222, 212, 109, 233,
		131, 144, 151, 219, 114, 64, 164, 164, 183, 179,
		103, 31, 203, 121, 230, 78, 204, 192, 229, 120,
		130, 90, 208, 125, 204, 255, 114, 33, 184, 8,
		70, 116, 247, 67, 36, 142, 224, 53, 144, 230,
		129, 58, 38, 76, 60, 40, 82, 187, 145, 195,
		0, 203, 136, 208, 101, 139, 27, 83, 46, 163,
		113, 100, 72, 151, 162, 13, 249, 78, 56, 25,
		239, 70, 169, 222, 172, 216, 168, 250, 118, 63,
		227, 156, 52, 63, 249, 220, 187, 199, 199, 11,
		79, 29, 138, 81, 224, 75, 205, 180, 89, 49,
		200, 159, 126, 201, 217, 120, 115, 100, 234, 197,
		172, 131, 52, 211, 235, 195, 197, 129, 160, 255,
		250, 19, 99, 235, 23, 13, 221, 81, 183, 240,
		218, 73, 211, 22, 85, 38, 41, 212, 104, 158,
		43, 22, 190, 88, 125, 71, 161, 252, 143, 248,
		184, 209, 122, 208, 49, 206, 69, 203, 58, 143,
		149, 22, 4, 40, 175, 215, 251, 202, 187, 75,
		64, 126
	};

	public unsafe static void Initialize(ref State state, ulong seed)
	{
		state.Seed = seed;
		fixed (byte* secret = state.Secret)
		{
			if (seed == 0L)
			{
				DefaultSecret.CopyTo(new Span<byte>(secret, 192));
			}
			else
			{
				DeriveSecretFromSeed(secret, seed);
			}
		}
		Reset(ref state);
	}

	public unsafe static void Reset(ref State state)
	{
		state.BufferedCount = 0u;
		state.StripesProcessedInCurrentBlock = 0uL;
		state.TotalLength = 0uL;
		fixed (ulong* accumulators = state.Accumulators)
		{
			InitializeAccumulators(accumulators);
		}
	}

	public unsafe static void HashInternalLoop(ulong* accumulators, byte* source, uint length, byte* secret)
	{
		int num = (int)((length - 1) / 1024);
		Accumulate(accumulators, source, secret, 16, scramble: true, num);
		int num2 = 1024 * num;
		int stripesToProcess = (int)((length - 1 - num2) / 64);
		Accumulate(accumulators, source + num2, secret, stripesToProcess);
		Accumulate512(accumulators, source + length - 64, secret + 121);
	}

	public unsafe static void ConsumeStripes(ulong* accumulators, ref ulong stripesSoFar, ulong stripesPerBlock, byte* source, ulong stripes, byte* secret)
	{
		ulong num = stripesPerBlock - stripesSoFar;
		if (num <= stripes)
		{
			ulong num2 = stripes - num;
			Accumulate(accumulators, source, secret + (int)stripesSoFar * 8, (int)num);
			ScrambleAccumulators(accumulators, secret + 128);
			Accumulate(accumulators, source + (int)num * 64, secret, (int)num2);
			stripesSoFar = num2;
		}
		else
		{
			Accumulate(accumulators, source, secret + (int)stripesSoFar * 8, (int)stripes);
			stripesSoFar += stripes;
		}
	}

	public unsafe static void Append(ref State state, ReadOnlySpan<byte> source)
	{
		state.TotalLength += (uint)source.Length;
		fixed (byte* buffer = state.Buffer)
		{
			if (source.Length <= 256 - state.BufferedCount)
			{
				source.CopyTo(new Span<byte>(buffer + state.BufferedCount, source.Length));
				state.BufferedCount += (uint)source.Length;
				return;
			}
			fixed (byte* secret = state.Secret)
			{
				fixed (ulong* accumulators = state.Accumulators)
				{
					fixed (byte* reference = &MemoryMarshal.GetReference(source))
					{
						int num = 0;
						if (state.BufferedCount != 0)
						{
							int num2 = (int)(256 - state.BufferedCount);
							source.Slice(0, num2).CopyTo(new Span<byte>(buffer + state.BufferedCount, num2));
							num = num2;
							ConsumeStripes(accumulators, ref state.StripesProcessedInCurrentBlock, 16uL, buffer, 4uL, secret);
							state.BufferedCount = 0u;
						}
						if (source.Length - num > 1024)
						{
							ulong num3 = (ulong)(source.Length - num - 1) / 64uL;
							ulong num4 = 16 - state.StripesProcessedInCurrentBlock;
							Accumulate(accumulators, reference + num, secret + (int)state.StripesProcessedInCurrentBlock * 8, (int)num4);
							ScrambleAccumulators(accumulators, secret + 128);
							state.StripesProcessedInCurrentBlock = 0uL;
							num += (int)num4 * 64;
							for (num3 -= num4; num3 >= 16; num3 -= 16)
							{
								Accumulate(accumulators, reference + num, secret, 16);
								ScrambleAccumulators(accumulators, secret + 128);
								num += 1024;
							}
							Accumulate(accumulators, reference + num, secret, (int)num3);
							num += (int)num3 * 64;
							state.StripesProcessedInCurrentBlock = num3;
							source.Slice(num - 64, 64).CopyTo(new Span<byte>(buffer + 256 - 64, 64));
						}
						else if (source.Length - num > 256)
						{
							do
							{
								ConsumeStripes(accumulators, ref state.StripesProcessedInCurrentBlock, 16uL, reference + num, 4uL, secret);
								num += 256;
							}
							while (source.Length - num > 256);
							source.Slice(num - 64, 64).CopyTo(new Span<byte>(buffer + 256 - 64, 64));
						}
						Span<byte> destination = new Span<byte>(buffer, source.Length - num);
						source.Slice(num).CopyTo(destination);
						state.BufferedCount = (uint)destination.Length;
					}
				}
			}
		}
	}

	public unsafe static void CopyAccumulators(ref State state, ulong* accumulators)
	{
		fixed (ulong* accumulators2 = state.Accumulators)
		{
			for (int i = 0; i < 8; i++)
			{
				accumulators[i] = accumulators2[i];
			}
		}
	}

	public unsafe static void DigestLong(ref State state, ulong* accumulators, byte* secret)
	{
		fixed (byte* buffer = state.Buffer)
		{
			byte* source;
			if (state.BufferedCount >= 64)
			{
				uint num = (state.BufferedCount - 1) / 64;
				ulong stripesSoFar = state.StripesProcessedInCurrentBlock;
				ConsumeStripes(accumulators, ref stripesSoFar, 16uL, buffer, num, secret);
				source = buffer + state.BufferedCount - 64;
			}
			else
			{
				byte* ptr = stackalloc byte[64];
				int num2 = (int)(64 - state.BufferedCount);
				new ReadOnlySpan<byte>(buffer + 256 - num2, num2).CopyTo(new Span<byte>(ptr, 64));
				new ReadOnlySpan<byte>(buffer, (int)state.BufferedCount).CopyTo(new Span<byte>(ptr + num2, (int)state.BufferedCount));
				source = ptr;
			}
			Accumulate512(accumulators, source, secret + 121);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static void InitializeAccumulators(ulong* accumulators)
	{
		*accumulators = 3266489917uL;
		accumulators[1] = 11400714785074694791uL;
		accumulators[2] = 14029467366897019727uL;
		accumulators[3] = 1609587929392839161uL;
		accumulators[4] = 9650029242287828579uL;
		accumulators[5] = 2246822519uL;
		accumulators[6] = 2870177450012600261uL;
		accumulators[7] = 2654435761uL;
	}

	public unsafe static ulong MergeAccumulators(ulong* accumulators, byte* secret, ulong start)
	{
		return Avalanche(start + Multiply64To128ThenFold(*accumulators ^ ReadUInt64LE(secret), accumulators[1] ^ ReadUInt64LE(secret + 8)) + Multiply64To128ThenFold(accumulators[2] ^ ReadUInt64LE(secret + 16), accumulators[3] ^ ReadUInt64LE(secret + 24)) + Multiply64To128ThenFold(accumulators[4] ^ ReadUInt64LE(secret + 32), accumulators[5] ^ ReadUInt64LE(secret + 40)) + Multiply64To128ThenFold(accumulators[6] ^ ReadUInt64LE(secret + 48), accumulators[7] ^ ReadUInt64LE(secret + 56)));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static ulong Mix16Bytes(byte* source, ulong secretLow, ulong secretHigh, ulong seed)
	{
		return Multiply64To128ThenFold(ReadUInt64LE(source) ^ (secretLow + seed), ReadUInt64LE(source + 8) ^ (secretHigh - seed));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ulong Multiply32To64(uint v1, uint v2)
	{
		return (ulong)v1 * (ulong)v2;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ulong Avalanche(ulong hash)
	{
		hash = XorShift(hash, 37);
		hash *= 1609587791953885689L;
		hash = XorShift(hash, 32);
		return hash;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ulong Multiply64To128(ulong left, ulong right, out ulong lower)
	{
		ulong num = Multiply32To64((uint)left, (uint)right);
		ulong num2 = Multiply32To64((uint)(left >> 32), (uint)right);
		ulong num3 = Multiply32To64((uint)left, (uint)(right >> 32));
		ulong num4 = Multiply32To64((uint)(left >> 32), (uint)(right >> 32));
		ulong num5 = (num >> 32) + (num2 & 0xFFFFFFFFu) + num3;
		ulong result = (num2 >> 32) + (num5 >> 32) + num4;
		lower = (num5 << 32) | (num & 0xFFFFFFFFu);
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ulong Multiply64To128ThenFold(ulong left, ulong right)
	{
		ulong num = Multiply64To128(left, right, out var lower);
		return lower ^ num;
	}

	public unsafe static void DeriveSecretFromSeed(byte* destinationSecret, ulong seed)
	{
		fixed (byte* reference = &MemoryMarshal.GetReference(DefaultSecret))
		{
			for (int i = 0; i < 192; i += 16)
			{
				WriteUInt64LE(destinationSecret + i, ReadUInt64LE(reference + i) + seed);
				WriteUInt64LE(destinationSecret + i + 8, ReadUInt64LE(reference + i + 8) - seed);
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private unsafe static void Accumulate(ulong* accumulators, byte* source, byte* secret, int stripesToProcess, bool scramble = false, int blockCount = 1)
	{
		byte* secret2 = secret + 128;
		for (int i = 0; i < blockCount; i++)
		{
			for (int j = 0; j < stripesToProcess; j++)
			{
				Accumulate512Inlined(accumulators, source, secret + j * 8);
				source += 64;
			}
			if (scramble)
			{
				ScrambleAccumulators(accumulators, secret2);
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public unsafe static void Accumulate512(ulong* accumulators, byte* source, byte* secret)
	{
		Accumulate512Inlined(accumulators, source, secret);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private unsafe static void Accumulate512Inlined(ulong* accumulators, byte* source, byte* secret)
	{
		for (int i = 0; i < 8; i++)
		{
			ulong num = ReadUInt64LE(source + 8 * i);
			ulong num2 = num ^ ReadUInt64LE(secret + i * 8);
			accumulators[i ^ 1] += num;
			accumulators[i] += Multiply32To64((uint)num2, (uint)(num2 >> 32));
		}
	}

	private unsafe static void ScrambleAccumulators(ulong* accumulators, byte* secret)
	{
		for (int i = 0; i < 8; i++)
		{
			ulong num = XorShift(*accumulators, 47) ^ ReadUInt64LE(secret);
			*accumulators = num * 2654435761u;
			accumulators++;
			secret += 8;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ulong XorShift(ulong value, int shift)
	{
		return value ^ (value >> shift);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static uint ReadUInt32LE(byte* data)
	{
		if (!BitConverter.IsLittleEndian)
		{
			return BinaryPrimitives.ReverseEndianness(Unsafe.ReadUnaligned<uint>(data));
		}
		return Unsafe.ReadUnaligned<uint>(data);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static ulong ReadUInt64LE(byte* data)
	{
		if (!BitConverter.IsLittleEndian)
		{
			return BinaryPrimitives.ReverseEndianness(Unsafe.ReadUnaligned<ulong>(data));
		}
		return Unsafe.ReadUnaligned<ulong>(data);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private unsafe static void WriteUInt64LE(byte* data, ulong value)
	{
		if (!BitConverter.IsLittleEndian)
		{
			value = BinaryPrimitives.ReverseEndianness(value);
		}
		Unsafe.WriteUnaligned(data, value);
	}
}
