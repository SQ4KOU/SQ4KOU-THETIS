#define TRACE
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using Svg.Pathing;

namespace Svg;

public class SvgPathBuilder : TypeConverter
{
	public static SvgPathSegmentList Parse(ReadOnlySpan<char> path)
	{
		SvgPathSegmentList svgPathSegmentList = new SvgPathSegmentList();
		try
		{
			ReadOnlySpan<char> readOnlySpan = path.TrimEnd();
			int num = 0;
			int length = readOnlySpan.Length;
			for (int i = 0; i < length; i++)
			{
				char c = readOnlySpan[i];
				if (char.IsLetter(c) && c != 'e' && c != 'E')
				{
					int start = num;
					int length2 = i - num;
					ReadOnlySpan<char> readOnlySpan2 = readOnlySpan.Slice(start, length2).Trim();
					num = i;
					if (readOnlySpan2.Length > 0)
					{
						ReadOnlySpan<char> chars = readOnlySpan.Slice(start, length2).Trim();
						CoordinateParserState state = new CoordinateParserState(ref chars);
						CreatePathSegment(chars[0], svgPathSegmentList, ref state, chars);
					}
					if (length == i + 1)
					{
						ReadOnlySpan<char> chars2 = readOnlySpan.Slice(i, 1).Trim();
						CoordinateParserState state2 = new CoordinateParserState(ref chars2);
						CreatePathSegment(chars2[0], svgPathSegmentList, ref state2, chars2);
					}
				}
				else if (length == i + 1)
				{
					int start2 = num;
					int length3 = i - num + 1;
					if (readOnlySpan.Slice(start2, length3).Trim().Length > 0)
					{
						ReadOnlySpan<char> chars3 = readOnlySpan.Slice(start2, length3).Trim();
						CoordinateParserState state3 = new CoordinateParserState(ref chars3);
						CreatePathSegment(chars3[0], svgPathSegmentList, ref state3, chars3);
					}
				}
			}
		}
		catch (Exception ex)
		{
			Trace.TraceError("Error parsing path \"{0}\": {1}", path.ToString(), ex.Message);
		}
		return svgPathSegmentList;
	}

	private static void CreatePathSegment(char command, SvgPathSegmentList segments, ref CoordinateParserState state, ReadOnlySpan<char> chars)
	{
		bool isRelative = char.IsLower(command);
		switch (command)
		{
		case 'M':
		case 'm':
		{
			if (CoordinateParser.TryGetFloat(out var result28, chars, ref state) && CoordinateParser.TryGetFloat(out var result29, chars, ref state))
			{
				segments.Add(new SvgMoveToSegment(isRelative, new PointF(result28, result29)));
			}
			while (CoordinateParser.TryGetFloat(out result28, chars, ref state) && CoordinateParser.TryGetFloat(out result29, chars, ref state))
			{
				segments.Add(new SvgLineSegment(isRelative, new PointF(result28, result29)));
			}
			break;
		}
		case 'A':
		case 'a':
		{
			float result21;
			float result22;
			float result23;
			bool result24;
			bool result25;
			float result26;
			float result27;
			while (CoordinateParser.TryGetFloat(out result21, chars, ref state) && CoordinateParser.TryGetFloat(out result22, chars, ref state) && CoordinateParser.TryGetFloat(out result23, chars, ref state) && CoordinateParser.TryGetBool(out result24, chars, ref state) && CoordinateParser.TryGetBool(out result25, chars, ref state) && CoordinateParser.TryGetFloat(out result26, chars, ref state) && CoordinateParser.TryGetFloat(out result27, chars, ref state))
			{
				segments.Add(new SvgArcSegment(result21, result22, result23, result24 ? SvgArcSize.Large : SvgArcSize.Small, result25 ? SvgArcSweep.Positive : SvgArcSweep.Negative, isRelative, new PointF(result26, result27)));
			}
			break;
		}
		case 'L':
		case 'l':
		{
			float result19;
			float result20;
			while (CoordinateParser.TryGetFloat(out result19, chars, ref state) && CoordinateParser.TryGetFloat(out result20, chars, ref state))
			{
				segments.Add(new SvgLineSegment(isRelative, new PointF(result19, result20)));
			}
			break;
		}
		case 'H':
		case 'h':
		{
			float result18;
			while (CoordinateParser.TryGetFloat(out result18, chars, ref state))
			{
				segments.Add(new SvgLineSegment(isRelative, new PointF(result18, float.NaN)));
			}
			break;
		}
		case 'V':
		case 'v':
		{
			float result17;
			while (CoordinateParser.TryGetFloat(out result17, chars, ref state))
			{
				segments.Add(new SvgLineSegment(isRelative, new PointF(float.NaN, result17)));
			}
			break;
		}
		case 'Q':
		case 'q':
		{
			float result13;
			float result14;
			float result15;
			float result16;
			while (CoordinateParser.TryGetFloat(out result13, chars, ref state) && CoordinateParser.TryGetFloat(out result14, chars, ref state) && CoordinateParser.TryGetFloat(out result15, chars, ref state) && CoordinateParser.TryGetFloat(out result16, chars, ref state))
			{
				segments.Add(new SvgQuadraticCurveSegment(isRelative, new PointF(result13, result14), new PointF(result15, result16)));
			}
			break;
		}
		case 'T':
		case 't':
		{
			float result11;
			float result12;
			while (CoordinateParser.TryGetFloat(out result11, chars, ref state) && CoordinateParser.TryGetFloat(out result12, chars, ref state))
			{
				segments.Add(new SvgQuadraticCurveSegment(isRelative, new PointF(result11, result12)));
			}
			break;
		}
		case 'C':
		case 'c':
		{
			float result5;
			float result6;
			float result7;
			float result8;
			float result9;
			float result10;
			while (CoordinateParser.TryGetFloat(out result5, chars, ref state) && CoordinateParser.TryGetFloat(out result6, chars, ref state) && CoordinateParser.TryGetFloat(out result7, chars, ref state) && CoordinateParser.TryGetFloat(out result8, chars, ref state) && CoordinateParser.TryGetFloat(out result9, chars, ref state) && CoordinateParser.TryGetFloat(out result10, chars, ref state))
			{
				segments.Add(new SvgCubicCurveSegment(isRelative, new PointF(result5, result6), new PointF(result7, result8), new PointF(result9, result10)));
			}
			break;
		}
		case 'S':
		case 's':
		{
			float result;
			float result2;
			float result3;
			float result4;
			while (CoordinateParser.TryGetFloat(out result, chars, ref state) && CoordinateParser.TryGetFloat(out result2, chars, ref state) && CoordinateParser.TryGetFloat(out result3, chars, ref state) && CoordinateParser.TryGetFloat(out result4, chars, ref state))
			{
				segments.Add(new SvgCubicCurveSegment(isRelative, new PointF(result, result2), new PointF(result3, result4)));
			}
			break;
		}
		case 'Z':
		case 'z':
			segments.Add(new SvgClosePathSegment(isRelative));
			break;
		}
	}

	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (value is string text)
		{
			return Parse(text.AsSpan());
		}
		return base.ConvertFrom(context, culture, value);
	}
}
