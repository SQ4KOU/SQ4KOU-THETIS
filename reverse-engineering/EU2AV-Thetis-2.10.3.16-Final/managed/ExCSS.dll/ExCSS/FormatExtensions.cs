using System.IO;
using System.Text;

namespace ExCSS;

public static class FormatExtensions
{
	public static string ToCss(this IStyleFormattable style)
	{
		return style.ToCss(CompressedStyleFormatter.Instance);
	}

	public static string ToCss(this IStyleFormattable style, IStyleFormatter formatter)
	{
		StringBuilder sb = Pool.NewStringBuilder();
		using (StringWriter writer = new StringWriter(sb))
		{
			style.ToCss(writer, formatter);
		}
		return sb.ToPool();
	}

	public static void ToCss(this IStyleFormattable style, TextWriter writer)
	{
		style.ToCss(writer, CompressedStyleFormatter.Instance);
	}
}
