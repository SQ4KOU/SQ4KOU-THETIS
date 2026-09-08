using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading;

namespace System.Reactive.Linq.ObservableImpl;

internal static class Zip<TFirst, TSecond, TResult>
{
	internal sealed class Observable : Producer<TResult, Observable._>
	{
		internal sealed class @_ : IdentitySink<TResult>
		{
			private sealed class FirstObserver : IObserver<TFirst>, IDisposable
			{
				private readonly @_ _parent;

				private readonly Queue<TFirst> _queue;

				private SecondObserver _other;

				public Queue<TFirst> Queue => _queue;

				public bool Done { get; private set; }

				public FirstObserver(@_ parent)
				{
					_parent = parent;
					_queue = new Queue<TFirst>();
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
						if (_other.Queue.Count > 0)
						{
							TSecond arg = _other.Queue.Dequeue();
							TResult value2;
							try
							{
								value2 = _parent._resultSelector(value, arg);
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
						else
						{
							_queue.Enqueue(value);
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

				public void Dispose()
				{
					_queue.Clear();
				}
			}

			private sealed class SecondObserver : IObserver<TSecond>, IDisposable
			{
				private readonly @_ _parent;

				private readonly Queue<TSecond> _queue;

				private FirstObserver _other;

				public Queue<TSecond> Queue => _queue;

				public bool Done { get; private set; }

				public SecondObserver(@_ parent)
				{
					_parent = parent;
					_queue = new Queue<TSecond>();
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
						if (_other.Queue.Count > 0)
						{
							TFirst arg = _other.Queue.Dequeue();
							TResult value2;
							try
							{
								value2 = _parent._resultSelector(arg, value);
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
						else
						{
							_queue.Enqueue(value);
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

				public void Dispose()
				{
					_queue.Clear();
				}
			}

			private readonly Func<TFirst, TSecond, TResult> _resultSelector;

			private readonly object _gate;

			private readonly FirstObserver _firstObserver;

			private SingleAssignmentDisposableValue _firstDisposable;

			private readonly SecondObserver _secondObserver;

			private SingleAssignmentDisposableValue _secondDisposable;

			public _(Func<TFirst, TSecond, TResult> resultSelector, IObserver<TResult> observer)
				: base(observer)
			{
				_gate = new object();
				_firstObserver = new FirstObserver(this);
				_secondObserver = new SecondObserver(this);
				_firstObserver.SetOther(_secondObserver);
				_secondObserver.SetOther(_firstObserver);
				_resultSelector = resultSelector;
			}

			public void Run(IObservable<TFirst> first, IObservable<TSecond> second)
			{
				_firstDisposable.Disposable = first.SubscribeSafe(_firstObserver);
				_secondDisposable.Disposable = second.SubscribeSafe(_secondObserver);
			}

			protected override void Dispose(bool disposing)
			{
				if (disposing)
				{
					_firstDisposable.Dispose();
					_secondDisposable.Dispose();
					lock (_gate)
					{
						_firstObserver.Dispose();
						_secondObserver.Dispose();
					}
				}
				base.Dispose(disposing);
			}
		}

		private readonly IObservable<TFirst> _first;

		private readonly IObservable<TSecond> _second;

		private readonly Func<TFirst, TSecond, TResult> _resultSelector;

		public Observable(IObservable<TFirst> first, IObservable<TSecond> second, Func<TFirst, TSecond, TResult> resultSelector)
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

	internal sealed class Enumerable : Producer<TResult, Enumerable._>
	{
		internal sealed class @_ : Sink<TFirst, TResult>
		{
			private readonly Func<TFirst, TSecond, TResult> _resultSelector;

			private int _enumerationInProgress;

			private IEnumerator<TSecond>? _rightEnumerator;

			private static readonly IEnumerator<TSecond> DisposedEnumerator = MakeDisposedEnumerator();

			public _(Func<TFirst, TSecond, TResult> resultSelector, IObserver<TResult> observer)
				: base(observer)
			{
				_resultSelector = resultSelector;
			}

			private static IEnumerator<TSecond> MakeDisposedEnumerator()
			{
				yield break;
			}

			public void Run(IObservable<TFirst> first, IEnumerable<TSecond> second)
			{
				try
				{
					IEnumerator<TSecond> enumerator = second.GetEnumerator();
					if (Interlocked.CompareExchange(ref _rightEnumerator, enumerator, null) != null)
					{
						enumerator.Dispose();
						return;
					}
				}
				catch (Exception error)
				{
					ForwardOnError(error);
					return;
				}
				Run(first);
			}

			protected override void Dispose(bool disposing)
			{
				if (disposing && Interlocked.Increment(ref _enumerationInProgress) == 1)
				{
					Interlocked.Exchange(ref _rightEnumerator, DisposedEnumerator)?.Dispose();
				}
				base.Dispose(disposing);
			}

			public override void OnNext(TFirst value)
			{
				IEnumerator<TSecond> enumerator = Volatile.Read(ref _rightEnumerator);
				if (enumerator == DisposedEnumerator || Interlocked.Increment(ref _enumerationInProgress) != 1)
				{
					return;
				}
				TSecond arg = default(TSecond);
				bool flag = false;
				bool flag2;
				try
				{
					try
					{
						flag2 = enumerator.MoveNext();
						if (flag2)
						{
							arg = enumerator.Current;
						}
					}
					finally
					{
						if (Interlocked.Decrement(ref _enumerationInProgress) != 0)
						{
							Interlocked.Exchange(ref _rightEnumerator, DisposedEnumerator)?.Dispose();
							flag = true;
						}
					}
				}
				catch (Exception error)
				{
					ForwardOnError(error);
					return;
				}
				if (flag)
				{
					return;
				}
				if (flag2)
				{
					TResult value2;
					try
					{
						value2 = _resultSelector(value, arg);
					}
					catch (Exception error2)
					{
						ForwardOnError(error2);
						return;
					}
					ForwardOnNext(value2);
				}
				else
				{
					ForwardOnCompleted();
				}
			}
		}

		private readonly IObservable<TFirst> _first;

		private readonly IEnumerable<TSecond> _second;

		private readonly Func<TFirst, TSecond, TResult> _resultSelector;

		public Enumerable(IObservable<TFirst> first, IEnumerable<TSecond> second, Func<TFirst, TSecond, TResult> resultSelector)
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
}
internal sealed class Zip<TSource> : Producer<IList<TSource>, Zip<TSource>._>
{
	internal sealed class @_ : IdentitySink<IList<TSource>>
	{
		private sealed class SourceObserver : IObserver<TSource>
		{
			private readonly @_ _parent;

			private readonly int _index;

			public SourceObserver(@_ parent, int index)
			{
				_parent = parent;
				_index = index;
			}

			public void OnNext(TSource value)
			{
				_parent.OnNext(_index, value);
			}

			public void OnError(Exception error)
			{
				_parent.OnError(error);
			}

			public void OnCompleted()
			{
				_parent.OnCompleted(_index);
			}
		}

		private readonly object _gate;

		private Queue<TSource>[] _queues;

		private bool[] _isDone;

		private SingleAssignmentDisposableValue[]? _subscriptions;

		public _(IObserver<IList<TSource>> observer)
			: base(observer)
		{
			_gate = new object();
			_queues = null;
			_isDone = null;
		}

		public void Run(IEnumerable<IObservable<TSource>> sources)
		{
			IObservable<TSource>[] array = sources.ToArray();
			int num = array.Length;
			_queues = new Queue<TSource>[num];
			for (int i = 0; i < num; i++)
			{
				_queues[i] = new Queue<TSource>();
			}
			_isDone = new bool[num];
			SingleAssignmentDisposableValue[] array2 = new SingleAssignmentDisposableValue[num];
			if (Interlocked.CompareExchange(ref _subscriptions, array2, null) == null)
			{
				for (int j = 0; j < num; j++)
				{
					SourceObserver observer = new SourceObserver(this, j);
					array2[j].Disposable = array[j].SubscribeSafe(observer);
				}
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				SingleAssignmentDisposableValue[] array = Interlocked.Exchange(ref _subscriptions, Array.Empty<SingleAssignmentDisposableValue>());
				if (array != null && array != Array.Empty<SingleAssignmentDisposableValue>())
				{
					for (int i = 0; i < array.Length; i++)
					{
						array[i].Dispose();
					}
					lock (_gate)
					{
						Queue<TSource>[] queues = _queues;
						for (int j = 0; j < queues.Length; j++)
						{
							queues[j].Clear();
						}
					}
				}
			}
			base.Dispose(disposing);
		}

		private void OnNext(int index, TSource value)
		{
			lock (_gate)
			{
				_queues[index].Enqueue(value);
				if (_queues.All((Queue<TSource> q) => q.Count > 0))
				{
					int num = _queues.Length;
					List<TSource> list = new List<TSource>(num);
					for (int num2 = 0; num2 < num; num2++)
					{
						list.Add(_queues[num2].Dequeue());
					}
					ForwardOnNext(list);
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
					return;
				}
				SingleAssignmentDisposableValue[] array = Volatile.Read(ref _subscriptions);
				if (array != null && array != Array.Empty<SingleAssignmentDisposableValue>())
				{
					array[index].Dispose();
				}
			}
		}
	}

	private readonly IEnumerable<IObservable<TSource>> _sources;

	public Zip(IEnumerable<IObservable<TSource>> sources)
	{
		_sources = sources;
	}

	protected override @_ CreateSink(IObserver<IList<TSource>> observer)
	{
		return new @_(observer);
	}

	protected override void Run(@_ sink)
	{
		sink.Run(_sources);
	}
}
internal sealed class Zip<T1, T2, T3, TResult> : Producer<TResult, Zip<T1, T2, T3, TResult>._>
{
	internal sealed class @_ : ZipSink<TResult>
	{
		private readonly Func<T1, T2, T3, TResult> _resultSelector;

		private readonly ZipObserver<T1> _observer1;

		private readonly ZipObserver<T2> _observer2;

		private readonly ZipObserver<T3> _observer3;

		public _(Func<T1, T2, T3, TResult> resultSelector, IObserver<TResult> observer)
			: base(3, observer)
		{
			_resultSelector = resultSelector;
			_observer1 = new ZipObserver<T1>(_gate, this, 0);
			_observer2 = new ZipObserver<T2>(_gate, this, 1);
			_observer3 = new ZipObserver<T3>(_gate, this, 2);
		}

		public void Run(IObservable<T1> source1, IObservable<T2> source2, IObservable<T3> source3)
		{
			IDisposable[] array = new IDisposable[3] { _observer1, null, null };
			base.Queues[0] = _observer1.Values;
			array[1] = _observer2;
			base.Queues[1] = _observer2.Values;
			array[2] = _observer3;
			base.Queues[2] = _observer3.Values;
			_observer1.SetResource(source1.SubscribeSafe(_observer1));
			_observer2.SetResource(source2.SubscribeSafe(_observer2));
			_observer3.SetResource(source3.SubscribeSafe(_observer3));
			SetUpstream(StableCompositeDisposable.CreateTrusted(array));
		}

		protected override TResult GetResult()
		{
			return _resultSelector(_observer1.Values.Dequeue(), _observer2.Values.Dequeue(), _observer3.Values.Dequeue());
		}
	}

	private readonly IObservable<T1> _source1;

	private readonly IObservable<T2> _source2;

	private readonly IObservable<T3> _source3;

	private readonly Func<T1, T2, T3, TResult> _resultSelector;

	public Zip(IObservable<T1> source1, IObservable<T2> source2, IObservable<T3> source3, Func<T1, T2, T3, TResult> resultSelector)
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
internal sealed class Zip<T1, T2, T3, T4, TResult> : Producer<TResult, Zip<T1, T2, T3, T4, TResult>._>
{
	internal sealed class @_ : ZipSink<TResult>
	{
		private readonly Func<T1, T2, T3, T4, TResult> _resultSelector;

		private readonly ZipObserver<T1> _observer1;

		private readonly ZipObserver<T2> _observer2;

		private readonly ZipObserver<T3> _observer3;

		private readonly ZipObserver<T4> _observer4;

		public _(Func<T1, T2, T3, T4, TResult> resultSelector, IObserver<TResult> observer)
			: base(4, observer)
		{
			_resultSelector = resultSelector;
			_observer1 = new ZipObserver<T1>(_gate, this, 0);
			_observer2 = new ZipObserver<T2>(_gate, this, 1);
			_observer3 = new ZipObserver<T3>(_gate, this, 2);
			_observer4 = new ZipObserver<T4>(_gate, this, 3);
		}

		public void Run(IObservable<T1> source1, IObservable<T2> source2, IObservable<T3> source3, IObservable<T4> source4)
		{
			IDisposable[] array = new IDisposable[4] { _observer1, null, null, null };
			base.Queues[0] = _observer1.Values;
			array[1] = _observer2;
			base.Queues[1] = _observer2.Values;
			array[2] = _observer3;
			base.Queues[2] = _observer3.Values;
			array[3] = _observer4;
			base.Queues[3] = _observer4.Values;
			_observer1.SetResource(source1.SubscribeSafe(_observer1));
			_observer2.SetResource(source2.SubscribeSafe(_observer2));
			_observer3.SetResource(source3.SubscribeSafe(_observer3));
			_observer4.SetResource(source4.SubscribeSafe(_observer4));
			SetUpstream(StableCompositeDisposable.CreateTrusted(array));
		}

		protected override TResult GetResult()
		{
			return _resultSelector(_observer1.Values.Dequeue(), _observer2.Values.Dequeue(), _observer3.Values.Dequeue(), _observer4.Values.Dequeue());
		}
	}

	private readonly IObservable<T1> _source1;

	private readonly IObservable<T2> _source2;

	private readonly IObservable<T3> _source3;

	private readonly IObservable<T4> _source4;

	private readonly Func<T1, T2, T3, T4, TResult> _resultSelector;

	public Zip(IObservable<T1> source1, IObservable<T2> source2, IObservable<T3> source3, IObservable<T4> source4, Func<T1, T2, T3, T4, TResult> resultSelector)
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
internal sealed class Zip<T1, T2, T3, T4, T5, TResult> : Producer<TResult, Zip<T1, T2, T3, T4, T5, TResult>._>
{
	internal sealed class @_ : ZipSink<TResult>
	{
		private readonly Func<T1, T2, T3, T4, T5, TResult> _resultSelector;

		private readonly ZipObserver<T1> _observer1;

		private readonly ZipObserver<T2> _observer2;

		private readonly ZipObserver<T3> _observer3;

		private readonly ZipObserver<T4> _observer4;

		private readonly ZipObserver<T5> _observer5;

		public _(Func<T1, T2, T3, T4, T5, TResult> resultSelector, IObserver<TResult> observer)
			: base(5, observer)
		{
			_resultSelector = resultSelector;
			_observer1 = new ZipObserver<T1>(_gate, this, 0);
			_observer2 = new ZipObserver<T2>(_gate, this, 1);
			_observer3 = new ZipObserver<T3>(_gate, this, 2);
			_observer4 = new ZipObserver<T4>(_gate, this, 3);
			_observer5 = new ZipObserver<T5>(_gate, this, 4);
		}

		public void Run(IObservable<T1> source1, IObservable<T2> source2, IObservable<T3> source3, IObservable<T4> source4, IObservable<T5> source5)
		{
			IDisposable[] array = new IDisposable[5] { _observer1, null, null, null, null };
			base.Queues[0] = _observer1.Values;
			array[1] = _observer2;
			base.Queues[1] = _observer2.Values;
			array[2] = _observer3;
			base.Queues[2] = _observer3.Values;
			array[3] = _observer4;
			base.Queues[3] = _observer4.Values;
			array[4] = _observer5;
			base.Queues[4] = _observer5.Values;
			_observer1.SetResource(source1.SubscribeSafe(_observer1));
			_observer2.SetResource(source2.SubscribeSafe(_observer2));
			_observer3.SetResource(source3.SubscribeSafe(_observer3));
			_observer4.SetResource(source4.SubscribeSafe(_observer4));
			_observer5.SetResource(source5.SubscribeSafe(_observer5));
			SetUpstream(StableCompositeDisposable.CreateTrusted(array));
		}

		protected override TResult GetResult()
		{
			return _resultSelector(_observer1.Values.Dequeue(), _observer2.Values.Dequeue(), _observer3.Values.Dequeue(), _observer4.Values.Dequeue(), _observer5.Values.Dequeue());
		}
	}

	private readonly IObservable<T1> _source1;

	private readonly IObservable<T2> _source2;

	private readonly IObservable<T3> _source3;

	private readonly IObservable<T4> _source4;

	private readonly IObservable<T5> _source5;

	private readonly Func<T1, T2, T3, T4, T5, TResult> _resultSelector;

	public Zip(IObservable<T1> source1, IObservable<T2> source2, IObservable<T3> source3, IObservable<T4> source4, IObservable<T5> source5, Func<T1, T2, T3, T4, T5, TResult> resultSelector)
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
internal sealed class Zip<T1, T2, T3, T4, T5, T6, TResult> : Producer<TResult, Zip<T1, T2, T3, T4, T5, T6, TResult>._>
{
	internal sealed class @_ : ZipSink<TResult>
	{
		private readonly Func<T1, T2, T3, T4, T5, T6, TResult> _resultSelector;

		private readonly ZipObserver<T1> _observer1;

		private readonly ZipObserver<T2> _observer2;

		private readonly ZipObserver<T3> _observer3;

		private readonly ZipObserver<T4> _observer4;

		private readonly ZipObserver<T5> _observer5;

		private readonly ZipObserver<T6> _observer6;

		public _(Func<T1, T2, T3, T4, T5, T6, TResult> resultSelector, IObserver<TResult> observer)
			: base(6, observer)
		{
			_resultSelector = resultSelector;
			_observer1 = new ZipObserver<T1>(_gate, this, 0);
			_observer2 = new ZipObserver<T2>(_gate, this, 1);
			_observer3 = new ZipObserver<T3>(_gate, this, 2);
			_observer4 = new ZipObserver<T4>(_gate, this, 3);
			_observer5 = new ZipObserver<T5>(_gate, this, 4);
			_observer6 = new ZipObserver<T6>(_gate, this, 5);
		}

		public void Run(IObservable<T1> source1, IObservable<T2> source2, IObservable<T3> source3, IObservable<T4> source4, IObservable<T5> source5, IObservable<T6> source6)
		{
			IDisposable[] array = new IDisposable[6] { _observer1, null, null, null, null, null };
			base.Queues[0] = _observer1.Values;
			array[1] = _observer2;
			base.Queues[1] = _observer2.Values;
			array[2] = _observer3;
			base.Queues[2] = _observer3.Values;
			array[3] = _observer4;
			base.Queues[3] = _observer4.Values;
			array[4] = _observer5;
			base.Queues[4] = _observer5.Values;
			array[5] = _observer6;
			base.Queues[5] = _observer6.Values;
			_observer1.SetResource(source1.SubscribeSafe(_observer1));
			_observer2.SetResource(source2.SubscribeSafe(_observer2));
			_observer3.SetResource(source3.SubscribeSafe(_observer3));
			_observer4.SetResource(source4.SubscribeSafe(_observer4));
			_observer5.SetResource(source5.SubscribeSafe(_observer5));
			_observer6.SetResource(source6.SubscribeSafe(_observer6));
			SetUpstream(StableCompositeDisposable.CreateTrusted(array));
		}

		protected override TResult GetResult()
		{
			return _resultSelector(_observer1.Values.Dequeue(), _observer2.Values.Dequeue(), _observer3.Values.Dequeue(), _observer4.Values.Dequeue(), _observer5.Values.Dequeue(), _observer6.Values.Dequeue());
		}
	}

	private readonly IObservable<T1> _source1;

	private readonly IObservable<T2> _source2;

	private readonly IObservable<T3> _source3;

	private readonly IObservable<T4> _source4;

	private readonly IObservable<T5> _source5;

	private readonly IObservable<T6> _source6;

	private readonly Func<T1, T2, T3, T4, T5, T6, TResult> _resultSelector;

	public Zip(IObservable<T1> source1, IObservable<T2> source2, IObservable<T3> source3, IObservable<T4> source4, IObservable<T5> source5, IObservable<T6> source6, Func<T1, T2, T3, T4, T5, T6, TResult> resultSelector)
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
internal sealed class Zip<T1, T2, T3, T4, T5, T6, T7, TResult> : Producer<TResult, Zip<T1, T2, T3, T4, T5, T6, T7, TResult>._>
{
	internal sealed class @_ : ZipSink<TResult>
	{
		private readonly Func<T1, T2, T3, T4, T5, T6, T7, TResult> _resultSelector;

		private readonly ZipObserver<T1> _observer1;

		private readonly ZipObserver<T2> _observer2;

		private readonly ZipObserver<T3> _observer3;

		private readonly ZipObserver<T4> _observer4;

		private readonly ZipObserver<T5> _observer5;

		private readonly ZipObserver<T6> _observer6;

		private readonly ZipObserver<T7> _observer7;

		public _(Func<T1, T2, T3, T4, T5, T6, T7, TResult> resultSelector, IObserver<TResult> observer)
			: base(7, observer)
		{
			_resultSelector = resultSelector;
			_observer1 = new ZipObserver<T1>(_gate, this, 0);
			_observer2 = new ZipObserver<T2>(_gate, this, 1);
			_observer3 = new ZipObserver<T3>(_gate, this, 2);
			_observer4 = new ZipObserver<T4>(_gate, this, 3);
			_observer5 = new ZipObserver<T5>(_gate, this, 4);
			_observer6 = new ZipObserver<T6>(_gate, this, 5);
			_observer7 = new ZipObserver<T7>(_gate, this, 6);
		}

		public void Run(IObservable<T1> source1, IObservable<T2> source2, IObservable<T3> source3, IObservable<T4> source4, IObservable<T5> source5, IObservable<T6> source6, IObservable<T7> source7)
		{
			IDisposable[] array = new IDisposable[7] { _observer1, null, null, null, null, null, null };
			base.Queues[0] = _observer1.Values;
			array[1] = _observer2;
			base.Queues[1] = _observer2.Values;
			array[2] = _observer3;
			base.Queues[2] = _observer3.Values;
			array[3] = _observer4;
			base.Queues[3] = _observer4.Values;
			array[4] = _observer5;
			base.Queues[4] = _observer5.Values;
			array[5] = _observer6;
			base.Queues[5] = _observer6.Values;
			array[6] = _observer7;
			base.Queues[6] = _observer7.Values;
			_observer1.SetResource(source1.SubscribeSafe(_observer1));
			_observer2.SetResource(source2.SubscribeSafe(_observer2));
			_observer3.SetResource(source3.SubscribeSafe(_observer3));
			_observer4.SetResource(source4.SubscribeSafe(_observer4));
			_observer5.SetResource(source5.SubscribeSafe(_observer5));
			_observer6.SetResource(source6.SubscribeSafe(_observer6));
			_observer7.SetResource(source7.SubscribeSafe(_observer7));
			SetUpstream(StableCompositeDisposable.CreateTrusted(array));
		}

		protected override TResult GetResult()
		{
			return _resultSelector(_observer1.Values.Dequeue(), _observer2.Values.Dequeue(), _observer3.Values.Dequeue(), _observer4.Values.Dequeue(), _observer5.Values.Dequeue(), _observer6.Values.Dequeue(), _observer7.Values.Dequeue());
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

	public Zip(IObservable<T1> source1, IObservable<T2> source2, IObservable<T3> source3, IObservable<T4> source4, IObservable<T5> source5, IObservable<T6> source6, IObservable<T7> source7, Func<T1, T2, T3, T4, T5, T6, T7, TResult> resultSelector)
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
internal sealed class Zip<T1, T2, T3, T4, T5, T6, T7, T8, TResult> : Producer<TResult, Zip<T1, T2, T3, T4, T5, T6, T7, T8, TResult>._>
{
	internal sealed class @_ : ZipSink<TResult>
	{
		private readonly Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> _resultSelector;

		private readonly ZipObserver<T1> _observer1;

		private readonly ZipObserver<T2> _observer2;

		private readonly ZipObserver<T3> _observer3;

		private readonly ZipObserver<T4> _observer4;

		private readonly ZipObserver<T5> _observer5;

		private readonly ZipObserver<T6> _observer6;

		private readonly ZipObserver<T7> _observer7;

		private readonly ZipObserver<T8> _observer8;

		public _(Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> resultSelector, IObserver<TResult> observer)
			: base(8, observer)
		{
			_resultSelector = resultSelector;
			_observer1 = new ZipObserver<T1>(_gate, this, 0);
			_observer2 = new ZipObserver<T2>(_gate, this, 1);
			_observer3 = new ZipObserver<T3>(_gate, this, 2);
			_observer4 = new ZipObserver<T4>(_gate, this, 3);
			_observer5 = new ZipObserver<T5>(_gate, this, 4);
			_observer6 = new ZipObserver<T6>(_gate, this, 5);
			_observer7 = new ZipObserver<T7>(_gate, this, 6);
			_observer8 = new ZipObserver<T8>(_gate, this, 7);
		}

		public void Run(IObservable<T1> source1, IObservable<T2> source2, IObservable<T3> source3, IObservable<T4> source4, IObservable<T5> source5, IObservable<T6> source6, IObservable<T7> source7, IObservable<T8> source8)
		{
			IDisposable[] array = new IDisposable[8] { _observer1, null, null, null, null, null, null, null };
			base.Queues[0] = _observer1.Values;
			array[1] = _observer2;
			base.Queues[1] = _observer2.Values;
			array[2] = _observer3;
			base.Queues[2] = _observer3.Values;
			array[3] = _observer4;
			base.Queues[3] = _observer4.Values;
			array[4] = _observer5;
			base.Queues[4] = _observer5.Values;
			array[5] = _observer6;
			base.Queues[5] = _observer6.Values;
			array[6] = _observer7;
			base.Queues[6] = _observer7.Values;
			array[7] = _observer8;
			base.Queues[7] = _observer8.Values;
			_observer1.SetResource(source1.SubscribeSafe(_observer1));
			_observer2.SetResource(source2.SubscribeSafe(_observer2));
			_observer3.SetResource(source3.SubscribeSafe(_observer3));
			_observer4.SetResource(source4.SubscribeSafe(_observer4));
			_observer5.SetResource(source5.SubscribeSafe(_observer5));
			_observer6.SetResource(source6.SubscribeSafe(_observer6));
			_observer7.SetResource(source7.SubscribeSafe(_observer7));
			_observer8.SetResource(source8.SubscribeSafe(_observer8));
			SetUpstream(StableCompositeDisposable.CreateTrusted(array));
		}

		protected override TResult GetResult()
		{
			return _resultSelector(_observer1.Values.Dequeue(), _observer2.Values.Dequeue(), _observer3.Values.Dequeue(), _observer4.Values.Dequeue(), _observer5.Values.Dequeue(), _observer6.Values.Dequeue(), _observer7.Values.Dequeue(), _observer8.Values.Dequeue());
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

	public Zip(IObservable<T1> source1, IObservable<T2> source2, IObservable<T3> source3, IObservable<T4> source4, IObservable<T5> source5, IObservable<T6> source6, IObservable<T7> source7, IObservable<T8> source8, Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> resultSelector)
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
internal sealed class Zip<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> : Producer<TResult, Zip<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>._>
{
	internal sealed class @_ : ZipSink<TResult>
	{
		private readonly Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> _resultSelector;

		private readonly ZipObserver<T1> _observer1;

		private readonly ZipObserver<T2> _observer2;

		private readonly ZipObserver<T3> _observer3;

		private readonly ZipObserver<T4> _observer4;

		private readonly ZipObserver<T5> _observer5;

		private readonly ZipObserver<T6> _observer6;

		private readonly ZipObserver<T7> _observer7;

		private readonly ZipObserver<T8> _observer8;

		private readonly ZipObserver<T9> _observer9;

		public _(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> resultSelector, IObserver<TResult> observer)
			: base(9, observer)
		{
			_resultSelector = resultSelector;
			_observer1 = new ZipObserver<T1>(_gate, this, 0);
			_observer2 = new ZipObserver<T2>(_gate, this, 1);
			_observer3 = new ZipObserver<T3>(_gate, this, 2);
			_observer4 = new ZipObserver<T4>(_gate, this, 3);
			_observer5 = new ZipObserver<T5>(_gate, this, 4);
			_observer6 = new ZipObserver<T6>(_gate, this, 5);
			_observer7 = new ZipObserver<T7>(_gate, this, 6);
			_observer8 = new ZipObserver<T8>(_gate, this, 7);
			_observer9 = new ZipObserver<T9>(_gate, this, 8);
		}

		public void Run(IObservable<T1> source1, IObservable<T2> source2, IObservable<T3> source3, IObservable<T4> source4, IObservable<T5> source5, IObservable<T6> source6, IObservable<T7> source7, IObservable<T8> source8, IObservable<T9> source9)
		{
			IDisposable[] array = new IDisposable[9] { _observer1, null, null, null, null, null, null, null, null };
			base.Queues[0] = _observer1.Values;
			array[1] = _observer2;
			base.Queues[1] = _observer2.Values;
			array[2] = _observer3;
			base.Queues[2] = _observer3.Values;
			array[3] = _observer4;
			base.Queues[3] = _observer4.Values;
			array[4] = _observer5;
			base.Queues[4] = _observer5.Values;
			array[5] = _observer6;
			base.Queues[5] = _observer6.Values;
			array[6] = _observer7;
			base.Queues[6] = _observer7.Values;
			array[7] = _observer8;
			base.Queues[7] = _observer8.Values;
			array[8] = _observer9;
			base.Queues[8] = _observer9.Values;
			_observer1.SetResource(source1.SubscribeSafe(_observer1));
			_observer2.SetResource(source2.SubscribeSafe(_observer2));
			_observer3.SetResource(source3.SubscribeSafe(_observer3));
			_observer4.SetResource(source4.SubscribeSafe(_observer4));
			_observer5.SetResource(source5.SubscribeSafe(_observer5));
			_observer6.SetResource(source6.SubscribeSafe(_observer6));
			_observer7.SetResource(source7.SubscribeSafe(_observer7));
			_observer8.SetResource(source8.SubscribeSafe(_observer8));
			_observer9.SetResource(source9.SubscribeSafe(_observer9));
			SetUpstream(StableCompositeDisposable.CreateTrusted(array));
		}

		protected override TResult GetResult()
		{
			return _resultSelector(_observer1.Values.Dequeue(), _observer2.Values.Dequeue(), _observer3.Values.Dequeue(), _observer4.Values.Dequeue(), _observer5.Values.Dequeue(), _observer6.Values.Dequeue(), _observer7.Values.Dequeue(), _observer8.Values.Dequeue(), _observer9.Values.Dequeue());
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

	public Zip(IObservable<T1> source1, IObservable<T2> source2, IObservable<T3> source3, IObservable<T4> source4, IObservable<T5> source5, IObservable<T6> source6, IObservable<T7> source7, IObservable<T8> source8, IObservable<T9> source9, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> resultSelector)
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
internal sealed class Zip<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> : Producer<TResult, Zip<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>._>
{
	internal sealed class @_ : ZipSink<TResult>
	{
		private readonly Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> _resultSelector;

		private readonly ZipObserver<T1> _observer1;

		private readonly ZipObserver<T2> _observer2;

		private readonly ZipObserver<T3> _observer3;

		private readonly ZipObserver<T4> _observer4;

		private readonly ZipObserver<T5> _observer5;

		private readonly ZipObserver<T6> _observer6;

		private readonly ZipObserver<T7> _observer7;

		private readonly ZipObserver<T8> _observer8;

		private readonly ZipObserver<T9> _observer9;

		private readonly ZipObserver<T10> _observer10;

		public _(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> resultSelector, IObserver<TResult> observer)
			: base(10, observer)
		{
			_resultSelector = resultSelector;
			_observer1 = new ZipObserver<T1>(_gate, this, 0);
			_observer2 = new ZipObserver<T2>(_gate, this, 1);
			_observer3 = new ZipObserver<T3>(_gate, this, 2);
			_observer4 = new ZipObserver<T4>(_gate, this, 3);
			_observer5 = new ZipObserver<T5>(_gate, this, 4);
			_observer6 = new ZipObserver<T6>(_gate, this, 5);
			_observer7 = new ZipObserver<T7>(_gate, this, 6);
			_observer8 = new ZipObserver<T8>(_gate, this, 7);
			_observer9 = new ZipObserver<T9>(_gate, this, 8);
			_observer10 = new ZipObserver<T10>(_gate, this, 9);
		}

		public void Run(IObservable<T1> source1, IObservable<T2> source2, IObservable<T3> source3, IObservable<T4> source4, IObservable<T5> source5, IObservable<T6> source6, IObservable<T7> source7, IObservable<T8> source8, IObservable<T9> source9, IObservable<T10> source10)
		{
			IDisposable[] array = new IDisposable[10] { _observer1, null, null, null, null, null, null, null, null, null };
			base.Queues[0] = _observer1.Values;
			array[1] = _observer2;
			base.Queues[1] = _observer2.Values;
			array[2] = _observer3;
			base.Queues[2] = _observer3.Values;
			array[3] = _observer4;
			base.Queues[3] = _observer4.Values;
			array[4] = _observer5;
			base.Queues[4] = _observer5.Values;
			array[5] = _observer6;
			base.Queues[5] = _observer6.Values;
			array[6] = _observer7;
			base.Queues[6] = _observer7.Values;
			array[7] = _observer8;
			base.Queues[7] = _observer8.Values;
			array[8] = _observer9;
			base.Queues[8] = _observer9.Values;
			array[9] = _observer10;
			base.Queues[9] = _observer10.Values;
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
			SetUpstream(StableCompositeDisposable.CreateTrusted(array));
		}

		protected override TResult GetResult()
		{
			return _resultSelector(_observer1.Values.Dequeue(), _observer2.Values.Dequeue(), _observer3.Values.Dequeue(), _observer4.Values.Dequeue(), _observer5.Values.Dequeue(), _observer6.Values.Dequeue(), _observer7.Values.Dequeue(), _observer8.Values.Dequeue(), _observer9.Values.Dequeue(), _observer10.Values.Dequeue());
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

	public Zip(IObservable<T1> source1, IObservable<T2> source2, IObservable<T3> source3, IObservable<T4> source4, IObservable<T5> source5, IObservable<T6> source6, IObservable<T7> source7, IObservable<T8> source8, IObservable<T9> source9, IObservable<T10> source10, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> resultSelector)
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
internal sealed class Zip<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult> : Producer<TResult, Zip<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>._>
{
	internal sealed class @_ : ZipSink<TResult>
	{
		private readonly Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult> _resultSelector;

		private readonly ZipObserver<T1> _observer1;

		private readonly ZipObserver<T2> _observer2;

		private readonly ZipObserver<T3> _observer3;

		private readonly ZipObserver<T4> _observer4;

		private readonly ZipObserver<T5> _observer5;

		private readonly ZipObserver<T6> _observer6;

		private readonly ZipObserver<T7> _observer7;

		private readonly ZipObserver<T8> _observer8;

		private readonly ZipObserver<T9> _observer9;

		private readonly ZipObserver<T10> _observer10;

		private readonly ZipObserver<T11> _observer11;

		public _(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult> resultSelector, IObserver<TResult> observer)
			: base(11, observer)
		{
			_resultSelector = resultSelector;
			_observer1 = new ZipObserver<T1>(_gate, this, 0);
			_observer2 = new ZipObserver<T2>(_gate, this, 1);
			_observer3 = new ZipObserver<T3>(_gate, this, 2);
			_observer4 = new ZipObserver<T4>(_gate, this, 3);
			_observer5 = new ZipObserver<T5>(_gate, this, 4);
			_observer6 = new ZipObserver<T6>(_gate, this, 5);
			_observer7 = new ZipObserver<T7>(_gate, this, 6);
			_observer8 = new ZipObserver<T8>(_gate, this, 7);
			_observer9 = new ZipObserver<T9>(_gate, this, 8);
			_observer10 = new ZipObserver<T10>(_gate, this, 9);
			_observer11 = new ZipObserver<T11>(_gate, this, 10);
		}

		public void Run(IObservable<T1> source1, IObservable<T2> source2, IObservable<T3> source3, IObservable<T4> source4, IObservable<T5> source5, IObservable<T6> source6, IObservable<T7> source7, IObservable<T8> source8, IObservable<T9> source9, IObservable<T10> source10, IObservable<T11> source11)
		{
			IDisposable[] array = new IDisposable[11]
			{
				_observer1, null, null, null, null, null, null, null, null, null,
				null
			};
			base.Queues[0] = _observer1.Values;
			array[1] = _observer2;
			base.Queues[1] = _observer2.Values;
			array[2] = _observer3;
			base.Queues[2] = _observer3.Values;
			array[3] = _observer4;
			base.Queues[3] = _observer4.Values;
			array[4] = _observer5;
			base.Queues[4] = _observer5.Values;
			array[5] = _observer6;
			base.Queues[5] = _observer6.Values;
			array[6] = _observer7;
			base.Queues[6] = _observer7.Values;
			array[7] = _observer8;
			base.Queues[7] = _observer8.Values;
			array[8] = _observer9;
			base.Queues[8] = _observer9.Values;
			array[9] = _observer10;
			base.Queues[9] = _observer10.Values;
			array[10] = _observer11;
			base.Queues[10] = _observer11.Values;
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
			SetUpstream(StableCompositeDisposable.CreateTrusted(array));
		}

		protected override TResult GetResult()
		{
			return _resultSelector(_observer1.Values.Dequeue(), _observer2.Values.Dequeue(), _observer3.Values.Dequeue(), _observer4.Values.Dequeue(), _observer5.Values.Dequeue(), _observer6.Values.Dequeue(), _observer7.Values.Dequeue(), _observer8.Values.Dequeue(), _observer9.Values.Dequeue(), _observer10.Values.Dequeue(), _observer11.Values.Dequeue());
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

	public Zip(IObservable<T1> source1, IObservable<T2> source2, IObservable<T3> source3, IObservable<T4> source4, IObservable<T5> source5, IObservable<T6> source6, IObservable<T7> source7, IObservable<T8> source8, IObservable<T9> source9, IObservable<T10> source10, IObservable<T11> source11, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult> resultSelector)
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
internal sealed class Zip<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult> : Producer<TResult, Zip<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>._>
{
	internal sealed class @_ : ZipSink<TResult>
	{
		private readonly Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult> _resultSelector;

		private readonly ZipObserver<T1> _observer1;

		private readonly ZipObserver<T2> _observer2;

		private readonly ZipObserver<T3> _observer3;

		private readonly ZipObserver<T4> _observer4;

		private readonly ZipObserver<T5> _observer5;

		private readonly ZipObserver<T6> _observer6;

		private readonly ZipObserver<T7> _observer7;

		private readonly ZipObserver<T8> _observer8;

		private readonly ZipObserver<T9> _observer9;

		private readonly ZipObserver<T10> _observer10;

		private readonly ZipObserver<T11> _observer11;

		private readonly ZipObserver<T12> _observer12;

		public _(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult> resultSelector, IObserver<TResult> observer)
			: base(12, observer)
		{
			_resultSelector = resultSelector;
			_observer1 = new ZipObserver<T1>(_gate, this, 0);
			_observer2 = new ZipObserver<T2>(_gate, this, 1);
			_observer3 = new ZipObserver<T3>(_gate, this, 2);
			_observer4 = new ZipObserver<T4>(_gate, this, 3);
			_observer5 = new ZipObserver<T5>(_gate, this, 4);
			_observer6 = new ZipObserver<T6>(_gate, this, 5);
			_observer7 = new ZipObserver<T7>(_gate, this, 6);
			_observer8 = new ZipObserver<T8>(_gate, this, 7);
			_observer9 = new ZipObserver<T9>(_gate, this, 8);
			_observer10 = new ZipObserver<T10>(_gate, this, 9);
			_observer11 = new ZipObserver<T11>(_gate, this, 10);
			_observer12 = new ZipObserver<T12>(_gate, this, 11);
		}

		public void Run(IObservable<T1> source1, IObservable<T2> source2, IObservable<T3> source3, IObservable<T4> source4, IObservable<T5> source5, IObservable<T6> source6, IObservable<T7> source7, IObservable<T8> source8, IObservable<T9> source9, IObservable<T10> source10, IObservable<T11> source11, IObservable<T12> source12)
		{
			IDisposable[] array = new IDisposable[12]
			{
				_observer1, null, null, null, null, null, null, null, null, null,
				null, null
			};
			base.Queues[0] = _observer1.Values;
			array[1] = _observer2;
			base.Queues[1] = _observer2.Values;
			array[2] = _observer3;
			base.Queues[2] = _observer3.Values;
			array[3] = _observer4;
			base.Queues[3] = _observer4.Values;
			array[4] = _observer5;
			base.Queues[4] = _observer5.Values;
			array[5] = _observer6;
			base.Queues[5] = _observer6.Values;
			array[6] = _observer7;
			base.Queues[6] = _observer7.Values;
			array[7] = _observer8;
			base.Queues[7] = _observer8.Values;
			array[8] = _observer9;
			base.Queues[8] = _observer9.Values;
			array[9] = _observer10;
			base.Queues[9] = _observer10.Values;
			array[10] = _observer11;
			base.Queues[10] = _observer11.Values;
			array[11] = _observer12;
			base.Queues[11] = _observer12.Values;
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
			SetUpstream(StableCompositeDisposable.CreateTrusted(array));
		}

		protected override TResult GetResult()
		{
			return _resultSelector(_observer1.Values.Dequeue(), _observer2.Values.Dequeue(), _observer3.Values.Dequeue(), _observer4.Values.Dequeue(), _observer5.Values.Dequeue(), _observer6.Values.Dequeue(), _observer7.Values.Dequeue(), _observer8.Values.Dequeue(), _observer9.Values.Dequeue(), _observer10.Values.Dequeue(), _observer11.Values.Dequeue(), _observer12.Values.Dequeue());
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

	public Zip(IObservable<T1> source1, IObservable<T2> source2, IObservable<T3> source3, IObservable<T4> source4, IObservable<T5> source5, IObservable<T6> source6, IObservable<T7> source7, IObservable<T8> source8, IObservable<T9> source9, IObservable<T10> source10, IObservable<T11> source11, IObservable<T12> source12, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult> resultSelector)
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
internal sealed class Zip<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult> : Producer<TResult, Zip<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>._>
{
	internal sealed class @_ : ZipSink<TResult>
	{
		private readonly Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult> _resultSelector;

		private readonly ZipObserver<T1> _observer1;

		private readonly ZipObserver<T2> _observer2;

		private readonly ZipObserver<T3> _observer3;

		private readonly ZipObserver<T4> _observer4;

		private readonly ZipObserver<T5> _observer5;

		private readonly ZipObserver<T6> _observer6;

		private readonly ZipObserver<T7> _observer7;

		private readonly ZipObserver<T8> _observer8;

		private readonly ZipObserver<T9> _observer9;

		private readonly ZipObserver<T10> _observer10;

		private readonly ZipObserver<T11> _observer11;

		private readonly ZipObserver<T12> _observer12;

		private readonly ZipObserver<T13> _observer13;

		public _(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult> resultSelector, IObserver<TResult> observer)
			: base(13, observer)
		{
			_resultSelector = resultSelector;
			_observer1 = new ZipObserver<T1>(_gate, this, 0);
			_observer2 = new ZipObserver<T2>(_gate, this, 1);
			_observer3 = new ZipObserver<T3>(_gate, this, 2);
			_observer4 = new ZipObserver<T4>(_gate, this, 3);
			_observer5 = new ZipObserver<T5>(_gate, this, 4);
			_observer6 = new ZipObserver<T6>(_gate, this, 5);
			_observer7 = new ZipObserver<T7>(_gate, this, 6);
			_observer8 = new ZipObserver<T8>(_gate, this, 7);
			_observer9 = new ZipObserver<T9>(_gate, this, 8);
			_observer10 = new ZipObserver<T10>(_gate, this, 9);
			_observer11 = new ZipObserver<T11>(_gate, this, 10);
			_observer12 = new ZipObserver<T12>(_gate, this, 11);
			_observer13 = new ZipObserver<T13>(_gate, this, 12);
		}

		public void Run(IObservable<T1> source1, IObservable<T2> source2, IObservable<T3> source3, IObservable<T4> source4, IObservable<T5> source5, IObservable<T6> source6, IObservable<T7> source7, IObservable<T8> source8, IObservable<T9> source9, IObservable<T10> source10, IObservable<T11> source11, IObservable<T12> source12, IObservable<T13> source13)
		{
			IDisposable[] array = new IDisposable[13]
			{
				_observer1, null, null, null, null, null, null, null, null, null,
				null, null, null
			};
			base.Queues[0] = _observer1.Values;
			array[1] = _observer2;
			base.Queues[1] = _observer2.Values;
			array[2] = _observer3;
			base.Queues[2] = _observer3.Values;
			array[3] = _observer4;
			base.Queues[3] = _observer4.Values;
			array[4] = _observer5;
			base.Queues[4] = _observer5.Values;
			array[5] = _observer6;
			base.Queues[5] = _observer6.Values;
			array[6] = _observer7;
			base.Queues[6] = _observer7.Values;
			array[7] = _observer8;
			base.Queues[7] = _observer8.Values;
			array[8] = _observer9;
			base.Queues[8] = _observer9.Values;
			array[9] = _observer10;
			base.Queues[9] = _observer10.Values;
			array[10] = _observer11;
			base.Queues[10] = _observer11.Values;
			array[11] = _observer12;
			base.Queues[11] = _observer12.Values;
			array[12] = _observer13;
			base.Queues[12] = _observer13.Values;
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
			SetUpstream(StableCompositeDisposable.CreateTrusted(array));
		}

		protected override TResult GetResult()
		{
			return _resultSelector(_observer1.Values.Dequeue(), _observer2.Values.Dequeue(), _observer3.Values.Dequeue(), _observer4.Values.Dequeue(), _observer5.Values.Dequeue(), _observer6.Values.Dequeue(), _observer7.Values.Dequeue(), _observer8.Values.Dequeue(), _observer9.Values.Dequeue(), _observer10.Values.Dequeue(), _observer11.Values.Dequeue(), _observer12.Values.Dequeue(), _observer13.Values.Dequeue());
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

	public Zip(IObservable<T1> source1, IObservable<T2> source2, IObservable<T3> source3, IObservable<T4> source4, IObservable<T5> source5, IObservable<T6> source6, IObservable<T7> source7, IObservable<T8> source8, IObservable<T9> source9, IObservable<T10> source10, IObservable<T11> source11, IObservable<T12> source12, IObservable<T13> source13, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult> resultSelector)
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
internal sealed class Zip<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult> : Producer<TResult, Zip<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>._>
{
	internal sealed class @_ : ZipSink<TResult>
	{
		private readonly Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult> _resultSelector;

		private readonly ZipObserver<T1> _observer1;

		private readonly ZipObserver<T2> _observer2;

		private readonly ZipObserver<T3> _observer3;

		private readonly ZipObserver<T4> _observer4;

		private readonly ZipObserver<T5> _observer5;

		private readonly ZipObserver<T6> _observer6;

		private readonly ZipObserver<T7> _observer7;

		private readonly ZipObserver<T8> _observer8;

		private readonly ZipObserver<T9> _observer9;

		private readonly ZipObserver<T10> _observer10;

		private readonly ZipObserver<T11> _observer11;

		private readonly ZipObserver<T12> _observer12;

		private readonly ZipObserver<T13> _observer13;

		private readonly ZipObserver<T14> _observer14;

		public _(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult> resultSelector, IObserver<TResult> observer)
			: base(14, observer)
		{
			_resultSelector = resultSelector;
			_observer1 = new ZipObserver<T1>(_gate, this, 0);
			_observer2 = new ZipObserver<T2>(_gate, this, 1);
			_observer3 = new ZipObserver<T3>(_gate, this, 2);
			_observer4 = new ZipObserver<T4>(_gate, this, 3);
			_observer5 = new ZipObserver<T5>(_gate, this, 4);
			_observer6 = new ZipObserver<T6>(_gate, this, 5);
			_observer7 = new ZipObserver<T7>(_gate, this, 6);
			_observer8 = new ZipObserver<T8>(_gate, this, 7);
			_observer9 = new ZipObserver<T9>(_gate, this, 8);
			_observer10 = new ZipObserver<T10>(_gate, this, 9);
			_observer11 = new ZipObserver<T11>(_gate, this, 10);
			_observer12 = new ZipObserver<T12>(_gate, this, 11);
			_observer13 = new ZipObserver<T13>(_gate, this, 12);
			_observer14 = new ZipObserver<T14>(_gate, this, 13);
		}

		public void Run(IObservable<T1> source1, IObservable<T2> source2, IObservable<T3> source3, IObservable<T4> source4, IObservable<T5> source5, IObservable<T6> source6, IObservable<T7> source7, IObservable<T8> source8, IObservable<T9> source9, IObservable<T10> source10, IObservable<T11> source11, IObservable<T12> source12, IObservable<T13> source13, IObservable<T14> source14)
		{
			IDisposable[] array = new IDisposable[14]
			{
				_observer1, null, null, null, null, null, null, null, null, null,
				null, null, null, null
			};
			base.Queues[0] = _observer1.Values;
			array[1] = _observer2;
			base.Queues[1] = _observer2.Values;
			array[2] = _observer3;
			base.Queues[2] = _observer3.Values;
			array[3] = _observer4;
			base.Queues[3] = _observer4.Values;
			array[4] = _observer5;
			base.Queues[4] = _observer5.Values;
			array[5] = _observer6;
			base.Queues[5] = _observer6.Values;
			array[6] = _observer7;
			base.Queues[6] = _observer7.Values;
			array[7] = _observer8;
			base.Queues[7] = _observer8.Values;
			array[8] = _observer9;
			base.Queues[8] = _observer9.Values;
			array[9] = _observer10;
			base.Queues[9] = _observer10.Values;
			array[10] = _observer11;
			base.Queues[10] = _observer11.Values;
			array[11] = _observer12;
			base.Queues[11] = _observer12.Values;
			array[12] = _observer13;
			base.Queues[12] = _observer13.Values;
			array[13] = _observer14;
			base.Queues[13] = _observer14.Values;
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
			SetUpstream(StableCompositeDisposable.CreateTrusted(array));
		}

		protected override TResult GetResult()
		{
			return _resultSelector(_observer1.Values.Dequeue(), _observer2.Values.Dequeue(), _observer3.Values.Dequeue(), _observer4.Values.Dequeue(), _observer5.Values.Dequeue(), _observer6.Values.Dequeue(), _observer7.Values.Dequeue(), _observer8.Values.Dequeue(), _observer9.Values.Dequeue(), _observer10.Values.Dequeue(), _observer11.Values.Dequeue(), _observer12.Values.Dequeue(), _observer13.Values.Dequeue(), _observer14.Values.Dequeue());
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

	public Zip(IObservable<T1> source1, IObservable<T2> source2, IObservable<T3> source3, IObservable<T4> source4, IObservable<T5> source5, IObservable<T6> source6, IObservable<T7> source7, IObservable<T8> source8, IObservable<T9> source9, IObservable<T10> source10, IObservable<T11> source11, IObservable<T12> source12, IObservable<T13> source13, IObservable<T14> source14, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult> resultSelector)
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
internal sealed class Zip<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult> : Producer<TResult, Zip<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>._>
{
	internal sealed class @_ : ZipSink<TResult>
	{
		private readonly Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult> _resultSelector;

		private readonly ZipObserver<T1> _observer1;

		private readonly ZipObserver<T2> _observer2;

		private readonly ZipObserver<T3> _observer3;

		private readonly ZipObserver<T4> _observer4;

		private readonly ZipObserver<T5> _observer5;

		private readonly ZipObserver<T6> _observer6;

		private readonly ZipObserver<T7> _observer7;

		private readonly ZipObserver<T8> _observer8;

		private readonly ZipObserver<T9> _observer9;

		private readonly ZipObserver<T10> _observer10;

		private readonly ZipObserver<T11> _observer11;

		private readonly ZipObserver<T12> _observer12;

		private readonly ZipObserver<T13> _observer13;

		private readonly ZipObserver<T14> _observer14;

		private readonly ZipObserver<T15> _observer15;

		public _(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult> resultSelector, IObserver<TResult> observer)
			: base(15, observer)
		{
			_resultSelector = resultSelector;
			_observer1 = new ZipObserver<T1>(_gate, this, 0);
			_observer2 = new ZipObserver<T2>(_gate, this, 1);
			_observer3 = new ZipObserver<T3>(_gate, this, 2);
			_observer4 = new ZipObserver<T4>(_gate, this, 3);
			_observer5 = new ZipObserver<T5>(_gate, this, 4);
			_observer6 = new ZipObserver<T6>(_gate, this, 5);
			_observer7 = new ZipObserver<T7>(_gate, this, 6);
			_observer8 = new ZipObserver<T8>(_gate, this, 7);
			_observer9 = new ZipObserver<T9>(_gate, this, 8);
			_observer10 = new ZipObserver<T10>(_gate, this, 9);
			_observer11 = new ZipObserver<T11>(_gate, this, 10);
			_observer12 = new ZipObserver<T12>(_gate, this, 11);
			_observer13 = new ZipObserver<T13>(_gate, this, 12);
			_observer14 = new ZipObserver<T14>(_gate, this, 13);
			_observer15 = new ZipObserver<T15>(_gate, this, 14);
		}

		public void Run(IObservable<T1> source1, IObservable<T2> source2, IObservable<T3> source3, IObservable<T4> source4, IObservable<T5> source5, IObservable<T6> source6, IObservable<T7> source7, IObservable<T8> source8, IObservable<T9> source9, IObservable<T10> source10, IObservable<T11> source11, IObservable<T12> source12, IObservable<T13> source13, IObservable<T14> source14, IObservable<T15> source15)
		{
			IDisposable[] array = new IDisposable[15]
			{
				_observer1, null, null, null, null, null, null, null, null, null,
				null, null, null, null, null
			};
			base.Queues[0] = _observer1.Values;
			array[1] = _observer2;
			base.Queues[1] = _observer2.Values;
			array[2] = _observer3;
			base.Queues[2] = _observer3.Values;
			array[3] = _observer4;
			base.Queues[3] = _observer4.Values;
			array[4] = _observer5;
			base.Queues[4] = _observer5.Values;
			array[5] = _observer6;
			base.Queues[5] = _observer6.Values;
			array[6] = _observer7;
			base.Queues[6] = _observer7.Values;
			array[7] = _observer8;
			base.Queues[7] = _observer8.Values;
			array[8] = _observer9;
			base.Queues[8] = _observer9.Values;
			array[9] = _observer10;
			base.Queues[9] = _observer10.Values;
			array[10] = _observer11;
			base.Queues[10] = _observer11.Values;
			array[11] = _observer12;
			base.Queues[11] = _observer12.Values;
			array[12] = _observer13;
			base.Queues[12] = _observer13.Values;
			array[13] = _observer14;
			base.Queues[13] = _observer14.Values;
			array[14] = _observer15;
			base.Queues[14] = _observer15.Values;
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
			SetUpstream(StableCompositeDisposable.CreateTrusted(array));
		}

		protected override TResult GetResult()
		{
			return _resultSelector(_observer1.Values.Dequeue(), _observer2.Values.Dequeue(), _observer3.Values.Dequeue(), _observer4.Values.Dequeue(), _observer5.Values.Dequeue(), _observer6.Values.Dequeue(), _observer7.Values.Dequeue(), _observer8.Values.Dequeue(), _observer9.Values.Dequeue(), _observer10.Values.Dequeue(), _observer11.Values.Dequeue(), _observer12.Values.Dequeue(), _observer13.Values.Dequeue(), _observer14.Values.Dequeue(), _observer15.Values.Dequeue());
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

	public Zip(IObservable<T1> source1, IObservable<T2> source2, IObservable<T3> source3, IObservable<T4> source4, IObservable<T5> source5, IObservable<T6> source6, IObservable<T7> source7, IObservable<T8> source8, IObservable<T9> source9, IObservable<T10> source10, IObservable<T11> source11, IObservable<T12> source12, IObservable<T13> source13, IObservable<T14> source14, IObservable<T15> source15, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult> resultSelector)
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
internal sealed class Zip<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult> : Producer<TResult, Zip<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult>._>
{
	internal sealed class @_ : ZipSink<TResult>
	{
		private readonly Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult> _resultSelector;

		private readonly ZipObserver<T1> _observer1;

		private readonly ZipObserver<T2> _observer2;

		private readonly ZipObserver<T3> _observer3;

		private readonly ZipObserver<T4> _observer4;

		private readonly ZipObserver<T5> _observer5;

		private readonly ZipObserver<T6> _observer6;

		private readonly ZipObserver<T7> _observer7;

		private readonly ZipObserver<T8> _observer8;

		private readonly ZipObserver<T9> _observer9;

		private readonly ZipObserver<T10> _observer10;

		private readonly ZipObserver<T11> _observer11;

		private readonly ZipObserver<T12> _observer12;

		private readonly ZipObserver<T13> _observer13;

		private readonly ZipObserver<T14> _observer14;

		private readonly ZipObserver<T15> _observer15;

		private readonly ZipObserver<T16> _observer16;

		public _(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult> resultSelector, IObserver<TResult> observer)
			: base(16, observer)
		{
			_resultSelector = resultSelector;
			_observer1 = new ZipObserver<T1>(_gate, this, 0);
			_observer2 = new ZipObserver<T2>(_gate, this, 1);
			_observer3 = new ZipObserver<T3>(_gate, this, 2);
			_observer4 = new ZipObserver<T4>(_gate, this, 3);
			_observer5 = new ZipObserver<T5>(_gate, this, 4);
			_observer6 = new ZipObserver<T6>(_gate, this, 5);
			_observer7 = new ZipObserver<T7>(_gate, this, 6);
			_observer8 = new ZipObserver<T8>(_gate, this, 7);
			_observer9 = new ZipObserver<T9>(_gate, this, 8);
			_observer10 = new ZipObserver<T10>(_gate, this, 9);
			_observer11 = new ZipObserver<T11>(_gate, this, 10);
			_observer12 = new ZipObserver<T12>(_gate, this, 11);
			_observer13 = new ZipObserver<T13>(_gate, this, 12);
			_observer14 = new ZipObserver<T14>(_gate, this, 13);
			_observer15 = new ZipObserver<T15>(_gate, this, 14);
			_observer16 = new ZipObserver<T16>(_gate, this, 15);
		}

		public void Run(IObservable<T1> source1, IObservable<T2> source2, IObservable<T3> source3, IObservable<T4> source4, IObservable<T5> source5, IObservable<T6> source6, IObservable<T7> source7, IObservable<T8> source8, IObservable<T9> source9, IObservable<T10> source10, IObservable<T11> source11, IObservable<T12> source12, IObservable<T13> source13, IObservable<T14> source14, IObservable<T15> source15, IObservable<T16> source16)
		{
			IDisposable[] array = new IDisposable[16]
			{
				_observer1, null, null, null, null, null, null, null, null, null,
				null, null, null, null, null, null
			};
			base.Queues[0] = _observer1.Values;
			array[1] = _observer2;
			base.Queues[1] = _observer2.Values;
			array[2] = _observer3;
			base.Queues[2] = _observer3.Values;
			array[3] = _observer4;
			base.Queues[3] = _observer4.Values;
			array[4] = _observer5;
			base.Queues[4] = _observer5.Values;
			array[5] = _observer6;
			base.Queues[5] = _observer6.Values;
			array[6] = _observer7;
			base.Queues[6] = _observer7.Values;
			array[7] = _observer8;
			base.Queues[7] = _observer8.Values;
			array[8] = _observer9;
			base.Queues[8] = _observer9.Values;
			array[9] = _observer10;
			base.Queues[9] = _observer10.Values;
			array[10] = _observer11;
			base.Queues[10] = _observer11.Values;
			array[11] = _observer12;
			base.Queues[11] = _observer12.Values;
			array[12] = _observer13;
			base.Queues[12] = _observer13.Values;
			array[13] = _observer14;
			base.Queues[13] = _observer14.Values;
			array[14] = _observer15;
			base.Queues[14] = _observer15.Values;
			array[15] = _observer16;
			base.Queues[15] = _observer16.Values;
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
			SetUpstream(StableCompositeDisposable.CreateTrusted(array));
		}

		protected override TResult GetResult()
		{
			return _resultSelector(_observer1.Values.Dequeue(), _observer2.Values.Dequeue(), _observer3.Values.Dequeue(), _observer4.Values.Dequeue(), _observer5.Values.Dequeue(), _observer6.Values.Dequeue(), _observer7.Values.Dequeue(), _observer8.Values.Dequeue(), _observer9.Values.Dequeue(), _observer10.Values.Dequeue(), _observer11.Values.Dequeue(), _observer12.Values.Dequeue(), _observer13.Values.Dequeue(), _observer14.Values.Dequeue(), _observer15.Values.Dequeue(), _observer16.Values.Dequeue());
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

	public Zip(IObservable<T1> source1, IObservable<T2> source2, IObservable<T3> source3, IObservable<T4> source4, IObservable<T5> source5, IObservable<T6> source6, IObservable<T7> source7, IObservable<T8> source8, IObservable<T9> source9, IObservable<T10> source10, IObservable<T11> source11, IObservable<T12> source12, IObservable<T13> source13, IObservable<T14> source14, IObservable<T15> source15, IObservable<T16> source16, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult> resultSelector)
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
