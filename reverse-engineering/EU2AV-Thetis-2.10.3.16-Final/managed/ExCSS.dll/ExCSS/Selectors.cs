using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ExCSS;

public abstract class Selectors : StylesheetNode, IEnumerable<ISelector>, IEnumerable
{
	protected readonly List<ISelector> _selectors;

	public Priority Specificity => _selectors.Aggregate(default(Priority), (Priority current, ISelector t) => current + t.Specificity);

	public string Text => this.ToCss();

	public int Length => _selectors.Count;

	public ISelector this[int index]
	{
		get
		{
			return _selectors[index];
		}
		set
		{
			_selectors[index] = value;
		}
	}

	protected Selectors()
	{
		_selectors = new List<ISelector>();
	}

	public void Add(ISelector selector)
	{
		_selectors.Add(selector);
	}

	public void Remove(ISelector selector)
	{
		_selectors.Remove(selector);
	}

	public IEnumerator<ISelector> GetEnumerator()
	{
		return _selectors.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
