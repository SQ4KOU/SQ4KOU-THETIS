using System.Collections.Generic;
using Markdig.Helpers;
using Markdig.Syntax;

namespace Markdig.Parsers;

public class IndentedCodeBlockParser : BlockParser
{
	public override bool CanInterrupt(BlockProcessor processor, Block block)
	{
		return !block.IsParagraphBlock;
	}

	public override BlockState TryOpen(BlockProcessor processor)
	{
		BlockState num = TryContinue(processor, null);
		if (num == BlockState.Continue)
		{
			int column = processor.Column;
			int start = processor.Start;
			processor.UnwindAllIndents();
			CodeBlock codeBlock = new CodeBlock(this)
			{
				Column = processor.Column,
				Span = new SourceSpan(processor.Start, processor.Line.End)
			};
			if (processor.TrackTrivia)
			{
				codeBlock.LinesBefore = processor.TakeLinesBefore();
				codeBlock.NewLine = processor.Line.NewLine;
			}
			CodeBlock.CodeBlockLine item = new CodeBlock.CodeBlockLine
			{
				TriviaBefore = processor.UseTrivia(start - 1)
			};
			codeBlock.CodeBlockLines.Add(item);
			processor.NewBlocks.Push(codeBlock);
			processor.GoToColumn(column);
		}
		return num;
	}

	public override BlockState TryContinue(BlockProcessor processor, Block? block)
	{
		if ((!processor.IsCodeIndent || processor.IsBlankLine) && (block == null || !processor.IsBlankLine))
		{
			if (block != null)
			{
				CodeBlock codeBlock = (CodeBlock)block;
				for (int num = codeBlock.Lines.Count - 1; num >= 0; num--)
				{
					StringLine stringLine = codeBlock.Lines.Lines[num];
					if (!stringLine.Slice.IsEmpty)
					{
						break;
					}
					codeBlock.Lines.RemoveAt(num);
					if (processor.TrackTrivia)
					{
						if (processor.LinesBefore == null)
						{
							List<StringSlice> list = (processor.LinesBefore = new List<StringSlice>());
						}
						processor.LinesBefore.Add(stringLine.Slice);
					}
				}
			}
			return BlockState.None;
		}
		if (processor.Indent > 4)
		{
			processor.GoToCodeIndent();
		}
		if (block != null)
		{
			block.UpdateSpanEnd(processor.Line.End);
			CodeBlock codeBlock2 = (CodeBlock)block;
			CodeBlock.CodeBlockLine codeBlockLine = new CodeBlock.CodeBlockLine();
			codeBlock2.CodeBlockLines.Add(codeBlockLine);
			if (processor.TrackTrivia)
			{
				codeBlockLine.TriviaBefore = processor.UseTrivia(processor.Start - 1);
				codeBlock2.NewLine = processor.Line.NewLine;
			}
		}
		return BlockState.Continue;
	}

	public override bool Close(BlockProcessor processor, Block block)
	{
		CodeBlock codeBlock = (CodeBlock)block;
		if (codeBlock == null)
		{
			return true;
		}
		for (int num = codeBlock.Lines.Count - 1; num >= 0; num--)
		{
			StringLine stringLine = codeBlock.Lines.Lines[num];
			if (!stringLine.Slice.IsEmpty)
			{
				break;
			}
			codeBlock.Lines.RemoveAt(num);
			if (processor.TrackTrivia)
			{
				CodeBlock.CodeBlockLine codeBlockLine = codeBlock.CodeBlockLines[num];
				StringSlice item = new StringSlice(stringLine.Slice.Text, codeBlockLine.TriviaBefore.Start, stringLine.Slice.End, stringLine.NewLine);
				if (block.LinesAfter == null)
				{
					List<StringSlice> list = (block.LinesAfter = new List<StringSlice>());
				}
				block.LinesAfter.Add(item);
			}
		}
		return true;
	}
}
