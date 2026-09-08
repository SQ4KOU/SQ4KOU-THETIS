using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Markdig.Helpers;

public class OrderedList<T> : List<T> where T : notnull
{
	public OrderedList()
	{
	}

	public OrderedList(IEnumerable<T> collection)
		: base(collection)
	{
	}

	public bool InsertBefore<TItem>(T item) where TItem : T
	{
		if (item == null)
		{
			ThrowHelper.ArgumentNullException_item();
		}
		for (int i = 0; i < base.Count; i++)
		{
			if (base[i] is TItem)
			{
				Insert(i, item);
				return true;
			}
		}
		return false;
	}

	public TItem? Find<TItem>() where TItem : T
	{
		for (int i = 0; i < base.Count; i++)
		{
			if (base[i] is TItem)
			{
				return (TItem)(object)base[i];
			}
		}
		return default(TItem);
	}

	public bool TryFind<TItem>([NotNullWhen(true)] out TItem? item) where TItem : T
	{
		item = this.Find<TItem>();
		return item != null;
	}

	public TItem? FindExact<TItem>() where TItem : T
	{
		for (int i = 0; i < base.Count; i++)
		{
			if (base[i].GetType() == typeof(TItem))
			{
				return (TItem)(object)base[i];
			}
		}
		return default(TItem);
	}

	public void AddIfNotAlready<TItem>() where TItem : class, T, new()
	{
		if (!this.Contains<TItem>())
		{
			Add((T)(object)new TItem());
		}
	}

	public void AddIfNotAlready<TItem>(TItem item) where TItem : T
	{
		if (!this.Contains<TItem>())
		{
			Add((T)(object)item);
		}
	}

	public bool InsertAfter<TItem>(T item) where TItem : T
	{
		if (item == null)
		{
			ThrowHelper.ArgumentNullException_item();
		}
		for (int i = 0; i < base.Count; i++)
		{
			if (base[i] is TItem)
			{
				Insert(i + 1, item);
				return true;
			}
		}
		return false;
	}

	public bool Contains<TItem>() where TItem : T
	{
		for (int i = 0; i < base.Count; i++)
		{
			if (base[i] is TItem)
			{
				return true;
			}
		}
		return false;
	}

	public bool Replace<TItem>(T replacement) where TItem : T
	{
		for (int i = 0; i < base.Count; i++)
		{
			if (base[i] is TItem)
			{
				RemoveAt(i);
				Insert(i, replacement);
				return true;
			}
		}
		return false;
	}

	public bool ReplaceOrAdd<TItem>(T newItem) where TItem : T
	{
		if (this.Replace<TItem>(newItem))
		{
			return true;
		}
		Add(newItem);
		return false;
	}

	public bool TryRemove<TItem>() where TItem : T
	{
		for (int i = 0; i < base.Count; i++)
		{
			if (base[i] is TItem)
			{
				RemoveAt(i);
				return true;
			}
		}
		return false;
	}
}
