namespace System.Text;

internal unsafe struct DecoderFallbackBufferHelper(DecoderFallbackBuffer fallbackBuffer)
{
	internal unsafe byte* byteStart = null;

	internal unsafe char* charEnd = null;

	private readonly DecoderFallbackBuffer _fallbackBuffer = fallbackBuffer;

	internal unsafe void InternalReset()
	{
		byteStart = null;
		_fallbackBuffer.Reset();
	}

	internal unsafe void InternalInitialize(byte* _byteStart, char* _charEnd)
	{
		byteStart = _byteStart;
		charEnd = _charEnd;
	}

	internal unsafe bool InternalFallback(byte[] bytes, byte* pBytes, ref char* chars)
	{
		if (_fallbackBuffer.Fallback(bytes, (int)(pBytes - byteStart - bytes.Length)))
		{
			char* ptr = chars;
			bool flag = false;
			char nextChar;
			while ((nextChar = _fallbackBuffer.GetNextChar()) != 0)
			{
				if (char.IsSurrogate(nextChar))
				{
					if (char.IsHighSurrogate(nextChar))
					{
						if (flag)
						{
							throw new ArgumentException(System.SR.Argument_InvalidCharSequenceNoIndex);
						}
						flag = true;
					}
					else
					{
						if (!flag)
						{
							throw new ArgumentException(System.SR.Argument_InvalidCharSequenceNoIndex);
						}
						flag = false;
					}
				}
				if (ptr >= charEnd)
				{
					return false;
				}
				*(ptr++) = nextChar;
			}
			if (flag)
			{
				throw new ArgumentException(System.SR.Argument_InvalidCharSequenceNoIndex);
			}
			chars = ptr;
		}
		return true;
	}

	internal unsafe int InternalFallback(byte[] bytes, byte* pBytes)
	{
		if (_fallbackBuffer.Fallback(bytes, (int)(pBytes - byteStart - bytes.Length)))
		{
			int num = 0;
			bool flag = false;
			char nextChar;
			while ((nextChar = _fallbackBuffer.GetNextChar()) != 0)
			{
				if (char.IsSurrogate(nextChar))
				{
					if (char.IsHighSurrogate(nextChar))
					{
						if (flag)
						{
							throw new ArgumentException(System.SR.Argument_InvalidCharSequenceNoIndex);
						}
						flag = true;
					}
					else
					{
						if (!flag)
						{
							throw new ArgumentException(System.SR.Argument_InvalidCharSequenceNoIndex);
						}
						flag = false;
					}
				}
				num++;
			}
			if (flag)
			{
				throw new ArgumentException(System.SR.Argument_InvalidCharSequenceNoIndex);
			}
			return num;
		}
		return 0;
	}
}
