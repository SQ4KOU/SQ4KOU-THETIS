using System.Collections.Generic;

namespace System.Reactive.Linq.ObservableImpl;

internal sealed class Min<TSource> : Producer<TSource, Min<TSource>._>
{
	internal abstract class @_ : IdentitySink<TSource>
	{
		protected readonly IComparer<TSource> _comparer;

		protected _(IComparer<TSource> comparer, IObserver<TSource> observer)
			: base(observer)
		{
			_comparer = comparer;
		}
	}

	private sealed class NonNull : @_
	{
		private bool _hasValue;

		private TSource? _lastValue;

		public NonNull(IComparer<TSource> comparer, IObserver<TSource> observer)
			: base(comparer, observer)
		{
		}

		public override void OnNext(TSource value)
		{
			if (_hasValue)
			{
				int num;
				try
				{
					num = _comparer.Compare(value, _lastValue);
				}
				catch (Exception error)
				{
					ForwardOnError(error);
					return;
				}
				if (num < 0)
				{
					_lastValue = value;
				}
			}
			else
			{
				_hasValue = true;
				_lastValue = value;
			}
		}

		public override void OnError(Exception error)
		{
			ForwardOnError(error);
		}

		public override void OnCompleted()
		{
			if (!_hasValue)
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
			ForwardOnNext(_lastValue);
			ForwardOnCompleted();
		}
	}

	private sealed class Null : @_
	{
		private TSource? _lastValue;

		public Null(IComparer<TSource> comparer, IObserver<TSource> observer)
			: base(comparer, observer)
		{
		}

		public override void OnNext(TSource value)
		{
			if (value == null)
			{
				return;
			}
			if (_lastValue == null)
			{
				_lastValue = value;
				return;
			}
			int num;
			try
			{
				num = _comparer.Compare(value, _lastValue);
			}
			catch (Exception error)
			{
				ForwardOnError(error);
				return;
			}
			if (num < 0)
			{
				_lastValue = value;
			}
		}

		public override void OnCompleted()
		{
			ForwardOnNext(_lastValue);
			ForwardOnCompleted();
		}
	}

	private readonly IObservable<TSource> _source;

	private readonly IComparer<TSource> _comparer;

	public Min(IObservable<TSource> source, IComparer<TSource> comparer)
	{
		_source = source;
		_comparer = comparer;
	}

	protected override @_ CreateSink(IObserver<TSource> observer)
	{
		if (default(TSource) != null)
		{
			return new NonNull(_comparer, observer);
		}
		return new Null(_comparer, observer);
	}

	protected override void Run(@_ sink)
	{
		sink.Run(_source);
	}
}
