using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reflection;

namespace System.Reactive;

internal class ObservableQueryProvider : IQbservableProvider, IQueryProvider
{
	private static MethodInfo? _staticAsQueryable;

	private static MethodInfo AsQueryable => _staticAsQueryable ?? (_staticAsQueryable = Qbservable.InfoOf((Expression<Func<object>>)(() => ((IEnumerable<object>)null).AsQueryable())).GetGenericMethodDefinition());

	public IQbservable<TResult> CreateQuery<TResult>(Expression expression)
	{
		if (expression == null)
		{
			throw new ArgumentNullException("expression");
		}
		if (!typeof(IObservable<TResult>).IsAssignableFrom(expression.Type))
		{
			throw new ArgumentException(Strings_Providers.INVALID_TREE_TYPE, "expression");
		}
		return new ObservableQuery<TResult>(expression);
	}

	IQueryable<TElement> IQueryProvider.CreateQuery<TElement>(Expression expression)
	{
		if (!(expression is MethodCallExpression methodCallExpression) || methodCallExpression.Method.DeclaringType != typeof(Qbservable) || methodCallExpression.Method.Name != "ToQueryable")
		{
			throw new ArgumentException(Strings_Providers.EXPECTED_TOQUERYABLE_METHODCALL, "expression");
		}
		Expression arg = methodCallExpression.Arguments[0];
		return Expression.Lambda<Func<IQueryable<TElement>>>(Expression.Call(AsQueryable.MakeGenericMethod(typeof(TElement)), Expression.Call(typeof(Observable).GetMethod("ToEnumerable").MakeGenericMethod(typeof(TElement)), arg)), Array.Empty<ParameterExpression>()).Compile()();
	}

	IQueryable IQueryProvider.CreateQuery(Expression expression)
	{
		throw new NotImplementedException();
	}

	TResult IQueryProvider.Execute<TResult>(Expression expression)
	{
		throw new NotImplementedException();
	}

	object IQueryProvider.Execute(Expression expression)
	{
		throw new NotImplementedException();
	}
}
