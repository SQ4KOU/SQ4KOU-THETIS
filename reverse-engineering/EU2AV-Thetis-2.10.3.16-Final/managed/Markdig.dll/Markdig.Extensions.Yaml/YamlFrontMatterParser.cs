using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Syntax;

namespace Markdig.Extensions.Yaml;

public class YamlFrontMatterParser : BlockParser
{
	public bool AllowInMiddleOfDocument { get; set; }

	public YamlFrontMatterParser()
	{
		base.OpeningCharacters = new char[1] { '-' };
	}

	protected virtual YamlFrontMatterBlock CreateFrontMatterBlock(BlockProcessor processor)
	{
		return new YamlFrontMatterBlock(this);
	}

	public override BlockState TryOpen(BlockProcessor processor)
	{
		if (processor.IsCodeIndent)
		{
			return BlockState.None;
		}
		if (!AllowInMiddleOfDocument && processor.Start != 0)
		{
			return BlockState.None;
		}
		int num = 0;
		StringSlice line = processor.Line;
		char c = line.CurrentChar;
		while (c == '-' && num < 4)
		{
			num++;
			c = line.NextChar();
		}
		if (num == 3 && c.IsWhiteSpaceOrZero() && line.TrimEnd())
		{
			bool flag = false;
			StringSlice stringSlice = new StringSlice(line.Text, line.Start, line.Text.Length - 1);
			c = stringSlice.CurrentChar;
			while (c != 0)
			{
				c = stringSlice.NextChar();
				if (c != '\n' && c != '\r')
				{
					continue;
				}
				char c2 = stringSlice.PeekChar();
				if (c == '\r' && c2 == '\n')
				{
					c = stringSlice.NextChar();
				}
				switch (stringSlice.PeekChar())
				{
				case '-':
					if (stringSlice.NextChar() != '-' || stringSlice.NextChar() != '-' || stringSlice.NextChar() != '-' || (stringSlice.NextChar() != 0 && !stringSlice.SkipSpacesToEndOfLineOrEndOfDocument()))
					{
						continue;
					}
					flag = true;
					break;
				case '.':
					if (stringSlice.NextChar() != '.' || stringSlice.NextChar() != '.' || stringSlice.NextChar() != '.' || (stringSlice.NextChar() != 0 && !stringSlice.SkipSpacesToEndOfLineOrEndOfDocument()))
					{
						continue;
					}
					flag = true;
					break;
				default:
					continue;
				}
				break;
			}
			if (flag)
			{
				YamlFrontMatterBlock yamlFrontMatterBlock = CreateFrontMatterBlock(processor);
				yamlFrontMatterBlock.Column = processor.Column;
				yamlFrontMatterBlock.Span.Start = 0;
				yamlFrontMatterBlock.Span.End = line.Start;
				processor.NewBlocks.Push(yamlFrontMatterBlock);
				return BlockState.ContinueDiscard;
			}
		}
		return BlockState.None;
	}

	public override BlockState TryContinue(BlockProcessor processor, Block block)
	{
		StringSlice line = processor.Line;
		char currentChar = line.CurrentChar;
		if (processor.Column == 0 && (currentChar == '-' || currentChar == '.'))
		{
			int num = line.CountAndSkipChar(currentChar);
			currentChar = line.CurrentChar;
			if (num == 3 && !processor.IsCodeIndent && currentChar.IsWhiteSpaceOrZero() && line.TrimEnd())
			{
				block.UpdateSpanEnd(line.Start - 1);
				return BlockState.BreakDiscard;
			}
		}
		processor.GoToColumn(processor.ColumnBeforeIndent);
		return BlockState.Continue;
	}
}
