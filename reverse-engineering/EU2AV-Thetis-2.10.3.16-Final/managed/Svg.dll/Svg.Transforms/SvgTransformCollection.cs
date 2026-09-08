using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Linq;

namespace Svg.Transforms;

[TypeConverter(typeof(SvgTransformConverter))]
public class SvgTransformCollection : List<SvgTransform>, ICloneable
{
	public new SvgTransform this[int i]
	{
		get
		{
			return base[i];
		}
		set
		{
			SvgTransform svgTransform = base[i];
			base[i] = value;
			if (svgTransform != value)
			{
				OnTransformChanged();
			}
		}
	}

	public event EventHandler<AttributeEventArgs> TransformChanged;

	private void AddItem(SvgTransform item)
	{
		base.Add(item);
	}

	public new void Add(SvgTransform item)
	{
		AddItem(item);
		OnTransformChanged();
	}

	public new void AddRange(IEnumerable<SvgTransform> collection)
	{
		base.AddRange(collection);
		OnTransformChanged();
	}

	public new void Remove(SvgTransform item)
	{
		base.Remove(item);
		OnTransformChanged();
	}

	public new void RemoveAt(int index)
	{
		base.RemoveAt(index);
		OnTransformChanged();
	}

	public override bool Equals(object obj)
	{
		if (base.Count == 0 && base.Count == base.Count)
		{
			return true;
		}
		return base.Equals(obj);
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	protected void OnTransformChanged()
	{
		TransformChanged?.Invoke(this, new AttributeEventArgs
		{
			Attribute = "transform",
			Value = Clone()
		});
	}

	public object Clone()
	{
		SvgTransformCollection svgTransformCollection = new SvgTransformCollection();
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			SvgTransform current = enumerator.Current;
			svgTransformCollection.AddItem(current.Clone() as SvgTransform);
		}
		return svgTransformCollection;
	}

	public override string ToString()
	{
		if (base.Count < 1)
		{
			return string.Empty;
		}
		return string.Join(" ", this.Select((SvgTransform t) => t.ToString()).ToArray());
	}

	public Matrix GetMatrix()
	{
		Matrix matrix = new Matrix();
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			using Matrix matrix2 = enumerator.Current.Matrix;
			matrix.Multiply(matrix2);
		}
		return matrix;
	}
}
