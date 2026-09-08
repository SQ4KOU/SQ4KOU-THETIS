namespace System.Reactive.Linq.ObservableImpl;

internal sealed class Defer<TValue> : Producer<TValue, Defer<TValue>._>, IEvaluatableObservable<TValue>
{
	internal sealed class @_ : IdentitySink<TValue>
	{
		private readonly Func<IObservable<TValue>> _observableFactory;

		public _(Func<IObservable<TValue>> observableFactory, IObserver<TValue> observer)
			: base(observer)
		{
			_observableFactory = observableFactory;
		}

		public void Run()
		{
			IObservable<TValue> source;
			try
			{
				source = _observableFactory();
			}
			catch (Exception error)
			{
				ForwardOnError(error);
				return;
			}
			Run(source);
		}
	}

	private readonly Func<IObservable<TValue>> _observableFactory;

	public Defer(Func<IObservable<TValue>> observableFactory)
	{
		_observableFactory = observableFactory;
	}

	protected override @_ CreateSink(IObserver<TValue> observer)
	{
		return new @_(_observableFactory, observer);
	}

	protected override void Run(@_ sink)
	{
		sink.Run();
	}

	public IObservable<TValue> Eval()
	{
		return _observableFactory();
	}
}
