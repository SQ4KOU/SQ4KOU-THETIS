namespace System.Reactive.Linq.ObservableImpl;

internal sealed class MinDouble : Producer<double, MinDouble._>
{
	internal sealed class @_ : IdentitySink<double>
	{
		private bool _hasValue;

		private double _lastValue;

		public _(IObserver<double> observer)
			: base(observer)
		{
		}

		public override void OnNext(double value)
		{
			if (_hasValue)
			{
				if (value < _lastValue || double.IsNaN(value))
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

	private readonly IObservable<double> _source;

	public MinDouble(IObservable<double> source)
	{
		_source = source;
	}

	protected override @_ CreateSink(IObserver<double> observer)
	{
		return new @_(observer);
	}

	protected override void Run(@_ sink)
	{
		sink.Run(_source);
	}
}
