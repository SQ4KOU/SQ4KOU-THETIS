using System.Globalization;
using System.Linq;

namespace ExCSS;

internal sealed class NumberToken : Token
{
	private static readonly char[] FloatIndicators = new char[3] { '.', 'e', 'E' };

	public bool IsInteger => base.Data.IndexOfAny(FloatIndicators) == -1;

	public int IntegerValue
	{
		get
		{
			if (int.TryParse(base.Data, out var result))
			{
				return result;
			}
			if (base.Data.All(char.IsDigit))
			{
				return int.MaxValue;
			}
			throw new ParseException("Unrecognized integer value '" + base.Data + ".'");
		}
	}

	public float Value => float.Parse(base.Data, CultureInfo.InvariantCulture);

	public NumberToken(string number, TextPosition position)
		: base(TokenType.Number, number, position)
	{
	}
}
