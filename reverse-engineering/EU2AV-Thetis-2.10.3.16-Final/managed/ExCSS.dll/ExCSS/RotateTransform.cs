using System;

namespace ExCSS;

internal sealed class RotateTransform : ITransform
{
	public float X { get; }

	public float Y { get; }

	public float Z { get; }

	public float Angle { get; }

	internal RotateTransform(float x, float y, float z, float angle)
	{
		X = x;
		Y = y;
		Z = z;
		Angle = angle;
	}

	public TransformMatrix ComputeMatrix()
	{
		float num = 1f / (float)Math.Sqrt(X * X + Y * Y + Z * Z);
		float num2 = (float)Math.Sin(Angle);
		float num3 = (float)Math.Cos(Angle);
		float num4 = X * num;
		float num5 = Y * num;
		float num6 = Z * num;
		float num7 = 1f - num3;
		return new TransformMatrix(num4 * num4 * num7 + num3, num5 * num4 * num7 - num6 * num2, num6 * num4 * num7 + num5 * num2, num4 * num5 * num7 + num6 * num2, num5 * num5 * num7 + num3, num6 * num5 * num7 - num4 * num2, num4 * num6 * num7 - num5 * num2, num5 * num6 * num7 + num4 * num2, num6 * num6 * num7 + num3, 0f, 0f, 0f, 0f, 0f, 0f);
	}
}
