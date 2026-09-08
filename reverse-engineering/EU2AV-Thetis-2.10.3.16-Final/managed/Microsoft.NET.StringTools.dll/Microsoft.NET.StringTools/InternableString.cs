using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Microsoft.NET.StringTools;

internal ref struct InternableString
{
	public ref struct Enumerator
	{
		private InternableString _string;

		private int _spanIndex;

		private int _charIndex;

		public readonly ref readonly char Current
		{
			get
			{
				if (_spanIndex == -1)
				{
					return ref _string._inlineSpan[_charIndex];
				}
				return ref _string._spans[_spanIndex].Span[_charIndex];
			}
		}

		internal Enumerator(scoped ref InternableString str)
		{
			_string = str;
			_spanIndex = -1;
			_charIndex = -1;
		}

		public bool MoveNext()
		{
			int num = _charIndex + 1;
			if (_spanIndex == -1)
			{
				if (num < _string._inlineSpan.Length)
				{
					_charIndex = num;
					return true;
				}
				_spanIndex = 0;
				num = 0;
			}
			if (_string._spans != null)
			{
				while (_spanIndex < _string._spans.Count)
				{
					if (num < _string._spans[_spanIndex].Length)
					{
						_charIndex = num;
						return true;
					}
					_spanIndex++;
					num = 0;
				}
			}
			return false;
		}
	}

	private readonly ReadOnlySpan<char> _inlineSpan;

	private List<ReadOnlyMemory<char>>? _spans;

	public int Length { get; private set; }

	internal InternableString(ReadOnlySpan<char> span)
	{
		_inlineSpan = span;
		_spans = null;
		Length = span.Length;
	}

	internal InternableString(string str)
	{
		if (str == null)
		{
			throw new ArgumentNullException("str");
		}
		_inlineSpan = str.AsSpan();
		_spans = null;
		Length = str.Length;
	}

	internal InternableString(SpanBasedStringBuilder stringBuilder)
	{
		_inlineSpan = default(ReadOnlySpan<char>);
		_spans = stringBuilder.Spans;
		Length = stringBuilder.Length;
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(ref this);
	}

	public readonly bool Equals(string other)
	{
		if (other.Length != Length)
		{
			return false;
		}
		if (_inlineSpan.SequenceCompareTo(other.AsSpan(0, _inlineSpan.Length)) != 0)
		{
			return false;
		}
		if (_spans != null)
		{
			int num = _inlineSpan.Length;
			foreach (ReadOnlyMemory<char> span in _spans)
			{
				if (span.Span.SequenceCompareTo(other.AsSpan(num, span.Length)) != 0)
				{
					return false;
				}
				num += span.Length;
			}
		}
		return true;
	}

	public unsafe readonly string ExpensiveConvertToString()
	{
		if (Length == 0)
		{
			return string.Empty;
		}
		if (_inlineSpan.Length == Length)
		{
			return _inlineSpan.ToString();
		}
		if (_inlineSpan.IsEmpty && _spans?[0].Length == Length)
		{
			return _spans[0].ToString();
		}
		string text = new string('\0', Length);
		fixed (char* ptr = text)
		{
			char* ptr2 = ptr;
			if (!_inlineSpan.IsEmpty)
			{
				fixed (char* source = _inlineSpan)
				{
					Unsafe.CopyBlockUnaligned(ptr2, source, (uint)(2 * _inlineSpan.Length));
				}
				ptr2 += _inlineSpan.Length;
			}
			if (_spans != null)
			{
				foreach (ReadOnlyMemory<char> span in _spans)
				{
					if (span.IsEmpty)
					{
						continue;
					}
					fixed (char* source2 = span.Span)
					{
						Unsafe.CopyBlockUnaligned(ptr2, source2, (uint)(2 * span.Length));
					}
					ptr2 += span.Length;
				}
			}
			if (ptr2 != ptr + Length)
			{
				throw new InvalidOperationException($"Length of {Length} does not match the sum of span lengths of {ptr2 - ptr}.");
			}
		}
		return text;
	}

	public readonly bool ReferenceEquals(string str)
	{
		if (_inlineSpan.Length == Length)
		{
			return _inlineSpan == str.AsSpan();
		}
		if (_inlineSpan.IsEmpty)
		{
			List<ReadOnlyMemory<char>>? spans = _spans;
			if (spans != null && spans.Count == 1 && _spans[0].Length == Length)
			{
				return _spans[0].Span == str.AsSpan();
			}
		}
		return false;
	}

	public override string ToString()
	{
		return WeakStringCacheInterner.Instance.InternableToString(ref this);
	}

	public unsafe override readonly int GetHashCode()
	{
		uint hash = 352654597u;
		bool hashedOddNumberOfCharacters = false;
		fixed (char* charPtr = _inlineSpan)
		{
			hash = GetHashCodeHelper(charPtr, _inlineSpan.Length, hash, ref hashedOddNumberOfCharacters);
		}
		if (_spans != null)
		{
			foreach (ReadOnlyMemory<char> span in _spans)
			{
				fixed (char* charPtr = span.Span)
				{
					hash = GetHashCodeHelper(charPtr, span.Length, hash, ref hashedOddNumberOfCharacters);
				}
			}
		}
		return (int)hash;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private unsafe static uint GetHashCodeHelper(char* charPtr, int length, uint hash, ref bool hashedOddNumberOfCharacters)
	{
		if (hashedOddNumberOfCharacters && length > 0)
		{
			hash ^= (BitConverter.IsLittleEndian ? ((uint)(*charPtr) << 16) : (*charPtr));
			length--;
			charPtr++;
			hashedOddNumberOfCharacters = false;
		}
		uint* ptr = (uint*)charPtr;
		while (length >= 2)
		{
			length -= 2;
			hash = (RotateLeft(hash, 5) + hash) ^ *ptr;
			ptr++;
		}
		if (length > 0)
		{
			hash = (RotateLeft(hash, 5) + hash) ^ (uint)(BitConverter.IsLittleEndian ? (*(ushort*)ptr) : (*(ushort*)ptr << 16));
			hashedOddNumberOfCharacters = true;
		}
		return hash;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static uint RotateLeft(uint value, int offset)
	{
		return (value << offset) | (value >> 32 - offset);
	}
}
