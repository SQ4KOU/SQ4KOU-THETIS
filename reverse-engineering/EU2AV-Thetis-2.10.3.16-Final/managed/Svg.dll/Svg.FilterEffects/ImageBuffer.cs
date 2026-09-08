using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace Svg.FilterEffects;

public class ImageBuffer : Dictionary<string, Bitmap>, IDisposable
{
	private const string BufferKey = "__!!BUFFER";

	private readonly RectangleF _bounds;

	private readonly ISvgRenderer _renderer;

	private readonly Action<ISvgRenderer> _renderMethod;

	private readonly float _inflate;

	private Matrix _transform;

	public Matrix Transform
	{
		get
		{
			return _transform?.Clone();
		}
		set
		{
			_transform?.Dispose();
			_transform = value?.Clone();
		}
	}

	public Bitmap Buffer => this["__!!BUFFER"];

	public new Bitmap this[string key]
	{
		get
		{
			return ProcessResult(ProcessKey(key), base[ProcessKey(key)]);
		}
		set
		{
			base[string.IsNullOrEmpty(key) ? "__!!BUFFER" : key] = value;
		}
	}

	public ImageBuffer(RectangleF bounds, float inflate, ISvgRenderer renderer, Action<ISvgRenderer> renderMethod)
	{
		_bounds = bounds;
		_inflate = inflate;
		_renderer = renderer;
		_renderMethod = renderMethod;
		this["SourceGraphic"] = null;
		this["SourceAlpha"] = null;
		this["BackgroundImage"] = null;
		this["BackgroundAlpha"] = null;
		this["FillPaint"] = null;
		this["StrokePaint"] = null;
	}

	public new void Add(string key, Bitmap value)
	{
		base.Add(ProcessKey(key), value);
	}

	public new bool ContainsKey(string key)
	{
		return base.ContainsKey(ProcessKey(key));
	}

	public new void Clear()
	{
		this.Clear(delegate(Bitmap i)
		{
			i?.Dispose();
		});
	}

	public new bool Remove(string key)
	{
		switch (ProcessKey(key))
		{
		case "SourceGraphic":
		case "SourceAlpha":
		case "BackgroundImage":
		case "BackgroundAlpha":
		case "FillPaint":
		case "StrokePaint":
			return false;
		default:
			return this.Remove(ProcessKey(key), delegate(Bitmap i)
			{
				i?.Dispose();
			});
		}
	}

	public new bool TryGetValue(string key, out Bitmap value)
	{
		if (base.TryGetValue(ProcessKey(key), out value))
		{
			value = ProcessResult(ProcessKey(key), value);
			return true;
		}
		return false;
	}

	private Bitmap ProcessResult(string key, Bitmap curr)
	{
		if (curr == null)
		{
			switch (key)
			{
			case "SourceGraphic":
				this[key] = CreateSourceGraphic();
				return this[key];
			case "SourceAlpha":
				this[key] = CreateSourceAlpha();
				return this[key];
			case "BackgroundImage":
			case "BackgroundAlpha":
			case "FillPaint":
			case "StrokePaint":
				return null;
			}
		}
		return curr;
	}

	private string ProcessKey(string key)
	{
		if (!string.IsNullOrEmpty(key))
		{
			return key;
		}
		if (!ContainsKey("__!!BUFFER"))
		{
			return "SourceGraphic";
		}
		return "__!!BUFFER";
	}

	private Bitmap CreateSourceGraphic()
	{
		Bitmap bitmap = new Bitmap((int)(_bounds.Width + 2f * _inflate * _bounds.Width + _bounds.X), (int)(_bounds.Height + 2f * _inflate * _bounds.Height + _bounds.Y));
		using ISvgRenderer svgRenderer = SvgRenderer.FromImage(bitmap);
		using Matrix matrix = new Matrix();
		svgRenderer.SetBoundable(_renderer.GetBoundable());
		matrix.Translate(_bounds.Width * _inflate, _bounds.Height * _inflate);
		svgRenderer.Transform = matrix;
		_renderMethod(svgRenderer);
		return bitmap;
	}

	private Bitmap CreateSourceAlpha()
	{
		Bitmap bitmap = this["SourceGraphic"];
		ColorMatrix colorMatrix = new ColorMatrix(new float[5][]
		{
			new float[5],
			new float[5],
			new float[5],
			new float[5] { 0f, 0f, 0f, 1f, 1f },
			new float[5]
		});
		Bitmap bitmap2 = new Bitmap(bitmap.Width, bitmap.Height);
		using Graphics graphics = Graphics.FromImage(bitmap2);
		using ImageAttributes imageAttributes = new ImageAttributes();
		imageAttributes.SetColorMatrix(colorMatrix);
		graphics.DrawImage(bitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height), 0, 0, bitmap.Width, bitmap.Height, GraphicsUnit.Pixel, imageAttributes);
		graphics.Save();
		return bitmap2;
	}

	public void Dispose()
	{
		Clear();
		_transform?.Dispose();
	}
}
