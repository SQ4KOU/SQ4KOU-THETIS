using System.IO;

namespace ExCSS;

public sealed class CompoundSelector : Selectors, ISelector, IStylesheetNode, IStyleFormattable
{
	public override void ToCss(TextWriter writer, IStyleFormatter formatter)
	{
		foreach (ISelector selector in _selectors)
		{
			writer.Write(selector.Text);
		}
	}
}
