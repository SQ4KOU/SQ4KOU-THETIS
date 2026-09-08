namespace System.Reactive.Linq.ObservableImpl;

internal sealed class MinDecimal : Producer<decimal, MinDecimal._>
{
	internal sealed class @_ : IdentitySink<decimal>
	{
		private bool _hasValue;

		private decimal _lastValue;

		public _(IObserver<decimal> observer)
			: base(observer)
		{
		}

		public override void OnNext(decimal value)
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

	private readonly IObservable<decimal> _source;

	public MinDecimal(IObservable<decimal> source)
	{
		_source = source;
	}

	protected override @_ CreateSink(IObserver<decimal> observer)
	{
		return new @_(observer);
	}

	protected override void Run(@_ sink)
	{
		sink.Run(_source);
	}
}
