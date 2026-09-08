using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Renderers.Html;
using Markdig.Syntax;

namespace Markdig.Extensions.Mathematics;

public class MathBlockParser : FencedBlockParserBase<MathBlock>
{
	public string DefaultClass { get; set; }

	public MathBlockParser()
	{
		base.OpeningCharacters = new char[1] { '$' };
		base.MinimumMatchCount = 2;
		base.MaximumMatchCount = 2;
		base.InfoParser = NoInfoParser;
		DefaultClass = "math";
		base.InfoPrefix = null;
	}

	protected override MathBlock CreateFencedBlock(BlockProcessor processor)
	{
		MathBlock mathBlock = new MathBlock(this);
		if (DefaultClass != null)
		{
			mathBlock.GetAttributes().AddClass(DefaultClass);
		}
		return mathBlock;
	}

	private static bool NoInfoParser(BlockProcessor state, ref StringSlice line, IFencedBlock fenced, char openingCharacter)
	{
		for (int i = line.Start; i <= line.End; i++)
		{
			if (!line.Text[i].IsSpaceOrTab())
			{
				return false;
			}
		}
		return true;
	}
}
