using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Subjects;

namespace System.Reactive.Linq.ObservableImpl;

internal abstract class EventProducer<TDelegate, TArgs> : BasicProducer<TArgs>
{
	private sealed class Session
	{
		private readonly EventProducer<TDelegate, TArgs> _parent;

		private readonly Subject<TArgs> _subject;

		private readonly SingleAssignmentDisposable _removeHandler = new SingleAssignmentDisposable();

		private int _count;

		public Session(EventProducer<TDelegate, TArgs> parent)
		{
			_parent = parent;
			_subject = new Subject<TArgs>();
		}

		public IDisposable Connect(IObserver<TArgs> observer)
		{
			IDisposable disposable = _subject.Subscribe(observer);
			if (++_count == 1)
			{
				try
				{
					Initialize();
				}
				catch (Exception error)
				{
					_count--;
					disposable.Dispose();
					observer.OnError(error);
					return Disposable.Empty;
				}
			}
			return Disposable.Create((this, _parent, disposable), delegate((Session, EventProducer<TDelegate, TArgs> _parent, IDisposable connection) tuple)
			{
				(Session, EventProducer<TDelegate, TArgs> _parent, IDisposable connection) tuple2 = tuple;
				var (session, eventProducer, _) = tuple2;
				tuple2.connection.Dispose();
				lock (eventProducer._gate)
				{
					if (--session._count == 0)
					{
						eventProducer._scheduler.ScheduleAction(session._removeHandler, delegate(SingleAssignmentDisposable handler)
						{
							handler.Dispose();
						});
						eventProducer._session = null;
					}
				}
			});
		}

		private void Initialize()
		{
			TDelegate handler = _parent.GetHandler(_subject.OnNext);
			_parent._scheduler.ScheduleAction(handler, (Action<TDelegate>)AddHandler);
		}

		private void AddHandler(TDelegate onNext)
		{
			IDisposable disposable;
			try
			{
				disposable = _parent.AddHandler(onNext);
			}
			catch (Exception error)
			{
				_subject.OnError(error);
				return;
			}
			_removeHandler.Disposable = disposable;
		}
	}

	private readonly IScheduler _scheduler;

	private readonly object _gate;

	private Session? _session;

	protected EventProducer(IScheduler scheduler)
	{
		_scheduler = scheduler;
		_gate = new object();
	}

	protected abstract TDelegate GetHandler(Action<TArgs> onNext);

	protected abstract IDisposable AddHandler(TDelegate handler);

	protected override IDisposable Run(IObserver<TArgs> observer)
	{
		lock (_gate)
		{
			if (_session == null)
			{
				_session = new Session(this);
			}
			return _session.Connect(observer);
		}
	}
}
