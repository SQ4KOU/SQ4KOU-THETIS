using System.Collections.Generic;
using System.Reactive.Concurrency;

namespace System.Reactive.Linq;

internal interface IQueryLanguageEx
{
	IObservable<(T1, T2)> CombineLatest<T1, T2>(IObservable<T1> source1, IObservable<T2> source2);

	IObservable<(T1, T2, T3)> CombineLatest<T1, T2, T3>(IObservable<T1> source1, IObservable<T2> source2, IObservable<T3> source3);

	IObservable<(T1, T2, T3, T4)> CombineLatest<T1, T2, T3, T4>(IObservable<T1> source1, IObservable<T2> source2, IObservable<T3> source3, IObservable<T4> source4);

	IObservable<(T1, T2, T3, T4, T5)> CombineLatest<T1, T2, T3, T4, T5>(IObservable<T1> source1, IObservable<T2> source2, IObservable<T3> source3, IObservable<T4> source4, IObservable<T5> source5);

	IObservable<(T1, T2, T3, T4, T5, T6)> CombineLatest<T1, T2, T3, T4, T5, T6>(IObservable<T1> source1, IObservable<T2> source2, IObservable<T3> source3, IObservable<T4> source4, IObservable<T5> source5, IObservable<T6> source6);

	IObservable<(T1, T2, T3, T4, T5, T6, T7)> CombineLatest<T1, T2, T3, T4, T5, T6, T7>(IObservable<T1> source1, IObservable<T2> source2, IObservable<T3> source3, IObservable<T4> source4, IObservable<T5> source5, IObservable<T6> source6, IObservable<T7> source7);

	IObservable<(T1, T2, T3, T4, T5, T6, T7, T8)> CombineLatest<T1, T2, T3, T4, T5, T6, T7, T8>(IObservable<T1> source1, IObservable<T2> source2, IObservable<T3> source3, IObservable<T4> source4, IObservable<T5> source5, IObservable<T6> source6, IObservable<T7> source7, IObservable<T8> source8);

	IObservable<(T1, T2)> Zip<T1, T2>(IObservable<T1> source1, IObservable<T2> source2);

	IObservable<(T1, T2, T3)> Zip<T1, T2, T3>(IObservable<T1> source1, IObservable<T2> source2, IObservable<T3> source3);

	IObservable<(T1, T2, T3, T4)> Zip<T1, T2, T3, T4>(IObservable<T1> source1, IObservable<T2> source2, IObservable<T3> source3, IObservable<T4> source4);

	IObservable<(T1, T2, T3, T4, T5)> Zip<T1, T2, T3, T4, T5>(IObservable<T1> source1, IObservable<T2> source2, IObservable<T3> source3, IObservable<T4> source4, IObservable<T5> source5);

	IObservable<(T1, T2, T3, T4, T5, T6)> Zip<T1, T2, T3, T4, T5, T6>(IObservable<T1> source1, IObservable<T2> source2, IObservable<T3> source3, IObservable<T4> source4, IObservable<T5> source5, IObservable<T6> source6);

	IObservable<(T1, T2, T3, T4, T5, T6, T7)> Zip<T1, T2, T3, T4, T5, T6, T7>(IObservable<T1> source1, IObservable<T2> source2, IObservable<T3> source3, IObservable<T4> source4, IObservable<T5> source5, IObservable<T6> source6, IObservable<T7> source7);

	IObservable<(T1, T2, T3, T4, T5, T6, T7, T8)> Zip<T1, T2, T3, T4, T5, T6, T7, T8>(IObservable<T1> source1, IObservable<T2> source2, IObservable<T3> source3, IObservable<T4> source4, IObservable<T5> source5, IObservable<T6> source6, IObservable<T7> source7, IObservable<T8> source8);

	IObservable<TResult> Create<TResult>(Func<IObserver<TResult>, IEnumerable<IObservable<object>>> iteratorMethod);

	IObservable<Unit> Create(Func<IEnumerable<IObservable<object>>> iteratorMethod);

	IObservable<TSource> Expand<TSource>(IObservable<TSource> source, Func<TSource, IObservable<TSource>> selector);

	IObservable<TSource> Expand<TSource>(IObservable<TSource> source, Func<TSource, IObservable<TSource>> selector, IScheduler scheduler);

	IObservable<TResult> ForkJoin<TFirst, TSecond, TResult>(IObservable<TFirst> first, IObservable<TSecond> second, Func<TFirst, TSecond, TResult> resultSelector);

	IObservable<TSource[]> ForkJoin<TSource>(params IObservable<TSource>[] sources);

	IObservable<TSource[]> ForkJoin<TSource>(IEnumerable<IObservable<TSource>> sources);

	IObservable<TResult> Let<TSource, TResult>(IObservable<TSource> source, Func<IObservable<TSource>, IObservable<TResult>> function);

	IObservable<TResult> ManySelect<TSource, TResult>(IObservable<TSource> source, Func<IObservable<TSource>, TResult> selector);

	IObservable<TResult> ManySelect<TSource, TResult>(IObservable<TSource> source, Func<IObservable<TSource>, TResult> selector, IScheduler scheduler);

	ListObservable<TSource> ToListObservable<TSource>(IObservable<TSource> source);

	IObservable<(TFirst First, TSecond Second)> WithLatestFrom<TFirst, TSecond>(IObservable<TFirst> first, IObservable<TSecond> second);

	IObservable<(TFirst First, TSecond Second)> Zip<TFirst, TSecond>(IObservable<TFirst> first, IEnumerable<TSecond> second);
}
