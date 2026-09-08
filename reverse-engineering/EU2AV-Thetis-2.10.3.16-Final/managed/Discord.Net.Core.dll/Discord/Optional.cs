using System;
using System.Diagnostics;

namespace Discord;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public struct Optional<T>
{
	private readonly T _value;

	public static Optional<T> Unspecified => default(Optional<T>);

	public T Value
	{
		get
		{
			if (!IsSpecified)
			{
				throw new InvalidOperationException("This property has no value set.");
			}
			return _value;
		}
	}

	public bool IsSpecified { get; }

	private string DebuggerDisplay
	{
		get
		{
			if (!IsSpecified)
			{
				return "<unspecified>";
			}
			T value = _value;
			return ((value != null) ? value.ToString() : null) ?? "<null>";
		}
	}

	public Optional(T value)
	{
		_value = value;
		IsSpecified = true;
	}

	public T GetValueOrDefault()
	{
		return _value;
	}

	public T GetValueOrDefault(T defaultValue)
	{
		if (!IsSpecified)
		{
			return defaultValue;
		}
		return _value;
	}

	public override bool Equals(object other)
	{
		if (!IsSpecified)
		{
			return other == null;
		}
		if (other == null)
		{
			return false;
		}
		return _value.Equals(other);
	}

	public override int GetHashCode()
	{
		if (!IsSpecified)
		{
			return 0;
		}
		return _value.GetHashCode();
	}

	public override string ToString()
	{
		if (!IsSpecified)
		{
			return null;
		}
		T value = _value;
		if (value == null)
		{
			return null;
		}
		return value.ToString();
	}

	public static implicit operator Optional<T>(T value)
	{
		return new Optional<T>(value);
	}

	public static explicit operator T(Optional<T> value)
	{
		return value.Value;
	}
}
public static class Optional
{
	public static Optional<T> Create<T>()
	{
		return Optional<T>.Unspecified;
	}

	public static Optional<T> Create<T>(T value)
	{
		return new Optional<T>(value);
	}

	public static T? ToNullable<T>(this Optional<T> val) where T : struct
	{
		if (!val.IsSpecified)
		{
			return null;
		}
		return val.Value;
	}
}
