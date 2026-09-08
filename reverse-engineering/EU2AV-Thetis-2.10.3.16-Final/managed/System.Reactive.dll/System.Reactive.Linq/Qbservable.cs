using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Concurrency;
using System.Reactive.Joins;
using System.Reactive.Subjects;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace System.Reactive.Linq;

[ExcludeFromCodeCoverage]
public static class Qbservable
{
	private static IQbservableProvider? s_provider;

	public static IQbservableProvider Provider
	{
		get
		{
			if (s_provider == null)
			{
				s_provider = new ObservableQueryProvider();
			}
			return s_provider;
		}
	}

	public static IQbservable<TSource> AsQbservable<TSource>(this IObservable<TSource> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return new ObservableQuery<TSource>(source);
	}

	public static IObservable<TSource> AsObservable<TSource>(this IQbservable<TSource> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source;
	}

	public static IQbservable<TSource> ToQbservable<TSource>(this IQueryable<TSource> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return ((IQbservableProvider)source.Provider).CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression));
	}

	public static IQbservable<TSource> ToQbservable<TSource>(this IQueryable<TSource> source, IScheduler scheduler)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return ((IQbservableProvider)source.Provider).CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, Expression.Constant(scheduler)));
	}

	internal static Expression GetSourceExpression<TSource>(IObservable<TSource> source)
	{
		if (source is IQbservable<TSource> qbservable)
		{
			return qbservable.Expression;
		}
		return Expression.Constant(source, typeof(IObservable<TSource>));
	}

	internal static Expression GetSourceExpression<TSource>(IEnumerable<TSource> source)
	{
		if (source is IQueryable<TSource> queryable)
		{
			return queryable.Expression;
		}
		return Expression.Constant(source, typeof(IEnumerable<TSource>));
	}

	internal static Expression GetSourceExpression<TSource>(IObservable<TSource>[] sources)
	{
		return Expression.NewArrayInit(typeof(IObservable<TSource>), sources.Select((IObservable<TSource> source) => GetSourceExpression(source)));
	}

	internal static Expression GetSourceExpression<TSource>(IEnumerable<TSource>[] sources)
	{
		return Expression.NewArrayInit(typeof(IEnumerable<TSource>), sources.Select((IEnumerable<TSource> source) => GetSourceExpression(source)));
	}

	internal static MethodInfo InfoOf<R>(Expression<Func<R>> f)
	{
		return ((MethodCallExpression)f.Body).Method;
	}

	public static IQbservable<TSource> Aggregate<TSource>(this IQbservable<TSource> source, Expression<Func<TSource, TSource, TSource>> accumulator)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (accumulator == null)
		{
			throw new ArgumentNullException("accumulator");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, accumulator));
	}

	public static IQbservable<TAccumulate> Aggregate<TSource, TAccumulate>(this IQbservable<TSource> source, TAccumulate seed, Expression<Func<TAccumulate, TSource, TAccumulate>> accumulator)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (accumulator == null)
		{
			throw new ArgumentNullException("accumulator");
		}
		return source.Provider.CreateQuery<TAccumulate>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TAccumulate)), source.Expression, Expression.Constant(seed, typeof(TAccumulate)), accumulator));
	}

	public static IQbservable<TResult> Aggregate<TSource, TAccumulate, TResult>(this IQbservable<TSource> source, TAccumulate seed, Expression<Func<TAccumulate, TSource, TAccumulate>> accumulator, Expression<Func<TAccumulate, TResult>> resultSelector)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (accumulator == null)
		{
			throw new ArgumentNullException("accumulator");
		}
		if (resultSelector == null)
		{
			throw new ArgumentNullException("resultSelector");
		}
		return source.Provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TAccumulate), typeof(TResult)), source.Expression, Expression.Constant(seed, typeof(TAccumulate)), accumulator, resultSelector));
	}

	public static IQbservable<bool> All<TSource>(this IQbservable<TSource> source, Expression<Func<TSource, bool>> predicate)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (predicate == null)
		{
			throw new ArgumentNullException("predicate");
		}
		return source.Provider.CreateQuery<bool>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, predicate));
	}

	public static IQbservable<TSource> Amb<TSource>(this IQbservable<TSource> first, IObservable<TSource> second)
	{
		if (first == null)
		{
			throw new ArgumentNullException("first");
		}
		if (second == null)
		{
			throw new ArgumentNullException("second");
		}
		return first.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), first.Expression, GetSourceExpression(second)));
	}

	public static IQbservable<TSource> Amb<TSource>(this IQbservableProvider provider, params IObservable<TSource>[] sources)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (sources == null)
		{
			throw new ArgumentNullException("sources");
		}
		return provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), Expression.Constant(provider, typeof(IQbservableProvider)), GetSourceExpression(sources)));
	}

	public static IQbservable<TSource> Amb<TSource>(this IQbservableProvider provider, IEnumerable<IObservable<TSource>> sources)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (sources == null)
		{
			throw new ArgumentNullException("sources");
		}
		return provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), Expression.Constant(provider, typeof(IQbservableProvider)), GetSourceExpression(sources)));
	}

	public static IQbservable<bool> Any<TSource>(this IQbservable<TSource> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<bool>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression));
	}

	public static IQbservable<bool> Any<TSource>(this IQbservable<TSource> source, Expression<Func<TSource, bool>> predicate)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (predicate == null)
		{
			throw new ArgumentNullException("predicate");
		}
		return source.Provider.CreateQuery<bool>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, predicate));
	}

	public static IQbservable<TSource> Append<TSource>(this IQbservable<TSource> source, TSource value)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, Expression.Constant(value, typeof(TSource))));
	}

	public static IQbservable<TSource> Append<TSource>(this IQbservable<TSource> source, TSource value, IScheduler scheduler)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, Expression.Constant(value, typeof(TSource)), Expression.Constant(scheduler, typeof(IScheduler))));
	}

	public static IQbservable<TSource> AutoConnect<TSource>(this IQbservableProvider provider, IConnectableObservable<TSource> source, int minObservers, Expression<Action<IDisposable>> onConnect)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (onConnect == null)
		{
			throw new ArgumentNullException("onConnect");
		}
		return provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), Expression.Constant(provider, typeof(IQbservableProvider)), Expression.Constant(source, typeof(IConnectableObservable<TSource>)), Expression.Constant(minObservers, typeof(int)), onConnect));
	}

	public static IQbservable<decimal> Average(this IQbservable<decimal> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<decimal>(Expression.Call(null, (MethodInfo)MethodBase.GetCurrentMethod(), source.Expression));
	}

	public static IQbservable<double> Average(this IQbservable<double> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<double>(Expression.Call(null, (MethodInfo)MethodBase.GetCurrentMethod(), source.Expression));
	}

	public static IQbservable<double> Average(this IQbservable<int> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<double>(Expression.Call(null, (MethodInfo)MethodBase.GetCurrentMethod(), source.Expression));
	}

	public static IQbservable<double> Average(this IQbservable<long> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<double>(Expression.Call(null, (MethodInfo)MethodBase.GetCurrentMethod(), source.Expression));
	}

	public static IQbservable<decimal?> Average(this IQbservable<decimal?> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<decimal?>(Expression.Call(null, (MethodInfo)MethodBase.GetCurrentMethod(), source.Expression));
	}

	public static IQbservable<double?> Average(this IQbservable<double?> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<double?>(Expression.Call(null, (MethodInfo)MethodBase.GetCurrentMethod(), source.Expression));
	}

	public static IQbservable<double?> Average(this IQbservable<int?> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<double?>(Expression.Call(null, (MethodInfo)MethodBase.GetCurrentMethod(), source.Expression));
	}

	public static IQbservable<double?> Average(this IQbservable<long?> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<double?>(Expression.Call(null, (MethodInfo)MethodBase.GetCurrentMethod(), source.Expression));
	}

	public static IQbservable<float?> Average(this IQbservable<float?> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<float?>(Expression.Call(null, (MethodInfo)MethodBase.GetCurrentMethod(), source.Expression));
	}

	public static IQbservable<float> Average(this IQbservable<float> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<float>(Expression.Call(null, (MethodInfo)MethodBase.GetCurrentMethod(), source.Expression));
	}

	public static IQbservable<decimal> Average<TSource>(this IQbservable<TSource> source, Expression<Func<TSource, decimal>> selector)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		return source.Provider.CreateQuery<decimal>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, selector));
	}

	public static IQbservable<double> Average<TSource>(this IQbservable<TSource> source, Expression<Func<TSource, double>> selector)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		return source.Provider.CreateQuery<double>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, selector));
	}

	public static IQbservable<float> Average<TSource>(this IQbservable<TSource> source, Expression<Func<TSource, float>> selector)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		return source.Provider.CreateQuery<float>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, selector));
	}

	public static IQbservable<double> Average<TSource>(this IQbservable<TSource> source, Expression<Func<TSource, int>> selector)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		return source.Provider.CreateQuery<double>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, selector));
	}

	public static IQbservable<double> Average<TSource>(this IQbservable<TSource> source, Expression<Func<TSource, long>> selector)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		return source.Provider.CreateQuery<double>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, selector));
	}

	public static IQbservable<decimal?> Average<TSource>(this IQbservable<TSource> source, Expression<Func<TSource, decimal?>> selector)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		return source.Provider.CreateQuery<decimal?>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, selector));
	}

	public static IQbservable<double?> Average<TSource>(this IQbservable<TSource> source, Expression<Func<TSource, double?>> selector)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		return source.Provider.CreateQuery<double?>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, selector));
	}

	public static IQbservable<float?> Average<TSource>(this IQbservable<TSource> source, Expression<Func<TSource, float?>> selector)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		return source.Provider.CreateQuery<float?>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, selector));
	}

	public static IQbservable<double?> Average<TSource>(this IQbservable<TSource> source, Expression<Func<TSource, int?>> selector)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		return source.Provider.CreateQuery<double?>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, selector));
	}

	public static IQbservable<double?> Average<TSource>(this IQbservable<TSource> source, Expression<Func<TSource, long?>> selector)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		return source.Provider.CreateQuery<double?>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, selector));
	}

	public static IQbservable<IList<TSource>> Buffer<TSource>(this IQbservable<TSource> source, int count)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<IList<TSource>>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, Expression.Constant(count, typeof(int))));
	}

	public static IQbservable<IList<TSource>> Buffer<TSource>(this IQbservable<TSource> source, int count, int skip)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<IList<TSource>>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, Expression.Constant(count, typeof(int)), Expression.Constant(skip, typeof(int))));
	}

	public static IQbservable<IList<TSource>> Buffer<TSource>(this IQbservable<TSource> source, TimeSpan timeSpan)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<IList<TSource>>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, Expression.Constant(timeSpan, typeof(TimeSpan))));
	}

	public static IQbservable<IList<TSource>> Buffer<TSource>(this IQbservable<TSource> source, TimeSpan timeSpan, int count)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<IList<TSource>>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, Expression.Constant(timeSpan, typeof(TimeSpan)), Expression.Constant(count, typeof(int))));
	}

	public static IQbservable<IList<TSource>> Buffer<TSource>(this IQbservable<TSource> source, TimeSpan timeSpan, int count, IScheduler scheduler)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return source.Provider.CreateQuery<IList<TSource>>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, Expression.Constant(timeSpan, typeof(TimeSpan)), Expression.Constant(count, typeof(int)), Expression.Constant(scheduler, typeof(IScheduler))));
	}

	public static IQbservable<IList<TSource>> Buffer<TSource>(this IQbservable<TSource> source, TimeSpan timeSpan, IScheduler scheduler)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return source.Provider.CreateQuery<IList<TSource>>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, Expression.Constant(timeSpan, typeof(TimeSpan)), Expression.Constant(scheduler, typeof(IScheduler))));
	}

	public static IQbservable<IList<TSource>> Buffer<TSource>(this IQbservable<TSource> source, TimeSpan timeSpan, TimeSpan timeShift)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<IList<TSource>>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, Expression.Constant(timeSpan, typeof(TimeSpan)), Expression.Constant(timeShift, typeof(TimeSpan))));
	}

	public static IQbservable<IList<TSource>> Buffer<TSource>(this IQbservable<TSource> source, TimeSpan timeSpan, TimeSpan timeShift, IScheduler scheduler)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return source.Provider.CreateQuery<IList<TSource>>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, Expression.Constant(timeSpan, typeof(TimeSpan)), Expression.Constant(timeShift, typeof(TimeSpan)), Expression.Constant(scheduler, typeof(IScheduler))));
	}

	public static IQbservable<IList<TSource>> Buffer<TSource, TBufferBoundary>(this IQbservable<TSource> source, IObservable<TBufferBoundary> bufferBoundaries)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (bufferBoundaries == null)
		{
			throw new ArgumentNullException("bufferBoundaries");
		}
		return source.Provider.CreateQuery<IList<TSource>>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TBufferBoundary)), source.Expression, GetSourceExpression(bufferBoundaries)));
	}

	public static IQbservable<IList<TSource>> Buffer<TSource, TBufferClosing>(this IQbservable<TSource> source, Expression<Func<IObservable<TBufferClosing>>> bufferClosingSelector)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (bufferClosingSelector == null)
		{
			throw new ArgumentNullException("bufferClosingSelector");
		}
		return source.Provider.CreateQuery<IList<TSource>>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TBufferClosing)), source.Expression, bufferClosingSelector));
	}

	public static IQbservable<IList<TSource>> Buffer<TSource, TBufferOpening, TBufferClosing>(this IQbservable<TSource> source, IObservable<TBufferOpening> bufferOpenings, Expression<Func<TBufferOpening, IObservable<TBufferClosing>>> bufferClosingSelector)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (bufferOpenings == null)
		{
			throw new ArgumentNullException("bufferOpenings");
		}
		if (bufferClosingSelector == null)
		{
			throw new ArgumentNullException("bufferClosingSelector");
		}
		return source.Provider.CreateQuery<IList<TSource>>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TBufferOpening), typeof(TBufferClosing)), source.Expression, GetSourceExpression(bufferOpenings), bufferClosingSelector));
	}

	public static IQbservable<TSource> ResetExceptionDispatchState<TSource>(this IQbservable<TSource> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression));
	}

	public static IQbservable<TResult> Case<TValue, TResult>(this IQbservableProvider provider, Expression<Func<TValue>> selector, IDictionary<TValue, IObservable<TResult>> sources) where TValue : notnull
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		if (sources == null)
		{
			throw new ArgumentNullException("sources");
		}
		return provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TValue), typeof(TResult)), Expression.Constant(provider, typeof(IQbservableProvider)), selector, Expression.Constant(sources, typeof(IDictionary<TValue, IObservable<TResult>>))));
	}

	public static IQbservable<TResult> Case<TValue, TResult>(this IQbservableProvider provider, Expression<Func<TValue>> selector, IDictionary<TValue, IObservable<TResult>> sources, IObservable<TResult> defaultSource) where TValue : notnull
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		if (sources == null)
		{
			throw new ArgumentNullException("sources");
		}
		if (defaultSource == null)
		{
			throw new ArgumentNullException("defaultSource");
		}
		return provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TValue), typeof(TResult)), Expression.Constant(provider, typeof(IQbservableProvider)), selector, Expression.Constant(sources, typeof(IDictionary<TValue, IObservable<TResult>>)), GetSourceExpression(defaultSource)));
	}

	public static IQbservable<TResult> Case<TValue, TResult>(this IQbservableProvider provider, Expression<Func<TValue>> selector, IDictionary<TValue, IObservable<TResult>> sources, IScheduler scheduler) where TValue : notnull
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		if (sources == null)
		{
			throw new ArgumentNullException("sources");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TValue), typeof(TResult)), Expression.Constant(provider, typeof(IQbservableProvider)), selector, Expression.Constant(sources, typeof(IDictionary<TValue, IObservable<TResult>>)), Expression.Constant(scheduler, typeof(IScheduler))));
	}

	public static IQbservable<TResult> Cast<TResult>(this IQbservable<object> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TResult)), source.Expression));
	}

	public static IQbservable<TSource> Catch<TSource>(this IQbservable<TSource> first, IObservable<TSource> second)
	{
		if (first == null)
		{
			throw new ArgumentNullException("first");
		}
		if (second == null)
		{
			throw new ArgumentNullException("second");
		}
		return first.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), first.Expression, GetSourceExpression(second)));
	}

	public static IQbservable<TSource> Catch<TSource>(this IQbservableProvider provider, params IObservable<TSource>[] sources)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (sources == null)
		{
			throw new ArgumentNullException("sources");
		}
		return provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), Expression.Constant(provider, typeof(IQbservableProvider)), GetSourceExpression(sources)));
	}

	public static IQbservable<TSource> Catch<TSource>(this IQbservableProvider provider, IEnumerable<IObservable<TSource>> sources)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (sources == null)
		{
			throw new ArgumentNullException("sources");
		}
		return provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), Expression.Constant(provider, typeof(IQbservableProvider)), GetSourceExpression(sources)));
	}

	public static IQbservable<TSource> Catch<TSource, TException>(this IQbservable<TSource> source, Expression<Func<TException, IObservable<TSource>>> handler) where TException : Exception
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (handler == null)
		{
			throw new ArgumentNullException("handler");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TException)), source.Expression, handler));
	}

	public static IQueryable<IList<TSource>> Chunkify<TSource>(this IQbservable<TSource> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return ((IQueryProvider)source.Provider).CreateQuery<IList<TSource>>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression));
	}

	public static IQueryable<TResult> Collect<TSource, TResult>(this IQbservable<TSource> source, Expression<Func<TResult>> getInitialCollector, Expression<Func<TResult, TSource, TResult>> merge, Expression<Func<TResult, TResult>> getNewCollector)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (getInitialCollector == null)
		{
			throw new ArgumentNullException("getInitialCollector");
		}
		if (merge == null)
		{
			throw new ArgumentNullException("merge");
		}
		if (getNewCollector == null)
		{
			throw new ArgumentNullException("getNewCollector");
		}
		return ((IQueryProvider)source.Provider).CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TResult)), source.Expression, getInitialCollector, merge, getNewCollector));
	}

	public static IQueryable<TResult> Collect<TSource, TResult>(this IQbservable<TSource> source, Expression<Func<TResult>> newCollector, Expression<Func<TResult, TSource, TResult>> merge)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (newCollector == null)
		{
			throw new ArgumentNullException("newCollector");
		}
		if (merge == null)
		{
			throw new ArgumentNullException("merge");
		}
		return ((IQueryProvider)source.Provider).CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TResult)), source.Expression, newCollector, merge));
	}

	public static IQbservable<IList<TSource>> CombineLatest<TSource>(this IQbservableProvider provider, IEnumerable<IObservable<TSource>> sources)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (sources == null)
		{
			throw new ArgumentNullException("sources");
		}
		return provider.CreateQuery<IList<TSource>>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), Expression.Constant(provider, typeof(IQbservableProvider)), GetSourceExpression(sources)));
	}

	public static IQbservable<IList<TSource>> CombineLatest<TSource>(this IQbservableProvider provider, params IObservable<TSource>[] sources)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (sources == null)
		{
			throw new ArgumentNullException("sources");
		}
		return provider.CreateQuery<IList<TSource>>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), Expression.Constant(provider, typeof(IQbservableProvider)), GetSourceExpression(sources)));
	}

	public static IQbservable<TResult> CombineLatest<TSource, TResult>(this IQbservableProvider provider, IEnumerable<IObservable<TSource>> sources, Expression<Func<IList<TSource>, TResult>> resultSelector)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (sources == null)
		{
			throw new ArgumentNullException("sources");
		}
		if (resultSelector == null)
		{
			throw new ArgumentNullException("resultSelector");
		}
		return provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TResult)), Expression.Constant(provider, typeof(IQbservableProvider)), GetSourceExpression(sources), resultSelector));
	}

	public static IQbservable<TResult> CombineLatest<TSource1, TSource2, TResult>(this IQbservable<TSource1> first, IObservable<TSource2> second, Expression<Func<TSource1, TSource2, TResult>> resultSelector)
	{
		if (first == null)
		{
			throw new ArgumentNullException("first");
		}
		if (second == null)
		{
			throw new ArgumentNullException("second");
		}
		if (resultSelector == null)
		{
			throw new ArgumentNullException("resultSelector");
		}
		return first.Provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource1), typeof(TSource2), typeof(TResult)), first.Expression, GetSourceExpression(second), resultSelector));
	}

	public static IQbservable<TResult> CombineLatest<TSource1, TSource2, TSource3, TResult>(this IQbservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, Expression<Func<TSource1, TSource2, TSource3, TResult>> resultSelector)
	{
		if (source1 == null)
		{
			throw new ArgumentNullException("source1");
		}
		if (source2 == null)
		{
			throw new ArgumentNullException("source2");
		}
		if (source3 == null)
		{
			throw new ArgumentNullException("source3");
		}
		if (resultSelector == null)
		{
			throw new ArgumentNullException("resultSelector");
		}
		return source1.Provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource1), typeof(TSource2), typeof(TSource3), typeof(TResult)), source1.Expression, GetSourceExpression(source2), GetSourceExpression(source3), resultSelector));
	}

	public static IQbservable<TResult> CombineLatest<TSource1, TSource2, TSource3, TSource4, TResult>(this IQbservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, IObservable<TSource4> source4, Expression<Func<TSource1, TSource2, TSource3, TSource4, TResult>> resultSelector)
	{
		if (source1 == null)
		{
			throw new ArgumentNullException("source1");
		}
		if (source2 == null)
		{
			throw new ArgumentNullException("source2");
		}
		if (source3 == null)
		{
			throw new ArgumentNullException("source3");
		}
		if (source4 == null)
		{
			throw new ArgumentNullException("source4");
		}
		if (resultSelector == null)
		{
			throw new ArgumentNullException("resultSelector");
		}
		return source1.Provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource1), typeof(TSource2), typeof(TSource3), typeof(TSource4), typeof(TResult)), source1.Expression, GetSourceExpression(source2), GetSourceExpression(source3), GetSourceExpression(source4), resultSelector));
	}

	public static IQbservable<TResult> CombineLatest<TSource1, TSource2, TSource3, TSource4, TSource5, TResult>(this IQbservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, IObservable<TSource4> source4, IObservable<TSource5> source5, Expression<Func<TSource1, TSource2, TSource3, TSource4, TSource5, TResult>> resultSelector)
	{
		if (source1 == null)
		{
			throw new ArgumentNullException("source1");
		}
		if (source2 == null)
		{
			throw new ArgumentNullException("source2");
		}
		if (source3 == null)
		{
			throw new ArgumentNullException("source3");
		}
		if (source4 == null)
		{
			throw new ArgumentNullException("source4");
		}
		if (source5 == null)
		{
			throw new ArgumentNullException("source5");
		}
		if (resultSelector == null)
		{
			throw new ArgumentNullException("resultSelector");
		}
		return source1.Provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource1), typeof(TSource2), typeof(TSource3), typeof(TSource4), typeof(TSource5), typeof(TResult)), source1.Expression, GetSourceExpression(source2), GetSourceExpression(source3), GetSourceExpression(source4), GetSourceExpression(source5), resultSelector));
	}

	public static IQbservable<TResult> CombineLatest<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TResult>(this IQbservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, IObservable<TSource4> source4, IObservable<TSource5> source5, IObservable<TSource6> source6, Expression<Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TResult>> resultSelector)
	{
		if (source1 == null)
		{
			throw new ArgumentNullException("source1");
		}
		if (source2 == null)
		{
			throw new ArgumentNullException("source2");
		}
		if (source3 == null)
		{
			throw new ArgumentNullException("source3");
		}
		if (source4 == null)
		{
			throw new ArgumentNullException("source4");
		}
		if (source5 == null)
		{
			throw new ArgumentNullException("source5");
		}
		if (source6 == null)
		{
			throw new ArgumentNullException("source6");
		}
		if (resultSelector == null)
		{
			throw new ArgumentNullException("resultSelector");
		}
		return source1.Provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource1), typeof(TSource2), typeof(TSource3), typeof(TSource4), typeof(TSource5), typeof(TSource6), typeof(TResult)), source1.Expression, GetSourceExpression(source2), GetSourceExpression(source3), GetSourceExpression(source4), GetSourceExpression(source5), GetSourceExpression(source6), resultSelector));
	}

	public static IQbservable<TResult> CombineLatest<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TResult>(this IQbservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, IObservable<TSource4> source4, IObservable<TSource5> source5, IObservable<TSource6> source6, IObservable<TSource7> source7, Expression<Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TResult>> resultSelector)
	{
		if (source1 == null)
		{
			throw new ArgumentNullException("source1");
		}
		if (source2 == null)
		{
			throw new ArgumentNullException("source2");
		}
		if (source3 == null)
		{
			throw new ArgumentNullException("source3");
		}
		if (source4 == null)
		{
			throw new ArgumentNullException("source4");
		}
		if (source5 == null)
		{
			throw new ArgumentNullException("source5");
		}
		if (source6 == null)
		{
			throw new ArgumentNullException("source6");
		}
		if (source7 == null)
		{
			throw new ArgumentNullException("source7");
		}
		if (resultSelector == null)
		{
			throw new ArgumentNullException("resultSelector");
		}
		return source1.Provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource1), typeof(TSource2), typeof(TSource3), typeof(TSource4), typeof(TSource5), typeof(TSource6), typeof(TSource7), typeof(TResult)), source1.Expression, GetSourceExpression(source2), GetSourceExpression(source3), GetSourceExpression(source4), GetSourceExpression(source5), GetSourceExpression(source6), GetSourceExpression(source7), resultSelector));
	}

	public static IQbservable<TResult> CombineLatest<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TResult>(this IQbservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, IObservable<TSource4> source4, IObservable<TSource5> source5, IObservable<TSource6> source6, IObservable<TSource7> source7, IObservable<TSource8> source8, Expression<Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TResult>> resultSelector)
	{
		if (source1 == null)
		{
			throw new ArgumentNullException("source1");
		}
		if (source2 == null)
		{
			throw new ArgumentNullException("source2");
		}
		if (source3 == null)
		{
			throw new ArgumentNullException("source3");
		}
		if (source4 == null)
		{
			throw new ArgumentNullException("source4");
		}
		if (source5 == null)
		{
			throw new ArgumentNullException("source5");
		}
		if (source6 == null)
		{
			throw new ArgumentNullException("source6");
		}
		if (source7 == null)
		{
			throw new ArgumentNullException("source7");
		}
		if (source8 == null)
		{
			throw new ArgumentNullException("source8");
		}
		if (resultSelector == null)
		{
			throw new ArgumentNullException("resultSelector");
		}
		return source1.Provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource1), typeof(TSource2), typeof(TSource3), typeof(TSource4), typeof(TSource5), typeof(TSource6), typeof(TSource7), typeof(TSource8), typeof(TResult)), source1.Expression, GetSourceExpression(source2), GetSourceExpression(source3), GetSourceExpression(source4), GetSourceExpression(source5), GetSourceExpression(source6), GetSourceExpression(source7), GetSourceExpression(source8), resultSelector));
	}

	public static IQbservable<TResult> CombineLatest<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TResult>(this IQbservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, IObservable<TSource4> source4, IObservable<TSource5> source5, IObservable<TSource6> source6, IObservable<TSource7> source7, IObservable<TSource8> source8, IObservable<TSource9> source9, Expression<Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TResult>> resultSelector)
	{
		if (source1 == null)
		{
			throw new ArgumentNullException("source1");
		}
		if (source2 == null)
		{
			throw new ArgumentNullException("source2");
		}
		if (source3 == null)
		{
			throw new ArgumentNullException("source3");
		}
		if (source4 == null)
		{
			throw new ArgumentNullException("source4");
		}
		if (source5 == null)
		{
			throw new ArgumentNullException("source5");
		}
		if (source6 == null)
		{
			throw new ArgumentNullException("source6");
		}
		if (source7 == null)
		{
			throw new ArgumentNullException("source7");
		}
		if (source8 == null)
		{
			throw new ArgumentNullException("source8");
		}
		if (source9 == null)
		{
			throw new ArgumentNullException("source9");
		}
		if (resultSelector == null)
		{
			throw new ArgumentNullException("resultSelector");
		}
		return source1.Provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource1), typeof(TSource2), typeof(TSource3), typeof(TSource4), typeof(TSource5), typeof(TSource6), typeof(TSource7), typeof(TSource8), typeof(TSource9), typeof(TResult)), source1.Expression, GetSourceExpression(source2), GetSourceExpression(source3), GetSourceExpression(source4), GetSourceExpression(source5), GetSourceExpression(source6), GetSourceExpression(source7), GetSourceExpression(source8), GetSourceExpression(source9), resultSelector));
	}

	public static IQbservable<TResult> CombineLatest<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TResult>(this IQbservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, IObservable<TSource4> source4, IObservable<TSource5> source5, IObservable<TSource6> source6, IObservable<TSource7> source7, IObservable<TSource8> source8, IObservable<TSource9> source9, IObservable<TSource10> source10, Expression<Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TResult>> resultSelector)
	{
		if (source1 == null)
		{
			throw new ArgumentNullException("source1");
		}
		if (source2 == null)
		{
			throw new ArgumentNullException("source2");
		}
		if (source3 == null)
		{
			throw new ArgumentNullException("source3");
		}
		if (source4 == null)
		{
			throw new ArgumentNullException("source4");
		}
		if (source5 == null)
		{
			throw new ArgumentNullException("source5");
		}
		if (source6 == null)
		{
			throw new ArgumentNullException("source6");
		}
		if (source7 == null)
		{
			throw new ArgumentNullException("source7");
		}
		if (source8 == null)
		{
			throw new ArgumentNullException("source8");
		}
		if (source9 == null)
		{
			throw new ArgumentNullException("source9");
		}
		if (source10 == null)
		{
			throw new ArgumentNullException("source10");
		}
		if (resultSelector == null)
		{
			throw new ArgumentNullException("resultSelector");
		}
		return source1.Provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource1), typeof(TSource2), typeof(TSource3), typeof(TSource4), typeof(TSource5), typeof(TSource6), typeof(TSource7), typeof(TSource8), typeof(TSource9), typeof(TSource10), typeof(TResult)), source1.Expression, GetSourceExpression(source2), GetSourceExpression(source3), GetSourceExpression(source4), GetSourceExpression(source5), GetSourceExpression(source6), GetSourceExpression(source7), GetSourceExpression(source8), GetSourceExpression(source9), GetSourceExpression(source10), resultSelector));
	}

	public static IQbservable<TResult> CombineLatest<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TResult>(this IQbservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, IObservable<TSource4> source4, IObservable<TSource5> source5, IObservable<TSource6> source6, IObservable<TSource7> source7, IObservable<TSource8> source8, IObservable<TSource9> source9, IObservable<TSource10> source10, IObservable<TSource11> source11, Expression<Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TResult>> resultSelector)
	{
		if (source1 == null)
		{
			throw new ArgumentNullException("source1");
		}
		if (source2 == null)
		{
			throw new ArgumentNullException("source2");
		}
		if (source3 == null)
		{
			throw new ArgumentNullException("source3");
		}
		if (source4 == null)
		{
			throw new ArgumentNullException("source4");
		}
		if (source5 == null)
		{
			throw new ArgumentNullException("source5");
		}
		if (source6 == null)
		{
			throw new ArgumentNullException("source6");
		}
		if (source7 == null)
		{
			throw new ArgumentNullException("source7");
		}
		if (source8 == null)
		{
			throw new ArgumentNullException("source8");
		}
		if (source9 == null)
		{
			throw new ArgumentNullException("source9");
		}
		if (source10 == null)
		{
			throw new ArgumentNullException("source10");
		}
		if (source11 == null)
		{
			throw new ArgumentNullException("source11");
		}
		if (resultSelector == null)
		{
			throw new ArgumentNullException("resultSelector");
		}
		return source1.Provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource1), typeof(TSource2), typeof(TSource3), typeof(TSource4), typeof(TSource5), typeof(TSource6), typeof(TSource7), typeof(TSource8), typeof(TSource9), typeof(TSource10), typeof(TSource11), typeof(TResult)), source1.Expression, GetSourceExpression(source2), GetSourceExpression(source3), GetSourceExpression(source4), GetSourceExpression(source5), GetSourceExpression(source6), GetSourceExpression(source7), GetSourceExpression(source8), GetSourceExpression(source9), GetSourceExpression(source10), GetSourceExpression(source11), resultSelector));
	}

	public static IQbservable<TResult> CombineLatest<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TResult>(this IQbservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, IObservable<TSource4> source4, IObservable<TSource5> source5, IObservable<TSource6> source6, IObservable<TSource7> source7, IObservable<TSource8> source8, IObservable<TSource9> source9, IObservable<TSource10> source10, IObservable<TSource11> source11, IObservable<TSource12> source12, Expression<Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TResult>> resultSelector)
	{
		if (source1 == null)
		{
			throw new ArgumentNullException("source1");
		}
		if (source2 == null)
		{
			throw new ArgumentNullException("source2");
		}
		if (source3 == null)
		{
			throw new ArgumentNullException("source3");
		}
		if (source4 == null)
		{
			throw new ArgumentNullException("source4");
		}
		if (source5 == null)
		{
			throw new ArgumentNullException("source5");
		}
		if (source6 == null)
		{
			throw new ArgumentNullException("source6");
		}
		if (source7 == null)
		{
			throw new ArgumentNullException("source7");
		}
		if (source8 == null)
		{
			throw new ArgumentNullException("source8");
		}
		if (source9 == null)
		{
			throw new ArgumentNullException("source9");
		}
		if (source10 == null)
		{
			throw new ArgumentNullException("source10");
		}
		if (source11 == null)
		{
			throw new ArgumentNullException("source11");
		}
		if (source12 == null)
		{
			throw new ArgumentNullException("source12");
		}
		if (resultSelector == null)
		{
			throw new ArgumentNullException("resultSelector");
		}
		return source1.Provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource1), typeof(TSource2), typeof(TSource3), typeof(TSource4), typeof(TSource5), typeof(TSource6), typeof(TSource7), typeof(TSource8), typeof(TSource9), typeof(TSource10), typeof(TSource11), typeof(TSource12), typeof(TResult)), source1.Expression, GetSourceExpression(source2), GetSourceExpression(source3), GetSourceExpression(source4), GetSourceExpression(source5), GetSourceExpression(source6), GetSourceExpression(source7), GetSourceExpression(source8), GetSourceExpression(source9), GetSourceExpression(source10), GetSourceExpression(source11), GetSourceExpression(source12), resultSelector));
	}

	public static IQbservable<TResult> CombineLatest<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TResult>(this IQbservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, IObservable<TSource4> source4, IObservable<TSource5> source5, IObservable<TSource6> source6, IObservable<TSource7> source7, IObservable<TSource8> source8, IObservable<TSource9> source9, IObservable<TSource10> source10, IObservable<TSource11> source11, IObservable<TSource12> source12, IObservable<TSource13> source13, Expression<Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TResult>> resultSelector)
	{
		if (source1 == null)
		{
			throw new ArgumentNullException("source1");
		}
		if (source2 == null)
		{
			throw new ArgumentNullException("source2");
		}
		if (source3 == null)
		{
			throw new ArgumentNullException("source3");
		}
		if (source4 == null)
		{
			throw new ArgumentNullException("source4");
		}
		if (source5 == null)
		{
			throw new ArgumentNullException("source5");
		}
		if (source6 == null)
		{
			throw new ArgumentNullException("source6");
		}
		if (source7 == null)
		{
			throw new ArgumentNullException("source7");
		}
		if (source8 == null)
		{
			throw new ArgumentNullException("source8");
		}
		if (source9 == null)
		{
			throw new ArgumentNullException("source9");
		}
		if (source10 == null)
		{
			throw new ArgumentNullException("source10");
		}
		if (source11 == null)
		{
			throw new ArgumentNullException("source11");
		}
		if (source12 == null)
		{
			throw new ArgumentNullException("source12");
		}
		if (source13 == null)
		{
			throw new ArgumentNullException("source13");
		}
		if (resultSelector == null)
		{
			throw new ArgumentNullException("resultSelector");
		}
		return source1.Provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource1), typeof(TSource2), typeof(TSource3), typeof(TSource4), typeof(TSource5), typeof(TSource6), typeof(TSource7), typeof(TSource8), typeof(TSource9), typeof(TSource10), typeof(TSource11), typeof(TSource12), typeof(TSource13), typeof(TResult)), source1.Expression, GetSourceExpression(source2), GetSourceExpression(source3), GetSourceExpression(source4), GetSourceExpression(source5), GetSourceExpression(source6), GetSourceExpression(source7), GetSourceExpression(source8), GetSourceExpression(source9), GetSourceExpression(source10), GetSourceExpression(source11), GetSourceExpression(source12), GetSourceExpression(source13), resultSelector));
	}

	public static IQbservable<TResult> CombineLatest<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TSource14, TResult>(this IQbservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, IObservable<TSource4> source4, IObservable<TSource5> source5, IObservable<TSource6> source6, IObservable<TSource7> source7, IObservable<TSource8> source8, IObservable<TSource9> source9, IObservable<TSource10> source10, IObservable<TSource11> source11, IObservable<TSource12> source12, IObservable<TSource13> source13, IObservable<TSource14> source14, Expression<Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TSource14, TResult>> resultSelector)
	{
		if (source1 == null)
		{
			throw new ArgumentNullException("source1");
		}
		if (source2 == null)
		{
			throw new ArgumentNullException("source2");
		}
		if (source3 == null)
		{
			throw new ArgumentNullException("source3");
		}
		if (source4 == null)
		{
			throw new ArgumentNullException("source4");
		}
		if (source5 == null)
		{
			throw new ArgumentNullException("source5");
		}
		if (source6 == null)
		{
			throw new ArgumentNullException("source6");
		}
		if (source7 == null)
		{
			throw new ArgumentNullException("source7");
		}
		if (source8 == null)
		{
			throw new ArgumentNullException("source8");
		}
		if (source9 == null)
		{
			throw new ArgumentNullException("source9");
		}
		if (source10 == null)
		{
			throw new ArgumentNullException("source10");
		}
		if (source11 == null)
		{
			throw new ArgumentNullException("source11");
		}
		if (source12 == null)
		{
			throw new ArgumentNullException("source12");
		}
		if (source13 == null)
		{
			throw new ArgumentNullException("source13");
		}
		if (source14 == null)
		{
			throw new ArgumentNullException("source14");
		}
		if (resultSelector == null)
		{
			throw new ArgumentNullException("resultSelector");
		}
		return source1.Provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource1), typeof(TSource2), typeof(TSource3), typeof(TSource4), typeof(TSource5), typeof(TSource6), typeof(TSource7), typeof(TSource8), typeof(TSource9), typeof(TSource10), typeof(TSource11), typeof(TSource12), typeof(TSource13), typeof(TSource14), typeof(TResult)), source1.Expression, GetSourceExpression(source2), GetSourceExpression(source3), GetSourceExpression(source4), GetSourceExpression(source5), GetSourceExpression(source6), GetSourceExpression(source7), GetSourceExpression(source8), GetSourceExpression(source9), GetSourceExpression(source10), GetSourceExpression(source11), GetSourceExpression(source12), GetSourceExpression(source13), GetSourceExpression(source14), resultSelector));
	}

	public static IQbservable<TResult> CombineLatest<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TSource14, TSource15, TResult>(this IQbservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, IObservable<TSource4> source4, IObservable<TSource5> source5, IObservable<TSource6> source6, IObservable<TSource7> source7, IObservable<TSource8> source8, IObservable<TSource9> source9, IObservable<TSource10> source10, IObservable<TSource11> source11, IObservable<TSource12> source12, IObservable<TSource13> source13, IObservable<TSource14> source14, IObservable<TSource15> source15, Expression<Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TSource14, TSource15, TResult>> resultSelector)
	{
		if (source1 == null)
		{
			throw new ArgumentNullException("source1");
		}
		if (source2 == null)
		{
			throw new ArgumentNullException("source2");
		}
		if (source3 == null)
		{
			throw new ArgumentNullException("source3");
		}
		if (source4 == null)
		{
			throw new ArgumentNullException("source4");
		}
		if (source5 == null)
		{
			throw new ArgumentNullException("source5");
		}
		if (source6 == null)
		{
			throw new ArgumentNullException("source6");
		}
		if (source7 == null)
		{
			throw new ArgumentNullException("source7");
		}
		if (source8 == null)
		{
			throw new ArgumentNullException("source8");
		}
		if (source9 == null)
		{
			throw new ArgumentNullException("source9");
		}
		if (source10 == null)
		{
			throw new ArgumentNullException("source10");
		}
		if (source11 == null)
		{
			throw new ArgumentNullException("source11");
		}
		if (source12 == null)
		{
			throw new ArgumentNullException("source12");
		}
		if (source13 == null)
		{
			throw new ArgumentNullException("source13");
		}
		if (source14 == null)
		{
			throw new ArgumentNullException("source14");
		}
		if (source15 == null)
		{
			throw new ArgumentNullException("source15");
		}
		if (resultSelector == null)
		{
			throw new ArgumentNullException("resultSelector");
		}
		return source1.Provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource1), typeof(TSource2), typeof(TSource3), typeof(TSource4), typeof(TSource5), typeof(TSource6), typeof(TSource7), typeof(TSource8), typeof(TSource9), typeof(TSource10), typeof(TSource11), typeof(TSource12), typeof(TSource13), typeof(TSource14), typeof(TSource15), typeof(TResult)), source1.Expression, GetSourceExpression(source2), GetSourceExpression(source3), GetSourceExpression(source4), GetSourceExpression(source5), GetSourceExpression(source6), GetSourceExpression(source7), GetSourceExpression(source8), GetSourceExpression(source9), GetSourceExpression(source10), GetSourceExpression(source11), GetSourceExpression(source12), GetSourceExpression(source13), GetSourceExpression(source14), GetSourceExpression(source15), resultSelector));
	}

	public static IQbservable<TResult> CombineLatest<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TSource14, TSource15, TSource16, TResult>(this IQbservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, IObservable<TSource4> source4, IObservable<TSource5> source5, IObservable<TSource6> source6, IObservable<TSource7> source7, IObservable<TSource8> source8, IObservable<TSource9> source9, IObservable<TSource10> source10, IObservable<TSource11> source11, IObservable<TSource12> source12, IObservable<TSource13> source13, IObservable<TSource14> source14, IObservable<TSource15> source15, IObservable<TSource16> source16, Expression<Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TSource14, TSource15, TSource16, TResult>> resultSelector)
	{
		if (source1 == null)
		{
			throw new ArgumentNullException("source1");
		}
		if (source2 == null)
		{
			throw new ArgumentNullException("source2");
		}
		if (source3 == null)
		{
			throw new ArgumentNullException("source3");
		}
		if (source4 == null)
		{
			throw new ArgumentNullException("source4");
		}
		if (source5 == null)
		{
			throw new ArgumentNullException("source5");
		}
		if (source6 == null)
		{
			throw new ArgumentNullException("source6");
		}
		if (source7 == null)
		{
			throw new ArgumentNullException("source7");
		}
		if (source8 == null)
		{
			throw new ArgumentNullException("source8");
		}
		if (source9 == null)
		{
			throw new ArgumentNullException("source9");
		}
		if (source10 == null)
		{
			throw new ArgumentNullException("source10");
		}
		if (source11 == null)
		{
			throw new ArgumentNullException("source11");
		}
		if (source12 == null)
		{
			throw new ArgumentNullException("source12");
		}
		if (source13 == null)
		{
			throw new ArgumentNullException("source13");
		}
		if (source14 == null)
		{
			throw new ArgumentNullException("source14");
		}
		if (source15 == null)
		{
			throw new ArgumentNullException("source15");
		}
		if (source16 == null)
		{
			throw new ArgumentNullException("source16");
		}
		if (resultSelector == null)
		{
			throw new ArgumentNullException("resultSelector");
		}
		return source1.Provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource1), typeof(TSource2), typeof(TSource3), typeof(TSource4), typeof(TSource5), typeof(TSource6), typeof(TSource7), typeof(TSource8), typeof(TSource9), typeof(TSource10), typeof(TSource11), typeof(TSource12), typeof(TSource13), typeof(TSource14), typeof(TSource15), typeof(TSource16), typeof(TResult)), source1.Expression, GetSourceExpression(source2), GetSourceExpression(source3), GetSourceExpression(source4), GetSourceExpression(source5), GetSourceExpression(source6), GetSourceExpression(source7), GetSourceExpression(source8), GetSourceExpression(source9), GetSourceExpression(source10), GetSourceExpression(source11), GetSourceExpression(source12), GetSourceExpression(source13), GetSourceExpression(source14), GetSourceExpression(source15), GetSourceExpression(source16), resultSelector));
	}

	public static IQbservable<TSource> Concat<TSource>(this IQbservable<TSource> first, IObservable<TSource> second)
	{
		if (first == null)
		{
			throw new ArgumentNullException("first");
		}
		if (second == null)
		{
			throw new ArgumentNullException("second");
		}
		return first.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), first.Expression, GetSourceExpression(second)));
	}

	public static IQbservable<TSource> Concat<TSource>(this IQbservableProvider provider, params IObservable<TSource>[] sources)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (sources == null)
		{
			throw new ArgumentNullException("sources");
		}
		return provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), Expression.Constant(provider, typeof(IQbservableProvider)), GetSourceExpression(sources)));
	}

	public static IQbservable<TSource> Concat<TSource>(this IQbservableProvider provider, IEnumerable<IObservable<TSource>> sources)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (sources == null)
		{
			throw new ArgumentNullException("sources");
		}
		return provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), Expression.Constant(provider, typeof(IQbservableProvider)), GetSourceExpression(sources)));
	}

	public static IQbservable<TSource> Concat<TSource>(this IQbservable<IObservable<TSource>> sources)
	{
		if (sources == null)
		{
			throw new ArgumentNullException("sources");
		}
		return sources.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), sources.Expression));
	}

	public static IQbservable<TSource> Concat<TSource>(this IQbservable<Task<TSource>> sources)
	{
		if (sources == null)
		{
			throw new ArgumentNullException("sources");
		}
		return sources.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), sources.Expression));
	}

	public static IQbservable<bool> Contains<TSource>(this IQbservable<TSource> source, TSource value)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<bool>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, Expression.Constant(value, typeof(TSource))));
	}

	public static IQbservable<bool> Contains<TSource>(this IQbservable<TSource> source, TSource value, IEqualityComparer<TSource> comparer)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (comparer == null)
		{
			throw new ArgumentNullException("comparer");
		}
		return source.Provider.CreateQuery<bool>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, Expression.Constant(value, typeof(TSource)), Expression.Constant(comparer, typeof(IEqualityComparer<TSource>))));
	}

	public static IQbservable<int> Count<TSource>(this IQbservable<TSource> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<int>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression));
	}

	public static IQbservable<int> Count<TSource>(this IQbservable<TSource> source, Expression<Func<TSource, bool>> predicate)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (predicate == null)
		{
			throw new ArgumentNullException("predicate");
		}
		return source.Provider.CreateQuery<int>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, predicate));
	}

	public static IQbservable<TResult> Create<TResult>(this IQbservableProvider provider, Expression<Func<IObserver<TResult>, IDisposable>> subscribe)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (subscribe == null)
		{
			throw new ArgumentNullException("subscribe");
		}
		return provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TResult)), Expression.Constant(provider, typeof(IQbservableProvider)), subscribe));
	}

	public static IQbservable<TResult> Create<TResult>(this IQbservableProvider provider, Expression<Func<IObserver<TResult>, Action>> subscribe)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (subscribe == null)
		{
			throw new ArgumentNullException("subscribe");
		}
		return provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TResult)), Expression.Constant(provider, typeof(IQbservableProvider)), subscribe));
	}

	public static IQbservable<TResult> Create<TResult>(this IQbservableProvider provider, Expression<Func<IObserver<TResult>, CancellationToken, Task>> subscribeAsync)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (subscribeAsync == null)
		{
			throw new ArgumentNullException("subscribeAsync");
		}
		return provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TResult)), Expression.Constant(provider, typeof(IQbservableProvider)), subscribeAsync));
	}

	public static IQbservable<TResult> Create<TResult>(this IQbservableProvider provider, Expression<Func<IObserver<TResult>, Task>> subscribeAsync)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (subscribeAsync == null)
		{
			throw new ArgumentNullException("subscribeAsync");
		}
		return provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TResult)), Expression.Constant(provider, typeof(IQbservableProvider)), subscribeAsync));
	}

	public static IQbservable<TResult> Create<TResult>(this IQbservableProvider provider, Expression<Func<IObserver<TResult>, CancellationToken, Task<IDisposable>>> subscribeAsync)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (subscribeAsync == null)
		{
			throw new ArgumentNullException("subscribeAsync");
		}
		return provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TResult)), Expression.Constant(provider, typeof(IQbservableProvider)), subscribeAsync));
	}

	public static IQbservable<TResult> Create<TResult>(this IQbservableProvider provider, Expression<Func<IObserver<TResult>, Task<IDisposable>>> subscribeAsync)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (subscribeAsync == null)
		{
			throw new ArgumentNullException("subscribeAsync");
		}
		return provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TResult)), Expression.Constant(provider, typeof(IQbservableProvider)), subscribeAsync));
	}

	public static IQbservable<TResult> Create<TResult>(this IQbservableProvider provider, Expression<Func<IObserver<TResult>, CancellationToken, Task<Action>>> subscribeAsync)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (subscribeAsync == null)
		{
			throw new ArgumentNullException("subscribeAsync");
		}
		return provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TResult)), Expression.Constant(provider, typeof(IQbservableProvider)), subscribeAsync));
	}

	public static IQbservable<TResult> Create<TResult>(this IQbservableProvider provider, Expression<Func<IObserver<TResult>, Task<Action>>> subscribeAsync)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (subscribeAsync == null)
		{
			throw new ArgumentNullException("subscribeAsync");
		}
		return provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TResult)), Expression.Constant(provider, typeof(IQbservableProvider)), subscribeAsync));
	}

	public static IQbservable<TSource?> DefaultIfEmpty<TSource>(this IQbservable<TSource> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression));
	}

	public static IQbservable<TSource> DefaultIfEmpty<TSource>(this IQbservable<TSource> source, TSource defaultValue)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, Expression.Constant(defaultValue, typeof(TSource))));
	}

	public static IQbservable<TResult> Defer<TResult>(this IQbservableProvider provider, Expression<Func<IObservable<TResult>>> observableFactory)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (observableFactory == null)
		{
			throw new ArgumentNullException("observableFactory");
		}
		return provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TResult)), Expression.Constant(provider, typeof(IQbservableProvider)), observableFactory));
	}

	public static IQbservable<TResult> Defer<TResult>(this IQbservableProvider provider, Expression<Func<Task<IObservable<TResult>>>> observableFactoryAsync)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (observableFactoryAsync == null)
		{
			throw new ArgumentNullException("observableFactoryAsync");
		}
		return provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TResult)), Expression.Constant(provider, typeof(IQbservableProvider)), observableFactoryAsync));
	}

	public static IQbservable<TResult> Defer<TResult>(this IQbservableProvider provider, Expression<Func<Task<IObservable<TResult>>>> observableFactoryAsync, bool ignoreExceptionsAfterUnsubscribe)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (observableFactoryAsync == null)
		{
			throw new ArgumentNullException("observableFactoryAsync");
		}
		return provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TResult)), Expression.Constant(provider, typeof(IQbservableProvider)), observableFactoryAsync, Expression.Constant(ignoreExceptionsAfterUnsubscribe, typeof(bool))));
	}

	public static IQbservable<TResult> DeferAsync<TResult>(this IQbservableProvider provider, Expression<Func<CancellationToken, Task<IObservable<TResult>>>> observableFactoryAsync)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (observableFactoryAsync == null)
		{
			throw new ArgumentNullException("observableFactoryAsync");
		}
		return provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TResult)), Expression.Constant(provider, typeof(IQbservableProvider)), observableFactoryAsync));
	}

	public static IQbservable<TResult> DeferAsync<TResult>(this IQbservableProvider provider, Expression<Func<CancellationToken, Task<IObservable<TResult>>>> observableFactoryAsync, bool ignoreExceptionsAfterUnsubscribe)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (observableFactoryAsync == null)
		{
			throw new ArgumentNullException("observableFactoryAsync");
		}
		return provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TResult)), Expression.Constant(provider, typeof(IQbservableProvider)), observableFactoryAsync, Expression.Constant(ignoreExceptionsAfterUnsubscribe, typeof(bool))));
	}

	public static IQbservable<TSource> Delay<TSource>(this IQbservable<TSource> source, DateTimeOffset dueTime)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, Expression.Constant(dueTime, typeof(DateTimeOffset))));
	}

	public static IQbservable<TSource> Delay<TSource>(this IQbservable<TSource> source, DateTimeOffset dueTime, IScheduler scheduler)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, Expression.Constant(dueTime, typeof(DateTimeOffset)), Expression.Constant(scheduler, typeof(IScheduler))));
	}

	public static IQbservable<TSource> Delay<TSource>(this IQbservable<TSource> source, TimeSpan dueTime)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, Expression.Constant(dueTime, typeof(TimeSpan))));
	}

	public static IQbservable<TSource> Delay<TSource>(this IQbservable<TSource> source, TimeSpan dueTime, IScheduler scheduler)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, Expression.Constant(dueTime, typeof(TimeSpan)), Expression.Constant(scheduler, typeof(IScheduler))));
	}

	public static IQbservable<TSource> Delay<TSource, TDelay>(this IQbservable<TSource> source, Expression<Func<TSource, IObservable<TDelay>>> delayDurationSelector)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (delayDurationSelector == null)
		{
			throw new ArgumentNullException("delayDurationSelector");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TDelay)), source.Expression, delayDurationSelector));
	}

	public static IQbservable<TSource> Delay<TSource, TDelay>(this IQbservable<TSource> source, IObservable<TDelay> subscriptionDelay, Expression<Func<TSource, IObservable<TDelay>>> delayDurationSelector)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (subscriptionDelay == null)
		{
			throw new ArgumentNullException("subscriptionDelay");
		}
		if (delayDurationSelector == null)
		{
			throw new ArgumentNullException("delayDurationSelector");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TDelay)), source.Expression, GetSourceExpression(subscriptionDelay), delayDurationSelector));
	}

	public static IQbservable<TSource> DelaySubscription<TSource>(this IQbservable<TSource> source, DateTimeOffset dueTime)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, Expression.Constant(dueTime, typeof(DateTimeOffset))));
	}

	public static IQbservable<TSource> DelaySubscription<TSource>(this IQbservable<TSource> source, DateTimeOffset dueTime, IScheduler scheduler)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, Expression.Constant(dueTime, typeof(DateTimeOffset)), Expression.Constant(scheduler, typeof(IScheduler))));
	}

	public static IQbservable<TSource> DelaySubscription<TSource>(this IQbservable<TSource> source, TimeSpan dueTime)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, Expression.Constant(dueTime, typeof(TimeSpan))));
	}

	public static IQbservable<TSource> DelaySubscription<TSource>(this IQbservable<TSource> source, TimeSpan dueTime, IScheduler scheduler)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, Expression.Constant(dueTime, typeof(TimeSpan)), Expression.Constant(scheduler, typeof(IScheduler))));
	}

	public static IQbservable<TSource> Dematerialize<TSource>(this IQbservable<Notification<TSource>> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression));
	}

	public static IQbservable<TSource> Distinct<TSource>(this IQbservable<TSource> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression));
	}

	public static IQbservable<TSource> Distinct<TSource>(this IQbservable<TSource> source, IEqualityComparer<TSource> comparer)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (comparer == null)
		{
			throw new ArgumentNullException("comparer");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, Expression.Constant(comparer, typeof(IEqualityComparer<TSource>))));
	}

	public static IQbservable<TSource> Distinct<TSource, TKey>(this IQbservable<TSource> source, Expression<Func<TSource, TKey>> keySelector)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (keySelector == null)
		{
			throw new ArgumentNullException("keySelector");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TKey)), source.Expression, keySelector));
	}

	public static IQbservable<TSource> Distinct<TSource, TKey>(this IQbservable<TSource> source, Expression<Func<TSource, TKey>> keySelector, IEqualityComparer<TKey> comparer)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (keySelector == null)
		{
			throw new ArgumentNullException("keySelector");
		}
		if (comparer == null)
		{
			throw new ArgumentNullException("comparer");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TKey)), source.Expression, keySelector, Expression.Constant(comparer, typeof(IEqualityComparer<TKey>))));
	}

	public static IQbservable<TSource> DistinctUntilChanged<TSource>(this IQbservable<TSource> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression));
	}

	public static IQbservable<TSource> DistinctUntilChanged<TSource>(this IQbservable<TSource> source, IEqualityComparer<TSource> comparer)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (comparer == null)
		{
			throw new ArgumentNullException("comparer");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, Expression.Constant(comparer, typeof(IEqualityComparer<TSource>))));
	}

	public static IQbservable<TSource> DistinctUntilChanged<TSource, TKey>(this IQbservable<TSource> source, Expression<Func<TSource, TKey>> keySelector)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (keySelector == null)
		{
			throw new ArgumentNullException("keySelector");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TKey)), source.Expression, keySelector));
	}

	public static IQbservable<TSource> DistinctUntilChanged<TSource, TKey>(this IQbservable<TSource> source, Expression<Func<TSource, TKey>> keySelector, IEqualityComparer<TKey> comparer)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (keySelector == null)
		{
			throw new ArgumentNullException("keySelector");
		}
		if (comparer == null)
		{
			throw new ArgumentNullException("comparer");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TKey)), source.Expression, keySelector, Expression.Constant(comparer, typeof(IEqualityComparer<TKey>))));
	}

	public static IQbservable<TSource> Do<TSource>(this IQbservable<TSource> source, IObserver<TSource> observer)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (observer == null)
		{
			throw new ArgumentNullException("observer");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, Expression.Constant(observer, typeof(IObserver<TSource>))));
	}

	public static IQbservable<TSource> Do<TSource>(this IQbservable<TSource> source, Expression<Action<TSource>> onNext)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (onNext == null)
		{
			throw new ArgumentNullException("onNext");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, onNext));
	}

	public static IQbservable<TSource> Do<TSource>(this IQbservable<TSource> source, Expression<Action<TSource>> onNext, Expression<Action> onCompleted)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (onNext == null)
		{
			throw new ArgumentNullException("onNext");
		}
		if (onCompleted == null)
		{
			throw new ArgumentNullException("onCompleted");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, onNext, onCompleted));
	}

	public static IQbservable<TSource> Do<TSource>(this IQbservable<TSource> source, Expression<Action<TSource>> onNext, Expression<Action<Exception>> onError)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (onNext == null)
		{
			throw new ArgumentNullException("onNext");
		}
		if (onError == null)
		{
			throw new ArgumentNullException("onError");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, onNext, onError));
	}

	public static IQbservable<TSource> Do<TSource>(this IQbservable<TSource> source, Expression<Action<TSource>> onNext, Expression<Action<Exception>> onError, Expression<Action> onCompleted)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (onNext == null)
		{
			throw new ArgumentNullException("onNext");
		}
		if (onError == null)
		{
			throw new ArgumentNullException("onError");
		}
		if (onCompleted == null)
		{
			throw new ArgumentNullException("onCompleted");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, onNext, onError, onCompleted));
	}

	public static IQbservable<TSource> DoWhile<TSource>(this IQbservable<TSource> source, Expression<Func<bool>> condition)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (condition == null)
		{
			throw new ArgumentNullException("condition");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, condition));
	}

	public static IQbservable<TSource> ElementAt<TSource>(this IQbservable<TSource> source, int index)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, Expression.Constant(index, typeof(int))));
	}

	public static IQbservable<TSource?> ElementAtOrDefault<TSource>(this IQbservable<TSource> source, int index)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, Expression.Constant(index, typeof(int))));
	}

	public static IQbservable<TResult> Empty<TResult>(this IQbservableProvider provider)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		return provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TResult)), Expression.Constant(provider, typeof(IQbservableProvider))));
	}

	public static IQbservable<TResult> Empty<TResult>(this IQbservableProvider provider, IScheduler scheduler)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TResult)), Expression.Constant(provider, typeof(IQbservableProvider)), Expression.Constant(scheduler, typeof(IScheduler))));
	}

	public static IQbservable<TResult> Empty<TResult>(this IQbservableProvider provider, IScheduler scheduler, TResult witness)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TResult)), Expression.Constant(provider, typeof(IQbservableProvider)), Expression.Constant(scheduler, typeof(IScheduler)), Expression.Constant(witness, typeof(TResult))));
	}

	public static IQbservable<TResult> Empty<TResult>(this IQbservableProvider provider, TResult witness)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		return provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TResult)), Expression.Constant(provider, typeof(IQbservableProvider)), Expression.Constant(witness, typeof(TResult))));
	}

	public static IQbservable<TSource> Finally<TSource>(this IQbservable<TSource> source, Expression<Action> finallyAction)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (finallyAction == null)
		{
			throw new ArgumentNullException("finallyAction");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, finallyAction));
	}

	public static IQbservable<TSource> FirstAsync<TSource>(this IQbservable<TSource> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression));
	}

	public static IQbservable<TSource> FirstAsync<TSource>(this IQbservable<TSource> source, Expression<Func<TSource, bool>> predicate)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (predicate == null)
		{
			throw new ArgumentNullException("predicate");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, predicate));
	}

	public static IQbservable<TSource?> FirstOrDefaultAsync<TSource>(this IQbservable<TSource> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression));
	}

	public static IQbservable<TSource?> FirstOrDefaultAsync<TSource>(this IQbservable<TSource> source, Expression<Func<TSource, bool>> predicate)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (predicate == null)
		{
			throw new ArgumentNullException("predicate");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, predicate));
	}

	public static IQbservable<TResult> For<TSource, TResult>(this IQbservableProvider provider, IEnumerable<TSource> source, Expression<Func<TSource, IObservable<TResult>>> resultSelector)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (resultSelector == null)
		{
			throw new ArgumentNullException("resultSelector");
		}
		return provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TResult)), Expression.Constant(provider, typeof(IQbservableProvider)), GetSourceExpression(source), resultSelector));
	}

	public static IQbservable<Unit> FromAsync(this IQbservableProvider provider, Expression<Func<Task>> actionAsync)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (actionAsync == null)
		{
			throw new ArgumentNullException("actionAsync");
		}
		return provider.CreateQuery<Unit>(Expression.Call(null, (MethodInfo)MethodBase.GetCurrentMethod(), Expression.Constant(provider, typeof(IQbservableProvider)), actionAsync));
	}

	public static IQbservable<Unit> FromAsync(this IQbservableProvider provider, Expression<Func<Task>> actionAsync, TaskObservationOptions options)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (actionAsync == null)
		{
			throw new ArgumentNullException("actionAsync");
		}
		if (options == null)
		{
			throw new ArgumentNullException("options");
		}
		return provider.CreateQuery<Unit>(Expression.Call(null, (MethodInfo)MethodBase.GetCurrentMethod(), Expression.Constant(provider, typeof(IQbservableProvider)), actionAsync, Expression.Constant(options, typeof(TaskObservationOptions))));
	}

	public static IQbservable<Unit> FromAsync(this IQbservableProvider provider, Expression<Func<Task>> actionAsync, IScheduler scheduler)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (actionAsync == null)
		{
			throw new ArgumentNullException("actionAsync");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return provider.CreateQuery<Unit>(Expression.Call(null, (MethodInfo)MethodBase.GetCurrentMethod(), Expression.Constant(provider, typeof(IQbservableProvider)), actionAsync, Expression.Constant(scheduler, typeof(IScheduler))));
	}

	public static IQbservable<Unit> FromAsync(this IQbservableProvider provider, Expression<Func<CancellationToken, Task>> actionAsync)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (actionAsync == null)
		{
			throw new ArgumentNullException("actionAsync");
		}
		return provider.CreateQuery<Unit>(Expression.Call(null, (MethodInfo)MethodBase.GetCurrentMethod(), Expression.Constant(provider, typeof(IQbservableProvider)), actionAsync));
	}

	public static IQbservable<Unit> FromAsync(this IQbservableProvider provider, Expression<Func<CancellationToken, Task>> actionAsync, TaskObservationOptions options)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (actionAsync == null)
		{
			throw new ArgumentNullException("actionAsync");
		}
		if (options == null)
		{
			throw new ArgumentNullException("options");
		}
		return provider.CreateQuery<Unit>(Expression.Call(null, (MethodInfo)MethodBase.GetCurrentMethod(), Expression.Constant(provider, typeof(IQbservableProvider)), actionAsync, Expression.Constant(options, typeof(TaskObservationOptions))));
	}

	public static IQbservable<Unit> FromAsync(this IQbservableProvider provider, Expression<Func<CancellationToken, Task>> actionAsync, IScheduler scheduler)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (actionAsync == null)
		{
			throw new ArgumentNullException("actionAsync");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return provider.CreateQuery<Unit>(Expression.Call(null, (MethodInfo)MethodBase.GetCurrentMethod(), Expression.Constant(provider, typeof(IQbservableProvider)), actionAsync, Expression.Constant(scheduler, typeof(IScheduler))));
	}

	public static IQbservable<TResult> FromAsync<TResult>(this IQbservableProvider provider, Expression<Func<Task<TResult>>> functionAsync)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (functionAsync == null)
		{
			throw new ArgumentNullException("functionAsync");
		}
		return provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TResult)), Expression.Constant(provider, typeof(IQbservableProvider)), functionAsync));
	}

	public static IQbservable<TResult> FromAsync<TResult>(this IQbservableProvider provider, Expression<Func<CancellationToken, Task<TResult>>> functionAsync)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (functionAsync == null)
		{
			throw new ArgumentNullException("functionAsync");
		}
		return provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TResult)), Expression.Constant(provider, typeof(IQbservableProvider)), functionAsync));
	}

	public static IQbservable<TResult> FromAsync<TResult>(this IQbservableProvider provider, Expression<Func<Task<TResult>>> functionAsync, TaskObservationOptions options)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (functionAsync == null)
		{
			throw new ArgumentNullException("functionAsync");
		}
		if (options == null)
		{
			throw new ArgumentNullException("options");
		}
		return provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TResult)), Expression.Constant(provider, typeof(IQbservableProvider)), functionAsync, Expression.Constant(options, typeof(TaskObservationOptions))));
	}

	public static IQbservable<TResult> FromAsync<TResult>(this IQbservableProvider provider, Expression<Func<CancellationToken, Task<TResult>>> functionAsync, TaskObservationOptions options)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (functionAsync == null)
		{
			throw new ArgumentNullException("functionAsync");
		}
		if (options == null)
		{
			throw new ArgumentNullException("options");
		}
		return provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TResult)), Expression.Constant(provider, typeof(IQbservableProvider)), functionAsync, Expression.Constant(options, typeof(TaskObservationOptions))));
	}

	public static IQbservable<TResult> FromAsync<TResult>(this IQbservableProvider provider, Expression<Func<Task<TResult>>> functionAsync, IScheduler scheduler)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (functionAsync == null)
		{
			throw new ArgumentNullException("functionAsync");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TResult)), Expression.Constant(provider, typeof(IQbservableProvider)), functionAsync, Expression.Constant(scheduler, typeof(IScheduler))));
	}

	public static IQbservable<TResult> FromAsync<TResult>(this IQbservableProvider provider, Expression<Func<CancellationToken, Task<TResult>>> functionAsync, IScheduler scheduler)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (functionAsync == null)
		{
			throw new ArgumentNullException("functionAsync");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TResult)), Expression.Constant(provider, typeof(IQbservableProvider)), functionAsync, Expression.Constant(scheduler, typeof(IScheduler))));
	}

	public static IQbservable<Unit> FromEvent(this IQbservableProvider provider, Expression<Action<Action>> addHandler, Expression<Action<Action>> removeHandler)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (addHandler == null)
		{
			throw new ArgumentNullException("addHandler");
		}
		if (removeHandler == null)
		{
			throw new ArgumentNullException("removeHandler");
		}
		return provider.CreateQuery<Unit>(Expression.Call(null, (MethodInfo)MethodBase.GetCurrentMethod(), Expression.Constant(provider, typeof(IQbservableProvider)), addHandler, removeHandler));
	}

	public static IQbservable<Unit> FromEvent(this IQbservableProvider provider, Expression<Action<Action>> addHandler, Expression<Action<Action>> removeHandler, IScheduler scheduler)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (addHandler == null)
		{
			throw new ArgumentNullException("addHandler");
		}
		if (removeHandler == null)
		{
			throw new ArgumentNullException("removeHandler");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return provider.CreateQuery<Unit>(Expression.Call(null, (MethodInfo)MethodBase.GetCurrentMethod(), Expression.Constant(provider, typeof(IQbservableProvider)), addHandler, removeHandler, Expression.Constant(scheduler, typeof(IScheduler))));
	}

	public static IQbservable<TEventArgs> FromEvent<TDelegate, TEventArgs>(this IQbservableProvider provider, Expression<Action<TDelegate>> addHandler, Expression<Action<TDelegate>> removeHandler)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (addHandler == null)
		{
			throw new ArgumentNullException("addHandler");
		}
		if (removeHandler == null)
		{
			throw new ArgumentNullException("removeHandler");
		}
		return provider.CreateQuery<TEventArgs>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TDelegate), typeof(TEventArgs)), Expression.Constant(provider, typeof(IQbservableProvider)), addHandler, removeHandler));
	}

	public static IQbservable<TEventArgs> FromEvent<TDelegate, TEventArgs>(this IQbservableProvider provider, Expression<Action<TDelegate>> addHandler, Expression<Action<TDelegate>> removeHandler, IScheduler scheduler)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (addHandler == null)
		{
			throw new ArgumentNullException("addHandler");
		}
		if (removeHandler == null)
		{
			throw new ArgumentNullException("removeHandler");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return provider.CreateQuery<TEventArgs>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TDelegate), typeof(TEventArgs)), Expression.Constant(provider, typeof(IQbservableProvider)), addHandler, removeHandler, Expression.Constant(scheduler, typeof(IScheduler))));
	}

	public static IQbservable<TEventArgs> FromEvent<TDelegate, TEventArgs>(this IQbservableProvider provider, Expression<Func<Action<TEventArgs>, TDelegate>> conversion, Expression<Action<TDelegate>> addHandler, Expression<Action<TDelegate>> removeHandler)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (conversion == null)
		{
			throw new ArgumentNullException("conversion");
		}
		if (addHandler == null)
		{
			throw new ArgumentNullException("addHandler");
		}
		if (removeHandler == null)
		{
			throw new ArgumentNullException("removeHandler");
		}
		return provider.CreateQuery<TEventArgs>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TDelegate), typeof(TEventArgs)), Expression.Constant(provider, typeof(IQbservableProvider)), conversion, addHandler, removeHandler));
	}

	public static IQbservable<TEventArgs> FromEvent<TDelegate, TEventArgs>(this IQbservableProvider provider, Expression<Func<Action<TEventArgs>, TDelegate>> conversion, Expression<Action<TDelegate>> addHandler, Expression<Action<TDelegate>> removeHandler, IScheduler scheduler)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (conversion == null)
		{
			throw new ArgumentNullException("conversion");
		}
		if (addHandler == null)
		{
			throw new ArgumentNullException("addHandler");
		}
		if (removeHandler == null)
		{
			throw new ArgumentNullException("removeHandler");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return provider.CreateQuery<TEventArgs>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TDelegate), typeof(TEventArgs)), Expression.Constant(provider, typeof(IQbservableProvider)), conversion, addHandler, removeHandler, Expression.Constant(scheduler, typeof(IScheduler))));
	}

	public static IQbservable<TEventArgs> FromEvent<TEventArgs>(this IQbservableProvider provider, Expression<Action<Action<TEventArgs>>> addHandler, Expression<Action<Action<TEventArgs>>> removeHandler)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (addHandler == null)
		{
			throw new ArgumentNullException("addHandler");
		}
		if (removeHandler == null)
		{
			throw new ArgumentNullException("removeHandler");
		}
		return provider.CreateQuery<TEventArgs>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TEventArgs)), Expression.Constant(provider, typeof(IQbservableProvider)), addHandler, removeHandler));
	}

	public static IQbservable<TEventArgs> FromEvent<TEventArgs>(this IQbservableProvider provider, Expression<Action<Action<TEventArgs>>> addHandler, Expression<Action<Action<TEventArgs>>> removeHandler, IScheduler scheduler)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (addHandler == null)
		{
			throw new ArgumentNullException("addHandler");
		}
		if (removeHandler == null)
		{
			throw new ArgumentNullException("removeHandler");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return provider.CreateQuery<TEventArgs>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TEventArgs)), Expression.Constant(provider, typeof(IQbservableProvider)), addHandler, removeHandler, Expression.Constant(scheduler, typeof(IScheduler))));
	}

	public static IQbservable<EventPattern<object>> FromEventPattern(this IQbservableProvider provider, Expression<Action<EventHandler>> addHandler, Expression<Action<EventHandler>> removeHandler)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (addHandler == null)
		{
			throw new ArgumentNullException("addHandler");
		}
		if (removeHandler == null)
		{
			throw new ArgumentNullException("removeHandler");
		}
		return provider.CreateQuery<EventPattern<object>>(Expression.Call(null, (MethodInfo)MethodBase.GetCurrentMethod(), Expression.Constant(provider, typeof(IQbservableProvider)), addHandler, removeHandler));
	}

	public static IQbservable<EventPattern<object>> FromEventPattern(this IQbservableProvider provider, Expression<Action<EventHandler>> addHandler, Expression<Action<EventHandler>> removeHandler, IScheduler scheduler)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (addHandler == null)
		{
			throw new ArgumentNullException("addHandler");
		}
		if (removeHandler == null)
		{
			throw new ArgumentNullException("removeHandler");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return provider.CreateQuery<EventPattern<object>>(Expression.Call(null, (MethodInfo)MethodBase.GetCurrentMethod(), Expression.Constant(provider, typeof(IQbservableProvider)), addHandler, removeHandler, Expression.Constant(scheduler, typeof(IScheduler))));
	}

	public static IQbservable<EventPattern<object>> FromEventPattern(this IQbservableProvider provider, object target, string eventName)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (target == null)
		{
			throw new ArgumentNullException("target");
		}
		if (eventName == null)
		{
			throw new ArgumentNullException("eventName");
		}
		return provider.CreateQuery<EventPattern<object>>(Expression.Call(null, (MethodInfo)MethodBase.GetCurrentMethod(), Expression.Constant(provider, typeof(IQbservableProvider)), Expression.Constant(target, typeof(object)), Expression.Constant(eventName, typeof(string))));
	}

	public static IQbservable<EventPattern<object>> FromEventPattern(this IQbservableProvider provider, object target, string eventName, IScheduler scheduler)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (target == null)
		{
			throw new ArgumentNullException("target");
		}
		if (eventName == null)
		{
			throw new ArgumentNullException("eventName");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return provider.CreateQuery<EventPattern<object>>(Expression.Call(null, (MethodInfo)MethodBase.GetCurrentMethod(), Expression.Constant(provider, typeof(IQbservableProvider)), Expression.Constant(target, typeof(object)), Expression.Constant(eventName, typeof(string)), Expression.Constant(scheduler, typeof(IScheduler))));
	}

	public static IQbservable<EventPattern<object>> FromEventPattern(this IQbservableProvider provider, Type type, string eventName)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		if (eventName == null)
		{
			throw new ArgumentNullException("eventName");
		}
		return provider.CreateQuery<EventPattern<object>>(Expression.Call(null, (MethodInfo)MethodBase.GetCurrentMethod(), Expression.Constant(provider, typeof(IQbservableProvider)), Expression.Constant(type, typeof(Type)), Expression.Constant(eventName, typeof(string))));
	}

	public static IQbservable<EventPattern<object>> FromEventPattern(this IQbservableProvider provider, Type type, string eventName, IScheduler scheduler)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		if (eventName == null)
		{
			throw new ArgumentNullException("eventName");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return provider.CreateQuery<EventPattern<object>>(Expression.Call(null, (MethodInfo)MethodBase.GetCurrentMethod(), Expression.Constant(provider, typeof(IQbservableProvider)), Expression.Constant(type, typeof(Type)), Expression.Constant(eventName, typeof(string)), Expression.Constant(scheduler, typeof(IScheduler))));
	}

	public static IQbservable<EventPattern<TEventArgs>> FromEventPattern<TDelegate, TEventArgs>(this IQbservableProvider provider, Expression<Action<TDelegate>> addHandler, Expression<Action<TDelegate>> removeHandler)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (addHandler == null)
		{
			throw new ArgumentNullException("addHandler");
		}
		if (removeHandler == null)
		{
			throw new ArgumentNullException("removeHandler");
		}
		return provider.CreateQuery<EventPattern<TEventArgs>>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TDelegate), typeof(TEventArgs)), Expression.Constant(provider, typeof(IQbservableProvider)), addHandler, removeHandler));
	}

	public static IQbservable<EventPattern<TEventArgs>> FromEventPattern<TDelegate, TEventArgs>(this IQbservableProvider provider, Expression<Action<TDelegate>> addHandler, Expression<Action<TDelegate>> removeHandler, IScheduler scheduler)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (addHandler == null)
		{
			throw new ArgumentNullException("addHandler");
		}
		if (removeHandler == null)
		{
			throw new ArgumentNullException("removeHandler");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return provider.CreateQuery<EventPattern<TEventArgs>>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TDelegate), typeof(TEventArgs)), Expression.Constant(provider, typeof(IQbservableProvider)), addHandler, removeHandler, Expression.Constant(scheduler, typeof(IScheduler))));
	}

	public static IQbservable<EventPattern<TEventArgs>> FromEventPattern<TDelegate, TEventArgs>(this IQbservableProvider provider, Expression<Func<EventHandler<TEventArgs>, TDelegate>> conversion, Expression<Action<TDelegate>> addHandler, Expression<Action<TDelegate>> removeHandler)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (conversion == null)
		{
			throw new ArgumentNullException("conversion");
		}
		if (addHandler == null)
		{
			throw new ArgumentNullException("addHandler");
		}
		if (removeHandler == null)
		{
			throw new ArgumentNullException("removeHandler");
		}
		return provider.CreateQuery<EventPattern<TEventArgs>>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TDelegate), typeof(TEventArgs)), Expression.Constant(provider, typeof(IQbservableProvider)), conversion, addHandler, removeHandler));
	}

	public static IQbservable<EventPattern<TEventArgs>> FromEventPattern<TDelegate, TEventArgs>(this IQbservableProvider provider, Expression<Func<EventHandler<TEventArgs>, TDelegate>> conversion, Expression<Action<TDelegate>> addHandler, Expression<Action<TDelegate>> removeHandler, IScheduler scheduler)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (conversion == null)
		{
			throw new ArgumentNullException("conversion");
		}
		if (addHandler == null)
		{
			throw new ArgumentNullException("addHandler");
		}
		if (removeHandler == null)
		{
			throw new ArgumentNullException("removeHandler");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return provider.CreateQuery<EventPattern<TEventArgs>>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TDelegate), typeof(TEventArgs)), Expression.Constant(provider, typeof(IQbservableProvider)), conversion, addHandler, removeHandler, Expression.Constant(scheduler, typeof(IScheduler))));
	}

	public static IQbservable<EventPattern<TSender, TEventArgs>> FromEventPattern<TDelegate, TSender, TEventArgs>(this IQbservableProvider provider, Expression<Action<TDelegate>> addHandler, Expression<Action<TDelegate>> removeHandler)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (addHandler == null)
		{
			throw new ArgumentNullException("addHandler");
		}
		if (removeHandler == null)
		{
			throw new ArgumentNullException("removeHandler");
		}
		return provider.CreateQuery<EventPattern<TSender, TEventArgs>>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TDelegate), typeof(TSender), typeof(TEventArgs)), Expression.Constant(provider, typeof(IQbservableProvider)), addHandler, removeHandler));
	}

	public static IQbservable<EventPattern<TSender, TEventArgs>> FromEventPattern<TDelegate, TSender, TEventArgs>(this IQbservableProvider provider, Expression<Action<TDelegate>> addHandler, Expression<Action<TDelegate>> removeHandler, IScheduler scheduler)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (addHandler == null)
		{
			throw new ArgumentNullException("addHandler");
		}
		if (removeHandler == null)
		{
			throw new ArgumentNullException("removeHandler");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return provider.CreateQuery<EventPattern<TSender, TEventArgs>>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TDelegate), typeof(TSender), typeof(TEventArgs)), Expression.Constant(provider, typeof(IQbservableProvider)), addHandler, removeHandler, Expression.Constant(scheduler, typeof(IScheduler))));
	}

	public static IQbservable<EventPattern<TEventArgs>> FromEventPattern<TEventArgs>(this IQbservableProvider provider, Expression<Action<EventHandler<TEventArgs>>> addHandler, Expression<Action<EventHandler<TEventArgs>>> removeHandler)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (addHandler == null)
		{
			throw new ArgumentNullException("addHandler");
		}
		if (removeHandler == null)
		{
			throw new ArgumentNullException("removeHandler");
		}
		return provider.CreateQuery<EventPattern<TEventArgs>>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TEventArgs)), Expression.Constant(provider, typeof(IQbservableProvider)), addHandler, removeHandler));
	}

	public static IQbservable<EventPattern<TEventArgs>> FromEventPattern<TEventArgs>(this IQbservableProvider provider, Expression<Action<EventHandler<TEventArgs>>> addHandler, Expression<Action<EventHandler<TEventArgs>>> removeHandler, IScheduler scheduler)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (addHandler == null)
		{
			throw new ArgumentNullException("addHandler");
		}
		if (removeHandler == null)
		{
			throw new ArgumentNullException("removeHandler");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return provider.CreateQuery<EventPattern<TEventArgs>>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TEventArgs)), Expression.Constant(provider, typeof(IQbservableProvider)), addHandler, removeHandler, Expression.Constant(scheduler, typeof(IScheduler))));
	}

	public static IQbservable<EventPattern<TEventArgs>> FromEventPattern<TEventArgs>(this IQbservableProvider provider, object target, string eventName)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (target == null)
		{
			throw new ArgumentNullException("target");
		}
		if (eventName == null)
		{
			throw new ArgumentNullException("eventName");
		}
		return provider.CreateQuery<EventPattern<TEventArgs>>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TEventArgs)), Expression.Constant(provider, typeof(IQbservableProvider)), Expression.Constant(target, typeof(object)), Expression.Constant(eventName, typeof(string))));
	}

	public static IQbservable<EventPattern<TEventArgs>> FromEventPattern<TEventArgs>(this IQbservableProvider provider, object target, string eventName, IScheduler scheduler)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (target == null)
		{
			throw new ArgumentNullException("target");
		}
		if (eventName == null)
		{
			throw new ArgumentNullException("eventName");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return provider.CreateQuery<EventPattern<TEventArgs>>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TEventArgs)), Expression.Constant(provider, typeof(IQbservableProvider)), Expression.Constant(target, typeof(object)), Expression.Constant(eventName, typeof(string)), Expression.Constant(scheduler, typeof(IScheduler))));
	}

	public static IQbservable<EventPattern<TEventArgs>> FromEventPattern<TEventArgs>(this IQbservableProvider provider, Type type, string eventName)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		if (eventName == null)
		{
			throw new ArgumentNullException("eventName");
		}
		return provider.CreateQuery<EventPattern<TEventArgs>>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TEventArgs)), Expression.Constant(provider, typeof(IQbservableProvider)), Expression.Constant(type, typeof(Type)), Expression.Constant(eventName, typeof(string))));
	}

	public static IQbservable<EventPattern<TEventArgs>> FromEventPattern<TEventArgs>(this IQbservableProvider provider, Type type, string eventName, IScheduler scheduler)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		if (eventName == null)
		{
			throw new ArgumentNullException("eventName");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return provider.CreateQuery<EventPattern<TEventArgs>>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TEventArgs)), Expression.Constant(provider, typeof(IQbservableProvider)), Expression.Constant(type, typeof(Type)), Expression.Constant(eventName, typeof(string)), Expression.Constant(scheduler, typeof(IScheduler))));
	}

	public static IQbservable<EventPattern<TSender, TEventArgs>> FromEventPattern<TSender, TEventArgs>(this IQbservableProvider provider, object target, string eventName)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (target == null)
		{
			throw new ArgumentNullException("target");
		}
		if (eventName == null)
		{
			throw new ArgumentNullException("eventName");
		}
		return provider.CreateQuery<EventPattern<TSender, TEventArgs>>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSender), typeof(TEventArgs)), Expression.Constant(provider, typeof(IQbservableProvider)), Expression.Constant(target, typeof(object)), Expression.Constant(eventName, typeof(string))));
	}

	public static IQbservable<EventPattern<TSender, TEventArgs>> FromEventPattern<TSender, TEventArgs>(this IQbservableProvider provider, object target, string eventName, IScheduler scheduler)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (target == null)
		{
			throw new ArgumentNullException("target");
		}
		if (eventName == null)
		{
			throw new ArgumentNullException("eventName");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return provider.CreateQuery<EventPattern<TSender, TEventArgs>>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSender), typeof(TEventArgs)), Expression.Constant(provider, typeof(IQbservableProvider)), Expression.Constant(target, typeof(object)), Expression.Constant(eventName, typeof(string)), Expression.Constant(scheduler, typeof(IScheduler))));
	}

	public static IQbservable<EventPattern<TSender, TEventArgs>> FromEventPattern<TSender, TEventArgs>(this IQbservableProvider provider, Type type, string eventName)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		if (eventName == null)
		{
			throw new ArgumentNullException("eventName");
		}
		return provider.CreateQuery<EventPattern<TSender, TEventArgs>>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSender), typeof(TEventArgs)), Expression.Constant(provider, typeof(IQbservableProvider)), Expression.Constant(type, typeof(Type)), Expression.Constant(eventName, typeof(string))));
	}

	public static IQbservable<EventPattern<TSender, TEventArgs>> FromEventPattern<TSender, TEventArgs>(this IQbservableProvider provider, Type type, string eventName, IScheduler scheduler)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		if (eventName == null)
		{
			throw new ArgumentNullException("eventName");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return provider.CreateQuery<EventPattern<TSender, TEventArgs>>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSender), typeof(TEventArgs)), Expression.Constant(provider, typeof(IQbservableProvider)), Expression.Constant(type, typeof(Type)), Expression.Constant(eventName, typeof(string)), Expression.Constant(scheduler, typeof(IScheduler))));
	}

	public static IQbservable<TResult> Generate<TState, TResult>(this IQbservableProvider provider, TState initialState, Expression<Func<TState, bool>> condition, Expression<Func<TState, TState>> iterate, Expression<Func<TState, TResult>> resultSelector)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (condition == null)
		{
			throw new ArgumentNullException("condition");
		}
		if (iterate == null)
		{
			throw new ArgumentNullException("iterate");
		}
		if (resultSelector == null)
		{
			throw new ArgumentNullException("resultSelector");
		}
		return provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TState), typeof(TResult)), Expression.Constant(provider, typeof(IQbservableProvider)), Expression.Constant(initialState, typeof(TState)), condition, iterate, resultSelector));
	}

	public static IQbservable<TResult> Generate<TState, TResult>(this IQbservableProvider provider, TState initialState, Expression<Func<TState, bool>> condition, Expression<Func<TState, TState>> iterate, Expression<Func<TState, TResult>> resultSelector, IScheduler scheduler)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (condition == null)
		{
			throw new ArgumentNullException("condition");
		}
		if (iterate == null)
		{
			throw new ArgumentNullException("iterate");
		}
		if (resultSelector == null)
		{
			throw new ArgumentNullException("resultSelector");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TState), typeof(TResult)), Expression.Constant(provider, typeof(IQbservableProvider)), Expression.Constant(initialState, typeof(TState)), condition, iterate, resultSelector, Expression.Constant(scheduler, typeof(IScheduler))));
	}

	public static IQbservable<TResult> Generate<TState, TResult>(this IQbservableProvider provider, TState initialState, Expression<Func<TState, bool>> condition, Expression<Func<TState, TState>> iterate, Expression<Func<TState, TResult>> resultSelector, Expression<Func<TState, TimeSpan>> timeSelector)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (condition == null)
		{
			throw new ArgumentNullException("condition");
		}
		if (iterate == null)
		{
			throw new ArgumentNullException("iterate");
		}
		if (resultSelector == null)
		{
			throw new ArgumentNullException("resultSelector");
		}
		if (timeSelector == null)
		{
			throw new ArgumentNullException("timeSelector");
		}
		return provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TState), typeof(TResult)), Expression.Constant(provider, typeof(IQbservableProvider)), Expression.Constant(initialState, typeof(TState)), condition, iterate, resultSelector, timeSelector));
	}

	public static IQbservable<TResult> Generate<TState, TResult>(this IQbservableProvider provider, TState initialState, Expression<Func<TState, bool>> condition, Expression<Func<TState, TState>> iterate, Expression<Func<TState, TResult>> resultSelector, Expression<Func<TState, DateTimeOffset>> timeSelector)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (condition == null)
		{
			throw new ArgumentNullException("condition");
		}
		if (iterate == null)
		{
			throw new ArgumentNullException("iterate");
		}
		if (resultSelector == null)
		{
			throw new ArgumentNullException("resultSelector");
		}
		if (timeSelector == null)
		{
			throw new ArgumentNullException("timeSelector");
		}
		return provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TState), typeof(TResult)), Expression.Constant(provider, typeof(IQbservableProvider)), Expression.Constant(initialState, typeof(TState)), condition, iterate, resultSelector, timeSelector));
	}

	public static IQbservable<TResult> Generate<TState, TResult>(this IQbservableProvider provider, TState initialState, Expression<Func<TState, bool>> condition, Expression<Func<TState, TState>> iterate, Expression<Func<TState, TResult>> resultSelector, Expression<Func<TState, TimeSpan>> timeSelector, IScheduler scheduler)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (condition == null)
		{
			throw new ArgumentNullException("condition");
		}
		if (iterate == null)
		{
			throw new ArgumentNullException("iterate");
		}
		if (resultSelector == null)
		{
			throw new ArgumentNullException("resultSelector");
		}
		if (timeSelector == null)
		{
			throw new ArgumentNullException("timeSelector");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TState), typeof(TResult)), Expression.Constant(provider, typeof(IQbservableProvider)), Expression.Constant(initialState, typeof(TState)), condition, iterate, resultSelector, timeSelector, Expression.Constant(scheduler, typeof(IScheduler))));
	}

	public static IQbservable<TResult> Generate<TState, TResult>(this IQbservableProvider provider, TState initialState, Expression<Func<TState, bool>> condition, Expression<Func<TState, TState>> iterate, Expression<Func<TState, TResult>> resultSelector, Expression<Func<TState, DateTimeOffset>> timeSelector, IScheduler scheduler)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (condition == null)
		{
			throw new ArgumentNullException("condition");
		}
		if (iterate == null)
		{
			throw new ArgumentNullException("iterate");
		}
		if (resultSelector == null)
		{
			throw new ArgumentNullException("resultSelector");
		}
		if (timeSelector == null)
		{
			throw new ArgumentNullException("timeSelector");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TState), typeof(TResult)), Expression.Constant(provider, typeof(IQbservableProvider)), Expression.Constant(initialState, typeof(TState)), condition, iterate, resultSelector, timeSelector, Expression.Constant(scheduler, typeof(IScheduler))));
	}

	public static IQbservable<IGroupedObservable<TKey, TSource>> GroupBy<TSource, TKey>(this IQbservable<TSource> source, Expression<Func<TSource, TKey>> keySelector)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (keySelector == null)
		{
			throw new ArgumentNullException("keySelector");
		}
		return source.Provider.CreateQuery<IGroupedObservable<TKey, TSource>>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TKey)), source.Expression, keySelector));
	}

	public static IQbservable<IGroupedObservable<TKey, TSource>> GroupBy<TSource, TKey>(this IQbservable<TSource> source, Expression<Func<TSource, TKey>> keySelector, int capacity)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (keySelector == null)
		{
			throw new ArgumentNullException("keySelector");
		}
		return source.Provider.CreateQuery<IGroupedObservable<TKey, TSource>>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TKey)), source.Expression, keySelector, Expression.Constant(capacity, typeof(int))));
	}

	public static IQbservable<IGroupedObservable<TKey, TSource>> GroupBy<TSource, TKey>(this IQbservable<TSource> source, Expression<Func<TSource, TKey>> keySelector, int capacity, IEqualityComparer<TKey> comparer)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (keySelector == null)
		{
			throw new ArgumentNullException("keySelector");
		}
		if (comparer == null)
		{
			throw new ArgumentNullException("comparer");
		}
		return source.Provider.CreateQuery<IGroupedObservable<TKey, TSource>>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TKey)), source.Expression, keySelector, Expression.Constant(capacity, typeof(int)), Expression.Constant(comparer, typeof(IEqualityComparer<TKey>))));
	}

	public static IQbservable<IGroupedObservable<TKey, TSource>> GroupBy<TSource, TKey>(this IQbservable<TSource> source, Expression<Func<TSource, TKey>> keySelector, IEqualityComparer<TKey> comparer)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (keySelector == null)
		{
			throw new ArgumentNullException("keySelector");
		}
		if (comparer == null)
		{
			throw new ArgumentNullException("comparer");
		}
		return source.Provider.CreateQuery<IGroupedObservable<TKey, TSource>>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TKey)), source.Expression, keySelector, Expression.Constant(comparer, typeof(IEqualityComparer<TKey>))));
	}

	public static IQbservable<IGroupedObservable<TKey, TElement>> GroupBy<TSource, TKey, TElement>(this IQbservable<TSource> source, Expression<Func<TSource, TKey>> keySelector, Expression<Func<TSource, TElement>> elementSelector)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (keySelector == null)
		{
			throw new ArgumentNullException("keySelector");
		}
		if (elementSelector == null)
		{
			throw new ArgumentNullException("elementSelector");
		}
		return source.Provider.CreateQuery<IGroupedObservable<TKey, TElement>>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TKey), typeof(TElement)), source.Expression, keySelector, elementSelector));
	}

	public static IQbservable<IGroupedObservable<TKey, TElement>> GroupBy<TSource, TKey, TElement>(this IQbservable<TSource> source, Expression<Func<TSource, TKey>> keySelector, Expression<Func<TSource, TElement>> elementSelector, int capacity)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (keySelector == null)
		{
			throw new ArgumentNullException("keySelector");
		}
		if (elementSelector == null)
		{
			throw new ArgumentNullException("elementSelector");
		}
		return source.Provider.CreateQuery<IGroupedObservable<TKey, TElement>>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TKey), typeof(TElement)), source.Expression, keySelector, elementSelector, Expression.Constant(capacity, typeof(int))));
	}

	public static IQbservable<IGroupedObservable<TKey, TElement>> GroupBy<TSource, TKey, TElement>(this IQbservable<TSource> source, Expression<Func<TSource, TKey>> keySelector, Expression<Func<TSource, TElement>> elementSelector, int capacity, IEqualityComparer<TKey> comparer)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (keySelector == null)
		{
			throw new ArgumentNullException("keySelector");
		}
		if (elementSelector == null)
		{
			throw new ArgumentNullException("elementSelector");
		}
		if (comparer == null)
		{
			throw new ArgumentNullException("comparer");
		}
		return source.Provider.CreateQuery<IGroupedObservable<TKey, TElement>>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TKey), typeof(TElement)), source.Expression, keySelector, elementSelector, Expression.Constant(capacity, typeof(int)), Expression.Constant(comparer, typeof(IEqualityComparer<TKey>))));
	}

	public static IQbservable<IGroupedObservable<TKey, TElement>> GroupBy<TSource, TKey, TElement>(this IQbservable<TSource> source, Expression<Func<TSource, TKey>> keySelector, Expression<Func<TSource, TElement>> elementSelector, IEqualityComparer<TKey> comparer)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (keySelector == null)
		{
			throw new ArgumentNullException("keySelector");
		}
		if (elementSelector == null)
		{
			throw new ArgumentNullException("elementSelector");
		}
		if (comparer == null)
		{
			throw new ArgumentNullException("comparer");
		}
		return source.Provider.CreateQuery<IGroupedObservable<TKey, TElement>>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TKey), typeof(TElement)), source.Expression, keySelector, elementSelector, Expression.Constant(comparer, typeof(IEqualityComparer<TKey>))));
	}

	public static IQbservable<IGroupedObservable<TKey, TSource>> GroupByUntil<TSource, TKey, TDuration>(this IQbservable<TSource> source, Expression<Func<TSource, TKey>> keySelector, Expression<Func<IGroupedObservable<TKey, TSource>, IObservable<TDuration>>> durationSelector)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (keySelector == null)
		{
			throw new ArgumentNullException("keySelector");
		}
		if (durationSelector == null)
		{
			throw new ArgumentNullException("durationSelector");
		}
		return source.Provider.CreateQuery<IGroupedObservable<TKey, TSource>>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TKey), typeof(TDuration)), source.Expression, keySelector, durationSelector));
	}

	public static IQbservable<IGroupedObservable<TKey, TSource>> GroupByUntil<TSource, TKey, TDuration>(this IQbservable<TSource> source, Expression<Func<TSource, TKey>> keySelector, Expression<Func<IGroupedObservable<TKey, TSource>, IObservable<TDuration>>> durationSelector, int capacity)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (keySelector == null)
		{
			throw new ArgumentNullException("keySelector");
		}
		if (durationSelector == null)
		{
			throw new ArgumentNullException("durationSelector");
		}
		return source.Provider.CreateQuery<IGroupedObservable<TKey, TSource>>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TKey), typeof(TDuration)), source.Expression, keySelector, durationSelector, Expression.Constant(capacity, typeof(int))));
	}

	public static IQbservable<IGroupedObservable<TKey, TSource>> GroupByUntil<TSource, TKey, TDuration>(this IQbservable<TSource> source, Expression<Func<TSource, TKey>> keySelector, Expression<Func<IGroupedObservable<TKey, TSource>, IObservable<TDuration>>> durationSelector, int capacity, IEqualityComparer<TKey> comparer)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (keySelector == null)
		{
			throw new ArgumentNullException("keySelector");
		}
		if (durationSelector == null)
		{
			throw new ArgumentNullException("durationSelector");
		}
		if (comparer == null)
		{
			throw new ArgumentNullException("comparer");
		}
		return source.Provider.CreateQuery<IGroupedObservable<TKey, TSource>>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TKey), typeof(TDuration)), source.Expression, keySelector, durationSelector, Expression.Constant(capacity, typeof(int)), Expression.Constant(comparer, typeof(IEqualityComparer<TKey>))));
	}

	public static IQbservable<IGroupedObservable<TKey, TSource>> GroupByUntil<TSource, TKey, TDuration>(this IQbservable<TSource> source, Expression<Func<TSource, TKey>> keySelector, Expression<Func<IGroupedObservable<TKey, TSource>, IObservable<TDuration>>> durationSelector, IEqualityComparer<TKey> comparer)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (keySelector == null)
		{
			throw new ArgumentNullException("keySelector");
		}
		if (durationSelector == null)
		{
			throw new ArgumentNullException("durationSelector");
		}
		if (comparer == null)
		{
			throw new ArgumentNullException("comparer");
		}
		return source.Provider.CreateQuery<IGroupedObservable<TKey, TSource>>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TKey), typeof(TDuration)), source.Expression, keySelector, durationSelector, Expression.Constant(comparer, typeof(IEqualityComparer<TKey>))));
	}

	public static IQbservable<IGroupedObservable<TKey, TElement>> GroupByUntil<TSource, TKey, TElement, TDuration>(this IQbservable<TSource> source, Expression<Func<TSource, TKey>> keySelector, Expression<Func<TSource, TElement>> elementSelector, Expression<Func<IGroupedObservable<TKey, TElement>, IObservable<TDuration>>> durationSelector)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (keySelector == null)
		{
			throw new ArgumentNullException("keySelector");
		}
		if (elementSelector == null)
		{
			throw new ArgumentNullException("elementSelector");
		}
		if (durationSelector == null)
		{
			throw new ArgumentNullException("durationSelector");
		}
		return source.Provider.CreateQuery<IGroupedObservable<TKey, TElement>>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TKey), typeof(TElement), typeof(TDuration)), source.Expression, keySelector, elementSelector, durationSelector));
	}

	public static IQbservable<IGroupedObservable<TKey, TElement>> GroupByUntil<TSource, TKey, TElement, TDuration>(this IQbservable<TSource> source, Expression<Func<TSource, TKey>> keySelector, Expression<Func<TSource, TElement>> elementSelector, Expression<Func<IGroupedObservable<TKey, TElement>, IObservable<TDuration>>> durationSelector, int capacity)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (keySelector == null)
		{
			throw new ArgumentNullException("keySelector");
		}
		if (elementSelector == null)
		{
			throw new ArgumentNullException("elementSelector");
		}
		if (durationSelector == null)
		{
			throw new ArgumentNullException("durationSelector");
		}
		return source.Provider.CreateQuery<IGroupedObservable<TKey, TElement>>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TKey), typeof(TElement), typeof(TDuration)), source.Expression, keySelector, elementSelector, durationSelector, Expression.Constant(capacity, typeof(int))));
	}

	public static IQbservable<IGroupedObservable<TKey, TElement>> GroupByUntil<TSource, TKey, TElement, TDuration>(this IQbservable<TSource> source, Expression<Func<TSource, TKey>> keySelector, Expression<Func<TSource, TElement>> elementSelector, Expression<Func<IGroupedObservable<TKey, TElement>, IObservable<TDuration>>> durationSelector, int capacity, IEqualityComparer<TKey> comparer)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (keySelector == null)
		{
			throw new ArgumentNullException("keySelector");
		}
		if (elementSelector == null)
		{
			throw new ArgumentNullException("elementSelector");
		}
		if (durationSelector == null)
		{
			throw new ArgumentNullException("durationSelector");
		}
		if (comparer == null)
		{
			throw new ArgumentNullException("comparer");
		}
		return source.Provider.CreateQuery<IGroupedObservable<TKey, TElement>>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TKey), typeof(TElement), typeof(TDuration)), source.Expression, keySelector, elementSelector, durationSelector, Expression.Constant(capacity, typeof(int)), Expression.Constant(comparer, typeof(IEqualityComparer<TKey>))));
	}

	public static IQbservable<IGroupedObservable<TKey, TElement>> GroupByUntil<TSource, TKey, TElement, TDuration>(this IQbservable<TSource> source, Expression<Func<TSource, TKey>> keySelector, Expression<Func<TSource, TElement>> elementSelector, Expression<Func<IGroupedObservable<TKey, TElement>, IObservable<TDuration>>> durationSelector, IEqualityComparer<TKey> comparer)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (keySelector == null)
		{
			throw new ArgumentNullException("keySelector");
		}
		if (elementSelector == null)
		{
			throw new ArgumentNullException("elementSelector");
		}
		if (durationSelector == null)
		{
			throw new ArgumentNullException("durationSelector");
		}
		if (comparer == null)
		{
			throw new ArgumentNullException("comparer");
		}
		return source.Provider.CreateQuery<IGroupedObservable<TKey, TElement>>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TKey), typeof(TElement), typeof(TDuration)), source.Expression, keySelector, elementSelector, durationSelector, Expression.Constant(comparer, typeof(IEqualityComparer<TKey>))));
	}

	public static IQbservable<TResult> GroupJoin<TLeft, TRight, TLeftDuration, TRightDuration, TResult>(this IQbservable<TLeft> left, IObservable<TRight> right, Expression<Func<TLeft, IObservable<TLeftDuration>>> leftDurationSelector, Expression<Func<TRight, IObservable<TRightDuration>>> rightDurationSelector, Expression<Func<TLeft, IObservable<TRight>, TResult>> resultSelector)
	{
		if (left == null)
		{
			throw new ArgumentNullException("left");
		}
		if (right == null)
		{
			throw new ArgumentNullException("right");
		}
		if (leftDurationSelector == null)
		{
			throw new ArgumentNullException("leftDurationSelector");
		}
		if (rightDurationSelector == null)
		{
			throw new ArgumentNullException("rightDurationSelector");
		}
		if (resultSelector == null)
		{
			throw new ArgumentNullException("resultSelector");
		}
		return left.Provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TLeft), typeof(TRight), typeof(TLeftDuration), typeof(TRightDuration), typeof(TResult)), left.Expression, GetSourceExpression(right), leftDurationSelector, rightDurationSelector, resultSelector));
	}

	public static IQbservable<TResult> If<TResult>(this IQbservableProvider provider, Expression<Func<bool>> condition, IObservable<TResult> thenSource)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (condition == null)
		{
			throw new ArgumentNullException("condition");
		}
		if (thenSource == null)
		{
			throw new ArgumentNullException("thenSource");
		}
		return provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TResult)), Expression.Constant(provider, typeof(IQbservableProvider)), condition, GetSourceExpression(thenSource)));
	}

	public static IQbservable<TResult> If<TResult>(this IQbservableProvider provider, Expression<Func<bool>> condition, IObservable<TResult> thenSource, IObservable<TResult> elseSource)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (condition == null)
		{
			throw new ArgumentNullException("condition");
		}
		if (thenSource == null)
		{
			throw new ArgumentNullException("thenSource");
		}
		if (elseSource == null)
		{
			throw new ArgumentNullException("elseSource");
		}
		return provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TResult)), Expression.Constant(provider, typeof(IQbservableProvider)), condition, GetSourceExpression(thenSource), GetSourceExpression(elseSource)));
	}

	public static IQbservable<TResult> If<TResult>(this IQbservableProvider provider, Expression<Func<bool>> condition, IObservable<TResult> thenSource, IScheduler scheduler)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (condition == null)
		{
			throw new ArgumentNullException("condition");
		}
		if (thenSource == null)
		{
			throw new ArgumentNullException("thenSource");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TResult)), Expression.Constant(provider, typeof(IQbservableProvider)), condition, GetSourceExpression(thenSource), Expression.Constant(scheduler, typeof(IScheduler))));
	}

	public static IQbservable<TSource> IgnoreElements<TSource>(this IQbservable<TSource> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression));
	}

	public static IQbservable<long> Interval(this IQbservableProvider provider, TimeSpan period)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		return provider.CreateQuery<long>(Expression.Call(null, (MethodInfo)MethodBase.GetCurrentMethod(), Expression.Constant(provider, typeof(IQbservableProvider)), Expression.Constant(period, typeof(TimeSpan))));
	}

	public static IQbservable<long> Interval(this IQbservableProvider provider, TimeSpan period, IScheduler scheduler)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return provider.CreateQuery<long>(Expression.Call(null, (MethodInfo)MethodBase.GetCurrentMethod(), Expression.Constant(provider, typeof(IQbservableProvider)), Expression.Constant(period, typeof(TimeSpan)), Expression.Constant(scheduler, typeof(IScheduler))));
	}

	public static IQbservable<bool> IsEmpty<TSource>(this IQbservable<TSource> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<bool>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression));
	}

	public static IQbservable<TResult> Join<TLeft, TRight, TLeftDuration, TRightDuration, TResult>(this IQbservable<TLeft> left, IObservable<TRight> right, Expression<Func<TLeft, IObservable<TLeftDuration>>> leftDurationSelector, Expression<Func<TRight, IObservable<TRightDuration>>> rightDurationSelector, Expression<Func<TLeft, TRight, TResult>> resultSelector)
	{
		if (left == null)
		{
			throw new ArgumentNullException("left");
		}
		if (right == null)
		{
			throw new ArgumentNullException("right");
		}
		if (leftDurationSelector == null)
		{
			throw new ArgumentNullException("leftDurationSelector");
		}
		if (rightDurationSelector == null)
		{
			throw new ArgumentNullException("rightDurationSelector");
		}
		if (resultSelector == null)
		{
			throw new ArgumentNullException("resultSelector");
		}
		return left.Provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TLeft), typeof(TRight), typeof(TLeftDuration), typeof(TRightDuration), typeof(TResult)), left.Expression, GetSourceExpression(right), leftDurationSelector, rightDurationSelector, resultSelector));
	}

	public static IQbservable<TSource> LastAsync<TSource>(this IQbservable<TSource> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression));
	}

	public static IQbservable<TSource> LastAsync<TSource>(this IQbservable<TSource> source, Expression<Func<TSource, bool>> predicate)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (predicate == null)
		{
			throw new ArgumentNullException("predicate");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, predicate));
	}

	public static IQbservable<TSource?> LastOrDefaultAsync<TSource>(this IQbservable<TSource> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression));
	}

	public static IQbservable<TSource?> LastOrDefaultAsync<TSource>(this IQbservable<TSource> source, Expression<Func<TSource, bool>> predicate)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (predicate == null)
		{
			throw new ArgumentNullException("predicate");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, predicate));
	}

	public static IQueryable<TSource> Latest<TSource>(this IQbservable<TSource> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return ((IQueryProvider)source.Provider).CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression));
	}

	public static IQbservable<long> LongCount<TSource>(this IQbservable<TSource> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<long>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression));
	}

	public static IQbservable<long> LongCount<TSource>(this IQbservable<TSource> source, Expression<Func<TSource, bool>> predicate)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (predicate == null)
		{
			throw new ArgumentNullException("predicate");
		}
		return source.Provider.CreateQuery<long>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, predicate));
	}

	public static IQbservable<Notification<TSource>> Materialize<TSource>(this IQbservable<TSource> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<Notification<TSource>>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression));
	}

	public static IQbservable<decimal> Max(this IQbservable<decimal> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<decimal>(Expression.Call(null, (MethodInfo)MethodBase.GetCurrentMethod(), source.Expression));
	}

	public static IQbservable<double> Max(this IQbservable<double> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<double>(Expression.Call(null, (MethodInfo)MethodBase.GetCurrentMethod(), source.Expression));
	}

	public static IQbservable<int> Max(this IQbservable<int> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<int>(Expression.Call(null, (MethodInfo)MethodBase.GetCurrentMethod(), source.Expression));
	}

	public static IQbservable<long> Max(this IQbservable<long> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<long>(Expression.Call(null, (MethodInfo)MethodBase.GetCurrentMethod(), source.Expression));
	}

	public static IQbservable<decimal?> Max(this IQbservable<decimal?> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<decimal?>(Expression.Call(null, (MethodInfo)MethodBase.GetCurrentMethod(), source.Expression));
	}

	public static IQbservable<double?> Max(this IQbservable<double?> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<double?>(Expression.Call(null, (MethodInfo)MethodBase.GetCurrentMethod(), source.Expression));
	}

	public static IQbservable<int?> Max(this IQbservable<int?> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<int?>(Expression.Call(null, (MethodInfo)MethodBase.GetCurrentMethod(), source.Expression));
	}

	public static IQbservable<long?> Max(this IQbservable<long?> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<long?>(Expression.Call(null, (MethodInfo)MethodBase.GetCurrentMethod(), source.Expression));
	}

	public static IQbservable<float?> Max(this IQbservable<float?> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<float?>(Expression.Call(null, (MethodInfo)MethodBase.GetCurrentMethod(), source.Expression));
	}

	public static IQbservable<float> Max(this IQbservable<float> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<float>(Expression.Call(null, (MethodInfo)MethodBase.GetCurrentMethod(), source.Expression));
	}

	public static IQbservable<TSource> Max<TSource>(this IQbservable<TSource> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression));
	}

	public static IQbservable<TSource> Max<TSource>(this IQbservable<TSource> source, IComparer<TSource> comparer)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (comparer == null)
		{
			throw new ArgumentNullException("comparer");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, Expression.Constant(comparer, typeof(IComparer<TSource>))));
	}

	public static IQbservable<double> Max<TSource>(this IQbservable<TSource> source, Expression<Func<TSource, double>> selector)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		return source.Provider.CreateQuery<double>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, selector));
	}

	public static IQbservable<float> Max<TSource>(this IQbservable<TSource> source, Expression<Func<TSource, float>> selector)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		return source.Provider.CreateQuery<float>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, selector));
	}

	public static IQbservable<decimal> Max<TSource>(this IQbservable<TSource> source, Expression<Func<TSource, decimal>> selector)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		return source.Provider.CreateQuery<decimal>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, selector));
	}

	public static IQbservable<int> Max<TSource>(this IQbservable<TSource> source, Expression<Func<TSource, int>> selector)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		return source.Provider.CreateQuery<int>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, selector));
	}

	public static IQbservable<long> Max<TSource>(this IQbservable<TSource> source, Expression<Func<TSource, long>> selector)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		return source.Provider.CreateQuery<long>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, selector));
	}

	public static IQbservable<double?> Max<TSource>(this IQbservable<TSource> source, Expression<Func<TSource, double?>> selector)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		return source.Provider.CreateQuery<double?>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, selector));
	}

	public static IQbservable<float?> Max<TSource>(this IQbservable<TSource> source, Expression<Func<TSource, float?>> selector)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		return source.Provider.CreateQuery<float?>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, selector));
	}

	public static IQbservable<decimal?> Max<TSource>(this IQbservable<TSource> source, Expression<Func<TSource, decimal?>> selector)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		return source.Provider.CreateQuery<decimal?>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, selector));
	}

	public static IQbservable<int?> Max<TSource>(this IQbservable<TSource> source, Expression<Func<TSource, int?>> selector)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		return source.Provider.CreateQuery<int?>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, selector));
	}

	public static IQbservable<long?> Max<TSource>(this IQbservable<TSource> source, Expression<Func<TSource, long?>> selector)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		return source.Provider.CreateQuery<long?>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, selector));
	}

	public static IQbservable<TResult> Max<TSource, TResult>(this IQbservable<TSource> source, Expression<Func<TSource, TResult>> selector)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		return source.Provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TResult)), source.Expression, selector));
	}

	public static IQbservable<TResult> Max<TSource, TResult>(this IQbservable<TSource> source, Expression<Func<TSource, TResult>> selector, IComparer<TResult> comparer)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		if (comparer == null)
		{
			throw new ArgumentNullException("comparer");
		}
		return source.Provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TResult)), source.Expression, selector, Expression.Constant(comparer, typeof(IComparer<TResult>))));
	}

	public static IQbservable<IList<TSource>> MaxBy<TSource, TKey>(this IQbservable<TSource> source, Expression<Func<TSource, TKey>> keySelector)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (keySelector == null)
		{
			throw new ArgumentNullException("keySelector");
		}
		return source.Provider.CreateQuery<IList<TSource>>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TKey)), source.Expression, keySelector));
	}

	public static IQbservable<IList<TSource>> MaxBy<TSource, TKey>(this IQbservable<TSource> source, Expression<Func<TSource, TKey>> keySelector, IComparer<TKey> comparer)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (keySelector == null)
		{
			throw new ArgumentNullException("keySelector");
		}
		if (comparer == null)
		{
			throw new ArgumentNullException("comparer");
		}
		return source.Provider.CreateQuery<IList<TSource>>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TKey)), source.Expression, keySelector, Expression.Constant(comparer, typeof(IComparer<TKey>))));
	}

	public static IQbservable<TSource> Merge<TSource>(this IQbservable<TSource> first, IObservable<TSource> second)
	{
		if (first == null)
		{
			throw new ArgumentNullException("first");
		}
		if (second == null)
		{
			throw new ArgumentNullException("second");
		}
		return first.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), first.Expression, GetSourceExpression(second)));
	}

	public static IQbservable<TSource> Merge<TSource>(this IQbservable<TSource> first, IObservable<TSource> second, IScheduler scheduler)
	{
		if (first == null)
		{
			throw new ArgumentNullException("first");
		}
		if (second == null)
		{
			throw new ArgumentNullException("second");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return first.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), first.Expression, GetSourceExpression(second), Expression.Constant(scheduler, typeof(IScheduler))));
	}

	public static IQbservable<TSource> Merge<TSource>(this IQbservableProvider provider, IScheduler scheduler, params IObservable<TSource>[] sources)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		if (sources == null)
		{
			throw new ArgumentNullException("sources");
		}
		return provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), Expression.Constant(provider, typeof(IQbservableProvider)), Expression.Constant(scheduler, typeof(IScheduler)), GetSourceExpression(sources)));
	}

	public static IQbservable<TSource> Merge<TSource>(this IQbservable<IObservable<TSource>> sources)
	{
		if (sources == null)
		{
			throw new ArgumentNullException("sources");
		}
		return sources.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), sources.Expression));
	}

	public static IQbservable<TSource> Merge<TSource>(this IQbservable<Task<TSource>> sources)
	{
		if (sources == null)
		{
			throw new ArgumentNullException("sources");
		}
		return sources.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), sources.Expression));
	}

	public static IQbservable<TSource> Merge<TSource>(this IQbservableProvider provider, params IObservable<TSource>[] sources)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (sources == null)
		{
			throw new ArgumentNullException("sources");
		}
		return provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), Expression.Constant(provider, typeof(IQbservableProvider)), GetSourceExpression(sources)));
	}

	public static IQbservable<TSource> Merge<TSource>(this IQbservableProvider provider, IEnumerable<IObservable<TSource>> sources)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (sources == null)
		{
			throw new ArgumentNullException("sources");
		}
		return provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), Expression.Constant(provider, typeof(IQbservableProvider)), GetSourceExpression(sources)));
	}

	public static IQbservable<TSource> Merge<TSource>(this IQbservable<IObservable<TSource>> sources, int maxConcurrent)
	{
		if (sources == null)
		{
			throw new ArgumentNullException("sources");
		}
		return sources.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), sources.Expression, Expression.Constant(maxConcurrent, typeof(int))));
	}

	public static IQbservable<TSource> Merge<TSource>(this IQbservableProvider provider, IEnumerable<IObservable<TSource>> sources, int maxConcurrent)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (sources == null)
		{
			throw new ArgumentNullException("sources");
		}
		return provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), Expression.Constant(provider, typeof(IQbservableProvider)), GetSourceExpression(sources), Expression.Constant(maxConcurrent, typeof(int))));
	}

	public static IQbservable<TSource> Merge<TSource>(this IQbservableProvider provider, IEnumerable<IObservable<TSource>> sources, int maxConcurrent, IScheduler scheduler)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (sources == null)
		{
			throw new ArgumentNullException("sources");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), Expression.Constant(provider, typeof(IQbservableProvider)), GetSourceExpression(sources), Expression.Constant(maxConcurrent, typeof(int)), Expression.Constant(scheduler, typeof(IScheduler))));
	}

	public static IQbservable<TSource> Merge<TSource>(this IQbservableProvider provider, IEnumerable<IObservable<TSource>> sources, IScheduler scheduler)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (sources == null)
		{
			throw new ArgumentNullException("sources");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), Expression.Constant(provider, typeof(IQbservableProvider)), GetSourceExpression(sources), Expression.Constant(scheduler, typeof(IScheduler))));
	}

	public static IQbservable<decimal> Min(this IQbservable<decimal> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<decimal>(Expression.Call(null, (MethodInfo)MethodBase.GetCurrentMethod(), source.Expression));
	}

	public static IQbservable<double> Min(this IQbservable<double> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<double>(Expression.Call(null, (MethodInfo)MethodBase.GetCurrentMethod(), source.Expression));
	}

	public static IQbservable<int> Min(this IQbservable<int> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<int>(Expression.Call(null, (MethodInfo)MethodBase.GetCurrentMethod(), source.Expression));
	}

	public static IQbservable<long> Min(this IQbservable<long> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<long>(Expression.Call(null, (MethodInfo)MethodBase.GetCurrentMethod(), source.Expression));
	}

	public static IQbservable<decimal?> Min(this IQbservable<decimal?> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<decimal?>(Expression.Call(null, (MethodInfo)MethodBase.GetCurrentMethod(), source.Expression));
	}

	public static IQbservable<double?> Min(this IQbservable<double?> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<double?>(Expression.Call(null, (MethodInfo)MethodBase.GetCurrentMethod(), source.Expression));
	}

	public static IQbservable<int?> Min(this IQbservable<int?> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<int?>(Expression.Call(null, (MethodInfo)MethodBase.GetCurrentMethod(), source.Expression));
	}

	public static IQbservable<long?> Min(this IQbservable<long?> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<long?>(Expression.Call(null, (MethodInfo)MethodBase.GetCurrentMethod(), source.Expression));
	}

	public static IQbservable<float?> Min(this IQbservable<float?> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<float?>(Expression.Call(null, (MethodInfo)MethodBase.GetCurrentMethod(), source.Expression));
	}

	public static IQbservable<float> Min(this IQbservable<float> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<float>(Expression.Call(null, (MethodInfo)MethodBase.GetCurrentMethod(), source.Expression));
	}

	public static IQbservable<TSource> Min<TSource>(this IQbservable<TSource> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression));
	}

	public static IQbservable<TSource> Min<TSource>(this IQbservable<TSource> source, IComparer<TSource> comparer)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (comparer == null)
		{
			throw new ArgumentNullException("comparer");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, Expression.Constant(comparer, typeof(IComparer<TSource>))));
	}

	public static IQbservable<double> Min<TSource>(this IQbservable<TSource> source, Expression<Func<TSource, double>> selector)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		return source.Provider.CreateQuery<double>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, selector));
	}

	public static IQbservable<float> Min<TSource>(this IQbservable<TSource> source, Expression<Func<TSource, float>> selector)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		return source.Provider.CreateQuery<float>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, selector));
	}

	public static IQbservable<decimal> Min<TSource>(this IQbservable<TSource> source, Expression<Func<TSource, decimal>> selector)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		return source.Provider.CreateQuery<decimal>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, selector));
	}

	public static IQbservable<int> Min<TSource>(this IQbservable<TSource> source, Expression<Func<TSource, int>> selector)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		return source.Provider.CreateQuery<int>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, selector));
	}

	public static IQbservable<long> Min<TSource>(this IQbservable<TSource> source, Expression<Func<TSource, long>> selector)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		return source.Provider.CreateQuery<long>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, selector));
	}

	public static IQbservable<double?> Min<TSource>(this IQbservable<TSource> source, Expression<Func<TSource, double?>> selector)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		return source.Provider.CreateQuery<double?>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, selector));
	}

	public static IQbservable<float?> Min<TSource>(this IQbservable<TSource> source, Expression<Func<TSource, float?>> selector)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		return source.Provider.CreateQuery<float?>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, selector));
	}

	public static IQbservable<decimal?> Min<TSource>(this IQbservable<TSource> source, Expression<Func<TSource, decimal?>> selector)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		return source.Provider.CreateQuery<decimal?>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, selector));
	}

	public static IQbservable<int?> Min<TSource>(this IQbservable<TSource> source, Expression<Func<TSource, int?>> selector)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		return source.Provider.CreateQuery<int?>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, selector));
	}

	public static IQbservable<long?> Min<TSource>(this IQbservable<TSource> source, Expression<Func<TSource, long?>> selector)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		return source.Provider.CreateQuery<long?>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, selector));
	}

	public static IQbservable<TResult> Min<TSource, TResult>(this IQbservable<TSource> source, Expression<Func<TSource, TResult>> selector)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		return source.Provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TResult)), source.Expression, selector));
	}

	public static IQbservable<TResult> Min<TSource, TResult>(this IQbservable<TSource> source, Expression<Func<TSource, TResult>> selector, IComparer<TResult> comparer)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		if (comparer == null)
		{
			throw new ArgumentNullException("comparer");
		}
		return source.Provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TResult)), source.Expression, selector, Expression.Constant(comparer, typeof(IComparer<TResult>))));
	}

	public static IQbservable<IList<TSource>> MinBy<TSource, TKey>(this IQbservable<TSource> source, Expression<Func<TSource, TKey>> keySelector)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (keySelector == null)
		{
			throw new ArgumentNullException("keySelector");
		}
		return source.Provider.CreateQuery<IList<TSource>>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TKey)), source.Expression, keySelector));
	}

	public static IQbservable<IList<TSource>> MinBy<TSource, TKey>(this IQbservable<TSource> source, Expression<Func<TSource, TKey>> keySelector, IComparer<TKey> comparer)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (keySelector == null)
		{
			throw new ArgumentNullException("keySelector");
		}
		if (comparer == null)
		{
			throw new ArgumentNullException("comparer");
		}
		return source.Provider.CreateQuery<IList<TSource>>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TKey)), source.Expression, keySelector, Expression.Constant(comparer, typeof(IComparer<TKey>))));
	}

	public static IQueryable<TSource> MostRecent<TSource>(this IQbservable<TSource> source, TSource initialValue)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return ((IQueryProvider)source.Provider).CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, Expression.Constant(initialValue, typeof(TSource))));
	}

	public static IQbservable<TResult> Multicast<TSource, TIntermediate, TResult>(this IQbservable<TSource> source, Expression<Func<ISubject<TSource, TIntermediate>>> subjectSelector, Expression<Func<IObservable<TIntermediate>, IObservable<TResult>>> selector)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (subjectSelector == null)
		{
			throw new ArgumentNullException("subjectSelector");
		}
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		return source.Provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TIntermediate), typeof(TResult)), source.Expression, subjectSelector, selector));
	}

	public static IQbservable<TResult> Never<TResult>(this IQbservableProvider provider)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		return provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TResult)), Expression.Constant(provider, typeof(IQbservableProvider))));
	}

	public static IQbservable<TResult> Never<TResult>(this IQbservableProvider provider, TResult witness)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		return provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TResult)), Expression.Constant(provider, typeof(IQbservableProvider)), Expression.Constant(witness, typeof(TResult))));
	}

	public static IQueryable<TSource> Next<TSource>(this IQbservable<TSource> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return ((IQueryProvider)source.Provider).CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression));
	}

	public static IQbservable<TSource> ObserveOn<TSource>(this IQbservable<TSource> source, SynchronizationContext context)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (context == null)
		{
			throw new ArgumentNullException("context");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, Expression.Constant(context, typeof(SynchronizationContext))));
	}

	public static IQbservable<TSource> ObserveOn<TSource>(this IQbservable<TSource> source, IScheduler scheduler)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, Expression.Constant(scheduler, typeof(IScheduler))));
	}

	public static IQbservable<TResult> OfType<TResult>(this IQbservable<object> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TResult)), source.Expression));
	}

	public static IQbservable<TSource> OnErrorResumeNext<TSource>(this IQbservable<TSource> first, IObservable<TSource> second)
	{
		if (first == null)
		{
			throw new ArgumentNullException("first");
		}
		if (second == null)
		{
			throw new ArgumentNullException("second");
		}
		return first.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), first.Expression, GetSourceExpression(second)));
	}

	public static IQbservable<TSource> OnErrorResumeNext<TSource>(this IQbservableProvider provider, params IObservable<TSource>[] sources)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (sources == null)
		{
			throw new ArgumentNullException("sources");
		}
		return provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), Expression.Constant(provider, typeof(IQbservableProvider)), GetSourceExpression(sources)));
	}

	public static IQbservable<TSource> OnErrorResumeNext<TSource>(this IQbservableProvider provider, IEnumerable<IObservable<TSource>> sources)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (sources == null)
		{
			throw new ArgumentNullException("sources");
		}
		return provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), Expression.Constant(provider, typeof(IQbservableProvider)), GetSourceExpression(sources)));
	}

	public static IQbservable<TSource> Prepend<TSource>(this IQbservable<TSource> source, TSource value)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, Expression.Constant(value, typeof(TSource))));
	}

	public static IQbservable<TSource> Prepend<TSource>(this IQbservable<TSource> source, TSource value, IScheduler scheduler)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, Expression.Constant(value, typeof(TSource)), Expression.Constant(scheduler, typeof(IScheduler))));
	}

	public static IQbservable<TResult> Publish<TSource, TResult>(this IQbservable<TSource> source, Expression<Func<IObservable<TSource>, IObservable<TResult>>> selector)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		return source.Provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TResult)), source.Expression, selector));
	}

	public static IQbservable<TResult> Publish<TSource, TResult>(this IQbservable<TSource> source, Expression<Func<IObservable<TSource>, IObservable<TResult>>> selector, TSource initialValue)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		return source.Provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TResult)), source.Expression, selector, Expression.Constant(initialValue, typeof(TSource))));
	}

	public static IQbservable<TResult> PublishLast<TSource, TResult>(this IQbservable<TSource> source, Expression<Func<IObservable<TSource>, IObservable<TResult>>> selector)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		return source.Provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TResult)), source.Expression, selector));
	}

	public static IQbservable<int> Range(this IQbservableProvider provider, int start, int count)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		return provider.CreateQuery<int>(Expression.Call(null, (MethodInfo)MethodBase.GetCurrentMethod(), Expression.Constant(provider, typeof(IQbservableProvider)), Expression.Constant(start, typeof(int)), Expression.Constant(count, typeof(int))));
	}

	public static IQbservable<int> Range(this IQbservableProvider provider, int start, int count, IScheduler scheduler)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return provider.CreateQuery<int>(Expression.Call(null, (MethodInfo)MethodBase.GetCurrentMethod(), Expression.Constant(provider, typeof(IQbservableProvider)), Expression.Constant(start, typeof(int)), Expression.Constant(count, typeof(int)), Expression.Constant(scheduler, typeof(IScheduler))));
	}

	public static IQbservable<TSource> RefCount<TSource>(this IQbservableProvider provider, IConnectableObservable<TSource> source)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), Expression.Constant(provider, typeof(IQbservableProvider)), Expression.Constant(source, typeof(IConnectableObservable<TSource>))));
	}

	public static IQbservable<TSource> RefCount<TSource>(this IQbservableProvider provider, IConnectableObservable<TSource> source, TimeSpan disconnectDelay)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), Expression.Constant(provider, typeof(IQbservableProvider)), Expression.Constant(source, typeof(IConnectableObservable<TSource>)), Expression.Constant(disconnectDelay, typeof(TimeSpan))));
	}

	public static IQbservable<TSource> RefCount<TSource>(this IQbservableProvider provider, IConnectableObservable<TSource> source, TimeSpan disconnectDelay, IScheduler scheduler)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), Expression.Constant(provider, typeof(IQbservableProvider)), Expression.Constant(source, typeof(IConnectableObservable<TSource>)), Expression.Constant(disconnectDelay, typeof(TimeSpan)), Expression.Constant(scheduler, typeof(IScheduler))));
	}

	public static IQbservable<TSource> RefCount<TSource>(this IQbservableProvider provider, IConnectableObservable<TSource> source, int minObservers)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), Expression.Constant(provider, typeof(IQbservableProvider)), Expression.Constant(source, typeof(IConnectableObservable<TSource>)), Expression.Constant(minObservers, typeof(int))));
	}

	public static IQbservable<TSource> RefCount<TSource>(this IQbservableProvider provider, IConnectableObservable<TSource> source, int minObservers, TimeSpan disconnectDelay)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), Expression.Constant(provider, typeof(IQbservableProvider)), Expression.Constant(source, typeof(IConnectableObservable<TSource>)), Expression.Constant(minObservers, typeof(int)), Expression.Constant(disconnectDelay, typeof(TimeSpan))));
	}

	public static IQbservable<TSource> RefCount<TSource>(this IQbservableProvider provider, IConnectableObservable<TSource> source, int minObservers, TimeSpan disconnectDelay, IScheduler scheduler)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), Expression.Constant(provider, typeof(IQbservableProvider)), Expression.Constant(source, typeof(IConnectableObservable<TSource>)), Expression.Constant(minObservers, typeof(int)), Expression.Constant(disconnectDelay, typeof(TimeSpan)), Expression.Constant(scheduler, typeof(IScheduler))));
	}

	public static IQbservable<TResult> Repeat<TResult>(this IQbservableProvider provider, TResult value)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		return provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TResult)), Expression.Constant(provider, typeof(IQbservableProvider)), Expression.Constant(value, typeof(TResult))));
	}

	public static IQbservable<TResult> Repeat<TResult>(this IQbservableProvider provider, TResult value, int repeatCount)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		return provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TResult)), Expression.Constant(provider, typeof(IQbservableProvider)), Expression.Constant(value, typeof(TResult)), Expression.Constant(repeatCount, typeof(int))));
	}

	public static IQbservable<TResult> Repeat<TResult>(this IQbservableProvider provider, TResult value, int repeatCount, IScheduler scheduler)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TResult)), Expression.Constant(provider, typeof(IQbservableProvider)), Expression.Constant(value, typeof(TResult)), Expression.Constant(repeatCount, typeof(int)), Expression.Constant(scheduler, typeof(IScheduler))));
	}

	public static IQbservable<TResult> Repeat<TResult>(this IQbservableProvider provider, TResult value, IScheduler scheduler)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TResult)), Expression.Constant(provider, typeof(IQbservableProvider)), Expression.Constant(value, typeof(TResult)), Expression.Constant(scheduler, typeof(IScheduler))));
	}

	public static IQbservable<TSource> Repeat<TSource>(this IQbservable<TSource> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression));
	}

	public static IQbservable<TSource> Repeat<TSource>(this IQbservable<TSource> source, int repeatCount)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, Expression.Constant(repeatCount, typeof(int))));
	}

	public static IQbservable<TSource> RepeatWhen<TSource, TSignal>(this IQbservable<TSource> source, Expression<Func<IObservable<object>, IObservable<TSignal>>> handler)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (handler == null)
		{
			throw new ArgumentNullException("handler");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TSignal)), source.Expression, handler));
	}

	public static IQbservable<TResult> Replay<TSource, TResult>(this IQbservable<TSource> source, Expression<Func<IObservable<TSource>, IObservable<TResult>>> selector)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		return source.Provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TResult)), source.Expression, selector));
	}

	public static IQbservable<TResult> Replay<TSource, TResult>(this IQbservable<TSource> source, Expression<Func<IObservable<TSource>, IObservable<TResult>>> selector, int bufferSize)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		return source.Provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TResult)), source.Expression, selector, Expression.Constant(bufferSize, typeof(int))));
	}

	public static IQbservable<TResult> Replay<TSource, TResult>(this IQbservable<TSource> source, Expression<Func<IObservable<TSource>, IObservable<TResult>>> selector, int bufferSize, IScheduler scheduler)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return source.Provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TResult)), source.Expression, selector, Expression.Constant(bufferSize, typeof(int)), Expression.Constant(scheduler, typeof(IScheduler))));
	}

	public static IQbservable<TResult> Replay<TSource, TResult>(this IQbservable<TSource> source, Expression<Func<IObservable<TSource>, IObservable<TResult>>> selector, int bufferSize, TimeSpan window)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		return source.Provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TResult)), source.Expression, selector, Expression.Constant(bufferSize, typeof(int)), Expression.Constant(window, typeof(TimeSpan))));
	}

	public static IQbservable<TResult> Replay<TSource, TResult>(this IQbservable<TSource> source, Expression<Func<IObservable<TSource>, IObservable<TResult>>> selector, int bufferSize, TimeSpan window, IScheduler scheduler)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return source.Provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TResult)), source.Expression, selector, Expression.Constant(bufferSize, typeof(int)), Expression.Constant(window, typeof(TimeSpan)), Expression.Constant(scheduler, typeof(IScheduler))));
	}

	public static IQbservable<TResult> Replay<TSource, TResult>(this IQbservable<TSource> source, Expression<Func<IObservable<TSource>, IObservable<TResult>>> selector, IScheduler scheduler)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return source.Provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TResult)), source.Expression, selector, Expression.Constant(scheduler, typeof(IScheduler))));
	}

	public static IQbservable<TResult> Replay<TSource, TResult>(this IQbservable<TSource> source, Expression<Func<IObservable<TSource>, IObservable<TResult>>> selector, TimeSpan window)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		return source.Provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TResult)), source.Expression, selector, Expression.Constant(window, typeof(TimeSpan))));
	}

	public static IQbservable<TResult> Replay<TSource, TResult>(this IQbservable<TSource> source, Expression<Func<IObservable<TSource>, IObservable<TResult>>> selector, TimeSpan window, IScheduler scheduler)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return source.Provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TResult)), source.Expression, selector, Expression.Constant(window, typeof(TimeSpan)), Expression.Constant(scheduler, typeof(IScheduler))));
	}

	public static IQbservable<TSource> Retry<TSource>(this IQbservable<TSource> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression));
	}

	public static IQbservable<TSource> Retry<TSource>(this IQbservable<TSource> source, int retryCount)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, Expression.Constant(retryCount, typeof(int))));
	}

	public static IQbservable<TSource> RetryWhen<TSource, TSignal>(this IQbservable<TSource> source, Expression<Func<IObservable<Exception>, IObservable<TSignal>>> handler)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (handler == null)
		{
			throw new ArgumentNullException("handler");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TSignal)), source.Expression, handler));
	}

	public static IQbservable<TResult> Return<TResult>(this IQbservableProvider provider, TResult value)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		return provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TResult)), Expression.Constant(provider, typeof(IQbservableProvider)), Expression.Constant(value, typeof(TResult))));
	}

	public static IQbservable<TResult> Return<TResult>(this IQbservableProvider provider, TResult value, IScheduler scheduler)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TResult)), Expression.Constant(provider, typeof(IQbservableProvider)), Expression.Constant(value, typeof(TResult)), Expression.Constant(scheduler, typeof(IScheduler))));
	}

	public static IQbservable<TSource> Sample<TSource>(this IQbservable<TSource> source, TimeSpan interval)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, Expression.Constant(interval, typeof(TimeSpan))));
	}

	public static IQbservable<TSource> Sample<TSource>(this IQbservable<TSource> source, TimeSpan interval, IScheduler scheduler)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, Expression.Constant(interval, typeof(TimeSpan)), Expression.Constant(scheduler, typeof(IScheduler))));
	}

	public static IQbservable<TSource> Sample<TSource, TSample>(this IQbservable<TSource> source, IObservable<TSample> sampler)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (sampler == null)
		{
			throw new ArgumentNullException("sampler");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TSample)), source.Expression, GetSourceExpression(sampler)));
	}

	public static IQbservable<TSource> Scan<TSource>(this IQbservable<TSource> source, Expression<Func<TSource, TSource, TSource>> accumulator)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (accumulator == null)
		{
			throw new ArgumentNullException("accumulator");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, accumulator));
	}

	public static IQbservable<TAccumulate> Scan<TSource, TAccumulate>(this IQbservable<TSource> source, TAccumulate seed, Expression<Func<TAccumulate, TSource, TAccumulate>> accumulator)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (accumulator == null)
		{
			throw new ArgumentNullException("accumulator");
		}
		return source.Provider.CreateQuery<TAccumulate>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TAccumulate)), source.Expression, Expression.Constant(seed, typeof(TAccumulate)), accumulator));
	}

	public static IQbservable<TResult> Select<TSource, TResult>(this IQbservable<TSource> source, Expression<Func<TSource, TResult>> selector)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		return source.Provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TResult)), source.Expression, selector));
	}

	public static IQbservable<TResult> Select<TSource, TResult>(this IQbservable<TSource> source, Expression<Func<TSource, int, TResult>> selector)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		return source.Provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TResult)), source.Expression, selector));
	}

	public static IQbservable<TResult> SelectMany<TSource, TCollection, TResult>(this IQbservable<TSource> source, Expression<Func<TSource, IObservable<TCollection>>> collectionSelector, Expression<Func<TSource, TCollection, TResult>> resultSelector)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (collectionSelector == null)
		{
			throw new ArgumentNullException("collectionSelector");
		}
		if (resultSelector == null)
		{
			throw new ArgumentNullException("resultSelector");
		}
		return source.Provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TCollection), typeof(TResult)), source.Expression, collectionSelector, resultSelector));
	}

	public static IQbservable<TResult> SelectMany<TSource, TCollection, TResult>(this IQbservable<TSource> source, Expression<Func<TSource, int, IObservable<TCollection>>> collectionSelector, Expression<Func<TSource, int, TCollection, int, TResult>> resultSelector)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (collectionSelector == null)
		{
			throw new ArgumentNullException("collectionSelector");
		}
		if (resultSelector == null)
		{
			throw new ArgumentNullException("resultSelector");
		}
		return source.Provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TCollection), typeof(TResult)), source.Expression, collectionSelector, resultSelector));
	}

	public static IQbservable<TResult> SelectMany<TSource, TCollection, TResult>(this IQbservable<TSource> source, Expression<Func<TSource, IEnumerable<TCollection>>> collectionSelector, Expression<Func<TSource, TCollection, TResult>> resultSelector)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (collectionSelector == null)
		{
			throw new ArgumentNullException("collectionSelector");
		}
		if (resultSelector == null)
		{
			throw new ArgumentNullException("resultSelector");
		}
		return source.Provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TCollection), typeof(TResult)), source.Expression, collectionSelector, resultSelector));
	}

	public static IQbservable<TResult> SelectMany<TSource, TCollection, TResult>(this IQbservable<TSource> source, Expression<Func<TSource, int, IEnumerable<TCollection>>> collectionSelector, Expression<Func<TSource, int, TCollection, int, TResult>> resultSelector)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (collectionSelector == null)
		{
			throw new ArgumentNullException("collectionSelector");
		}
		if (resultSelector == null)
		{
			throw new ArgumentNullException("resultSelector");
		}
		return source.Provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TCollection), typeof(TResult)), source.Expression, collectionSelector, resultSelector));
	}

	public static IQbservable<TOther> SelectMany<TSource, TOther>(this IQbservable<TSource> source, IObservable<TOther> other)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		return source.Provider.CreateQuery<TOther>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TOther)), source.Expression, GetSourceExpression(other)));
	}

	public static IQbservable<TResult> SelectMany<TSource, TResult>(this IQbservable<TSource> source, Expression<Func<TSource, IObservable<TResult>>> onNext, Expression<Func<Exception, IObservable<TResult>>> onError, Expression<Func<IObservable<TResult>>> onCompleted)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (onNext == null)
		{
			throw new ArgumentNullException("onNext");
		}
		if (onError == null)
		{
			throw new ArgumentNullException("onError");
		}
		if (onCompleted == null)
		{
			throw new ArgumentNullException("onCompleted");
		}
		return source.Provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TResult)), source.Expression, onNext, onError, onCompleted));
	}

	public static IQbservable<TResult> SelectMany<TSource, TResult>(this IQbservable<TSource> source, Expression<Func<TSource, int, IObservable<TResult>>> onNext, Expression<Func<Exception, IObservable<TResult>>> onError, Expression<Func<IObservable<TResult>>> onCompleted)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (onNext == null)
		{
			throw new ArgumentNullException("onNext");
		}
		if (onError == null)
		{
			throw new ArgumentNullException("onError");
		}
		if (onCompleted == null)
		{
			throw new ArgumentNullException("onCompleted");
		}
		return source.Provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TResult)), source.Expression, onNext, onError, onCompleted));
	}

	public static IQbservable<TResult> SelectMany<TSource, TResult>(this IQbservable<TSource> source, Expression<Func<TSource, IObservable<TResult>>> selector)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		return source.Provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TResult)), source.Expression, selector));
	}

	public static IQbservable<TResult> SelectMany<TSource, TResult>(this IQbservable<TSource> source, Expression<Func<TSource, int, IObservable<TResult>>> selector)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		return source.Provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TResult)), source.Expression, selector));
	}

	public static IQbservable<TResult> SelectMany<TSource, TResult>(this IQbservable<TSource> source, Expression<Func<TSource, Task<TResult>>> selector)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		return source.Provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TResult)), source.Expression, selector));
	}

	public static IQbservable<TResult> SelectMany<TSource, TResult>(this IQbservable<TSource> source, Expression<Func<TSource, int, Task<TResult>>> selector)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		return source.Provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TResult)), source.Expression, selector));
	}

	public static IQbservable<TResult> SelectMany<TSource, TResult>(this IQbservable<TSource> source, Expression<Func<TSource, CancellationToken, Task<TResult>>> selector)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		return source.Provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TResult)), source.Expression, selector));
	}

	public static IQbservable<TResult> SelectMany<TSource, TResult>(this IQbservable<TSource> source, Expression<Func<TSource, int, CancellationToken, Task<TResult>>> selector)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		return source.Provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TResult)), source.Expression, selector));
	}

	public static IQbservable<TResult> SelectMany<TSource, TResult>(this IQbservable<TSource> source, Expression<Func<TSource, IEnumerable<TResult>>> selector)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		return source.Provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TResult)), source.Expression, selector));
	}

	public static IQbservable<TResult> SelectMany<TSource, TResult>(this IQbservable<TSource> source, Expression<Func<TSource, int, IEnumerable<TResult>>> selector)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		return source.Provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TResult)), source.Expression, selector));
	}

	public static IQbservable<TResult> SelectMany<TSource, TTaskResult, TResult>(this IQbservable<TSource> source, Expression<Func<TSource, Task<TTaskResult>>> taskSelector, Expression<Func<TSource, TTaskResult, TResult>> resultSelector)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (taskSelector == null)
		{
			throw new ArgumentNullException("taskSelector");
		}
		if (resultSelector == null)
		{
			throw new ArgumentNullException("resultSelector");
		}
		return source.Provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TTaskResult), typeof(TResult)), source.Expression, taskSelector, resultSelector));
	}

	public static IQbservable<TResult> SelectMany<TSource, TTaskResult, TResult>(this IQbservable<TSource> source, Expression<Func<TSource, int, Task<TTaskResult>>> taskSelector, Expression<Func<TSource, int, TTaskResult, TResult>> resultSelector)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (taskSelector == null)
		{
			throw new ArgumentNullException("taskSelector");
		}
		if (resultSelector == null)
		{
			throw new ArgumentNullException("resultSelector");
		}
		return source.Provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TTaskResult), typeof(TResult)), source.Expression, taskSelector, resultSelector));
	}

	public static IQbservable<TResult> SelectMany<TSource, TTaskResult, TResult>(this IQbservable<TSource> source, Expression<Func<TSource, CancellationToken, Task<TTaskResult>>> taskSelector, Expression<Func<TSource, TTaskResult, TResult>> resultSelector)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (taskSelector == null)
		{
			throw new ArgumentNullException("taskSelector");
		}
		if (resultSelector == null)
		{
			throw new ArgumentNullException("resultSelector");
		}
		return source.Provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TTaskResult), typeof(TResult)), source.Expression, taskSelector, resultSelector));
	}

	public static IQbservable<TResult> SelectMany<TSource, TTaskResult, TResult>(this IQbservable<TSource> source, Expression<Func<TSource, int, CancellationToken, Task<TTaskResult>>> taskSelector, Expression<Func<TSource, int, TTaskResult, TResult>> resultSelector)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (taskSelector == null)
		{
			throw new ArgumentNullException("taskSelector");
		}
		if (resultSelector == null)
		{
			throw new ArgumentNullException("resultSelector");
		}
		return source.Provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TTaskResult), typeof(TResult)), source.Expression, taskSelector, resultSelector));
	}

	public static IQbservable<bool> SequenceEqual<TSource>(this IQbservable<TSource> first, IObservable<TSource> second)
	{
		if (first == null)
		{
			throw new ArgumentNullException("first");
		}
		if (second == null)
		{
			throw new ArgumentNullException("second");
		}
		return first.Provider.CreateQuery<bool>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), first.Expression, GetSourceExpression(second)));
	}

	public static IQbservable<bool> SequenceEqual<TSource>(this IQbservable<TSource> first, IEnumerable<TSource> second)
	{
		if (first == null)
		{
			throw new ArgumentNullException("first");
		}
		if (second == null)
		{
			throw new ArgumentNullException("second");
		}
		return first.Provider.CreateQuery<bool>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), first.Expression, GetSourceExpression(second)));
	}

	public static IQbservable<bool> SequenceEqual<TSource>(this IQbservable<TSource> first, IObservable<TSource> second, IEqualityComparer<TSource> comparer)
	{
		if (first == null)
		{
			throw new ArgumentNullException("first");
		}
		if (second == null)
		{
			throw new ArgumentNullException("second");
		}
		if (comparer == null)
		{
			throw new ArgumentNullException("comparer");
		}
		return first.Provider.CreateQuery<bool>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), first.Expression, GetSourceExpression(second), Expression.Constant(comparer, typeof(IEqualityComparer<TSource>))));
	}

	public static IQbservable<bool> SequenceEqual<TSource>(this IQbservable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
	{
		if (first == null)
		{
			throw new ArgumentNullException("first");
		}
		if (second == null)
		{
			throw new ArgumentNullException("second");
		}
		if (comparer == null)
		{
			throw new ArgumentNullException("comparer");
		}
		return first.Provider.CreateQuery<bool>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), first.Expression, GetSourceExpression(second), Expression.Constant(comparer, typeof(IEqualityComparer<TSource>))));
	}

	public static IQbservable<TSource> SingleAsync<TSource>(this IQbservable<TSource> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression));
	}

	public static IQbservable<TSource> SingleAsync<TSource>(this IQbservable<TSource> source, Expression<Func<TSource, bool>> predicate)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (predicate == null)
		{
			throw new ArgumentNullException("predicate");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, predicate));
	}

	public static IQbservable<TSource?> SingleOrDefaultAsync<TSource>(this IQbservable<TSource> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression));
	}

	public static IQbservable<TSource> SingleOrDefaultAsync<TSource>(this IQbservable<TSource> source, Expression<Func<TSource, bool>> predicate)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (predicate == null)
		{
			throw new ArgumentNullException("predicate");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, predicate));
	}

	public static IQbservable<TSource> Skip<TSource>(this IQbservable<TSource> source, int count)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, Expression.Constant(count, typeof(int))));
	}

	public static IQbservable<TSource> Skip<TSource>(this IQbservable<TSource> source, TimeSpan duration)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, Expression.Constant(duration, typeof(TimeSpan))));
	}

	public static IQbservable<TSource> Skip<TSource>(this IQbservable<TSource> source, TimeSpan duration, IScheduler scheduler)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, Expression.Constant(duration, typeof(TimeSpan)), Expression.Constant(scheduler, typeof(IScheduler))));
	}

	public static IQbservable<TSource> SkipLast<TSource>(this IQbservable<TSource> source, int count)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, Expression.Constant(count, typeof(int))));
	}

	public static IQbservable<TSource> SkipLast<TSource>(this IQbservable<TSource> source, TimeSpan duration)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, Expression.Constant(duration, typeof(TimeSpan))));
	}

	public static IQbservable<TSource> SkipLast<TSource>(this IQbservable<TSource> source, TimeSpan duration, IScheduler scheduler)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, Expression.Constant(duration, typeof(TimeSpan)), Expression.Constant(scheduler, typeof(IScheduler))));
	}

	public static IQbservable<TSource> SkipUntil<TSource>(this IQbservable<TSource> source, DateTimeOffset startTime)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, Expression.Constant(startTime, typeof(DateTimeOffset))));
	}

	public static IQbservable<TSource> SkipUntil<TSource>(this IQbservable<TSource> source, DateTimeOffset startTime, IScheduler scheduler)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, Expression.Constant(startTime, typeof(DateTimeOffset)), Expression.Constant(scheduler, typeof(IScheduler))));
	}

	public static IQbservable<TSource> SkipUntil<TSource, TOther>(this IQbservable<TSource> source, IObservable<TOther> other)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TOther)), source.Expression, GetSourceExpression(other)));
	}

	public static IQbservable<TSource> SkipWhile<TSource>(this IQbservable<TSource> source, Expression<Func<TSource, bool>> predicate)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (predicate == null)
		{
			throw new ArgumentNullException("predicate");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, predicate));
	}

	public static IQbservable<TSource> SkipWhile<TSource>(this IQbservable<TSource> source, Expression<Func<TSource, int, bool>> predicate)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (predicate == null)
		{
			throw new ArgumentNullException("predicate");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, predicate));
	}

	public static IQbservable<Unit> Start(this IQbservableProvider provider, Expression<Action> action)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		return provider.CreateQuery<Unit>(Expression.Call(null, (MethodInfo)MethodBase.GetCurrentMethod(), Expression.Constant(provider, typeof(IQbservableProvider)), action));
	}

	public static IQbservable<Unit> Start(this IQbservableProvider provider, Expression<Action> action, IScheduler scheduler)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return provider.CreateQuery<Unit>(Expression.Call(null, (MethodInfo)MethodBase.GetCurrentMethod(), Expression.Constant(provider, typeof(IQbservableProvider)), action, Expression.Constant(scheduler, typeof(IScheduler))));
	}

	public static IQbservable<TResult> Start<TResult>(this IQbservableProvider provider, Expression<Func<TResult>> function)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (function == null)
		{
			throw new ArgumentNullException("function");
		}
		return provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TResult)), Expression.Constant(provider, typeof(IQbservableProvider)), function));
	}

	public static IQbservable<TResult> Start<TResult>(this IQbservableProvider provider, Expression<Func<TResult>> function, IScheduler scheduler)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (function == null)
		{
			throw new ArgumentNullException("function");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TResult)), Expression.Constant(provider, typeof(IQbservableProvider)), function, Expression.Constant(scheduler, typeof(IScheduler))));
	}

	public static IQbservable<Unit> StartAsync(this IQbservableProvider provider, Expression<Func<Task>> actionAsync)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (actionAsync == null)
		{
			throw new ArgumentNullException("actionAsync");
		}
		return provider.CreateQuery<Unit>(Expression.Call(null, (MethodInfo)MethodBase.GetCurrentMethod(), Expression.Constant(provider, typeof(IQbservableProvider)), actionAsync));
	}

	public static IQbservable<Unit> StartAsync(this IQbservableProvider provider, Expression<Func<Task>> actionAsync, TaskObservationOptions options)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (actionAsync == null)
		{
			throw new ArgumentNullException("actionAsync");
		}
		if (options == null)
		{
			throw new ArgumentNullException("options");
		}
		return provider.CreateQuery<Unit>(Expression.Call(null, (MethodInfo)MethodBase.GetCurrentMethod(), Expression.Constant(provider, typeof(IQbservableProvider)), actionAsync, Expression.Constant(options, typeof(TaskObservationOptions))));
	}

	public static IQbservable<Unit> StartAsync(this IQbservableProvider provider, Expression<Func<Task>> actionAsync, IScheduler scheduler)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (actionAsync == null)
		{
			throw new ArgumentNullException("actionAsync");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return provider.CreateQuery<Unit>(Expression.Call(null, (MethodInfo)MethodBase.GetCurrentMethod(), Expression.Constant(provider, typeof(IQbservableProvider)), actionAsync, Expression.Constant(scheduler, typeof(IScheduler))));
	}

	public static IQbservable<Unit> StartAsync(this IQbservableProvider provider, Expression<Func<CancellationToken, Task>> actionAsync)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (actionAsync == null)
		{
			throw new ArgumentNullException("actionAsync");
		}
		return provider.CreateQuery<Unit>(Expression.Call(null, (MethodInfo)MethodBase.GetCurrentMethod(), Expression.Constant(provider, typeof(IQbservableProvider)), actionAsync));
	}

	public static IQbservable<Unit> StartAsync(this IQbservableProvider provider, Expression<Func<CancellationToken, Task>> actionAsync, TaskObservationOptions options)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (actionAsync == null)
		{
			throw new ArgumentNullException("actionAsync");
		}
		if (options == null)
		{
			throw new ArgumentNullException("options");
		}
		return provider.CreateQuery<Unit>(Expression.Call(null, (MethodInfo)MethodBase.GetCurrentMethod(), Expression.Constant(provider, typeof(IQbservableProvider)), actionAsync, Expression.Constant(options, typeof(TaskObservationOptions))));
	}

	public static IQbservable<Unit> StartAsync(this IQbservableProvider provider, Expression<Func<CancellationToken, Task>> actionAsync, IScheduler scheduler)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (actionAsync == null)
		{
			throw new ArgumentNullException("actionAsync");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return provider.CreateQuery<Unit>(Expression.Call(null, (MethodInfo)MethodBase.GetCurrentMethod(), Expression.Constant(provider, typeof(IQbservableProvider)), actionAsync, Expression.Constant(scheduler, typeof(IScheduler))));
	}

	public static IQbservable<TResult> StartAsync<TResult>(this IQbservableProvider provider, Expression<Func<Task<TResult>>> functionAsync)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (functionAsync == null)
		{
			throw new ArgumentNullException("functionAsync");
		}
		return provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TResult)), Expression.Constant(provider, typeof(IQbservableProvider)), functionAsync));
	}

	public static IQbservable<TResult> StartAsync<TResult>(this IQbservableProvider provider, Expression<Func<CancellationToken, Task<TResult>>> functionAsync)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (functionAsync == null)
		{
			throw new ArgumentNullException("functionAsync");
		}
		return provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TResult)), Expression.Constant(provider, typeof(IQbservableProvider)), functionAsync));
	}

	public static IQbservable<TResult> StartAsync<TResult>(this IQbservableProvider provider, Expression<Func<CancellationToken, Task<TResult>>> functionAsync, TaskObservationOptions options)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (functionAsync == null)
		{
			throw new ArgumentNullException("functionAsync");
		}
		if (options == null)
		{
			throw new ArgumentNullException("options");
		}
		return provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TResult)), Expression.Constant(provider, typeof(IQbservableProvider)), functionAsync, Expression.Constant(options, typeof(TaskObservationOptions))));
	}

	public static IQbservable<TResult> StartAsync<TResult>(this IQbservableProvider provider, Expression<Func<Task<TResult>>> functionAsync, TaskObservationOptions options)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (functionAsync == null)
		{
			throw new ArgumentNullException("functionAsync");
		}
		if (options == null)
		{
			throw new ArgumentNullException("options");
		}
		return provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TResult)), Expression.Constant(provider, typeof(IQbservableProvider)), functionAsync, Expression.Constant(options, typeof(TaskObservationOptions))));
	}

	public static IQbservable<TResult> StartAsync<TResult>(this IQbservableProvider provider, Expression<Func<CancellationToken, Task<TResult>>> functionAsync, IScheduler scheduler)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (functionAsync == null)
		{
			throw new ArgumentNullException("functionAsync");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TResult)), Expression.Constant(provider, typeof(IQbservableProvider)), functionAsync, Expression.Constant(scheduler, typeof(IScheduler))));
	}

	public static IQbservable<TResult> StartAsync<TResult>(this IQbservableProvider provider, Expression<Func<Task<TResult>>> functionAsync, IScheduler scheduler)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (functionAsync == null)
		{
			throw new ArgumentNullException("functionAsync");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TResult)), Expression.Constant(provider, typeof(IQbservableProvider)), functionAsync, Expression.Constant(scheduler, typeof(IScheduler))));
	}

	public static IQbservable<TSource> StartWith<TSource>(this IQbservable<TSource> source, IScheduler scheduler, params TSource[] values)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		if (values == null)
		{
			throw new ArgumentNullException("values");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, Expression.Constant(scheduler, typeof(IScheduler)), Expression.Constant(values, typeof(TSource[]))));
	}

	public static IQbservable<TSource> StartWith<TSource>(this IQbservable<TSource> source, IScheduler scheduler, IEnumerable<TSource> values)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		if (values == null)
		{
			throw new ArgumentNullException("values");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, Expression.Constant(scheduler, typeof(IScheduler)), GetSourceExpression(values)));
	}

	public static IQbservable<TSource> StartWith<TSource>(this IQbservable<TSource> source, params TSource[] values)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (values == null)
		{
			throw new ArgumentNullException("values");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, Expression.Constant(values, typeof(TSource[]))));
	}

	public static IQbservable<TSource> StartWith<TSource>(this IQbservable<TSource> source, IEnumerable<TSource> values)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (values == null)
		{
			throw new ArgumentNullException("values");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, GetSourceExpression(values)));
	}

	public static IQbservable<TSource> SubscribeOn<TSource>(this IQbservable<TSource> source, SynchronizationContext context)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (context == null)
		{
			throw new ArgumentNullException("context");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, Expression.Constant(context, typeof(SynchronizationContext))));
	}

	public static IQbservable<TSource> SubscribeOn<TSource>(this IQbservable<TSource> source, IScheduler scheduler)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, Expression.Constant(scheduler, typeof(IScheduler))));
	}

	public static IQbservable<decimal> Sum(this IQbservable<decimal> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<decimal>(Expression.Call(null, (MethodInfo)MethodBase.GetCurrentMethod(), source.Expression));
	}

	public static IQbservable<double> Sum(this IQbservable<double> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<double>(Expression.Call(null, (MethodInfo)MethodBase.GetCurrentMethod(), source.Expression));
	}

	public static IQbservable<int> Sum(this IQbservable<int> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<int>(Expression.Call(null, (MethodInfo)MethodBase.GetCurrentMethod(), source.Expression));
	}

	public static IQbservable<long> Sum(this IQbservable<long> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<long>(Expression.Call(null, (MethodInfo)MethodBase.GetCurrentMethod(), source.Expression));
	}

	public static IQbservable<decimal?> Sum(this IQbservable<decimal?> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<decimal?>(Expression.Call(null, (MethodInfo)MethodBase.GetCurrentMethod(), source.Expression));
	}

	public static IQbservable<double?> Sum(this IQbservable<double?> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<double?>(Expression.Call(null, (MethodInfo)MethodBase.GetCurrentMethod(), source.Expression));
	}

	public static IQbservable<int?> Sum(this IQbservable<int?> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<int?>(Expression.Call(null, (MethodInfo)MethodBase.GetCurrentMethod(), source.Expression));
	}

	public static IQbservable<long?> Sum(this IQbservable<long?> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<long?>(Expression.Call(null, (MethodInfo)MethodBase.GetCurrentMethod(), source.Expression));
	}

	public static IQbservable<float?> Sum(this IQbservable<float?> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<float?>(Expression.Call(null, (MethodInfo)MethodBase.GetCurrentMethod(), source.Expression));
	}

	public static IQbservable<float> Sum(this IQbservable<float> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<float>(Expression.Call(null, (MethodInfo)MethodBase.GetCurrentMethod(), source.Expression));
	}

	public static IQbservable<double> Sum<TSource>(this IQbservable<TSource> source, Expression<Func<TSource, double>> selector)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		return source.Provider.CreateQuery<double>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, selector));
	}

	public static IQbservable<float> Sum<TSource>(this IQbservable<TSource> source, Expression<Func<TSource, float>> selector)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		return source.Provider.CreateQuery<float>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, selector));
	}

	public static IQbservable<decimal> Sum<TSource>(this IQbservable<TSource> source, Expression<Func<TSource, decimal>> selector)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		return source.Provider.CreateQuery<decimal>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, selector));
	}

	public static IQbservable<int> Sum<TSource>(this IQbservable<TSource> source, Expression<Func<TSource, int>> selector)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		return source.Provider.CreateQuery<int>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, selector));
	}

	public static IQbservable<long> Sum<TSource>(this IQbservable<TSource> source, Expression<Func<TSource, long>> selector)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		return source.Provider.CreateQuery<long>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, selector));
	}

	public static IQbservable<double?> Sum<TSource>(this IQbservable<TSource> source, Expression<Func<TSource, double?>> selector)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		return source.Provider.CreateQuery<double?>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, selector));
	}

	public static IQbservable<float?> Sum<TSource>(this IQbservable<TSource> source, Expression<Func<TSource, float?>> selector)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		return source.Provider.CreateQuery<float?>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, selector));
	}

	public static IQbservable<decimal?> Sum<TSource>(this IQbservable<TSource> source, Expression<Func<TSource, decimal?>> selector)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		return source.Provider.CreateQuery<decimal?>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, selector));
	}

	public static IQbservable<int?> Sum<TSource>(this IQbservable<TSource> source, Expression<Func<TSource, int?>> selector)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		return source.Provider.CreateQuery<int?>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, selector));
	}

	public static IQbservable<long?> Sum<TSource>(this IQbservable<TSource> source, Expression<Func<TSource, long?>> selector)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		return source.Provider.CreateQuery<long?>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, selector));
	}

	public static IQbservable<TSource> Switch<TSource>(this IQbservable<IObservable<TSource>> sources)
	{
		if (sources == null)
		{
			throw new ArgumentNullException("sources");
		}
		return sources.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), sources.Expression));
	}

	public static IQbservable<TSource> Switch<TSource>(this IQbservable<Task<TSource>> sources)
	{
		if (sources == null)
		{
			throw new ArgumentNullException("sources");
		}
		return sources.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), sources.Expression));
	}

	public static IQbservable<TSource> Synchronize<TSource>(this IQbservable<TSource> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression));
	}

	public static IQbservable<TSource> Synchronize<TSource>(this IQbservable<TSource> source, object gate)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (gate == null)
		{
			throw new ArgumentNullException("gate");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, Expression.Constant(gate, typeof(object))));
	}

	public static IQbservable<TSource> Take<TSource>(this IQbservable<TSource> source, int count)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, Expression.Constant(count, typeof(int))));
	}

	public static IQbservable<TSource> Take<TSource>(this IQbservable<TSource> source, int count, IScheduler scheduler)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, Expression.Constant(count, typeof(int)), Expression.Constant(scheduler, typeof(IScheduler))));
	}

	public static IQbservable<TSource> Take<TSource>(this IQbservable<TSource> source, TimeSpan duration)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, Expression.Constant(duration, typeof(TimeSpan))));
	}

	public static IQbservable<TSource> Take<TSource>(this IQbservable<TSource> source, TimeSpan duration, IScheduler scheduler)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, Expression.Constant(duration, typeof(TimeSpan)), Expression.Constant(scheduler, typeof(IScheduler))));
	}

	public static IQbservable<TSource> TakeLast<TSource>(this IQbservable<TSource> source, int count)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, Expression.Constant(count, typeof(int))));
	}

	public static IQbservable<TSource> TakeLast<TSource>(this IQbservable<TSource> source, int count, IScheduler scheduler)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, Expression.Constant(count, typeof(int)), Expression.Constant(scheduler, typeof(IScheduler))));
	}

	public static IQbservable<TSource> TakeLast<TSource>(this IQbservable<TSource> source, TimeSpan duration)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, Expression.Constant(duration, typeof(TimeSpan))));
	}

	public static IQbservable<TSource> TakeLast<TSource>(this IQbservable<TSource> source, TimeSpan duration, IScheduler scheduler)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, Expression.Constant(duration, typeof(TimeSpan)), Expression.Constant(scheduler, typeof(IScheduler))));
	}

	public static IQbservable<TSource> TakeLast<TSource>(this IQbservable<TSource> source, TimeSpan duration, IScheduler timerScheduler, IScheduler loopScheduler)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (timerScheduler == null)
		{
			throw new ArgumentNullException("timerScheduler");
		}
		if (loopScheduler == null)
		{
			throw new ArgumentNullException("loopScheduler");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, Expression.Constant(duration, typeof(TimeSpan)), Expression.Constant(timerScheduler, typeof(IScheduler)), Expression.Constant(loopScheduler, typeof(IScheduler))));
	}

	public static IQbservable<IList<TSource>> TakeLastBuffer<TSource>(this IQbservable<TSource> source, int count)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<IList<TSource>>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, Expression.Constant(count, typeof(int))));
	}

	public static IQbservable<IList<TSource>> TakeLastBuffer<TSource>(this IQbservable<TSource> source, TimeSpan duration)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<IList<TSource>>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, Expression.Constant(duration, typeof(TimeSpan))));
	}

	public static IQbservable<IList<TSource>> TakeLastBuffer<TSource>(this IQbservable<TSource> source, TimeSpan duration, IScheduler scheduler)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return source.Provider.CreateQuery<IList<TSource>>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, Expression.Constant(duration, typeof(TimeSpan)), Expression.Constant(scheduler, typeof(IScheduler))));
	}

	public static IQbservable<TSource> TakeUntil<TSource>(this IQbservable<TSource> source, CancellationToken cancellationToken)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, Expression.Constant(cancellationToken, typeof(CancellationToken))));
	}

	public static IQbservable<TSource> TakeUntil<TSource>(this IQbservable<TSource> source, DateTimeOffset endTime)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, Expression.Constant(endTime, typeof(DateTimeOffset))));
	}

	public static IQbservable<TSource> TakeUntil<TSource>(this IQbservable<TSource> source, DateTimeOffset endTime, IScheduler scheduler)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, Expression.Constant(endTime, typeof(DateTimeOffset)), Expression.Constant(scheduler, typeof(IScheduler))));
	}

	public static IQbservable<TSource> TakeUntil<TSource, TOther>(this IQbservable<TSource> source, IObservable<TOther> other)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TOther)), source.Expression, GetSourceExpression(other)));
	}

	public static IQbservable<TSource> TakeUntil<TSource>(this IQbservable<TSource> source, Expression<Func<TSource, bool>> stopPredicate)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (stopPredicate == null)
		{
			throw new ArgumentNullException("stopPredicate");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, stopPredicate));
	}

	public static IQbservable<TSource> TakeWhile<TSource>(this IQbservable<TSource> source, Expression<Func<TSource, bool>> predicate)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (predicate == null)
		{
			throw new ArgumentNullException("predicate");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, predicate));
	}

	public static IQbservable<TSource> TakeWhile<TSource>(this IQbservable<TSource> source, Expression<Func<TSource, int, bool>> predicate)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (predicate == null)
		{
			throw new ArgumentNullException("predicate");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, predicate));
	}

	public static IQbservable<TSource> Throttle<TSource>(this IQbservable<TSource> source, TimeSpan dueTime)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, Expression.Constant(dueTime, typeof(TimeSpan))));
	}

	public static IQbservable<TSource> Throttle<TSource>(this IQbservable<TSource> source, TimeSpan dueTime, IScheduler scheduler)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, Expression.Constant(dueTime, typeof(TimeSpan)), Expression.Constant(scheduler, typeof(IScheduler))));
	}

	public static IQbservable<TSource> Throttle<TSource, TThrottle>(this IQbservable<TSource> source, Expression<Func<TSource, IObservable<TThrottle>>> throttleDurationSelector)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (throttleDurationSelector == null)
		{
			throw new ArgumentNullException("throttleDurationSelector");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TThrottle)), source.Expression, throttleDurationSelector));
	}

	public static IQbservable<TResult> Throw<TResult>(this IQbservableProvider provider, Exception exception)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (exception == null)
		{
			throw new ArgumentNullException("exception");
		}
		return provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TResult)), Expression.Constant(provider, typeof(IQbservableProvider)), Expression.Constant(exception, typeof(Exception))));
	}

	public static IQbservable<TResult> Throw<TResult>(this IQbservableProvider provider, Exception exception, IScheduler scheduler)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (exception == null)
		{
			throw new ArgumentNullException("exception");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TResult)), Expression.Constant(provider, typeof(IQbservableProvider)), Expression.Constant(exception, typeof(Exception)), Expression.Constant(scheduler, typeof(IScheduler))));
	}

	public static IQbservable<TResult> Throw<TResult>(this IQbservableProvider provider, Exception exception, IScheduler scheduler, TResult witness)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (exception == null)
		{
			throw new ArgumentNullException("exception");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TResult)), Expression.Constant(provider, typeof(IQbservableProvider)), Expression.Constant(exception, typeof(Exception)), Expression.Constant(scheduler, typeof(IScheduler)), Expression.Constant(witness, typeof(TResult))));
	}

	public static IQbservable<TResult> Throw<TResult>(this IQbservableProvider provider, Exception exception, TResult witness)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (exception == null)
		{
			throw new ArgumentNullException("exception");
		}
		return provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TResult)), Expression.Constant(provider, typeof(IQbservableProvider)), Expression.Constant(exception, typeof(Exception)), Expression.Constant(witness, typeof(TResult))));
	}

	public static IQbservable<TimeInterval<TSource>> TimeInterval<TSource>(this IQbservable<TSource> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<TimeInterval<TSource>>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression));
	}

	public static IQbservable<TimeInterval<TSource>> TimeInterval<TSource>(this IQbservable<TSource> source, IScheduler scheduler)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return source.Provider.CreateQuery<TimeInterval<TSource>>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, Expression.Constant(scheduler, typeof(IScheduler))));
	}

	public static IQbservable<TSource> Timeout<TSource>(this IQbservable<TSource> source, DateTimeOffset dueTime)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, Expression.Constant(dueTime, typeof(DateTimeOffset))));
	}

	public static IQbservable<TSource> Timeout<TSource>(this IQbservable<TSource> source, DateTimeOffset dueTime, IObservable<TSource> other)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, Expression.Constant(dueTime, typeof(DateTimeOffset)), GetSourceExpression(other)));
	}

	public static IQbservable<TSource> Timeout<TSource>(this IQbservable<TSource> source, DateTimeOffset dueTime, IObservable<TSource> other, IScheduler scheduler)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, Expression.Constant(dueTime, typeof(DateTimeOffset)), GetSourceExpression(other), Expression.Constant(scheduler, typeof(IScheduler))));
	}

	public static IQbservable<TSource> Timeout<TSource>(this IQbservable<TSource> source, DateTimeOffset dueTime, IScheduler scheduler)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, Expression.Constant(dueTime, typeof(DateTimeOffset)), Expression.Constant(scheduler, typeof(IScheduler))));
	}

	public static IQbservable<TSource> Timeout<TSource>(this IQbservable<TSource> source, TimeSpan dueTime)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, Expression.Constant(dueTime, typeof(TimeSpan))));
	}

	public static IQbservable<TSource> Timeout<TSource>(this IQbservable<TSource> source, TimeSpan dueTime, IObservable<TSource> other)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, Expression.Constant(dueTime, typeof(TimeSpan)), GetSourceExpression(other)));
	}

	public static IQbservable<TSource> Timeout<TSource>(this IQbservable<TSource> source, TimeSpan dueTime, IObservable<TSource> other, IScheduler scheduler)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, Expression.Constant(dueTime, typeof(TimeSpan)), GetSourceExpression(other), Expression.Constant(scheduler, typeof(IScheduler))));
	}

	public static IQbservable<TSource> Timeout<TSource>(this IQbservable<TSource> source, TimeSpan dueTime, IScheduler scheduler)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, Expression.Constant(dueTime, typeof(TimeSpan)), Expression.Constant(scheduler, typeof(IScheduler))));
	}

	public static IQbservable<TSource> Timeout<TSource, TTimeout>(this IQbservable<TSource> source, IObservable<TTimeout> firstTimeout, Expression<Func<TSource, IObservable<TTimeout>>> timeoutDurationSelector)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (firstTimeout == null)
		{
			throw new ArgumentNullException("firstTimeout");
		}
		if (timeoutDurationSelector == null)
		{
			throw new ArgumentNullException("timeoutDurationSelector");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TTimeout)), source.Expression, GetSourceExpression(firstTimeout), timeoutDurationSelector));
	}

	public static IQbservable<TSource> Timeout<TSource, TTimeout>(this IQbservable<TSource> source, IObservable<TTimeout> firstTimeout, Expression<Func<TSource, IObservable<TTimeout>>> timeoutDurationSelector, IObservable<TSource> other)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (firstTimeout == null)
		{
			throw new ArgumentNullException("firstTimeout");
		}
		if (timeoutDurationSelector == null)
		{
			throw new ArgumentNullException("timeoutDurationSelector");
		}
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TTimeout)), source.Expression, GetSourceExpression(firstTimeout), timeoutDurationSelector, GetSourceExpression(other)));
	}

	public static IQbservable<TSource> Timeout<TSource, TTimeout>(this IQbservable<TSource> source, Expression<Func<TSource, IObservable<TTimeout>>> timeoutDurationSelector)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (timeoutDurationSelector == null)
		{
			throw new ArgumentNullException("timeoutDurationSelector");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TTimeout)), source.Expression, timeoutDurationSelector));
	}

	public static IQbservable<TSource> Timeout<TSource, TTimeout>(this IQbservable<TSource> source, Expression<Func<TSource, IObservable<TTimeout>>> timeoutDurationSelector, IObservable<TSource> other)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (timeoutDurationSelector == null)
		{
			throw new ArgumentNullException("timeoutDurationSelector");
		}
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TTimeout)), source.Expression, timeoutDurationSelector, GetSourceExpression(other)));
	}

	public static IQbservable<long> Timer(this IQbservableProvider provider, DateTimeOffset dueTime)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		return provider.CreateQuery<long>(Expression.Call(null, (MethodInfo)MethodBase.GetCurrentMethod(), Expression.Constant(provider, typeof(IQbservableProvider)), Expression.Constant(dueTime, typeof(DateTimeOffset))));
	}

	public static IQbservable<long> Timer(this IQbservableProvider provider, DateTimeOffset dueTime, TimeSpan period)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		return provider.CreateQuery<long>(Expression.Call(null, (MethodInfo)MethodBase.GetCurrentMethod(), Expression.Constant(provider, typeof(IQbservableProvider)), Expression.Constant(dueTime, typeof(DateTimeOffset)), Expression.Constant(period, typeof(TimeSpan))));
	}

	public static IQbservable<long> Timer(this IQbservableProvider provider, DateTimeOffset dueTime, TimeSpan period, IScheduler scheduler)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return provider.CreateQuery<long>(Expression.Call(null, (MethodInfo)MethodBase.GetCurrentMethod(), Expression.Constant(provider, typeof(IQbservableProvider)), Expression.Constant(dueTime, typeof(DateTimeOffset)), Expression.Constant(period, typeof(TimeSpan)), Expression.Constant(scheduler, typeof(IScheduler))));
	}

	public static IQbservable<long> Timer(this IQbservableProvider provider, DateTimeOffset dueTime, IScheduler scheduler)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return provider.CreateQuery<long>(Expression.Call(null, (MethodInfo)MethodBase.GetCurrentMethod(), Expression.Constant(provider, typeof(IQbservableProvider)), Expression.Constant(dueTime, typeof(DateTimeOffset)), Expression.Constant(scheduler, typeof(IScheduler))));
	}

	public static IQbservable<long> Timer(this IQbservableProvider provider, TimeSpan dueTime)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		return provider.CreateQuery<long>(Expression.Call(null, (MethodInfo)MethodBase.GetCurrentMethod(), Expression.Constant(provider, typeof(IQbservableProvider)), Expression.Constant(dueTime, typeof(TimeSpan))));
	}

	public static IQbservable<long> Timer(this IQbservableProvider provider, TimeSpan dueTime, TimeSpan period)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		return provider.CreateQuery<long>(Expression.Call(null, (MethodInfo)MethodBase.GetCurrentMethod(), Expression.Constant(provider, typeof(IQbservableProvider)), Expression.Constant(dueTime, typeof(TimeSpan)), Expression.Constant(period, typeof(TimeSpan))));
	}

	public static IQbservable<long> Timer(this IQbservableProvider provider, TimeSpan dueTime, TimeSpan period, IScheduler scheduler)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return provider.CreateQuery<long>(Expression.Call(null, (MethodInfo)MethodBase.GetCurrentMethod(), Expression.Constant(provider, typeof(IQbservableProvider)), Expression.Constant(dueTime, typeof(TimeSpan)), Expression.Constant(period, typeof(TimeSpan)), Expression.Constant(scheduler, typeof(IScheduler))));
	}

	public static IQbservable<long> Timer(this IQbservableProvider provider, TimeSpan dueTime, IScheduler scheduler)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return provider.CreateQuery<long>(Expression.Call(null, (MethodInfo)MethodBase.GetCurrentMethod(), Expression.Constant(provider, typeof(IQbservableProvider)), Expression.Constant(dueTime, typeof(TimeSpan)), Expression.Constant(scheduler, typeof(IScheduler))));
	}

	public static IQbservable<Timestamped<TSource>> Timestamp<TSource>(this IQbservable<TSource> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<Timestamped<TSource>>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression));
	}

	public static IQbservable<Timestamped<TSource>> Timestamp<TSource>(this IQbservable<TSource> source, IScheduler scheduler)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return source.Provider.CreateQuery<Timestamped<TSource>>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, Expression.Constant(scheduler, typeof(IScheduler))));
	}

	public static IQbservable<TSource[]> ToArray<TSource>(this IQbservable<TSource> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<TSource[]>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression));
	}

	public static IQbservable<IDictionary<TKey, TSource>> ToDictionary<TSource, TKey>(this IQbservable<TSource> source, Expression<Func<TSource, TKey>> keySelector) where TKey : notnull
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (keySelector == null)
		{
			throw new ArgumentNullException("keySelector");
		}
		return source.Provider.CreateQuery<IDictionary<TKey, TSource>>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TKey)), source.Expression, keySelector));
	}

	public static IQbservable<IDictionary<TKey, TSource>> ToDictionary<TSource, TKey>(this IQbservable<TSource> source, Expression<Func<TSource, TKey>> keySelector, IEqualityComparer<TKey> comparer) where TKey : notnull
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (keySelector == null)
		{
			throw new ArgumentNullException("keySelector");
		}
		if (comparer == null)
		{
			throw new ArgumentNullException("comparer");
		}
		return source.Provider.CreateQuery<IDictionary<TKey, TSource>>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TKey)), source.Expression, keySelector, Expression.Constant(comparer, typeof(IEqualityComparer<TKey>))));
	}

	public static IQbservable<IDictionary<TKey, TElement>> ToDictionary<TSource, TKey, TElement>(this IQbservable<TSource> source, Expression<Func<TSource, TKey>> keySelector, Expression<Func<TSource, TElement>> elementSelector) where TKey : notnull
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (keySelector == null)
		{
			throw new ArgumentNullException("keySelector");
		}
		if (elementSelector == null)
		{
			throw new ArgumentNullException("elementSelector");
		}
		return source.Provider.CreateQuery<IDictionary<TKey, TElement>>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TKey), typeof(TElement)), source.Expression, keySelector, elementSelector));
	}

	public static IQbservable<IDictionary<TKey, TElement>> ToDictionary<TSource, TKey, TElement>(this IQbservable<TSource> source, Expression<Func<TSource, TKey>> keySelector, Expression<Func<TSource, TElement>> elementSelector, IEqualityComparer<TKey> comparer) where TKey : notnull
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (keySelector == null)
		{
			throw new ArgumentNullException("keySelector");
		}
		if (elementSelector == null)
		{
			throw new ArgumentNullException("elementSelector");
		}
		if (comparer == null)
		{
			throw new ArgumentNullException("comparer");
		}
		return source.Provider.CreateQuery<IDictionary<TKey, TElement>>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TKey), typeof(TElement)), source.Expression, keySelector, elementSelector, Expression.Constant(comparer, typeof(IEqualityComparer<TKey>))));
	}

	public static IQueryable<TSource> ToQueryable<TSource>(this IQbservable<TSource> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return ((IQueryProvider)source.Provider).CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression));
	}

	public static IQbservable<IList<TSource>> ToList<TSource>(this IQbservable<TSource> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<IList<TSource>>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression));
	}

	public static IQbservable<ILookup<TKey, TSource>> ToLookup<TSource, TKey>(this IQbservable<TSource> source, Expression<Func<TSource, TKey>> keySelector)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (keySelector == null)
		{
			throw new ArgumentNullException("keySelector");
		}
		return source.Provider.CreateQuery<ILookup<TKey, TSource>>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TKey)), source.Expression, keySelector));
	}

	public static IQbservable<ILookup<TKey, TSource>> ToLookup<TSource, TKey>(this IQbservable<TSource> source, Expression<Func<TSource, TKey>> keySelector, IEqualityComparer<TKey> comparer)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (keySelector == null)
		{
			throw new ArgumentNullException("keySelector");
		}
		if (comparer == null)
		{
			throw new ArgumentNullException("comparer");
		}
		return source.Provider.CreateQuery<ILookup<TKey, TSource>>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TKey)), source.Expression, keySelector, Expression.Constant(comparer, typeof(IEqualityComparer<TKey>))));
	}

	public static IQbservable<ILookup<TKey, TElement>> ToLookup<TSource, TKey, TElement>(this IQbservable<TSource> source, Expression<Func<TSource, TKey>> keySelector, Expression<Func<TSource, TElement>> elementSelector)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (keySelector == null)
		{
			throw new ArgumentNullException("keySelector");
		}
		if (elementSelector == null)
		{
			throw new ArgumentNullException("elementSelector");
		}
		return source.Provider.CreateQuery<ILookup<TKey, TElement>>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TKey), typeof(TElement)), source.Expression, keySelector, elementSelector));
	}

	public static IQbservable<ILookup<TKey, TElement>> ToLookup<TSource, TKey, TElement>(this IQbservable<TSource> source, Expression<Func<TSource, TKey>> keySelector, Expression<Func<TSource, TElement>> elementSelector, IEqualityComparer<TKey> comparer)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (keySelector == null)
		{
			throw new ArgumentNullException("keySelector");
		}
		if (elementSelector == null)
		{
			throw new ArgumentNullException("elementSelector");
		}
		if (comparer == null)
		{
			throw new ArgumentNullException("comparer");
		}
		return source.Provider.CreateQuery<ILookup<TKey, TElement>>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TKey), typeof(TElement)), source.Expression, keySelector, elementSelector, Expression.Constant(comparer, typeof(IEqualityComparer<TKey>))));
	}

	public static IQbservable<TSource> ToObservable<TSource>(this IQbservableProvider provider, IEnumerable<TSource> source)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), Expression.Constant(provider, typeof(IQbservableProvider)), GetSourceExpression(source)));
	}

	public static IQbservable<TSource> ToObservable<TSource>(this IQbservableProvider provider, IEnumerable<TSource> source, IScheduler scheduler)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), Expression.Constant(provider, typeof(IQbservableProvider)), GetSourceExpression(source), Expression.Constant(scheduler, typeof(IScheduler))));
	}

	public static IQbservable<TResult> Using<TResult, TResource>(this IQbservableProvider provider, Expression<Func<TResource>> resourceFactory, Expression<Func<TResource, IObservable<TResult>>> observableFactory) where TResource : IDisposable
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (resourceFactory == null)
		{
			throw new ArgumentNullException("resourceFactory");
		}
		if (observableFactory == null)
		{
			throw new ArgumentNullException("observableFactory");
		}
		return provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TResult), typeof(TResource)), Expression.Constant(provider, typeof(IQbservableProvider)), resourceFactory, observableFactory));
	}

	public static IQbservable<TResult> Using<TResult, TResource>(this IQbservableProvider provider, Expression<Func<CancellationToken, Task<TResource>>> resourceFactoryAsync, Expression<Func<TResource, CancellationToken, Task<IObservable<TResult>>>> observableFactoryAsync) where TResource : IDisposable
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (resourceFactoryAsync == null)
		{
			throw new ArgumentNullException("resourceFactoryAsync");
		}
		if (observableFactoryAsync == null)
		{
			throw new ArgumentNullException("observableFactoryAsync");
		}
		return provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TResult), typeof(TResource)), Expression.Constant(provider, typeof(IQbservableProvider)), resourceFactoryAsync, observableFactoryAsync));
	}

	public static IQbservable<TSource> Where<TSource>(this IQbservable<TSource> source, Expression<Func<TSource, bool>> predicate)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (predicate == null)
		{
			throw new ArgumentNullException("predicate");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, predicate));
	}

	public static IQbservable<TSource> Where<TSource>(this IQbservable<TSource> source, Expression<Func<TSource, int, bool>> predicate)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (predicate == null)
		{
			throw new ArgumentNullException("predicate");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, predicate));
	}

	public static IQbservable<TSource> While<TSource>(this IQbservableProvider provider, Expression<Func<bool>> condition, IObservable<TSource> source)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (condition == null)
		{
			throw new ArgumentNullException("condition");
		}
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), Expression.Constant(provider, typeof(IQbservableProvider)), condition, GetSourceExpression(source)));
	}

	public static IQbservable<IObservable<TSource>> Window<TSource>(this IQbservable<TSource> source, int count)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<IObservable<TSource>>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, Expression.Constant(count, typeof(int))));
	}

	public static IQbservable<IObservable<TSource>> Window<TSource>(this IQbservable<TSource> source, int count, int skip)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<IObservable<TSource>>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, Expression.Constant(count, typeof(int)), Expression.Constant(skip, typeof(int))));
	}

	public static IQbservable<IObservable<TSource>> Window<TSource>(this IQbservable<TSource> source, TimeSpan timeSpan)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<IObservable<TSource>>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, Expression.Constant(timeSpan, typeof(TimeSpan))));
	}

	public static IQbservable<IObservable<TSource>> Window<TSource>(this IQbservable<TSource> source, TimeSpan timeSpan, int count)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<IObservable<TSource>>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, Expression.Constant(timeSpan, typeof(TimeSpan)), Expression.Constant(count, typeof(int))));
	}

	public static IQbservable<IObservable<TSource>> Window<TSource>(this IQbservable<TSource> source, TimeSpan timeSpan, int count, IScheduler scheduler)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return source.Provider.CreateQuery<IObservable<TSource>>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, Expression.Constant(timeSpan, typeof(TimeSpan)), Expression.Constant(count, typeof(int)), Expression.Constant(scheduler, typeof(IScheduler))));
	}

	public static IQbservable<IObservable<TSource>> Window<TSource>(this IQbservable<TSource> source, TimeSpan timeSpan, IScheduler scheduler)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return source.Provider.CreateQuery<IObservable<TSource>>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, Expression.Constant(timeSpan, typeof(TimeSpan)), Expression.Constant(scheduler, typeof(IScheduler))));
	}

	public static IQbservable<IObservable<TSource>> Window<TSource>(this IQbservable<TSource> source, TimeSpan timeSpan, TimeSpan timeShift)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return source.Provider.CreateQuery<IObservable<TSource>>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, Expression.Constant(timeSpan, typeof(TimeSpan)), Expression.Constant(timeShift, typeof(TimeSpan))));
	}

	public static IQbservable<IObservable<TSource>> Window<TSource>(this IQbservable<TSource> source, TimeSpan timeSpan, TimeSpan timeShift, IScheduler scheduler)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		return source.Provider.CreateQuery<IObservable<TSource>>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, Expression.Constant(timeSpan, typeof(TimeSpan)), Expression.Constant(timeShift, typeof(TimeSpan)), Expression.Constant(scheduler, typeof(IScheduler))));
	}

	public static IQbservable<IObservable<TSource>> Window<TSource, TWindowBoundary>(this IQbservable<TSource> source, IObservable<TWindowBoundary> windowBoundaries)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (windowBoundaries == null)
		{
			throw new ArgumentNullException("windowBoundaries");
		}
		return source.Provider.CreateQuery<IObservable<TSource>>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TWindowBoundary)), source.Expression, GetSourceExpression(windowBoundaries)));
	}

	public static IQbservable<IObservable<TSource>> Window<TSource, TWindowClosing>(this IQbservable<TSource> source, Expression<Func<IObservable<TWindowClosing>>> windowClosingSelector)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (windowClosingSelector == null)
		{
			throw new ArgumentNullException("windowClosingSelector");
		}
		return source.Provider.CreateQuery<IObservable<TSource>>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TWindowClosing)), source.Expression, windowClosingSelector));
	}

	public static IQbservable<IObservable<TSource>> Window<TSource, TWindowOpening, TWindowClosing>(this IQbservable<TSource> source, IObservable<TWindowOpening> windowOpenings, Expression<Func<TWindowOpening, IObservable<TWindowClosing>>> windowClosingSelector)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (windowOpenings == null)
		{
			throw new ArgumentNullException("windowOpenings");
		}
		if (windowClosingSelector == null)
		{
			throw new ArgumentNullException("windowClosingSelector");
		}
		return source.Provider.CreateQuery<IObservable<TSource>>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TWindowOpening), typeof(TWindowClosing)), source.Expression, GetSourceExpression(windowOpenings), windowClosingSelector));
	}

	public static IQbservable<TResult> WithLatestFrom<TFirst, TSecond, TResult>(this IQbservable<TFirst> first, IObservable<TSecond> second, Expression<Func<TFirst, TSecond, TResult>> resultSelector)
	{
		if (first == null)
		{
			throw new ArgumentNullException("first");
		}
		if (second == null)
		{
			throw new ArgumentNullException("second");
		}
		if (resultSelector == null)
		{
			throw new ArgumentNullException("resultSelector");
		}
		return first.Provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TFirst), typeof(TSecond), typeof(TResult)), first.Expression, GetSourceExpression(second), resultSelector));
	}

	public static IQbservable<IList<TSource>> Zip<TSource>(this IQbservableProvider provider, IEnumerable<IObservable<TSource>> sources)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (sources == null)
		{
			throw new ArgumentNullException("sources");
		}
		return provider.CreateQuery<IList<TSource>>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), Expression.Constant(provider, typeof(IQbservableProvider)), GetSourceExpression(sources)));
	}

	public static IQbservable<IList<TSource>> Zip<TSource>(this IQbservableProvider provider, params IObservable<TSource>[] sources)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (sources == null)
		{
			throw new ArgumentNullException("sources");
		}
		return provider.CreateQuery<IList<TSource>>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), Expression.Constant(provider, typeof(IQbservableProvider)), GetSourceExpression(sources)));
	}

	public static IQbservable<TResult> Zip<TSource, TResult>(this IQbservableProvider provider, IEnumerable<IObservable<TSource>> sources, Expression<Func<IList<TSource>, TResult>> resultSelector)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (sources == null)
		{
			throw new ArgumentNullException("sources");
		}
		if (resultSelector == null)
		{
			throw new ArgumentNullException("resultSelector");
		}
		return provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TResult)), Expression.Constant(provider, typeof(IQbservableProvider)), GetSourceExpression(sources), resultSelector));
	}

	public static IQbservable<TResult> Zip<TSource1, TSource2, TResult>(this IQbservable<TSource1> first, IObservable<TSource2> second, Expression<Func<TSource1, TSource2, TResult>> resultSelector)
	{
		if (first == null)
		{
			throw new ArgumentNullException("first");
		}
		if (second == null)
		{
			throw new ArgumentNullException("second");
		}
		if (resultSelector == null)
		{
			throw new ArgumentNullException("resultSelector");
		}
		return first.Provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource1), typeof(TSource2), typeof(TResult)), first.Expression, GetSourceExpression(second), resultSelector));
	}

	public static IQbservable<TResult> Zip<TSource1, TSource2, TResult>(this IQbservable<TSource1> first, IEnumerable<TSource2> second, Expression<Func<TSource1, TSource2, TResult>> resultSelector)
	{
		if (first == null)
		{
			throw new ArgumentNullException("first");
		}
		if (second == null)
		{
			throw new ArgumentNullException("second");
		}
		if (resultSelector == null)
		{
			throw new ArgumentNullException("resultSelector");
		}
		return first.Provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource1), typeof(TSource2), typeof(TResult)), first.Expression, GetSourceExpression(second), resultSelector));
	}

	public static IQbservable<TResult> Zip<TSource1, TSource2, TSource3, TResult>(this IQbservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, Expression<Func<TSource1, TSource2, TSource3, TResult>> resultSelector)
	{
		if (source1 == null)
		{
			throw new ArgumentNullException("source1");
		}
		if (source2 == null)
		{
			throw new ArgumentNullException("source2");
		}
		if (source3 == null)
		{
			throw new ArgumentNullException("source3");
		}
		if (resultSelector == null)
		{
			throw new ArgumentNullException("resultSelector");
		}
		return source1.Provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource1), typeof(TSource2), typeof(TSource3), typeof(TResult)), source1.Expression, GetSourceExpression(source2), GetSourceExpression(source3), resultSelector));
	}

	public static IQbservable<TResult> Zip<TSource1, TSource2, TSource3, TSource4, TResult>(this IQbservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, IObservable<TSource4> source4, Expression<Func<TSource1, TSource2, TSource3, TSource4, TResult>> resultSelector)
	{
		if (source1 == null)
		{
			throw new ArgumentNullException("source1");
		}
		if (source2 == null)
		{
			throw new ArgumentNullException("source2");
		}
		if (source3 == null)
		{
			throw new ArgumentNullException("source3");
		}
		if (source4 == null)
		{
			throw new ArgumentNullException("source4");
		}
		if (resultSelector == null)
		{
			throw new ArgumentNullException("resultSelector");
		}
		return source1.Provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource1), typeof(TSource2), typeof(TSource3), typeof(TSource4), typeof(TResult)), source1.Expression, GetSourceExpression(source2), GetSourceExpression(source3), GetSourceExpression(source4), resultSelector));
	}

	public static IQbservable<TResult> Zip<TSource1, TSource2, TSource3, TSource4, TSource5, TResult>(this IQbservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, IObservable<TSource4> source4, IObservable<TSource5> source5, Expression<Func<TSource1, TSource2, TSource3, TSource4, TSource5, TResult>> resultSelector)
	{
		if (source1 == null)
		{
			throw new ArgumentNullException("source1");
		}
		if (source2 == null)
		{
			throw new ArgumentNullException("source2");
		}
		if (source3 == null)
		{
			throw new ArgumentNullException("source3");
		}
		if (source4 == null)
		{
			throw new ArgumentNullException("source4");
		}
		if (source5 == null)
		{
			throw new ArgumentNullException("source5");
		}
		if (resultSelector == null)
		{
			throw new ArgumentNullException("resultSelector");
		}
		return source1.Provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource1), typeof(TSource2), typeof(TSource3), typeof(TSource4), typeof(TSource5), typeof(TResult)), source1.Expression, GetSourceExpression(source2), GetSourceExpression(source3), GetSourceExpression(source4), GetSourceExpression(source5), resultSelector));
	}

	public static IQbservable<TResult> Zip<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TResult>(this IQbservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, IObservable<TSource4> source4, IObservable<TSource5> source5, IObservable<TSource6> source6, Expression<Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TResult>> resultSelector)
	{
		if (source1 == null)
		{
			throw new ArgumentNullException("source1");
		}
		if (source2 == null)
		{
			throw new ArgumentNullException("source2");
		}
		if (source3 == null)
		{
			throw new ArgumentNullException("source3");
		}
		if (source4 == null)
		{
			throw new ArgumentNullException("source4");
		}
		if (source5 == null)
		{
			throw new ArgumentNullException("source5");
		}
		if (source6 == null)
		{
			throw new ArgumentNullException("source6");
		}
		if (resultSelector == null)
		{
			throw new ArgumentNullException("resultSelector");
		}
		return source1.Provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource1), typeof(TSource2), typeof(TSource3), typeof(TSource4), typeof(TSource5), typeof(TSource6), typeof(TResult)), source1.Expression, GetSourceExpression(source2), GetSourceExpression(source3), GetSourceExpression(source4), GetSourceExpression(source5), GetSourceExpression(source6), resultSelector));
	}

	public static IQbservable<TResult> Zip<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TResult>(this IQbservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, IObservable<TSource4> source4, IObservable<TSource5> source5, IObservable<TSource6> source6, IObservable<TSource7> source7, Expression<Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TResult>> resultSelector)
	{
		if (source1 == null)
		{
			throw new ArgumentNullException("source1");
		}
		if (source2 == null)
		{
			throw new ArgumentNullException("source2");
		}
		if (source3 == null)
		{
			throw new ArgumentNullException("source3");
		}
		if (source4 == null)
		{
			throw new ArgumentNullException("source4");
		}
		if (source5 == null)
		{
			throw new ArgumentNullException("source5");
		}
		if (source6 == null)
		{
			throw new ArgumentNullException("source6");
		}
		if (source7 == null)
		{
			throw new ArgumentNullException("source7");
		}
		if (resultSelector == null)
		{
			throw new ArgumentNullException("resultSelector");
		}
		return source1.Provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource1), typeof(TSource2), typeof(TSource3), typeof(TSource4), typeof(TSource5), typeof(TSource6), typeof(TSource7), typeof(TResult)), source1.Expression, GetSourceExpression(source2), GetSourceExpression(source3), GetSourceExpression(source4), GetSourceExpression(source5), GetSourceExpression(source6), GetSourceExpression(source7), resultSelector));
	}

	public static IQbservable<TResult> Zip<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TResult>(this IQbservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, IObservable<TSource4> source4, IObservable<TSource5> source5, IObservable<TSource6> source6, IObservable<TSource7> source7, IObservable<TSource8> source8, Expression<Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TResult>> resultSelector)
	{
		if (source1 == null)
		{
			throw new ArgumentNullException("source1");
		}
		if (source2 == null)
		{
			throw new ArgumentNullException("source2");
		}
		if (source3 == null)
		{
			throw new ArgumentNullException("source3");
		}
		if (source4 == null)
		{
			throw new ArgumentNullException("source4");
		}
		if (source5 == null)
		{
			throw new ArgumentNullException("source5");
		}
		if (source6 == null)
		{
			throw new ArgumentNullException("source6");
		}
		if (source7 == null)
		{
			throw new ArgumentNullException("source7");
		}
		if (source8 == null)
		{
			throw new ArgumentNullException("source8");
		}
		if (resultSelector == null)
		{
			throw new ArgumentNullException("resultSelector");
		}
		return source1.Provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource1), typeof(TSource2), typeof(TSource3), typeof(TSource4), typeof(TSource5), typeof(TSource6), typeof(TSource7), typeof(TSource8), typeof(TResult)), source1.Expression, GetSourceExpression(source2), GetSourceExpression(source3), GetSourceExpression(source4), GetSourceExpression(source5), GetSourceExpression(source6), GetSourceExpression(source7), GetSourceExpression(source8), resultSelector));
	}

	public static IQbservable<TResult> Zip<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TResult>(this IQbservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, IObservable<TSource4> source4, IObservable<TSource5> source5, IObservable<TSource6> source6, IObservable<TSource7> source7, IObservable<TSource8> source8, IObservable<TSource9> source9, Expression<Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TResult>> resultSelector)
	{
		if (source1 == null)
		{
			throw new ArgumentNullException("source1");
		}
		if (source2 == null)
		{
			throw new ArgumentNullException("source2");
		}
		if (source3 == null)
		{
			throw new ArgumentNullException("source3");
		}
		if (source4 == null)
		{
			throw new ArgumentNullException("source4");
		}
		if (source5 == null)
		{
			throw new ArgumentNullException("source5");
		}
		if (source6 == null)
		{
			throw new ArgumentNullException("source6");
		}
		if (source7 == null)
		{
			throw new ArgumentNullException("source7");
		}
		if (source8 == null)
		{
			throw new ArgumentNullException("source8");
		}
		if (source9 == null)
		{
			throw new ArgumentNullException("source9");
		}
		if (resultSelector == null)
		{
			throw new ArgumentNullException("resultSelector");
		}
		return source1.Provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource1), typeof(TSource2), typeof(TSource3), typeof(TSource4), typeof(TSource5), typeof(TSource6), typeof(TSource7), typeof(TSource8), typeof(TSource9), typeof(TResult)), source1.Expression, GetSourceExpression(source2), GetSourceExpression(source3), GetSourceExpression(source4), GetSourceExpression(source5), GetSourceExpression(source6), GetSourceExpression(source7), GetSourceExpression(source8), GetSourceExpression(source9), resultSelector));
	}

	public static IQbservable<TResult> Zip<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TResult>(this IQbservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, IObservable<TSource4> source4, IObservable<TSource5> source5, IObservable<TSource6> source6, IObservable<TSource7> source7, IObservable<TSource8> source8, IObservable<TSource9> source9, IObservable<TSource10> source10, Expression<Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TResult>> resultSelector)
	{
		if (source1 == null)
		{
			throw new ArgumentNullException("source1");
		}
		if (source2 == null)
		{
			throw new ArgumentNullException("source2");
		}
		if (source3 == null)
		{
			throw new ArgumentNullException("source3");
		}
		if (source4 == null)
		{
			throw new ArgumentNullException("source4");
		}
		if (source5 == null)
		{
			throw new ArgumentNullException("source5");
		}
		if (source6 == null)
		{
			throw new ArgumentNullException("source6");
		}
		if (source7 == null)
		{
			throw new ArgumentNullException("source7");
		}
		if (source8 == null)
		{
			throw new ArgumentNullException("source8");
		}
		if (source9 == null)
		{
			throw new ArgumentNullException("source9");
		}
		if (source10 == null)
		{
			throw new ArgumentNullException("source10");
		}
		if (resultSelector == null)
		{
			throw new ArgumentNullException("resultSelector");
		}
		return source1.Provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource1), typeof(TSource2), typeof(TSource3), typeof(TSource4), typeof(TSource5), typeof(TSource6), typeof(TSource7), typeof(TSource8), typeof(TSource9), typeof(TSource10), typeof(TResult)), source1.Expression, GetSourceExpression(source2), GetSourceExpression(source3), GetSourceExpression(source4), GetSourceExpression(source5), GetSourceExpression(source6), GetSourceExpression(source7), GetSourceExpression(source8), GetSourceExpression(source9), GetSourceExpression(source10), resultSelector));
	}

	public static IQbservable<TResult> Zip<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TResult>(this IQbservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, IObservable<TSource4> source4, IObservable<TSource5> source5, IObservable<TSource6> source6, IObservable<TSource7> source7, IObservable<TSource8> source8, IObservable<TSource9> source9, IObservable<TSource10> source10, IObservable<TSource11> source11, Expression<Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TResult>> resultSelector)
	{
		if (source1 == null)
		{
			throw new ArgumentNullException("source1");
		}
		if (source2 == null)
		{
			throw new ArgumentNullException("source2");
		}
		if (source3 == null)
		{
			throw new ArgumentNullException("source3");
		}
		if (source4 == null)
		{
			throw new ArgumentNullException("source4");
		}
		if (source5 == null)
		{
			throw new ArgumentNullException("source5");
		}
		if (source6 == null)
		{
			throw new ArgumentNullException("source6");
		}
		if (source7 == null)
		{
			throw new ArgumentNullException("source7");
		}
		if (source8 == null)
		{
			throw new ArgumentNullException("source8");
		}
		if (source9 == null)
		{
			throw new ArgumentNullException("source9");
		}
		if (source10 == null)
		{
			throw new ArgumentNullException("source10");
		}
		if (source11 == null)
		{
			throw new ArgumentNullException("source11");
		}
		if (resultSelector == null)
		{
			throw new ArgumentNullException("resultSelector");
		}
		return source1.Provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource1), typeof(TSource2), typeof(TSource3), typeof(TSource4), typeof(TSource5), typeof(TSource6), typeof(TSource7), typeof(TSource8), typeof(TSource9), typeof(TSource10), typeof(TSource11), typeof(TResult)), source1.Expression, GetSourceExpression(source2), GetSourceExpression(source3), GetSourceExpression(source4), GetSourceExpression(source5), GetSourceExpression(source6), GetSourceExpression(source7), GetSourceExpression(source8), GetSourceExpression(source9), GetSourceExpression(source10), GetSourceExpression(source11), resultSelector));
	}

	public static IQbservable<TResult> Zip<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TResult>(this IQbservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, IObservable<TSource4> source4, IObservable<TSource5> source5, IObservable<TSource6> source6, IObservable<TSource7> source7, IObservable<TSource8> source8, IObservable<TSource9> source9, IObservable<TSource10> source10, IObservable<TSource11> source11, IObservable<TSource12> source12, Expression<Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TResult>> resultSelector)
	{
		if (source1 == null)
		{
			throw new ArgumentNullException("source1");
		}
		if (source2 == null)
		{
			throw new ArgumentNullException("source2");
		}
		if (source3 == null)
		{
			throw new ArgumentNullException("source3");
		}
		if (source4 == null)
		{
			throw new ArgumentNullException("source4");
		}
		if (source5 == null)
		{
			throw new ArgumentNullException("source5");
		}
		if (source6 == null)
		{
			throw new ArgumentNullException("source6");
		}
		if (source7 == null)
		{
			throw new ArgumentNullException("source7");
		}
		if (source8 == null)
		{
			throw new ArgumentNullException("source8");
		}
		if (source9 == null)
		{
			throw new ArgumentNullException("source9");
		}
		if (source10 == null)
		{
			throw new ArgumentNullException("source10");
		}
		if (source11 == null)
		{
			throw new ArgumentNullException("source11");
		}
		if (source12 == null)
		{
			throw new ArgumentNullException("source12");
		}
		if (resultSelector == null)
		{
			throw new ArgumentNullException("resultSelector");
		}
		return source1.Provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource1), typeof(TSource2), typeof(TSource3), typeof(TSource4), typeof(TSource5), typeof(TSource6), typeof(TSource7), typeof(TSource8), typeof(TSource9), typeof(TSource10), typeof(TSource11), typeof(TSource12), typeof(TResult)), source1.Expression, GetSourceExpression(source2), GetSourceExpression(source3), GetSourceExpression(source4), GetSourceExpression(source5), GetSourceExpression(source6), GetSourceExpression(source7), GetSourceExpression(source8), GetSourceExpression(source9), GetSourceExpression(source10), GetSourceExpression(source11), GetSourceExpression(source12), resultSelector));
	}

	public static IQbservable<TResult> Zip<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TResult>(this IQbservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, IObservable<TSource4> source4, IObservable<TSource5> source5, IObservable<TSource6> source6, IObservable<TSource7> source7, IObservable<TSource8> source8, IObservable<TSource9> source9, IObservable<TSource10> source10, IObservable<TSource11> source11, IObservable<TSource12> source12, IObservable<TSource13> source13, Expression<Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TResult>> resultSelector)
	{
		if (source1 == null)
		{
			throw new ArgumentNullException("source1");
		}
		if (source2 == null)
		{
			throw new ArgumentNullException("source2");
		}
		if (source3 == null)
		{
			throw new ArgumentNullException("source3");
		}
		if (source4 == null)
		{
			throw new ArgumentNullException("source4");
		}
		if (source5 == null)
		{
			throw new ArgumentNullException("source5");
		}
		if (source6 == null)
		{
			throw new ArgumentNullException("source6");
		}
		if (source7 == null)
		{
			throw new ArgumentNullException("source7");
		}
		if (source8 == null)
		{
			throw new ArgumentNullException("source8");
		}
		if (source9 == null)
		{
			throw new ArgumentNullException("source9");
		}
		if (source10 == null)
		{
			throw new ArgumentNullException("source10");
		}
		if (source11 == null)
		{
			throw new ArgumentNullException("source11");
		}
		if (source12 == null)
		{
			throw new ArgumentNullException("source12");
		}
		if (source13 == null)
		{
			throw new ArgumentNullException("source13");
		}
		if (resultSelector == null)
		{
			throw new ArgumentNullException("resultSelector");
		}
		return source1.Provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource1), typeof(TSource2), typeof(TSource3), typeof(TSource4), typeof(TSource5), typeof(TSource6), typeof(TSource7), typeof(TSource8), typeof(TSource9), typeof(TSource10), typeof(TSource11), typeof(TSource12), typeof(TSource13), typeof(TResult)), source1.Expression, GetSourceExpression(source2), GetSourceExpression(source3), GetSourceExpression(source4), GetSourceExpression(source5), GetSourceExpression(source6), GetSourceExpression(source7), GetSourceExpression(source8), GetSourceExpression(source9), GetSourceExpression(source10), GetSourceExpression(source11), GetSourceExpression(source12), GetSourceExpression(source13), resultSelector));
	}

	public static IQbservable<TResult> Zip<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TSource14, TResult>(this IQbservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, IObservable<TSource4> source4, IObservable<TSource5> source5, IObservable<TSource6> source6, IObservable<TSource7> source7, IObservable<TSource8> source8, IObservable<TSource9> source9, IObservable<TSource10> source10, IObservable<TSource11> source11, IObservable<TSource12> source12, IObservable<TSource13> source13, IObservable<TSource14> source14, Expression<Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TSource14, TResult>> resultSelector)
	{
		if (source1 == null)
		{
			throw new ArgumentNullException("source1");
		}
		if (source2 == null)
		{
			throw new ArgumentNullException("source2");
		}
		if (source3 == null)
		{
			throw new ArgumentNullException("source3");
		}
		if (source4 == null)
		{
			throw new ArgumentNullException("source4");
		}
		if (source5 == null)
		{
			throw new ArgumentNullException("source5");
		}
		if (source6 == null)
		{
			throw new ArgumentNullException("source6");
		}
		if (source7 == null)
		{
			throw new ArgumentNullException("source7");
		}
		if (source8 == null)
		{
			throw new ArgumentNullException("source8");
		}
		if (source9 == null)
		{
			throw new ArgumentNullException("source9");
		}
		if (source10 == null)
		{
			throw new ArgumentNullException("source10");
		}
		if (source11 == null)
		{
			throw new ArgumentNullException("source11");
		}
		if (source12 == null)
		{
			throw new ArgumentNullException("source12");
		}
		if (source13 == null)
		{
			throw new ArgumentNullException("source13");
		}
		if (source14 == null)
		{
			throw new ArgumentNullException("source14");
		}
		if (resultSelector == null)
		{
			throw new ArgumentNullException("resultSelector");
		}
		return source1.Provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource1), typeof(TSource2), typeof(TSource3), typeof(TSource4), typeof(TSource5), typeof(TSource6), typeof(TSource7), typeof(TSource8), typeof(TSource9), typeof(TSource10), typeof(TSource11), typeof(TSource12), typeof(TSource13), typeof(TSource14), typeof(TResult)), source1.Expression, GetSourceExpression(source2), GetSourceExpression(source3), GetSourceExpression(source4), GetSourceExpression(source5), GetSourceExpression(source6), GetSourceExpression(source7), GetSourceExpression(source8), GetSourceExpression(source9), GetSourceExpression(source10), GetSourceExpression(source11), GetSourceExpression(source12), GetSourceExpression(source13), GetSourceExpression(source14), resultSelector));
	}

	public static IQbservable<TResult> Zip<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TSource14, TSource15, TResult>(this IQbservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, IObservable<TSource4> source4, IObservable<TSource5> source5, IObservable<TSource6> source6, IObservable<TSource7> source7, IObservable<TSource8> source8, IObservable<TSource9> source9, IObservable<TSource10> source10, IObservable<TSource11> source11, IObservable<TSource12> source12, IObservable<TSource13> source13, IObservable<TSource14> source14, IObservable<TSource15> source15, Expression<Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TSource14, TSource15, TResult>> resultSelector)
	{
		if (source1 == null)
		{
			throw new ArgumentNullException("source1");
		}
		if (source2 == null)
		{
			throw new ArgumentNullException("source2");
		}
		if (source3 == null)
		{
			throw new ArgumentNullException("source3");
		}
		if (source4 == null)
		{
			throw new ArgumentNullException("source4");
		}
		if (source5 == null)
		{
			throw new ArgumentNullException("source5");
		}
		if (source6 == null)
		{
			throw new ArgumentNullException("source6");
		}
		if (source7 == null)
		{
			throw new ArgumentNullException("source7");
		}
		if (source8 == null)
		{
			throw new ArgumentNullException("source8");
		}
		if (source9 == null)
		{
			throw new ArgumentNullException("source9");
		}
		if (source10 == null)
		{
			throw new ArgumentNullException("source10");
		}
		if (source11 == null)
		{
			throw new ArgumentNullException("source11");
		}
		if (source12 == null)
		{
			throw new ArgumentNullException("source12");
		}
		if (source13 == null)
		{
			throw new ArgumentNullException("source13");
		}
		if (source14 == null)
		{
			throw new ArgumentNullException("source14");
		}
		if (source15 == null)
		{
			throw new ArgumentNullException("source15");
		}
		if (resultSelector == null)
		{
			throw new ArgumentNullException("resultSelector");
		}
		return source1.Provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource1), typeof(TSource2), typeof(TSource3), typeof(TSource4), typeof(TSource5), typeof(TSource6), typeof(TSource7), typeof(TSource8), typeof(TSource9), typeof(TSource10), typeof(TSource11), typeof(TSource12), typeof(TSource13), typeof(TSource14), typeof(TSource15), typeof(TResult)), source1.Expression, GetSourceExpression(source2), GetSourceExpression(source3), GetSourceExpression(source4), GetSourceExpression(source5), GetSourceExpression(source6), GetSourceExpression(source7), GetSourceExpression(source8), GetSourceExpression(source9), GetSourceExpression(source10), GetSourceExpression(source11), GetSourceExpression(source12), GetSourceExpression(source13), GetSourceExpression(source14), GetSourceExpression(source15), resultSelector));
	}

	public static IQbservable<TResult> Zip<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TSource14, TSource15, TSource16, TResult>(this IQbservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, IObservable<TSource4> source4, IObservable<TSource5> source5, IObservable<TSource6> source6, IObservable<TSource7> source7, IObservable<TSource8> source8, IObservable<TSource9> source9, IObservable<TSource10> source10, IObservable<TSource11> source11, IObservable<TSource12> source12, IObservable<TSource13> source13, IObservable<TSource14> source14, IObservable<TSource15> source15, IObservable<TSource16> source16, Expression<Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TSource14, TSource15, TSource16, TResult>> resultSelector)
	{
		if (source1 == null)
		{
			throw new ArgumentNullException("source1");
		}
		if (source2 == null)
		{
			throw new ArgumentNullException("source2");
		}
		if (source3 == null)
		{
			throw new ArgumentNullException("source3");
		}
		if (source4 == null)
		{
			throw new ArgumentNullException("source4");
		}
		if (source5 == null)
		{
			throw new ArgumentNullException("source5");
		}
		if (source6 == null)
		{
			throw new ArgumentNullException("source6");
		}
		if (source7 == null)
		{
			throw new ArgumentNullException("source7");
		}
		if (source8 == null)
		{
			throw new ArgumentNullException("source8");
		}
		if (source9 == null)
		{
			throw new ArgumentNullException("source9");
		}
		if (source10 == null)
		{
			throw new ArgumentNullException("source10");
		}
		if (source11 == null)
		{
			throw new ArgumentNullException("source11");
		}
		if (source12 == null)
		{
			throw new ArgumentNullException("source12");
		}
		if (source13 == null)
		{
			throw new ArgumentNullException("source13");
		}
		if (source14 == null)
		{
			throw new ArgumentNullException("source14");
		}
		if (source15 == null)
		{
			throw new ArgumentNullException("source15");
		}
		if (source16 == null)
		{
			throw new ArgumentNullException("source16");
		}
		if (resultSelector == null)
		{
			throw new ArgumentNullException("resultSelector");
		}
		return source1.Provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource1), typeof(TSource2), typeof(TSource3), typeof(TSource4), typeof(TSource5), typeof(TSource6), typeof(TSource7), typeof(TSource8), typeof(TSource9), typeof(TSource10), typeof(TSource11), typeof(TSource12), typeof(TSource13), typeof(TSource14), typeof(TSource15), typeof(TSource16), typeof(TResult)), source1.Expression, GetSourceExpression(source2), GetSourceExpression(source3), GetSourceExpression(source4), GetSourceExpression(source5), GetSourceExpression(source6), GetSourceExpression(source7), GetSourceExpression(source8), GetSourceExpression(source9), GetSourceExpression(source10), GetSourceExpression(source11), GetSourceExpression(source12), GetSourceExpression(source13), GetSourceExpression(source14), GetSourceExpression(source15), GetSourceExpression(source16), resultSelector));
	}

	public static Func<IQbservable<Unit>> ToAsync(this IQbservableProvider provider, Expression<Action> action)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		MethodInfo m = (MethodInfo)MethodBase.GetCurrentMethod();
		return () => provider.CreateQuery<Unit>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), action)));
	}

	public static Func<IQbservable<Unit>> ToAsync(this IQbservableProvider provider, Expression<Action> action, IScheduler scheduler)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		MethodInfo m = (MethodInfo)MethodBase.GetCurrentMethod();
		return () => provider.CreateQuery<Unit>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), action, Expression.Constant(scheduler, typeof(IScheduler)))));
	}

	public static Func<TArg1, IQbservable<Unit>> ToAsync<TArg1>(this IQbservableProvider provider, Expression<Action<TArg1>> action)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		MethodInfo m = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TArg1));
		return (TArg1 t1) => provider.CreateQuery<Unit>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), action), Expression.Constant(t1, typeof(TArg1))));
	}

	public static Func<TArg1, IQbservable<Unit>> ToAsync<TArg1>(this IQbservableProvider provider, Expression<Action<TArg1>> action, IScheduler scheduler)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		MethodInfo m = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TArg1));
		return (TArg1 t1) => provider.CreateQuery<Unit>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), action, Expression.Constant(scheduler, typeof(IScheduler))), Expression.Constant(t1, typeof(TArg1))));
	}

	public static Func<TArg1, TArg2, IQbservable<Unit>> ToAsync<TArg1, TArg2>(this IQbservableProvider provider, Expression<Action<TArg1, TArg2>> action)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		MethodInfo m = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TArg1), typeof(TArg2));
		return (TArg1 t1, TArg2 t2) => provider.CreateQuery<Unit>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), action), Expression.Constant(t1, typeof(TArg1)), Expression.Constant(t2, typeof(TArg2))));
	}

	public static Func<TArg1, TArg2, IQbservable<Unit>> ToAsync<TArg1, TArg2>(this IQbservableProvider provider, Expression<Action<TArg1, TArg2>> action, IScheduler scheduler)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		MethodInfo m = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TArg1), typeof(TArg2));
		return (TArg1 t1, TArg2 t2) => provider.CreateQuery<Unit>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), action, Expression.Constant(scheduler, typeof(IScheduler))), Expression.Constant(t1, typeof(TArg1)), Expression.Constant(t2, typeof(TArg2))));
	}

	public static Func<TArg1, TArg2, TArg3, IQbservable<Unit>> ToAsync<TArg1, TArg2, TArg3>(this IQbservableProvider provider, Expression<Action<TArg1, TArg2, TArg3>> action)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		MethodInfo m = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TArg1), typeof(TArg2), typeof(TArg3));
		return (TArg1 t1, TArg2 t2, TArg3 t3) => provider.CreateQuery<Unit>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), action), Expression.Constant(t1, typeof(TArg1)), Expression.Constant(t2, typeof(TArg2)), Expression.Constant(t3, typeof(TArg3))));
	}

	public static Func<TArg1, TArg2, TArg3, IQbservable<Unit>> ToAsync<TArg1, TArg2, TArg3>(this IQbservableProvider provider, Expression<Action<TArg1, TArg2, TArg3>> action, IScheduler scheduler)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		MethodInfo m = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TArg1), typeof(TArg2), typeof(TArg3));
		return (TArg1 t1, TArg2 t2, TArg3 t3) => provider.CreateQuery<Unit>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), action, Expression.Constant(scheduler, typeof(IScheduler))), Expression.Constant(t1, typeof(TArg1)), Expression.Constant(t2, typeof(TArg2)), Expression.Constant(t3, typeof(TArg3))));
	}

	public static Func<TArg1, TArg2, TArg3, TArg4, IQbservable<Unit>> ToAsync<TArg1, TArg2, TArg3, TArg4>(this IQbservableProvider provider, Expression<Action<TArg1, TArg2, TArg3, TArg4>> action)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		MethodInfo m = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4));
		return (TArg1 t1, TArg2 t2, TArg3 t3, TArg4 t4) => provider.CreateQuery<Unit>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), action), Expression.Constant(t1, typeof(TArg1)), Expression.Constant(t2, typeof(TArg2)), Expression.Constant(t3, typeof(TArg3)), Expression.Constant(t4, typeof(TArg4))));
	}

	public static Func<TArg1, TArg2, TArg3, TArg4, IQbservable<Unit>> ToAsync<TArg1, TArg2, TArg3, TArg4>(this IQbservableProvider provider, Expression<Action<TArg1, TArg2, TArg3, TArg4>> action, IScheduler scheduler)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		MethodInfo m = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4));
		return (TArg1 t1, TArg2 t2, TArg3 t3, TArg4 t4) => provider.CreateQuery<Unit>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), action, Expression.Constant(scheduler, typeof(IScheduler))), Expression.Constant(t1, typeof(TArg1)), Expression.Constant(t2, typeof(TArg2)), Expression.Constant(t3, typeof(TArg3)), Expression.Constant(t4, typeof(TArg4))));
	}

	public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, IQbservable<Unit>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5>(this IQbservableProvider provider, Expression<Action<TArg1, TArg2, TArg3, TArg4, TArg5>> action)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		MethodInfo m = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5));
		return (TArg1 t1, TArg2 t2, TArg3 t3, TArg4 t4, TArg5 t5) => provider.CreateQuery<Unit>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), action), Expression.Constant(t1, typeof(TArg1)), Expression.Constant(t2, typeof(TArg2)), Expression.Constant(t3, typeof(TArg3)), Expression.Constant(t4, typeof(TArg4)), Expression.Constant(t5, typeof(TArg5))));
	}

	public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, IQbservable<Unit>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5>(this IQbservableProvider provider, Expression<Action<TArg1, TArg2, TArg3, TArg4, TArg5>> action, IScheduler scheduler)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		MethodInfo m = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5));
		return (TArg1 t1, TArg2 t2, TArg3 t3, TArg4 t4, TArg5 t5) => provider.CreateQuery<Unit>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), action, Expression.Constant(scheduler, typeof(IScheduler))), Expression.Constant(t1, typeof(TArg1)), Expression.Constant(t2, typeof(TArg2)), Expression.Constant(t3, typeof(TArg3)), Expression.Constant(t4, typeof(TArg4)), Expression.Constant(t5, typeof(TArg5))));
	}

	public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, IQbservable<Unit>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>(this IQbservableProvider provider, Expression<Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>> action)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		MethodInfo m = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5), typeof(TArg6));
		return (TArg1 t1, TArg2 t2, TArg3 t3, TArg4 t4, TArg5 t5, TArg6 t6) => provider.CreateQuery<Unit>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), action), Expression.Constant(t1, typeof(TArg1)), Expression.Constant(t2, typeof(TArg2)), Expression.Constant(t3, typeof(TArg3)), Expression.Constant(t4, typeof(TArg4)), Expression.Constant(t5, typeof(TArg5)), Expression.Constant(t6, typeof(TArg6))));
	}

	public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, IQbservable<Unit>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>(this IQbservableProvider provider, Expression<Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>> action, IScheduler scheduler)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		MethodInfo m = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5), typeof(TArg6));
		return (TArg1 t1, TArg2 t2, TArg3 t3, TArg4 t4, TArg5 t5, TArg6 t6) => provider.CreateQuery<Unit>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), action, Expression.Constant(scheduler, typeof(IScheduler))), Expression.Constant(t1, typeof(TArg1)), Expression.Constant(t2, typeof(TArg2)), Expression.Constant(t3, typeof(TArg3)), Expression.Constant(t4, typeof(TArg4)), Expression.Constant(t5, typeof(TArg5)), Expression.Constant(t6, typeof(TArg6))));
	}

	public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, IQbservable<Unit>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7>(this IQbservableProvider provider, Expression<Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7>> action)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		MethodInfo m = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5), typeof(TArg6), typeof(TArg7));
		return (TArg1 t1, TArg2 t2, TArg3 t3, TArg4 t4, TArg5 t5, TArg6 t6, TArg7 t7) => provider.CreateQuery<Unit>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), action), Expression.Constant(t1, typeof(TArg1)), Expression.Constant(t2, typeof(TArg2)), Expression.Constant(t3, typeof(TArg3)), Expression.Constant(t4, typeof(TArg4)), Expression.Constant(t5, typeof(TArg5)), Expression.Constant(t6, typeof(TArg6)), Expression.Constant(t7, typeof(TArg7))));
	}

	public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, IQbservable<Unit>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7>(this IQbservableProvider provider, Expression<Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7>> action, IScheduler scheduler)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		MethodInfo m = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5), typeof(TArg6), typeof(TArg7));
		return (TArg1 t1, TArg2 t2, TArg3 t3, TArg4 t4, TArg5 t5, TArg6 t6, TArg7 t7) => provider.CreateQuery<Unit>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), action, Expression.Constant(scheduler, typeof(IScheduler))), Expression.Constant(t1, typeof(TArg1)), Expression.Constant(t2, typeof(TArg2)), Expression.Constant(t3, typeof(TArg3)), Expression.Constant(t4, typeof(TArg4)), Expression.Constant(t5, typeof(TArg5)), Expression.Constant(t6, typeof(TArg6)), Expression.Constant(t7, typeof(TArg7))));
	}

	public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, IQbservable<Unit>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8>(this IQbservableProvider provider, Expression<Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8>> action)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		MethodInfo m = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5), typeof(TArg6), typeof(TArg7), typeof(TArg8));
		return (TArg1 t1, TArg2 t2, TArg3 t3, TArg4 t4, TArg5 t5, TArg6 t6, TArg7 t7, TArg8 t8) => provider.CreateQuery<Unit>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), action), Expression.Constant(t1, typeof(TArg1)), Expression.Constant(t2, typeof(TArg2)), Expression.Constant(t3, typeof(TArg3)), Expression.Constant(t4, typeof(TArg4)), Expression.Constant(t5, typeof(TArg5)), Expression.Constant(t6, typeof(TArg6)), Expression.Constant(t7, typeof(TArg7)), Expression.Constant(t8, typeof(TArg8))));
	}

	public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, IQbservable<Unit>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8>(this IQbservableProvider provider, Expression<Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8>> action, IScheduler scheduler)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		MethodInfo m = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5), typeof(TArg6), typeof(TArg7), typeof(TArg8));
		return (TArg1 t1, TArg2 t2, TArg3 t3, TArg4 t4, TArg5 t5, TArg6 t6, TArg7 t7, TArg8 t8) => provider.CreateQuery<Unit>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), action, Expression.Constant(scheduler, typeof(IScheduler))), Expression.Constant(t1, typeof(TArg1)), Expression.Constant(t2, typeof(TArg2)), Expression.Constant(t3, typeof(TArg3)), Expression.Constant(t4, typeof(TArg4)), Expression.Constant(t5, typeof(TArg5)), Expression.Constant(t6, typeof(TArg6)), Expression.Constant(t7, typeof(TArg7)), Expression.Constant(t8, typeof(TArg8))));
	}

	public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, IQbservable<Unit>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9>(this IQbservableProvider provider, Expression<Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9>> action)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		MethodInfo m = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5), typeof(TArg6), typeof(TArg7), typeof(TArg8), typeof(TArg9));
		return (TArg1 t1, TArg2 t2, TArg3 t3, TArg4 t4, TArg5 t5, TArg6 t6, TArg7 t7, TArg8 t8, TArg9 t9) => provider.CreateQuery<Unit>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), action), Expression.Constant(t1, typeof(TArg1)), Expression.Constant(t2, typeof(TArg2)), Expression.Constant(t3, typeof(TArg3)), Expression.Constant(t4, typeof(TArg4)), Expression.Constant(t5, typeof(TArg5)), Expression.Constant(t6, typeof(TArg6)), Expression.Constant(t7, typeof(TArg7)), Expression.Constant(t8, typeof(TArg8)), Expression.Constant(t9, typeof(TArg9))));
	}

	public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, IQbservable<Unit>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9>(this IQbservableProvider provider, Expression<Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9>> action, IScheduler scheduler)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		MethodInfo m = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5), typeof(TArg6), typeof(TArg7), typeof(TArg8), typeof(TArg9));
		return (TArg1 t1, TArg2 t2, TArg3 t3, TArg4 t4, TArg5 t5, TArg6 t6, TArg7 t7, TArg8 t8, TArg9 t9) => provider.CreateQuery<Unit>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), action, Expression.Constant(scheduler, typeof(IScheduler))), Expression.Constant(t1, typeof(TArg1)), Expression.Constant(t2, typeof(TArg2)), Expression.Constant(t3, typeof(TArg3)), Expression.Constant(t4, typeof(TArg4)), Expression.Constant(t5, typeof(TArg5)), Expression.Constant(t6, typeof(TArg6)), Expression.Constant(t7, typeof(TArg7)), Expression.Constant(t8, typeof(TArg8)), Expression.Constant(t9, typeof(TArg9))));
	}

	public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, IQbservable<Unit>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10>(this IQbservableProvider provider, Expression<Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10>> action)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		MethodInfo m = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5), typeof(TArg6), typeof(TArg7), typeof(TArg8), typeof(TArg9), typeof(TArg10));
		return (TArg1 t1, TArg2 t2, TArg3 t3, TArg4 t4, TArg5 t5, TArg6 t6, TArg7 t7, TArg8 t8, TArg9 t9, TArg10 t10) => provider.CreateQuery<Unit>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), action), Expression.Constant(t1, typeof(TArg1)), Expression.Constant(t2, typeof(TArg2)), Expression.Constant(t3, typeof(TArg3)), Expression.Constant(t4, typeof(TArg4)), Expression.Constant(t5, typeof(TArg5)), Expression.Constant(t6, typeof(TArg6)), Expression.Constant(t7, typeof(TArg7)), Expression.Constant(t8, typeof(TArg8)), Expression.Constant(t9, typeof(TArg9)), Expression.Constant(t10, typeof(TArg10))));
	}

	public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, IQbservable<Unit>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10>(this IQbservableProvider provider, Expression<Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10>> action, IScheduler scheduler)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		MethodInfo m = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5), typeof(TArg6), typeof(TArg7), typeof(TArg8), typeof(TArg9), typeof(TArg10));
		return (TArg1 t1, TArg2 t2, TArg3 t3, TArg4 t4, TArg5 t5, TArg6 t6, TArg7 t7, TArg8 t8, TArg9 t9, TArg10 t10) => provider.CreateQuery<Unit>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), action, Expression.Constant(scheduler, typeof(IScheduler))), Expression.Constant(t1, typeof(TArg1)), Expression.Constant(t2, typeof(TArg2)), Expression.Constant(t3, typeof(TArg3)), Expression.Constant(t4, typeof(TArg4)), Expression.Constant(t5, typeof(TArg5)), Expression.Constant(t6, typeof(TArg6)), Expression.Constant(t7, typeof(TArg7)), Expression.Constant(t8, typeof(TArg8)), Expression.Constant(t9, typeof(TArg9)), Expression.Constant(t10, typeof(TArg10))));
	}

	public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, IQbservable<Unit>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11>(this IQbservableProvider provider, Expression<Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11>> action)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		MethodInfo m = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5), typeof(TArg6), typeof(TArg7), typeof(TArg8), typeof(TArg9), typeof(TArg10), typeof(TArg11));
		return (TArg1 t1, TArg2 t2, TArg3 t3, TArg4 t4, TArg5 t5, TArg6 t6, TArg7 t7, TArg8 t8, TArg9 t9, TArg10 t10, TArg11 t11) => provider.CreateQuery<Unit>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), action), Expression.Constant(t1, typeof(TArg1)), Expression.Constant(t2, typeof(TArg2)), Expression.Constant(t3, typeof(TArg3)), Expression.Constant(t4, typeof(TArg4)), Expression.Constant(t5, typeof(TArg5)), Expression.Constant(t6, typeof(TArg6)), Expression.Constant(t7, typeof(TArg7)), Expression.Constant(t8, typeof(TArg8)), Expression.Constant(t9, typeof(TArg9)), Expression.Constant(t10, typeof(TArg10)), Expression.Constant(t11, typeof(TArg11))));
	}

	public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, IQbservable<Unit>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11>(this IQbservableProvider provider, Expression<Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11>> action, IScheduler scheduler)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		MethodInfo m = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5), typeof(TArg6), typeof(TArg7), typeof(TArg8), typeof(TArg9), typeof(TArg10), typeof(TArg11));
		return (TArg1 t1, TArg2 t2, TArg3 t3, TArg4 t4, TArg5 t5, TArg6 t6, TArg7 t7, TArg8 t8, TArg9 t9, TArg10 t10, TArg11 t11) => provider.CreateQuery<Unit>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), action, Expression.Constant(scheduler, typeof(IScheduler))), Expression.Constant(t1, typeof(TArg1)), Expression.Constant(t2, typeof(TArg2)), Expression.Constant(t3, typeof(TArg3)), Expression.Constant(t4, typeof(TArg4)), Expression.Constant(t5, typeof(TArg5)), Expression.Constant(t6, typeof(TArg6)), Expression.Constant(t7, typeof(TArg7)), Expression.Constant(t8, typeof(TArg8)), Expression.Constant(t9, typeof(TArg9)), Expression.Constant(t10, typeof(TArg10)), Expression.Constant(t11, typeof(TArg11))));
	}

	public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, IQbservable<Unit>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12>(this IQbservableProvider provider, Expression<Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12>> action)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		MethodInfo m = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5), typeof(TArg6), typeof(TArg7), typeof(TArg8), typeof(TArg9), typeof(TArg10), typeof(TArg11), typeof(TArg12));
		return (TArg1 t1, TArg2 t2, TArg3 t3, TArg4 t4, TArg5 t5, TArg6 t6, TArg7 t7, TArg8 t8, TArg9 t9, TArg10 t10, TArg11 t11, TArg12 t12) => provider.CreateQuery<Unit>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), action), Expression.Constant(t1, typeof(TArg1)), Expression.Constant(t2, typeof(TArg2)), Expression.Constant(t3, typeof(TArg3)), Expression.Constant(t4, typeof(TArg4)), Expression.Constant(t5, typeof(TArg5)), Expression.Constant(t6, typeof(TArg6)), Expression.Constant(t7, typeof(TArg7)), Expression.Constant(t8, typeof(TArg8)), Expression.Constant(t9, typeof(TArg9)), Expression.Constant(t10, typeof(TArg10)), Expression.Constant(t11, typeof(TArg11)), Expression.Constant(t12, typeof(TArg12))));
	}

	public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, IQbservable<Unit>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12>(this IQbservableProvider provider, Expression<Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12>> action, IScheduler scheduler)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		MethodInfo m = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5), typeof(TArg6), typeof(TArg7), typeof(TArg8), typeof(TArg9), typeof(TArg10), typeof(TArg11), typeof(TArg12));
		return (TArg1 t1, TArg2 t2, TArg3 t3, TArg4 t4, TArg5 t5, TArg6 t6, TArg7 t7, TArg8 t8, TArg9 t9, TArg10 t10, TArg11 t11, TArg12 t12) => provider.CreateQuery<Unit>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), action, Expression.Constant(scheduler, typeof(IScheduler))), Expression.Constant(t1, typeof(TArg1)), Expression.Constant(t2, typeof(TArg2)), Expression.Constant(t3, typeof(TArg3)), Expression.Constant(t4, typeof(TArg4)), Expression.Constant(t5, typeof(TArg5)), Expression.Constant(t6, typeof(TArg6)), Expression.Constant(t7, typeof(TArg7)), Expression.Constant(t8, typeof(TArg8)), Expression.Constant(t9, typeof(TArg9)), Expression.Constant(t10, typeof(TArg10)), Expression.Constant(t11, typeof(TArg11)), Expression.Constant(t12, typeof(TArg12))));
	}

	public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, IQbservable<Unit>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13>(this IQbservableProvider provider, Expression<Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13>> action)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		MethodInfo m = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5), typeof(TArg6), typeof(TArg7), typeof(TArg8), typeof(TArg9), typeof(TArg10), typeof(TArg11), typeof(TArg12), typeof(TArg13));
		return (TArg1 t1, TArg2 t2, TArg3 t3, TArg4 t4, TArg5 t5, TArg6 t6, TArg7 t7, TArg8 t8, TArg9 t9, TArg10 t10, TArg11 t11, TArg12 t12, TArg13 t13) => provider.CreateQuery<Unit>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), action), Expression.Constant(t1, typeof(TArg1)), Expression.Constant(t2, typeof(TArg2)), Expression.Constant(t3, typeof(TArg3)), Expression.Constant(t4, typeof(TArg4)), Expression.Constant(t5, typeof(TArg5)), Expression.Constant(t6, typeof(TArg6)), Expression.Constant(t7, typeof(TArg7)), Expression.Constant(t8, typeof(TArg8)), Expression.Constant(t9, typeof(TArg9)), Expression.Constant(t10, typeof(TArg10)), Expression.Constant(t11, typeof(TArg11)), Expression.Constant(t12, typeof(TArg12)), Expression.Constant(t13, typeof(TArg13))));
	}

	public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, IQbservable<Unit>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13>(this IQbservableProvider provider, Expression<Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13>> action, IScheduler scheduler)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		MethodInfo m = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5), typeof(TArg6), typeof(TArg7), typeof(TArg8), typeof(TArg9), typeof(TArg10), typeof(TArg11), typeof(TArg12), typeof(TArg13));
		return (TArg1 t1, TArg2 t2, TArg3 t3, TArg4 t4, TArg5 t5, TArg6 t6, TArg7 t7, TArg8 t8, TArg9 t9, TArg10 t10, TArg11 t11, TArg12 t12, TArg13 t13) => provider.CreateQuery<Unit>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), action, Expression.Constant(scheduler, typeof(IScheduler))), Expression.Constant(t1, typeof(TArg1)), Expression.Constant(t2, typeof(TArg2)), Expression.Constant(t3, typeof(TArg3)), Expression.Constant(t4, typeof(TArg4)), Expression.Constant(t5, typeof(TArg5)), Expression.Constant(t6, typeof(TArg6)), Expression.Constant(t7, typeof(TArg7)), Expression.Constant(t8, typeof(TArg8)), Expression.Constant(t9, typeof(TArg9)), Expression.Constant(t10, typeof(TArg10)), Expression.Constant(t11, typeof(TArg11)), Expression.Constant(t12, typeof(TArg12)), Expression.Constant(t13, typeof(TArg13))));
	}

	public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, IQbservable<Unit>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14>(this IQbservableProvider provider, Expression<Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14>> action)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		MethodInfo m = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5), typeof(TArg6), typeof(TArg7), typeof(TArg8), typeof(TArg9), typeof(TArg10), typeof(TArg11), typeof(TArg12), typeof(TArg13), typeof(TArg14));
		return (TArg1 t1, TArg2 t2, TArg3 t3, TArg4 t4, TArg5 t5, TArg6 t6, TArg7 t7, TArg8 t8, TArg9 t9, TArg10 t10, TArg11 t11, TArg12 t12, TArg13 t13, TArg14 t14) => provider.CreateQuery<Unit>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), action), Expression.Constant(t1, typeof(TArg1)), Expression.Constant(t2, typeof(TArg2)), Expression.Constant(t3, typeof(TArg3)), Expression.Constant(t4, typeof(TArg4)), Expression.Constant(t5, typeof(TArg5)), Expression.Constant(t6, typeof(TArg6)), Expression.Constant(t7, typeof(TArg7)), Expression.Constant(t8, typeof(TArg8)), Expression.Constant(t9, typeof(TArg9)), Expression.Constant(t10, typeof(TArg10)), Expression.Constant(t11, typeof(TArg11)), Expression.Constant(t12, typeof(TArg12)), Expression.Constant(t13, typeof(TArg13)), Expression.Constant(t14, typeof(TArg14))));
	}

	public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, IQbservable<Unit>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14>(this IQbservableProvider provider, Expression<Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14>> action, IScheduler scheduler)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		MethodInfo m = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5), typeof(TArg6), typeof(TArg7), typeof(TArg8), typeof(TArg9), typeof(TArg10), typeof(TArg11), typeof(TArg12), typeof(TArg13), typeof(TArg14));
		return (TArg1 t1, TArg2 t2, TArg3 t3, TArg4 t4, TArg5 t5, TArg6 t6, TArg7 t7, TArg8 t8, TArg9 t9, TArg10 t10, TArg11 t11, TArg12 t12, TArg13 t13, TArg14 t14) => provider.CreateQuery<Unit>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), action, Expression.Constant(scheduler, typeof(IScheduler))), Expression.Constant(t1, typeof(TArg1)), Expression.Constant(t2, typeof(TArg2)), Expression.Constant(t3, typeof(TArg3)), Expression.Constant(t4, typeof(TArg4)), Expression.Constant(t5, typeof(TArg5)), Expression.Constant(t6, typeof(TArg6)), Expression.Constant(t7, typeof(TArg7)), Expression.Constant(t8, typeof(TArg8)), Expression.Constant(t9, typeof(TArg9)), Expression.Constant(t10, typeof(TArg10)), Expression.Constant(t11, typeof(TArg11)), Expression.Constant(t12, typeof(TArg12)), Expression.Constant(t13, typeof(TArg13)), Expression.Constant(t14, typeof(TArg14))));
	}

	public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, IQbservable<Unit>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15>(this IQbservableProvider provider, Expression<Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15>> action)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		MethodInfo m = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5), typeof(TArg6), typeof(TArg7), typeof(TArg8), typeof(TArg9), typeof(TArg10), typeof(TArg11), typeof(TArg12), typeof(TArg13), typeof(TArg14), typeof(TArg15));
		return (TArg1 t1, TArg2 t2, TArg3 t3, TArg4 t4, TArg5 t5, TArg6 t6, TArg7 t7, TArg8 t8, TArg9 t9, TArg10 t10, TArg11 t11, TArg12 t12, TArg13 t13, TArg14 t14, TArg15 t15) => provider.CreateQuery<Unit>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), action), Expression.Constant(t1, typeof(TArg1)), Expression.Constant(t2, typeof(TArg2)), Expression.Constant(t3, typeof(TArg3)), Expression.Constant(t4, typeof(TArg4)), Expression.Constant(t5, typeof(TArg5)), Expression.Constant(t6, typeof(TArg6)), Expression.Constant(t7, typeof(TArg7)), Expression.Constant(t8, typeof(TArg8)), Expression.Constant(t9, typeof(TArg9)), Expression.Constant(t10, typeof(TArg10)), Expression.Constant(t11, typeof(TArg11)), Expression.Constant(t12, typeof(TArg12)), Expression.Constant(t13, typeof(TArg13)), Expression.Constant(t14, typeof(TArg14)), Expression.Constant(t15, typeof(TArg15))));
	}

	public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, IQbservable<Unit>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15>(this IQbservableProvider provider, Expression<Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15>> action, IScheduler scheduler)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		MethodInfo m = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5), typeof(TArg6), typeof(TArg7), typeof(TArg8), typeof(TArg9), typeof(TArg10), typeof(TArg11), typeof(TArg12), typeof(TArg13), typeof(TArg14), typeof(TArg15));
		return (TArg1 t1, TArg2 t2, TArg3 t3, TArg4 t4, TArg5 t5, TArg6 t6, TArg7 t7, TArg8 t8, TArg9 t9, TArg10 t10, TArg11 t11, TArg12 t12, TArg13 t13, TArg14 t14, TArg15 t15) => provider.CreateQuery<Unit>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), action, Expression.Constant(scheduler, typeof(IScheduler))), Expression.Constant(t1, typeof(TArg1)), Expression.Constant(t2, typeof(TArg2)), Expression.Constant(t3, typeof(TArg3)), Expression.Constant(t4, typeof(TArg4)), Expression.Constant(t5, typeof(TArg5)), Expression.Constant(t6, typeof(TArg6)), Expression.Constant(t7, typeof(TArg7)), Expression.Constant(t8, typeof(TArg8)), Expression.Constant(t9, typeof(TArg9)), Expression.Constant(t10, typeof(TArg10)), Expression.Constant(t11, typeof(TArg11)), Expression.Constant(t12, typeof(TArg12)), Expression.Constant(t13, typeof(TArg13)), Expression.Constant(t14, typeof(TArg14)), Expression.Constant(t15, typeof(TArg15))));
	}

	public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, TArg16, IQbservable<Unit>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, TArg16>(this IQbservableProvider provider, Expression<Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, TArg16>> action)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		MethodInfo m = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5), typeof(TArg6), typeof(TArg7), typeof(TArg8), typeof(TArg9), typeof(TArg10), typeof(TArg11), typeof(TArg12), typeof(TArg13), typeof(TArg14), typeof(TArg15), typeof(TArg16));
		return (TArg1 t1, TArg2 t2, TArg3 t3, TArg4 t4, TArg5 t5, TArg6 t6, TArg7 t7, TArg8 t8, TArg9 t9, TArg10 t10, TArg11 t11, TArg12 t12, TArg13 t13, TArg14 t14, TArg15 t15, TArg16 t16) => provider.CreateQuery<Unit>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), action), Expression.Constant(t1, typeof(TArg1)), Expression.Constant(t2, typeof(TArg2)), Expression.Constant(t3, typeof(TArg3)), Expression.Constant(t4, typeof(TArg4)), Expression.Constant(t5, typeof(TArg5)), Expression.Constant(t6, typeof(TArg6)), Expression.Constant(t7, typeof(TArg7)), Expression.Constant(t8, typeof(TArg8)), Expression.Constant(t9, typeof(TArg9)), Expression.Constant(t10, typeof(TArg10)), Expression.Constant(t11, typeof(TArg11)), Expression.Constant(t12, typeof(TArg12)), Expression.Constant(t13, typeof(TArg13)), Expression.Constant(t14, typeof(TArg14)), Expression.Constant(t15, typeof(TArg15)), Expression.Constant(t16, typeof(TArg16))));
	}

	public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, TArg16, IQbservable<Unit>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, TArg16>(this IQbservableProvider provider, Expression<Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, TArg16>> action, IScheduler scheduler)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		MethodInfo m = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5), typeof(TArg6), typeof(TArg7), typeof(TArg8), typeof(TArg9), typeof(TArg10), typeof(TArg11), typeof(TArg12), typeof(TArg13), typeof(TArg14), typeof(TArg15), typeof(TArg16));
		return (TArg1 t1, TArg2 t2, TArg3 t3, TArg4 t4, TArg5 t5, TArg6 t6, TArg7 t7, TArg8 t8, TArg9 t9, TArg10 t10, TArg11 t11, TArg12 t12, TArg13 t13, TArg14 t14, TArg15 t15, TArg16 t16) => provider.CreateQuery<Unit>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), action, Expression.Constant(scheduler, typeof(IScheduler))), Expression.Constant(t1, typeof(TArg1)), Expression.Constant(t2, typeof(TArg2)), Expression.Constant(t3, typeof(TArg3)), Expression.Constant(t4, typeof(TArg4)), Expression.Constant(t5, typeof(TArg5)), Expression.Constant(t6, typeof(TArg6)), Expression.Constant(t7, typeof(TArg7)), Expression.Constant(t8, typeof(TArg8)), Expression.Constant(t9, typeof(TArg9)), Expression.Constant(t10, typeof(TArg10)), Expression.Constant(t11, typeof(TArg11)), Expression.Constant(t12, typeof(TArg12)), Expression.Constant(t13, typeof(TArg13)), Expression.Constant(t14, typeof(TArg14)), Expression.Constant(t15, typeof(TArg15)), Expression.Constant(t16, typeof(TArg16))));
	}

	public static Func<IQbservable<TResult>> ToAsync<TResult>(this IQbservableProvider provider, Expression<Func<TResult>> function)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (function == null)
		{
			throw new ArgumentNullException("function");
		}
		MethodInfo m = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TResult));
		return () => provider.CreateQuery<TResult>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), function)));
	}

	public static Func<IQbservable<TResult>> ToAsync<TResult>(this IQbservableProvider provider, Expression<Func<TResult>> function, IScheduler scheduler)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (function == null)
		{
			throw new ArgumentNullException("function");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		MethodInfo m = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TResult));
		return () => provider.CreateQuery<TResult>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), function, Expression.Constant(scheduler, typeof(IScheduler)))));
	}

	public static Func<TArg1, IQbservable<TResult>> ToAsync<TArg1, TResult>(this IQbservableProvider provider, Expression<Func<TArg1, TResult>> function)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (function == null)
		{
			throw new ArgumentNullException("function");
		}
		MethodInfo m = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TArg1), typeof(TResult));
		return (TArg1 t1) => provider.CreateQuery<TResult>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), function), Expression.Constant(t1, typeof(TArg1))));
	}

	public static Func<TArg1, IQbservable<TResult>> ToAsync<TArg1, TResult>(this IQbservableProvider provider, Expression<Func<TArg1, TResult>> function, IScheduler scheduler)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (function == null)
		{
			throw new ArgumentNullException("function");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		MethodInfo m = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TArg1), typeof(TResult));
		return (TArg1 t1) => provider.CreateQuery<TResult>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), function, Expression.Constant(scheduler, typeof(IScheduler))), Expression.Constant(t1, typeof(TArg1))));
	}

	public static Func<TArg1, TArg2, IQbservable<TResult>> ToAsync<TArg1, TArg2, TResult>(this IQbservableProvider provider, Expression<Func<TArg1, TArg2, TResult>> function)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (function == null)
		{
			throw new ArgumentNullException("function");
		}
		MethodInfo m = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TArg1), typeof(TArg2), typeof(TResult));
		return (TArg1 t1, TArg2 t2) => provider.CreateQuery<TResult>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), function), Expression.Constant(t1, typeof(TArg1)), Expression.Constant(t2, typeof(TArg2))));
	}

	public static Func<TArg1, TArg2, IQbservable<TResult>> ToAsync<TArg1, TArg2, TResult>(this IQbservableProvider provider, Expression<Func<TArg1, TArg2, TResult>> function, IScheduler scheduler)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (function == null)
		{
			throw new ArgumentNullException("function");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		MethodInfo m = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TArg1), typeof(TArg2), typeof(TResult));
		return (TArg1 t1, TArg2 t2) => provider.CreateQuery<TResult>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), function, Expression.Constant(scheduler, typeof(IScheduler))), Expression.Constant(t1, typeof(TArg1)), Expression.Constant(t2, typeof(TArg2))));
	}

	public static Func<TArg1, TArg2, TArg3, IQbservable<TResult>> ToAsync<TArg1, TArg2, TArg3, TResult>(this IQbservableProvider provider, Expression<Func<TArg1, TArg2, TArg3, TResult>> function)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (function == null)
		{
			throw new ArgumentNullException("function");
		}
		MethodInfo m = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TResult));
		return (TArg1 t1, TArg2 t2, TArg3 t3) => provider.CreateQuery<TResult>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), function), Expression.Constant(t1, typeof(TArg1)), Expression.Constant(t2, typeof(TArg2)), Expression.Constant(t3, typeof(TArg3))));
	}

	public static Func<TArg1, TArg2, TArg3, IQbservable<TResult>> ToAsync<TArg1, TArg2, TArg3, TResult>(this IQbservableProvider provider, Expression<Func<TArg1, TArg2, TArg3, TResult>> function, IScheduler scheduler)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (function == null)
		{
			throw new ArgumentNullException("function");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		MethodInfo m = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TResult));
		return (TArg1 t1, TArg2 t2, TArg3 t3) => provider.CreateQuery<TResult>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), function, Expression.Constant(scheduler, typeof(IScheduler))), Expression.Constant(t1, typeof(TArg1)), Expression.Constant(t2, typeof(TArg2)), Expression.Constant(t3, typeof(TArg3))));
	}

	public static Func<TArg1, TArg2, TArg3, TArg4, IQbservable<TResult>> ToAsync<TArg1, TArg2, TArg3, TArg4, TResult>(this IQbservableProvider provider, Expression<Func<TArg1, TArg2, TArg3, TArg4, TResult>> function)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (function == null)
		{
			throw new ArgumentNullException("function");
		}
		MethodInfo m = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TResult));
		return (TArg1 t1, TArg2 t2, TArg3 t3, TArg4 t4) => provider.CreateQuery<TResult>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), function), Expression.Constant(t1, typeof(TArg1)), Expression.Constant(t2, typeof(TArg2)), Expression.Constant(t3, typeof(TArg3)), Expression.Constant(t4, typeof(TArg4))));
	}

	public static Func<TArg1, TArg2, TArg3, TArg4, IQbservable<TResult>> ToAsync<TArg1, TArg2, TArg3, TArg4, TResult>(this IQbservableProvider provider, Expression<Func<TArg1, TArg2, TArg3, TArg4, TResult>> function, IScheduler scheduler)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (function == null)
		{
			throw new ArgumentNullException("function");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		MethodInfo m = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TResult));
		return (TArg1 t1, TArg2 t2, TArg3 t3, TArg4 t4) => provider.CreateQuery<TResult>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), function, Expression.Constant(scheduler, typeof(IScheduler))), Expression.Constant(t1, typeof(TArg1)), Expression.Constant(t2, typeof(TArg2)), Expression.Constant(t3, typeof(TArg3)), Expression.Constant(t4, typeof(TArg4))));
	}

	public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, IQbservable<TResult>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5, TResult>(this IQbservableProvider provider, Expression<Func<TArg1, TArg2, TArg3, TArg4, TArg5, TResult>> function)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (function == null)
		{
			throw new ArgumentNullException("function");
		}
		MethodInfo m = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5), typeof(TResult));
		return (TArg1 t1, TArg2 t2, TArg3 t3, TArg4 t4, TArg5 t5) => provider.CreateQuery<TResult>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), function), Expression.Constant(t1, typeof(TArg1)), Expression.Constant(t2, typeof(TArg2)), Expression.Constant(t3, typeof(TArg3)), Expression.Constant(t4, typeof(TArg4)), Expression.Constant(t5, typeof(TArg5))));
	}

	public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, IQbservable<TResult>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5, TResult>(this IQbservableProvider provider, Expression<Func<TArg1, TArg2, TArg3, TArg4, TArg5, TResult>> function, IScheduler scheduler)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (function == null)
		{
			throw new ArgumentNullException("function");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		MethodInfo m = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5), typeof(TResult));
		return (TArg1 t1, TArg2 t2, TArg3 t3, TArg4 t4, TArg5 t5) => provider.CreateQuery<TResult>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), function, Expression.Constant(scheduler, typeof(IScheduler))), Expression.Constant(t1, typeof(TArg1)), Expression.Constant(t2, typeof(TArg2)), Expression.Constant(t3, typeof(TArg3)), Expression.Constant(t4, typeof(TArg4)), Expression.Constant(t5, typeof(TArg5))));
	}

	public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, IQbservable<TResult>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult>(this IQbservableProvider provider, Expression<Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult>> function)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (function == null)
		{
			throw new ArgumentNullException("function");
		}
		MethodInfo m = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5), typeof(TArg6), typeof(TResult));
		return (TArg1 t1, TArg2 t2, TArg3 t3, TArg4 t4, TArg5 t5, TArg6 t6) => provider.CreateQuery<TResult>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), function), Expression.Constant(t1, typeof(TArg1)), Expression.Constant(t2, typeof(TArg2)), Expression.Constant(t3, typeof(TArg3)), Expression.Constant(t4, typeof(TArg4)), Expression.Constant(t5, typeof(TArg5)), Expression.Constant(t6, typeof(TArg6))));
	}

	public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, IQbservable<TResult>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult>(this IQbservableProvider provider, Expression<Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult>> function, IScheduler scheduler)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (function == null)
		{
			throw new ArgumentNullException("function");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		MethodInfo m = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5), typeof(TArg6), typeof(TResult));
		return (TArg1 t1, TArg2 t2, TArg3 t3, TArg4 t4, TArg5 t5, TArg6 t6) => provider.CreateQuery<TResult>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), function, Expression.Constant(scheduler, typeof(IScheduler))), Expression.Constant(t1, typeof(TArg1)), Expression.Constant(t2, typeof(TArg2)), Expression.Constant(t3, typeof(TArg3)), Expression.Constant(t4, typeof(TArg4)), Expression.Constant(t5, typeof(TArg5)), Expression.Constant(t6, typeof(TArg6))));
	}

	public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, IQbservable<TResult>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult>(this IQbservableProvider provider, Expression<Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult>> function)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (function == null)
		{
			throw new ArgumentNullException("function");
		}
		MethodInfo m = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5), typeof(TArg6), typeof(TArg7), typeof(TResult));
		return (TArg1 t1, TArg2 t2, TArg3 t3, TArg4 t4, TArg5 t5, TArg6 t6, TArg7 t7) => provider.CreateQuery<TResult>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), function), Expression.Constant(t1, typeof(TArg1)), Expression.Constant(t2, typeof(TArg2)), Expression.Constant(t3, typeof(TArg3)), Expression.Constant(t4, typeof(TArg4)), Expression.Constant(t5, typeof(TArg5)), Expression.Constant(t6, typeof(TArg6)), Expression.Constant(t7, typeof(TArg7))));
	}

	public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, IQbservable<TResult>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult>(this IQbservableProvider provider, Expression<Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult>> function, IScheduler scheduler)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (function == null)
		{
			throw new ArgumentNullException("function");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		MethodInfo m = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5), typeof(TArg6), typeof(TArg7), typeof(TResult));
		return (TArg1 t1, TArg2 t2, TArg3 t3, TArg4 t4, TArg5 t5, TArg6 t6, TArg7 t7) => provider.CreateQuery<TResult>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), function, Expression.Constant(scheduler, typeof(IScheduler))), Expression.Constant(t1, typeof(TArg1)), Expression.Constant(t2, typeof(TArg2)), Expression.Constant(t3, typeof(TArg3)), Expression.Constant(t4, typeof(TArg4)), Expression.Constant(t5, typeof(TArg5)), Expression.Constant(t6, typeof(TArg6)), Expression.Constant(t7, typeof(TArg7))));
	}

	public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, IQbservable<TResult>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TResult>(this IQbservableProvider provider, Expression<Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TResult>> function)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (function == null)
		{
			throw new ArgumentNullException("function");
		}
		MethodInfo m = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5), typeof(TArg6), typeof(TArg7), typeof(TArg8), typeof(TResult));
		return (TArg1 t1, TArg2 t2, TArg3 t3, TArg4 t4, TArg5 t5, TArg6 t6, TArg7 t7, TArg8 t8) => provider.CreateQuery<TResult>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), function), Expression.Constant(t1, typeof(TArg1)), Expression.Constant(t2, typeof(TArg2)), Expression.Constant(t3, typeof(TArg3)), Expression.Constant(t4, typeof(TArg4)), Expression.Constant(t5, typeof(TArg5)), Expression.Constant(t6, typeof(TArg6)), Expression.Constant(t7, typeof(TArg7)), Expression.Constant(t8, typeof(TArg8))));
	}

	public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, IQbservable<TResult>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TResult>(this IQbservableProvider provider, Expression<Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TResult>> function, IScheduler scheduler)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (function == null)
		{
			throw new ArgumentNullException("function");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		MethodInfo m = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5), typeof(TArg6), typeof(TArg7), typeof(TArg8), typeof(TResult));
		return (TArg1 t1, TArg2 t2, TArg3 t3, TArg4 t4, TArg5 t5, TArg6 t6, TArg7 t7, TArg8 t8) => provider.CreateQuery<TResult>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), function, Expression.Constant(scheduler, typeof(IScheduler))), Expression.Constant(t1, typeof(TArg1)), Expression.Constant(t2, typeof(TArg2)), Expression.Constant(t3, typeof(TArg3)), Expression.Constant(t4, typeof(TArg4)), Expression.Constant(t5, typeof(TArg5)), Expression.Constant(t6, typeof(TArg6)), Expression.Constant(t7, typeof(TArg7)), Expression.Constant(t8, typeof(TArg8))));
	}

	public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, IQbservable<TResult>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TResult>(this IQbservableProvider provider, Expression<Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TResult>> function)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (function == null)
		{
			throw new ArgumentNullException("function");
		}
		MethodInfo m = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5), typeof(TArg6), typeof(TArg7), typeof(TArg8), typeof(TArg9), typeof(TResult));
		return (TArg1 t1, TArg2 t2, TArg3 t3, TArg4 t4, TArg5 t5, TArg6 t6, TArg7 t7, TArg8 t8, TArg9 t9) => provider.CreateQuery<TResult>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), function), Expression.Constant(t1, typeof(TArg1)), Expression.Constant(t2, typeof(TArg2)), Expression.Constant(t3, typeof(TArg3)), Expression.Constant(t4, typeof(TArg4)), Expression.Constant(t5, typeof(TArg5)), Expression.Constant(t6, typeof(TArg6)), Expression.Constant(t7, typeof(TArg7)), Expression.Constant(t8, typeof(TArg8)), Expression.Constant(t9, typeof(TArg9))));
	}

	public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, IQbservable<TResult>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TResult>(this IQbservableProvider provider, Expression<Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TResult>> function, IScheduler scheduler)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (function == null)
		{
			throw new ArgumentNullException("function");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		MethodInfo m = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5), typeof(TArg6), typeof(TArg7), typeof(TArg8), typeof(TArg9), typeof(TResult));
		return (TArg1 t1, TArg2 t2, TArg3 t3, TArg4 t4, TArg5 t5, TArg6 t6, TArg7 t7, TArg8 t8, TArg9 t9) => provider.CreateQuery<TResult>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), function, Expression.Constant(scheduler, typeof(IScheduler))), Expression.Constant(t1, typeof(TArg1)), Expression.Constant(t2, typeof(TArg2)), Expression.Constant(t3, typeof(TArg3)), Expression.Constant(t4, typeof(TArg4)), Expression.Constant(t5, typeof(TArg5)), Expression.Constant(t6, typeof(TArg6)), Expression.Constant(t7, typeof(TArg7)), Expression.Constant(t8, typeof(TArg8)), Expression.Constant(t9, typeof(TArg9))));
	}

	public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, IQbservable<TResult>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TResult>(this IQbservableProvider provider, Expression<Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TResult>> function)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (function == null)
		{
			throw new ArgumentNullException("function");
		}
		MethodInfo m = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5), typeof(TArg6), typeof(TArg7), typeof(TArg8), typeof(TArg9), typeof(TArg10), typeof(TResult));
		return (TArg1 t1, TArg2 t2, TArg3 t3, TArg4 t4, TArg5 t5, TArg6 t6, TArg7 t7, TArg8 t8, TArg9 t9, TArg10 t10) => provider.CreateQuery<TResult>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), function), Expression.Constant(t1, typeof(TArg1)), Expression.Constant(t2, typeof(TArg2)), Expression.Constant(t3, typeof(TArg3)), Expression.Constant(t4, typeof(TArg4)), Expression.Constant(t5, typeof(TArg5)), Expression.Constant(t6, typeof(TArg6)), Expression.Constant(t7, typeof(TArg7)), Expression.Constant(t8, typeof(TArg8)), Expression.Constant(t9, typeof(TArg9)), Expression.Constant(t10, typeof(TArg10))));
	}

	public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, IQbservable<TResult>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TResult>(this IQbservableProvider provider, Expression<Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TResult>> function, IScheduler scheduler)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (function == null)
		{
			throw new ArgumentNullException("function");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		MethodInfo m = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5), typeof(TArg6), typeof(TArg7), typeof(TArg8), typeof(TArg9), typeof(TArg10), typeof(TResult));
		return (TArg1 t1, TArg2 t2, TArg3 t3, TArg4 t4, TArg5 t5, TArg6 t6, TArg7 t7, TArg8 t8, TArg9 t9, TArg10 t10) => provider.CreateQuery<TResult>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), function, Expression.Constant(scheduler, typeof(IScheduler))), Expression.Constant(t1, typeof(TArg1)), Expression.Constant(t2, typeof(TArg2)), Expression.Constant(t3, typeof(TArg3)), Expression.Constant(t4, typeof(TArg4)), Expression.Constant(t5, typeof(TArg5)), Expression.Constant(t6, typeof(TArg6)), Expression.Constant(t7, typeof(TArg7)), Expression.Constant(t8, typeof(TArg8)), Expression.Constant(t9, typeof(TArg9)), Expression.Constant(t10, typeof(TArg10))));
	}

	public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, IQbservable<TResult>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TResult>(this IQbservableProvider provider, Expression<Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TResult>> function)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (function == null)
		{
			throw new ArgumentNullException("function");
		}
		MethodInfo m = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5), typeof(TArg6), typeof(TArg7), typeof(TArg8), typeof(TArg9), typeof(TArg10), typeof(TArg11), typeof(TResult));
		return (TArg1 t1, TArg2 t2, TArg3 t3, TArg4 t4, TArg5 t5, TArg6 t6, TArg7 t7, TArg8 t8, TArg9 t9, TArg10 t10, TArg11 t11) => provider.CreateQuery<TResult>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), function), Expression.Constant(t1, typeof(TArg1)), Expression.Constant(t2, typeof(TArg2)), Expression.Constant(t3, typeof(TArg3)), Expression.Constant(t4, typeof(TArg4)), Expression.Constant(t5, typeof(TArg5)), Expression.Constant(t6, typeof(TArg6)), Expression.Constant(t7, typeof(TArg7)), Expression.Constant(t8, typeof(TArg8)), Expression.Constant(t9, typeof(TArg9)), Expression.Constant(t10, typeof(TArg10)), Expression.Constant(t11, typeof(TArg11))));
	}

	public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, IQbservable<TResult>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TResult>(this IQbservableProvider provider, Expression<Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TResult>> function, IScheduler scheduler)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (function == null)
		{
			throw new ArgumentNullException("function");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		MethodInfo m = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5), typeof(TArg6), typeof(TArg7), typeof(TArg8), typeof(TArg9), typeof(TArg10), typeof(TArg11), typeof(TResult));
		return (TArg1 t1, TArg2 t2, TArg3 t3, TArg4 t4, TArg5 t5, TArg6 t6, TArg7 t7, TArg8 t8, TArg9 t9, TArg10 t10, TArg11 t11) => provider.CreateQuery<TResult>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), function, Expression.Constant(scheduler, typeof(IScheduler))), Expression.Constant(t1, typeof(TArg1)), Expression.Constant(t2, typeof(TArg2)), Expression.Constant(t3, typeof(TArg3)), Expression.Constant(t4, typeof(TArg4)), Expression.Constant(t5, typeof(TArg5)), Expression.Constant(t6, typeof(TArg6)), Expression.Constant(t7, typeof(TArg7)), Expression.Constant(t8, typeof(TArg8)), Expression.Constant(t9, typeof(TArg9)), Expression.Constant(t10, typeof(TArg10)), Expression.Constant(t11, typeof(TArg11))));
	}

	public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, IQbservable<TResult>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TResult>(this IQbservableProvider provider, Expression<Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TResult>> function)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (function == null)
		{
			throw new ArgumentNullException("function");
		}
		MethodInfo m = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5), typeof(TArg6), typeof(TArg7), typeof(TArg8), typeof(TArg9), typeof(TArg10), typeof(TArg11), typeof(TArg12), typeof(TResult));
		return (TArg1 t1, TArg2 t2, TArg3 t3, TArg4 t4, TArg5 t5, TArg6 t6, TArg7 t7, TArg8 t8, TArg9 t9, TArg10 t10, TArg11 t11, TArg12 t12) => provider.CreateQuery<TResult>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), function), Expression.Constant(t1, typeof(TArg1)), Expression.Constant(t2, typeof(TArg2)), Expression.Constant(t3, typeof(TArg3)), Expression.Constant(t4, typeof(TArg4)), Expression.Constant(t5, typeof(TArg5)), Expression.Constant(t6, typeof(TArg6)), Expression.Constant(t7, typeof(TArg7)), Expression.Constant(t8, typeof(TArg8)), Expression.Constant(t9, typeof(TArg9)), Expression.Constant(t10, typeof(TArg10)), Expression.Constant(t11, typeof(TArg11)), Expression.Constant(t12, typeof(TArg12))));
	}

	public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, IQbservable<TResult>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TResult>(this IQbservableProvider provider, Expression<Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TResult>> function, IScheduler scheduler)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (function == null)
		{
			throw new ArgumentNullException("function");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		MethodInfo m = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5), typeof(TArg6), typeof(TArg7), typeof(TArg8), typeof(TArg9), typeof(TArg10), typeof(TArg11), typeof(TArg12), typeof(TResult));
		return (TArg1 t1, TArg2 t2, TArg3 t3, TArg4 t4, TArg5 t5, TArg6 t6, TArg7 t7, TArg8 t8, TArg9 t9, TArg10 t10, TArg11 t11, TArg12 t12) => provider.CreateQuery<TResult>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), function, Expression.Constant(scheduler, typeof(IScheduler))), Expression.Constant(t1, typeof(TArg1)), Expression.Constant(t2, typeof(TArg2)), Expression.Constant(t3, typeof(TArg3)), Expression.Constant(t4, typeof(TArg4)), Expression.Constant(t5, typeof(TArg5)), Expression.Constant(t6, typeof(TArg6)), Expression.Constant(t7, typeof(TArg7)), Expression.Constant(t8, typeof(TArg8)), Expression.Constant(t9, typeof(TArg9)), Expression.Constant(t10, typeof(TArg10)), Expression.Constant(t11, typeof(TArg11)), Expression.Constant(t12, typeof(TArg12))));
	}

	public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, IQbservable<TResult>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TResult>(this IQbservableProvider provider, Expression<Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TResult>> function)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (function == null)
		{
			throw new ArgumentNullException("function");
		}
		MethodInfo m = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5), typeof(TArg6), typeof(TArg7), typeof(TArg8), typeof(TArg9), typeof(TArg10), typeof(TArg11), typeof(TArg12), typeof(TArg13), typeof(TResult));
		return (TArg1 t1, TArg2 t2, TArg3 t3, TArg4 t4, TArg5 t5, TArg6 t6, TArg7 t7, TArg8 t8, TArg9 t9, TArg10 t10, TArg11 t11, TArg12 t12, TArg13 t13) => provider.CreateQuery<TResult>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), function), Expression.Constant(t1, typeof(TArg1)), Expression.Constant(t2, typeof(TArg2)), Expression.Constant(t3, typeof(TArg3)), Expression.Constant(t4, typeof(TArg4)), Expression.Constant(t5, typeof(TArg5)), Expression.Constant(t6, typeof(TArg6)), Expression.Constant(t7, typeof(TArg7)), Expression.Constant(t8, typeof(TArg8)), Expression.Constant(t9, typeof(TArg9)), Expression.Constant(t10, typeof(TArg10)), Expression.Constant(t11, typeof(TArg11)), Expression.Constant(t12, typeof(TArg12)), Expression.Constant(t13, typeof(TArg13))));
	}

	public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, IQbservable<TResult>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TResult>(this IQbservableProvider provider, Expression<Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TResult>> function, IScheduler scheduler)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (function == null)
		{
			throw new ArgumentNullException("function");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		MethodInfo m = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5), typeof(TArg6), typeof(TArg7), typeof(TArg8), typeof(TArg9), typeof(TArg10), typeof(TArg11), typeof(TArg12), typeof(TArg13), typeof(TResult));
		return (TArg1 t1, TArg2 t2, TArg3 t3, TArg4 t4, TArg5 t5, TArg6 t6, TArg7 t7, TArg8 t8, TArg9 t9, TArg10 t10, TArg11 t11, TArg12 t12, TArg13 t13) => provider.CreateQuery<TResult>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), function, Expression.Constant(scheduler, typeof(IScheduler))), Expression.Constant(t1, typeof(TArg1)), Expression.Constant(t2, typeof(TArg2)), Expression.Constant(t3, typeof(TArg3)), Expression.Constant(t4, typeof(TArg4)), Expression.Constant(t5, typeof(TArg5)), Expression.Constant(t6, typeof(TArg6)), Expression.Constant(t7, typeof(TArg7)), Expression.Constant(t8, typeof(TArg8)), Expression.Constant(t9, typeof(TArg9)), Expression.Constant(t10, typeof(TArg10)), Expression.Constant(t11, typeof(TArg11)), Expression.Constant(t12, typeof(TArg12)), Expression.Constant(t13, typeof(TArg13))));
	}

	public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, IQbservable<TResult>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TResult>(this IQbservableProvider provider, Expression<Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TResult>> function)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (function == null)
		{
			throw new ArgumentNullException("function");
		}
		MethodInfo m = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5), typeof(TArg6), typeof(TArg7), typeof(TArg8), typeof(TArg9), typeof(TArg10), typeof(TArg11), typeof(TArg12), typeof(TArg13), typeof(TArg14), typeof(TResult));
		return (TArg1 t1, TArg2 t2, TArg3 t3, TArg4 t4, TArg5 t5, TArg6 t6, TArg7 t7, TArg8 t8, TArg9 t9, TArg10 t10, TArg11 t11, TArg12 t12, TArg13 t13, TArg14 t14) => provider.CreateQuery<TResult>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), function), Expression.Constant(t1, typeof(TArg1)), Expression.Constant(t2, typeof(TArg2)), Expression.Constant(t3, typeof(TArg3)), Expression.Constant(t4, typeof(TArg4)), Expression.Constant(t5, typeof(TArg5)), Expression.Constant(t6, typeof(TArg6)), Expression.Constant(t7, typeof(TArg7)), Expression.Constant(t8, typeof(TArg8)), Expression.Constant(t9, typeof(TArg9)), Expression.Constant(t10, typeof(TArg10)), Expression.Constant(t11, typeof(TArg11)), Expression.Constant(t12, typeof(TArg12)), Expression.Constant(t13, typeof(TArg13)), Expression.Constant(t14, typeof(TArg14))));
	}

	public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, IQbservable<TResult>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TResult>(this IQbservableProvider provider, Expression<Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TResult>> function, IScheduler scheduler)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (function == null)
		{
			throw new ArgumentNullException("function");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		MethodInfo m = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5), typeof(TArg6), typeof(TArg7), typeof(TArg8), typeof(TArg9), typeof(TArg10), typeof(TArg11), typeof(TArg12), typeof(TArg13), typeof(TArg14), typeof(TResult));
		return (TArg1 t1, TArg2 t2, TArg3 t3, TArg4 t4, TArg5 t5, TArg6 t6, TArg7 t7, TArg8 t8, TArg9 t9, TArg10 t10, TArg11 t11, TArg12 t12, TArg13 t13, TArg14 t14) => provider.CreateQuery<TResult>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), function, Expression.Constant(scheduler, typeof(IScheduler))), Expression.Constant(t1, typeof(TArg1)), Expression.Constant(t2, typeof(TArg2)), Expression.Constant(t3, typeof(TArg3)), Expression.Constant(t4, typeof(TArg4)), Expression.Constant(t5, typeof(TArg5)), Expression.Constant(t6, typeof(TArg6)), Expression.Constant(t7, typeof(TArg7)), Expression.Constant(t8, typeof(TArg8)), Expression.Constant(t9, typeof(TArg9)), Expression.Constant(t10, typeof(TArg10)), Expression.Constant(t11, typeof(TArg11)), Expression.Constant(t12, typeof(TArg12)), Expression.Constant(t13, typeof(TArg13)), Expression.Constant(t14, typeof(TArg14))));
	}

	public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, IQbservable<TResult>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, TResult>(this IQbservableProvider provider, Expression<Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, TResult>> function)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (function == null)
		{
			throw new ArgumentNullException("function");
		}
		MethodInfo m = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5), typeof(TArg6), typeof(TArg7), typeof(TArg8), typeof(TArg9), typeof(TArg10), typeof(TArg11), typeof(TArg12), typeof(TArg13), typeof(TArg14), typeof(TArg15), typeof(TResult));
		return (TArg1 t1, TArg2 t2, TArg3 t3, TArg4 t4, TArg5 t5, TArg6 t6, TArg7 t7, TArg8 t8, TArg9 t9, TArg10 t10, TArg11 t11, TArg12 t12, TArg13 t13, TArg14 t14, TArg15 t15) => provider.CreateQuery<TResult>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), function), Expression.Constant(t1, typeof(TArg1)), Expression.Constant(t2, typeof(TArg2)), Expression.Constant(t3, typeof(TArg3)), Expression.Constant(t4, typeof(TArg4)), Expression.Constant(t5, typeof(TArg5)), Expression.Constant(t6, typeof(TArg6)), Expression.Constant(t7, typeof(TArg7)), Expression.Constant(t8, typeof(TArg8)), Expression.Constant(t9, typeof(TArg9)), Expression.Constant(t10, typeof(TArg10)), Expression.Constant(t11, typeof(TArg11)), Expression.Constant(t12, typeof(TArg12)), Expression.Constant(t13, typeof(TArg13)), Expression.Constant(t14, typeof(TArg14)), Expression.Constant(t15, typeof(TArg15))));
	}

	public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, IQbservable<TResult>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, TResult>(this IQbservableProvider provider, Expression<Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, TResult>> function, IScheduler scheduler)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (function == null)
		{
			throw new ArgumentNullException("function");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		MethodInfo m = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5), typeof(TArg6), typeof(TArg7), typeof(TArg8), typeof(TArg9), typeof(TArg10), typeof(TArg11), typeof(TArg12), typeof(TArg13), typeof(TArg14), typeof(TArg15), typeof(TResult));
		return (TArg1 t1, TArg2 t2, TArg3 t3, TArg4 t4, TArg5 t5, TArg6 t6, TArg7 t7, TArg8 t8, TArg9 t9, TArg10 t10, TArg11 t11, TArg12 t12, TArg13 t13, TArg14 t14, TArg15 t15) => provider.CreateQuery<TResult>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), function, Expression.Constant(scheduler, typeof(IScheduler))), Expression.Constant(t1, typeof(TArg1)), Expression.Constant(t2, typeof(TArg2)), Expression.Constant(t3, typeof(TArg3)), Expression.Constant(t4, typeof(TArg4)), Expression.Constant(t5, typeof(TArg5)), Expression.Constant(t6, typeof(TArg6)), Expression.Constant(t7, typeof(TArg7)), Expression.Constant(t8, typeof(TArg8)), Expression.Constant(t9, typeof(TArg9)), Expression.Constant(t10, typeof(TArg10)), Expression.Constant(t11, typeof(TArg11)), Expression.Constant(t12, typeof(TArg12)), Expression.Constant(t13, typeof(TArg13)), Expression.Constant(t14, typeof(TArg14)), Expression.Constant(t15, typeof(TArg15))));
	}

	public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, TArg16, IQbservable<TResult>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, TArg16, TResult>(this IQbservableProvider provider, Expression<Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, TArg16, TResult>> function)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (function == null)
		{
			throw new ArgumentNullException("function");
		}
		MethodInfo m = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5), typeof(TArg6), typeof(TArg7), typeof(TArg8), typeof(TArg9), typeof(TArg10), typeof(TArg11), typeof(TArg12), typeof(TArg13), typeof(TArg14), typeof(TArg15), typeof(TArg16), typeof(TResult));
		return (TArg1 t1, TArg2 t2, TArg3 t3, TArg4 t4, TArg5 t5, TArg6 t6, TArg7 t7, TArg8 t8, TArg9 t9, TArg10 t10, TArg11 t11, TArg12 t12, TArg13 t13, TArg14 t14, TArg15 t15, TArg16 t16) => provider.CreateQuery<TResult>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), function), Expression.Constant(t1, typeof(TArg1)), Expression.Constant(t2, typeof(TArg2)), Expression.Constant(t3, typeof(TArg3)), Expression.Constant(t4, typeof(TArg4)), Expression.Constant(t5, typeof(TArg5)), Expression.Constant(t6, typeof(TArg6)), Expression.Constant(t7, typeof(TArg7)), Expression.Constant(t8, typeof(TArg8)), Expression.Constant(t9, typeof(TArg9)), Expression.Constant(t10, typeof(TArg10)), Expression.Constant(t11, typeof(TArg11)), Expression.Constant(t12, typeof(TArg12)), Expression.Constant(t13, typeof(TArg13)), Expression.Constant(t14, typeof(TArg14)), Expression.Constant(t15, typeof(TArg15)), Expression.Constant(t16, typeof(TArg16))));
	}

	public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, TArg16, IQbservable<TResult>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, TArg16, TResult>(this IQbservableProvider provider, Expression<Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, TArg16, TResult>> function, IScheduler scheduler)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (function == null)
		{
			throw new ArgumentNullException("function");
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		MethodInfo m = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5), typeof(TArg6), typeof(TArg7), typeof(TArg8), typeof(TArg9), typeof(TArg10), typeof(TArg11), typeof(TArg12), typeof(TArg13), typeof(TArg14), typeof(TArg15), typeof(TArg16), typeof(TResult));
		return (TArg1 t1, TArg2 t2, TArg3 t3, TArg4 t4, TArg5 t5, TArg6 t6, TArg7 t7, TArg8 t8, TArg9 t9, TArg10 t10, TArg11 t11, TArg12 t12, TArg13 t13, TArg14 t14, TArg15 t15, TArg16 t16) => provider.CreateQuery<TResult>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), function, Expression.Constant(scheduler, typeof(IScheduler))), Expression.Constant(t1, typeof(TArg1)), Expression.Constant(t2, typeof(TArg2)), Expression.Constant(t3, typeof(TArg3)), Expression.Constant(t4, typeof(TArg4)), Expression.Constant(t5, typeof(TArg5)), Expression.Constant(t6, typeof(TArg6)), Expression.Constant(t7, typeof(TArg7)), Expression.Constant(t8, typeof(TArg8)), Expression.Constant(t9, typeof(TArg9)), Expression.Constant(t10, typeof(TArg10)), Expression.Constant(t11, typeof(TArg11)), Expression.Constant(t12, typeof(TArg12)), Expression.Constant(t13, typeof(TArg13)), Expression.Constant(t14, typeof(TArg14)), Expression.Constant(t15, typeof(TArg15)), Expression.Constant(t16, typeof(TArg16))));
	}

	public static Func<IQbservable<Unit>> FromAsyncPattern(this IQbservableProvider provider, Expression<Func<AsyncCallback, object, IAsyncResult>> begin, Expression<Action<IAsyncResult>> end)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (begin == null)
		{
			throw new ArgumentNullException("begin");
		}
		if (end == null)
		{
			throw new ArgumentNullException("end");
		}
		MethodInfo m = (MethodInfo)MethodBase.GetCurrentMethod();
		return () => provider.CreateQuery<Unit>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), begin, end)));
	}

	public static Func<TArg1, IQbservable<Unit>> FromAsyncPattern<TArg1>(this IQbservableProvider provider, Expression<Func<TArg1, AsyncCallback, object, IAsyncResult>> begin, Expression<Action<IAsyncResult>> end)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (begin == null)
		{
			throw new ArgumentNullException("begin");
		}
		if (end == null)
		{
			throw new ArgumentNullException("end");
		}
		MethodInfo m = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TArg1));
		return (TArg1 t1) => provider.CreateQuery<Unit>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), begin, end), Expression.Constant(t1, typeof(TArg1))));
	}

	public static Func<TArg1, TArg2, IQbservable<Unit>> FromAsyncPattern<TArg1, TArg2>(this IQbservableProvider provider, Expression<Func<TArg1, TArg2, AsyncCallback, object, IAsyncResult>> begin, Expression<Action<IAsyncResult>> end)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (begin == null)
		{
			throw new ArgumentNullException("begin");
		}
		if (end == null)
		{
			throw new ArgumentNullException("end");
		}
		MethodInfo m = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TArg1), typeof(TArg2));
		return (TArg1 t1, TArg2 t2) => provider.CreateQuery<Unit>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), begin, end), Expression.Constant(t1, typeof(TArg1)), Expression.Constant(t2, typeof(TArg2))));
	}

	public static Func<TArg1, TArg2, TArg3, IQbservable<Unit>> FromAsyncPattern<TArg1, TArg2, TArg3>(this IQbservableProvider provider, Expression<Func<TArg1, TArg2, TArg3, AsyncCallback, object, IAsyncResult>> begin, Expression<Action<IAsyncResult>> end)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (begin == null)
		{
			throw new ArgumentNullException("begin");
		}
		if (end == null)
		{
			throw new ArgumentNullException("end");
		}
		MethodInfo m = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TArg1), typeof(TArg2), typeof(TArg3));
		return (TArg1 t1, TArg2 t2, TArg3 t3) => provider.CreateQuery<Unit>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), begin, end), Expression.Constant(t1, typeof(TArg1)), Expression.Constant(t2, typeof(TArg2)), Expression.Constant(t3, typeof(TArg3))));
	}

	public static Func<TArg1, TArg2, TArg3, TArg4, IQbservable<Unit>> FromAsyncPattern<TArg1, TArg2, TArg3, TArg4>(this IQbservableProvider provider, Expression<Func<TArg1, TArg2, TArg3, TArg4, AsyncCallback, object, IAsyncResult>> begin, Expression<Action<IAsyncResult>> end)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (begin == null)
		{
			throw new ArgumentNullException("begin");
		}
		if (end == null)
		{
			throw new ArgumentNullException("end");
		}
		MethodInfo m = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4));
		return (TArg1 t1, TArg2 t2, TArg3 t3, TArg4 t4) => provider.CreateQuery<Unit>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), begin, end), Expression.Constant(t1, typeof(TArg1)), Expression.Constant(t2, typeof(TArg2)), Expression.Constant(t3, typeof(TArg3)), Expression.Constant(t4, typeof(TArg4))));
	}

	public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, IQbservable<Unit>> FromAsyncPattern<TArg1, TArg2, TArg3, TArg4, TArg5>(this IQbservableProvider provider, Expression<Func<TArg1, TArg2, TArg3, TArg4, TArg5, AsyncCallback, object, IAsyncResult>> begin, Expression<Action<IAsyncResult>> end)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (begin == null)
		{
			throw new ArgumentNullException("begin");
		}
		if (end == null)
		{
			throw new ArgumentNullException("end");
		}
		MethodInfo m = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5));
		return (TArg1 t1, TArg2 t2, TArg3 t3, TArg4 t4, TArg5 t5) => provider.CreateQuery<Unit>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), begin, end), Expression.Constant(t1, typeof(TArg1)), Expression.Constant(t2, typeof(TArg2)), Expression.Constant(t3, typeof(TArg3)), Expression.Constant(t4, typeof(TArg4)), Expression.Constant(t5, typeof(TArg5))));
	}

	public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, IQbservable<Unit>> FromAsyncPattern<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>(this IQbservableProvider provider, Expression<Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, AsyncCallback, object, IAsyncResult>> begin, Expression<Action<IAsyncResult>> end)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (begin == null)
		{
			throw new ArgumentNullException("begin");
		}
		if (end == null)
		{
			throw new ArgumentNullException("end");
		}
		MethodInfo m = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5), typeof(TArg6));
		return (TArg1 t1, TArg2 t2, TArg3 t3, TArg4 t4, TArg5 t5, TArg6 t6) => provider.CreateQuery<Unit>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), begin, end), Expression.Constant(t1, typeof(TArg1)), Expression.Constant(t2, typeof(TArg2)), Expression.Constant(t3, typeof(TArg3)), Expression.Constant(t4, typeof(TArg4)), Expression.Constant(t5, typeof(TArg5)), Expression.Constant(t6, typeof(TArg6))));
	}

	public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, IQbservable<Unit>> FromAsyncPattern<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7>(this IQbservableProvider provider, Expression<Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, AsyncCallback, object, IAsyncResult>> begin, Expression<Action<IAsyncResult>> end)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (begin == null)
		{
			throw new ArgumentNullException("begin");
		}
		if (end == null)
		{
			throw new ArgumentNullException("end");
		}
		MethodInfo m = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5), typeof(TArg6), typeof(TArg7));
		return (TArg1 t1, TArg2 t2, TArg3 t3, TArg4 t4, TArg5 t5, TArg6 t6, TArg7 t7) => provider.CreateQuery<Unit>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), begin, end), Expression.Constant(t1, typeof(TArg1)), Expression.Constant(t2, typeof(TArg2)), Expression.Constant(t3, typeof(TArg3)), Expression.Constant(t4, typeof(TArg4)), Expression.Constant(t5, typeof(TArg5)), Expression.Constant(t6, typeof(TArg6)), Expression.Constant(t7, typeof(TArg7))));
	}

	public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, IQbservable<Unit>> FromAsyncPattern<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8>(this IQbservableProvider provider, Expression<Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, AsyncCallback, object, IAsyncResult>> begin, Expression<Action<IAsyncResult>> end)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (begin == null)
		{
			throw new ArgumentNullException("begin");
		}
		if (end == null)
		{
			throw new ArgumentNullException("end");
		}
		MethodInfo m = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5), typeof(TArg6), typeof(TArg7), typeof(TArg8));
		return (TArg1 t1, TArg2 t2, TArg3 t3, TArg4 t4, TArg5 t5, TArg6 t6, TArg7 t7, TArg8 t8) => provider.CreateQuery<Unit>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), begin, end), Expression.Constant(t1, typeof(TArg1)), Expression.Constant(t2, typeof(TArg2)), Expression.Constant(t3, typeof(TArg3)), Expression.Constant(t4, typeof(TArg4)), Expression.Constant(t5, typeof(TArg5)), Expression.Constant(t6, typeof(TArg6)), Expression.Constant(t7, typeof(TArg7)), Expression.Constant(t8, typeof(TArg8))));
	}

	public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, IQbservable<Unit>> FromAsyncPattern<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9>(this IQbservableProvider provider, Expression<Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, AsyncCallback, object, IAsyncResult>> begin, Expression<Action<IAsyncResult>> end)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (begin == null)
		{
			throw new ArgumentNullException("begin");
		}
		if (end == null)
		{
			throw new ArgumentNullException("end");
		}
		MethodInfo m = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5), typeof(TArg6), typeof(TArg7), typeof(TArg8), typeof(TArg9));
		return (TArg1 t1, TArg2 t2, TArg3 t3, TArg4 t4, TArg5 t5, TArg6 t6, TArg7 t7, TArg8 t8, TArg9 t9) => provider.CreateQuery<Unit>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), begin, end), Expression.Constant(t1, typeof(TArg1)), Expression.Constant(t2, typeof(TArg2)), Expression.Constant(t3, typeof(TArg3)), Expression.Constant(t4, typeof(TArg4)), Expression.Constant(t5, typeof(TArg5)), Expression.Constant(t6, typeof(TArg6)), Expression.Constant(t7, typeof(TArg7)), Expression.Constant(t8, typeof(TArg8)), Expression.Constant(t9, typeof(TArg9))));
	}

	public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, IQbservable<Unit>> FromAsyncPattern<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10>(this IQbservableProvider provider, Expression<Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, AsyncCallback, object, IAsyncResult>> begin, Expression<Action<IAsyncResult>> end)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (begin == null)
		{
			throw new ArgumentNullException("begin");
		}
		if (end == null)
		{
			throw new ArgumentNullException("end");
		}
		MethodInfo m = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5), typeof(TArg6), typeof(TArg7), typeof(TArg8), typeof(TArg9), typeof(TArg10));
		return (TArg1 t1, TArg2 t2, TArg3 t3, TArg4 t4, TArg5 t5, TArg6 t6, TArg7 t7, TArg8 t8, TArg9 t9, TArg10 t10) => provider.CreateQuery<Unit>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), begin, end), Expression.Constant(t1, typeof(TArg1)), Expression.Constant(t2, typeof(TArg2)), Expression.Constant(t3, typeof(TArg3)), Expression.Constant(t4, typeof(TArg4)), Expression.Constant(t5, typeof(TArg5)), Expression.Constant(t6, typeof(TArg6)), Expression.Constant(t7, typeof(TArg7)), Expression.Constant(t8, typeof(TArg8)), Expression.Constant(t9, typeof(TArg9)), Expression.Constant(t10, typeof(TArg10))));
	}

	public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, IQbservable<Unit>> FromAsyncPattern<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11>(this IQbservableProvider provider, Expression<Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, AsyncCallback, object, IAsyncResult>> begin, Expression<Action<IAsyncResult>> end)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (begin == null)
		{
			throw new ArgumentNullException("begin");
		}
		if (end == null)
		{
			throw new ArgumentNullException("end");
		}
		MethodInfo m = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5), typeof(TArg6), typeof(TArg7), typeof(TArg8), typeof(TArg9), typeof(TArg10), typeof(TArg11));
		return (TArg1 t1, TArg2 t2, TArg3 t3, TArg4 t4, TArg5 t5, TArg6 t6, TArg7 t7, TArg8 t8, TArg9 t9, TArg10 t10, TArg11 t11) => provider.CreateQuery<Unit>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), begin, end), Expression.Constant(t1, typeof(TArg1)), Expression.Constant(t2, typeof(TArg2)), Expression.Constant(t3, typeof(TArg3)), Expression.Constant(t4, typeof(TArg4)), Expression.Constant(t5, typeof(TArg5)), Expression.Constant(t6, typeof(TArg6)), Expression.Constant(t7, typeof(TArg7)), Expression.Constant(t8, typeof(TArg8)), Expression.Constant(t9, typeof(TArg9)), Expression.Constant(t10, typeof(TArg10)), Expression.Constant(t11, typeof(TArg11))));
	}

	public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, IQbservable<Unit>> FromAsyncPattern<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12>(this IQbservableProvider provider, Expression<Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, AsyncCallback, object, IAsyncResult>> begin, Expression<Action<IAsyncResult>> end)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (begin == null)
		{
			throw new ArgumentNullException("begin");
		}
		if (end == null)
		{
			throw new ArgumentNullException("end");
		}
		MethodInfo m = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5), typeof(TArg6), typeof(TArg7), typeof(TArg8), typeof(TArg9), typeof(TArg10), typeof(TArg11), typeof(TArg12));
		return (TArg1 t1, TArg2 t2, TArg3 t3, TArg4 t4, TArg5 t5, TArg6 t6, TArg7 t7, TArg8 t8, TArg9 t9, TArg10 t10, TArg11 t11, TArg12 t12) => provider.CreateQuery<Unit>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), begin, end), Expression.Constant(t1, typeof(TArg1)), Expression.Constant(t2, typeof(TArg2)), Expression.Constant(t3, typeof(TArg3)), Expression.Constant(t4, typeof(TArg4)), Expression.Constant(t5, typeof(TArg5)), Expression.Constant(t6, typeof(TArg6)), Expression.Constant(t7, typeof(TArg7)), Expression.Constant(t8, typeof(TArg8)), Expression.Constant(t9, typeof(TArg9)), Expression.Constant(t10, typeof(TArg10)), Expression.Constant(t11, typeof(TArg11)), Expression.Constant(t12, typeof(TArg12))));
	}

	public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, IQbservable<Unit>> FromAsyncPattern<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13>(this IQbservableProvider provider, Expression<Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, AsyncCallback, object, IAsyncResult>> begin, Expression<Action<IAsyncResult>> end)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (begin == null)
		{
			throw new ArgumentNullException("begin");
		}
		if (end == null)
		{
			throw new ArgumentNullException("end");
		}
		MethodInfo m = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5), typeof(TArg6), typeof(TArg7), typeof(TArg8), typeof(TArg9), typeof(TArg10), typeof(TArg11), typeof(TArg12), typeof(TArg13));
		return (TArg1 t1, TArg2 t2, TArg3 t3, TArg4 t4, TArg5 t5, TArg6 t6, TArg7 t7, TArg8 t8, TArg9 t9, TArg10 t10, TArg11 t11, TArg12 t12, TArg13 t13) => provider.CreateQuery<Unit>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), begin, end), Expression.Constant(t1, typeof(TArg1)), Expression.Constant(t2, typeof(TArg2)), Expression.Constant(t3, typeof(TArg3)), Expression.Constant(t4, typeof(TArg4)), Expression.Constant(t5, typeof(TArg5)), Expression.Constant(t6, typeof(TArg6)), Expression.Constant(t7, typeof(TArg7)), Expression.Constant(t8, typeof(TArg8)), Expression.Constant(t9, typeof(TArg9)), Expression.Constant(t10, typeof(TArg10)), Expression.Constant(t11, typeof(TArg11)), Expression.Constant(t12, typeof(TArg12)), Expression.Constant(t13, typeof(TArg13))));
	}

	public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, IQbservable<Unit>> FromAsyncPattern<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14>(this IQbservableProvider provider, Expression<Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, AsyncCallback, object, IAsyncResult>> begin, Expression<Action<IAsyncResult>> end)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (begin == null)
		{
			throw new ArgumentNullException("begin");
		}
		if (end == null)
		{
			throw new ArgumentNullException("end");
		}
		MethodInfo m = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5), typeof(TArg6), typeof(TArg7), typeof(TArg8), typeof(TArg9), typeof(TArg10), typeof(TArg11), typeof(TArg12), typeof(TArg13), typeof(TArg14));
		return (TArg1 t1, TArg2 t2, TArg3 t3, TArg4 t4, TArg5 t5, TArg6 t6, TArg7 t7, TArg8 t8, TArg9 t9, TArg10 t10, TArg11 t11, TArg12 t12, TArg13 t13, TArg14 t14) => provider.CreateQuery<Unit>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), begin, end), Expression.Constant(t1, typeof(TArg1)), Expression.Constant(t2, typeof(TArg2)), Expression.Constant(t3, typeof(TArg3)), Expression.Constant(t4, typeof(TArg4)), Expression.Constant(t5, typeof(TArg5)), Expression.Constant(t6, typeof(TArg6)), Expression.Constant(t7, typeof(TArg7)), Expression.Constant(t8, typeof(TArg8)), Expression.Constant(t9, typeof(TArg9)), Expression.Constant(t10, typeof(TArg10)), Expression.Constant(t11, typeof(TArg11)), Expression.Constant(t12, typeof(TArg12)), Expression.Constant(t13, typeof(TArg13)), Expression.Constant(t14, typeof(TArg14))));
	}

	public static Func<IQbservable<TResult>> FromAsyncPattern<TResult>(this IQbservableProvider provider, Expression<Func<AsyncCallback, object, IAsyncResult>> begin, Expression<Func<IAsyncResult, TResult>> end)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (begin == null)
		{
			throw new ArgumentNullException("begin");
		}
		if (end == null)
		{
			throw new ArgumentNullException("end");
		}
		MethodInfo m = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TResult));
		return () => provider.CreateQuery<TResult>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), begin, end)));
	}

	public static Func<TArg1, IQbservable<TResult>> FromAsyncPattern<TArg1, TResult>(this IQbservableProvider provider, Expression<Func<TArg1, AsyncCallback, object, IAsyncResult>> begin, Expression<Func<IAsyncResult, TResult>> end)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (begin == null)
		{
			throw new ArgumentNullException("begin");
		}
		if (end == null)
		{
			throw new ArgumentNullException("end");
		}
		MethodInfo m = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TArg1), typeof(TResult));
		return (TArg1 t1) => provider.CreateQuery<TResult>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), begin, end), Expression.Constant(t1, typeof(TArg1))));
	}

	public static Func<TArg1, TArg2, IQbservable<TResult>> FromAsyncPattern<TArg1, TArg2, TResult>(this IQbservableProvider provider, Expression<Func<TArg1, TArg2, AsyncCallback, object, IAsyncResult>> begin, Expression<Func<IAsyncResult, TResult>> end)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (begin == null)
		{
			throw new ArgumentNullException("begin");
		}
		if (end == null)
		{
			throw new ArgumentNullException("end");
		}
		MethodInfo m = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TArg1), typeof(TArg2), typeof(TResult));
		return (TArg1 t1, TArg2 t2) => provider.CreateQuery<TResult>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), begin, end), Expression.Constant(t1, typeof(TArg1)), Expression.Constant(t2, typeof(TArg2))));
	}

	public static Func<TArg1, TArg2, TArg3, IQbservable<TResult>> FromAsyncPattern<TArg1, TArg2, TArg3, TResult>(this IQbservableProvider provider, Expression<Func<TArg1, TArg2, TArg3, AsyncCallback, object, IAsyncResult>> begin, Expression<Func<IAsyncResult, TResult>> end)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (begin == null)
		{
			throw new ArgumentNullException("begin");
		}
		if (end == null)
		{
			throw new ArgumentNullException("end");
		}
		MethodInfo m = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TResult));
		return (TArg1 t1, TArg2 t2, TArg3 t3) => provider.CreateQuery<TResult>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), begin, end), Expression.Constant(t1, typeof(TArg1)), Expression.Constant(t2, typeof(TArg2)), Expression.Constant(t3, typeof(TArg3))));
	}

	public static Func<TArg1, TArg2, TArg3, TArg4, IQbservable<TResult>> FromAsyncPattern<TArg1, TArg2, TArg3, TArg4, TResult>(this IQbservableProvider provider, Expression<Func<TArg1, TArg2, TArg3, TArg4, AsyncCallback, object, IAsyncResult>> begin, Expression<Func<IAsyncResult, TResult>> end)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (begin == null)
		{
			throw new ArgumentNullException("begin");
		}
		if (end == null)
		{
			throw new ArgumentNullException("end");
		}
		MethodInfo m = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TResult));
		return (TArg1 t1, TArg2 t2, TArg3 t3, TArg4 t4) => provider.CreateQuery<TResult>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), begin, end), Expression.Constant(t1, typeof(TArg1)), Expression.Constant(t2, typeof(TArg2)), Expression.Constant(t3, typeof(TArg3)), Expression.Constant(t4, typeof(TArg4))));
	}

	public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, IQbservable<TResult>> FromAsyncPattern<TArg1, TArg2, TArg3, TArg4, TArg5, TResult>(this IQbservableProvider provider, Expression<Func<TArg1, TArg2, TArg3, TArg4, TArg5, AsyncCallback, object, IAsyncResult>> begin, Expression<Func<IAsyncResult, TResult>> end)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (begin == null)
		{
			throw new ArgumentNullException("begin");
		}
		if (end == null)
		{
			throw new ArgumentNullException("end");
		}
		MethodInfo m = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5), typeof(TResult));
		return (TArg1 t1, TArg2 t2, TArg3 t3, TArg4 t4, TArg5 t5) => provider.CreateQuery<TResult>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), begin, end), Expression.Constant(t1, typeof(TArg1)), Expression.Constant(t2, typeof(TArg2)), Expression.Constant(t3, typeof(TArg3)), Expression.Constant(t4, typeof(TArg4)), Expression.Constant(t5, typeof(TArg5))));
	}

	public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, IQbservable<TResult>> FromAsyncPattern<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult>(this IQbservableProvider provider, Expression<Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, AsyncCallback, object, IAsyncResult>> begin, Expression<Func<IAsyncResult, TResult>> end)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (begin == null)
		{
			throw new ArgumentNullException("begin");
		}
		if (end == null)
		{
			throw new ArgumentNullException("end");
		}
		MethodInfo m = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5), typeof(TArg6), typeof(TResult));
		return (TArg1 t1, TArg2 t2, TArg3 t3, TArg4 t4, TArg5 t5, TArg6 t6) => provider.CreateQuery<TResult>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), begin, end), Expression.Constant(t1, typeof(TArg1)), Expression.Constant(t2, typeof(TArg2)), Expression.Constant(t3, typeof(TArg3)), Expression.Constant(t4, typeof(TArg4)), Expression.Constant(t5, typeof(TArg5)), Expression.Constant(t6, typeof(TArg6))));
	}

	public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, IQbservable<TResult>> FromAsyncPattern<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult>(this IQbservableProvider provider, Expression<Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, AsyncCallback, object, IAsyncResult>> begin, Expression<Func<IAsyncResult, TResult>> end)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (begin == null)
		{
			throw new ArgumentNullException("begin");
		}
		if (end == null)
		{
			throw new ArgumentNullException("end");
		}
		MethodInfo m = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5), typeof(TArg6), typeof(TArg7), typeof(TResult));
		return (TArg1 t1, TArg2 t2, TArg3 t3, TArg4 t4, TArg5 t5, TArg6 t6, TArg7 t7) => provider.CreateQuery<TResult>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), begin, end), Expression.Constant(t1, typeof(TArg1)), Expression.Constant(t2, typeof(TArg2)), Expression.Constant(t3, typeof(TArg3)), Expression.Constant(t4, typeof(TArg4)), Expression.Constant(t5, typeof(TArg5)), Expression.Constant(t6, typeof(TArg6)), Expression.Constant(t7, typeof(TArg7))));
	}

	public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, IQbservable<TResult>> FromAsyncPattern<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TResult>(this IQbservableProvider provider, Expression<Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, AsyncCallback, object, IAsyncResult>> begin, Expression<Func<IAsyncResult, TResult>> end)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (begin == null)
		{
			throw new ArgumentNullException("begin");
		}
		if (end == null)
		{
			throw new ArgumentNullException("end");
		}
		MethodInfo m = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5), typeof(TArg6), typeof(TArg7), typeof(TArg8), typeof(TResult));
		return (TArg1 t1, TArg2 t2, TArg3 t3, TArg4 t4, TArg5 t5, TArg6 t6, TArg7 t7, TArg8 t8) => provider.CreateQuery<TResult>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), begin, end), Expression.Constant(t1, typeof(TArg1)), Expression.Constant(t2, typeof(TArg2)), Expression.Constant(t3, typeof(TArg3)), Expression.Constant(t4, typeof(TArg4)), Expression.Constant(t5, typeof(TArg5)), Expression.Constant(t6, typeof(TArg6)), Expression.Constant(t7, typeof(TArg7)), Expression.Constant(t8, typeof(TArg8))));
	}

	public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, IQbservable<TResult>> FromAsyncPattern<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TResult>(this IQbservableProvider provider, Expression<Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, AsyncCallback, object, IAsyncResult>> begin, Expression<Func<IAsyncResult, TResult>> end)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (begin == null)
		{
			throw new ArgumentNullException("begin");
		}
		if (end == null)
		{
			throw new ArgumentNullException("end");
		}
		MethodInfo m = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5), typeof(TArg6), typeof(TArg7), typeof(TArg8), typeof(TArg9), typeof(TResult));
		return (TArg1 t1, TArg2 t2, TArg3 t3, TArg4 t4, TArg5 t5, TArg6 t6, TArg7 t7, TArg8 t8, TArg9 t9) => provider.CreateQuery<TResult>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), begin, end), Expression.Constant(t1, typeof(TArg1)), Expression.Constant(t2, typeof(TArg2)), Expression.Constant(t3, typeof(TArg3)), Expression.Constant(t4, typeof(TArg4)), Expression.Constant(t5, typeof(TArg5)), Expression.Constant(t6, typeof(TArg6)), Expression.Constant(t7, typeof(TArg7)), Expression.Constant(t8, typeof(TArg8)), Expression.Constant(t9, typeof(TArg9))));
	}

	public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, IQbservable<TResult>> FromAsyncPattern<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TResult>(this IQbservableProvider provider, Expression<Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, AsyncCallback, object, IAsyncResult>> begin, Expression<Func<IAsyncResult, TResult>> end)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (begin == null)
		{
			throw new ArgumentNullException("begin");
		}
		if (end == null)
		{
			throw new ArgumentNullException("end");
		}
		MethodInfo m = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5), typeof(TArg6), typeof(TArg7), typeof(TArg8), typeof(TArg9), typeof(TArg10), typeof(TResult));
		return (TArg1 t1, TArg2 t2, TArg3 t3, TArg4 t4, TArg5 t5, TArg6 t6, TArg7 t7, TArg8 t8, TArg9 t9, TArg10 t10) => provider.CreateQuery<TResult>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), begin, end), Expression.Constant(t1, typeof(TArg1)), Expression.Constant(t2, typeof(TArg2)), Expression.Constant(t3, typeof(TArg3)), Expression.Constant(t4, typeof(TArg4)), Expression.Constant(t5, typeof(TArg5)), Expression.Constant(t6, typeof(TArg6)), Expression.Constant(t7, typeof(TArg7)), Expression.Constant(t8, typeof(TArg8)), Expression.Constant(t9, typeof(TArg9)), Expression.Constant(t10, typeof(TArg10))));
	}

	public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, IQbservable<TResult>> FromAsyncPattern<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TResult>(this IQbservableProvider provider, Expression<Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, AsyncCallback, object, IAsyncResult>> begin, Expression<Func<IAsyncResult, TResult>> end)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (begin == null)
		{
			throw new ArgumentNullException("begin");
		}
		if (end == null)
		{
			throw new ArgumentNullException("end");
		}
		MethodInfo m = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5), typeof(TArg6), typeof(TArg7), typeof(TArg8), typeof(TArg9), typeof(TArg10), typeof(TArg11), typeof(TResult));
		return (TArg1 t1, TArg2 t2, TArg3 t3, TArg4 t4, TArg5 t5, TArg6 t6, TArg7 t7, TArg8 t8, TArg9 t9, TArg10 t10, TArg11 t11) => provider.CreateQuery<TResult>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), begin, end), Expression.Constant(t1, typeof(TArg1)), Expression.Constant(t2, typeof(TArg2)), Expression.Constant(t3, typeof(TArg3)), Expression.Constant(t4, typeof(TArg4)), Expression.Constant(t5, typeof(TArg5)), Expression.Constant(t6, typeof(TArg6)), Expression.Constant(t7, typeof(TArg7)), Expression.Constant(t8, typeof(TArg8)), Expression.Constant(t9, typeof(TArg9)), Expression.Constant(t10, typeof(TArg10)), Expression.Constant(t11, typeof(TArg11))));
	}

	public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, IQbservable<TResult>> FromAsyncPattern<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TResult>(this IQbservableProvider provider, Expression<Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, AsyncCallback, object, IAsyncResult>> begin, Expression<Func<IAsyncResult, TResult>> end)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (begin == null)
		{
			throw new ArgumentNullException("begin");
		}
		if (end == null)
		{
			throw new ArgumentNullException("end");
		}
		MethodInfo m = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5), typeof(TArg6), typeof(TArg7), typeof(TArg8), typeof(TArg9), typeof(TArg10), typeof(TArg11), typeof(TArg12), typeof(TResult));
		return (TArg1 t1, TArg2 t2, TArg3 t3, TArg4 t4, TArg5 t5, TArg6 t6, TArg7 t7, TArg8 t8, TArg9 t9, TArg10 t10, TArg11 t11, TArg12 t12) => provider.CreateQuery<TResult>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), begin, end), Expression.Constant(t1, typeof(TArg1)), Expression.Constant(t2, typeof(TArg2)), Expression.Constant(t3, typeof(TArg3)), Expression.Constant(t4, typeof(TArg4)), Expression.Constant(t5, typeof(TArg5)), Expression.Constant(t6, typeof(TArg6)), Expression.Constant(t7, typeof(TArg7)), Expression.Constant(t8, typeof(TArg8)), Expression.Constant(t9, typeof(TArg9)), Expression.Constant(t10, typeof(TArg10)), Expression.Constant(t11, typeof(TArg11)), Expression.Constant(t12, typeof(TArg12))));
	}

	public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, IQbservable<TResult>> FromAsyncPattern<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TResult>(this IQbservableProvider provider, Expression<Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, AsyncCallback, object, IAsyncResult>> begin, Expression<Func<IAsyncResult, TResult>> end)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (begin == null)
		{
			throw new ArgumentNullException("begin");
		}
		if (end == null)
		{
			throw new ArgumentNullException("end");
		}
		MethodInfo m = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5), typeof(TArg6), typeof(TArg7), typeof(TArg8), typeof(TArg9), typeof(TArg10), typeof(TArg11), typeof(TArg12), typeof(TArg13), typeof(TResult));
		return (TArg1 t1, TArg2 t2, TArg3 t3, TArg4 t4, TArg5 t5, TArg6 t6, TArg7 t7, TArg8 t8, TArg9 t9, TArg10 t10, TArg11 t11, TArg12 t12, TArg13 t13) => provider.CreateQuery<TResult>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), begin, end), Expression.Constant(t1, typeof(TArg1)), Expression.Constant(t2, typeof(TArg2)), Expression.Constant(t3, typeof(TArg3)), Expression.Constant(t4, typeof(TArg4)), Expression.Constant(t5, typeof(TArg5)), Expression.Constant(t6, typeof(TArg6)), Expression.Constant(t7, typeof(TArg7)), Expression.Constant(t8, typeof(TArg8)), Expression.Constant(t9, typeof(TArg9)), Expression.Constant(t10, typeof(TArg10)), Expression.Constant(t11, typeof(TArg11)), Expression.Constant(t12, typeof(TArg12)), Expression.Constant(t13, typeof(TArg13))));
	}

	public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, IQbservable<TResult>> FromAsyncPattern<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TResult>(this IQbservableProvider provider, Expression<Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, AsyncCallback, object, IAsyncResult>> begin, Expression<Func<IAsyncResult, TResult>> end)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (begin == null)
		{
			throw new ArgumentNullException("begin");
		}
		if (end == null)
		{
			throw new ArgumentNullException("end");
		}
		MethodInfo m = ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5), typeof(TArg6), typeof(TArg7), typeof(TArg8), typeof(TArg9), typeof(TArg10), typeof(TArg11), typeof(TArg12), typeof(TArg13), typeof(TArg14), typeof(TResult));
		return (TArg1 t1, TArg2 t2, TArg3 t3, TArg4 t4, TArg5 t5, TArg6 t6, TArg7 t7, TArg8 t8, TArg9 t9, TArg10 t10, TArg11 t11, TArg12 t12, TArg13 t13, TArg14 t14) => provider.CreateQuery<TResult>(Expression.Invoke(Expression.Call(null, m, Expression.Constant(provider, typeof(IQbservableProvider)), begin, end), Expression.Constant(t1, typeof(TArg1)), Expression.Constant(t2, typeof(TArg2)), Expression.Constant(t3, typeof(TArg3)), Expression.Constant(t4, typeof(TArg4)), Expression.Constant(t5, typeof(TArg5)), Expression.Constant(t6, typeof(TArg6)), Expression.Constant(t7, typeof(TArg7)), Expression.Constant(t8, typeof(TArg8)), Expression.Constant(t9, typeof(TArg9)), Expression.Constant(t10, typeof(TArg10)), Expression.Constant(t11, typeof(TArg11)), Expression.Constant(t12, typeof(TArg12)), Expression.Constant(t13, typeof(TArg13)), Expression.Constant(t14, typeof(TArg14))));
	}

	public static QueryablePattern<TLeft, TRight> And<TLeft, TRight>(this IQbservable<TLeft> left, IObservable<TRight> right)
	{
		if (left == null)
		{
			throw new ArgumentNullException("left");
		}
		if (right == null)
		{
			throw new ArgumentNullException("right");
		}
		return new QueryablePattern<TLeft, TRight>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TLeft), typeof(TRight)), left.Expression, GetSourceExpression(right)));
	}

	public static QueryablePlan<TResult> Then<TSource, TResult>(this IQbservable<TSource> source, Expression<Func<TSource, TResult>> selector)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		return new QueryablePlan<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TResult)), source.Expression, selector));
	}

	public static IQbservable<TResult> When<TResult>(this IQbservableProvider provider, params QueryablePlan<TResult>[] plans)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (plans == null)
		{
			throw new ArgumentNullException("plans");
		}
		return provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TResult)), Expression.Constant(provider, typeof(IQbservableProvider)), Expression.NewArrayInit(typeof(QueryablePlan<TResult>), plans.Select((QueryablePlan<TResult> p) => p.Expression))));
	}

	public static IQbservable<TResult> When<TResult>(this IQbservableProvider provider, IEnumerable<QueryablePlan<TResult>> plans)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (plans == null)
		{
			throw new ArgumentNullException("plans");
		}
		return provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TResult)), Expression.Constant(provider, typeof(IQbservableProvider)), Expression.Constant(plans, typeof(IEnumerable<QueryablePlan<TResult>>))));
	}
}
