using System;
using System.Collections.Generic;

namespace ExCSS;

internal sealed class ContentProperty : Property
{
	private abstract class ContentMode
	{
	}

	private sealed class NormalContentMode : ContentMode
	{
	}

	private sealed class OpenQuoteContentMode : ContentMode
	{
	}

	private sealed class CloseQuoteContentMode : ContentMode
	{
	}

	private sealed class NoOpenQuoteContentMode : ContentMode
	{
	}

	private sealed class NoCloseQuoteContentMode : ContentMode
	{
	}

	private static readonly Dictionary<string, ContentMode> ContentModes = new Dictionary<string, ContentMode>(StringComparer.OrdinalIgnoreCase)
	{
		{
			Keywords.OpenQuote,
			new OpenQuoteContentMode()
		},
		{
			Keywords.NoOpenQuote,
			new NoOpenQuoteContentMode()
		},
		{
			Keywords.CloseQuote,
			new CloseQuoteContentMode()
		},
		{
			Keywords.NoCloseQuote,
			new NoCloseQuoteContentMode()
		}
	};

	private static readonly ContentMode[] Default = new ContentMode[1]
	{
		new NormalContentMode()
	};

	private static readonly IValueConverter StyleConverter = Converters.Assign(Keywords.Normal, Default).OrNone().Or(ContentModes.ToConverter().Or(Converters.UrlConverter).Or(Converters.StringConverter)
		.Or(Converters.AttrConverter)
		.Or(Converters.CounterConverter)
		.Many())
		.OrDefault();

	internal override IValueConverter Converter => StyleConverter;

	internal ContentProperty()
		: base(PropertyNames.Content)
	{
	}
}
