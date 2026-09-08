namespace ExCSS;

internal sealed class StringToken : Token
{
	public bool IsValid { get; }

	public char Quote { get; }

	public StringToken(string data, bool valid, char quote, TextPosition position)
		: base(TokenType.String, data, position)
	{
		IsValid = valid;
		Quote = quote;
	}

	public override string ToValue()
	{
		return base.Data.StylesheetString();
	}
}
