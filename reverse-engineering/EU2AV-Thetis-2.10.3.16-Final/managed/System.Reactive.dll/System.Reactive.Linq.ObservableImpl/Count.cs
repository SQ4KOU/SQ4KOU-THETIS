namespace System.Reactive.Linq.ObservableImpl;

internal static class Count<TSource>
{
	internal sealed class All : Producer<int, All._>
	{
		internal sealed class @_ : Sink<TSource, int>
		{
			private int _count;

			public _(IObserver<int> observer)
				: base(observer)
			{
			}

			public override void OnNext(TSource value)
			{
				checked
				{
					try
					{
						_count++;
					}
					catch (Exception error)
					{
						ForwardOnError(error);
					}
				}
			}

			public override void OnCompleted()
			{
				ForwardOnNext(_count);
				ForwardOnCompleted();
			}
		}

		private readonly IObservable<TSource> _source;

		public All(IObservable<TSource> source)
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

	internal sealed class Predicate : Producer<int, Predicate._>
	{
		internal sealed class @_ : Sink<TSource, int>
		{
			private readonly Func<TSource, bool> _predicate;

			private int _count;

			public _(Func<TSource, bool> predicate, IObserver<int> observer)
				: base(observer)
			{
				_predicate = predicate;
			}

			public override void OnNext(TSource value)
			{
				checked
				{
					try
					{
						if (_predicate(value))
						{
							_count++;
						}
					}
					catch (Exception error)
					{
						ForwardOnError(error);
					}
				}
			}

			public override void OnCompleted()
			{
				ForwardOnNext(_count);
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

		protected override @_ CreateSink(IObserver<int> observer)
		{
			return new @_(_predicate, observer);
		}

		protected override void Run(@_ sink)
		{
			sink.Run(_source);
		}
	}
}
