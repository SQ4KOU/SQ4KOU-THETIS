using System;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using WindowsFirewallHelper.COMInterop;
using WindowsFirewallHelper.InternalHelpers.Collections;

namespace WindowsFirewallHelper.Collections;

internal class FirewallLegacyApplicationCollection : ComNativeCollectionBase<INetFwAuthorizedApplications, INetFwAuthorizedApplication, string>
{
	public override bool IsReadOnly { get; }

	public FirewallLegacyApplicationCollection(INetFwAuthorizedApplications authorizedApplicationsCollection)
		: base(authorizedApplicationsCollection)
	{
	}

	public override bool Remove(INetFwAuthorizedApplication item)
	{
		if (item == null)
		{
			throw new ArgumentNullException("item");
		}
		string processImageFileName = item.ProcessImageFileName;
		string tempFileName = Path.GetTempFileName();
		try
		{
			item.ProcessImageFileName = tempFileName;
			base.Remove(item);
			return true;
		}
		catch (Exception)
		{
			item.ProcessImageFileName = processImageFileName;
			throw;
		}
		finally
		{
			if (File.Exists(tempFileName))
			{
				File.Delete(tempFileName);
			}
		}
	}

	protected override string GetCollectionKey(INetFwAuthorizedApplication managed)
	{
		return managed.ProcessImageFileName;
	}

	protected override IEnumVARIANT GetEnumVariant()
	{
		return NativeEnumerable.GetEnumeratorVariant();
	}

	protected override void InternalAdd(INetFwAuthorizedApplication native)
	{
		NativeEnumerable.Add(native);
	}

	protected override int InternalCount()
	{
		return NativeEnumerable.Count;
	}

	protected override INetFwAuthorizedApplication InternalItem(string key)
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

	protected override void InternalRemove(string key)
	{
		NativeEnumerable.Remove(key);
	}
}
