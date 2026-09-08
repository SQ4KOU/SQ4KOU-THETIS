using System.Collections.Generic;

namespace ExCSS;

internal static class Symbols
{
	public const char StartOfHeading = '\u0001';

	public const char Backspace = '\b';

	public const char UnitSeparator = '\u001f';

	public const char ShiftOut = '\u000e';

	public const char Zero = '0';

	public const char Seven = '7';

	public const char Nine = '9';

	public const char CapitalA = 'A';

	public const char CapitalF = 'F';

	public const char CapitalW = 'W';

	public const char CapitalZ = 'Z';

	public const char LowerA = 'a';

	public const char LowerF = 'f';

	public const char LowerZ = 'z';

	public const char Delete = '\u007f';

	public const char EndOfFile = '\uffff';

	public const char Tilde = '~';

	public const char Pipe = '|';

	public const char Null = '\0';

	public const char Num = '#';

	public const char Dollar = '$';

	public const char Semicolon = ';';

	public const char Asterisk = '*';

	public const char Equality = '=';

	public const char Plus = '+';

	public const char Minus = '-';

	public const char Comma = ',';

	public const char Dot = '.';

	public const char Accent = '^';

	public const char At = '@';

	public const char LessThan = '<';

	public const char GreaterThan = '>';

	public const char SingleQuote = '\'';

	public const char DoubleQuote = '"';

	public const char CurvedQuote = '`';

	public const char QuestionMark = '?';

	public const char Tab = '\t';

	public const char LineFeed = '\n';

	public const char CarriageReturn = '\r';

	public const char FormFeed = '\f';

	public const char Space = ' ';

	public const char Solidus = '/';

	public const char ReverseSolidus = '\\';

	public const char Colon = ':';

	public const char ExclamationMark = '!';

	public const char Replacement = '\ufffd';

	public const char Underscore = '_';

	public const char RoundBracketOpen = '(';

	public const char RoundBracketClose = ')';

	public const char SquareBracketOpen = '[';

	public const char SquareBracketClose = ']';

	public const char CurlyBracketOpen = '{';

	public const char CurlyBracketClose = '}';

	public const char Percent = '%';

	public const int MaximumCodepoint = 1114111;

	public const char ExtendedAsciiStart = '\u0080';

	public const char NonBreakingSpace = '\u00a0';

	public const char UTF16SurrogateMin = '\ud800';

	public const char UTF16SurrogateMax = '\udfff';

	public static Dictionary<char, char> Punycode = new Dictionary<char, char>
	{
		{ '。', '.' },
		{ '．', '.' },
		{ 'Ｇ', 'g' },
		{ 'ｏ', 'o' },
		{ 'ｃ', 'c' },
		{ 'Ｘ', 'x' },
		{ '０', '0' },
		{ '１', '1' },
		{ '２', '2' },
		{ '５', '5' }
	};
}
