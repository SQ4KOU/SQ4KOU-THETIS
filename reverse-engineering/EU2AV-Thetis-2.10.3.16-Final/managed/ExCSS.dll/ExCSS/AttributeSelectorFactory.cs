using System;

namespace ExCSS;

public sealed class AttributeSelectorFactory
{
	private static readonly Lazy<AttributeSelectorFactory> Lazy = new Lazy<AttributeSelectorFactory>(() => new AttributeSelectorFactory());

	internal static AttributeSelectorFactory Instance => Lazy.Value;

	private AttributeSelectorFactory()
	{
	}

	public IAttrSelector Create(string combinator, string match, string value, string prefix)
	{
		string attribute = match;
		if (!string.IsNullOrEmpty(prefix))
		{
			attribute = FormFront(prefix, match);
			FormMatch(prefix, match);
		}
		if (combinator == Combinators.Exactly)
		{
			return new AttrMatchSelector(attribute, value);
		}
		if (combinator == Combinators.InList)
		{
			return new AttrListSelector(attribute, value);
		}
		if (combinator == Combinators.InToken)
		{
			return new AttrHyphenSelector(attribute, value);
		}
		if (combinator == Combinators.Begins)
		{
			return new AttrBeginsSelector(attribute, value);
		}
		if (combinator == Combinators.Ends)
		{
			return new AttrEndsSelector(attribute, value);
		}
		if (combinator == Combinators.InText)
		{
			return new AttrContainsSelector(attribute, value);
		}
		if (combinator == Combinators.Unlike)
		{
			return new AttrNotMatchSelector(attribute, value);
		}
		return new AttrAvailableSelector(attribute, value);
	}

	private static string FormFront(string prefix, string match)
	{
		return prefix + Combinators.Pipe + match;
	}

	private static string FormMatch(string prefix, string match)
	{
		if (!prefix.Is(Keywords.Asterisk))
		{
			return prefix + PseudoClassNames.Separator + match;
		}
		return match;
	}
}
