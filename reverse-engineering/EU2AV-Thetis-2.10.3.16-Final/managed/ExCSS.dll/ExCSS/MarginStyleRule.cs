using System.IO;
using System.Linq;

namespace ExCSS;

public sealed class MarginStyleRule : Rule, IStyleRule, IRule, IStylesheetNode, IStyleFormattable
{
	public ISelector Selector
	{
		get
		{
			return base.Children.OfType<ISelector>().FirstOrDefault();
		}
		set
		{
			ReplaceSingle(Selector, value);
		}
	}

	public string SelectorText
	{
		get
		{
			return "@" + Selector.Text;
		}
		set
		{
			Selector = base.Parser.ParseSelector(value);
		}
	}

	public StyleDeclaration Style => base.Children.OfType<StyleDeclaration>().FirstOrDefault();

	public MarginStyleRule(StylesheetParser parser)
		: base(RuleType.Style, parser)
	{
		AppendChild(new StyleDeclaration(this));
	}

	public override void ToCss(TextWriter writer, IStyleFormatter formatter)
	{
		writer.Write(formatter.Style(SelectorText, Style));
	}
}
