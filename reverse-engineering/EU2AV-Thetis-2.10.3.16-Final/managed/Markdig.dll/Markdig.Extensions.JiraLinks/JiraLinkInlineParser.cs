using System;
using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace Markdig.Extensions.JiraLinks;

public class JiraLinkInlineParser : InlineParser
{
	private readonly JiraLinkOptions _options;

	private readonly string _baseUrl;

	public JiraLinkInlineParser(JiraLinkOptions options)
	{
		_options = options ?? throw new ArgumentNullException("options");
		_baseUrl = _options.GetUrl();
		base.OpeningCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
	}

	public override bool Match(InlineProcessor processor, ref StringSlice slice)
	{
		char c = slice.PeekCharExtra(-1);
		if (!c.IsWhiteSpaceOrZero() && c != '(')
		{
			return false;
		}
		char c2 = slice.CurrentChar;
		int start = slice.Start;
		int start2 = slice.Start;
		if (c2.IsDigit())
		{
			return false;
		}
		while (c2.IsAlphaUpper() || c2.IsDigit())
		{
			start2 = slice.Start;
			c2 = slice.NextChar();
		}
		if (!c2.Equals('-'))
		{
			return false;
		}
		c2 = slice.NextChar();
		if (!c2.IsDigit())
		{
			return false;
		}
		int start3 = slice.Start;
		int start4 = slice.Start;
		while (c2.IsDigit())
		{
			start4 = slice.Start;
			c2 = slice.NextChar();
		}
		if (!c2.IsWhiteSpaceOrZero() && c2 != ')')
		{
			return false;
		}
		int sourcePosition = processor.GetSourcePosition(start, out var lineIndex, out var column);
		JiraLink jiraLink = new JiraLink
		{
			Span = new SourceSpan(sourcePosition, sourcePosition + (start4 - start)),
			Line = lineIndex,
			Column = column,
			Issue = new StringSlice(slice.Text, start3, start4),
			ProjectKey = new StringSlice(slice.Text, start, start2)
		};
		Span<char> initialBuffer = stackalloc char[64];
		ValueStringBuilder valueStringBuilder = new ValueStringBuilder(initialBuffer);
		valueStringBuilder.Append(_baseUrl);
		valueStringBuilder.Append('/');
		valueStringBuilder.Append(jiraLink.ProjectKey.AsSpan());
		valueStringBuilder.Append('-');
		valueStringBuilder.Append(jiraLink.Issue.AsSpan());
		jiraLink.Url = valueStringBuilder.AsSpan().ToString();
		valueStringBuilder.Length = 0;
		valueStringBuilder.Append(jiraLink.ProjectKey.AsSpan());
		valueStringBuilder.Append('-');
		valueStringBuilder.Append(jiraLink.Issue.AsSpan());
		jiraLink.AppendChild(new LiteralInline(valueStringBuilder.ToString())
		{
			Span = jiraLink.Span,
			Line = lineIndex,
			Column = column
		});
		if (_options.OpenInNewWindow)
		{
			jiraLink.GetAttributes().AddProperty("target", "_blank");
		}
		processor.Inline = jiraLink;
		return true;
	}
}
