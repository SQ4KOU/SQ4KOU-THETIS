#define TRACE
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Svg;

public abstract class SvgTextBase : SvgVisualElement
{
	private class FontBoundable : ISvgBoundable
	{
		private IFontDefn _font;

		private float _width = 1f;

		public PointF Location => PointF.Empty;

		public SizeF Size => new SizeF(_width, _font.Size);

		public RectangleF Bounds => new RectangleF(Location, Size);

		public FontBoundable(IFontDefn font)
		{
			_font = font;
		}

		public FontBoundable(IFontDefn font, float width)
		{
			_font = font;
			_width = width;
		}
	}

	private class TextDrawingState
	{
		private float _xAnchor = float.MinValue;

		private IList<GraphicsPath> _anchoredPaths = new List<GraphicsPath>();

		private GraphicsPath _currPath;

		private GraphicsPath _finalPath;

		private float _authorPathLength;

		public GraphicsPath BaselinePath { get; set; }

		public PointF Current { get; set; }

		public RectangleF TextBounds { get; set; }

		public SvgTextBase Element { get; set; }

		public float LetterSpacingAdjust { get; set; }

		public int NumChars { get; set; }

		public TextDrawingState Parent { get; set; }

		public ISvgRenderer Renderer { get; set; }

		public float StartOffsetAdjust { get; set; }

		private TextDrawingState()
		{
		}

		public TextDrawingState(ISvgRenderer renderer, SvgTextBase element)
		{
			Element = element;
			Renderer = renderer;
			Current = PointF.Empty;
			TextBounds = RectangleF.Empty;
			_xAnchor = 0f;
			BaselinePath = element.GetBaselinePath(renderer);
			_authorPathLength = element.GetAuthorPathLength();
		}

		public TextDrawingState(TextDrawingState parent, SvgTextBase element)
			: this(parent.Renderer, element)
		{
			Parent = parent;
			Current = parent.Current;
			TextBounds = parent.TextBounds;
			BaselinePath = BaselinePath ?? parent.BaselinePath;
			if (_authorPathLength == 0f)
			{
				_authorPathLength = parent._authorPathLength;
			}
		}

		public GraphicsPath GetPath()
		{
			FlushPath();
			return _finalPath;
		}

		public TextDrawingState Clone()
		{
			return new TextDrawingState
			{
				_anchoredPaths = _anchoredPaths.ToList(),
				BaselinePath = BaselinePath,
				_xAnchor = _xAnchor,
				Current = Current,
				TextBounds = TextBounds,
				Element = Element,
				NumChars = NumChars,
				Parent = Parent,
				Renderer = Renderer
			};
		}

		public void DrawString(string value)
		{
			IList<float> values = GetValues(value.Length, (SvgTextBase e) => e._x, UnitRenderingType.HorizontalOffset);
			IList<float> values2 = GetValues(value.Length, (SvgTextBase e) => e._y, UnitRenderingType.VerticalOffset);
			using SvgFontManager fontManager = ((Element.OwnerDocument?.FontManager == null) ? new SvgFontManager() : null);
			using IFontDefn fontDefn = Element.GetFont(Renderer, fontManager);
			float fontBaselineHeight = fontDefn.Ascent(Renderer);
			PathStatistics pathStatistics = null;
			double num = 1.0;
			if (BaselinePath != null)
			{
				pathStatistics = new PathStatistics(BaselinePath.PathData);
				if (_authorPathLength > 0f)
				{
					num = (double)_authorPathLength / pathStatistics.TotalLength;
				}
			}
			float num2 = 0f;
			IList<float> values3;
			IList<float> values4;
			IList<float> values5;
			try
			{
				Renderer.SetBoundable(new FontBoundable(fontDefn, (float)(pathStatistics?.TotalLength ?? 1.0)));
				values3 = GetValues(value.Length, (SvgTextBase e) => e._dx, UnitRenderingType.Horizontal);
				values4 = GetValues(value.Length, (SvgTextBase e) => e._dy, UnitRenderingType.Vertical);
				if (StartOffsetAdjust != 0f)
				{
					if (values3.Count < 1)
					{
						values3.Add(StartOffsetAdjust);
					}
					else
					{
						values3[0] += StartOffsetAdjust;
					}
				}
				if (Element.LetterSpacing.Value != 0f || Element.WordSpacing.Value != 0f || LetterSpacingAdjust != 0f)
				{
					float num3 = Element.LetterSpacing.ToDeviceValue(Renderer, UnitRenderingType.Horizontal, Element) + LetterSpacingAdjust;
					float num4 = Element.WordSpacing.ToDeviceValue(Renderer, UnitRenderingType.Horizontal, Element);
					if (Parent == null && NumChars == 0 && values3.Count < 1)
					{
						values3.Add(0f);
					}
					for (int num5 = ((Parent == null && NumChars == 0) ? 1 : 0); num5 < value.Length; num5++)
					{
						if (num5 >= values3.Count)
						{
							values3.Add(num3 + (char.IsWhiteSpace(value[num5]) ? num4 : 0f));
						}
						else
						{
							values3[num5] += num3 + (char.IsWhiteSpace(value[num5]) ? num4 : 0f);
						}
					}
				}
				values5 = GetValues(value.Length, (SvgTextBase e) => e._rotations);
				string text = Element.BaselineShift.Trim().ToLower();
				if (string.IsNullOrEmpty(text))
				{
					text = "baseline";
				}
				switch (text)
				{
				case "sub":
					num2 = new SvgUnit(SvgUnitType.Ex, 1f).ToDeviceValue(Renderer, UnitRenderingType.Vertical, Element);
					break;
				case "super":
					num2 = -1f * new SvgUnit(SvgUnitType.Ex, 1f).ToDeviceValue(Renderer, UnitRenderingType.Vertical, Element);
					break;
				default:
					num2 = -1f * ((SvgUnit)new SvgUnitConverter().ConvertFromInvariantString(text)).ToDeviceValue(Renderer, UnitRenderingType.Vertical, Element);
					break;
				case "baseline":
					break;
				}
				if (num2 != 0f)
				{
					if (values4.Any())
					{
						values4[0] += num2;
					}
					else
					{
						values4.Add(num2);
					}
				}
			}
			finally
			{
				Renderer.PopBoundable();
			}
			float num6 = Current.X;
			float num7 = Current.Y;
			for (int num8 = 0; num8 < values.Count - 1; num8++)
			{
				FlushPath();
				_xAnchor = values[num8] + ((values3.Count > num8) ? values3[num8] : 0f);
				EnsurePath();
				num7 = ((values2.Count > num8) ? values2[num8] : num7) + ((values4.Count > num8) ? values4[num8] : 0f);
				num6 = (num6.Equals(Current.X) ? _xAnchor : num6);
				DrawStringOnCurrPath(value[num8].ToString(), fontDefn, new PointF(_xAnchor, num7), fontBaselineHeight, (values5.Count > num8) ? values5[num8] : values5.LastOrDefault());
			}
			int num9 = 0;
			float num10 = Current.X;
			if (values.Any())
			{
				FlushPath();
				num9 = values.Count - 1;
				num10 = (_xAnchor = values.Last());
			}
			EnsurePath();
			int num11 = num9 + Math.Max(Math.Max(Math.Max(Math.Max(values3.Count, values4.Count), values2.Count), values5.Count) - num9 - 1, 0);
			if (values5.LastOrDefault() != 0f || pathStatistics != null)
			{
				num11 = value.Length;
			}
			if (num11 > num9)
			{
				IList<RectangleF> list = fontDefn.MeasureCharacters(Renderer, value.Substring(num9, Math.Min(num11 + 1, value.Length) - num9));
				for (int num12 = num9; num12 < num11; num12++)
				{
					num10 += (float)num * ((values3.Count > num12) ? values3[num12] : 0f) + (list[num12 - num9].X - ((num12 == num9) ? 0f : list[num12 - num9 - 1].X));
					num7 = ((values2.Count > num12) ? values2[num12] : num7) + ((values4.Count > num12) ? values4[num12] : 0f);
					if (pathStatistics == null)
					{
						num6 = (num6.Equals(Current.X) ? num10 : num6);
						DrawStringOnCurrPath(value[num12].ToString(), fontDefn, new PointF(num10, num7), fontBaselineHeight, (values5.Count > num12) ? values5[num12] : values5.LastOrDefault());
						continue;
					}
					num10 = Math.Max(num10, 0f);
					float num13 = list[num12 - num9].Width / 2f;
					if (pathStatistics.OffsetOnPath(num10 + num13))
					{
						pathStatistics.LocationAngleAtOffset(num10 + num13, out var point, out var angle);
						point = new PointF((float)((double)point.X - (double)num13 * Math.Cos((double)angle * Math.PI / 180.0) - (double)((float)num * num7) * Math.Sin((double)angle * Math.PI / 180.0)), (float)((double)point.Y - (double)num13 * Math.Sin((double)angle * Math.PI / 180.0) + (double)((float)num * num7) * Math.Cos((double)angle * Math.PI / 180.0)));
						num6 = (num6.Equals(Current.X) ? point.X : num6);
						DrawStringOnCurrPath(value[num12].ToString(), fontDefn, point, fontBaselineHeight, angle);
					}
				}
				num10 = ((num11 >= value.Length) ? (num10 + list.Last().Width) : (num10 + (list[list.Count - 1].X - list[list.Count - 2].X)));
			}
			if (num11 < value.Length)
			{
				num10 += ((values3.Count > num11) ? values3[num11] : 0f);
				num7 = ((values2.Count > num11) ? values2[num11] : num7) + ((values4.Count > num11) ? values4[num11] : 0f);
				num6 = (num6.Equals(Current.X) ? num10 : num6);
				DrawStringOnCurrPath(value.Substring(num11), fontDefn, new PointF(num10, num7), fontBaselineHeight, values5.LastOrDefault());
				num10 += fontDefn.MeasureString(Renderer, value.Substring(num11)).Width;
			}
			NumChars += value.Length;
			Current = new PointF(num10, num7 - num2);
			TextBounds = new RectangleF(num6, 0f, Current.X - num6, 0f);
		}

		private void DrawStringOnCurrPath(string value, IFontDefn font, PointF location, float fontBaselineHeight, float rotation)
		{
			GraphicsPath graphicsPath = _currPath;
			if (rotation != 0f)
			{
				graphicsPath = new GraphicsPath();
			}
			font.AddStringToPath(Renderer, graphicsPath, value, new PointF(location.X, location.Y - fontBaselineHeight));
			if (rotation != 0f && graphicsPath.PointCount > 0)
			{
				using (Matrix matrix = new Matrix())
				{
					matrix.Translate(-1f * location.X, -1f * location.Y, MatrixOrder.Append);
					matrix.Rotate(rotation, MatrixOrder.Append);
					matrix.Translate(location.X, location.Y, MatrixOrder.Append);
					graphicsPath.Transform(matrix);
					_currPath.AddPath(graphicsPath, connect: false);
				}
			}
		}

		private void EnsurePath()
		{
			if (_currPath == null)
			{
				_currPath = new GraphicsPath();
				_currPath.StartFigure();
				TextDrawingState textDrawingState = this;
				while (textDrawingState != null && textDrawingState._xAnchor <= float.MinValue)
				{
					textDrawingState = textDrawingState.Parent;
				}
				textDrawingState._anchoredPaths.Add(_currPath);
			}
		}

		private void FlushPath()
		{
			if (_currPath == null)
			{
				return;
			}
			_currPath.CloseFigure();
			if (_currPath.PointCount < 1)
			{
				_anchoredPaths.Clear();
				_xAnchor = float.MinValue;
				_currPath = null;
				return;
			}
			if (_xAnchor > float.MinValue)
			{
				float num = float.MaxValue;
				float num2 = float.MinValue;
				foreach (GraphicsPath anchoredPath in _anchoredPaths)
				{
					RectangleF bounds = anchoredPath.GetBounds();
					if (bounds.Left < num)
					{
						num = bounds.Left;
					}
					if (bounds.Right > num2)
					{
						num2 = bounds.Right;
					}
				}
				float num3 = 0f;
				switch (Element.TextAnchor)
				{
				case SvgTextAnchor.Middle:
					num3 = ((_anchoredPaths.Count != 1) ? (num3 - (num2 - num) / 2f) : (num3 - TextBounds.Width / 2f));
					break;
				case SvgTextAnchor.End:
					num3 = ((_anchoredPaths.Count != 1) ? (num3 - (num2 - num)) : (num3 - TextBounds.Width));
					break;
				}
				if (num3 != 0f)
				{
					using Matrix matrix = new Matrix();
					matrix.Translate(num3, 0f);
					foreach (GraphicsPath anchoredPath2 in _anchoredPaths)
					{
						anchoredPath2.Transform(matrix);
					}
				}
				_anchoredPaths.Clear();
				_xAnchor = float.MinValue;
			}
			if (_finalPath == null)
			{
				_finalPath = _currPath;
			}
			else
			{
				_finalPath.AddPath(_currPath, connect: false);
			}
			_currPath = null;
		}

		private IList<float> GetValues(int maxCount, Func<SvgTextBase, IEnumerable<float>> listGetter)
		{
			TextDrawingState textDrawingState = this;
			int num = 0;
			List<float> list = new List<float>();
			int num2 = 0;
			while (textDrawingState != null)
			{
				num += textDrawingState.NumChars;
				list.AddRange(listGetter(textDrawingState.Element).Skip(num).Take(maxCount));
				if (list.Count > num2)
				{
					maxCount -= list.Count - num2;
					num += list.Count - num2;
					num2 = list.Count;
				}
				if (maxCount < 1)
				{
					return list;
				}
				textDrawingState = textDrawingState.Parent;
			}
			return list;
		}

		private IList<float> GetValues(int maxCount, Func<SvgTextBase, IEnumerable<SvgUnit>> listGetter, UnitRenderingType renderingType)
		{
			TextDrawingState currState = this;
			int num = 0;
			List<float> list = new List<float>();
			int num2 = 0;
			while (currState != null)
			{
				num += currState.NumChars;
				list.AddRange(from p in listGetter(currState.Element).Skip(num).Take(maxCount)
					select p.ToDeviceValue(currState.Renderer, renderingType, currState.Element));
				if (list.Count > num2)
				{
					maxCount -= list.Count - num2;
					num += list.Count - num2;
					num2 = list.Count;
				}
				if (maxCount < 1)
				{
					return list;
				}
				currState = currState.Parent;
			}
			return list;
		}
	}

	private SvgUnitCollection _x = new SvgUnitCollection();

	private SvgUnitCollection _y = new SvgUnitCollection();

	private SvgUnitCollection _dy = new SvgUnitCollection();

	private SvgUnitCollection _dx = new SvgUnitCollection();

	private string _rotate;

	private List<float> _rotations = new List<float>();

	private static readonly Regex MultipleSpaces = new Regex(" {2,}", RegexOptions.Compiled);

	private GraphicsPath _path;

	internal static List<Type> SvgTextBaseClassNames = new List<Type> { typeof(SvgTextBase) };

	internal static Dictionary<string, ISvgPropertyDescriptor> SvgTextBaseProperties = new Dictionary<string, ISvgPropertyDescriptor>
	{
		["x"] = new SvgPropertyDescriptor<SvgTextBase, SvgUnitCollection>(DescriptorType.Property, "x", "http://www.w3.org/2000/svg", new SvgUnitCollectionConverter(), (SvgTextBase t) => t.X, delegate(SvgTextBase t, SvgUnitCollection v)
		{
			t.X = v;
		}),
		["dx"] = new SvgPropertyDescriptor<SvgTextBase, SvgUnitCollection>(DescriptorType.Property, "dx", "http://www.w3.org/2000/svg", new SvgUnitCollectionConverter(), (SvgTextBase t) => t.Dx, delegate(SvgTextBase t, SvgUnitCollection v)
		{
			t.Dx = v;
		}),
		["y"] = new SvgPropertyDescriptor<SvgTextBase, SvgUnitCollection>(DescriptorType.Property, "y", "http://www.w3.org/2000/svg", new SvgUnitCollectionConverter(), (SvgTextBase t) => t.Y, delegate(SvgTextBase t, SvgUnitCollection v)
		{
			t.Y = v;
		}),
		["dy"] = new SvgPropertyDescriptor<SvgTextBase, SvgUnitCollection>(DescriptorType.Property, "dy", "http://www.w3.org/2000/svg", new SvgUnitCollectionConverter(), (SvgTextBase t) => t.Dy, delegate(SvgTextBase t, SvgUnitCollection v)
		{
			t.Dy = v;
		}),
		["rotate"] = new SvgPropertyDescriptor<SvgTextBase, string>(DescriptorType.Property, "rotate", "http://www.w3.org/2000/svg", new StringConverter(), (SvgTextBase t) => t.Rotate, delegate(SvgTextBase t, string v)
		{
			t.Rotate = v;
		}),
		["textLength"] = new SvgPropertyDescriptor<SvgTextBase, SvgUnit>(DescriptorType.Property, "textLength", "http://www.w3.org/2000/svg", new SvgUnitConverter(), (SvgTextBase t) => t.TextLength, delegate(SvgTextBase t, SvgUnit v)
		{
			t.TextLength = v;
		}),
		["lengthAdjust"] = new SvgPropertyDescriptor<SvgTextBase, SvgTextLengthAdjust>(DescriptorType.Property, "lengthAdjust", "http://www.w3.org/2000/svg", new SvgTextLengthAdjustConverter(), (SvgTextBase t) => t.LengthAdjust, delegate(SvgTextBase t, SvgTextLengthAdjust v)
		{
			t.LengthAdjust = v;
		}),
		["letter-spacing"] = new SvgPropertyDescriptor<SvgTextBase, SvgUnit>(DescriptorType.Property, "letter-spacing", "http://www.w3.org/2000/svg", new SvgUnitConverter(), (SvgTextBase t) => t.LetterSpacing, delegate(SvgTextBase t, SvgUnit v)
		{
			t.LetterSpacing = v;
		}),
		["word-spacing"] = new SvgPropertyDescriptor<SvgTextBase, SvgUnit>(DescriptorType.Property, "word-spacing", "http://www.w3.org/2000/svg", new SvgUnitConverter(), (SvgTextBase t) => t.WordSpacing, delegate(SvgTextBase t, SvgUnit v)
		{
			t.WordSpacing = v;
		}),
		["onchange"] = new SvgPropertyDescriptor<SvgTextBase, EventHandler<StringArg>>(DescriptorType.Event, "onchange", "http://www.w3.org/2000/svg", null, (SvgTextBase t) => t.Change, delegate(SvgTextBase t, EventHandler<StringArg> v)
		{
			t.Change += v;
		})
	};

	public virtual string Text
	{
		get
		{
			return Content;
		}
		set
		{
			base.Nodes.Clear();
			Children.Clear();
			if (value != null)
			{
				base.Nodes.Add(new SvgContentNode
				{
					Content = value
				});
			}
			Content = value;
			IsPathDirty = true;
		}
	}

	public override XmlSpaceHandling SpaceHandling
	{
		set
		{
			base.SpaceHandling = value;
			IsPathDirty = true;
		}
	}

	[SvgAttribute("x")]
	public virtual SvgUnitCollection X
	{
		get
		{
			return _x;
		}
		set
		{
			if (_x != value)
			{
				if (_x != null)
				{
					_x.CollectionChanged -= OnCoordinateChanged;
				}
				_x = value;
				if (_x != null)
				{
					_x.CollectionChanged += OnCoordinateChanged;
				}
				IsPathDirty = true;
			}
			Attributes["x"] = value;
		}
	}

	[SvgAttribute("dx")]
	public virtual SvgUnitCollection Dx
	{
		get
		{
			return _dx;
		}
		set
		{
			if (_dx != value)
			{
				if (_dx != null)
				{
					_dx.CollectionChanged -= OnCoordinateChanged;
				}
				_dx = value;
				if (_dx != null)
				{
					_dx.CollectionChanged += OnCoordinateChanged;
				}
				IsPathDirty = true;
			}
			Attributes["dx"] = value;
		}
	}

	[SvgAttribute("y")]
	public virtual SvgUnitCollection Y
	{
		get
		{
			return _y;
		}
		set
		{
			if (_y != value)
			{
				if (_y != null)
				{
					_y.CollectionChanged -= OnCoordinateChanged;
				}
				_y = value;
				if (_y != null)
				{
					_y.CollectionChanged += OnCoordinateChanged;
				}
				IsPathDirty = true;
			}
			Attributes["y"] = value;
		}
	}

	[SvgAttribute("dy")]
	public virtual SvgUnitCollection Dy
	{
		get
		{
			return _dy;
		}
		set
		{
			if (_dy != value)
			{
				if (_dy != null)
				{
					_dy.CollectionChanged -= OnCoordinateChanged;
				}
				_dy = value;
				if (_dy != null)
				{
					_dy.CollectionChanged += OnCoordinateChanged;
				}
				IsPathDirty = true;
			}
			Attributes["dy"] = value;
		}
	}

	[SvgAttribute("rotate")]
	public virtual string Rotate
	{
		get
		{
			return _rotate;
		}
		set
		{
			if (_rotate != value)
			{
				_rotate = value;
				_rotations.Clear();
				_rotations.AddRange(from r in _rotate.Split(new char[5] { ',', ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries)
					select float.Parse(r, NumberStyles.Any, CultureInfo.InvariantCulture));
				IsPathDirty = true;
			}
			Attributes["rotate"] = value;
		}
	}

	[SvgAttribute("textLength")]
	public virtual SvgUnit TextLength
	{
		get
		{
			return GetAttribute("textLength", inherited: true, SvgUnit.None);
		}
		set
		{
			Attributes["textLength"] = value;
			IsPathDirty = true;
		}
	}

	[SvgAttribute("lengthAdjust")]
	public virtual SvgTextLengthAdjust LengthAdjust
	{
		get
		{
			return GetAttribute("lengthAdjust", inherited: true, SvgTextLengthAdjust.Spacing);
		}
		set
		{
			Attributes["lengthAdjust"] = value;
			IsPathDirty = true;
		}
	}

	[SvgAttribute("letter-spacing")]
	public virtual SvgUnit LetterSpacing
	{
		get
		{
			return GetAttribute("letter-spacing", inherited: true, SvgUnit.None);
		}
		set
		{
			Attributes["letter-spacing"] = value;
			IsPathDirty = true;
		}
	}

	[SvgAttribute("word-spacing")]
	public virtual SvgUnit WordSpacing
	{
		get
		{
			return GetAttribute("word-spacing", inherited: true, SvgUnit.None);
		}
		set
		{
			Attributes["word-spacing"] = value;
			IsPathDirty = true;
		}
	}

	public override SvgPaintServer Fill
	{
		get
		{
			return GetAttribute("fill", true, (SvgPaintServer)new SvgColourServer(System.Drawing.Color.Black));
		}
		set
		{
			Attributes["fill"] = value;
		}
	}

	public override RectangleF Bounds
	{
		get
		{
			GraphicsPath graphicsPath = Path(null);
			foreach (SvgVisualElement item in Children.OfType<SvgVisualElement>())
			{
				if (!(item is SvgTextSpan svgTextSpan) || !string.IsNullOrWhiteSpace(svgTextSpan.Text))
				{
					graphicsPath.AddPath(item.Path(null), connect: false);
				}
			}
			if (base.Transforms == null || base.Transforms.Count == 0)
			{
				return graphicsPath.GetBounds();
			}
			using (graphicsPath = (GraphicsPath)graphicsPath.Clone())
			{
				using Matrix matrix = base.Transforms.GetMatrix();
				graphicsPath.Transform(matrix);
				return graphicsPath.GetBounds();
			}
		}
	}

	internal override string AttributeName => "";

	internal override List<Type> ClassNames => SvgTextBaseClassNames;

	internal override Dictionary<string, ISvgPropertyDescriptor> Properties => SvgTextBaseProperties;

	[SvgAttribute("onchange")]
	public event EventHandler<StringArg> Change;

	public SvgTextBase()
	{
		SvgTextBase svgTextBase = this;
		NotifyCollectionChangedEventHandler xHandler = null;
		_x.CollectionChanged += (xHandler = delegate(object s, NotifyCollectionChangedEventArgs e)
		{
			svgTextBase.Attributes["x"] = s;
			svgTextBase._x.CollectionChanged -= xHandler;
		});
		NotifyCollectionChangedEventHandler dxHandler = null;
		_dx.CollectionChanged += (dxHandler = delegate(object s, NotifyCollectionChangedEventArgs e)
		{
			svgTextBase.Attributes["dx"] = s;
			svgTextBase._dx.CollectionChanged -= dxHandler;
		});
		NotifyCollectionChangedEventHandler yHandler = null;
		_y.CollectionChanged += (yHandler = delegate(object s, NotifyCollectionChangedEventArgs e)
		{
			svgTextBase.Attributes["y"] = s;
			svgTextBase._y.CollectionChanged -= yHandler;
		});
		NotifyCollectionChangedEventHandler dyHandler = null;
		_dy.CollectionChanged += (dyHandler = delegate(object s, NotifyCollectionChangedEventArgs e)
		{
			svgTextBase.Attributes["dy"] = s;
			svgTextBase._dy.CollectionChanged -= dyHandler;
		});
		_x.CollectionChanged += OnCoordinateChanged;
		_dx.CollectionChanged += OnCoordinateChanged;
		_y.CollectionChanged += OnCoordinateChanged;
		_dy.CollectionChanged += OnCoordinateChanged;
	}

	private void OnCoordinateChanged(object sender, NotifyCollectionChangedEventArgs args)
	{
		IsPathDirty = true;
	}

	public override string ToString()
	{
		return Text;
	}

	protected string PrepareText(string value)
	{
		value = ApplyTransformation(value);
		value = new StringBuilder(value).Replace("\r\n", " ").Replace('\r', ' ').Replace('\n', ' ')
			.Replace('\t', ' ')
			.ToString();
		if (SpaceHandling != XmlSpaceHandling.Preserve)
		{
			return MultipleSpaces.Replace(value.Trim(), " ");
		}
		return value;
	}

	private string ApplyTransformation(string value)
	{
		return TextTransformation switch
		{
			SvgTextTransformation.Capitalize => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value), 
			SvgTextTransformation.Uppercase => value.ToUpper(), 
			SvgTextTransformation.Lowercase => value.ToLower(), 
			_ => value, 
		};
	}

	protected void OnChange(string newString, string sessionID)
	{
		RaiseChange(this, new StringArg
		{
			s = newString,
			SessionID = sessionID
		});
	}

	protected void RaiseChange(object sender, StringArg s)
	{
		Change?.Invoke(sender, s);
	}

	public override void RegisterEvents(ISvgEventCaller caller)
	{
		base.RegisterEvents(caller);
		caller.RegisterAction<string, string>(base.ID + "/onchange", OnChange);
	}

	public override void UnregisterEvents(ISvgEventCaller caller)
	{
		base.UnregisterEvents(caller);
		caller.UnregisterAction(base.ID + "/onchange");
	}

	public override SvgElement DeepCopy<T>()
	{
		SvgTextBase svgTextBase = base.DeepCopy<T>() as SvgTextBase;
		if (svgTextBase.Attributes.ContainsKey("x"))
		{
			svgTextBase._x = (SvgUnitCollection)svgTextBase.Attributes["x"];
			if (svgTextBase._x != null)
			{
				svgTextBase._x.CollectionChanged += svgTextBase.OnCoordinateChanged;
			}
		}
		if (svgTextBase.Attributes.ContainsKey("y"))
		{
			svgTextBase._y = (SvgUnitCollection)svgTextBase.Attributes["y"];
			if (svgTextBase._y != null)
			{
				svgTextBase._y.CollectionChanged += svgTextBase.OnCoordinateChanged;
			}
		}
		if (svgTextBase.Attributes.ContainsKey("dx"))
		{
			svgTextBase._dx = (SvgUnitCollection)svgTextBase.Attributes["dx"];
			if (svgTextBase._dx != null)
			{
				svgTextBase._dx.CollectionChanged += svgTextBase.OnCoordinateChanged;
			}
		}
		if (svgTextBase.Attributes.ContainsKey("dy"))
		{
			svgTextBase._dy = (SvgUnitCollection)svgTextBase.Attributes["dy"];
			if (svgTextBase._dy != null)
			{
				svgTextBase._dy.CollectionChanged += svgTextBase.OnCoordinateChanged;
			}
		}
		svgTextBase._rotate = _rotate;
		foreach (float rotation in _rotations)
		{
			svgTextBase._rotations.Add(rotation);
		}
		return svgTextBase;
	}

	public override bool ShouldWriteElement()
	{
		if (!HasChildren())
		{
			return base.Nodes.Count > 0;
		}
		return true;
	}

	protected internal override void RenderFillAndStroke(ISvgRenderer renderer)
	{
		base.RenderFillAndStroke(renderer);
		RenderChildren(renderer);
	}

	internal virtual IEnumerable<ISvgNode> GetContentNodes()
	{
		if (base.Nodes != null && base.Nodes.Count >= 1)
		{
			return base.Nodes;
		}
		return from o in Children.OfType<ISvgNode>()
			where !(o is ISvgDescriptiveElement)
			select o;
	}

	protected virtual GraphicsPath GetBaselinePath(ISvgRenderer renderer)
	{
		return null;
	}

	protected virtual float GetAuthorPathLength()
	{
		return 0f;
	}

	public override GraphicsPath Path(ISvgRenderer renderer)
	{
		int num = GetContentNodes().Count((ISvgNode x) => x is SvgContentNode && string.IsNullOrEmpty(x.Content.Trim('\r', '\n', '\t')));
		if (_path == null || IsPathDirty || num == 1)
		{
			if (renderer != null && renderer is IGraphicsProvider)
			{
				SetPath(new TextDrawingState(renderer, this));
			}
			else
			{
				using ISvgRenderer renderer2 = SvgRenderer.FromNull();
				SetPath(new TextDrawingState(renderer2, this));
			}
		}
		return _path;
	}

	private void SetPath(TextDrawingState state)
	{
		SetPath(state, doMeasurements: true);
	}

	private void SetPath(TextDrawingState state, bool doMeasurements)
	{
		TextDrawingState textDrawingState = null;
		bool flag = state.BaselinePath != null && (TextAnchor == SvgTextAnchor.Middle || TextAnchor == SvgTextAnchor.End);
		if (doMeasurements)
		{
			if (TextLength != SvgUnit.None)
			{
				textDrawingState = state.Clone();
			}
			else if (flag)
			{
				textDrawingState = state.Clone();
				state.BaselinePath = null;
			}
		}
		foreach (ISvgNode contentNode in GetContentNodes())
		{
			if (!(contentNode is SvgTextBase svgTextBase))
			{
				if (!string.IsNullOrEmpty(contentNode.Content))
				{
					state.DrawString(PrepareText(contentNode.Content));
				}
			}
			else
			{
				TextDrawingState textDrawingState2 = new TextDrawingState(state, svgTextBase);
				svgTextBase.SetPath(textDrawingState2);
				state.NumChars += textDrawingState2.NumChars;
				state.Current = textDrawingState2.Current;
			}
		}
		GraphicsPath graphicsPath = state.GetPath() ?? new GraphicsPath();
		if (doMeasurements)
		{
			if (TextLength != SvgUnit.None)
			{
				float num = TextLength.ToDeviceValue(state.Renderer, UnitRenderingType.Horizontal, this);
				float width = state.TextBounds.Width;
				float num2 = width - num;
				if ((double)Math.Abs(num2) > 1.5)
				{
					if (LengthAdjust == SvgTextLengthAdjust.Spacing)
					{
						if (X.Count < 2)
						{
							int num3 = state.NumChars - textDrawingState.NumChars - 1;
							if (num3 != 0)
							{
								textDrawingState.LetterSpacingAdjust = -1f * num2 / (float)num3;
								SetPath(textDrawingState, doMeasurements: false);
								return;
							}
						}
					}
					else
					{
						using Matrix matrix = new Matrix();
						matrix.Translate(-1f * state.TextBounds.X, 0f, MatrixOrder.Append);
						matrix.Scale(num / width, 1f, MatrixOrder.Append);
						matrix.Translate(state.TextBounds.X, 0f, MatrixOrder.Append);
						graphicsPath.Transform(matrix);
					}
				}
			}
			else if (flag)
			{
				RectangleF bounds = graphicsPath.GetBounds();
				if (TextAnchor == SvgTextAnchor.Middle)
				{
					textDrawingState.StartOffsetAdjust = -1f * bounds.Width / 2f;
				}
				else
				{
					textDrawingState.StartOffsetAdjust = -1f * bounds.Width;
				}
				SetPath(textDrawingState, doMeasurements: false);
				return;
			}
		}
		_path = graphicsPath;
		IsPathDirty = false;
	}

	internal override IEnumerable<ISvgPropertyDescriptor> GetProperties()
	{
		foreach (KeyValuePair<string, ISvgPropertyDescriptor> svgTextBaseProperty in SvgTextBaseProperties)
		{
			yield return svgTextBaseProperty.Value;
		}
		foreach (ISvgPropertyDescriptor property in base.GetProperties())
		{
			if (!SvgTextBaseProperties.ContainsKey(property.AttributeName))
			{
				yield return property;
			}
		}
	}

	internal override object GetValue(string attributeName)
	{
		if (SvgTextBaseProperties.TryGetValue(attributeName, out var value))
		{
			return value.GetValue(this);
		}
		return base.GetValue(attributeName);
	}

	internal override bool SetValue(string attributeName, ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (SvgTextBaseProperties.TryGetValue(attributeName, out var value2))
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
