using System;
using System.Buffers;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using Markdig.Helpers;
using Markdig.Renderers.Html;
using Markdig.Renderers.Html.Inlines;
using Markdig.Syntax;

namespace Markdig.Renderers;

public class HtmlRenderer : TextRendererBase<HtmlRenderer>
{
	private static readonly IdnMapping s_idnMapping = new IdnMapping();

	private static readonly SearchValues<char> s_asciiNonEscapeChars = SearchValues.Create("!#$%()*+,-./0123456789:;=?@ABCDEFGHIJKLMNOPQRSTUVWXYZ_abcdefghijklmnopqrstuvwxyz");

	private static readonly SearchValues<char> s_escapedChars = SearchValues.Create("<>&\"");

	public bool EnableHtmlForInline { get; set; }

	public bool EnableHtmlForBlock { get; set; }

	public bool EnableHtmlEscape { get; set; }

	public bool ImplicitParagraph { get; set; }

	public bool UseNonAsciiNoEscape { get; set; }

	public Uri? BaseUrl { get; set; }

	public Func<string, string>? LinkRewriter { get; set; }

	public HtmlRenderer(TextWriter writer)
		: base(writer)
	{
		base.ObjectRenderers.Add(new CodeBlockRenderer());
		base.ObjectRenderers.Add(new ListRenderer());
		base.ObjectRenderers.Add(new HeadingRenderer());
		base.ObjectRenderers.Add(new HtmlBlockRenderer());
		base.ObjectRenderers.Add(new ParagraphRenderer());
		base.ObjectRenderers.Add(new QuoteBlockRenderer());
		base.ObjectRenderers.Add(new ThematicBreakRenderer());
		base.ObjectRenderers.Add(new AutolinkInlineRenderer());
		base.ObjectRenderers.Add(new CodeInlineRenderer());
		base.ObjectRenderers.Add(new DelimiterInlineRenderer());
		base.ObjectRenderers.Add(new EmphasisInlineRenderer());
		base.ObjectRenderers.Add(new LineBreakInlineRenderer());
		base.ObjectRenderers.Add(new HtmlInlineRenderer());
		base.ObjectRenderers.Add(new HtmlEntityInlineRenderer());
		base.ObjectRenderers.Add(new LinkInlineRenderer());
		base.ObjectRenderers.Add(new LiteralInlineRenderer());
		EnableHtmlForBlock = true;
		EnableHtmlForInline = true;
		EnableHtmlEscape = true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public HtmlRenderer WriteEscape(string? content)
	{
		WriteEscape(content.AsSpan());
		return this;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public HtmlRenderer WriteEscape(ref StringSlice slice, bool softEscape = false)
	{
		WriteEscape(slice.AsSpan(), softEscape);
		return this;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public HtmlRenderer WriteEscape(StringSlice slice, bool softEscape = false)
	{
		WriteEscape(slice.AsSpan(), softEscape);
		return this;
	}

	public HtmlRenderer WriteEscape(string content, int offset, int length, bool softEscape = false)
	{
		WriteEscape(content.AsSpan(offset, length), softEscape);
		return this;
	}

	public void WriteEscape(ReadOnlySpan<char> content, bool softEscape = false)
	{
		if (content.IsEmpty)
		{
			return;
		}
		WriteIndent();
		while (true)
		{
			int num = (softEscape ? content.IndexOfAny('<', '&') : content.IndexOfAny(s_escapedChars));
			if ((uint)num >= (uint)content.Length)
			{
				break;
			}
			WriteRaw(content.Slice(0, num));
			if (EnableHtmlEscape)
			{
				WriteRaw(content[num] switch
				{
					'<' => "&lt;", 
					'>' => "&gt;", 
					'&' => "&amp;", 
					_ => "&quot;", 
				});
			}
			content = content.Slice(num + 1);
		}
		WriteRaw(content);
	}

	public HtmlRenderer WriteEscapeUrl(string? content)
	{
		if (content == null)
		{
			return this;
		}
		if (BaseUrl != null && Uri.TryCreate(content, UriKind.RelativeOrAbsolute, out var result) && !result.IsAbsoluteUri)
		{
			content = new Uri(BaseUrl, result).AbsoluteUri;
		}
		if (LinkRewriter != null)
		{
			content = LinkRewriter(content);
		}
		if (!content.IsValid())
		{
			int num = content.IndexOf("://", StringComparison.Ordinal);
			if (num > 0)
			{
				num += 3;
				int num2 = content.AsSpan(num).IndexOfAny("/?#:");
				if (num2 < 0)
				{
					num2 = content.Length - num;
				}
				string text = null;
				try
				{
					text = s_idnMapping.GetAscii(content, num, num2);
				}
				catch
				{
				}
				if (text != null)
				{
					WriteEscapeUrlCore(content.AsSpan(0, num));
					WriteEscapeUrlCore(text.AsSpan());
					WriteEscapeUrlCore(content.AsSpan(num + num2));
					return this;
				}
			}
		}
		WriteEscapeUrlCore(content.AsSpan());
		return this;
	}

	private void WriteEscapeUrlCore(ReadOnlySpan<char> content)
	{
		WriteIndent();
		while (true)
		{
			int num = content.IndexOfAnyExcept(s_asciiNonEscapeChars);
			if ((uint)num >= (uint)content.Length)
			{
				break;
			}
			WriteRaw(content.Slice(0, num));
			char c = content[num];
			if (c < '\u0080')
			{
				WriteRaw(HtmlHelper.EscapeUrlCharacter(c));
			}
			else if (UseNonAsciiNoEscape)
			{
				WriteRaw(c);
			}
			else
			{
				num = WriteEscapedUtf8Bytes(this, content, c, num);
			}
			content = content.Slice(num + 1);
		}
		WriteRaw(content);
		static int WriteEscapedUtf8Bytes(HtmlRenderer renderer, ReadOnlySpan<char> readOnlySpan, char c2, int i)
		{
			ReadOnlySpan<char> chars;
			if (char.IsHighSurrogate(c2) && (uint)(i + 1) < (uint)readOnlySpan.Length)
			{
				chars = stackalloc char[2]
				{
					c2,
					readOnlySpan[i + 1]
				};
				i++;
			}
			else
			{
				chars = stackalloc char[1] { c2 };
			}
			Span<byte> span = stackalloc byte[4];
			int bytes = Encoding.UTF8.GetBytes(chars, span);
			span = span.Slice(0, bytes);
			Span<char> span2 = stackalloc char[3];
			span2[0] = '%';
			Span<byte> span3 = span;
			for (int j = 0; j < span3.Length; j++)
			{
				HexConverter.ToCharsBuffer(span3[j], span2, 1);
				renderer.WriteRaw(span2);
			}
			return i;
		}
	}

	public HtmlRenderer WriteAttributes(MarkdownObject markdownObject)
	{
		if (markdownObject == null)
		{
			ThrowHelper.ArgumentNullException_markdownObject();
		}
		return WriteAttributes(markdownObject.TryGetAttributes());
	}

	public HtmlRenderer WriteAttributes(HtmlAttributes? attributes, Func<string, string>? classFilter = null)
	{
		if (attributes == null)
		{
			return this;
		}
		if (attributes.Id != null)
		{
			Write(" id=\"");
			WriteEscape(attributes.Id);
			WriteRaw('"');
		}
		List<string> classes = attributes.Classes;
		if (classes != null && classes.Count > 0)
		{
			Write(" class=\"");
			for (int i = 0; i < attributes.Classes.Count; i++)
			{
				string text = attributes.Classes[i];
				if (i > 0)
				{
					WriteRaw(' ');
				}
				WriteEscape((classFilter != null) ? classFilter(text) : text);
			}
			WriteRaw('"');
		}
		List<KeyValuePair<string, string>> properties = attributes.Properties;
		if (properties != null && properties.Count > 0)
		{
			foreach (KeyValuePair<string, string> property in attributes.Properties)
			{
				Write(' ');
				WriteRaw(property.Key);
				WriteRaw("=\"");
				WriteEscape(property.Value ?? "");
				WriteRaw('"');
			}
		}
		return this;
	}

	public HtmlRenderer WriteLeafRawLines(LeafBlock leafBlock, bool writeEndOfLines, bool escape, bool softEscape = false)
	{
		if (leafBlock == null)
		{
			ThrowHelper.ArgumentNullException_leafBlock();
		}
		StringLine[] lines = leafBlock.Lines.Lines;
		if (lines != null)
		{
			for (int i = 0; i < lines.Length; i++)
			{
				ref StringSlice slice = ref lines[i].Slice;
				if (slice.Text == null)
				{
					break;
				}
				if (!writeEndOfLines && i > 0)
				{
					WriteLine();
				}
				ReadOnlySpan<char> content = slice.AsSpan();
				if (escape)
				{
					WriteEscape(content, softEscape);
				}
				else
				{
					Write(content);
				}
				if (writeEndOfLines)
				{
					WriteLine();
				}
			}
		}
		return this;
	}
}
