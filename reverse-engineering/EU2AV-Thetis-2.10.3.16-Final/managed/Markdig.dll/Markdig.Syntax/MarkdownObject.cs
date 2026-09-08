using System;
using System.Runtime.CompilerServices;
using Markdig.Helpers;

namespace Markdig.Syntax;

public abstract class MarkdownObject : IMarkdownObject
{
	private sealed class DataEntriesAndTrivia
	{
		private struct DataEntry(object key, object value)
		{
			public readonly object Key = key;

			public object Value = value;
		}

		private DataEntry[]? _entries;

		private int _count;

		public object? Trivia;

		public void SetData(object key, object value)
		{
			if (key == null)
			{
				ThrowHelper.ArgumentNullException_key();
			}
			DataEntry[] entries = _entries;
			int count = _count;
			if (entries == null)
			{
				_entries = new DataEntry[2];
			}
			else
			{
				for (int i = 0; i < entries.Length && i < count; i++)
				{
					ref DataEntry reference = ref entries[i];
					if (reference.Key == key)
					{
						reference.Value = value;
						return;
					}
				}
				if (count == entries.Length)
				{
					Array.Resize(ref _entries, count + 2);
				}
			}
			_entries[count] = new DataEntry(key, value);
			_count++;
		}

		public object? GetData(object key)
		{
			if (key == null)
			{
				ThrowHelper.ArgumentNullException_key();
			}
			DataEntry[] entries = _entries;
			if (entries == null)
			{
				return null;
			}
			int count = _count;
			for (int i = 0; i < entries.Length && i < count; i++)
			{
				ref DataEntry reference = ref entries[i];
				if (reference.Key == key)
				{
					return reference.Value;
				}
			}
			return null;
		}

		public bool ContainsData(object key)
		{
			if (key == null)
			{
				ThrowHelper.ArgumentNullException_key();
			}
			DataEntry[] entries = _entries;
			if (entries == null)
			{
				return false;
			}
			int count = _count;
			for (int i = 0; i < entries.Length && i < count; i++)
			{
				if (entries[i].Key == key)
				{
					return true;
				}
			}
			return false;
		}

		public bool RemoveData(object key)
		{
			if (key == null)
			{
				ThrowHelper.ArgumentNullException_key();
			}
			DataEntry[] entries = _entries;
			if (entries == null)
			{
				return false;
			}
			int count = _count;
			for (int i = 0; i < entries.Length && i < count; i++)
			{
				if (entries[i].Key == key)
				{
					if (i < count - 1)
					{
						Array.Copy(entries, i + 1, entries, i, count - i - 1);
					}
					count--;
					entries[count] = default(DataEntry);
					_count = count;
					return true;
				}
			}
			return false;
		}
	}

	private const uint ValueBitMask = 1073741823u;

	private const uint FirstBitMask = 2147483648u;

	private const uint SecondBitMask = 1073741824u;

	private const uint IsInlineMask = 2147483648u;

	private const uint IsContainerMask = 1073741824u;

	private const uint TypeKindMask = 3221225472u;

	private uint _lineBits;

	private uint _columnBits;

	private DataEntriesAndTrivia? _attachedDatas;

	public SourceSpan Span;

	internal bool IsContainerInline => (_lineBits & 0xC0000000u) == 3221225472u;

	internal bool IsContainerBlock => (_lineBits & 0xC0000000u) == 1073741824;

	internal bool IsContainer => (_lineBits & 0x40000000) != 0;

	internal bool IsInline => (_lineBits & 0x80000000u) != 0;

	private protected bool IsClosedInternal
	{
		get
		{
			return (_columnBits & 0x80000000u) != 0;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			if (value)
			{
				_columnBits |= 2147483648u;
			}
			else
			{
				_columnBits &= 2147483647u;
			}
		}
	}

	private protected bool InternalSpareBit
	{
		get
		{
			return (_columnBits & 0x40000000) != 0;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			if (value)
			{
				_columnBits |= 1073741824u;
			}
			else
			{
				_columnBits &= 3221225471u;
			}
		}
	}

	public int Column
	{
		get
		{
			return (int)(_columnBits & 0x3FFFFFFF);
		}
		set
		{
			_columnBits = (_columnBits & 0xC0000000u) | (uint)(value & 0x3FFFFFFF);
		}
	}

	public int Line
	{
		get
		{
			return (int)(_lineBits & 0x3FFFFFFF);
		}
		set
		{
			_lineBits = (_lineBits & 0xC0000000u) | (uint)(value & 0x3FFFFFFF);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private protected void SetTypeKind(bool isInline, bool isContainer)
	{
		_lineBits = (uint)((isInline ? int.MinValue : 0) | (isContainer ? 1073741824 : 0));
	}

	protected MarkdownObject()
	{
		Span = SourceSpan.Empty;
	}

	public string ToPositionText()
	{
		return $"${Line}, {Column}, {Span.Start}-{Span.End}";
	}

	public void SetData(object key, object value)
	{
		(_attachedDatas ?? (_attachedDatas = new DataEntriesAndTrivia())).SetData(key, value);
	}

	public bool ContainsData(object key)
	{
		return _attachedDatas?.ContainsData(key) ?? false;
	}

	public object? GetData(object key)
	{
		return _attachedDatas?.GetData(key);
	}

	public bool RemoveData(object key)
	{
		return _attachedDatas?.RemoveData(key) ?? false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private protected T? GetTrivia<T>() where T : class
	{
		return Unsafe.As<T>(_attachedDatas?.Trivia);
	}

	private protected T GetOrSetTrivia<T>() where T : class, new()
	{
		DataEntriesAndTrivia? obj = _attachedDatas ?? (_attachedDatas = new DataEntriesAndTrivia());
		DataEntriesAndTrivia dataEntriesAndTrivia = obj;
		if (dataEntriesAndTrivia.Trivia == null)
		{
			dataEntriesAndTrivia.Trivia = new T();
		}
		return Unsafe.As<T>(obj.Trivia);
	}
}
