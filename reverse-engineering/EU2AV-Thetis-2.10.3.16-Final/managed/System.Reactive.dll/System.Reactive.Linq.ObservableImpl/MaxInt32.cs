namespace System.Reactive.Linq.ObservableImpl;

internal sealed class MaxInt32 : Producer<int, MaxInt32._>
{
	internal sealed class @_ : IdentitySink<int>
	{
		private bool _hasValue;

		private int _lastValue;

		public _(IObserver<int> observer)
			: base(observer)
		{
		}

		public override void OnNext(int value)
		{
			if (_hasValue)
			{
				if (value > _lastValue)
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

	private readonly IObservable<int> _source;

	public MaxInt32(IObservable<int> source)
	{
		_source = source;
	}

	protected override @_ CreateSink(IObserver<int> observer)
	{
		return new @_(observer);
	}

	protected override void Run(@_ sink)
	{
		sink.Run(_source);
	}
}
