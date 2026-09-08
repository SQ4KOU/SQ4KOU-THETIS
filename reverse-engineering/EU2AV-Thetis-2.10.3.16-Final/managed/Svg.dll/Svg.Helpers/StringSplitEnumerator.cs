using System;

namespace Svg.Helpers;

internal ref struct StringSplitEnumerator(ReadOnlySpan<char> str, ReadOnlySpan<char> chars)
{
	private ReadOnlySpan<char> _str = str;

	private readonly ReadOnlySpan<char> _chars = chars;

	public StringPart Current { get; private set; } = default(StringPart);

	public StringSplitEnumerator GetEnumerator()
	{
		return this;
	}

	public bool MoveNext()
	{
		ReadOnlySpan<char> value;
		do
		{
			ReadOnlySpan<char> str = _str;
			if (str.Length == 0)
			{
				return false;
			}
			int num = str.IndexOfAny(_chars);
			if (num == -1)
			{
				Current = new StringPart(str);
				_str = ReadOnlySpan<char>.Empty;
				return true;
			}
			value = str.Slice(0, num);
			_str = str.Slice(num + 1);
		}
		while (value.Length == 0);
		Current = new StringPart(value);
		return true;
	}
}
