using System;
using System.Collections.Generic;

namespace Microsoft.NET.StringTools;

public class SpanBasedStringBuilder : IDisposable
{
	public struct Enumerator
	{
		private readonly List<ReadOnlyMemory<char>> _spans;

		private int _spanIndex;

		private int _charIndex;

		public readonly char Current => _spans[_spanIndex].Span[_charIndex];

		internal Enumerator(List<ReadOnlyMemory<char>> spans)
		{
			_spans = spans;
			_spanIndex = 0;
			_charIndex = -1;
		}

		public bool MoveNext()
		{
			int num = _charIndex + 1;
			while (_spanIndex < _spans.Count)
			{
				if (num < _spans[_spanIndex].Length)
				{
					_charIndex = num;
					return true;
				}
				_spanIndex++;
				num = 0;
			}
			return false;
		}
	}

	private readonly List<ReadOnlyMemory<char>> _spans;

	internal List<ReadOnlyMemory<char>> Spans => _spans;

	public int Length { get; private set; }

	public int Capacity => _spans.Capacity;

	public char this[int index]
	{
		get
		{
			if (index < 0 || index >= Length)
			{
				throw new ArgumentOutOfRangeException("index");
			}
			int num = index;
			foreach (ReadOnlyMemory<char> span in _spans)
			{
				if (num < span.Length)
				{
					return span.Span[num];
				}
				num -= span.Length;
			}
			throw new IndexOutOfRangeException();
		}
	}

	public SpanBasedStringBuilder(string str)
		: this()
	{
		if (str == null)
		{
			throw new ArgumentNullException("str");
		}
		Append(str);
	}

	public SpanBasedStringBuilder(int capacity = 4)
	{
		_spans = new List<ReadOnlyMemory<char>>(capacity);
		Length = 0;
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(_spans);
	}

	public bool Equals(string other)
	{
		return Equals(other.AsSpan(), StringComparison.Ordinal);
	}

	public bool Equals(string other, StringComparison comparison)
	{
		return Equals(other.AsSpan(), comparison);
	}

	public bool Equals(ReadOnlySpan<char> other)
	{
		return Equals(other, StringComparison.Ordinal);
	}

	public bool Equals(ReadOnlySpan<char> other, StringComparison comparison)
	{
		if (_spans.Count == 0 && other.IsEmpty)
		{
			return true;
		}
		if (_spans.Count == 0 || other.IsEmpty || Length != other.Length)
		{
			return false;
		}
		int num = 0;
		foreach (ReadOnlyMemory<char> span in _spans)
		{
			if (!other.Slice(num, span.Length).Equals(span.Span, comparison))
			{
				return false;
			}
			num += span.Length;
		}
		return true;
	}

	public override string ToString()
	{
		return new InternableString(this).ToString();
	}

	public void Dispose()
	{
		Strings.ReturnSpanBasedStringBuilder(this);
	}

	public void Append(string? value)
	{
		if (!string.IsNullOrEmpty(value))
		{
			_spans.Add(value.AsMemory());
			Length += value.Length;
		}
	}

	public void Append(string value, int startIndex, int count)
	{
		if (value != null)
		{
			if (count > 0)
			{
				_spans.Add(value.AsMemory(startIndex, count));
				Length += count;
			}
		}
		else if (startIndex != 0 || count != 0)
		{
			throw new ArgumentNullException("value");
		}
	}

	public void Append(ReadOnlyMemory<char> span)
	{
		if (!span.IsEmpty)
		{
			_spans.Add(span);
			Length += span.Length;
		}
	}

	public void TrimStart()
	{
		for (int i = 0; i < _spans.Count; i++)
		{
			ReadOnlyMemory<char> readOnlyMemory = _spans[i];
			ReadOnlySpan<char> span = readOnlyMemory.Span;
			int j;
			for (j = 0; j < span.Length && char.IsWhiteSpace(span[j]); j++)
			{
			}
			if (j > 0)
			{
				List<ReadOnlyMemory<char>> spans = _spans;
				int index = i;
				readOnlyMemory = _spans[i];
				spans[index] = readOnlyMemory.Slice(j);
				Length -= j;
			}
			readOnlyMemory = _spans[i];
			if (!readOnlyMemory.IsEmpty)
			{
				break;
			}
		}
	}

	public void TrimStart(char c)
	{
		for (int i = 0; i < _spans.Count; i++)
		{
			ReadOnlyMemory<char> readOnlyMemory = _spans[i];
			ReadOnlySpan<char> span = readOnlyMemory.Span;
			int j;
			for (j = 0; j < span.Length && span[j] == c; j++)
			{
			}
			if (j > 0)
			{
				List<ReadOnlyMemory<char>> spans = _spans;
				int index = i;
				readOnlyMemory = _spans[i];
				spans[index] = readOnlyMemory.Slice(j);
				Length -= j;
			}
			readOnlyMemory = _spans[i];
			if (!readOnlyMemory.IsEmpty)
			{
				break;
			}
		}
	}

	public void TrimEnd()
	{
		for (int num = _spans.Count - 1; num >= 0; num--)
		{
			ReadOnlyMemory<char> readOnlyMemory = _spans[num];
			ReadOnlySpan<char> span = readOnlyMemory.Span;
			int num2 = span.Length - 1;
			while (num2 >= 0 && char.IsWhiteSpace(span[num2]))
			{
				num2--;
			}
			if (num2 + 1 < span.Length)
			{
				List<ReadOnlyMemory<char>> spans = _spans;
				int index = num;
				readOnlyMemory = _spans[num];
				spans[index] = readOnlyMemory.Slice(0, num2 + 1);
				Length -= span.Length - (num2 + 1);
			}
			readOnlyMemory = _spans[num];
			if (!readOnlyMemory.IsEmpty)
			{
				break;
			}
		}
	}

	public void TrimEnd(char c)
	{
		for (int num = _spans.Count - 1; num >= 0; num--)
		{
			ReadOnlyMemory<char> readOnlyMemory = _spans[num];
			ReadOnlySpan<char> span = readOnlyMemory.Span;
			int num2 = span.Length - 1;
			while (num2 >= 0 && span[num2] == c)
			{
				num2--;
			}
			if (num2 + 1 < span.Length)
			{
				List<ReadOnlyMemory<char>> spans = _spans;
				int index = num;
				readOnlyMemory = _spans[num];
				spans[index] = readOnlyMemory.Slice(0, num2 + 1);
				Length -= span.Length - (num2 + 1);
			}
			readOnlyMemory = _spans[num];
			if (!readOnlyMemory.IsEmpty)
			{
				break;
			}
		}
	}

	public void Trim()
	{
		TrimStart();
		TrimEnd();
	}

	public void Trim(char c)
	{
		TrimStart(c);
		TrimEnd(c);
	}

	public void Clear()
	{
		_spans.Clear();
		Length = 0;
	}
}
