using System;
using System.Globalization;

namespace ExCSS;

internal sealed class Lexer : LexerBase
{
	private TextPosition _position;

	public bool IsInValue { get; set; }

	public event EventHandler<TokenizerError> Error;

	public Lexer(TextSource source)
		: base(source)
	{
		IsInValue = false;
	}

	public Token Get()
	{
		char next = GetNext();
		_position = GetCurrentPosition();
		return Data(next);
	}

	internal void RaiseErrorOccurred(ParseError error, TextPosition position)
	{
		EventHandler<TokenizerError> eventHandler = Error;
		if (eventHandler != null)
		{
			TokenizerError e = new TokenizerError(error, position);
			eventHandler(this, e);
		}
	}

	private Token Data(char current)
	{
		_position = GetCurrentPosition();
		switch (current)
		{
		case '\t':
		case '\n':
		case '\f':
		case '\r':
		case ' ':
			return NewWhitespace(current);
		case '"':
			return StringDoubleQuote();
		case '#':
			if (!IsInValue)
			{
				return HashStart();
			}
			return ColorLiteral();
		case '$':
			current = GetNext();
			if (current != '=')
			{
				return NewDelimiter(GetPrevious());
			}
			return NewMatch(Combinators.Ends);
		case '\'':
			return StringSingleQuote();
		case '(':
			return NewOpenRound();
		case ')':
			return NewCloseRound();
		case '*':
			current = GetNext();
			if (current != '=')
			{
				return NewDelimiter(GetPrevious());
			}
			return NewMatch(Combinators.InText);
		case '+':
		{
			char next = GetNext();
			if (next != '\uffff')
			{
				char next2 = GetNext();
				Back(2);
				if (next.IsDigit() || (next == '.' && next2.IsDigit()))
				{
					return NumberStart(current);
				}
			}
			else
			{
				Back();
			}
			return NewDelimiter(current);
		}
		case ',':
			return NewComma();
		case '.':
			if (!GetNext().IsDigit())
			{
				return NewDelimiter(GetPrevious());
			}
			return NumberStart(GetPrevious());
		case '-':
		{
			char next3 = GetNext();
			if (next3 != '\uffff')
			{
				char next4 = GetNext();
				Back(2);
				if (next3.IsDigit() || (next3 == '.' && next4.IsDigit()))
				{
					return NumberStart(current);
				}
				if (next3.IsNameStart())
				{
					return IdentStart(current);
				}
				if (next3 == '\\' && !next4.IsLineBreak() && next4 != '\uffff')
				{
					return IdentStart(current);
				}
				if (next3 != '-' || next4 != '>')
				{
					return NewDelimiter(current);
				}
				Advance(2);
				return NewCloseComment();
			}
			Back();
			return NewDelimiter(current);
		}
		case '/':
			current = GetNext();
			if (current != '*')
			{
				return NewDelimiter(GetPrevious());
			}
			return Comment();
		case '\\':
			current = GetNext();
			if (current.IsLineBreak())
			{
				RaiseErrorOccurred(ParseError.LineBreakUnexpected);
				return NewDelimiter(GetPrevious());
			}
			if (current != '\uffff')
			{
				return IdentStart(GetPrevious());
			}
			RaiseErrorOccurred(ParseError.EOF);
			return NewDelimiter(GetPrevious());
		case ':':
			return NewColon();
		case ';':
			return NewSemicolon();
		case '<':
			current = GetNext();
			switch (current)
			{
			case '!':
				current = GetNext();
				if (current == '-')
				{
					current = GetNext();
					if (current == '-')
					{
						return NewOpenComment();
					}
					current = GetPrevious();
				}
				GetPrevious();
				return NewDelimiter(GetPrevious());
			case '=':
				Advance();
				return NewLessThanOrEqual();
			default:
				return NewLessThan();
			}
		case '@':
			return AtKeywordStart();
		case '[':
			return NewOpenSquare();
		case ']':
			return NewCloseSquare();
		case '^':
			current = GetNext();
			if (current != '=')
			{
				return NewDelimiter(GetPrevious());
			}
			return NewMatch(Combinators.Begins);
		case '{':
			return NewOpenCurly();
		case '}':
			return NewCloseCurly();
		case '0':
		case '1':
		case '2':
		case '3':
		case '4':
		case '5':
		case '6':
		case '7':
		case '8':
		case '9':
			return NumberStart(current);
		case 'U':
		case 'u':
			current = GetNext();
			if (current != '+')
			{
				return IdentStart(GetPrevious());
			}
			current = GetNext();
			if (current.IsHex() || current == '?')
			{
				return UnicodeRange(current);
			}
			current = GetPrevious();
			return IdentStart(GetPrevious());
		case '|':
			current = GetNext();
			return current switch
			{
				'=' => NewMatch(Combinators.InToken), 
				'|' => NewColumn(), 
				_ => NewDelimiter(GetPrevious()), 
			};
		case '~':
			current = GetNext();
			if (current != '=')
			{
				return NewDelimiter(GetPrevious());
			}
			return NewMatch(Combinators.InList);
		case '\uffff':
			return NewEof();
		case '!':
			current = GetNext();
			if (current != '=')
			{
				return NewDelimiter(GetPrevious());
			}
			return NewMatch(Combinators.Unlike);
		case '>':
			if (GetNext() == '=')
			{
				Advance();
				return NewGreaterThanOrEqual();
			}
			GetPrevious();
			return NewGreaterThan();
		default:
			if (!current.IsNameStart())
			{
				return NewDelimiter(current);
			}
			return IdentStart(current);
		}
	}

	private Token StringDoubleQuote()
	{
		while (true)
		{
			char next = GetNext();
			switch (next)
			{
			case '"':
			case '\uffff':
				return NewString(FlushBuffer(), '"');
			case '\n':
			case '\f':
				RaiseErrorOccurred(ParseError.LineBreakUnexpected);
				Back();
				return NewString(FlushBuffer(), '"', bad: true);
			case '\\':
				next = GetNext();
				if (next.IsLineBreak())
				{
					base.StringBuffer.AppendLine();
					break;
				}
				if (next != '\uffff')
				{
					base.StringBuffer.Append(ConsumeEscape(next));
					break;
				}
				RaiseErrorOccurred(ParseError.EOF);
				Back();
				return NewString(FlushBuffer(), '"', bad: true);
			default:
				base.StringBuffer.Append(next);
				break;
			}
		}
	}

	private Token StringSingleQuote()
	{
		while (true)
		{
			char next = GetNext();
			switch (next)
			{
			case '\'':
			case '\uffff':
				return NewString(FlushBuffer(), '\'');
			case '\n':
			case '\f':
				RaiseErrorOccurred(ParseError.LineBreakUnexpected);
				Back();
				return NewString(FlushBuffer(), '\'', bad: true);
			case '\\':
				next = GetNext();
				if (next.IsLineBreak())
				{
					base.StringBuffer.AppendLine();
					break;
				}
				if (next != '\uffff')
				{
					base.StringBuffer.Append(ConsumeEscape(next));
					break;
				}
				RaiseErrorOccurred(ParseError.EOF);
				Back();
				return NewString(FlushBuffer(), '\'', bad: true);
			default:
				base.StringBuffer.Append(next);
				break;
			}
		}
	}

	private Token ColorLiteral()
	{
		char next = GetNext();
		while (next.IsHex())
		{
			base.StringBuffer.Append(next);
			next = GetNext();
		}
		Back();
		return NewColor(FlushBuffer());
	}

	private Token HashStart()
	{
		char next = GetNext();
		if (next.IsNameStart())
		{
			base.StringBuffer.Append(next);
			return HashRest();
		}
		if (IsValidEscape(next))
		{
			next = GetNext();
			base.StringBuffer.Append(ConsumeEscape(next));
			return HashRest();
		}
		if (next == '\\')
		{
			RaiseErrorOccurred(ParseError.InvalidCharacter);
			Back();
			return NewDelimiter('#');
		}
		Back();
		return NewDelimiter('#');
	}

	private Token HashRest()
	{
		char next;
		while (true)
		{
			next = GetNext();
			if (next.IsName())
			{
				base.StringBuffer.Append(next);
				continue;
			}
			if (!IsValidEscape(next))
			{
				break;
			}
			next = GetNext();
			base.StringBuffer.Append(ConsumeEscape(next));
		}
		if (next == '\\')
		{
			RaiseErrorOccurred(ParseError.InvalidCharacter);
			Back();
			return NewHash(FlushBuffer());
		}
		Back();
		return NewHash(FlushBuffer());
	}

	private Token Comment()
	{
		char next = GetNext();
		while (true)
		{
			switch (next)
			{
			case '*':
				next = GetNext();
				if (next == '/')
				{
					return NewComment(FlushBuffer());
				}
				base.StringBuffer.Append('*');
				break;
			default:
				base.StringBuffer.Append(next);
				next = GetNext();
				break;
			case '\uffff':
				RaiseErrorOccurred(ParseError.EOF);
				return NewComment(FlushBuffer(), bad: true);
			}
		}
	}

	private Token AtKeywordStart()
	{
		char next = GetNext();
		if (next == '-')
		{
			next = GetNext();
			if (next.IsNameStart() || IsValidEscape(next))
			{
				base.StringBuffer.Append('-');
				return AtKeywordRest(next);
			}
			Back(2);
			return NewDelimiter('@');
		}
		if (next.IsNameStart())
		{
			base.StringBuffer.Append(next);
			return AtKeywordRest(GetNext());
		}
		if (IsValidEscape(next))
		{
			next = GetNext();
			base.StringBuffer.Append(ConsumeEscape(next));
			return AtKeywordRest(GetNext());
		}
		Back();
		return NewDelimiter('@');
	}

	private Token AtKeywordRest(char current)
	{
		while (true)
		{
			if (current.IsName())
			{
				base.StringBuffer.Append(current);
			}
			else
			{
				if (!IsValidEscape(current))
				{
					break;
				}
				current = GetNext();
				base.StringBuffer.Append(ConsumeEscape(current));
			}
			current = GetNext();
		}
		Back();
		return NewAtKeyword(FlushBuffer());
	}

	private Token IdentStart(char current)
	{
		if (current == '-')
		{
			current = GetNext();
			if (current.IsNameStart() || IsValidEscape(current))
			{
				base.StringBuffer.Append('-');
				return IdentRest(current);
			}
			Back();
			return NewDelimiter('-');
		}
		if (current.IsNameStart())
		{
			base.StringBuffer.Append(current);
			return IdentRest(GetNext());
		}
		if (current == '\\' && IsValidEscape(current))
		{
			current = GetNext();
			base.StringBuffer.Append(ConsumeEscape(current));
			return IdentRest(GetNext());
		}
		return Data(current);
	}

	private Token IdentRest(char current)
	{
		while (true)
		{
			if (current.IsName())
			{
				base.StringBuffer.Append(current);
			}
			else
			{
				if (!IsValidEscape(current))
				{
					break;
				}
				current = GetNext();
				base.StringBuffer.Append(ConsumeEscape(current));
			}
			current = GetNext();
		}
		if (current == '(')
		{
			string text = FlushBuffer();
			if (text.GetTypeFromName() != TokenType.Function)
			{
				return UrlStart(text);
			}
			return NewFunction(text);
		}
		Back();
		return NewIdent(FlushBuffer());
	}

	private Token NumberStart(char current)
	{
		while (true)
		{
			if (current.IsOneOf('+', '-'))
			{
				base.StringBuffer.Append(current);
				current = GetNext();
				if (current == '.')
				{
					base.StringBuffer.Append(current);
					base.StringBuffer.Append(GetNext());
					return NumberFraction();
				}
				base.StringBuffer.Append(current);
				return NumberRest();
			}
			if (current == '.')
			{
				base.StringBuffer.Append(current);
				base.StringBuffer.Append(GetNext());
				return NumberFraction();
			}
			if (current.IsDigit())
			{
				break;
			}
			current = GetNext();
		}
		base.StringBuffer.Append(current);
		return NumberRest();
	}

	private Token NumberRest()
	{
		char next = GetNext();
		while (next.IsDigit())
		{
			base.StringBuffer.Append(next);
			next = GetNext();
		}
		if (next.IsNameStart())
		{
			string number = FlushBuffer();
			base.StringBuffer.Append(next);
			return Dimension(number);
		}
		if (IsValidEscape(next))
		{
			next = GetNext();
			string number2 = FlushBuffer();
			base.StringBuffer.Append(ConsumeEscape(next));
			return Dimension(number2);
		}
		switch (next)
		{
		case '.':
			next = GetNext();
			if (next.IsDigit())
			{
				base.StringBuffer.Append('.').Append(next);
				return NumberFraction();
			}
			Back();
			return NewNumber(FlushBuffer());
		case '%':
			return NewPercentage(FlushBuffer());
		case 'E':
		case 'e':
			return NumberExponential(next);
		case '-':
			return NumberDash();
		default:
			Back();
			return NewNumber(FlushBuffer());
		}
	}

	private Token NumberFraction()
	{
		char next = GetNext();
		while (next.IsDigit())
		{
			base.StringBuffer.Append(next);
			next = GetNext();
		}
		if (next.IsNameStart())
		{
			string number = FlushBuffer();
			base.StringBuffer.Append(next);
			return Dimension(number);
		}
		if (IsValidEscape(next))
		{
			next = GetNext();
			string number2 = FlushBuffer();
			base.StringBuffer.Append(ConsumeEscape(next));
			return Dimension(number2);
		}
		switch (next)
		{
		case 'E':
		case 'e':
			return NumberExponential(next);
		case '%':
			return NewPercentage(FlushBuffer());
		case '-':
			return NumberDash();
		default:
			Back();
			return NewNumber(FlushBuffer());
		}
	}

	private Token Dimension(string number)
	{
		while (true)
		{
			char next = GetNext();
			if (next.IsLetter())
			{
				base.StringBuffer.Append(next);
				continue;
			}
			if (!IsValidEscape(next))
			{
				break;
			}
			next = GetNext();
			base.StringBuffer.Append(ConsumeEscape(next));
		}
		Back();
		return NewDimension(number, FlushBuffer());
	}

	private Token SciNotation()
	{
		while (true)
		{
			char next = GetNext();
			if (!next.IsDigit())
			{
				break;
			}
			base.StringBuffer.Append(next);
		}
		Back();
		return NewNumber(FlushBuffer());
	}

	private Token UrlStart(string functionName)
	{
		char c = SkipSpaces();
		switch (c)
		{
		case '\uffff':
			RaiseErrorOccurred(ParseError.EOF);
			return NewUrl(functionName, string.Empty, bad: true);
		case '"':
			return UrlDoubleQuote(functionName);
		case '\'':
			return UrlSingleQuote(functionName);
		case ')':
			return NewUrl(functionName, string.Empty);
		default:
			return UrlUnquoted(c, functionName);
		}
	}

	private Token UrlDoubleQuote(string functionName)
	{
		while (true)
		{
			char next = GetNext();
			if (next.IsLineBreak())
			{
				break;
			}
			switch (next)
			{
			case '\uffff':
				return NewUrl(functionName, FlushBuffer());
			case '"':
				return UrlEnd(functionName);
			default:
				base.StringBuffer.Append(next);
				break;
			case '\\':
				next = GetNext();
				if (next == '\uffff')
				{
					Back(2);
					RaiseErrorOccurred(ParseError.EOF);
					return NewUrl(functionName, FlushBuffer(), bad: true);
				}
				if (next.IsLineBreak())
				{
					base.StringBuffer.AppendLine();
				}
				else
				{
					base.StringBuffer.Append(ConsumeEscape(next));
				}
				break;
			}
		}
		RaiseErrorOccurred(ParseError.LineBreakUnexpected);
		return UrlBad(functionName);
	}

	private Token UrlSingleQuote(string functionName)
	{
		while (true)
		{
			char next = GetNext();
			if (next.IsLineBreak())
			{
				break;
			}
			switch (next)
			{
			case '\uffff':
				return NewUrl(functionName, FlushBuffer());
			case '\'':
				return UrlEnd(functionName);
			default:
				base.StringBuffer.Append(next);
				break;
			case '\\':
				next = GetNext();
				if (next == '\uffff')
				{
					Back(2);
					RaiseErrorOccurred(ParseError.EOF);
					return NewUrl(functionName, FlushBuffer(), bad: true);
				}
				if (next.IsLineBreak())
				{
					base.StringBuffer.AppendLine();
				}
				else
				{
					base.StringBuffer.Append(ConsumeEscape(next));
				}
				break;
			}
		}
		RaiseErrorOccurred(ParseError.LineBreakUnexpected);
		return UrlBad(functionName);
	}

	private Token UrlUnquoted(char current, string functionName)
	{
		while (true)
		{
			if (current.IsSpaceCharacter())
			{
				return UrlEnd(functionName);
			}
			if (current.IsOneOf(')', '\uffff'))
			{
				return NewUrl(functionName, FlushBuffer());
			}
			if (current.IsOneOf('"', '\'', '(') || current.IsNonPrintable())
			{
				RaiseErrorOccurred(ParseError.InvalidCharacter);
				return UrlBad(functionName);
			}
			if (current != '\\')
			{
				base.StringBuffer.Append(current);
			}
			else
			{
				if (!IsValidEscape(current))
				{
					break;
				}
				current = GetNext();
				base.StringBuffer.Append(ConsumeEscape(current));
			}
			current = GetNext();
		}
		RaiseErrorOccurred(ParseError.InvalidCharacter);
		return UrlBad(functionName);
	}

	private Token UrlEnd(string functionName)
	{
		char next;
		do
		{
			next = GetNext();
			if (next == ')')
			{
				return NewUrl(functionName, FlushBuffer());
			}
		}
		while (next.IsSpaceCharacter());
		RaiseErrorOccurred(ParseError.InvalidCharacter);
		Back();
		return UrlBad(functionName);
	}

	private Token UrlBad(string functionName)
	{
		char c = base.Current;
		int num = 0;
		int num2 = 1;
		while (c != '\uffff')
		{
			switch (c)
			{
			case ';':
				Back();
				return NewUrl(functionName, FlushBuffer(), bad: true);
			case '}':
				if (--num == -1)
				{
					Back();
					return NewUrl(functionName, FlushBuffer(), bad: true);
				}
				break;
			}
			if (c == ')' && --num2 == 0)
			{
				return NewUrl(functionName, FlushBuffer(), bad: true);
			}
			if (IsValidEscape(c))
			{
				c = GetNext();
				base.StringBuffer.Append(ConsumeEscape(c));
			}
			else
			{
				if (c == '(')
				{
					num2++;
				}
				else if (num == 123)
				{
					num++;
				}
				base.StringBuffer.Append(c);
			}
			c = GetNext();
		}
		RaiseErrorOccurred(ParseError.EOF);
		return NewUrl(functionName, FlushBuffer(), bad: true);
	}

	private Token UnicodeRange(char current)
	{
		for (int i = 0; i < 6; i++)
		{
			if (!current.IsHex())
			{
				break;
			}
			base.StringBuffer.Append(current);
			current = GetNext();
		}
		if (base.StringBuffer.Length != 6)
		{
			for (int j = 0; j < 6 - base.StringBuffer.Length; j++)
			{
				if (current != '?')
				{
					current = GetPrevious();
					break;
				}
				base.StringBuffer.Append(current);
				current = GetNext();
			}
			return NewRange(FlushBuffer());
		}
		if (current == '-')
		{
			current = GetNext();
			if (current.IsHex())
			{
				string start = FlushBuffer();
				for (int k = 0; k < 6; k++)
				{
					if (!current.IsHex())
					{
						current = GetPrevious();
						break;
					}
					base.StringBuffer.Append(current);
					current = GetNext();
				}
				string end = FlushBuffer();
				return NewRange(start, end);
			}
			Back(2);
			return NewRange(FlushBuffer());
		}
		Back();
		return NewRange(FlushBuffer());
	}

	private Token NewMatch(string match)
	{
		return new Token(TokenType.Match, match, _position);
	}

	private Token NewColumn()
	{
		return new Token(TokenType.Column, Combinators.Column, _position);
	}

	private Token NewCloseCurly()
	{
		return new Token(TokenType.CurlyBracketClose, "}", _position);
	}

	private Token NewOpenCurly()
	{
		return new Token(TokenType.CurlyBracketOpen, "{", _position);
	}

	private Token NewCloseSquare()
	{
		return new Token(TokenType.SquareBracketClose, "]", _position);
	}

	private Token NewOpenSquare()
	{
		return new Token(TokenType.SquareBracketOpen, "[", _position);
	}

	private Token NewOpenComment()
	{
		return new Token(TokenType.Cdo, "<!--", _position);
	}

	private Token NewSemicolon()
	{
		return new Token(TokenType.Semicolon, ";", _position);
	}

	private Token NewColon()
	{
		return new Token(TokenType.Colon, ":", _position);
	}

	private Token NewCloseComment()
	{
		return new Token(TokenType.Cdc, "-->", _position);
	}

	private Token NewComma()
	{
		return new Token(TokenType.Comma, ",", _position);
	}

	private Token NewCloseRound()
	{
		return new Token(TokenType.RoundBracketClose, ")", _position);
	}

	private Token NewOpenRound()
	{
		return new Token(TokenType.RoundBracketOpen, "(", _position);
	}

	private Token NewString(string value, char quote, bool bad = false)
	{
		return new StringToken(value, bad, quote, _position);
	}

	private Token NewHash(string value)
	{
		return new KeywordToken(TokenType.Hash, value, _position);
	}

	private Token NewComment(string value, bool bad = false)
	{
		return new CommentToken(value, bad, _position);
	}

	private Token NewAtKeyword(string value)
	{
		return new KeywordToken(TokenType.AtKeyword, value, _position);
	}

	private Token NewIdent(string value)
	{
		return new KeywordToken(TokenType.Ident, value, _position);
	}

	private Token NewFunction(string value)
	{
		FunctionToken functionToken = new FunctionToken(value, _position);
		Token token = Get();
		while (token.Type != TokenType.EndOfFile)
		{
			functionToken.AddArgumentToken(token);
			if (token.Type == TokenType.RoundBracketClose)
			{
				break;
			}
			token = Get();
		}
		return functionToken;
	}

	private Token NewPercentage(string value)
	{
		return new UnitToken(TokenType.Percentage, value, "%", _position);
	}

	private Token NewDimension(string value, string unit)
	{
		return new UnitToken(TokenType.Dimension, value, unit, _position);
	}

	private Token NewUrl(string functionName, string data, bool bad = false)
	{
		return new UrlToken(functionName, data, bad, _position);
	}

	private Token NewRange(string range)
	{
		return new RangeToken(range, _position);
	}

	private Token NewRange(string start, string end)
	{
		return new RangeToken(start, end, _position);
	}

	private Token NewWhitespace(char character)
	{
		return new Token(TokenType.Whitespace, character.ToString(), _position);
	}

	private Token NewNumber(string number)
	{
		return new NumberToken(number, _position);
	}

	private Token NewDelimiter(char c)
	{
		return new Token(TokenType.Delim, c.ToString(), _position);
	}

	private Token NewColor(string text)
	{
		return new ColorToken(text, _position);
	}

	private Token NewEof()
	{
		return new Token(TokenType.EndOfFile, string.Empty, _position);
	}

	private Token NewGreaterThan()
	{
		return new Token(TokenType.GreaterThan, ">", _position);
	}

	private Token NewGreaterThanOrEqual()
	{
		return new Token(TokenType.GreaterThanOrEqual, ">=", _position);
	}

	private Token NewLessThan()
	{
		return new Token(TokenType.LessThan, "<", _position);
	}

	private Token NewLessThanOrEqual()
	{
		return new Token(TokenType.LessThanOrEqual, "<=", _position);
	}

	private Token NewEqual()
	{
		return new Token(TokenType.Equal, "=", _position);
	}

	private Token NumberExponential(char letter)
	{
		char next = GetNext();
		if (next.IsDigit())
		{
			base.StringBuffer.Append(letter).Append(next);
			return SciNotation();
		}
		if (next == '+' || next == '-')
		{
			char value = next;
			next = GetNext();
			if (next.IsDigit())
			{
				base.StringBuffer.Append(letter).Append(value).Append(next);
				return SciNotation();
			}
			Back();
		}
		string number = FlushBuffer();
		base.StringBuffer.Append(letter);
		Back();
		return Dimension(number);
	}

	private Token NumberDash()
	{
		char next = GetNext();
		if (next.IsNameStart())
		{
			string number = FlushBuffer();
			base.StringBuffer.Append('-').Append(next);
			return Dimension(number);
		}
		if (IsValidEscape(next))
		{
			next = GetNext();
			string number2 = FlushBuffer();
			base.StringBuffer.Append('-').Append(ConsumeEscape(next));
			return Dimension(number2);
		}
		Back(2);
		return NewNumber(FlushBuffer());
	}

	private string ConsumeEscape(char current)
	{
		if (!current.IsHex())
		{
			return current.ToString();
		}
		bool flag = true;
		char[] array = new char[6];
		int num = 0;
		while (flag && num < array.Length)
		{
			array[num++] = current;
			current = GetNext();
			flag = current.IsHex();
		}
		if (!current.IsSpaceCharacter())
		{
			Back();
		}
		int num2 = int.Parse(new string(array, 0, num), NumberStyles.HexNumber);
		if (!num2.IsInvalid())
		{
			return num2.ConvertFromUtf32();
		}
		current = '\ufffd';
		return current.ToString();
	}

	private bool IsValidEscape(char current)
	{
		if (current != '\\')
		{
			return false;
		}
		current = GetNext();
		Back();
		if (current != '\uffff')
		{
			return !current.IsLineBreak();
		}
		return false;
	}

	private void RaiseErrorOccurred(ParseError code)
	{
		RaiseErrorOccurred(code, GetCurrentPosition());
	}
}
