using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq.ObservableImpl;
using System.Reactive.Subjects;

namespace System.Reactive.Linq;

internal class QueryLanguageEx : IQueryLanguageEx
{
	private sealed class CreateWithEnumerableObservable<TResult> : ObservableBase<TResult>
	{
		private readonly Func<IObserver<TResult>, IEnumerable<IObservable<object>>> _iteratorMethod;

		public CreateWithEnumerableObservable(Func<IObserver<TResult>, IEnumerable<IObservable<object>>> iteratorMethod)
		{
			_iteratorMethod = iteratorMethod;
		}

		protected override IDisposable SubscribeCore(IObserver<TResult> observer)
		{
			return _iteratorMethod(observer).Concat().Subscribe(new TerminalOnlyObserver<TResult>(observer));
		}
	}

	private sealed class TerminalOnlyObserver<TResult> : IObserver<object>
	{
		private readonly IObserver<TResult> _observer;

		public TerminalOnlyObserver(IObserver<TResult> observer)
		{
			_observer = observer;
		}

		public void OnCompleted()
		{
			_observer.OnCompleted();
		}

		public void OnError(Exception error)
		{
			_observer.OnError(error);
		}

		public void OnNext(object value)
		{
		}
	}

	private sealed class CreateWithOnlyEnumerableObservable<TResult> : ObservableBase<TResult>
	{
		private readonly Func<IEnumerable<IObservable<object>>> _iteratorMethod;

		public CreateWithOnlyEnumerableObservable(Func<IEnumerable<IObservable<object>>> iteratorMethod)
		{
			_iteratorMethod = iteratorMethod;
		}

		protected override IDisposable SubscribeCore(IObserver<TResult> observer)
		{
			return _iteratorMethod().Concat().Subscribe(new TerminalOnlyObserver<TResult>(observer));
		}
	}

	private sealed class ExpandObservable<TSource> : ObservableBase<TSource>
	{
		private readonly IObservable<TSource> _source;

		private readonly Func<TSource, IObservable<TSource>> _selector;

		private readonly IScheduler _scheduler;

		public ExpandObservable(IObservable<TSource> source, Func<TSource, IObservable<TSource>> selector, IScheduler scheduler)
		{
			_source = source;
			_selector = selector;
			_scheduler = scheduler;
		}

		protected override IDisposable SubscribeCore(IObserver<TSource> observer)
		{
			object outGate = new object();
			Queue<IObservable<TSource>> q = new Queue<IObservable<TSource>>();
			SerialDisposable m = new SerialDisposable();
			CompositeDisposable d = new CompositeDisposable { m };
			int activeCount = 0;
			bool isAcquired = false;
			lock (q)
			{
				q.Enqueue(_source);
				activeCount++;
			}
			ensureActive();
			return d;
			void ensureActive()
			{
				bool flag = false;
				lock (q)
				{
					if (q.Count > 0)
					{
						flag = !isAcquired;
						isAcquired = true;
					}
				}
				if (flag)
				{
					m.Disposable = _scheduler.Schedule(delegate(Action self)
					{
						IObservable<TSource> source;
						lock (q)
						{
							if (q.Count <= 0)
							{
								isAcquired = false;
								return;
							}
							source = q.Dequeue();
						}
						SingleAssignmentDisposable m2 = new SingleAssignmentDisposable();
						d.Add(m2);
						m2.Disposable = source.Subscribe(delegate(TSource x)
						{
							lock (outGate)
							{
								observer.OnNext(x);
							}
							IObservable<TSource> item;
							try
							{
								item = _selector(x);
							}
							catch (Exception error)
							{
								lock (outGate)
								{
									observer.OnError(error);
									return;
								}
							}
							lock (q)
							{
								q.Enqueue(item);
								activeCount++;
							}
							ensureActive();
						}, delegate(Exception exception)
						{
							lock (outGate)
							{
								observer.OnError(exception);
							}
						}, delegate
						{
							d.Remove(m2);
							bool flag2 = false;
							lock (q)
							{
								int num = activeCount;
								activeCount = num - 1;
								if (activeCount == 0)
								{
									flag2 = true;
								}
							}
							if (flag2)
							{
								lock (outGate)
								{
									observer.OnCompleted();
								}
							}
						});
						self();
					});
				}
			}
		}
	}

	private sealed class ForkJoinObservable<TSource> : ObservableBase<TSource[]>
	{
		private readonly IEnumerable<IObservable<TSource>> _sources;

		public ForkJoinObservable(IEnumerable<IObservable<TSource>> sources)
		{
			_sources = sources;
		}

		protected override IDisposable SubscribeCore(IObserver<TSource[]> observer)
		{
			IObservable<TSource>[] array = _sources.ToArray();
			int num = array.Length;
			if (num == 0)
			{
				observer.OnCompleted();
				return Disposable.Empty;
			}
			CompositeDisposable group = new CompositeDisposable(array.Length);
			object gate = new object();
			bool finished = false;
			bool[] hasResults = new bool[num];
			bool[] hasCompleted = new bool[num];
			List<TSource> results = new List<TSource>(num);
			lock (gate)
			{
				for (int i = 0; i < num; i++)
				{
					int currentIndex = i;
					IObservable<TSource> source = array[i];
					results.Add(default(TSource));
					group.Add(source.Subscribe(delegate(TSource value)
					{
						lock (gate)
						{
							if (!finished)
							{
								hasResults[currentIndex] = true;
								results[currentIndex] = value;
							}
						}
					}, delegate(Exception error)
					{
						lock (gate)
						{
							finished = true;
							observer.OnError(error);
							group.Dispose();
						}
					}, delegate
					{
						lock (gate)
						{
							if (!finished)
							{
								if (!hasResults[currentIndex])
								{
									observer.OnCompleted();
								}
								else
								{
									hasCompleted[currentIndex] = true;
									bool[] array2 = hasCompleted;
									for (int j = 0; j < array2.Length; j++)
									{
										if (!array2[j])
										{
											return;
										}
									}
									finished = true;
									observer.OnNext(results.ToArray());
									observer.OnCompleted();
								}
							}
						}
					}));
				}
			}
			return group;
		}
	}

	private class ChainObservable<T> : ISubject<IObservable<T>, T>, IObserver<IObservable<T>>, IObservable<T>
	{
		private readonly T _head;

		private readonly AsyncSubject<IObservable<T>> _tail = new AsyncSubject<IObservable<T>>();

		public ChainObservable(T head)
		{
			_head = head;
		}

		public IDisposable Subscribe(IObserver<T> observer)
		{
			CompositeDisposable compositeDisposable = new CompositeDisposable();
			compositeDisposable.Add(CurrentThreadScheduler.Instance.ScheduleAction((observer, compositeDisposable, this), delegate((IObserver<T> observer, CompositeDisposable g, ChainObservable<T> @this) state)
			{
				state.observer.OnNext(state.@this._head);
				state.g.Add(state.@this._tail.Merge().Subscribe(state.observer));
			}));
			return compositeDisposable;
		}

		public void OnCompleted()
		{
			OnNext(Observable.Empty<T>());
		}

		public void OnError(Exception error)
		{
			OnNext(Observable.Throw<T>(error));
		}

		public void OnNext(IObservable<T> value)
		{
			_tail.OnNext(value);
			_tail.OnCompleted();
		}
	}

	private sealed class CombineObservable<TLeft, TRight, TResult> : ObservableBase<TResult>
	{
		private readonly IObservable<TLeft> _leftSource;

		private readonly IObservable<TRight> _rightSource;

		private readonly Func<IObserver<TResult>, IDisposable, IDisposable, IObserver<Either<Notification<TLeft>, Notification<TRight>>>> _combinerSelector;

		public CombineObservable(IObservable<TLeft> leftSource, IObservable<TRight> rightSource, Func<IObserver<TResult>, IDisposable, IDisposable, IObserver<Either<Notification<TLeft>, Notification<TRight>>>> combinerSelector)
		{
			_leftSource = leftSource;
			_rightSource = rightSource;
			_combinerSelector = combinerSelector;
		}

		protected override IDisposable SubscribeCore(IObserver<TResult> observer)
		{
			SingleAssignmentDisposable singleAssignmentDisposable = new SingleAssignmentDisposable();
			SingleAssignmentDisposable singleAssignmentDisposable2 = new SingleAssignmentDisposable();
			IObserver<Either<Notification<TLeft>, Notification<TRight>>> observer2 = _combinerSelector(observer, singleAssignmentDisposable, singleAssignmentDisposable2);
			object gate = new object();
			singleAssignmentDisposable.Disposable = (from x in _leftSource.Materialize()
				select Either<Notification<TLeft>, Notification<TRight>>.CreateLeft(x)).Synchronize(gate).Subscribe(observer2);
			singleAssignmentDisposable2.Disposable = (from x in _rightSource.Materialize()
				select Either<Notification<TLeft>, Notification<TRight>>.CreateRight(x)).Synchronize(gate).Subscribe(observer2);
			return StableCompositeDisposable.Create(singleAssignmentDisposable, singleAssignmentDisposable2);
		}
	}

	public virtual IObservable<(TSource1, TSource2)> CombineLatest<TSource1, TSource2>(IObservable<TSource1> source1, IObservable<TSource2> source2)
	{
		return new CombineLatest<TSource1, TSource2, (TSource1, TSource2)>(source1, source2, (TSource1 t1, TSource2 t2) => (t1, t2));
	}

	public virtual IObservable<(TSource1, TSource2, TSource3)> CombineLatest<TSource1, TSource2, TSource3>(IObservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3)
	{
		return new CombineLatest<TSource1, TSource2, TSource3, (TSource1, TSource2, TSource3)>(source1, source2, source3, (TSource1 t1, TSource2 t2, TSource3 t3) => (t1, t2, t3));
	}

	public virtual IObservable<(TSource1, TSource2, TSource3, TSource4)> CombineLatest<TSource1, TSource2, TSource3, TSource4>(IObservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, IObservable<TSource4> source4)
	{
		return new CombineLatest<TSource1, TSource2, TSource3, TSource4, (TSource1, TSource2, TSource3, TSource4)>(source1, source2, source3, source4, (TSource1 t1, TSource2 t2, TSource3 t3, TSource4 t4) => (t1, t2, t3, t4));
	}

	public virtual IObservable<(TSource1, TSource2, TSource3, TSource4, TSource5)> CombineLatest<TSource1, TSource2, TSource3, TSource4, TSource5>(IObservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, IObservable<TSource4> source4, IObservable<TSource5> source5)
	{
		return new CombineLatest<TSource1, TSource2, TSource3, TSource4, TSource5, (TSource1, TSource2, TSource3, TSource4, TSource5)>(source1, source2, source3, source4, source5, (TSource1 t1, TSource2 t2, TSource3 t3, TSource4 t4, TSource5 t5) => (t1, t2, t3, t4, t5));
	}

	public virtual IObservable<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6)> CombineLatest<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6>(IObservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, IObservable<TSource4> source4, IObservable<TSource5> source5, IObservable<TSource6> source6)
	{
		return new CombineLatest<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, (TSource1, TSource2, TSource3, TSource4, TSource5, TSource6)>(source1, source2, source3, source4, source5, source6, (TSource1 t1, TSource2 t2, TSource3 t3, TSource4 t4, TSource5 t5, TSource6 t6) => (t1, t2, t3, t4, t5, t6));
	}

	public virtual IObservable<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7)> CombineLatest<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7>(IObservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, IObservable<TSource4> source4, IObservable<TSource5> source5, IObservable<TSource6> source6, IObservable<TSource7> source7)
	{
		return new CombineLatest<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, (TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7)>(source1, source2, source3, source4, source5, source6, source7, (TSource1 t1, TSource2 t2, TSource3 t3, TSource4 t4, TSource5 t5, TSource6 t6, TSource7 t7) => (t1, t2, t3, t4, t5, t6, t7));
	}

	public virtual IObservable<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8)> CombineLatest<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8>(IObservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, IObservable<TSource4> source4, IObservable<TSource5> source5, IObservable<TSource6> source6, IObservable<TSource7> source7, IObservable<TSource8> source8)
	{
		return new CombineLatest<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, (TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8)>(source1, source2, source3, source4, source5, source6, source7, source8, (TSource1 t1, TSource2 t2, TSource3 t3, TSource4 t4, TSource5 t5, TSource6 t6, TSource7 t7, TSource8 t8) => (t1, t2, t3, t4, t5, t6, t7, t8));
	}

	public virtual IObservable<(TSource1, TSource2)> Zip<TSource1, TSource2>(IObservable<TSource1> source1, IObservable<TSource2> source2)
	{
		return new Zip<TSource1, TSource2, (TSource1, TSource2)>.Observable(source1, source2, (TSource1 t1, TSource2 t2) => (t1, t2));
	}

	public virtual IObservable<(TSource1, TSource2, TSource3)> Zip<TSource1, TSource2, TSource3>(IObservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3)
	{
		return new Zip<TSource1, TSource2, TSource3, (TSource1, TSource2, TSource3)>(source1, source2, source3, (TSource1 t1, TSource2 t2, TSource3 t3) => (t1, t2, t3));
	}

	public virtual IObservable<(TSource1, TSource2, TSource3, TSource4)> Zip<TSource1, TSource2, TSource3, TSource4>(IObservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, IObservable<TSource4> source4)
	{
		return new Zip<TSource1, TSource2, TSource3, TSource4, (TSource1, TSource2, TSource3, TSource4)>(source1, source2, source3, source4, (TSource1 t1, TSource2 t2, TSource3 t3, TSource4 t4) => (t1, t2, t3, t4));
	}

	public virtual IObservable<(TSource1, TSource2, TSource3, TSource4, TSource5)> Zip<TSource1, TSource2, TSource3, TSource4, TSource5>(IObservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, IObservable<TSource4> source4, IObservable<TSource5> source5)
	{
		return new Zip<TSource1, TSource2, TSource3, TSource4, TSource5, (TSource1, TSource2, TSource3, TSource4, TSource5)>(source1, source2, source3, source4, source5, (TSource1 t1, TSource2 t2, TSource3 t3, TSource4 t4, TSource5 t5) => (t1, t2, t3, t4, t5));
	}

	public virtual IObservable<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6)> Zip<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6>(IObservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, IObservable<TSource4> source4, IObservable<TSource5> source5, IObservable<TSource6> source6)
	{
		return new Zip<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, (TSource1, TSource2, TSource3, TSource4, TSource5, TSource6)>(source1, source2, source3, source4, source5, source6, (TSource1 t1, TSource2 t2, TSource3 t3, TSource4 t4, TSource5 t5, TSource6 t6) => (t1, t2, t3, t4, t5, t6));
	}

	public virtual IObservable<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7)> Zip<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7>(IObservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, IObservable<TSource4> source4, IObservable<TSource5> source5, IObservable<TSource6> source6, IObservable<TSource7> source7)
	{
		return new Zip<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, (TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7)>(source1, source2, source3, source4, source5, source6, source7, (TSource1 t1, TSource2 t2, TSource3 t3, TSource4 t4, TSource5 t5, TSource6 t6, TSource7 t7) => (t1, t2, t3, t4, t5, t6, t7));
	}

	public virtual IObservable<(TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8)> Zip<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8>(IObservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, IObservable<TSource4> source4, IObservable<TSource5> source5, IObservable<TSource6> source6, IObservable<TSource7> source7, IObservable<TSource8> source8)
	{
		return new Zip<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, (TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8)>(source1, source2, source3, source4, source5, source6, source7, source8, (TSource1 t1, TSource2 t2, TSource3 t3, TSource4 t4, TSource5 t5, TSource6 t6, TSource7 t7, TSource8 t8) => (t1, t2, t3, t4, t5, t6, t7, t8));
	}

	public virtual IObservable<TResult> Create<TResult>(Func<IObserver<TResult>, IEnumerable<IObservable<object>>> iteratorMethod)
	{
		return new CreateWithEnumerableObservable<TResult>(iteratorMethod);
	}

	public virtual IObservable<Unit> Create(Func<IEnumerable<IObservable<object>>> iteratorMethod)
	{
		return new CreateWithOnlyEnumerableObservable<Unit>(iteratorMethod);
	}

	public virtual IObservable<TSource> Expand<TSource>(IObservable<TSource> source, Func<TSource, IObservable<TSource>> selector, IScheduler scheduler)
	{
		return new ExpandObservable<TSource>(source, selector, scheduler);
	}

	public virtual IObservable<TSource> Expand<TSource>(IObservable<TSource> source, Func<TSource, IObservable<TSource>> selector)
	{
		return source.Expand(selector, SchedulerDefaults.Iteration);
	}

	public virtual IObservable<TResult> ForkJoin<TFirst, TSecond, TResult>(IObservable<TFirst> first, IObservable<TSecond> second, Func<TFirst, TSecond, TResult> resultSelector)
	{
		return Combine(first, second, delegate(IObserver<TResult> observer, IDisposable leftSubscription, IDisposable rightSubscription)
		{
			bool leftStopped = false;
			bool rightStopped = false;
			bool hasLeft = false;
			bool hasRight = false;
			TFirst lastLeft = default(TFirst);
			TSecond lastRight = default(TSecond);
			return new BinaryObserver<TFirst, TSecond>(delegate(Notification<TFirst> left)
			{
				switch (left.Kind)
				{
				case NotificationKind.OnNext:
					hasLeft = true;
					lastLeft = left.Value;
					break;
				case NotificationKind.OnError:
					rightSubscription.Dispose();
					observer.OnError(left.Exception);
					break;
				case NotificationKind.OnCompleted:
					leftStopped = true;
					if (rightStopped)
					{
						if (!hasLeft)
						{
							observer.OnCompleted();
						}
						else if (!hasRight)
						{
							observer.OnCompleted();
						}
						else
						{
							TResult value;
							try
							{
								value = resultSelector(lastLeft, lastRight);
							}
							catch (Exception error)
							{
								observer.OnError(error);
								break;
							}
							observer.OnNext(value);
							observer.OnCompleted();
						}
					}
					break;
				}
			}, delegate(Notification<TSecond> right)
			{
				switch (right.Kind)
				{
				case NotificationKind.OnNext:
					hasRight = true;
					lastRight = right.Value;
					break;
				case NotificationKind.OnError:
					leftSubscription.Dispose();
					observer.OnError(right.Exception);
					break;
				case NotificationKind.OnCompleted:
					rightStopped = true;
					if (leftStopped)
					{
						if (!hasLeft)
						{
							observer.OnCompleted();
						}
						else if (!hasRight)
						{
							observer.OnCompleted();
						}
						else
						{
							TResult value;
							try
							{
								value = resultSelector(lastLeft, lastRight);
							}
							catch (Exception error)
							{
								observer.OnError(error);
								break;
							}
							observer.OnNext(value);
							observer.OnCompleted();
						}
					}
					break;
				}
			});
		});
	}

	public virtual IObservable<TSource[]> ForkJoin<TSource>(params IObservable<TSource>[] sources)
	{
		return ((IEnumerable<IObservable<TSource>>)sources).ForkJoin();
	}

	public virtual IObservable<TSource[]> ForkJoin<TSource>(IEnumerable<IObservable<TSource>> sources)
	{
		return new ForkJoinObservable<TSource>(sources);
	}

	public virtual IObservable<TResult> Let<TSource, TResult>(IObservable<TSource> source, Func<IObservable<TSource>, IObservable<TResult>> function)
	{
		return function(source);
	}

	public virtual IObservable<TResult> ManySelect<TSource, TResult>(IObservable<TSource> source, Func<IObservable<TSource>, TResult> selector)
	{
		return ManySelect(source, selector, DefaultScheduler.Instance);
	}

	public virtual IObservable<TResult> ManySelect<TSource, TResult>(IObservable<TSource> source, Func<IObservable<TSource>, TResult> selector, IScheduler scheduler)
	{
		return Observable.Defer(delegate
		{
			ChainObservable<TSource> chain = null;
			return source.Select((Func<TSource, IObservable<TSource>>)delegate(TSource x)
			{
				ChainObservable<TSource> chainObservable = new ChainObservable<TSource>(x);
				chain?.OnNext(chainObservable);
				chain = chainObservable;
				return chainObservable;
			}).Do(delegate
			{
			}, delegate(Exception exception)
			{
				chain?.OnError(exception);
			}, delegate
			{
				chain?.OnCompleted();
			}).ObserveOn(scheduler)
				.Select<IObservable<TSource>, TResult>(selector);
		});
	}

	public virtual ListObservable<TSource> ToListObservable<TSource>(IObservable<TSource> source)
	{
		return new ListObservable<TSource>(source);
	}

	public virtual IObservable<(TFirst First, TSecond Second)> WithLatestFrom<TFirst, TSecond>(IObservable<TFirst> first, IObservable<TSecond> second)
	{
		return new WithLatestFrom<TFirst, TSecond, (TFirst, TSecond)>(first, second, (TFirst t1, TSecond t2) => (t1, t2));
	}

	public virtual IObservable<(TFirst First, TSecond Second)> Zip<TFirst, TSecond>(IObservable<TFirst> first, IEnumerable<TSecond> second)
	{
		return new Zip<TFirst, TSecond, (TFirst, TSecond)>.Enumerable(first, second, (TFirst t1, TSecond t2) => (t1, t2));
	}

	private static IObservable<TResult> Combine<TLeft, TRight, TResult>(IObservable<TLeft> leftSource, IObservable<TRight> rightSource, Func<IObserver<TResult>, IDisposable, IDisposable, IObserver<Either<Notification<TLeft>, Notification<TRight>>>> combinerSelector)
	{
		return new CombineObservable<TLeft, TRight, TResult>(leftSource, rightSource, combinerSelector);
	}
}
