using System.Collections.Generic;

namespace System.Linq;

internal sealed class SingleLinkedNode<TSource>
{
	public TSource Item { get; }

	public System.Linq.SingleLinkedNode<TSource>? Linked { get; }

	public SingleLinkedNode(TSource item)
	{
		Item = item;
	}

	private SingleLinkedNode(System.Linq.SingleLinkedNode<TSource> linked, TSource item)
	{
		Linked = linked;
		Item = item;
	}

	public System.Linq.SingleLinkedNode<TSource> Add(TSource item)
	{
		return new System.Linq.SingleLinkedNode<TSource>(this, item);
	}

	public int GetCount()
	{
		int num = 0;
		for (System.Linq.SingleLinkedNode<TSource> singleLinkedNode = this; singleLinkedNode != null; singleLinkedNode = singleLinkedNode.Linked)
		{
			num++;
		}
		return num;
	}

	public IEnumerator<TSource> GetEnumerator(int count)
	{
		return ((IEnumerable<TSource>)ToArray(count)).GetEnumerator();
	}

	public System.Linq.SingleLinkedNode<TSource> GetNode(int index)
	{
		System.Linq.SingleLinkedNode<TSource> singleLinkedNode = this;
		while (index > 0)
		{
			singleLinkedNode = singleLinkedNode.Linked;
			index--;
		}
		return singleLinkedNode;
	}

	private TSource[] ToArray(int count)
	{
		TSource[] array = new TSource[count];
		int num = count;
		for (System.Linq.SingleLinkedNode<TSource> singleLinkedNode = this; singleLinkedNode != null; singleLinkedNode = singleLinkedNode.Linked)
		{
			num--;
			array[num] = singleLinkedNode.Item;
		}
		return array;
	}
}
