using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Roslyn.Utilities;

internal static class Hash
{
	internal const int FnvOffsetBias = -2128831035;

	internal const int FnvPrime = 16777619;

	internal static int Combine(int newKey, int currentKey)
	{
		return currentKey * -1521134295 + newKey;
	}

	internal static int Combine(bool newKeyPart, int currentKey)
	{
		return Combine(currentKey, newKeyPart ? 1 : 0);
	}

	internal static int Combine<T>(T newKeyPart, int currentKey) where T : class?
	{
		int num = currentKey * -1521134295;
		if (newKeyPart != null)
		{
			return num + newKeyPart.GetHashCode();
		}
		return num;
	}

	internal static int CombineValues<T>(IEnumerable<T>? values, int maxItemsToHash = int.MaxValue)
	{
		if (values == null)
		{
			return 0;
		}
		int num = 0;
		int num2 = 0;
		foreach (T value in values)
		{
			if (num2++ >= maxItemsToHash)
			{
				break;
			}
			if (value != null)
			{
				num = Combine(value.GetHashCode(), num);
			}
		}
		return num;
	}

	internal static int CombineValues<TKey, TValue>(ImmutableDictionary<TKey, TValue> values, int maxItemsToHash = int.MaxValue) where TKey : notnull
	{
		if (values == null)
		{
			return 0;
		}
		int num = 0;
		int num2 = 0;
		foreach (KeyValuePair<TKey, TValue> value in values)
		{
			if (num2++ >= maxItemsToHash)
			{
				break;
			}
			num = Combine(value.GetHashCode(), num);
		}
		return num;
	}

	internal static int CombineValues<T>(T[]? values, int maxItemsToHash = int.MaxValue)
	{
		if (values == null)
		{
			return 0;
		}
		int num = Math.Min(maxItemsToHash, values.Length);
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			T val = values[i];
			if (val != null)
			{
				num2 = Combine(val.GetHashCode(), num2);
			}
		}
		return num2;
	}

	internal static int CombineValues<T>(ImmutableArray<T> values, int maxItemsToHash = int.MaxValue)
	{
		if (values.IsDefaultOrEmpty)
		{
			return 0;
		}
		int num = 0;
		int num2 = 0;
		foreach (T item in values)
		{
			if (num2++ >= maxItemsToHash)
			{
				break;
			}
			if (item != null)
			{
				num = Combine(item.GetHashCode(), num);
			}
		}
		return num;
	}

	internal static int CombineValues(IEnumerable<string?>? values, StringComparer stringComparer, int maxItemsToHash = int.MaxValue)
	{
		if (values == null)
		{
			return 0;
		}
		int num = 0;
		int num2 = 0;
		foreach (string value in values)
		{
			if (num2++ >= maxItemsToHash)
			{
				break;
			}
			if (value != null)
			{
				num = Combine(stringComparer.GetHashCode(value), num);
			}
		}
		return num;
	}

	internal static int CombineValues(ImmutableArray<string> values, StringComparer stringComparer, int maxItemsToHash = int.MaxValue)
	{
		if (values == null)
		{
			return 0;
		}
		int num = 0;
		int num2 = 0;
		foreach (string item in values)
		{
			if (num2++ >= maxItemsToHash)
			{
				break;
			}
			if (item != null)
			{
				num = Combine(stringComparer.GetHashCode(item), num);
			}
		}
		return num;
	}

	internal static int GetFNVHashCode(byte[] data)
	{
		int num = -2128831035;
		for (int i = 0; i < data.Length; i++)
		{
			num = (num ^ data[i]) * 16777619;
		}
		return num;
	}

	internal static int GetFNVHashCode(ReadOnlySpan<byte> data, out bool isAscii)
	{
		int num = -2128831035;
		byte b = 0;
		for (int i = 0; i < data.Length; i++)
		{
			byte b2 = data[i];
			b |= b2;
			num = (num ^ b2) * 16777619;
		}
		isAscii = (b & 0x80) == 0;
		return num;
	}

	internal static int GetFNVHashCode(ImmutableArray<byte> data)
	{
		int num = -2128831035;
		for (int i = 0; i < data.Length; i++)
		{
			num = (num ^ data[i]) * 16777619;
		}
		return num;
	}

	internal static int GetFNVHashCode(ReadOnlySpan<char> data)
	{
		return CombineFNVHash(-2128831035, data);
	}

	internal static int GetFNVHashCode(string text, int start, int length)
	{
		return GetFNVHashCode(System.MemoryExtensions.AsSpan(text, start, length));
	}

	internal static int GetCaseInsensitiveFNVHashCode(string text)
	{
		return GetCaseInsensitiveFNVHashCode(System.MemoryExtensions.AsSpan(text));
	}

	internal static int GetCaseInsensitiveFNVHashCode(ReadOnlySpan<char> data)
	{
		int num = -2128831035;
		for (int i = 0; i < data.Length; i++)
		{
			num = (num ^ CaseInsensitiveComparison.ToLower(data[i])) * 16777619;
		}
		return num;
	}

	internal static int GetFNVHashCode(string text)
	{
		return CombineFNVHash(-2128831035, text);
	}

	internal static int GetFNVHashCode(StringBuilder text)
	{
		int num = -2128831035;
		int length = text.Length;
		for (int i = 0; i < length; i++)
		{
			num = (num ^ text[i]) * 16777619;
		}
		return num;
	}

	internal static int GetFNVHashCode(char[] text, int start, int length)
	{
		return GetFNVHashCode(System.MemoryExtensions.AsSpan(text, start, length));
	}

	internal static int GetFNVHashCode(char ch)
	{
		return CombineFNVHash(-2128831035, ch);
	}

	internal static int CombineFNVHash(int hashCode, string text)
	{
		return CombineFNVHash(hashCode, System.MemoryExtensions.AsSpan(text));
	}

	internal static int CombineFNVHash(int hashCode, char ch)
	{
		return (hashCode ^ ch) * 16777619;
	}

	internal static int CombineFNVHash(int hashCode, ReadOnlySpan<char> data)
	{
		for (int i = 0; i < data.Length; i++)
		{
			hashCode = (hashCode ^ data[i]) * 16777619;
		}
		return hashCode;
	}
}
