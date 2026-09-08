using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis;

internal static class TopologicalSort
{
	public static bool TryIterativeSort<TNode>(TNode node, TopologicalSortAddSuccessors<TNode> addSuccessors, out ImmutableArray<TNode> result) where TNode : notnull
	{
		return TryIterativeSort(SpecializedCollections.SingletonEnumerable(node), addSuccessors, out result);
	}

	public static bool TryIterativeSort<TNode>(IEnumerable<TNode> nodes, TopologicalSortAddSuccessors<TNode> addSuccessors, out ImmutableArray<TNode> result) where TNode : notnull
	{
		PooledDictionary<TNode, int> pooledDictionary = PredecessorCounts(nodes, addSuccessors, out var allNodes);
		using TemporaryArray<TNode> array = TemporaryArray<TNode>.Empty;
		ArrayBuilder<TNode> instance = ArrayBuilder<TNode>.GetInstance();
		foreach (TNode item in allNodes)
		{
			if (pooledDictionary[item] == 0)
			{
				instance.Push(item);
			}
		}
		ArrayBuilder<TNode> instance2 = ArrayBuilder<TNode>.GetInstance();
		while (instance.Count != 0)
		{
			TNode val = instance.Pop();
			instance2.Add(val);
			array.Clear();
			addSuccessors(ref TemporaryArrayExtensions.AsRef(in array), val);
			foreach (TNode item2 in array)
			{
				if (pooledDictionary[item2]-- == 1)
				{
					instance.Push(item2);
				}
			}
		}
		bool flag = pooledDictionary.Count != instance2.Count;
		result = (flag ? ImmutableArray<TNode>.Empty : instance2.ToImmutable());
		pooledDictionary.Free();
		instance.Free();
		instance2.Free();
		return !flag;
	}

	private static PooledDictionary<TNode, int> PredecessorCounts<TNode>(IEnumerable<TNode> nodes, TopologicalSortAddSuccessors<TNode> addSuccessors, out ImmutableArray<TNode> allNodes) where TNode : notnull
	{
		PooledDictionary<TNode, int> instance = PooledDictionary<TNode, int>.GetInstance();
		PooledHashSet<TNode> instance2 = PooledHashSet<TNode>.GetInstance();
		ArrayBuilder<TNode> instance3 = ArrayBuilder<TNode>.GetInstance();
		ArrayBuilder<TNode> instance4 = ArrayBuilder<TNode>.GetInstance();
		using TemporaryArray<TNode> array = TemporaryArray<TNode>.Empty;
		instance3.AddRange(nodes);
		while (instance3.Count != 0)
		{
			TNode val = instance3.Pop();
			if (!instance2.Add(val))
			{
				continue;
			}
			instance4.Add(val);
			if (!instance.ContainsKey(val))
			{
				instance.Add(val, 0);
			}
			array.Clear();
			addSuccessors(ref TemporaryArrayExtensions.AsRef(in array), val);
			foreach (TNode item in array)
			{
				instance3.Push(item);
				if (instance.TryGetValue(item, out var value))
				{
					instance[item] = value + 1;
				}
				else
				{
					instance.Add(item, 1);
				}
			}
		}
		instance2.Free();
		instance3.Free();
		allNodes = instance4.ToImmutableAndFree();
		return instance;
	}
}
