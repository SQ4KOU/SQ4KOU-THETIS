namespace ExCSS;

internal sealed class TranslateTransform : ITransform
{
	public Length Dx { get; }

	public Length Dy { get; }

	public Length Dz { get; }

	internal TranslateTransform(Length x, Length y, Length z)
	{
		Dx = x;
		Dy = y;
		Dz = z;
	}

	public TransformMatrix ComputeMatrix()
	{
		float tx = Dx.ToPixel();
		float ty = Dy.ToPixel();
		float tz = Dz.ToPixel();
		return new TransformMatrix(1f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 1f, tx, ty, tz, 0f, 0f, 0f);
	}
}
