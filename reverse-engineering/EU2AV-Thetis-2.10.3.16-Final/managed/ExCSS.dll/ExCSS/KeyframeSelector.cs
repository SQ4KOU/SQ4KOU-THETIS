using System.Collections.Generic;
using System.IO;

namespace ExCSS;

public sealed class KeyframeSelector : StylesheetNode
{
	private readonly List<Percent> _stops;

	public IEnumerable<Percent> Stops => _stops;

	public string Text => this.ToCss();

	public KeyframeSelector(IEnumerable<Percent> stops)
	{
		_stops = new List<Percent>(stops);
	}

	public override void ToCss(TextWriter writer, IStyleFormatter formatter)
	{
		if (_stops.Count > 0)
		{
			writer.Write(_stops[0].ToString());
			for (int i = 1; i < _stops.Count; i++)
			{
				writer.Write(", ");
				writer.Write(_stops[i].ToString());
			}
		}
	}
}
