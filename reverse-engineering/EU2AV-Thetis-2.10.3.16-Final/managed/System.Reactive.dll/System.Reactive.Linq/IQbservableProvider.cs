using System.Linq.Expressions;

namespace System.Reactive.Linq;

public interface IQbservableProvider
{
	IQbservable<TResult> CreateQuery<TResult>(Expression expression);
}
