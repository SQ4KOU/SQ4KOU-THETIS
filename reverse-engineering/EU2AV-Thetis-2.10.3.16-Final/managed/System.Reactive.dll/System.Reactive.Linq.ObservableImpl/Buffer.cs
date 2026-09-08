using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;

namespace System.Reactive.Linq.ObservableImpl;

internal static class Buffer<TSource>
{
	internal sealed class CountExact : Producer<IList<TSource>, CountExact.ExactSink>
	{
		internal sealed class ExactSink : Sink<TSource, IList<TSource>>
		{
			private readonly int _count;

			private int _index;

			private List<TSource>? _buffer;

			internal ExactSink(IObserver<IList<TSource>> observer, int count)
				: base(observer)
			{
				_count = count;
			}

			public override void OnNext(TSource value)
			{
				List<TSource> list = _buffer;
				if (list == null)
				{
					list = (_buffer = new List<TSource>());
				}
				list.Add(value);
				int num = _index + 1;
				if (num == _count)
				{
					_buffer = null;
					_index = 0;
					ForwardOnNext(list);
				}
				else
				{
					_index = num;
				}
			}

			public override void OnError(Exception error)
			{
				_buffer = null;
				ForwardOnError(error);
			}

			public override void OnCompleted()
			{
				List<TSource> buffer = _buffer;
				_buffer = null;
				if (buffer != null)
				{
					ForwardOnNext(buffer);
				}
				ForwardOnCompleted();
			}
		}

		private readonly IObservable<TSource> _source;

		private readonly int _count;

		public CountExact(IObservable<TSource> source, int count)
		{
			_source = source;
			_count = count;
		}

		protected override ExactSink CreateSink(IObserver<IList<TSource>> observer)
		{
			return new ExactSink(observer, _count);
		}

		protected override void Run(ExactSink sink)
		{
			sink.Run(_source);
		}
	}

	internal sealed class CountSkip : Producer<IList<TSource>, CountSkip.SkipSink>
	{
		internal sealed class SkipSink : Sink<TSource, IList<TSource>>
		{
			private readonly int _count;

			private readonly int _skip;

			private int _index;

			private List<TSource>? _buffer;

			internal SkipSink(IObserver<IList<TSource>> observer, int count, int skip)
				: base(observer)
			{
				_count = count;
				_skip = skip;
			}

			public override void OnNext(TSource value)
			{
				int index = _index;
				List<TSource> list = _buffer;
				if (index == 0)
				{
					list = (_buffer = new List<TSource>());
				}
				list?.Add(value);
				if (++index == _count)
				{
					_buffer = null;
					ForwardOnNext(list);
				}
				if (index == _skip)
				{
					_index = 0;
				}
				else
				{
					_index = index;
				}
			}

			public override void OnError(Exception error)
			{
				_buffer = null;
				ForwardOnError(error);
			}

			public override void OnCompleted()
			{
				List<TSource> buffer = _buffer;
				_buffer = null;
				if (buffer != null)
				{
					ForwardOnNext(buffer);
				}
				ForwardOnCompleted();
			}
		}

		private readonly IObservable<TSource> _source;

		private readonly int _count;

		private readonly int _skip;

		public CountSkip(IObservable<TSource> source, int count, int skip)
		{
			_source = source;
			_count = count;
			_skip = skip;
		}

		protected override SkipSink CreateSink(IObserver<IList<TSource>> observer)
		{
			return new SkipSink(observer, _count, _skip);
		}

		protected override void Run(SkipSink sink)
		{
			sink.Run(_source);
		}
	}

	internal sealed class CountOverlap : Producer<IList<TSource>, CountOverlap.OverlapSink>
	{
		internal sealed class OverlapSink : Sink<TSource, IList<TSource>>
		{
			private readonly Queue<IList<TSource>> _queue;

			private readonly int _count;

			private readonly int _skip;

			private int _n;

			public OverlapSink(IObserver<IList<TSource>> observer, int count, int skip)
				: base(observer)
			{
				_queue = new Queue<IList<TSource>>();
				_count = count;
				_skip = skip;
				CreateWindow();
			}

			private void CreateWindow()
			{
				List<TSource> item = new List<TSource>();
				_queue.Enqueue(item);
			}

			public override void OnNext(TSource value)
			{
				foreach (IList<TSource> item in _queue)
				{
					item.Add(value);
				}
				int num = _n - _count + 1;
				if (num >= 0 && num % _skip == 0)
				{
					IList<TSource> list = _queue.Dequeue();
					if (list.Count > 0)
					{
						ForwardOnNext(list);
					}
				}
				_n++;
				if (_n % _skip == 0)
				{
					CreateWindow();
				}
			}

			public override void OnError(Exception error)
			{
				_queue.Clear();
				ForwardOnError(error);
			}

			public override void OnCompleted()
			{
				while (_queue.Count > 0)
				{
					IList<TSource> list = _queue.Dequeue();
					if (list.Count > 0)
					{
						ForwardOnNext(list);
					}
				}
				ForwardOnCompleted();
			}
		}

		private readonly IObservable<TSource> _source;

		private readonly int _count;

		private readonly int _skip;

		public CountOverlap(IObservable<TSource> source, int count, int skip)
		{
			_source = source;
			_count = count;
			_skip = skip;
		}

		protected override OverlapSink CreateSink(IObserver<IList<TSource>> observer)
		{
			return new OverlapSink(observer, _count, _skip);
		}

		protected override void Run(OverlapSink sink)
		{
			sink.Run(_source);
		}
	}

	internal sealed class TimeSliding : Producer<IList<TSource>, TimeSliding._>
	{
		internal sealed class @_ : Sink<TSource, IList<TSource>>
		{
			private readonly TimeSpan _timeShift;

			private readonly IScheduler _scheduler;

			private readonly object _gate = new object();

			private readonly Queue<List<TSource>> _q = new Queue<List<TSource>>();

			private SerialDisposableValue _timerSerial;

			private TimeSpan _totalTime;

			private TimeSpan _nextShift;

			private TimeSpan _nextSpan;

			public _(TimeSliding parent, IObserver<IList<TSource>> observer)
				: base(observer)
			{
				_timeShift = parent._timeShift;
				_scheduler = parent._scheduler;
			}

			public void Run(TimeSliding parent)
			{
				_totalTime = TimeSpan.Zero;
				_nextShift = parent._timeShift;
				_nextSpan = parent._timeSpan;
				CreateWindow();
				CreateTimer();
				Run(parent._source);
			}

			protected override void Dispose(bool disposing)
			{
				if (disposing)
				{
					_timerSerial.Dispose();
				}
				base.Dispose(disposing);
			}

			private void CreateWindow()
			{
				List<TSource> item = new List<TSource>();
				_q.Enqueue(item);
			}

			private void CreateTimer()
			{
				SingleAssignmentDisposable singleAssignmentDisposable = new SingleAssignmentDisposable();
				_timerSerial.Disposable = singleAssignmentDisposable;
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
					if (isSpan && _q.Count > 0)
					{
						List<TSource> value = _q.Dequeue();
						ForwardOnNext(value);
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
					foreach (List<TSource> item in _q)
					{
						item.Add(value);
					}
				}
			}

			public override void OnError(Exception error)
			{
				lock (_gate)
				{
					while (_q.Count > 0)
					{
						_q.Dequeue().Clear();
					}
					ForwardOnError(error);
				}
			}

			public override void OnCompleted()
			{
				lock (_gate)
				{
					while (_q.Count > 0)
					{
						ForwardOnNext(_q.Dequeue());
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

		protected override @_ CreateSink(IObserver<IList<TSource>> observer)
		{
			return new @_(this, observer);
		}

		protected override void Run(@_ sink)
		{
			sink.Run(this);
		}
	}

	internal sealed class TimeHopping : Producer<IList<TSource>, TimeHopping._>
	{
		internal sealed class @_(IObserver<IList<TSource>> observer) : Sink<TSource, IList<TSource>>(observer)
		{
			private readonly object _gate = new object();

			private List<TSource> _list = new List<TSource>();

			private SingleAssignmentDisposableValue _periodicDisposable;

			public void Run(TimeHopping parent)
			{
				_periodicDisposable.Disposable = parent._scheduler.SchedulePeriodic(this, parent._timeSpan, delegate(@_ @this)
				{
					@this.Tick();
				});
				Run(parent._source);
			}

			protected override void Dispose(bool disposing)
			{
				if (disposing)
				{
					_periodicDisposable.Dispose();
				}
				base.Dispose(disposing);
			}

			private void Tick()
			{
				lock (_gate)
				{
					ForwardOnNext(_list);
					_list = new List<TSource>();
				}
			}

			public override void OnNext(TSource value)
			{
				lock (_gate)
				{
					_list.Add(value);
				}
			}

			public override void OnError(Exception error)
			{
				lock (_gate)
				{
					_list.Clear();
					ForwardOnError(error);
				}
			}

			public override void OnCompleted()
			{
				lock (_gate)
				{
					ForwardOnNext(_list);
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

		protected override @_ CreateSink(IObserver<IList<TSource>> observer)
		{
			return new @_(observer);
		}

		protected override void Run(@_ sink)
		{
			sink.Run(this);
		}
	}

	internal sealed class Ferry : Producer<IList<TSource>, Ferry._>
	{
		internal sealed class @_ : Sink<TSource, IList<TSource>>
		{
			private readonly Ferry _parent;

			private readonly object _gate = new object();

			private List<TSource> _s = new List<TSource>();

			private SerialDisposableValue _timerSerial;

			private int _n;

			private int _windowId;

			public _(Ferry parent, IObserver<IList<TSource>> observer)
				: base(observer)
			{
				_parent = parent;
			}

			public void Run()
			{
				_n = 0;
				_windowId = 0;
				CreateTimer(0);
				SetUpstream(_parent._source.SubscribeSafe(this));
			}

			protected override void Dispose(bool disposing)
			{
				if (disposing)
				{
					_timerSerial.Dispose();
				}
				base.Dispose(disposing);
			}

			private void CreateTimer(int id)
			{
				SingleAssignmentDisposable singleAssignmentDisposable = new SingleAssignmentDisposable();
				_timerSerial.Disposable = singleAssignmentDisposable;
				singleAssignmentDisposable.Disposable = _parent._scheduler.ScheduleAction((this, id), _parent._timeSpan, delegate((@_ @this, int id) tuple)
				{
					tuple.@this.Tick(tuple.id);
				});
			}

			private void Tick(int id)
			{
				lock (_gate)
				{
					if (id == _windowId)
					{
						_n = 0;
						int id2 = ++_windowId;
						List<TSource> s = _s;
						_s = new List<TSource>();
						ForwardOnNext(s);
						CreateTimer(id2);
					}
				}
			}

			public override void OnNext(TSource value)
			{
				bool flag = false;
				int id = 0;
				lock (_gate)
				{
					_s.Add(value);
					_n++;
					if (_n == _parent._count)
					{
						flag = true;
						_n = 0;
						id = ++_windowId;
						List<TSource> s = _s;
						_s = new List<TSource>();
						ForwardOnNext(s);
					}
					if (flag)
					{
						CreateTimer(id);
					}
				}
			}

			public override void OnError(Exception error)
			{
				lock (_gate)
				{
					_s.Clear();
					ForwardOnError(error);
				}
			}

			public override void OnCompleted()
			{
				lock (_gate)
				{
					ForwardOnNext(_s);
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

		protected override @_ CreateSink(IObserver<IList<TSource>> observer)
		{
			return new @_(this, observer);
		}

		protected override void Run(@_ sink)
		{
			sink.Run();
		}
	}
}
internal static class Buffer<TSource, TBufferClosing>
{
	internal sealed class Selector : Producer<IList<TSource>, Selector._>
	{
		internal sealed class @_ : Sink<TSource, IList<TSource>>
		{
			private sealed class BufferClosingObserver : SafeObserver<TBufferClosing>
			{
				private readonly @_ _parent;

				public BufferClosingObserver(@_ parent)
				{
					_parent = parent;
				}

				public override void OnNext(TBufferClosing value)
				{
					_parent.CloseBuffer(this);
				}

				public override void OnError(Exception error)
				{
					_parent.OnError(error);
				}

				public override void OnCompleted()
				{
					_parent.CloseBuffer(this);
				}
			}

			private readonly object _gate = new object();

			private readonly AsyncLock _bufferGate = new AsyncLock();

			private readonly Func<IObservable<TBufferClosing>> _bufferClosingSelector;

			private List<TSource> _buffer = new List<TSource>();

			private SerialDisposableValue _bufferClosingSerialDisposable;

			public _(Selector parent, IObserver<IList<TSource>> observer)
				: base(observer)
			{
				_bufferClosingSelector = parent._bufferClosingSelector;
			}

			public override void Run(IObservable<TSource> source)
			{
				base.Run(source);
				_bufferGate.Wait(this, delegate(@_ @this)
				{
					@this.CreateBufferClose();
				});
			}

			protected override void Dispose(bool disposing)
			{
				if (disposing)
				{
					_bufferClosingSerialDisposable.Dispose();
				}
				base.Dispose(disposing);
			}

			private void CreateBufferClose()
			{
				IObservable<TBufferClosing> source;
				try
				{
					source = _bufferClosingSelector();
				}
				catch (Exception error)
				{
					lock (_gate)
					{
						ForwardOnError(error);
						return;
					}
				}
				BufferClosingObserver bufferClosingObserver = new BufferClosingObserver(this);
				_bufferClosingSerialDisposable.Disposable = bufferClosingObserver;
				bufferClosingObserver.SetResource(source.SubscribeSafe(bufferClosingObserver));
			}

			private void CloseBuffer(IDisposable closingSubscription)
			{
				closingSubscription.Dispose();
				lock (_gate)
				{
					List<TSource> buffer = _buffer;
					_buffer = new List<TSource>();
					ForwardOnNext(buffer);
				}
				_bufferGate.Wait(this, delegate(@_ @this)
				{
					@this.CreateBufferClose();
				});
			}

			public override void OnNext(TSource value)
			{
				lock (_gate)
				{
					_buffer.Add(value);
				}
			}

			public override void OnError(Exception error)
			{
				lock (_gate)
				{
					_buffer.Clear();
					ForwardOnError(error);
				}
			}

			public override void OnCompleted()
			{
				lock (_gate)
				{
					ForwardOnNext(_buffer);
					ForwardOnCompleted();
				}
			}
		}

		private readonly IObservable<TSource> _source;

		private readonly Func<IObservable<TBufferClosing>> _bufferClosingSelector;

		public Selector(IObservable<TSource> source, Func<IObservable<TBufferClosing>> bufferClosingSelector)
		{
			_source = source;
			_bufferClosingSelector = bufferClosingSelector;
		}

		protected override @_ CreateSink(IObserver<IList<TSource>> observer)
		{
			return new @_(this, observer);
		}

		protected override void Run(@_ sink)
		{
			sink.Run(_source);
		}
	}

	internal sealed class Boundaries : Producer<IList<TSource>, Boundaries._>
	{
		internal sealed class @_(IObserver<IList<TSource>> observer) : Sink<TSource, IList<TSource>>(observer)
		{
			private sealed class BufferClosingObserver : IObserver<TBufferClosing>
			{
				private readonly @_ _parent;

				public BufferClosingObserver(@_ parent)
				{
					_parent = parent;
				}

				public void OnNext(TBufferClosing value)
				{
					lock (_parent._gate)
					{
						List<TSource> buffer = _parent._buffer;
						_parent._buffer = new List<TSource>();
						_parent.ForwardOnNext(buffer);
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

			private List<TSource> _buffer = new List<TSource>();

			private SingleAssignmentDisposableValue _boundariesDisposable;

			public void Run(Boundaries parent)
			{
				Run(parent._source);
				_boundariesDisposable.Disposable = parent._bufferBoundaries.SubscribeSafe(new BufferClosingObserver(this));
			}

			protected override void Dispose(bool disposing)
			{
				if (disposing)
				{
					_boundariesDisposable.Dispose();
				}
				base.Dispose(disposing);
			}

			public override void OnNext(TSource value)
			{
				lock (_gate)
				{
					_buffer.Add(value);
				}
			}

			public override void OnError(Exception error)
			{
				lock (_gate)
				{
					_buffer.Clear();
					ForwardOnError(error);
				}
			}

			public override void OnCompleted()
			{
				lock (_gate)
				{
					ForwardOnNext(_buffer);
					ForwardOnCompleted();
				}
			}
		}

		private readonly IObservable<TSource> _source;

		private readonly IObservable<TBufferClosing> _bufferBoundaries;

		public Boundaries(IObservable<TSource> source, IObservable<TBufferClosing> bufferBoundaries)
		{
			_source = source;
			_bufferBoundaries = bufferBoundaries;
		}

		protected override @_ CreateSink(IObserver<IList<TSource>> observer)
		{
			return new @_(observer);
		}

		protected override void Run(@_ sink)
		{
			sink.Run(this);
		}
	}
}
