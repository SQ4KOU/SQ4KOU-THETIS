using System;
using System.Collections.Generic;
using System.Linq;

namespace Svg.Css;

internal class ExSvgElementOps : IExCssSelectorOps<SvgElement>
{
	private readonly SvgElementFactory _elementFactory;

	public ExSvgElementOps(SvgElementFactory elementFactory)
	{
		_elementFactory = elementFactory;
	}

	public Func<IEnumerable<SvgElement>, IEnumerable<SvgElement>> Type(string name)
	{
		if (_elementFactory.AvailableElementsDictionary.TryGetValue(name, out var types))
		{
			return (IEnumerable<SvgElement> nodes) => nodes.Where((SvgElement n) => types.Contains(n.GetType()));
		}
		return (IEnumerable<SvgElement> nodes) => Enumerable.Empty<SvgElement>();
	}

	public Func<IEnumerable<SvgElement>, IEnumerable<SvgElement>> Universal()
	{
		return (IEnumerable<SvgElement> nodes) => nodes;
	}

	public Func<IEnumerable<SvgElement>, IEnumerable<SvgElement>> Id(string id)
	{
		return (IEnumerable<SvgElement> nodes) => nodes.Where((SvgElement n) => n.ID == id);
	}

	public Func<IEnumerable<SvgElement>, IEnumerable<SvgElement>> Class(string clazz)
	{
		return AttributeIncludes("class", clazz);
	}

	public Func<IEnumerable<SvgElement>, IEnumerable<SvgElement>> AttributeExists(string name)
	{
		return (IEnumerable<SvgElement> nodes) => nodes.Where((SvgElement n) => n.ContainsAttribute(name));
	}

	public Func<IEnumerable<SvgElement>, IEnumerable<SvgElement>> AttributeExact(string name, string value)
	{
		return (IEnumerable<SvgElement> nodes) => nodes.Where((SvgElement n) => n.TryGetAttribute(name, out var value2) && value2 == value);
	}

	public Func<IEnumerable<SvgElement>, IEnumerable<SvgElement>> AttributeNotMatch(string name, string value)
	{
		return (IEnumerable<SvgElement> nodes) => nodes.Where((SvgElement n) => n.TryGetAttribute(name, out var value2) && value2 != value);
	}

	public Func<IEnumerable<SvgElement>, IEnumerable<SvgElement>> NthType(int step, int offset)
	{
		return (IEnumerable<SvgElement> nodes) => nodes.Where((SvgElement n) => n.Parent != null && GetByTypes(n.Parent.Children, step, offset).Contains(n));
	}

	public Func<IEnumerable<SvgElement>, IEnumerable<SvgElement>> NthLastType(int step, int offset)
	{
		return (IEnumerable<SvgElement> nodes) => nodes.Where((SvgElement n) => n.Parent != null && GetByTypes(n.Parent.Children.Reverse(), step, offset).Contains(n));
	}

	public Func<IEnumerable<SvgElement>, IEnumerable<SvgElement>> AttributeIncludes(string name, string value)
	{
		return (IEnumerable<SvgElement> nodes) => nodes.Where((SvgElement n) => n.TryGetAttribute(name, out var value2) && value2.Split(' ').Contains(value));
	}

	public Func<IEnumerable<SvgElement>, IEnumerable<SvgElement>> AttributeDashMatch(string name, string value)
	{
		if (!string.IsNullOrEmpty(value))
		{
			return (IEnumerable<SvgElement> nodes) => nodes.Where((SvgElement n) => n.TryGetAttribute(name, out var value2) && value2.Split('-').Contains(value));
		}
		return (IEnumerable<SvgElement> nodes) => Enumerable.Empty<SvgElement>();
	}

	public Func<IEnumerable<SvgElement>, IEnumerable<SvgElement>> AttributePrefixMatch(string name, string value)
	{
		if (!string.IsNullOrEmpty(value))
		{
			return (IEnumerable<SvgElement> nodes) => nodes.Where((SvgElement n) => n.TryGetAttribute(name, out var value2) && value2.StartsWith(value));
		}
		return (IEnumerable<SvgElement> nodes) => Enumerable.Empty<SvgElement>();
	}

	public Func<IEnumerable<SvgElement>, IEnumerable<SvgElement>> AttributeSuffixMatch(string name, string value)
	{
		if (!string.IsNullOrEmpty(value))
		{
			return (IEnumerable<SvgElement> nodes) => nodes.Where((SvgElement n) => n.TryGetAttribute(name, out var value2) && value2.EndsWith(value));
		}
		return (IEnumerable<SvgElement> nodes) => Enumerable.Empty<SvgElement>();
	}

	public Func<IEnumerable<SvgElement>, IEnumerable<SvgElement>> AttributeSubstring(string name, string value)
	{
		if (!string.IsNullOrEmpty(value))
		{
			return (IEnumerable<SvgElement> nodes) => nodes.Where((SvgElement n) => n.TryGetAttribute(name, out var value2) && value2.Contains(value));
		}
		return (IEnumerable<SvgElement> nodes) => Enumerable.Empty<SvgElement>();
	}

	public Func<IEnumerable<SvgElement>, IEnumerable<SvgElement>> FirstChild()
	{
		return (IEnumerable<SvgElement> nodes) => nodes.Where((SvgElement n) => n.Parent == null || n.Parent.Children.First() == n);
	}

	public Func<IEnumerable<SvgElement>, IEnumerable<SvgElement>> LastChild()
	{
		return (IEnumerable<SvgElement> nodes) => nodes.Where((SvgElement n) => n.Parent == null || n.Parent.Children.Last() == n);
	}

	private IEnumerable<T> GetByIds<T>(IList<T> items, IEnumerable<int> indices)
	{
		foreach (int index in indices)
		{
			if (index >= 0 && index < items.Count)
			{
				yield return items[index];
			}
		}
	}

	private IEnumerable<SvgElement> GetByTypes(IEnumerable<SvgElement> items, int step, int offset)
	{
		Dictionary<string, int> counter = new Dictionary<string, int>();
		foreach (SvgElement item in items)
		{
			string type = item.ElementName;
			counter.TryGetValue(type, out var count);
			if (offset == count)
			{
				yield return item;
			}
			else if (offset > count && step != 0 && (count - offset) % step == 0)
			{
				yield return item;
			}
			count = (counter[type] = count + 1);
		}
	}

	private IEnumerable<T> GetByIdsReverse<T>(IList<T> items, IEnumerable<int> indices)
	{
		foreach (int index in indices)
		{
			if (index >= 0 && index < items.Count)
			{
				yield return items[items.Count - 1 - index];
			}
		}
	}

	public Func<IEnumerable<SvgElement>, IEnumerable<SvgElement>> NthChild(int step, int offset)
	{
		return (IEnumerable<SvgElement> nodes) => nodes.Where(delegate(SvgElement n)
		{
			if (n.Parent != null)
			{
				ExSvgElementOps exSvgElementOps = this;
				SvgElementCollection children = n.Parent.Children;
				IEnumerable<int> indices;
				if (step != 0)
				{
					indices = from i in Enumerable.Range(0, n.Parent.Children.Count / step)
						select step * i + offset;
				}
				else
				{
					IEnumerable<int> enumerable = new int[1] { offset };
					indices = enumerable;
				}
				return exSvgElementOps.GetByIds(children, indices).Contains(n);
			}
			return false;
		});
	}

	public Func<IEnumerable<SvgElement>, IEnumerable<SvgElement>> OnlyChild()
	{
		return (IEnumerable<SvgElement> nodes) => nodes.Where((SvgElement n) => n.Parent == null || n.Parent.Children.Count == 1);
	}

	public Func<IEnumerable<SvgElement>, IEnumerable<SvgElement>> Empty()
	{
		return (IEnumerable<SvgElement> nodes) => nodes.Where((SvgElement n) => n.Children.Count == 0);
	}

	public Func<IEnumerable<SvgElement>, IEnumerable<SvgElement>> Child()
	{
		return (IEnumerable<SvgElement> nodes) => nodes.SelectMany((SvgElement n) => n.Children);
	}

	public Func<IEnumerable<SvgElement>, IEnumerable<SvgElement>> Descendant()
	{
		return (IEnumerable<SvgElement> nodes) => nodes.SelectMany(Descendants);
	}

	private IEnumerable<SvgElement> Descendants(SvgElement elem)
	{
		foreach (SvgElement child in elem.Children)
		{
			yield return child;
			foreach (SvgElement item in child.Descendants())
			{
				yield return item;
			}
		}
	}

	public Func<IEnumerable<SvgElement>, IEnumerable<SvgElement>> Adjacent()
	{
		return (IEnumerable<SvgElement> nodes) => nodes.SelectMany((SvgElement n) => ElementsAfterSelf(n).Take(1));
	}

	public Func<IEnumerable<SvgElement>, IEnumerable<SvgElement>> GeneralSibling()
	{
		return (IEnumerable<SvgElement> nodes) => nodes.SelectMany(ElementsAfterSelf);
	}

	private IEnumerable<SvgElement> ElementsAfterSelf(SvgElement self)
	{
		if (self.Parent != null)
		{
			return self.Parent.Children.Skip(self.Parent.Children.IndexOf(self) + 1);
		}
		return Enumerable.Empty<SvgElement>();
	}

	public Func<IEnumerable<SvgElement>, IEnumerable<SvgElement>> NthLastChild(int step, int offset)
	{
		return (IEnumerable<SvgElement> nodes) => nodes.Where(delegate(SvgElement n)
		{
			if (n.Parent != null)
			{
				ExSvgElementOps exSvgElementOps = this;
				SvgElementCollection children = n.Parent.Children;
				IEnumerable<int> indices;
				if (step != 0)
				{
					indices = from i in Enumerable.Range(0, n.Parent.Children.Count / step)
						select step * i + offset;
				}
				else
				{
					IEnumerable<int> enumerable = new int[1] { offset };
					indices = enumerable;
				}
				return exSvgElementOps.GetByIdsReverse(children, indices).Contains(n);
			}
			return false;
		});
	}

	public Func<IEnumerable<SvgElement>, IEnumerable<SvgElement>> Root()
	{
		return delegate(IEnumerable<SvgElement> nodes)
		{
			SvgElement svgElement = nodes.FirstOrDefault();
			if (svgElement == null)
			{
				return Enumerable.Empty<SvgElement>();
			}
			SvgElement svgElement2 = svgElement;
			while (svgElement2.Parent != null)
			{
				svgElement2 = svgElement2.Parent;
			}
			return new List<SvgElement> { svgElement2 };
		};
	}
}
