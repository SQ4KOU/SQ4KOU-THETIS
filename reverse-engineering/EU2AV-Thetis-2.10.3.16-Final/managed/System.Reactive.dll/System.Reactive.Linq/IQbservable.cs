using System.Linq.Expressions;

namespace System.Reactive.Linq;

public interface IQbservable<out T> : IQbservable, IObservable<T>
{
}
public interface IQbservable
{
	Type ElementType { get; }

	Expression Expression { get; }

	IQbservableProvider Provider { get; }
}
