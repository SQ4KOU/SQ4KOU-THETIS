using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Renderers;
using Markdig.Renderers.Normalize;
using Markdig.Syntax;

namespace Markdig;

public static class Markdown
{
	[CompilerGenerated]
	[MaybeNull]
	private static string _003CVersion_003Ek__BackingField;

	internal static readonly MarkdownPipeline DefaultPipeline = new MarkdownPipelineBuilder().Build();

	[CompilerGenerated]
	[MaybeNull]
	private static MarkdownPipeline _003CDefaultTrackTriviaPipeline_003Ek__BackingField;

	public static string Version => _003CVersion_003Ek__BackingField ?? (_003CVersion_003Ek__BackingField = typeof(Markdown).Assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version ?? "Unknown");

	private static MarkdownPipeline DefaultTrackTriviaPipeline => _003CDefaultTrackTriviaPipeline_003Ek__BackingField ?? (_003CDefaultTrackTriviaPipeline_003Ek__BackingField = new MarkdownPipelineBuilder().EnableTrackTrivia().Build());

	private static MarkdownPipeline GetPipeline(MarkdownPipeline? pipeline, string markdown)
	{
		if (pipeline == null)
		{
			return DefaultPipeline;
		}
		if (pipeline.SelfPipeline != null)
		{
			return pipeline.SelfPipeline.CreatePipelineFromInput(markdown);
		}
		return pipeline;
	}

	public static string Normalize([StringSyntax("Markdown")] string markdown, NormalizeOptions? options = null, MarkdownPipeline? pipeline = null, MarkdownParserContext? context = null)
	{
		StringWriter stringWriter = new StringWriter();
		Normalize(markdown, stringWriter, options, pipeline, context);
		return stringWriter.ToString();
	}

	public static MarkdownDocument Normalize([StringSyntax("Markdown")] string markdown, TextWriter writer, NormalizeOptions? options = null, MarkdownPipeline? pipeline = null, MarkdownParserContext? context = null)
	{
		if (markdown == null)
		{
			ThrowHelper.ArgumentNullException_markdown();
		}
		pipeline = GetPipeline(pipeline, markdown);
		MarkdownDocument markdownDocument = MarkdownParser.Parse(markdown, pipeline, context);
		NormalizeRenderer normalizeRenderer = new NormalizeRenderer(writer, options);
		pipeline.Setup(normalizeRenderer);
		normalizeRenderer.Render(markdownDocument);
		writer.Flush();
		return markdownDocument;
	}

	public static string ToHtml([StringSyntax("Markdown")] string markdown, MarkdownPipeline? pipeline = null, MarkdownParserContext? context = null)
	{
		if (markdown == null)
		{
			ThrowHelper.ArgumentNullException_markdown();
		}
		pipeline = GetPipeline(pipeline, markdown);
		return MarkdownParser.Parse(markdown, pipeline, context).ToHtml(pipeline);
	}

	public static string ToHtml(this MarkdownDocument document, MarkdownPipeline? pipeline = null)
	{
		if (document == null)
		{
			ThrowHelper.ArgumentNullException("document");
		}
		if (pipeline == null)
		{
			pipeline = DefaultPipeline;
		}
		MarkdownPipeline.RentedHtmlRenderer rentedHtmlRenderer = pipeline.RentHtmlRenderer();
		try
		{
			HtmlRenderer instance = rentedHtmlRenderer.Instance;
			instance.Render(document);
			instance.Writer.Flush();
			return instance.Writer.ToString() ?? string.Empty;
		}
		finally
		{
			rentedHtmlRenderer.Dispose();
		}
	}

	public static void ToHtml(this MarkdownDocument document, TextWriter writer, MarkdownPipeline? pipeline = null)
	{
		if (document == null)
		{
			ThrowHelper.ArgumentNullException("document");
		}
		if (writer == null)
		{
			ThrowHelper.ArgumentNullException_writer();
		}
		if (pipeline == null)
		{
			pipeline = DefaultPipeline;
		}
		MarkdownPipeline.RentedHtmlRenderer rentedHtmlRenderer = pipeline.RentHtmlRenderer(writer);
		try
		{
			HtmlRenderer instance = rentedHtmlRenderer.Instance;
			instance.Render(document);
			instance.Writer.Flush();
		}
		finally
		{
			rentedHtmlRenderer.Dispose();
		}
	}

	public static MarkdownDocument ToHtml([StringSyntax("Markdown")] string markdown, TextWriter writer, MarkdownPipeline? pipeline = null, MarkdownParserContext? context = null)
	{
		if (markdown == null)
		{
			ThrowHelper.ArgumentNullException_markdown();
		}
		if (writer == null)
		{
			ThrowHelper.ArgumentNullException_writer();
		}
		pipeline = GetPipeline(pipeline, markdown);
		MarkdownDocument markdownDocument = MarkdownParser.Parse(markdown, pipeline, context);
		markdownDocument.ToHtml(writer, pipeline);
		return markdownDocument;
	}

	public static object Convert([StringSyntax("Markdown")] string markdown, IMarkdownRenderer renderer, MarkdownPipeline? pipeline = null, MarkdownParserContext? context = null)
	{
		if (markdown == null)
		{
			ThrowHelper.ArgumentNullException_markdown();
		}
		if (renderer == null)
		{
			ThrowHelper.ArgumentNullException("renderer");
		}
		pipeline = GetPipeline(pipeline, markdown);
		MarkdownDocument markdownObject = MarkdownParser.Parse(markdown, pipeline, context);
		pipeline.Setup(renderer);
		return renderer.Render(markdownObject);
	}

	public static MarkdownDocument Parse([StringSyntax("Markdown")] string markdown, bool trackTrivia = false)
	{
		if (markdown == null)
		{
			ThrowHelper.ArgumentNullException_markdown();
		}
		MarkdownPipeline pipeline = (trackTrivia ? DefaultTrackTriviaPipeline : null);
		return Parse(markdown, pipeline);
	}

	public static MarkdownDocument Parse([StringSyntax("Markdown")] string markdown, MarkdownPipeline? pipeline, MarkdownParserContext? context = null)
	{
		if (markdown == null)
		{
			ThrowHelper.ArgumentNullException_markdown();
		}
		pipeline = GetPipeline(pipeline, markdown);
		return MarkdownParser.Parse(markdown, pipeline, context);
	}

	public static MarkdownDocument ToPlainText([StringSyntax("Markdown")] string markdown, TextWriter writer, MarkdownPipeline? pipeline = null, MarkdownParserContext? context = null)
	{
		if (markdown == null)
		{
			ThrowHelper.ArgumentNullException_markdown();
		}
		if (writer == null)
		{
			ThrowHelper.ArgumentNullException_writer();
		}
		pipeline = GetPipeline(pipeline, markdown);
		MarkdownDocument markdownDocument = MarkdownParser.Parse(markdown, pipeline, context);
		HtmlRenderer htmlRenderer = new HtmlRenderer(writer)
		{
			EnableHtmlForBlock = false,
			EnableHtmlForInline = false,
			EnableHtmlEscape = false
		};
		pipeline.Setup(htmlRenderer);
		htmlRenderer.Render(markdownDocument);
		writer.Flush();
		return markdownDocument;
	}

	public static string ToPlainText([StringSyntax("Markdown")] string markdown, MarkdownPipeline? pipeline = null, MarkdownParserContext? context = null)
	{
		if (markdown == null)
		{
			ThrowHelper.ArgumentNullException_markdown();
		}
		StringWriter stringWriter = new StringWriter();
		ToPlainText(markdown, stringWriter, pipeline, context);
		return stringWriter.ToString();
	}
}
