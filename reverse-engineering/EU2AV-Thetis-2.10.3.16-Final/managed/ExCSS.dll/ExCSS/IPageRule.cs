namespace ExCSS;

public interface IPageRule : IRule, IStylesheetNode, IStyleFormattable
{
	string SelectorText { get; set; }

	StyleDeclaration Style { get; }
}
