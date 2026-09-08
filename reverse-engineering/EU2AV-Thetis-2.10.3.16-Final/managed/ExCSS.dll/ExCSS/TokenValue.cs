using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace ExCSS;

internal sealed class TokenValue : StylesheetNode, IEnumerable<Token>, IEnumerable
{
	private readonly List<Token> _tokens;

	public static TokenValue Initial = FromString(Keywords.Initial);

	public static TokenValue Empty = new TokenValue(Enumerable.Empty<Token>());

	public Token this[int index] => _tokens[index];

	public int Count => _tokens.Count;

	public string Text => this.ToCss();

	public override void ToCss(TextWriter writer, IStyleFormatter formatter)
	{
		writer.Write(_tokens.ToText());
	}

	private TokenValue(Token token)
	{
		_tokens = new List<Token> { token };
	}

	public TokenValue(IEnumerable<Token> tokens)
	{
		_tokens = new List<Token>(tokens);
	}

	public static TokenValue FromString(string text)
	{
		return new TokenValue(new Token(TokenType.Ident, text, TextPosition.Empty));
	}

	public static TokenValue FromNumber(int number)
	{
		return new TokenValue(new NumberToken(number.ToString(CultureInfo.InvariantCulture), TextPosition.Empty));
	}

	public IEnumerator<Token> GetEnumerator()
	{
		return _tokens.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
