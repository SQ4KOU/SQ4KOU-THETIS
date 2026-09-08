using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace Svg;

public class GdiFontDefn : IFontDefn, IDisposable
{
	private readonly Font _font;

	private float _ppi;

	public float Size => _font.Size;

	public float SizeInPoints => _font.SizeInPoints;

	public GdiFontDefn(Font font, float ppi)
	{
		_font = font;
		_ppi = ppi;
	}

	public void AddStringToPath(ISvgRenderer renderer, GraphicsPath path, string text, PointF location)
	{
		path.AddString(text, _font.FontFamily, (int)_font.Style, _font.Size, location, StringFormat.GenericTypographic);
	}

	public float Ascent(ISvgRenderer renderer)
	{
		FontFamily fontFamily = _font.FontFamily;
		int cellAscent = fontFamily.GetCellAscent(_font.Style);
		float num = _font.SizeInPoints / (float)fontFamily.GetEmHeight(_font.Style) * (float)cellAscent;
		return _ppi / 72f * num;
	}

	public IList<RectangleF> MeasureCharacters(ISvgRenderer renderer, string text)
	{
		Graphics g = GetGraphics(renderer);
		List<RectangleF> list = new List<RectangleF>();
		using StringFormat stringFormat = new StringFormat(StringFormat.GenericTypographic);
		stringFormat.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces;
		PointF pointF = new PointF(0f, 0f);
		RectangleF layoutRect = new RectangleF(pointF, new SizeF(System.Drawing.Size.Ceiling(g.MeasureString(text, _font, pointF, stringFormat)).Width, 1000f));
		for (int i = 0; i <= (text.Length - 1) / 32; i++)
		{
			int count = Math.Min(32, text.Length - 32 * i);
			stringFormat.SetMeasurableCharacterRanges((from r in Enumerable.Range(32 * i, count)
				select new CharacterRange(r, 1)).ToArray());
			list.AddRange(from r in g.MeasureCharacterRanges(text, _font, layoutRect, stringFormat)
				select r.GetBounds(g));
		}
		return list;
	}

	public SizeF MeasureString(ISvgRenderer renderer, string text)
	{
		Graphics graphics = GetGraphics(renderer);
		using StringFormat stringFormat = new StringFormat(StringFormat.GenericTypographic);
		stringFormat.SetMeasurableCharacterRanges(new CharacterRange[1]
		{
			new CharacterRange(0, text.Length)
		});
		stringFormat.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces;
		return new SizeF(graphics.MeasureCharacterRanges(text, _font, new Rectangle(0, 0, 1000, 1000), stringFormat)[0].GetBounds(graphics).Width, Ascent(renderer));
	}

	private Graphics GetGraphics(ISvgRenderer renderer)
	{
		return ((renderer as IGraphicsProvider) ?? throw new NotImplementedException("renderer is not IGraphicsProvider")).GetGraphics();
	}

	public void Dispose()
	{
		_font.Dispose();
	}
}
