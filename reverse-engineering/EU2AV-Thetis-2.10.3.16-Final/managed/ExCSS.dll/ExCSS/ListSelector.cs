using System.IO;

namespace ExCSS;

public sealed class ListSelector : Selectors, ISelector, IStylesheetNode, IStyleFormattable
{
	public bool IsInvalid { get; internal set; }

	public override void ToCss(TextWriter writer, IStyleFormatter formatter)
	{
		if (_selectors.Count > 0)
		{
			writer.Write(_selectors[0].Text);
			for (int i = 1; i < _selectors.Count; i++)
			{
				writer.Write(',');
				writer.Write(_selectors[i].Text);
			}
		}
	}
}
