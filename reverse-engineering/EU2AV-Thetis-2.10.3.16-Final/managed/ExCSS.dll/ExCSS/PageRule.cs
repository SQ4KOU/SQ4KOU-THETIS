using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ExCSS;

internal sealed class PageRule : Rule, IPageRule, IRule, IStylesheetNode, IStyleFormattable
{
	public string SelectorText
	{
		get
		{
			return Selector.Text;
		}
		set
		{
			Selector = base.Parser.ParseSelector(value);
		}
	}

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

	public StyleDeclaration Style => base.Children.OfType<StyleDeclaration>().FirstOrDefault();

	public IEnumerable<MarginStyleRule> Margins => base.Children.OfType<MarginStyleRule>();

	internal PageRule(StylesheetParser parser)
		: base(RuleType.Page, parser)
	{
		AppendChild(new StyleDeclaration(this));
	}

	public override void ToCss(TextWriter writer, IStyleFormatter formatter)
	{
		writer.Write(formatter.Rule("@page", (Selector == null) ? "" : SelectorText, "{"));
		Style.ToCss(writer, formatter);
		if (Style.Any())
		{
			writer.Write("; ");
			foreach (MarginStyleRule margin in Margins)
			{
				margin.ToCss(writer, formatter);
			}
		}
		writer.Write("}");
	}
}
