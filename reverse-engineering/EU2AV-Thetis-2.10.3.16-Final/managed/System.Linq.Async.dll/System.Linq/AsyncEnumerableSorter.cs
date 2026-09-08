using System.Threading.Tasks;

namespace System.Linq;

internal abstract class AsyncEnumerableSorter<TElement>
{
	internal abstract ValueTask ComputeKeys(TElement[] elements, int count);

	internal abstract int CompareAnyKeys(int index1, int index2);

	public async ValueTask<int[]> Sort(TElement[] elements, int count)
	{
		int[] array = await ComputeMap(elements, count).ConfigureAwait(continueOnCapturedContext: false);
		QuickSort(array, 0, count - 1);
		return array;
	}

	public async ValueTask<int[]> Sort(TElement[] elements, int count, int minIndexInclusive, int maxIndexInclusive)
	{
		int[] array = await ComputeMap(elements, count).ConfigureAwait(continueOnCapturedContext: false);
		PartialQuickSort(array, 0, count - 1, minIndexInclusive, maxIndexInclusive);
		return array;
	}

	public async ValueTask<TElement> ElementAt(TElement[] elements, int count, int index)
	{
		int[] map = await ComputeMap(elements, count).ConfigureAwait(continueOnCapturedContext: false);
		return (index == 0) ? elements[Min(map, count)] : elements[QuickSelect(map, count - 1, index)];
	}

	private async ValueTask<int[]> ComputeMap(TElement[] elements, int count)
	{
		await ComputeKeys(elements, count).ConfigureAwait(continueOnCapturedContext: false);
		int[] array = new int[count];
		for (int i = 0; i < count; i++)
		{
			array[i] = i;
		}
		return array;
	}

	protected abstract void QuickSort(int[] map, int left, int right);

	protected abstract void PartialQuickSort(int[] map, int left, int right, int minIndexInclusive, int maxIndexInclusive);

	protected abstract int QuickSelect(int[] map, int right, int idx);

	protected abstract int Min(int[] map, int count);
}
