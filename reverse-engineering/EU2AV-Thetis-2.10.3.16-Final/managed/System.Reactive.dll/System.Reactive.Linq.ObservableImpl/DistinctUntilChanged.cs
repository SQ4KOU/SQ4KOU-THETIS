using System.Collections.Generic;

namespace System.Reactive.Linq.ObservableImpl;

internal sealed class DistinctUntilChanged<TSource, TKey> : Producer<TSource, DistinctUntilChanged<TSource, TKey>._>
{
	internal sealed class @_ : IdentitySink<TSource>
	{
		private readonly Func<TSource, TKey> _keySelector;

		private readonly IEqualityComparer<TKey> _comparer;

		private TKey? _currentKey;

		private bool _hasCurrentKey;

		public _(DistinctUntilChanged<TSource, TKey> parent, IObserver<TSource> observer)
			: base(observer)
		{
			_keySelector = parent._keySelector;
			_comparer = parent._comparer;
		}

		public override void OnNext(TSource value)
		{
			TKey val;
			try
			{
				val = _keySelector(value);
			}
			catch (Exception error)
			{
				ForwardOnError(error);
				return;
			}
			bool flag = false;
			if (_hasCurrentKey)
			{
				try
				{
					flag = _comparer.Equals(_currentKey, val);
				}
				catch (Exception error2)
				{
					ForwardOnError(error2);
					return;
				}
			}
			if (!_hasCurrentKey || !flag)
			{
				_hasCurrentKey = true;
				_currentKey = val;
				ForwardOnNext(value);
			}
		}
	}

	private readonly IObservable<TSource> _source;

	private readonly Func<TSource, TKey> _keySelector;

	private readonly IEqualityComparer<TKey> _comparer;

	public DistinctUntilChanged(IObservable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
	{
		_source = source;
		_keySelector = keySelector;
		_comparer = comparer;
	}

	protected override @_ CreateSink(IObserver<TSource> observer)
	{
		return new @_(this, observer);
	}

	protected override void Run(@_ sink)
	{
		sink.Run(_source);
	}
}
