namespace Microsoft.CodeAnalysis.CSharp;

internal readonly struct SafeContext
{
	private const uint CallingMethodRaw = 0u;

	private const uint ReturnOnlyRaw = 1u;

	private const uint CurrentMethodRaw = 2u;

	private readonly uint _value;

	public static readonly SafeContext CallingMethod = new SafeContext(0u);

	public static readonly SafeContext ReturnOnly = new SafeContext(1u);

	public static readonly SafeContext CurrentMethod = new SafeContext(2u);

	public static readonly SafeContext Empty = new SafeContext(uint.MaxValue);

	public bool IsCallingMethod => _value == 0;

	public bool IsReturnOnly => _value == 1;

	public bool IsReturnable
	{
		get
		{
			uint value = _value;
			if (value <= 1)
			{
				return true;
			}
			return false;
		}
	}

	private SafeContext(uint value)
	{
		_value = value;
	}

	public SafeContext Narrower()
	{
		return new SafeContext(_value + 1);
	}

	public SafeContext Wider()
	{
		return new SafeContext(_value - 1);
	}

	public bool IsConvertibleTo(SafeContext other)
	{
		return _value <= other._value;
	}

	public SafeContext Intersect(SafeContext other)
	{
		if (!IsConvertibleTo(other))
		{
			return this;
		}
		return other;
	}

	public SafeContext Union(SafeContext other)
	{
		if (!IsConvertibleTo(other))
		{
			return other;
		}
		return this;
	}

	public bool Equals(SafeContext other)
	{
		return _value == other._value;
	}

	public override bool Equals(object? obj)
	{
		if (obj is SafeContext other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (int)_value;
	}

	public static bool operator ==(SafeContext lhs, SafeContext rhs)
	{
		return lhs._value == rhs._value;
	}

	public static bool operator !=(SafeContext lhs, SafeContext rhs)
	{
		return lhs._value != rhs._value;
	}

	public override string ToString()
	{
		return _value switch
		{
			0u => "SafeContext<CallingMethod>", 
			1u => "SafeContext<ReturnOnly>", 
			2u => "SafeContext<CurrentMethod>", 
			_ => $"SafeContext<{_value}>", 
		};
	}
}
