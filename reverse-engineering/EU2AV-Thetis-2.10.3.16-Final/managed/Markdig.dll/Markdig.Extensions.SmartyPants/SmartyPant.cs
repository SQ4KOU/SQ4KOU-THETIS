using System.Diagnostics;
using Markdig.Syntax.Inlines;

namespace Markdig.Extensions.SmartyPants;

[DebuggerDisplay("SmartyPant {ToString()}")]
public class SmartyPant : LeafInline
{
	public char OpeningCharacter { get; set; }

	public SmartyPantType Type { get; set; }

	public override string ToString()
	{
		switch (Type)
		{
		case SmartyPantType.Quote:
		case SmartyPantType.LeftQuote:
		case SmartyPantType.RightQuote:
			return "'";
		case SmartyPantType.DoubleQuote:
			return "\"";
		case SmartyPantType.LeftDoubleQuote:
			if (OpeningCharacter != '`')
			{
				return "\"";
			}
			return "``";
		case SmartyPantType.RightDoubleQuote:
			if (OpeningCharacter != '\'')
			{
				return "\"";
			}
			return "''";
		case SmartyPantType.Dash2:
			return "--";
		case SmartyPantType.Dash3:
			return "--";
		case SmartyPantType.LeftAngleQuote:
			return "<<";
		case SmartyPantType.RightAngleQuote:
			return ">>";
		default:
			if (OpeningCharacter == '\0')
			{
				return string.Empty;
			}
			return OpeningCharacter.ToString();
		}
	}

	public LiteralInline AsLiteralInline()
	{
		return new LiteralInline(ToString())
		{
			Span = Span,
			Line = base.Line,
			Column = base.Column
		};
	}
}
