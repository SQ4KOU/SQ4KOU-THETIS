using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace HtmlAgilityPack;

internal static class Utilities
{
	public static TValue GetDictionaryValueOrDefault<TKey, TValue>(Dictionary<TKey, TValue> dict, TKey key, TValue defaultValue = default(TValue)) where TKey : class
	{
		if (!dict.TryGetValue(key, out var value))
		{
			return defaultValue;
		}
		return value;
	}

	internal static object To(this object @this, Type type)
	{
		if (@this != null)
		{
			if (@this.GetType() == type)
			{
				return @this;
			}
			TypeConverter converter = TypeDescriptor.GetConverter(@this);
			if (converter != null && converter.CanConvertTo(type))
			{
				return converter.ConvertTo(@this, type);
			}
			converter = TypeDescriptor.GetConverter(type);
			if (converter != null && converter.CanConvertFrom(@this.GetType()))
			{
				return converter.ConvertFrom(@this);
			}
			if (@this == DBNull.Value)
			{
				return null;
			}
		}
		return @this;
	}
}
