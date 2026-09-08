using System;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace Markdig.Helpers;

internal ref struct ValueStringBuilder(Span<char> initialBuffer)
{
	public const int StackallocThreshold = 64;

	private char[]? _arrayToReturnToPool = null;

	private Span<char> _chars = initialBuffer;

	private int _pos = 0;

	public int Length
	{
		get
		{
			return _pos;
		}
		set
		{
			_pos = value;
		}
	}

	public ref char this[int index] => ref _chars[index];

	public override string ToString()
	{
		string result = _chars.Slice(0, _pos).ToString();
		Dispose();
		return result;
	}

	public ReadOnlySpan<char> AsSpan()
	{
		return _chars.Slice(0, _pos);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Append(char c)
	{
		int pos = _pos;
		Span<char> chars = _chars;
		if ((uint)pos < (uint)chars.Length)
		{
			chars[pos] = c;
			_pos = pos + 1;
		}
		else
		{
			GrowAndAppend(c);
		}
	}

	public void Append(char c, int count)
	{
		if (_pos > _chars.Length - count)
		{
			Grow(count);
		}
		Span<char> span = _chars.Slice(_pos, count);
		for (int i = 0; i < span.Length; i++)
		{
			span[i] = c;
		}
		_pos += count;
	}

	public void Append(uint i)
	{
		if (i < 10)
		{
			Append((char)(48 + i));
		}
		else
		{
			Append(i.ToString());
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Append(string s)
	{
		int pos = _pos;
		if (pos > _chars.Length - s.Length)
		{
			Grow(s.Length);
		}
		s.AsSpan().CopyTo(_chars.Slice(pos));
		_pos += s.Length;
	}

	public void Append(ReadOnlySpan<char> value)
	{
		if (_pos > _chars.Length - value.Length)
		{
			Grow(value.Length);
		}
		value.CopyTo(_chars.Slice(_pos));
		_pos += value.Length;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Span<char> AppendSpan(int length)
	{
		int pos = _pos;
		if (pos > _chars.Length - length)
		{
			Grow(length);
		}
		_pos = pos + length;
		return _chars.Slice(pos, length);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void GrowAndAppend(char c)
	{
		Grow(1);
		Append(c);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Grow(int additionalCapacityBeyondPos)
	{
		char[] array = ArrayPool<char>.Shared.Rent((int)Math.Max((uint)(_pos + additionalCapacityBeyondPos), (uint)(_chars.Length * 2)));
		_chars.Slice(0, _pos).CopyTo(array);
		char[] arrayToReturnToPool = _arrayToReturnToPool;
		_chars = (_arrayToReturnToPool = array);
		if (arrayToReturnToPool != null)
		{
			ArrayPool<char>.Shared.Return(arrayToReturnToPool);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Dispose()
	{
		char[] arrayToReturnToPool = _arrayToReturnToPool;
		this = default(ValueStringBuilder);
		if (arrayToReturnToPool != null)
		{
			ArrayPool<char>.Shared.Return(arrayToReturnToPool);
		}
	}
}
