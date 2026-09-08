using System.Collections;
using System.Collections.Generic;

namespace System.Reactive;

internal sealed class AnonymousEnumerable<T> : IEnumerable<T>, IEnumerable
{
	private readonly Func<IEnumerator<T>> _getEnumerator;

	public AnonymousEnumerable(Func<IEnumerator<T>> getEnumerator)
	{
		_getEnumerator = getEnumerator;
	}

	public IEnumerator<T> GetEnumerator()
	{
		return _getEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return _getEnumerator();
	}
}
