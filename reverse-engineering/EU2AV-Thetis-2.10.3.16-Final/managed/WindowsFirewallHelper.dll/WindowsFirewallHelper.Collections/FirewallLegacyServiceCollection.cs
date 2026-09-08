using System;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using WindowsFirewallHelper.COMInterop;
using WindowsFirewallHelper.InternalHelpers.Collections;

namespace WindowsFirewallHelper.Collections;

internal class FirewallLegacyServiceCollection(INetFwServices servicesCollection) : ComNativeCollectionBase<INetFwServices, INetFwService, NetFwServiceType>(servicesCollection)
{
	public override bool IsReadOnly { get; } = true;

	protected override NetFwServiceType GetCollectionKey(INetFwService managed)
	{
		return managed.Type;
	}

	protected override IEnumVARIANT GetEnumVariant()
	{
		return NativeEnumerable.GetEnumeratorVariant();
	}

	protected override void InternalAdd(INetFwService native)
	{
		throw new InvalidOperationException();
	}

	protected override int InternalCount()
	{
		return NativeEnumerable.Count;
	}

	protected override INetFwService InternalItem(NetFwServiceType key)
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

	protected override void InternalRemove(NetFwServiceType key)
	{
		throw new InvalidOperationException();
	}
}
