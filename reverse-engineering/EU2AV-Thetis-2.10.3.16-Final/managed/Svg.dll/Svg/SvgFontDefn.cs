using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace Svg;

public class SvgFontDefn : IFontDefn, IDisposable
{
	private SvgFont _font;

	private float _emScale;

	private float _ppi;

	private float _size;

	private Dictionary<string, SvgGlyph> _glyphs;

	private Dictionary<string, SvgKern> _kerning;

	public float Size => _size;

	public float SizeInPoints => _size * 72f / _ppi;

	public SvgFontDefn(SvgFont font, float size, float ppi)
	{
		_font = font;
		_size = size;
		_ppi = ppi;
		SvgFontFace svgFontFace = _font.Children.OfType<SvgFontFace>().First();
		_emScale = _size / svgFontFace.UnitsPerEm;
	}

	public float Ascent(ISvgRenderer renderer)
	{
		float ascent = _font.Descendants().OfType<SvgFontFace>().First()
			.Ascent;
		float num = SizeInPoints * (_emScale / _size) * ascent;
		return _ppi / 72f * num;
	}

	public IList<RectangleF> MeasureCharacters(ISvgRenderer renderer, string text)
	{
		List<RectangleF> list = new List<RectangleF>();
		using (GetPath(renderer, text, list, measureSpaces: false))
		{
			return list;
		}
	}

	public SizeF MeasureString(ISvgRenderer renderer, string text)
	{
		List<RectangleF> list = new List<RectangleF>();
		using (GetPath(renderer, text, list, measureSpaces: true))
		{
		}
		float? num = null;
		float? num2 = null;
		foreach (RectangleF item in list.Where((RectangleF r) => r != RectangleF.Empty))
		{
			float valueOrDefault = num.GetValueOrDefault();
			if (!num.HasValue)
			{
				valueOrDefault = item.Left;
				num = valueOrDefault;
			}
			num2 = item.Right;
		}
		if (!num.HasValue)
		{
			return SizeF.Empty;
		}
		return new SizeF(num2.Value - num.Value, Ascent(renderer));
	}

	public void AddStringToPath(ISvgRenderer renderer, GraphicsPath path, string text, PointF location)
	{
		GraphicsPath path2 = GetPath(renderer, text, null, measureSpaces: false);
		if (path2.PointCount > 0)
		{
			using (Matrix matrix = new Matrix())
			{
				matrix.Translate(location.X, location.Y);
				path2.Transform(matrix);
				path.AddPath(path2, connect: false);
			}
		}
	}

	private GraphicsPath GetPath(ISvgRenderer renderer, string text, IList<RectangleF> ranges, bool measureSpaces)
	{
		EnsureDictionaries();
		SvgGlyph svgGlyph = null;
		float num = 0f;
		float num2 = Ascent(renderer);
		GraphicsPath graphicsPath = new GraphicsPath();
		if (string.IsNullOrEmpty(text))
		{
			return graphicsPath;
		}
		for (int i = 0; i < text.Length; i++)
		{
			if (!_glyphs.TryGetValue(text.Substring(i, 1), out var value))
			{
				value = _font.Descendants().OfType<SvgMissingGlyph>().First();
			}
			if (svgGlyph != null && _kerning.TryGetValue(svgGlyph.GlyphName + "|" + value.GlyphName, out var value2))
			{
				num -= value2.Kerning * _emScale;
			}
			GraphicsPath graphicsPath2 = (GraphicsPath)value.Path(renderer).Clone();
			Matrix matrix = new Matrix();
			matrix.Scale(_emScale, -1f * _emScale, MatrixOrder.Append);
			matrix.Translate(num, num2, MatrixOrder.Append);
			graphicsPath2.Transform(matrix);
			matrix.Dispose();
			RectangleF bounds = graphicsPath2.GetBounds();
			if (ranges != null)
			{
				if (measureSpaces && bounds == RectangleF.Empty)
				{
					ranges.Add(new RectangleF(num, 0f, value.HorizAdvX * _emScale, num2));
				}
				else
				{
					ranges.Add(bounds);
				}
			}
			if (graphicsPath2.PointCount > 0)
			{
				graphicsPath.AddPath(graphicsPath2, connect: false);
			}
			num += value.HorizAdvX * _emScale;
			svgGlyph = value;
		}
		return graphicsPath;
	}

	private void EnsureDictionaries()
	{
		if (_glyphs == null)
		{
			_glyphs = _font.Descendants().OfType<SvgGlyph>().ToDictionary((SvgGlyph g) => g.Unicode ?? g.GlyphName ?? g.ID);
		}
		if (_kerning == null)
		{
			_kerning = _font.Descendants().OfType<SvgKern>().ToDictionary((SvgKern k) => k.Glyph1 + "|" + k.Glyph2);
		}
	}

	public void Dispose()
	{
		_glyphs = null;
		_kerning = null;
	}
}
