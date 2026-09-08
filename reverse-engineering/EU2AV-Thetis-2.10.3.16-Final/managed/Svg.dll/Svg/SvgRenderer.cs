using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;

namespace Svg;

public sealed class SvgRenderer : ISvgRenderer, IDisposable, IGraphicsProvider
{
	private readonly Graphics _innerGraphics;

	private readonly bool _disposable;

	private readonly Image _image;

	private readonly Stack<ISvgBoundable> _boundables = new Stack<ISvgBoundable>();

	public float DpiY => _innerGraphics.DpiY;

	public SmoothingMode SmoothingMode
	{
		get
		{
			return _innerGraphics.SmoothingMode;
		}
		set
		{
			_innerGraphics.SmoothingMode = value;
		}
	}

	public Matrix Transform
	{
		get
		{
			return _innerGraphics.Transform;
		}
		set
		{
			_innerGraphics.Transform = value;
		}
	}

	public void SetBoundable(ISvgBoundable boundable)
	{
		_boundables.Push(boundable);
	}

	public ISvgBoundable GetBoundable()
	{
		if (_boundables.Count <= 0)
		{
			return null;
		}
		return _boundables.Peek();
	}

	public ISvgBoundable PopBoundable()
	{
		return _boundables.Pop();
	}

	private SvgRenderer(Graphics graphics, bool disposable = true)
	{
		_innerGraphics = graphics;
		_disposable = disposable;
	}

	private SvgRenderer(Graphics graphics, Image image)
		: this(graphics)
	{
		_image = image;
	}

	public void DrawImage(Image image, RectangleF destRect, RectangleF srcRect, GraphicsUnit graphicsUnit)
	{
		_innerGraphics.DrawImage(image, destRect, srcRect, graphicsUnit);
	}

	public void DrawImage(Image image, RectangleF destRect, RectangleF srcRect, GraphicsUnit graphicsUnit, float opacity)
	{
		using ImageAttributes imageAttributes = new ImageAttributes();
		ColorMatrix newColorMatrix = new ColorMatrix
		{
			Matrix33 = opacity
		};
		imageAttributes.SetColorMatrix(newColorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
		PointF[] destPoints = new PointF[3]
		{
			destRect.Location,
			new PointF(destRect.X + destRect.Width, destRect.Y),
			new PointF(destRect.X, destRect.Y + destRect.Height)
		};
		_innerGraphics.DrawImage(image, destPoints, srcRect, graphicsUnit, imageAttributes);
	}

	public void DrawImageUnscaled(Image image, Point location)
	{
		_innerGraphics.DrawImageUnscaled(image, location);
	}

	public void DrawPath(Pen pen, GraphicsPath path)
	{
		_innerGraphics.DrawPath(pen, path);
	}

	public void FillPath(Brush brush, GraphicsPath path)
	{
		_innerGraphics.FillPath(brush, path);
	}

	public Region GetClip()
	{
		return _innerGraphics.Clip;
	}

	public void RotateTransform(float fAngle, MatrixOrder order = MatrixOrder.Append)
	{
		_innerGraphics.RotateTransform(fAngle, order);
	}

	public void ScaleTransform(float sx, float sy, MatrixOrder order = MatrixOrder.Append)
	{
		_innerGraphics.ScaleTransform(sx, sy, order);
	}

	public void SetClip(Region region, CombineMode combineMode = CombineMode.Replace)
	{
		_innerGraphics.SetClip(region, combineMode);
	}

	public void TranslateTransform(float dx, float dy, MatrixOrder order = MatrixOrder.Append)
	{
		_innerGraphics.TranslateTransform(dx, dy, order);
	}

	public void Dispose()
	{
		if (_disposable)
		{
			_innerGraphics.Dispose();
		}
		if (_image != null)
		{
			_image.Dispose();
		}
	}

	Graphics IGraphicsProvider.GetGraphics()
	{
		return _innerGraphics;
	}

	private static Graphics CreateGraphics(Image image)
	{
		Graphics graphics = Graphics.FromImage(image);
		graphics.PixelOffsetMode = PixelOffsetMode.Half;
		graphics.CompositingQuality = CompositingQuality.HighQuality;
		graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
		graphics.TextContrast = 1;
		graphics.InterpolationMode = InterpolationMode.Default;
		return graphics;
	}

	public static ISvgRenderer FromImage(Image image)
	{
		return new SvgRenderer(CreateGraphics(image));
	}

	public static ISvgRenderer FromGraphics(Graphics graphics)
	{
		return new SvgRenderer(graphics, disposable: false);
	}

	public static ISvgRenderer FromNull()
	{
		Bitmap image = new Bitmap(1, 1);
		return new SvgRenderer(CreateGraphics(image), image);
	}
}
