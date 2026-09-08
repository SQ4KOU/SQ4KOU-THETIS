using System.Reactive.Concurrency;
using System.Reactive.Disposables;

namespace System.Reactive.Linq.ObservableImpl;

internal static class AppendPrepend<TSource>
{
	internal interface IAppendPrepend : IObservable<TSource>
	{
		IScheduler Scheduler { get; }

		IAppendPrepend Append(TSource value);

		IAppendPrepend Prepend(TSource value);
	}

	internal abstract class SingleBase<TSink> : Producer<TSource, TSink>, IAppendPrepend, IObservable<TSource> where TSink : IDisposable
	{
		protected readonly IObservable<TSource> _source;

		protected readonly TSource _value;

		protected readonly bool _append;

		public abstract IScheduler Scheduler { get; }

		public SingleBase(IObservable<TSource> source, TSource value, bool append)
		{
			_source = source;
			_value = value;
			_append = append;
		}

		public IAppendPrepend Append(TSource value)
		{
			Node<TSource> node = new Node<TSource>(_value);
			Node<TSource> prepend = null;
			Node<TSource> append;
			if (_append)
			{
				append = new Node<TSource>(node, value);
			}
			else
			{
				prepend = node;
				append = new Node<TSource>(value);
			}
			return CreateAppendPrepend(prepend, append);
		}

		public IAppendPrepend Prepend(TSource value)
		{
			Node<TSource> node = new Node<TSource>(_value);
			Node<TSource> append = null;
			Node<TSource> prepend;
			if (_append)
			{
				prepend = new Node<TSource>(value);
				append = node;
			}
			else
			{
				prepend = new Node<TSource>(node, value);
			}
			return CreateAppendPrepend(prepend, append);
		}

		private IAppendPrepend CreateAppendPrepend(Node<TSource>? prepend, Node<TSource>? append)
		{
			if (Scheduler is ISchedulerLongRunning longRunningScheduler)
			{
				return new LongRunning(_source, prepend, append, Scheduler, longRunningScheduler);
			}
			return new Recursive(_source, prepend, append, Scheduler);
		}
	}

	internal sealed class SingleValue : SingleBase<SingleValue._>
	{
		internal sealed class @_ : IdentitySink<TSource>
		{
			private readonly IObservable<TSource> _source;

			private readonly TSource _value;

			private readonly IScheduler _scheduler;

			private readonly bool _append;

			private SingleAssignmentDisposableValue _schedulerDisposable;

			public _(SingleValue parent, IObserver<TSource> observer)
				: base(observer)
			{
				_source = parent._source;
				_value = parent._value;
				_scheduler = parent.Scheduler;
				_append = parent._append;
			}

			public void Run()
			{
				IDisposable upstream = (_append ? _source.SubscribeSafe(this) : _scheduler.ScheduleAction(this, (Func<@_, IDisposable>)PrependValue));
				SetUpstream(upstream);
			}

			private static IDisposable PrependValue(@_ sink)
			{
				sink.ForwardOnNext(sink._value);
				return sink._source.SubscribeSafe(sink);
			}

			public override void OnCompleted()
			{
				if (_append)
				{
					IDisposable disposable = _scheduler.ScheduleAction(this, (Action<@_>)AppendValue);
					_schedulerDisposable.Disposable = disposable;
				}
				else
				{
					ForwardOnCompleted();
				}
			}

			private static void AppendValue(@_ sink)
			{
				sink.ForwardOnNext(sink._value);
				sink.ForwardOnCompleted();
			}

			protected override void Dispose(bool disposing)
			{
				if (disposing)
				{
					_schedulerDisposable.Dispose();
				}
				base.Dispose(disposing);
			}
		}

		public override IScheduler Scheduler { get; }

		public SingleValue(IObservable<TSource> source, TSource value, IScheduler scheduler, bool append)
			: base(source, value, append)
		{
			Scheduler = scheduler;
		}

		protected override @_ CreateSink(IObserver<TSource> observer)
		{
			return new @_(this, observer);
		}

		protected override void Run(@_ sink)
		{
			sink.Run();
		}
	}

	private sealed class Recursive : Producer<TSource, Recursive._>, IAppendPrepend, IObservable<TSource>
	{
		internal sealed class @_ : IdentitySink<TSource>
		{
			private readonly IObservable<TSource> _source;

			private readonly Node<TSource>? _appends;

			private readonly IScheduler _scheduler;

			private Node<TSource>? _currentPrependNode;

			private TSource[]? _appendArray;

			private int _currentAppendIndex;

			private volatile bool _disposed;

			public _(Recursive parent, IObserver<TSource> observer)
				: base(observer)
			{
				_source = parent._source;
				_scheduler = parent.Scheduler;
				_currentPrependNode = parent._prepends;
				_appends = parent._appends;
			}

			public void Run()
			{
				if (_currentPrependNode == null)
				{
					SetUpstream(_source.SubscribeSafe(this));
					return;
				}
				_scheduler.Schedule(this, (IScheduler innerScheduler, @_ @this) => @this.PrependValues(innerScheduler));
			}

			public override void OnCompleted()
			{
				if (_appends == null)
				{
					ForwardOnCompleted();
					return;
				}
				_appendArray = _appends.ToReverseArray();
				_scheduler.Schedule(this, (IScheduler innerScheduler, @_ @this) => @this.AppendValues(innerScheduler));
			}

			protected override void Dispose(bool disposing)
			{
				if (disposing)
				{
					_disposed = true;
				}
				base.Dispose(disposing);
			}

			private IDisposable PrependValues(IScheduler scheduler)
			{
				if (_disposed)
				{
					return Disposable.Empty;
				}
				TSource value = _currentPrependNode.Value;
				ForwardOnNext(value);
				_currentPrependNode = _currentPrependNode.Parent;
				if (_currentPrependNode == null)
				{
					SetUpstream(_source.SubscribeSafe(this));
				}
				else
				{
					scheduler.Schedule(this, (IScheduler innerScheduler, @_ @this) => @this.PrependValues(innerScheduler));
				}
				return Disposable.Empty;
			}

			private IDisposable AppendValues(IScheduler scheduler)
			{
				if (_disposed)
				{
					return Disposable.Empty;
				}
				TSource value = _appendArray[_currentAppendIndex];
				ForwardOnNext(value);
				_currentAppendIndex++;
				if (_currentAppendIndex == _appendArray.Length)
				{
					ForwardOnCompleted();
				}
				else
				{
					scheduler.Schedule(this, (IScheduler innerScheduler, @_ @this) => @this.AppendValues(innerScheduler));
				}
				return Disposable.Empty;
			}
		}

		private readonly IObservable<TSource> _source;

		private readonly Node<TSource>? _appends;

		private readonly Node<TSource>? _prepends;

		public IScheduler Scheduler { get; }

		public Recursive(IObservable<TSource> source, Node<TSource>? prepends, Node<TSource>? appends, IScheduler scheduler)
		{
			_source = source;
			_appends = appends;
			_prepends = prepends;
			Scheduler = scheduler;
		}

		public IAppendPrepend Append(TSource value)
		{
			return new Recursive(_source, _prepends, new Node<TSource>(_appends, value), Scheduler);
		}

		public IAppendPrepend Prepend(TSource value)
		{
			return new Recursive(_source, new Node<TSource>(_prepends, value), _appends, Scheduler);
		}

		protected override @_ CreateSink(IObserver<TSource> observer)
		{
			return new @_(this, observer);
		}

		protected override void Run(@_ sink)
		{
			sink.Run();
		}
	}

	private sealed class LongRunning : Producer<TSource, LongRunning._>, IAppendPrepend, IObservable<TSource>
	{
		internal sealed class @_ : IdentitySink<TSource>
		{
			private readonly IObservable<TSource> _source;

			private readonly Node<TSource>? _prepends;

			private readonly Node<TSource>? _appends;

			private readonly ISchedulerLongRunning _scheduler;

			private SerialDisposableValue _schedulerDisposable;

			public _(LongRunning parent, IObserver<TSource> observer)
				: base(observer)
			{
				_source = parent._source;
				_scheduler = parent._longRunningScheduler;
				_prepends = parent._prepends;
				_appends = parent._appends;
			}

			public void Run()
			{
				if (_prepends == null)
				{
					SetUpstream(_source.SubscribeSafe(this));
					return;
				}
				IDisposable disposable = _scheduler.ScheduleLongRunning(this, delegate(@_ @this, ICancelable cancel)
				{
					@this.PrependValues(cancel);
				});
				_schedulerDisposable.TrySetFirst(disposable);
			}

			public override void OnCompleted()
			{
				if (_appends == null)
				{
					ForwardOnCompleted();
					return;
				}
				IDisposable disposable = _scheduler.ScheduleLongRunning(this, delegate(@_ @this, ICancelable cancel)
				{
					@this.AppendValues(cancel);
				});
				_schedulerDisposable.Disposable = disposable;
			}

			protected override void Dispose(bool disposing)
			{
				if (disposing)
				{
					_schedulerDisposable.Dispose();
				}
				base.Dispose(disposing);
			}

			private void PrependValues(ICancelable cancel)
			{
				Node<TSource> node = _prepends;
				while (!cancel.IsDisposed)
				{
					ForwardOnNext(node.Value);
					node = node.Parent;
					if (node == null)
					{
						SetUpstream(_source.SubscribeSafe(this));
						break;
					}
				}
			}

			private void AppendValues(ICancelable cancel)
			{
				TSource[] array = _appends.ToReverseArray();
				int num = 0;
				while (!cancel.IsDisposed)
				{
					ForwardOnNext(array[num]);
					num++;
					if (num == array.Length)
					{
						ForwardOnCompleted();
						break;
					}
				}
			}
		}

		private readonly IObservable<TSource> _source;

		private readonly Node<TSource>? _appends;

		private readonly Node<TSource>? _prepends;

		private readonly ISchedulerLongRunning _longRunningScheduler;

		public IScheduler Scheduler { get; }

		public LongRunning(IObservable<TSource> source, Node<TSource>? prepends, Node<TSource>? appends, IScheduler scheduler, ISchedulerLongRunning longRunningScheduler)
		{
			_source = source;
			_appends = appends;
			_prepends = prepends;
			Scheduler = scheduler;
			_longRunningScheduler = longRunningScheduler;
		}

		public IAppendPrepend Append(TSource value)
		{
			return new LongRunning(_source, _prepends, new Node<TSource>(_appends, value), Scheduler, _longRunningScheduler);
		}

		public IAppendPrepend Prepend(TSource value)
		{
			return new LongRunning(_source, new Node<TSource>(_prepends, value), _appends, Scheduler, _longRunningScheduler);
		}

		protected override @_ CreateSink(IObserver<TSource> observer)
		{
			return new @_(this, observer);
		}

		protected override void Run(@_ sink)
		{
			sink.Run();
		}
	}

	private sealed class Node<T>
	{
		public readonly Node<T>? Parent;

		public readonly T Value;

		public readonly int Count;

		public Node(T value)
			: this((Node<T>?)null, value)
		{
		}

		public Node(Node<T>? parent, T value)
		{
			Parent = parent;
			Value = value;
			if (parent == null)
			{
				Count = 1;
				return;
			}
			if (parent.Count == int.MaxValue)
			{
				throw new NotSupportedException($"Consecutive appends or prepends with a count of more than int.MaxValue ({int.MaxValue}) are not supported.");
			}
			Count = parent.Count + 1;
		}

		public T[] ToReverseArray()
		{
			T[] array = new T[Count];
			Node<T> node = this;
			for (int num = Count - 1; num >= 0; num--)
			{
				array[num] = node.Value;
				node = node.Parent;
			}
			return array;
		}
	}

	internal sealed class SingleImmediate : SingleBase<SingleImmediate._>
	{
		internal sealed class @_ : IdentitySink<TSource>
		{
			private readonly IObservable<TSource> _source;

			private readonly TSource _value;

			private readonly bool _append;

			public _(SingleImmediate parent, IObserver<TSource> observer)
				: base(observer)
			{
				_source = parent._source;
				_value = parent._value;
				_append = parent._append;
			}

			public void Run()
			{
				if (!_append)
				{
					ForwardOnNext(_value);
				}
				Run(_source);
			}

			public override void OnCompleted()
			{
				if (_append)
				{
					ForwardOnNext(_value);
				}
				ForwardOnCompleted();
			}
		}

		public override IScheduler Scheduler => ImmediateScheduler.Instance;

		public SingleImmediate(IObservable<TSource> source, TSource value, bool append)
			: base(source, value, append)
		{
		}

		protected override @_ CreateSink(IObserver<TSource> observer)
		{
			return new @_(this, observer);
		}

		protected override void Run(@_ sink)
		{
			sink.Run();
		}
	}
}
