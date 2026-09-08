using System.Diagnostics;
using Markdig.Helpers;

namespace Markdig.Syntax.Inlines;

[DebuggerDisplay("{Content}")]
public class LiteralInline : LeafInline
{
	public StringSlice Content;

	public bool IsFirstCharacterEscaped
	{
		get
		{
			return base.InternalSpareBit;
		}
		set
		{
			base.InternalSpareBit = value;
		}
	}

	public LiteralInline()
	{
		Content = new StringSlice(null);
	}

	public LiteralInline(StringSlice content)
	{
		Content = content;
	}

	public LiteralInline(string text)
	{
		if (text == null)
		{
			ThrowHelper.ArgumentNullException_text();
		}
		Content = new StringSlice(text);
	}

	public override string ToString()
	{
		return Content.ToString();
	}
}
