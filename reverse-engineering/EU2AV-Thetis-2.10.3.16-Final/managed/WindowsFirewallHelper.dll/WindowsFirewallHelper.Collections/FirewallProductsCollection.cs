using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using WindowsFirewallHelper.COMInterop;
using WindowsFirewallHelper.InternalHelpers.Collections;

namespace WindowsFirewallHelper.Collections;

internal class FirewallProductsCollection(INetFwProducts nativeEnumerable) : ComCollectionBase<INetFwProducts, INetFwProduct, int, FirewallProduct>(nativeEnumerable), IFirewallProductsCollection, ICollection<FirewallProduct>, IEnumerable<FirewallProduct>, IEnumerable
{
	public override bool IsReadOnly { get; } = true;

	public int IndexOf(FirewallProduct product)
	{
		int num = 0;
		using (IEnumerator<FirewallProduct> enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.Equals(product))
				{
					return num;
				}
				num++;
			}
		}
		return -1;
	}

	protected override INetFwProduct ConvertManagedToNative(FirewallProduct managed)
	{
		return managed.GetCOMObject();
	}

	protected override FirewallProduct ConvertNativeToManaged(INetFwProduct native)
	{
		return new FirewallProduct(native);
	}

	protected override int GetCollectionKey(FirewallProduct managed)
	{
		throw new InvalidOperationException();
	}

	protected override IEnumVARIANT GetEnumVariant()
	{
		return NativeEnumerable.GetEnumeratorVariant();
	}

	protected override void InternalAdd(INetFwProduct native)
	{
		throw new InvalidOperationException();
	}

	protected override int InternalCount()
	{
		return NativeEnumerable.Count;
	}

	protected override INetFwProduct InternalItem(int key)
	{
		try
		{
			return NativeEnumerable.Item(key);
		}
		catch (FileNotFoundException)
		{
			return null;
		}
	}

	protected override void InternalRemove(int key)
	{
		throw new InvalidOperationException();
	}
}
