using System.Collections.Generic;

namespace System.Reactive.Linq.ObservableImpl;

internal sealed class Concat<TSource> : Producer<TSource, Concat<TSource>._>, IConcatenatable<TSource>
{
	internal sealed class @_ : ConcatSink<TSource>
	{
		public _(IObserver<TSource> observer)
			: base(observer)
		{
		}
	}

	private readonly IEnumerable<IObservable<TSource>> _sources;

	public Concat(IEnumerable<IObservable<TSource>> sources)
	{
		_sources = sources;
	}

	protected override @_ CreateSink(IObserver<TSource> observer)
	{
		return new @_(observer);
	}

	protected override void Run(@_ sink)
	{
		sink.Run(_sources);
	}

	public IEnumerable<IObservable<TSource>> GetSources()
	{
		return _sources;
	}
}
