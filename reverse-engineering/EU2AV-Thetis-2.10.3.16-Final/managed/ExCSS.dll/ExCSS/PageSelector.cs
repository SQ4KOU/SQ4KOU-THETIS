using System.IO;

namespace ExCSS;

public sealed class PageSelector : StylesheetNode, ISelector, IStylesheetNode, IStyleFormattable
{
	private readonly string _name;

	public Priority Specificity => Priority.Inline;

	public string Text => this.ToCss();

	public PageSelector(string name)
	{
		_name = name;
	}

	public PageSelector()
		: this(string.Empty)
	{
	}

	public override void ToCss(TextWriter writer, IStyleFormatter formatter)
	{
		string text = ((_name == string.Empty) ? "" : ":");
		writer.Write(text + _name);
	}
}
