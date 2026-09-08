using System.Reactive.Concurrency;

namespace System.Reactive.Linq.ObservableImpl;

internal sealed class FromEvent<TDelegate, TEventArgs> : ClassicEventProducer<TDelegate, TEventArgs>
{
	private readonly Func<Action<TEventArgs>, TDelegate>? _conversion;

	public FromEvent(Action<TDelegate> addHandler, Action<TDelegate> removeHandler, IScheduler scheduler)
		: base(addHandler, removeHandler, scheduler)
	{
	}

	public FromEvent(Func<Action<TEventArgs>, TDelegate> conversion, Action<TDelegate> addHandler, Action<TDelegate> removeHandler, IScheduler scheduler)
		: base(addHandler, removeHandler, scheduler)
	{
		_conversion = conversion;
	}

	protected override TDelegate GetHandler(Action<TEventArgs> onNext)
	{
		if (_conversion == null)
		{
			return ReflectionUtils.CreateDelegate<TDelegate>(onNext, typeof(Action<TEventArgs>).GetMethod("Invoke"));
		}
		return _conversion(onNext);
	}
}
