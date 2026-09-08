using System;

namespace ExCSS;

public struct TextPosition(ushort line, ushort column, int position) : IEquatable<TextPosition>, IComparable<TextPosition>
{
	public static readonly TextPosition Empty;

	private readonly ushort _line = line;

	private readonly ushort _column = column;

	public int Line => _line;

	public int Column => _column;

	public int Position { get; } = position;

	public TextPosition Shift(int columns)
	{
		return new TextPosition(_line, (ushort)(_column + columns), Position + columns);
	}

	public TextPosition After(char chr)
	{
		ushort line = _line;
		ushort column = _column;
		if (chr != '\n')
		{
			return new TextPosition(line, ++column, Position + 1);
		}
		line++;
		column = 0;
		return new TextPosition(line, ++column, Position + 1);
	}

	public TextPosition After(string str)
	{
		ushort num = _line;
		ushort num2 = _column;
		for (int i = 0; i < str.Length; i++)
		{
			if (str[i] == '\n')
			{
				num++;
				num2 = 0;
			}
			num2++;
		}
		return new TextPosition(num, num2, Position + str.Length);
	}

	public override string ToString()
	{
		return $"Line {_line}, Column {_column}, Position {Position}";
	}

	public override int GetHashCode()
	{
		return Position ^ ((_line | _column) + _line);
	}

	public override bool Equals(object obj)
	{
		if (obj is TextPosition other)
		{
			return Equals(other);
		}
		return false;
	}

	public bool Equals(TextPosition other)
	{
		if (Position == other.Position && _column == other._column)
		{
			return _line == other._line;
		}
		return false;
	}

	public static bool operator >(TextPosition a, TextPosition b)
	{
		return a.Position > b.Position;
	}

	public static bool operator <(TextPosition a, TextPosition b)
	{
		return a.Position < b.Position;
	}

	public int CompareTo(TextPosition other)
	{
		if (!Equals(other))
		{
			if (!(this > other))
			{
				return -1;
			}
			return 1;
		}
		return 0;
	}
}
