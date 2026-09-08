namespace ExCSS;

public interface INamespaceRule : IRule, IStylesheetNode, IStyleFormattable
{
	string NamespaceUri { get; set; }

	string Prefix { get; set; }
}
