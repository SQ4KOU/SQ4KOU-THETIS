using System;

namespace SkiaSharp;

internal struct SKTimeDateTimeInternal : IEquatable<SKTimeDateTimeInternal>
{
	public short fTimeZoneMinutes;

	public ushort fYear;

	public byte fMonth;

	public byte fDayOfWeek;

	public byte fDay;

	public byte fHour;

	public byte fMinute;

	public byte fSecond;

	public static SKTimeDateTimeInternal Create(DateTime datetime)
	{
		int num = datetime.ToLocalTime().Hour - datetime.ToUniversalTime().Hour;
		return new SKTimeDateTimeInternal
		{
			fTimeZoneMinutes = (short)(num * 60),
			fYear = (ushort)datetime.Year,
			fMonth = (byte)datetime.Month,
			fDayOfWeek = (byte)datetime.DayOfWeek,
			fDay = (byte)datetime.Day,
			fHour = (byte)datetime.Hour,
			fMinute = (byte)datetime.Minute,
			fSecond = (byte)datetime.Second
		};
	}

	public readonly bool Equals(SKTimeDateTimeInternal obj)
	{
		if (fTimeZoneMinutes == obj.fTimeZoneMinutes && fYear == obj.fYear && fMonth == obj.fMonth && fDayOfWeek == obj.fDayOfWeek && fDay == obj.fDay && fHour == obj.fHour && fMinute == obj.fMinute)
		{
			return fSecond == obj.fSecond;
		}
		return false;
	}

	public override readonly bool Equals(object obj)
	{
		if (obj is SKTimeDateTimeInternal obj2)
		{
			return Equals(obj2);
		}
		return false;
	}

	public static bool operator ==(SKTimeDateTimeInternal left, SKTimeDateTimeInternal right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(SKTimeDateTimeInternal left, SKTimeDateTimeInternal right)
	{
		return !left.Equals(right);
	}

	public override readonly int GetHashCode()
	{
		HashCode hashCode = default(HashCode);
		hashCode.Add(fTimeZoneMinutes);
		hashCode.Add(fYear);
		hashCode.Add(fMonth);
		hashCode.Add(fDayOfWeek);
		hashCode.Add(fDay);
		hashCode.Add(fHour);
		hashCode.Add(fMinute);
		hashCode.Add(fSecond);
		return hashCode.ToHashCode();
	}
}
