using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace System.Linq;

public static class AsyncEnumerableDeprecated
{
	public static ValueTask<TSource> AggregateAwaitAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, TSource, ValueTask<TSource>> accumulator, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.AggregateAwaitAsync(accumulator, cancellationToken);
	}

	public static ValueTask<TSource> AggregateAwaitWithCancellationAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, TSource, CancellationToken, ValueTask<TSource>> accumulator, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.AggregateAwaitWithCancellationAsync(accumulator, cancellationToken);
	}

	public static ValueTask<TAccumulate> AggregateAwaitAsync<TSource, TAccumulate>(IAsyncEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, ValueTask<TAccumulate>> accumulator, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.AggregateAwaitAsync(seed, accumulator, cancellationToken);
	}

	public static ValueTask<TAccumulate> AggregateAwaitWithCancellationAsync<TSource, TAccumulate>(IAsyncEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, CancellationToken, ValueTask<TAccumulate>> accumulator, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.AggregateAwaitWithCancellationAsync(seed, accumulator, cancellationToken);
	}

	public static ValueTask<TResult> AggregateAwaitAsync<TSource, TAccumulate, TResult>(IAsyncEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, ValueTask<TAccumulate>> accumulator, Func<TAccumulate, ValueTask<TResult>> resultSelector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.AggregateAwaitAsync(seed, accumulator, resultSelector, cancellationToken);
	}

	public static ValueTask<TResult> AggregateAwaitWithCancellationAsync<TSource, TAccumulate, TResult>(IAsyncEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, CancellationToken, ValueTask<TAccumulate>> accumulator, Func<TAccumulate, CancellationToken, ValueTask<TResult>> resultSelector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.AggregateAwaitWithCancellationAsync(seed, accumulator, resultSelector, cancellationToken);
	}

	public static ValueTask<bool> AllAwaitAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.AllAwaitAsync(predicate, cancellationToken);
	}

	public static ValueTask<bool> AllAwaitWithCancellationAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.AllAwaitWithCancellationAsync(predicate, cancellationToken);
	}

	public static ValueTask<bool> AnyAwaitAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.AnyAwaitAsync(predicate, cancellationToken);
	}

	public static ValueTask<bool> AnyAwaitWithCancellationAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.AnyAwaitWithCancellationAsync(predicate, cancellationToken);
	}

	public static ValueTask<double> AverageAwaitAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<int>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.AverageAwaitAsync(selector, cancellationToken);
	}

	public static ValueTask<double> AverageAwaitWithCancellationAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<int>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.AverageAwaitWithCancellationAsync(selector, cancellationToken);
	}

	public static ValueTask<double> AverageAwaitAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<long>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.AverageAwaitAsync(selector, cancellationToken);
	}

	public static ValueTask<double> AverageAwaitWithCancellationAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<long>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.AverageAwaitWithCancellationAsync(selector, cancellationToken);
	}

	public static ValueTask<float> AverageAwaitAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<float>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.AverageAwaitAsync(selector, cancellationToken);
	}

	public static ValueTask<float> AverageAwaitWithCancellationAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<float>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.AverageAwaitWithCancellationAsync(selector, cancellationToken);
	}

	public static ValueTask<double> AverageAwaitAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<double>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.AverageAwaitAsync(selector, cancellationToken);
	}

	public static ValueTask<double> AverageAwaitWithCancellationAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<double>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.AverageAwaitWithCancellationAsync(selector, cancellationToken);
	}

	public static ValueTask<decimal> AverageAwaitAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<decimal>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.AverageAwaitAsync(selector, cancellationToken);
	}

	public static ValueTask<decimal> AverageAwaitWithCancellationAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<decimal>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.AverageAwaitWithCancellationAsync(selector, cancellationToken);
	}

	public static ValueTask<double?> AverageAwaitAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<int?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.AverageAwaitAsync(selector, cancellationToken);
	}

	public static ValueTask<double?> AverageAwaitWithCancellationAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<int?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.AverageAwaitWithCancellationAsync(selector, cancellationToken);
	}

	public static ValueTask<double?> AverageAwaitAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<long?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.AverageAwaitAsync(selector, cancellationToken);
	}

	public static ValueTask<double?> AverageAwaitWithCancellationAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<long?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.AverageAwaitWithCancellationAsync(selector, cancellationToken);
	}

	public static ValueTask<float?> AverageAwaitAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<float?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.AverageAwaitAsync(selector, cancellationToken);
	}

	public static ValueTask<float?> AverageAwaitWithCancellationAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<float?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.AverageAwaitWithCancellationAsync(selector, cancellationToken);
	}

	public static ValueTask<double?> AverageAwaitAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<double?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.AverageAwaitAsync(selector, cancellationToken);
	}

	public static ValueTask<double?> AverageAwaitWithCancellationAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<double?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.AverageAwaitWithCancellationAsync(selector, cancellationToken);
	}

	public static ValueTask<decimal?> AverageAwaitAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<decimal?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.AverageAwaitAsync(selector, cancellationToken);
	}

	public static ValueTask<decimal?> AverageAwaitWithCancellationAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<decimal?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.AverageAwaitWithCancellationAsync(selector, cancellationToken);
	}

	public static ValueTask<int> CountAwaitAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.CountAwaitAsync(predicate, cancellationToken);
	}

	public static ValueTask<int> CountAwaitWithCancellationAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.CountAwaitWithCancellationAsync(predicate, cancellationToken);
	}

	public static ValueTask<TSource> FirstAwaitAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.FirstAwaitAsync(predicate, cancellationToken);
	}

	public static ValueTask<TSource> FirstAwaitWithCancellationAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.FirstAwaitWithCancellationAsync(predicate, cancellationToken);
	}

	public static ValueTask<TSource?> FirstOrDefaultAwaitAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.FirstOrDefaultAwaitAsync(predicate, cancellationToken);
	}

	public static ValueTask<TSource?> FirstOrDefaultAwaitWithCancellationAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.FirstOrDefaultAwaitWithCancellationAsync(predicate, cancellationToken);
	}

	public static Task ForEachAsync<TSource>(IAsyncEnumerable<TSource> source, Action<TSource> action, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.ForEachAsync(action, cancellationToken);
	}

	public static Task ForEachAsync<TSource>(IAsyncEnumerable<TSource> source, Action<TSource, int> action, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.ForEachAsync(action, cancellationToken);
	}

	public static Task ForEachAwaitAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, Task> action, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.ForEachAwaitAsync(action, cancellationToken);
	}

	public static Task ForEachAwaitWithCancellationAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, Task> action, CancellationToken cancellationToken)
	{
		return source.ForEachAwaitWithCancellationAsync(action, cancellationToken);
	}

	public static Task ForEachAwaitAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, int, Task> action, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.ForEachAwaitAsync(action, cancellationToken);
	}

	public static Task ForEachAwaitWithCancellationAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, int, CancellationToken, Task> action, CancellationToken cancellationToken)
	{
		return source.ForEachAwaitWithCancellationAsync(action, cancellationToken);
	}

	public static IAsyncEnumerable<IAsyncGrouping<TKey, TSource>> GroupByAwait<TSource, TKey>(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector)
	{
		return source.GroupByAwait(keySelector);
	}

	public static IAsyncEnumerable<IAsyncGrouping<TKey, TSource>> GroupByAwait<TSource, TKey>(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector, IEqualityComparer<TKey>? comparer)
	{
		return source.GroupByAwait(keySelector, comparer);
	}

	public static IAsyncEnumerable<IAsyncGrouping<TKey, TSource>> GroupByAwaitWithCancellation<TSource, TKey>(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector)
	{
		return source.GroupByAwaitWithCancellation(keySelector);
	}

	public static IAsyncEnumerable<IAsyncGrouping<TKey, TSource>> GroupByAwaitWithCancellation<TSource, TKey>(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, IEqualityComparer<TKey>? comparer)
	{
		return source.GroupByAwaitWithCancellation(keySelector, comparer);
	}

	public static IAsyncEnumerable<IAsyncGrouping<TKey, TElement>> GroupByAwait<TSource, TKey, TElement>(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector, Func<TSource, ValueTask<TElement>> elementSelector)
	{
		return source.GroupByAwait(keySelector, elementSelector);
	}

	public static IAsyncEnumerable<IAsyncGrouping<TKey, TElement>> GroupByAwait<TSource, TKey, TElement>(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector, Func<TSource, ValueTask<TElement>> elementSelector, IEqualityComparer<TKey>? comparer)
	{
		return source.GroupByAwait(keySelector, elementSelector, comparer);
	}

	public static IAsyncEnumerable<IAsyncGrouping<TKey, TElement>> GroupByAwaitWithCancellation<TSource, TKey, TElement>(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, Func<TSource, CancellationToken, ValueTask<TElement>> elementSelector)
	{
		return source.GroupByAwaitWithCancellation(keySelector, elementSelector);
	}

	public static IAsyncEnumerable<IAsyncGrouping<TKey, TElement>> GroupByAwaitWithCancellation<TSource, TKey, TElement>(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, Func<TSource, CancellationToken, ValueTask<TElement>> elementSelector, IEqualityComparer<TKey>? comparer)
	{
		return source.GroupByAwaitWithCancellation(keySelector, elementSelector, comparer);
	}

	public static IAsyncEnumerable<TResult> GroupByAwait<TSource, TKey, TResult>(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector, Func<TKey, IAsyncEnumerable<TSource>, ValueTask<TResult>> resultSelector)
	{
		return source.GroupByAwait(keySelector, resultSelector);
	}

	public static IAsyncEnumerable<TResult> GroupByAwait<TSource, TKey, TResult>(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector, Func<TKey, IAsyncEnumerable<TSource>, ValueTask<TResult>> resultSelector, IEqualityComparer<TKey>? comparer)
	{
		return source.GroupByAwait(keySelector, resultSelector, comparer);
	}

	public static IAsyncEnumerable<TResult> GroupByAwaitWithCancellation<TSource, TKey, TResult>(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, Func<TKey, IAsyncEnumerable<TSource>, CancellationToken, ValueTask<TResult>> resultSelector)
	{
		return source.GroupByAwaitWithCancellation(keySelector, resultSelector);
	}

	public static IAsyncEnumerable<TResult> GroupByAwaitWithCancellation<TSource, TKey, TResult>(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, Func<TKey, IAsyncEnumerable<TSource>, CancellationToken, ValueTask<TResult>> resultSelector, IEqualityComparer<TKey>? comparer)
	{
		return source.GroupByAwaitWithCancellation(keySelector, resultSelector, comparer);
	}

	public static IAsyncEnumerable<TResult> GroupByAwait<TSource, TKey, TElement, TResult>(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector, Func<TSource, ValueTask<TElement>> elementSelector, Func<TKey, IAsyncEnumerable<TElement>, ValueTask<TResult>> resultSelector)
	{
		return source.GroupByAwait(keySelector, elementSelector, resultSelector);
	}

	public static IAsyncEnumerable<TResult> GroupByAwait<TSource, TKey, TElement, TResult>(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector, Func<TSource, ValueTask<TElement>> elementSelector, Func<TKey, IAsyncEnumerable<TElement>, ValueTask<TResult>> resultSelector, IEqualityComparer<TKey>? comparer)
	{
		return source.GroupByAwait(keySelector, elementSelector, resultSelector, comparer);
	}

	public static IAsyncEnumerable<TResult> GroupByAwaitWithCancellation<TSource, TKey, TElement, TResult>(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, Func<TSource, CancellationToken, ValueTask<TElement>> elementSelector, Func<TKey, IAsyncEnumerable<TElement>, CancellationToken, ValueTask<TResult>> resultSelector)
	{
		return source.GroupByAwaitWithCancellation(keySelector, elementSelector, resultSelector);
	}

	public static IAsyncEnumerable<TResult> GroupByAwaitWithCancellation<TSource, TKey, TElement, TResult>(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, Func<TSource, CancellationToken, ValueTask<TElement>> elementSelector, Func<TKey, IAsyncEnumerable<TElement>, CancellationToken, ValueTask<TResult>> resultSelector, IEqualityComparer<TKey>? comparer)
	{
		return source.GroupByAwaitWithCancellation(keySelector, elementSelector, resultSelector, comparer);
	}

	public static IAsyncEnumerable<TResult> GroupJoinAwait<TOuter, TInner, TKey, TResult>(IAsyncEnumerable<TOuter> outer, IAsyncEnumerable<TInner> inner, Func<TOuter, ValueTask<TKey>> outerKeySelector, Func<TInner, ValueTask<TKey>> innerKeySelector, Func<TOuter, IAsyncEnumerable<TInner>, ValueTask<TResult>> resultSelector)
	{
		return outer.GroupJoinAwait(inner, outerKeySelector, innerKeySelector, resultSelector);
	}

	public static IAsyncEnumerable<TResult> GroupJoinAwait<TOuter, TInner, TKey, TResult>(IAsyncEnumerable<TOuter> outer, IAsyncEnumerable<TInner> inner, Func<TOuter, ValueTask<TKey>> outerKeySelector, Func<TInner, ValueTask<TKey>> innerKeySelector, Func<TOuter, IAsyncEnumerable<TInner>, ValueTask<TResult>> resultSelector, IEqualityComparer<TKey>? comparer)
	{
		return outer.GroupJoinAwait(inner, outerKeySelector, innerKeySelector, resultSelector, comparer);
	}

	public static IAsyncEnumerable<TResult> GroupJoinAwaitWithCancellation<TOuter, TInner, TKey, TResult>(IAsyncEnumerable<TOuter> outer, IAsyncEnumerable<TInner> inner, Func<TOuter, CancellationToken, ValueTask<TKey>> outerKeySelector, Func<TInner, CancellationToken, ValueTask<TKey>> innerKeySelector, Func<TOuter, IAsyncEnumerable<TInner>, CancellationToken, ValueTask<TResult>> resultSelector)
	{
		return outer.GroupJoinAwaitWithCancellation(inner, outerKeySelector, innerKeySelector, resultSelector);
	}

	public static IAsyncEnumerable<TResult> GroupJoinAwaitWithCancellation<TOuter, TInner, TKey, TResult>(IAsyncEnumerable<TOuter> outer, IAsyncEnumerable<TInner> inner, Func<TOuter, CancellationToken, ValueTask<TKey>> outerKeySelector, Func<TInner, CancellationToken, ValueTask<TKey>> innerKeySelector, Func<TOuter, IAsyncEnumerable<TInner>, CancellationToken, ValueTask<TResult>> resultSelector, IEqualityComparer<TKey>? comparer)
	{
		return outer.GroupJoinAwaitWithCancellation(inner, outerKeySelector, innerKeySelector, resultSelector, comparer);
	}

	public static IAsyncEnumerable<TResult> JoinAwait<TOuter, TInner, TKey, TResult>(IAsyncEnumerable<TOuter> outer, IAsyncEnumerable<TInner> inner, Func<TOuter, ValueTask<TKey>> outerKeySelector, Func<TInner, ValueTask<TKey>> innerKeySelector, Func<TOuter, TInner, ValueTask<TResult>> resultSelector)
	{
		return outer.JoinAwait(inner, outerKeySelector, innerKeySelector, resultSelector);
	}

	public static IAsyncEnumerable<TResult> JoinAwait<TOuter, TInner, TKey, TResult>(IAsyncEnumerable<TOuter> outer, IAsyncEnumerable<TInner> inner, Func<TOuter, ValueTask<TKey>> outerKeySelector, Func<TInner, ValueTask<TKey>> innerKeySelector, Func<TOuter, TInner, ValueTask<TResult>> resultSelector, IEqualityComparer<TKey>? comparer)
	{
		return outer.JoinAwait(inner, outerKeySelector, innerKeySelector, resultSelector, comparer);
	}

	public static IAsyncEnumerable<TResult> JoinAwaitWithCancellation<TOuter, TInner, TKey, TResult>(IAsyncEnumerable<TOuter> outer, IAsyncEnumerable<TInner> inner, Func<TOuter, CancellationToken, ValueTask<TKey>> outerKeySelector, Func<TInner, CancellationToken, ValueTask<TKey>> innerKeySelector, Func<TOuter, TInner, CancellationToken, ValueTask<TResult>> resultSelector)
	{
		return outer.JoinAwaitWithCancellation(inner, outerKeySelector, innerKeySelector, resultSelector);
	}

	public static IAsyncEnumerable<TResult> JoinAwaitWithCancellation<TOuter, TInner, TKey, TResult>(IAsyncEnumerable<TOuter> outer, IAsyncEnumerable<TInner> inner, Func<TOuter, CancellationToken, ValueTask<TKey>> outerKeySelector, Func<TInner, CancellationToken, ValueTask<TKey>> innerKeySelector, Func<TOuter, TInner, CancellationToken, ValueTask<TResult>> resultSelector, IEqualityComparer<TKey>? comparer)
	{
		return outer.JoinAwaitWithCancellation(inner, outerKeySelector, innerKeySelector, resultSelector, comparer);
	}

	public static ValueTask<TSource> LastAwaitAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.LastAwaitAsync(predicate, cancellationToken);
	}

	public static ValueTask<TSource> LastAwaitWithCancellationAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.LastAwaitWithCancellationAsync(predicate, cancellationToken);
	}

	public static ValueTask<TSource?> LastOrDefaultAwaitAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.LastOrDefaultAwaitAsync(predicate, cancellationToken);
	}

	public static ValueTask<TSource?> LastOrDefaultAwaitWithCancellationAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.LastOrDefaultAwaitWithCancellationAsync(predicate, cancellationToken);
	}

	public static ValueTask<long> LongCountAwaitAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.LongCountAwaitAsync(predicate, cancellationToken);
	}

	public static ValueTask<long> LongCountAwaitWithCancellationAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.LongCountAwaitWithCancellationAsync(predicate, cancellationToken);
	}

	public static ValueTask<TResult> MaxAsync<TSource, TResult>(IAsyncEnumerable<TSource> source, Func<TSource, TResult> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.MaxAsync(selector, cancellationToken);
	}

	public static ValueTask<TResult> MaxAwaitAsync<TSource, TResult>(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TResult>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.MaxAwaitAsync(selector, cancellationToken);
	}

	public static ValueTask<TResult> MaxAwaitWithCancellationAsync<TSource, TResult>(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TResult>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.MaxAwaitWithCancellationAsync(selector, cancellationToken);
	}

	public static ValueTask<TResult> MinAsync<TSource, TResult>(IAsyncEnumerable<TSource> source, Func<TSource, TResult> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.MinAsync(selector, cancellationToken);
	}

	public static ValueTask<TResult> MinAwaitAsync<TSource, TResult>(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TResult>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.MinAwaitAsync(selector, cancellationToken);
	}

	public static ValueTask<TResult> MinAwaitWithCancellationAsync<TSource, TResult>(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TResult>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.MinAwaitWithCancellationAsync(selector, cancellationToken);
	}

	public static ValueTask<int> MaxAwaitAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<int>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.MaxAwaitAsync(selector, cancellationToken);
	}

	public static ValueTask<int> MaxAwaitWithCancellationAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<int>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.MaxAwaitWithCancellationAsync(selector, cancellationToken);
	}

	public static ValueTask<int?> MaxAwaitAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<int?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.MaxAwaitAsync(selector, cancellationToken);
	}

	public static ValueTask<int?> MaxAwaitWithCancellationAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<int?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.MaxAwaitWithCancellationAsync(selector, cancellationToken);
	}

	public static ValueTask<long> MaxAwaitAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<long>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.MaxAwaitAsync(selector, cancellationToken);
	}

	public static ValueTask<long> MaxAwaitWithCancellationAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<long>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.MaxAwaitWithCancellationAsync(selector, cancellationToken);
	}

	public static ValueTask<long?> MaxAwaitAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<long?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.MaxAwaitAsync(selector, cancellationToken);
	}

	public static ValueTask<long?> MaxAwaitWithCancellationAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<long?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.MaxAwaitWithCancellationAsync(selector, cancellationToken);
	}

	public static ValueTask<float> MaxAwaitAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<float>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.MaxAwaitAsync(selector, cancellationToken);
	}

	public static ValueTask<float> MaxAwaitWithCancellationAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<float>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.MaxAwaitWithCancellationAsync(selector, cancellationToken);
	}

	public static ValueTask<float?> MaxAwaitAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<float?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.MaxAwaitAsync(selector, cancellationToken);
	}

	public static ValueTask<float?> MaxAwaitWithCancellationAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<float?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.MaxAwaitWithCancellationAsync(selector, cancellationToken);
	}

	public static ValueTask<double> MaxAwaitAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<double>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.MaxAwaitAsync(selector, cancellationToken);
	}

	public static ValueTask<double> MaxAwaitWithCancellationAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<double>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.MaxAwaitWithCancellationAsync(selector, cancellationToken);
	}

	public static ValueTask<double?> MaxAwaitAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<double?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.MaxAwaitAsync(selector, cancellationToken);
	}

	public static ValueTask<double?> MaxAwaitWithCancellationAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<double?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.MaxAwaitWithCancellationAsync(selector, cancellationToken);
	}

	public static ValueTask<decimal> MaxAwaitAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<decimal>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.MaxAwaitAsync(selector, cancellationToken);
	}

	public static ValueTask<decimal> MaxAwaitWithCancellationAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<decimal>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.MaxAwaitWithCancellationAsync(selector, cancellationToken);
	}

	public static ValueTask<decimal?> MaxAwaitAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<decimal?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.MaxAwaitAsync(selector, cancellationToken);
	}

	public static ValueTask<decimal?> MaxAwaitWithCancellationAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<decimal?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.MaxAwaitWithCancellationAsync(selector, cancellationToken);
	}

	public static ValueTask<int> MinAwaitAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<int>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.MinAwaitAsync(selector, cancellationToken);
	}

	public static ValueTask<int> MinAwaitWithCancellationAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<int>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.MinAwaitWithCancellationAsync(selector, cancellationToken);
	}

	public static ValueTask<int?> MinAwaitAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<int?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.MinAwaitAsync(selector, cancellationToken);
	}

	public static ValueTask<int?> MinAwaitWithCancellationAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<int?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.MinAwaitWithCancellationAsync(selector, cancellationToken);
	}

	public static ValueTask<long> MinAwaitAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<long>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.MinAwaitAsync(selector, cancellationToken);
	}

	public static ValueTask<long> MinAwaitWithCancellationAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<long>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.MinAwaitWithCancellationAsync(selector, cancellationToken);
	}

	public static ValueTask<long?> MinAwaitAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<long?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.MinAwaitAsync(selector, cancellationToken);
	}

	public static ValueTask<long?> MinAwaitWithCancellationAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<long?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.MinAwaitWithCancellationAsync(selector, cancellationToken);
	}

	public static ValueTask<float> MinAwaitAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<float>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.MinAwaitAsync(selector, cancellationToken);
	}

	public static ValueTask<float> MinAwaitWithCancellationAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<float>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.MinAwaitWithCancellationAsync(selector, cancellationToken);
	}

	public static ValueTask<float?> MinAwaitAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<float?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.MinAwaitAsync(selector, cancellationToken);
	}

	public static ValueTask<float?> MinAwaitWithCancellationAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<float?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.MinAwaitWithCancellationAsync(selector, cancellationToken);
	}

	public static ValueTask<double> MinAwaitAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<double>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.MinAwaitAsync(selector, cancellationToken);
	}

	public static ValueTask<double> MinAwaitWithCancellationAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<double>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.MinAwaitWithCancellationAsync(selector, cancellationToken);
	}

	public static ValueTask<double?> MinAwaitAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<double?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.MinAwaitAsync(selector, cancellationToken);
	}

	public static ValueTask<double?> MinAwaitWithCancellationAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<double?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.MinAwaitWithCancellationAsync(selector, cancellationToken);
	}

	public static ValueTask<decimal> MinAwaitAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<decimal>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.MinAwaitAsync(selector, cancellationToken);
	}

	public static ValueTask<decimal> MinAwaitWithCancellationAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<decimal>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.MinAwaitWithCancellationAsync(selector, cancellationToken);
	}

	public static ValueTask<decimal?> MinAwaitAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<decimal?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.MinAwaitAsync(selector, cancellationToken);
	}

	public static ValueTask<decimal?> MinAwaitWithCancellationAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<decimal?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.MinAwaitWithCancellationAsync(selector, cancellationToken);
	}

	public static IOrderedAsyncEnumerable<TSource> OrderByAwait<TSource, TKey>(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector)
	{
		return source.OrderByAwait(keySelector);
	}

	public static IOrderedAsyncEnumerable<TSource> OrderByAwaitWithCancellation<TSource, TKey>(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector)
	{
		return source.OrderByAwaitWithCancellation(keySelector);
	}

	public static IOrderedAsyncEnumerable<TSource> OrderByAwait<TSource, TKey>(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector, IComparer<TKey> comparer)
	{
		return source.OrderByAwait(keySelector, comparer);
	}

	public static IOrderedAsyncEnumerable<TSource> OrderByAwaitWithCancellation<TSource, TKey>(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, IComparer<TKey> comparer)
	{
		return source.OrderByAwaitWithCancellation(keySelector, comparer);
	}

	public static IOrderedAsyncEnumerable<TSource> OrderByDescendingAwait<TSource, TKey>(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector)
	{
		return source.OrderByDescendingAwait(keySelector);
	}

	public static IOrderedAsyncEnumerable<TSource> OrderByDescendingAwaitWithCancellation<TSource, TKey>(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector)
	{
		return source.OrderByDescendingAwaitWithCancellation(keySelector);
	}

	public static IOrderedAsyncEnumerable<TSource> OrderByDescendingAwait<TSource, TKey>(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector, IComparer<TKey> comparer)
	{
		return source.OrderByDescendingAwait(keySelector, comparer);
	}

	public static IOrderedAsyncEnumerable<TSource> OrderByDescendingAwaitWithCancellation<TSource, TKey>(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, IComparer<TKey> comparer)
	{
		return source.OrderByDescendingAwaitWithCancellation(keySelector, comparer);
	}

	public static IOrderedAsyncEnumerable<TSource> ThenByAwait<TSource, TKey>(IOrderedAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector)
	{
		return source.ThenByAwait(keySelector);
	}

	public static IOrderedAsyncEnumerable<TSource> ThenByAwaitWithCancellation<TSource, TKey>(IOrderedAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector)
	{
		return source.ThenByAwaitWithCancellation(keySelector);
	}

	public static IOrderedAsyncEnumerable<TSource> ThenByAwait<TSource, TKey>(IOrderedAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector, IComparer<TKey> comparer)
	{
		return source.ThenByAwait(keySelector, comparer);
	}

	public static IOrderedAsyncEnumerable<TSource> ThenByAwaitWithCancellation<TSource, TKey>(IOrderedAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, IComparer<TKey> comparer)
	{
		return source.ThenByAwaitWithCancellation(keySelector, comparer);
	}

	public static IOrderedAsyncEnumerable<TSource> ThenByDescendingAwait<TSource, TKey>(IOrderedAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector)
	{
		return source.ThenByDescendingAwait(keySelector);
	}

	public static IOrderedAsyncEnumerable<TSource> ThenByDescendingAwaitWithCancellation<TSource, TKey>(IOrderedAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector)
	{
		return source.ThenByDescendingAwaitWithCancellation(keySelector);
	}

	public static IOrderedAsyncEnumerable<TSource> ThenByDescendingAwait<TSource, TKey>(IOrderedAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector, IComparer<TKey> comparer)
	{
		return source.ThenByDescendingAwait(keySelector, comparer);
	}

	public static IOrderedAsyncEnumerable<TSource> ThenByDescendingAwaitWithCancellation<TSource, TKey>(IOrderedAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, IComparer<TKey> comparer)
	{
		return source.ThenByDescendingAwaitWithCancellation(keySelector, comparer);
	}

	public static IAsyncEnumerable<TResult> SelectAwait<TSource, TResult>(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TResult>> selector)
	{
		return source.SelectAwait(selector);
	}

	public static IAsyncEnumerable<TResult> SelectAwaitWithCancellation<TSource, TResult>(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TResult>> selector)
	{
		return source.SelectAwaitWithCancellation(selector);
	}

	public static IAsyncEnumerable<TResult> SelectAwait<TSource, TResult>(IAsyncEnumerable<TSource> source, Func<TSource, int, ValueTask<TResult>> selector)
	{
		return source.SelectAwait(selector);
	}

	public static IAsyncEnumerable<TResult> SelectAwaitWithCancellation<TSource, TResult>(IAsyncEnumerable<TSource> source, Func<TSource, int, CancellationToken, ValueTask<TResult>> selector)
	{
		return source.SelectAwaitWithCancellation(selector);
	}

	public static IAsyncEnumerable<TResult> SelectManyAwait<TSource, TResult>(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<IAsyncEnumerable<TResult>>> selector)
	{
		return source.SelectManyAwait(selector);
	}

	public static IAsyncEnumerable<TResult> SelectManyAwaitWithCancellation<TSource, TResult>(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<IAsyncEnumerable<TResult>>> selector)
	{
		return source.SelectManyAwaitWithCancellation(selector);
	}

	public static IAsyncEnumerable<TResult> SelectManyAwait<TSource, TResult>(IAsyncEnumerable<TSource> source, Func<TSource, int, ValueTask<IAsyncEnumerable<TResult>>> selector)
	{
		return source.SelectManyAwait(selector);
	}

	public static IAsyncEnumerable<TResult> SelectManyAwaitWithCancellation<TSource, TResult>(IAsyncEnumerable<TSource> source, Func<TSource, int, CancellationToken, ValueTask<IAsyncEnumerable<TResult>>> selector)
	{
		return source.SelectManyAwaitWithCancellation(selector);
	}

	public static IAsyncEnumerable<TResult> SelectManyAwait<TSource, TCollection, TResult>(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<IAsyncEnumerable<TCollection>>> collectionSelector, Func<TSource, TCollection, ValueTask<TResult>> resultSelector)
	{
		return source.SelectManyAwait(collectionSelector, resultSelector);
	}

	public static IAsyncEnumerable<TResult> SelectManyAwaitWithCancellation<TSource, TCollection, TResult>(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<IAsyncEnumerable<TCollection>>> collectionSelector, Func<TSource, TCollection, CancellationToken, ValueTask<TResult>> resultSelector)
	{
		return source.SelectManyAwaitWithCancellation(collectionSelector, resultSelector);
	}

	public static IAsyncEnumerable<TResult> SelectManyAwait<TSource, TCollection, TResult>(IAsyncEnumerable<TSource> source, Func<TSource, int, ValueTask<IAsyncEnumerable<TCollection>>> collectionSelector, Func<TSource, TCollection, ValueTask<TResult>> resultSelector)
	{
		return source.SelectManyAwait(collectionSelector, resultSelector);
	}

	public static IAsyncEnumerable<TResult> SelectManyAwaitWithCancellation<TSource, TCollection, TResult>(IAsyncEnumerable<TSource> source, Func<TSource, int, CancellationToken, ValueTask<IAsyncEnumerable<TCollection>>> collectionSelector, Func<TSource, TCollection, CancellationToken, ValueTask<TResult>> resultSelector)
	{
		return source.SelectManyAwaitWithCancellation(collectionSelector, resultSelector);
	}

	public static ValueTask<TSource> SingleAwaitAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.SingleAwaitAsync(predicate, cancellationToken);
	}

	public static ValueTask<TSource> SingleAwaitWithCancellationAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.SingleAwaitWithCancellationAsync(predicate, cancellationToken);
	}

	public static ValueTask<TSource?> SingleOrDefaultAwaitAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.SingleOrDefaultAwaitAsync(predicate, cancellationToken);
	}

	public static ValueTask<TSource?> SingleOrDefaultAwaitWithCancellationAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.SingleOrDefaultAwaitWithCancellationAsync(predicate, cancellationToken);
	}

	public static IAsyncEnumerable<TSource> SkipWhileAwait<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<bool>> predicate)
	{
		return source.SkipWhileAwait(predicate);
	}

	public static IAsyncEnumerable<TSource> SkipWhileAwaitWithCancellation<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<bool>> predicate)
	{
		return source.SkipWhileAwaitWithCancellation(predicate);
	}

	public static IAsyncEnumerable<TSource> SkipWhileAwait<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, int, ValueTask<bool>> predicate)
	{
		return source.SkipWhileAwait(predicate);
	}

	public static IAsyncEnumerable<TSource> SkipWhileAwaitWithCancellation<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, int, CancellationToken, ValueTask<bool>> predicate)
	{
		return source.SkipWhileAwaitWithCancellation(predicate);
	}

	public static ValueTask<int> SumAwaitAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<int>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.SumAwaitAsync(selector, cancellationToken);
	}

	public static ValueTask<int> SumAwaitWithCancellationAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<int>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.SumAwaitWithCancellationAsync(selector, cancellationToken);
	}

	public static ValueTask<long> SumAwaitAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<long>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.SumAwaitAsync(selector, cancellationToken);
	}

	public static ValueTask<long> SumAwaitWithCancellationAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<long>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.SumAwaitWithCancellationAsync(selector, cancellationToken);
	}

	public static ValueTask<float> SumAwaitAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<float>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.SumAwaitAsync(selector, cancellationToken);
	}

	public static ValueTask<float> SumAwaitWithCancellationAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<float>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.SumAwaitWithCancellationAsync(selector, cancellationToken);
	}

	public static ValueTask<double> SumAwaitAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<double>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.SumAwaitAsync(selector, cancellationToken);
	}

	public static ValueTask<double> SumAwaitWithCancellationAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<double>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.SumAwaitWithCancellationAsync(selector, cancellationToken);
	}

	public static ValueTask<decimal> SumAwaitAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<decimal>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.SumAwaitAsync(selector, cancellationToken);
	}

	public static ValueTask<decimal> SumAwaitWithCancellationAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<decimal>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.SumAwaitWithCancellationAsync(selector, cancellationToken);
	}

	public static ValueTask<int?> SumAwaitAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<int?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.SumAwaitAsync(selector, cancellationToken);
	}

	public static ValueTask<int?> SumAwaitWithCancellationAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<int?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.SumAwaitWithCancellationAsync(selector, cancellationToken);
	}

	public static ValueTask<long?> SumAwaitAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<long?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.SumAwaitAsync(selector, cancellationToken);
	}

	public static ValueTask<long?> SumAwaitWithCancellationAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<long?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.SumAwaitWithCancellationAsync(selector, cancellationToken);
	}

	public static ValueTask<float?> SumAwaitAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<float?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.SumAwaitAsync(selector, cancellationToken);
	}

	public static ValueTask<float?> SumAwaitWithCancellationAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<float?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.SumAwaitWithCancellationAsync(selector, cancellationToken);
	}

	public static ValueTask<double?> SumAwaitAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<double?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.SumAwaitAsync(selector, cancellationToken);
	}

	public static ValueTask<double?> SumAwaitWithCancellationAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<double?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.SumAwaitWithCancellationAsync(selector, cancellationToken);
	}

	public static ValueTask<decimal?> SumAwaitAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<decimal?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.SumAwaitAsync(selector, cancellationToken);
	}

	public static ValueTask<decimal?> SumAwaitWithCancellationAsync<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<decimal?>> selector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.SumAwaitWithCancellationAsync(selector, cancellationToken);
	}

	public static IAsyncEnumerable<TSource> TakeWhileAwait<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<bool>> predicate)
	{
		return source.TakeWhileAwait(predicate);
	}

	public static IAsyncEnumerable<TSource> TakeWhileAwaitWithCancellation<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<bool>> predicate)
	{
		return source.TakeWhileAwaitWithCancellation(predicate);
	}

	public static IAsyncEnumerable<TSource> TakeWhileAwait<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, int, ValueTask<bool>> predicate)
	{
		return source.TakeWhileAwait(predicate);
	}

	public static IAsyncEnumerable<TSource> TakeWhileAwaitWithCancellation<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, int, CancellationToken, ValueTask<bool>> predicate)
	{
		return source.TakeWhileAwaitWithCancellation(predicate);
	}

	public static ValueTask<Dictionary<TKey, TSource>> ToDictionaryAwaitAsync<TSource, TKey>(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector, CancellationToken cancellationToken = default(CancellationToken)) where TKey : notnull
	{
		return source.ToDictionaryAwaitAsync(keySelector, cancellationToken);
	}

	public static ValueTask<Dictionary<TKey, TSource>> ToDictionaryAwaitAsync<TSource, TKey>(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector, IEqualityComparer<TKey>? comparer, CancellationToken cancellationToken = default(CancellationToken)) where TKey : notnull
	{
		return source.ToDictionaryAwaitAsync(keySelector, comparer, cancellationToken);
	}

	public static ValueTask<Dictionary<TKey, TSource>> ToDictionaryAwaitWithCancellationAsync<TSource, TKey>(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, CancellationToken cancellationToken = default(CancellationToken)) where TKey : notnull
	{
		return source.ToDictionaryAwaitWithCancellationAsync(keySelector, cancellationToken);
	}

	public static ValueTask<Dictionary<TKey, TSource>> ToDictionaryAwaitWithCancellationAsync<TSource, TKey>(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, IEqualityComparer<TKey>? comparer, CancellationToken cancellationToken = default(CancellationToken)) where TKey : notnull
	{
		return source.ToDictionaryAwaitWithCancellationAsync(keySelector, comparer, cancellationToken);
	}

	public static ValueTask<Dictionary<TKey, TElement>> ToDictionaryAwaitAsync<TSource, TKey, TElement>(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector, Func<TSource, ValueTask<TElement>> elementSelector, CancellationToken cancellationToken = default(CancellationToken)) where TKey : notnull
	{
		return source.ToDictionaryAwaitAsync(keySelector, elementSelector, cancellationToken);
	}

	public static ValueTask<Dictionary<TKey, TElement>> ToDictionaryAwaitAsync<TSource, TKey, TElement>(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector, Func<TSource, ValueTask<TElement>> elementSelector, IEqualityComparer<TKey>? comparer, CancellationToken cancellationToken = default(CancellationToken)) where TKey : notnull
	{
		return source.ToDictionaryAwaitAsync(keySelector, elementSelector, comparer, cancellationToken);
	}

	public static ValueTask<Dictionary<TKey, TElement>> ToDictionaryAwaitWithCancellationAsync<TSource, TKey, TElement>(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, Func<TSource, CancellationToken, ValueTask<TElement>> elementSelector, CancellationToken cancellationToken = default(CancellationToken)) where TKey : notnull
	{
		return source.ToDictionaryAwaitWithCancellationAsync(keySelector, elementSelector, cancellationToken);
	}

	public static ValueTask<Dictionary<TKey, TElement>> ToDictionaryAwaitWithCancellationAsync<TSource, TKey, TElement>(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, Func<TSource, CancellationToken, ValueTask<TElement>> elementSelector, IEqualityComparer<TKey>? comparer, CancellationToken cancellationToken = default(CancellationToken)) where TKey : notnull
	{
		return source.ToDictionaryAwaitWithCancellationAsync(keySelector, elementSelector, comparer, cancellationToken);
	}

	public static IEnumerable<TSource> ToEnumerable<TSource>(IAsyncEnumerable<TSource> source)
	{
		return source.ToEnumerable();
	}

	public static ValueTask<ILookup<TKey, TSource>> ToLookupAwaitAsync<TSource, TKey>(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.ToLookupAwaitAsync(keySelector, cancellationToken);
	}

	public static ValueTask<ILookup<TKey, TSource>> ToLookupAwaitAsync<TSource, TKey>(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector, IEqualityComparer<TKey>? comparer, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.ToLookupAwaitAsync(keySelector, comparer, cancellationToken);
	}

	public static ValueTask<ILookup<TKey, TSource>> ToLookupAwaitWithCancellationAsync<TSource, TKey>(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.ToLookupAwaitWithCancellationAsync(keySelector, cancellationToken);
	}

	public static ValueTask<ILookup<TKey, TSource>> ToLookupAwaitWithCancellationAsync<TSource, TKey>(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, IEqualityComparer<TKey>? comparer, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.ToLookupAwaitWithCancellationAsync(keySelector, comparer, cancellationToken);
	}

	public static ValueTask<ILookup<TKey, TElement>> ToLookupAwaitAsync<TSource, TKey, TElement>(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector, Func<TSource, ValueTask<TElement>> elementSelector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.ToLookupAwaitAsync(keySelector, elementSelector, cancellationToken);
	}

	public static ValueTask<ILookup<TKey, TElement>> ToLookupAwaitAsync<TSource, TKey, TElement>(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<TKey>> keySelector, Func<TSource, ValueTask<TElement>> elementSelector, IEqualityComparer<TKey>? comparer, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.ToLookupAwaitAsync(keySelector, elementSelector, comparer, cancellationToken);
	}

	public static ValueTask<ILookup<TKey, TElement>> ToLookupAwaitWithCancellationAsync<TSource, TKey, TElement>(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, Func<TSource, CancellationToken, ValueTask<TElement>> elementSelector, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.ToLookupAwaitWithCancellationAsync(keySelector, elementSelector, cancellationToken);
	}

	public static ValueTask<ILookup<TKey, TElement>> ToLookupAwaitWithCancellationAsync<TSource, TKey, TElement>(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<TKey>> keySelector, Func<TSource, CancellationToken, ValueTask<TElement>> elementSelector, IEqualityComparer<TKey>? comparer, CancellationToken cancellationToken = default(CancellationToken))
	{
		return source.ToLookupAwaitWithCancellationAsync(keySelector, elementSelector, comparer, cancellationToken);
	}

	public static IAsyncEnumerable<TSource> WhereAwait<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, ValueTask<bool>> predicate)
	{
		return source.WhereAwait(predicate);
	}

	public static IAsyncEnumerable<TSource> WhereAwaitWithCancellation<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, CancellationToken, ValueTask<bool>> predicate)
	{
		return source.WhereAwaitWithCancellation(predicate);
	}

	public static IAsyncEnumerable<TSource> WhereAwait<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, int, ValueTask<bool>> predicate)
	{
		return source.WhereAwait(predicate);
	}

	public static IAsyncEnumerable<TSource> WhereAwaitWithCancellation<TSource>(IAsyncEnumerable<TSource> source, Func<TSource, int, CancellationToken, ValueTask<bool>> predicate)
	{
		return source.WhereAwaitWithCancellation(predicate);
	}

	public static IAsyncEnumerable<TResult> ZipAwait<TFirst, TSecond, TResult>(IAsyncEnumerable<TFirst> first, IAsyncEnumerable<TSecond> second, Func<TFirst, TSecond, ValueTask<TResult>> selector)
	{
		return first.ZipAwait(second, selector);
	}

	public static IAsyncEnumerable<TResult> ZipAwaitWithCancellation<TFirst, TSecond, TResult>(IAsyncEnumerable<TFirst> first, IAsyncEnumerable<TSecond> second, Func<TFirst, TSecond, CancellationToken, ValueTask<TResult>> selector)
	{
		return first.ZipAwaitWithCancellation(second, selector);
	}
}
