using System.Collections.Generic;
using System.Globalization;

namespace System.Reactive;

[Serializable]
public readonly struct TimeInterval<T>(T value, TimeSpan interval) : IEquatable<TimeInterval<T>>
{
	public T Value { get; } = value;

	public TimeSpan Interval { get; } = interval;

	public void Deconstruct(out T value, out TimeSpan interval)
	{
		T value2 = Value;
		TimeSpan interval2 = Interval;
		value = value2;
		interval = interval2;
	}

	public bool Equals(TimeInterval<T> other)
	{
		if (other.Interval.Equals(Interval))
		{
			return EqualityComparer<T>.Default.Equals(Value, other.Value);
		}
		return false;
	}

	public static bool operator ==(TimeInterval<T> first, TimeInterval<T> second)
	{
		return first.Equals(second);
	}

	public static bool operator !=(TimeInterval<T> first, TimeInterval<T> second)
	{
		return !first.Equals(second);
	}

	public override bool Equals(object? obj)
	{
		if (obj is TimeInterval<T> other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		int hashCode = Interval.GetHashCode();
		T value = Value;
		return hashCode ^ ((value != null) ? value.GetHashCode() : 1963);
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.CurrentCulture, "{0}@{1}", Value, Interval);
	}
}
