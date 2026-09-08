using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.Scripting;

internal static class ParameterValidationHelpers
{
	internal static ImmutableArray<T> CheckImmutableArray<T>(ImmutableArray<T> items, string parameterName)
	{
		if (items.IsDefault)
		{
			throw new ArgumentNullException(parameterName);
		}
		for (int i = 0; i < items.Length; i++)
		{
			if (items[i] == null)
			{
				throw new ArgumentNullException($"{parameterName}[{i}]");
			}
		}
		return items;
	}

	internal static ImmutableArray<T> ToImmutableArrayChecked<T>(IEnumerable<T> items, string parameterName) where T : class
	{
		ArrayBuilder<T> instance = ArrayBuilder<T>.GetInstance();
		AddRangeChecked(instance, items, parameterName);
		return instance.ToImmutableAndFree();
	}

	internal static ImmutableArray<T> ConcatChecked<T>(ImmutableArray<T> existing, IEnumerable<T> items, string parameterName) where T : class
	{
		ArrayBuilder<T> instance = ArrayBuilder<T>.GetInstance();
		instance.AddRange(existing);
		AddRangeChecked(instance, items, parameterName);
		return instance.ToImmutableAndFree();
	}

	internal static void AddRangeChecked<T>(ArrayBuilder<T> builder, IEnumerable<T> items, string parameterName) where T : class
	{
		RequireNonNull(items, parameterName);
		foreach (T item in items)
		{
			if (item == null)
			{
				throw new ArgumentNullException($"{parameterName}[{builder.Count}]");
			}
			builder.Add(item);
		}
	}

	internal static IEnumerable<S> SelectChecked<T, S>(IEnumerable<T> items, string parameterName, Func<T, S> selector) where T : class where S : class
	{
		RequireNonNull(items, parameterName);
		return items.Select((T item) => (item == null) ? null : selector(item));
	}

	internal static void RequireNonNull<T>(IEnumerable<T> items, string parameterName)
	{
		if (items == null || items is ImmutableArray<T> { IsDefault: not false })
		{
			throw new ArgumentNullException(parameterName);
		}
	}
}
