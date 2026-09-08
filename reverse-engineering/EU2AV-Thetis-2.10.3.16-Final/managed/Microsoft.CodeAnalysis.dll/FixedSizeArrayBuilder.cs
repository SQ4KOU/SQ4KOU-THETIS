using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.PooledObjects;

[NonCopyable]
internal struct FixedSizeArrayBuilder<T>(int capacity)
{
	private T[] _values = new T[capacity];

	private int _index = 0;

	public void Add(T value)
	{
		_values[_index++] = value;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void ThrowIfTrue([DoesNotReturnIf(true)] bool condition, [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string? filePath = null)
	{
		if (condition)
		{
			string arg = ((filePath == null) ? null : Path.GetFileName(filePath));
			throw new InvalidOperationException($"Unexpected true - file {arg} line {lineNumber}");
		}
	}

	public void AddRange(ImmutableArray<T> values)
	{
		ThrowIfTrue(_index + values.Length > _values.Length, 72, "/_/src/Dependencies/Collections/Extensions/FixedSizeArrayBuilder.cs");
		Array.Copy(ImmutableCollectionsMarshal.AsArray(values), 0, _values, _index, values.Length);
		_index += values.Length;
	}

	public void AddRange(List<T> values)
	{
		ThrowIfTrue(_index + values.Count > _values.Length, 79, "/_/src/Dependencies/Collections/Extensions/FixedSizeArrayBuilder.cs");
		foreach (T value in values)
		{
			Add(value);
		}
	}

	public void AddRange(HashSet<T> values)
	{
		ThrowIfTrue(_index + values.Count > _values.Length, 86, "/_/src/Dependencies/Collections/Extensions/FixedSizeArrayBuilder.cs");
		foreach (T value in values)
		{
			Add(value);
		}
	}

	public void AddRange(ArrayBuilder<T> values)
	{
		ThrowIfTrue(_index + values.Count > _values.Length, 93, "/_/src/Dependencies/Collections/Extensions/FixedSizeArrayBuilder.cs");
		foreach (T value in values)
		{
			Add(value);
		}
	}

	public void AddRange(IEnumerable<T> values)
	{
		foreach (T value in values)
		{
			Add(value);
		}
	}

	public readonly void Sort()
	{
		Sort(Comparer<T>.Default);
	}

	public readonly void Sort(IComparer<T> comparer)
	{
		if (_index > 1)
		{
			Array.Sort(_values, 0, _index, comparer);
		}
	}

	public ImmutableArray<T> MoveToImmutable()
	{
		return ImmutableCollectionsMarshal.AsImmutableArray(MoveToArray());
	}

	public T[] MoveToArray()
	{
		ThrowIfTrue(_index != _values.Length, 127, "/_/src/Dependencies/Collections/Extensions/FixedSizeArrayBuilder.cs");
		T[] values = _values;
		_values = Array.Empty<T>();
		_index = 0;
		return values;
	}
}
