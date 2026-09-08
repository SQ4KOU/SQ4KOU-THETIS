namespace Markdig.Syntax;

public static class BlockExtensions
{
	public static Block? FindBlockAtPosition(this Block rootBlock, int position)
	{
		bool flag = rootBlock.CompareToPosition(position) == 0;
		if (!(rootBlock is ContainerBlock { Count: not 0 } containerBlock) || !flag)
		{
			if (!flag)
			{
				return null;
			}
			return rootBlock;
		}
		int num = 0;
		int num2 = containerBlock.Count - 1;
		Block block = null;
		while (num <= num2)
		{
			int num3 = (num2 - num) / 2 + num;
			block = containerBlock[num3];
			int num4 = block.CompareToPosition(position);
			if (num4 == 0)
			{
				break;
			}
			block = null;
			if (num4 < 0)
			{
				num = num3 + 1;
			}
			else
			{
				num2 = num3 - 1;
			}
		}
		if (block == null)
		{
			return rootBlock;
		}
		return block.FindBlockAtPosition(position);
	}

	public static int FindClosestLine(this MarkdownDocument root, int line)
	{
		return root.FindClosestBlock(line)?.Line ?? 0;
	}

	public static Block? FindClosestBlock(this Block rootBlock, int line)
	{
		if (!(rootBlock is ContainerBlock { Count: not 0 } containerBlock))
		{
			if (rootBlock.Line != line)
			{
				return null;
			}
			return rootBlock;
		}
		int num = 0;
		int num2 = containerBlock.Count - 1;
		while (num <= num2)
		{
			int num3 = (num2 - num) / 2 + num;
			Block block = containerBlock[num3];
			int num4 = block.Line.CompareTo(line);
			if (num4 == 0)
			{
				return block;
			}
			if (num4 < 0)
			{
				num = num3 + 1;
			}
			else
			{
				num2 = num3 - 1;
			}
		}
		if (num > 0 && num < containerBlock.Count)
		{
			Block block2 = containerBlock[num - 1].FindClosestBlock(line) ?? containerBlock[num - 1];
			Block block3 = containerBlock[num].FindClosestBlock(line) ?? containerBlock[num];
			if (block2.Line == line)
			{
				return block2;
			}
			if (block3.Line == line)
			{
				return block3;
			}
			int line2 = block2.Line;
			int line3 = block3.Line;
			if (!((double)(line - line2) * 1.0 / (double)(line3 - line2) < 0.5))
			{
				return block3;
			}
			return block2;
		}
		if (num == 0)
		{
			return containerBlock[num].FindClosestBlock(line) ?? containerBlock[num];
		}
		if (num == containerBlock.Count)
		{
			return containerBlock[num - 1].FindClosestBlock(line) ?? containerBlock[num - 1];
		}
		return null;
	}

	public static bool ContainsPosition(this Block block, int position)
	{
		return block.CompareToPosition(position) == 0;
	}

	public static int CompareToPosition(this Block block, int position)
	{
		if (position >= block.Span.Start)
		{
			if (position <= block.Span.End + 1)
			{
				return 0;
			}
			return -1;
		}
		return 1;
	}
}
