using System;
using Markdig.Syntax;

namespace Markdig.Helpers;

internal readonly struct BlockWrapper(Block block) : IEquatable<BlockWrapper>
{
	public readonly Block Block = block;

	public static implicit operator Block(BlockWrapper wrapper)
	{
		return wrapper.Block;
	}

	public static implicit operator BlockWrapper(Block block)
	{
		return new BlockWrapper(block);
	}

	public bool Equals(BlockWrapper other)
	{
		return Block == other.Block;
	}

	public override bool Equals(object? obj)
	{
		return Block.Equals(obj);
	}

	public override int GetHashCode()
	{
		return Block.GetHashCode();
	}
}
