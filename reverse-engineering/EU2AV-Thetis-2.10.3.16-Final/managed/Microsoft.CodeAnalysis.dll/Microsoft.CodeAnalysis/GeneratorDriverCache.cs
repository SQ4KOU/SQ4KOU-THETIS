using System;

namespace Microsoft.CodeAnalysis;

internal sealed class GeneratorDriverCache(int maxCacheSize = 100)
{
	private readonly int MaxCacheSize = maxCacheSize;

	private readonly (string cacheKey, GeneratorDriver driver)[] _cachedDrivers = new(string, GeneratorDriver)[maxCacheSize];

	private readonly object _cacheLock = new object();

	private int _cacheSize;

	public int CacheSize => _cacheSize;

	public GeneratorDriver? TryGetDriver(string cacheKey)
	{
		return AddOrUpdateMostRecentlyUsed(cacheKey, null);
	}

	public void CacheGenerator(string cacheKey, GeneratorDriver driver)
	{
		AddOrUpdateMostRecentlyUsed(cacheKey, driver);
	}

	private GeneratorDriver? AddOrUpdateMostRecentlyUsed(string cacheKey, GeneratorDriver? driver)
	{
		lock (_cacheLock)
		{
			int i;
			for (i = 0; i < _cacheSize; i++)
			{
				if (_cachedDrivers[i].cacheKey == cacheKey)
				{
					if (driver == null)
					{
						driver = _cachedDrivers[i].driver;
					}
					break;
				}
			}
			if (driver != null)
			{
				for (i = Math.Min(i, MaxCacheSize - 1); i > 0; i--)
				{
					_cachedDrivers[i] = _cachedDrivers[i - 1];
				}
				_cachedDrivers[0] = (cacheKey: cacheKey, driver: driver);
				_cacheSize = Math.Min(MaxCacheSize, _cacheSize + 1);
			}
			return driver;
		}
	}
}
