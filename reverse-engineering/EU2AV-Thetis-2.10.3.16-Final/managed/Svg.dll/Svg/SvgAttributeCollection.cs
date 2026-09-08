using System;
using System.Collections.Generic;

namespace Svg;

public sealed class SvgAttributeCollection : Dictionary<string, object>
{
	private readonly SvgElement _owner;

	public new object this[string attributeName]
	{
		get
		{
			return GetInheritedAttribute<object>(attributeName, inherited: true);
		}
		set
		{
			if (ContainsKey(attributeName))
			{
				object a = base[attributeName];
				if (TryUnboxedCheck(a, value))
				{
					base[attributeName] = value;
					OnAttributeChanged(attributeName, value);
				}
			}
			else
			{
				base[attributeName] = value;
				OnAttributeChanged(attributeName, value);
			}
		}
	}

	public event EventHandler<AttributeEventArgs> AttributeChanged;

	public SvgAttributeCollection(SvgElement owner)
	{
		_owner = owner;
	}

	public TAttributeType GetAttribute<TAttributeType>(string attributeName, TAttributeType defaultValue = default(TAttributeType))
	{
		if (ContainsKey(attributeName) && base[attributeName] != null)
		{
			return (TAttributeType)base[attributeName];
		}
		return defaultValue;
	}

	public TAttributeType GetInheritedAttribute<TAttributeType>(string attributeName, bool inherited, TAttributeType defaultValue = default(TAttributeType))
	{
		bool flag = false;
		if (ContainsKey(attributeName))
		{
			TAttributeType val = (TAttributeType)base[attributeName];
			if (IsInheritValue(val))
			{
				flag = true;
			}
			else
			{
				if (!(val is SvgDeferredPaintServer server))
				{
					return val;
				}
				if (SvgDeferredPaintServer.TryGet<SvgPaintServer>(server, _owner) != SvgPaintServer.Inherit)
				{
					return val;
				}
				flag = true;
			}
		}
		if (inherited | flag)
		{
			object obj = _owner.Parent?.Attributes.GetInheritedAttribute<object>(attributeName, inherited);
			if (obj != null)
			{
				return (TAttributeType)obj;
			}
		}
		return defaultValue;
	}

	private bool IsInheritValue(object value)
	{
		return string.Equals(value?.ToString().Trim(), "inherit", StringComparison.OrdinalIgnoreCase);
	}

	private bool TryUnboxedCheck(object a, object b)
	{
		if (IsValueType(a))
		{
			if (a is SvgUnit)
			{
				return UnboxAndCheck<SvgUnit>(a, b);
			}
			if (a is bool)
			{
				return UnboxAndCheck<bool>(a, b);
			}
			if (a is int)
			{
				return UnboxAndCheck<int>(a, b);
			}
			if (a is float)
			{
				return UnboxAndCheck<float>(a, b);
			}
			if (a is SvgViewBox)
			{
				return UnboxAndCheck<SvgViewBox>(a, b);
			}
			return true;
		}
		return a != b;
	}

	private bool UnboxAndCheck<T>(object a, object b)
	{
		return !((T)a/*cast due to constrained. prefix*/).Equals((T)b);
	}

	private bool IsValueType(object obj)
	{
		return obj?.GetType().IsValueType ?? false;
	}

	private void OnAttributeChanged(string attribute, object value)
	{
		AttributeChanged?.Invoke(_owner, new AttributeEventArgs
		{
			Attribute = attribute,
			Value = value
		});
	}
}
