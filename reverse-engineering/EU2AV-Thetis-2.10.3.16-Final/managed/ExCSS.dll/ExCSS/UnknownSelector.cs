using System.IO;

namespace ExCSS;

public sealed class UnknownSelector : StylesheetNode, ISelector, IStylesheetNode, IStyleFormattable
{
	public Priority Specificity => Priority.Zero;

	public string Text => this.ToCss();

	public override void ToCss(TextWriter writer, IStyleFormatter formatter)
	{
		writer.Write(base.StylesheetText?.Text);
	}
}
