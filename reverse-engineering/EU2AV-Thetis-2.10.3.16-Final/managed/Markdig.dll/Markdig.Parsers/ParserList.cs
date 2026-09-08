using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Markdig.Helpers;

namespace Markdig.Parsers;

public abstract class ParserList<T, TState> : OrderedList<T> where T : notnull, ParserBase<TState>
{
	private readonly CharacterMap<T[]> charMap;

	private readonly T[]? globalParsers;

	public T[]? GlobalParsers => globalParsers;

	public char[] OpeningCharacters => charMap.OpeningCharacters;

	protected ParserList(IEnumerable<T> parsersArg)
		: base(parsersArg)
	{
		Dictionary<char, int> dictionary = new Dictionary<char, int>();
		int num = 0;
		for (int i = 0; i < base.Count; i++)
		{
			T val = base[i];
			if (val == null)
			{
				ThrowHelper.InvalidOperationException("Unexpected null parser found");
			}
			val.Initialize();
			val.Index = i;
			char[] openingCharacters = val.OpeningCharacters;
			if (openingCharacters != null && openingCharacters.Length > 0)
			{
				openingCharacters = val.OpeningCharacters;
				foreach (char key in openingCharacters)
				{
					if (!dictionary.TryAdd(key, 1))
					{
						dictionary[key]++;
					}
				}
			}
			else
			{
				num++;
			}
		}
		if (num > 0)
		{
			globalParsers = new T[num];
		}
		Dictionary<char, T[]> dictionary2 = new Dictionary<char, T[]>();
		using (List<T>.Enumerator enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				T current = enumerator.Current;
				char[] openingCharacters = current.OpeningCharacters;
				if (openingCharacters != null && openingCharacters.Length > 0)
				{
					openingCharacters = current.OpeningCharacters;
					foreach (char key2 in openingCharacters)
					{
						if (!dictionary2.TryGetValue(key2, out var value))
						{
							value = (dictionary2[key2] = new T[dictionary[key2]]);
						}
						int num2 = value.Length - dictionary[key2];
						value[num2] = current;
						dictionary[key2]--;
					}
				}
				else
				{
					globalParsers[globalParsers.Length - num] = current;
					num--;
				}
			}
		}
		charMap = new CharacterMap<T[]>(dictionary2);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public T[]? GetParsersForOpeningCharacter(uint openingChar)
	{
		return charMap[openingChar];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int IndexOfOpeningCharacter(string text, int start, int end)
	{
		return charMap.IndexOfOpeningCharacter(text, start, end);
	}
}
