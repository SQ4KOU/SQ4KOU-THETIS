using System.Collections.Generic;

namespace System.Reactive.Linq.ObservableImpl;

internal sealed class DoWhile<TSource> : Producer<TSource, DoWhile<TSource>._>, IConcatenatable<TSource>
{
	internal sealed class @_ : ConcatSink<TSource>
	{
		public _(IObserver<TSource> observer)
			: base(observer)
		{
		}
	}

	private readonly IObservable<TSource> _source;

	private readonly Func<bool> _condition;

	public DoWhile(IObservable<TSource> source, Func<bool> condition)
	{
		_condition = condition;
		_source = source;
	}

	protected override @_ CreateSink(IObserver<TSource> observer)
	{
		return new @_(observer);
	}

	protected override void Run(@_ sink)
	{
		sink.Run(GetSources());
	}

	public IEnumerable<IObservable<TSource>> GetSources()
	{
		yield return _source;
		while (_condition())
		{
			yield return _source;
		}
	}
}
