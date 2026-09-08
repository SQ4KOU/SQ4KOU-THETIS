using Markdig.Helpers;
using Markdig.Syntax;

namespace Markdig.Renderers.Roundtrip;

public class ListRenderer : RoundtripObjectRenderer<ListBlock>
{
	protected override void Write(RoundtripRenderer renderer, ListBlock listBlock)
	{
		renderer.RenderLinesBefore(listBlock);
		if (listBlock.IsOrdered)
		{
			for (int i = 0; i < listBlock.Count; i++)
			{
				ListItemBlock listItemBlock = (ListItemBlock)listBlock[i];
				renderer.RenderLinesBefore(listItemBlock);
				string arg = listItemBlock.TriviaBefore.ToString();
				string arg2 = listItemBlock.SourceBullet.ToString();
				char orderedDelimiter = listBlock.OrderedDelimiter;
				renderer.PushIndent(new string[1] { $"{arg}{arg2}{orderedDelimiter}" });
				if (listItemBlock.Count == 0)
				{
					renderer.Write("");
				}
				else
				{
					renderer.WriteChildren(listItemBlock);
				}
				renderer.PopIndent();
				renderer.RenderLinesAfter(listItemBlock);
			}
		}
		else
		{
			for (int j = 0; j < listBlock.Count; j++)
			{
				ListItemBlock listItemBlock2 = (ListItemBlock)listBlock[j];
				renderer.RenderLinesBefore(listItemBlock2);
				StringSlice triviaBefore = listItemBlock2.TriviaBefore;
				char bulletType = listBlock.BulletType;
				StringSlice triviaAfter = listItemBlock2.TriviaAfter;
				renderer.PushIndent(new string[1] { $"{triviaBefore}{bulletType}{triviaAfter}" });
				if (listItemBlock2.Count == 0)
				{
					renderer.Write("");
				}
				else
				{
					renderer.WriteChildren(listItemBlock2);
				}
				renderer.PopIndent();
				renderer.RenderLinesAfter(listItemBlock2);
			}
		}
		renderer.RenderLinesAfter(listBlock);
	}
}
