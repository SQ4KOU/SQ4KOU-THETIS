using System;

namespace ExCSS;

internal sealed class SkewTransform : ITransform
{
	public float Alpha { get; }

	public float Beta { get; }

	internal SkewTransform(float alpha, float beta)
	{
		Alpha = alpha;
		Beta = beta;
	}

	public TransformMatrix ComputeMatrix()
	{
		float m = (float)Math.Tan(Alpha);
		float m2 = (float)Math.Tan(Beta);
		return new TransformMatrix(1f, m, 0f, m2, 1f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 0f, 0f);
	}
}
