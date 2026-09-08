using System.Collections.Generic;

namespace System.Reactive;

internal abstract class ConcatSink<TSource> : TailRecursiveSink<TSource>
{
	protected ConcatSink(IObserver<TSource> observer)
		: base(observer)
	{
	}

	protected override IEnumerable<IObservable<TSource>>? Extract(IObservable<TSource> source)
	{
		return (source as IConcatenatable<TSource>)?.GetSources();
	}

	public override void OnCompleted()
	{
		Recurse();
	}
}
