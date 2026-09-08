using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace System.Collections.Generic;

internal static class AsyncEnumerableHelpers
{
	internal struct ArrayWithLength<T>
	{
		public T[] Array;

		public int Length;
	}

	internal static async ValueTask<T[]> ToArray<T>(IAsyncEnumerable<T> source, CancellationToken cancellationToken)
	{
		ArrayWithLength<T> arrayWithLength = await ToArrayWithLength(source, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		Array.Resize(ref arrayWithLength.Array, arrayWithLength.Length);
		return arrayWithLength.Array;
	}

	internal static async ValueTask<ArrayWithLength<T>> ToArrayWithLength<T>(IAsyncEnumerable<T> source, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		ArrayWithLength<T> result = default(ArrayWithLength<T>);
		if (source is ICollection<T> { Count: var count } collection)
		{
			if (count != 0)
			{
				result.Array = new T[count];
				collection.CopyTo(result.Array, 0);
				result.Length = count;
				return result;
			}
		}
		else
		{
			{
				ConfiguredCancelableAsyncEnumerable<T>.Enumerator en = source.GetConfiguredAsyncEnumerator(cancellationToken, continueOnCapturedContext: false);
				try
				{
					if (await en.MoveNextAsync())
					{
						T[] arr = new T[4]
						{
							en.Current,
							default(T),
							default(T),
							default(T)
						};
						int count2 = 1;
						while (await en.MoveNextAsync())
						{
							if (count2 == arr.Length)
							{
								int num = count2 << 1;
								if ((uint)num > 2146435071u)
								{
									num = ((2146435071 <= count2) ? (count2 + 1) : 2146435071);
								}
								Array.Resize(ref arr, num);
							}
							arr[count2++] = en.Current;
						}
						result.Length = count2;
						result.Array = arr;
						return result;
					}
				}
				finally
				{
					IAsyncDisposable asyncDisposable = en as IAsyncDisposable;
					if (asyncDisposable != null)
					{
						await asyncDisposable.DisposeAsync();
					}
				}
			}
		}
		result.Length = 0;
		result.Array = Array.Empty<T>();
		return result;
	}

	internal static async Task<System.Linq.Set<T>> ToSet<T>(IAsyncEnumerable<T> source, IEqualityComparer<T>? comparer, CancellationToken cancellationToken)
	{
		System.Linq.Set<T> set = new System.Linq.Set<T>(comparer);
		await foreach (T item in source.WithCancellation(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
		{
			set.Add(item);
		}
		return set;
	}
}
