using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;

namespace System.Reactive.Linq.ObservableImpl;

internal static class Merge<TSource>
{
	internal sealed class ObservablesMaxConcurrency : Producer<TSource, ObservablesMaxConcurrency._>
	{
		internal sealed class @_ : Sink<IObservable<TSource>, TSource>
		{
			private sealed class InnerObserver : SafeObserver<TSource>
			{
				private readonly @_ _parent;

				public InnerObserver(@_ parent)
				{
					_parent = parent;
				}

				public override void OnNext(TSource value)
				{
					lock (_parent._gate)
					{
						_parent.ForwardOnNext(value);
					}
				}

				public override void OnError(Exception error)
				{
					lock (_parent._gate)
					{
						_parent.ForwardOnError(error);
					}
				}

				public override void OnCompleted()
				{
					_parent._group.Remove(this);
					lock (_parent._gate)
					{
						if (_parent._q.Count > 0)
						{
							IObservable<TSource> innerSource = _parent._q.Dequeue();
							_parent.Subscribe(innerSource);
							return;
						}
						_parent._activeCount--;
						if (_parent._isStopped && _parent._activeCount == 0)
						{
							_parent.ForwardOnCompleted();
						}
					}
				}
			}

			private readonly object _gate = new object();

			private readonly int _maxConcurrent;

			private readonly Queue<IObservable<TSource>> _q = new Queue<IObservable<TSource>>();

			private readonly CompositeDisposable _group = new CompositeDisposable();

			private volatile bool _isStopped;

			private int _activeCount;

			public _(int maxConcurrent, IObserver<TSource> observer)
				: base(observer)
			{
				_maxConcurrent = maxConcurrent;
			}

			public override void OnNext(IObservable<TSource> value)
			{
				lock (_gate)
				{
					if (_activeCount < _maxConcurrent)
					{
						_activeCount++;
						Subscribe(value);
					}
					else
					{
						_q.Enqueue(value);
					}
				}
			}

			public override void OnError(Exception error)
			{
				lock (_gate)
				{
					ForwardOnError(error);
				}
			}

			public override void OnCompleted()
			{
				lock (_gate)
				{
					_isStopped = true;
					if (_activeCount == 0)
					{
						ForwardOnCompleted();
					}
					else
					{
						DisposeUpstream();
					}
				}
			}

			protected override void Dispose(bool disposing)
			{
				base.Dispose(disposing);
				if (disposing)
				{
					_group.Dispose();
				}
			}

			private void Subscribe(IObservable<TSource> innerSource)
			{
				InnerObserver innerObserver = new InnerObserver(this);
				_group.Add(innerObserver);
				innerObserver.SetResource(innerSource.SubscribeSafe(innerObserver));
			}
		}

		private readonly IObservable<IObservable<TSource>> _sources;

		private readonly int _maxConcurrent;

		public ObservablesMaxConcurrency(IObservable<IObservable<TSource>> sources, int maxConcurrent)
		{
			_sources = sources;
			_maxConcurrent = maxConcurrent;
		}

		protected override @_ CreateSink(IObserver<TSource> observer)
		{
			return new @_(_maxConcurrent, observer);
		}

		protected override void Run(@_ sink)
		{
			sink.Run(_sources);
		}
	}

	internal sealed class Observables : Producer<TSource, Observables._>
	{
		internal sealed class @_(IObserver<TSource> observer) : Sink<IObservable<TSource>, TSource>(observer)
		{
			private sealed class InnerObserver : SafeObserver<TSource>
			{
				private readonly @_ _parent;

				public InnerObserver(@_ parent)
				{
					_parent = parent;
				}

				public override void OnNext(TSource value)
				{
					lock (_parent._gate)
					{
						_parent.ForwardOnNext(value);
					}
				}

				public override void OnError(Exception error)
				{
					lock (_parent._gate)
					{
						_parent.ForwardOnError(error);
					}
				}

				public override void OnCompleted()
				{
					_parent._group.Remove(this);
					if (_parent._isStopped && _parent._group.Count == 0)
					{
						lock (_parent._gate)
						{
							_parent.ForwardOnCompleted();
						}
					}
				}
			}

			private readonly object _gate = new object();

			private readonly CompositeDisposable _group = new CompositeDisposable();

			private volatile bool _isStopped;

			public override void OnNext(IObservable<TSource> value)
			{
				InnerObserver innerObserver = new InnerObserver(this);
				_group.Add(innerObserver);
				innerObserver.SetResource(value.SubscribeSafe(innerObserver));
			}

			public override void OnError(Exception error)
			{
				lock (_gate)
				{
					ForwardOnError(error);
				}
			}

			public override void OnCompleted()
			{
				_isStopped = true;
				if (_group.Count == 0)
				{
					lock (_gate)
					{
						ForwardOnCompleted();
						return;
					}
				}
				DisposeUpstream();
			}

			protected override void Dispose(bool disposing)
			{
				base.Dispose(disposing);
				if (disposing)
				{
					_group.Dispose();
				}
			}
		}

		private readonly IObservable<IObservable<TSource>> _sources;

		public Observables(IObservable<IObservable<TSource>> sources)
		{
			_sources = sources;
		}

		protected override @_ CreateSink(IObserver<TSource> observer)
		{
			return new @_(observer);
		}

		protected override void Run(@_ sink)
		{
			sink.Run(_sources);
		}
	}

	internal sealed class Tasks : Producer<TSource, Tasks._>
	{
		internal sealed class @_(IObserver<TSource> observer) : Sink<Task<TSource>, TSource>(observer)
		{
			private readonly object _gate = new object();

			private readonly CancellationTokenSource _cts = new CancellationTokenSource();

			private volatile int _count = 1;

			public override void OnNext(Task<TSource> value)
			{
				Interlocked.Increment(ref _count);
				if (value.IsCompleted)
				{
					OnCompletedTask(value);
					return;
				}
				value.ContinueWith(delegate(Task<TSource> t, object thisObject)
				{
					((@_)thisObject).OnCompletedTask(t);
				}, this, _cts.Token);
			}

			private void OnCompletedTask(Task<TSource> task)
			{
				switch (task.Status)
				{
				case TaskStatus.RanToCompletion:
					lock (_gate)
					{
						ForwardOnNext(task.Result);
					}
					OnCompleted();
					break;
				case TaskStatus.Faulted:
					lock (_gate)
					{
						ForwardOnError(task.GetSingleException());
						break;
					}
				case TaskStatus.Canceled:
					lock (_gate)
					{
						ForwardOnError(new TaskCanceledException(task));
						break;
					}
				}
			}

			public override void OnError(Exception error)
			{
				lock (_gate)
				{
					ForwardOnError(error);
				}
			}

			public override void OnCompleted()
			{
				if (Interlocked.Decrement(ref _count) == 0)
				{
					lock (_gate)
					{
						ForwardOnCompleted();
					}
				}
			}

			protected override void Dispose(bool disposing)
			{
				if (disposing)
				{
					_cts.Cancel();
				}
				base.Dispose(disposing);
			}
		}

		private readonly IObservable<Task<TSource>> _sources;

		public Tasks(IObservable<Task<TSource>> sources)
		{
			_sources = sources;
		}

		protected override @_ CreateSink(IObserver<TSource> observer)
		{
			return new @_(observer);
		}

		protected override void Run(@_ sink)
		{
			sink.Run(_sources);
		}
	}
}
