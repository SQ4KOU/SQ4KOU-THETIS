using System;

namespace Markdig.Helpers;

internal struct LazySubstring
{
	private string _text;

	public int Offset;

	public int Length;

	public LazySubstring(string text)
	{
		_text = text;
		Offset = 0;
		Length = text.Length;
	}

	public LazySubstring(string text, int offset, int length)
	{
		_text = text;
		Offset = offset;
		Length = length;
	}

	public ReadOnlySpan<char> AsSpan()
	{
		return _text.AsSpan(Offset, Length);
	}

	public override string ToString()
	{
		if (Offset != 0 || Length != _text.Length)
		{
			_text = _text.Substring(Offset, Length);
			Offset = 0;
		}
		return _text;
	}
}
