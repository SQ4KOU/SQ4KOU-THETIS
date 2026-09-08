using Markdig.Helpers;
using Markdig.Syntax;

namespace Markdig.Renderers.Roundtrip;

public class CodeBlockRenderer : RoundtripObjectRenderer<CodeBlock>
{
	protected override void Write(RoundtripRenderer renderer, CodeBlock obj)
	{
		renderer.RenderLinesBefore(obj);
		if (obj is FencedCodeBlock fencedCodeBlock)
		{
			renderer.Write(obj.TriviaBefore);
			renderer.Write(fencedCodeBlock.FencedChar, fencedCodeBlock.OpeningFencedCharCount);
			if (!fencedCodeBlock.TriviaAfterFencedChar.IsEmpty)
			{
				renderer.Write(fencedCodeBlock.TriviaAfterFencedChar);
			}
			if (fencedCodeBlock.Info != null)
			{
				renderer.Write(fencedCodeBlock.UnescapedInfo);
			}
			if (!fencedCodeBlock.TriviaAfterInfo.IsEmpty)
			{
				renderer.Write(fencedCodeBlock.TriviaAfterInfo);
			}
			if (!string.IsNullOrEmpty(fencedCodeBlock.Arguments))
			{
				renderer.Write(fencedCodeBlock.UnescapedArguments);
			}
			if (!fencedCodeBlock.TriviaAfterArguments.IsEmpty)
			{
				renderer.Write(fencedCodeBlock.TriviaAfterArguments);
			}
			renderer.WriteLine(fencedCodeBlock.InfoNewLine);
			renderer.WriteLeafRawLines(obj);
			renderer.Write(fencedCodeBlock.TriviaBeforeClosingFence);
			renderer.Write(fencedCodeBlock.FencedChar, fencedCodeBlock.ClosingFencedCharCount);
			if (fencedCodeBlock.ClosingFencedCharCount > 0)
			{
				renderer.WriteLine(obj.NewLine);
			}
			renderer.Write(obj.TriviaAfter);
		}
		else
		{
			string[] array = new string[obj.CodeBlockLines.Count];
			for (int i = 0; i < obj.CodeBlockLines.Count; i++)
			{
				array[i] = obj.CodeBlockLines[i].TriviaBefore.ToString();
			}
			renderer.PushIndent(array);
			WriteLeafRawLines(renderer, obj);
			renderer.PopIndent();
		}
		renderer.RenderLinesAfter(obj);
	}

	public void WriteLeafRawLines(RoundtripRenderer renderer, LeafBlock leafBlock)
	{
		if (leafBlock.Lines.Lines != null)
		{
			StringLineGroup lines = leafBlock.Lines;
			StringLine[] lines2 = lines.Lines;
			for (int i = 0; i < lines.Count; i++)
			{
				ref StringSlice slice = ref lines2[i].Slice;
				renderer.Write(ref slice);
				renderer.WriteLine(slice.NewLine);
			}
		}
	}
}
