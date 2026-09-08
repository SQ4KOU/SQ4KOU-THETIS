using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Roslyn.Utilities;

internal static class InterlockedOperations
{
	private static T GetOrStore<T>([NotNull] ref T? target, T value) where T : class
	{
		return Interlocked.CompareExchange(ref target, value, null) ?? value;
	}

	private static int GetOrStore(ref int target, int value, int uninitializedValue)
	{
		int num = Interlocked.CompareExchange(ref target, value, uninitializedValue);
		if (num != uninitializedValue)
		{
			return num;
		}
		return value;
	}

	public static T Initialize<T>([NotNull] ref T? target, Func<T> valueFactory) where T : class
	{
		return Volatile.Read(in target) ?? GetOrStore(ref target, valueFactory());
	}

	public static T Initialize<T, TArg>([NotNull] ref T? target, Func<TArg, T> valueFactory, TArg arg) where T : class
	{
		return Volatile.Read(in target) ?? GetOrStore(ref target, valueFactory(arg));
	}

	public static int Initialize<TArg>(ref int target, int uninitializedValue, Func<TArg, int> valueFactory, TArg arg)
	{
		int num = Volatile.Read(in target);
		if (num != uninitializedValue)
		{
			return num;
		}
		return GetOrStore(ref target, valueFactory(arg), uninitializedValue);
	}

	public static T? Initialize<T>([NotNull] ref StrongBox<T?>? target, Func<T?> valueFactory)
	{
		return (Volatile.Read(in target) ?? GetOrStore(ref target, new StrongBox<T>(valueFactory()))).Value;
	}

	public static T? Initialize<T, TArg>([NotNull] ref StrongBox<T?>? target, Func<TArg, T?> valueFactory, TArg arg)
	{
		return (Volatile.Read(in target) ?? GetOrStore(ref target, new StrongBox<T>(valueFactory(arg)))).Value;
	}

	public static T Initialize<T>([NotNull] ref T? target, T value) where T : class
	{
		return GetOrStore(ref target, value);
	}

	[return: NotNullIfNotNull("initializedValue")]
	public static T Initialize<T>(ref T target, T initializedValue, T uninitializedValue) where T : class?
	{
		T val = Interlocked.CompareExchange(ref target, initializedValue, uninitializedValue);
		if (val != uninitializedValue)
		{
			return val;
		}
		return initializedValue;
	}

	public static ImmutableArray<T> Initialize<T>(ref ImmutableArray<T> target, ImmutableArray<T> initializedValue)
	{
		ImmutableArray<T> result = ImmutableInterlocked.InterlockedCompareExchange(ref target, initializedValue, default(ImmutableArray<T>));
		if (!result.IsDefault)
		{
			return result;
		}
		return initializedValue;
	}

	public static ImmutableArray<T> Initialize<T>(ref ImmutableArray<T> target, Func<ImmutableArray<T>> createArray)
	{
		return Initialize(ref target, (Func<ImmutableArray<T>> func) => func(), createArray);
	}

	public static ImmutableArray<T> Initialize<T, TArg>(ref ImmutableArray<T> target, Func<TArg, ImmutableArray<T>> createArray, TArg arg)
	{
		if (!target.IsDefault)
		{
			return target;
		}
		return Initialize_Slow(ref target, createArray, arg);
	}

	private static ImmutableArray<T> Initialize_Slow<T, TArg>(ref ImmutableArray<T> target, Func<TArg, ImmutableArray<T>> createArray, TArg arg)
	{
		ImmutableInterlocked.Update(ref target, (ImmutableArray<T> current, (Func<TArg, ImmutableArray<T>> createArray, TArg arg) tuple) => (!current.IsDefault) ? current : tuple.createArray(tuple.arg), (createArray, arg));
		return target;
	}
}
