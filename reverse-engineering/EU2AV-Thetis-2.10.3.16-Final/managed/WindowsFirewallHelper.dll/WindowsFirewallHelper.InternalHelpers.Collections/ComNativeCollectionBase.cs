using System.Collections;

namespace WindowsFirewallHelper.InternalHelpers.Collections;

internal abstract class ComNativeCollectionBase<TCollection, TValue, TKey> : ComCollectionBase<TCollection, TValue, TKey, TValue> where TCollection : IEnumerable where TValue : class
{
	protected ComNativeCollectionBase(TCollection nativeEnumerable)
		: base(nativeEnumerable)
	{
	}

	protected override TValue ConvertManagedToNative(TValue managed)
	{
		return managed;
	}

	protected override TValue ConvertNativeToManaged(TValue native)
	{
		return native;
	}
}
