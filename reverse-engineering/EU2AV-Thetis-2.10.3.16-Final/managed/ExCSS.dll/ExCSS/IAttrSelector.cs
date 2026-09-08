namespace ExCSS;

public interface IAttrSelector : ISelector, IStylesheetNode, IStyleFormattable
{
	string Attribute { get; }

	string Value { get; }
}
