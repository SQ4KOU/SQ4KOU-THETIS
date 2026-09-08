using System.Collections;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace System.Reactive;

[Experimental]
public class ListObservable<T> : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable, IObservable<object>
{
	private readonly IDisposable _subscription;

	private readonly AsyncSubject<object> _subject = new AsyncSubject<object>();

	private readonly List<T> _results = new List<T>();

	public T Value
	{
		get
		{
			Wait();
			if (_results.Count == 0)
			{
				throw new InvalidOperationException(Strings_Linq.NO_ELEMENTS);
			}
			return _results[_results.Count - 1];
		}
	}

	public T this[int index]
	{
		get
		{
			Wait();
			return _results[index];
		}
		set
		{
			Wait();
			_results[index] = value;
		}
	}

	public int Count
	{
		get
		{
			Wait();
			return _results.Count;
		}
	}

	public bool IsReadOnly => false;

	public ListObservable(IObservable<T> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		_subscription = source.Subscribe(_results.Add, _subject.OnError, _subject.OnCompleted);
	}

	private void Wait()
	{
		_subject.DefaultIfEmpty().Wait();
	}

	public int IndexOf(T item)
	{
		Wait();
		return _results.IndexOf(item);
	}

	public void Insert(int index, T item)
	{
		Wait();
		_results.Insert(index, item);
	}

	public void RemoveAt(int index)
	{
		Wait();
		_results.RemoveAt(index);
	}

	public void Add(T item)
	{
		Wait();
		_results.Add(item);
	}

	public void Clear()
	{
		Wait();
		_results.Clear();
	}

	public bool Contains(T item)
	{
		Wait();
		return _results.Contains(item);
	}

	public void CopyTo(T[] array, int arrayIndex)
	{
		Wait();
		_results.CopyTo(array, arrayIndex);
	}

	public bool Remove(T item)
	{
		Wait();
		return _results.Remove(item);
	}

	public IEnumerator<T> GetEnumerator()
	{
		Wait();
		return _results.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public IDisposable Subscribe(IObserver<object> observer)
	{
		if (observer == null)
		{
			throw new ArgumentNullException("observer");
		}
		return StableCompositeDisposable.Create(_subscription, _subject.Subscribe(observer));
	}
}
