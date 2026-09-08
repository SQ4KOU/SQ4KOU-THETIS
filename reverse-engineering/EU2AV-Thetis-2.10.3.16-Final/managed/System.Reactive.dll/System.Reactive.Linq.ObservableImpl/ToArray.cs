using System.Collections.Generic;

namespace System.Reactive.Linq.ObservableImpl;

internal sealed class ToArray<TSource> : Producer<TSource[], ToArray<TSource>._>
{
	internal sealed class @_(IObserver<TSource[]> observer) : Sink<TSource, TSource[]>(observer)
	{
		private List<TSource> _list = new List<TSource>();

		public override void OnNext(TSource value)
		{
			_list.Add(value);
		}

		public override void OnError(Exception error)
		{
			Cleanup();
			base.OnError(error);
		}

		public override void OnCompleted()
		{
			List<TSource> list = _list;
			Cleanup();
			ForwardOnNext(list.ToArray());
			ForwardOnCompleted();
		}

		private void Cleanup()
		{
			_list = null;
		}
	}

	private readonly IObservable<TSource> _source;

	public ToArray(IObservable<TSource> source)
	{
		_source = source;
	}

	protected override @_ CreateSink(IObserver<TSource[]> observer)
	{
		return new @_(observer);
	}

	protected override void Run(@_ sink)
	{
		sink.Run(_source);
	}
}
