namespace ExCSS;

internal sealed class KeywordToken : Token
{
	public KeywordToken(TokenType type, string data, TextPosition position)
		: base(type, data, position)
	{
	}

	public override string ToValue()
	{
		return base.Type switch
		{
			TokenType.Hash => "#" + base.Data, 
			TokenType.AtKeyword => "@" + base.Data, 
			TokenType.Function => base.Data + "(", 
			_ => base.Data, 
		};
	}
}
