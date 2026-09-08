using System.Reactive.Disposables;

namespace System.Reactive.Linq.ObservableImpl;

internal class AddRef<TSource> : Producer<TSource, AddRef<TSource>._>
{
	internal sealed class @_ : IdentitySink<TSource>
	{
		private readonly IDisposable _refCountDisposable;

		public _(IObserver<TSource> observer, IDisposable refCountDisposable)
			: base(observer)
		{
			_refCountDisposable = refCountDisposable;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_refCountDisposable.Dispose();
			}
			base.Dispose(disposing);
		}
	}

	private readonly IObservable<TSource> _source;

	private readonly RefCountDisposable _refCount;

	public AddRef(IObservable<TSource> source, RefCountDisposable refCount)
	{
		_source = source;
		_refCount = refCount;
	}

	protected override @_ CreateSink(IObserver<TSource> observer)
	{
		return new @_(observer, _refCount.GetDisposable());
	}

	protected override void Run(@_ sink)
	{
		sink.Run(_source);
	}
}
