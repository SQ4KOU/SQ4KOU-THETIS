#define TRACE
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using Svg.ExtensionMethods;
using Svg.FilterEffects;

namespace Svg;

public abstract class SvgVisualElement : SvgElement, ISvgStylable, ISvgBoundable, ISvgClipable
{
	private bool? _requiresSmoothRendering;

	private Region _previousClip;

	internal static List<Type> SvgVisualElementClassNames = new List<Type> { typeof(SvgVisualElement) };

	internal static Dictionary<string, ISvgPropertyDescriptor> SvgVisualElementProperties = new Dictionary<string, ISvgPropertyDescriptor>
	{
		["clip"] = new SvgPropertyDescriptor<SvgVisualElement, string>(DescriptorType.Property, "clip", "http://www.w3.org/2000/svg", new StringConverter(), (SvgVisualElement t) => t.Clip, delegate(SvgVisualElement t, string v)
		{
			t.Clip = v;
		}),
		["clip-path"] = new SvgPropertyDescriptor<SvgVisualElement, Uri>(DescriptorType.Property, "clip-path", "http://www.w3.org/2000/svg", new UriTypeConverter(), (SvgVisualElement t) => t.ClipPath, delegate(SvgVisualElement t, Uri v)
		{
			t.ClipPath = v;
		}),
		["clip-rule"] = new SvgPropertyDescriptor<SvgVisualElement, SvgClipRule>(DescriptorType.Property, "clip-rule", "http://www.w3.org/2000/svg", new SvgClipRuleConverter(), (SvgVisualElement t) => t.ClipRule, delegate(SvgVisualElement t, SvgClipRule v)
		{
			t.ClipRule = v;
		}),
		["filter"] = new SvgPropertyDescriptor<SvgVisualElement, Uri>(DescriptorType.Property, "filter", "http://www.w3.org/2000/svg", new UriTypeConverter(), (SvgVisualElement t) => t.Filter, delegate(SvgVisualElement t, Uri v)
		{
			t.Filter = v;
		}),
		["enable-background"] = new SvgPropertyDescriptor<SvgVisualElement, string>(DescriptorType.Property, "enable-background", "http://www.w3.org/2000/svg", new StringConverter(), (SvgVisualElement t) => t.EnableBackground, delegate(SvgVisualElement t, string v)
		{
			t.EnableBackground = v;
		})
	};

	[SvgAttribute("clip")]
	public virtual string Clip
	{
		get
		{
			return GetAttribute("clip", inherited: true, "auto");
		}
		set
		{
			Attributes["clip"] = value;
		}
	}

	[SvgAttribute("clip-path")]
	public virtual Uri ClipPath
	{
		get
		{
			return GetAttribute<Uri>("clip-path", inherited: false);
		}
		set
		{
			Attributes["clip-path"] = value;
		}
	}

	[SvgAttribute("clip-rule")]
	public SvgClipRule ClipRule
	{
		get
		{
			return GetAttribute("clip-rule", inherited: true, SvgClipRule.NonZero);
		}
		set
		{
			Attributes["clip-rule"] = value;
		}
	}

	[SvgAttribute("filter")]
	public virtual Uri Filter
	{
		get
		{
			return GetAttribute<Uri>("filter", inherited: false);
		}
		set
		{
			Attributes["filter"] = value;
		}
	}

	protected virtual bool RequiresSmoothRendering
	{
		get
		{
			if (!_requiresSmoothRendering.HasValue)
			{
				_requiresSmoothRendering = ConvertShapeRendering2AntiAlias(ShapeRendering);
			}
			return _requiresSmoothRendering.Value;
		}
	}

	protected virtual bool Renderable => true;

	PointF ISvgBoundable.Location => Bounds.Location;

	SizeF ISvgBoundable.Size => Bounds.Size;

	public abstract RectangleF Bounds { get; }

	public virtual bool Visible => string.Equals(Visibility.Trim(), "visible", StringComparison.OrdinalIgnoreCase);

	protected virtual bool Displayable => !string.Equals(Display.Trim(), "none", StringComparison.OrdinalIgnoreCase);

	[SvgAttribute("enable-background")]
	public virtual string EnableBackground
	{
		get
		{
			return GetAttribute("enable-background", inherited: false, "accumulate");
		}
		set
		{
			Attributes["enable-background"] = value;
		}
	}

	internal override string AttributeName => "";

	internal override List<Type> ClassNames => SvgVisualElementClassNames;

	internal override Dictionary<string, ISvgPropertyDescriptor> Properties => SvgVisualElementProperties;

	private bool ConvertShapeRendering2AntiAlias(SvgShapeRendering shapeRendering)
	{
		if ((uint)(shapeRendering - 2) <= 1u)
		{
			return false;
		}
		return true;
	}

	public SvgVisualElement()
	{
		IsPathDirty = true;
	}

	public abstract GraphicsPath Path(ISvgRenderer renderer);

	protected override void Render(ISvgRenderer renderer)
	{
		if (Visible && Displayable && (!Renderable || Path(renderer) != null))
		{
			RenderInternal(renderer, renderFilter: true);
		}
	}

	private void RenderInternal(ISvgRenderer renderer, bool renderFilter)
	{
		if (renderFilter && RenderFilter(renderer))
		{
			return;
		}
		float opacity = SvgElement.FixOpacityValue(Opacity);
		if (opacity == 1f)
		{
			if (Renderable)
			{
				RenderInternal(renderer, RenderFillAndStroke);
			}
			else
			{
				RenderInternal(renderer, RenderChildren);
			}
			return;
		}
		IsPathDirty = true;
		RectangleF bounds = (Renderable ? Bounds : Path(null).GetBounds());
		IsPathDirty = true;
		if (!(bounds.Width > 0f) || !(bounds.Height > 0f))
		{
			return;
		}
		float num = 1f;
		float num2 = 1f;
		using (Matrix matrix = renderer.Transform)
		{
			num = Math.Max(num, Math.Abs(matrix.Elements[0]));
			num2 = Math.Max(num2, Math.Abs(matrix.Elements[3]));
		}
		Bitmap canvas = new Bitmap((int)Math.Ceiling(bounds.Width * num), (int)Math.Ceiling(bounds.Height * num2));
		try
		{
			using (ISvgRenderer svgRenderer = SvgRenderer.FromImage(canvas))
			{
				svgRenderer.SetBoundable(renderer.GetBoundable());
				svgRenderer.TranslateTransform(0f - bounds.X, 0f - bounds.Y);
				svgRenderer.ScaleTransform(num, num2);
				if (Renderable)
				{
					RenderInternal(svgRenderer, RenderFillAndStroke);
				}
				else
				{
					RenderChildren(svgRenderer);
				}
			}
			RectangleF srcRect = new RectangleF(0f, 0f, bounds.Width * num, bounds.Height * num2);
			if (Renderable)
			{
				renderer.DrawImage(canvas, bounds, srcRect, GraphicsUnit.Pixel, opacity);
				return;
			}
			RenderInternal(renderer, delegate(ISvgRenderer r)
			{
				r.DrawImage(canvas, bounds, srcRect, GraphicsUnit.Pixel, opacity);
			});
		}
		finally
		{
			if (canvas != null)
			{
				((IDisposable)canvas).Dispose();
			}
		}
	}

	private void RenderInternal(ISvgRenderer renderer, Action<ISvgRenderer> renderMethod)
	{
		try
		{
			if (PushTransforms(renderer))
			{
				SetClip(renderer);
				renderMethod(renderer);
				ResetClip(renderer);
			}
		}
		finally
		{
			PopTransforms(renderer);
		}
	}

	private bool RenderFilter(ISvgRenderer renderer)
	{
		bool result = false;
		Uri uri = Filter.ReplaceWithNullIfNone();
		if (uri != null)
		{
			SvgElement elementById = OwnerDocument.IdManager.GetElementById(uri);
			if (elementById is SvgFilter)
			{
				try
				{
					((SvgFilter)elementById).ApplyFilter(this, renderer, delegate(ISvgRenderer r)
					{
						RenderInternal(r, renderFilter: false);
					});
				}
				catch (Exception)
				{
				}
				result = true;
			}
		}
		return result;
	}

	protected internal virtual void RenderFillAndStroke(ISvgRenderer renderer)
	{
		SmoothingMode smoothingMode = renderer.SmoothingMode;
		try
		{
			if (RequiresSmoothRendering)
			{
				renderer.SmoothingMode = SmoothingMode.AntiAlias;
			}
			RenderFill(renderer);
			RenderStroke(renderer);
		}
		finally
		{
			renderer.SmoothingMode = smoothingMode;
		}
	}

	protected internal virtual void RenderFill(ISvgRenderer renderer)
	{
		if (Fill == null)
		{
			return;
		}
		using Brush brush = Fill.GetBrush(this, renderer, SvgElement.FixOpacityValue(FillOpacity));
		if (brush != null)
		{
			GraphicsPath graphicsPath = Path(renderer);
			graphicsPath.FillMode = ((FillRule == SvgFillRule.NonZero) ? FillMode.Winding : FillMode.Alternate);
			renderer.FillPath(brush, graphicsPath);
		}
	}

	protected internal virtual bool RenderStroke(ISvgRenderer renderer)
	{
		if (Stroke != null && Stroke != SvgPaintServer.None && (float)StrokeWidth > 0f)
		{
			float strokeWidth = StrokeWidth.ToDeviceValue(renderer, UnitRenderingType.Other, this);
			using Brush brush = Stroke.GetBrush(this, renderer, SvgElement.FixOpacityValue(StrokeOpacity), forStroke: true);
			if (brush != null)
			{
				GraphicsPath graphicsPath = Path(renderer);
				RectangleF bounds = graphicsPath.GetBounds();
				if (graphicsPath.PointCount < 1)
				{
					return false;
				}
				if (!(bounds.Width <= 0f) || !(bounds.Height <= 0f))
				{
					using (Pen pen = new Pen(brush, strokeWidth))
					{
						if (StrokeDashArray != null && StrokeDashArray.Count > 0)
						{
							SvgUnitCollection svgUnitCollection = StrokeDashArray;
							if (svgUnitCollection.Count % 2 != 0)
							{
								svgUnitCollection = (SvgUnitCollection)StrokeDashArray.Clone();
								svgUnitCollection.AddRange(StrokeDashArray);
							}
							SvgUnit svgUnit = StrokeDashOffset;
							strokeWidth = Math.Max(strokeWidth, 1f);
							float[] array = svgUnitCollection.Select((SvgUnit u) => ((u.ToDeviceValue(renderer, UnitRenderingType.Other, this) <= 0f) ? 1f : u.ToDeviceValue(renderer, UnitRenderingType.Other, this)) / strokeWidth).ToArray();
							int num = array.Length;
							if (StrokeLineCap == SvgStrokeLineCap.Round)
							{
								float[] array2 = new float[num];
								int num2 = 1;
								for (int num3 = 0; num3 < num; num3++)
								{
									array2[num3] = array[num3] + (float)num2;
									if (array2[num3] <= 0f)
									{
										if (num3 < num - 1)
										{
											array[num3 - 1] += array[num3] + array[num3 + 1];
											num -= 2;
											for (int num4 = num3; num4 < num; num4++)
											{
												array[num4] = array[num4 + 2];
											}
											num3 -= 2;
										}
										else
										{
											if (num3 <= 2)
											{
												num = 0;
												break;
											}
											float num5 = array[num3 - 1] + array[num3];
											array2[0] += num5;
											num -= 2;
											svgUnit = (float)svgUnit + num5 * strokeWidth;
										}
									}
									num2 *= -1;
								}
								if (num > 0)
								{
									if (num < array.Length)
									{
										Array.Resize(ref array2, num);
									}
									array = array2;
									pen.DashCap = DashCap.Round;
								}
							}
							if (num > 0)
							{
								pen.DashPattern = array;
								if (svgUnit != 0f)
								{
									pen.DashOffset = ((svgUnit.ToDeviceValue(renderer, UnitRenderingType.Other, this) <= 0f) ? 1f : svgUnit.ToDeviceValue(renderer, UnitRenderingType.Other, this)) / strokeWidth;
								}
							}
						}
						switch (StrokeLineJoin)
						{
						case SvgStrokeLineJoin.Bevel:
							pen.LineJoin = LineJoin.Bevel;
							break;
						case SvgStrokeLineJoin.Round:
							pen.LineJoin = LineJoin.Round;
							break;
						case SvgStrokeLineJoin.MiterClip:
							pen.LineJoin = LineJoin.MiterClipped;
							break;
						default:
							pen.LineJoin = LineJoin.Miter;
							break;
						}
						pen.MiterLimit = StrokeMiterLimit;
						switch (StrokeLineCap)
						{
						case SvgStrokeLineCap.Round:
							pen.StartCap = LineCap.Round;
							pen.EndCap = LineCap.Round;
							break;
						case SvgStrokeLineCap.Square:
							pen.StartCap = LineCap.Square;
							pen.EndCap = LineCap.Square;
							break;
						}
						renderer.DrawPath(pen, graphicsPath);
						return true;
					}
				}
				switch (StrokeLineCap)
				{
				case SvgStrokeLineCap.Round:
				{
					using (GraphicsPath graphicsPath3 = new GraphicsPath())
					{
						graphicsPath3.AddEllipse(graphicsPath.PathPoints[0].X - strokeWidth / 2f, graphicsPath.PathPoints[0].Y - strokeWidth / 2f, strokeWidth, strokeWidth);
						renderer.FillPath(brush, graphicsPath3);
					}
					break;
				}
				case SvgStrokeLineCap.Square:
				{
					using (GraphicsPath graphicsPath2 = new GraphicsPath())
					{
						graphicsPath2.AddRectangle(new RectangleF(graphicsPath.PathPoints[0].X - strokeWidth / 2f, graphicsPath.PathPoints[0].Y - strokeWidth / 2f, strokeWidth, strokeWidth));
						renderer.FillPath(brush, graphicsPath2);
					}
					break;
				}
				}
			}
		}
		return false;
	}

	protected internal virtual void SetClip(ISvgRenderer renderer)
	{
		Uri uri = ClipPath.ReplaceWithNullIfNone();
		string clip = Clip;
		if (!(uri != null) && string.IsNullOrEmpty(clip))
		{
			return;
		}
		_previousClip = renderer.GetClip();
		if (uri != null)
		{
			SvgClipPath elementById = OwnerDocument.GetElementById<SvgClipPath>(uri.ToString());
			if (elementById != null)
			{
				renderer.SetClip(elementById.GetClipRegion(this, renderer), CombineMode.Intersect);
			}
		}
		if (!string.IsNullOrEmpty(clip) && clip.StartsWith("rect("))
		{
			clip = clip.Trim();
			List<float> list = (from o in clip.Substring(5, clip.Length - 6).Split(',')
				select float.Parse(o.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture)).ToList();
			RectangleF bounds = Bounds;
			RectangleF rect = new RectangleF(bounds.Left + list[3], bounds.Top + list[0], bounds.Width - (list[3] + list[1]), bounds.Height - (list[2] + list[0]));
			renderer.SetClip(new Region(rect), CombineMode.Intersect);
		}
	}

	protected internal virtual void ResetClip(ISvgRenderer renderer)
	{
		if (_previousClip != null)
		{
			renderer.SetClip(_previousClip);
			_previousClip = null;
		}
	}

	void ISvgClipable.SetClip(ISvgRenderer renderer)
	{
		SetClip(renderer);
	}

	void ISvgClipable.ResetClip(ISvgRenderer renderer)
	{
		ResetClip(renderer);
	}

	internal override IEnumerable<ISvgPropertyDescriptor> GetProperties()
	{
		foreach (KeyValuePair<string, ISvgPropertyDescriptor> svgVisualElementProperty in SvgVisualElementProperties)
		{
			yield return svgVisualElementProperty.Value;
		}
		foreach (ISvgPropertyDescriptor property in base.GetProperties())
		{
			if (!SvgVisualElementProperties.ContainsKey(property.AttributeName))
			{
				yield return property;
			}
		}
	}

	internal override object GetValue(string attributeName)
	{
		if (SvgVisualElementProperties.TryGetValue(attributeName, out var value))
		{
			return value.GetValue(this);
		}
		return base.GetValue(attributeName);
	}

	internal override bool SetValue(string attributeName, ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (SvgVisualElementProperties.TryGetValue(attributeName, out var value2))
		{
			try
			{
				value2.SetValue(this, context, culture, value);
			}
			catch
			{
				Trace.TraceWarning($"Attribute '{attributeName}' cannot be set - type '{GetType().FullName}' cannot convert from string '{value}'.");
			}
			return true;
		}
		return base.SetValue(attributeName, context, culture, value);
	}
}
