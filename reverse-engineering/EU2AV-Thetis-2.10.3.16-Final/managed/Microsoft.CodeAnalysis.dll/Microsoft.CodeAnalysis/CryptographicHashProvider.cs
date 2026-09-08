using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

internal abstract class CryptographicHashProvider
{
	private ImmutableArray<byte> _lazySHA1Hash;

	private ImmutableArray<byte> _lazySHA256Hash;

	private ImmutableArray<byte> _lazySHA384Hash;

	private ImmutableArray<byte> _lazySHA512Hash;

	private ImmutableArray<byte> _lazyMD5Hash;

	internal const int Sha1HashSize = 20;

	private static readonly byte[] _singleZeroByteArray = new byte[1];

	internal abstract ImmutableArray<byte> ComputeHash(HashAlgorithm algorithm);

	internal ImmutableArray<byte> GetHash(AssemblyHashAlgorithm algorithmId)
	{
		using HashAlgorithm hashAlgorithm = TryGetAlgorithm(algorithmId);
		if (hashAlgorithm == null)
		{
			return ImmutableArray.Create<byte>();
		}
		switch (algorithmId)
		{
		case AssemblyHashAlgorithm.None:
		case AssemblyHashAlgorithm.Sha1:
			return GetHash(ref _lazySHA1Hash, hashAlgorithm);
		case AssemblyHashAlgorithm.Sha256:
			return GetHash(ref _lazySHA256Hash, hashAlgorithm);
		case AssemblyHashAlgorithm.Sha384:
			return GetHash(ref _lazySHA384Hash, hashAlgorithm);
		case AssemblyHashAlgorithm.Sha512:
			return GetHash(ref _lazySHA512Hash, hashAlgorithm);
		case AssemblyHashAlgorithm.MD5:
			return GetHash(ref _lazyMD5Hash, hashAlgorithm);
		default:
			throw ExceptionUtilities.UnexpectedValue(algorithmId);
		}
	}

	internal static int GetHashSize(SourceHashAlgorithm algorithmId)
	{
		return algorithmId switch
		{
			SourceHashAlgorithm.Sha1 => 20, 
			SourceHashAlgorithm.Sha256 => 32, 
			_ => throw ExceptionUtilities.UnexpectedValue(algorithmId), 
		};
	}

	internal static HashAlgorithm? TryGetAlgorithm(SourceHashAlgorithm algorithmId)
	{
		return algorithmId switch
		{
			SourceHashAlgorithm.Sha1 => SHA1.Create(), 
			SourceHashAlgorithm.Sha256 => SHA256.Create(), 
			_ => null, 
		};
	}

	internal static HashAlgorithmName GetAlgorithmName(SourceHashAlgorithm algorithmId)
	{
		return algorithmId switch
		{
			SourceHashAlgorithm.Sha1 => HashAlgorithmName.SHA1, 
			SourceHashAlgorithm.Sha256 => HashAlgorithmName.SHA256, 
			_ => throw ExceptionUtilities.UnexpectedValue(algorithmId), 
		};
	}

	internal static HashAlgorithm? TryGetAlgorithm(AssemblyHashAlgorithm algorithmId)
	{
		switch (algorithmId)
		{
		case AssemblyHashAlgorithm.None:
		case AssemblyHashAlgorithm.Sha1:
			return SHA1.Create();
		case AssemblyHashAlgorithm.Sha256:
			return SHA256.Create();
		case AssemblyHashAlgorithm.Sha384:
			return SHA384.Create();
		case AssemblyHashAlgorithm.Sha512:
			return SHA512.Create();
		case AssemblyHashAlgorithm.MD5:
			return MD5.Create();
		default:
			return null;
		}
	}

	internal static bool IsSupportedAlgorithm(AssemblyHashAlgorithm algorithmId)
	{
		if (algorithmId == AssemblyHashAlgorithm.None || (uint)(algorithmId - 32771) <= 1u || (uint)(algorithmId - 32780) <= 2u)
		{
			return true;
		}
		return false;
	}

	private ImmutableArray<byte> GetHash(ref ImmutableArray<byte> lazyHash, HashAlgorithm algorithm)
	{
		if (lazyHash.IsDefault)
		{
			ImmutableInterlocked.InterlockedCompareExchange(ref lazyHash, ComputeHash(algorithm), default(ImmutableArray<byte>));
		}
		return lazyHash;
	}

	internal static ImmutableArray<byte> ComputeSha1(Stream stream)
	{
		if (stream != null)
		{
			stream.Seek(0L, SeekOrigin.Begin);
			using SHA1 sHA = SHA1.Create();
			return ImmutableArray.Create(sHA.ComputeHash(stream));
		}
		return ImmutableArray<byte>.Empty;
	}

	internal static ImmutableArray<byte> ComputeSha1(ImmutableArray<byte> bytes)
	{
		return ComputeSha1(bytes.ToArray());
	}

	internal static ImmutableArray<byte> ComputeSha1(byte[] bytes)
	{
		using SHA1 sHA = SHA1.Create();
		return ImmutableArray.Create(sHA.ComputeHash(bytes));
	}

	internal static ImmutableArray<byte> ComputeHash(HashAlgorithmName algorithmName, IEnumerable<Blob> bytes)
	{
		using IncrementalHash incrementalHash = IncrementalHash.CreateHash(algorithmName);
		incrementalHash.AppendData(bytes);
		return ImmutableArray.Create(incrementalHash.GetHashAndReset());
	}

	internal static ImmutableArray<byte> ComputeHash(HashAlgorithmName algorithmName, IEnumerable<ArraySegment<byte>> bytes)
	{
		using IncrementalHash incrementalHash = IncrementalHash.CreateHash(algorithmName);
		incrementalHash.AppendData(bytes);
		return ImmutableArray.Create(incrementalHash.GetHashAndReset());
	}

	internal static ImmutableArray<byte> ComputeSourceHash(ImmutableArray<byte> bytes, SourceHashAlgorithm hashAlgorithm = SourceHashAlgorithm.Sha256)
	{
		using IncrementalHash incrementalHash = IncrementalHash.CreateHash(GetAlgorithmName(hashAlgorithm));
		incrementalHash.AppendData(bytes.ToArray());
		return ImmutableArray.Create(incrementalHash.GetHashAndReset());
	}

	internal static ImmutableArray<byte> ComputeSourceHash(ImmutableArray<ConstantValue> constants, SourceHashAlgorithm hashAlgorithm = SourceHashAlgorithm.Sha256)
	{
		using (IncrementalHash incrementalHash = IncrementalHash.CreateHash(GetAlgorithmName(hashAlgorithm)))
		{
			foreach (ConstantValue item in constants)
			{
				incrementalHash.AppendData(getBytes(item));
			}
			return ImmutableArray.Create(incrementalHash.GetHashAndReset());
		}
		static byte[] getBytes(ConstantValue constant)
		{
			switch (constant.Discriminator)
			{
			case ConstantValueTypeDiscriminator.Nothing:
				return _singleZeroByteArray;
			case ConstantValueTypeDiscriminator.String:
				return Encoding.Unicode.GetBytes(constant.StringValue);
			case ConstantValueTypeDiscriminator.NInt:
				return getBytes2(constant.UInt32Value);
			case ConstantValueTypeDiscriminator.NUInt:
				return getBytes2(constant.UInt32Value);
			case ConstantValueTypeDiscriminator.Decimal:
			{
				int[] bits = decimal.GetBits(constant.DecimalValue);
				byte[] array = new byte[16];
				Span<byte> destination = array;
				BinaryPrimitives.WriteInt32LittleEndian(destination, bits[0]);
				BinaryPrimitives.WriteInt32LittleEndian(destination.Slice(4), bits[1]);
				BinaryPrimitives.WriteInt32LittleEndian(destination.Slice(8), bits[2]);
				BinaryPrimitives.WriteInt32LittleEndian(destination.Slice(12), bits[3]);
				return array;
			}
			default:
				throw ExceptionUtilities.UnexpectedValue(constant.Discriminator);
			}
		}
		static byte[] getBytes2(uint value)
		{
			byte[] array = new byte[4];
			BinaryPrimitives.WriteUInt32LittleEndian(array, value);
			return array;
		}
	}

	internal static ImmutableArray<byte> ComputeSourceHash(IEnumerable<Blob> bytes, SourceHashAlgorithm hashAlgorithm = SourceHashAlgorithm.Sha256)
	{
		return ComputeHash(GetAlgorithmName(hashAlgorithm), bytes);
	}
}
