namespace System.Reactive.Linq.ObservableImpl;

internal sealed class AsObservable<TSource> : Producer<TSource, AsObservable<TSource>._>, IEvaluatableObservable<TSource>
{
	internal sealed class @_ : IdentitySink<TSource>
	{
		public _(IObserver<TSource> observer)
			: base(observer)
		{
		}
	}

	private readonly IObservable<TSource> _source;

	public AsObservable(IObservable<TSource> source)
	{
		_source = source;
	}

	public IObservable<TSource> Eval()
	{
		return _source;
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
