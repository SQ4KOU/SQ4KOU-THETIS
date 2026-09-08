using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Markdig.Helpers;
using Markdig.Syntax;

namespace Markdig.Parsers;

public static class MarkdownParser
{
	private struct ContainerItem(ContainerBlock container)
	{
		public readonly ContainerBlock Container = container;

		public int Index = 0;
	}

	public static MarkdownDocument Parse([StringSyntax("Markdown")] string text, MarkdownPipeline? pipeline = null, MarkdownParserContext? context = null)
	{
		if (text == null)
		{
			ThrowHelper.ArgumentNullException_text();
		}
		if (pipeline == null)
		{
			pipeline = Markdown.DefaultPipeline;
		}
		text = FixupZero(text);
		MarkdownDocument markdownDocument = new MarkdownDocument
		{
			IsOpen = true
		};
		if (pipeline.PreciseSourceLocation)
		{
			int val = text.Length / 32;
			val = Math.Max(4, Math.Min(512, val));
			markdownDocument.LineStartIndexes = new List<int>(val);
		}
		BlockProcessor blockProcessor = BlockProcessor.Rent(markdownDocument, pipeline.BlockParsers, context, pipeline.TrackTrivia);
		try
		{
			blockProcessor.Open(markdownDocument);
			ProcessBlocks(blockProcessor, text);
			if (pipeline.TrackTrivia)
			{
				ProcessBlocksTrivia(blockProcessor, markdownDocument);
			}
			markdownDocument.LineCount = blockProcessor.LineIndex;
		}
		finally
		{
			BlockProcessor.Release(blockProcessor);
		}
		InlineProcessor inlineProcessor = InlineProcessor.Rent(markdownDocument, pipeline.InlineParsers, pipeline.PreciseSourceLocation, context, pipeline.TrackTrivia);
		inlineProcessor.DebugLog = pipeline.DebugLog;
		try
		{
			ProcessInlines(inlineProcessor, markdownDocument);
		}
		finally
		{
			InlineProcessor.Release(inlineProcessor);
		}
		pipeline.DocumentProcessed?.Invoke(markdownDocument);
		return markdownDocument;
	}

	private static string FixupZero(string text)
	{
		return text.Replace('\0', '\ufffd');
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void ProcessBlocks(BlockProcessor blockProcessor, string text)
	{
		LineReader lineReader = new LineReader(text);
		while (true)
		{
			StringSlice newLine = lineReader.ReadLine();
			if (newLine.Text == null)
			{
				break;
			}
			blockProcessor.ProcessLine(newLine);
		}
		blockProcessor.CloseAll(force: true);
	}

	private static void ProcessBlocksTrivia(BlockProcessor blockProcessor, MarkdownDocument document)
	{
		Block lastBlock = blockProcessor.LastBlock;
		if (lastBlock == null && document.Count == 0)
		{
			EmptyBlock emptyBlock = new EmptyBlock(null);
			List<StringSlice> list = blockProcessor.TakeLinesBefore();
			emptyBlock.LinesAfter = new List<StringSlice>();
			if (list != null)
			{
				emptyBlock.LinesAfter.AddRange(list);
			}
			document.Add(emptyBlock);
		}
		else if (lastBlock != null && blockProcessor.LinesBefore != null)
		{
			Block block = Block.FindRootMostContainerParent(lastBlock);
			Block block2 = block;
			if (block2.LinesAfter == null)
			{
				List<StringSlice> list2 = (block2.LinesAfter = new List<StringSlice>());
			}
			List<StringSlice> list4 = blockProcessor.TakeLinesBefore();
			if (list4 != null)
			{
				block.LinesAfter.AddRange(list4);
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void ProcessInlines(InlineProcessor inlineProcessor, MarkdownDocument document)
	{
		int num = 1;
		ContainerItem[] array = new ContainerItem[4]
		{
			new ContainerItem(document),
			default(ContainerItem),
			default(ContainerItem),
			default(ContainerItem)
		};
		document.OnProcessInlinesBegin(inlineProcessor);
		while (num != 0)
		{
			ContainerBlock container;
			while (true)
			{
				ref ContainerItem reference = ref array[num - 1];
				Block block;
				for (container = reference.Container; reference.Index < container.Count; reference.Index++)
				{
					block = container[reference.Index];
					if (block.IsLeafBlock)
					{
						LeafBlock leafBlock = Unsafe.As<LeafBlock>(block);
						leafBlock.OnProcessInlinesBegin(inlineProcessor);
						if (leafBlock.ProcessInlines)
						{
							inlineProcessor.ProcessInlineLeaf(leafBlock);
							if (inlineProcessor.PreviousContainerToReplace != null)
							{
								if (container == inlineProcessor.PreviousContainerToReplace)
								{
									reference = new ContainerItem(inlineProcessor.NewContainerToReplace)
									{
										Index = reference.Index
									};
									container = reference.Container;
								}
								else
								{
									bool flag = false;
									for (int num2 = num - 2; num2 >= 0; num2--)
									{
										ref ContainerItem reference2 = ref array[num2];
										if (reference2.Container == inlineProcessor.PreviousContainerToReplace)
										{
											reference2 = new ContainerItem(inlineProcessor.NewContainerToReplace)
											{
												Index = reference2.Index
											};
											flag = true;
											break;
										}
									}
									if (!flag)
									{
										throw new InvalidOperationException("Cannot find the parent block to replace");
									}
								}
								inlineProcessor.PreviousContainerToReplace = null;
								inlineProcessor.NewContainerToReplace = null;
							}
							if (leafBlock.RemoveAfterProcessInlines)
							{
								container.RemoveAt(reference.Index);
								reference.Index--;
							}
							else if (inlineProcessor.BlockNew != null)
							{
								container[reference.Index] = inlineProcessor.BlockNew;
							}
						}
						leafBlock.OnProcessInlinesEnd(inlineProcessor);
						continue;
					}
					if (!block.IsContainerBlock)
					{
						continue;
					}
					goto IL_0179;
				}
				break;
				IL_0179:
				if (block.RemoveAfterProcessInlines)
				{
					container.RemoveAt(reference.Index);
				}
				else
				{
					reference.Index++;
				}
				if (num == array.Length)
				{
					Array.Resize(ref array, num * 2);
					ThrowHelper.CheckDepthLimit(array.Length);
				}
				array[num++] = new ContainerItem(Unsafe.As<ContainerBlock>(block));
				block.OnProcessInlinesBegin(inlineProcessor);
			}
			container.OnProcessInlinesEnd(inlineProcessor);
			array[--num] = default(ContainerItem);
		}
	}
}
