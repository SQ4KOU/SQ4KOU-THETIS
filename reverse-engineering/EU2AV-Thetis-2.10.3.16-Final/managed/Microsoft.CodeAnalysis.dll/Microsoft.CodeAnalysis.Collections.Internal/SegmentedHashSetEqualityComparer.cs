using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.CodeAnalysis.Collections.Internal;

internal sealed class SegmentedHashSetEqualityComparer<T> : IEqualityComparer<SegmentedHashSet<T>?>
{
	public bool Equals(SegmentedHashSet<T>? x, SegmentedHashSet<T>? y)
	{
		if (x == y)
		{
			return true;
		}
		if (x == null || y == null)
		{
			return false;
		}
		EqualityComparer<T> equalityComparer = EqualityComparer<T>.Default;
		if (SegmentedHashSet<T>.EqualityComparersAreEqual(x, y))
		{
			if (x.Count == y.Count)
			{
				return y.IsSubsetOfHashSetWithSameComparer(x);
			}
			return false;
		}
		foreach (T item in y)
		{
			bool flag = false;
			foreach (T item2 in x)
			{
				if (equalityComparer.Equals(item, item2))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return false;
			}
		}
		return true;
	}

	public int GetHashCode(SegmentedHashSet<T>? obj)
	{
		int num = 0;
		if (obj != null)
		{
			foreach (T item in obj)
			{
				if (item != null)
				{
					num ^= item.GetHashCode();
				}
			}
		}
		return num;
	}

	public override bool Equals([NotNullWhen(true)] object? obj)
	{
		return obj is SegmentedHashSetEqualityComparer<T>;
	}

	public override int GetHashCode()
	{
		return EqualityComparer<T>.Default.GetHashCode();
	}
}
