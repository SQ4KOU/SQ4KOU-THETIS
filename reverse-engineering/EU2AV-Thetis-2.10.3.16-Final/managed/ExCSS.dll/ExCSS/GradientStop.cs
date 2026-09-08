namespace ExCSS;

public struct GradientStop(Color color, Length location)
{
	public Color Color { get; } = color;

	public Length Location { get; } = location;
}
