using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection.Internal;

namespace System.Reflection.Metadata.Ecma335;

[DebuggerDisplay("Count = {Count}")]
internal readonly struct BlobDictionary
{
	private readonly Dictionary<int, KeyValuePair<ImmutableArray<byte>, BlobHandle>> _dictionary;

	public int Count => _dictionary.Count;

	private static int GetNextDictionaryKey(int dictionaryKey)
	{
		return dictionaryKey * 747796405 + -1403630843;
	}

	public BlobHandle GetOrAdd(ReadOnlySpan<byte> key, ImmutableArray<byte> immutableKey, BlobHandle value, out bool exists)
	{
		int num = System.Reflection.Internal.Hash.GetFNVHashCode(key);
		KeyValuePair<ImmutableArray<byte>, BlobHandle> value2;
		while ((exists = _dictionary.TryGetValue(num, out value2)) && !value2.Key.AsSpan().SequenceEqual(key))
		{
			num = GetNextDictionaryKey(num);
		}
		if (exists)
		{
			return value2.Value;
		}
		if (immutableKey.IsDefault)
		{
			immutableKey = key.ToImmutableArray();
		}
		_dictionary.Add(num, new KeyValuePair<ImmutableArray<byte>, BlobHandle>(immutableKey, value));
		return value;
	}

	public BlobDictionary(int capacity = 0)
	{
		_dictionary = new Dictionary<int, KeyValuePair<ImmutableArray<byte>, BlobHandle>>(capacity);
	}

	public Dictionary<int, KeyValuePair<ImmutableArray<byte>, BlobHandle>>.Enumerator GetEnumerator()
	{
		return _dictionary.GetEnumerator();
	}
}
