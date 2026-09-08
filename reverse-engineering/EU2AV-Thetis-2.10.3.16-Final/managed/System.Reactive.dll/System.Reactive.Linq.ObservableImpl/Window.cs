using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Subjects;

namespace System.Reactive.Linq.ObservableImpl;

internal static class Window<TSource>
{
	internal sealed class Count : Producer<IObservable<TSource>, Count._>
	{
		internal sealed class @_ : Sink<TSource, IObservable<TSource>>
		{
			private readonly Queue<ISubject<TSource>> _queue = new Queue<ISubject<TSource>>();

			private readonly SingleAssignmentDisposable _m = new SingleAssignmentDisposable();

			private readonly RefCountDisposable _refCountDisposable;

			private readonly int _count;

			private readonly int _skip;

			private int _n;

			public _(Count parent, IObserver<IObservable<TSource>> observer)
				: base(observer)
			{
				_refCountDisposable = new RefCountDisposable(_m);
				_count = parent._count;
				_skip = parent._skip;
			}

			public override void Run(IObservable<TSource> source)
			{
				IObservable<TSource> value = CreateWindow();
				ForwardOnNext(value);
				_m.Disposable = source.SubscribeSafe(this);
				SetUpstream(_refCountDisposable);
			}

			private IObservable<TSource> CreateWindow()
			{
				Subject<TSource> subject = new Subject<TSource>();
				_queue.Enqueue(subject);
				return new WindowObservable<TSource>(subject, _refCountDisposable);
			}

			public override void OnNext(TSource value)
			{
				foreach (ISubject<TSource> item in _queue)
				{
					item.OnNext(value);
				}
				int num = _n - _count + 1;
				if (num >= 0 && num % _skip == 0)
				{
					_queue.Dequeue().OnCompleted();
				}
				_n++;
				if (_n % _skip == 0)
				{
					IObservable<TSource> value2 = CreateWindow();
					ForwardOnNext(value2);
				}
			}

			public override void OnError(Exception error)
			{
				while (_queue.Count > 0)
				{
					_queue.Dequeue().OnError(error);
				}
				ForwardOnError(error);
			}

			public override void OnCompleted()
			{
				while (_queue.Count > 0)
				{
					_queue.Dequeue().OnCompleted();
				}
				ForwardOnCompleted();
			}
		}

		private readonly IObservable<TSource> _source;

		private readonly int _count;

		private readonly int _skip;

		public Count(IObservable<TSource> source, int count, int skip)
		{
			_source = source;
			_count = count;
			_skip = skip;
		}

		protected override @_ CreateSink(IObserver<IObservable<TSource>> observer)
		{
			return new @_(this, observer);
		}

		protected override void Run(@_ sink)
		{
			sink.Run(_source);
		}
	}

	internal sealed class TimeSliding : Producer<IObservable<TSource>, TimeSliding._>
	{
		internal sealed class @_ : Sink<TSource, IObservable<TSource>>
		{
			private readonly object _gate = new object();

			private readonly Queue<ISubject<TSource>> _q = new Queue<ISubject<TSource>>();

			private readonly SerialDisposable _timerD = new SerialDisposable();

			private readonly IScheduler _scheduler;

			private readonly TimeSpan _timeShift;

			private RefCountDisposable? _refCountDisposable;

			private TimeSpan _totalTime;

			private TimeSpan _nextShift;

			private TimeSpan _nextSpan;

			public _(TimeSliding parent, IObserver<IObservable<TSource>> observer)
				: base(observer)
			{
				_scheduler = parent._scheduler;
				_timeShift = parent._timeShift;
			}

			public void Run(TimeSliding parent)
			{
				_totalTime = TimeSpan.Zero;
				_nextShift = parent._timeShift;
				_nextSpan = parent._timeSpan;
				CompositeDisposable compositeDisposable = new CompositeDisposable(2) { _timerD };
				_refCountDisposable = new RefCountDisposable(compositeDisposable);
				CreateWindow();
				CreateTimer();
				compositeDisposable.Add(parent._source.SubscribeSafe(this));
				SetUpstream(_refCountDisposable);
			}

			private void CreateWindow()
			{
				Subject<TSource> subject = new Subject<TSource>();
				_q.Enqueue(subject);
				ForwardOnNext(new WindowObservable<TSource>(subject, _refCountDisposable));
			}

			private void CreateTimer()
			{
				SingleAssignmentDisposable singleAssignmentDisposable = new SingleAssignmentDisposable();
				_timerD.Disposable = singleAssignmentDisposable;
				bool flag = false;
				bool flag2 = false;
				if (_nextSpan == _nextShift)
				{
					flag = true;
					flag2 = true;
				}
				else if (_nextSpan < _nextShift)
				{
					flag = true;
				}
				else
				{
					flag2 = true;
				}
				TimeSpan timeSpan = (flag ? _nextSpan : _nextShift);
				TimeSpan dueTime = timeSpan - _totalTime;
				_totalTime = timeSpan;
				if (flag)
				{
					_nextSpan += _timeShift;
				}
				if (flag2)
				{
					_nextShift += _timeShift;
				}
				singleAssignmentDisposable.Disposable = _scheduler.ScheduleAction((this, flag, flag2), dueTime, delegate((@_ @this, bool isSpan, bool isShift) tuple)
				{
					tuple.@this.Tick(tuple.isSpan, tuple.isShift);
				});
			}

			private void Tick(bool isSpan, bool isShift)
			{
				lock (_gate)
				{
					if (isSpan)
					{
						_q.Dequeue().OnCompleted();
					}
					if (isShift)
					{
						CreateWindow();
					}
				}
				CreateTimer();
			}

			public override void OnNext(TSource value)
			{
				lock (_gate)
				{
					foreach (ISubject<TSource> item in _q)
					{
						item.OnNext(value);
					}
				}
			}

			public override void OnError(Exception error)
			{
				lock (_gate)
				{
					foreach (ISubject<TSource> item in _q)
					{
						item.OnError(error);
					}
					ForwardOnError(error);
				}
			}

			public override void OnCompleted()
			{
				lock (_gate)
				{
					foreach (ISubject<TSource> item in _q)
					{
						item.OnCompleted();
					}
					ForwardOnCompleted();
				}
			}
		}

		private readonly IObservable<TSource> _source;

		private readonly TimeSpan _timeSpan;

		private readonly TimeSpan _timeShift;

		private readonly IScheduler _scheduler;

		public TimeSliding(IObservable<TSource> source, TimeSpan timeSpan, TimeSpan timeShift, IScheduler scheduler)
		{
			_source = source;
			_timeSpan = timeSpan;
			_timeShift = timeShift;
			_scheduler = scheduler;
		}

		protected override @_ CreateSink(IObserver<IObservable<TSource>> observer)
		{
			return new @_(this, observer);
		}

		protected override void Run(@_ sink)
		{
			sink.Run(this);
		}
	}

	internal sealed class TimeHopping : Producer<IObservable<TSource>, TimeHopping._>
	{
		internal sealed class @_ : Sink<TSource, IObservable<TSource>>
		{
			private readonly object _gate = new object();

			private Subject<TSource> _subject;

			private RefCountDisposable? _refCountDisposable;

			public _(IObserver<IObservable<TSource>> observer)
				: base(observer)
			{
				_subject = new Subject<TSource>();
			}

			public void Run(TimeHopping parent)
			{
				CompositeDisposable compositeDisposable = new CompositeDisposable(2);
				_refCountDisposable = new RefCountDisposable(compositeDisposable);
				NextWindow();
				compositeDisposable.Add(parent._scheduler.SchedulePeriodic(this, parent._timeSpan, delegate(@_ @this)
				{
					@this.Tick();
				}));
				compositeDisposable.Add(parent._source.SubscribeSafe(this));
				SetUpstream(_refCountDisposable);
			}

			private void Tick()
			{
				lock (_gate)
				{
					_subject.OnCompleted();
					_subject = new Subject<TSource>();
					NextWindow();
				}
			}

			private void NextWindow()
			{
				ForwardOnNext(new WindowObservable<TSource>(_subject, _refCountDisposable));
			}

			public override void OnNext(TSource value)
			{
				lock (_gate)
				{
					_subject.OnNext(value);
				}
			}

			public override void OnError(Exception error)
			{
				lock (_gate)
				{
					_subject.OnError(error);
					ForwardOnError(error);
				}
			}

			public override void OnCompleted()
			{
				lock (_gate)
				{
					_subject.OnCompleted();
					ForwardOnCompleted();
				}
			}
		}

		private readonly IObservable<TSource> _source;

		private readonly TimeSpan _timeSpan;

		private readonly IScheduler _scheduler;

		public TimeHopping(IObservable<TSource> source, TimeSpan timeSpan, IScheduler scheduler)
		{
			_source = source;
			_timeSpan = timeSpan;
			_scheduler = scheduler;
		}

		protected override @_ CreateSink(IObserver<IObservable<TSource>> observer)
		{
			return new @_(observer);
		}

		protected override void Run(@_ sink)
		{
			sink.Run(this);
		}
	}

	internal sealed class Ferry : Producer<IObservable<TSource>, Ferry._>
	{
		internal sealed class @_ : Sink<TSource, IObservable<TSource>>
		{
			private readonly object _gate = new object();

			private readonly SerialDisposable _timerD = new SerialDisposable();

			private readonly int _count;

			private readonly TimeSpan _timeSpan;

			private readonly IScheduler _scheduler;

			private Subject<TSource> _s;

			private int _n;

			private RefCountDisposable? _refCountDisposable;

			public _(Ferry parent, IObserver<IObservable<TSource>> observer)
				: base(observer)
			{
				_count = parent._count;
				_timeSpan = parent._timeSpan;
				_scheduler = parent._scheduler;
				_s = new Subject<TSource>();
			}

			public override void Run(IObservable<TSource> source)
			{
				CompositeDisposable compositeDisposable = new CompositeDisposable(2) { _timerD };
				_refCountDisposable = new RefCountDisposable(compositeDisposable);
				NextWindow();
				CreateTimer(_s);
				compositeDisposable.Add(source.SubscribeSafe(this));
				SetUpstream(_refCountDisposable);
			}

			private void CreateTimer(Subject<TSource> window)
			{
				SingleAssignmentDisposable singleAssignmentDisposable = new SingleAssignmentDisposable();
				_timerD.Disposable = singleAssignmentDisposable;
				singleAssignmentDisposable.Disposable = _scheduler.ScheduleAction((this, window), _timeSpan, delegate((@_ @this, Subject<TSource> window) tuple)
				{
					tuple.@this.Tick(tuple.window);
				});
			}

			private void NextWindow()
			{
				ForwardOnNext(new WindowObservable<TSource>(_s, _refCountDisposable));
			}

			private void Tick(Subject<TSource> window)
			{
				Subject<TSource> subject;
				lock (_gate)
				{
					if (window != _s)
					{
						return;
					}
					_n = 0;
					subject = new Subject<TSource>();
					_s.OnCompleted();
					_s = subject;
					NextWindow();
				}
				CreateTimer(subject);
			}

			public override void OnNext(TSource value)
			{
				Subject<TSource> subject = null;
				lock (_gate)
				{
					_s.OnNext(value);
					_n++;
					if (_n == _count)
					{
						_n = 0;
						subject = new Subject<TSource>();
						_s.OnCompleted();
						_s = subject;
						NextWindow();
					}
				}
				if (subject != null)
				{
					CreateTimer(subject);
				}
			}

			public override void OnError(Exception error)
			{
				lock (_gate)
				{
					_s.OnError(error);
					ForwardOnError(error);
				}
			}

			public override void OnCompleted()
			{
				lock (_gate)
				{
					_s.OnCompleted();
					ForwardOnCompleted();
				}
			}
		}

		private readonly IObservable<TSource> _source;

		private readonly int _count;

		private readonly TimeSpan _timeSpan;

		private readonly IScheduler _scheduler;

		public Ferry(IObservable<TSource> source, TimeSpan timeSpan, int count, IScheduler scheduler)
		{
			_source = source;
			_timeSpan = timeSpan;
			_count = count;
			_scheduler = scheduler;
		}

		protected override @_ CreateSink(IObserver<IObservable<TSource>> observer)
		{
			return new @_(this, observer);
		}

		protected override void Run(@_ sink)
		{
			sink.Run(_source);
		}
	}
}
internal static class Window<TSource, TWindowClosing>
{
	internal sealed class Selector : Producer<IObservable<TSource>, Selector._>
	{
		internal sealed class @_ : Sink<TSource, IObservable<TSource>>
		{
			private sealed class WindowClosingObserver : SafeObserver<TWindowClosing>
			{
				private readonly @_ _parent;

				public WindowClosingObserver(@_ parent)
				{
					_parent = parent;
				}

				public override void OnNext(TWindowClosing value)
				{
					_parent.CloseWindow(this);
				}

				public override void OnError(Exception error)
				{
					_parent.OnError(error);
				}

				public override void OnCompleted()
				{
					_parent.CloseWindow(this);
				}
			}

			private readonly object _gate = new object();

			private readonly AsyncLock _windowGate = new AsyncLock();

			private readonly SerialDisposable _m = new SerialDisposable();

			private readonly Func<IObservable<TWindowClosing>> _windowClosingSelector;

			private Subject<TSource> _window;

			private RefCountDisposable? _refCountDisposable;

			public _(Selector parent, IObserver<IObservable<TSource>> observer)
				: base(observer)
			{
				_windowClosingSelector = parent._windowClosingSelector;
				_window = new Subject<TSource>();
			}

			public override void Run(IObservable<TSource> source)
			{
				CompositeDisposable compositeDisposable = new CompositeDisposable(2) { _m };
				_refCountDisposable = new RefCountDisposable(compositeDisposable);
				NextWindow();
				compositeDisposable.Add(source.SubscribeSafe(this));
				_windowGate.Wait(this, delegate(@_ @this)
				{
					@this.CreateWindowClose();
				});
				SetUpstream(_refCountDisposable);
			}

			private void NextWindow()
			{
				WindowObservable<TSource> value = new WindowObservable<TSource>(_window, _refCountDisposable);
				ForwardOnNext(value);
			}

			private void CreateWindowClose()
			{
				IObservable<TWindowClosing> source;
				try
				{
					source = _windowClosingSelector();
				}
				catch (Exception error)
				{
					lock (_gate)
					{
						ForwardOnError(error);
						return;
					}
				}
				WindowClosingObserver windowClosingObserver = new WindowClosingObserver(this);
				_m.Disposable = windowClosingObserver;
				windowClosingObserver.SetResource(source.SubscribeSafe(windowClosingObserver));
			}

			private void CloseWindow(IDisposable closingSubscription)
			{
				closingSubscription.Dispose();
				lock (_gate)
				{
					_window.OnCompleted();
					_window = new Subject<TSource>();
					NextWindow();
				}
				_windowGate.Wait(this, delegate(@_ @this)
				{
					@this.CreateWindowClose();
				});
			}

			public override void OnNext(TSource value)
			{
				lock (_gate)
				{
					_window.OnNext(value);
				}
			}

			public override void OnError(Exception error)
			{
				lock (_gate)
				{
					_window.OnError(error);
					ForwardOnError(error);
				}
			}

			public override void OnCompleted()
			{
				lock (_gate)
				{
					_window.OnCompleted();
					ForwardOnCompleted();
				}
			}
		}

		private readonly IObservable<TSource> _source;

		private readonly Func<IObservable<TWindowClosing>> _windowClosingSelector;

		public Selector(IObservable<TSource> source, Func<IObservable<TWindowClosing>> windowClosingSelector)
		{
			_source = source;
			_windowClosingSelector = windowClosingSelector;
		}

		protected override @_ CreateSink(IObserver<IObservable<TSource>> observer)
		{
			return new @_(this, observer);
		}

		protected override void Run(@_ sink)
		{
			sink.Run(_source);
		}
	}

	internal sealed class Boundaries : Producer<IObservable<TSource>, Boundaries._>
	{
		internal sealed class @_ : Sink<TSource, IObservable<TSource>>
		{
			private sealed class WindowClosingObserver : IObserver<TWindowClosing>
			{
				private readonly @_ _parent;

				public WindowClosingObserver(@_ parent)
				{
					_parent = parent;
				}

				public void OnNext(TWindowClosing value)
				{
					lock (_parent._gate)
					{
						_parent._window.OnCompleted();
						_parent._window = new Subject<TSource>();
						_parent.NextWindow();
					}
				}

				public void OnError(Exception error)
				{
					_parent.OnError(error);
				}

				public void OnCompleted()
				{
					_parent.OnCompleted();
				}
			}

			private readonly object _gate = new object();

			private Subject<TSource> _window;

			private RefCountDisposable? _refCountDisposable;

			public _(IObserver<IObservable<TSource>> observer)
				: base(observer)
			{
				_window = new Subject<TSource>();
			}

			public void Run(Boundaries parent)
			{
				CompositeDisposable compositeDisposable = new CompositeDisposable(2);
				_refCountDisposable = new RefCountDisposable(compositeDisposable);
				NextWindow();
				compositeDisposable.Add(parent._source.SubscribeSafe(this));
				compositeDisposable.Add(parent._windowBoundaries.SubscribeSafe(new WindowClosingObserver(this)));
				SetUpstream(_refCountDisposable);
			}

			private void NextWindow()
			{
				WindowObservable<TSource> value = new WindowObservable<TSource>(_window, _refCountDisposable);
				ForwardOnNext(value);
			}

			public override void OnNext(TSource value)
			{
				lock (_gate)
				{
					_window.OnNext(value);
				}
			}

			public override void OnError(Exception error)
			{
				lock (_gate)
				{
					_window.OnError(error);
					ForwardOnError(error);
				}
			}

			public override void OnCompleted()
			{
				lock (_gate)
				{
					_window.OnCompleted();
					ForwardOnCompleted();
				}
			}
		}

		private readonly IObservable<TSource> _source;

		private readonly IObservable<TWindowClosing> _windowBoundaries;

		public Boundaries(IObservable<TSource> source, IObservable<TWindowClosing> windowBoundaries)
		{
			_source = source;
			_windowBoundaries = windowBoundaries;
		}

		protected override @_ CreateSink(IObserver<IObservable<TSource>> observer)
		{
			return new @_(observer);
		}

		protected override void Run(@_ sink)
		{
			sink.Run(this);
		}
	}
}
