using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace Microsoft.NET.StringTools;

internal sealed class WeakStringCache : IDisposable
{
	public struct DebugInfo
	{
		public int LiveStringCount;

		public int CollectedStringCount;
	}

	private class StringWeakHandle
	{
		private GCHandle weakHandle;

		public bool IsUsed
		{
			get
			{
				if (weakHandle.IsAllocated)
				{
					return weakHandle.Target != null;
				}
				return false;
			}
		}

		public string? GetString(ref InternableString internable)
		{
			if (!weakHandle.IsAllocated)
			{
				return null;
			}
			if (!(weakHandle.Target is string text))
			{
				return null;
			}
			if (internable.Equals(text))
			{
				return text;
			}
			return null;
		}

		public void SetString(string str)
		{
			if (weakHandle.IsAllocated)
			{
				weakHandle.Target = str;
			}
			else
			{
				weakHandle = GCHandle.Alloc(str, GCHandleType.Weak);
			}
		}

		public void Free()
		{
			if (weakHandle.IsAllocated)
			{
				weakHandle.Free();
			}
		}
	}

	private const int WeakHandleMinimumLength = 500;

	private readonly ConcurrentDictionary<int, string> _stringsByHashCode;

	private readonly ConcurrentDictionary<int, StringWeakHandle> _weakHandlesByHashCode;

	private int _count;

	private const int _initialCapacity = 503;

	private int _scavengeThreshold = 503;

	public WeakStringCache()
	{
		_stringsByHashCode = new ConcurrentDictionary<int, string>(Environment.ProcessorCount, 503);
		_weakHandlesByHashCode = new ConcurrentDictionary<int, StringWeakHandle>(Environment.ProcessorCount, 503);
	}

	public string GetOrCreateEntry(ref InternableString internable, out bool cacheHit)
	{
		int hashCode = internable.GetHashCode();
		if (internable.Length <= 500)
		{
			return GetString(ref internable, out cacheHit, hashCode);
		}
		return GetStringFromWeakHandle(ref internable, out cacheHit, hashCode);
		string GetString(ref InternableString reference, out bool reference2, int key)
		{
			ConcurrentDictionary<int, string> stringsByHashCode = _stringsByHashCode;
			if (stringsByHashCode.TryGetValue(key, out var value) && reference.Equals(value))
			{
				reference2 = true;
				return value;
			}
			reference2 = false;
			return stringsByHashCode[key] = reference.ExpensiveConvertToString();
		}
		string GetStringFromWeakHandle(ref InternableString reference, out bool reference2, int key)
		{
			ConcurrentDictionary<int, StringWeakHandle> weakHandlesByHashCode = _weakHandlesByHashCode;
			string text;
			if (weakHandlesByHashCode.TryGetValue(key, out var value))
			{
				Monitor.Enter(value);
				try
				{
					text = value.GetString(ref reference);
					if (text != null)
					{
						reference2 = true;
						return text;
					}
					text = reference.ExpensiveConvertToString();
					value.SetString(text);
					reference2 = false;
					return text;
				}
				finally
				{
					Monitor.Exit(value);
				}
			}
			text = reference.ExpensiveConvertToString();
			value = new StringWeakHandle();
			value.SetString(text);
			if (weakHandlesByHashCode.TryAdd(key, value))
			{
				Interlocked.Increment(ref _count);
			}
			int scavengeThreshold = _scavengeThreshold;
			if (_count >= scavengeThreshold && Interlocked.CompareExchange(ref _scavengeThreshold, int.MaxValue, scavengeThreshold) == scavengeThreshold)
			{
				try
				{
					Scavenge();
				}
				finally
				{
					_scavengeThreshold = _weakHandlesByHashCode.Count * 2;
					_count = _weakHandlesByHashCode.Count;
				}
			}
			reference2 = false;
			return text;
		}
	}

	public void Scavenge()
	{
		foreach (KeyValuePair<int, StringWeakHandle> item in _weakHandlesByHashCode)
		{
			if (item.Value.IsUsed || !_weakHandlesByHashCode.TryRemove(item.Key, out StringWeakHandle value))
			{
				continue;
			}
			lock (value)
			{
				if (!value.IsUsed || !_weakHandlesByHashCode.TryAdd(item.Key, value))
				{
					value.Free();
				}
			}
		}
	}

	public DebugInfo GetDebugInfo()
	{
		return GetDebugInfoImpl();
	}

	private void DisposeImpl()
	{
		foreach (KeyValuePair<int, StringWeakHandle> item in _weakHandlesByHashCode)
		{
			item.Value.Free();
		}
		_stringsByHashCode.Clear();
		_weakHandlesByHashCode.Clear();
	}

	public void Dispose()
	{
		DisposeImpl();
	}

	private DebugInfo GetDebugInfoImpl()
	{
		DebugInfo result = new DebugInfo
		{
			LiveStringCount = _stringsByHashCode.Count
		};
		foreach (KeyValuePair<int, StringWeakHandle> item in _weakHandlesByHashCode)
		{
			if (item.Value.IsUsed)
			{
				result.LiveStringCount++;
			}
			else
			{
				result.CollectedStringCount++;
			}
		}
		return result;
	}
}
