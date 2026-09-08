using System.Collections.Generic;
using System.Globalization;

namespace System.Reactive;

[Serializable]
public readonly struct Timestamped<T>(T value, DateTimeOffset timestamp) : IEquatable<Timestamped<T>>
{
	public T Value { get; } = value;

	public DateTimeOffset Timestamp { get; } = timestamp;

	public void Deconstruct(out T value, out DateTimeOffset timestamp)
	{
		T value2 = Value;
		DateTimeOffset timestamp2 = Timestamp;
		value = value2;
		timestamp = timestamp2;
	}

	public bool Equals(Timestamped<T> other)
	{
		if (other.Timestamp.Equals(Timestamp))
		{
			return EqualityComparer<T>.Default.Equals(Value, other.Value);
		}
		return false;
	}

	public static bool operator ==(Timestamped<T> first, Timestamped<T> second)
	{
		return first.Equals(second);
	}

	public static bool operator !=(Timestamped<T> first, Timestamped<T> second)
	{
		return !first.Equals(second);
	}

	public override bool Equals(object? obj)
	{
		if (obj is Timestamped<T> other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		int hashCode = Timestamp.GetHashCode();
		T value = Value;
		return hashCode ^ ((value != null) ? value.GetHashCode() : 1979);
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.CurrentCulture, "{0}@{1}", Value, Timestamp);
	}
}
public static class Timestamped
{
	public static Timestamped<T> Create<T>(T value, DateTimeOffset timestamp)
	{
		return new Timestamped<T>(value, timestamp);
	}
}
