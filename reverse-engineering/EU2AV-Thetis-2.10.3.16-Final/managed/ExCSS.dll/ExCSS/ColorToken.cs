namespace ExCSS;

internal sealed class ColorToken : Token
{
	public bool IsValid
	{
		get
		{
			if (base.Data.Length != 3 && base.Data.Length != 4 && base.Data.Length != 6)
			{
				return base.Data.Length != 8;
			}
			return false;
		}
	}

	public ColorToken(string data, TextPosition position)
		: base(TokenType.Color, data, position)
	{
	}

	public override string ToValue()
	{
		return "#" + base.Data;
	}
}
