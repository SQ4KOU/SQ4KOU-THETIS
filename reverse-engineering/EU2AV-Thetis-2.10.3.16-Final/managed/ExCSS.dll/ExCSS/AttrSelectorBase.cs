namespace ExCSS;

public abstract class AttrSelectorBase : SelectorBase, IAttrSelector, ISelector, IStylesheetNode, IStyleFormattable
{
	public string Attribute { get; }

	public string Value { get; }

	protected AttrSelectorBase(string attribute, string value, string text)
		: base(Priority.OneClass, text)
	{
		Attribute = attribute;
		Value = value;
	}
}
