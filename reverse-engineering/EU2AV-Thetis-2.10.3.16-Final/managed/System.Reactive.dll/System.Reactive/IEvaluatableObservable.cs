namespace System.Reactive;

internal interface IEvaluatableObservable<out T>
{
	IObservable<T> Eval();
}
