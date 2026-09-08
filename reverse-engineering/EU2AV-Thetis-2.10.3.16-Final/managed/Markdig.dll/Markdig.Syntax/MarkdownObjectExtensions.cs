using System;
using System.Collections.Generic;
using Markdig.Syntax.Inlines;

namespace Markdig.Syntax;

public static class MarkdownObjectExtensions
{
	public static IEnumerable<MarkdownObject> Descendants(this MarkdownObject markdownObject)
	{
		Stack<MarkdownObject> stack = new Stack<MarkdownObject>();
		Stack<bool> pushStack = new Stack<bool>();
		stack.Push(markdownObject);
		pushStack.Push(item: false);
		while (stack.Count > 0)
		{
			MarkdownObject block = stack.Pop();
			if (pushStack.Pop())
			{
				yield return block;
			}
			if (block is ContainerBlock { Count: var num } containerBlock)
			{
				while (true)
				{
					int num2 = num;
					num = num2 - 1;
					if (num2 > 0)
					{
						Block block2 = containerBlock[num];
						if (block2 is LeafBlock { Inline: not null } leafBlock)
						{
							stack.Push(leafBlock.Inline);
							pushStack.Push(item: false);
						}
						stack.Push(block2);
						pushStack.Push(item: true);
						continue;
					}
					break;
				}
			}
			else if (block is ContainerInline containerInline)
			{
				for (Inline inline = containerInline.LastChild; inline != null; inline = inline.PreviousSibling)
				{
					stack.Push(inline);
					pushStack.Push(item: true);
				}
			}
		}
	}

	public static IEnumerable<T> Descendants<T>(this MarkdownObject markdownObject) where T : MarkdownObject
	{
		if (typeof(T).IsSubclassOf(typeof(Block)))
		{
			if (markdownObject is ContainerBlock { Count: >0 } containerBlock)
			{
				return BlockDescendantsInternal<T>(containerBlock);
			}
		}
		else if (markdownObject is ContainerBlock containerBlock2)
		{
			if (containerBlock2.Count > 0)
			{
				return InlineDescendantsInternal<T>(containerBlock2);
			}
		}
		else if (markdownObject is ContainerInline { FirstChild: not null } containerInline)
		{
			return containerInline.FindDescendantsInternal<T>();
		}
		return Array.Empty<T>();
	}

	public static IEnumerable<T> Descendants<T>(this ContainerInline inline) where T : Inline
	{
		return inline.FindDescendants<T>();
	}

	public static IEnumerable<T> Descendants<T>(this ContainerBlock block) where T : Block
	{
		if (block != null && block.Count > 0)
		{
			return BlockDescendantsInternal<T>(block);
		}
		return Array.Empty<T>();
	}

	private static IEnumerable<T> BlockDescendantsInternal<T>(ContainerBlock block) where T : MarkdownObject
	{
		Stack<Block> stack = new Stack<Block>();
		int count = block.Count;
		while (count-- > 0)
		{
			stack.Push(block[count]);
		}
		while (stack.Count > 0)
		{
			Block subBlock = stack.Pop();
			if (subBlock is T val)
			{
				yield return val;
			}
			if (!(subBlock is ContainerBlock { Count: var num } containerBlock))
			{
				continue;
			}
			while (true)
			{
				int num2 = num;
				num = num2 - 1;
				if (num2 <= 0)
				{
					break;
				}
				stack.Push(containerBlock[num]);
			}
		}
	}

	private static IEnumerable<T> InlineDescendantsInternal<T>(ContainerBlock block) where T : MarkdownObject
	{
		foreach (MarkdownObject item in block.Descendants())
		{
			if (item is T val)
			{
				yield return val;
			}
		}
	}
}
