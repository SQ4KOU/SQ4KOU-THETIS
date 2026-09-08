using System.Collections.Generic;

namespace System.Reactive.Linq.ObservableImpl;

internal sealed class Contains<TSource> : Producer<bool, Contains<TSource>._>
{
	internal sealed class @_ : Sink<TSource, bool>
	{
		private readonly TSource _value;

		private readonly IEqualityComparer<TSource> _comparer;

		public _(Contains<TSource> parent, IObserver<bool> observer)
			: base(observer)
		{
			_value = parent._value;
			_comparer = parent._comparer;
		}

		public override void OnNext(TSource value)
		{
			bool flag;
			try
			{
				flag = _comparer.Equals(value, _value);
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

	private readonly TSource _value;

	private readonly IEqualityComparer<TSource> _comparer;

	public Contains(IObservable<TSource> source, TSource value, IEqualityComparer<TSource> comparer)
	{
		_source = source;
		_value = value;
		_comparer = comparer;
	}

	protected override @_ CreateSink(IObserver<bool> observer)
	{
		return new @_(this, observer);
	}

	protected override void Run(@_ sink)
	{
		sink.Run(_source);
	}
}
