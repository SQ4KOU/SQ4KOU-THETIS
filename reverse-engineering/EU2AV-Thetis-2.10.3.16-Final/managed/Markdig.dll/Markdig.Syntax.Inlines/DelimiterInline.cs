using System.Diagnostics;
using Markdig.Helpers;
using Markdig.Parsers;

namespace Markdig.Syntax.Inlines;

[DebuggerDisplay("{ToLiteral()} {Type}")]
public abstract class DelimiterInline : ContainerInline
{
	public InlineParser Parser { get; }

	public DelimiterType Type { get; set; }

	public bool IsActive { get; set; }

	protected DelimiterInline(InlineParser parser)
	{
		if (parser == null)
		{
			ThrowHelper.ArgumentNullException("parser");
		}
		Parser = parser;
		IsActive = true;
	}

	public abstract string ToLiteral();

	public void ReplaceByLiteral()
	{
		LiteralInline inline = new LiteralInline
		{
			Content = new StringSlice(ToLiteral()),
			Span = Span,
			Line = base.Line,
			Column = base.Column,
			IsClosed = true
		};
		ReplaceBy(inline);
	}
}
