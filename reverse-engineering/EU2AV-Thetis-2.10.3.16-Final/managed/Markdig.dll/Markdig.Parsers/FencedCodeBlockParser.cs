using Markdig.Helpers;
using Markdig.Syntax;

namespace Markdig.Parsers;

public class FencedCodeBlockParser : FencedBlockParserBase<FencedCodeBlock>
{
	public const string DefaultInfoPrefix = "language-";

	public FencedCodeBlockParser()
	{
		base.OpeningCharacters = new char[2] { '`', '~' };
		base.InfoPrefix = "language-";
	}

	protected override FencedCodeBlock CreateFencedBlock(BlockProcessor processor)
	{
		FencedCodeBlock fencedCodeBlock = new FencedCodeBlock(this)
		{
			IndentCount = processor.Indent
		};
		if (processor.TrackTrivia)
		{
			fencedCodeBlock.LinesBefore = processor.TakeLinesBefore();
			fencedCodeBlock.TriviaBefore = processor.UseTrivia(processor.Start - 1);
			fencedCodeBlock.NewLine = processor.Line.NewLine;
		}
		return fencedCodeBlock;
	}

	public override BlockState TryContinue(BlockProcessor processor, Block block)
	{
		BlockState blockState = base.TryContinue(processor, block);
		if (blockState == BlockState.Continue && !processor.TrackTrivia)
		{
			FencedCodeBlock obj = (FencedCodeBlock)block;
			char c = processor.CurrentChar;
			int num = obj.IndentCount;
			while (num > 0 && c.IsSpace())
			{
				num--;
				c = processor.NextChar();
			}
		}
		return blockState;
	}
}
