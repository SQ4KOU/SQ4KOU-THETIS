namespace System.Reactive.Linq.ObservableImpl;

internal sealed class Cast<TSource, TResult> : Producer<TResult, Cast<TSource, TResult>._>
{
	internal sealed class @_ : Sink<TSource, TResult>
	{
		public _(IObserver<TResult> observer)
			: base(observer)
		{
		}

		public override void OnNext(TSource value)
		{
			TResult value2;
			try
			{
				value2 = (TResult)(object)value;
			}
			catch (Exception error)
			{
				ForwardOnError(error);
				return;
			}
			ForwardOnNext(value2);
		}
	}

	private readonly IObservable<TSource> _source;

	public Cast(IObservable<TSource> source)
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
