using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.CodeAnalysis;

[DebuggerDisplay("{Value,nq}")]
internal struct ArrayElement<T>
{
	internal T Value;

	public static implicit operator T(ArrayElement<T> element)
	{
		return element.Value;
	}

	[return: NotNullIfNotNull("items")]
	public static ArrayElement<T>[]? MakeElementArray(T[]? items)
	{
		if (items == null)
		{
			return null;
		}
		ArrayElement<T>[] array = new ArrayElement<T>[items.Length];
		for (int i = 0; i < items.Length; i++)
		{
			array[i].Value = items[i];
		}
		return array;
	}

	[return: NotNullIfNotNull("items")]
	public static T[]? MakeArray(ArrayElement<T>[]? items)
	{
		if (items == null)
		{
			return null;
		}
		T[] array = new T[items.Length];
		for (int i = 0; i < items.Length; i++)
		{
			array[i] = items[i].Value;
		}
		return array;
	}
}
