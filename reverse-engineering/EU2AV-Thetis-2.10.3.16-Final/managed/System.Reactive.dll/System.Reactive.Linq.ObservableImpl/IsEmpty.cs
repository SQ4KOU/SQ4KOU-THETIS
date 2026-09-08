namespace System.Reactive.Linq.ObservableImpl;

internal sealed class IsEmpty<TSource> : Producer<bool, IsEmpty<TSource>._>
{
	internal sealed class @_ : Sink<TSource, bool>
	{
		public _(IObserver<bool> observer)
			: base(observer)
		{
		}

		public override void OnNext(TSource value)
		{
			ForwardOnNext(value: false);
			ForwardOnCompleted();
		}

		public override void OnCompleted()
		{
			ForwardOnNext(value: true);
			ForwardOnCompleted();
		}
	}

	private readonly IObservable<TSource> _source;

	public IsEmpty(IObservable<TSource> source)
	{
		_source = source;
	}

	protected override @_ CreateSink(IObserver<bool> observer)
	{
		return new @_(observer);
	}

	protected override void Run(@_ sink)
	{
		sink.Run(_source);
	}
}
