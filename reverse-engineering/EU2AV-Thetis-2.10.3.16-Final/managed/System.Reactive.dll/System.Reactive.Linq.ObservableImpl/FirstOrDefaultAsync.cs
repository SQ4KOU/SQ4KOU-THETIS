namespace System.Reactive.Linq.ObservableImpl;

internal static class FirstOrDefaultAsync<TSource>
{
	internal sealed class Sequence : Producer<TSource?, Sequence._>
	{
		internal sealed class @_ : Sink<TSource, TSource?>
		{
			public _(IObserver<TSource?> observer)
				: base(observer)
			{
			}

			public override void OnNext(TSource value)
			{
				ForwardOnNext(value);
				ForwardOnCompleted();
			}

			public override void OnCompleted()
			{
				ForwardOnNext(default(TSource));
				ForwardOnCompleted();
			}
		}

		private readonly IObservable<TSource> _source;

		public Sequence(IObservable<TSource> source)
		{
			_source = source;
		}

		protected override @_ CreateSink(IObserver<TSource?> observer)
		{
			return new @_(observer);
		}

		protected override void Run(@_ sink)
		{
			sink.Run(_source);
		}
	}

	internal sealed class Predicate : Producer<TSource?, Predicate._>
	{
		internal sealed class @_ : Sink<TSource, TSource?>
		{
			private readonly Func<TSource, bool> _predicate;

			public _(Func<TSource, bool> predicate, IObserver<TSource?> observer)
				: base(observer)
			{
				_predicate = predicate;
			}

			public override void OnNext(TSource value)
			{
				bool flag;
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
					ForwardOnCompleted();
				}
			}

			public override void OnCompleted()
			{
				ForwardOnNext(default(TSource));
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

		protected override @_ CreateSink(IObserver<TSource?> observer)
		{
			return new @_(_predicate, observer);
		}

		protected override void Run(@_ sink)
		{
			sink.Run(_source);
		}
	}
}
