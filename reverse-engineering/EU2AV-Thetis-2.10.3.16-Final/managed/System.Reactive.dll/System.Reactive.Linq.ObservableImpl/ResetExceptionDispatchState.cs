namespace System.Reactive.Linq.ObservableImpl;

internal class ResetExceptionDispatchState<TSource> : Producer<TSource, ResetExceptionDispatchState<TSource>._>
{
	internal sealed class @_ : IdentitySink<TSource>
	{
		public _(IObserver<TSource> observer)
			: base(observer)
		{
		}

		public override void OnError(Exception error)
		{
			try
			{
				throw error;
			}
			catch
			{
			}
			base.OnError(error);
		}
	}

	private readonly IObservable<TSource> _source;

	public ResetExceptionDispatchState(IObservable<TSource> source)
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
