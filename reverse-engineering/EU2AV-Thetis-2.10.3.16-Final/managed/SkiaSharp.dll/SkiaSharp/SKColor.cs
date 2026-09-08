using System;
using System.Globalization;

namespace SkiaSharp;

public readonly struct SKColor : IEquatable<SKColor>
{
	public static readonly SKColor Empty;

	private readonly uint color;

	public byte Alpha => (byte)((color >> 24) & 0xFF);

	public byte Red => (byte)((color >> 16) & 0xFF);

	public byte Green => (byte)((color >> 8) & 0xFF);

	public byte Blue => (byte)(color & 0xFF);

	public float Hue
	{
		get
		{
			ToHsv(out var h, out var _, out var _);
			return h;
		}
	}

	public SKColor(uint value)
	{
		color = value;
	}

	public SKColor(byte red, byte green, byte blue, byte alpha)
	{
		color = (uint)((alpha << 24) | (red << 16) | (green << 8) | blue);
	}

	public SKColor(byte red, byte green, byte blue)
	{
		color = (uint)(-16777216 | (red << 16) | (green << 8) | blue);
	}

	public SKColor WithRed(byte red)
	{
		return new SKColor(red, Green, Blue, Alpha);
	}

	public SKColor WithGreen(byte green)
	{
		return new SKColor(Red, green, Blue, Alpha);
	}

	public SKColor WithBlue(byte blue)
	{
		return new SKColor(Red, Green, blue, Alpha);
	}

	public SKColor WithAlpha(byte alpha)
	{
		return new SKColor(Red, Green, Blue, alpha);
	}

	public static SKColor FromHsl(float h, float s, float l, byte a = byte.MaxValue)
	{
		SKColorF sKColorF = SKColorF.FromHsl(h, s, l);
		float num = sKColorF.Red * 255f;
		float num2 = sKColorF.Green * 255f;
		float num3 = sKColorF.Blue * 255f;
		return new SKColor((byte)num, (byte)num2, (byte)num3, a);
	}

	public static SKColor FromHsv(float h, float s, float v, byte a = byte.MaxValue)
	{
		SKColorF sKColorF = SKColorF.FromHsv(h, s, v);
		float num = sKColorF.Red * 255f;
		float num2 = sKColorF.Green * 255f;
		float num3 = sKColorF.Blue * 255f;
		return new SKColor((byte)num, (byte)num2, (byte)num3, a);
	}

	public void ToHsl(out float h, out float s, out float l)
	{
		float red = (float)(int)Red / 255f;
		float green = (float)(int)Green / 255f;
		float blue = (float)(int)Blue / 255f;
		new SKColorF(red, green, blue).ToHsl(out h, out s, out l);
	}

	public void ToHsv(out float h, out float s, out float v)
	{
		float red = (float)(int)Red / 255f;
		float green = (float)(int)Green / 255f;
		float blue = (float)(int)Blue / 255f;
		new SKColorF(red, green, blue).ToHsv(out h, out s, out v);
	}

	public override string ToString()
	{
		return $"#{Alpha:x2}{Red:x2}{Green:x2}{Blue:x2}";
	}

	public bool Equals(SKColor obj)
	{
		return obj.color == color;
	}

	public override bool Equals(object other)
	{
		if (other is SKColor obj)
		{
			return Equals(obj);
		}
		return false;
	}

	public static bool operator ==(SKColor left, SKColor right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(SKColor left, SKColor right)
	{
		return !left.Equals(right);
	}

	public override int GetHashCode()
	{
		return color.GetHashCode();
	}

	public static implicit operator SKColor(uint color)
	{
		return new SKColor(color);
	}

	public static explicit operator uint(SKColor color)
	{
		return color.color;
	}

	public static SKColor Parse(string hexString)
	{
		if (!TryParse(hexString, out var result))
		{
			throw new ArgumentException("Invalid hexadecimal color string.", "hexString");
		}
		return result;
	}

	public static bool TryParse(string hexString, out SKColor color)
	{
		if (string.IsNullOrWhiteSpace(hexString))
		{
			color = Empty;
			return false;
		}
		hexString = hexString.Trim();
		int num = ((hexString[0] == '#') ? 1 : 0);
		int num2 = hexString.Length - num;
		switch (num2)
		{
		case 3:
		case 4:
		{
			byte result2;
			if (num2 == 4)
			{
				if (!byte.TryParse(string.Concat(new string(hexString[num2 - 4 + num], 2)), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out result2))
				{
					color = Empty;
					return false;
				}
			}
			else
			{
				result2 = byte.MaxValue;
			}
			if (!byte.TryParse(new string(hexString[num2 - 3 + num], 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var result3) || !byte.TryParse(new string(hexString[num2 - 2 + num], 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var result4) || !byte.TryParse(new string(hexString[num2 - 1 + num], 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var result5))
			{
				color = Empty;
				return false;
			}
			color = new SKColor(result3, result4, result5, result2);
			return true;
		}
		case 6:
		case 8:
		{
			if (!uint.TryParse(hexString.Substring(num), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var result))
			{
				color = Empty;
				return false;
			}
			color = result;
			if (num2 == 6)
			{
				color = color.WithAlpha(byte.MaxValue);
			}
			return true;
		}
		default:
			color = Empty;
			return false;
		}
	}
}
