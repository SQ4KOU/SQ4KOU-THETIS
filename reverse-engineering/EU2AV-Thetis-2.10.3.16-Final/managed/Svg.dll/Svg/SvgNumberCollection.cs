using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Svg;

[TypeConverter(typeof(SvgNumberCollectionConverter))]
public class SvgNumberCollection : List<float>, ICloneable
{
	public object Clone()
	{
		SvgNumberCollection svgNumberCollection = new SvgNumberCollection();
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			float current = enumerator.Current;
			svgNumberCollection.Add(current);
		}
		return svgNumberCollection;
	}

	public override string ToString()
	{
		return string.Join(" ", this.Select((float v) => v.ToSvgString()).ToArray());
	}
}
