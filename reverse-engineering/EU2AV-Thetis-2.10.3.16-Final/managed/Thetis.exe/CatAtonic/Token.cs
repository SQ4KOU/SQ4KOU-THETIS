namespace CatAtonic;

public class Token
{
	public TokenType type;

	public string text;

	public Token(TokenType type, string text)
	{
		this.type = type;
		this.text = text;
	}
}
