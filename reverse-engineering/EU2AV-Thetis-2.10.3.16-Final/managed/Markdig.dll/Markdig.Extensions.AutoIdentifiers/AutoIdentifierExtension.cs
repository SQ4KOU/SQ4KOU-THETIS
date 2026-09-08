using System;
using System.Collections.Generic;
using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace Markdig.Extensions.AutoIdentifiers;

public class AutoIdentifierExtension : IMarkdownExtension
{
	private sealed class StripRendererCache : ObjectCache<HtmlRenderer>
	{
		protected override HtmlRenderer NewInstance()
		{
			return new HtmlRenderer(new FastStringWriter())
			{
				EnableHtmlForInline = false,
				EnableHtmlEscape = false
			};
		}

		protected override void Reset(HtmlRenderer instance)
		{
			instance.ResetInternal();
			((FastStringWriter)instance.Writer).Reset();
		}
	}

	private const string AutoIdentifierKey = "AutoIdentifier";

	private static readonly StripRendererCache _rendererCache = new StripRendererCache();

	private readonly AutoIdentifierOptions _options;

	private readonly ProcessInlineDelegate _processInlinesBegin;

	private readonly ProcessInlineDelegate _processInlinesEnd;

	public AutoIdentifierExtension(AutoIdentifierOptions options)
	{
		_options = options;
		_processInlinesBegin = DocumentOnProcessInlinesBegin;
		_processInlinesEnd = HeadingBlock_ProcessInlinesEnd;
	}

	public void Setup(MarkdownPipelineBuilder pipeline)
	{
		HeadingBlockParser headingBlockParser = pipeline.BlockParsers.Find<HeadingBlockParser>();
		if (headingBlockParser != null)
		{
			headingBlockParser.Closed -= HeadingBlockParser_Closed;
			headingBlockParser.Closed += HeadingBlockParser_Closed;
		}
		ParagraphBlockParser paragraphBlockParser = pipeline.BlockParsers.FindExact<ParagraphBlockParser>();
		if (paragraphBlockParser != null)
		{
			paragraphBlockParser.Closed -= HeadingBlockParser_Closed;
			paragraphBlockParser.Closed += HeadingBlockParser_Closed;
		}
	}

	public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
	{
	}

	private void HeadingBlockParser_Closed(BlockProcessor processor, Block block)
	{
		if (!(block is HeadingBlock headingBlock))
		{
			return;
		}
		if ((_options & AutoIdentifierOptions.AutoLink) != AutoIdentifierOptions.None)
		{
			StringLine stringLine = headingBlock.Lines.Lines[0];
			string key = stringLine.ToString();
			HeadingLinkReferenceDefinition value = new HeadingLinkReferenceDefinition(headingBlock)
			{
				CreateLinkInline = CreateLinkInlineForHeading
			};
			MarkdownDocument document = processor.Document;
			Dictionary<string, HeadingLinkReferenceDefinition> dictionary = document.GetData(this) as Dictionary<string, HeadingLinkReferenceDefinition>;
			if (dictionary == null)
			{
				dictionary = new Dictionary<string, HeadingLinkReferenceDefinition>();
				document.SetData(this, dictionary);
				document.ProcessInlinesBegin += _processInlinesBegin;
			}
			dictionary[key] = value;
		}
		headingBlock.ProcessInlinesEnd += _processInlinesEnd;
	}

	private void DocumentOnProcessInlinesBegin(InlineProcessor processor, Inline? inline)
	{
		MarkdownDocument document = processor.Document;
		foreach (KeyValuePair<string, HeadingLinkReferenceDefinition> item in (Dictionary<string, HeadingLinkReferenceDefinition>)document.GetData(this))
		{
			if (!document.TryGetLinkReferenceDefinition(item.Key, out LinkReferenceDefinition _))
			{
				document.SetLinkReferenceDefinition(item.Key, item.Value, addGroup: true);
			}
		}
		document.RemoveData(this);
	}

	private static Inline CreateLinkInlineForHeading(InlineProcessor inlineState, LinkReferenceDefinition linkRef, Inline? child)
	{
		HeadingLinkReferenceDefinition headingRef = (HeadingLinkReferenceDefinition)linkRef;
		return new LinkInline
		{
			GetDynamicUrl = () => HtmlHelper.Unescape("#" + headingRef.Heading.GetAttributes().Id),
			Title = HtmlHelper.Unescape(linkRef.Title)
		};
	}

	private void HeadingBlock_ProcessInlinesEnd(InlineProcessor processor, Inline? inline)
	{
		HashSet<string> hashSet = processor.Document.GetData("AutoIdentifier") as HashSet<string>;
		if (hashSet == null)
		{
			hashSet = new HashSet<string>();
			processor.Document.SetData("AutoIdentifier", hashSet);
		}
		HeadingBlock headingBlock = (HeadingBlock)processor.Block;
		if (headingBlock.Inline == null)
		{
			return;
		}
		HtmlAttributes attributes = processor.Block.GetAttributes();
		if (attributes.Id != null)
		{
			return;
		}
		HtmlRenderer htmlRenderer = _rendererCache.Get();
		htmlRenderer.Render(headingBlock.Inline);
		ReadOnlySpan<char> headingText = ((FastStringWriter)htmlRenderer.Writer).AsSpan();
		string text = (((_options & AutoIdentifierOptions.GitHub) != AutoIdentifierOptions.None) ? LinkHelper.UrilizeAsGfm(headingText) : LinkHelper.Urilize(headingText, (_options & AutoIdentifierOptions.AllowOnlyAscii) != 0));
		_rendererCache.Release(htmlRenderer);
		string text2 = (string.IsNullOrEmpty(text) ? "section" : text);
		string text3 = text2;
		if (!hashSet.Add(text3))
		{
			Span<char> initialBuffer = stackalloc char[64];
			ValueStringBuilder valueStringBuilder = new ValueStringBuilder(initialBuffer);
			valueStringBuilder.Append(text2);
			valueStringBuilder.Append('-');
			uint num = 0u;
			do
			{
				num++;
				valueStringBuilder.Append(num);
				text3 = valueStringBuilder.AsSpan().ToString();
				valueStringBuilder.Length = text2.Length + 1;
			}
			while (!hashSet.Add(text3));
			valueStringBuilder.Dispose();
		}
		attributes.Id = text3;
	}
}
