namespace System.Reactive.Linq.ObservableImpl;

internal static class Select<TSource, TResult>
{
	internal sealed class Selector : Producer<TResult, Selector._>
	{
		internal sealed class @_ : Sink<TSource, TResult>
		{
			private readonly Func<TSource, TResult> _selector;

			public _(Func<TSource, TResult> selector, IObserver<TResult> observer)
				: base(observer)
			{
				_selector = selector;
			}

			public override void OnNext(TSource value)
			{
				TResult value2;
				try
				{
					value2 = _selector(value);
				}
				catch (Exception error)
				{
					ForwardOnError(error);
					return;
				}
				ForwardOnNext(value2);
			}
		}

		private readonly IObservable<TSource> _source;

		private readonly Func<TSource, TResult> _selector;

		public Selector(IObservable<TSource> source, Func<TSource, TResult> selector)
		{
			_source = source;
			_selector = selector;
		}

		protected override @_ CreateSink(IObserver<TResult> observer)
		{
			return new @_(_selector, observer);
		}

		protected override void Run(@_ sink)
		{
			sink.Run(_source);
		}
	}

	internal sealed class SelectorIndexed : Producer<TResult, SelectorIndexed._>
	{
		internal sealed class @_ : Sink<TSource, TResult>
		{
			private readonly Func<TSource, int, TResult> _selector;

			private int _index;

			public _(Func<TSource, int, TResult> selector, IObserver<TResult> observer)
				: base(observer)
			{
				_selector = selector;
			}

			public override void OnNext(TSource value)
			{
				TResult value2;
				try
				{
					value2 = _selector(value, checked(_index++));
				}
				catch (Exception error)
				{
					ForwardOnError(error);
					return;
				}
				ForwardOnNext(value2);
			}
		}

		private readonly IObservable<TSource> _source;

		private readonly Func<TSource, int, TResult> _selector;

		public SelectorIndexed(IObservable<TSource> source, Func<TSource, int, TResult> selector)
		{
			_source = source;
			_selector = selector;
		}

		protected override @_ CreateSink(IObserver<TResult> observer)
		{
			return new @_(_selector, observer);
		}

		protected override void Run(@_ sink)
		{
			sink.Run(_source);
		}
	}
}
