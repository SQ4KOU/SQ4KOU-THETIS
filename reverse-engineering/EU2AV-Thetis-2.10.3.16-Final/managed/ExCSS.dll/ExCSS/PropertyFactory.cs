using System;
using System.Collections.Generic;
using System.Linq;

namespace ExCSS;

internal sealed class PropertyFactory
{
	private delegate Property LonghandCreator();

	private delegate ShorthandProperty ShorthandCreator();

	private static readonly Lazy<PropertyFactory> Lazy = new Lazy<PropertyFactory>(() => new PropertyFactory());

	private readonly List<string> _animatables = new List<string>();

	private readonly Dictionary<string, LonghandCreator> _fonts = new Dictionary<string, LonghandCreator>(StringComparer.OrdinalIgnoreCase);

	private readonly Dictionary<string, LonghandCreator> _longhands = new Dictionary<string, LonghandCreator>(StringComparer.OrdinalIgnoreCase);

	private readonly Dictionary<string, string[]> _mappings = new Dictionary<string, string[]>();

	private readonly Dictionary<string, ShorthandCreator> _shorthands = new Dictionary<string, ShorthandCreator>(StringComparer.OrdinalIgnoreCase);

	internal static PropertyFactory Instance => Lazy.Value;

	private PropertyFactory()
	{
		AddLonghand(PropertyNames.AlignContent, () => new AlignContentProperty());
		AddLonghand(PropertyNames.AlignItems, () => new AlignItemsProperty());
		AddLonghand(PropertyNames.AlignSelf, () => new AlignSelfProperty());
		AddShorthand(PropertyNames.Animation, () => new AnimationProperty(), PropertyNames.AnimationName, PropertyNames.AnimationDuration, PropertyNames.AnimationTimingFunction, PropertyNames.AnimationDelay, PropertyNames.AnimationDirection, PropertyNames.AnimationFillMode, PropertyNames.AnimationIterationCount, PropertyNames.AnimationPlayState);
		AddLonghand(PropertyNames.AnimationDelay, () => new AnimationDelayProperty());
		AddLonghand(PropertyNames.AnimationDirection, () => new AnimationDirectionProperty());
		AddLonghand(PropertyNames.AnimationDuration, () => new AnimationDurationProperty());
		AddLonghand(PropertyNames.AnimationFillMode, () => new AnimationFillModeProperty());
		AddLonghand(PropertyNames.AnimationIterationCount, () => new AnimationIterationCountProperty());
		AddLonghand(PropertyNames.AnimationName, () => new AnimationNameProperty());
		AddLonghand(PropertyNames.AnimationPlayState, () => new AnimationPlayStateProperty());
		AddLonghand(PropertyNames.AnimationTimingFunction, () => new AnimationTimingFunctionProperty());
		AddShorthand(PropertyNames.Background, () => new BackgroundProperty(), PropertyNames.BackgroundAttachment, PropertyNames.BackgroundClip, PropertyNames.BackgroundColor, PropertyNames.BackgroundImage, PropertyNames.BackgroundOrigin, PropertyNames.BackgroundPosition, PropertyNames.BackgroundRepeat, PropertyNames.BackgroundSize);
		AddLonghand(PropertyNames.BackgroundAttachment, () => new BackgroundAttachmentProperty());
		AddLonghand(PropertyNames.BackgroundColor, () => new BackgroundColorProperty(), animatable: true);
		AddLonghand(PropertyNames.BackgroundClip, () => new BackgroundClipProperty());
		AddLonghand(PropertyNames.BackgroundOrigin, () => new BackgroundOriginProperty());
		AddLonghand(PropertyNames.BackgroundSize, () => new BackgroundSizeProperty(), animatable: true);
		AddLonghand(PropertyNames.BackgroundImage, () => new BackgroundImageProperty());
		AddLonghand(PropertyNames.BackgroundPosition, () => new BackgroundPositionProperty(), animatable: true);
		AddLonghand(PropertyNames.BackgroundRepeat, () => new BackgroundRepeatProperty());
		AddLonghand(PropertyNames.BorderSpacing, () => new BorderSpacingProperty());
		AddLonghand(PropertyNames.BorderCollapse, () => new BorderCollapseProperty());
		AddLonghand(PropertyNames.BoxSizing, () => new BoxSizingProperty());
		AddLonghand(PropertyNames.BoxShadow, () => new BoxShadowProperty(), animatable: true);
		AddLonghand(PropertyNames.BoxDecorationBreak, () => new BoxDecorationBreak());
		AddLonghand(PropertyNames.BreakAfter, () => new BreakAfterProperty());
		AddLonghand(PropertyNames.BreakBefore, () => new BreakBeforeProperty());
		AddLonghand(PropertyNames.BreakInside, () => new BreakInsideProperty());
		AddLonghand(PropertyNames.BackfaceVisibility, () => new BackfaceVisibilityProperty());
		AddShorthand(PropertyNames.BorderRadius, () => new BorderRadiusProperty(), PropertyNames.BorderTopLeftRadius, PropertyNames.BorderTopRightRadius, PropertyNames.BorderBottomRightRadius, PropertyNames.BorderBottomLeftRadius);
		AddLonghand(PropertyNames.BorderTopLeftRadius, () => new BorderTopLeftRadiusProperty(), animatable: true);
		AddLonghand(PropertyNames.BorderTopRightRadius, () => new BorderTopRightRadiusProperty(), animatable: true);
		AddLonghand(PropertyNames.BorderBottomLeftRadius, () => new BorderBottomLeftRadiusProperty(), animatable: true);
		AddLonghand(PropertyNames.BorderBottomRightRadius, () => new BorderBottomRightRadiusProperty(), animatable: true);
		AddShorthand(PropertyNames.BorderImage, () => new BorderImageProperty(), PropertyNames.BorderImageOutset, PropertyNames.BorderImageRepeat, PropertyNames.BorderImageSlice, PropertyNames.BorderImageSource, PropertyNames.BorderImageWidth);
		AddLonghand(PropertyNames.BorderImageOutset, () => new BorderImageOutsetProperty());
		AddLonghand(PropertyNames.BorderImageRepeat, () => new BorderImageRepeatProperty());
		AddLonghand(PropertyNames.BorderImageSource, () => new BorderImageSourceProperty());
		AddLonghand(PropertyNames.BorderImageSlice, () => new BorderImageSliceProperty());
		AddLonghand(PropertyNames.BorderImageWidth, () => new BorderImageWidthProperty());
		AddShorthand(PropertyNames.BorderColor, () => new BorderColorProperty(), PropertyNames.BorderTopColor, PropertyNames.BorderRightColor, PropertyNames.BorderBottomColor, PropertyNames.BorderLeftColor);
		AddShorthand(PropertyNames.BorderStyle, () => new BorderStyleProperty(), PropertyNames.BorderTopStyle, PropertyNames.BorderRightStyle, PropertyNames.BorderBottomStyle, PropertyNames.BorderLeftStyle);
		AddShorthand(PropertyNames.BorderWidth, () => new BorderWidthProperty(), PropertyNames.BorderTopWidth, PropertyNames.BorderRightWidth, PropertyNames.BorderBottomWidth, PropertyNames.BorderLeftWidth);
		AddShorthand(PropertyNames.BorderTop, () => new BorderTopProperty(), PropertyNames.BorderTopWidth, PropertyNames.BorderTopStyle, PropertyNames.BorderTopColor);
		AddShorthand(PropertyNames.BorderRight, () => new BorderRightProperty(), PropertyNames.BorderRightWidth, PropertyNames.BorderRightStyle, PropertyNames.BorderRightColor);
		AddShorthand(PropertyNames.BorderBottom, () => new BorderBottomProperty(), PropertyNames.BorderBottomWidth, PropertyNames.BorderBottomStyle, PropertyNames.BorderBottomColor);
		AddShorthand(PropertyNames.BorderLeft, () => new BorderLeftProperty(), PropertyNames.BorderLeftWidth, PropertyNames.BorderLeftStyle, PropertyNames.BorderLeftColor);
		AddShorthand(PropertyNames.Border, () => new BorderProperty(), PropertyNames.BorderTopWidth, PropertyNames.BorderTopStyle, PropertyNames.BorderTopColor, PropertyNames.BorderRightWidth, PropertyNames.BorderRightStyle, PropertyNames.BorderRightColor, PropertyNames.BorderBottomWidth, PropertyNames.BorderBottomStyle, PropertyNames.BorderBottomColor, PropertyNames.BorderLeftWidth, PropertyNames.BorderLeftStyle, PropertyNames.BorderLeftColor);
		AddLonghand(PropertyNames.BorderTopColor, () => new BorderTopColorProperty(), animatable: true);
		AddLonghand(PropertyNames.BorderLeftColor, () => new BorderLeftColorProperty(), animatable: true);
		AddLonghand(PropertyNames.BorderRightColor, () => new BorderRightColorProperty(), animatable: true);
		AddLonghand(PropertyNames.BorderBottomColor, () => new BorderBottomColorProperty(), animatable: true);
		AddLonghand(PropertyNames.BorderTopStyle, () => new BorderTopStyleProperty());
		AddLonghand(PropertyNames.BorderLeftStyle, () => new BorderLeftStyleProperty());
		AddLonghand(PropertyNames.BorderRightStyle, () => new BorderRightStyleProperty());
		AddLonghand(PropertyNames.BorderBottomStyle, () => new BorderBottomStyleProperty());
		AddLonghand(PropertyNames.BorderTopWidth, () => new BorderTopWidthProperty(), animatable: true);
		AddLonghand(PropertyNames.BorderLeftWidth, () => new BorderLeftWidthProperty(), animatable: true);
		AddLonghand(PropertyNames.BorderRightWidth, () => new BorderRightWidthProperty(), animatable: true);
		AddLonghand(PropertyNames.BorderBottomWidth, () => new BorderBottomWidthProperty(), animatable: true);
		AddLonghand(PropertyNames.Bottom, () => new BottomProperty(), animatable: true);
		AddShorthand(PropertyNames.Columns, () => new ColumnsProperty(), PropertyNames.ColumnWidth, PropertyNames.ColumnCount);
		AddLonghand(PropertyNames.ColumnCount, () => new ColumnCountProperty(), animatable: true);
		AddLonghand(PropertyNames.ColumnWidth, () => new ColumnWidthProperty(), animatable: true);
		AddLonghand(PropertyNames.ColumnFill, () => new ColumnFillProperty());
		AddLonghand(PropertyNames.ColumnGap, () => new ColumnGapProperty(), animatable: true);
		AddLonghand(PropertyNames.ColumnSpan, () => new ColumnSpanProperty());
		AddShorthand(PropertyNames.ColumnRule, () => new ColumnRuleProperty(), PropertyNames.ColumnRuleWidth, PropertyNames.ColumnRuleStyle, PropertyNames.ColumnRuleColor);
		AddLonghand(PropertyNames.ColumnRuleColor, () => new ColumnRuleColorProperty(), animatable: true);
		AddLonghand(PropertyNames.ColumnRuleStyle, () => new ColumnRuleStyleProperty());
		AddLonghand(PropertyNames.ColumnRuleWidth, () => new ColumnRuleWidthProperty(), animatable: true);
		AddLonghand(PropertyNames.CaptionSide, () => new CaptionSideProperty());
		AddLonghand(PropertyNames.Clear, () => new ClearProperty());
		AddLonghand(PropertyNames.Clip, () => new ClipProperty(), animatable: true);
		AddLonghand(PropertyNames.Color, () => new ColorProperty(), animatable: true);
		AddLonghand(PropertyNames.ContainerName, () => new ContainerNameProperty());
		AddLonghand(PropertyNames.ContainerType, () => new ContainerTypeProperty());
		AddLonghand(PropertyNames.Content, () => new ContentProperty());
		AddLonghand(PropertyNames.CounterIncrement, () => new CounterIncrementProperty());
		AddLonghand(PropertyNames.CounterReset, () => new CounterResetProperty());
		AddLonghand(PropertyNames.Cursor, () => new CursorProperty());
		AddLonghand(PropertyNames.Direction, () => new DirectionProperty());
		AddLonghand(PropertyNames.Display, () => new DisplayProperty());
		AddLonghand(PropertyNames.EmptyCells, () => new EmptyCellsProperty());
		AddLonghand(PropertyNames.Fill, () => new FillProperty(), animatable: true);
		AddLonghand(PropertyNames.FillOpacity, () => new FillOpacityProperty(), animatable: true);
		AddLonghand(PropertyNames.FillRule, () => new FillRuleProperty(), animatable: true);
		AddShorthand(PropertyNames.Flex, () => new FlexProperty(), PropertyNames.FlexGrow, PropertyNames.FlexShrink, PropertyNames.FlexBasis);
		AddLonghand(PropertyNames.FlexBasis, () => new FlexBasisProperty(), animatable: true);
		AddLonghand(PropertyNames.FlexDirection, () => new FlexDirectionProperty());
		AddShorthand(PropertyNames.FlexFlow, () => new FlexFlowProperty(), PropertyNames.FlexDirection, PropertyNames.FlexWrap);
		AddLonghand(PropertyNames.FlexGrow, () => new FlexGrowProperty());
		AddLonghand(PropertyNames.FlexShrink, () => new FlexShrinkProperty());
		AddLonghand(PropertyNames.FlexWrap, () => new FlexWrapProperty());
		AddLonghand(PropertyNames.Float, () => new FloatProperty());
		AddShorthand(PropertyNames.Font, () => new FontProperty(), PropertyNames.FontFamily, PropertyNames.FontSize, PropertyNames.FontStretch, PropertyNames.FontStyle, PropertyNames.FontVariant, PropertyNames.FontWeight, PropertyNames.LineHeight);
		AddLonghand(PropertyNames.FontFamily, () => new FontFamilyProperty(), animatable: false, font: true);
		AddLonghand(PropertyNames.FontSize, () => new FontSizeProperty(), animatable: true);
		AddLonghand(PropertyNames.FontSizeAdjust, () => new FontSizeAdjustProperty(), animatable: true);
		AddLonghand(PropertyNames.FontStyle, () => new FontStyleProperty(), animatable: false, font: true);
		AddLonghand(PropertyNames.FontVariant, () => new FontVariantProperty(), animatable: false, font: true);
		AddLonghand(PropertyNames.FontWeight, () => new FontWeightProperty(), animatable: true, font: true);
		AddLonghand(PropertyNames.FontStretch, () => new FontStretchProperty(), animatable: true, font: true);
		AddShorthand(PropertyNames.Gap, () => new GapProperty(), PropertyNames.RowGap, PropertyNames.ColumnGap);
		AddLonghand(PropertyNames.Height, () => new HeightProperty(), animatable: true);
		AddLonghand(PropertyNames.JustifyContent, () => new JustifyContentProperty());
		AddLonghand(PropertyNames.Left, () => new LeftProperty(), animatable: true);
		AddLonghand(PropertyNames.LetterSpacing, () => new LetterSpacingProperty());
		AddLonghand(PropertyNames.LineHeight, () => new LineHeightProperty(), animatable: true);
		AddShorthand(PropertyNames.ListStyle, () => new ListStyleProperty(), PropertyNames.ListStyleType, PropertyNames.ListStyleImage, PropertyNames.ListStylePosition);
		AddLonghand(PropertyNames.ListStyleImage, () => new ListStyleImageProperty());
		AddLonghand(PropertyNames.ListStylePosition, () => new ListStylePositionProperty());
		AddLonghand(PropertyNames.ListStyleType, () => new ListStyleTypeProperty());
		AddShorthand(PropertyNames.Margin, () => new MarginProperty(), PropertyNames.MarginTop, PropertyNames.MarginRight, PropertyNames.MarginBottom, PropertyNames.MarginLeft);
		AddLonghand(PropertyNames.MarginRight, () => new MarginRightProperty(), animatable: true);
		AddLonghand(PropertyNames.MarginLeft, () => new MarginLeftProperty(), animatable: true);
		AddLonghand(PropertyNames.MarginTop, () => new MarginTopProperty(), animatable: true);
		AddLonghand(PropertyNames.MarginBottom, () => new MarginBottomProperty(), animatable: true);
		AddLonghand(PropertyNames.MaxHeight, () => new MaxHeightProperty(), animatable: true);
		AddLonghand(PropertyNames.MaxWidth, () => new MaxWidthProperty(), animatable: true);
		AddLonghand(PropertyNames.MinHeight, () => new MinHeightProperty(), animatable: true);
		AddLonghand(PropertyNames.MinWidth, () => new MinWidthProperty(), animatable: true);
		AddLonghand(PropertyNames.Opacity, () => new OpacityProperty(), animatable: true);
		AddLonghand(PropertyNames.Order, () => new OrderProperty(), animatable: true);
		AddLonghand(PropertyNames.Orphans, () => new OrphansProperty());
		AddShorthand(PropertyNames.Outline, () => new OutlineProperty(), PropertyNames.OutlineWidth, PropertyNames.OutlineStyle, PropertyNames.OutlineColor);
		AddLonghand(PropertyNames.OutlineColor, () => new OutlineColorProperty(), animatable: true);
		AddLonghand(PropertyNames.OutlineStyle, () => new OutlineStyleProperty());
		AddLonghand(PropertyNames.OutlineWidth, () => new OutlineWidthProperty(), animatable: true);
		AddLonghand(PropertyNames.Overflow, () => new OverflowProperty());
		AddLonghand(PropertyNames.OverflowWrap, () => new OverflowWrapProperty());
		AddShorthand(PropertyNames.Padding, () => new PaddingProperty(), PropertyNames.PaddingTop, PropertyNames.PaddingRight, PropertyNames.PaddingBottom, PropertyNames.PaddingLeft);
		AddLonghand(PropertyNames.PaddingTop, () => new PaddingTopProperty(), animatable: true);
		AddLonghand(PropertyNames.PaddingRight, () => new PaddingRightProperty(), animatable: true);
		AddLonghand(PropertyNames.PaddingLeft, () => new PaddingLeftProperty(), animatable: true);
		AddLonghand(PropertyNames.PaddingBottom, () => new PaddingBottomProperty(), animatable: true);
		AddLonghand(PropertyNames.PageBreakAfter, () => new PageBreakAfterProperty());
		AddLonghand(PropertyNames.PageBreakBefore, () => new PageBreakBeforeProperty());
		AddLonghand(PropertyNames.PageBreakInside, () => new PageBreakInsideProperty());
		AddLonghand(PropertyNames.Perspective, () => new PerspectiveProperty(), animatable: true);
		AddLonghand(PropertyNames.PerspectiveOrigin, () => new PerspectiveOriginProperty(), animatable: true);
		AddLonghand(PropertyNames.Position, () => new PositionProperty());
		AddLonghand(PropertyNames.Quotes, () => new QuotesProperty());
		AddLonghand(PropertyNames.Right, () => new RightProperty(), animatable: true);
		AddLonghand(PropertyNames.RowGap, () => new RowGapProperty(), animatable: true);
		AddLonghand(PropertyNames.Stroke, () => new StrokeProperty(), animatable: true);
		AddLonghand(PropertyNames.StrokeDasharray, () => new StrokeDasharrayProperty(), animatable: true);
		AddLonghand(PropertyNames.StrokeDashoffset, () => new StrokeDashoffsetProperty(), animatable: true);
		AddLonghand(PropertyNames.StrokeLinecap, () => new StrokeLinecapProperty(), animatable: true);
		AddLonghand(PropertyNames.StrokeLinejoin, () => new StrokeLinejoinProperty(), animatable: true);
		AddLonghand(PropertyNames.StrokeMiterlimit, () => new StrokeMiterlimitProperty(), animatable: true);
		AddLonghand(PropertyNames.StrokeOpacity, () => new StrokeOpacityProperty(), animatable: true);
		AddLonghand(PropertyNames.StrokeWidth, () => new StrokeWidthProperty(), animatable: true);
		AddLonghand(PropertyNames.TableLayout, () => new TableLayoutProperty());
		AddLonghand(PropertyNames.TextAlign, () => new TextAlignProperty());
		AddLonghand(PropertyNames.TextAlignLast, () => new TextAlignLastProperty());
		AddLonghand(PropertyNames.TextAnchor, () => new TextAnchorProperty());
		AddShorthand(PropertyNames.TextDecoration, () => new TextDecorationProperty(), PropertyNames.TextDecorationLine, PropertyNames.TextDecorationStyle, PropertyNames.TextDecorationColor);
		AddLonghand(PropertyNames.TextDecorationStyle, () => new TextDecorationStyleProperty());
		AddLonghand(PropertyNames.TextDecorationLine, () => new TextDecorationLineProperty());
		AddLonghand(PropertyNames.TextDecorationColor, () => new TextDecorationColorProperty(), animatable: true);
		AddLonghand(PropertyNames.TextIndent, () => new TextIndentProperty(), animatable: true);
		AddLonghand(PropertyNames.TextJustify, () => new TextJustifyProperty());
		AddLonghand(PropertyNames.TextTransform, () => new TextTransformProperty());
		AddLonghand(PropertyNames.TextShadow, () => new TextShadowProperty(), animatable: true);
		AddLonghand(PropertyNames.Transform, () => new TransformProperty(), animatable: true);
		AddLonghand(PropertyNames.TransformOrigin, () => new TransformOriginProperty(), animatable: true);
		AddLonghand(PropertyNames.TransformStyle, () => new TransformStyleProperty());
		AddShorthand(PropertyNames.Transition, () => new TransitionProperty(), PropertyNames.TransitionProperty, PropertyNames.TransitionDuration, PropertyNames.TransitionTimingFunction, PropertyNames.TransitionDelay);
		AddLonghand(PropertyNames.TransitionDelay, () => new TransitionDelayProperty());
		AddLonghand(PropertyNames.TransitionDuration, () => new TransitionDurationProperty());
		AddLonghand(PropertyNames.TransitionTimingFunction, () => new TransitionTimingFunctionProperty());
		AddLonghand(PropertyNames.TransitionProperty, () => new TransitionPropertyProperty());
		AddLonghand(PropertyNames.Top, () => new TopProperty(), animatable: true);
		AddLonghand(PropertyNames.UnicodeBidirectional, () => new UnicodeBidirectionalProperty());
		AddLonghand(PropertyNames.VerticalAlign, () => new VerticalAlignProperty(), animatable: true);
		AddLonghand(PropertyNames.Visibility, () => new VisibilityProperty(), animatable: true);
		AddLonghand(PropertyNames.WhiteSpace, () => new WhiteSpaceProperty());
		AddLonghand(PropertyNames.Widows, () => new WidowsProperty());
		AddLonghand(PropertyNames.Width, () => new WidthProperty(), animatable: true);
		AddLonghand(PropertyNames.WordBreak, () => new WordBreakProperty(), animatable: true);
		AddLonghand(PropertyNames.WordSpacing, () => new WordSpacingProperty(), animatable: true);
		AddLonghand(PropertyNames.WordWrap, () => new OverflowWrapProperty());
		AddLonghand(PropertyNames.ZIndex, () => new ZIndexProperty(), animatable: true);
		AddLonghand(PropertyNames.ObjectFit, () => new ObjectFitProperty());
		AddLonghand(PropertyNames.ObjectPosition, () => new ObjectPositionProperty(), animatable: true);
		_fonts.Add(PropertyNames.Src, () => new SrcProperty());
		_fonts.Add(PropertyNames.UnicodeRange, () => new UnicodeRangeProperty());
	}

	private void AddShorthand(string name, ShorthandCreator creator, params string[] longhands)
	{
		_shorthands.Add(name, creator);
		_mappings.Add(name, longhands);
	}

	private void AddLonghand(string name, LonghandCreator creator, bool animatable = false, bool font = false)
	{
		_longhands.Add(name, creator);
		if (animatable)
		{
			_animatables.Add(name);
		}
		if (font)
		{
			_fonts.Add(name, creator);
		}
	}

	public Property Create(string name)
	{
		return CreateLonghand(name) ?? CreateShorthand(name);
	}

	public Property CreateFont(string name)
	{
		if (!_fonts.TryGetValue(name, out var value))
		{
			return null;
		}
		return value();
	}

	public Property CreateViewport(string name)
	{
		MediaFeature mediaFeature = MediaFeatureFactory.Instance.Create(name);
		if (mediaFeature == null)
		{
			return null;
		}
		return new FeatureProperty(mediaFeature);
	}

	public Property CreateLonghand(string name)
	{
		if (!_longhands.TryGetValue(name, out var value))
		{
			return null;
		}
		return value();
	}

	public ShorthandProperty CreateShorthand(string name)
	{
		if (!_shorthands.TryGetValue(name, out var value))
		{
			return null;
		}
		return value();
	}

	public Property[] CreateLonghandsFor(string name)
	{
		return GetLonghands(name).Select(CreateLonghand).ToArray();
	}

	public bool IsShorthand(string name)
	{
		return _shorthands.ContainsKey(name);
	}

	public bool IsAnimatable(string name)
	{
		if (!_longhands.ContainsKey(name))
		{
			return GetLonghands(name).Any((string _) => _animatables.Contains(name));
		}
		return _animatables.Contains(name);
	}

	public string[] GetLonghands(string name)
	{
		if (!_mappings.TryGetValue(name, out var value))
		{
			return Array.Empty<string>();
		}
		return value;
	}

	public IEnumerable<string> GetShorthands(string name)
	{
		return _mappings.Where(delegate(KeyValuePair<string, string[]> mapping)
		{
			KeyValuePair<string, string[]> keyValuePair = mapping;
			return keyValuePair.Value.Contains(name, StringComparison.OrdinalIgnoreCase);
		}).Select(delegate(KeyValuePair<string, string[]> mapping)
		{
			KeyValuePair<string, string[]> keyValuePair = mapping;
			return keyValuePair.Key;
		});
	}
}
