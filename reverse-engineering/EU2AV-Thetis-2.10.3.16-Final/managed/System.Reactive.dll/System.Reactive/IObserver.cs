namespace System.Reactive;

public interface IObserver<in TValue, out TResult>
{
	TResult OnNext(TValue value);

	TResult OnError(Exception exception);

	TResult OnCompleted();
}
