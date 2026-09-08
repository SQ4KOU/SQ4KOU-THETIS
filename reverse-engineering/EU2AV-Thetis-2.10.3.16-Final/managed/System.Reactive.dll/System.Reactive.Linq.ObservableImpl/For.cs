using System.Collections.Generic;

namespace System.Reactive.Linq.ObservableImpl;

internal sealed class For<TSource, TResult> : Producer<TResult, For<TSource, TResult>._>, IConcatenatable<TResult>
{
	internal sealed class @_ : ConcatSink<TResult>
	{
		public _(IObserver<TResult> observer)
			: base(observer)
		{
		}
	}

	private readonly IEnumerable<TSource> _source;

	private readonly Func<TSource, IObservable<TResult>> _resultSelector;

	public For(IEnumerable<TSource> source, Func<TSource, IObservable<TResult>> resultSelector)
	{
		_source = source;
		_resultSelector = resultSelector;
	}

	protected override @_ CreateSink(IObserver<TResult> observer)
	{
		return new @_(observer);
	}

	protected override void Run(@_ sink)
	{
		sink.Run(GetSources());
	}

	public IEnumerable<IObservable<TResult>> GetSources()
	{
		foreach (TSource item in _source)
		{
			yield return _resultSelector(item);
		}
	}
}
