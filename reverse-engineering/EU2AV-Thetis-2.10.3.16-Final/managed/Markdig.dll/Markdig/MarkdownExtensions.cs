using System;
using Markdig.Extensions.Abbreviations;
using Markdig.Extensions.Alerts;
using Markdig.Extensions.AutoIdentifiers;
using Markdig.Extensions.AutoLinks;
using Markdig.Extensions.Bootstrap;
using Markdig.Extensions.Citations;
using Markdig.Extensions.CustomContainers;
using Markdig.Extensions.DefinitionLists;
using Markdig.Extensions.Diagrams;
using Markdig.Extensions.Emoji;
using Markdig.Extensions.EmphasisExtras;
using Markdig.Extensions.Figures;
using Markdig.Extensions.Footers;
using Markdig.Extensions.Footnotes;
using Markdig.Extensions.GenericAttributes;
using Markdig.Extensions.Globalization;
using Markdig.Extensions.Hardlines;
using Markdig.Extensions.JiraLinks;
using Markdig.Extensions.ListExtras;
using Markdig.Extensions.Mathematics;
using Markdig.Extensions.MediaLinks;
using Markdig.Extensions.NonAsciiNoEscape;
using Markdig.Extensions.PragmaLines;
using Markdig.Extensions.ReferralLinks;
using Markdig.Extensions.SelfPipeline;
using Markdig.Extensions.SmartyPants;
using Markdig.Extensions.Tables;
using Markdig.Extensions.TaskLists;
using Markdig.Extensions.TextRenderer;
using Markdig.Extensions.Yaml;
using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Parsers.Inlines;
using Markdig.Renderers;
using Markdig.Syntax;

namespace Markdig;

public static class MarkdownExtensions
{
	public static MarkdownPipelineBuilder Use<TExtension>(this MarkdownPipelineBuilder pipeline) where TExtension : class, IMarkdownExtension, new()
	{
		pipeline.Extensions.AddIfNotAlready<TExtension>();
		return pipeline;
	}

	public static MarkdownPipelineBuilder Use<TExtension>(this MarkdownPipelineBuilder pipeline, TExtension extension) where TExtension : class, IMarkdownExtension
	{
		pipeline.Extensions.AddIfNotAlready(extension);
		return pipeline;
	}

	public static MarkdownPipelineBuilder UseAdvancedExtensions(this MarkdownPipelineBuilder pipeline)
	{
		return pipeline.UseAlertBlocks().UseAbbreviations().UseAutoIdentifiers()
			.UseCitations()
			.UseCustomContainers()
			.UseDefinitionLists()
			.UseEmphasisExtras()
			.UseFigures()
			.UseFooters()
			.UseFootnotes()
			.UseGridTables()
			.UseMathematics()
			.UseMediaLinks()
			.UsePipeTables()
			.UseListExtras()
			.UseTaskLists()
			.UseDiagrams()
			.UseAutoLinks()
			.UseGenericAttributes();
	}

	public static MarkdownPipelineBuilder UseAlertBlocks(this MarkdownPipelineBuilder pipeline, Action<HtmlRenderer, StringSlice>? renderKind = null)
	{
		pipeline.Extensions.ReplaceOrAdd<AlertExtension>(new AlertExtension
		{
			RenderKind = renderKind
		});
		return pipeline;
	}

	public static MarkdownPipelineBuilder UseAutoLinks(this MarkdownPipelineBuilder pipeline, AutoLinkOptions? options = null)
	{
		pipeline.Extensions.ReplaceOrAdd<AutoLinkExtension>(new AutoLinkExtension(options));
		return pipeline;
	}

	public static MarkdownPipelineBuilder UseNonAsciiNoEscape(this MarkdownPipelineBuilder pipeline)
	{
		pipeline.Extensions.AddIfNotAlready<NonAsciiNoEscapeExtension>();
		return pipeline;
	}

	public static MarkdownPipelineBuilder UseYamlFrontMatter(this MarkdownPipelineBuilder pipeline)
	{
		pipeline.Extensions.AddIfNotAlready<YamlFrontMatterExtension>();
		return pipeline;
	}

	public static MarkdownPipelineBuilder UseSelfPipeline(this MarkdownPipelineBuilder pipeline, string defaultTag = "markdig", string? defaultExtensions = null)
	{
		if (pipeline.Extensions.Count != 0)
		{
			ThrowHelper.InvalidOperationException("The SelfPipeline extension cannot be used with other extensions");
		}
		pipeline.Extensions.Add(new SelfPipelineExtension(defaultTag, defaultExtensions));
		return pipeline;
	}

	public static MarkdownPipelineBuilder UsePragmaLines(this MarkdownPipelineBuilder pipeline)
	{
		pipeline.Extensions.AddIfNotAlready<PragmaLineExtension>();
		return pipeline;
	}

	public static MarkdownPipelineBuilder UseDiagrams(this MarkdownPipelineBuilder pipeline)
	{
		pipeline.Extensions.AddIfNotAlready<DiagramExtension>();
		return pipeline;
	}

	public static MarkdownPipelineBuilder UsePreciseSourceLocation(this MarkdownPipelineBuilder pipeline)
	{
		pipeline.PreciseSourceLocation = true;
		return pipeline;
	}

	public static MarkdownPipelineBuilder UseTaskLists(this MarkdownPipelineBuilder pipeline)
	{
		pipeline.Extensions.AddIfNotAlready<TaskListExtension>();
		return pipeline;
	}

	public static MarkdownPipelineBuilder UseCustomContainers(this MarkdownPipelineBuilder pipeline)
	{
		pipeline.Extensions.AddIfNotAlready<CustomContainerExtension>();
		return pipeline;
	}

	public static MarkdownPipelineBuilder UseMediaLinks(this MarkdownPipelineBuilder pipeline, MediaOptions? options = null)
	{
		if (!pipeline.Extensions.Contains<MediaLinkExtension>())
		{
			pipeline.Extensions.Add(new MediaLinkExtension(options));
		}
		return pipeline;
	}

	public static MarkdownPipelineBuilder UseAutoIdentifiers(this MarkdownPipelineBuilder pipeline, AutoIdentifierOptions options = AutoIdentifierOptions.Default)
	{
		if (!pipeline.Extensions.Contains<AutoIdentifierExtension>())
		{
			pipeline.Extensions.Add(new AutoIdentifierExtension(options));
		}
		return pipeline;
	}

	public static MarkdownPipelineBuilder UseSmartyPants(this MarkdownPipelineBuilder pipeline, SmartyPantOptions? options = null)
	{
		if (!pipeline.Extensions.Contains<SmartyPantsExtension>())
		{
			pipeline.Extensions.Add(new SmartyPantsExtension(options));
		}
		return pipeline;
	}

	public static MarkdownPipelineBuilder UseBootstrap(this MarkdownPipelineBuilder pipeline)
	{
		pipeline.Extensions.AddIfNotAlready<BootstrapExtension>();
		return pipeline;
	}

	public static MarkdownPipelineBuilder UseMathematics(this MarkdownPipelineBuilder pipeline)
	{
		pipeline.Extensions.AddIfNotAlready<MathExtension>();
		return pipeline;
	}

	public static MarkdownPipelineBuilder UseFigures(this MarkdownPipelineBuilder pipeline)
	{
		pipeline.Extensions.AddIfNotAlready<FigureExtension>();
		return pipeline;
	}

	public static MarkdownPipelineBuilder UseAbbreviations(this MarkdownPipelineBuilder pipeline)
	{
		pipeline.Extensions.AddIfNotAlready<AbbreviationExtension>();
		return pipeline;
	}

	public static MarkdownPipelineBuilder UseDefinitionLists(this MarkdownPipelineBuilder pipeline)
	{
		pipeline.Extensions.AddIfNotAlready<DefinitionListExtension>();
		return pipeline;
	}

	public static MarkdownPipelineBuilder UsePipeTables(this MarkdownPipelineBuilder pipeline, PipeTableOptions? options = null)
	{
		if (!pipeline.Extensions.Contains<PipeTableExtension>())
		{
			pipeline.Extensions.Add(new PipeTableExtension(options));
		}
		return pipeline;
	}

	public static MarkdownPipelineBuilder UseGridTables(this MarkdownPipelineBuilder pipeline)
	{
		pipeline.Extensions.AddIfNotAlready<GridTableExtension>();
		return pipeline;
	}

	public static MarkdownPipelineBuilder UseCitations(this MarkdownPipelineBuilder pipeline)
	{
		pipeline.Extensions.AddIfNotAlready<CitationExtension>();
		return pipeline;
	}

	public static MarkdownPipelineBuilder UseFooters(this MarkdownPipelineBuilder pipeline)
	{
		pipeline.Extensions.AddIfNotAlready<FooterExtension>();
		return pipeline;
	}

	public static MarkdownPipelineBuilder UseFootnotes(this MarkdownPipelineBuilder pipeline)
	{
		pipeline.Extensions.AddIfNotAlready<FootnoteExtension>();
		return pipeline;
	}

	public static MarkdownPipelineBuilder UseSoftlineBreakAsHardlineBreak(this MarkdownPipelineBuilder pipeline)
	{
		pipeline.Extensions.AddIfNotAlready<SoftlineBreakAsHardlineExtension>();
		return pipeline;
	}

	public static MarkdownPipelineBuilder UseEmphasisExtras(this MarkdownPipelineBuilder pipeline, EmphasisExtraOptions options = EmphasisExtraOptions.Default)
	{
		if (!pipeline.Extensions.Contains<EmphasisExtraExtension>())
		{
			pipeline.Extensions.Add(new EmphasisExtraExtension(options));
		}
		return pipeline;
	}

	public static MarkdownPipelineBuilder UseListExtras(this MarkdownPipelineBuilder pipeline)
	{
		pipeline.Extensions.AddIfNotAlready<ListExtraExtension>();
		return pipeline;
	}

	public static MarkdownPipelineBuilder UseGenericAttributes(this MarkdownPipelineBuilder pipeline)
	{
		pipeline.Extensions.AddIfNotAlready<GenericAttributesExtension>();
		return pipeline;
	}

	public static MarkdownPipelineBuilder UseEmojiAndSmiley(this MarkdownPipelineBuilder pipeline, bool enableSmileys = true)
	{
		if (!pipeline.Extensions.Contains<EmojiExtension>())
		{
			EmojiMapping emojiMapping = (enableSmileys ? EmojiMapping.DefaultEmojisAndSmileysMapping : EmojiMapping.DefaultEmojisOnlyMapping);
			pipeline.Extensions.Add(new EmojiExtension(emojiMapping));
		}
		return pipeline;
	}

	public static MarkdownPipelineBuilder UseEmojiAndSmiley(this MarkdownPipelineBuilder pipeline, EmojiMapping customEmojiMapping)
	{
		if (!pipeline.Extensions.Contains<EmojiExtension>())
		{
			pipeline.Extensions.Add(new EmojiExtension(customEmojiMapping));
		}
		return pipeline;
	}

	public static MarkdownPipelineBuilder UseReferralLinks(this MarkdownPipelineBuilder pipeline, params string[] rels)
	{
		if (pipeline.Extensions.TryFind<ReferralLinksExtension>(out ReferralLinksExtension item))
		{
			foreach (string item2 in rels)
			{
				if (!item.Rels.Contains(item2))
				{
					item.Rels.Add(item2);
				}
			}
		}
		else
		{
			pipeline.Extensions.Add(new ReferralLinksExtension(rels));
		}
		return pipeline;
	}

	public static MarkdownPipelineBuilder UseJiraLinks(this MarkdownPipelineBuilder pipeline, JiraLinkOptions options)
	{
		if (!pipeline.Extensions.Contains<JiraLinkExtension>())
		{
			pipeline.Extensions.Add(new JiraLinkExtension(options));
		}
		return pipeline;
	}

	public static MarkdownPipelineBuilder UseGlobalization(this MarkdownPipelineBuilder pipeline)
	{
		pipeline.Extensions.AddIfNotAlready<GlobalizationExtension>();
		return pipeline;
	}

	public static MarkdownPipelineBuilder UseCjkFriendlyEmphasis(this MarkdownPipelineBuilder pipeline)
	{
		EmphasisInlineParser? emphasisInlineParser = pipeline.InlineParsers.FindExact<EmphasisInlineParser>();
		if (emphasisInlineParser != null)
		{
			emphasisInlineParser.CjkFriendlyEmphasis = true;
		}
		return pipeline;
	}

	public static MarkdownPipelineBuilder DisableHtml(this MarkdownPipelineBuilder pipeline)
	{
		HtmlBlockParser htmlBlockParser = pipeline.BlockParsers.Find<HtmlBlockParser>();
		if (htmlBlockParser != null)
		{
			pipeline.BlockParsers.Remove(htmlBlockParser);
		}
		AutolinkInlineParser autolinkInlineParser = pipeline.InlineParsers.Find<AutolinkInlineParser>();
		if (autolinkInlineParser != null)
		{
			autolinkInlineParser.Options.EnableHtmlParsing = false;
		}
		return pipeline;
	}

	public static MarkdownPipelineBuilder Configure(this MarkdownPipelineBuilder pipeline, string? extensions)
	{
		if (extensions == null)
		{
			return pipeline;
		}
		string[] array = extensions.Split(new char[1] { '+' }, StringSplitOptions.RemoveEmptyEntries);
		foreach (string text in array)
		{
			switch (text.ToLowerInvariant())
			{
			case "advanced":
				pipeline.UseAdvancedExtensions();
				continue;
			case "alerts":
				pipeline.UseAlertBlocks();
				continue;
			case "pipetables":
				pipeline.UsePipeTables();
				continue;
			case "gfm-pipetables":
				pipeline.UsePipeTables(new PipeTableOptions
				{
					UseHeaderForColumnCount = true
				});
				continue;
			case "emphasisextras":
				pipeline.UseEmphasisExtras();
				continue;
			case "listextras":
				pipeline.UseListExtras();
				continue;
			case "hardlinebreak":
				pipeline.UseSoftlineBreakAsHardlineBreak();
				continue;
			case "footnotes":
				pipeline.UseFootnotes();
				continue;
			case "footers":
				pipeline.UseFooters();
				continue;
			case "citations":
				pipeline.UseCitations();
				continue;
			case "attributes":
				pipeline.UseGenericAttributes();
				continue;
			case "gridtables":
				pipeline.UseGridTables();
				continue;
			case "abbreviations":
				pipeline.UseAbbreviations();
				continue;
			case "emojis":
				pipeline.UseEmojiAndSmiley();
				continue;
			case "definitionlists":
				pipeline.UseDefinitionLists();
				continue;
			case "customcontainers":
				pipeline.UseCustomContainers();
				continue;
			case "figures":
				pipeline.UseFigures();
				continue;
			case "mathematics":
				pipeline.UseMathematics();
				continue;
			case "bootstrap":
				pipeline.UseBootstrap();
				continue;
			case "medialinks":
				pipeline.UseMediaLinks();
				continue;
			case "smartypants":
				pipeline.UseSmartyPants();
				continue;
			case "autoidentifiers":
				pipeline.UseAutoIdentifiers();
				continue;
			case "tasklists":
				pipeline.UseTaskLists();
				continue;
			case "diagrams":
				pipeline.UseDiagrams();
				continue;
			case "nofollowlinks":
				pipeline.UseReferralLinks("nofollow");
				continue;
			case "noopenerlinks":
				pipeline.UseReferralLinks("noopener");
				continue;
			case "noreferrerlinks":
				pipeline.UseReferralLinks("noreferrer");
				continue;
			case "nohtml":
				pipeline.DisableHtml();
				continue;
			case "yaml":
				pipeline.UseYamlFrontMatter();
				continue;
			case "nonascii-noescape":
				pipeline.UseNonAsciiNoEscape();
				continue;
			case "autolinks":
				pipeline.UseAutoLinks();
				continue;
			case "globalization":
				pipeline.UseGlobalization();
				continue;
			case "cjk-friendly-emphasis":
				pipeline.UseCjkFriendlyEmphasis();
				continue;
			case "common":
				continue;
			}
			throw new ArgumentException("Invalid extension `" + text + "` from `" + extensions + "`", "extensions");
		}
		return pipeline;
	}

	public static MarkdownPipelineBuilder ConfigureNewLine(this MarkdownPipelineBuilder pipeline, string newLine)
	{
		pipeline.Use(new ConfigureNewLineExtension(newLine));
		return pipeline;
	}

	public static MarkdownPipelineBuilder DisableHeadings(this MarkdownPipelineBuilder pipeline)
	{
		pipeline.BlockParsers.TryRemove<HeadingBlockParser>();
		if (pipeline.BlockParsers.TryFind<ParagraphBlockParser>(out ParagraphBlockParser item))
		{
			item.ParseSetexHeadings = false;
		}
		return pipeline;
	}

	public static MarkdownPipelineBuilder EnableTrackTrivia(this MarkdownPipelineBuilder pipeline)
	{
		pipeline.TrackTrivia = true;
		if (pipeline.BlockParsers.TryFind<FencedCodeBlockParser>(out FencedCodeBlockParser item))
		{
			item.InfoParser = FencedBlockParserBase<FencedCodeBlock>.RoundtripInfoParser;
		}
		return pipeline;
	}
}
