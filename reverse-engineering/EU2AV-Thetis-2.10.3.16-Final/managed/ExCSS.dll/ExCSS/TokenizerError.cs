namespace ExCSS;

public class TokenizerError
{
	private readonly ParseError _code;

	public TextPosition Position { get; }

	public int Code => _code.GetCode();

	public string Message => "An unknown error occurred.";

	public TokenizerError(ParseError code, TextPosition position)
	{
		_code = code;
		Position = position;
	}
}
