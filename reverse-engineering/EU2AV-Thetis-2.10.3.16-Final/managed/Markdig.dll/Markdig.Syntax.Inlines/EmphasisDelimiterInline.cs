using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Parsers.Inlines;

namespace Markdig.Syntax.Inlines;

public class EmphasisDelimiterInline : DelimiterInline
{
	public StringSlice Content;

	public EmphasisDescriptor Descriptor { get; }

	public char DelimiterChar { get; }

	public int DelimiterCount { get; set; }

	public EmphasisDelimiterInline(InlineParser parser, EmphasisDescriptor descriptor)
		: base(parser)
	{
		if (descriptor == null)
		{
			ThrowHelper.ArgumentNullException("descriptor");
		}
		Descriptor = descriptor;
		DelimiterChar = descriptor.Character;
		Content = new StringSlice(ToLiteral());
	}

	internal EmphasisDelimiterInline(InlineParser parser, EmphasisDescriptor descriptor, StringSlice content)
		: base(parser)
	{
		if (descriptor == null)
		{
			ThrowHelper.ArgumentNullException("descriptor");
		}
		Descriptor = descriptor;
		DelimiterChar = descriptor.Character;
		Content = content;
	}

	public override string ToLiteral()
	{
		if (DelimiterCount == 1)
		{
			return DelimiterChar switch
			{
				'*' => "*", 
				'_' => "_", 
				'~' => "~", 
				'^' => "^", 
				'+' => "+", 
				'=' => "=", 
				_ => DelimiterChar.ToString(), 
			};
		}
		return new string(DelimiterChar, DelimiterCount);
	}

	public LiteralInline AsLiteralInline()
	{
		return new LiteralInline
		{
			Content = Content,
			IsClosed = true,
			Span = Span,
			Line = base.Line,
			Column = base.Column
		};
	}
}
