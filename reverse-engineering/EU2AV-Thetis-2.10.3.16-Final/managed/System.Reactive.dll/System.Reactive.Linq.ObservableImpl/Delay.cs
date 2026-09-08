using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Threading;

namespace System.Reactive.Linq.ObservableImpl;

internal static class Delay<TSource>
{
	internal abstract class Base<TParent> : Producer<TSource, Base<TParent>._> where TParent : Base<TParent>
	{
		internal abstract class @_ : IdentitySink<TSource>
		{
			protected readonly IScheduler _scheduler;

			private IStopwatch? _watch;

			protected TimeSpan Elapsed => _watch.Elapsed;

			protected _(TParent parent, IObserver<TSource> observer)
				: base(observer)
			{
				_scheduler = parent._scheduler;
			}

			public void Run(TParent parent)
			{
				_watch = _scheduler.StartStopwatch();
				RunCore(parent);
				base.Run(parent._source);
			}

			protected abstract void RunCore(TParent parent);
		}

		internal abstract class S(TParent parent, IObserver<TSource> observer) : @_(parent, observer)
		{
			protected readonly object _gate = new object();

			protected SerialDisposableValue _cancelable;

			protected TimeSpan _delay;

			protected bool _ready;

			protected bool _active;

			protected bool _running;

			protected Queue<System.Reactive.TimeInterval<TSource>> _queue = new Queue<System.Reactive.TimeInterval<TSource>>();

			private bool _hasCompleted;

			private TimeSpan _completeAt;

			private bool _hasFailed;

			private Exception? _exception;

			protected override void Dispose(bool disposing)
			{
				base.Dispose(disposing);
				if (disposing)
				{
					_cancelable.Dispose();
				}
			}

			public override void OnNext(TSource value)
			{
				bool flag = false;
				lock (_gate)
				{
					TimeSpan interval = base.Elapsed.Add(_delay);
					_queue.Enqueue(new System.Reactive.TimeInterval<TSource>(value, interval));
					flag = _ready && !_active;
					_active = true;
				}
				if (flag)
				{
					DrainQueue(_delay);
				}
			}

			public override void OnError(Exception error)
			{
				DisposeUpstream();
				bool flag = false;
				lock (_gate)
				{
					_queue.Clear();
					_exception = error;
					_hasFailed = true;
					flag = !_running;
				}
				if (flag)
				{
					ForwardOnError(error);
				}
			}

			public override void OnCompleted()
			{
				DisposeUpstream();
				bool flag = false;
				lock (_gate)
				{
					TimeSpan completeAt = base.Elapsed.Add(_delay);
					_completeAt = completeAt;
					_hasCompleted = true;
					flag = _ready && !_active;
					_active = true;
				}
				if (flag)
				{
					DrainQueue(_delay);
				}
			}

			protected void DrainQueue(TimeSpan next)
			{
				_cancelable.Disposable = _scheduler.Schedule(this, next, delegate(S @this, Action<S, TimeSpan> a)
				{
					@this.DrainQueue(a);
				});
			}

			private void DrainQueue(Action<S, TimeSpan> recurse)
			{
				lock (_gate)
				{
					if (_hasFailed)
					{
						return;
					}
					_running = true;
				}
				bool flag = false;
				bool flag2;
				Exception error;
				bool flag4;
				bool flag5;
				TimeSpan arg;
				while (true)
				{
					flag2 = false;
					error = null;
					bool flag3 = false;
					TSource value = default(TSource);
					flag4 = false;
					flag5 = false;
					arg = default(TimeSpan);
					lock (_gate)
					{
						if (_hasFailed)
						{
							error = _exception;
							flag2 = true;
							_running = false;
						}
						else
						{
							TimeSpan elapsed = base.Elapsed;
							if (_queue.Count > 0)
							{
								TimeSpan interval = _queue.Peek().Interval;
								if (interval.CompareTo(elapsed) <= 0 && !flag)
								{
									value = _queue.Dequeue().Value;
									flag3 = true;
								}
								else
								{
									flag5 = true;
									arg = Scheduler.Normalize(interval.Subtract(elapsed));
									_running = false;
								}
							}
							else if (_hasCompleted)
							{
								if (_completeAt.CompareTo(elapsed) <= 0 && !flag)
								{
									flag4 = true;
								}
								else
								{
									flag5 = true;
									arg = Scheduler.Normalize(_completeAt.Subtract(elapsed));
									_running = false;
								}
							}
							else
							{
								_running = false;
								_active = false;
							}
						}
					}
					if (!flag3)
					{
						break;
					}
					ForwardOnNext(value);
					flag = true;
				}
				if (flag4)
				{
					ForwardOnCompleted();
				}
				else if (flag2)
				{
					ForwardOnError(error);
				}
				else if (flag5)
				{
					recurse(this, arg);
				}
			}
		}

		protected abstract class L(TParent parent, IObserver<TSource> observer) : @_(parent, observer)
		{
			protected readonly object _gate = new object();

			private readonly SemaphoreSlim _evt = new SemaphoreSlim(0);

			protected Queue<System.Reactive.TimeInterval<TSource>> _queue = new Queue<System.Reactive.TimeInterval<TSource>>();

			protected SerialDisposableValue _cancelable;

			protected TimeSpan _delay;

			private bool _hasCompleted;

			private TimeSpan _completeAt;

			private bool _hasFailed;

			private Exception? _exception;

			protected override void Dispose(bool disposing)
			{
				base.Dispose(disposing);
				if (disposing)
				{
					_cancelable.Dispose();
				}
			}

			protected void ScheduleDrain()
			{
				CancellationDisposable cancellationDisposable = new CancellationDisposable();
				_cancelable.Disposable = cancellationDisposable;
				_scheduler.AsLongRunning().ScheduleLongRunning(cancellationDisposable.Token, DrainQueue);
			}

			public override void OnNext(TSource value)
			{
				lock (_gate)
				{
					TimeSpan interval = base.Elapsed.Add(_delay);
					_queue.Enqueue(new System.Reactive.TimeInterval<TSource>(value, interval));
					_evt.Release();
				}
			}

			public override void OnError(Exception error)
			{
				DisposeUpstream();
				lock (_gate)
				{
					_queue.Clear();
					_exception = error;
					_hasFailed = true;
					_evt.Release();
				}
			}

			public override void OnCompleted()
			{
				DisposeUpstream();
				lock (_gate)
				{
					TimeSpan completeAt = base.Elapsed.Add(_delay);
					_completeAt = completeAt;
					_hasCompleted = true;
					_evt.Release();
				}
			}

			private void DrainQueue(CancellationToken token, ICancelable cancel)
			{
				bool flag;
				Exception error;
				bool flag3;
				while (true)
				{
					try
					{
						_evt.Wait(token);
					}
					catch (OperationCanceledException)
					{
						return;
					}
					flag = false;
					error = null;
					bool flag2 = false;
					TSource value = default(TSource);
					flag3 = false;
					bool flag4 = false;
					TimeSpan dueTime = default(TimeSpan);
					lock (_gate)
					{
						if (_hasFailed)
						{
							error = _exception;
							flag = true;
						}
						else
						{
							TimeSpan elapsed = base.Elapsed;
							if (_queue.Count > 0)
							{
								System.Reactive.TimeInterval<TSource> timeInterval = _queue.Dequeue();
								flag2 = true;
								value = timeInterval.Value;
								TimeSpan interval = timeInterval.Interval;
								if (interval.CompareTo(elapsed) > 0)
								{
									flag4 = true;
									dueTime = Scheduler.Normalize(interval.Subtract(elapsed));
								}
							}
							else if (_hasCompleted)
							{
								flag3 = true;
								if (_completeAt.CompareTo(elapsed) > 0)
								{
									flag4 = true;
									dueTime = Scheduler.Normalize(_completeAt.Subtract(elapsed));
								}
							}
						}
					}
					if (flag4)
					{
						ManualResetEventSlim manualResetEventSlim = new ManualResetEventSlim();
						_scheduler.ScheduleAction(manualResetEventSlim, dueTime, delegate(ManualResetEventSlim slimTimer)
						{
							slimTimer.Set();
						});
						try
						{
							manualResetEventSlim.Wait(token);
						}
						catch (OperationCanceledException)
						{
							return;
						}
					}
					if (!flag2)
					{
						break;
					}
					ForwardOnNext(value);
				}
				if (flag3)
				{
					ForwardOnCompleted();
				}
				else if (flag)
				{
					ForwardOnError(error);
				}
			}
		}

		protected readonly IObservable<TSource> _source;

		protected readonly IScheduler _scheduler;

		protected Base(IObservable<TSource> source, IScheduler scheduler)
		{
			_source = source;
			_scheduler = scheduler;
		}
	}

	internal sealed class Absolute : Base<Absolute>
	{
		private new sealed class S : Base<Absolute>.S
		{
			public S(Absolute parent, IObserver<TSource> observer)
				: base(parent, observer)
			{
			}

			protected override void RunCore(Absolute parent)
			{
				_ready = false;
				_cancelable.TrySetFirst(parent._scheduler.ScheduleAction(this, parent._dueTime, delegate(S @this)
				{
					@this.Start();
				}));
			}

			private void Start()
			{
				TimeSpan next = default(TimeSpan);
				bool flag = false;
				lock (_gate)
				{
					_delay = base.Elapsed;
					Queue<System.Reactive.TimeInterval<TSource>> queue = _queue;
					_queue = new Queue<System.Reactive.TimeInterval<TSource>>();
					if (queue.Count > 0)
					{
						next = queue.Peek().Interval;
						while (queue.Count > 0)
						{
							System.Reactive.TimeInterval<TSource> timeInterval = queue.Dequeue();
							_queue.Enqueue(new System.Reactive.TimeInterval<TSource>(timeInterval.Value, timeInterval.Interval.Add(_delay)));
						}
						flag = true;
						_active = true;
					}
					_ready = true;
				}
				if (flag)
				{
					DrainQueue(next);
				}
			}
		}

		private new sealed class L : Base<Absolute>.L
		{
			public L(Absolute parent, IObserver<TSource> observer)
				: base(parent, observer)
			{
			}

			protected override void RunCore(Absolute parent)
			{
				_cancelable.TrySetFirst(parent._scheduler.ScheduleAction(this, parent._dueTime, delegate(L @this)
				{
					@this.Start();
				}));
			}

			private void Start()
			{
				lock (_gate)
				{
					_delay = base.Elapsed;
					Queue<System.Reactive.TimeInterval<TSource>> queue = _queue;
					_queue = new Queue<System.Reactive.TimeInterval<TSource>>();
					while (queue.Count > 0)
					{
						System.Reactive.TimeInterval<TSource> timeInterval = queue.Dequeue();
						_queue.Enqueue(new System.Reactive.TimeInterval<TSource>(timeInterval.Value, timeInterval.Interval.Add(_delay)));
					}
				}
				ScheduleDrain();
			}
		}

		private readonly DateTimeOffset _dueTime;

		public Absolute(IObservable<TSource> source, DateTimeOffset dueTime, IScheduler scheduler)
			: base(source, scheduler)
		{
			_dueTime = dueTime;
		}

		protected override @_ CreateSink(IObserver<TSource> observer)
		{
			if (_scheduler.AsLongRunning() == null)
			{
				return new S(this, observer);
			}
			return new L(this, observer);
		}

		protected override void Run(@_ sink)
		{
			sink.Run(this);
		}
	}

	internal sealed class Relative : Base<Relative>
	{
		private new sealed class S : Base<Relative>.S
		{
			public S(Relative parent, IObserver<TSource> observer)
				: base(parent, observer)
			{
			}

			protected override void RunCore(Relative parent)
			{
				_ready = true;
				_delay = Scheduler.Normalize(parent._dueTime);
			}
		}

		private new sealed class L : Base<Relative>.L
		{
			public L(Relative parent, IObserver<TSource> observer)
				: base(parent, observer)
			{
			}

			protected override void RunCore(Relative parent)
			{
				_delay = Scheduler.Normalize(parent._dueTime);
				ScheduleDrain();
			}
		}

		private readonly TimeSpan _dueTime;

		public Relative(IObservable<TSource> source, TimeSpan dueTime, IScheduler scheduler)
			: base(source, scheduler)
		{
			_dueTime = dueTime;
		}

		protected override @_ CreateSink(IObserver<TSource> observer)
		{
			if (_scheduler.AsLongRunning() == null)
			{
				return new S(this, observer);
			}
			return new L(this, observer);
		}

		protected override void Run(@_ sink)
		{
			sink.Run(this);
		}
	}
}
internal static class Delay<TSource, TDelay>
{
	internal abstract class Base<TParent> : Producer<TSource, Base<TParent>._> where TParent : Base<TParent>
	{
		internal abstract class @_ : IdentitySink<TSource>
		{
			private sealed class DelayObserver : SafeObserver<TDelay>
			{
				private readonly @_ _parent;

				private readonly TSource _value;

				private bool _once;

				public DelayObserver(@_ parent, TSource value)
				{
					_parent = parent;
					_value = value;
				}

				public override void OnNext(TDelay value)
				{
					if (!_once)
					{
						_once = true;
						lock (_parent._gate)
						{
							_parent.ForwardOnNext(_value);
							_parent._delays.Remove(this);
							_parent.CheckDone();
						}
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
					if (!_once)
					{
						lock (_parent._gate)
						{
							_parent.ForwardOnNext(_value);
							_parent._delays.Remove(this);
							_parent.CheckDone();
						}
					}
				}
			}

			private readonly CompositeDisposable _delays = new CompositeDisposable();

			private readonly object _gate = new object();

			private readonly Func<TSource, IObservable<TDelay>> _delaySelector;

			private bool _atEnd;

			private SingleAssignmentDisposableValue _subscription;

			protected _(Func<TSource, IObservable<TDelay>> delaySelector, IObserver<TSource> observer)
				: base(observer)
			{
				_delaySelector = delaySelector;
			}

			public void Run(TParent parent)
			{
				_atEnd = false;
				_subscription.Disposable = RunCore(parent);
			}

			protected override void Dispose(bool disposing)
			{
				if (disposing)
				{
					_subscription.Dispose();
					_delays.Dispose();
				}
				base.Dispose(disposing);
			}

			protected abstract IDisposable RunCore(TParent parent);

			public override void OnNext(TSource value)
			{
				IObservable<TDelay> source;
				try
				{
					source = _delaySelector(value);
				}
				catch (Exception error)
				{
					lock (_gate)
					{
						ForwardOnError(error);
						return;
					}
				}
				DelayObserver delayObserver = new DelayObserver(this, value);
				_delays.Add(delayObserver);
				delayObserver.SetResource(source.SubscribeSafe(delayObserver));
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
					_atEnd = true;
					_subscription.Dispose();
					CheckDone();
				}
			}

			private void CheckDone()
			{
				if (_atEnd && _delays.Count == 0)
				{
					ForwardOnCompleted();
				}
			}
		}

		protected readonly IObservable<TSource> _source;

		protected Base(IObservable<TSource> source)
		{
			_source = source;
		}
	}

	internal class Selector : Base<Selector>
	{
		private new sealed class @_ : Base<Selector>._
		{
			public _(Func<TSource, IObservable<TDelay>> delaySelector, IObserver<TSource> observer)
				: base(delaySelector, observer)
			{
			}

			protected override IDisposable RunCore(Selector parent)
			{
				return parent._source.SubscribeSafe(this);
			}
		}

		private readonly Func<TSource, IObservable<TDelay>> _delaySelector;

		public Selector(IObservable<TSource> source, Func<TSource, IObservable<TDelay>> delaySelector)
			: base(source)
		{
			_delaySelector = delaySelector;
		}

		protected override Base<Selector>._ CreateSink(IObserver<TSource> observer)
		{
			return new @_(_delaySelector, observer);
		}

		protected override void Run(Base<Selector>._ sink)
		{
			sink.Run(this);
		}
	}

	internal sealed class SelectorWithSubscriptionDelay : Base<SelectorWithSubscriptionDelay>
	{
		private new sealed class @_ : Base<SelectorWithSubscriptionDelay>._
		{
			private sealed class SubscriptionDelayObserver : IObserver<TDelay>, IDisposable
			{
				private readonly @_ _parent;

				private readonly IObservable<TSource> _source;

				private SerialDisposableValue _subscription;

				public SubscriptionDelayObserver(@_ parent, IObservable<TSource> source)
				{
					_parent = parent;
					_source = source;
				}

				internal void SetFirst(IDisposable d)
				{
					_subscription.TrySetFirst(d);
				}

				public void OnNext(TDelay value)
				{
					_subscription.Disposable = _source.SubscribeSafe(_parent);
				}

				public void OnError(Exception error)
				{
					_parent.ForwardOnError(error);
				}

				public void OnCompleted()
				{
					_subscription.Disposable = _source.SubscribeSafe(_parent);
				}

				public void Dispose()
				{
					_subscription.Dispose();
				}
			}

			public _(Func<TSource, IObservable<TDelay>> delaySelector, IObserver<TSource> observer)
				: base(delaySelector, observer)
			{
			}

			protected override IDisposable RunCore(SelectorWithSubscriptionDelay parent)
			{
				SubscriptionDelayObserver subscriptionDelayObserver = new SubscriptionDelayObserver(this, parent._source);
				subscriptionDelayObserver.SetFirst(parent._subscriptionDelay.SubscribeSafe(subscriptionDelayObserver));
				return subscriptionDelayObserver;
			}
		}

		private readonly IObservable<TDelay> _subscriptionDelay;

		private readonly Func<TSource, IObservable<TDelay>> _delaySelector;

		public SelectorWithSubscriptionDelay(IObservable<TSource> source, IObservable<TDelay> subscriptionDelay, Func<TSource, IObservable<TDelay>> delaySelector)
			: base(source)
		{
			_subscriptionDelay = subscriptionDelay;
			_delaySelector = delaySelector;
		}

		protected override Base<SelectorWithSubscriptionDelay>._ CreateSink(IObserver<TSource> observer)
		{
			return new @_(_delaySelector, observer);
		}

		protected override void Run(Base<SelectorWithSubscriptionDelay>._ sink)
		{
			sink.Run(this);
		}
	}
}
