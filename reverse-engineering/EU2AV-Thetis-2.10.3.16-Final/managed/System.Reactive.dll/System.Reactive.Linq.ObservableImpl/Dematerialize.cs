namespace System.Reactive.Linq.ObservableImpl;

internal sealed class Dematerialize<TSource> : Producer<TSource, Dematerialize<TSource>._>
{
	internal sealed class @_ : Sink<Notification<TSource>, TSource>
	{
		public _(IObserver<TSource> observer)
			: base(observer)
		{
		}

		public override void OnNext(Notification<TSource> value)
		{
			switch (value.Kind)
			{
			case NotificationKind.OnNext:
				ForwardOnNext(value.Value);
				break;
			case NotificationKind.OnError:
				ForwardOnError(value.Exception);
				break;
			case NotificationKind.OnCompleted:
				ForwardOnCompleted();
				break;
			}
		}
	}

	private readonly IObservable<Notification<TSource>> _source;

	public Dematerialize(IObservable<Notification<TSource>> source)
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
