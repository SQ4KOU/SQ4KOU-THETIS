using System;
using System.Drawing.Drawing2D;

namespace Svg.Transforms;

public abstract class SvgTransform : ICloneable
{
	public abstract Matrix Matrix { get; }

	public abstract string WriteToString();

	public abstract object Clone();

	public override string ToString()
	{
		return WriteToString();
	}

	public override bool Equals(object obj)
	{
		SvgTransform svgTransform = obj as SvgTransform;
		if (svgTransform == null)
		{
			return false;
		}
		return Matrix.Equals(svgTransform.Matrix);
	}

	public override int GetHashCode()
	{
		return Matrix.GetHashCode();
	}

	public static bool operator ==(SvgTransform lhs, SvgTransform rhs)
	{
		if ((object)lhs == rhs)
		{
			return true;
		}
		if ((object)lhs == null || (object)rhs == null)
		{
			return false;
		}
		return lhs.Equals(rhs);
	}

	public static bool operator !=(SvgTransform lhs, SvgTransform rhs)
	{
		return !(lhs == rhs);
	}
}
