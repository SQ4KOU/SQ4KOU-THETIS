using System.Collections.Generic;

namespace System.Reactive.Linq.ObservableImpl;

internal sealed class ToList<TSource> : Producer<IList<TSource>, ToList<TSource>._>
{
	internal sealed class @_(IObserver<IList<TSource>> observer) : Sink<TSource, IList<TSource>>(observer)
	{
		private List<TSource> _list = new List<TSource>();

		public override void OnNext(TSource value)
		{
			_list.Add(value);
		}

		public override void OnError(Exception error)
		{
			Cleanup();
			ForwardOnError(error);
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
		}
	}

	private readonly IObservable<TSource> _source;

	public ToList(IObservable<TSource> source)
	{
		_source = source;
	}

	protected override @_ CreateSink(IObserver<IList<TSource>> observer)
	{
		return new @_(observer);
	}

	protected override void Run(@_ sink)
	{
		sink.Run(_source);
	}
}
