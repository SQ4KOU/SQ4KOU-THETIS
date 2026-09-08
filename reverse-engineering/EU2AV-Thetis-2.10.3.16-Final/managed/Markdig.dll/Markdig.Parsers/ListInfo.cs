using Markdig.Helpers;

namespace Markdig.Parsers;

public struct ListInfo
{
	public char BulletType { get; set; }

	public string? OrderedStart { get; set; }

	public char OrderedDelimiter { get; set; }

	public string? DefaultOrderedStart { get; set; }

	public StringSlice SourceBullet { get; set; }

	public ListInfo(char bulletType)
	{
		BulletType = bulletType;
		OrderedStart = null;
		OrderedDelimiter = '\0';
		DefaultOrderedStart = null;
		SourceBullet = StringSlice.Empty;
	}

	public ListInfo(char bulletType, string orderedStart, char orderedDelimiter, string defaultOrderedStart)
	{
		BulletType = bulletType;
		OrderedStart = orderedStart;
		OrderedDelimiter = orderedDelimiter;
		DefaultOrderedStart = defaultOrderedStart;
		SourceBullet = StringSlice.Empty;
	}
}
