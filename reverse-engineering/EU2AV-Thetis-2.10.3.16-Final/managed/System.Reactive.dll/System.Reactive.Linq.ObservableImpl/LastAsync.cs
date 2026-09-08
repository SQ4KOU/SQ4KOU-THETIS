namespace System.Reactive.Linq.ObservableImpl;

internal static class LastAsync<TSource>
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
				_value = value;
				_seenValue = true;
			}

			public override void OnError(Exception error)
			{
				_value = default(TSource);
				ForwardOnError(error);
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
				TSource value = _value;
				_value = default(TSource);
				ForwardOnNext(value);
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
					_value = default(TSource);
					ForwardOnError(error);
					return;
				}
				if (flag)
				{
					_value = value;
					_seenValue = true;
				}
			}

			public override void OnError(Exception error)
			{
				_value = default(TSource);
				ForwardOnError(error);
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
				TSource value = _value;
				_value = default(TSource);
				ForwardOnNext(value);
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
