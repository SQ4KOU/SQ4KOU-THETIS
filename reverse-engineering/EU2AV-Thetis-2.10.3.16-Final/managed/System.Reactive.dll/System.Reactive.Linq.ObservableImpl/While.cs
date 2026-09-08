using System.Collections.Generic;

namespace System.Reactive.Linq.ObservableImpl;

internal sealed class While<TSource> : Producer<TSource, While<TSource>._>, IConcatenatable<TSource>
{
	internal sealed class @_ : ConcatSink<TSource>
	{
		public _(IObserver<TSource> observer)
			: base(observer)
		{
		}
	}

	private readonly Func<bool> _condition;

	private readonly IObservable<TSource> _source;

	public While(Func<bool> condition, IObservable<TSource> source)
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
		while (_condition())
		{
			yield return _source;
		}
	}
}
