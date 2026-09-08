using System.Globalization;

namespace ExCSS;

internal sealed class UnitToken : Token
{
	public float Value => float.Parse(base.Data, CultureInfo.InvariantCulture);

	public string Unit { get; }

	public UnitToken(TokenType type, string value, string dimension, TextPosition position)
		: base(type, value, position)
	{
		Unit = dimension;
	}

	public override string ToValue()
	{
		return base.Data + Unit;
	}
}
