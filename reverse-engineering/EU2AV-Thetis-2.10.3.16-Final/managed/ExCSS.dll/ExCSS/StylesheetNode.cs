using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ExCSS;

public abstract class StylesheetNode : IStylesheetNode, IStyleFormattable
{
	private readonly List<IStylesheetNode> _children = new List<IStylesheetNode>();

	public StylesheetText StylesheetText { get; internal set; }

	public IEnumerable<IStylesheetNode> Children => _children.AsEnumerable();

	protected void ReplaceAll(IStylesheetNode node)
	{
		Clear();
		StylesheetText = node.StylesheetText;
		foreach (IStylesheetNode child in node.Children)
		{
			AppendChild(child);
		}
	}

	public abstract void ToCss(TextWriter writer, IStyleFormatter formatter);

	public void AppendChild(IStylesheetNode child)
	{
		Setup(child);
		_children.Add(child);
	}

	public void ReplaceChild(IStylesheetNode oldChild, IStylesheetNode newChild)
	{
		for (int i = 0; i < _children.Count; i++)
		{
			if (oldChild == _children[i])
			{
				Teardown(oldChild);
				Setup(newChild);
				_children[i] = newChild;
				break;
			}
		}
	}

	public void InsertBefore(IStylesheetNode referenceChild, IStylesheetNode child)
	{
		if (referenceChild != null)
		{
			int index = _children.IndexOf(referenceChild);
			InsertChild(index, child);
		}
		else
		{
			AppendChild(child);
		}
	}

	public void InsertChild(int index, IStylesheetNode child)
	{
		Setup(child);
		_children.Insert(index, child);
	}

	public void RemoveChild(IStylesheetNode child)
	{
		Teardown(child);
		_children.Remove(child);
	}

	public void Clear()
	{
		for (int num = _children.Count - 1; num >= 0; num--)
		{
			IStylesheetNode child = _children[num];
			RemoveChild(child);
		}
	}

	private void Setup(IStylesheetNode child)
	{
		if (child is Rule rule)
		{
			rule.Owner = this as Stylesheet;
			rule.Parent = this as IRule;
		}
	}

	private static void Teardown(IStylesheetNode child)
	{
		if (child is Rule rule)
		{
			rule.Parent = null;
			rule.Owner = null;
		}
	}
}
