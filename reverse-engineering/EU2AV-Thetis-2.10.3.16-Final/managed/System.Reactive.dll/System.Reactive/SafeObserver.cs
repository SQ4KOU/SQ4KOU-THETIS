using System.Reactive.Disposables;

namespace System.Reactive;

internal abstract class SafeObserver<TSource> : ISafeObserver<TSource>, IObserver<TSource>, IDisposable
{
	private sealed class WrappingSafeObserver : SafeObserver<TSource>
	{
		private readonly IObserver<TSource> _observer;

		public WrappingSafeObserver(IObserver<TSource> observer)
		{
			_observer = observer;
		}

		public override void OnNext(TSource value)
		{
			bool flag = false;
			try
			{
				_observer.OnNext(value);
				flag = true;
			}
			finally
			{
				if (!flag)
				{
					Dispose();
				}
			}
		}

		public override void OnError(Exception error)
		{
			using (this)
			{
				_observer.OnError(error);
			}
		}

		public override void OnCompleted()
		{
			using (this)
			{
				_observer.OnCompleted();
			}
		}
	}

	private SingleAssignmentDisposableValue _disposable;

	public static ISafeObserver<TSource> Wrap(IObserver<TSource> observer)
	{
		if (observer is AnonymousObserver<TSource> anonymousObserver)
		{
			return anonymousObserver.MakeSafe();
		}
		return new WrappingSafeObserver(observer);
	}

	public abstract void OnNext(TSource value);

	public abstract void OnError(Exception error);

	public abstract void OnCompleted();

	public void SetResource(IDisposable resource)
	{
		_disposable.Disposable = resource;
	}

	public void Dispose()
	{
		Dispose(disposing: true);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			_disposable.Dispose();
		}
	}
}
