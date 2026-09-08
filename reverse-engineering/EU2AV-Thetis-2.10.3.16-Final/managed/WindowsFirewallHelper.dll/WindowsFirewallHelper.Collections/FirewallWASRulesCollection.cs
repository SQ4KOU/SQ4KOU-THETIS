using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using WindowsFirewallHelper.COMInterop;
using WindowsFirewallHelper.FirewallRules;
using WindowsFirewallHelper.InternalHelpers.Collections;

namespace WindowsFirewallHelper.Collections;

internal class FirewallWASRulesCollection<TManaged> : ComCollectionBase<INetFwRules, INetFwRule, string, TManaged>, IFirewallWASRulesCollection<TManaged>, ICollection<TManaged>, IEnumerable<TManaged>, IEnumerable where TManaged : class, IFirewallRule
{
	public override bool IsReadOnly { get; }

	public new FirewallWASRule this[string name] => base[name] as FirewallWASRule;

	public FirewallWASRulesCollection(INetFwRules rulesCollection)
		: base(rulesCollection)
	{
	}

	public override bool Remove(TManaged item)
	{
		if (item == null)
		{
			throw new ArgumentNullException("item");
		}
		string name = item.Name;
		try
		{
			item.Name = Guid.NewGuid().ToString("N");
			base.Remove(item);
			return true;
		}
		catch
		{
			item.Name = name;
			throw;
		}
	}

	protected override INetFwRule ConvertManagedToNative(TManaged managed)
	{
		if (!(managed is FirewallWASRule))
		{
			throw new ArgumentException("Passed argument is invalid.", "managed");
		}
		return (managed as FirewallWASRule).GetCOMObject();
	}

	protected override TManaged ConvertNativeToManaged(INetFwRule native)
	{
		if (native == null)
		{
			return null;
		}
		if (!(native is INetFwRule3 rule))
		{
			if (native is INetFwRule2 rule2)
			{
				return new FirewallWASRuleWin7(rule2) as TManaged;
			}
			return new FirewallWASRule(native) as TManaged;
		}
		return new FirewallWASRuleWin8(rule) as TManaged;
	}

	protected override string GetCollectionKey(TManaged managed)
	{
		return ConvertManagedToNative(managed).Name;
	}

	protected override IEnumVARIANT GetEnumVariant()
	{
		return NativeEnumerable.GetEnumeratorVariant();
	}

	protected override void InternalAdd(INetFwRule native)
	{
		NativeEnumerable.Add(native);
	}

	protected override int InternalCount()
	{
		return NativeEnumerable.Count;
	}

	protected override INetFwRule InternalItem(string key)
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
