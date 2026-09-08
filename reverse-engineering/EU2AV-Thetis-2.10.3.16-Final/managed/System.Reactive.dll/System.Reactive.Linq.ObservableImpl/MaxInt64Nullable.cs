namespace System.Reactive.Linq.ObservableImpl;

internal sealed class MaxInt64Nullable : Producer<long?, MaxInt64Nullable._>
{
	internal sealed class @_ : IdentitySink<long?>
	{
		private long? _lastValue;

		public _(IObserver<long?> observer)
			: base(observer)
		{
		}

		public override void OnNext(long? value)
		{
			if (!value.HasValue)
			{
				return;
			}
			if (_lastValue.HasValue)
			{
				if (value > _lastValue)
				{
					_lastValue = value;
				}
			}
			else
			{
				_lastValue = value;
			}
		}

		public override void OnCompleted()
		{
			ForwardOnNext(_lastValue);
			ForwardOnCompleted();
		}
	}

	private readonly IObservable<long?> _source;

	public MaxInt64Nullable(IObservable<long?> source)
	{
		_source = source;
	}

	protected override @_ CreateSink(IObserver<long?> observer)
	{
		return new @_(observer);
	}

	protected override void Run(@_ sink)
	{
		sink.Run(_source);
	}
}
