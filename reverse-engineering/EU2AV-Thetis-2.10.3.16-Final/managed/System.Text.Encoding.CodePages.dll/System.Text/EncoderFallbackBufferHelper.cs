namespace System.Text;

internal unsafe struct EncoderFallbackBufferHelper(EncoderFallbackBuffer fallbackBuffer)
{
	internal unsafe char* charStart;

	internal unsafe char* charEnd = (charStart = null);

	internal System.Text.EncoderNLS encoder = null;

	internal bool setEncoder;

	internal bool bUsedEncoder;

	internal bool bFallingBack = (bUsedEncoder = (setEncoder = false));

	internal int iRecursionCount = 0;

	private const int iMaxRecursion = 250;

	private readonly EncoderFallbackBuffer _fallbackBuffer = fallbackBuffer;

	internal unsafe void InternalReset()
	{
		charStart = null;
		bFallingBack = false;
		iRecursionCount = 0;
		_fallbackBuffer.Reset();
	}

	internal unsafe void InternalInitialize(char* _charStart, char* _charEnd, System.Text.EncoderNLS _encoder, bool _setEncoder)
	{
		charStart = _charStart;
		charEnd = _charEnd;
		encoder = _encoder;
		setEncoder = _setEncoder;
		bUsedEncoder = false;
		bFallingBack = false;
		iRecursionCount = 0;
	}

	internal char InternalGetNextChar()
	{
		char nextChar = _fallbackBuffer.GetNextChar();
		bFallingBack = nextChar != '\0';
		if (nextChar == '\0')
		{
			iRecursionCount = 0;
		}
		return nextChar;
	}

	internal unsafe bool InternalFallback(char ch, ref char* chars)
	{
		int index = (int)(chars - charStart) - 1;
		if (char.IsHighSurrogate(ch))
		{
			if (chars >= charEnd)
			{
				if (encoder != null && !encoder.MustFlush)
				{
					if (setEncoder)
					{
						bUsedEncoder = true;
						encoder.charLeftOver = ch;
					}
					bFallingBack = false;
					return false;
				}
			}
			else
			{
				char c = *chars;
				if (char.IsLowSurrogate(c))
				{
					if (bFallingBack && iRecursionCount++ > 250)
					{
						ThrowLastCharRecursive(char.ConvertToUtf32(ch, c));
					}
					chars++;
					bFallingBack = _fallbackBuffer.Fallback(ch, c, index);
					return bFallingBack;
				}
			}
		}
		if (bFallingBack && iRecursionCount++ > 250)
		{
			ThrowLastCharRecursive(ch);
		}
		bFallingBack = _fallbackBuffer.Fallback(ch, index);
		return bFallingBack;
	}

	internal static void ThrowLastCharRecursive(int charRecursive)
	{
		throw new ArgumentException(System.SR.Format(System.SR.Argument_RecursiveFallback, charRecursive), "chars");
	}
}
