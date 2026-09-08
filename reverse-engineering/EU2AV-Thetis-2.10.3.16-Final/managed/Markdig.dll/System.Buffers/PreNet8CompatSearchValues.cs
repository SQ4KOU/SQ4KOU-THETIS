using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace System.Buffers;

internal sealed class PreNet8CompatSearchValues : SearchValues<char>
{
	private struct BoolVector128
	{
		private unsafe fixed bool _values[128];

		public unsafe readonly bool this[uint c]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				return _values[c];
			}
		}

		public unsafe void Set(char c)
		{
			_values[(uint)c] = true;
		}
	}

	private readonly BoolVector128 _ascii;

	private readonly HashSet<char>? _nonAscii;

	public PreNet8CompatSearchValues(ReadOnlySpan<char> values)
	{
		ReadOnlySpan<char> readOnlySpan = values;
		for (int i = 0; i < readOnlySpan.Length; i++)
		{
			char c = readOnlySpan[i];
			if (c < '\u0080')
			{
				_ascii.Set(c);
				continue;
			}
			if (_nonAscii == null)
			{
				_nonAscii = new HashSet<char>();
			}
			_nonAscii.Add(c);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override bool Contains(char value)
	{
		if (value >= '\u0080')
		{
			return _nonAscii?.Contains(value) ?? false;
		}
		return _ascii[value];
	}

	public override int IndexOfAny(ReadOnlySpan<char> span)
	{
		if (_nonAscii == null)
		{
			for (int i = 0; i < span.Length; i++)
			{
				char c = span[i];
				if (c < '\u0080' && _ascii[c])
				{
					return i;
				}
			}
		}
		else
		{
			for (int j = 0; j < span.Length; j++)
			{
				char c2 = span[j];
				if ((c2 < '\u0080') ? _ascii[c2] : _nonAscii.Contains(c2))
				{
					return j;
				}
			}
		}
		return -1;
	}

	public override int IndexOfAnyExcept(ReadOnlySpan<char> span)
	{
		if (_nonAscii == null)
		{
			for (int i = 0; i < span.Length; i++)
			{
				char c = span[i];
				if (c >= '\u0080' || !_ascii[c])
				{
					return i;
				}
			}
		}
		else
		{
			for (int j = 0; j < span.Length; j++)
			{
				char c2 = span[j];
				if ((c2 < '\u0080') ? (!_ascii[c2]) : (!_nonAscii.Contains(c2)))
				{
					return j;
				}
			}
		}
		return -1;
	}
}
