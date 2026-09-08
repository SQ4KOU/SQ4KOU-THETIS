using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal struct RefKindVector : IEquatable<RefKindVector>
{
	private const int BitsPerRefKind = 3;

	private BitVector _bits;

	internal bool IsNull => _bits.IsNull;

	internal int Capacity => _bits.Capacity / 3;

	internal RefKind this[int index]
	{
		get
		{
			index *= 3;
			(bool, bool, bool) tuple = (_bits[index + 2], _bits[index + 1], _bits[index]);
			if (!tuple.Item1)
			{
				if (!tuple.Item2)
				{
					if (!tuple.Item3)
					{
						return RefKind.None;
					}
					return RefKind.Ref;
				}
				if (!tuple.Item3)
				{
					return RefKind.Out;
				}
				return RefKind.In;
			}
			if (!tuple.Item2 && !tuple.Item3)
			{
				return RefKind.RefReadOnlyParameter;
			}
			throw ExceptionUtilities.UnexpectedValue(tuple);
		}
		set
		{
			index *= 3;
			(_bits[index + 2], _bits[index + 1], _bits[index]) = value switch
			{
				RefKind.None => (false, false, false), 
				RefKind.Ref => (false, false, true), 
				RefKind.Out => (false, true, false), 
				RefKind.In => (false, true, true), 
				RefKind.RefReadOnlyParameter => (true, false, false), 
				_ => throw ExceptionUtilities.UnexpectedValue(value), 
			};
		}
	}

	internal static RefKindVector Create(int capacity)
	{
		return new RefKindVector(capacity);
	}

	private RefKindVector(int capacity)
	{
		_bits = BitVector.Create(capacity * 3);
	}

	private RefKindVector(BitVector bits)
	{
		_bits = bits;
	}

	internal IEnumerable<ulong> Words()
	{
		return _bits.Words();
	}

	public bool Equals(RefKindVector other)
	{
		return _bits.Equals(other._bits);
	}

	public override bool Equals(object? obj)
	{
		if (obj is RefKindVector other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return _bits.GetHashCode();
	}

	public string ToRefKindString()
	{
		PooledStringBuilder instance = PooledStringBuilder.GetInstance();
		StringBuilder builder = instance.Builder;
		builder.Append('{');
		int num = 0;
		foreach (ulong item in Words())
		{
			if (num > 0)
			{
				builder.Append(',');
			}
			builder.AppendFormat("{0:x8}", item);
			num++;
		}
		builder.Append('}');
		return instance.ToStringAndFree();
	}

	public static bool TryParse(string refKindString, int capacity, out RefKindVector result)
	{
		ulong? num = null;
		ArrayBuilder<ulong> arrayBuilder = null;
		string[] array = refKindString.Split(new char[1] { ',' });
		foreach (string value in array)
		{
			ulong num2;
			try
			{
				num2 = Convert.ToUInt64(value, 16);
			}
			catch (Exception)
			{
				result = default(RefKindVector);
				return false;
			}
			if (!num.HasValue)
			{
				num = num2;
				continue;
			}
			if (arrayBuilder == null)
			{
				arrayBuilder = ArrayBuilder<ulong>.GetInstance();
			}
			arrayBuilder.Add(num2);
		}
		BitVector bits = BitVector.FromWords(num.Value, arrayBuilder?.ToArrayAndFree() ?? Array.Empty<ulong>(), capacity * 3);
		result = new RefKindVector(bits);
		return true;
	}
}
