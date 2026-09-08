using System.Collections.Generic;

namespace System.Reactive.Linq.ObservableImpl;

internal sealed class MaxBy<TSource, TKey> : Producer<IList<TSource>, MaxBy<TSource, TKey>._>
{
	internal sealed class @_ : Sink<TSource, IList<TSource>>
	{
		private readonly Func<TSource, TKey> _keySelector;

		private readonly IComparer<TKey> _comparer;

		private bool _hasValue;

		private TKey? _lastKey;

		private List<TSource> _list = new List<TSource>();

		public _(MaxBy<TSource, TKey> parent, IObserver<IList<TSource>> observer)
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
				Cleanup();
				ForwardOnError(error);
				return;
			}
			int num = 0;
			if (!_hasValue)
			{
				_hasValue = true;
				_lastKey = val;
			}
			else
			{
				try
				{
					num = _comparer.Compare(val, _lastKey);
				}
				catch (Exception error2)
				{
					Cleanup();
					ForwardOnError(error2);
					return;
				}
			}
			if (num > 0)
			{
				_lastKey = val;
				_list.Clear();
			}
			if (num >= 0)
			{
				_list.Add(value);
			}
		}

		public override void OnError(Exception error)
		{
			Cleanup();
			base.OnError(error);
		}

		public override void OnCompleted()
		{
			List<TSource> list = _list;
			Cleanup();
			ForwardOnNext(list);
			ForwardOnCompleted();
		}

		private void Cleanup()
		{
			_list = null;
			_lastKey = default(TKey);
		}
	}

	private readonly IObservable<TSource> _source;

	private readonly Func<TSource, TKey> _keySelector;

	private readonly IComparer<TKey> _comparer;

	public MaxBy(IObservable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey> comparer)
	{
		_source = source;
		_keySelector = keySelector;
		_comparer = comparer;
	}

	protected override @_ CreateSink(IObserver<IList<TSource>> observer)
	{
		return new @_(this, observer);
	}

	protected override void Run(@_ sink)
	{
		sink.Run(_source);
	}
}
