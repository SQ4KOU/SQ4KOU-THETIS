using Markdig.Parsers.Inlines;
using Markdig.Renderers;
using Markdig.Renderers.Normalize;

namespace Markdig.Extensions.TaskLists;

public class TaskListExtension : IMarkdownExtension
{
	public void Setup(MarkdownPipelineBuilder pipeline)
	{
		if (!pipeline.InlineParsers.Contains<TaskListInlineParser>())
		{
			pipeline.InlineParsers.InsertBefore<LinkInlineParser>(new TaskListInlineParser());
		}
	}

	public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
	{
		if (renderer is HtmlRenderer htmlRenderer)
		{
			htmlRenderer.ObjectRenderers.AddIfNotAlready<HtmlTaskListRenderer>();
		}
		if (renderer is NormalizeRenderer normalizeRenderer)
		{
			normalizeRenderer.ObjectRenderers.AddIfNotAlready<NormalizeTaskListRenderer>();
		}
	}
}
