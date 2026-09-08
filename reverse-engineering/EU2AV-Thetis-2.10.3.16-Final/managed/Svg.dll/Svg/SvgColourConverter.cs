using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Text;

namespace Svg;

public class SvgColourConverter : ColorConverter
{
	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (value is string text)
		{
			string text2 = text.Trim();
			if (text2.StartsWith("rgb", StringComparison.InvariantCulture))
			{
				try
				{
					int num = text2.IndexOf("(", StringComparison.InvariantCulture) + 1;
					string[] array = text2.Substring(num, text2.IndexOf(")", StringComparison.InvariantCulture) - num).Split(new char[2] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
					int alpha = 255;
					if (array.Length > 3)
					{
						string text3 = array[3];
						if (text3.StartsWith(".", StringComparison.InvariantCulture))
						{
							text3 = "0" + text3;
						}
						decimal num2 = decimal.Parse(text3, CultureInfo.InvariantCulture);
						alpha = ((!(num2 <= 1m)) ? ((int)Math.Round(num2)) : ((int)Math.Round(num2 * 255m)));
					}
					Color color = ((!array[0].Trim().EndsWith("%", StringComparison.InvariantCulture)) ? Color.FromArgb(alpha, int.Parse(array[0], CultureInfo.InvariantCulture), int.Parse(array[1], CultureInfo.InvariantCulture), int.Parse(array[2], CultureInfo.InvariantCulture)) : Color.FromArgb(alpha, (int)Math.Round(255f * float.Parse(array[0].Trim().TrimEnd('%'), NumberStyles.Any, CultureInfo.InvariantCulture) / 100f), (int)Math.Round(255f * float.Parse(array[1].Trim().TrimEnd('%'), NumberStyles.Any, CultureInfo.InvariantCulture) / 100f), (int)Math.Round(255f * float.Parse(array[2].Trim().TrimEnd('%'), NumberStyles.Any, CultureInfo.InvariantCulture) / 100f)));
					return color;
				}
				catch
				{
					throw new SvgException("Colour is in an invalid format: '" + text2 + "'");
				}
			}
			if (text2.StartsWith("hsl", StringComparison.InvariantCulture))
			{
				try
				{
					int num3 = text2.IndexOf("(", StringComparison.InvariantCulture) + 1;
					string[] array2 = text2.Substring(num3, text2.IndexOf(")", StringComparison.InvariantCulture) - num3).Split(new char[2] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
					if (array2[1].EndsWith("%", StringComparison.InvariantCulture))
					{
						array2[1] = array2[1].TrimEnd('%');
					}
					if (array2[2].EndsWith("%", StringComparison.InvariantCulture))
					{
						array2[2] = array2[2].TrimEnd('%');
					}
					double h = double.Parse(array2[0], CultureInfo.InvariantCulture) / 360.0;
					double sl = double.Parse(array2[1], CultureInfo.InvariantCulture) / 100.0;
					double l = double.Parse(array2[2], CultureInfo.InvariantCulture) / 100.0;
					return Hsl2Rgb(h, sl, l);
				}
				catch
				{
					throw new SvgException("Colour is in an invalid format: '" + text2 + "'");
				}
			}
			if (text2.StartsWith("#", StringComparison.InvariantCulture))
			{
				if (text2.Length == 4)
				{
					text2 = string.Format(culture, "#{0}{0}{1}{1}{2}{2}", text2[1], text2[2], text2[3]);
					return base.ConvertFrom(context, culture, (object)text2);
				}
				if (text2.Length != 7)
				{
					return SvgPaintServer.NotSet;
				}
			}
			switch (text2.ToLowerInvariant())
			{
			case "activeborder":
				return SystemColors.ActiveBorder;
			case "activecaption":
				return SystemColors.ActiveCaption;
			case "appworkspace":
				return SystemColors.AppWorkspace;
			case "background":
				return SystemColors.Desktop;
			case "buttonface":
				return SystemColors.Control;
			case "buttonhighlight":
				return SystemColors.ControlLightLight;
			case "buttonshadow":
				return SystemColors.ControlDark;
			case "buttontext":
				return SystemColors.ControlText;
			case "captiontext":
				return SystemColors.ActiveCaptionText;
			case "graytext":
				return SystemColors.GrayText;
			case "highlight":
				return SystemColors.Highlight;
			case "highlighttext":
				return SystemColors.HighlightText;
			case "inactiveborder":
				return SystemColors.InactiveBorder;
			case "inactivecaption":
				return SystemColors.InactiveCaption;
			case "inactivecaptiontext":
				return SystemColors.InactiveCaptionText;
			case "infobackground":
				return SystemColors.Info;
			case "infotext":
				return SystemColors.InfoText;
			case "menu":
				return SystemColors.Menu;
			case "menutext":
				return SystemColors.MenuText;
			case "scrollbar":
				return SystemColors.ScrollBar;
			case "threeddarkshadow":
				return SystemColors.ControlDarkDark;
			case "threedface":
				return SystemColors.Control;
			case "threedhighlight":
				return SystemColors.ControlLight;
			case "threedlightshadow":
				return SystemColors.ControlLightLight;
			case "window":
				return SystemColors.Window;
			case "windowframe":
				return SystemColors.WindowFrame;
			case "windowtext":
				return SystemColors.WindowText;
			}
			if (int.TryParse(text2, NumberStyles.Integer, CultureInfo.InvariantCulture, out var _))
			{
				return SvgPaintServer.NotSet;
			}
			int num4 = text2.LastIndexOf("grey", StringComparison.InvariantCultureIgnoreCase);
			if (num4 >= 0 && num4 + 4 == text2.Length)
			{
				value = new StringBuilder(text2).Replace("grey", "gray", num4, 4).Replace("Grey", "Gray", num4, 4).ToString();
			}
		}
		return base.ConvertFrom(context, culture, value);
	}

	public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
	{
		if (sourceType == typeof(string))
		{
			return true;
		}
		return base.CanConvertFrom(context, sourceType);
	}

	public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	{
		if (destinationType == typeof(string))
		{
			return true;
		}
		return base.CanConvertTo(context, destinationType);
	}

	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		if (destinationType == typeof(string))
		{
			string text = ColorTranslator.ToHtml((Color)value).Replace("LightGrey", "LightGray");
			if (!text.StartsWith("#", StringComparison.InvariantCulture))
			{
				return text.ToLowerInvariant();
			}
			return text;
		}
		return ToHtml((Color)value);
	}

	private static string ToHtml(Color c)
	{
		string empty = string.Empty;
		if (c.IsEmpty)
		{
			return empty;
		}
		if (c.IsNamedColor)
		{
			empty = ((c == Color.LightGray) ? "LightGrey" : c.Name);
			return empty.ToLowerInvariant();
		}
		return $"#{c.R:X2}{c.G:X2}{c.B:X2}";
	}

	private static Color Hsl2Rgb(double h, double sl, double l)
	{
		double num = l;
		double num2 = l;
		double num3 = l;
		double num4 = ((l <= 0.5) ? (l * (1.0 + sl)) : (l + sl - l * sl));
		if (num4 > 0.0)
		{
			double num5 = l + l - num4;
			double num6 = (num4 - num5) / num4;
			h *= 6.0;
			int num7 = (int)h;
			double num8 = h - (double)num7;
			double num9 = num4 * num6 * num8;
			double num10 = num5 + num9;
			double num11 = num4 - num9;
			switch (num7)
			{
			case 0:
				num = num4;
				num2 = num10;
				num3 = num5;
				break;
			case 1:
				num = num11;
				num2 = num4;
				num3 = num5;
				break;
			case 2:
				num = num5;
				num2 = num4;
				num3 = num10;
				break;
			case 3:
				num = num5;
				num2 = num11;
				num3 = num4;
				break;
			case 4:
				num = num10;
				num2 = num5;
				num3 = num4;
				break;
			case 5:
				num = num4;
				num2 = num5;
				num3 = num11;
				break;
			}
		}
		return Color.FromArgb((int)Math.Round(num * 255.0), (int)Math.Round(num2 * 255.0), (int)Math.Round(num3 * 255.0));
	}
}
