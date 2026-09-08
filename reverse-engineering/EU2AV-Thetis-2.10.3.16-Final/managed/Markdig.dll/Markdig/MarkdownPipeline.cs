using System;
using System.IO;
using Markdig.Extensions.SelfPipeline;
using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Renderers;

namespace Markdig;

public sealed class MarkdownPipeline
{
	internal sealed class HtmlRendererCache(MarkdownPipeline pipeline, bool customWriter = false) : ObjectCache<HtmlRenderer>
	{
		private static readonly FastStringWriter s_dummyWriter = new FastStringWriter();

		private readonly MarkdownPipeline _pipeline = pipeline;

		private readonly bool _customWriter = customWriter;

		protected override HtmlRenderer NewInstance()
		{
			HtmlRenderer htmlRenderer = new HtmlRenderer(_customWriter ? s_dummyWriter : new FastStringWriter());
			_pipeline.Setup(htmlRenderer);
			return htmlRenderer;
		}

		protected override void Reset(HtmlRenderer instance)
		{
			instance.ResetInternal();
			if (_customWriter)
			{
				instance.Writer = s_dummyWriter;
			}
			else
			{
				((FastStringWriter)instance.Writer).Reset();
			}
		}
	}

	internal readonly ref struct RentedHtmlRenderer : IDisposable
	{
		private readonly HtmlRendererCache _cache;

		public readonly HtmlRenderer Instance;

		internal RentedHtmlRenderer(HtmlRendererCache cache, HtmlRenderer renderer)
		{
			_cache = cache;
			Instance = renderer;
		}

		public void Dispose()
		{
			_cache.Release(Instance);
		}
	}

	internal ProcessDocumentDelegate? DocumentProcessed;

	internal SelfPipelineExtension? SelfPipeline;

	private HtmlRendererCache? _rendererCache;

	private HtmlRendererCache? _rendererCacheForCustomWriter;

	internal bool PreciseSourceLocation { get; set; }

	public OrderedList<IMarkdownExtension> Extensions { get; }

	internal BlockParserList BlockParsers { get; }

	internal InlineParserList InlineParsers { get; }

	internal TextWriter? DebugLog { get; }

	public bool TrackTrivia { get; internal set; }

	internal MarkdownPipeline(OrderedList<IMarkdownExtension> extensions, BlockParserList blockParsers, InlineParserList inlineParsers, TextWriter? debugLog, ProcessDocumentDelegate? documentProcessed)
	{
		if (blockParsers == null)
		{
			ThrowHelper.ArgumentNullException("blockParsers");
		}
		if (inlineParsers == null)
		{
			ThrowHelper.ArgumentNullException("inlineParsers");
		}
		Extensions = extensions;
		BlockParsers = blockParsers;
		InlineParsers = inlineParsers;
		DebugLog = debugLog;
		DocumentProcessed = documentProcessed;
		SelfPipeline = Extensions.Find<SelfPipelineExtension>();
	}

	public void Setup(IMarkdownRenderer renderer)
	{
		if (renderer == null)
		{
			ThrowHelper.ArgumentNullException("renderer");
		}
		foreach (IMarkdownExtension extension in Extensions)
		{
			extension.Setup(this, renderer);
		}
	}

	internal RentedHtmlRenderer RentHtmlRenderer(TextWriter? writer = null)
	{
		HtmlRendererCache? obj = ((writer == null) ? (_rendererCache ?? (_rendererCache = new HtmlRendererCache(this))) : (_rendererCacheForCustomWriter ?? (_rendererCacheForCustomWriter = new HtmlRendererCache(this, customWriter: true))));
		HtmlRenderer htmlRenderer = obj.Get();
		if (writer != null)
		{
			htmlRenderer.Writer = writer;
		}
		return new RentedHtmlRenderer(obj, htmlRenderer);
	}
}
