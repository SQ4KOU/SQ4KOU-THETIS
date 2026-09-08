using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.CodeAnalysis;

internal sealed class WrappedUserComparer<T> : IEqualityComparer<T>
{
	private readonly IEqualityComparer<T> _inner;

	public static WrappedUserComparer<T> Default { get; } = new WrappedUserComparer<T>(EqualityComparer<T>.Default);

	public WrappedUserComparer(IEqualityComparer<T> inner)
	{
		_inner = inner;
	}

	public bool Equals(T? x, T? y)
	{
		try
		{
			return _inner.Equals(x, y);
		}
		catch (Exception innerException)
		{
			throw new UserFunctionException(innerException);
		}
	}

	public int GetHashCode([DisallowNull] T obj)
	{
		try
		{
			return _inner.GetHashCode(obj);
		}
		catch (Exception innerException)
		{
			throw new UserFunctionException(innerException);
		}
	}
}
