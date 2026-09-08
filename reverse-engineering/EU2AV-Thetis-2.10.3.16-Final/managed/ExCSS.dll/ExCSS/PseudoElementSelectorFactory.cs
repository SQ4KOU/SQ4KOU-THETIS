using System;
using System.Collections.Generic;
using System.Linq;

namespace ExCSS;

public sealed class PseudoElementSelectorFactory
{
	private static readonly Lazy<PseudoElementSelectorFactory> Lazy = new Lazy<PseudoElementSelectorFactory>(() => new PseudoElementSelectorFactory());

	private readonly StylesheetParser _parser;

	private readonly Dictionary<string, ISelector> _selectors = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
	{
		PseudoElementNames.Before,
		PseudoElementNames.After,
		PseudoElementNames.Selection,
		PseudoElementNames.FirstLine,
		PseudoElementNames.FirstLetter,
		PseudoElementNames.Content
	}.ToDictionary((string x) => x, PseudoElementSelector.Create);

	internal static PseudoElementSelectorFactory Instance => Lazy.Value;

	internal PseudoElementSelectorFactory(StylesheetParser parser = null)
	{
		_parser = parser;
	}

	public ISelector Create(string name)
	{
		if (!_selectors.TryGetValue(name, out var value))
		{
			StylesheetParser parser = _parser;
			if (parser == null || !parser.Options.AllowInvalidSelectors)
			{
				return null;
			}
			return PseudoElementSelector.Create(name);
		}
		return value;
	}
}
