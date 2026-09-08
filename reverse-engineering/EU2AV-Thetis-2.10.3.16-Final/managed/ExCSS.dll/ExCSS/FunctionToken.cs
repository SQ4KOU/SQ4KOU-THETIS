using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ExCSS;

internal sealed class FunctionToken : Token, IEnumerable<Token>, IEnumerable
{
	private readonly List<Token> _arguments;

	public IEnumerable<Token> ArgumentTokens
	{
		get
		{
			int num = _arguments.Count - 1;
			if (num >= 0 && _arguments[num].Type == TokenType.RoundBracketClose)
			{
				num--;
			}
			return _arguments.Take(1 + num);
		}
	}

	public FunctionToken(string data, TextPosition position)
		: base(TokenType.Function, data, position)
	{
		_arguments = new List<Token>();
	}

	public override string ToValue()
	{
		return base.Data + "(" + _arguments.ToText();
	}

	public void AddArgumentToken(Token token)
	{
		_arguments.Add(token);
	}

	public IEnumerator<Token> GetEnumerator()
	{
		return _arguments.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
