using System.Runtime.CompilerServices;

namespace System.Reactive;

public interface ITaskObservableAwaiter<out T> : INotifyCompletion
{
	bool IsCompleted { get; }

	T GetResult();
}
