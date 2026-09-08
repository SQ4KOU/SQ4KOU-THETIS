using System;
using Markdig.Syntax;

namespace Markdig.Renderers.Normalize;

public class CodeBlockRenderer : NormalizeObjectRenderer<CodeBlock>
{
	public bool OutputAttributesOnPre { get; set; }

	protected override void Write(NormalizeRenderer renderer, CodeBlock obj)
	{
		if (obj is FencedCodeBlock fencedCodeBlock)
		{
			int count = Math.Min(fencedCodeBlock.OpeningFencedCharCount, fencedCodeBlock.ClosingFencedCharCount);
			renderer.Write(fencedCodeBlock.FencedChar, count);
			if (fencedCodeBlock.Info != null)
			{
				renderer.Write(fencedCodeBlock.Info);
			}
			if (!string.IsNullOrEmpty(fencedCodeBlock.Arguments))
			{
				renderer.Write(' ').Write(fencedCodeBlock.Arguments);
			}
			renderer.WriteLine();
			renderer.WriteLeafRawLines(obj, writeEndOfLines: true);
			renderer.Write(fencedCodeBlock.FencedChar, count);
		}
		else
		{
			renderer.WriteLeafRawLines(obj, writeEndOfLines: false, indent: true);
		}
		renderer.FinishBlock(renderer.Options.EmptyLineAfterCodeBlock);
	}
}
