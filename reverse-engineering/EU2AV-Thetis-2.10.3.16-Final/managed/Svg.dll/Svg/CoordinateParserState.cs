using System;

namespace Svg;

public ref struct CoordinateParserState
{
	public NumState CurrNumState;

	public NumState NewNumState;

	public int CharsPosition;

	public int Position;

	public bool HasMore;

	public CoordinateParserState(ref ReadOnlySpan<char> chars)
	{
		CurrNumState = NumState.Separator;
		NewNumState = NumState.Separator;
		CharsPosition = 0;
		Position = 0;
		HasMore = chars.Length > 0;
		if (char.IsLetter(chars[0]))
		{
			CharsPosition++;
		}
	}
}
