using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Markdig.Syntax;

namespace Markdig.Helpers;

public struct StringLineGroup : IEnumerable
{
	public struct Enumerator(StringLineGroup parent) : IEnumerator
	{
		private readonly StringLineGroup _parent = parent;

		private int _index = -1;

		public object Current => _parent.Lines[_index];

		public bool MoveNext()
		{
			return ++_index < _parent.Count;
		}

		public void Reset()
		{
			_index = -1;
		}
	}

	public struct Iterator : ICharIterator
	{
		private readonly StringLineGroup _lines;

		private StringSlice _currentSlice;

		private int _offset;

		public int Start { get; private set; }

		public char CurrentChar { get; private set; }

		public int End { get; private set; }

		public readonly bool IsEmpty => Start > End;

		public int SliceIndex { get; private set; }

		public Iterator(StringLineGroup stringLineGroup)
		{
			_lines = stringLineGroup;
			Start = -1;
			_offset = -1;
			SliceIndex = 0;
			CurrentChar = '\0';
			End = -1;
			StringLine[] lines = stringLineGroup.Lines;
			for (int i = 0; i < stringLineGroup.Count && i < lines.Length; i++)
			{
				ref StringSlice slice = ref lines[i].Slice;
				End += slice.Length + slice.NewLine.Length();
			}
			_currentSlice = _lines.Lines[0].Slice;
			SkipChar();
		}

		public StringLineGroup Remaining()
		{
			StringLineGroup lines = _lines;
			if (IsEmpty)
			{
				lines.Clear();
			}
			else
			{
				lines.RemoveStartRange(SliceIndex);
				if (lines.Count > 0 && _offset > 0)
				{
					ref StringLine reference = ref lines.Lines[0];
					reference.Column += _offset;
					reference.Slice.Start += _offset;
				}
			}
			return lines;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public char NextChar()
		{
			Start++;
			if (Start <= End)
			{
				ref StringSlice currentSlice = ref _currentSlice;
				_offset++;
				int num = currentSlice.Start + _offset;
				string text = currentSlice.Text;
				if (num <= currentSlice.End && (uint)num < (uint)text.Length)
				{
					return CurrentChar = text[num];
				}
				return NextCharNewLine();
			}
			return NextCharEndOfEnumerator();
		}

		private char NextCharNewLine()
		{
			int length = _currentSlice.Length;
			NewLine newLine = _currentSlice.NewLine;
			if (_offset == length)
			{
				if (newLine == NewLine.LineFeed)
				{
					CurrentChar = '\n';
				}
				else
				{
					if (newLine != NewLine.CarriageReturn)
					{
						if (newLine == NewLine.CarriageReturnLineFeed)
						{
							CurrentChar = '\r';
						}
						goto IL_00b4;
					}
					CurrentChar = '\r';
				}
			}
			else
			{
				if (_offset - 1 != length || newLine != NewLine.CarriageReturnLineFeed)
				{
					goto IL_00b4;
				}
				CurrentChar = '\n';
			}
			if (SliceIndex < _lines.Lines.Length - 1)
			{
				SliceIndex++;
				_offset = -1;
				_currentSlice = _lines.Lines[SliceIndex];
			}
			goto IL_00b4;
			IL_00b4:
			return CurrentChar;
		}

		private char NextCharEndOfEnumerator()
		{
			CurrentChar = '\0';
			Start = End + 1;
			SliceIndex = _lines.Count;
			return '\0';
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SkipChar()
		{
			NextChar();
		}

		public readonly char PeekChar()
		{
			return PeekChar(1);
		}

		public readonly char PeekChar(int offset)
		{
			if (offset < 0)
			{
				ThrowHelper.ArgumentOutOfRangeException("Negative offset are not supported for StringLineGroup", "offset");
			}
			if (Start + offset > End)
			{
				return '\0';
			}
			offset += _offset;
			int num = SliceIndex;
			ref StringSlice slice = ref _lines.Lines[num].Slice;
			NewLine newLine = slice.NewLine;
			if (newLine != NewLine.CarriageReturnLineFeed || offset != slice.Length + 1)
			{
				while (offset > slice.Length)
				{
					offset -= slice.Length + 1;
					slice = ref _lines.Lines[++num].Slice;
				}
			}
			else if (slice.NewLine == NewLine.CarriageReturnLineFeed)
			{
				return '\n';
			}
			if (offset == slice.Length)
			{
				switch (newLine)
				{
				case NewLine.LineFeed:
					return '\n';
				case NewLine.CarriageReturn:
					return '\r';
				case NewLine.CarriageReturnLineFeed:
					return '\r';
				}
			}
			return slice[slice.Start + offset];
		}

		public bool TrimStart()
		{
			char c = CurrentChar;
			while (c.IsWhitespace())
			{
				c = NextChar();
			}
			return IsEmpty;
		}
	}

	public readonly struct LineOffset(int linePosition, int column, int offset, int start, int end)
	{
		public readonly int LinePosition = linePosition;

		public readonly int Column = column;

		public readonly int Offset = offset;

		public readonly int Start = start;

		public readonly int End = end;
	}

	private static readonly CustomArrayPool<StringLine> _pool = new CustomArrayPool<StringLine>(512, 386, 128, 64);

	public StringLine[] Lines { get; private set; }

	public int Count { get; private set; }

	public StringLineGroup(int capacity)
	{
		if (capacity <= 0)
		{
			ThrowHelper.ArgumentOutOfRangeException("capacity");
		}
		Lines = _pool.Rent(capacity);
		Count = 0;
	}

	internal StringLineGroup(int capacity, bool willRelease)
	{
		if (capacity <= 0)
		{
			ThrowHelper.ArgumentOutOfRangeException("capacity");
		}
		Lines = _pool.Rent(willRelease ? Math.Max(8, capacity) : capacity);
		Count = 0;
	}

	public StringLineGroup(string text)
	{
		if (text == null)
		{
			ThrowHelper.ArgumentNullException_text();
		}
		Lines = new StringLine[1];
		Count = 0;
		Add(new StringSlice(text));
	}

	public void Clear()
	{
		Array.Clear(Lines, 0, Lines.Length);
		Count = 0;
	}

	public void RemoveAt(int index)
	{
		if (index != Count - 1)
		{
			Array.Copy(Lines, index + 1, Lines, index, Count - index - 1);
		}
		Lines[Count - 1] = default(StringLine);
		Count--;
	}

	internal void RemoveStartRange(int toRemove)
	{
		int num = (Count -= toRemove);
		Array.Copy(Lines, toRemove, Lines, 0, num);
		Array.Clear(Lines, num, toRemove);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Add(ref StringLine line)
	{
		if (Count == Lines.Length)
		{
			IncreaseCapacity();
		}
		Lines[Count++] = line;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Add(StringSlice slice)
	{
		if (Count == Lines.Length)
		{
			IncreaseCapacity();
		}
		Lines[Count++] = new StringLine(ref slice);
	}

	public override readonly string ToString()
	{
		return ToSlice().ToString();
	}

	public readonly StringSlice ToSlice(List<LineOffset>? lineOffsets = null)
	{
		if (Count == 1)
		{
			ref StringLine reference = ref Lines[0];
			lineOffsets?.Add(new LineOffset(reference.Position, reference.Column, reference.Slice.Start - reference.Position, reference.Slice.Start, reference.Slice.End + 1));
			return Lines[0];
		}
		if (Count == 0)
		{
			return StringSlice.Empty;
		}
		if (lineOffsets != null && lineOffsets.Capacity < lineOffsets.Count + Count)
		{
			lineOffsets.Capacity = Math.Max(lineOffsets.Count + Count, lineOffsets.Capacity * 2);
		}
		Span<char> initialBuffer = stackalloc char[64];
		ValueStringBuilder valueStringBuilder = new ValueStringBuilder(initialBuffer);
		int start = 0;
		NewLine newLine = NewLine.None;
		for (int i = 0; i < Count; i++)
		{
			if (i > 0)
			{
				valueStringBuilder.Append(newLine.AsString());
				start = valueStringBuilder.Length;
			}
			ref StringLine reference2 = ref Lines[i];
			if (!reference2.Slice.IsEmpty)
			{
				valueStringBuilder.Append(reference2.Slice.AsSpan());
			}
			newLine = reference2.NewLine;
			lineOffsets?.Add(new LineOffset(reference2.Position, reference2.Column, reference2.Slice.Start - reference2.Position, start, valueStringBuilder.Length));
		}
		return new StringSlice(valueStringBuilder.ToString());
	}

	public readonly Iterator ToCharIterator()
	{
		return new Iterator(this);
	}

	public void Trim()
	{
		for (int i = 0; i < Count; i++)
		{
			Lines[i].Slice.Trim();
		}
	}

	internal SourceSpan ConvertToAbsoluteSpan(SourceSpan span)
	{
		if (span.IsEmpty || Count == 0)
		{
			return span;
		}
		int absolutePosition = GetAbsolutePosition(span.Start);
		int absolutePosition2 = GetAbsolutePosition(span.End);
		return new SourceSpan(absolutePosition, absolutePosition2);
	}

	private int GetAbsolutePosition(int position)
	{
		int num = 0;
		for (int i = 0; i < Count; i++)
		{
			ref StringSlice slice = ref Lines[i].Slice;
			int num2 = slice.Length + slice.NewLine.Length();
			if (i == Count - 1 || position < num + num2)
			{
				return slice.Start + (position - num);
			}
			num += num2;
		}
		return Lines[Count - 1].Slice.End + 1;
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(this);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	private void IncreaseCapacity()
	{
		StringLine[] array = _pool.Rent(Lines.Length * 2);
		if (Count > 0)
		{
			Array.Copy(Lines, 0, array, 0, Count);
			Array.Clear(Lines, 0, Count);
		}
		_pool.Return(Lines);
		Lines = array;
	}

	internal void Release()
	{
		Array.Clear(Lines, 0, Count);
		_pool.Return(Lines);
		Lines = null;
		Count = -1;
	}
}
