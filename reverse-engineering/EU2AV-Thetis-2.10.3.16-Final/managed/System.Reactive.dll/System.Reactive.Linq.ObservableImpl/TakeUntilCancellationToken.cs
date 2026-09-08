using System.Reactive.Disposables;
using System.Threading;

namespace System.Reactive.Linq.ObservableImpl;

internal sealed class TakeUntilCancellationToken<TSource> : Producer<TSource, TakeUntilCancellationToken<TSource>._>
{
	internal sealed class @_ : IdentitySink<TSource>
	{
		private SingleAssignmentDisposableValue _cancellationTokenRegistration;

		private int _wip;

		private Exception? _error;

		public _(IObserver<TSource> observer)
			: base(observer)
		{
		}

		public void Run(TakeUntilCancellationToken<TSource> parent)
		{
			if (parent._token.IsCancellationRequested)
			{
				OnCompleted();
				return;
			}
			_cancellationTokenRegistration.Disposable = parent._token.Register(OnCompleted);
			Run(parent._source);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_cancellationTokenRegistration.Dispose();
			}
			base.Dispose(disposing);
		}

		public override void OnNext(TSource value)
		{
			HalfSerializer.ForwardOnNext(this, value, ref _wip, ref _error);
		}

		public override void OnError(Exception error)
		{
			HalfSerializer.ForwardOnError(this, error, ref _wip, ref _error);
		}

		public override void OnCompleted()
		{
			HalfSerializer.ForwardOnCompleted(this, ref _wip, ref _error);
		}
	}

	private readonly IObservable<TSource> _source;

	private readonly CancellationToken _token;

	public TakeUntilCancellationToken(IObservable<TSource> source, CancellationToken token)
	{
		_source = source;
		_token = token;
	}

	protected override @_ CreateSink(IObserver<TSource> observer)
	{
		return new @_(observer);
	}

	protected override void Run(@_ sink)
	{
		sink.Run(this);
	}
}
