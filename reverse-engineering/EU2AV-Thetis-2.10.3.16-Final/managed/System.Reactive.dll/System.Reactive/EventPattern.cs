using System.Collections.Generic;

namespace System.Reactive;

public class EventPattern<TEventArgs> : EventPattern<object, TEventArgs>
{
	public EventPattern(object? sender, TEventArgs e)
		: base(sender, e)
	{
	}
}
public class EventPattern<TSender, TEventArgs> : IEquatable<EventPattern<TSender, TEventArgs>>, IEventPattern<TSender, TEventArgs>
{
	public TSender? Sender { get; }

	public TEventArgs EventArgs { get; }

	public EventPattern(TSender? sender, TEventArgs e)
	{
		Sender = sender;
		EventArgs = e;
	}

	public void Deconstruct(out TSender? sender, out TEventArgs e)
	{
		TSender sender2 = Sender;
		TEventArgs eventArgs = EventArgs;
		sender = sender2;
		e = eventArgs;
	}

	public bool Equals(EventPattern<TSender, TEventArgs>? other)
	{
		if ((object)other == null)
		{
			return false;
		}
		if ((object)this == other)
		{
			return true;
		}
		if (EqualityComparer<TSender>.Default.Equals(Sender, other.Sender))
		{
			return EqualityComparer<TEventArgs>.Default.Equals(EventArgs, other.EventArgs);
		}
		return false;
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as EventPattern<TSender, TEventArgs>);
	}

	public override int GetHashCode()
	{
		TSender sender = Sender;
		int num = ((sender != null) ? sender.GetHashCode() : 0);
		TEventArgs eventArgs = EventArgs;
		int num2 = ((eventArgs != null) ? eventArgs.GetHashCode() : 0);
		return (num << 5) + (num ^ num2);
	}

	public static bool operator ==(EventPattern<TSender, TEventArgs> first, EventPattern<TSender, TEventArgs> second)
	{
		return object.Equals(first, second);
	}

	public static bool operator !=(EventPattern<TSender, TEventArgs> first, EventPattern<TSender, TEventArgs> second)
	{
		return !object.Equals(first, second);
	}
}
