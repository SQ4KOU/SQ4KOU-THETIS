using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;

namespace Discord;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public struct Color
{
	public const uint MaxDecimalValue = 16777215u;

	public static readonly Color Default = new Color(0u);

	public static readonly Color Teal = new Color(1752220u);

	public static readonly Color DarkTeal = new Color(1146986u);

	public static readonly Color Green = new Color(3066993u);

	public static readonly Color DarkGreen = new Color(2067276u);

	public static readonly Color Blue = new Color(3447003u);

	public static readonly Color DarkBlue = new Color(2123412u);

	public static readonly Color Purple = new Color(10181046u);

	public static readonly Color DarkPurple = new Color(7419530u);

	public static readonly Color Magenta = new Color(15277667u);

	public static readonly Color DarkMagenta = new Color(11342935u);

	public static readonly Color Gold = new Color(15844367u);

	public static readonly Color LightOrange = new Color(12745742u);

	public static readonly Color Orange = new Color(15105570u);

	public static readonly Color DarkOrange = new Color(11027200u);

	public static readonly Color Red = new Color(15158332u);

	public static readonly Color DarkRed = new Color(10038562u);

	public static readonly Color LightGrey = new Color(9936031u);

	public static readonly Color LighterGrey = new Color(9807270u);

	public static readonly Color DarkGrey = new Color(6323595u);

	public static readonly Color DarkerGrey = new Color(5533306u);

	public uint RawValue { get; }

	public byte R => (byte)(RawValue >> 16);

	public byte G => (byte)(RawValue >> 8);

	public byte B => (byte)RawValue;

	private string DebuggerDisplay => string.Format("#{0:X6} ({0})", RawValue);

	public Color(uint rawValue)
	{
		if (rawValue > 16777215)
		{
			throw new ArgumentException(string.Format("{0} of color cannot be greater than {1}!", "RawValue", 16777215u), "rawValue");
		}
		RawValue = rawValue;
	}

	public Color(byte r, byte g, byte b)
	{
		uint num = (uint)((r << 16) | (g << 8) | b);
		if (num > 16777215)
		{
			throw new ArgumentException(string.Format("{0} of color cannot be greater than {1}!", "RawValue", 16777215u));
		}
		RawValue = num;
	}

	public Color(int r, int g, int b)
	{
		if (r < 0 || r > 255)
		{
			throw new ArgumentOutOfRangeException("r", "Value must be within [0,255].");
		}
		if (g < 0 || g > 255)
		{
			throw new ArgumentOutOfRangeException("g", "Value must be within [0,255].");
		}
		if (b < 0 || b > 255)
		{
			throw new ArgumentOutOfRangeException("b", "Value must be within [0,255].");
		}
		RawValue = (uint)((r << 16) | (g << 8) | b);
	}

	public Color(float r, float g, float b)
	{
		if (r < 0f || r > 1f)
		{
			throw new ArgumentOutOfRangeException("r", "Value must be within [0,1].");
		}
		if (g < 0f || g > 1f)
		{
			throw new ArgumentOutOfRangeException("g", "Value must be within [0,1].");
		}
		if (b < 0f || b > 1f)
		{
			throw new ArgumentOutOfRangeException("b", "Value must be within [0,1].");
		}
		RawValue = ((uint)(r * 255f) << 16) | ((uint)(g * 255f) << 8) | (uint)(b * 255f);
	}

	public static bool operator ==(Color lhs, Color rhs)
	{
		return lhs.RawValue == rhs.RawValue;
	}

	public static bool operator !=(Color lhs, Color rhs)
	{
		return lhs.RawValue != rhs.RawValue;
	}

	public static implicit operator Color(uint rawValue)
	{
		return new Color(rawValue);
	}

	public static implicit operator uint(Color color)
	{
		return color.RawValue;
	}

	public static Color Parse(string rawValue, ColorType colorType = ColorType.CssHexColor)
	{
		if (TryParse(rawValue, out var color, colorType))
		{
			return color;
		}
		throw new ArgumentOutOfRangeException("rawValue", "Value must be a number of valid length - matching the specified ColorType");
	}

	public static bool TryParse(string rawValue, out Color color, ColorType colorType = ColorType.CssHexColor)
	{
		color = default(Color);
		if (string.IsNullOrWhiteSpace(rawValue))
		{
			return false;
		}
		rawValue = rawValue.TrimStart('#');
		if (rawValue.StartsWith("0x"))
		{
			rawValue = rawValue.Substring(2);
		}
		if (!uint.TryParse(rawValue, NumberStyles.HexNumber, null, out var result))
		{
			return false;
		}
		uint rawValue2;
		switch (rawValue.Length)
		{
		case 3:
		{
			if (colorType != ColorType.CssHexColor)
			{
				goto default;
			}
			uint num = result >> 8;
			uint num2 = (result & 0xF0) >> 4;
			uint num3 = result & 0xF;
			num = (num << 4) | num;
			num2 = (num2 << 4) | num2;
			num3 = (num3 << 4) | num3;
			rawValue2 = (num << 16) | (num2 << 8) | num3;
			break;
		}
		case 4:
		{
			if (colorType != ColorType.CssHexColor)
			{
				goto default;
			}
			uint num = (result & 0xF00) >> 8;
			uint num2 = (result & 0xF0) >> 4;
			uint num3 = result & 0xF;
			num = (num << 4) | num;
			num2 = (num2 << 4) | num2;
			num3 = (num3 << 4) | num3;
			rawValue2 = (num << 16) | (num2 << 8) | num3;
			break;
		}
		case 6:
			if (colorType != ColorType.CssHexColor)
			{
				goto default;
			}
			color = new Color(result);
			return true;
		case 8:
			if (colorType != ColorType.CssHexColor)
			{
				goto default;
			}
			result &= 0xFFFFFF;
			color = new Color(result);
			return true;
		default:
			return false;
		}
		color = new Color(rawValue2);
		return true;
	}

	public override bool Equals(object obj)
	{
		if (obj is Color color)
		{
			return RawValue == color.RawValue;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return RawValue.GetHashCode();
	}

	public static implicit operator System.Drawing.Color(Color color)
	{
		return System.Drawing.Color.FromArgb((int)color.RawValue);
	}

	public static explicit operator Color(System.Drawing.Color color)
	{
		return new Color((uint)(color.ToArgb() << 8) >> 8);
	}

	public override string ToString()
	{
		return $"#{RawValue:X6}";
	}
}
