using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Svg.Pathing;

[TypeConverter(typeof(SvgPathBuilder))]
public sealed class SvgPathSegmentList : IList<SvgPathSegment>, ICollection<SvgPathSegment>, IEnumerable<SvgPathSegment>, IEnumerable, ICloneable
{
	private readonly List<SvgPathSegment> _segments = new List<SvgPathSegment>();

	public ISvgPathElement Owner { get; set; }

	public SvgPathSegment First => _segments[0];

	public SvgPathSegment Last => _segments[_segments.Count - 1];

	public SvgPathSegment this[int index]
	{
		get
		{
			return _segments[index];
		}
		set
		{
			_segments[index] = value;
			Owner?.OnPathUpdated();
		}
	}

	public int Count => _segments.Count;

	public bool IsReadOnly => false;

	public int IndexOf(SvgPathSegment item)
	{
		return _segments.IndexOf(item);
	}

	public void Insert(int index, SvgPathSegment item)
	{
		_segments.Insert(index, item);
		Owner?.OnPathUpdated();
	}

	public void RemoveAt(int index)
	{
		_segments.RemoveAt(index);
		Owner?.OnPathUpdated();
	}

	public void Add(SvgPathSegment item)
	{
		_segments.Add(item);
		Owner?.OnPathUpdated();
	}

	public void Clear()
	{
		_segments.Clear();
	}

	public bool Contains(SvgPathSegment item)
	{
		return _segments.Contains(item);
	}

	public void CopyTo(SvgPathSegment[] array, int arrayIndex)
	{
		_segments.CopyTo(array, arrayIndex);
	}

	public bool Remove(SvgPathSegment item)
	{
		bool num = _segments.Remove(item);
		if (num)
		{
			ISvgPathElement owner = Owner;
			if (owner == null)
			{
				return num;
			}
			owner.OnPathUpdated();
		}
		return num;
	}

	public IEnumerator<SvgPathSegment> GetEnumerator()
	{
		return _segments.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return _segments.GetEnumerator();
	}

	public object Clone()
	{
		SvgPathSegmentList svgPathSegmentList = new SvgPathSegmentList();
		using IEnumerator<SvgPathSegment> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			SvgPathSegment current = enumerator.Current;
			svgPathSegmentList.Add(current.Clone());
		}
		return svgPathSegmentList;
	}

	public override string ToString()
	{
		return string.Join(" ", this.Select((SvgPathSegment p) => p.ToString()));
	}
}
