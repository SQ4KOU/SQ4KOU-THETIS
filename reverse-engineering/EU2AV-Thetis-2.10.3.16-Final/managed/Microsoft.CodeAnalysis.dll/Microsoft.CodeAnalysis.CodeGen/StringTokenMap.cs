using System;
using System.Collections.Concurrent;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CodeGen;

internal sealed class StringTokenMap(int initialHeapSize)
{
	private readonly ConcurrentDictionary<string, uint> _valueToToken = new ConcurrentDictionary<string, uint>(StringComparer.Ordinal);

	private readonly ArrayBuilder<string> _uniqueValues = new ArrayBuilder<string>();

	private int _heapSize = initialHeapSize;

	public bool TryGetOrAddToken(string value, out uint token)
	{
		if (_valueToToken.TryGetValue(value, out token))
		{
			return true;
		}
		lock (_uniqueValues)
		{
			if (_valueToToken.TryGetValue(value, out token))
			{
				return true;
			}
			if (_heapSize > 16777214)
			{
				return false;
			}
			_heapSize += MetadataHelpers.GetUserStringBlobSize(value);
			token = (uint)_uniqueValues.Count;
			_uniqueValues.Add(value);
			_valueToToken.Add(value, token);
			return true;
		}
	}

	public string GetValue(uint token)
	{
		lock (_uniqueValues)
		{
			return _uniqueValues[(int)token];
		}
	}

	public string[] CopyValues()
	{
		lock (_uniqueValues)
		{
			return _uniqueValues.ToArray();
		}
	}
}
