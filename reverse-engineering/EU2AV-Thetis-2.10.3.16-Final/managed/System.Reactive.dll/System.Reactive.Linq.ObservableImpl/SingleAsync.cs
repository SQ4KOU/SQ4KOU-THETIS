namespace System.Reactive.Linq.ObservableImpl;

internal static class SingleAsync<TSource>
{
	internal sealed class Sequence : Producer<TSource, Sequence._>
	{
		internal sealed class @_ : IdentitySink<TSource>
		{
			private TSource? _value;

			private bool _seenValue;

			public _(IObserver<TSource> observer)
				: base(observer)
			{
			}

			public override void OnNext(TSource value)
			{
				if (_seenValue)
				{
					try
					{
						throw new InvalidOperationException(Strings_Linq.MORE_THAN_ONE_ELEMENT);
					}
					catch (Exception error)
					{
						ForwardOnError(error);
						return;
					}
				}
				_value = value;
				_seenValue = true;
			}

			public override void OnCompleted()
			{
				if (!_seenValue)
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
				ForwardOnNext(_value);
				ForwardOnCompleted();
			}
		}

		private readonly IObservable<TSource> _source;

		public Sequence(IObservable<TSource> source)
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

	internal sealed class Predicate : Producer<TSource, Predicate._>
	{
		internal sealed class @_ : IdentitySink<TSource>
		{
			private readonly Func<TSource, bool> _predicate;

			private TSource? _value;

			private bool _seenValue;

			public _(Func<TSource, bool> predicate, IObserver<TSource> observer)
				: base(observer)
			{
				_predicate = predicate;
			}

			public override void OnNext(TSource value)
			{
				bool flag = false;
				try
				{
					flag = _predicate(value);
				}
				catch (Exception error)
				{
					ForwardOnError(error);
					return;
				}
				if (!flag)
				{
					return;
				}
				if (_seenValue)
				{
					try
					{
						throw new InvalidOperationException(Strings_Linq.MORE_THAN_ONE_MATCHING_ELEMENT);
					}
					catch (Exception error2)
					{
						ForwardOnError(error2);
						return;
					}
				}
				_value = value;
				_seenValue = true;
			}

			public override void OnCompleted()
			{
				if (!_seenValue)
				{
					try
					{
						throw new InvalidOperationException(Strings_Linq.NO_MATCHING_ELEMENTS);
					}
					catch (Exception error)
					{
						ForwardOnError(error);
						return;
					}
				}
				ForwardOnNext(_value);
				ForwardOnCompleted();
			}
		}

		private readonly IObservable<TSource> _source;

		private readonly Func<TSource, bool> _predicate;

		public Predicate(IObservable<TSource> source, Func<TSource, bool> predicate)
		{
			_source = source;
			_predicate = predicate;
		}

		protected override @_ CreateSink(IObserver<TSource> observer)
		{
			return new @_(_predicate, observer);
		}

		protected override void Run(@_ sink)
		{
			sink.Run(_source);
		}
	}
}
