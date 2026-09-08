namespace System.Reactive.Linq.ObservableImpl;

internal static class TakeWhile<TSource>
{
	internal sealed class Predicate : Producer<TSource, Predicate._>
	{
		internal sealed class @_ : IdentitySink<TSource>
		{
			private readonly Func<TSource, bool> _predicate;

			private bool _running;

			public _(Func<TSource, bool> predicate, IObserver<TSource> observer)
				: base(observer)
			{
				_predicate = predicate;
				_running = true;
			}

			public override void OnNext(TSource value)
			{
				if (_running)
				{
					try
					{
						_running = _predicate(value);
					}
					catch (Exception error)
					{
						ForwardOnError(error);
						return;
					}
					if (_running)
					{
						ForwardOnNext(value);
					}
					else
					{
						ForwardOnCompleted();
					}
				}
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

	internal sealed class PredicateIndexed : Producer<TSource, PredicateIndexed._>
	{
		internal sealed class @_ : IdentitySink<TSource>
		{
			private readonly Func<TSource, int, bool> _predicate;

			private bool _running;

			private int _index;

			public _(Func<TSource, int, bool> predicate, IObserver<TSource> observer)
				: base(observer)
			{
				_predicate = predicate;
				_running = true;
			}

			public override void OnNext(TSource value)
			{
				if (_running)
				{
					try
					{
						_running = _predicate(value, checked(_index++));
					}
					catch (Exception error)
					{
						ForwardOnError(error);
						return;
					}
					if (_running)
					{
						ForwardOnNext(value);
					}
					else
					{
						ForwardOnCompleted();
					}
				}
			}
		}

		private readonly IObservable<TSource> _source;

		private readonly Func<TSource, int, bool> _predicate;

		public PredicateIndexed(IObservable<TSource> source, Func<TSource, int, bool> predicate)
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
