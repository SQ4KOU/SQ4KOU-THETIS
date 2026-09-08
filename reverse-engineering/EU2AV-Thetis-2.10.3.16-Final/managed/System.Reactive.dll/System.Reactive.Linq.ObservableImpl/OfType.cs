namespace System.Reactive.Linq.ObservableImpl;

internal sealed class OfType<TSource, TResult> : Producer<TResult, OfType<TSource, TResult>._>
{
	internal sealed class @_ : Sink<TSource, TResult>
	{
		public _(IObserver<TResult> observer)
			: base(observer)
		{
		}

		public override void OnNext(TSource value)
		{
			if (value is TResult)
			{
				TResult value2 = (TResult)((((object)value) is TResult) ? ((object)value) : null);
				ForwardOnNext(value2);
			}
		}
	}

	private readonly IObservable<TSource> _source;

	public OfType(IObservable<TSource> source)
	{
		_source = source;
	}

	protected override @_ CreateSink(IObserver<TResult> observer)
	{
		return new @_(observer);
	}

	protected override void Run(@_ sink)
	{
		sink.Run(_source);
	}
}
