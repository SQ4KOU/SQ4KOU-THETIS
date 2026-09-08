using Markdig.Parsers.Inlines;
using Markdig.Renderers;
using Markdig.Renderers.Html.Inlines;
using Markdig.Syntax.Inlines;

namespace Markdig.Extensions.EmphasisExtras;

public class EmphasisExtraExtension : IMarkdownExtension
{
	public EmphasisExtraOptions Options { get; }

	public EmphasisExtraExtension(EmphasisExtraOptions options = EmphasisExtraOptions.Default)
	{
		Options = options;
	}

	public void Setup(MarkdownPipelineBuilder pipeline)
	{
		EmphasisInlineParser emphasisInlineParser = pipeline.InlineParsers.FindExact<EmphasisInlineParser>();
		if (emphasisInlineParser == null)
		{
			return;
		}
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		bool flag5 = (Options & EmphasisExtraOptions.Strikethrough) != 0 || (Options & EmphasisExtraOptions.Subscript) != 0;
		bool flag6 = (Options & EmphasisExtraOptions.Superscript) != 0;
		bool flag7 = (Options & EmphasisExtraOptions.Inserted) != 0;
		bool flag8 = (Options & EmphasisExtraOptions.Marked) != 0;
		foreach (EmphasisDescriptor emphasisDescriptor in emphasisInlineParser.EmphasisDescriptors)
		{
			if (flag5 && emphasisDescriptor.Character == '~')
			{
				flag = true;
			}
			if (flag6 && emphasisDescriptor.Character == '^')
			{
				flag2 = true;
			}
			if (flag7 && emphasisDescriptor.Character == '+')
			{
				flag3 = true;
			}
			if (flag8 && emphasisDescriptor.Character == '=')
			{
				flag4 = true;
			}
		}
		if (flag5 && !flag)
		{
			int minimumCount = (((Options & EmphasisExtraOptions.Subscript) != 0) ? 1 : 2);
			int maximumCount = (((Options & EmphasisExtraOptions.Strikethrough) == 0) ? 1 : 2);
			emphasisInlineParser.EmphasisDescriptors.Add(new EmphasisDescriptor('~', minimumCount, maximumCount, enableWithinWord: true));
		}
		if (flag6 && !flag2)
		{
			emphasisInlineParser.EmphasisDescriptors.Add(new EmphasisDescriptor('^', 1, 1, enableWithinWord: true));
		}
		if (flag7 && !flag3)
		{
			emphasisInlineParser.EmphasisDescriptors.Add(new EmphasisDescriptor('+', 2, 2, enableWithinWord: true));
		}
		if (flag8 && !flag4)
		{
			emphasisInlineParser.EmphasisDescriptors.Add(new EmphasisDescriptor('=', 2, 2, enableWithinWord: true));
		}
	}

	public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
	{
		if (!(renderer is HtmlRenderer htmlRenderer))
		{
			return;
		}
		EmphasisInlineRenderer emphasisInlineRenderer = htmlRenderer.ObjectRenderers.FindExact<EmphasisInlineRenderer>();
		if (emphasisInlineRenderer != null)
		{
			EmphasisInlineRenderer.GetTagDelegate previousTag = emphasisInlineRenderer.GetTag;
			emphasisInlineRenderer.GetTag = (EmphasisInline inline) => GetTag(inline) ?? previousTag(inline);
		}
	}

	private string? GetTag(EmphasisInline emphasisInline)
	{
		switch (emphasisInline.DelimiterChar)
		{
		case '~':
			if (emphasisInline.DelimiterCount != 2)
			{
				return "sub";
			}
			return "del";
		case '^':
			return "sup";
		case '+':
			return "ins";
		case '=':
			return "mark";
		default:
			return null;
		}
	}
}
