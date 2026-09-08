using System.Reactive.Disposables;
using System.Reactive.Subjects;
using System.Threading;

namespace System.Reactive.Linq.ObservableImpl;

internal sealed class RetryWhen<T, U> : IObservable<T>
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

		private readonly IObserver<Exception> _errorSignal;

		internal readonly HandlerObserver HandlerConsumer;

		private IDisposable? _upstream;

		internal SingleAssignmentDisposableValue HandlerUpstream;

		private int _trampoline;

		private int _halfSerializer;

		private Exception? _error;

		internal MainObserver(IObserver<T> downstream, IObservable<T> source, IObserver<Exception> errorSignal)
			: base(downstream)
		{
			_source = source;
			_errorSignal = errorSignal;
			HandlerConsumer = new HandlerObserver(this);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Disposable.Dispose(ref _upstream);
				HandlerUpstream.Dispose();
			}
			base.Dispose(disposing);
		}

		public void OnCompleted()
		{
			HalfSerializer.ForwardOnCompleted(this, ref _halfSerializer, ref _error);
		}

		public void OnError(Exception error)
		{
			if (Disposable.TrySetSerial(ref _upstream, null))
			{
				_errorSignal.OnNext(error);
			}
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
				if (Disposable.TrySetSingle(ref _upstream, singleAssignmentDisposable) != TrySetSingleResult.Success)
				{
					break;
				}
				singleAssignmentDisposable.Disposable = _source.SubscribeSafe(this);
			}
			while (Interlocked.Decrement(ref _trampoline) != 0);
		}
	}

	private readonly IObservable<T> _source;

	private readonly Func<IObservable<Exception>, IObservable<U>> _handler;

	internal RetryWhen(IObservable<T> source, Func<IObservable<Exception>, IObservable<U>> handler)
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
		Subject<Exception> subject = new Subject<Exception>();
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
		MainObserver mainObserver = new MainObserver(observer, _source, new RedoSerializedObserver<Exception>(subject));
		IDisposable disposable = observable.SubscribeSafe(mainObserver.HandlerConsumer);
		mainObserver.HandlerUpstream.Disposable = disposable;
		mainObserver.HandlerNext();
		return mainObserver;
	}
}
