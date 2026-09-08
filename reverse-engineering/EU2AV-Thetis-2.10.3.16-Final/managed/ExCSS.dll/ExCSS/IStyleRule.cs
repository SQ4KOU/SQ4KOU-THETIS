namespace ExCSS;

public interface IStyleRule : IRule, IStylesheetNode, IStyleFormattable
{
	string SelectorText { get; set; }

	StyleDeclaration Style { get; }

	ISelector Selector { get; set; }
}
