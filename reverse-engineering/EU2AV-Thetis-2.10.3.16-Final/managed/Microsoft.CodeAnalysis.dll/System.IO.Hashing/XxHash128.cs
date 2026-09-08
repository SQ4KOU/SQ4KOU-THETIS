using System.Buffers.Binary;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.IO.Hashing;

internal sealed class XxHash128 : NonCryptographicHashAlgorithm
{
	[DebuggerDisplay("Low64 = {Low64}, High64 = {High64}")]
	private readonly struct Hash128(ulong low64, ulong high64)
	{
		public readonly ulong Low64 = low64;

		public readonly ulong High64 = high64;
	}

	private new const int HashLengthInBytes = 16;

	private XxHashShared.State _state;

	public XxHash128()
		: this(0L)
	{
	}

	public XxHash128(long seed)
		: base(16)
	{
		XxHashShared.Initialize(ref _state, (ulong)seed);
	}

	public static byte[] Hash(byte[] source)
	{
		return Hash(source, 0L);
	}

	public static byte[] Hash(byte[] source, long seed)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return Hash(new ReadOnlySpan<byte>(source), seed);
	}

	public static byte[] Hash(ReadOnlySpan<byte> source, long seed = 0L)
	{
		byte[] array = new byte[16];
		Hash(source, array, seed);
		return array;
	}

	public static int Hash(ReadOnlySpan<byte> source, Span<byte> destination, long seed = 0L)
	{
		if (!TryHash(source, destination, out var bytesWritten, seed))
		{
			NonCryptographicHashAlgorithm.ThrowDestinationTooShort();
		}
		return bytesWritten;
	}

	public static bool TryHash(ReadOnlySpan<byte> source, Span<byte> destination, out int bytesWritten, long seed = 0L)
	{
		if (destination.Length >= 16)
		{
			WriteBigEndian128(HashToHash128(source, seed), destination);
			bytesWritten = 16;
			return true;
		}
		bytesWritten = 0;
		return false;
	}

	private unsafe static Hash128 HashToHash128(ReadOnlySpan<byte> source, long seed = 0L)
	{
		uint length = (uint)source.Length;
		fixed (byte* reference = &MemoryMarshal.GetReference(source))
		{
			if (length <= 16)
			{
				return HashLength0To16(reference, length, (ulong)seed);
			}
			if (length <= 128)
			{
				return HashLength17To128(reference, length, (ulong)seed);
			}
			if (length <= 240)
			{
				return HashLength129To240(reference, length, (ulong)seed);
			}
			return HashLengthOver240(reference, length, (ulong)seed);
		}
	}

	public override void Reset()
	{
		XxHashShared.Reset(ref _state);
	}

	public override void Append(ReadOnlySpan<byte> source)
	{
		XxHashShared.Append(ref _state, source);
	}

	protected override void GetCurrentHashCore(Span<byte> destination)
	{
		WriteBigEndian128(GetCurrentHashAsHash128(), destination);
	}

	private unsafe Hash128 GetCurrentHashAsHash128()
	{
		Hash128 result;
		if (_state.TotalLength > 240)
		{
			ulong* accumulators = stackalloc ulong[8];
			XxHashShared.CopyAccumulators(ref _state, accumulators);
			fixed (byte* secret = _state.Secret)
			{
				XxHashShared.DigestLong(ref _state, accumulators, secret);
				result = new Hash128(XxHashShared.MergeAccumulators(accumulators, secret + 11, _state.TotalLength * 11400714785074694791uL), XxHashShared.MergeAccumulators(accumulators, secret + 192 - 64 - 11, (ulong)(~((long)_state.TotalLength * -4417276706812531889L))));
			}
		}
		else
		{
			fixed (byte* buffer = _state.Buffer)
			{
				result = HashToHash128(new ReadOnlySpan<byte>(buffer, (int)_state.TotalLength), (long)_state.Seed);
			}
		}
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void WriteBigEndian128(in Hash128 hash, Span<byte> destination)
	{
		ulong value = hash.Low64;
		ulong value2 = hash.High64;
		if (BitConverter.IsLittleEndian)
		{
			value = BinaryPrimitives.ReverseEndianness(value);
			value2 = BinaryPrimitives.ReverseEndianness(value2);
		}
		ref byte reference = ref MemoryMarshal.GetReference(destination);
		Unsafe.WriteUnaligned(ref reference, value2);
		Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref reference, new IntPtr(8)), value);
	}

	private unsafe static Hash128 HashLength0To16(byte* source, uint length, ulong seed)
	{
		switch (length)
		{
		default:
			return HashLength9To16(source, length, seed);
		case 4u:
		case 5u:
		case 6u:
		case 7u:
		case 8u:
			return HashLength4To8(source, length, seed);
		case 1u:
		case 2u:
		case 3u:
			return HashLength1To3(source, length, seed);
		case 0u:
			return new Hash128(XxHash64.Avalanche(seed ^ 0x682E908A3037F8B4L), XxHash64.Avalanche(seed ^ 0x9655D30BD1A77D49uL));
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private unsafe static Hash128 HashLength1To3(byte* source, uint length, ulong seed)
	{
		byte num = *source;
		byte b = source[length >> 1];
		byte b2 = source[length - 1];
		int num2 = (num << 16) | (b << 24) | b2 | (int)(length << 8);
		uint num3 = RuntimeBitOperations.RotateLeft(BinaryPrimitives.ReverseEndianness((uint)num2), 13);
		ulong num4 = 2267503259u + seed;
		ulong num5 = 808198283 - seed;
		ulong hash = (uint)num2 ^ num4;
		ulong hash2 = num3 ^ num5;
		return new Hash128(XxHash64.Avalanche(hash), XxHash64.Avalanche(hash2));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private unsafe static Hash128 HashLength4To8(byte* source, uint length, ulong seed)
	{
		seed ^= (ulong)BinaryPrimitives.ReverseEndianness((uint)seed) << 32;
		uint num = XxHashShared.ReadUInt32LE(source);
		uint num2 = XxHashShared.ReadUInt32LE(source + length - 4);
		ulong num3 = num + ((ulong)num2 << 32);
		ulong num4 = 14190881133394760876uL + seed;
		ulong num5 = XxHashShared.Multiply64To128(num3 ^ num4, (ulong)(-7046029288634856825L + (length << 2)), out var lower);
		num5 += lower << 1;
		lower ^= num5 >> 3;
		lower = XxHashShared.XorShift(lower, 35);
		lower *= 11507291218515648293uL;
		lower = XxHashShared.XorShift(lower, 28);
		num5 = XxHashShared.Avalanche(num5);
		return new Hash128(lower, num5);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private unsafe static Hash128 HashLength9To16(byte* source, uint length, ulong seed)
	{
		ulong num = 6455697860950631241L - seed;
		ulong num2 = 13979869743488056664uL + seed;
		ulong num3 = XxHashShared.ReadUInt64LE(source);
		ulong num4 = XxHashShared.ReadUInt64LE(source + length - 8);
		ulong num5 = XxHashShared.Multiply64To128(num3 ^ num4 ^ num, 11400714785074694791uL, out var lower);
		lower += (ulong)(length - 1) << 54;
		num4 ^= num2;
		num5 += ((sizeof(void*) < 8) ? ((num4 & 0xFFFFFFFF00000000uL) + XxHashShared.Multiply32To64((uint)num4, 2246822519u)) : (num4 + XxHashShared.Multiply32To64((uint)num4, 2246822518u)));
		lower ^= BinaryPrimitives.ReverseEndianness(num5);
		ulong num6 = XxHashShared.Multiply64To128(lower, 14029467366897019727uL, out var lower2);
		num6 += (ulong)((long)num5 * -4417276706812531889L);
		lower2 = XxHashShared.Avalanche(lower2);
		num6 = XxHashShared.Avalanche(num6);
		return new Hash128(lower2, num6);
	}

	private unsafe static Hash128 HashLength17To128(byte* source, uint length, ulong seed)
	{
		ulong accLow = (ulong)(length * -7046029288634856825L);
		ulong accHigh = 0uL;
		switch ((length - 1) / 32)
		{
		default:
			Mix32Bytes(ref accLow, ref accHigh, source + 48, source + length - 64, 4554437623014685352uL, 2111919702937427193uL, 3556072174620004746uL, 7238261902898274248uL, seed);
			goto case 2u;
		case 2u:
			Mix32Bytes(ref accLow, ref accHigh, source + 32, source + length - 48, 14627906620379768892uL, 11758427054878871688uL, 5690594596133299313uL, 15613098826807580984uL, seed);
			goto case 1u;
		case 1u:
			Mix32Bytes(ref accLow, ref accHigh, source + 16, source + length - 32, 8711581037947681227uL, 2410270004345854594uL, 10242386182634080440uL, 5487137525590930912uL, seed);
			break;
		case 0u:
			break;
		}
		Mix32Bytes(ref accLow, ref accHigh, source, source + length - 16, 13712233961653862072uL, 2066345149520216444uL, 15823274712020931806uL, 2262974939099578482uL, seed);
		return AvalancheHash(accLow, accHigh, length, seed);
	}

	private unsafe static Hash128 HashLength129To240(byte* source, uint length, ulong seed)
	{
		ulong accLow = (ulong)(length * -7046029288634856825L);
		ulong accHigh = 0uL;
		Mix32Bytes(ref accLow, ref accHigh, source, source + 16, 13712233961653862072uL, 2066345149520216444uL, 15823274712020931806uL, 2262974939099578482uL, seed);
		Mix32Bytes(ref accLow, ref accHigh, source + 32, source + 32 + 16, 8711581037947681227uL, 2410270004345854594uL, 10242386182634080440uL, 5487137525590930912uL, seed);
		Mix32Bytes(ref accLow, ref accHigh, source + 64, source + 64 + 16, 14627906620379768892uL, 11758427054878871688uL, 5690594596133299313uL, 15613098826807580984uL, seed);
		Mix32Bytes(ref accLow, ref accHigh, source + 96, source + 96 + 16, 4554437623014685352uL, 2111919702937427193uL, 3556072174620004746uL, 7238261902898274248uL, seed);
		accLow = XxHashShared.Avalanche(accLow);
		accHigh = XxHashShared.Avalanche(accHigh);
		uint num = (length - 128) / 32;
		if (num != 0)
		{
			Mix32Bytes(ref accLow, ref accHigh, source + 128, source + 128 + 16, 9295848262624092985uL, 7914194659941938988uL, 11835586108195898345uL, 16607528436649670564uL, seed);
			if (num >= 2)
			{
				Mix32Bytes(ref accLow, ref accHigh, source + 160, source + 160 + 16, 15013455763555273806uL, 5046485836271438973uL, 10391458616325699444uL, 5920048007935066598uL, seed);
				if (num == 3)
				{
					Mix32Bytes(ref accLow, ref accHigh, source + 192, source + 192 + 16, 7336514198459093435uL, 5216419214072683403uL, 17228863761319568023uL, 8573350489219836230uL, seed);
				}
			}
		}
		Mix32Bytes(ref accLow, ref accHigh, source + length - 16, source + length - 32, 5695865814404364607uL, 6464017090953185821uL, 8320639771003045937uL, 16992983559143025252uL, 0 - seed);
		return AvalancheHash(accLow, accHigh, length, seed);
	}

	private unsafe static Hash128 HashLengthOver240(byte* source, uint length, ulong seed)
	{
		fixed (byte* reference = &MemoryMarshal.GetReference(XxHashShared.DefaultSecret))
		{
			byte* ptr = reference;
			if (seed != 0L)
			{
				byte* intPtr = stackalloc byte[192];
				XxHashShared.DeriveSecretFromSeed(intPtr, seed);
				ptr = intPtr;
			}
			ulong* accumulators = stackalloc ulong[8];
			XxHashShared.InitializeAccumulators(accumulators);
			XxHashShared.HashInternalLoop(accumulators, source, length, ptr);
			return new Hash128(XxHashShared.MergeAccumulators(accumulators, ptr + 11, (ulong)(length * -7046029288634856825L)), XxHashShared.MergeAccumulators(accumulators, ptr + 192 - 64 - 11, (ulong)(~(length * -4417276706812531889L))));
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static Hash128 AvalancheHash(ulong accLow, ulong accHigh, uint length, ulong seed)
	{
		ulong hash = accLow + accHigh;
		ulong hash2 = (ulong)((long)accLow * -7046029288634856825L + (long)accHigh * -8796714831421723037L + (long)(length - seed) * -4417276706812531889L);
		ulong low = XxHashShared.Avalanche(hash);
		hash2 = 0 - XxHashShared.Avalanche(hash2);
		return new Hash128(low, hash2);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private unsafe static void Mix32Bytes(ref ulong accLow, ref ulong accHigh, byte* input1, byte* input2, ulong secret1, ulong secret2, ulong secret3, ulong secret4, ulong seed)
	{
		accLow += XxHashShared.Mix16Bytes(input1, secret1, secret2, seed);
		accLow ^= XxHashShared.ReadUInt64LE(input2) + XxHashShared.ReadUInt64LE(input2 + 8);
		accHigh += XxHashShared.Mix16Bytes(input2, secret3, secret4, seed);
		accHigh ^= XxHashShared.ReadUInt64LE(input1) + XxHashShared.ReadUInt64LE(input1 + 8);
	}
}
