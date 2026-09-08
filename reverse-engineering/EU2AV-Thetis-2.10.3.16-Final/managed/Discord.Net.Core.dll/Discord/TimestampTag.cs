using System;

namespace Discord;

public readonly struct TimestampTag
{
	public DateTimeOffset Time { get; }

	public TimestampTagStyles? Style { get; }

	public TimestampTag(DateTimeOffset time, TimestampTagStyles? style = null)
	{
		Time = time;
		Style = style;
	}

	public override string ToString()
	{
		return ToString(Style ?? TimestampTagStyles.ShortDateTime);
	}

	public string ToString(TimestampTagStyles style)
	{
		return $"<t:{Time.ToUnixTimeSeconds()}:{(char)style}>";
	}

	public static TimestampTag FromDateTime(DateTime time, TimestampTagStyles? style = null)
	{
		return new TimestampTag(time, style);
	}

	public static TimestampTag FromDateTimeOffset(DateTimeOffset time, TimestampTagStyles? style = null)
	{
		return new TimestampTag(time, style);
	}

	public static string FormatFromDateTime(DateTime time, TimestampTagStyles style)
	{
		return FormatFromDateTimeOffset(time, style);
	}

	public static string FormatFromDateTimeOffset(DateTimeOffset time, TimestampTagStyles style)
	{
		return $"<t:{time.ToUnixTimeSeconds()}:{(char)style}>";
	}
}
