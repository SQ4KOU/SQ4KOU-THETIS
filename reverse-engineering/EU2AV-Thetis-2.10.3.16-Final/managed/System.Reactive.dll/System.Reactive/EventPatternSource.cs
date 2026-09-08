namespace System.Reactive;

internal sealed class EventPatternSource<TEventArgs> : EventPatternSourceBase<object, TEventArgs>, IEventPatternSource<TEventArgs>
{
	event EventHandler<TEventArgs> IEventPatternSource<TEventArgs>.OnNext
	{
		add
		{
			Add(value, delegate(object? o, TEventArgs e)
			{
				value(o, e);
			});
		}
		remove
		{
			Remove(value);
		}
	}

	public EventPatternSource(IObservable<EventPattern<object, TEventArgs>> source, Action<Action<object?, TEventArgs>, EventPattern<object, TEventArgs>> invokeHandler)
		: base(source, invokeHandler)
	{
	}
}
