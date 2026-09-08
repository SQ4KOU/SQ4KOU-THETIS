using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;

namespace System.Reactive.Linq.ObservableImpl;

internal sealed class CombineLatest<TFirst, TSecond, TResult> : Producer<TResult, CombineLatest<TFirst, TSecond, TResult>._>
{
	internal sealed class @_ : IdentitySink<TResult>
	{
		private sealed class FirstObserver : IObserver<TFirst>
		{
			private readonly @_ _parent;

			private SecondObserver _other;

			public bool HasValue { get; private set; }

			public TFirst? Value { get; private set; }

			public bool Done { get; private set; }

			public FirstObserver(@_ parent)
			{
				_parent = parent;
				_other = null;
			}

			public void SetOther(SecondObserver other)
			{
				_other = other;
			}

			public void OnNext(TFirst value)
			{
				lock (_parent._gate)
				{
					HasValue = true;
					Value = value;
					if (_other.HasValue)
					{
						TResult value2;
						try
						{
							value2 = _parent._resultSelector(value, _other.Value);
						}
						catch (Exception error)
						{
							_parent.ForwardOnError(error);
							return;
						}
						_parent.ForwardOnNext(value2);
					}
					else if (_other.Done)
					{
						_parent.ForwardOnCompleted();
					}
				}
			}

			public void OnError(Exception error)
			{
				lock (_parent._gate)
				{
					_parent.ForwardOnError(error);
				}
			}

			public void OnCompleted()
			{
				lock (_parent._gate)
				{
					Done = true;
					if (_other.Done)
					{
						_parent.ForwardOnCompleted();
					}
					else
					{
						_parent._firstDisposable.Dispose();
					}
				}
			}
		}

		private sealed class SecondObserver : IObserver<TSecond>
		{
			private readonly @_ _parent;

			private FirstObserver _other;

			public bool HasValue { get; private set; }

			public TSecond? Value { get; private set; }

			public bool Done { get; private set; }

			public SecondObserver(@_ parent)
			{
				_parent = parent;
				_other = null;
			}

			public void SetOther(FirstObserver other)
			{
				_other = other;
			}

			public void OnNext(TSecond value)
			{
				lock (_parent._gate)
				{
					HasValue = true;
					Value = value;
					if (_other.HasValue)
					{
						TResult value2;
						try
						{
							value2 = _parent._resultSelector(_other.Value, value);
						}
						catch (Exception error)
						{
							_parent.ForwardOnError(error);
							return;
						}
						_parent.ForwardOnNext(value2);
					}
					else if (_other.Done)
					{
						_parent.ForwardOnCompleted();
					}
				}
			}

			public void OnError(Exception error)
			{
				lock (_parent._gate)
				{
					_parent.ForwardOnError(error);
				}
			}

			public void OnCompleted()
			{
				lock (_parent._gate)
				{
					Done = true;
					if (_other.Done)
					{
						_parent.ForwardOnCompleted();
					}
					else
					{
						_parent._secondDisposable.Dispose();
					}
				}
			}
		}

		private readonly Func<TFirst, TSecond, TResult> _resultSelector;

		private readonly object _gate = new object();

		private SingleAssignmentDisposableValue _firstDisposable;

		private SingleAssignmentDisposableValue _secondDisposable;

		public _(Func<TFirst, TSecond, TResult> resultSelector, IObserver<TResult> observer)
			: base(observer)
		{
			_resultSelector = resultSelector;
		}

		public void Run(IObservable<TFirst> first, IObservable<TSecond> second)
		{
			FirstObserver firstObserver = new FirstObserver(this);
			SecondObserver secondObserver = new SecondObserver(this);
			firstObserver.SetOther(secondObserver);
			secondObserver.SetOther(firstObserver);
			_firstDisposable.Disposable = first.SubscribeSafe(firstObserver);
			_secondDisposable.Disposable = second.SubscribeSafe(secondObserver);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_firstDisposable.Dispose();
				_secondDisposable.Dispose();
			}
			base.Dispose(disposing);
		}
	}

	private readonly IObservable<TFirst> _first;

	private readonly IObservable<TSecond> _second;

	private readonly Func<TFirst, TSecond, TResult> _resultSelector;

	public CombineLatest(IObservable<TFirst> first, IObservable<TSecond> second, Func<TFirst, TSecond, TResult> resultSelector)
	{
		_first = first;
		_second = second;
		_resultSelector = resultSelector;
	}

	protected override @_ CreateSink(IObserver<TResult> observer)
	{
		return new @_(_resultSelector, observer);
	}

	protected override void Run(@_ sink)
	{
		sink.Run(_first, _second);
	}
}
internal sealed class CombineLatest<TSource, TResult> : Producer<TResult, CombineLatest<TSource, TResult>._>
{
	internal sealed class @_ : IdentitySink<TResult>
	{
		private sealed class SourceObserver : SafeObserver<TSource>
		{
			private readonly @_ _parent;

			private readonly int _index;

			public SourceObserver(@_ parent, int index)
			{
				_parent = parent;
				_index = index;
			}

			public override void OnNext(TSource value)
			{
				_parent.OnNext(_index, value);
			}

			public override void OnError(Exception error)
			{
				_parent.OnError(error);
			}

			public override void OnCompleted()
			{
				_parent.OnCompleted(_index);
			}
		}

		private readonly object _gate = new object();

		private readonly Func<IList<TSource>, TResult> _resultSelector;

		private bool[] _hasValue;

		private bool _hasValueAll;

		private TSource[] _values;

		private bool[] _isDone;

		private IDisposable[] _subscriptions;

		public _(Func<IList<TSource>, TResult> resultSelector, IObserver<TResult> observer)
			: base(observer)
		{
			_resultSelector = resultSelector;
			_hasValue = null;
			_values = null;
			_isDone = null;
			_subscriptions = null;
		}

		public void Run(IEnumerable<IObservable<TSource>> sources)
		{
			IObservable<TSource>[] array = sources.ToArray();
			int num = array.Length;
			_hasValue = new bool[num];
			_hasValueAll = false;
			_values = new TSource[num];
			_isDone = new bool[num];
			_subscriptions = new IDisposable[num];
			for (int i = 0; i < num; i++)
			{
				int num2 = i;
				SourceObserver sourceObserver = new SourceObserver(this, num2);
				_subscriptions[num2] = sourceObserver;
				sourceObserver.SetResource(array[num2].SubscribeSafe(sourceObserver));
			}
			SetUpstream(StableCompositeDisposable.CreateTrusted(_subscriptions));
		}

		private void OnNext(int index, TSource value)
		{
			lock (_gate)
			{
				_values[index] = value;
				_hasValue[index] = true;
				if (_hasValueAll || (_hasValueAll = _hasValue.All()))
				{
					TResult value2;
					try
					{
						value2 = _resultSelector(new ReadOnlyCollection<TSource>(_values));
					}
					catch (Exception error)
					{
						ForwardOnError(error);
						return;
					}
					ForwardOnNext(value2);
				}
				else if (_isDone.AllExcept(index))
				{
					ForwardOnCompleted();
				}
			}
		}

		private new void OnError(Exception error)
		{
			lock (_gate)
			{
				ForwardOnError(error);
			}
		}

		private void OnCompleted(int index)
		{
			lock (_gate)
			{
				_isDone[index] = true;
				if (_isDone.All())
				{
					ForwardOnCompleted();
				}
				else
				{
					_subscriptions[index].Dispose();
				}
			}
		}
	}

	private readonly IEnumerable<IObservable<TSource>> _sources;

	private readonly Func<IList<TSource>, TResult> _resultSelector;

	public CombineLatest(IEnumerable<IObservable<TSource>> sources, Func<IList<TSource>, TResult> resultSelector)
	{
		_sources = sources;
		_resultSelector = resultSelector;
	}

	protected override @_ CreateSink(IObserver<TResult> observer)
	{
		return new @_(_resultSelector, observer);
	}

	protected override void Run(@_ sink)
	{
		sink.Run(_sources);
	}
}
internal sealed class CombineLatest<T1, T2, T3, TResult> : Producer<TResult, CombineLatest<T1, T2, T3, TResult>._>
{
	internal sealed class @_ : CombineLatestSink<TResult>
	{
		private readonly Func<T1, T2, T3, TResult> _resultSelector;

		private readonly CombineLatestObserver<T1> _observer1;

		private readonly CombineLatestObserver<T2> _observer2;

		private readonly CombineLatestObserver<T3> _observer3;

		public _(Func<T1, T2, T3, TResult> resultSelector, IObserver<TResult> observer)
			: base(3, observer)
		{
			_resultSelector = resultSelector;
			_observer1 = new CombineLatestObserver<T1>(_gate, this, 0);
			_observer2 = new CombineLatestObserver<T2>(_gate, this, 1);
			_observer3 = new CombineLatestObserver<T3>(_gate, this, 2);
		}

		public void Run(IObservable<T1> source1, IObservable<T2> source2, IObservable<T3> source3)
		{
			IDisposable[] disposables = new IDisposable[3] { _observer1, _observer2, _observer3 };
			_observer1.SetResource(source1.SubscribeSafe(_observer1));
			_observer2.SetResource(source2.SubscribeSafe(_observer2));
			_observer3.SetResource(source3.SubscribeSafe(_observer3));
			SetUpstream(StableCompositeDisposable.CreateTrusted(disposables));
		}

		protected override TResult GetResult()
		{
			return _resultSelector(_observer1.Value, _observer2.Value, _observer3.Value);
		}
	}

	private readonly IObservable<T1> _source1;

	private readonly IObservable<T2> _source2;

	private readonly IObservable<T3> _source3;

	private readonly Func<T1, T2, T3, TResult> _resultSelector;

	public CombineLatest(IObservable<T1> source1, IObservable<T2> source2, IObservable<T3> source3, Func<T1, T2, T3, TResult> resultSelector)
	{
		_source1 = source1;
		_source2 = source2;
		_source3 = source3;
		_resultSelector = resultSelector;
	}

	protected override @_ CreateSink(IObserver<TResult> observer)
	{
		return new @_(_resultSelector, observer);
	}

	protected override void Run(@_ sink)
	{
		sink.Run(_source1, _source2, _source3);
	}
}
internal sealed class CombineLatest<T1, T2, T3, T4, TResult> : Producer<TResult, CombineLatest<T1, T2, T3, T4, TResult>._>
{
	internal sealed class @_ : CombineLatestSink<TResult>
	{
		private readonly Func<T1, T2, T3, T4, TResult> _resultSelector;

		private readonly CombineLatestObserver<T1> _observer1;

		private readonly CombineLatestObserver<T2> _observer2;

		private readonly CombineLatestObserver<T3> _observer3;

		private readonly CombineLatestObserver<T4> _observer4;

		public _(Func<T1, T2, T3, T4, TResult> resultSelector, IObserver<TResult> observer)
			: base(4, observer)
		{
			_resultSelector = resultSelector;
			_observer1 = new CombineLatestObserver<T1>(_gate, this, 0);
			_observer2 = new CombineLatestObserver<T2>(_gate, this, 1);
			_observer3 = new CombineLatestObserver<T3>(_gate, this, 2);
			_observer4 = new CombineLatestObserver<T4>(_gate, this, 3);
		}

		public void Run(IObservable<T1> source1, IObservable<T2> source2, IObservable<T3> source3, IObservable<T4> source4)
		{
			IDisposable[] disposables = new IDisposable[4] { _observer1, _observer2, _observer3, _observer4 };
			_observer1.SetResource(source1.SubscribeSafe(_observer1));
			_observer2.SetResource(source2.SubscribeSafe(_observer2));
			_observer3.SetResource(source3.SubscribeSafe(_observer3));
			_observer4.SetResource(source4.SubscribeSafe(_observer4));
			SetUpstream(StableCompositeDisposable.CreateTrusted(disposables));
		}

		protected override TResult GetResult()
		{
			return _resultSelector(_observer1.Value, _observer2.Value, _observer3.Value, _observer4.Value);
		}
	}

	private readonly IObservable<T1> _source1;

	private readonly IObservable<T2> _source2;

	private readonly IObservable<T3> _source3;

	private readonly IObservable<T4> _source4;

	private readonly Func<T1, T2, T3, T4, TResult> _resultSelector;

	public CombineLatest(IObservable<T1> source1, IObservable<T2> source2, IObservable<T3> source3, IObservable<T4> source4, Func<T1, T2, T3, T4, TResult> resultSelector)
	{
		_source1 = source1;
		_source2 = source2;
		_source3 = source3;
		_source4 = source4;
		_resultSelector = resultSelector;
	}

	protected override @_ CreateSink(IObserver<TResult> observer)
	{
		return new @_(_resultSelector, observer);
	}

	protected override void Run(@_ sink)
	{
		sink.Run(_source1, _source2, _source3, _source4);
	}
}
internal sealed class CombineLatest<T1, T2, T3, T4, T5, TResult> : Producer<TResult, CombineLatest<T1, T2, T3, T4, T5, TResult>._>
{
	internal sealed class @_ : CombineLatestSink<TResult>
	{
		private readonly Func<T1, T2, T3, T4, T5, TResult> _resultSelector;

		private readonly CombineLatestObserver<T1> _observer1;

		private readonly CombineLatestObserver<T2> _observer2;

		private readonly CombineLatestObserver<T3> _observer3;

		private readonly CombineLatestObserver<T4> _observer4;

		private readonly CombineLatestObserver<T5> _observer5;

		public _(Func<T1, T2, T3, T4, T5, TResult> resultSelector, IObserver<TResult> observer)
			: base(5, observer)
		{
			_resultSelector = resultSelector;
			_observer1 = new CombineLatestObserver<T1>(_gate, this, 0);
			_observer2 = new CombineLatestObserver<T2>(_gate, this, 1);
			_observer3 = new CombineLatestObserver<T3>(_gate, this, 2);
			_observer4 = new CombineLatestObserver<T4>(_gate, this, 3);
			_observer5 = new CombineLatestObserver<T5>(_gate, this, 4);
		}

		public void Run(IObservable<T1> source1, IObservable<T2> source2, IObservable<T3> source3, IObservable<T4> source4, IObservable<T5> source5)
		{
			IDisposable[] disposables = new IDisposable[5] { _observer1, _observer2, _observer3, _observer4, _observer5 };
			_observer1.SetResource(source1.SubscribeSafe(_observer1));
			_observer2.SetResource(source2.SubscribeSafe(_observer2));
			_observer3.SetResource(source3.SubscribeSafe(_observer3));
			_observer4.SetResource(source4.SubscribeSafe(_observer4));
			_observer5.SetResource(source5.SubscribeSafe(_observer5));
			SetUpstream(StableCompositeDisposable.CreateTrusted(disposables));
		}

		protected override TResult GetResult()
		{
			return _resultSelector(_observer1.Value, _observer2.Value, _observer3.Value, _observer4.Value, _observer5.Value);
		}
	}

	private readonly IObservable<T1> _source1;

	private readonly IObservable<T2> _source2;

	private readonly IObservable<T3> _source3;

	private readonly IObservable<T4> _source4;

	private readonly IObservable<T5> _source5;

	private readonly Func<T1, T2, T3, T4, T5, TResult> _resultSelector;

	public CombineLatest(IObservable<T1> source1, IObservable<T2> source2, IObservable<T3> source3, IObservable<T4> source4, IObservable<T5> source5, Func<T1, T2, T3, T4, T5, TResult> resultSelector)
	{
		_source1 = source1;
		_source2 = source2;
		_source3 = source3;
		_source4 = source4;
		_source5 = source5;
		_resultSelector = resultSelector;
	}

	protected override @_ CreateSink(IObserver<TResult> observer)
	{
		return new @_(_resultSelector, observer);
	}

	protected override void Run(@_ sink)
	{
		sink.Run(_source1, _source2, _source3, _source4, _source5);
	}
}
internal sealed class CombineLatest<T1, T2, T3, T4, T5, T6, TResult> : Producer<TResult, CombineLatest<T1, T2, T3, T4, T5, T6, TResult>._>
{
	internal sealed class @_ : CombineLatestSink<TResult>
	{
		private readonly Func<T1, T2, T3, T4, T5, T6, TResult> _resultSelector;

		private readonly CombineLatestObserver<T1> _observer1;

		private readonly CombineLatestObserver<T2> _observer2;

		private readonly CombineLatestObserver<T3> _observer3;

		private readonly CombineLatestObserver<T4> _observer4;

		private readonly CombineLatestObserver<T5> _observer5;

		private readonly CombineLatestObserver<T6> _observer6;

		public _(Func<T1, T2, T3, T4, T5, T6, TResult> resultSelector, IObserver<TResult> observer)
			: base(6, observer)
		{
			_resultSelector = resultSelector;
			_observer1 = new CombineLatestObserver<T1>(_gate, this, 0);
			_observer2 = new CombineLatestObserver<T2>(_gate, this, 1);
			_observer3 = new CombineLatestObserver<T3>(_gate, this, 2);
			_observer4 = new CombineLatestObserver<T4>(_gate, this, 3);
			_observer5 = new CombineLatestObserver<T5>(_gate, this, 4);
			_observer6 = new CombineLatestObserver<T6>(_gate, this, 5);
		}

		public void Run(IObservable<T1> source1, IObservable<T2> source2, IObservable<T3> source3, IObservable<T4> source4, IObservable<T5> source5, IObservable<T6> source6)
		{
			IDisposable[] disposables = new IDisposable[6] { _observer1, _observer2, _observer3, _observer4, _observer5, _observer6 };
			_observer1.SetResource(source1.SubscribeSafe(_observer1));
			_observer2.SetResource(source2.SubscribeSafe(_observer2));
			_observer3.SetResource(source3.SubscribeSafe(_observer3));
			_observer4.SetResource(source4.SubscribeSafe(_observer4));
			_observer5.SetResource(source5.SubscribeSafe(_observer5));
			_observer6.SetResource(source6.SubscribeSafe(_observer6));
			SetUpstream(StableCompositeDisposable.CreateTrusted(disposables));
		}

		protected override TResult GetResult()
		{
			return _resultSelector(_observer1.Value, _observer2.Value, _observer3.Value, _observer4.Value, _observer5.Value, _observer6.Value);
		}
	}

	private readonly IObservable<T1> _source1;

	private readonly IObservable<T2> _source2;

	private readonly IObservable<T3> _source3;

	private readonly IObservable<T4> _source4;

	private readonly IObservable<T5> _source5;

	private readonly IObservable<T6> _source6;

	private readonly Func<T1, T2, T3, T4, T5, T6, TResult> _resultSelector;

	public CombineLatest(IObservable<T1> source1, IObservable<T2> source2, IObservable<T3> source3, IObservable<T4> source4, IObservable<T5> source5, IObservable<T6> source6, Func<T1, T2, T3, T4, T5, T6, TResult> resultSelector)
	{
		_source1 = source1;
		_source2 = source2;
		_source3 = source3;
		_source4 = source4;
		_source5 = source5;
		_source6 = source6;
		_resultSelector = resultSelector;
	}

	protected override @_ CreateSink(IObserver<TResult> observer)
	{
		return new @_(_resultSelector, observer);
	}

	protected override void Run(@_ sink)
	{
		sink.Run(_source1, _source2, _source3, _source4, _source5, _source6);
	}
}
internal sealed class CombineLatest<T1, T2, T3, T4, T5, T6, T7, TResult> : Producer<TResult, CombineLatest<T1, T2, T3, T4, T5, T6, T7, TResult>._>
{
	internal sealed class @_ : CombineLatestSink<TResult>
	{
		private readonly Func<T1, T2, T3, T4, T5, T6, T7, TResult> _resultSelector;

		private readonly CombineLatestObserver<T1> _observer1;

		private readonly CombineLatestObserver<T2> _observer2;

		private readonly CombineLatestObserver<T3> _observer3;

		private readonly CombineLatestObserver<T4> _observer4;

		private readonly CombineLatestObserver<T5> _observer5;

		private readonly CombineLatestObserver<T6> _observer6;

		private readonly CombineLatestObserver<T7> _observer7;

		public _(Func<T1, T2, T3, T4, T5, T6, T7, TResult> resultSelector, IObserver<TResult> observer)
			: base(7, observer)
		{
			_resultSelector = resultSelector;
			_observer1 = new CombineLatestObserver<T1>(_gate, this, 0);
			_observer2 = new CombineLatestObserver<T2>(_gate, this, 1);
			_observer3 = new CombineLatestObserver<T3>(_gate, this, 2);
			_observer4 = new CombineLatestObserver<T4>(_gate, this, 3);
			_observer5 = new CombineLatestObserver<T5>(_gate, this, 4);
			_observer6 = new CombineLatestObserver<T6>(_gate, this, 5);
			_observer7 = new CombineLatestObserver<T7>(_gate, this, 6);
		}

		public void Run(IObservable<T1> source1, IObservable<T2> source2, IObservable<T3> source3, IObservable<T4> source4, IObservable<T5> source5, IObservable<T6> source6, IObservable<T7> source7)
		{
			IDisposable[] disposables = new IDisposable[7] { _observer1, _observer2, _observer3, _observer4, _observer5, _observer6, _observer7 };
			_observer1.SetResource(source1.SubscribeSafe(_observer1));
			_observer2.SetResource(source2.SubscribeSafe(_observer2));
			_observer3.SetResource(source3.SubscribeSafe(_observer3));
			_observer4.SetResource(source4.SubscribeSafe(_observer4));
			_observer5.SetResource(source5.SubscribeSafe(_observer5));
			_observer6.SetResource(source6.SubscribeSafe(_observer6));
			_observer7.SetResource(source7.SubscribeSafe(_observer7));
			SetUpstream(StableCompositeDisposable.CreateTrusted(disposables));
		}

		protected override TResult GetResult()
		{
			return _resultSelector(_observer1.Value, _observer2.Value, _observer3.Value, _observer4.Value, _observer5.Value, _observer6.Value, _observer7.Value);
		}
	}

	private readonly IObservable<T1> _source1;

	private readonly IObservable<T2> _source2;

	private readonly IObservable<T3> _source3;

	private readonly IObservable<T4> _source4;

	private readonly IObservable<T5> _source5;

	private readonly IObservable<T6> _source6;

	private readonly IObservable<T7> _source7;

	private readonly Func<T1, T2, T3, T4, T5, T6, T7, TResult> _resultSelector;

	public CombineLatest(IObservable<T1> source1, IObservable<T2> source2, IObservable<T3> source3, IObservable<T4> source4, IObservable<T5> source5, IObservable<T6> source6, IObservable<T7> source7, Func<T1, T2, T3, T4, T5, T6, T7, TResult> resultSelector)
	{
		_source1 = source1;
		_source2 = source2;
		_source3 = source3;
		_source4 = source4;
		_source5 = source5;
		_source6 = source6;
		_source7 = source7;
		_resultSelector = resultSelector;
	}

	protected override @_ CreateSink(IObserver<TResult> observer)
	{
		return new @_(_resultSelector, observer);
	}

	protected override void Run(@_ sink)
	{
		sink.Run(_source1, _source2, _source3, _source4, _source5, _source6, _source7);
	}
}
internal sealed class CombineLatest<T1, T2, T3, T4, T5, T6, T7, T8, TResult> : Producer<TResult, CombineLatest<T1, T2, T3, T4, T5, T6, T7, T8, TResult>._>
{
	internal sealed class @_ : CombineLatestSink<TResult>
	{
		private readonly Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> _resultSelector;

		private readonly CombineLatestObserver<T1> _observer1;

		private readonly CombineLatestObserver<T2> _observer2;

		private readonly CombineLatestObserver<T3> _observer3;

		private readonly CombineLatestObserver<T4> _observer4;

		private readonly CombineLatestObserver<T5> _observer5;

		private readonly CombineLatestObserver<T6> _observer6;

		private readonly CombineLatestObserver<T7> _observer7;

		private readonly CombineLatestObserver<T8> _observer8;

		public _(Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> resultSelector, IObserver<TResult> observer)
			: base(8, observer)
		{
			_resultSelector = resultSelector;
			_observer1 = new CombineLatestObserver<T1>(_gate, this, 0);
			_observer2 = new CombineLatestObserver<T2>(_gate, this, 1);
			_observer3 = new CombineLatestObserver<T3>(_gate, this, 2);
			_observer4 = new CombineLatestObserver<T4>(_gate, this, 3);
			_observer5 = new CombineLatestObserver<T5>(_gate, this, 4);
			_observer6 = new CombineLatestObserver<T6>(_gate, this, 5);
			_observer7 = new CombineLatestObserver<T7>(_gate, this, 6);
			_observer8 = new CombineLatestObserver<T8>(_gate, this, 7);
		}

		public void Run(IObservable<T1> source1, IObservable<T2> source2, IObservable<T3> source3, IObservable<T4> source4, IObservable<T5> source5, IObservable<T6> source6, IObservable<T7> source7, IObservable<T8> source8)
		{
			IDisposable[] disposables = new IDisposable[8] { _observer1, _observer2, _observer3, _observer4, _observer5, _observer6, _observer7, _observer8 };
			_observer1.SetResource(source1.SubscribeSafe(_observer1));
			_observer2.SetResource(source2.SubscribeSafe(_observer2));
			_observer3.SetResource(source3.SubscribeSafe(_observer3));
			_observer4.SetResource(source4.SubscribeSafe(_observer4));
			_observer5.SetResource(source5.SubscribeSafe(_observer5));
			_observer6.SetResource(source6.SubscribeSafe(_observer6));
			_observer7.SetResource(source7.SubscribeSafe(_observer7));
			_observer8.SetResource(source8.SubscribeSafe(_observer8));
			SetUpstream(StableCompositeDisposable.CreateTrusted(disposables));
		}

		protected override TResult GetResult()
		{
			return _resultSelector(_observer1.Value, _observer2.Value, _observer3.Value, _observer4.Value, _observer5.Value, _observer6.Value, _observer7.Value, _observer8.Value);
		}
	}

	private readonly IObservable<T1> _source1;

	private readonly IObservable<T2> _source2;

	private readonly IObservable<T3> _source3;

	private readonly IObservable<T4> _source4;

	private readonly IObservable<T5> _source5;

	private readonly IObservable<T6> _source6;

	private readonly IObservable<T7> _source7;

	private readonly IObservable<T8> _source8;

	private readonly Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> _resultSelector;

	public CombineLatest(IObservable<T1> source1, IObservable<T2> source2, IObservable<T3> source3, IObservable<T4> source4, IObservable<T5> source5, IObservable<T6> source6, IObservable<T7> source7, IObservable<T8> source8, Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> resultSelector)
	{
		_source1 = source1;
		_source2 = source2;
		_source3 = source3;
		_source4 = source4;
		_source5 = source5;
		_source6 = source6;
		_source7 = source7;
		_source8 = source8;
		_resultSelector = resultSelector;
	}

	protected override @_ CreateSink(IObserver<TResult> observer)
	{
		return new @_(_resultSelector, observer);
	}

	protected override void Run(@_ sink)
	{
		sink.Run(_source1, _source2, _source3, _source4, _source5, _source6, _source7, _source8);
	}
}
internal sealed class CombineLatest<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> : Producer<TResult, CombineLatest<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>._>
{
	internal sealed class @_ : CombineLatestSink<TResult>
	{
		private readonly Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> _resultSelector;

		private readonly CombineLatestObserver<T1> _observer1;

		private readonly CombineLatestObserver<T2> _observer2;

		private readonly CombineLatestObserver<T3> _observer3;

		private readonly CombineLatestObserver<T4> _observer4;

		private readonly CombineLatestObserver<T5> _observer5;

		private readonly CombineLatestObserver<T6> _observer6;

		private readonly CombineLatestObserver<T7> _observer7;

		private readonly CombineLatestObserver<T8> _observer8;

		private readonly CombineLatestObserver<T9> _observer9;

		public _(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> resultSelector, IObserver<TResult> observer)
			: base(9, observer)
		{
			_resultSelector = resultSelector;
			_observer1 = new CombineLatestObserver<T1>(_gate, this, 0);
			_observer2 = new CombineLatestObserver<T2>(_gate, this, 1);
			_observer3 = new CombineLatestObserver<T3>(_gate, this, 2);
			_observer4 = new CombineLatestObserver<T4>(_gate, this, 3);
			_observer5 = new CombineLatestObserver<T5>(_gate, this, 4);
			_observer6 = new CombineLatestObserver<T6>(_gate, this, 5);
			_observer7 = new CombineLatestObserver<T7>(_gate, this, 6);
			_observer8 = new CombineLatestObserver<T8>(_gate, this, 7);
			_observer9 = new CombineLatestObserver<T9>(_gate, this, 8);
		}

		public void Run(IObservable<T1> source1, IObservable<T2> source2, IObservable<T3> source3, IObservable<T4> source4, IObservable<T5> source5, IObservable<T6> source6, IObservable<T7> source7, IObservable<T8> source8, IObservable<T9> source9)
		{
			IDisposable[] disposables = new IDisposable[9] { _observer1, _observer2, _observer3, _observer4, _observer5, _observer6, _observer7, _observer8, _observer9 };
			_observer1.SetResource(source1.SubscribeSafe(_observer1));
			_observer2.SetResource(source2.SubscribeSafe(_observer2));
			_observer3.SetResource(source3.SubscribeSafe(_observer3));
			_observer4.SetResource(source4.SubscribeSafe(_observer4));
			_observer5.SetResource(source5.SubscribeSafe(_observer5));
			_observer6.SetResource(source6.SubscribeSafe(_observer6));
			_observer7.SetResource(source7.SubscribeSafe(_observer7));
			_observer8.SetResource(source8.SubscribeSafe(_observer8));
			_observer9.SetResource(source9.SubscribeSafe(_observer9));
			SetUpstream(StableCompositeDisposable.CreateTrusted(disposables));
		}

		protected override TResult GetResult()
		{
			return _resultSelector(_observer1.Value, _observer2.Value, _observer3.Value, _observer4.Value, _observer5.Value, _observer6.Value, _observer7.Value, _observer8.Value, _observer9.Value);
		}
	}

	private readonly IObservable<T1> _source1;

	private readonly IObservable<T2> _source2;

	private readonly IObservable<T3> _source3;

	private readonly IObservable<T4> _source4;

	private readonly IObservable<T5> _source5;

	private readonly IObservable<T6> _source6;

	private readonly IObservable<T7> _source7;

	private readonly IObservable<T8> _source8;

	private readonly IObservable<T9> _source9;

	private readonly Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> _resultSelector;

	public CombineLatest(IObservable<T1> source1, IObservable<T2> source2, IObservable<T3> source3, IObservable<T4> source4, IObservable<T5> source5, IObservable<T6> source6, IObservable<T7> source7, IObservable<T8> source8, IObservable<T9> source9, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> resultSelector)
	{
		_source1 = source1;
		_source2 = source2;
		_source3 = source3;
		_source4 = source4;
		_source5 = source5;
		_source6 = source6;
		_source7 = source7;
		_source8 = source8;
		_source9 = source9;
		_resultSelector = resultSelector;
	}

	protected override @_ CreateSink(IObserver<TResult> observer)
	{
		return new @_(_resultSelector, observer);
	}

	protected override void Run(@_ sink)
	{
		sink.Run(_source1, _source2, _source3, _source4, _source5, _source6, _source7, _source8, _source9);
	}
}
internal sealed class CombineLatest<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> : Producer<TResult, CombineLatest<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>._>
{
	internal sealed class @_ : CombineLatestSink<TResult>
	{
		private readonly Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> _resultSelector;

		private readonly CombineLatestObserver<T1> _observer1;

		private readonly CombineLatestObserver<T2> _observer2;

		private readonly CombineLatestObserver<T3> _observer3;

		private readonly CombineLatestObserver<T4> _observer4;

		private readonly CombineLatestObserver<T5> _observer5;

		private readonly CombineLatestObserver<T6> _observer6;

		private readonly CombineLatestObserver<T7> _observer7;

		private readonly CombineLatestObserver<T8> _observer8;

		private readonly CombineLatestObserver<T9> _observer9;

		private readonly CombineLatestObserver<T10> _observer10;

		public _(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> resultSelector, IObserver<TResult> observer)
			: base(10, observer)
		{
			_resultSelector = resultSelector;
			_observer1 = new CombineLatestObserver<T1>(_gate, this, 0);
			_observer2 = new CombineLatestObserver<T2>(_gate, this, 1);
			_observer3 = new CombineLatestObserver<T3>(_gate, this, 2);
			_observer4 = new CombineLatestObserver<T4>(_gate, this, 3);
			_observer5 = new CombineLatestObserver<T5>(_gate, this, 4);
			_observer6 = new CombineLatestObserver<T6>(_gate, this, 5);
			_observer7 = new CombineLatestObserver<T7>(_gate, this, 6);
			_observer8 = new CombineLatestObserver<T8>(_gate, this, 7);
			_observer9 = new CombineLatestObserver<T9>(_gate, this, 8);
			_observer10 = new CombineLatestObserver<T10>(_gate, this, 9);
		}

		public void Run(IObservable<T1> source1, IObservable<T2> source2, IObservable<T3> source3, IObservable<T4> source4, IObservable<T5> source5, IObservable<T6> source6, IObservable<T7> source7, IObservable<T8> source8, IObservable<T9> source9, IObservable<T10> source10)
		{
			IDisposable[] disposables = new IDisposable[10] { _observer1, _observer2, _observer3, _observer4, _observer5, _observer6, _observer7, _observer8, _observer9, _observer10 };
			_observer1.SetResource(source1.SubscribeSafe(_observer1));
			_observer2.SetResource(source2.SubscribeSafe(_observer2));
			_observer3.SetResource(source3.SubscribeSafe(_observer3));
			_observer4.SetResource(source4.SubscribeSafe(_observer4));
			_observer5.SetResource(source5.SubscribeSafe(_observer5));
			_observer6.SetResource(source6.SubscribeSafe(_observer6));
			_observer7.SetResource(source7.SubscribeSafe(_observer7));
			_observer8.SetResource(source8.SubscribeSafe(_observer8));
			_observer9.SetResource(source9.SubscribeSafe(_observer9));
			_observer10.SetResource(source10.SubscribeSafe(_observer10));
			SetUpstream(StableCompositeDisposable.CreateTrusted(disposables));
		}

		protected override TResult GetResult()
		{
			return _resultSelector(_observer1.Value, _observer2.Value, _observer3.Value, _observer4.Value, _observer5.Value, _observer6.Value, _observer7.Value, _observer8.Value, _observer9.Value, _observer10.Value);
		}
	}

	private readonly IObservable<T1> _source1;

	private readonly IObservable<T2> _source2;

	private readonly IObservable<T3> _source3;

	private readonly IObservable<T4> _source4;

	private readonly IObservable<T5> _source5;

	private readonly IObservable<T6> _source6;

	private readonly IObservable<T7> _source7;

	private readonly IObservable<T8> _source8;

	private readonly IObservable<T9> _source9;

	private readonly IObservable<T10> _source10;

	private readonly Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> _resultSelector;

	public CombineLatest(IObservable<T1> source1, IObservable<T2> source2, IObservable<T3> source3, IObservable<T4> source4, IObservable<T5> source5, IObservable<T6> source6, IObservable<T7> source7, IObservable<T8> source8, IObservable<T9> source9, IObservable<T10> source10, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> resultSelector)
	{
		_source1 = source1;
		_source2 = source2;
		_source3 = source3;
		_source4 = source4;
		_source5 = source5;
		_source6 = source6;
		_source7 = source7;
		_source8 = source8;
		_source9 = source9;
		_source10 = source10;
		_resultSelector = resultSelector;
	}

	protected override @_ CreateSink(IObserver<TResult> observer)
	{
		return new @_(_resultSelector, observer);
	}

	protected override void Run(@_ sink)
	{
		sink.Run(_source1, _source2, _source3, _source4, _source5, _source6, _source7, _source8, _source9, _source10);
	}
}
internal sealed class CombineLatest<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult> : Producer<TResult, CombineLatest<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>._>
{
	internal sealed class @_ : CombineLatestSink<TResult>
	{
		private readonly Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult> _resultSelector;

		private readonly CombineLatestObserver<T1> _observer1;

		private readonly CombineLatestObserver<T2> _observer2;

		private readonly CombineLatestObserver<T3> _observer3;

		private readonly CombineLatestObserver<T4> _observer4;

		private readonly CombineLatestObserver<T5> _observer5;

		private readonly CombineLatestObserver<T6> _observer6;

		private readonly CombineLatestObserver<T7> _observer7;

		private readonly CombineLatestObserver<T8> _observer8;

		private readonly CombineLatestObserver<T9> _observer9;

		private readonly CombineLatestObserver<T10> _observer10;

		private readonly CombineLatestObserver<T11> _observer11;

		public _(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult> resultSelector, IObserver<TResult> observer)
			: base(11, observer)
		{
			_resultSelector = resultSelector;
			_observer1 = new CombineLatestObserver<T1>(_gate, this, 0);
			_observer2 = new CombineLatestObserver<T2>(_gate, this, 1);
			_observer3 = new CombineLatestObserver<T3>(_gate, this, 2);
			_observer4 = new CombineLatestObserver<T4>(_gate, this, 3);
			_observer5 = new CombineLatestObserver<T5>(_gate, this, 4);
			_observer6 = new CombineLatestObserver<T6>(_gate, this, 5);
			_observer7 = new CombineLatestObserver<T7>(_gate, this, 6);
			_observer8 = new CombineLatestObserver<T8>(_gate, this, 7);
			_observer9 = new CombineLatestObserver<T9>(_gate, this, 8);
			_observer10 = new CombineLatestObserver<T10>(_gate, this, 9);
			_observer11 = new CombineLatestObserver<T11>(_gate, this, 10);
		}

		public void Run(IObservable<T1> source1, IObservable<T2> source2, IObservable<T3> source3, IObservable<T4> source4, IObservable<T5> source5, IObservable<T6> source6, IObservable<T7> source7, IObservable<T8> source8, IObservable<T9> source9, IObservable<T10> source10, IObservable<T11> source11)
		{
			IDisposable[] disposables = new IDisposable[11]
			{
				_observer1, _observer2, _observer3, _observer4, _observer5, _observer6, _observer7, _observer8, _observer9, _observer10,
				_observer11
			};
			_observer1.SetResource(source1.SubscribeSafe(_observer1));
			_observer2.SetResource(source2.SubscribeSafe(_observer2));
			_observer3.SetResource(source3.SubscribeSafe(_observer3));
			_observer4.SetResource(source4.SubscribeSafe(_observer4));
			_observer5.SetResource(source5.SubscribeSafe(_observer5));
			_observer6.SetResource(source6.SubscribeSafe(_observer6));
			_observer7.SetResource(source7.SubscribeSafe(_observer7));
			_observer8.SetResource(source8.SubscribeSafe(_observer8));
			_observer9.SetResource(source9.SubscribeSafe(_observer9));
			_observer10.SetResource(source10.SubscribeSafe(_observer10));
			_observer11.SetResource(source11.SubscribeSafe(_observer11));
			SetUpstream(StableCompositeDisposable.CreateTrusted(disposables));
		}

		protected override TResult GetResult()
		{
			return _resultSelector(_observer1.Value, _observer2.Value, _observer3.Value, _observer4.Value, _observer5.Value, _observer6.Value, _observer7.Value, _observer8.Value, _observer9.Value, _observer10.Value, _observer11.Value);
		}
	}

	private readonly IObservable<T1> _source1;

	private readonly IObservable<T2> _source2;

	private readonly IObservable<T3> _source3;

	private readonly IObservable<T4> _source4;

	private readonly IObservable<T5> _source5;

	private readonly IObservable<T6> _source6;

	private readonly IObservable<T7> _source7;

	private readonly IObservable<T8> _source8;

	private readonly IObservable<T9> _source9;

	private readonly IObservable<T10> _source10;

	private readonly IObservable<T11> _source11;

	private readonly Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult> _resultSelector;

	public CombineLatest(IObservable<T1> source1, IObservable<T2> source2, IObservable<T3> source3, IObservable<T4> source4, IObservable<T5> source5, IObservable<T6> source6, IObservable<T7> source7, IObservable<T8> source8, IObservable<T9> source9, IObservable<T10> source10, IObservable<T11> source11, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult> resultSelector)
	{
		_source1 = source1;
		_source2 = source2;
		_source3 = source3;
		_source4 = source4;
		_source5 = source5;
		_source6 = source6;
		_source7 = source7;
		_source8 = source8;
		_source9 = source9;
		_source10 = source10;
		_source11 = source11;
		_resultSelector = resultSelector;
	}

	protected override @_ CreateSink(IObserver<TResult> observer)
	{
		return new @_(_resultSelector, observer);
	}

	protected override void Run(@_ sink)
	{
		sink.Run(_source1, _source2, _source3, _source4, _source5, _source6, _source7, _source8, _source9, _source10, _source11);
	}
}
internal sealed class CombineLatest<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult> : Producer<TResult, CombineLatest<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>._>
{
	internal sealed class @_ : CombineLatestSink<TResult>
	{
		private readonly Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult> _resultSelector;

		private readonly CombineLatestObserver<T1> _observer1;

		private readonly CombineLatestObserver<T2> _observer2;

		private readonly CombineLatestObserver<T3> _observer3;

		private readonly CombineLatestObserver<T4> _observer4;

		private readonly CombineLatestObserver<T5> _observer5;

		private readonly CombineLatestObserver<T6> _observer6;

		private readonly CombineLatestObserver<T7> _observer7;

		private readonly CombineLatestObserver<T8> _observer8;

		private readonly CombineLatestObserver<T9> _observer9;

		private readonly CombineLatestObserver<T10> _observer10;

		private readonly CombineLatestObserver<T11> _observer11;

		private readonly CombineLatestObserver<T12> _observer12;

		public _(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult> resultSelector, IObserver<TResult> observer)
			: base(12, observer)
		{
			_resultSelector = resultSelector;
			_observer1 = new CombineLatestObserver<T1>(_gate, this, 0);
			_observer2 = new CombineLatestObserver<T2>(_gate, this, 1);
			_observer3 = new CombineLatestObserver<T3>(_gate, this, 2);
			_observer4 = new CombineLatestObserver<T4>(_gate, this, 3);
			_observer5 = new CombineLatestObserver<T5>(_gate, this, 4);
			_observer6 = new CombineLatestObserver<T6>(_gate, this, 5);
			_observer7 = new CombineLatestObserver<T7>(_gate, this, 6);
			_observer8 = new CombineLatestObserver<T8>(_gate, this, 7);
			_observer9 = new CombineLatestObserver<T9>(_gate, this, 8);
			_observer10 = new CombineLatestObserver<T10>(_gate, this, 9);
			_observer11 = new CombineLatestObserver<T11>(_gate, this, 10);
			_observer12 = new CombineLatestObserver<T12>(_gate, this, 11);
		}

		public void Run(IObservable<T1> source1, IObservable<T2> source2, IObservable<T3> source3, IObservable<T4> source4, IObservable<T5> source5, IObservable<T6> source6, IObservable<T7> source7, IObservable<T8> source8, IObservable<T9> source9, IObservable<T10> source10, IObservable<T11> source11, IObservable<T12> source12)
		{
			IDisposable[] disposables = new IDisposable[12]
			{
				_observer1, _observer2, _observer3, _observer4, _observer5, _observer6, _observer7, _observer8, _observer9, _observer10,
				_observer11, _observer12
			};
			_observer1.SetResource(source1.SubscribeSafe(_observer1));
			_observer2.SetResource(source2.SubscribeSafe(_observer2));
			_observer3.SetResource(source3.SubscribeSafe(_observer3));
			_observer4.SetResource(source4.SubscribeSafe(_observer4));
			_observer5.SetResource(source5.SubscribeSafe(_observer5));
			_observer6.SetResource(source6.SubscribeSafe(_observer6));
			_observer7.SetResource(source7.SubscribeSafe(_observer7));
			_observer8.SetResource(source8.SubscribeSafe(_observer8));
			_observer9.SetResource(source9.SubscribeSafe(_observer9));
			_observer10.SetResource(source10.SubscribeSafe(_observer10));
			_observer11.SetResource(source11.SubscribeSafe(_observer11));
			_observer12.SetResource(source12.SubscribeSafe(_observer12));
			SetUpstream(StableCompositeDisposable.CreateTrusted(disposables));
		}

		protected override TResult GetResult()
		{
			return _resultSelector(_observer1.Value, _observer2.Value, _observer3.Value, _observer4.Value, _observer5.Value, _observer6.Value, _observer7.Value, _observer8.Value, _observer9.Value, _observer10.Value, _observer11.Value, _observer12.Value);
		}
	}

	private readonly IObservable<T1> _source1;

	private readonly IObservable<T2> _source2;

	private readonly IObservable<T3> _source3;

	private readonly IObservable<T4> _source4;

	private readonly IObservable<T5> _source5;

	private readonly IObservable<T6> _source6;

	private readonly IObservable<T7> _source7;

	private readonly IObservable<T8> _source8;

	private readonly IObservable<T9> _source9;

	private readonly IObservable<T10> _source10;

	private readonly IObservable<T11> _source11;

	private readonly IObservable<T12> _source12;

	private readonly Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult> _resultSelector;

	public CombineLatest(IObservable<T1> source1, IObservable<T2> source2, IObservable<T3> source3, IObservable<T4> source4, IObservable<T5> source5, IObservable<T6> source6, IObservable<T7> source7, IObservable<T8> source8, IObservable<T9> source9, IObservable<T10> source10, IObservable<T11> source11, IObservable<T12> source12, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult> resultSelector)
	{
		_source1 = source1;
		_source2 = source2;
		_source3 = source3;
		_source4 = source4;
		_source5 = source5;
		_source6 = source6;
		_source7 = source7;
		_source8 = source8;
		_source9 = source9;
		_source10 = source10;
		_source11 = source11;
		_source12 = source12;
		_resultSelector = resultSelector;
	}

	protected override @_ CreateSink(IObserver<TResult> observer)
	{
		return new @_(_resultSelector, observer);
	}

	protected override void Run(@_ sink)
	{
		sink.Run(_source1, _source2, _source3, _source4, _source5, _source6, _source7, _source8, _source9, _source10, _source11, _source12);
	}
}
internal sealed class CombineLatest<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult> : Producer<TResult, CombineLatest<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>._>
{
	internal sealed class @_ : CombineLatestSink<TResult>
	{
		private readonly Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult> _resultSelector;

		private readonly CombineLatestObserver<T1> _observer1;

		private readonly CombineLatestObserver<T2> _observer2;

		private readonly CombineLatestObserver<T3> _observer3;

		private readonly CombineLatestObserver<T4> _observer4;

		private readonly CombineLatestObserver<T5> _observer5;

		private readonly CombineLatestObserver<T6> _observer6;

		private readonly CombineLatestObserver<T7> _observer7;

		private readonly CombineLatestObserver<T8> _observer8;

		private readonly CombineLatestObserver<T9> _observer9;

		private readonly CombineLatestObserver<T10> _observer10;

		private readonly CombineLatestObserver<T11> _observer11;

		private readonly CombineLatestObserver<T12> _observer12;

		private readonly CombineLatestObserver<T13> _observer13;

		public _(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult> resultSelector, IObserver<TResult> observer)
			: base(13, observer)
		{
			_resultSelector = resultSelector;
			_observer1 = new CombineLatestObserver<T1>(_gate, this, 0);
			_observer2 = new CombineLatestObserver<T2>(_gate, this, 1);
			_observer3 = new CombineLatestObserver<T3>(_gate, this, 2);
			_observer4 = new CombineLatestObserver<T4>(_gate, this, 3);
			_observer5 = new CombineLatestObserver<T5>(_gate, this, 4);
			_observer6 = new CombineLatestObserver<T6>(_gate, this, 5);
			_observer7 = new CombineLatestObserver<T7>(_gate, this, 6);
			_observer8 = new CombineLatestObserver<T8>(_gate, this, 7);
			_observer9 = new CombineLatestObserver<T9>(_gate, this, 8);
			_observer10 = new CombineLatestObserver<T10>(_gate, this, 9);
			_observer11 = new CombineLatestObserver<T11>(_gate, this, 10);
			_observer12 = new CombineLatestObserver<T12>(_gate, this, 11);
			_observer13 = new CombineLatestObserver<T13>(_gate, this, 12);
		}

		public void Run(IObservable<T1> source1, IObservable<T2> source2, IObservable<T3> source3, IObservable<T4> source4, IObservable<T5> source5, IObservable<T6> source6, IObservable<T7> source7, IObservable<T8> source8, IObservable<T9> source9, IObservable<T10> source10, IObservable<T11> source11, IObservable<T12> source12, IObservable<T13> source13)
		{
			IDisposable[] disposables = new IDisposable[13]
			{
				_observer1, _observer2, _observer3, _observer4, _observer5, _observer6, _observer7, _observer8, _observer9, _observer10,
				_observer11, _observer12, _observer13
			};
			_observer1.SetResource(source1.SubscribeSafe(_observer1));
			_observer2.SetResource(source2.SubscribeSafe(_observer2));
			_observer3.SetResource(source3.SubscribeSafe(_observer3));
			_observer4.SetResource(source4.SubscribeSafe(_observer4));
			_observer5.SetResource(source5.SubscribeSafe(_observer5));
			_observer6.SetResource(source6.SubscribeSafe(_observer6));
			_observer7.SetResource(source7.SubscribeSafe(_observer7));
			_observer8.SetResource(source8.SubscribeSafe(_observer8));
			_observer9.SetResource(source9.SubscribeSafe(_observer9));
			_observer10.SetResource(source10.SubscribeSafe(_observer10));
			_observer11.SetResource(source11.SubscribeSafe(_observer11));
			_observer12.SetResource(source12.SubscribeSafe(_observer12));
			_observer13.SetResource(source13.SubscribeSafe(_observer13));
			SetUpstream(StableCompositeDisposable.CreateTrusted(disposables));
		}

		protected override TResult GetResult()
		{
			return _resultSelector(_observer1.Value, _observer2.Value, _observer3.Value, _observer4.Value, _observer5.Value, _observer6.Value, _observer7.Value, _observer8.Value, _observer9.Value, _observer10.Value, _observer11.Value, _observer12.Value, _observer13.Value);
		}
	}

	private readonly IObservable<T1> _source1;

	private readonly IObservable<T2> _source2;

	private readonly IObservable<T3> _source3;

	private readonly IObservable<T4> _source4;

	private readonly IObservable<T5> _source5;

	private readonly IObservable<T6> _source6;

	private readonly IObservable<T7> _source7;

	private readonly IObservable<T8> _source8;

	private readonly IObservable<T9> _source9;

	private readonly IObservable<T10> _source10;

	private readonly IObservable<T11> _source11;

	private readonly IObservable<T12> _source12;

	private readonly IObservable<T13> _source13;

	private readonly Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult> _resultSelector;

	public CombineLatest(IObservable<T1> source1, IObservable<T2> source2, IObservable<T3> source3, IObservable<T4> source4, IObservable<T5> source5, IObservable<T6> source6, IObservable<T7> source7, IObservable<T8> source8, IObservable<T9> source9, IObservable<T10> source10, IObservable<T11> source11, IObservable<T12> source12, IObservable<T13> source13, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult> resultSelector)
	{
		_source1 = source1;
		_source2 = source2;
		_source3 = source3;
		_source4 = source4;
		_source5 = source5;
		_source6 = source6;
		_source7 = source7;
		_source8 = source8;
		_source9 = source9;
		_source10 = source10;
		_source11 = source11;
		_source12 = source12;
		_source13 = source13;
		_resultSelector = resultSelector;
	}

	protected override @_ CreateSink(IObserver<TResult> observer)
	{
		return new @_(_resultSelector, observer);
	}

	protected override void Run(@_ sink)
	{
		sink.Run(_source1, _source2, _source3, _source4, _source5, _source6, _source7, _source8, _source9, _source10, _source11, _source12, _source13);
	}
}
internal sealed class CombineLatest<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult> : Producer<TResult, CombineLatest<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>._>
{
	internal sealed class @_ : CombineLatestSink<TResult>
	{
		private readonly Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult> _resultSelector;

		private readonly CombineLatestObserver<T1> _observer1;

		private readonly CombineLatestObserver<T2> _observer2;

		private readonly CombineLatestObserver<T3> _observer3;

		private readonly CombineLatestObserver<T4> _observer4;

		private readonly CombineLatestObserver<T5> _observer5;

		private readonly CombineLatestObserver<T6> _observer6;

		private readonly CombineLatestObserver<T7> _observer7;

		private readonly CombineLatestObserver<T8> _observer8;

		private readonly CombineLatestObserver<T9> _observer9;

		private readonly CombineLatestObserver<T10> _observer10;

		private readonly CombineLatestObserver<T11> _observer11;

		private readonly CombineLatestObserver<T12> _observer12;

		private readonly CombineLatestObserver<T13> _observer13;

		private readonly CombineLatestObserver<T14> _observer14;

		public _(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult> resultSelector, IObserver<TResult> observer)
			: base(14, observer)
		{
			_resultSelector = resultSelector;
			_observer1 = new CombineLatestObserver<T1>(_gate, this, 0);
			_observer2 = new CombineLatestObserver<T2>(_gate, this, 1);
			_observer3 = new CombineLatestObserver<T3>(_gate, this, 2);
			_observer4 = new CombineLatestObserver<T4>(_gate, this, 3);
			_observer5 = new CombineLatestObserver<T5>(_gate, this, 4);
			_observer6 = new CombineLatestObserver<T6>(_gate, this, 5);
			_observer7 = new CombineLatestObserver<T7>(_gate, this, 6);
			_observer8 = new CombineLatestObserver<T8>(_gate, this, 7);
			_observer9 = new CombineLatestObserver<T9>(_gate, this, 8);
			_observer10 = new CombineLatestObserver<T10>(_gate, this, 9);
			_observer11 = new CombineLatestObserver<T11>(_gate, this, 10);
			_observer12 = new CombineLatestObserver<T12>(_gate, this, 11);
			_observer13 = new CombineLatestObserver<T13>(_gate, this, 12);
			_observer14 = new CombineLatestObserver<T14>(_gate, this, 13);
		}

		public void Run(IObservable<T1> source1, IObservable<T2> source2, IObservable<T3> source3, IObservable<T4> source4, IObservable<T5> source5, IObservable<T6> source6, IObservable<T7> source7, IObservable<T8> source8, IObservable<T9> source9, IObservable<T10> source10, IObservable<T11> source11, IObservable<T12> source12, IObservable<T13> source13, IObservable<T14> source14)
		{
			IDisposable[] disposables = new IDisposable[14]
			{
				_observer1, _observer2, _observer3, _observer4, _observer5, _observer6, _observer7, _observer8, _observer9, _observer10,
				_observer11, _observer12, _observer13, _observer14
			};
			_observer1.SetResource(source1.SubscribeSafe(_observer1));
			_observer2.SetResource(source2.SubscribeSafe(_observer2));
			_observer3.SetResource(source3.SubscribeSafe(_observer3));
			_observer4.SetResource(source4.SubscribeSafe(_observer4));
			_observer5.SetResource(source5.SubscribeSafe(_observer5));
			_observer6.SetResource(source6.SubscribeSafe(_observer6));
			_observer7.SetResource(source7.SubscribeSafe(_observer7));
			_observer8.SetResource(source8.SubscribeSafe(_observer8));
			_observer9.SetResource(source9.SubscribeSafe(_observer9));
			_observer10.SetResource(source10.SubscribeSafe(_observer10));
			_observer11.SetResource(source11.SubscribeSafe(_observer11));
			_observer12.SetResource(source12.SubscribeSafe(_observer12));
			_observer13.SetResource(source13.SubscribeSafe(_observer13));
			_observer14.SetResource(source14.SubscribeSafe(_observer14));
			SetUpstream(StableCompositeDisposable.CreateTrusted(disposables));
		}

		protected override TResult GetResult()
		{
			return _resultSelector(_observer1.Value, _observer2.Value, _observer3.Value, _observer4.Value, _observer5.Value, _observer6.Value, _observer7.Value, _observer8.Value, _observer9.Value, _observer10.Value, _observer11.Value, _observer12.Value, _observer13.Value, _observer14.Value);
		}
	}

	private readonly IObservable<T1> _source1;

	private readonly IObservable<T2> _source2;

	private readonly IObservable<T3> _source3;

	private readonly IObservable<T4> _source4;

	private readonly IObservable<T5> _source5;

	private readonly IObservable<T6> _source6;

	private readonly IObservable<T7> _source7;

	private readonly IObservable<T8> _source8;

	private readonly IObservable<T9> _source9;

	private readonly IObservable<T10> _source10;

	private readonly IObservable<T11> _source11;

	private readonly IObservable<T12> _source12;

	private readonly IObservable<T13> _source13;

	private readonly IObservable<T14> _source14;

	private readonly Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult> _resultSelector;

	public CombineLatest(IObservable<T1> source1, IObservable<T2> source2, IObservable<T3> source3, IObservable<T4> source4, IObservable<T5> source5, IObservable<T6> source6, IObservable<T7> source7, IObservable<T8> source8, IObservable<T9> source9, IObservable<T10> source10, IObservable<T11> source11, IObservable<T12> source12, IObservable<T13> source13, IObservable<T14> source14, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult> resultSelector)
	{
		_source1 = source1;
		_source2 = source2;
		_source3 = source3;
		_source4 = source4;
		_source5 = source5;
		_source6 = source6;
		_source7 = source7;
		_source8 = source8;
		_source9 = source9;
		_source10 = source10;
		_source11 = source11;
		_source12 = source12;
		_source13 = source13;
		_source14 = source14;
		_resultSelector = resultSelector;
	}

	protected override @_ CreateSink(IObserver<TResult> observer)
	{
		return new @_(_resultSelector, observer);
	}

	protected override void Run(@_ sink)
	{
		sink.Run(_source1, _source2, _source3, _source4, _source5, _source6, _source7, _source8, _source9, _source10, _source11, _source12, _source13, _source14);
	}
}
internal sealed class CombineLatest<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult> : Producer<TResult, CombineLatest<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>._>
{
	internal sealed class @_ : CombineLatestSink<TResult>
	{
		private readonly Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult> _resultSelector;

		private readonly CombineLatestObserver<T1> _observer1;

		private readonly CombineLatestObserver<T2> _observer2;

		private readonly CombineLatestObserver<T3> _observer3;

		private readonly CombineLatestObserver<T4> _observer4;

		private readonly CombineLatestObserver<T5> _observer5;

		private readonly CombineLatestObserver<T6> _observer6;

		private readonly CombineLatestObserver<T7> _observer7;

		private readonly CombineLatestObserver<T8> _observer8;

		private readonly CombineLatestObserver<T9> _observer9;

		private readonly CombineLatestObserver<T10> _observer10;

		private readonly CombineLatestObserver<T11> _observer11;

		private readonly CombineLatestObserver<T12> _observer12;

		private readonly CombineLatestObserver<T13> _observer13;

		private readonly CombineLatestObserver<T14> _observer14;

		private readonly CombineLatestObserver<T15> _observer15;

		public _(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult> resultSelector, IObserver<TResult> observer)
			: base(15, observer)
		{
			_resultSelector = resultSelector;
			_observer1 = new CombineLatestObserver<T1>(_gate, this, 0);
			_observer2 = new CombineLatestObserver<T2>(_gate, this, 1);
			_observer3 = new CombineLatestObserver<T3>(_gate, this, 2);
			_observer4 = new CombineLatestObserver<T4>(_gate, this, 3);
			_observer5 = new CombineLatestObserver<T5>(_gate, this, 4);
			_observer6 = new CombineLatestObserver<T6>(_gate, this, 5);
			_observer7 = new CombineLatestObserver<T7>(_gate, this, 6);
			_observer8 = new CombineLatestObserver<T8>(_gate, this, 7);
			_observer9 = new CombineLatestObserver<T9>(_gate, this, 8);
			_observer10 = new CombineLatestObserver<T10>(_gate, this, 9);
			_observer11 = new CombineLatestObserver<T11>(_gate, this, 10);
			_observer12 = new CombineLatestObserver<T12>(_gate, this, 11);
			_observer13 = new CombineLatestObserver<T13>(_gate, this, 12);
			_observer14 = new CombineLatestObserver<T14>(_gate, this, 13);
			_observer15 = new CombineLatestObserver<T15>(_gate, this, 14);
		}

		public void Run(IObservable<T1> source1, IObservable<T2> source2, IObservable<T3> source3, IObservable<T4> source4, IObservable<T5> source5, IObservable<T6> source6, IObservable<T7> source7, IObservable<T8> source8, IObservable<T9> source9, IObservable<T10> source10, IObservable<T11> source11, IObservable<T12> source12, IObservable<T13> source13, IObservable<T14> source14, IObservable<T15> source15)
		{
			IDisposable[] disposables = new IDisposable[15]
			{
				_observer1, _observer2, _observer3, _observer4, _observer5, _observer6, _observer7, _observer8, _observer9, _observer10,
				_observer11, _observer12, _observer13, _observer14, _observer15
			};
			_observer1.SetResource(source1.SubscribeSafe(_observer1));
			_observer2.SetResource(source2.SubscribeSafe(_observer2));
			_observer3.SetResource(source3.SubscribeSafe(_observer3));
			_observer4.SetResource(source4.SubscribeSafe(_observer4));
			_observer5.SetResource(source5.SubscribeSafe(_observer5));
			_observer6.SetResource(source6.SubscribeSafe(_observer6));
			_observer7.SetResource(source7.SubscribeSafe(_observer7));
			_observer8.SetResource(source8.SubscribeSafe(_observer8));
			_observer9.SetResource(source9.SubscribeSafe(_observer9));
			_observer10.SetResource(source10.SubscribeSafe(_observer10));
			_observer11.SetResource(source11.SubscribeSafe(_observer11));
			_observer12.SetResource(source12.SubscribeSafe(_observer12));
			_observer13.SetResource(source13.SubscribeSafe(_observer13));
			_observer14.SetResource(source14.SubscribeSafe(_observer14));
			_observer15.SetResource(source15.SubscribeSafe(_observer15));
			SetUpstream(StableCompositeDisposable.CreateTrusted(disposables));
		}

		protected override TResult GetResult()
		{
			return _resultSelector(_observer1.Value, _observer2.Value, _observer3.Value, _observer4.Value, _observer5.Value, _observer6.Value, _observer7.Value, _observer8.Value, _observer9.Value, _observer10.Value, _observer11.Value, _observer12.Value, _observer13.Value, _observer14.Value, _observer15.Value);
		}
	}

	private readonly IObservable<T1> _source1;

	private readonly IObservable<T2> _source2;

	private readonly IObservable<T3> _source3;

	private readonly IObservable<T4> _source4;

	private readonly IObservable<T5> _source5;

	private readonly IObservable<T6> _source6;

	private readonly IObservable<T7> _source7;

	private readonly IObservable<T8> _source8;

	private readonly IObservable<T9> _source9;

	private readonly IObservable<T10> _source10;

	private readonly IObservable<T11> _source11;

	private readonly IObservable<T12> _source12;

	private readonly IObservable<T13> _source13;

	private readonly IObservable<T14> _source14;

	private readonly IObservable<T15> _source15;

	private readonly Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult> _resultSelector;

	public CombineLatest(IObservable<T1> source1, IObservable<T2> source2, IObservable<T3> source3, IObservable<T4> source4, IObservable<T5> source5, IObservable<T6> source6, IObservable<T7> source7, IObservable<T8> source8, IObservable<T9> source9, IObservable<T10> source10, IObservable<T11> source11, IObservable<T12> source12, IObservable<T13> source13, IObservable<T14> source14, IObservable<T15> source15, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult> resultSelector)
	{
		_source1 = source1;
		_source2 = source2;
		_source3 = source3;
		_source4 = source4;
		_source5 = source5;
		_source6 = source6;
		_source7 = source7;
		_source8 = source8;
		_source9 = source9;
		_source10 = source10;
		_source11 = source11;
		_source12 = source12;
		_source13 = source13;
		_source14 = source14;
		_source15 = source15;
		_resultSelector = resultSelector;
	}

	protected override @_ CreateSink(IObserver<TResult> observer)
	{
		return new @_(_resultSelector, observer);
	}

	protected override void Run(@_ sink)
	{
		sink.Run(_source1, _source2, _source3, _source4, _source5, _source6, _source7, _source8, _source9, _source10, _source11, _source12, _source13, _source14, _source15);
	}
}
internal sealed class CombineLatest<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult> : Producer<TResult, CombineLatest<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult>._>
{
	internal sealed class @_ : CombineLatestSink<TResult>
	{
		private readonly Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult> _resultSelector;

		private readonly CombineLatestObserver<T1> _observer1;

		private readonly CombineLatestObserver<T2> _observer2;

		private readonly CombineLatestObserver<T3> _observer3;

		private readonly CombineLatestObserver<T4> _observer4;

		private readonly CombineLatestObserver<T5> _observer5;

		private readonly CombineLatestObserver<T6> _observer6;

		private readonly CombineLatestObserver<T7> _observer7;

		private readonly CombineLatestObserver<T8> _observer8;

		private readonly CombineLatestObserver<T9> _observer9;

		private readonly CombineLatestObserver<T10> _observer10;

		private readonly CombineLatestObserver<T11> _observer11;

		private readonly CombineLatestObserver<T12> _observer12;

		private readonly CombineLatestObserver<T13> _observer13;

		private readonly CombineLatestObserver<T14> _observer14;

		private readonly CombineLatestObserver<T15> _observer15;

		private readonly CombineLatestObserver<T16> _observer16;

		public _(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult> resultSelector, IObserver<TResult> observer)
			: base(16, observer)
		{
			_resultSelector = resultSelector;
			_observer1 = new CombineLatestObserver<T1>(_gate, this, 0);
			_observer2 = new CombineLatestObserver<T2>(_gate, this, 1);
			_observer3 = new CombineLatestObserver<T3>(_gate, this, 2);
			_observer4 = new CombineLatestObserver<T4>(_gate, this, 3);
			_observer5 = new CombineLatestObserver<T5>(_gate, this, 4);
			_observer6 = new CombineLatestObserver<T6>(_gate, this, 5);
			_observer7 = new CombineLatestObserver<T7>(_gate, this, 6);
			_observer8 = new CombineLatestObserver<T8>(_gate, this, 7);
			_observer9 = new CombineLatestObserver<T9>(_gate, this, 8);
			_observer10 = new CombineLatestObserver<T10>(_gate, this, 9);
			_observer11 = new CombineLatestObserver<T11>(_gate, this, 10);
			_observer12 = new CombineLatestObserver<T12>(_gate, this, 11);
			_observer13 = new CombineLatestObserver<T13>(_gate, this, 12);
			_observer14 = new CombineLatestObserver<T14>(_gate, this, 13);
			_observer15 = new CombineLatestObserver<T15>(_gate, this, 14);
			_observer16 = new CombineLatestObserver<T16>(_gate, this, 15);
		}

		public void Run(IObservable<T1> source1, IObservable<T2> source2, IObservable<T3> source3, IObservable<T4> source4, IObservable<T5> source5, IObservable<T6> source6, IObservable<T7> source7, IObservable<T8> source8, IObservable<T9> source9, IObservable<T10> source10, IObservable<T11> source11, IObservable<T12> source12, IObservable<T13> source13, IObservable<T14> source14, IObservable<T15> source15, IObservable<T16> source16)
		{
			IDisposable[] disposables = new IDisposable[16]
			{
				_observer1, _observer2, _observer3, _observer4, _observer5, _observer6, _observer7, _observer8, _observer9, _observer10,
				_observer11, _observer12, _observer13, _observer14, _observer15, _observer16
			};
			_observer1.SetResource(source1.SubscribeSafe(_observer1));
			_observer2.SetResource(source2.SubscribeSafe(_observer2));
			_observer3.SetResource(source3.SubscribeSafe(_observer3));
			_observer4.SetResource(source4.SubscribeSafe(_observer4));
			_observer5.SetResource(source5.SubscribeSafe(_observer5));
			_observer6.SetResource(source6.SubscribeSafe(_observer6));
			_observer7.SetResource(source7.SubscribeSafe(_observer7));
			_observer8.SetResource(source8.SubscribeSafe(_observer8));
			_observer9.SetResource(source9.SubscribeSafe(_observer9));
			_observer10.SetResource(source10.SubscribeSafe(_observer10));
			_observer11.SetResource(source11.SubscribeSafe(_observer11));
			_observer12.SetResource(source12.SubscribeSafe(_observer12));
			_observer13.SetResource(source13.SubscribeSafe(_observer13));
			_observer14.SetResource(source14.SubscribeSafe(_observer14));
			_observer15.SetResource(source15.SubscribeSafe(_observer15));
			_observer16.SetResource(source16.SubscribeSafe(_observer16));
			SetUpstream(StableCompositeDisposable.CreateTrusted(disposables));
		}

		protected override TResult GetResult()
		{
			return _resultSelector(_observer1.Value, _observer2.Value, _observer3.Value, _observer4.Value, _observer5.Value, _observer6.Value, _observer7.Value, _observer8.Value, _observer9.Value, _observer10.Value, _observer11.Value, _observer12.Value, _observer13.Value, _observer14.Value, _observer15.Value, _observer16.Value);
		}
	}

	private readonly IObservable<T1> _source1;

	private readonly IObservable<T2> _source2;

	private readonly IObservable<T3> _source3;

	private readonly IObservable<T4> _source4;

	private readonly IObservable<T5> _source5;

	private readonly IObservable<T6> _source6;

	private readonly IObservable<T7> _source7;

	private readonly IObservable<T8> _source8;

	private readonly IObservable<T9> _source9;

	private readonly IObservable<T10> _source10;

	private readonly IObservable<T11> _source11;

	private readonly IObservable<T12> _source12;

	private readonly IObservable<T13> _source13;

	private readonly IObservable<T14> _source14;

	private readonly IObservable<T15> _source15;

	private readonly IObservable<T16> _source16;

	private readonly Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult> _resultSelector;

	public CombineLatest(IObservable<T1> source1, IObservable<T2> source2, IObservable<T3> source3, IObservable<T4> source4, IObservable<T5> source5, IObservable<T6> source6, IObservable<T7> source7, IObservable<T8> source8, IObservable<T9> source9, IObservable<T10> source10, IObservable<T11> source11, IObservable<T12> source12, IObservable<T13> source13, IObservable<T14> source14, IObservable<T15> source15, IObservable<T16> source16, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult> resultSelector)
	{
		_source1 = source1;
		_source2 = source2;
		_source3 = source3;
		_source4 = source4;
		_source5 = source5;
		_source6 = source6;
		_source7 = source7;
		_source8 = source8;
		_source9 = source9;
		_source10 = source10;
		_source11 = source11;
		_source12 = source12;
		_source13 = source13;
		_source14 = source14;
		_source15 = source15;
		_source16 = source16;
		_resultSelector = resultSelector;
	}

	protected override @_ CreateSink(IObserver<TResult> observer)
	{
		return new @_(_resultSelector, observer);
	}

	protected override void Run(@_ sink)
	{
		sink.Run(_source1, _source2, _source3, _source4, _source5, _source6, _source7, _source8, _source9, _source10, _source11, _source12, _source13, _source14, _source15, _source16);
	}
}
