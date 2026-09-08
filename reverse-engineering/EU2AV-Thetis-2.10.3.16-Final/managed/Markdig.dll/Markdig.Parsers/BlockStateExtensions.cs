using System.Runtime.CompilerServices;

namespace Markdig.Parsers;

public static class BlockStateExtensions
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsDiscard(this BlockState blockState)
	{
		if (blockState != BlockState.ContinueDiscard)
		{
			return blockState == BlockState.BreakDiscard;
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsContinue(this BlockState blockState)
	{
		if (blockState != BlockState.Continue)
		{
			return blockState == BlockState.ContinueDiscard;
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsBreak(this BlockState blockState)
	{
		if (blockState != BlockState.Break)
		{
			return blockState == BlockState.BreakDiscard;
		}
		return true;
	}
}
