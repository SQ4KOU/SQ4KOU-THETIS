namespace System.Reactive.Linq.ObservableImpl;

internal static class Where<TSource>
{
	internal sealed class Predicate : Producer<TSource, Predicate._>
	{
		internal sealed class @_ : IdentitySink<TSource>
		{
			private readonly Func<TSource, bool> _predicate;

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
				if (flag)
				{
					ForwardOnNext(value);
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

		public IObservable<TSource> Combine(Func<TSource, bool> predicate)
		{
			return new Predicate(_source, (TSource x) => _predicate(x) && predicate(x));
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

			private int _index;

			public _(Func<TSource, int, bool> predicate, IObserver<TSource> observer)
				: base(observer)
			{
				_predicate = predicate;
			}

			public override void OnNext(TSource value)
			{
				bool flag = false;
				try
				{
					flag = _predicate(value, checked(_index++));
				}
				catch (Exception error)
				{
					ForwardOnError(error);
				}
				if (flag)
				{
					ForwardOnNext(value);
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
