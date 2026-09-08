using System.Runtime.CompilerServices;

namespace System.Reactive;

[AsyncMethodBuilder(typeof(TaskObservableMethodBuilder<>))]
public interface ITaskObservable<out T> : IObservable<T>
{
	ITaskObservableAwaiter<T> GetAwaiter();
}
