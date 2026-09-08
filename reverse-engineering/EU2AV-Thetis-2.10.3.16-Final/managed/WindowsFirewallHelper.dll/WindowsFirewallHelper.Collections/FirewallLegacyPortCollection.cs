using System;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using WindowsFirewallHelper.COMInterop;
using WindowsFirewallHelper.InternalHelpers.Collections;

namespace WindowsFirewallHelper.Collections;

internal class FirewallLegacyPortCollection : ComNativeCollectionBase<INetFwOpenPorts, INetFwOpenPort, FirewallLegacyPortCollectionKey>
{
	private static readonly Random Random = new Random();

	public override bool IsReadOnly { get; }

	public FirewallLegacyPortCollection(INetFwOpenPorts authorizedPortsCollection)
		: base(authorizedPortsCollection)
	{
	}

	public override bool Remove(INetFwOpenPort item)
	{
		if (item == null)
		{
			throw new ArgumentNullException("item");
		}
		int port = item.Port;
		try
		{
			item.Port = (ushort)Random.Next(10240, 65535);
			base.Remove(item);
			return true;
		}
		catch (Exception)
		{
			item.Port = port;
			throw;
		}
	}

	protected override FirewallLegacyPortCollectionKey GetCollectionKey(INetFwOpenPort managed)
	{
		return new FirewallLegacyPortCollectionKey(managed.Port, managed.Protocol);
	}

	protected override IEnumVARIANT GetEnumVariant()
	{
		return NativeEnumerable.GetEnumeratorVariant();
	}

	protected override void InternalAdd(INetFwOpenPort native)
	{
		NativeEnumerable.Add(native);
	}

	protected override int InternalCount()
	{
		return NativeEnumerable.Count;
	}

	protected override INetFwOpenPort InternalItem(FirewallLegacyPortCollectionKey key)
	{
		try
		{
			return NativeEnumerable.Item(key.PortNumber, key.ProtocolType);
		}
		catch (FileNotFoundException)
		{
			return null;
		}
	}

	protected override void InternalRemove(FirewallLegacyPortCollectionKey key)
	{
		NativeEnumerable.Remove(key.PortNumber, key.ProtocolType);
	}
}
