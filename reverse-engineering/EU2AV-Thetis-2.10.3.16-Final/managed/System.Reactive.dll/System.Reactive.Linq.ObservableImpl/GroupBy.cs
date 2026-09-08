using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Subjects;

namespace System.Reactive.Linq.ObservableImpl;

internal sealed class GroupBy<TSource, TKey, TElement> : Producer<IGroupedObservable<TKey, TElement>, GroupBy<TSource, TKey, TElement>._>
{
	internal sealed class @_ : Sink<TSource, IGroupedObservable<TKey, TElement>>
	{
		private readonly Func<TSource, TKey> _keySelector;

		private readonly Func<TSource, TElement> _elementSelector;

		private readonly Grouping<TKey, TElement> _map;

		private RefCountDisposable? _refCountDisposable;

		private Subject<TElement>? _null;

		public _(GroupBy<TSource, TKey, TElement> parent, IObserver<IGroupedObservable<TKey, TElement>> observer)
			: base(observer)
		{
			_keySelector = parent._keySelector;
			_elementSelector = parent._elementSelector;
			if (parent._capacity.HasValue)
			{
				_map = new Grouping<TKey, TElement>(parent._capacity.Value, parent._comparer);
			}
			else
			{
				_map = new Grouping<TKey, TElement>(parent._comparer);
			}
		}

		public override void Run(IObservable<TSource> source)
		{
			SingleAssignmentDisposable singleAssignmentDisposable = new SingleAssignmentDisposable();
			_refCountDisposable = new RefCountDisposable(singleAssignmentDisposable);
			singleAssignmentDisposable.Disposable = source.SubscribeSafe(this);
			SetUpstream(_refCountDisposable);
		}

		public override void OnNext(TSource value)
		{
			TKey val;
			try
			{
				val = _keySelector(value);
			}
			catch (Exception exception)
			{
				Error(exception);
				return;
			}
			bool flag = false;
			Subject<TElement> value2;
			try
			{
				if (val == null)
				{
					if (_null == null)
					{
						_null = new Subject<TElement>();
						flag = true;
					}
					value2 = _null;
				}
				else if (!_map.TryGetValue(val, out value2))
				{
					value2 = new Subject<TElement>();
					_map.Add(val, value2);
					flag = true;
				}
			}
			catch (Exception exception2)
			{
				Error(exception2);
				return;
			}
			if (flag)
			{
				GroupedObservable<TKey, TElement> value3 = new GroupedObservable<TKey, TElement>(val, value2, _refCountDisposable);
				ForwardOnNext(value3);
			}
			TElement value4;
			try
			{
				value4 = _elementSelector(value);
			}
			catch (Exception exception3)
			{
				Error(exception3);
				return;
			}
			value2.OnNext(value4);
		}

		public override void OnError(Exception error)
		{
			Error(error);
		}

		public override void OnCompleted()
		{
			_null?.OnCompleted();
			foreach (Subject<TElement> value in _map.Values)
			{
				value.OnCompleted();
			}
			ForwardOnCompleted();
		}

		private void Error(Exception exception)
		{
			_null?.OnError(exception);
			foreach (Subject<TElement> value in _map.Values)
			{
				value.OnError(exception);
			}
			ForwardOnError(exception);
		}
	}

	private readonly IObservable<TSource> _source;

	private readonly Func<TSource, TKey> _keySelector;

	private readonly Func<TSource, TElement> _elementSelector;

	private readonly int? _capacity;

	private readonly IEqualityComparer<TKey> _comparer;

	public GroupBy(IObservable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, int? capacity, IEqualityComparer<TKey> comparer)
	{
		_source = source;
		_keySelector = keySelector;
		_elementSelector = elementSelector;
		_capacity = capacity;
		_comparer = comparer;
	}

	protected override @_ CreateSink(IObserver<IGroupedObservable<TKey, TElement>> observer)
	{
		return new @_(this, observer);
	}

	protected override void Run(@_ sink)
	{
		sink.Run(_source);
	}
}
