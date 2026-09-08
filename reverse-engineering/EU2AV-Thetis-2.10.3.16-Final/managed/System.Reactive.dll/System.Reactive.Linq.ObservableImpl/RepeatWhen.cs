using System.Reactive.Disposables;
using System.Reactive.Subjects;
using System.Threading;

namespace System.Reactive.Linq.ObservableImpl;

internal sealed class RepeatWhen<T, U> : IObservable<T>
{
	private sealed class MainObserver : Sink<T>, IObserver<T>
	{
		internal sealed class HandlerObserver : IObserver<U>
		{
			private readonly MainObserver _main;

			internal HandlerObserver(MainObserver main)
			{
				_main = main;
			}

			public void OnCompleted()
			{
				_main.HandlerComplete();
			}

			public void OnError(Exception error)
			{
				_main.HandlerError(error);
			}

			public void OnNext(U value)
			{
				_main.HandlerNext();
			}
		}

		private readonly IObservable<T> _source;

		private readonly IObserver<object> _completeSignal;

		internal readonly HandlerObserver HandlerConsumer;

		internal SingleAssignmentDisposableValue _handlerUpstream;

		private IDisposable? _upstream;

		private int _trampoline;

		private int _halfSerializer;

		private Exception? _error;

		internal MainObserver(IObserver<T> downstream, IObservable<T> source, IObserver<object> completeSignal)
			: base(downstream)
		{
			_source = source;
			_completeSignal = completeSignal;
			HandlerConsumer = new HandlerObserver(this);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Disposable.Dispose(ref _upstream);
				_handlerUpstream.Dispose();
			}
			base.Dispose(disposing);
		}

		public void OnCompleted()
		{
			if (Disposable.TrySetSerial(ref _upstream, null))
			{
				_completeSignal.OnNext(null);
			}
		}

		public void OnError(Exception error)
		{
			HalfSerializer.ForwardOnError(this, error, ref _halfSerializer, ref _error);
		}

		public void OnNext(T value)
		{
			HalfSerializer.ForwardOnNext(this, value, ref _halfSerializer, ref _error);
		}

		private void HandlerError(Exception error)
		{
			HalfSerializer.ForwardOnError(this, error, ref _halfSerializer, ref _error);
		}

		private void HandlerComplete()
		{
			HalfSerializer.ForwardOnCompleted(this, ref _halfSerializer, ref _error);
		}

		internal void HandlerNext()
		{
			if (Interlocked.Increment(ref _trampoline) != 1)
			{
				return;
			}
			do
			{
				SingleAssignmentDisposable singleAssignmentDisposable = new SingleAssignmentDisposable();
				if (Interlocked.CompareExchange(ref _upstream, singleAssignmentDisposable, null) != null)
				{
					break;
				}
				singleAssignmentDisposable.Disposable = _source.SubscribeSafe(this);
			}
			while (Interlocked.Decrement(ref _trampoline) != 0);
		}
	}

	private readonly IObservable<T> _source;

	private readonly Func<IObservable<object>, IObservable<U>> _handler;

	internal RepeatWhen(IObservable<T> source, Func<IObservable<object>, IObservable<U>> handler)
	{
		_source = source;
		_handler = handler;
	}

	public IDisposable Subscribe(IObserver<T> observer)
	{
		if (observer == null)
		{
			throw new ArgumentNullException("observer");
		}
		Subject<object> subject = new Subject<object>();
		IObservable<U> observable;
		try
		{
			observable = _handler(subject);
			if (observable == null)
			{
				throw new NullReferenceException("The handler returned a null IObservable");
			}
		}
		catch (Exception error)
		{
			observer.OnError(error);
			return Disposable.Empty;
		}
		MainObserver mainObserver = new MainObserver(observer, _source, new RedoSerializedObserver<object>(subject));
		IDisposable disposable = observable.SubscribeSafe(mainObserver.HandlerConsumer);
		mainObserver._handlerUpstream.Disposable = disposable;
		mainObserver.HandlerNext();
		return mainObserver;
	}
}
