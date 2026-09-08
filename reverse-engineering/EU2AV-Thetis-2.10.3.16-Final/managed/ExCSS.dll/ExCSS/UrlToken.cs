namespace ExCSS;

internal sealed class UrlToken : Token
{
	public bool IsValid { get; }

	public string FunctionName { get; }

	public UrlToken(string functionName, string data, bool valid, TextPosition position)
		: base(TokenType.Url, data, position)
	{
		IsValid = valid;
		FunctionName = functionName;
	}

	public override string ToValue()
	{
		string argument = base.Data.StylesheetString();
		return FunctionName.StylesheetFunction(argument);
	}
}
