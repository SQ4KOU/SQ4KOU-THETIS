namespace ExCSS;

public sealed class Shape
{
	public Length Top { get; }

	public Length Right { get; }

	public Length Bottom { get; }

	public Length Left { get; }

	public Shape(Length top, Length right, Length bottom, Length left)
	{
		Top = top;
		Right = right;
		Bottom = bottom;
		Left = left;
	}
}
