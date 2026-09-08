namespace ExCSS;

internal sealed class CommentToken : Token
{
	public bool IsValid { get; }

	public CommentToken(string data, bool valid, TextPosition position)
		: base(TokenType.Comment, data, position)
	{
		IsValid = valid;
	}

	public override string ToValue()
	{
		string text = (IsValid ? string.Empty : "*/");
		return "/*" + base.Data + text;
	}
}
