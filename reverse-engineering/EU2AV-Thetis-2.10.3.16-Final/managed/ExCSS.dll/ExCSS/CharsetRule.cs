using System.IO;

namespace ExCSS;

public sealed class CharsetRule : Rule, ICharsetRule, IRule, IStylesheetNode, IStyleFormattable
{
	public string CharacterSet { get; set; }

	internal CharsetRule(StylesheetParser parser)
		: base(RuleType.Charset, parser)
	{
	}

	public override void ToCss(TextWriter writer, IStyleFormatter formatter)
	{
		writer.Write(formatter.Rule("@charset", CharacterSet.StylesheetString()));
	}

	protected override void ReplaceWith(IRule rule)
	{
		CharacterSet = (rule as CharsetRule)?.CharacterSet;
		base.ReplaceWith(rule);
	}
}
