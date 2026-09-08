using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace Svg;

[TypeConverter(typeof(SvgUnitCollectionConverter))]
public class SvgUnitCollection : ObservableCollection<SvgUnit>, ICloneable
{
	public const string None = "none";

	public const string Inherit = "inherit";

	public string StringForEmptyValue { get; set; }

	public void AddRange(IEnumerable<SvgUnit> collection)
	{
		if (collection == null)
		{
			throw new ArgumentNullException("collection");
		}
		if (collection == this)
		{
			SvgUnitCollection svgUnitCollection = new SvgUnitCollection();
			foreach (SvgUnit item in collection)
			{
				svgUnitCollection.Add(item);
			}
			collection = svgUnitCollection;
		}
		foreach (SvgUnit item2 in collection)
		{
			Add(item2);
		}
	}

	public override string ToString()
	{
		if (base.Count <= 0 && !string.IsNullOrEmpty(StringForEmptyValue))
		{
			return StringForEmptyValue;
		}
		return string.Join(" ", this.Select((SvgUnit u) => u.ToString()));
	}

	public static bool IsNullOrEmpty(SvgUnitCollection collection)
	{
		if (collection != null && collection.Count >= 1)
		{
			if (collection.Count == 1)
			{
				if (!(collection[0] == SvgUnit.Empty))
				{
					return collection[0] == SvgUnit.None;
				}
				return true;
			}
			return false;
		}
		return true;
	}

	public object Clone()
	{
		SvgUnitCollection svgUnitCollection = new SvgUnitCollection();
		svgUnitCollection.StringForEmptyValue = StringForEmptyValue;
		svgUnitCollection.AddRange(this);
		return svgUnitCollection;
	}
}
