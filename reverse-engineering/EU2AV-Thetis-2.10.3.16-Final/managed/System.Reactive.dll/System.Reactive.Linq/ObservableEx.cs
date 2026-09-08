using System.Collections.Generic;
using System.Reactive.Concurrency;

namespace System.Reactive.Linq;

public static class ObservableEx
{
	private static IQueryLanguageEx s_impl = QueryServices.GetQueryImpl((IQueryLanguageEx)new QueryLanguageEx());

	public static IObservable<(TFirst First, TSecond Second)> CombineLatest<TFirst, TSecond>(this IObservable<TFirst> first, IObservable<TSecond> second)
	{
		if (first == null)
		{
			throw new ArgumentNullException("first");
		}
		if (second == null)
		{
			throw new ArgumentNullException("second");
		}
		return s_impl.CombineLatest(first, second);
	}

	public static IObservable<(TFirst First, TSecond Second, TThird Third)> CombineLatest<TFirst, TSecond, TThird>(this IObservable<TFirst> first, IObservable<TSecond> second, IObservable<TThird> third)
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
		return s_impl.CombineLatest(first, second, third);
	}

	public static IObservable<(TFirst First, TSecond Second, TThird Third, TFourth Fourth)> CombineLatest<TFirst, TSecond, TThird, TFourth>(this IObservable<TFirst> first, IObservable<TSecond> second, IObservable<TThird> third, IObservable<TFourth> fourth)
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
		return s_impl.CombineLatest(first, second, third, fourth);
	}

	public static IObservable<(TFirst First, TSecond Second, TThird Third, TFourth Fourth, TFifth Fifth)> CombineLatest<TFirst, TSecond, TThird, TFourth, TFifth>(this IObservable<TFirst> first, IObservable<TSecond> second, IObservable<TThird> third, IObservable<TFourth> fourth, IObservable<TFifth> fifth)
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
		return s_impl.CombineLatest(first, second, third, fourth, fifth);
	}

	public static IObservable<(TFirst First, TSecond Second, TThird Third, TFourth Fourth, TFifth Fifth, TSixth Sixth)> CombineLatest<TFirst, TSecond, TThird, TFourth, TFifth, TSixth>(this IObservable<TFirst> first, IObservable<TSecond> second, IObservable<TThird> third, IObservable<TFourth> fourth, IObservable<TFifth> fifth, IObservable<TSixth> sixth)
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
		return s_impl.CombineLatest(first, second, third, fourth, fifth, sixth);
	}

	public static IObservable<(TFirst First, TSecond Second, TThird Third, TFourth Fourth, TFifth Fifth, TSixth Sixth, TSeventh Seventh)> CombineLatest<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh>(this IObservable<TFirst> first, IObservable<TSecond> second, IObservable<TThird> third, IObservable<TFourth> fourth, IObservable<TFifth> fifth, IObservable<TSixth> sixth, IObservable<TSeventh> seventh)
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
		return s_impl.CombineLatest(first, second, third, fourth, fifth, sixth, seventh);
	}

	public static IObservable<(TFirst First, TSecond Second, TThird Third, TFourth Fourth, TFifth Fifth, TSixth Sixth, TSeventh Seventh, TEighth Eighth)> CombineLatest<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TEighth>(this IObservable<TFirst> first, IObservable<TSecond> second, IObservable<TThird> third, IObservable<TFourth> fourth, IObservable<TFifth> fifth, IObservable<TSixth> sixth, IObservable<TSeventh> seventh, IObservable<TEighth> eighth)
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
		return s_impl.CombineLatest(first, second, third, fourth, fifth, sixth, seventh, eighth);
	}

	public static IObservable<(TFirst First, TSecond Second)> WithLatestFrom<TFirst, TSecond>(this IObservable<TFirst> first, IObservable<TSecond> second)
	{
		if (first == null)
		{
			throw new ArgumentNullException("first");
		}
		if (second == null)
		{
			throw new ArgumentNullException("second");
		}
		return s_impl.WithLatestFrom(first, second);
	}

	public static IObservable<(TFirst First, TSecond Second)> Zip<TFirst, TSecond>(this IObservable<TFirst> first, IEnumerable<TSecond> second)
	{
		if (first == null)
		{
			throw new ArgumentNullException("first");
		}
		if (second == null)
		{
			throw new ArgumentNullException("second");
		}
		return s_impl.Zip(first, second);
	}

	public static IObservable<(TFirst First, TSecond Second)> Zip<TFirst, TSecond>(this IObservable<TFirst> first, IObservable<TSecond> second)
	{
		if (first == null)
		{
			throw new ArgumentNullException("first");
		}
		if (second == null)
		{
			throw new ArgumentNullException("second");
		}
		return s_impl.Zip(first, second);
	}

	public static IObservable<(TFirst First, TSecond Second, TThird Third)> Zip<TFirst, TSecond, TThird>(this IObservable<TFirst> first, IObservable<TSecond> second, IObservable<TThird> third)
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
		return s_impl.Zip(first, second, third);
	}

	public static IObservable<(TFirst First, TSecond Second, TThird Third, TFourth Fourth)> Zip<TFirst, TSecond, TThird, TFourth>(this IObservable<TFirst> first, IObservable<TSecond> second, IObservable<TThird> third, IObservable<TFourth> fourth)
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
		return s_impl.Zip(first, second, third, fourth);
	}

	public static IObservable<(TFirst First, TSecond Second, TThird Third, TFourth Fourth, TFifth Fifth)> Zip<TFirst, TSecond, TThird, TFourth, TFifth>(this IObservable<TFirst> first, IObservable<TSecond> second, IObservable<TThird> third, IObservable<TFourth> fourth, IObservable<TFifth> fifth)
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
		return s_impl.Zip(first, second, third, fourth, fifth);
	}

	public static IObservable<(TFirst First, TSecond Second, TThird Third, TFourth Fourth, TFifth Fifth, TSixth Sixth)> Zip<TFirst, TSecond, TThird, TFourth, TFifth, TSixth>(this IObservable<TFirst> first, IObservable<TSecond> second, IObservable<TThird> third, IObservable<TFourth> fourth, IObservable<TFifth> fifth, IObservable<TSixth> sixth)
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
		return s_impl.Zip(first, second, third, fourth, fifth, sixth);
	}

	public static IObservable<(TFirst First, TSecond Second, TThird Third, TFourth Fourth, TFifth Fifth, TSixth Sixth, TSeventh Seventh)> Zip<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh>(this IObservable<TFirst> first, IObservable<TSecond> second, IObservable<TThird> third, IObservable<TFourth> fourth, IObservable<TFifth> fifth, IObservable<TSixth> sixth, IObservable<TSeventh> seventh)
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
		return s_impl.Zip(first, second, third, fourth, fifth, sixth, seventh);
	}

	public static IObservable<(TFirst First, TSecond Second, TThird Third, TFourth Fourth, TFifth Fifth, TSixth Sixth, TSeventh Seventh, TEighth Eighth)> Zip<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TEighth>(this IObservable<TFirst> first, IObservable<TSecond> second, IObservable<TThird> third, IObservable<TFourth> fourth, IObservable<TFifth> fifth, IObservable<TSixth> sixth, IObservable<TSeventh> seventh, IObservable<TEighth> eighth)
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
		return s_impl.Zip(first, second, third, fourth, fifth, sixth, seventh, eighth);
	}

	[Experimental]
	public static IObservable<TResult> Create<TResult>(Func<IObserver<TResult>, IEnumerable<IObservable<object>>> iteratorMethod)
	{
		if (iteratorMethod == null)
		{
			throw new ArgumentNullException("iteratorMethod");
		}
		return s_impl.Create(iteratorMethod);
	}

	[Experimental]
	public static IObservable<Unit> Create(Func<IEnumerable<IObservable<object>>> iteratorMethod)
	{
		if (iteratorMethod == null)
		{
			throw new ArgumentNullException("iteratorMethod");
		}
		return s_impl.Create(iteratorMethod);
	}

	[Experimental]
	public static IObservable<TSource> Expand<TSource>(this IObservable<TSource> source, Func<TSource, IObservable<TSource>> selector, IScheduler scheduler)
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
		return s_impl.Expand(source, selector, scheduler);
	}

	[Experimental]
	public static IObservable<TSource> Expand<TSource>(this IObservable<TSource> source, Func<TSource, IObservable<TSource>> selector)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		return s_impl.Expand(source, selector);
	}

	[Experimental]
	public static IObservable<TResult> ForkJoin<TSource1, TSource2, TResult>(this IObservable<TSource1> first, IObservable<TSource2> second, Func<TSource1, TSource2, TResult> resultSelector)
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
		return s_impl.ForkJoin(first, second, resultSelector);
	}

	[Experimental]
	public static IObservable<TSource[]> ForkJoin<TSource>(params IObservable<TSource>[] sources)
	{
		if (sources == null)
		{
			throw new ArgumentNullException("sources");
		}
		return s_impl.ForkJoin(sources);
	}

	[Experimental]
	public static IObservable<TSource[]> ForkJoin<TSource>(this IEnumerable<IObservable<TSource>> sources)
	{
		if (sources == null)
		{
			throw new ArgumentNullException("sources");
		}
		return s_impl.ForkJoin(sources);
	}

	[Experimental]
	public static IObservable<TResult> Let<TSource, TResult>(this IObservable<TSource> source, Func<IObservable<TSource>, IObservable<TResult>> selector)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		return s_impl.Let(source, selector);
	}

	[Experimental]
	public static IObservable<TResult> ManySelect<TSource, TResult>(this IObservable<TSource> source, Func<IObservable<TSource>, TResult> selector, IScheduler scheduler)
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
		return s_impl.ManySelect(source, selector, scheduler);
	}

	[Experimental]
	public static IObservable<TResult> ManySelect<TSource, TResult>(this IObservable<TSource> source, Func<IObservable<TSource>, TResult> selector)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		return s_impl.ManySelect(source, selector);
	}

	[Experimental]
	public static ListObservable<TSource> ToListObservable<TSource>(this IObservable<TSource> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return s_impl.ToListObservable(source);
	}
}
