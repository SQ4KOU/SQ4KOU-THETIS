using System.Collections.Generic;

namespace System.Reactive.Linq.ObservableImpl;

internal sealed class Distinct<TSource, TKey> : Producer<TSource, Distinct<TSource, TKey>._>
{
	internal sealed class @_ : IdentitySink<TSource>
	{
		private readonly Func<TSource, TKey> _keySelector;

		private readonly HashSet<TKey> _hashSet;

		public _(Distinct<TSource, TKey> parent, IObserver<TSource> observer)
			: base(observer)
		{
			_keySelector = parent._keySelector;
			_hashSet = new HashSet<TKey>(parent._comparer);
		}

		public override void OnNext(TSource value)
		{
			bool flag;
			try
			{
				TKey item = _keySelector(value);
				flag = _hashSet.Add(item);
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

	private readonly Func<TSource, TKey> _keySelector;

	private readonly IEqualityComparer<TKey> _comparer;

	public Distinct(IObservable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
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
