using System.Collections.Generic;

namespace System.Reactive.Linq.ObservableImpl;

internal sealed class OnErrorResumeNext<TSource> : Producer<TSource, OnErrorResumeNext<TSource>._>
{
	internal sealed class @_ : TailRecursiveSink<TSource>
	{
		public _(IObserver<TSource> observer)
			: base(observer)
		{
		}

		protected override IEnumerable<IObservable<TSource>>? Extract(IObservable<TSource> source)
		{
			if (source is OnErrorResumeNext<TSource> onErrorResumeNext)
			{
				return onErrorResumeNext._sources;
			}
			return null;
		}

		public override void OnError(Exception error)
		{
			Recurse();
		}

		public override void OnCompleted()
		{
			Recurse();
		}

		protected override bool Fail(Exception error)
		{
			OnError(error);
			return true;
		}
	}

	private readonly IEnumerable<IObservable<TSource>> _sources;

	public OnErrorResumeNext(IEnumerable<IObservable<TSource>> sources)
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
}
