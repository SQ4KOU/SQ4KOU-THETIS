using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Microsoft.NET.StringTools;

internal class WeakStringCacheInterner : IDisposable
{
	private enum InternResult
	{
		FoundInWeakStringCache,
		AddedToWeakStringCache
	}

	internal static WeakStringCacheInterner Instance = new WeakStringCacheInterner();

	private readonly WeakStringCache _weakStringCache = new WeakStringCache();

	private int _regularInternHits;

	private int _regularInternMisses;

	private int _internEliminatedStrings;

	private int _internEliminatedChars;

	private Dictionary<string, int>? _internCallCountsByString;

	private InternResult Intern(ref InternableString candidate, out string interned)
	{
		interned = _weakStringCache.GetOrCreateEntry(ref candidate, out var cacheHit);
		if (!cacheHit)
		{
			return InternResult.AddedToWeakStringCache;
		}
		return InternResult.FoundInWeakStringCache;
	}

	public string InternableToString(ref InternableString candidate)
	{
		if (candidate.Length == 0)
		{
			return string.Empty;
		}
		InternResult internResult = Intern(ref candidate, out string interned);
		if (_internCallCountsByString != null)
		{
			lock (_internCallCountsByString)
			{
				switch (internResult)
				{
				case InternResult.FoundInWeakStringCache:
					_regularInternHits++;
					break;
				case InternResult.AddedToWeakStringCache:
					_regularInternMisses++;
					break;
				}
				_internCallCountsByString.TryGetValue(interned, out var value);
				_internCallCountsByString[interned] = value + 1;
				if (!candidate.ReferenceEquals(interned))
				{
					_internEliminatedStrings++;
					_internEliminatedChars += candidate.Length;
				}
			}
		}
		return interned;
	}

	public void EnableStatistics()
	{
		_internCallCountsByString = new Dictionary<string, int>();
	}

	public string FormatStatistics()
	{
		StringBuilder stringBuilder = new StringBuilder(1024);
		string text = "Opportunistic Intern";
		if (_internCallCountsByString != null)
		{
			stringBuilder.AppendLine("\n" + new string('=', 41 - text.Length / 2) + text + new string('=', 41 - text.Length / 2));
			stringBuilder.AppendLine(string.Format("||{0,50}|{1,20:N0}|{2,8}|", "WeakStringCache Hits", _regularInternHits, "hits"));
			stringBuilder.AppendLine(string.Format("||{0,50}|{1,20:N0}|{2,8}|", "WeakStringCache Misses", _regularInternMisses, "misses"));
			stringBuilder.AppendLine(string.Format("||{0,50}|{1,20:N0}|{2,8}|", "Eliminated Strings*", _internEliminatedStrings, "strings"));
			stringBuilder.AppendLine(string.Format("||{0,50}|{1,20:N0}|{2,8}|", "Eliminated Chars", _internEliminatedChars, "chars"));
			stringBuilder.AppendLine(string.Format("||{0,50}|{1,20:N0}|{2,8}|", "Estimated Eliminated Bytes", _internEliminatedChars * 2, "bytes"));
			stringBuilder.AppendLine("Elimination assumes that strings provided were unique objects.");
			stringBuilder.AppendLine("|---------------------------------------------------------------------------------|");
			IEnumerable<string> source = from kv in (from kv in _internCallCountsByString
					orderby kv.Value * kv.Key.Length descending
					where kv.Value > 1
					select kv).Take(15)
				select string.Format(CultureInfo.InvariantCulture, "({1} instances x each {2} chars)\n{0}", kv.Key, kv.Value, kv.Key.Length);
			stringBuilder.AppendLine(string.Format("##########Top Top Interned Strings:  \n{0} ", string.Join("\n==============\n", source.ToArray())));
			stringBuilder.AppendLine();
			WeakStringCache.DebugInfo debugInfo = _weakStringCache.GetDebugInfo();
			stringBuilder.AppendLine("WeakStringCache statistics:");
			stringBuilder.AppendLine($"String count live/collected/total = {debugInfo.LiveStringCount}/{debugInfo.CollectedStringCount}/{debugInfo.LiveStringCount + debugInfo.CollectedStringCount}");
		}
		else
		{
			stringBuilder.Append(text);
			stringBuilder.AppendLine(" - EnableStatisticsGathering() has not been called");
		}
		return stringBuilder.ToString();
	}

	public void Dispose()
	{
		_weakStringCache.Dispose();
	}
}
