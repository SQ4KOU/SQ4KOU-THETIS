using System.Reactive.Disposables;
using System.Threading;

namespace System.Reactive;

internal abstract class Sink<TTarget> : ISink<TTarget>, IDisposable
{
	private SingleAssignmentDisposableValue _upstream;

	private volatile IObserver<TTarget> _observer;

	protected Sink(IObserver<TTarget> observer)
	{
		_observer = observer;
	}

	public void Dispose()
	{
		if (Interlocked.Exchange(ref _observer, NopObserver<TTarget>.Instance) != NopObserver<TTarget>.Instance)
		{
			Dispose(disposing: true);
		}
	}

	protected virtual void Dispose(bool disposing)
	{
		_upstream.Dispose();
	}

	public void ForwardOnNext(TTarget value)
	{
		_observer.OnNext(value);
	}

	public void ForwardOnCompleted()
	{
		_observer.OnCompleted();
		Dispose();
	}

	public void ForwardOnError(Exception error)
	{
		_observer.OnError(error);
		Dispose();
	}

	protected void SetUpstream(IDisposable upstream)
	{
		_upstream.Disposable = upstream;
	}

	protected void DisposeUpstream()
	{
		_upstream.Dispose();
	}
}
internal abstract class Sink<TSource, TTarget> : Sink<TTarget>, IObserver<TSource>
{
	private sealed class @_ : IObserver<TTarget>
	{
		private readonly Sink<TSource, TTarget> _forward;

		public _(Sink<TSource, TTarget> forward)
		{
			_forward = forward;
		}

		public void OnNext(TTarget value)
		{
			_forward.ForwardOnNext(value);
		}

		public void OnError(Exception error)
		{
			_forward.ForwardOnError(error);
		}

		public void OnCompleted()
		{
			_forward.ForwardOnCompleted();
		}
	}

	protected Sink(IObserver<TTarget> observer)
		: base(observer)
	{
	}

	public virtual void Run(IObservable<TSource> source)
	{
		SetUpstream(source.SubscribeSafe(this));
	}

	public abstract void OnNext(TSource value);

	public virtual void OnError(Exception error)
	{
		ForwardOnError(error);
	}

	public virtual void OnCompleted()
	{
		ForwardOnCompleted();
	}

	public IObserver<TTarget> GetForwarder()
	{
		return new @_(this);
	}
}
