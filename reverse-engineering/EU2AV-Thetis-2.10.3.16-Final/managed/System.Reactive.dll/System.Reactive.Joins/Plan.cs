using System.Collections.Generic;

namespace System.Reactive.Joins;

public abstract class Plan<TResult>
{
	internal Plan()
	{
	}

	internal abstract ActivePlan Activate(Dictionary<object, IJoinObserver> externalSubscriptions, IObserver<TResult> observer, Action<ActivePlan> deactivate);

	internal static JoinObserver<TSource> CreateObserver<TSource>(Dictionary<object, IJoinObserver> externalSubscriptions, IObservable<TSource> observable, Action<Exception> onError)
	{
		JoinObserver<TSource> joinObserver;
		if (!externalSubscriptions.TryGetValue(observable, out IJoinObserver value))
		{
			joinObserver = new JoinObserver<TSource>(observable, onError);
			externalSubscriptions.Add(observable, joinObserver);
		}
		else
		{
			joinObserver = (JoinObserver<TSource>)value;
		}
		return joinObserver;
	}
}
internal sealed class Plan<T1, TResult> : Plan<TResult>
{
	internal Pattern<T1> Expression { get; }

	internal Func<T1, TResult> Selector { get; }

	internal Plan(Pattern<T1> expression, Func<T1, TResult> selector)
	{
		Expression = expression;
		Selector = selector;
	}

	internal override ActivePlan Activate(Dictionary<object, IJoinObserver> externalSubscriptions, IObserver<TResult> observer, Action<ActivePlan> deactivate)
	{
		Action<Exception> onError = observer.OnError;
		JoinObserver<T1> firstJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.First, onError);
		ActivePlan<T1> activePlan = null;
		activePlan = new ActivePlan<T1>(firstJoinObserver, delegate(T1 first)
		{
			TResult value;
			try
			{
				value = Selector(first);
			}
			catch (Exception error)
			{
				observer.OnError(error);
				return;
			}
			observer.OnNext(value);
		}, delegate
		{
			firstJoinObserver.RemoveActivePlan(activePlan);
			deactivate(activePlan);
		});
		firstJoinObserver.AddActivePlan(activePlan);
		return activePlan;
	}
}
internal sealed class Plan<T1, T2, TResult> : Plan<TResult>
{
	internal Pattern<T1, T2> Expression { get; }

	internal Func<T1, T2, TResult> Selector { get; }

	internal Plan(Pattern<T1, T2> expression, Func<T1, T2, TResult> selector)
	{
		Expression = expression;
		Selector = selector;
	}

	internal override ActivePlan Activate(Dictionary<object, IJoinObserver> externalSubscriptions, IObserver<TResult> observer, Action<ActivePlan> deactivate)
	{
		Action<Exception> onError = observer.OnError;
		JoinObserver<T1> firstJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.First, onError);
		JoinObserver<T2> secondJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Second, onError);
		ActivePlan<T1, T2> activePlan = null;
		activePlan = new ActivePlan<T1, T2>(firstJoinObserver, secondJoinObserver, delegate(T1 first, T2 second)
		{
			TResult value;
			try
			{
				value = Selector(first, second);
			}
			catch (Exception error)
			{
				observer.OnError(error);
				return;
			}
			observer.OnNext(value);
		}, delegate
		{
			firstJoinObserver.RemoveActivePlan(activePlan);
			secondJoinObserver.RemoveActivePlan(activePlan);
			deactivate(activePlan);
		});
		firstJoinObserver.AddActivePlan(activePlan);
		secondJoinObserver.AddActivePlan(activePlan);
		return activePlan;
	}
}
internal sealed class Plan<T1, T2, T3, TResult> : Plan<TResult>
{
	internal Pattern<T1, T2, T3> Expression { get; }

	internal Func<T1, T2, T3, TResult> Selector { get; }

	internal Plan(Pattern<T1, T2, T3> expression, Func<T1, T2, T3, TResult> selector)
	{
		Expression = expression;
		Selector = selector;
	}

	internal override ActivePlan Activate(Dictionary<object, IJoinObserver> externalSubscriptions, IObserver<TResult> observer, Action<ActivePlan> deactivate)
	{
		Action<Exception> onError = observer.OnError;
		JoinObserver<T1> firstJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.First, onError);
		JoinObserver<T2> secondJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Second, onError);
		JoinObserver<T3> thirdJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Third, onError);
		ActivePlan<T1, T2, T3> activePlan = null;
		activePlan = new ActivePlan<T1, T2, T3>(firstJoinObserver, secondJoinObserver, thirdJoinObserver, delegate(T1 first, T2 second, T3 third)
		{
			TResult value;
			try
			{
				value = Selector(first, second, third);
			}
			catch (Exception error)
			{
				observer.OnError(error);
				return;
			}
			observer.OnNext(value);
		}, delegate
		{
			firstJoinObserver.RemoveActivePlan(activePlan);
			secondJoinObserver.RemoveActivePlan(activePlan);
			thirdJoinObserver.RemoveActivePlan(activePlan);
			deactivate(activePlan);
		});
		firstJoinObserver.AddActivePlan(activePlan);
		secondJoinObserver.AddActivePlan(activePlan);
		thirdJoinObserver.AddActivePlan(activePlan);
		return activePlan;
	}
}
internal sealed class Plan<T1, T2, T3, T4, TResult> : Plan<TResult>
{
	internal Pattern<T1, T2, T3, T4> Expression { get; }

	internal Func<T1, T2, T3, T4, TResult> Selector { get; }

	internal Plan(Pattern<T1, T2, T3, T4> expression, Func<T1, T2, T3, T4, TResult> selector)
	{
		Expression = expression;
		Selector = selector;
	}

	internal override ActivePlan Activate(Dictionary<object, IJoinObserver> externalSubscriptions, IObserver<TResult> observer, Action<ActivePlan> deactivate)
	{
		Action<Exception> onError = observer.OnError;
		JoinObserver<T1> firstJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.First, onError);
		JoinObserver<T2> secondJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Second, onError);
		JoinObserver<T3> thirdJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Third, onError);
		JoinObserver<T4> fourthJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Fourth, onError);
		ActivePlan<T1, T2, T3, T4> activePlan = null;
		activePlan = new ActivePlan<T1, T2, T3, T4>(firstJoinObserver, secondJoinObserver, thirdJoinObserver, fourthJoinObserver, delegate(T1 first, T2 second, T3 third, T4 fourth)
		{
			TResult value;
			try
			{
				value = Selector(first, second, third, fourth);
			}
			catch (Exception error)
			{
				observer.OnError(error);
				return;
			}
			observer.OnNext(value);
		}, delegate
		{
			firstJoinObserver.RemoveActivePlan(activePlan);
			secondJoinObserver.RemoveActivePlan(activePlan);
			thirdJoinObserver.RemoveActivePlan(activePlan);
			fourthJoinObserver.RemoveActivePlan(activePlan);
			deactivate(activePlan);
		});
		firstJoinObserver.AddActivePlan(activePlan);
		secondJoinObserver.AddActivePlan(activePlan);
		thirdJoinObserver.AddActivePlan(activePlan);
		fourthJoinObserver.AddActivePlan(activePlan);
		return activePlan;
	}
}
internal sealed class Plan<T1, T2, T3, T4, T5, TResult> : Plan<TResult>
{
	internal Pattern<T1, T2, T3, T4, T5> Expression { get; }

	internal Func<T1, T2, T3, T4, T5, TResult> Selector { get; }

	internal Plan(Pattern<T1, T2, T3, T4, T5> expression, Func<T1, T2, T3, T4, T5, TResult> selector)
	{
		Expression = expression;
		Selector = selector;
	}

	internal override ActivePlan Activate(Dictionary<object, IJoinObserver> externalSubscriptions, IObserver<TResult> observer, Action<ActivePlan> deactivate)
	{
		Action<Exception> onError = observer.OnError;
		JoinObserver<T1> firstJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.First, onError);
		JoinObserver<T2> secondJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Second, onError);
		JoinObserver<T3> thirdJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Third, onError);
		JoinObserver<T4> fourthJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Fourth, onError);
		JoinObserver<T5> fifthJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Fifth, onError);
		ActivePlan<T1, T2, T3, T4, T5> activePlan = null;
		activePlan = new ActivePlan<T1, T2, T3, T4, T5>(firstJoinObserver, secondJoinObserver, thirdJoinObserver, fourthJoinObserver, fifthJoinObserver, delegate(T1 first, T2 second, T3 third, T4 fourth, T5 fifth)
		{
			TResult value;
			try
			{
				value = Selector(first, second, third, fourth, fifth);
			}
			catch (Exception error)
			{
				observer.OnError(error);
				return;
			}
			observer.OnNext(value);
		}, delegate
		{
			firstJoinObserver.RemoveActivePlan(activePlan);
			secondJoinObserver.RemoveActivePlan(activePlan);
			thirdJoinObserver.RemoveActivePlan(activePlan);
			fourthJoinObserver.RemoveActivePlan(activePlan);
			fifthJoinObserver.RemoveActivePlan(activePlan);
			deactivate(activePlan);
		});
		firstJoinObserver.AddActivePlan(activePlan);
		secondJoinObserver.AddActivePlan(activePlan);
		thirdJoinObserver.AddActivePlan(activePlan);
		fourthJoinObserver.AddActivePlan(activePlan);
		fifthJoinObserver.AddActivePlan(activePlan);
		return activePlan;
	}
}
internal sealed class Plan<T1, T2, T3, T4, T5, T6, TResult> : Plan<TResult>
{
	internal Pattern<T1, T2, T3, T4, T5, T6> Expression { get; }

	internal Func<T1, T2, T3, T4, T5, T6, TResult> Selector { get; }

	internal Plan(Pattern<T1, T2, T3, T4, T5, T6> expression, Func<T1, T2, T3, T4, T5, T6, TResult> selector)
	{
		Expression = expression;
		Selector = selector;
	}

	internal override ActivePlan Activate(Dictionary<object, IJoinObserver> externalSubscriptions, IObserver<TResult> observer, Action<ActivePlan> deactivate)
	{
		Action<Exception> onError = observer.OnError;
		JoinObserver<T1> firstJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.First, onError);
		JoinObserver<T2> secondJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Second, onError);
		JoinObserver<T3> thirdJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Third, onError);
		JoinObserver<T4> fourthJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Fourth, onError);
		JoinObserver<T5> fifthJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Fifth, onError);
		JoinObserver<T6> sixthJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Sixth, onError);
		ActivePlan<T1, T2, T3, T4, T5, T6> activePlan = null;
		activePlan = new ActivePlan<T1, T2, T3, T4, T5, T6>(firstJoinObserver, secondJoinObserver, thirdJoinObserver, fourthJoinObserver, fifthJoinObserver, sixthJoinObserver, delegate(T1 first, T2 second, T3 third, T4 fourth, T5 fifth, T6 sixth)
		{
			TResult value;
			try
			{
				value = Selector(first, second, third, fourth, fifth, sixth);
			}
			catch (Exception error)
			{
				observer.OnError(error);
				return;
			}
			observer.OnNext(value);
		}, delegate
		{
			firstJoinObserver.RemoveActivePlan(activePlan);
			secondJoinObserver.RemoveActivePlan(activePlan);
			thirdJoinObserver.RemoveActivePlan(activePlan);
			fourthJoinObserver.RemoveActivePlan(activePlan);
			fifthJoinObserver.RemoveActivePlan(activePlan);
			sixthJoinObserver.RemoveActivePlan(activePlan);
			deactivate(activePlan);
		});
		firstJoinObserver.AddActivePlan(activePlan);
		secondJoinObserver.AddActivePlan(activePlan);
		thirdJoinObserver.AddActivePlan(activePlan);
		fourthJoinObserver.AddActivePlan(activePlan);
		fifthJoinObserver.AddActivePlan(activePlan);
		sixthJoinObserver.AddActivePlan(activePlan);
		return activePlan;
	}
}
internal sealed class Plan<T1, T2, T3, T4, T5, T6, T7, TResult> : Plan<TResult>
{
	internal Pattern<T1, T2, T3, T4, T5, T6, T7> Expression { get; }

	internal Func<T1, T2, T3, T4, T5, T6, T7, TResult> Selector { get; }

	internal Plan(Pattern<T1, T2, T3, T4, T5, T6, T7> expression, Func<T1, T2, T3, T4, T5, T6, T7, TResult> selector)
	{
		Expression = expression;
		Selector = selector;
	}

	internal override ActivePlan Activate(Dictionary<object, IJoinObserver> externalSubscriptions, IObserver<TResult> observer, Action<ActivePlan> deactivate)
	{
		Action<Exception> onError = observer.OnError;
		JoinObserver<T1> firstJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.First, onError);
		JoinObserver<T2> secondJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Second, onError);
		JoinObserver<T3> thirdJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Third, onError);
		JoinObserver<T4> fourthJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Fourth, onError);
		JoinObserver<T5> fifthJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Fifth, onError);
		JoinObserver<T6> sixthJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Sixth, onError);
		JoinObserver<T7> seventhJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Seventh, onError);
		ActivePlan<T1, T2, T3, T4, T5, T6, T7> activePlan = null;
		activePlan = new ActivePlan<T1, T2, T3, T4, T5, T6, T7>(firstJoinObserver, secondJoinObserver, thirdJoinObserver, fourthJoinObserver, fifthJoinObserver, sixthJoinObserver, seventhJoinObserver, delegate(T1 first, T2 second, T3 third, T4 fourth, T5 fifth, T6 sixth, T7 seventh)
		{
			TResult value;
			try
			{
				value = Selector(first, second, third, fourth, fifth, sixth, seventh);
			}
			catch (Exception error)
			{
				observer.OnError(error);
				return;
			}
			observer.OnNext(value);
		}, delegate
		{
			firstJoinObserver.RemoveActivePlan(activePlan);
			secondJoinObserver.RemoveActivePlan(activePlan);
			thirdJoinObserver.RemoveActivePlan(activePlan);
			fourthJoinObserver.RemoveActivePlan(activePlan);
			fifthJoinObserver.RemoveActivePlan(activePlan);
			sixthJoinObserver.RemoveActivePlan(activePlan);
			seventhJoinObserver.RemoveActivePlan(activePlan);
			deactivate(activePlan);
		});
		firstJoinObserver.AddActivePlan(activePlan);
		secondJoinObserver.AddActivePlan(activePlan);
		thirdJoinObserver.AddActivePlan(activePlan);
		fourthJoinObserver.AddActivePlan(activePlan);
		fifthJoinObserver.AddActivePlan(activePlan);
		sixthJoinObserver.AddActivePlan(activePlan);
		seventhJoinObserver.AddActivePlan(activePlan);
		return activePlan;
	}
}
internal sealed class Plan<T1, T2, T3, T4, T5, T6, T7, T8, TResult> : Plan<TResult>
{
	internal Pattern<T1, T2, T3, T4, T5, T6, T7, T8> Expression { get; }

	internal Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> Selector { get; }

	internal Plan(Pattern<T1, T2, T3, T4, T5, T6, T7, T8> expression, Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> selector)
	{
		Expression = expression;
		Selector = selector;
	}

	internal override ActivePlan Activate(Dictionary<object, IJoinObserver> externalSubscriptions, IObserver<TResult> observer, Action<ActivePlan> deactivate)
	{
		Action<Exception> onError = observer.OnError;
		JoinObserver<T1> firstJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.First, onError);
		JoinObserver<T2> secondJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Second, onError);
		JoinObserver<T3> thirdJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Third, onError);
		JoinObserver<T4> fourthJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Fourth, onError);
		JoinObserver<T5> fifthJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Fifth, onError);
		JoinObserver<T6> sixthJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Sixth, onError);
		JoinObserver<T7> seventhJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Seventh, onError);
		JoinObserver<T8> eighthJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Eighth, onError);
		ActivePlan<T1, T2, T3, T4, T5, T6, T7, T8> activePlan = null;
		activePlan = new ActivePlan<T1, T2, T3, T4, T5, T6, T7, T8>(firstJoinObserver, secondJoinObserver, thirdJoinObserver, fourthJoinObserver, fifthJoinObserver, sixthJoinObserver, seventhJoinObserver, eighthJoinObserver, delegate(T1 first, T2 second, T3 third, T4 fourth, T5 fifth, T6 sixth, T7 seventh, T8 eighth)
		{
			TResult value;
			try
			{
				value = Selector(first, second, third, fourth, fifth, sixth, seventh, eighth);
			}
			catch (Exception error)
			{
				observer.OnError(error);
				return;
			}
			observer.OnNext(value);
		}, delegate
		{
			firstJoinObserver.RemoveActivePlan(activePlan);
			secondJoinObserver.RemoveActivePlan(activePlan);
			thirdJoinObserver.RemoveActivePlan(activePlan);
			fourthJoinObserver.RemoveActivePlan(activePlan);
			fifthJoinObserver.RemoveActivePlan(activePlan);
			sixthJoinObserver.RemoveActivePlan(activePlan);
			seventhJoinObserver.RemoveActivePlan(activePlan);
			eighthJoinObserver.RemoveActivePlan(activePlan);
			deactivate(activePlan);
		});
		firstJoinObserver.AddActivePlan(activePlan);
		secondJoinObserver.AddActivePlan(activePlan);
		thirdJoinObserver.AddActivePlan(activePlan);
		fourthJoinObserver.AddActivePlan(activePlan);
		fifthJoinObserver.AddActivePlan(activePlan);
		sixthJoinObserver.AddActivePlan(activePlan);
		seventhJoinObserver.AddActivePlan(activePlan);
		eighthJoinObserver.AddActivePlan(activePlan);
		return activePlan;
	}
}
internal sealed class Plan<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> : Plan<TResult>
{
	internal Pattern<T1, T2, T3, T4, T5, T6, T7, T8, T9> Expression { get; }

	internal Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> Selector { get; }

	internal Plan(Pattern<T1, T2, T3, T4, T5, T6, T7, T8, T9> expression, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> selector)
	{
		Expression = expression;
		Selector = selector;
	}

	internal override ActivePlan Activate(Dictionary<object, IJoinObserver> externalSubscriptions, IObserver<TResult> observer, Action<ActivePlan> deactivate)
	{
		Action<Exception> onError = observer.OnError;
		JoinObserver<T1> firstJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.First, onError);
		JoinObserver<T2> secondJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Second, onError);
		JoinObserver<T3> thirdJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Third, onError);
		JoinObserver<T4> fourthJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Fourth, onError);
		JoinObserver<T5> fifthJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Fifth, onError);
		JoinObserver<T6> sixthJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Sixth, onError);
		JoinObserver<T7> seventhJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Seventh, onError);
		JoinObserver<T8> eighthJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Eighth, onError);
		JoinObserver<T9> ninthJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Ninth, onError);
		ActivePlan<T1, T2, T3, T4, T5, T6, T7, T8, T9> activePlan = null;
		activePlan = new ActivePlan<T1, T2, T3, T4, T5, T6, T7, T8, T9>(firstJoinObserver, secondJoinObserver, thirdJoinObserver, fourthJoinObserver, fifthJoinObserver, sixthJoinObserver, seventhJoinObserver, eighthJoinObserver, ninthJoinObserver, delegate(T1 first, T2 second, T3 third, T4 fourth, T5 fifth, T6 sixth, T7 seventh, T8 eighth, T9 ninth)
		{
			TResult value;
			try
			{
				value = Selector(first, second, third, fourth, fifth, sixth, seventh, eighth, ninth);
			}
			catch (Exception error)
			{
				observer.OnError(error);
				return;
			}
			observer.OnNext(value);
		}, delegate
		{
			firstJoinObserver.RemoveActivePlan(activePlan);
			secondJoinObserver.RemoveActivePlan(activePlan);
			thirdJoinObserver.RemoveActivePlan(activePlan);
			fourthJoinObserver.RemoveActivePlan(activePlan);
			fifthJoinObserver.RemoveActivePlan(activePlan);
			sixthJoinObserver.RemoveActivePlan(activePlan);
			seventhJoinObserver.RemoveActivePlan(activePlan);
			eighthJoinObserver.RemoveActivePlan(activePlan);
			ninthJoinObserver.RemoveActivePlan(activePlan);
			deactivate(activePlan);
		});
		firstJoinObserver.AddActivePlan(activePlan);
		secondJoinObserver.AddActivePlan(activePlan);
		thirdJoinObserver.AddActivePlan(activePlan);
		fourthJoinObserver.AddActivePlan(activePlan);
		fifthJoinObserver.AddActivePlan(activePlan);
		sixthJoinObserver.AddActivePlan(activePlan);
		seventhJoinObserver.AddActivePlan(activePlan);
		eighthJoinObserver.AddActivePlan(activePlan);
		ninthJoinObserver.AddActivePlan(activePlan);
		return activePlan;
	}
}
internal sealed class Plan<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> : Plan<TResult>
{
	internal Pattern<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Expression { get; }

	internal Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> Selector { get; }

	internal Plan(Pattern<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> expression, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> selector)
	{
		Expression = expression;
		Selector = selector;
	}

	internal override ActivePlan Activate(Dictionary<object, IJoinObserver> externalSubscriptions, IObserver<TResult> observer, Action<ActivePlan> deactivate)
	{
		Action<Exception> onError = observer.OnError;
		JoinObserver<T1> firstJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.First, onError);
		JoinObserver<T2> secondJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Second, onError);
		JoinObserver<T3> thirdJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Third, onError);
		JoinObserver<T4> fourthJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Fourth, onError);
		JoinObserver<T5> fifthJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Fifth, onError);
		JoinObserver<T6> sixthJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Sixth, onError);
		JoinObserver<T7> seventhJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Seventh, onError);
		JoinObserver<T8> eighthJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Eighth, onError);
		JoinObserver<T9> ninthJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Ninth, onError);
		JoinObserver<T10> tenthJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Tenth, onError);
		ActivePlan<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> activePlan = null;
		activePlan = new ActivePlan<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(firstJoinObserver, secondJoinObserver, thirdJoinObserver, fourthJoinObserver, fifthJoinObserver, sixthJoinObserver, seventhJoinObserver, eighthJoinObserver, ninthJoinObserver, tenthJoinObserver, delegate(T1 first, T2 second, T3 third, T4 fourth, T5 fifth, T6 sixth, T7 seventh, T8 eighth, T9 ninth, T10 tenth)
		{
			TResult value;
			try
			{
				value = Selector(first, second, third, fourth, fifth, sixth, seventh, eighth, ninth, tenth);
			}
			catch (Exception error)
			{
				observer.OnError(error);
				return;
			}
			observer.OnNext(value);
		}, delegate
		{
			firstJoinObserver.RemoveActivePlan(activePlan);
			secondJoinObserver.RemoveActivePlan(activePlan);
			thirdJoinObserver.RemoveActivePlan(activePlan);
			fourthJoinObserver.RemoveActivePlan(activePlan);
			fifthJoinObserver.RemoveActivePlan(activePlan);
			sixthJoinObserver.RemoveActivePlan(activePlan);
			seventhJoinObserver.RemoveActivePlan(activePlan);
			eighthJoinObserver.RemoveActivePlan(activePlan);
			ninthJoinObserver.RemoveActivePlan(activePlan);
			tenthJoinObserver.RemoveActivePlan(activePlan);
			deactivate(activePlan);
		});
		firstJoinObserver.AddActivePlan(activePlan);
		secondJoinObserver.AddActivePlan(activePlan);
		thirdJoinObserver.AddActivePlan(activePlan);
		fourthJoinObserver.AddActivePlan(activePlan);
		fifthJoinObserver.AddActivePlan(activePlan);
		sixthJoinObserver.AddActivePlan(activePlan);
		seventhJoinObserver.AddActivePlan(activePlan);
		eighthJoinObserver.AddActivePlan(activePlan);
		ninthJoinObserver.AddActivePlan(activePlan);
		tenthJoinObserver.AddActivePlan(activePlan);
		return activePlan;
	}
}
internal sealed class Plan<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult> : Plan<TResult>
{
	internal Pattern<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Expression { get; }

	internal Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult> Selector { get; }

	internal Plan(Pattern<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> expression, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult> selector)
	{
		Expression = expression;
		Selector = selector;
	}

	internal override ActivePlan Activate(Dictionary<object, IJoinObserver> externalSubscriptions, IObserver<TResult> observer, Action<ActivePlan> deactivate)
	{
		Action<Exception> onError = observer.OnError;
		JoinObserver<T1> firstJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.First, onError);
		JoinObserver<T2> secondJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Second, onError);
		JoinObserver<T3> thirdJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Third, onError);
		JoinObserver<T4> fourthJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Fourth, onError);
		JoinObserver<T5> fifthJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Fifth, onError);
		JoinObserver<T6> sixthJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Sixth, onError);
		JoinObserver<T7> seventhJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Seventh, onError);
		JoinObserver<T8> eighthJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Eighth, onError);
		JoinObserver<T9> ninthJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Ninth, onError);
		JoinObserver<T10> tenthJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Tenth, onError);
		JoinObserver<T11> eleventhJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Eleventh, onError);
		ActivePlan<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> activePlan = null;
		activePlan = new ActivePlan<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(firstJoinObserver, secondJoinObserver, thirdJoinObserver, fourthJoinObserver, fifthJoinObserver, sixthJoinObserver, seventhJoinObserver, eighthJoinObserver, ninthJoinObserver, tenthJoinObserver, eleventhJoinObserver, delegate(T1 first, T2 second, T3 third, T4 fourth, T5 fifth, T6 sixth, T7 seventh, T8 eighth, T9 ninth, T10 tenth, T11 eleventh)
		{
			TResult value;
			try
			{
				value = Selector(first, second, third, fourth, fifth, sixth, seventh, eighth, ninth, tenth, eleventh);
			}
			catch (Exception error)
			{
				observer.OnError(error);
				return;
			}
			observer.OnNext(value);
		}, delegate
		{
			firstJoinObserver.RemoveActivePlan(activePlan);
			secondJoinObserver.RemoveActivePlan(activePlan);
			thirdJoinObserver.RemoveActivePlan(activePlan);
			fourthJoinObserver.RemoveActivePlan(activePlan);
			fifthJoinObserver.RemoveActivePlan(activePlan);
			sixthJoinObserver.RemoveActivePlan(activePlan);
			seventhJoinObserver.RemoveActivePlan(activePlan);
			eighthJoinObserver.RemoveActivePlan(activePlan);
			ninthJoinObserver.RemoveActivePlan(activePlan);
			tenthJoinObserver.RemoveActivePlan(activePlan);
			eleventhJoinObserver.RemoveActivePlan(activePlan);
			deactivate(activePlan);
		});
		firstJoinObserver.AddActivePlan(activePlan);
		secondJoinObserver.AddActivePlan(activePlan);
		thirdJoinObserver.AddActivePlan(activePlan);
		fourthJoinObserver.AddActivePlan(activePlan);
		fifthJoinObserver.AddActivePlan(activePlan);
		sixthJoinObserver.AddActivePlan(activePlan);
		seventhJoinObserver.AddActivePlan(activePlan);
		eighthJoinObserver.AddActivePlan(activePlan);
		ninthJoinObserver.AddActivePlan(activePlan);
		tenthJoinObserver.AddActivePlan(activePlan);
		eleventhJoinObserver.AddActivePlan(activePlan);
		return activePlan;
	}
}
internal sealed class Plan<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult> : Plan<TResult>
{
	internal Pattern<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Expression { get; }

	internal Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult> Selector { get; }

	internal Plan(Pattern<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> expression, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult> selector)
	{
		Expression = expression;
		Selector = selector;
	}

	internal override ActivePlan Activate(Dictionary<object, IJoinObserver> externalSubscriptions, IObserver<TResult> observer, Action<ActivePlan> deactivate)
	{
		Action<Exception> onError = observer.OnError;
		JoinObserver<T1> firstJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.First, onError);
		JoinObserver<T2> secondJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Second, onError);
		JoinObserver<T3> thirdJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Third, onError);
		JoinObserver<T4> fourthJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Fourth, onError);
		JoinObserver<T5> fifthJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Fifth, onError);
		JoinObserver<T6> sixthJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Sixth, onError);
		JoinObserver<T7> seventhJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Seventh, onError);
		JoinObserver<T8> eighthJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Eighth, onError);
		JoinObserver<T9> ninthJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Ninth, onError);
		JoinObserver<T10> tenthJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Tenth, onError);
		JoinObserver<T11> eleventhJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Eleventh, onError);
		JoinObserver<T12> twelfthJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Twelfth, onError);
		ActivePlan<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> activePlan = null;
		activePlan = new ActivePlan<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(firstJoinObserver, secondJoinObserver, thirdJoinObserver, fourthJoinObserver, fifthJoinObserver, sixthJoinObserver, seventhJoinObserver, eighthJoinObserver, ninthJoinObserver, tenthJoinObserver, eleventhJoinObserver, twelfthJoinObserver, delegate(T1 first, T2 second, T3 third, T4 fourth, T5 fifth, T6 sixth, T7 seventh, T8 eighth, T9 ninth, T10 tenth, T11 eleventh, T12 twelfth)
		{
			TResult value;
			try
			{
				value = Selector(first, second, third, fourth, fifth, sixth, seventh, eighth, ninth, tenth, eleventh, twelfth);
			}
			catch (Exception error)
			{
				observer.OnError(error);
				return;
			}
			observer.OnNext(value);
		}, delegate
		{
			firstJoinObserver.RemoveActivePlan(activePlan);
			secondJoinObserver.RemoveActivePlan(activePlan);
			thirdJoinObserver.RemoveActivePlan(activePlan);
			fourthJoinObserver.RemoveActivePlan(activePlan);
			fifthJoinObserver.RemoveActivePlan(activePlan);
			sixthJoinObserver.RemoveActivePlan(activePlan);
			seventhJoinObserver.RemoveActivePlan(activePlan);
			eighthJoinObserver.RemoveActivePlan(activePlan);
			ninthJoinObserver.RemoveActivePlan(activePlan);
			tenthJoinObserver.RemoveActivePlan(activePlan);
			eleventhJoinObserver.RemoveActivePlan(activePlan);
			twelfthJoinObserver.RemoveActivePlan(activePlan);
			deactivate(activePlan);
		});
		firstJoinObserver.AddActivePlan(activePlan);
		secondJoinObserver.AddActivePlan(activePlan);
		thirdJoinObserver.AddActivePlan(activePlan);
		fourthJoinObserver.AddActivePlan(activePlan);
		fifthJoinObserver.AddActivePlan(activePlan);
		sixthJoinObserver.AddActivePlan(activePlan);
		seventhJoinObserver.AddActivePlan(activePlan);
		eighthJoinObserver.AddActivePlan(activePlan);
		ninthJoinObserver.AddActivePlan(activePlan);
		tenthJoinObserver.AddActivePlan(activePlan);
		eleventhJoinObserver.AddActivePlan(activePlan);
		twelfthJoinObserver.AddActivePlan(activePlan);
		return activePlan;
	}
}
internal sealed class Plan<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult> : Plan<TResult>
{
	internal Pattern<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Expression { get; }

	internal Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult> Selector { get; }

	internal Plan(Pattern<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> expression, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult> selector)
	{
		Expression = expression;
		Selector = selector;
	}

	internal override ActivePlan Activate(Dictionary<object, IJoinObserver> externalSubscriptions, IObserver<TResult> observer, Action<ActivePlan> deactivate)
	{
		Action<Exception> onError = observer.OnError;
		JoinObserver<T1> firstJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.First, onError);
		JoinObserver<T2> secondJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Second, onError);
		JoinObserver<T3> thirdJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Third, onError);
		JoinObserver<T4> fourthJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Fourth, onError);
		JoinObserver<T5> fifthJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Fifth, onError);
		JoinObserver<T6> sixthJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Sixth, onError);
		JoinObserver<T7> seventhJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Seventh, onError);
		JoinObserver<T8> eighthJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Eighth, onError);
		JoinObserver<T9> ninthJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Ninth, onError);
		JoinObserver<T10> tenthJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Tenth, onError);
		JoinObserver<T11> eleventhJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Eleventh, onError);
		JoinObserver<T12> twelfthJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Twelfth, onError);
		JoinObserver<T13> thirteenthJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Thirteenth, onError);
		ActivePlan<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> activePlan = null;
		activePlan = new ActivePlan<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(firstJoinObserver, secondJoinObserver, thirdJoinObserver, fourthJoinObserver, fifthJoinObserver, sixthJoinObserver, seventhJoinObserver, eighthJoinObserver, ninthJoinObserver, tenthJoinObserver, eleventhJoinObserver, twelfthJoinObserver, thirteenthJoinObserver, delegate(T1 first, T2 second, T3 third, T4 fourth, T5 fifth, T6 sixth, T7 seventh, T8 eighth, T9 ninth, T10 tenth, T11 eleventh, T12 twelfth, T13 thirteenth)
		{
			TResult value;
			try
			{
				value = Selector(first, second, third, fourth, fifth, sixth, seventh, eighth, ninth, tenth, eleventh, twelfth, thirteenth);
			}
			catch (Exception error)
			{
				observer.OnError(error);
				return;
			}
			observer.OnNext(value);
		}, delegate
		{
			firstJoinObserver.RemoveActivePlan(activePlan);
			secondJoinObserver.RemoveActivePlan(activePlan);
			thirdJoinObserver.RemoveActivePlan(activePlan);
			fourthJoinObserver.RemoveActivePlan(activePlan);
			fifthJoinObserver.RemoveActivePlan(activePlan);
			sixthJoinObserver.RemoveActivePlan(activePlan);
			seventhJoinObserver.RemoveActivePlan(activePlan);
			eighthJoinObserver.RemoveActivePlan(activePlan);
			ninthJoinObserver.RemoveActivePlan(activePlan);
			tenthJoinObserver.RemoveActivePlan(activePlan);
			eleventhJoinObserver.RemoveActivePlan(activePlan);
			twelfthJoinObserver.RemoveActivePlan(activePlan);
			thirteenthJoinObserver.RemoveActivePlan(activePlan);
			deactivate(activePlan);
		});
		firstJoinObserver.AddActivePlan(activePlan);
		secondJoinObserver.AddActivePlan(activePlan);
		thirdJoinObserver.AddActivePlan(activePlan);
		fourthJoinObserver.AddActivePlan(activePlan);
		fifthJoinObserver.AddActivePlan(activePlan);
		sixthJoinObserver.AddActivePlan(activePlan);
		seventhJoinObserver.AddActivePlan(activePlan);
		eighthJoinObserver.AddActivePlan(activePlan);
		ninthJoinObserver.AddActivePlan(activePlan);
		tenthJoinObserver.AddActivePlan(activePlan);
		eleventhJoinObserver.AddActivePlan(activePlan);
		twelfthJoinObserver.AddActivePlan(activePlan);
		thirteenthJoinObserver.AddActivePlan(activePlan);
		return activePlan;
	}
}
internal sealed class Plan<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult> : Plan<TResult>
{
	internal Pattern<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Expression { get; }

	internal Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult> Selector { get; }

	internal Plan(Pattern<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> expression, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult> selector)
	{
		Expression = expression;
		Selector = selector;
	}

	internal override ActivePlan Activate(Dictionary<object, IJoinObserver> externalSubscriptions, IObserver<TResult> observer, Action<ActivePlan> deactivate)
	{
		Action<Exception> onError = observer.OnError;
		JoinObserver<T1> firstJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.First, onError);
		JoinObserver<T2> secondJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Second, onError);
		JoinObserver<T3> thirdJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Third, onError);
		JoinObserver<T4> fourthJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Fourth, onError);
		JoinObserver<T5> fifthJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Fifth, onError);
		JoinObserver<T6> sixthJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Sixth, onError);
		JoinObserver<T7> seventhJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Seventh, onError);
		JoinObserver<T8> eighthJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Eighth, onError);
		JoinObserver<T9> ninthJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Ninth, onError);
		JoinObserver<T10> tenthJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Tenth, onError);
		JoinObserver<T11> eleventhJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Eleventh, onError);
		JoinObserver<T12> twelfthJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Twelfth, onError);
		JoinObserver<T13> thirteenthJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Thirteenth, onError);
		JoinObserver<T14> fourteenthJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Fourteenth, onError);
		ActivePlan<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> activePlan = null;
		activePlan = new ActivePlan<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(firstJoinObserver, secondJoinObserver, thirdJoinObserver, fourthJoinObserver, fifthJoinObserver, sixthJoinObserver, seventhJoinObserver, eighthJoinObserver, ninthJoinObserver, tenthJoinObserver, eleventhJoinObserver, twelfthJoinObserver, thirteenthJoinObserver, fourteenthJoinObserver, delegate(T1 first, T2 second, T3 third, T4 fourth, T5 fifth, T6 sixth, T7 seventh, T8 eighth, T9 ninth, T10 tenth, T11 eleventh, T12 twelfth, T13 thirteenth, T14 fourteenth)
		{
			TResult value;
			try
			{
				value = Selector(first, second, third, fourth, fifth, sixth, seventh, eighth, ninth, tenth, eleventh, twelfth, thirteenth, fourteenth);
			}
			catch (Exception error)
			{
				observer.OnError(error);
				return;
			}
			observer.OnNext(value);
		}, delegate
		{
			firstJoinObserver.RemoveActivePlan(activePlan);
			secondJoinObserver.RemoveActivePlan(activePlan);
			thirdJoinObserver.RemoveActivePlan(activePlan);
			fourthJoinObserver.RemoveActivePlan(activePlan);
			fifthJoinObserver.RemoveActivePlan(activePlan);
			sixthJoinObserver.RemoveActivePlan(activePlan);
			seventhJoinObserver.RemoveActivePlan(activePlan);
			eighthJoinObserver.RemoveActivePlan(activePlan);
			ninthJoinObserver.RemoveActivePlan(activePlan);
			tenthJoinObserver.RemoveActivePlan(activePlan);
			eleventhJoinObserver.RemoveActivePlan(activePlan);
			twelfthJoinObserver.RemoveActivePlan(activePlan);
			thirteenthJoinObserver.RemoveActivePlan(activePlan);
			fourteenthJoinObserver.RemoveActivePlan(activePlan);
			deactivate(activePlan);
		});
		firstJoinObserver.AddActivePlan(activePlan);
		secondJoinObserver.AddActivePlan(activePlan);
		thirdJoinObserver.AddActivePlan(activePlan);
		fourthJoinObserver.AddActivePlan(activePlan);
		fifthJoinObserver.AddActivePlan(activePlan);
		sixthJoinObserver.AddActivePlan(activePlan);
		seventhJoinObserver.AddActivePlan(activePlan);
		eighthJoinObserver.AddActivePlan(activePlan);
		ninthJoinObserver.AddActivePlan(activePlan);
		tenthJoinObserver.AddActivePlan(activePlan);
		eleventhJoinObserver.AddActivePlan(activePlan);
		twelfthJoinObserver.AddActivePlan(activePlan);
		thirteenthJoinObserver.AddActivePlan(activePlan);
		fourteenthJoinObserver.AddActivePlan(activePlan);
		return activePlan;
	}
}
internal sealed class Plan<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult> : Plan<TResult>
{
	internal Pattern<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Expression { get; }

	internal Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult> Selector { get; }

	internal Plan(Pattern<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> expression, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult> selector)
	{
		Expression = expression;
		Selector = selector;
	}

	internal override ActivePlan Activate(Dictionary<object, IJoinObserver> externalSubscriptions, IObserver<TResult> observer, Action<ActivePlan> deactivate)
	{
		Action<Exception> onError = observer.OnError;
		JoinObserver<T1> firstJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.First, onError);
		JoinObserver<T2> secondJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Second, onError);
		JoinObserver<T3> thirdJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Third, onError);
		JoinObserver<T4> fourthJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Fourth, onError);
		JoinObserver<T5> fifthJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Fifth, onError);
		JoinObserver<T6> sixthJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Sixth, onError);
		JoinObserver<T7> seventhJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Seventh, onError);
		JoinObserver<T8> eighthJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Eighth, onError);
		JoinObserver<T9> ninthJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Ninth, onError);
		JoinObserver<T10> tenthJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Tenth, onError);
		JoinObserver<T11> eleventhJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Eleventh, onError);
		JoinObserver<T12> twelfthJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Twelfth, onError);
		JoinObserver<T13> thirteenthJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Thirteenth, onError);
		JoinObserver<T14> fourteenthJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Fourteenth, onError);
		JoinObserver<T15> fifteenthJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Fifteenth, onError);
		ActivePlan<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> activePlan = null;
		activePlan = new ActivePlan<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(firstJoinObserver, secondJoinObserver, thirdJoinObserver, fourthJoinObserver, fifthJoinObserver, sixthJoinObserver, seventhJoinObserver, eighthJoinObserver, ninthJoinObserver, tenthJoinObserver, eleventhJoinObserver, twelfthJoinObserver, thirteenthJoinObserver, fourteenthJoinObserver, fifteenthJoinObserver, delegate(T1 first, T2 second, T3 third, T4 fourth, T5 fifth, T6 sixth, T7 seventh, T8 eighth, T9 ninth, T10 tenth, T11 eleventh, T12 twelfth, T13 thirteenth, T14 fourteenth, T15 fifteenth)
		{
			TResult value;
			try
			{
				value = Selector(first, second, third, fourth, fifth, sixth, seventh, eighth, ninth, tenth, eleventh, twelfth, thirteenth, fourteenth, fifteenth);
			}
			catch (Exception error)
			{
				observer.OnError(error);
				return;
			}
			observer.OnNext(value);
		}, delegate
		{
			firstJoinObserver.RemoveActivePlan(activePlan);
			secondJoinObserver.RemoveActivePlan(activePlan);
			thirdJoinObserver.RemoveActivePlan(activePlan);
			fourthJoinObserver.RemoveActivePlan(activePlan);
			fifthJoinObserver.RemoveActivePlan(activePlan);
			sixthJoinObserver.RemoveActivePlan(activePlan);
			seventhJoinObserver.RemoveActivePlan(activePlan);
			eighthJoinObserver.RemoveActivePlan(activePlan);
			ninthJoinObserver.RemoveActivePlan(activePlan);
			tenthJoinObserver.RemoveActivePlan(activePlan);
			eleventhJoinObserver.RemoveActivePlan(activePlan);
			twelfthJoinObserver.RemoveActivePlan(activePlan);
			thirteenthJoinObserver.RemoveActivePlan(activePlan);
			fourteenthJoinObserver.RemoveActivePlan(activePlan);
			fifteenthJoinObserver.RemoveActivePlan(activePlan);
			deactivate(activePlan);
		});
		firstJoinObserver.AddActivePlan(activePlan);
		secondJoinObserver.AddActivePlan(activePlan);
		thirdJoinObserver.AddActivePlan(activePlan);
		fourthJoinObserver.AddActivePlan(activePlan);
		fifthJoinObserver.AddActivePlan(activePlan);
		sixthJoinObserver.AddActivePlan(activePlan);
		seventhJoinObserver.AddActivePlan(activePlan);
		eighthJoinObserver.AddActivePlan(activePlan);
		ninthJoinObserver.AddActivePlan(activePlan);
		tenthJoinObserver.AddActivePlan(activePlan);
		eleventhJoinObserver.AddActivePlan(activePlan);
		twelfthJoinObserver.AddActivePlan(activePlan);
		thirteenthJoinObserver.AddActivePlan(activePlan);
		fourteenthJoinObserver.AddActivePlan(activePlan);
		fifteenthJoinObserver.AddActivePlan(activePlan);
		return activePlan;
	}
}
internal sealed class Plan<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult> : Plan<TResult>
{
	internal Pattern<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Expression { get; }

	internal Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult> Selector { get; }

	internal Plan(Pattern<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> expression, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult> selector)
	{
		Expression = expression;
		Selector = selector;
	}

	internal override ActivePlan Activate(Dictionary<object, IJoinObserver> externalSubscriptions, IObserver<TResult> observer, Action<ActivePlan> deactivate)
	{
		Action<Exception> onError = observer.OnError;
		JoinObserver<T1> firstJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.First, onError);
		JoinObserver<T2> secondJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Second, onError);
		JoinObserver<T3> thirdJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Third, onError);
		JoinObserver<T4> fourthJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Fourth, onError);
		JoinObserver<T5> fifthJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Fifth, onError);
		JoinObserver<T6> sixthJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Sixth, onError);
		JoinObserver<T7> seventhJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Seventh, onError);
		JoinObserver<T8> eighthJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Eighth, onError);
		JoinObserver<T9> ninthJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Ninth, onError);
		JoinObserver<T10> tenthJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Tenth, onError);
		JoinObserver<T11> eleventhJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Eleventh, onError);
		JoinObserver<T12> twelfthJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Twelfth, onError);
		JoinObserver<T13> thirteenthJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Thirteenth, onError);
		JoinObserver<T14> fourteenthJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Fourteenth, onError);
		JoinObserver<T15> fifteenthJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Fifteenth, onError);
		JoinObserver<T16> sixteenthJoinObserver = Plan<TResult>.CreateObserver(externalSubscriptions, Expression.Sixteenth, onError);
		ActivePlan<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> activePlan = null;
		activePlan = new ActivePlan<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(firstJoinObserver, secondJoinObserver, thirdJoinObserver, fourthJoinObserver, fifthJoinObserver, sixthJoinObserver, seventhJoinObserver, eighthJoinObserver, ninthJoinObserver, tenthJoinObserver, eleventhJoinObserver, twelfthJoinObserver, thirteenthJoinObserver, fourteenthJoinObserver, fifteenthJoinObserver, sixteenthJoinObserver, delegate(T1 first, T2 second, T3 third, T4 fourth, T5 fifth, T6 sixth, T7 seventh, T8 eighth, T9 ninth, T10 tenth, T11 eleventh, T12 twelfth, T13 thirteenth, T14 fourteenth, T15 fifteenth, T16 sixteenth)
		{
			TResult value;
			try
			{
				value = Selector(first, second, third, fourth, fifth, sixth, seventh, eighth, ninth, tenth, eleventh, twelfth, thirteenth, fourteenth, fifteenth, sixteenth);
			}
			catch (Exception error)
			{
				observer.OnError(error);
				return;
			}
			observer.OnNext(value);
		}, delegate
		{
			firstJoinObserver.RemoveActivePlan(activePlan);
			secondJoinObserver.RemoveActivePlan(activePlan);
			thirdJoinObserver.RemoveActivePlan(activePlan);
			fourthJoinObserver.RemoveActivePlan(activePlan);
			fifthJoinObserver.RemoveActivePlan(activePlan);
			sixthJoinObserver.RemoveActivePlan(activePlan);
			seventhJoinObserver.RemoveActivePlan(activePlan);
			eighthJoinObserver.RemoveActivePlan(activePlan);
			ninthJoinObserver.RemoveActivePlan(activePlan);
			tenthJoinObserver.RemoveActivePlan(activePlan);
			eleventhJoinObserver.RemoveActivePlan(activePlan);
			twelfthJoinObserver.RemoveActivePlan(activePlan);
			thirteenthJoinObserver.RemoveActivePlan(activePlan);
			fourteenthJoinObserver.RemoveActivePlan(activePlan);
			fifteenthJoinObserver.RemoveActivePlan(activePlan);
			sixteenthJoinObserver.RemoveActivePlan(activePlan);
			deactivate(activePlan);
		});
		firstJoinObserver.AddActivePlan(activePlan);
		secondJoinObserver.AddActivePlan(activePlan);
		thirdJoinObserver.AddActivePlan(activePlan);
		fourthJoinObserver.AddActivePlan(activePlan);
		fifthJoinObserver.AddActivePlan(activePlan);
		sixthJoinObserver.AddActivePlan(activePlan);
		seventhJoinObserver.AddActivePlan(activePlan);
		eighthJoinObserver.AddActivePlan(activePlan);
		ninthJoinObserver.AddActivePlan(activePlan);
		tenthJoinObserver.AddActivePlan(activePlan);
		eleventhJoinObserver.AddActivePlan(activePlan);
		twelfthJoinObserver.AddActivePlan(activePlan);
		thirteenthJoinObserver.AddActivePlan(activePlan);
		fourteenthJoinObserver.AddActivePlan(activePlan);
		fifteenthJoinObserver.AddActivePlan(activePlan);
		sixteenthJoinObserver.AddActivePlan(activePlan);
		return activePlan;
	}
}
