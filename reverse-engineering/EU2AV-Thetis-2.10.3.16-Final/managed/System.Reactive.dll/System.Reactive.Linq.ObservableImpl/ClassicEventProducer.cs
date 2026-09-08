using System.Reactive.Concurrency;
using System.Reactive.Disposables;

namespace System.Reactive.Linq.ObservableImpl;

internal abstract class ClassicEventProducer<TDelegate, TArgs> : EventProducer<TDelegate, TArgs>
{
	private readonly Action<TDelegate> _addHandler;

	private readonly Action<TDelegate> _removeHandler;

	protected ClassicEventProducer(Action<TDelegate> addHandler, Action<TDelegate> removeHandler, IScheduler scheduler)
		: base(scheduler)
	{
		_addHandler = addHandler;
		_removeHandler = removeHandler;
	}

	protected override IDisposable AddHandler(TDelegate handler)
	{
		_addHandler(handler);
		return Disposable.Create((_removeHandler, handler), delegate((Action<TDelegate> _removeHandler, TDelegate handler) tuple)
		{
			tuple._removeHandler(tuple.handler);
		});
	}
}
