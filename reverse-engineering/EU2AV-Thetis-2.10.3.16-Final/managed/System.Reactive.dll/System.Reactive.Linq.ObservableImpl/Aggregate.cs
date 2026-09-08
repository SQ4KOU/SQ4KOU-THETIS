namespace System.Reactive.Linq.ObservableImpl;

internal sealed class Aggregate<TSource> : Producer<TSource, Aggregate<TSource>._>
{
	internal sealed class @_ : IdentitySink<TSource>
	{
		private readonly Func<TSource, TSource, TSource> _accumulator;

		private TSource? _accumulation;

		private bool _hasAccumulation;

		public _(Func<TSource, TSource, TSource> accumulator, IObserver<TSource> observer)
			: base(observer)
		{
			_accumulator = accumulator;
		}

		public override void OnNext(TSource value)
		{
			if (!_hasAccumulation)
			{
				_accumulation = value;
				_hasAccumulation = true;
				return;
			}
			try
			{
				_accumulation = _accumulator(_accumulation, value);
			}
			catch (Exception error)
			{
				_accumulation = default(TSource);
				ForwardOnError(error);
			}
		}

		public override void OnError(Exception error)
		{
			_accumulation = default(TSource);
			ForwardOnError(error);
		}

		public override void OnCompleted()
		{
			if (!_hasAccumulation)
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
			TSource accumulation = _accumulation;
			_accumulation = default(TSource);
			ForwardOnNext(accumulation);
			ForwardOnCompleted();
		}
	}

	private readonly IObservable<TSource> _source;

	private readonly Func<TSource, TSource, TSource> _accumulator;

	public Aggregate(IObservable<TSource> source, Func<TSource, TSource, TSource> accumulator)
	{
		_source = source;
		_accumulator = accumulator;
	}

	protected override @_ CreateSink(IObserver<TSource> observer)
	{
		return new @_(_accumulator, observer);
	}

	protected override void Run(@_ sink)
	{
		sink.Run(_source);
	}
}
internal sealed class Aggregate<TSource, TAccumulate> : Producer<TAccumulate, Aggregate<TSource, TAccumulate>._>
{
	internal sealed class @_ : Sink<TSource, TAccumulate>
	{
		private readonly Func<TAccumulate, TSource, TAccumulate> _accumulator;

		private TAccumulate? _accumulation;

		public _(TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> accumulator, IObserver<TAccumulate> observer)
			: base(observer)
		{
			_accumulator = accumulator;
			_accumulation = seed;
		}

		public override void OnNext(TSource value)
		{
			try
			{
				_accumulation = _accumulator(_accumulation, value);
			}
			catch (Exception error)
			{
				_accumulation = default(TAccumulate);
				ForwardOnError(error);
			}
		}

		public override void OnError(Exception error)
		{
			_accumulation = default(TAccumulate);
			ForwardOnError(error);
		}

		public override void OnCompleted()
		{
			TAccumulate accumulation = _accumulation;
			_accumulation = default(TAccumulate);
			ForwardOnNext(accumulation);
			ForwardOnCompleted();
		}
	}

	private readonly IObservable<TSource> _source;

	private readonly TAccumulate _seed;

	private readonly Func<TAccumulate, TSource, TAccumulate> _accumulator;

	public Aggregate(IObservable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> accumulator)
	{
		_source = source;
		_seed = seed;
		_accumulator = accumulator;
	}

	protected override @_ CreateSink(IObserver<TAccumulate> observer)
	{
		return new @_(_seed, _accumulator, observer);
	}

	protected override void Run(@_ sink)
	{
		sink.Run(_source);
	}
}
internal sealed class Aggregate<TSource, TAccumulate, TResult> : Producer<TResult, Aggregate<TSource, TAccumulate, TResult>._>
{
	internal sealed class @_ : Sink<TSource, TResult>
	{
		private readonly Func<TAccumulate, TSource, TAccumulate> _accumulator;

		private readonly Func<TAccumulate, TResult> _resultSelector;

		private TAccumulate? _accumulation;

		public _(Aggregate<TSource, TAccumulate, TResult> parent, IObserver<TResult> observer)
			: base(observer)
		{
			_accumulator = parent._accumulator;
			_resultSelector = parent._resultSelector;
			_accumulation = parent._seed;
		}

		public override void OnNext(TSource value)
		{
			try
			{
				_accumulation = _accumulator(_accumulation, value);
			}
			catch (Exception error)
			{
				_accumulation = default(TAccumulate);
				ForwardOnError(error);
			}
		}

		public override void OnError(Exception error)
		{
			_accumulation = default(TAccumulate);
			ForwardOnError(error);
		}

		public override void OnCompleted()
		{
			TAccumulate accumulation = _accumulation;
			_accumulation = default(TAccumulate);
			TResult value;
			try
			{
				value = _resultSelector(accumulation);
			}
			catch (Exception error)
			{
				ForwardOnError(error);
				return;
			}
			ForwardOnNext(value);
			ForwardOnCompleted();
		}
	}

	private readonly IObservable<TSource> _source;

	private readonly TAccumulate _seed;

	private readonly Func<TAccumulate, TSource, TAccumulate> _accumulator;

	private readonly Func<TAccumulate, TResult> _resultSelector;

	public Aggregate(IObservable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> accumulator, Func<TAccumulate, TResult> resultSelector)
	{
		_source = source;
		_seed = seed;
		_accumulator = accumulator;
		_resultSelector = resultSelector;
	}

	protected override @_ CreateSink(IObserver<TResult> observer)
	{
		return new @_(this, observer);
	}

	protected override void Run(@_ sink)
	{
		sink.Run(_source);
	}
}
