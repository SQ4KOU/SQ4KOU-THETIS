using System.Reactive.Disposables;

namespace System.Reactive.Linq.ObservableImpl;

internal sealed class Using<TSource, TResource> : Producer<TSource, Using<TSource, TResource>._> where TResource : IDisposable
{
	internal sealed class @_ : IdentitySink<TSource>
	{
		private SingleAssignmentDisposableValue _disposable;

		public _(IObserver<TSource> observer)
			: base(observer)
		{
		}

		public void Run(Using<TSource, TResource> parent)
		{
			IDisposable disposable = Disposable.Empty;
			IObservable<TSource> source;
			try
			{
				TResource val = parent._resourceFactory();
				if (val != null)
				{
					disposable = val;
				}
				source = parent._observableFactory(val);
			}
			catch (Exception exception)
			{
				source = Observable.Throw<TSource>(exception);
			}
			Run(source);
			_disposable.Disposable = disposable;
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (disposing)
			{
				_disposable.Dispose();
			}
		}
	}

	private readonly Func<TResource> _resourceFactory;

	private readonly Func<TResource, IObservable<TSource>> _observableFactory;

	public Using(Func<TResource> resourceFactory, Func<TResource, IObservable<TSource>> observableFactory)
	{
		_resourceFactory = resourceFactory;
		_observableFactory = observableFactory;
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
