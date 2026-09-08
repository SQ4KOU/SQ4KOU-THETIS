namespace System.Reactive.Linq.ObservableImpl;

internal static class Any<TSource>
{
	internal sealed class Count : Producer<bool, Count._>
	{
		internal sealed class @_ : Sink<TSource, bool>
		{
			public _(IObserver<bool> observer)
				: base(observer)
			{
			}

			public override void OnNext(TSource value)
			{
				ForwardOnNext(value: true);
				ForwardOnCompleted();
			}

			public override void OnCompleted()
			{
				ForwardOnNext(value: false);
				ForwardOnCompleted();
			}
		}

		private readonly IObservable<TSource> _source;

		public Count(IObservable<TSource> source)
		{
			_source = source;
		}

		protected override @_ CreateSink(IObserver<bool> observer)
		{
			return new @_(observer);
		}

		protected override void Run(@_ sink)
		{
			sink.Run(_source);
		}
	}

	internal sealed class Predicate : Producer<bool, Predicate._>
	{
		internal sealed class @_ : Sink<TSource, bool>
		{
			private readonly Func<TSource, bool> _predicate;

			public _(Func<TSource, bool> predicate, IObserver<bool> observer)
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
					ForwardOnNext(value: true);
					ForwardOnCompleted();
				}
			}

			public override void OnCompleted()
			{
				ForwardOnNext(value: false);
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

		protected override @_ CreateSink(IObserver<bool> observer)
		{
			return new @_(_predicate, observer);
		}

		protected override void Run(@_ sink)
		{
			sink.Run(_source);
		}
	}
}
