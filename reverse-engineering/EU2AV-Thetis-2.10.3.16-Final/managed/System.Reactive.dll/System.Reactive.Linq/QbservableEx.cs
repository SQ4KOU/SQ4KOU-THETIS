using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Concurrency;
using System.Reflection;

namespace System.Reactive.Linq;

[LocalQueryMethodImplementationType(typeof(ObservableEx))]
public static class QbservableEx
{
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

	[Experimental]
	public static IQbservable<Unit> Create(this IQbservableProvider provider, Expression<Func<IEnumerable<IObservable<object>>>> iteratorMethod)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (iteratorMethod == null)
		{
			throw new ArgumentNullException("iteratorMethod");
		}
		return provider.CreateQuery<Unit>(Expression.Call(null, (MethodInfo)MethodBase.GetCurrentMethod(), Expression.Constant(provider, typeof(IQbservableProvider)), iteratorMethod));
	}

	[Experimental]
	public static IQbservable<TResult> Create<TResult>(this IQbservableProvider provider, Expression<Func<IObserver<TResult>, IEnumerable<IObservable<object>>>> iteratorMethod)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (iteratorMethod == null)
		{
			throw new ArgumentNullException("iteratorMethod");
		}
		return provider.CreateQuery<TResult>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TResult)), Expression.Constant(provider, typeof(IQbservableProvider)), iteratorMethod));
	}

	[Experimental]
	public static IQbservable<TSource> Expand<TSource>(this IQbservable<TSource> source, Expression<Func<TSource, IObservable<TSource>>> selector)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, selector));
	}

	[Experimental]
	public static IQbservable<TSource> Expand<TSource>(this IQbservable<TSource> source, Expression<Func<TSource, IObservable<TSource>>> selector, IScheduler scheduler)
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
		return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression, selector, Expression.Constant(scheduler, typeof(IScheduler))));
	}

	[Experimental]
	public static IQbservable<TSource[]> ForkJoin<TSource>(this IQbservableProvider provider, params IObservable<TSource>[] sources)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (sources == null)
		{
			throw new ArgumentNullException("sources");
		}
		return provider.CreateQuery<TSource[]>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), Expression.Constant(provider, typeof(IQbservableProvider)), GetSourceExpression(sources)));
	}

	[Experimental]
	public static IQbservable<TSource[]> ForkJoin<TSource>(this IQbservableProvider provider, IEnumerable<IObservable<TSource>> sources)
	{
		if (provider == null)
		{
			throw new ArgumentNullException("provider");
		}
		if (sources == null)
		{
			throw new ArgumentNullException("sources");
		}
		return provider.CreateQuery<TSource[]>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), Expression.Constant(provider, typeof(IQbservableProvider)), GetSourceExpression(sources)));
	}

	[Experimental]
	public static IQbservable<TResult> ForkJoin<TSource1, TSource2, TResult>(this IQbservable<TSource1> first, IObservable<TSource2> second, Expression<Func<TSource1, TSource2, TResult>> resultSelector)
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

	[Experimental]
	public static IQbservable<TResult> Let<TSource, TResult>(this IQbservable<TSource> source, Expression<Func<IObservable<TSource>, IObservable<TResult>>> selector)
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

	[Experimental]
	public static IQbservable<TResult> ManySelect<TSource, TResult>(this IQbservable<TSource> source, Expression<Func<IObservable<TSource>, TResult>> selector)
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

	[Experimental]
	public static IQbservable<TResult> ManySelect<TSource, TResult>(this IQbservable<TSource> source, Expression<Func<IObservable<TSource>, TResult>> selector, IScheduler scheduler)
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

	public static IQbservable<(TFirst First, TSecond Second)> WithLatestFrom<TFirst, TSecond>(this IQbservable<TFirst> first, IObservable<TSecond> second)
	{
		if (first == null)
		{
			throw new ArgumentNullException("first");
		}
		if (second == null)
		{
			throw new ArgumentNullException("second");
		}
		return first.Provider.CreateQuery<(TFirst, TSecond)>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TFirst), typeof(TSecond)), first.Expression, GetSourceExpression(second)));
	}

	public static IQbservable<(TFirst First, TSecond Second)> Zip<TFirst, TSecond>(this IQbservable<TFirst> first, IEnumerable<TSecond> second)
	{
		if (first == null)
		{
			throw new ArgumentNullException("first");
		}
		if (second == null)
		{
			throw new ArgumentNullException("second");
		}
		return first.Provider.CreateQuery<(TFirst, TSecond)>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TFirst), typeof(TSecond)), first.Expression, GetSourceExpression(second)));
	}

	public static IQbservable<(TFirst First, TSecond Second)> CombineLatest<TFirst, TSecond>(this IQbservable<TFirst> first, IObservable<TSecond> second)
	{
		if (first == null)
		{
			throw new ArgumentNullException("first");
		}
		if (second == null)
		{
			throw new ArgumentNullException("second");
		}
		return first.Provider.CreateQuery<(TFirst, TSecond)>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TFirst), typeof(TSecond)), first.Expression, GetSourceExpression(second)));
	}

	public static IQbservable<(TFirst First, TSecond Second, TThird Third)> CombineLatest<TFirst, TSecond, TThird>(this IQbservable<TFirst> first, IObservable<TSecond> second, IObservable<TThird> third)
	{
		if (first == null)
		{
			throw new ArgumentNullException("first");
		}
		if (second == null)
		{
			throw new ArgumentNullException("second");
		}
		if (third == null)
		{
			throw new ArgumentNullException("third");
		}
		return first.Provider.CreateQuery<(TFirst, TSecond, TThird)>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TFirst), typeof(TSecond), typeof(TThird)), first.Expression, GetSourceExpression(second), GetSourceExpression(third)));
	}

	public static IQbservable<(TFirst First, TSecond Second, TThird Third, TFourth Fourth)> CombineLatest<TFirst, TSecond, TThird, TFourth>(this IQbservable<TFirst> first, IObservable<TSecond> second, IObservable<TThird> third, IObservable<TFourth> fourth)
	{
		if (first == null)
		{
			throw new ArgumentNullException("first");
		}
		if (second == null)
		{
			throw new ArgumentNullException("second");
		}
		if (third == null)
		{
			throw new ArgumentNullException("third");
		}
		if (fourth == null)
		{
			throw new ArgumentNullException("fourth");
		}
		return first.Provider.CreateQuery<(TFirst, TSecond, TThird, TFourth)>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TFirst), typeof(TSecond), typeof(TThird), typeof(TFourth)), first.Expression, GetSourceExpression(second), GetSourceExpression(third), GetSourceExpression(fourth)));
	}

	public static IQbservable<(TFirst First, TSecond Second, TThird Third, TFourth Fourth, TFifth Fifth)> CombineLatest<TFirst, TSecond, TThird, TFourth, TFifth>(this IQbservable<TFirst> first, IObservable<TSecond> second, IObservable<TThird> third, IObservable<TFourth> fourth, IObservable<TFifth> fifth)
	{
		if (first == null)
		{
			throw new ArgumentNullException("first");
		}
		if (second == null)
		{
			throw new ArgumentNullException("second");
		}
		if (third == null)
		{
			throw new ArgumentNullException("third");
		}
		if (fourth == null)
		{
			throw new ArgumentNullException("fourth");
		}
		if (fifth == null)
		{
			throw new ArgumentNullException("fifth");
		}
		return first.Provider.CreateQuery<(TFirst, TSecond, TThird, TFourth, TFifth)>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TFirst), typeof(TSecond), typeof(TThird), typeof(TFourth), typeof(TFifth)), first.Expression, GetSourceExpression(second), GetSourceExpression(third), GetSourceExpression(fourth), GetSourceExpression(fifth)));
	}

	public static IQbservable<(TFirst First, TSecond Second, TThird Third, TFourth Fourth, TFifth Fifth, TSixth Sixth)> CombineLatest<TFirst, TSecond, TThird, TFourth, TFifth, TSixth>(this IQbservable<TFirst> first, IObservable<TSecond> second, IObservable<TThird> third, IObservable<TFourth> fourth, IObservable<TFifth> fifth, IObservable<TSixth> sixth)
	{
		if (first == null)
		{
			throw new ArgumentNullException("first");
		}
		if (second == null)
		{
			throw new ArgumentNullException("second");
		}
		if (third == null)
		{
			throw new ArgumentNullException("third");
		}
		if (fourth == null)
		{
			throw new ArgumentNullException("fourth");
		}
		if (fifth == null)
		{
			throw new ArgumentNullException("fifth");
		}
		if (sixth == null)
		{
			throw new ArgumentNullException("sixth");
		}
		return first.Provider.CreateQuery<(TFirst, TSecond, TThird, TFourth, TFifth, TSixth)>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TFirst), typeof(TSecond), typeof(TThird), typeof(TFourth), typeof(TFifth), typeof(TSixth)), first.Expression, GetSourceExpression(second), GetSourceExpression(third), GetSourceExpression(fourth), GetSourceExpression(fifth), GetSourceExpression(sixth)));
	}

	public static IQbservable<(TFirst First, TSecond Second, TThird Third, TFourth Fourth, TFifth Fifth, TSixth Sixth, TSeventh Seventh)> CombineLatest<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh>(this IQbservable<TFirst> first, IObservable<TSecond> second, IObservable<TThird> third, IObservable<TFourth> fourth, IObservable<TFifth> fifth, IObservable<TSixth> sixth, IObservable<TSeventh> seventh)
	{
		if (first == null)
		{
			throw new ArgumentNullException("first");
		}
		if (second == null)
		{
			throw new ArgumentNullException("second");
		}
		if (third == null)
		{
			throw new ArgumentNullException("third");
		}
		if (fourth == null)
		{
			throw new ArgumentNullException("fourth");
		}
		if (fifth == null)
		{
			throw new ArgumentNullException("fifth");
		}
		if (sixth == null)
		{
			throw new ArgumentNullException("sixth");
		}
		if (seventh == null)
		{
			throw new ArgumentNullException("seventh");
		}
		return first.Provider.CreateQuery<(TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh)>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TFirst), typeof(TSecond), typeof(TThird), typeof(TFourth), typeof(TFifth), typeof(TSixth), typeof(TSeventh)), first.Expression, GetSourceExpression(second), GetSourceExpression(third), GetSourceExpression(fourth), GetSourceExpression(fifth), GetSourceExpression(sixth), GetSourceExpression(seventh)));
	}

	public static IQbservable<(TFirst First, TSecond Second, TThird Third, TFourth Fourth, TFifth Fifth, TSixth Sixth, TSeventh Seventh, TEighth Eighth)> CombineLatest<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TEighth>(this IQbservable<TFirst> first, IObservable<TSecond> second, IObservable<TThird> third, IObservable<TFourth> fourth, IObservable<TFifth> fifth, IObservable<TSixth> sixth, IObservable<TSeventh> seventh, IObservable<TEighth> eighth)
	{
		if (first == null)
		{
			throw new ArgumentNullException("first");
		}
		if (second == null)
		{
			throw new ArgumentNullException("second");
		}
		if (third == null)
		{
			throw new ArgumentNullException("third");
		}
		if (fourth == null)
		{
			throw new ArgumentNullException("fourth");
		}
		if (fifth == null)
		{
			throw new ArgumentNullException("fifth");
		}
		if (sixth == null)
		{
			throw new ArgumentNullException("sixth");
		}
		if (seventh == null)
		{
			throw new ArgumentNullException("seventh");
		}
		if (eighth == null)
		{
			throw new ArgumentNullException("eighth");
		}
		return first.Provider.CreateQuery<(TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TEighth)>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TFirst), typeof(TSecond), typeof(TThird), typeof(TFourth), typeof(TFifth), typeof(TSixth), typeof(TSeventh), typeof(TEighth)), first.Expression, GetSourceExpression(second), GetSourceExpression(third), GetSourceExpression(fourth), GetSourceExpression(fifth), GetSourceExpression(sixth), GetSourceExpression(seventh), GetSourceExpression(eighth)));
	}

	public static IQbservable<(TFirst First, TSecond Second, TThird Third, TFourth Fourth, TFifth Fifth, TSixth Sixth, TSeventh Seventh, TEighth Eighth, TNinth Ninth)> CombineLatest<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TEighth, TNinth>(this IQbservable<TFirst> first, IObservable<TSecond> second, IObservable<TThird> third, IObservable<TFourth> fourth, IObservable<TFifth> fifth, IObservable<TSixth> sixth, IObservable<TSeventh> seventh, IObservable<TEighth> eighth, IObservable<TNinth> ninth)
	{
		if (first == null)
		{
			throw new ArgumentNullException("first");
		}
		if (second == null)
		{
			throw new ArgumentNullException("second");
		}
		if (third == null)
		{
			throw new ArgumentNullException("third");
		}
		if (fourth == null)
		{
			throw new ArgumentNullException("fourth");
		}
		if (fifth == null)
		{
			throw new ArgumentNullException("fifth");
		}
		if (sixth == null)
		{
			throw new ArgumentNullException("sixth");
		}
		if (seventh == null)
		{
			throw new ArgumentNullException("seventh");
		}
		if (eighth == null)
		{
			throw new ArgumentNullException("eighth");
		}
		if (ninth == null)
		{
			throw new ArgumentNullException("ninth");
		}
		return first.Provider.CreateQuery<(TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TEighth, TNinth)>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TFirst), typeof(TSecond), typeof(TThird), typeof(TFourth), typeof(TFifth), typeof(TSixth), typeof(TSeventh), typeof(TEighth), typeof(TNinth)), first.Expression, GetSourceExpression(second), GetSourceExpression(third), GetSourceExpression(fourth), GetSourceExpression(fifth), GetSourceExpression(sixth), GetSourceExpression(seventh), GetSourceExpression(eighth), GetSourceExpression(ninth)));
	}

	public static IQbservable<(TFirst First, TSecond Second, TThird Third, TFourth Fourth, TFifth Fifth, TSixth Sixth, TSeventh Seventh, TEighth Eighth, TNinth Ninth, TTenth Tenth)> CombineLatest<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TEighth, TNinth, TTenth>(this IQbservable<TFirst> first, IObservable<TSecond> second, IObservable<TThird> third, IObservable<TFourth> fourth, IObservable<TFifth> fifth, IObservable<TSixth> sixth, IObservable<TSeventh> seventh, IObservable<TEighth> eighth, IObservable<TNinth> ninth, IObservable<TTenth> tenth)
	{
		if (first == null)
		{
			throw new ArgumentNullException("first");
		}
		if (second == null)
		{
			throw new ArgumentNullException("second");
		}
		if (third == null)
		{
			throw new ArgumentNullException("third");
		}
		if (fourth == null)
		{
			throw new ArgumentNullException("fourth");
		}
		if (fifth == null)
		{
			throw new ArgumentNullException("fifth");
		}
		if (sixth == null)
		{
			throw new ArgumentNullException("sixth");
		}
		if (seventh == null)
		{
			throw new ArgumentNullException("seventh");
		}
		if (eighth == null)
		{
			throw new ArgumentNullException("eighth");
		}
		if (ninth == null)
		{
			throw new ArgumentNullException("ninth");
		}
		if (tenth == null)
		{
			throw new ArgumentNullException("tenth");
		}
		return first.Provider.CreateQuery<(TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TEighth, TNinth, TTenth)>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TFirst), typeof(TSecond), typeof(TThird), typeof(TFourth), typeof(TFifth), typeof(TSixth), typeof(TSeventh), typeof(TEighth), typeof(TNinth), typeof(TTenth)), first.Expression, GetSourceExpression(second), GetSourceExpression(third), GetSourceExpression(fourth), GetSourceExpression(fifth), GetSourceExpression(sixth), GetSourceExpression(seventh), GetSourceExpression(eighth), GetSourceExpression(ninth), GetSourceExpression(tenth)));
	}

	public static IQbservable<(TFirst First, TSecond Second, TThird Third, TFourth Fourth, TFifth Fifth, TSixth Sixth, TSeventh Seventh, TEighth Eighth, TNinth Ninth, TTenth Tenth, TEleventh Eleventh)> CombineLatest<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TEighth, TNinth, TTenth, TEleventh>(this IQbservable<TFirst> first, IObservable<TSecond> second, IObservable<TThird> third, IObservable<TFourth> fourth, IObservable<TFifth> fifth, IObservable<TSixth> sixth, IObservable<TSeventh> seventh, IObservable<TEighth> eighth, IObservable<TNinth> ninth, IObservable<TTenth> tenth, IObservable<TEleventh> eleventh)
	{
		if (first == null)
		{
			throw new ArgumentNullException("first");
		}
		if (second == null)
		{
			throw new ArgumentNullException("second");
		}
		if (third == null)
		{
			throw new ArgumentNullException("third");
		}
		if (fourth == null)
		{
			throw new ArgumentNullException("fourth");
		}
		if (fifth == null)
		{
			throw new ArgumentNullException("fifth");
		}
		if (sixth == null)
		{
			throw new ArgumentNullException("sixth");
		}
		if (seventh == null)
		{
			throw new ArgumentNullException("seventh");
		}
		if (eighth == null)
		{
			throw new ArgumentNullException("eighth");
		}
		if (ninth == null)
		{
			throw new ArgumentNullException("ninth");
		}
		if (tenth == null)
		{
			throw new ArgumentNullException("tenth");
		}
		if (eleventh == null)
		{
			throw new ArgumentNullException("eleventh");
		}
		return first.Provider.CreateQuery<(TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TEighth, TNinth, TTenth, TEleventh)>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TFirst), typeof(TSecond), typeof(TThird), typeof(TFourth), typeof(TFifth), typeof(TSixth), typeof(TSeventh), typeof(TEighth), typeof(TNinth), typeof(TTenth), typeof(TEleventh)), first.Expression, GetSourceExpression(second), GetSourceExpression(third), GetSourceExpression(fourth), GetSourceExpression(fifth), GetSourceExpression(sixth), GetSourceExpression(seventh), GetSourceExpression(eighth), GetSourceExpression(ninth), GetSourceExpression(tenth), GetSourceExpression(eleventh)));
	}

	public static IQbservable<(TFirst First, TSecond Second, TThird Third, TFourth Fourth, TFifth Fifth, TSixth Sixth, TSeventh Seventh, TEighth Eighth, TNinth Ninth, TTenth Tenth, TEleventh Eleventh, TTwelfth Twelfth)> CombineLatest<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TEighth, TNinth, TTenth, TEleventh, TTwelfth>(this IQbservable<TFirst> first, IObservable<TSecond> second, IObservable<TThird> third, IObservable<TFourth> fourth, IObservable<TFifth> fifth, IObservable<TSixth> sixth, IObservable<TSeventh> seventh, IObservable<TEighth> eighth, IObservable<TNinth> ninth, IObservable<TTenth> tenth, IObservable<TEleventh> eleventh, IObservable<TTwelfth> twelfth)
	{
		if (first == null)
		{
			throw new ArgumentNullException("first");
		}
		if (second == null)
		{
			throw new ArgumentNullException("second");
		}
		if (third == null)
		{
			throw new ArgumentNullException("third");
		}
		if (fourth == null)
		{
			throw new ArgumentNullException("fourth");
		}
		if (fifth == null)
		{
			throw new ArgumentNullException("fifth");
		}
		if (sixth == null)
		{
			throw new ArgumentNullException("sixth");
		}
		if (seventh == null)
		{
			throw new ArgumentNullException("seventh");
		}
		if (eighth == null)
		{
			throw new ArgumentNullException("eighth");
		}
		if (ninth == null)
		{
			throw new ArgumentNullException("ninth");
		}
		if (tenth == null)
		{
			throw new ArgumentNullException("tenth");
		}
		if (eleventh == null)
		{
			throw new ArgumentNullException("eleventh");
		}
		if (twelfth == null)
		{
			throw new ArgumentNullException("twelfth");
		}
		return first.Provider.CreateQuery<(TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TEighth, TNinth, TTenth, TEleventh, TTwelfth)>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TFirst), typeof(TSecond), typeof(TThird), typeof(TFourth), typeof(TFifth), typeof(TSixth), typeof(TSeventh), typeof(TEighth), typeof(TNinth), typeof(TTenth), typeof(TEleventh), typeof(TTwelfth)), first.Expression, GetSourceExpression(second), GetSourceExpression(third), GetSourceExpression(fourth), GetSourceExpression(fifth), GetSourceExpression(sixth), GetSourceExpression(seventh), GetSourceExpression(eighth), GetSourceExpression(ninth), GetSourceExpression(tenth), GetSourceExpression(eleventh), GetSourceExpression(twelfth)));
	}

	public static IQbservable<(TFirst First, TSecond Second, TThird Third, TFourth Fourth, TFifth Fifth, TSixth Sixth, TSeventh Seventh, TEighth Eighth, TNinth Ninth, TTenth Tenth, TEleventh Eleventh, TTwelfth Twelfth, TThirteenth Thirteenth)> CombineLatest<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TEighth, TNinth, TTenth, TEleventh, TTwelfth, TThirteenth>(this IQbservable<TFirst> first, IObservable<TSecond> second, IObservable<TThird> third, IObservable<TFourth> fourth, IObservable<TFifth> fifth, IObservable<TSixth> sixth, IObservable<TSeventh> seventh, IObservable<TEighth> eighth, IObservable<TNinth> ninth, IObservable<TTenth> tenth, IObservable<TEleventh> eleventh, IObservable<TTwelfth> twelfth, IObservable<TThirteenth> thirteenth)
	{
		if (first == null)
		{
			throw new ArgumentNullException("first");
		}
		if (second == null)
		{
			throw new ArgumentNullException("second");
		}
		if (third == null)
		{
			throw new ArgumentNullException("third");
		}
		if (fourth == null)
		{
			throw new ArgumentNullException("fourth");
		}
		if (fifth == null)
		{
			throw new ArgumentNullException("fifth");
		}
		if (sixth == null)
		{
			throw new ArgumentNullException("sixth");
		}
		if (seventh == null)
		{
			throw new ArgumentNullException("seventh");
		}
		if (eighth == null)
		{
			throw new ArgumentNullException("eighth");
		}
		if (ninth == null)
		{
			throw new ArgumentNullException("ninth");
		}
		if (tenth == null)
		{
			throw new ArgumentNullException("tenth");
		}
		if (eleventh == null)
		{
			throw new ArgumentNullException("eleventh");
		}
		if (twelfth == null)
		{
			throw new ArgumentNullException("twelfth");
		}
		if (thirteenth == null)
		{
			throw new ArgumentNullException("thirteenth");
		}
		return first.Provider.CreateQuery<(TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TEighth, TNinth, TTenth, TEleventh, TTwelfth, TThirteenth)>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TFirst), typeof(TSecond), typeof(TThird), typeof(TFourth), typeof(TFifth), typeof(TSixth), typeof(TSeventh), typeof(TEighth), typeof(TNinth), typeof(TTenth), typeof(TEleventh), typeof(TTwelfth), typeof(TThirteenth)), first.Expression, GetSourceExpression(second), GetSourceExpression(third), GetSourceExpression(fourth), GetSourceExpression(fifth), GetSourceExpression(sixth), GetSourceExpression(seventh), GetSourceExpression(eighth), GetSourceExpression(ninth), GetSourceExpression(tenth), GetSourceExpression(eleventh), GetSourceExpression(twelfth), GetSourceExpression(thirteenth)));
	}

	public static IQbservable<(TFirst First, TSecond Second, TThird Third, TFourth Fourth, TFifth Fifth, TSixth Sixth, TSeventh Seventh, TEighth Eighth, TNinth Ninth, TTenth Tenth, TEleventh Eleventh, TTwelfth Twelfth, TThirteenth Thirteenth, TFourteenth Fourteenth)> CombineLatest<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TEighth, TNinth, TTenth, TEleventh, TTwelfth, TThirteenth, TFourteenth>(this IQbservable<TFirst> first, IObservable<TSecond> second, IObservable<TThird> third, IObservable<TFourth> fourth, IObservable<TFifth> fifth, IObservable<TSixth> sixth, IObservable<TSeventh> seventh, IObservable<TEighth> eighth, IObservable<TNinth> ninth, IObservable<TTenth> tenth, IObservable<TEleventh> eleventh, IObservable<TTwelfth> twelfth, IObservable<TThirteenth> thirteenth, IObservable<TFourteenth> fourteenth)
	{
		if (first == null)
		{
			throw new ArgumentNullException("first");
		}
		if (second == null)
		{
			throw new ArgumentNullException("second");
		}
		if (third == null)
		{
			throw new ArgumentNullException("third");
		}
		if (fourth == null)
		{
			throw new ArgumentNullException("fourth");
		}
		if (fifth == null)
		{
			throw new ArgumentNullException("fifth");
		}
		if (sixth == null)
		{
			throw new ArgumentNullException("sixth");
		}
		if (seventh == null)
		{
			throw new ArgumentNullException("seventh");
		}
		if (eighth == null)
		{
			throw new ArgumentNullException("eighth");
		}
		if (ninth == null)
		{
			throw new ArgumentNullException("ninth");
		}
		if (tenth == null)
		{
			throw new ArgumentNullException("tenth");
		}
		if (eleventh == null)
		{
			throw new ArgumentNullException("eleventh");
		}
		if (twelfth == null)
		{
			throw new ArgumentNullException("twelfth");
		}
		if (thirteenth == null)
		{
			throw new ArgumentNullException("thirteenth");
		}
		if (fourteenth == null)
		{
			throw new ArgumentNullException("fourteenth");
		}
		return first.Provider.CreateQuery<(TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TEighth, TNinth, TTenth, TEleventh, TTwelfth, TThirteenth, TFourteenth)>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TFirst), typeof(TSecond), typeof(TThird), typeof(TFourth), typeof(TFifth), typeof(TSixth), typeof(TSeventh), typeof(TEighth), typeof(TNinth), typeof(TTenth), typeof(TEleventh), typeof(TTwelfth), typeof(TThirteenth), typeof(TFourteenth)), first.Expression, GetSourceExpression(second), GetSourceExpression(third), GetSourceExpression(fourth), GetSourceExpression(fifth), GetSourceExpression(sixth), GetSourceExpression(seventh), GetSourceExpression(eighth), GetSourceExpression(ninth), GetSourceExpression(tenth), GetSourceExpression(eleventh), GetSourceExpression(twelfth), GetSourceExpression(thirteenth), GetSourceExpression(fourteenth)));
	}

	public static IQbservable<(TFirst First, TSecond Second, TThird Third, TFourth Fourth, TFifth Fifth, TSixth Sixth, TSeventh Seventh, TEighth Eighth, TNinth Ninth, TTenth Tenth, TEleventh Eleventh, TTwelfth Twelfth, TThirteenth Thirteenth, TFourteenth Fourteenth, TFifteenth Fifteenth)> CombineLatest<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TEighth, TNinth, TTenth, TEleventh, TTwelfth, TThirteenth, TFourteenth, TFifteenth>(this IQbservable<TFirst> first, IObservable<TSecond> second, IObservable<TThird> third, IObservable<TFourth> fourth, IObservable<TFifth> fifth, IObservable<TSixth> sixth, IObservable<TSeventh> seventh, IObservable<TEighth> eighth, IObservable<TNinth> ninth, IObservable<TTenth> tenth, IObservable<TEleventh> eleventh, IObservable<TTwelfth> twelfth, IObservable<TThirteenth> thirteenth, IObservable<TFourteenth> fourteenth, IObservable<TFifteenth> fifteenth)
	{
		if (first == null)
		{
			throw new ArgumentNullException("first");
		}
		if (second == null)
		{
			throw new ArgumentNullException("second");
		}
		if (third == null)
		{
			throw new ArgumentNullException("third");
		}
		if (fourth == null)
		{
			throw new ArgumentNullException("fourth");
		}
		if (fifth == null)
		{
			throw new ArgumentNullException("fifth");
		}
		if (sixth == null)
		{
			throw new ArgumentNullException("sixth");
		}
		if (seventh == null)
		{
			throw new ArgumentNullException("seventh");
		}
		if (eighth == null)
		{
			throw new ArgumentNullException("eighth");
		}
		if (ninth == null)
		{
			throw new ArgumentNullException("ninth");
		}
		if (tenth == null)
		{
			throw new ArgumentNullException("tenth");
		}
		if (eleventh == null)
		{
			throw new ArgumentNullException("eleventh");
		}
		if (twelfth == null)
		{
			throw new ArgumentNullException("twelfth");
		}
		if (thirteenth == null)
		{
			throw new ArgumentNullException("thirteenth");
		}
		if (fourteenth == null)
		{
			throw new ArgumentNullException("fourteenth");
		}
		if (fifteenth == null)
		{
			throw new ArgumentNullException("fifteenth");
		}
		return first.Provider.CreateQuery<(TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TEighth, TNinth, TTenth, TEleventh, TTwelfth, TThirteenth, TFourteenth, TFifteenth)>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TFirst), typeof(TSecond), typeof(TThird), typeof(TFourth), typeof(TFifth), typeof(TSixth), typeof(TSeventh), typeof(TEighth), typeof(TNinth), typeof(TTenth), typeof(TEleventh), typeof(TTwelfth), typeof(TThirteenth), typeof(TFourteenth), typeof(TFifteenth)), first.Expression, GetSourceExpression(second), GetSourceExpression(third), GetSourceExpression(fourth), GetSourceExpression(fifth), GetSourceExpression(sixth), GetSourceExpression(seventh), GetSourceExpression(eighth), GetSourceExpression(ninth), GetSourceExpression(tenth), GetSourceExpression(eleventh), GetSourceExpression(twelfth), GetSourceExpression(thirteenth), GetSourceExpression(fourteenth), GetSourceExpression(fifteenth)));
	}

	public static IQbservable<(TFirst First, TSecond Second, TThird Third, TFourth Fourth, TFifth Fifth, TSixth Sixth, TSeventh Seventh, TEighth Eighth, TNinth Ninth, TTenth Tenth, TEleventh Eleventh, TTwelfth Twelfth, TThirteenth Thirteenth, TFourteenth Fourteenth, TFifteenth Fifteenth, TSixteenth Sixteenth)> CombineLatest<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TEighth, TNinth, TTenth, TEleventh, TTwelfth, TThirteenth, TFourteenth, TFifteenth, TSixteenth>(this IQbservable<TFirst> first, IObservable<TSecond> second, IObservable<TThird> third, IObservable<TFourth> fourth, IObservable<TFifth> fifth, IObservable<TSixth> sixth, IObservable<TSeventh> seventh, IObservable<TEighth> eighth, IObservable<TNinth> ninth, IObservable<TTenth> tenth, IObservable<TEleventh> eleventh, IObservable<TTwelfth> twelfth, IObservable<TThirteenth> thirteenth, IObservable<TFourteenth> fourteenth, IObservable<TFifteenth> fifteenth, IObservable<TSixteenth> sixteenth)
	{
		if (first == null)
		{
			throw new ArgumentNullException("first");
		}
		if (second == null)
		{
			throw new ArgumentNullException("second");
		}
		if (third == null)
		{
			throw new ArgumentNullException("third");
		}
		if (fourth == null)
		{
			throw new ArgumentNullException("fourth");
		}
		if (fifth == null)
		{
			throw new ArgumentNullException("fifth");
		}
		if (sixth == null)
		{
			throw new ArgumentNullException("sixth");
		}
		if (seventh == null)
		{
			throw new ArgumentNullException("seventh");
		}
		if (eighth == null)
		{
			throw new ArgumentNullException("eighth");
		}
		if (ninth == null)
		{
			throw new ArgumentNullException("ninth");
		}
		if (tenth == null)
		{
			throw new ArgumentNullException("tenth");
		}
		if (eleventh == null)
		{
			throw new ArgumentNullException("eleventh");
		}
		if (twelfth == null)
		{
			throw new ArgumentNullException("twelfth");
		}
		if (thirteenth == null)
		{
			throw new ArgumentNullException("thirteenth");
		}
		if (fourteenth == null)
		{
			throw new ArgumentNullException("fourteenth");
		}
		if (fifteenth == null)
		{
			throw new ArgumentNullException("fifteenth");
		}
		if (sixteenth == null)
		{
			throw new ArgumentNullException("sixteenth");
		}
		return first.Provider.CreateQuery<(TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TEighth, TNinth, TTenth, TEleventh, TTwelfth, TThirteenth, TFourteenth, TFifteenth, TSixteenth)>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TFirst), typeof(TSecond), typeof(TThird), typeof(TFourth), typeof(TFifth), typeof(TSixth), typeof(TSeventh), typeof(TEighth), typeof(TNinth), typeof(TTenth), typeof(TEleventh), typeof(TTwelfth), typeof(TThirteenth), typeof(TFourteenth), typeof(TFifteenth), typeof(TSixteenth)), first.Expression, GetSourceExpression(second), GetSourceExpression(third), GetSourceExpression(fourth), GetSourceExpression(fifth), GetSourceExpression(sixth), GetSourceExpression(seventh), GetSourceExpression(eighth), GetSourceExpression(ninth), GetSourceExpression(tenth), GetSourceExpression(eleventh), GetSourceExpression(twelfth), GetSourceExpression(thirteenth), GetSourceExpression(fourteenth), GetSourceExpression(fifteenth), GetSourceExpression(sixteenth)));
	}

	public static IQbservable<(TFirst First, TSecond Second)> Zip<TFirst, TSecond>(this IQbservable<TFirst> first, IObservable<TSecond> second)
	{
		if (first == null)
		{
			throw new ArgumentNullException("first");
		}
		if (second == null)
		{
			throw new ArgumentNullException("second");
		}
		return first.Provider.CreateQuery<(TFirst, TSecond)>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TFirst), typeof(TSecond)), first.Expression, GetSourceExpression(second)));
	}

	public static IQbservable<(TFirst First, TSecond Second, TThird Third)> Zip<TFirst, TSecond, TThird>(this IQbservable<TFirst> first, IObservable<TSecond> second, IObservable<TThird> third)
	{
		if (first == null)
		{
			throw new ArgumentNullException("first");
		}
		if (second == null)
		{
			throw new ArgumentNullException("second");
		}
		if (third == null)
		{
			throw new ArgumentNullException("third");
		}
		return first.Provider.CreateQuery<(TFirst, TSecond, TThird)>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TFirst), typeof(TSecond), typeof(TThird)), first.Expression, GetSourceExpression(second), GetSourceExpression(third)));
	}

	public static IQbservable<(TFirst First, TSecond Second, TThird Third, TFourth Fourth)> Zip<TFirst, TSecond, TThird, TFourth>(this IQbservable<TFirst> first, IObservable<TSecond> second, IObservable<TThird> third, IObservable<TFourth> fourth)
	{
		if (first == null)
		{
			throw new ArgumentNullException("first");
		}
		if (second == null)
		{
			throw new ArgumentNullException("second");
		}
		if (third == null)
		{
			throw new ArgumentNullException("third");
		}
		if (fourth == null)
		{
			throw new ArgumentNullException("fourth");
		}
		return first.Provider.CreateQuery<(TFirst, TSecond, TThird, TFourth)>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TFirst), typeof(TSecond), typeof(TThird), typeof(TFourth)), first.Expression, GetSourceExpression(second), GetSourceExpression(third), GetSourceExpression(fourth)));
	}

	public static IQbservable<(TFirst First, TSecond Second, TThird Third, TFourth Fourth, TFifth Fifth)> Zip<TFirst, TSecond, TThird, TFourth, TFifth>(this IQbservable<TFirst> first, IObservable<TSecond> second, IObservable<TThird> third, IObservable<TFourth> fourth, IObservable<TFifth> fifth)
	{
		if (first == null)
		{
			throw new ArgumentNullException("first");
		}
		if (second == null)
		{
			throw new ArgumentNullException("second");
		}
		if (third == null)
		{
			throw new ArgumentNullException("third");
		}
		if (fourth == null)
		{
			throw new ArgumentNullException("fourth");
		}
		if (fifth == null)
		{
			throw new ArgumentNullException("fifth");
		}
		return first.Provider.CreateQuery<(TFirst, TSecond, TThird, TFourth, TFifth)>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TFirst), typeof(TSecond), typeof(TThird), typeof(TFourth), typeof(TFifth)), first.Expression, GetSourceExpression(second), GetSourceExpression(third), GetSourceExpression(fourth), GetSourceExpression(fifth)));
	}

	public static IQbservable<(TFirst First, TSecond Second, TThird Third, TFourth Fourth, TFifth Fifth, TSixth Sixth)> Zip<TFirst, TSecond, TThird, TFourth, TFifth, TSixth>(this IQbservable<TFirst> first, IObservable<TSecond> second, IObservable<TThird> third, IObservable<TFourth> fourth, IObservable<TFifth> fifth, IObservable<TSixth> sixth)
	{
		if (first == null)
		{
			throw new ArgumentNullException("first");
		}
		if (second == null)
		{
			throw new ArgumentNullException("second");
		}
		if (third == null)
		{
			throw new ArgumentNullException("third");
		}
		if (fourth == null)
		{
			throw new ArgumentNullException("fourth");
		}
		if (fifth == null)
		{
			throw new ArgumentNullException("fifth");
		}
		if (sixth == null)
		{
			throw new ArgumentNullException("sixth");
		}
		return first.Provider.CreateQuery<(TFirst, TSecond, TThird, TFourth, TFifth, TSixth)>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TFirst), typeof(TSecond), typeof(TThird), typeof(TFourth), typeof(TFifth), typeof(TSixth)), first.Expression, GetSourceExpression(second), GetSourceExpression(third), GetSourceExpression(fourth), GetSourceExpression(fifth), GetSourceExpression(sixth)));
	}

	public static IQbservable<(TFirst First, TSecond Second, TThird Third, TFourth Fourth, TFifth Fifth, TSixth Sixth, TSeventh Seventh)> Zip<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh>(this IQbservable<TFirst> first, IObservable<TSecond> second, IObservable<TThird> third, IObservable<TFourth> fourth, IObservable<TFifth> fifth, IObservable<TSixth> sixth, IObservable<TSeventh> seventh)
	{
		if (first == null)
		{
			throw new ArgumentNullException("first");
		}
		if (second == null)
		{
			throw new ArgumentNullException("second");
		}
		if (third == null)
		{
			throw new ArgumentNullException("third");
		}
		if (fourth == null)
		{
			throw new ArgumentNullException("fourth");
		}
		if (fifth == null)
		{
			throw new ArgumentNullException("fifth");
		}
		if (sixth == null)
		{
			throw new ArgumentNullException("sixth");
		}
		if (seventh == null)
		{
			throw new ArgumentNullException("seventh");
		}
		return first.Provider.CreateQuery<(TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh)>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TFirst), typeof(TSecond), typeof(TThird), typeof(TFourth), typeof(TFifth), typeof(TSixth), typeof(TSeventh)), first.Expression, GetSourceExpression(second), GetSourceExpression(third), GetSourceExpression(fourth), GetSourceExpression(fifth), GetSourceExpression(sixth), GetSourceExpression(seventh)));
	}

	public static IQbservable<(TFirst First, TSecond Second, TThird Third, TFourth Fourth, TFifth Fifth, TSixth Sixth, TSeventh Seventh, TEighth Eighth)> Zip<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TEighth>(this IQbservable<TFirst> first, IObservable<TSecond> second, IObservable<TThird> third, IObservable<TFourth> fourth, IObservable<TFifth> fifth, IObservable<TSixth> sixth, IObservable<TSeventh> seventh, IObservable<TEighth> eighth)
	{
		if (first == null)
		{
			throw new ArgumentNullException("first");
		}
		if (second == null)
		{
			throw new ArgumentNullException("second");
		}
		if (third == null)
		{
			throw new ArgumentNullException("third");
		}
		if (fourth == null)
		{
			throw new ArgumentNullException("fourth");
		}
		if (fifth == null)
		{
			throw new ArgumentNullException("fifth");
		}
		if (sixth == null)
		{
			throw new ArgumentNullException("sixth");
		}
		if (seventh == null)
		{
			throw new ArgumentNullException("seventh");
		}
		if (eighth == null)
		{
			throw new ArgumentNullException("eighth");
		}
		return first.Provider.CreateQuery<(TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TEighth)>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TFirst), typeof(TSecond), typeof(TThird), typeof(TFourth), typeof(TFifth), typeof(TSixth), typeof(TSeventh), typeof(TEighth)), first.Expression, GetSourceExpression(second), GetSourceExpression(third), GetSourceExpression(fourth), GetSourceExpression(fifth), GetSourceExpression(sixth), GetSourceExpression(seventh), GetSourceExpression(eighth)));
	}

	public static IQbservable<(TFirst First, TSecond Second, TThird Third, TFourth Fourth, TFifth Fifth, TSixth Sixth, TSeventh Seventh, TEighth Eighth, TNinth Ninth)> Zip<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TEighth, TNinth>(this IQbservable<TFirst> first, IObservable<TSecond> second, IObservable<TThird> third, IObservable<TFourth> fourth, IObservable<TFifth> fifth, IObservable<TSixth> sixth, IObservable<TSeventh> seventh, IObservable<TEighth> eighth, IObservable<TNinth> ninth)
	{
		if (first == null)
		{
			throw new ArgumentNullException("first");
		}
		if (second == null)
		{
			throw new ArgumentNullException("second");
		}
		if (third == null)
		{
			throw new ArgumentNullException("third");
		}
		if (fourth == null)
		{
			throw new ArgumentNullException("fourth");
		}
		if (fifth == null)
		{
			throw new ArgumentNullException("fifth");
		}
		if (sixth == null)
		{
			throw new ArgumentNullException("sixth");
		}
		if (seventh == null)
		{
			throw new ArgumentNullException("seventh");
		}
		if (eighth == null)
		{
			throw new ArgumentNullException("eighth");
		}
		if (ninth == null)
		{
			throw new ArgumentNullException("ninth");
		}
		return first.Provider.CreateQuery<(TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TEighth, TNinth)>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TFirst), typeof(TSecond), typeof(TThird), typeof(TFourth), typeof(TFifth), typeof(TSixth), typeof(TSeventh), typeof(TEighth), typeof(TNinth)), first.Expression, GetSourceExpression(second), GetSourceExpression(third), GetSourceExpression(fourth), GetSourceExpression(fifth), GetSourceExpression(sixth), GetSourceExpression(seventh), GetSourceExpression(eighth), GetSourceExpression(ninth)));
	}

	public static IQbservable<(TFirst First, TSecond Second, TThird Third, TFourth Fourth, TFifth Fifth, TSixth Sixth, TSeventh Seventh, TEighth Eighth, TNinth Ninth, TTenth Tenth)> Zip<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TEighth, TNinth, TTenth>(this IQbservable<TFirst> first, IObservable<TSecond> second, IObservable<TThird> third, IObservable<TFourth> fourth, IObservable<TFifth> fifth, IObservable<TSixth> sixth, IObservable<TSeventh> seventh, IObservable<TEighth> eighth, IObservable<TNinth> ninth, IObservable<TTenth> tenth)
	{
		if (first == null)
		{
			throw new ArgumentNullException("first");
		}
		if (second == null)
		{
			throw new ArgumentNullException("second");
		}
		if (third == null)
		{
			throw new ArgumentNullException("third");
		}
		if (fourth == null)
		{
			throw new ArgumentNullException("fourth");
		}
		if (fifth == null)
		{
			throw new ArgumentNullException("fifth");
		}
		if (sixth == null)
		{
			throw new ArgumentNullException("sixth");
		}
		if (seventh == null)
		{
			throw new ArgumentNullException("seventh");
		}
		if (eighth == null)
		{
			throw new ArgumentNullException("eighth");
		}
		if (ninth == null)
		{
			throw new ArgumentNullException("ninth");
		}
		if (tenth == null)
		{
			throw new ArgumentNullException("tenth");
		}
		return first.Provider.CreateQuery<(TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TEighth, TNinth, TTenth)>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TFirst), typeof(TSecond), typeof(TThird), typeof(TFourth), typeof(TFifth), typeof(TSixth), typeof(TSeventh), typeof(TEighth), typeof(TNinth), typeof(TTenth)), first.Expression, GetSourceExpression(second), GetSourceExpression(third), GetSourceExpression(fourth), GetSourceExpression(fifth), GetSourceExpression(sixth), GetSourceExpression(seventh), GetSourceExpression(eighth), GetSourceExpression(ninth), GetSourceExpression(tenth)));
	}

	public static IQbservable<(TFirst First, TSecond Second, TThird Third, TFourth Fourth, TFifth Fifth, TSixth Sixth, TSeventh Seventh, TEighth Eighth, TNinth Ninth, TTenth Tenth, TEleventh Eleventh)> Zip<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TEighth, TNinth, TTenth, TEleventh>(this IQbservable<TFirst> first, IObservable<TSecond> second, IObservable<TThird> third, IObservable<TFourth> fourth, IObservable<TFifth> fifth, IObservable<TSixth> sixth, IObservable<TSeventh> seventh, IObservable<TEighth> eighth, IObservable<TNinth> ninth, IObservable<TTenth> tenth, IObservable<TEleventh> eleventh)
	{
		if (first == null)
		{
			throw new ArgumentNullException("first");
		}
		if (second == null)
		{
			throw new ArgumentNullException("second");
		}
		if (third == null)
		{
			throw new ArgumentNullException("third");
		}
		if (fourth == null)
		{
			throw new ArgumentNullException("fourth");
		}
		if (fifth == null)
		{
			throw new ArgumentNullException("fifth");
		}
		if (sixth == null)
		{
			throw new ArgumentNullException("sixth");
		}
		if (seventh == null)
		{
			throw new ArgumentNullException("seventh");
		}
		if (eighth == null)
		{
			throw new ArgumentNullException("eighth");
		}
		if (ninth == null)
		{
			throw new ArgumentNullException("ninth");
		}
		if (tenth == null)
		{
			throw new ArgumentNullException("tenth");
		}
		if (eleventh == null)
		{
			throw new ArgumentNullException("eleventh");
		}
		return first.Provider.CreateQuery<(TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TEighth, TNinth, TTenth, TEleventh)>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TFirst), typeof(TSecond), typeof(TThird), typeof(TFourth), typeof(TFifth), typeof(TSixth), typeof(TSeventh), typeof(TEighth), typeof(TNinth), typeof(TTenth), typeof(TEleventh)), first.Expression, GetSourceExpression(second), GetSourceExpression(third), GetSourceExpression(fourth), GetSourceExpression(fifth), GetSourceExpression(sixth), GetSourceExpression(seventh), GetSourceExpression(eighth), GetSourceExpression(ninth), GetSourceExpression(tenth), GetSourceExpression(eleventh)));
	}

	public static IQbservable<(TFirst First, TSecond Second, TThird Third, TFourth Fourth, TFifth Fifth, TSixth Sixth, TSeventh Seventh, TEighth Eighth, TNinth Ninth, TTenth Tenth, TEleventh Eleventh, TTwelfth Twelfth)> Zip<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TEighth, TNinth, TTenth, TEleventh, TTwelfth>(this IQbservable<TFirst> first, IObservable<TSecond> second, IObservable<TThird> third, IObservable<TFourth> fourth, IObservable<TFifth> fifth, IObservable<TSixth> sixth, IObservable<TSeventh> seventh, IObservable<TEighth> eighth, IObservable<TNinth> ninth, IObservable<TTenth> tenth, IObservable<TEleventh> eleventh, IObservable<TTwelfth> twelfth)
	{
		if (first == null)
		{
			throw new ArgumentNullException("first");
		}
		if (second == null)
		{
			throw new ArgumentNullException("second");
		}
		if (third == null)
		{
			throw new ArgumentNullException("third");
		}
		if (fourth == null)
		{
			throw new ArgumentNullException("fourth");
		}
		if (fifth == null)
		{
			throw new ArgumentNullException("fifth");
		}
		if (sixth == null)
		{
			throw new ArgumentNullException("sixth");
		}
		if (seventh == null)
		{
			throw new ArgumentNullException("seventh");
		}
		if (eighth == null)
		{
			throw new ArgumentNullException("eighth");
		}
		if (ninth == null)
		{
			throw new ArgumentNullException("ninth");
		}
		if (tenth == null)
		{
			throw new ArgumentNullException("tenth");
		}
		if (eleventh == null)
		{
			throw new ArgumentNullException("eleventh");
		}
		if (twelfth == null)
		{
			throw new ArgumentNullException("twelfth");
		}
		return first.Provider.CreateQuery<(TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TEighth, TNinth, TTenth, TEleventh, TTwelfth)>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TFirst), typeof(TSecond), typeof(TThird), typeof(TFourth), typeof(TFifth), typeof(TSixth), typeof(TSeventh), typeof(TEighth), typeof(TNinth), typeof(TTenth), typeof(TEleventh), typeof(TTwelfth)), first.Expression, GetSourceExpression(second), GetSourceExpression(third), GetSourceExpression(fourth), GetSourceExpression(fifth), GetSourceExpression(sixth), GetSourceExpression(seventh), GetSourceExpression(eighth), GetSourceExpression(ninth), GetSourceExpression(tenth), GetSourceExpression(eleventh), GetSourceExpression(twelfth)));
	}

	public static IQbservable<(TFirst First, TSecond Second, TThird Third, TFourth Fourth, TFifth Fifth, TSixth Sixth, TSeventh Seventh, TEighth Eighth, TNinth Ninth, TTenth Tenth, TEleventh Eleventh, TTwelfth Twelfth, TThirteenth Thirteenth)> Zip<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TEighth, TNinth, TTenth, TEleventh, TTwelfth, TThirteenth>(this IQbservable<TFirst> first, IObservable<TSecond> second, IObservable<TThird> third, IObservable<TFourth> fourth, IObservable<TFifth> fifth, IObservable<TSixth> sixth, IObservable<TSeventh> seventh, IObservable<TEighth> eighth, IObservable<TNinth> ninth, IObservable<TTenth> tenth, IObservable<TEleventh> eleventh, IObservable<TTwelfth> twelfth, IObservable<TThirteenth> thirteenth)
	{
		if (first == null)
		{
			throw new ArgumentNullException("first");
		}
		if (second == null)
		{
			throw new ArgumentNullException("second");
		}
		if (third == null)
		{
			throw new ArgumentNullException("third");
		}
		if (fourth == null)
		{
			throw new ArgumentNullException("fourth");
		}
		if (fifth == null)
		{
			throw new ArgumentNullException("fifth");
		}
		if (sixth == null)
		{
			throw new ArgumentNullException("sixth");
		}
		if (seventh == null)
		{
			throw new ArgumentNullException("seventh");
		}
		if (eighth == null)
		{
			throw new ArgumentNullException("eighth");
		}
		if (ninth == null)
		{
			throw new ArgumentNullException("ninth");
		}
		if (tenth == null)
		{
			throw new ArgumentNullException("tenth");
		}
		if (eleventh == null)
		{
			throw new ArgumentNullException("eleventh");
		}
		if (twelfth == null)
		{
			throw new ArgumentNullException("twelfth");
		}
		if (thirteenth == null)
		{
			throw new ArgumentNullException("thirteenth");
		}
		return first.Provider.CreateQuery<(TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TEighth, TNinth, TTenth, TEleventh, TTwelfth, TThirteenth)>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TFirst), typeof(TSecond), typeof(TThird), typeof(TFourth), typeof(TFifth), typeof(TSixth), typeof(TSeventh), typeof(TEighth), typeof(TNinth), typeof(TTenth), typeof(TEleventh), typeof(TTwelfth), typeof(TThirteenth)), first.Expression, GetSourceExpression(second), GetSourceExpression(third), GetSourceExpression(fourth), GetSourceExpression(fifth), GetSourceExpression(sixth), GetSourceExpression(seventh), GetSourceExpression(eighth), GetSourceExpression(ninth), GetSourceExpression(tenth), GetSourceExpression(eleventh), GetSourceExpression(twelfth), GetSourceExpression(thirteenth)));
	}

	public static IQbservable<(TFirst First, TSecond Second, TThird Third, TFourth Fourth, TFifth Fifth, TSixth Sixth, TSeventh Seventh, TEighth Eighth, TNinth Ninth, TTenth Tenth, TEleventh Eleventh, TTwelfth Twelfth, TThirteenth Thirteenth, TFourteenth Fourteenth)> Zip<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TEighth, TNinth, TTenth, TEleventh, TTwelfth, TThirteenth, TFourteenth>(this IQbservable<TFirst> first, IObservable<TSecond> second, IObservable<TThird> third, IObservable<TFourth> fourth, IObservable<TFifth> fifth, IObservable<TSixth> sixth, IObservable<TSeventh> seventh, IObservable<TEighth> eighth, IObservable<TNinth> ninth, IObservable<TTenth> tenth, IObservable<TEleventh> eleventh, IObservable<TTwelfth> twelfth, IObservable<TThirteenth> thirteenth, IObservable<TFourteenth> fourteenth)
	{
		if (first == null)
		{
			throw new ArgumentNullException("first");
		}
		if (second == null)
		{
			throw new ArgumentNullException("second");
		}
		if (third == null)
		{
			throw new ArgumentNullException("third");
		}
		if (fourth == null)
		{
			throw new ArgumentNullException("fourth");
		}
		if (fifth == null)
		{
			throw new ArgumentNullException("fifth");
		}
		if (sixth == null)
		{
			throw new ArgumentNullException("sixth");
		}
		if (seventh == null)
		{
			throw new ArgumentNullException("seventh");
		}
		if (eighth == null)
		{
			throw new ArgumentNullException("eighth");
		}
		if (ninth == null)
		{
			throw new ArgumentNullException("ninth");
		}
		if (tenth == null)
		{
			throw new ArgumentNullException("tenth");
		}
		if (eleventh == null)
		{
			throw new ArgumentNullException("eleventh");
		}
		if (twelfth == null)
		{
			throw new ArgumentNullException("twelfth");
		}
		if (thirteenth == null)
		{
			throw new ArgumentNullException("thirteenth");
		}
		if (fourteenth == null)
		{
			throw new ArgumentNullException("fourteenth");
		}
		return first.Provider.CreateQuery<(TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TEighth, TNinth, TTenth, TEleventh, TTwelfth, TThirteenth, TFourteenth)>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TFirst), typeof(TSecond), typeof(TThird), typeof(TFourth), typeof(TFifth), typeof(TSixth), typeof(TSeventh), typeof(TEighth), typeof(TNinth), typeof(TTenth), typeof(TEleventh), typeof(TTwelfth), typeof(TThirteenth), typeof(TFourteenth)), first.Expression, GetSourceExpression(second), GetSourceExpression(third), GetSourceExpression(fourth), GetSourceExpression(fifth), GetSourceExpression(sixth), GetSourceExpression(seventh), GetSourceExpression(eighth), GetSourceExpression(ninth), GetSourceExpression(tenth), GetSourceExpression(eleventh), GetSourceExpression(twelfth), GetSourceExpression(thirteenth), GetSourceExpression(fourteenth)));
	}

	public static IQbservable<(TFirst First, TSecond Second, TThird Third, TFourth Fourth, TFifth Fifth, TSixth Sixth, TSeventh Seventh, TEighth Eighth, TNinth Ninth, TTenth Tenth, TEleventh Eleventh, TTwelfth Twelfth, TThirteenth Thirteenth, TFourteenth Fourteenth, TFifteenth Fifteenth)> Zip<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TEighth, TNinth, TTenth, TEleventh, TTwelfth, TThirteenth, TFourteenth, TFifteenth>(this IQbservable<TFirst> first, IObservable<TSecond> second, IObservable<TThird> third, IObservable<TFourth> fourth, IObservable<TFifth> fifth, IObservable<TSixth> sixth, IObservable<TSeventh> seventh, IObservable<TEighth> eighth, IObservable<TNinth> ninth, IObservable<TTenth> tenth, IObservable<TEleventh> eleventh, IObservable<TTwelfth> twelfth, IObservable<TThirteenth> thirteenth, IObservable<TFourteenth> fourteenth, IObservable<TFifteenth> fifteenth)
	{
		if (first == null)
		{
			throw new ArgumentNullException("first");
		}
		if (second == null)
		{
			throw new ArgumentNullException("second");
		}
		if (third == null)
		{
			throw new ArgumentNullException("third");
		}
		if (fourth == null)
		{
			throw new ArgumentNullException("fourth");
		}
		if (fifth == null)
		{
			throw new ArgumentNullException("fifth");
		}
		if (sixth == null)
		{
			throw new ArgumentNullException("sixth");
		}
		if (seventh == null)
		{
			throw new ArgumentNullException("seventh");
		}
		if (eighth == null)
		{
			throw new ArgumentNullException("eighth");
		}
		if (ninth == null)
		{
			throw new ArgumentNullException("ninth");
		}
		if (tenth == null)
		{
			throw new ArgumentNullException("tenth");
		}
		if (eleventh == null)
		{
			throw new ArgumentNullException("eleventh");
		}
		if (twelfth == null)
		{
			throw new ArgumentNullException("twelfth");
		}
		if (thirteenth == null)
		{
			throw new ArgumentNullException("thirteenth");
		}
		if (fourteenth == null)
		{
			throw new ArgumentNullException("fourteenth");
		}
		if (fifteenth == null)
		{
			throw new ArgumentNullException("fifteenth");
		}
		return first.Provider.CreateQuery<(TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TEighth, TNinth, TTenth, TEleventh, TTwelfth, TThirteenth, TFourteenth, TFifteenth)>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TFirst), typeof(TSecond), typeof(TThird), typeof(TFourth), typeof(TFifth), typeof(TSixth), typeof(TSeventh), typeof(TEighth), typeof(TNinth), typeof(TTenth), typeof(TEleventh), typeof(TTwelfth), typeof(TThirteenth), typeof(TFourteenth), typeof(TFifteenth)), first.Expression, GetSourceExpression(second), GetSourceExpression(third), GetSourceExpression(fourth), GetSourceExpression(fifth), GetSourceExpression(sixth), GetSourceExpression(seventh), GetSourceExpression(eighth), GetSourceExpression(ninth), GetSourceExpression(tenth), GetSourceExpression(eleventh), GetSourceExpression(twelfth), GetSourceExpression(thirteenth), GetSourceExpression(fourteenth), GetSourceExpression(fifteenth)));
	}

	public static IQbservable<(TFirst First, TSecond Second, TThird Third, TFourth Fourth, TFifth Fifth, TSixth Sixth, TSeventh Seventh, TEighth Eighth, TNinth Ninth, TTenth Tenth, TEleventh Eleventh, TTwelfth Twelfth, TThirteenth Thirteenth, TFourteenth Fourteenth, TFifteenth Fifteenth, TSixteenth Sixteenth)> Zip<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TEighth, TNinth, TTenth, TEleventh, TTwelfth, TThirteenth, TFourteenth, TFifteenth, TSixteenth>(this IQbservable<TFirst> first, IObservable<TSecond> second, IObservable<TThird> third, IObservable<TFourth> fourth, IObservable<TFifth> fifth, IObservable<TSixth> sixth, IObservable<TSeventh> seventh, IObservable<TEighth> eighth, IObservable<TNinth> ninth, IObservable<TTenth> tenth, IObservable<TEleventh> eleventh, IObservable<TTwelfth> twelfth, IObservable<TThirteenth> thirteenth, IObservable<TFourteenth> fourteenth, IObservable<TFifteenth> fifteenth, IObservable<TSixteenth> sixteenth)
	{
		if (first == null)
		{
			throw new ArgumentNullException("first");
		}
		if (second == null)
		{
			throw new ArgumentNullException("second");
		}
		if (third == null)
		{
			throw new ArgumentNullException("third");
		}
		if (fourth == null)
		{
			throw new ArgumentNullException("fourth");
		}
		if (fifth == null)
		{
			throw new ArgumentNullException("fifth");
		}
		if (sixth == null)
		{
			throw new ArgumentNullException("sixth");
		}
		if (seventh == null)
		{
			throw new ArgumentNullException("seventh");
		}
		if (eighth == null)
		{
			throw new ArgumentNullException("eighth");
		}
		if (ninth == null)
		{
			throw new ArgumentNullException("ninth");
		}
		if (tenth == null)
		{
			throw new ArgumentNullException("tenth");
		}
		if (eleventh == null)
		{
			throw new ArgumentNullException("eleventh");
		}
		if (twelfth == null)
		{
			throw new ArgumentNullException("twelfth");
		}
		if (thirteenth == null)
		{
			throw new ArgumentNullException("thirteenth");
		}
		if (fourteenth == null)
		{
			throw new ArgumentNullException("fourteenth");
		}
		if (fifteenth == null)
		{
			throw new ArgumentNullException("fifteenth");
		}
		if (sixteenth == null)
		{
			throw new ArgumentNullException("sixteenth");
		}
		return first.Provider.CreateQuery<(TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TEighth, TNinth, TTenth, TEleventh, TTwelfth, TThirteenth, TFourteenth, TFifteenth, TSixteenth)>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TFirst), typeof(TSecond), typeof(TThird), typeof(TFourth), typeof(TFifth), typeof(TSixth), typeof(TSeventh), typeof(TEighth), typeof(TNinth), typeof(TTenth), typeof(TEleventh), typeof(TTwelfth), typeof(TThirteenth), typeof(TFourteenth), typeof(TFifteenth), typeof(TSixteenth)), first.Expression, GetSourceExpression(second), GetSourceExpression(third), GetSourceExpression(fourth), GetSourceExpression(fifth), GetSourceExpression(sixth), GetSourceExpression(seventh), GetSourceExpression(eighth), GetSourceExpression(ninth), GetSourceExpression(tenth), GetSourceExpression(eleventh), GetSourceExpression(twelfth), GetSourceExpression(thirteenth), GetSourceExpression(fourteenth), GetSourceExpression(fifteenth), GetSourceExpression(sixteenth)));
	}
}
