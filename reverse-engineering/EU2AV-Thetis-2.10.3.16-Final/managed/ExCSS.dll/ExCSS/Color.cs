using System;
using System.Runtime.InteropServices;

namespace ExCSS;

[StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode, Pack = 1)]
public struct Color : IEquatable<Color>, IComparable<Color>, IFormattable
{
	[FieldOffset(0)]
	private readonly byte _alpha;

	[FieldOffset(1)]
	private readonly byte _red;

	[FieldOffset(2)]
	private readonly byte _green;

	[FieldOffset(3)]
	private readonly byte _blue;

	[FieldOffset(0)]
	private readonly int _hashcode;

	public static readonly Color Black = new Color(0, 0, 0);

	public static readonly Color White = new Color(byte.MaxValue, byte.MaxValue, byte.MaxValue);

	public static readonly Color Red = new Color(byte.MaxValue, 0, 0);

	public static readonly Color Magenta = new Color(byte.MaxValue, 0, byte.MaxValue);

	public static readonly Color Green = new Color(0, 128, 0);

	public static readonly Color PureGreen = new Color(0, byte.MaxValue, 0);

	public static readonly Color Blue = new Color(0, 0, byte.MaxValue);

	public static readonly Color Transparent = new Color(0, 0, 0, 0);

	public int Value => _hashcode;

	public byte A => _alpha;

	public double Alpha => Math.Round((double)(int)_alpha / 255.0, 2);

	public byte R => _red;

	public byte G => _green;

	public byte B => _blue;

	public Color(byte r, byte g, byte b)
	{
		_hashcode = 0;
		_alpha = byte.MaxValue;
		_red = r;
		_blue = b;
		_green = g;
	}

	public Color(byte red, byte green, byte blue, byte alpha)
	{
		_hashcode = 0;
		_alpha = alpha;
		_red = red;
		_blue = blue;
		_green = green;
	}

	public static Color FromRgba(byte red, byte green, byte blue, float alpha)
	{
		return new Color(red, green, blue, Normalize(alpha));
	}

	public static Color FromRgba(float red, float green, float blue, float alpha)
	{
		return new Color(Normalize(red), Normalize(green), Normalize(blue), Normalize(alpha));
	}

	public static Color FromGray(byte number, float alpha = 1f)
	{
		return new Color(number, number, number, Normalize(alpha));
	}

	public static Color FromGray(float value, float alpha = 1f)
	{
		return FromGray(Normalize(value), alpha);
	}

	public static Color? FromName(string name)
	{
		return Colors.GetColor(name);
	}

	public static Color FromRgb(byte red, byte green, byte blue)
	{
		return new Color(red, green, blue);
	}

	public static Color FromHex(string color)
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 255;
		switch (color.Length)
		{
		case 4:
			num4 = 17 * color[3].FromHex();
			goto case 3;
		case 3:
			num = 17 * color[0].FromHex();
			num2 = 17 * color[1].FromHex();
			num3 = 17 * color[2].FromHex();
			break;
		case 8:
			num4 = 16 * color[6].FromHex() + color[7].FromHex();
			goto case 6;
		case 6:
			num = 16 * color[0].FromHex() + color[1].FromHex();
			num2 = 16 * color[2].FromHex() + color[3].FromHex();
			num3 = 16 * color[4].FromHex() + color[5].FromHex();
			break;
		}
		return new Color((byte)num, (byte)num2, (byte)num3, (byte)num4);
	}

	public static bool TryFromHex(string color, out Color value)
	{
		if (color.Length == 6 || color.Length == 3 || color.Length == 8 || color.Length == 4)
		{
			int num = 0;
			while (true)
			{
				if (num < color.Length)
				{
					if (!color[num].IsHex())
					{
						break;
					}
					num++;
					continue;
				}
				value = FromHex(color);
				return true;
			}
		}
		value = default(Color);
		return false;
	}

	public static Color FromFlexHex(string color)
	{
		int num = Math.Max(color.Length, 3);
		int num2 = num % 3;
		if (num2 != 0)
		{
			num += 3 - num2;
		}
		int num3 = num / 3;
		int num4 = Math.Min(2, num3);
		int num5 = Math.Max(num3 - 8, 0);
		char[] array = new char[num];
		for (int i = 0; i < color.Length; i++)
		{
			array[i] = (color[i].IsHex() ? color[i] : '0');
		}
		for (int j = color.Length; j < num; j++)
		{
			array[j] = '0';
		}
		if (num4 == 1)
		{
			int num6 = array[num5].FromHex();
			int num7 = array[num3 + num5].FromHex();
			int num8 = array[2 * num3 + num5].FromHex();
			return new Color((byte)num6, (byte)num7, (byte)num8);
		}
		int num9 = 16 * array[num5].FromHex() + array[num5 + 1].FromHex();
		int num10 = 16 * array[num3 + num5].FromHex() + array[num3 + num5 + 1].FromHex();
		int num11 = 16 * array[2 * num3 + num5].FromHex() + array[2 * num3 + num5 + 1].FromHex();
		return new Color((byte)num9, (byte)num10, (byte)num11);
	}

	public static Color FromHsl(float hue, float saturation, float luminosity)
	{
		return FromHsla(hue, saturation, luminosity, 1f);
	}

	public static Color FromHsla(float hue, float saturation, float luminosity, float alpha)
	{
		float num = ((luminosity <= 0.5f) ? (luminosity * (saturation + 1f)) : (luminosity + saturation - luminosity * saturation));
		float m = 2f * luminosity - num;
		byte red = Convert(HueToRgb(m, num, hue + 1f / 3f));
		byte green = Convert(HueToRgb(m, num, hue));
		byte blue = Convert(HueToRgb(m, num, hue - 1f / 3f));
		return new Color(red, green, blue, Normalize(alpha));
	}

	public static Color FromHwb(float hue, float whiteness, float blackness)
	{
		return FromHwba(hue, whiteness, blackness, 1f);
	}

	public static Color FromHwba(float hue, float whiteness, float blackness, float alpha)
	{
		float num = 1f / (whiteness + blackness);
		if (num < 1f)
		{
			whiteness *= num;
			blackness *= num;
		}
		int num2 = (int)(6f * hue);
		float num3 = 6f * hue - (float)num2;
		if ((num2 & 1) != 0)
		{
			num3 = 1f - num3;
		}
		float num4 = 1f - blackness;
		float num5 = whiteness + num3 * (num4 - whiteness);
		float red;
		float green;
		float blue;
		switch (num2)
		{
		default:
			red = num4;
			green = num5;
			blue = whiteness;
			break;
		case 1:
			red = num5;
			green = num4;
			blue = whiteness;
			break;
		case 2:
			red = whiteness;
			green = num4;
			blue = num5;
			break;
		case 3:
			red = whiteness;
			green = num5;
			blue = num4;
			break;
		case 4:
			red = num5;
			green = whiteness;
			blue = num4;
			break;
		case 5:
			red = num4;
			green = whiteness;
			blue = num5;
			break;
		}
		return FromRgba(red, green, blue, alpha);
	}

	public static bool operator ==(Color a, Color b)
	{
		return a._hashcode == b._hashcode;
	}

	public static bool operator !=(Color a, Color b)
	{
		return a._hashcode != b._hashcode;
	}

	public bool Equals(Color other)
	{
		return _hashcode == other._hashcode;
	}

	public override bool Equals(object obj)
	{
		Color? color = obj as Color?;
		if (color.HasValue)
		{
			return Equals(color.Value);
		}
		return false;
	}

	int IComparable<Color>.CompareTo(Color other)
	{
		return _hashcode - other._hashcode;
	}

	public override int GetHashCode()
	{
		return _hashcode;
	}

	public static Color Mix(Color above, Color below)
	{
		return Mix(above.Alpha, above, below);
	}

	public static Color Mix(double alpha, Color above, Color below)
	{
		double num = 1.0 - alpha;
		double num2 = num * (double)(int)below.R + alpha * (double)(int)above.R;
		double num3 = num * (double)(int)below.G + alpha * (double)(int)above.G;
		double num4 = num * (double)(int)below.B + alpha * (double)(int)above.B;
		return new Color((byte)num2, (byte)num3, (byte)num4);
	}

	private static byte Normalize(float value)
	{
		return (byte)Math.Max(Math.Min(Math.Round(255f * value), 255.0), 0.0);
	}

	private static byte Convert(float value)
	{
		return (byte)Math.Round(255f * value);
	}

	private static float HueToRgb(float m1, float m2, float h)
	{
		if (h < 0f)
		{
			h++;
		}
		else if (h > 1f)
		{
			h--;
		}
		if (h < 1f / 6f)
		{
			return m1 + (m2 - m1) * h * 6f;
		}
		if ((double)h < 0.5)
		{
			return m2;
		}
		if (h < 2f / 3f)
		{
			return m1 + (m2 - m1) * (2f / 3f - h) * 6f;
		}
		return m1;
	}

	public override string ToString()
	{
		if (_alpha == byte.MaxValue)
		{
			string argument = string.Join(", ", R.ToString(), G.ToString(), B.ToString());
			return FunctionNames.Rgb.StylesheetFunction(argument);
		}
		string argument2 = string.Join(", ", R.ToString(), G.ToString(), B.ToString(), Alpha.ToString());
		return FunctionNames.Rgba.StylesheetFunction(argument2);
	}

	public string ToString(string format, IFormatProvider formatProvider)
	{
		if (_alpha == byte.MaxValue)
		{
			string argument = string.Join(", ", R.ToString(format, formatProvider), G.ToString(format, formatProvider), B.ToString(format, formatProvider));
			return FunctionNames.Rgb.StylesheetFunction(argument);
		}
		string argument2 = string.Join(", ", R.ToString(format, formatProvider), G.ToString(format, formatProvider), B.ToString(format, formatProvider), Alpha.ToString(format, formatProvider));
		return FunctionNames.Rgba.StylesheetFunction(argument2);
	}
}
