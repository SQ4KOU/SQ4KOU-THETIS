namespace System.Reactive;

public interface IEventPatternSource<TEventArgs>
{
	event EventHandler<TEventArgs> OnNext;
}
