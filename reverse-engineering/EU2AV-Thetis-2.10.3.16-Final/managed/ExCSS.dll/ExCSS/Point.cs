namespace ExCSS;

public struct Point(Length x, Length y)
{
	public static readonly Point Center = new Point(Length.Half, Length.Half);

	public static readonly Point LeftTop = new Point(Length.Zero, Length.Zero);

	public static readonly Point RightTop = new Point(Length.Full, Length.Zero);

	public static readonly Point RightBottom = new Point(Length.Full, Length.Full);

	public static readonly Point LeftBottom = new Point(Length.Zero, Length.Full);

	public Length X { get; } = x;

	public Length Y { get; } = y;
}
