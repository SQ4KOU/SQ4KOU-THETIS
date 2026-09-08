using System;
using System.Collections.Generic;
using System.Linq;

namespace Svg;

public static class Extensions
{
	public static IEnumerable<SvgElement> Descendants<T>(this IEnumerable<T> source) where T : SvgElement
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return GetDescendants(source, self: false);
	}

	private static IEnumerable<SvgElement> GetAncestors<T>(IEnumerable<T> source, bool self) where T : SvgElement
	{
		foreach (T item in source)
		{
			if (item != null)
			{
				for (SvgElement elem = (self ? item : item.Parent); elem != null; elem = elem.Parent)
				{
					yield return elem;
				}
			}
		}
	}

	private static IEnumerable<SvgElement> GetDescendants<T>(IEnumerable<T> source, bool self) where T : SvgElement
	{
		foreach (T top in source)
		{
			if (top == null)
			{
				continue;
			}
			if (self)
			{
				yield return top;
			}
			Stack<SvgElement> elements = new Stack<SvgElement>(top.Children.Reverse());
			while (elements.Count > 0)
			{
				SvgElement element = elements.Pop();
				yield return element;
				foreach (SvgElement item in element.Children.Reverse())
				{
					elements.Push(item);
				}
			}
		}
	}
}
