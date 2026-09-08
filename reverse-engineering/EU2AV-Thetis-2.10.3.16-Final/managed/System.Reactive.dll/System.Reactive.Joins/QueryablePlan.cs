using System.Linq.Expressions;

namespace System.Reactive.Joins;

public class QueryablePlan<TResult>
{
	public Expression Expression { get; }

	internal QueryablePlan(Expression expression)
	{
		Expression = expression;
	}
}
