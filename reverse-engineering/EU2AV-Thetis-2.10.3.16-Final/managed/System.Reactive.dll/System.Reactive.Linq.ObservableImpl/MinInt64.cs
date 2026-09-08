namespace System.Reactive.Linq.ObservableImpl;

internal sealed class MinInt64 : Producer<long, MinInt64._>
{
	internal sealed class @_ : IdentitySink<long>
	{
		private bool _hasValue;

		private long _lastValue;

		public _(IObserver<long> observer)
			: base(observer)
		{
		}

		public override void OnNext(long value)
		{
			if (_hasValue)
			{
				if (value < _lastValue)
				{
					_lastValue = value;
				}
			}
			else
			{
				_lastValue = value;
				_hasValue = true;
			}
		}

		public override void OnCompleted()
		{
			if (!_hasValue)
			{
				try
				{
					throw new InvalidOperationException(Strings_Linq.NO_ELEMENTS);
				}
				catch (Exception error)
				{
					ForwardOnError(error);
					return;
				}
			}
			ForwardOnNext(_lastValue);
			ForwardOnCompleted();
		}
	}

	private readonly IObservable<long> _source;

	public MinInt64(IObservable<long> source)
	{
		_source = source;
	}

	protected override @_ CreateSink(IObserver<long> observer)
	{
		return new @_(observer);
	}

	protected override void Run(@_ sink)
	{
		sink.Run(_source);
	}
}
