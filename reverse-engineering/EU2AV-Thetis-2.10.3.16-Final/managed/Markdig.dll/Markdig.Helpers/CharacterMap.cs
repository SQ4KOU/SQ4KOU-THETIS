using System;
using System.Buffers;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Markdig.Helpers;

public sealed class CharacterMap<T> where T : class
{
	private readonly SearchValues<char> _values;

	private readonly T[] _asciiMap;

	private readonly FrozenDictionary<uint, T>? _nonAsciiMap;

	public char[] OpeningCharacters { get; }

	public T? this[uint openingChar]
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			T[] asciiMap = _asciiMap;
			if (openingChar < (uint)asciiMap.Length)
			{
				return asciiMap[openingChar];
			}
			T value = null;
			_nonAsciiMap?.TryGetValue(openingChar, out value);
			return value;
		}
	}

	public CharacterMap(IEnumerable<KeyValuePair<char, T>> maps)
	{
		if (maps == null)
		{
			ThrowHelper.ArgumentNullException("maps");
		}
		HashSet<char> hashSet = new HashSet<char>();
		foreach (KeyValuePair<char, T> map in maps)
		{
			hashSet.Add(map.Key);
		}
		OpeningCharacters = hashSet.ToArray();
		Array.Sort(OpeningCharacters);
		_asciiMap = new T[128];
		Dictionary<uint, T> dictionary = null;
		foreach (KeyValuePair<char, T> map2 in maps)
		{
			char key = map2.Key;
			if (key < '\u0080')
			{
				T[] asciiMap = _asciiMap;
				int num = key;
				if (asciiMap[num] == null)
				{
					asciiMap[num] = map2.Value;
				}
			}
			else
			{
				if (dictionary == null)
				{
					dictionary = new Dictionary<uint, T>();
				}
				dictionary.TryAdd(key, map2.Value);
			}
		}
		_values = SearchValues.Create(OpeningCharacters);
		if (dictionary != null)
		{
			_nonAsciiMap = dictionary.ToFrozenDictionary();
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int IndexOfOpeningCharacter(string text, int start, int end)
	{
		int num = text.AsSpan(start, end - start + 1).IndexOfAny(_values);
		if (num >= 0)
		{
			num += start;
		}
		return num;
	}
}
