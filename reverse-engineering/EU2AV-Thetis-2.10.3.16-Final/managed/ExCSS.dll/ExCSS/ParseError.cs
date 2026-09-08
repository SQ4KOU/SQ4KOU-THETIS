namespace ExCSS;

public enum ParseError : byte
{
	EOF = 0,
	InvalidCharacter = 16,
	InvalidBlockStart = 17,
	InvalidToken = 18,
	ColonMissing = 19,
	IdentExpected = 20,
	InputUnexpected = 21,
	LineBreakUnexpected = 22,
	UnknownAtRule = 32,
	InvalidSelector = 48,
	InvalidKeyframe = 49,
	ValueMissing = 64,
	InvalidValue = 65,
	UnknownDeclarationName = 80
}
