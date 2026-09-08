namespace System.Reactive.Subjects;

public interface ISubject<T> : ISubject<T, T>, IObserver<T>, IObservable<T>
{
}
public interface ISubject<in TSource, out TResult> : IObserver<TSource>, IObservable<TResult>
{
}
