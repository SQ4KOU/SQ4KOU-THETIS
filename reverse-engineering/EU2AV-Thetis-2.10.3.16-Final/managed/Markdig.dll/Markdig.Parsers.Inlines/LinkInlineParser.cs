using System;
using Markdig.Helpers;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace Markdig.Parsers.Inlines;

public class LinkInlineParser : InlineParser
{
	public readonly LinkOptions Options;

	public LinkInlineParser()
		: this(new LinkOptions())
	{
	}

	public LinkInlineParser(LinkOptions options)
	{
		Options = options ?? throw new ArgumentNullException("options");
		base.OpeningCharacters = new char[3] { '[', ']', '!' };
	}

	public override bool Match(InlineProcessor processor, ref StringSlice slice)
	{
		char c = slice.CurrentChar;
		int sourcePosition = processor.GetSourcePosition(slice.Start, out var lineIndex, out var column);
		bool isImage = false;
		if (c == '!')
		{
			isImage = true;
			c = slice.NextChar();
			if (c != '[')
			{
				return false;
			}
		}
		SourceSpan empty = SourceSpan.Empty;
		switch (c)
		{
		case '[':
		{
			StringSlice stringSlice = slice;
			string label;
			SourceSpan labelSpan;
			if (processor.TrackTrivia)
			{
				if (LinkHelper.TryParseLabelTrivia(ref slice, out label, out labelSpan))
				{
					empty.Start = labelSpan.Start;
					empty.End = labelSpan.End;
					if (!processor.Document.ContainsLinkReferenceDefinition(label))
					{
						label = null;
					}
				}
			}
			else if (LinkHelper.TryParseLabel(ref slice, out label, out labelSpan) && !processor.Document.ContainsLinkReferenceDefinition(label))
			{
				label = null;
			}
			slice = stringSlice;
			slice.SkipChar();
			LinkDelimiterInline linkDelimiterInline = new LinkDelimiterInline(this)
			{
				Type = DelimiterType.Open,
				Label = label,
				LabelSpan = processor.GetSourcePositionFromLocalSpan(labelSpan),
				IsImage = isImage,
				Span = new SourceSpan(sourcePosition, processor.GetSourcePosition(slice.Start - 1)),
				Line = lineIndex,
				Column = column
			};
			if (processor.TrackTrivia)
			{
				linkDelimiterInline.LabelWithTrivia = new StringSlice(slice.Text, empty.Start, empty.End);
			}
			processor.Inline = linkDelimiterInline;
			return true;
		}
		case ']':
			slice.SkipChar();
			if (processor.Inline != null && TryProcessLinkOrImage(processor, ref slice))
			{
				return true;
			}
			return false;
		default:
			return false;
		}
	}

	private bool ProcessLinkReference(InlineProcessor state, StringSlice text, string label, SourceSpan labelWithriviaSpan, bool isShortcut, SourceSpan labelSpan, LinkDelimiterInline parent, int endPosition, LocalLabel localLabel)
	{
		if (!state.Document.TryGetLinkReferenceDefinition(label, out LinkReferenceDefinition linkReferenceDefinition))
		{
			return false;
		}
		Inline inline = null;
		if (linkReferenceDefinition.CreateLinkInline != null)
		{
			inline = linkReferenceDefinition.CreateLinkInline(state, linkReferenceDefinition, parent.FirstChild);
			inline.Span = new SourceSpan(parent.Span.Start, endPosition);
			inline.Line = parent.Line;
			inline.Column = parent.Column;
		}
		if (inline == null)
		{
			LinkInline linkInline = new LinkInline
			{
				Url = HtmlHelper.Unescape(linkReferenceDefinition.Url, removeBackSlash: false),
				Title = HtmlHelper.Unescape(linkReferenceDefinition.Title, removeBackSlash: false),
				Label = label,
				LabelSpan = labelSpan,
				UrlSpan = linkReferenceDefinition.UrlSpan,
				IsImage = parent.IsImage,
				IsShortcut = isShortcut,
				Reference = linkReferenceDefinition,
				Span = new SourceSpan(parent.Span.Start, endPosition),
				Line = parent.Line,
				Column = parent.Column
			};
			if (state.TrackTrivia)
			{
				linkInline.LabelWithTrivia = new StringSlice(text.Text, labelWithriviaSpan.Start, labelWithriviaSpan.End);
				linkInline.LinkRefDefLabel = linkReferenceDefinition.Label;
				linkInline.LinkRefDefLabelWithTrivia = linkReferenceDefinition.LabelWithTrivia;
				linkInline.LocalLabel = localLabel;
			}
			if (Options.OpenInNewWindow)
			{
				linkInline.GetAttributes().AddPropertyIfNotExist("target", "_blank");
			}
			inline = linkInline;
		}
		if (inline is ContainerInline containerInline)
		{
			Inline inline2 = parent.FirstChild;
			if (inline2 == null)
			{
				inline2 = new LiteralInline
				{
					Content = StringSlice.Empty,
					IsClosed = true,
					Span = parent.Span,
					Line = parent.Line,
					Column = parent.Column
				};
				containerInline.AppendChild(inline2);
			}
			else
			{
				while (inline2 != null)
				{
					Inline? nextSibling = inline2.NextSibling;
					inline2.Remove();
					containerInline.AppendChild(inline2);
					inline2 = nextSibling;
				}
			}
		}
		inline.IsClosed = true;
		state.PostProcessInlines(0, inline, null, isFinalProcessing: false);
		state.Inline = inline;
		return true;
	}

	private bool TryProcessLinkOrImage(InlineProcessor inlineState, ref StringSlice text)
	{
		LinkDelimiterInline linkDelimiterInline = inlineState.Inline.FirstParentOfType<LinkDelimiterInline>();
		if (linkDelimiterInline == null)
		{
			return false;
		}
		if (!linkDelimiterInline.IsActive)
		{
			inlineState.Inline = new LiteralInline
			{
				Content = new StringSlice("["),
				Span = linkDelimiterInline.Span,
				Line = linkDelimiterInline.Line,
				Column = linkDelimiterInline.Column
			};
			linkDelimiterInline.ReplaceBy(inlineState.Inline);
			return false;
		}
		ContainerInline parent = linkDelimiterInline.Parent;
		StringSlice stringSlice = text;
		if (text.CurrentChar == '(')
		{
			LinkInline linkInline = null;
			string link;
			string title;
			SourceSpan linkSpan;
			SourceSpan titleSpan;
			if (inlineState.TrackTrivia)
			{
				linkInline = TryParseInlineLinkTrivia(ref text, inlineState, linkDelimiterInline);
			}
			else if (LinkHelper.TryParseInlineLink(ref text, out link, out title, out linkSpan, out titleSpan))
			{
				linkInline = new LinkInline
				{
					Url = HtmlHelper.Unescape(link, removeBackSlash: false),
					Title = ((title == null) ? null : HtmlHelper.Unescape(title, removeBackSlash: false)),
					IsImage = linkDelimiterInline.IsImage,
					LabelSpan = linkDelimiterInline.LabelSpan,
					UrlSpan = inlineState.GetSourcePositionFromLocalSpan(linkSpan),
					TitleSpan = inlineState.GetSourcePositionFromLocalSpan(titleSpan),
					Span = new SourceSpan(linkDelimiterInline.Span.Start, inlineState.GetSourcePosition(text.Start - 1)),
					Line = linkDelimiterInline.Line,
					Column = linkDelimiterInline.Column
				};
			}
			if (linkInline != null)
			{
				linkDelimiterInline.ReplaceBy(linkInline);
				inlineState.Inline = linkInline;
				inlineState.PostProcessInlines(0, linkInline, null, isFinalProcessing: false);
				if (!linkDelimiterInline.IsImage)
				{
					MarkParentAsInactive(parent);
				}
				linkInline.IsClosed = true;
				return true;
			}
			text = stringSlice;
		}
		SourceSpan labelSpan = SourceSpan.Empty;
		string label = null;
		bool flag = true;
		bool isShortcut = false;
		LocalLabel localLabel = LocalLabel.Local;
		if (text.CurrentChar == '[')
		{
			if (text.PeekChar() == ']')
			{
				label = linkDelimiterInline.Label;
				labelSpan = linkDelimiterInline.LabelSpan;
				flag = false;
				localLabel = LocalLabel.Empty;
				text.SkipChar();
				text.SkipChar();
			}
		}
		else
		{
			localLabel = LocalLabel.None;
			label = linkDelimiterInline.Label;
			isShortcut = true;
		}
		if (label != null || LinkHelper.TryParseLabelTrivia(ref text, allowEmpty: true, out label, out labelSpan))
		{
			SourceSpan labelWithriviaSpan = new SourceSpan(labelSpan.Start, labelSpan.End);
			if (flag)
			{
				labelSpan = inlineState.GetSourcePositionFromLocalSpan(labelSpan);
			}
			if (ProcessLinkReference(inlineState, text, label, labelWithriviaSpan, isShortcut, labelSpan, linkDelimiterInline, inlineState.GetSourcePosition(text.Start - 1), localLabel))
			{
				linkDelimiterInline.Remove();
				if (!linkDelimiterInline.IsImage)
				{
					MarkParentAsInactive(parent);
				}
				return true;
			}
			if (text.CurrentChar != ']' && text.CurrentChar != '[')
			{
				return false;
			}
		}
		LiteralInline inline = new LiteralInline
		{
			Span = linkDelimiterInline.Span,
			Content = new StringSlice(linkDelimiterInline.IsImage ? "![" : "[")
		};
		inlineState.Inline = linkDelimiterInline.ReplaceBy(inline);
		return false;
		static LinkInline? TryParseInlineLinkTrivia(ref StringSlice reference, InlineProcessor inlineProcessor, LinkDelimiterInline openParent)
		{
			if (LinkHelper.TryParseInlineLinkTrivia(ref reference, out string link2, out SourceSpan unescapedLink, out string title2, out SourceSpan unescapedTitle, out char titleEnclosingCharacter, out SourceSpan linkSpan2, out SourceSpan titleSpan2, out SourceSpan triviaBeforeLink, out SourceSpan triviaAfterLink, out SourceSpan triviaAfterTitle, out bool urlHasPointyBrackets))
			{
				StringSlice triviaBeforeUrl = new StringSlice(reference.Text, triviaBeforeLink.Start, triviaBeforeLink.End);
				StringSlice triviaAfterUrl = new StringSlice(reference.Text, triviaAfterLink.Start, triviaAfterLink.End);
				StringSlice triviaAfterTitle2 = new StringSlice(reference.Text, triviaAfterTitle.Start, triviaAfterTitle.End);
				StringSlice unescapedUrl = new StringSlice(reference.Text, unescapedLink.Start, unescapedLink.End);
				StringSlice unescapedTitle2 = new StringSlice(reference.Text, unescapedTitle.Start, unescapedTitle.End);
				return new LinkInline
				{
					TriviaBeforeUrl = triviaBeforeUrl,
					Url = HtmlHelper.Unescape(link2, removeBackSlash: false),
					UnescapedUrl = unescapedUrl,
					UrlHasPointyBrackets = urlHasPointyBrackets,
					TriviaAfterUrl = triviaAfterUrl,
					Title = HtmlHelper.Unescape(title2, removeBackSlash: false),
					UnescapedTitle = unescapedTitle2,
					TitleEnclosingCharacter = titleEnclosingCharacter,
					TriviaAfterTitle = triviaAfterTitle2,
					IsImage = openParent.IsImage,
					LabelSpan = openParent.LabelSpan,
					UrlSpan = inlineProcessor.GetSourcePositionFromLocalSpan(linkSpan2),
					TitleSpan = inlineProcessor.GetSourcePositionFromLocalSpan(titleSpan2),
					Span = new SourceSpan(openParent.Span.Start, inlineProcessor.GetSourcePosition(reference.Start - 1)),
					Line = openParent.Line,
					Column = openParent.Column
				};
			}
			return null;
		}
	}

	private static void MarkParentAsInactive(Inline? inline)
	{
		while (inline != null)
		{
			if (inline is LinkDelimiterInline linkDelimiterInline)
			{
				if (linkDelimiterInline.IsImage)
				{
					break;
				}
				linkDelimiterInline.IsActive = false;
			}
			inline = inline.Parent;
		}
	}
}
