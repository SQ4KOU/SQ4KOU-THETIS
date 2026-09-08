using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reflection;

namespace System.Reactive.Joins;

public abstract class QueryablePattern
{
	public Expression Expression { get; }

	protected QueryablePattern(Expression expression)
	{
		Expression = expression;
	}
}
public class QueryablePattern<TSource1, TSource2> : QueryablePattern
{
	internal QueryablePattern(Expression expression)
		: base(expression)
	{
	}

	public QueryablePattern<TSource1, TSource2, TSource3> And<TSource3>(IObservable<TSource3> other)
	{
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		MethodInfo method = typeof(QueryablePattern<TSource1, TSource2>).GetMethod("And").MakeGenericMethod(typeof(TSource3));
		return new QueryablePattern<TSource1, TSource2, TSource3>(System.Linq.Expressions.Expression.Call(base.Expression, method, Qbservable.GetSourceExpression(other)));
	}

	public QueryablePlan<TResult> Then<TResult>(Expression<Func<TSource1, TSource2, TResult>> selector)
	{
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		MethodInfo method = typeof(QueryablePattern<TSource1, TSource2>).GetMethod("Then").MakeGenericMethod(typeof(TResult));
		return new QueryablePlan<TResult>(System.Linq.Expressions.Expression.Call(base.Expression, method, selector));
	}
}
public class QueryablePattern<TSource1, TSource2, TSource3> : QueryablePattern
{
	internal QueryablePattern(Expression expression)
		: base(expression)
	{
	}

	public QueryablePattern<TSource1, TSource2, TSource3, TSource4> And<TSource4>(IObservable<TSource4> other)
	{
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		MethodInfo method = typeof(QueryablePattern<TSource1, TSource2, TSource3>).GetMethod("And").MakeGenericMethod(typeof(TSource4));
		return new QueryablePattern<TSource1, TSource2, TSource3, TSource4>(System.Linq.Expressions.Expression.Call(base.Expression, method, Qbservable.GetSourceExpression(other)));
	}

	public QueryablePlan<TResult> Then<TResult>(Expression<Func<TSource1, TSource2, TSource3, TResult>> selector)
	{
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		MethodInfo method = typeof(QueryablePattern<TSource1, TSource2, TSource3>).GetMethod("Then").MakeGenericMethod(typeof(TResult));
		return new QueryablePlan<TResult>(System.Linq.Expressions.Expression.Call(base.Expression, method, selector));
	}
}
public class QueryablePattern<TSource1, TSource2, TSource3, TSource4> : QueryablePattern
{
	internal QueryablePattern(Expression expression)
		: base(expression)
	{
	}

	public QueryablePattern<TSource1, TSource2, TSource3, TSource4, TSource5> And<TSource5>(IObservable<TSource5> other)
	{
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		MethodInfo method = typeof(QueryablePattern<TSource1, TSource2, TSource3, TSource4>).GetMethod("And").MakeGenericMethod(typeof(TSource5));
		return new QueryablePattern<TSource1, TSource2, TSource3, TSource4, TSource5>(System.Linq.Expressions.Expression.Call(base.Expression, method, Qbservable.GetSourceExpression(other)));
	}

	public QueryablePlan<TResult> Then<TResult>(Expression<Func<TSource1, TSource2, TSource3, TSource4, TResult>> selector)
	{
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		MethodInfo method = typeof(QueryablePattern<TSource1, TSource2, TSource3, TSource4>).GetMethod("Then").MakeGenericMethod(typeof(TResult));
		return new QueryablePlan<TResult>(System.Linq.Expressions.Expression.Call(base.Expression, method, selector));
	}
}
public class QueryablePattern<TSource1, TSource2, TSource3, TSource4, TSource5> : QueryablePattern
{
	internal QueryablePattern(Expression expression)
		: base(expression)
	{
	}

	public QueryablePattern<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6> And<TSource6>(IObservable<TSource6> other)
	{
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		MethodInfo method = typeof(QueryablePattern<TSource1, TSource2, TSource3, TSource4, TSource5>).GetMethod("And").MakeGenericMethod(typeof(TSource6));
		return new QueryablePattern<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6>(System.Linq.Expressions.Expression.Call(base.Expression, method, Qbservable.GetSourceExpression(other)));
	}

	public QueryablePlan<TResult> Then<TResult>(Expression<Func<TSource1, TSource2, TSource3, TSource4, TSource5, TResult>> selector)
	{
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		MethodInfo method = typeof(QueryablePattern<TSource1, TSource2, TSource3, TSource4, TSource5>).GetMethod("Then").MakeGenericMethod(typeof(TResult));
		return new QueryablePlan<TResult>(System.Linq.Expressions.Expression.Call(base.Expression, method, selector));
	}
}
public class QueryablePattern<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6> : QueryablePattern
{
	internal QueryablePattern(Expression expression)
		: base(expression)
	{
	}

	public QueryablePattern<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7> And<TSource7>(IObservable<TSource7> other)
	{
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		MethodInfo method = typeof(QueryablePattern<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6>).GetMethod("And").MakeGenericMethod(typeof(TSource7));
		return new QueryablePattern<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7>(System.Linq.Expressions.Expression.Call(base.Expression, method, Qbservable.GetSourceExpression(other)));
	}

	public QueryablePlan<TResult> Then<TResult>(Expression<Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TResult>> selector)
	{
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		MethodInfo method = typeof(QueryablePattern<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6>).GetMethod("Then").MakeGenericMethod(typeof(TResult));
		return new QueryablePlan<TResult>(System.Linq.Expressions.Expression.Call(base.Expression, method, selector));
	}
}
public class QueryablePattern<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7> : QueryablePattern
{
	internal QueryablePattern(Expression expression)
		: base(expression)
	{
	}

	public QueryablePattern<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8> And<TSource8>(IObservable<TSource8> other)
	{
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		MethodInfo method = typeof(QueryablePattern<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7>).GetMethod("And").MakeGenericMethod(typeof(TSource8));
		return new QueryablePattern<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8>(System.Linq.Expressions.Expression.Call(base.Expression, method, Qbservable.GetSourceExpression(other)));
	}

	public QueryablePlan<TResult> Then<TResult>(Expression<Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TResult>> selector)
	{
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		MethodInfo method = typeof(QueryablePattern<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7>).GetMethod("Then").MakeGenericMethod(typeof(TResult));
		return new QueryablePlan<TResult>(System.Linq.Expressions.Expression.Call(base.Expression, method, selector));
	}
}
public class QueryablePattern<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8> : QueryablePattern
{
	internal QueryablePattern(Expression expression)
		: base(expression)
	{
	}

	public QueryablePattern<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9> And<TSource9>(IObservable<TSource9> other)
	{
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		MethodInfo method = typeof(QueryablePattern<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8>).GetMethod("And").MakeGenericMethod(typeof(TSource9));
		return new QueryablePattern<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9>(System.Linq.Expressions.Expression.Call(base.Expression, method, Qbservable.GetSourceExpression(other)));
	}

	public QueryablePlan<TResult> Then<TResult>(Expression<Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TResult>> selector)
	{
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		MethodInfo method = typeof(QueryablePattern<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8>).GetMethod("Then").MakeGenericMethod(typeof(TResult));
		return new QueryablePlan<TResult>(System.Linq.Expressions.Expression.Call(base.Expression, method, selector));
	}
}
public class QueryablePattern<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9> : QueryablePattern
{
	internal QueryablePattern(Expression expression)
		: base(expression)
	{
	}

	public QueryablePattern<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10> And<TSource10>(IObservable<TSource10> other)
	{
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		MethodInfo method = typeof(QueryablePattern<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9>).GetMethod("And").MakeGenericMethod(typeof(TSource10));
		return new QueryablePattern<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10>(System.Linq.Expressions.Expression.Call(base.Expression, method, Qbservable.GetSourceExpression(other)));
	}

	public QueryablePlan<TResult> Then<TResult>(Expression<Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TResult>> selector)
	{
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		MethodInfo method = typeof(QueryablePattern<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9>).GetMethod("Then").MakeGenericMethod(typeof(TResult));
		return new QueryablePlan<TResult>(System.Linq.Expressions.Expression.Call(base.Expression, method, selector));
	}
}
public class QueryablePattern<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10> : QueryablePattern
{
	internal QueryablePattern(Expression expression)
		: base(expression)
	{
	}

	public QueryablePattern<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11> And<TSource11>(IObservable<TSource11> other)
	{
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		MethodInfo method = typeof(QueryablePattern<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10>).GetMethod("And").MakeGenericMethod(typeof(TSource11));
		return new QueryablePattern<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11>(System.Linq.Expressions.Expression.Call(base.Expression, method, Qbservable.GetSourceExpression(other)));
	}

	public QueryablePlan<TResult> Then<TResult>(Expression<Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TResult>> selector)
	{
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		MethodInfo method = typeof(QueryablePattern<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10>).GetMethod("Then").MakeGenericMethod(typeof(TResult));
		return new QueryablePlan<TResult>(System.Linq.Expressions.Expression.Call(base.Expression, method, selector));
	}
}
public class QueryablePattern<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11> : QueryablePattern
{
	internal QueryablePattern(Expression expression)
		: base(expression)
	{
	}

	public QueryablePattern<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12> And<TSource12>(IObservable<TSource12> other)
	{
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		MethodInfo method = typeof(QueryablePattern<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11>).GetMethod("And").MakeGenericMethod(typeof(TSource12));
		return new QueryablePattern<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12>(System.Linq.Expressions.Expression.Call(base.Expression, method, Qbservable.GetSourceExpression(other)));
	}

	public QueryablePlan<TResult> Then<TResult>(Expression<Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TResult>> selector)
	{
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		MethodInfo method = typeof(QueryablePattern<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11>).GetMethod("Then").MakeGenericMethod(typeof(TResult));
		return new QueryablePlan<TResult>(System.Linq.Expressions.Expression.Call(base.Expression, method, selector));
	}
}
public class QueryablePattern<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12> : QueryablePattern
{
	internal QueryablePattern(Expression expression)
		: base(expression)
	{
	}

	public QueryablePattern<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13> And<TSource13>(IObservable<TSource13> other)
	{
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		MethodInfo method = typeof(QueryablePattern<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12>).GetMethod("And").MakeGenericMethod(typeof(TSource13));
		return new QueryablePattern<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13>(System.Linq.Expressions.Expression.Call(base.Expression, method, Qbservable.GetSourceExpression(other)));
	}

	public QueryablePlan<TResult> Then<TResult>(Expression<Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TResult>> selector)
	{
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		MethodInfo method = typeof(QueryablePattern<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12>).GetMethod("Then").MakeGenericMethod(typeof(TResult));
		return new QueryablePlan<TResult>(System.Linq.Expressions.Expression.Call(base.Expression, method, selector));
	}
}
public class QueryablePattern<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13> : QueryablePattern
{
	internal QueryablePattern(Expression expression)
		: base(expression)
	{
	}

	public QueryablePattern<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TSource14> And<TSource14>(IObservable<TSource14> other)
	{
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		MethodInfo method = typeof(QueryablePattern<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13>).GetMethod("And").MakeGenericMethod(typeof(TSource14));
		return new QueryablePattern<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TSource14>(System.Linq.Expressions.Expression.Call(base.Expression, method, Qbservable.GetSourceExpression(other)));
	}

	public QueryablePlan<TResult> Then<TResult>(Expression<Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TResult>> selector)
	{
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		MethodInfo method = typeof(QueryablePattern<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13>).GetMethod("Then").MakeGenericMethod(typeof(TResult));
		return new QueryablePlan<TResult>(System.Linq.Expressions.Expression.Call(base.Expression, method, selector));
	}
}
public class QueryablePattern<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TSource14> : QueryablePattern
{
	internal QueryablePattern(Expression expression)
		: base(expression)
	{
	}

	public QueryablePattern<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TSource14, TSource15> And<TSource15>(IObservable<TSource15> other)
	{
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		MethodInfo method = typeof(QueryablePattern<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TSource14>).GetMethod("And").MakeGenericMethod(typeof(TSource15));
		return new QueryablePattern<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TSource14, TSource15>(System.Linq.Expressions.Expression.Call(base.Expression, method, Qbservable.GetSourceExpression(other)));
	}

	public QueryablePlan<TResult> Then<TResult>(Expression<Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TSource14, TResult>> selector)
	{
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		MethodInfo method = typeof(QueryablePattern<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TSource14>).GetMethod("Then").MakeGenericMethod(typeof(TResult));
		return new QueryablePlan<TResult>(System.Linq.Expressions.Expression.Call(base.Expression, method, selector));
	}
}
public class QueryablePattern<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TSource14, TSource15> : QueryablePattern
{
	internal QueryablePattern(Expression expression)
		: base(expression)
	{
	}

	public QueryablePattern<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TSource14, TSource15, TSource16> And<TSource16>(IObservable<TSource16> other)
	{
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		MethodInfo method = typeof(QueryablePattern<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TSource14, TSource15>).GetMethod("And").MakeGenericMethod(typeof(TSource16));
		return new QueryablePattern<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TSource14, TSource15, TSource16>(System.Linq.Expressions.Expression.Call(base.Expression, method, Qbservable.GetSourceExpression(other)));
	}

	public QueryablePlan<TResult> Then<TResult>(Expression<Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TSource14, TSource15, TResult>> selector)
	{
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		MethodInfo method = typeof(QueryablePattern<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TSource14, TSource15>).GetMethod("Then").MakeGenericMethod(typeof(TResult));
		return new QueryablePlan<TResult>(System.Linq.Expressions.Expression.Call(base.Expression, method, selector));
	}
}
public class QueryablePattern<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TSource14, TSource15, TSource16> : QueryablePattern
{
	internal QueryablePattern(Expression expression)
		: base(expression)
	{
	}

	public QueryablePlan<TResult> Then<TResult>(Expression<Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TSource14, TSource15, TSource16, TResult>> selector)
	{
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		MethodInfo method = typeof(QueryablePattern<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TSource14, TSource15, TSource16>).GetMethod("Then").MakeGenericMethod(typeof(TResult));
		return new QueryablePlan<TResult>(System.Linq.Expressions.Expression.Call(base.Expression, method, selector));
	}
}
