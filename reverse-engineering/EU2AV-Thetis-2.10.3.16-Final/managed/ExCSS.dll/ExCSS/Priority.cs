using System;
using System.Runtime.InteropServices;

namespace ExCSS;

[StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode, Pack = 1)]
public struct Priority : IEquatable<Priority>, IComparable<Priority>
{
	[FieldOffset(0)]
	private readonly uint _priority;

	public static readonly Priority Zero = new Priority(0u);

	public static readonly Priority OneTag = new Priority(0, 0, 0, 1);

	public static readonly Priority OneClass = new Priority(0, 0, 1, 0);

	public static readonly Priority OneId = new Priority(0, 1, 0, 0);

	public static readonly Priority Inline = new Priority(1, 0, 0, 0);

	[field: FieldOffset(2)]
	public byte Ids { get; }

	[field: FieldOffset(0)]
	public byte Tags { get; }

	[field: FieldOffset(1)]
	public byte Classes { get; }

	[field: FieldOffset(3)]
	public byte Inlines { get; }

	public Priority(uint priority)
	{
		Inlines = (Ids = (Classes = (Tags = 0)));
		_priority = priority;
	}

	public Priority(byte inlines, byte ids, byte classes, byte tags)
	{
		_priority = 0u;
		Inlines = inlines;
		Ids = ids;
		Classes = classes;
		Tags = tags;
	}

	public static Priority operator +(Priority a, Priority b)
	{
		return new Priority(a._priority + b._priority);
	}

	public static bool operator ==(Priority a, Priority b)
	{
		return a._priority == b._priority;
	}

	public static bool operator >(Priority a, Priority b)
	{
		return a._priority > b._priority;
	}

	public static bool operator >=(Priority a, Priority b)
	{
		return a._priority >= b._priority;
	}

	public static bool operator <(Priority a, Priority b)
	{
		return a._priority < b._priority;
	}

	public static bool operator <=(Priority a, Priority b)
	{
		return a._priority <= b._priority;
	}

	public static bool operator !=(Priority a, Priority b)
	{
		return a._priority != b._priority;
	}

	public bool Equals(Priority other)
	{
		return _priority == other._priority;
	}

	public override bool Equals(object obj)
	{
		if (obj is Priority other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (int)_priority;
	}

	public int CompareTo(Priority other)
	{
		if (!(this == other))
		{
			if (!(this > other))
			{
				return -1;
			}
			return 1;
		}
		return 0;
	}

	public override string ToString()
	{
		return $"({Inlines}, {Ids}, {Classes}, {Tags})";
	}
}
