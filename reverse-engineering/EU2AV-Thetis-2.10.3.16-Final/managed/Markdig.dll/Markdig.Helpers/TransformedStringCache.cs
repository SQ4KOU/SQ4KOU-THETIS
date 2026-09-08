using System;
using System.Threading;

namespace Markdig.Helpers;

internal sealed class TransformedStringCache
{
	private struct EntryGroup
	{
		private struct Entry
		{
			public string Input;

			public string Transformed;
		}

		private Entry[]? _entries;

		public string? TryGet(ReadOnlySpan<char> inputSpan)
		{
			Entry[] entries = _entries;
			if (entries != null)
			{
				for (int i = 0; i < entries.Length; i++)
				{
					if (inputSpan.SequenceEqual(entries[i].Input.AsSpan()))
					{
						return entries[i].Transformed;
					}
				}
			}
			return null;
		}

		public void TryAdd(string input, string transformed)
		{
			if (_entries == null)
			{
				Interlocked.CompareExchange(ref _entries, new Entry[8], null);
			}
			if (_entries[7].Input != null)
			{
				return;
			}
			lock (_entries)
			{
				for (int i = 0; i < _entries.Length; i++)
				{
					string input2 = _entries[i].Input;
					if (input2 == null)
					{
						ref Entry reference = ref _entries[i];
						Volatile.Write(ref reference.Transformed, transformed);
						Volatile.Write(ref reference.Input, input);
						break;
					}
					if (input == input2)
					{
						break;
					}
				}
			}
		}
	}

	internal const int InputLengthLimit = 20;

	internal const int MaxEntriesPerCharacter = 8;

	private readonly EntryGroup[] _groups;

	private readonly Func<string, string> _transformation;

	public TransformedStringCache(Func<string, string> transformation)
	{
		_transformation = transformation ?? throw new ArgumentNullException("transformation");
		_groups = new EntryGroup[128];
	}

	public string Get(ReadOnlySpan<char> inputSpan)
	{
		if ((uint)(inputSpan.Length - 1) < 20u)
		{
			int num = inputSpan[0];
			EntryGroup[] groups = _groups;
			if ((uint)num < (uint)groups.Length)
			{
				ref EntryGroup reference = ref groups[num];
				string text = reference.TryGet(inputSpan);
				if (text == null)
				{
					string text2 = inputSpan.ToString();
					text = _transformation(text2);
					reference.TryAdd(text2, text);
				}
				return text;
			}
		}
		return _transformation(inputSpan.ToString());
	}

	public string Get(string input)
	{
		if ((uint)(input.Length - 1) < 20u)
		{
			int num = input[0];
			EntryGroup[] groups = _groups;
			if ((uint)num < (uint)groups.Length)
			{
				ref EntryGroup reference = ref groups[num];
				string text = reference.TryGet(input.AsSpan());
				if (text == null)
				{
					text = _transformation(input);
					reference.TryAdd(input, text);
				}
				return text;
			}
		}
		return _transformation(input);
	}
}
