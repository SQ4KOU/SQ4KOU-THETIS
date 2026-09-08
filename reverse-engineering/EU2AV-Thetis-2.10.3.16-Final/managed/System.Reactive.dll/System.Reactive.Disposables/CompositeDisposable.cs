using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace System.Reactive.Disposables;

public sealed class CompositeDisposable : ICollection<IDisposable>, IEnumerable<IDisposable>, IEnumerable, ICancelable, IDisposable
{
	private sealed class CompositeEnumerator : IEnumerator<IDisposable>, IDisposable, IEnumerator
	{
		private readonly IDisposable?[] _disposables;

		private int _index;

		public IDisposable Current => _disposables[_index];

		object IEnumerator.Current => _disposables[_index];

		public CompositeEnumerator(IDisposable?[] disposables)
		{
			_disposables = disposables;
			_index = -1;
		}

		public void Dispose()
		{
			IDisposable[] disposables = _disposables;
			Array.Clear(disposables, 0, disposables.Length);
		}

		public bool MoveNext()
		{
			IDisposable[] disposables = _disposables;
			int num;
			do
			{
				num = ++_index;
				if (num >= disposables.Length)
				{
					return false;
				}
			}
			while (disposables[num] == null);
			return true;
		}

		public void Reset()
		{
			_index = -1;
		}
	}

	private readonly object _gate = new object();

	private bool _disposed;

	private object _disposables;

	private int _count;

	private const int ShrinkThreshold = 64;

	private const int MaximumLinearSearchThreshold = 1024;

	private const int DefaultCapacity = 16;

	private static readonly CompositeEnumerator EmptyEnumerator = new CompositeEnumerator(Array.Empty<IDisposable>());

	public int Count => Volatile.Read(ref _count);

	public bool IsReadOnly => false;

	public bool IsDisposed => Volatile.Read(ref _disposed);

	public CompositeDisposable()
	{
		_disposables = new List<IDisposable>();
	}

	public CompositeDisposable(int capacity)
	{
		if (capacity < 0)
		{
			throw new ArgumentOutOfRangeException("capacity");
		}
		_disposables = new List<IDisposable>(capacity);
	}

	public CompositeDisposable(params IDisposable[] disposables)
	{
		if (disposables == null)
		{
			throw new ArgumentNullException("disposables");
		}
		(_disposables, _) = ToListOrDictionary(disposables);
		Volatile.Write(ref _count, disposables.Length);
	}

	public CompositeDisposable(IEnumerable<IDisposable> disposables)
	{
		if (disposables == null)
		{
			throw new ArgumentNullException("disposables");
		}
		int value;
		(_disposables, value) = ToListOrDictionary(disposables);
		Volatile.Write(ref _count, value);
	}

	private static (object Collection, int Count) ToListOrDictionary(IEnumerable<IDisposable> disposables)
	{
		int num = ((disposables is IDisposable[] array) ? array.Length : ((!(disposables is ICollection<IDisposable> collection)) ? 16 : collection.Count));
		int num2 = num;
		if (num2 > 1024)
		{
			Dictionary<IDisposable, int> dictionary = new Dictionary<IDisposable, int>(num2);
			int num3 = 0;
			foreach (IDisposable disposable in disposables)
			{
				if (disposable == null)
				{
					throw new ArgumentException(Strings_Core.DISPOSABLES_CANT_CONTAIN_NULL, "disposables");
				}
				dictionary.TryGetValue(disposable, out var value);
				dictionary[disposable] = value + 1;
				num3++;
			}
			return (Collection: dictionary, Count: num3);
		}
		List<IDisposable> list = new List<IDisposable>(num2);
		foreach (IDisposable disposable2 in disposables)
		{
			if (disposable2 == null)
			{
				throw new ArgumentException(Strings_Core.DISPOSABLES_CANT_CONTAIN_NULL, "disposables");
			}
			list.Add(disposable2);
		}
		_ = list.Count;
		_ = 1024;
		return (Collection: list, Count: list.Count);
	}

	public void Add(IDisposable item)
	{
		if (item == null)
		{
			throw new ArgumentNullException("item");
		}
		lock (_gate)
		{
			if (!_disposed)
			{
				if (_disposables is List<IDisposable> list)
				{
					list.Add(item);
					if (list.Count > 1024)
					{
						Dictionary<IDisposable, int> dictionary = new Dictionary<IDisposable, int>(list.Count + list.Count / 4);
						foreach (IDisposable item2 in list)
						{
							if (item2 != null)
							{
								dictionary.TryGetValue(item2, out var value);
								dictionary[item2] = value + 1;
							}
						}
						_disposables = dictionary;
					}
				}
				else
				{
					Dictionary<IDisposable, int> obj = (Dictionary<IDisposable, int>)_disposables;
					obj.TryGetValue(item, out var value2);
					obj[item] = value2 + 1;
				}
				Volatile.Write(ref _count, _count + 1);
				return;
			}
		}
		item.Dispose();
	}

	public bool Remove(IDisposable item)
	{
		if (item == null)
		{
			throw new ArgumentNullException("item");
		}
		lock (_gate)
		{
			if (_disposed)
			{
				return false;
			}
			if (_disposables is List<IDisposable> list)
			{
				int num = list.IndexOf(item);
				if (num < 0)
				{
					return false;
				}
				list[num] = null;
				if (list.Capacity > 64 && _count < list.Capacity / 2)
				{
					List<IDisposable> list2 = new List<IDisposable>(list.Capacity / 2);
					foreach (IDisposable item2 in list)
					{
						if (item2 != null)
						{
							list2.Add(item2);
						}
					}
					_disposables = list2;
				}
			}
			else
			{
				Dictionary<IDisposable, int> dictionary = (Dictionary<IDisposable, int>)_disposables;
				if (!dictionary.TryGetValue(item, out var value))
				{
					return false;
				}
				value--;
				if (value == 0)
				{
					dictionary.Remove(item);
				}
				else
				{
					dictionary[item] = value;
				}
			}
			Volatile.Write(ref _count, _count - 1);
		}
		item.Dispose();
		return true;
	}

	public void Dispose()
	{
		List<IDisposable> list = null;
		Dictionary<IDisposable, int> dictionary = null;
		lock (_gate)
		{
			if (!_disposed)
			{
				list = _disposables as List<IDisposable>;
				dictionary = _disposables as Dictionary<IDisposable, int>;
				_disposables = null;
				Volatile.Write(ref _count, 0);
				Volatile.Write(ref _disposed, value: true);
			}
		}
		if (list != null)
		{
			foreach (IDisposable item in list)
			{
				item?.Dispose();
			}
		}
		if (dictionary == null)
		{
			return;
		}
		foreach (KeyValuePair<IDisposable, int> item2 in dictionary)
		{
			item2.Key.Dispose();
		}
	}

	public void Clear()
	{
		IDisposable[] array;
		lock (_gate)
		{
			if (_disposed)
			{
				return;
			}
			object disposables = _disposables;
			if (disposables is List<IDisposable> list)
			{
				array = list.ToArray();
				list.Clear();
			}
			else
			{
				Dictionary<IDisposable, int> obj = (Dictionary<IDisposable, int>)disposables;
				array = new IDisposable[obj.Count];
				obj.Keys.CopyTo(array, 0);
				obj.Clear();
			}
			Volatile.Write(ref _count, 0);
		}
		IDisposable[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i]?.Dispose();
		}
	}

	public bool Contains(IDisposable item)
	{
		if (item == null)
		{
			throw new ArgumentNullException("item");
		}
		lock (_gate)
		{
			if (_disposed)
			{
				return false;
			}
			object disposables = _disposables;
			return (disposables is List<IDisposable> list) ? list.Contains(item) : ((Dictionary<IDisposable, int>)disposables).ContainsKey(item);
		}
	}

	public void CopyTo(IDisposable[] array, int arrayIndex)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (arrayIndex < 0 || arrayIndex >= array.Length)
		{
			throw new ArgumentOutOfRangeException("arrayIndex");
		}
		lock (_gate)
		{
			if (_disposed)
			{
				return;
			}
			if (arrayIndex + _count > array.Length)
			{
				throw new ArgumentOutOfRangeException("arrayIndex");
			}
			int num = arrayIndex;
			object disposables = _disposables;
			if (disposables is List<IDisposable> list)
			{
				{
					foreach (IDisposable item in list)
					{
						if (item != null)
						{
							array[num++] = item;
						}
					}
					return;
				}
			}
			foreach (KeyValuePair<IDisposable, int> item2 in (Dictionary<IDisposable, int>)disposables)
			{
				for (int i = 0; i < item2.Value; i++)
				{
					array[num++] = item2.Key;
				}
			}
		}
	}

	public IEnumerator<IDisposable> GetEnumerator()
	{
		lock (_gate)
		{
			if (_disposed || _count == 0)
			{
				return EmptyEnumerator;
			}
			object disposables = _disposables;
			return new CompositeEnumerator((disposables is List<IDisposable> list) ? list.ToArray() : ((Dictionary<IDisposable, int>)disposables).Keys.ToArray());
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
