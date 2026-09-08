using System;
using System.Linq;
using Markdig.Extensions.Alerts;
using Markdig.Extensions.Figures;
using Markdig.Extensions.Tables;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace Markdig.Extensions.Bootstrap;

public class BootstrapExtension : IMarkdownExtension
{
	public void Setup(MarkdownPipelineBuilder pipeline)
	{
		pipeline.DocumentProcessed -= PipelineOnDocumentProcessed;
		pipeline.DocumentProcessed += PipelineOnDocumentProcessed;
	}

	public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
	{
		if (renderer is HtmlRenderer htmlRenderer)
		{
			AlertBlockRenderer alertBlockRenderer = htmlRenderer.ObjectRenderers.OfType<AlertBlockRenderer>().FirstOrDefault();
			if (alertBlockRenderer == null)
			{
				alertBlockRenderer = new AlertBlockRenderer();
				renderer.ObjectRenderers.InsertBefore<QuoteBlockRenderer>(new AlertBlockRenderer());
			}
			alertBlockRenderer.RenderKind = delegate
			{
			};
		}
	}

	private static void PipelineOnDocumentProcessed(MarkdownDocument document)
	{
		Span<char> destination = new char[16];
		foreach (MarkdownObject item in document.Descendants())
		{
			if (item.IsInline)
			{
				if (item.IsContainerInline && item is LinkInline { IsImage: not false } linkInline)
				{
					linkInline.GetAttributes().AddClass("img-fluid");
				}
			}
			else if (item.IsContainerBlock)
			{
				if (item is Table)
				{
					item.GetAttributes().AddClass("table");
				}
				else if (item is AlertBlock alertBlock)
				{
					HtmlAttributes attributes = item.GetAttributes();
					attributes.AddClass("alert");
					attributes.AddProperty("role", "alert");
					if (alertBlock.Kind.Length <= destination.Length)
					{
						alertBlock.Kind.AsSpan().ToUpperInvariant(destination);
						HtmlAttributes htmlAttributes = attributes;
						htmlAttributes.AddClass(destination.Slice(0, alertBlock.Kind.Length) switch
						{
							"NOTE" => "alert-primary", 
							"TIP" => "alert-success", 
							"IMPORTANT" => "alert-info", 
							"WARNING" => "alert-warning", 
							"CAUTION" => "alert-danger", 
							_ => "alert-dark", 
						});
					}
					alertBlock.Descendants().OfType<ParagraphBlock>().LastOrDefault()?.GetAttributes().AddClass("mb-0");
				}
				else if (item is QuoteBlock)
				{
					item.GetAttributes().AddClass("blockquote");
				}
				else if (item is Figure)
				{
					item.GetAttributes().AddClass("figure");
				}
			}
			else if (item is FigureCaption)
			{
				item.GetAttributes().AddClass("figure-caption");
			}
		}
	}
}
