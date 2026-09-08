using System.Collections.Generic;

namespace System.Linq;

internal readonly struct Maybe<T>(T value) : IEquatable<Maybe<T>>
{
	public bool HasValue { get; } = true;

	public T Value { get; } = value;

	public bool Equals(Maybe<T> other)
	{
		if (HasValue == other.HasValue)
		{
			return EqualityComparer<T>.Default.Equals(Value, other.Value);
		}
		return false;
	}

	public override bool Equals(object? other)
	{
		if (other is Maybe<T> other2)
		{
			return Equals(other2);
		}
		return false;
	}

	public override int GetHashCode()
	{
		if (!HasValue)
		{
			return 0;
		}
		return EqualityComparer<T>.Default.GetHashCode(Value);
	}

	public static bool operator ==(Maybe<T> first, Maybe<T> second)
	{
		return first.Equals(second);
	}

	public static bool operator !=(Maybe<T> first, Maybe<T> second)
	{
		return !first.Equals(second);
	}
}
