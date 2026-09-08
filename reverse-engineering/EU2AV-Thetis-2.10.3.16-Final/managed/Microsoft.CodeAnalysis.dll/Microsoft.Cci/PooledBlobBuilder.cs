using System;
using System.Reflection.Metadata;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.Cci;

internal sealed class PooledBlobBuilder : BlobBuilder, IDisposable
{
	private const int PoolSize = 128;

	private const int PoolChunkSize = 1024;

	private static readonly ObjectPool<PooledBlobBuilder> s_chunkPool = new ObjectPool<PooledBlobBuilder>(() => new PooledBlobBuilder(1024), 128);

	private PooledBlobBuilder(int size)
		: base(size)
	{
	}

	public static PooledBlobBuilder GetInstance(bool zero = false)
	{
		PooledBlobBuilder pooledBlobBuilder = s_chunkPool.Allocate();
		if (zero)
		{
			pooledBlobBuilder.WriteBytes(0, pooledBlobBuilder.ChunkCapacity);
			pooledBlobBuilder.Clear();
		}
		return pooledBlobBuilder;
	}

	protected override BlobBuilder AllocateChunk(int minimalSize)
	{
		if (minimalSize <= 1024)
		{
			return s_chunkPool.Allocate();
		}
		return new BlobBuilder(minimalSize);
	}

	protected override void FreeChunk()
	{
		if (base.ChunkCapacity == 1024)
		{
			s_chunkPool.Free(this);
		}
	}

	public new void Free()
	{
		base.Free();
	}

	void IDisposable.Dispose()
	{
		Free();
	}
}
