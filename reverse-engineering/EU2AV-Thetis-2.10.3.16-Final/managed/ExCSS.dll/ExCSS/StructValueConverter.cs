using System;
using System.Collections.Generic;
using System.Globalization;

namespace ExCSS;

internal sealed class StructValueConverter<T> : IValueConverter where T : struct, IFormattable
{
	private sealed class StructValue : IPropertyValue
	{
		private readonly T _value;

		public string CssText => _value.ToString(null, CultureInfo.InvariantCulture);

		public TokenValue Original { get; }

		public StructValue(T value, IEnumerable<Token> tokens)
		{
			_value = value;
			Original = new TokenValue(tokens);
		}

		public TokenValue ExtractFor(string name)
		{
			return Original;
		}
	}

	private readonly Func<IEnumerable<Token>, T?> _converter;

	public StructValueConverter(Func<IEnumerable<Token>, T?> converter)
	{
		_converter = converter;
	}

	public IPropertyValue Convert(IEnumerable<Token> value)
	{
		T? val = _converter(value);
		if (!val.HasValue)
		{
			return null;
		}
		return new StructValue(val.Value, value);
	}

	public IPropertyValue Construct(Property[] properties)
	{
		return properties.Guard<StructValue>();
	}
}
