namespace ExCSS;

public interface IKeyframeRule : IRule, IStylesheetNode, IStyleFormattable
{
	string KeyText { get; set; }

	StyleDeclaration Style { get; }

	KeyframeSelector Key { get; set; }
}
