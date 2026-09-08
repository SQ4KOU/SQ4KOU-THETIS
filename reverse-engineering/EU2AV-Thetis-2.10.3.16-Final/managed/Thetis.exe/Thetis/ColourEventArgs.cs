using System.Drawing;

namespace Thetis;

public class ColourEventArgs
{
	public Color Colour { get; }

	public ColourEventArgs(Color c)
	{
		Colour = c;
	}
}
