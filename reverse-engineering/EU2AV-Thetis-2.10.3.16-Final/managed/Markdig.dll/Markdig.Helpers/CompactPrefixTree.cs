using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Markdig.Helpers;

[ExcludeFromCodeCoverage]
internal sealed class CompactPrefixTree<TValue> : IReadOnlyDictionary<string, TValue>, IReadOnlyCollection<KeyValuePair<string, TValue>>, IEnumerable<KeyValuePair<string, TValue>>, IEnumerable, IReadOnlyList<KeyValuePair<string, TValue>>
{
	internal enum InsertionBehavior : byte
	{
		None,
		OverwriteExisting,
		ThrowOnExisting
	}

	[DebuggerDisplay("{Char}, Child: {ChildChar} at {ChildIndex}, Match: {MatchIndex}, Children: {Children?.Count ?? 0}")]
	private struct Node
	{
		public char Char;

		public char ChildChar;

		public int ChildIndex;

		public int MatchIndex;

		public int Children;
	}

	public struct Enumerator : IEnumerator<KeyValuePair<string, TValue>>, IDisposable, IEnumerator
	{
		private readonly KeyValuePair<string, TValue>[] _matches;

		private int _index;

		public KeyValuePair<string, TValue> Current => _matches[_index];

		object IEnumerator.Current => _matches[_index];

		internal Enumerator(KeyValuePair<string, TValue>[] matches)
		{
			_matches = matches;
			_index = -1;
		}

		public bool MoveNext()
		{
			return ++_index < _matches.Length;
		}

		public void Dispose()
		{
		}

		public void Reset()
		{
			_index = -1;
		}
	}

	private Node[] _tree;

	private static readonly Node[] _emptyTree = new Node[0];

	private KeyValuePair<string, TValue>[] _matches;

	private static readonly KeyValuePair<string, TValue>[] _emptyMatches = new KeyValuePair<string, TValue>[0];

	private int _childrenIndex;

	private int[] _children = _emptyChildren;

	private static readonly int[] _emptyChildren = new int[0];

	private readonly int[] _asciiRootMap = new int[128];

	private Dictionary<char, int> _unicodeRootMap;

	public int TreeSize { get; private set; }

	public int TreeCapacity
	{
		get
		{
			return _tree.Length;
		}
		set
		{
			if (value < TreeSize)
			{
				ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.value, ExceptionReason.SmallCapacity);
			}
			if (value != TreeSize)
			{
				Node[] array = new Node[value];
				if (TreeSize > 0)
				{
					Array.Copy(_tree, 0, array, 0, TreeSize);
				}
				_tree = array;
			}
		}
	}

	public int Count { get; private set; }

	public int Capacity
	{
		get
		{
			return _matches.Length;
		}
		set
		{
			if (value < Count)
			{
				ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.value, ExceptionReason.SmallCapacity);
			}
			if (value != Count)
			{
				KeyValuePair<string, TValue>[] array = new KeyValuePair<string, TValue>[value];
				if (Count > 0)
				{
					Array.Copy(_matches, 0, array, 0, Count);
				}
				_matches = array;
			}
		}
	}

	public int ChildrenCount => _childrenIndex;

	public int ChildrenCapacity
	{
		get
		{
			return _children.Length;
		}
		set
		{
			if (value < _childrenIndex)
			{
				ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.value, ExceptionReason.SmallCapacity);
			}
			if (value != _childrenIndex)
			{
				int[] array = new int[value];
				if (_childrenIndex > 0)
				{
					Array.Copy(_children, 0, array, 0, _childrenIndex);
				}
				for (int i = _childrenIndex + 1; i < array.Length; i += 2)
				{
					array[i] = -1;
				}
				_children = array;
			}
		}
	}

	public KeyValuePair<string, TValue> this[int index]
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			if ((uint)index >= (uint)Count)
			{
				ThrowHelper.ThrowIndexOutOfRangeException();
			}
			return _matches[index];
		}
	}

	public TValue this[string key]
	{
		get
		{
			if (TryMatchExact(key.AsSpan(), out var match))
			{
				return match.Value;
			}
			throw new KeyNotFoundException(key);
		}
		set
		{
			TryInsert(new KeyValuePair<string, TValue>(key, value), InsertionBehavior.OverwriteExisting);
		}
	}

	public KeyValuePair<string, TValue> this[ReadOnlySpan<char> key]
	{
		get
		{
			if (TryMatchExact(key, out var match))
			{
				return match;
			}
			throw new KeyNotFoundException(key.ToString());
		}
	}

	public IEnumerable<string> Keys
	{
		get
		{
			for (int i = 0; i < Count; i++)
			{
				yield return _matches[i].Key;
			}
		}
	}

	public IEnumerable<TValue> Values
	{
		get
		{
			for (int i = 0; i < Count; i++)
			{
				yield return _matches[i].Value;
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void EnsureTreeCapacity(int min)
	{
		if (_tree.Length < min)
		{
			EnsureTreeCapacityRare(min);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void EnsureTreeCapacityRare(int min)
	{
		int num = _tree.Length * 2;
		if ((uint)min > 2147483647u)
		{
			num = int.MaxValue;
		}
		if (num < min)
		{
			num = min;
		}
		TreeCapacity = num;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void EnsureCapacity(int min)
	{
		if (_matches.Length < min)
		{
			EnsureCapacityRare(min);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void EnsureCapacityRare(int min)
	{
		int num = _matches.Length * 2;
		if ((uint)min > 2147483647u)
		{
			num = int.MaxValue;
		}
		if (num < min)
		{
			num = min;
		}
		Capacity = num;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void EnsureChildrenCapacity(int min)
	{
		if (_children.Length < min)
		{
			EnsureChildrenCapacityRare(min);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void EnsureChildrenCapacityRare(int min)
	{
		int num = _children.Length * 2;
		if ((uint)min > 2147483647u)
		{
			num = int.MaxValue;
		}
		if (num < min)
		{
			num = min;
		}
		ChildrenCapacity = num;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool TryGetRoot(char rootChar, out int rootNodeIndex)
	{
		if (rootChar < '\u0080')
		{
			rootNodeIndex = _asciiRootMap[(uint)rootChar];
			return rootNodeIndex != -1;
		}
		if (_unicodeRootMap != null)
		{
			return _unicodeRootMap.TryGetValue(rootChar, out rootNodeIndex);
		}
		rootNodeIndex = -1;
		return false;
	}

	private void SetRootChar(char rootChar)
	{
		if (rootChar < '\u0080')
		{
			_asciiRootMap[(uint)rootChar] = TreeSize;
			return;
		}
		if (_unicodeRootMap == null)
		{
			_unicodeRootMap = new Dictionary<char, int>();
		}
		_unicodeRootMap.Add(rootChar, TreeSize);
	}

	private void Init(int matchCapacity, int treeCapacity, int childrenCapacity)
	{
		for (int i = 0; i < _asciiRootMap.Length; i++)
		{
			_asciiRootMap[i] = -1;
		}
		_matches = ((matchCapacity == 0) ? _emptyMatches : new KeyValuePair<string, TValue>[matchCapacity]);
		_tree = ((treeCapacity == 0) ? _emptyTree : new Node[treeCapacity]);
		EnsureChildrenCapacity(childrenCapacity);
	}

	public CompactPrefixTree(int matchCapacity = 0, int treeCapacity = 0, int childrenCapacity = 0)
	{
		Init(matchCapacity, treeCapacity, childrenCapacity);
	}

	public CompactPrefixTree(ICollection<KeyValuePair<string, TValue>> input)
	{
		if (input == null)
		{
			ThrowHelper.ThrowArgumentNullException(ExceptionArgument.input);
		}
		Init(input.Count, input.Count * 2, input.Count * 2);
		using IEnumerator<KeyValuePair<string, TValue>> enumerator = input.GetEnumerator();
		for (int i = 0; i < input.Count; i++)
		{
			enumerator.MoveNext();
			TryInsert(enumerator.Current, InsertionBehavior.ThrowOnExisting);
		}
	}

	public void Add(string key, TValue value)
	{
		TryInsert(new KeyValuePair<string, TValue>(key, value), InsertionBehavior.ThrowOnExisting);
	}

	public void Add(KeyValuePair<string, TValue> pair)
	{
		TryInsert(in pair, InsertionBehavior.ThrowOnExisting);
	}

	public bool TryAdd(string key, TValue value)
	{
		return TryInsert(new KeyValuePair<string, TValue>(key, value), InsertionBehavior.None);
	}

	public bool TryAdd(KeyValuePair<string, TValue> pair)
	{
		return TryInsert(in pair, InsertionBehavior.None);
	}

	private bool TryInsert(in KeyValuePair<string, TValue> pair, InsertionBehavior behavior)
	{
		string key = pair.Key;
		if (key == null)
		{
			ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key);
		}
		if (key.Length == 0)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.key, ExceptionReason.String_Empty);
		}
		char c = key[0];
		if (TryGetRoot(c, out var rootNodeIndex))
		{
			Node[] tree = _tree;
			ref Node reference = ref tree[rootNodeIndex];
			int i = 1;
			while (true)
			{
				if (i < key.Length)
				{
					char c2 = key[i];
					if (reference.ChildChar == c2)
					{
						reference = ref tree[reference.ChildIndex];
					}
					else
					{
						if (reference.Children == -1)
						{
							if (reference.ChildChar == '\0')
							{
								int matchIndex = reference.MatchIndex;
								string key2 = _matches[matchIndex].Key;
								int num = i;
								int num2;
								for (num2 = Math.Min(key.Length, key2.Length); i < num2 && key[i] == key2[i]; i++)
								{
								}
								if (i == num2 && key.Length == key2.Length)
								{
									break;
								}
								reference.MatchIndex = -1;
								int num3 = i - num;
								if (num3 > 0)
								{
									reference.ChildIndex = TreeSize;
									reference.ChildChar = key[num];
									EnsureTreeCapacity(TreeSize + num3);
									tree = _tree;
									for (int j = 0; j < num3 - 1; j++)
									{
										tree[TreeSize + j] = new Node
										{
											Char = key2[num + j],
											ChildChar = key2[num + j + 1],
											ChildIndex = TreeSize + j + 1,
											MatchIndex = -1,
											Children = -1
										};
									}
									TreeSize += num3;
									tree[TreeSize - 1] = new Node
									{
										Char = key2[num + num3 - 1],
										MatchIndex = -1,
										Children = -1
									};
									reference = ref tree[TreeSize - 1];
								}
								reference.ChildIndex = TreeSize;
								EnsureCapacity(Count + 1);
								_matches[Count] = pair;
								if (i == num2)
								{
									if (key2.Length < key.Length)
									{
										reference.ChildChar = key[i];
										reference.MatchIndex = matchIndex;
										EnsureTreeCapacity(TreeSize + 1);
										_tree[TreeSize] = new Node
										{
											Char = key[i],
											MatchIndex = Count,
											Children = -1
										};
									}
									else
									{
										reference.ChildChar = key2[i];
										reference.MatchIndex = Count;
										EnsureTreeCapacity(TreeSize + 1);
										_tree[TreeSize] = new Node
										{
											Char = key2[i],
											MatchIndex = matchIndex,
											Children = -1
										};
									}
									Count++;
									TreeSize++;
									return true;
								}
								reference.ChildChar = key2[i];
								reference.Children = _childrenIndex;
								EnsureChildrenCapacity(_childrenIndex + 2);
								_children[_childrenIndex] = TreeSize + 1;
								_childrenIndex += 2;
								EnsureTreeCapacity(TreeSize + 2);
								_tree[TreeSize] = new Node
								{
									Char = key2[i],
									MatchIndex = matchIndex,
									Children = -1
								};
								_tree[TreeSize + 1] = new Node
								{
									Char = key[i],
									MatchIndex = Count,
									Children = -1
								};
								Count++;
								TreeSize += 2;
								return true;
							}
							reference.Children = _childrenIndex;
							EnsureChildrenCapacity(_childrenIndex + 2);
							_children[_childrenIndex] = TreeSize;
							_childrenIndex += 2;
							InsertLeafNode(in pair, c2);
							return true;
						}
						int[] children = _children;
						int num4 = reference.Children;
						int num5 = num4;
						while (true)
						{
							if ((uint)num4 < (uint)children.Length)
							{
								reference = ref _tree[children[num4]];
								if (reference.Char == c2)
								{
									break;
								}
								num5 = num4;
								num4 = children[num4 + 1];
								continue;
							}
							EnsureChildrenCapacity(_childrenIndex + 2);
							_children[num5 + 1] = _childrenIndex;
							_children[_childrenIndex] = TreeSize;
							_childrenIndex += 2;
							InsertLeafNode(in pair, c2);
							return true;
						}
					}
					i++;
					continue;
				}
				if (reference.MatchIndex != -1)
				{
					ref KeyValuePair<string, TValue> reference2 = ref _matches[reference.MatchIndex];
					if (reference2.Key.Length == key.Length)
					{
						break;
					}
					int matchIndex2 = reference.MatchIndex;
					reference.MatchIndex = Count;
					reference.ChildChar = reference2.Key[key.Length];
					reference.ChildIndex = TreeSize;
					EnsureTreeCapacity(TreeSize + 1);
					_tree[TreeSize] = new Node
					{
						Char = reference2.Key[key.Length],
						MatchIndex = matchIndex2,
						Children = -1
					};
					TreeSize++;
				}
				reference.MatchIndex = Count;
				EnsureCapacity(Count + 1);
				_matches[Count] = pair;
				Count++;
				return true;
			}
			switch (behavior)
			{
			case InsertionBehavior.None:
				return false;
			case InsertionBehavior.OverwriteExisting:
				_matches[reference.MatchIndex] = pair;
				return true;
			default:
				ThrowHelper.ThrowArgumentException(ExceptionArgument.key, ExceptionReason.DuplicateKey);
				return false;
			}
		}
		SetRootChar(c);
		InsertLeafNode(in pair, c);
		return true;
	}

	private void InsertLeafNode(in KeyValuePair<string, TValue> pair, char nodeChar)
	{
		EnsureCapacity(Count + 1);
		_matches[Count] = pair;
		EnsureTreeCapacity(TreeSize + 1);
		_tree[TreeSize] = new Node
		{
			Char = nodeChar,
			MatchIndex = Count,
			Children = -1
		};
		Count++;
		TreeSize++;
	}

	public bool TryMatchLongest(ReadOnlySpan<char> text, out KeyValuePair<string, TValue> match)
	{
		match = default(KeyValuePair<string, TValue>);
		if (text.Length == 0 || !TryGetRoot(text[0], out var rootNodeIndex))
		{
			return false;
		}
		int num = -1;
		int num2 = 1;
		ref Node reference = ref _tree[rootNodeIndex];
		if (reference.ChildChar == '\0')
		{
			goto IL_00f6;
		}
		if (reference.MatchIndex != -1)
		{
			num = reference.MatchIndex;
		}
		int num3 = 1;
		while (num3 < text.Length)
		{
			char c = text[num3];
			if (reference.ChildChar == c)
			{
				reference = ref _tree[reference.ChildIndex];
				goto IL_00c4;
			}
			int[] children = _children;
			int num4 = reference.Children;
			while ((uint)num4 < (uint)children.Length)
			{
				reference = ref _tree[children[num4]];
				if (reference.Char != c)
				{
					num4 = children[num4 + 1];
					continue;
				}
				goto IL_00c4;
			}
			break;
			IL_00c4:
			num2++;
			if (reference.ChildChar != 0)
			{
				if (reference.MatchIndex != -1)
				{
					num = reference.MatchIndex;
				}
				num3++;
				continue;
			}
			goto IL_00f6;
		}
		goto IL_0142;
		IL_00f6:
		ref KeyValuePair<string, TValue> reference2 = ref _matches[reference.MatchIndex];
		if (reference2.Key.Length <= text.Length && text.Slice(num2).StartsWith(reference2.Key.AsSpan(num2), StringComparison.Ordinal))
		{
			num = reference.MatchIndex;
		}
		goto IL_0142;
		IL_0142:
		if (num != -1)
		{
			match = _matches[num];
			return true;
		}
		return false;
	}

	public bool TryMatchExact(ReadOnlySpan<char> text, out KeyValuePair<string, TValue> match)
	{
		match = default(KeyValuePair<string, TValue>);
		if (text.Length == 0 || !TryGetRoot(text[0], out var rootNodeIndex))
		{
			return false;
		}
		int num = 1;
		ref Node reference = ref _tree[rootNodeIndex];
		if (reference.ChildChar != 0)
		{
			if (reference.MatchIndex != -1 && text.Length == 1)
			{
				match = _matches[reference.MatchIndex];
				return true;
			}
			int num2 = 1;
			while (true)
			{
				if (num2 < text.Length)
				{
					char c = text[num2];
					if (reference.ChildChar == c)
					{
						reference = ref _tree[reference.ChildIndex];
					}
					else
					{
						int[] children = _children;
						int num3 = reference.Children;
						while (true)
						{
							if ((uint)num3 >= (uint)children.Length)
							{
								return false;
							}
							reference = ref _tree[children[num3]];
							if (reference.Char == c)
							{
								break;
							}
							num3 = children[num3 + 1];
						}
					}
					num++;
					if (reference.ChildChar == '\0')
					{
						break;
					}
					num2++;
					continue;
				}
				if (reference.MatchIndex == -1)
				{
					return false;
				}
				match = _matches[reference.MatchIndex];
				return true;
			}
		}
		match = _matches[reference.MatchIndex];
		if (match.Key.Length == text.Length)
		{
			return text.Slice(num).Equals(match.Key.AsSpan(num), StringComparison.Ordinal);
		}
		return false;
	}

	public bool TryMatchShortest(ReadOnlySpan<char> text, out KeyValuePair<string, TValue> match)
	{
		match = default(KeyValuePair<string, TValue>);
		if (text.Length == 0 || !TryGetRoot(text[0], out var rootNodeIndex))
		{
			return false;
		}
		ref Node reference = ref _tree[rootNodeIndex];
		if (reference.MatchIndex != -1)
		{
			match = _matches[reference.MatchIndex];
			return true;
		}
		for (int i = 1; i < text.Length; i++)
		{
			char c = text[i];
			if (reference.ChildChar == c)
			{
				reference = ref _tree[reference.ChildIndex];
			}
			else
			{
				int[] children = _children;
				int num = reference.Children;
				while (true)
				{
					if ((uint)num >= (uint)children.Length)
					{
						return false;
					}
					reference = ref _tree[children[num]];
					if (reference.Char == c)
					{
						break;
					}
					num = children[num + 1];
				}
			}
			if (reference.MatchIndex != -1)
			{
				match = _matches[reference.MatchIndex];
				return true;
			}
		}
		return false;
	}

	public bool ContainsKey(string key)
	{
		KeyValuePair<string, TValue> match;
		return TryMatchExact(key.AsSpan(), out match);
	}

	public bool TryGetValue(string key, out TValue value)
	{
		bool result = TryMatchExact(key.AsSpan(), out var match);
		value = match.Value;
		return result;
	}

	public IEnumerator<KeyValuePair<string, TValue>> GetEnumerator()
	{
		return new Enumerator(_matches);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return new Enumerator(_matches);
	}
}
