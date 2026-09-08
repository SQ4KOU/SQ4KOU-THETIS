using System.Collections;
using System.Collections.Generic;

namespace System.Reactive.Linq.ObservableImpl;

internal abstract class PushToPullAdapter<TSource, TResult> : IEnumerable<TResult>, IEnumerable
{
	private readonly IObservable<TSource> _source;

	protected PushToPullAdapter(IObservable<TSource> source)
	{
		_source = source;
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public IEnumerator<TResult> GetEnumerator()
	{
		PushToPullSink<TSource, TResult> pushToPullSink = Run();
		pushToPullSink.SetUpstream(_source.SubscribeSafe(pushToPullSink));
		return pushToPullSink;
	}

	protected abstract PushToPullSink<TSource, TResult> Run();
}
