using System.Collections.Generic;
using System.Linq;

namespace System.Reactive.Linq.ObservableImpl;

internal sealed class ToLookup<TSource, TKey, TElement> : Producer<ILookup<TKey, TElement>, ToLookup<TSource, TKey, TElement>._>
{
	internal sealed class @_ : Sink<TSource, ILookup<TKey, TElement>>
	{
		private readonly Func<TSource, TKey> _keySelector;

		private readonly Func<TSource, TElement> _elementSelector;

		private Lookup<TKey, TElement> _lookup;

		public _(ToLookup<TSource, TKey, TElement> parent, IObserver<ILookup<TKey, TElement>> observer)
			: base(observer)
		{
			_keySelector = parent._keySelector;
			_elementSelector = parent._elementSelector;
			_lookup = new Lookup<TKey, TElement>(parent._comparer);
		}

		public override void OnNext(TSource value)
		{
			try
			{
				_lookup.Add(_keySelector(value), _elementSelector(value));
			}
			catch (Exception error)
			{
				Cleanup();
				ForwardOnError(error);
			}
		}

		public override void OnError(Exception error)
		{
			Cleanup();
			ForwardOnError(error);
		}

		public override void OnCompleted()
		{
			Lookup<TKey, TElement> lookup = _lookup;
			Cleanup();
			ForwardOnNext(lookup);
			ForwardOnCompleted();
		}

		private void Cleanup()
		{
			_lookup = null;
		}
	}

	private readonly IObservable<TSource> _source;

	private readonly Func<TSource, TKey> _keySelector;

	private readonly Func<TSource, TElement> _elementSelector;

	private readonly IEqualityComparer<TKey> _comparer;

	public ToLookup(IObservable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer)
	{
		_source = source;
		_keySelector = keySelector;
		_elementSelector = elementSelector;
		_comparer = comparer;
	}

	protected override @_ CreateSink(IObserver<ILookup<TKey, TElement>> observer)
	{
		return new @_(this, observer);
	}

	protected override void Run(@_ sink)
	{
		sink.Run(_source);
	}
}
