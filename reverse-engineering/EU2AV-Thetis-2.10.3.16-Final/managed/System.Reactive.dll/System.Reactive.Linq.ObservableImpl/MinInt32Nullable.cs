namespace System.Reactive.Linq.ObservableImpl;

internal sealed class MinInt32Nullable : Producer<int?, MinInt32Nullable._>
{
	internal sealed class @_ : IdentitySink<int?>
	{
		private int? _lastValue;

		public _(IObserver<int?> observer)
			: base(observer)
		{
		}

		public override void OnNext(int? value)
		{
			if (!value.HasValue)
			{
				return;
			}
			if (_lastValue.HasValue)
			{
				if (value < _lastValue)
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

	private readonly IObservable<int?> _source;

	public MinInt32Nullable(IObservable<int?> source)
	{
		_source = source;
	}

	protected override @_ CreateSink(IObserver<int?> observer)
	{
		return new @_(observer);
	}

	protected override void Run(@_ sink)
	{
		sink.Run(_source);
	}
}
