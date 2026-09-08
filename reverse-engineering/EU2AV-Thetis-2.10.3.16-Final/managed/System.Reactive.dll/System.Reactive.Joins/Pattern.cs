namespace System.Reactive.Joins;

public abstract class Pattern
{
	internal Pattern()
	{
	}
}
public class Pattern<TSource1> : Pattern
{
	internal IObservable<TSource1> First { get; }

	internal Pattern(IObservable<TSource1> first)
	{
		First = first;
	}

	public Plan<TResult> Then<TResult>(Func<TSource1, TResult> selector)
	{
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		return new Plan<TSource1, TResult>(this, selector);
	}
}
public class Pattern<TSource1, TSource2> : Pattern
{
	internal IObservable<TSource1> First { get; }

	internal IObservable<TSource2> Second { get; }

	internal Pattern(IObservable<TSource1> first, IObservable<TSource2> second)
	{
		First = first;
		Second = second;
	}

	public Pattern<TSource1, TSource2, TSource3> And<TSource3>(IObservable<TSource3> other)
	{
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		return new Pattern<TSource1, TSource2, TSource3>(First, Second, other);
	}

	public Plan<TResult> Then<TResult>(Func<TSource1, TSource2, TResult> selector)
	{
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		return new Plan<TSource1, TSource2, TResult>(this, selector);
	}
}
public class Pattern<TSource1, TSource2, TSource3> : Pattern
{
	internal IObservable<TSource1> First { get; }

	internal IObservable<TSource2> Second { get; }

	internal IObservable<TSource3> Third { get; }

	internal Pattern(IObservable<TSource1> first, IObservable<TSource2> second, IObservable<TSource3> third)
	{
		First = first;
		Second = second;
		Third = third;
	}

	public Pattern<TSource1, TSource2, TSource3, TSource4> And<TSource4>(IObservable<TSource4> other)
	{
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		return new Pattern<TSource1, TSource2, TSource3, TSource4>(First, Second, Third, other);
	}

	public Plan<TResult> Then<TResult>(Func<TSource1, TSource2, TSource3, TResult> selector)
	{
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		return new Plan<TSource1, TSource2, TSource3, TResult>(this, selector);
	}
}
public class Pattern<TSource1, TSource2, TSource3, TSource4> : Pattern
{
	internal IObservable<TSource1> First { get; }

	internal IObservable<TSource2> Second { get; }

	internal IObservable<TSource3> Third { get; }

	internal IObservable<TSource4> Fourth { get; }

	internal Pattern(IObservable<TSource1> first, IObservable<TSource2> second, IObservable<TSource3> third, IObservable<TSource4> fourth)
	{
		First = first;
		Second = second;
		Third = third;
		Fourth = fourth;
	}

	public Pattern<TSource1, TSource2, TSource3, TSource4, TSource5> And<TSource5>(IObservable<TSource5> other)
	{
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		return new Pattern<TSource1, TSource2, TSource3, TSource4, TSource5>(First, Second, Third, Fourth, other);
	}

	public Plan<TResult> Then<TResult>(Func<TSource1, TSource2, TSource3, TSource4, TResult> selector)
	{
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		return new Plan<TSource1, TSource2, TSource3, TSource4, TResult>(this, selector);
	}
}
public class Pattern<TSource1, TSource2, TSource3, TSource4, TSource5> : Pattern
{
	internal IObservable<TSource1> First { get; }

	internal IObservable<TSource2> Second { get; }

	internal IObservable<TSource3> Third { get; }

	internal IObservable<TSource4> Fourth { get; }

	internal IObservable<TSource5> Fifth { get; }

	internal Pattern(IObservable<TSource1> first, IObservable<TSource2> second, IObservable<TSource3> third, IObservable<TSource4> fourth, IObservable<TSource5> fifth)
	{
		First = first;
		Second = second;
		Third = third;
		Fourth = fourth;
		Fifth = fifth;
	}

	public Pattern<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6> And<TSource6>(IObservable<TSource6> other)
	{
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		return new Pattern<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6>(First, Second, Third, Fourth, Fifth, other);
	}

	public Plan<TResult> Then<TResult>(Func<TSource1, TSource2, TSource3, TSource4, TSource5, TResult> selector)
	{
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		return new Plan<TSource1, TSource2, TSource3, TSource4, TSource5, TResult>(this, selector);
	}
}
public class Pattern<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6> : Pattern
{
	internal IObservable<TSource1> First { get; }

	internal IObservable<TSource2> Second { get; }

	internal IObservable<TSource3> Third { get; }

	internal IObservable<TSource4> Fourth { get; }

	internal IObservable<TSource5> Fifth { get; }

	internal IObservable<TSource6> Sixth { get; }

	internal Pattern(IObservable<TSource1> first, IObservable<TSource2> second, IObservable<TSource3> third, IObservable<TSource4> fourth, IObservable<TSource5> fifth, IObservable<TSource6> sixth)
	{
		First = first;
		Second = second;
		Third = third;
		Fourth = fourth;
		Fifth = fifth;
		Sixth = sixth;
	}

	public Pattern<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7> And<TSource7>(IObservable<TSource7> other)
	{
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		return new Pattern<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7>(First, Second, Third, Fourth, Fifth, Sixth, other);
	}

	public Plan<TResult> Then<TResult>(Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TResult> selector)
	{
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		return new Plan<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TResult>(this, selector);
	}
}
public class Pattern<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7> : Pattern
{
	internal IObservable<TSource1> First { get; }

	internal IObservable<TSource2> Second { get; }

	internal IObservable<TSource3> Third { get; }

	internal IObservable<TSource4> Fourth { get; }

	internal IObservable<TSource5> Fifth { get; }

	internal IObservable<TSource6> Sixth { get; }

	internal IObservable<TSource7> Seventh { get; }

	internal Pattern(IObservable<TSource1> first, IObservable<TSource2> second, IObservable<TSource3> third, IObservable<TSource4> fourth, IObservable<TSource5> fifth, IObservable<TSource6> sixth, IObservable<TSource7> seventh)
	{
		First = first;
		Second = second;
		Third = third;
		Fourth = fourth;
		Fifth = fifth;
		Sixth = sixth;
		Seventh = seventh;
	}

	public Pattern<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8> And<TSource8>(IObservable<TSource8> other)
	{
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		return new Pattern<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8>(First, Second, Third, Fourth, Fifth, Sixth, Seventh, other);
	}

	public Plan<TResult> Then<TResult>(Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TResult> selector)
	{
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		return new Plan<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TResult>(this, selector);
	}
}
public class Pattern<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8> : Pattern
{
	internal IObservable<TSource1> First { get; }

	internal IObservable<TSource2> Second { get; }

	internal IObservable<TSource3> Third { get; }

	internal IObservable<TSource4> Fourth { get; }

	internal IObservable<TSource5> Fifth { get; }

	internal IObservable<TSource6> Sixth { get; }

	internal IObservable<TSource7> Seventh { get; }

	internal IObservable<TSource8> Eighth { get; }

	internal Pattern(IObservable<TSource1> first, IObservable<TSource2> second, IObservable<TSource3> third, IObservable<TSource4> fourth, IObservable<TSource5> fifth, IObservable<TSource6> sixth, IObservable<TSource7> seventh, IObservable<TSource8> eighth)
	{
		First = first;
		Second = second;
		Third = third;
		Fourth = fourth;
		Fifth = fifth;
		Sixth = sixth;
		Seventh = seventh;
		Eighth = eighth;
	}

	public Pattern<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9> And<TSource9>(IObservable<TSource9> other)
	{
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		return new Pattern<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9>(First, Second, Third, Fourth, Fifth, Sixth, Seventh, Eighth, other);
	}

	public Plan<TResult> Then<TResult>(Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TResult> selector)
	{
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		return new Plan<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TResult>(this, selector);
	}
}
public class Pattern<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9> : Pattern
{
	internal IObservable<TSource1> First { get; }

	internal IObservable<TSource2> Second { get; }

	internal IObservable<TSource3> Third { get; }

	internal IObservable<TSource4> Fourth { get; }

	internal IObservable<TSource5> Fifth { get; }

	internal IObservable<TSource6> Sixth { get; }

	internal IObservable<TSource7> Seventh { get; }

	internal IObservable<TSource8> Eighth { get; }

	internal IObservable<TSource9> Ninth { get; }

	internal Pattern(IObservable<TSource1> first, IObservable<TSource2> second, IObservable<TSource3> third, IObservable<TSource4> fourth, IObservable<TSource5> fifth, IObservable<TSource6> sixth, IObservable<TSource7> seventh, IObservable<TSource8> eighth, IObservable<TSource9> ninth)
	{
		First = first;
		Second = second;
		Third = third;
		Fourth = fourth;
		Fifth = fifth;
		Sixth = sixth;
		Seventh = seventh;
		Eighth = eighth;
		Ninth = ninth;
	}

	public Pattern<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10> And<TSource10>(IObservable<TSource10> other)
	{
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		return new Pattern<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10>(First, Second, Third, Fourth, Fifth, Sixth, Seventh, Eighth, Ninth, other);
	}

	public Plan<TResult> Then<TResult>(Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TResult> selector)
	{
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		return new Plan<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TResult>(this, selector);
	}
}
public class Pattern<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10> : Pattern
{
	internal IObservable<TSource1> First { get; }

	internal IObservable<TSource2> Second { get; }

	internal IObservable<TSource3> Third { get; }

	internal IObservable<TSource4> Fourth { get; }

	internal IObservable<TSource5> Fifth { get; }

	internal IObservable<TSource6> Sixth { get; }

	internal IObservable<TSource7> Seventh { get; }

	internal IObservable<TSource8> Eighth { get; }

	internal IObservable<TSource9> Ninth { get; }

	internal IObservable<TSource10> Tenth { get; }

	internal Pattern(IObservable<TSource1> first, IObservable<TSource2> second, IObservable<TSource3> third, IObservable<TSource4> fourth, IObservable<TSource5> fifth, IObservable<TSource6> sixth, IObservable<TSource7> seventh, IObservable<TSource8> eighth, IObservable<TSource9> ninth, IObservable<TSource10> tenth)
	{
		First = first;
		Second = second;
		Third = third;
		Fourth = fourth;
		Fifth = fifth;
		Sixth = sixth;
		Seventh = seventh;
		Eighth = eighth;
		Ninth = ninth;
		Tenth = tenth;
	}

	public Pattern<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11> And<TSource11>(IObservable<TSource11> other)
	{
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		return new Pattern<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11>(First, Second, Third, Fourth, Fifth, Sixth, Seventh, Eighth, Ninth, Tenth, other);
	}

	public Plan<TResult> Then<TResult>(Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TResult> selector)
	{
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		return new Plan<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TResult>(this, selector);
	}
}
public class Pattern<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11> : Pattern
{
	internal IObservable<TSource1> First { get; }

	internal IObservable<TSource2> Second { get; }

	internal IObservable<TSource3> Third { get; }

	internal IObservable<TSource4> Fourth { get; }

	internal IObservable<TSource5> Fifth { get; }

	internal IObservable<TSource6> Sixth { get; }

	internal IObservable<TSource7> Seventh { get; }

	internal IObservable<TSource8> Eighth { get; }

	internal IObservable<TSource9> Ninth { get; }

	internal IObservable<TSource10> Tenth { get; }

	internal IObservable<TSource11> Eleventh { get; }

	internal Pattern(IObservable<TSource1> first, IObservable<TSource2> second, IObservable<TSource3> third, IObservable<TSource4> fourth, IObservable<TSource5> fifth, IObservable<TSource6> sixth, IObservable<TSource7> seventh, IObservable<TSource8> eighth, IObservable<TSource9> ninth, IObservable<TSource10> tenth, IObservable<TSource11> eleventh)
	{
		First = first;
		Second = second;
		Third = third;
		Fourth = fourth;
		Fifth = fifth;
		Sixth = sixth;
		Seventh = seventh;
		Eighth = eighth;
		Ninth = ninth;
		Tenth = tenth;
		Eleventh = eleventh;
	}

	public Pattern<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12> And<TSource12>(IObservable<TSource12> other)
	{
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		return new Pattern<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12>(First, Second, Third, Fourth, Fifth, Sixth, Seventh, Eighth, Ninth, Tenth, Eleventh, other);
	}

	public Plan<TResult> Then<TResult>(Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TResult> selector)
	{
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		return new Plan<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TResult>(this, selector);
	}
}
public class Pattern<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12> : Pattern
{
	internal IObservable<TSource1> First { get; }

	internal IObservable<TSource2> Second { get; }

	internal IObservable<TSource3> Third { get; }

	internal IObservable<TSource4> Fourth { get; }

	internal IObservable<TSource5> Fifth { get; }

	internal IObservable<TSource6> Sixth { get; }

	internal IObservable<TSource7> Seventh { get; }

	internal IObservable<TSource8> Eighth { get; }

	internal IObservable<TSource9> Ninth { get; }

	internal IObservable<TSource10> Tenth { get; }

	internal IObservable<TSource11> Eleventh { get; }

	internal IObservable<TSource12> Twelfth { get; }

	internal Pattern(IObservable<TSource1> first, IObservable<TSource2> second, IObservable<TSource3> third, IObservable<TSource4> fourth, IObservable<TSource5> fifth, IObservable<TSource6> sixth, IObservable<TSource7> seventh, IObservable<TSource8> eighth, IObservable<TSource9> ninth, IObservable<TSource10> tenth, IObservable<TSource11> eleventh, IObservable<TSource12> twelfth)
	{
		First = first;
		Second = second;
		Third = third;
		Fourth = fourth;
		Fifth = fifth;
		Sixth = sixth;
		Seventh = seventh;
		Eighth = eighth;
		Ninth = ninth;
		Tenth = tenth;
		Eleventh = eleventh;
		Twelfth = twelfth;
	}

	public Pattern<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13> And<TSource13>(IObservable<TSource13> other)
	{
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		return new Pattern<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13>(First, Second, Third, Fourth, Fifth, Sixth, Seventh, Eighth, Ninth, Tenth, Eleventh, Twelfth, other);
	}

	public Plan<TResult> Then<TResult>(Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TResult> selector)
	{
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		return new Plan<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TResult>(this, selector);
	}
}
public class Pattern<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13> : Pattern
{
	internal IObservable<TSource1> First { get; }

	internal IObservable<TSource2> Second { get; }

	internal IObservable<TSource3> Third { get; }

	internal IObservable<TSource4> Fourth { get; }

	internal IObservable<TSource5> Fifth { get; }

	internal IObservable<TSource6> Sixth { get; }

	internal IObservable<TSource7> Seventh { get; }

	internal IObservable<TSource8> Eighth { get; }

	internal IObservable<TSource9> Ninth { get; }

	internal IObservable<TSource10> Tenth { get; }

	internal IObservable<TSource11> Eleventh { get; }

	internal IObservable<TSource12> Twelfth { get; }

	internal IObservable<TSource13> Thirteenth { get; }

	internal Pattern(IObservable<TSource1> first, IObservable<TSource2> second, IObservable<TSource3> third, IObservable<TSource4> fourth, IObservable<TSource5> fifth, IObservable<TSource6> sixth, IObservable<TSource7> seventh, IObservable<TSource8> eighth, IObservable<TSource9> ninth, IObservable<TSource10> tenth, IObservable<TSource11> eleventh, IObservable<TSource12> twelfth, IObservable<TSource13> thirteenth)
	{
		First = first;
		Second = second;
		Third = third;
		Fourth = fourth;
		Fifth = fifth;
		Sixth = sixth;
		Seventh = seventh;
		Eighth = eighth;
		Ninth = ninth;
		Tenth = tenth;
		Eleventh = eleventh;
		Twelfth = twelfth;
		Thirteenth = thirteenth;
	}

	public Pattern<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TSource14> And<TSource14>(IObservable<TSource14> other)
	{
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		return new Pattern<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TSource14>(First, Second, Third, Fourth, Fifth, Sixth, Seventh, Eighth, Ninth, Tenth, Eleventh, Twelfth, Thirteenth, other);
	}

	public Plan<TResult> Then<TResult>(Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TResult> selector)
	{
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		return new Plan<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TResult>(this, selector);
	}
}
public class Pattern<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TSource14> : Pattern
{
	internal IObservable<TSource1> First { get; }

	internal IObservable<TSource2> Second { get; }

	internal IObservable<TSource3> Third { get; }

	internal IObservable<TSource4> Fourth { get; }

	internal IObservable<TSource5> Fifth { get; }

	internal IObservable<TSource6> Sixth { get; }

	internal IObservable<TSource7> Seventh { get; }

	internal IObservable<TSource8> Eighth { get; }

	internal IObservable<TSource9> Ninth { get; }

	internal IObservable<TSource10> Tenth { get; }

	internal IObservable<TSource11> Eleventh { get; }

	internal IObservable<TSource12> Twelfth { get; }

	internal IObservable<TSource13> Thirteenth { get; }

	internal IObservable<TSource14> Fourteenth { get; }

	internal Pattern(IObservable<TSource1> first, IObservable<TSource2> second, IObservable<TSource3> third, IObservable<TSource4> fourth, IObservable<TSource5> fifth, IObservable<TSource6> sixth, IObservable<TSource7> seventh, IObservable<TSource8> eighth, IObservable<TSource9> ninth, IObservable<TSource10> tenth, IObservable<TSource11> eleventh, IObservable<TSource12> twelfth, IObservable<TSource13> thirteenth, IObservable<TSource14> fourteenth)
	{
		First = first;
		Second = second;
		Third = third;
		Fourth = fourth;
		Fifth = fifth;
		Sixth = sixth;
		Seventh = seventh;
		Eighth = eighth;
		Ninth = ninth;
		Tenth = tenth;
		Eleventh = eleventh;
		Twelfth = twelfth;
		Thirteenth = thirteenth;
		Fourteenth = fourteenth;
	}

	public Pattern<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TSource14, TSource15> And<TSource15>(IObservable<TSource15> other)
	{
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		return new Pattern<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TSource14, TSource15>(First, Second, Third, Fourth, Fifth, Sixth, Seventh, Eighth, Ninth, Tenth, Eleventh, Twelfth, Thirteenth, Fourteenth, other);
	}

	public Plan<TResult> Then<TResult>(Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TSource14, TResult> selector)
	{
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		return new Plan<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TSource14, TResult>(this, selector);
	}
}
public class Pattern<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TSource14, TSource15> : Pattern
{
	internal IObservable<TSource1> First { get; }

	internal IObservable<TSource2> Second { get; }

	internal IObservable<TSource3> Third { get; }

	internal IObservable<TSource4> Fourth { get; }

	internal IObservable<TSource5> Fifth { get; }

	internal IObservable<TSource6> Sixth { get; }

	internal IObservable<TSource7> Seventh { get; }

	internal IObservable<TSource8> Eighth { get; }

	internal IObservable<TSource9> Ninth { get; }

	internal IObservable<TSource10> Tenth { get; }

	internal IObservable<TSource11> Eleventh { get; }

	internal IObservable<TSource12> Twelfth { get; }

	internal IObservable<TSource13> Thirteenth { get; }

	internal IObservable<TSource14> Fourteenth { get; }

	internal IObservable<TSource15> Fifteenth { get; }

	internal Pattern(IObservable<TSource1> first, IObservable<TSource2> second, IObservable<TSource3> third, IObservable<TSource4> fourth, IObservable<TSource5> fifth, IObservable<TSource6> sixth, IObservable<TSource7> seventh, IObservable<TSource8> eighth, IObservable<TSource9> ninth, IObservable<TSource10> tenth, IObservable<TSource11> eleventh, IObservable<TSource12> twelfth, IObservable<TSource13> thirteenth, IObservable<TSource14> fourteenth, IObservable<TSource15> fifteenth)
	{
		First = first;
		Second = second;
		Third = third;
		Fourth = fourth;
		Fifth = fifth;
		Sixth = sixth;
		Seventh = seventh;
		Eighth = eighth;
		Ninth = ninth;
		Tenth = tenth;
		Eleventh = eleventh;
		Twelfth = twelfth;
		Thirteenth = thirteenth;
		Fourteenth = fourteenth;
		Fifteenth = fifteenth;
	}

	public Pattern<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TSource14, TSource15, TSource16> And<TSource16>(IObservable<TSource16> other)
	{
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		return new Pattern<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TSource14, TSource15, TSource16>(First, Second, Third, Fourth, Fifth, Sixth, Seventh, Eighth, Ninth, Tenth, Eleventh, Twelfth, Thirteenth, Fourteenth, Fifteenth, other);
	}

	public Plan<TResult> Then<TResult>(Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TSource14, TSource15, TResult> selector)
	{
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		return new Plan<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TSource14, TSource15, TResult>(this, selector);
	}
}
public class Pattern<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TSource14, TSource15, TSource16> : Pattern
{
	internal IObservable<TSource1> First { get; }

	internal IObservable<TSource2> Second { get; }

	internal IObservable<TSource3> Third { get; }

	internal IObservable<TSource4> Fourth { get; }

	internal IObservable<TSource5> Fifth { get; }

	internal IObservable<TSource6> Sixth { get; }

	internal IObservable<TSource7> Seventh { get; }

	internal IObservable<TSource8> Eighth { get; }

	internal IObservable<TSource9> Ninth { get; }

	internal IObservable<TSource10> Tenth { get; }

	internal IObservable<TSource11> Eleventh { get; }

	internal IObservable<TSource12> Twelfth { get; }

	internal IObservable<TSource13> Thirteenth { get; }

	internal IObservable<TSource14> Fourteenth { get; }

	internal IObservable<TSource15> Fifteenth { get; }

	internal IObservable<TSource16> Sixteenth { get; }

	internal Pattern(IObservable<TSource1> first, IObservable<TSource2> second, IObservable<TSource3> third, IObservable<TSource4> fourth, IObservable<TSource5> fifth, IObservable<TSource6> sixth, IObservable<TSource7> seventh, IObservable<TSource8> eighth, IObservable<TSource9> ninth, IObservable<TSource10> tenth, IObservable<TSource11> eleventh, IObservable<TSource12> twelfth, IObservable<TSource13> thirteenth, IObservable<TSource14> fourteenth, IObservable<TSource15> fifteenth, IObservable<TSource16> sixteenth)
	{
		First = first;
		Second = second;
		Third = third;
		Fourth = fourth;
		Fifth = fifth;
		Sixth = sixth;
		Seventh = seventh;
		Eighth = eighth;
		Ninth = ninth;
		Tenth = tenth;
		Eleventh = eleventh;
		Twelfth = twelfth;
		Thirteenth = thirteenth;
		Fourteenth = fourteenth;
		Fifteenth = fifteenth;
		Sixteenth = sixteenth;
	}

	public Plan<TResult> Then<TResult>(Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TSource14, TSource15, TSource16, TResult> selector)
	{
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		return new Plan<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TSource14, TSource15, TSource16, TResult>(this, selector);
	}
}
