namespace System.Reactive.Linq.ObservableImpl;

internal sealed class IgnoreElements<TSource> : Producer<TSource, IgnoreElements<TSource>._>
{
	internal sealed class @_ : IdentitySink<TSource>
	{
		public _(IObserver<TSource> observer)
			: base(observer)
		{
		}

		public override void OnNext(TSource value)
		{
		}
	}

	private readonly IObservable<TSource> _source;

	public IgnoreElements(IObservable<TSource> source)
	{
		_source = source;
	}

	protected override @_ CreateSink(IObserver<TSource> observer)
	{
		return new @_(observer);
	}

	protected override void Run(@_ sink)
	{
		sink.Run(_source);
	}
}
