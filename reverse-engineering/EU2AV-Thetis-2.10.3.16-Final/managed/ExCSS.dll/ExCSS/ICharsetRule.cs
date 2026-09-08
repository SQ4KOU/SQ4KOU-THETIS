namespace ExCSS;

public interface ICharsetRule : IRule, IStylesheetNode, IStyleFormattable
{
	string CharacterSet { get; set; }
}
