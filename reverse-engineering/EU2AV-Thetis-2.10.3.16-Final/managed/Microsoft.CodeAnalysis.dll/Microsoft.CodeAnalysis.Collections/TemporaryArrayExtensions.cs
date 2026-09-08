using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Microsoft.CodeAnalysis.Collections;

internal static class TemporaryArrayExtensions
{
	public static ref TemporaryArray<T> AsRef<T>(this in TemporaryArray<T> array)
	{
		return ref Unsafe.AsRef(in array);
	}

	public static bool Any<T>(this in TemporaryArray<T> array, Func<T, bool> predicate)
	{
		foreach (T item in array)
		{
			if (predicate(item))
			{
				return true;
			}
		}
		return false;
	}

	public static bool All<T>(this in TemporaryArray<T> array, Func<T, bool> predicate)
	{
		foreach (T item in array)
		{
			if (!predicate(item))
			{
				return false;
			}
		}
		return true;
	}

	private static void ThrowSequenceContainsMoreThanOneElement()
	{
		new int[2].Single();
	}

	public static T? SingleOrDefault<T>(this in TemporaryArray<T> array, Func<T, bool> predicate)
	{
		bool flag = true;
		T result = default(T);
		foreach (T item in array)
		{
			if (predicate(item))
			{
				if (!flag)
				{
					ThrowSequenceContainsMoreThanOneElement();
				}
				flag = false;
				result = item;
			}
		}
		return result;
	}

	public static T? SingleOrDefault<T, TArg>(this in TemporaryArray<T> array, Func<T, TArg, bool> predicate, TArg arg)
	{
		bool flag = true;
		T result = default(T);
		foreach (T item in array)
		{
			if (predicate(item, arg))
			{
				if (!flag)
				{
					ThrowSequenceContainsMoreThanOneElement();
				}
				flag = false;
				result = item;
			}
		}
		return result;
	}

	public static T? FirstOrDefault<T>(this in TemporaryArray<T> array)
	{
		if (array.Count <= 0)
		{
			return default(T);
		}
		return array[0];
	}

	public static T? FirstOrDefault<T, TArg>(this in TemporaryArray<T> array, Func<T, TArg, bool> predicate, TArg arg)
	{
		foreach (T item in array)
		{
			if (predicate(item, arg))
			{
				return item;
			}
		}
		return default(T);
	}

	public static int IndexOf<T, TArg>(this in TemporaryArray<T> array, Func<T, TArg, bool> predicate, TArg arg)
	{
		int num = 0;
		foreach (T item in array)
		{
			if (predicate(item, arg))
			{
				return num;
			}
			num++;
		}
		return -1;
	}

	public static void AddIfNotNull<T>(this ref TemporaryArray<T> array, T? value) where T : struct
	{
		if (value.HasValue)
		{
			array.Add(value.Value);
		}
	}

	public static void AddIfNotNull<T>(this ref TemporaryArray<T> array, T? value) where T : class
	{
		if (value != null)
		{
			array.Add(value);
		}
	}
}
