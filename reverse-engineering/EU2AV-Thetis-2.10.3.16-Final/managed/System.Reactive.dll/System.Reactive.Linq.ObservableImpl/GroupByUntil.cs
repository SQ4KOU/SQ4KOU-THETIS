using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Subjects;

namespace System.Reactive.Linq.ObservableImpl;

internal sealed class GroupByUntil<TSource, TKey, TElement, TDuration> : Producer<IGroupedObservable<TKey, TElement>, GroupByUntil<TSource, TKey, TElement, TDuration>._>
{
	internal sealed class @_ : Sink<TSource, IGroupedObservable<TKey, TElement>>
	{
		private sealed class DurationObserver : SafeObserver<TDuration>
		{
			private readonly @_ _parent;

			private readonly TKey _key;

			private readonly ISubject<TElement> _writer;

			public DurationObserver(@_ parent, TKey key, ISubject<TElement> writer)
			{
				_parent = parent;
				_key = key;
				_writer = writer;
			}

			public override void OnNext(TDuration value)
			{
				OnCompleted();
			}

			public override void OnError(Exception error)
			{
				_parent.Error(error);
				Dispose();
			}

			public override void OnCompleted()
			{
				if (_key == null)
				{
					ISubject<TElement> subject;
					lock (_parent._nullGate)
					{
						subject = _parent._null;
						_parent._null = null;
					}
					subject?.OnCompleted();
				}
				else if (_parent._map.Remove(_key))
				{
					_writer.OnCompleted();
				}
				_parent._groupDisposable.Remove(this);
			}
		}

		private readonly object _gate = new object();

		private readonly object _nullGate = new object();

		private readonly CompositeDisposable _groupDisposable = new CompositeDisposable();

		private readonly RefCountDisposable _refCountDisposable;

		private readonly Map<TKey, ISubject<TElement>> _map;

		private readonly Func<TSource, TKey> _keySelector;

		private readonly Func<TSource, TElement> _elementSelector;

		private readonly Func<IGroupedObservable<TKey, TElement>, IObservable<TDuration>> _durationSelector;

		private ISubject<TElement>? _null;

		public _(GroupByUntil<TSource, TKey, TElement, TDuration> parent, IObserver<IGroupedObservable<TKey, TElement>> observer)
			: base(observer)
		{
			_refCountDisposable = new RefCountDisposable(_groupDisposable);
			_map = new Map<TKey, ISubject<TElement>>(parent._capacity, parent._comparer);
			_keySelector = parent._keySelector;
			_elementSelector = parent._elementSelector;
			_durationSelector = parent._durationSelector;
		}

		public override void Run(IObservable<TSource> source)
		{
			_groupDisposable.Add(source.SubscribeSafe(this));
			SetUpstream(_refCountDisposable);
		}

		private ISubject<TElement> NewSubject()
		{
			Subject<TElement> subject = new Subject<TElement>();
			return Subject.Create<TElement>((IObserver<TElement>)new AsyncLockObserver<TElement>(subject, new AsyncLock()), (IObservable<TElement>)subject);
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
			bool added = false;
			ISubject<TElement> subject;
			try
			{
				if (val == null)
				{
					lock (_nullGate)
					{
						if (_null == null)
						{
							_null = NewSubject();
							added = true;
						}
						subject = _null;
					}
				}
				else
				{
					subject = _map.GetOrAdd(val, NewSubject, out added);
				}
			}
			catch (Exception exception2)
			{
				Error(exception2);
				return;
			}
			if (added)
			{
				GroupedObservable<TKey, TElement> value2 = new GroupedObservable<TKey, TElement>(val, subject, _refCountDisposable);
				GroupedObservable<TKey, TElement> arg = new GroupedObservable<TKey, TElement>(val, subject);
				IObservable<TDuration> source;
				try
				{
					source = _durationSelector(arg);
				}
				catch (Exception exception3)
				{
					Error(exception3);
					return;
				}
				lock (_gate)
				{
					ForwardOnNext(value2);
				}
				DurationObserver durationObserver = new DurationObserver(this, val, subject);
				_groupDisposable.Add(durationObserver);
				durationObserver.SetResource(source.SubscribeSafe(durationObserver));
			}
			TElement value3;
			try
			{
				value3 = _elementSelector(value);
			}
			catch (Exception exception4)
			{
				Error(exception4);
				return;
			}
			subject.OnNext(value3);
		}

		public override void OnError(Exception error)
		{
			Error(error);
		}

		public override void OnCompleted()
		{
			ISubject<TElement> subject;
			lock (_nullGate)
			{
				subject = _null;
			}
			subject?.OnCompleted();
			foreach (ISubject<TElement> value in _map.Values)
			{
				value.OnCompleted();
			}
			lock (_gate)
			{
				ForwardOnCompleted();
			}
		}

		private void Error(Exception exception)
		{
			ISubject<TElement> subject;
			lock (_nullGate)
			{
				subject = _null;
			}
			subject?.OnError(exception);
			foreach (ISubject<TElement> value in _map.Values)
			{
				value.OnError(exception);
			}
			lock (_gate)
			{
				ForwardOnError(exception);
			}
		}
	}

	private readonly IObservable<TSource> _source;

	private readonly Func<TSource, TKey> _keySelector;

	private readonly Func<TSource, TElement> _elementSelector;

	private readonly Func<IGroupedObservable<TKey, TElement>, IObservable<TDuration>> _durationSelector;

	private readonly int? _capacity;

	private readonly IEqualityComparer<TKey> _comparer;

	public GroupByUntil(IObservable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, Func<IGroupedObservable<TKey, TElement>, IObservable<TDuration>> durationSelector, int? capacity, IEqualityComparer<TKey> comparer)
	{
		_source = source;
		_keySelector = keySelector;
		_elementSelector = elementSelector;
		_durationSelector = durationSelector;
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
