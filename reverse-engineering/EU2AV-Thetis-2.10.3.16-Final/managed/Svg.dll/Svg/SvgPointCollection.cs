using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Svg;

[TypeConverter(typeof(SvgPointCollectionConverter))]
public class SvgPointCollection : List<SvgUnit>, ICloneable
{
	public object Clone()
	{
		SvgPointCollection svgPointCollection = new SvgPointCollection();
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			SvgUnit current = enumerator.Current;
			svgPointCollection.Add(current);
		}
		return svgPointCollection;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < base.Count; i += 2)
		{
			if (i + 1 < base.Count)
			{
				if (i > 1)
				{
					stringBuilder.Append(" ");
				}
				stringBuilder.Append(base[i].Value.ToSvgString());
				stringBuilder.Append(",");
				stringBuilder.Append(base[i + 1].Value.ToSvgString());
			}
		}
		return stringBuilder.ToString();
	}
}
