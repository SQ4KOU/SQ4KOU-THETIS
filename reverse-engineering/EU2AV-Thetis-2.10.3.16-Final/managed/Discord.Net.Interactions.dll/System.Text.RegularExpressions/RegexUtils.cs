using System.Collections.Generic;
using System.Linq;
using Discord.Interactions;

namespace System.Text.RegularExpressions;

internal static class RegexUtils
{
	private enum MatchType
	{
		Quantified,
		Unquantified
	}

	private record MatchPair
	{
		public MatchType Type { get; }

		public Match Match { get; }

		public MatchPair(MatchType type, Match match)
		{
			Type = type;
			Match = match;
		}
	}

	internal const byte Q = 5;

	internal const byte S = 4;

	internal const byte Z = 3;

	internal const byte X = 2;

	internal const byte E = 1;

	internal static readonly byte[] _category = new byte[128]
	{
		0, 0, 0, 0, 0, 0, 0, 0, 0, 2,
		2, 0, 2, 2, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 2, 0, 0, 3, 4, 0, 0, 0,
		4, 4, 5, 5, 0, 0, 4, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 5, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 4, 4, 0, 4, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 5, 4, 0, 0, 0
	};

	internal static string EscapeExcluding(string input, params char[] exclude)
	{
		if (exclude == null)
		{
			throw new ArgumentNullException("exclude");
		}
		for (int i = 0; i < input.Length; i++)
		{
			if (!IsMetachar(input[i]) || Enumerable.Contains(exclude, input[i]))
			{
				continue;
			}
			StringBuilder stringBuilder = new StringBuilder();
			char c = input[i];
			stringBuilder.Append(input, 0, i);
			do
			{
				stringBuilder.Append('\\');
				switch (c)
				{
				case '\n':
					c = 'n';
					break;
				case '\r':
					c = 'r';
					break;
				case '\t':
					c = 't';
					break;
				case '\f':
					c = 'f';
					break;
				}
				stringBuilder.Append(c);
				i++;
				int num = i;
				for (; i < input.Length; i++)
				{
					c = input[i];
					if (IsMetachar(c) && !Enumerable.Contains(exclude, input[i]))
					{
						break;
					}
				}
				stringBuilder.Append(input, num, i - num);
			}
			while (i < input.Length);
			return stringBuilder.ToString();
		}
		return input;
	}

	internal static bool IsMetachar(char ch)
	{
		if (ch <= '|')
		{
			return _category[(uint)ch] >= 1;
		}
		return false;
	}

	internal static int GetWildCardCount(string input, string wildCardExpression)
	{
		string text = Regex.Escape(wildCardExpression);
		return Regex.Matches(input, "(?<!\\\\)" + text + "|(?<!\\\\){[0-9]+(?:,[0-9]*)?(?<!\\\\)}").Count;
	}

	internal static bool TryBuildRegexPattern<T>(T commandInfo, string wildCardStr, out string pattern) where T : class, ICommandInfo
	{
		if (commandInfo.TreatNameAsRegex)
		{
			pattern = commandInfo.Name;
			return true;
		}
		if (GetWildCardCount(commandInfo.Name, wildCardStr) == 0)
		{
			pattern = null;
			return false;
		}
		string text = Regex.Escape(wildCardStr);
		string pattern2 = "(?<!\\\\)" + text + "(?<delimiter>[^" + text + "]?)";
		string pattern3 = "(?<!\\\\){(?<start>[0-9]+)(?<end>,[0-9]*)?(?<!\\\\)}(?<delimiter>[^" + text + "]?)";
		string name = commandInfo.Name;
		SortedDictionary<int, MatchPair> sortedDictionary = new SortedDictionary<int, MatchPair>();
		foreach (Match item in Regex.Matches(name, pattern2))
		{
			sortedDictionary.Add(item.Index, new MatchPair(MatchType.Unquantified, item));
		}
		foreach (Match item2 in Regex.Matches(name, pattern3))
		{
			sortedDictionary.Add(item2.Index, new MatchPair(MatchType.Quantified, item2));
		}
		StringBuilder stringBuilder = new StringBuilder();
		int num = 0;
		foreach (KeyValuePair<int, MatchPair> item3 in sortedDictionary)
		{
			stringBuilder.Append(Regex.Escape(name.Substring(num, item3.Key - num)));
			Match match3 = item3.Value.Match;
			MatchType type = item3.Value.Type;
			num = item3.Key + match3.Length;
			string value = match3.Groups["delimiter"].Value;
			switch (type)
			{
			case MatchType.Unquantified:
				stringBuilder.Append("([^\\n\\t" + Regex.Escape(value) + "]+)" + Regex.Escape(value));
				break;
			case MatchType.Quantified:
			{
				string value2 = match3.Groups["start"].Value;
				string value3 = match3.Groups["end"].Value;
				stringBuilder.Append("([^\\n\\t" + Regex.Escape(value) + "]{" + value2 + value3 + "})" + Regex.Escape(value));
				break;
			}
			}
		}
		pattern = "\\A" + stringBuilder.ToString() + "\\Z";
		return true;
	}
}
