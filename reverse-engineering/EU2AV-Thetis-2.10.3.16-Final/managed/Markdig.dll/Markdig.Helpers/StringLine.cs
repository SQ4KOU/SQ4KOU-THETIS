namespace Markdig.Helpers;

public struct StringLine
{
	public StringSlice Slice;

	public int Line;

	public int Position;

	public int Column;

	public NewLine NewLine;

	public StringLine(ref StringSlice slice)
	{
		this = default(StringLine);
		Slice = slice;
		NewLine = slice.NewLine;
	}

	public StringLine(StringSlice slice, int line, int column, int position, NewLine newLine)
	{
		Slice = slice;
		Line = line;
		Column = column;
		Position = position;
		NewLine = newLine;
	}

	public StringLine(ref StringSlice slice, int line, int column, int position, NewLine newLine)
	{
		Slice = slice;
		Line = line;
		Column = column;
		Position = position;
		NewLine = newLine;
	}

	public static implicit operator StringSlice(StringLine line)
	{
		return line.Slice;
	}

	public override readonly string ToString()
	{
		return Slice.ToString();
	}
}
