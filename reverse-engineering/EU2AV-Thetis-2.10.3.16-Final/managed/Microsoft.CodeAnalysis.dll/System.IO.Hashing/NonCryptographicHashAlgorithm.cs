using System.Buffers;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace System.IO.Hashing;

internal abstract class NonCryptographicHashAlgorithm
{
	public int HashLengthInBytes { get; }

	protected NonCryptographicHashAlgorithm(int hashLengthInBytes)
	{
		if (hashLengthInBytes < 1)
		{
			throw new ArgumentOutOfRangeException("hashLengthInBytes");
		}
		HashLengthInBytes = hashLengthInBytes;
	}

	public abstract void Append(ReadOnlySpan<byte> source);

	public abstract void Reset();

	protected abstract void GetCurrentHashCore(Span<byte> destination);

	public void Append(byte[] source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		Append(new ReadOnlySpan<byte>(source));
	}

	public void Append(Stream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		byte[] array = ArrayPool<byte>.Shared.Rent(4096);
		while (true)
		{
			int num = stream.Read(array, 0, array.Length);
			if (num == 0)
			{
				break;
			}
			Append(new ReadOnlySpan<byte>(array, 0, num));
		}
		ArrayPool<byte>.Shared.Return(array);
	}

	public Task AppendAsync(Stream stream, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		return AppendAsyncCore(stream, cancellationToken);
	}

	private async Task AppendAsyncCore(Stream stream, CancellationToken cancellationToken)
	{
		byte[] buffer = ArrayPool<byte>.Shared.Rent(4096);
		while (true)
		{
			int num = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			if (num == 0)
			{
				break;
			}
			Append(new ReadOnlySpan<byte>(buffer, 0, num));
		}
		ArrayPool<byte>.Shared.Return(buffer);
	}

	public byte[] GetCurrentHash()
	{
		byte[] array = new byte[HashLengthInBytes];
		GetCurrentHashCore(array);
		return array;
	}

	public bool TryGetCurrentHash(Span<byte> destination, out int bytesWritten)
	{
		if (destination.Length < HashLengthInBytes)
		{
			bytesWritten = 0;
			return false;
		}
		GetCurrentHashCore(destination.Slice(0, HashLengthInBytes));
		bytesWritten = HashLengthInBytes;
		return true;
	}

	public int GetCurrentHash(Span<byte> destination)
	{
		if (destination.Length < HashLengthInBytes)
		{
			ThrowDestinationTooShort();
		}
		GetCurrentHashCore(destination.Slice(0, HashLengthInBytes));
		return HashLengthInBytes;
	}

	public byte[] GetHashAndReset()
	{
		byte[] array = new byte[HashLengthInBytes];
		GetHashAndResetCore(array);
		return array;
	}

	public bool TryGetHashAndReset(Span<byte> destination, out int bytesWritten)
	{
		if (destination.Length < HashLengthInBytes)
		{
			bytesWritten = 0;
			return false;
		}
		GetHashAndResetCore(destination.Slice(0, HashLengthInBytes));
		bytesWritten = HashLengthInBytes;
		return true;
	}

	public int GetHashAndReset(Span<byte> destination)
	{
		if (destination.Length < HashLengthInBytes)
		{
			ThrowDestinationTooShort();
		}
		GetHashAndResetCore(destination.Slice(0, HashLengthInBytes));
		return HashLengthInBytes;
	}

	protected virtual void GetHashAndResetCore(Span<byte> destination)
	{
		GetCurrentHashCore(destination);
		Reset();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use GetCurrentHash() to retrieve the computed hash code.", true)]
	public override int GetHashCode()
	{
		throw new NotSupportedException("GetHashCode not supported");
	}

	[DoesNotReturn]
	private protected static void ThrowDestinationTooShort()
	{
		throw new ArgumentException("Destination is too short", "destination");
	}
}
