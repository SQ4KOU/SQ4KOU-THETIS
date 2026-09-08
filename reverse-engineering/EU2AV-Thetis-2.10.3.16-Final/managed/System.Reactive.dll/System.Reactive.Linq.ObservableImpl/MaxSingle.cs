namespace System.Reactive.Linq.ObservableImpl;

internal sealed class MaxSingle : Producer<float, MaxSingle._>
{
	internal sealed class @_ : IdentitySink<float>
	{
		private bool _hasValue;

		private float _lastValue;

		public _(IObserver<float> observer)
			: base(observer)
		{
		}

		public override void OnNext(float value)
		{
			if (_hasValue)
			{
				if (value > _lastValue || float.IsNaN(value))
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

	private readonly IObservable<float> _source;

	public MaxSingle(IObservable<float> source)
	{
		_source = source;
	}

	protected override @_ CreateSink(IObserver<float> observer)
	{
		return new @_(observer);
	}

	protected override void Run(@_ sink)
	{
		sink.Run(_source);
	}
}
