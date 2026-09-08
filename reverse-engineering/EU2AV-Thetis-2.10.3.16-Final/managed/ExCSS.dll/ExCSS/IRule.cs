namespace ExCSS;

public interface IRule : IStylesheetNode, IStyleFormattable
{
	RuleType Type { get; }

	string Text { get; set; }

	IRule Parent { get; }

	Stylesheet Owner { get; }
}
