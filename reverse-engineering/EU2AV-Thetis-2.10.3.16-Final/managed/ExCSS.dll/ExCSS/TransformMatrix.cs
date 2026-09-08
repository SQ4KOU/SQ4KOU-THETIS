using System;

namespace ExCSS;

public sealed class TransformMatrix : IEquatable<TransformMatrix>
{
	public static readonly TransformMatrix Zero = new TransformMatrix();

	public static readonly TransformMatrix One = new TransformMatrix(1f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 0f, 0f);

	private readonly float[,] _matrix;

	public float Tx => _matrix[0, 3];

	public float Ty => _matrix[1, 3];

	public float Tz => _matrix[2, 3];

	public bool Equals(TransformMatrix other)
	{
		float[,] matrix = _matrix;
		float[,] matrix2 = other._matrix;
		for (int i = 0; i < 4; i++)
		{
			for (int j = 0; j < 4; j++)
			{
				if (matrix[i, j] != matrix2[i, j])
				{
					return false;
				}
			}
		}
		return true;
	}

	private TransformMatrix()
	{
		_matrix = new float[4, 4];
	}

	public TransformMatrix(float[] values)
		: this()
	{
		if (values == null)
		{
			throw new ArgumentNullException("values");
		}
		if (values.Length != 16)
		{
			throw new ArgumentException("You need to provide 16 (4x4) values.", "values");
		}
		int i = 0;
		int num = 0;
		for (; i < 4; i++)
		{
			int num2 = 0;
			while (num2 < 4)
			{
				_matrix[num2, i] = values[num];
				num2++;
				num++;
			}
		}
	}

	public TransformMatrix(float m11, float m12, float m13, float m21, float m22, float m23, float m31, float m32, float m33, float tx, float ty, float tz, float px, float py, float pz)
		: this()
	{
		_matrix[0, 0] = m11;
		_matrix[0, 1] = m12;
		_matrix[0, 2] = m13;
		_matrix[1, 0] = m21;
		_matrix[1, 1] = m22;
		_matrix[1, 2] = m23;
		_matrix[2, 0] = m31;
		_matrix[2, 1] = m32;
		_matrix[2, 2] = m33;
		_matrix[0, 3] = tx;
		_matrix[1, 3] = ty;
		_matrix[2, 3] = tz;
		_matrix[3, 0] = px;
		_matrix[3, 1] = py;
		_matrix[3, 2] = pz;
		_matrix[3, 3] = 1f;
	}

	public override bool Equals(object obj)
	{
		if (obj is TransformMatrix other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		float num = 0f;
		for (int i = 0; i < 4; i++)
		{
			for (int j = 0; j < 4; j++)
			{
				num += _matrix[i, j] * (float)(4 * i + j);
			}
		}
		return (int)num;
	}
}
