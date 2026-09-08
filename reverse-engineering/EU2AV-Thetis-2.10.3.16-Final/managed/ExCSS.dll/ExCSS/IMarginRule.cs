namespace ExCSS;

public interface IMarginRule : IRule, IStylesheetNode, IStyleFormattable
{
	string Name { get; }

	StyleDeclaration Style { get; }
}
