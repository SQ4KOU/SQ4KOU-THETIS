using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace ExCSS;

public sealed class ComplexSelector : StylesheetNode, ISelector, IStylesheetNode, IStyleFormattable, IEnumerable<CombinatorSelector>, IEnumerable
{
	private readonly List<CombinatorSelector> _selectors;

	public string Text => this.ToCss();

	public int Length => _selectors.Count;

	public bool IsReady { get; private set; }

	public Priority Specificity
	{
		get
		{
			Priority result = default(Priority);
			int count = _selectors.Count;
			for (int i = 0; i < count; i++)
			{
				result += _selectors[i].Selector.Specificity;
			}
			return result;
		}
	}

	public ComplexSelector()
	{
		_selectors = new List<CombinatorSelector>();
	}

	public override void ToCss(TextWriter writer, IStyleFormatter formatter)
	{
		if (_selectors.Count > 0)
		{
			int num = _selectors.Count - 1;
			for (int i = 0; i < num; i++)
			{
				writer.Write(_selectors[i].Selector.Text);
				writer.Write(_selectors[i].Delimiter);
			}
			writer.Write(_selectors[num].Selector.Text);
		}
	}

	public void ConcludeSelector(ISelector selector)
	{
		if (!IsReady)
		{
			_selectors.Add(new CombinatorSelector
			{
				Selector = selector,
				Delimiter = null
			});
			IsReady = true;
		}
	}

	public void AppendSelector(ISelector selector, Combinator combinator)
	{
		if (!IsReady)
		{
			_selectors.Add(new CombinatorSelector
			{
				Selector = combinator.Change(selector),
				Delimiter = combinator.Delimiter
			});
		}
	}

	public IEnumerator<CombinatorSelector> GetEnumerator()
	{
		return _selectors.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
