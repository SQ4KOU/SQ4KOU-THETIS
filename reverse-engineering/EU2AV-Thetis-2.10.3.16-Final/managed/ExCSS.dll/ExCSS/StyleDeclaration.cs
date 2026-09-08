using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ExCSS;

public sealed class StyleDeclaration : StylesheetNode, IProperties, IEnumerable<IProperty>, IEnumerable
{
	private readonly Rule _parent;

	private readonly StylesheetParser _parser;

	public IRule Parent => _parent;

	public string this[int index] => Declarations.GetItemByIndex(index).Name;

	public string this[string name] => GetPropertyValue(name);

	public int Length => Declarations.Count();

	public bool IsStrictMode => !_parser.Options.IncludeUnknownDeclarations;

	public IEnumerable<Property> Declarations => base.Children.OfType<Property>();

	public string CssText
	{
		get
		{
			return this.ToCss();
		}
		set
		{
			Update(value);
			RaiseChanged();
		}
	}

	public string AlignContent
	{
		get
		{
			return GetPropertyValue(PropertyNames.AlignContent);
		}
		set
		{
			SetPropertyValue(PropertyNames.AlignContent, value);
		}
	}

	public string AlignItems
	{
		get
		{
			return GetPropertyValue(PropertyNames.AlignItems);
		}
		set
		{
			SetPropertyValue(PropertyNames.AlignItems, value);
		}
	}

	public string AlignSelf
	{
		get
		{
			return GetPropertyValue(PropertyNames.AlignSelf);
		}
		set
		{
			SetPropertyValue(PropertyNames.AlignSelf, value);
		}
	}

	public string Accelerator
	{
		get
		{
			return GetPropertyValue(PropertyNames.Accelerator);
		}
		set
		{
			SetPropertyValue(PropertyNames.Accelerator, value);
		}
	}

	public string AlignmentBaseline
	{
		get
		{
			return GetPropertyValue(PropertyNames.AlignBaseline);
		}
		set
		{
			SetPropertyValue(PropertyNames.AlignBaseline, value);
		}
	}

	public string Animation
	{
		get
		{
			return GetPropertyValue(PropertyNames.Animation);
		}
		set
		{
			SetPropertyValue(PropertyNames.Animation, value);
		}
	}

	public string AnimationDelay
	{
		get
		{
			return GetPropertyValue(PropertyNames.AnimationDelay);
		}
		set
		{
			SetPropertyValue(PropertyNames.AnimationDelay, value);
		}
	}

	public string AnimationDirection
	{
		get
		{
			return GetPropertyValue(PropertyNames.AnimationDirection);
		}
		set
		{
			SetPropertyValue(PropertyNames.AnimationDirection, value);
		}
	}

	public string AnimationDuration
	{
		get
		{
			return GetPropertyValue(PropertyNames.AnimationDuration);
		}
		set
		{
			SetPropertyValue(PropertyNames.AnimationDuration, value);
		}
	}

	public string AnimationFillMode
	{
		get
		{
			return GetPropertyValue(PropertyNames.AnimationFillMode);
		}
		set
		{
			SetPropertyValue(PropertyNames.AnimationFillMode, value);
		}
	}

	public string AnimationIterationCount
	{
		get
		{
			return GetPropertyValue(PropertyNames.AnimationIterationCount);
		}
		set
		{
			SetPropertyValue(PropertyNames.AnimationIterationCount, value);
		}
	}

	public string AnimationName
	{
		get
		{
			return GetPropertyValue(PropertyNames.AnimationName);
		}
		set
		{
			SetPropertyValue(PropertyNames.AnimationName, value);
		}
	}

	public string AnimationPlayState
	{
		get
		{
			return GetPropertyValue(PropertyNames.AnimationPlayState);
		}
		set
		{
			SetPropertyValue(PropertyNames.AnimationPlayState, value);
		}
	}

	public string AnimationTimingFunction
	{
		get
		{
			return GetPropertyValue(PropertyNames.AnimationTimingFunction);
		}
		set
		{
			SetPropertyValue(PropertyNames.AnimationTimingFunction, value);
		}
	}

	public string BackfaceVisibility
	{
		get
		{
			return GetPropertyValue(PropertyNames.BackfaceVisibility);
		}
		set
		{
			SetPropertyValue(PropertyNames.BackfaceVisibility, value);
		}
	}

	public string Background
	{
		get
		{
			return GetPropertyValue(PropertyNames.Background);
		}
		set
		{
			SetPropertyValue(PropertyNames.Background, value);
		}
	}

	public string BackgroundAttachment
	{
		get
		{
			return GetPropertyValue(PropertyNames.BackgroundAttachment);
		}
		set
		{
			SetPropertyValue(PropertyNames.BackgroundAttachment, value);
		}
	}

	public string BackgroundClip
	{
		get
		{
			return GetPropertyValue(PropertyNames.BackgroundClip);
		}
		set
		{
			SetPropertyValue(PropertyNames.BackgroundClip, value);
		}
	}

	public string BackgroundColor
	{
		get
		{
			return GetPropertyValue(PropertyNames.BackgroundColor);
		}
		set
		{
			SetPropertyValue(PropertyNames.BackgroundColor, value);
		}
	}

	public string BackgroundImage
	{
		get
		{
			return GetPropertyValue(PropertyNames.BackgroundImage);
		}
		set
		{
			SetPropertyValue(PropertyNames.BackgroundImage, value);
		}
	}

	public string BackgroundOrigin
	{
		get
		{
			return GetPropertyValue(PropertyNames.BackgroundOrigin);
		}
		set
		{
			SetPropertyValue(PropertyNames.BackgroundOrigin, value);
		}
	}

	public string BackgroundPosition
	{
		get
		{
			return GetPropertyValue(PropertyNames.BackgroundPosition);
		}
		set
		{
			SetPropertyValue(PropertyNames.BackgroundPosition, value);
		}
	}

	public string BackgroundPositionX
	{
		get
		{
			return GetPropertyValue(PropertyNames.BackgroundPositionX);
		}
		set
		{
			SetPropertyValue(PropertyNames.BackgroundPositionX, value);
		}
	}

	public string BackgroundPositionY
	{
		get
		{
			return GetPropertyValue(PropertyNames.BackgroundPositionY);
		}
		set
		{
			SetPropertyValue(PropertyNames.BackgroundPositionY, value);
		}
	}

	public string BackgroundRepeat
	{
		get
		{
			return GetPropertyValue(PropertyNames.BackgroundRepeat);
		}
		set
		{
			SetPropertyValue(PropertyNames.BackgroundRepeat, value);
		}
	}

	public string BackgroundSize
	{
		get
		{
			return GetPropertyValue(PropertyNames.BackgroundSize);
		}
		set
		{
			SetPropertyValue(PropertyNames.BackgroundSize, value);
		}
	}

	public string BaselineShift
	{
		get
		{
			return GetPropertyValue(PropertyNames.BaselineShift);
		}
		set
		{
			SetPropertyValue(PropertyNames.BaselineShift, value);
		}
	}

	public string Behavior
	{
		get
		{
			return GetPropertyValue(PropertyNames.Behavior);
		}
		set
		{
			SetPropertyValue(PropertyNames.Behavior, value);
		}
	}

	public string Bottom
	{
		get
		{
			return GetPropertyValue(PropertyNames.Bottom);
		}
		set
		{
			SetPropertyValue(PropertyNames.Bottom, value);
		}
	}

	public string Border
	{
		get
		{
			return GetPropertyValue(PropertyNames.Border);
		}
		set
		{
			SetPropertyValue(PropertyNames.Border, value);
		}
	}

	public string BorderBottom
	{
		get
		{
			return GetPropertyValue(PropertyNames.BorderBottom);
		}
		set
		{
			SetPropertyValue(PropertyNames.BorderBottom, value);
		}
	}

	public string BorderBottomColor
	{
		get
		{
			return GetPropertyValue(PropertyNames.BorderBottomColor);
		}
		set
		{
			SetPropertyValue(PropertyNames.BorderBottomColor, value);
		}
	}

	public string BorderBottomLeftRadius
	{
		get
		{
			return GetPropertyValue(PropertyNames.BorderBottomLeftRadius);
		}
		set
		{
			SetPropertyValue(PropertyNames.BorderBottomLeftRadius, value);
		}
	}

	public string BorderBottomRightRadius
	{
		get
		{
			return GetPropertyValue(PropertyNames.BorderBottomRightRadius);
		}
		set
		{
			SetPropertyValue(PropertyNames.BorderBottomRightRadius, value);
		}
	}

	public string BorderBottomStyle
	{
		get
		{
			return GetPropertyValue(PropertyNames.BorderBottomStyle);
		}
		set
		{
			SetPropertyValue(PropertyNames.BorderBottomStyle, value);
		}
	}

	public string BorderBottomWidth
	{
		get
		{
			return GetPropertyValue(PropertyNames.BorderBottomWidth);
		}
		set
		{
			SetPropertyValue(PropertyNames.BorderBottomWidth, value);
		}
	}

	public string BorderCollapse
	{
		get
		{
			return GetPropertyValue(PropertyNames.BorderCollapse);
		}
		set
		{
			SetPropertyValue(PropertyNames.BorderCollapse, value);
		}
	}

	public string BorderColor
	{
		get
		{
			return GetPropertyValue(PropertyNames.BorderColor);
		}
		set
		{
			SetPropertyValue(PropertyNames.BorderColor, value);
		}
	}

	public string BorderImage
	{
		get
		{
			return GetPropertyValue(PropertyNames.BorderImage);
		}
		set
		{
			SetPropertyValue(PropertyNames.BorderImage, value);
		}
	}

	public string BorderImageOutset
	{
		get
		{
			return GetPropertyValue(PropertyNames.BorderImageOutset);
		}
		set
		{
			SetPropertyValue(PropertyNames.BorderImageOutset, value);
		}
	}

	public string BorderImageRepeat
	{
		get
		{
			return GetPropertyValue(PropertyNames.BorderImageRepeat);
		}
		set
		{
			SetPropertyValue(PropertyNames.BorderImageRepeat, value);
		}
	}

	public string BorderImageSlice
	{
		get
		{
			return GetPropertyValue(PropertyNames.BorderImageSlice);
		}
		set
		{
			SetPropertyValue(PropertyNames.BorderImageSlice, value);
		}
	}

	public string BorderImageSource
	{
		get
		{
			return GetPropertyValue(PropertyNames.BorderImageSource);
		}
		set
		{
			SetPropertyValue(PropertyNames.BorderImageSource, value);
		}
	}

	public string BorderImageWidth
	{
		get
		{
			return GetPropertyValue(PropertyNames.BorderImageWidth);
		}
		set
		{
			SetPropertyValue(PropertyNames.BorderImageWidth, value);
		}
	}

	public string BorderLeft
	{
		get
		{
			return GetPropertyValue(PropertyNames.BorderLeft);
		}
		set
		{
			SetPropertyValue(PropertyNames.BorderLeft, value);
		}
	}

	public string BorderLeftColor
	{
		get
		{
			return GetPropertyValue(PropertyNames.BorderLeftColor);
		}
		set
		{
			SetPropertyValue(PropertyNames.BorderLeftColor, value);
		}
	}

	public string BorderLeftStyle
	{
		get
		{
			return GetPropertyValue(PropertyNames.BorderLeftStyle);
		}
		set
		{
			SetPropertyValue(PropertyNames.BorderLeftStyle, value);
		}
	}

	public string BorderLeftWidth
	{
		get
		{
			return GetPropertyValue(PropertyNames.BorderLeftWidth);
		}
		set
		{
			SetPropertyValue(PropertyNames.BorderLeftWidth, value);
		}
	}

	public string BorderRadius
	{
		get
		{
			return GetPropertyValue(PropertyNames.BorderRadius);
		}
		set
		{
			SetPropertyValue(PropertyNames.BorderRadius, value);
		}
	}

	public string BorderRight
	{
		get
		{
			return GetPropertyValue(PropertyNames.BorderRight);
		}
		set
		{
			SetPropertyValue(PropertyNames.BorderRight, value);
		}
	}

	public string BorderRightColor
	{
		get
		{
			return GetPropertyValue(PropertyNames.BorderRightColor);
		}
		set
		{
			SetPropertyValue(PropertyNames.BorderRightColor, value);
		}
	}

	public string BorderRightStyle
	{
		get
		{
			return GetPropertyValue(PropertyNames.BorderRightStyle);
		}
		set
		{
			SetPropertyValue(PropertyNames.BorderRightStyle, value);
		}
	}

	public string BorderRightWidth
	{
		get
		{
			return GetPropertyValue(PropertyNames.BorderRightWidth);
		}
		set
		{
			SetPropertyValue(PropertyNames.BorderRightWidth, value);
		}
	}

	public string BorderSpacing
	{
		get
		{
			return GetPropertyValue(PropertyNames.BorderSpacing);
		}
		set
		{
			SetPropertyValue(PropertyNames.BorderSpacing, value);
		}
	}

	public string BorderStyle
	{
		get
		{
			return GetPropertyValue(PropertyNames.BorderStyle);
		}
		set
		{
			SetPropertyValue(PropertyNames.BorderStyle, value);
		}
	}

	public string BorderTop
	{
		get
		{
			return GetPropertyValue(PropertyNames.BorderTop);
		}
		set
		{
			SetPropertyValue(PropertyNames.BorderTop, value);
		}
	}

	public string BorderTopColor
	{
		get
		{
			return GetPropertyValue(PropertyNames.BorderTopColor);
		}
		set
		{
			SetPropertyValue(PropertyNames.BorderTopColor, value);
		}
	}

	public string BorderTopLeftRadius
	{
		get
		{
			return GetPropertyValue(PropertyNames.BorderTopLeftRadius);
		}
		set
		{
			SetPropertyValue(PropertyNames.BorderTopLeftRadius, value);
		}
	}

	public string BorderTopRightRadius
	{
		get
		{
			return GetPropertyValue(PropertyNames.BorderTopRightRadius);
		}
		set
		{
			SetPropertyValue(PropertyNames.BorderTopRightRadius, value);
		}
	}

	public string BorderTopStyle
	{
		get
		{
			return GetPropertyValue(PropertyNames.BorderTopStyle);
		}
		set
		{
			SetPropertyValue(PropertyNames.BorderTopStyle, value);
		}
	}

	public string BorderTopWidth
	{
		get
		{
			return GetPropertyValue(PropertyNames.BorderTopWidth);
		}
		set
		{
			SetPropertyValue(PropertyNames.BorderTopWidth, value);
		}
	}

	public string BorderWidth
	{
		get
		{
			return GetPropertyValue(PropertyNames.BorderWidth);
		}
		set
		{
			SetPropertyValue(PropertyNames.BorderWidth, value);
		}
	}

	public string BoxShadow
	{
		get
		{
			return GetPropertyValue(PropertyNames.BoxShadow);
		}
		set
		{
			SetPropertyValue(PropertyNames.BoxShadow, value);
		}
	}

	public string BoxSizing
	{
		get
		{
			return GetPropertyValue(PropertyNames.BoxSizing);
		}
		set
		{
			SetPropertyValue(PropertyNames.BoxSizing, value);
		}
	}

	public string BreakAfter
	{
		get
		{
			return GetPropertyValue(PropertyNames.BreakAfter);
		}
		set
		{
			SetPropertyValue(PropertyNames.BreakAfter, value);
		}
	}

	public string BreakBefore
	{
		get
		{
			return GetPropertyValue(PropertyNames.BreakBefore);
		}
		set
		{
			SetPropertyValue(PropertyNames.BreakBefore, value);
		}
	}

	public string BreakInside
	{
		get
		{
			return GetPropertyValue(PropertyNames.BreakInside);
		}
		set
		{
			SetPropertyValue(PropertyNames.BreakInside, value);
		}
	}

	public string CaptionSide
	{
		get
		{
			return GetPropertyValue(PropertyNames.CaptionSide);
		}
		set
		{
			SetPropertyValue(PropertyNames.CaptionSide, value);
		}
	}

	public new string Clear
	{
		get
		{
			return GetPropertyValue(PropertyNames.Clear);
		}
		set
		{
			SetPropertyValue(PropertyNames.Clear, value);
		}
	}

	public string Clip
	{
		get
		{
			return GetPropertyValue(PropertyNames.Clip);
		}
		set
		{
			SetPropertyValue(PropertyNames.Clip, value);
		}
	}

	public string ClipBottom
	{
		get
		{
			return GetPropertyValue(PropertyNames.ClipBottom);
		}
		set
		{
			SetPropertyValue(PropertyNames.ClipBottom, value);
		}
	}

	public string ClipLeft
	{
		get
		{
			return GetPropertyValue(PropertyNames.ClipLeft);
		}
		set
		{
			SetPropertyValue(PropertyNames.ClipLeft, value);
		}
	}

	public string ClipPath
	{
		get
		{
			return GetPropertyValue(PropertyNames.ClipPath);
		}
		set
		{
			SetPropertyValue(PropertyNames.ClipPath, value);
		}
	}

	public string ClipRight
	{
		get
		{
			return GetPropertyValue(PropertyNames.ClipRight);
		}
		set
		{
			SetPropertyValue(PropertyNames.ClipRight, value);
		}
	}

	public string ClipRule
	{
		get
		{
			return GetPropertyValue(PropertyNames.ClipRule);
		}
		set
		{
			SetPropertyValue(PropertyNames.ClipRule, value);
		}
	}

	public string ClipTop
	{
		get
		{
			return GetPropertyValue(PropertyNames.ClipTop);
		}
		set
		{
			SetPropertyValue(PropertyNames.ClipTop, value);
		}
	}

	public string Color
	{
		get
		{
			return GetPropertyValue(PropertyNames.Color);
		}
		set
		{
			SetPropertyValue(PropertyNames.Color, value);
		}
	}

	public string ColorInterpolationFilters
	{
		get
		{
			return GetPropertyValue(PropertyNames.ColorInterpolationFilters);
		}
		set
		{
			SetPropertyValue(PropertyNames.ColorInterpolationFilters, value);
		}
	}

	public string ColumnCount
	{
		get
		{
			return GetPropertyValue(PropertyNames.ColumnCount);
		}
		set
		{
			SetPropertyValue(PropertyNames.ColumnCount, value);
		}
	}

	public string ColumnFill
	{
		get
		{
			return GetPropertyValue(PropertyNames.ColumnFill);
		}
		set
		{
			SetPropertyValue(PropertyNames.ColumnFill, value);
		}
	}

	public string ColumnGap
	{
		get
		{
			return GetPropertyValue(PropertyNames.ColumnGap);
		}
		set
		{
			SetPropertyValue(PropertyNames.ColumnGap, value);
		}
	}

	public string ColumnRule
	{
		get
		{
			return GetPropertyValue(PropertyNames.ColumnRule);
		}
		set
		{
			SetPropertyValue(PropertyNames.ColumnRule, value);
		}
	}

	public string ColumnRuleColor
	{
		get
		{
			return GetPropertyValue(PropertyNames.ColumnRuleColor);
		}
		set
		{
			SetPropertyValue(PropertyNames.ColumnRuleColor, value);
		}
	}

	public string ColumnRuleStyle
	{
		get
		{
			return GetPropertyValue(PropertyNames.ColumnRuleStyle);
		}
		set
		{
			SetPropertyValue(PropertyNames.ColumnRuleStyle, value);
		}
	}

	public string ColumnRuleWidth
	{
		get
		{
			return GetPropertyValue(PropertyNames.ColumnRuleWidth);
		}
		set
		{
			SetPropertyValue(PropertyNames.ColumnRuleWidth, value);
		}
	}

	public string Columns
	{
		get
		{
			return GetPropertyValue(PropertyNames.Columns);
		}
		set
		{
			SetPropertyValue(PropertyNames.Columns, value);
		}
	}

	public string ColumnSpan
	{
		get
		{
			return GetPropertyValue(PropertyNames.ColumnSpan);
		}
		set
		{
			SetPropertyValue(PropertyNames.ColumnSpan, value);
		}
	}

	public string ColumnWidth
	{
		get
		{
			return GetPropertyValue(PropertyNames.ColumnWidth);
		}
		set
		{
			SetPropertyValue(PropertyNames.ColumnWidth, value);
		}
	}

	public string ContainerName
	{
		get
		{
			return GetPropertyValue(PropertyNames.ContainerName);
		}
		set
		{
			SetPropertyValue(PropertyNames.ContainerName, value);
		}
	}

	public string ContainerType
	{
		get
		{
			return GetPropertyValue(PropertyNames.ContainerType);
		}
		set
		{
			SetPropertyValue(PropertyNames.ContainerType, value);
		}
	}

	public string Content
	{
		get
		{
			return GetPropertyValue(PropertyNames.Content);
		}
		set
		{
			SetPropertyValue(PropertyNames.Content, value);
		}
	}

	public string CounterIncrement
	{
		get
		{
			return GetPropertyValue(PropertyNames.CounterIncrement);
		}
		set
		{
			SetPropertyValue(PropertyNames.CounterIncrement, value);
		}
	}

	public string CounterReset
	{
		get
		{
			return GetPropertyValue(PropertyNames.CounterReset);
		}
		set
		{
			SetPropertyValue(PropertyNames.CounterReset, value);
		}
	}

	public string Float
	{
		get
		{
			return GetPropertyValue(PropertyNames.Float);
		}
		set
		{
			SetPropertyValue(PropertyNames.Float, value);
		}
	}

	public string Cursor
	{
		get
		{
			return GetPropertyValue(PropertyNames.Cursor);
		}
		set
		{
			SetPropertyValue(PropertyNames.Cursor, value);
		}
	}

	public string Direction
	{
		get
		{
			return GetPropertyValue(PropertyNames.Direction);
		}
		set
		{
			SetPropertyValue(PropertyNames.Direction, value);
		}
	}

	public string Display
	{
		get
		{
			return GetPropertyValue(PropertyNames.Display);
		}
		set
		{
			SetPropertyValue(PropertyNames.Display, value);
		}
	}

	public string DominantBaseline
	{
		get
		{
			return GetPropertyValue(PropertyNames.DominantBaseline);
		}
		set
		{
			SetPropertyValue(PropertyNames.DominantBaseline, value);
		}
	}

	public string EmptyCells
	{
		get
		{
			return GetPropertyValue(PropertyNames.EmptyCells);
		}
		set
		{
			SetPropertyValue(PropertyNames.EmptyCells, value);
		}
	}

	public string EnableBackground
	{
		get
		{
			return GetPropertyValue(PropertyNames.EnableBackground);
		}
		set
		{
			SetPropertyValue(PropertyNames.EnableBackground, value);
		}
	}

	public string Fill
	{
		get
		{
			return GetPropertyValue(PropertyNames.Fill);
		}
		set
		{
			SetPropertyValue(PropertyNames.Fill, value);
		}
	}

	public string FillOpacity
	{
		get
		{
			return GetPropertyValue(PropertyNames.FillOpacity);
		}
		set
		{
			SetPropertyValue(PropertyNames.FillOpacity, value);
		}
	}

	public string FillRule
	{
		get
		{
			return GetPropertyValue(PropertyNames.FillRule);
		}
		set
		{
			SetPropertyValue(PropertyNames.FillRule, value);
		}
	}

	public string Filter
	{
		get
		{
			return GetPropertyValue(PropertyNames.Filter);
		}
		set
		{
			SetPropertyValue(PropertyNames.Filter, value);
		}
	}

	public string Flex
	{
		get
		{
			return GetPropertyValue(PropertyNames.Flex);
		}
		set
		{
			SetPropertyValue(PropertyNames.Flex, value);
		}
	}

	public string FlexBasis
	{
		get
		{
			return GetPropertyValue(PropertyNames.FlexBasis);
		}
		set
		{
			SetPropertyValue(PropertyNames.FlexBasis, value);
		}
	}

	public string FlexDirection
	{
		get
		{
			return GetPropertyValue(PropertyNames.FlexDirection);
		}
		set
		{
			SetPropertyValue(PropertyNames.FlexDirection, value);
		}
	}

	public string FlexFlow
	{
		get
		{
			return GetPropertyValue(PropertyNames.FlexFlow);
		}
		set
		{
			SetPropertyValue(PropertyNames.FlexFlow, value);
		}
	}

	public string FlexGrow
	{
		get
		{
			return GetPropertyValue(PropertyNames.FlexGrow);
		}
		set
		{
			SetPropertyValue(PropertyNames.FlexGrow, value);
		}
	}

	public string FlexShrink
	{
		get
		{
			return GetPropertyValue(PropertyNames.FlexShrink);
		}
		set
		{
			SetPropertyValue(PropertyNames.FlexShrink, value);
		}
	}

	public string FlexWrap
	{
		get
		{
			return GetPropertyValue(PropertyNames.FlexWrap);
		}
		set
		{
			SetPropertyValue(PropertyNames.FlexWrap, value);
		}
	}

	public string Font
	{
		get
		{
			return GetPropertyValue(PropertyNames.Font);
		}
		set
		{
			SetPropertyValue(PropertyNames.Font, value);
		}
	}

	public string FontFamily
	{
		get
		{
			return GetPropertyValue(PropertyNames.FontFamily);
		}
		set
		{
			SetPropertyValue(PropertyNames.FontFamily, value);
		}
	}

	public string FontFeatureSettings
	{
		get
		{
			return GetPropertyValue(PropertyNames.FontFeatureSettings);
		}
		set
		{
			SetPropertyValue(PropertyNames.FontFeatureSettings, value);
		}
	}

	public string FontSize
	{
		get
		{
			return GetPropertyValue(PropertyNames.FontSize);
		}
		set
		{
			SetPropertyValue(PropertyNames.FontSize, value);
		}
	}

	public string FontSizeAdjust
	{
		get
		{
			return GetPropertyValue(PropertyNames.FontSizeAdjust);
		}
		set
		{
			SetPropertyValue(PropertyNames.FontSizeAdjust, value);
		}
	}

	public string FontStretch
	{
		get
		{
			return GetPropertyValue(PropertyNames.FontStretch);
		}
		set
		{
			SetPropertyValue(PropertyNames.FontStretch, value);
		}
	}

	public string FontStyle
	{
		get
		{
			return GetPropertyValue(PropertyNames.FontStyle);
		}
		set
		{
			SetPropertyValue(PropertyNames.FontStyle, value);
		}
	}

	public string FontVariant
	{
		get
		{
			return GetPropertyValue(PropertyNames.FontVariant);
		}
		set
		{
			SetPropertyValue(PropertyNames.FontVariant, value);
		}
	}

	public string FontWeight
	{
		get
		{
			return GetPropertyValue(PropertyNames.FontWeight);
		}
		set
		{
			SetPropertyValue(PropertyNames.FontWeight, value);
		}
	}

	public string Gap
	{
		get
		{
			return GetPropertyValue(PropertyNames.Gap);
		}
		set
		{
			SetPropertyValue(PropertyNames.Gap, value);
		}
	}

	public string GlyphOrientationHorizontal
	{
		get
		{
			return GetPropertyValue(PropertyNames.GlyphOrientationHorizontal);
		}
		set
		{
			SetPropertyValue(PropertyNames.GlyphOrientationHorizontal, value);
		}
	}

	public string GlyphOrientationVertical
	{
		get
		{
			return GetPropertyValue(PropertyNames.GlyphOrientationVertical);
		}
		set
		{
			SetPropertyValue(PropertyNames.GlyphOrientationVertical, value);
		}
	}

	public string Height
	{
		get
		{
			return GetPropertyValue(PropertyNames.Height);
		}
		set
		{
			SetPropertyValue(PropertyNames.Height, value);
		}
	}

	public string ImeMode
	{
		get
		{
			return GetPropertyValue(PropertyNames.ImeMode);
		}
		set
		{
			SetPropertyValue(PropertyNames.ImeMode, value);
		}
	}

	public string JustifyContent
	{
		get
		{
			return GetPropertyValue(PropertyNames.JustifyContent);
		}
		set
		{
			SetPropertyValue(PropertyNames.JustifyContent, value);
		}
	}

	public string LayoutGrid
	{
		get
		{
			return GetPropertyValue(PropertyNames.LayoutGrid);
		}
		set
		{
			SetPropertyValue(PropertyNames.LayoutGrid, value);
		}
	}

	public string LayoutGridChar
	{
		get
		{
			return GetPropertyValue(PropertyNames.LayoutGridChar);
		}
		set
		{
			SetPropertyValue(PropertyNames.LayoutGridChar, value);
		}
	}

	public string LayoutGridLine
	{
		get
		{
			return GetPropertyValue(PropertyNames.LayoutGridLine);
		}
		set
		{
			SetPropertyValue(PropertyNames.LayoutGridLine, value);
		}
	}

	public string LayoutGridMode
	{
		get
		{
			return GetPropertyValue(PropertyNames.LayoutGridMode);
		}
		set
		{
			SetPropertyValue(PropertyNames.LayoutGridMode, value);
		}
	}

	public string LayoutGridType
	{
		get
		{
			return GetPropertyValue(PropertyNames.LayoutGridType);
		}
		set
		{
			SetPropertyValue(PropertyNames.LayoutGridType, value);
		}
	}

	public string Left
	{
		get
		{
			return GetPropertyValue(PropertyNames.Left);
		}
		set
		{
			SetPropertyValue(PropertyNames.Left, value);
		}
	}

	public string LetterSpacing
	{
		get
		{
			return GetPropertyValue(PropertyNames.LetterSpacing);
		}
		set
		{
			SetPropertyValue(PropertyNames.LetterSpacing, value);
		}
	}

	public string LineHeight
	{
		get
		{
			return GetPropertyValue(PropertyNames.LineHeight);
		}
		set
		{
			SetPropertyValue(PropertyNames.LineHeight, value);
		}
	}

	public string ListStyle
	{
		get
		{
			return GetPropertyValue(PropertyNames.ListStyle);
		}
		set
		{
			SetPropertyValue(PropertyNames.ListStyle, value);
		}
	}

	public string ListStyleImage
	{
		get
		{
			return GetPropertyValue(PropertyNames.ListStyleImage);
		}
		set
		{
			SetPropertyValue(PropertyNames.ListStyleImage, value);
		}
	}

	public string ListStylePosition
	{
		get
		{
			return GetPropertyValue(PropertyNames.ListStylePosition);
		}
		set
		{
			SetPropertyValue(PropertyNames.ListStylePosition, value);
		}
	}

	public string ListStyleType
	{
		get
		{
			return GetPropertyValue(PropertyNames.ListStyleType);
		}
		set
		{
			SetPropertyValue(PropertyNames.ListStyleType, value);
		}
	}

	public string Margin
	{
		get
		{
			return GetPropertyValue(PropertyNames.Margin);
		}
		set
		{
			SetPropertyValue(PropertyNames.Margin, value);
		}
	}

	public string MarginBottom
	{
		get
		{
			return GetPropertyValue(PropertyNames.MarginBottom);
		}
		set
		{
			SetPropertyValue(PropertyNames.MarginBottom, value);
		}
	}

	public string MarginLeft
	{
		get
		{
			return GetPropertyValue(PropertyNames.MarginLeft);
		}
		set
		{
			SetPropertyValue(PropertyNames.MarginLeft, value);
		}
	}

	public string MarginRight
	{
		get
		{
			return GetPropertyValue(PropertyNames.MarginRight);
		}
		set
		{
			SetPropertyValue(PropertyNames.MarginRight, value);
		}
	}

	public string MarginTop
	{
		get
		{
			return GetPropertyValue(PropertyNames.MarginTop);
		}
		set
		{
			SetPropertyValue(PropertyNames.MarginTop, value);
		}
	}

	public string Marker
	{
		get
		{
			return GetPropertyValue(PropertyNames.Marker);
		}
		set
		{
			SetPropertyValue(PropertyNames.Marker, value);
		}
	}

	public string MarkerEnd
	{
		get
		{
			return GetPropertyValue(PropertyNames.MarkerEnd);
		}
		set
		{
			SetPropertyValue(PropertyNames.MarkerEnd, value);
		}
	}

	public string MarkerMid
	{
		get
		{
			return GetPropertyValue(PropertyNames.MarkerMid);
		}
		set
		{
			SetPropertyValue(PropertyNames.MarkerMid, value);
		}
	}

	public string MarkerStart
	{
		get
		{
			return GetPropertyValue(PropertyNames.MarkerStart);
		}
		set
		{
			SetPropertyValue(PropertyNames.MarkerStart, value);
		}
	}

	public string Mask
	{
		get
		{
			return GetPropertyValue(PropertyNames.Mask);
		}
		set
		{
			SetPropertyValue(PropertyNames.Mask, value);
		}
	}

	public string MaxHeight
	{
		get
		{
			return GetPropertyValue(PropertyNames.MaxHeight);
		}
		set
		{
			SetPropertyValue(PropertyNames.MaxHeight, value);
		}
	}

	public string MaxWidth
	{
		get
		{
			return GetPropertyValue(PropertyNames.MaxWidth);
		}
		set
		{
			SetPropertyValue(PropertyNames.MaxWidth, value);
		}
	}

	public string MinHeight
	{
		get
		{
			return GetPropertyValue(PropertyNames.MinHeight);
		}
		set
		{
			SetPropertyValue(PropertyNames.MinHeight, value);
		}
	}

	public string MinWidth
	{
		get
		{
			return GetPropertyValue(PropertyNames.MinWidth);
		}
		set
		{
			SetPropertyValue(PropertyNames.MinWidth, value);
		}
	}

	public string Opacity
	{
		get
		{
			return GetPropertyValue(PropertyNames.Opacity);
		}
		set
		{
			SetPropertyValue(PropertyNames.Opacity, value);
		}
	}

	public string Order
	{
		get
		{
			return GetPropertyValue(PropertyNames.Order);
		}
		set
		{
			SetPropertyValue(PropertyNames.Order, value);
		}
	}

	public string Orphans
	{
		get
		{
			return GetPropertyValue(PropertyNames.Orphans);
		}
		set
		{
			SetPropertyValue(PropertyNames.Orphans, value);
		}
	}

	public string Outline
	{
		get
		{
			return GetPropertyValue(PropertyNames.Outline);
		}
		set
		{
			SetPropertyValue(PropertyNames.Outline, value);
		}
	}

	public string OutlineColor
	{
		get
		{
			return GetPropertyValue(PropertyNames.OutlineColor);
		}
		set
		{
			SetPropertyValue(PropertyNames.OutlineColor, value);
		}
	}

	public string OutlineStyle
	{
		get
		{
			return GetPropertyValue(PropertyNames.OutlineStyle);
		}
		set
		{
			SetPropertyValue(PropertyNames.OutlineStyle, value);
		}
	}

	public string OutlineWidth
	{
		get
		{
			return GetPropertyValue(PropertyNames.OutlineWidth);
		}
		set
		{
			SetPropertyValue(PropertyNames.OutlineWidth, value);
		}
	}

	public string Overflow
	{
		get
		{
			return GetPropertyValue(PropertyNames.Overflow);
		}
		set
		{
			SetPropertyValue(PropertyNames.Overflow, value);
		}
	}

	public string OverflowX
	{
		get
		{
			return GetPropertyValue(PropertyNames.OverflowX);
		}
		set
		{
			SetPropertyValue(PropertyNames.OverflowX, value);
		}
	}

	public string OverflowY
	{
		get
		{
			return GetPropertyValue(PropertyNames.OverflowY);
		}
		set
		{
			SetPropertyValue(PropertyNames.OverflowY, value);
		}
	}

	public string OverflowWrap
	{
		get
		{
			return GetPropertyValue(PropertyNames.WordWrap);
		}
		set
		{
			SetPropertyValue(PropertyNames.WordWrap, value);
		}
	}

	public string Padding
	{
		get
		{
			return GetPropertyValue(PropertyNames.Padding);
		}
		set
		{
			SetPropertyValue(PropertyNames.Padding, value);
		}
	}

	public string PaddingBottom
	{
		get
		{
			return GetPropertyValue(PropertyNames.PaddingBottom);
		}
		set
		{
			SetPropertyValue(PropertyNames.PaddingBottom, value);
		}
	}

	public string PaddingLeft
	{
		get
		{
			return GetPropertyValue(PropertyNames.PaddingLeft);
		}
		set
		{
			SetPropertyValue(PropertyNames.PaddingLeft, value);
		}
	}

	public string PaddingRight
	{
		get
		{
			return GetPropertyValue(PropertyNames.PaddingRight);
		}
		set
		{
			SetPropertyValue(PropertyNames.PaddingRight, value);
		}
	}

	public string PaddingTop
	{
		get
		{
			return GetPropertyValue(PropertyNames.PaddingTop);
		}
		set
		{
			SetPropertyValue(PropertyNames.PaddingTop, value);
		}
	}

	public string PageBreakAfter
	{
		get
		{
			return GetPropertyValue(PropertyNames.PageBreakAfter);
		}
		set
		{
			SetPropertyValue(PropertyNames.PageBreakAfter, value);
		}
	}

	public string PageBreakBefore
	{
		get
		{
			return GetPropertyValue(PropertyNames.PageBreakBefore);
		}
		set
		{
			SetPropertyValue(PropertyNames.PageBreakBefore, value);
		}
	}

	public string PageBreakInside
	{
		get
		{
			return GetPropertyValue(PropertyNames.PageBreakInside);
		}
		set
		{
			SetPropertyValue(PropertyNames.PageBreakInside, value);
		}
	}

	public string Perspective
	{
		get
		{
			return GetPropertyValue(PropertyNames.Perspective);
		}
		set
		{
			SetPropertyValue(PropertyNames.Perspective, value);
		}
	}

	public string PerspectiveOrigin
	{
		get
		{
			return GetPropertyValue(PropertyNames.PerspectiveOrigin);
		}
		set
		{
			SetPropertyValue(PropertyNames.PerspectiveOrigin, value);
		}
	}

	public string PointerEvents
	{
		get
		{
			return GetPropertyValue(PropertyNames.PointerEvents);
		}
		set
		{
			SetPropertyValue(PropertyNames.PointerEvents, value);
		}
	}

	public string RowGap
	{
		get
		{
			return GetPropertyValue(PropertyNames.RowGap);
		}
		set
		{
			SetPropertyValue(PropertyNames.RowGap, value);
		}
	}

	public string Quotes
	{
		get
		{
			return GetPropertyValue(PropertyNames.Quotes);
		}
		set
		{
			SetPropertyValue(PropertyNames.Quotes, value);
		}
	}

	public string Position
	{
		get
		{
			return GetPropertyValue(PropertyNames.Position);
		}
		set
		{
			SetPropertyValue(PropertyNames.Position, value);
		}
	}

	public string Right
	{
		get
		{
			return GetPropertyValue(PropertyNames.Right);
		}
		set
		{
			SetPropertyValue(PropertyNames.Right, value);
		}
	}

	public string RubyAlign
	{
		get
		{
			return GetPropertyValue(PropertyNames.RubyAlign);
		}
		set
		{
			SetPropertyValue(PropertyNames.RubyAlign, value);
		}
	}

	public string RubyOverhang
	{
		get
		{
			return GetPropertyValue(PropertyNames.RubyOverhang);
		}
		set
		{
			SetPropertyValue(PropertyNames.RubyOverhang, value);
		}
	}

	public string RubyPosition
	{
		get
		{
			return GetPropertyValue(PropertyNames.RubyPosition);
		}
		set
		{
			SetPropertyValue(PropertyNames.RubyPosition, value);
		}
	}

	public string Scrollbar3DLightColor
	{
		get
		{
			return GetPropertyValue(PropertyNames.Scrollbar3dLightColor);
		}
		set
		{
			SetPropertyValue(PropertyNames.Scrollbar3dLightColor, value);
		}
	}

	public string ScrollbarArrowColor
	{
		get
		{
			return GetPropertyValue(PropertyNames.ScrollbarArrowColor);
		}
		set
		{
			SetPropertyValue(PropertyNames.ScrollbarArrowColor, value);
		}
	}

	public string ScrollbarDarkShadowColor
	{
		get
		{
			return GetPropertyValue(PropertyNames.ScrollbarDarkShadowColor);
		}
		set
		{
			SetPropertyValue(PropertyNames.ScrollbarDarkShadowColor, value);
		}
	}

	public string ScrollbarFaceColor
	{
		get
		{
			return GetPropertyValue(PropertyNames.ScrollbarFaceColor);
		}
		set
		{
			SetPropertyValue(PropertyNames.ScrollbarFaceColor, value);
		}
	}

	public string ScrollbarHighlightColor
	{
		get
		{
			return GetPropertyValue(PropertyNames.ScrollbarHighlightColor);
		}
		set
		{
			SetPropertyValue(PropertyNames.ScrollbarHighlightColor, value);
		}
	}

	public string ScrollbarShadowColor
	{
		get
		{
			return GetPropertyValue(PropertyNames.ScrollbarShadowColor);
		}
		set
		{
			SetPropertyValue(PropertyNames.ScrollbarShadowColor, value);
		}
	}

	public string ScrollbarTrackColor
	{
		get
		{
			return GetPropertyValue(PropertyNames.ScrollbarTrackColor);
		}
		set
		{
			SetPropertyValue(PropertyNames.ScrollbarTrackColor, value);
		}
	}

	public string Stroke
	{
		get
		{
			return GetPropertyValue(PropertyNames.Stroke);
		}
		set
		{
			SetPropertyValue(PropertyNames.Stroke, value);
		}
	}

	public string StrokeDasharray
	{
		get
		{
			return GetPropertyValue(PropertyNames.StrokeDasharray);
		}
		set
		{
			SetPropertyValue(PropertyNames.StrokeDasharray, value);
		}
	}

	public string StrokeDashoffset
	{
		get
		{
			return GetPropertyValue(PropertyNames.StrokeDashoffset);
		}
		set
		{
			SetPropertyValue(PropertyNames.StrokeDashoffset, value);
		}
	}

	public string StrokeLinecap
	{
		get
		{
			return GetPropertyValue(PropertyNames.StrokeLinecap);
		}
		set
		{
			SetPropertyValue(PropertyNames.StrokeLinecap, value);
		}
	}

	public string StrokeLinejoin
	{
		get
		{
			return GetPropertyValue(PropertyNames.StrokeLinejoin);
		}
		set
		{
			SetPropertyValue(PropertyNames.StrokeLinejoin, value);
		}
	}

	public string StrokeMiterlimit
	{
		get
		{
			return GetPropertyValue(PropertyNames.StrokeMiterlimit);
		}
		set
		{
			SetPropertyValue(PropertyNames.StrokeMiterlimit, value);
		}
	}

	public string StrokeOpacity
	{
		get
		{
			return GetPropertyValue(PropertyNames.StrokeOpacity);
		}
		set
		{
			SetPropertyValue(PropertyNames.StrokeOpacity, value);
		}
	}

	public string StrokeWidth
	{
		get
		{
			return GetPropertyValue(PropertyNames.StrokeWidth);
		}
		set
		{
			SetPropertyValue(PropertyNames.StrokeWidth, value);
		}
	}

	public string TableLayout
	{
		get
		{
			return GetPropertyValue(PropertyNames.TableLayout);
		}
		set
		{
			SetPropertyValue(PropertyNames.TableLayout, value);
		}
	}

	public string TextAlign
	{
		get
		{
			return GetPropertyValue(PropertyNames.TextAlign);
		}
		set
		{
			SetPropertyValue(PropertyNames.TextAlign, value);
		}
	}

	public string TextAlignLast
	{
		get
		{
			return GetPropertyValue(PropertyNames.TextAlignLast);
		}
		set
		{
			SetPropertyValue(PropertyNames.TextAlignLast, value);
		}
	}

	public string TextAnchor
	{
		get
		{
			return GetPropertyValue(PropertyNames.TextAnchor);
		}
		set
		{
			SetPropertyValue(PropertyNames.TextAnchor, value);
		}
	}

	public string TextAutospace
	{
		get
		{
			return GetPropertyValue(PropertyNames.TextAutospace);
		}
		set
		{
			SetPropertyValue(PropertyNames.TextAutospace, value);
		}
	}

	public string TextDecoration
	{
		get
		{
			return GetPropertyValue(PropertyNames.TextDecoration);
		}
		set
		{
			SetPropertyValue(PropertyNames.TextDecoration, value);
		}
	}

	public string TextIndent
	{
		get
		{
			return GetPropertyValue(PropertyNames.TextIndent);
		}
		set
		{
			SetPropertyValue(PropertyNames.TextIndent, value);
		}
	}

	public string TextJustify
	{
		get
		{
			return GetPropertyValue(PropertyNames.TextJustify);
		}
		set
		{
			SetPropertyValue(PropertyNames.TextJustify, value);
		}
	}

	public string TextOverflow
	{
		get
		{
			return GetPropertyValue(PropertyNames.TextOverflow);
		}
		set
		{
			SetPropertyValue(PropertyNames.TextOverflow, value);
		}
	}

	public string TextShadow
	{
		get
		{
			return GetPropertyValue(PropertyNames.TextShadow);
		}
		set
		{
			SetPropertyValue(PropertyNames.TextShadow, value);
		}
	}

	public string TextTransform
	{
		get
		{
			return GetPropertyValue(PropertyNames.TextTransform);
		}
		set
		{
			SetPropertyValue(PropertyNames.TextTransform, value);
		}
	}

	public string TextUnderlinePosition
	{
		get
		{
			return GetPropertyValue(PropertyNames.TextUnderlinePosition);
		}
		set
		{
			SetPropertyValue(PropertyNames.TextUnderlinePosition, value);
		}
	}

	public string Top
	{
		get
		{
			return GetPropertyValue(PropertyNames.Top);
		}
		set
		{
			SetPropertyValue(PropertyNames.Top, value);
		}
	}

	public string Transform
	{
		get
		{
			return GetPropertyValue(PropertyNames.Transform);
		}
		set
		{
			SetPropertyValue(PropertyNames.Transform, value);
		}
	}

	public string TransformOrigin
	{
		get
		{
			return GetPropertyValue(PropertyNames.TransformOrigin);
		}
		set
		{
			SetPropertyValue(PropertyNames.TransformOrigin, value);
		}
	}

	public string TransformStyle
	{
		get
		{
			return GetPropertyValue(PropertyNames.TransformStyle);
		}
		set
		{
			SetPropertyValue(PropertyNames.TransformStyle, value);
		}
	}

	public string Transition
	{
		get
		{
			return GetPropertyValue(PropertyNames.Transition);
		}
		set
		{
			SetPropertyValue(PropertyNames.Transition, value);
		}
	}

	public string TransitionDelay
	{
		get
		{
			return GetPropertyValue(PropertyNames.TransitionDelay);
		}
		set
		{
			SetPropertyValue(PropertyNames.TransitionDelay, value);
		}
	}

	public string TransitionDuration
	{
		get
		{
			return GetPropertyValue(PropertyNames.TransitionDuration);
		}
		set
		{
			SetPropertyValue(PropertyNames.TransitionDuration, value);
		}
	}

	public string TransitionProperty
	{
		get
		{
			return GetPropertyValue(PropertyNames.TransitionProperty);
		}
		set
		{
			SetPropertyValue(PropertyNames.TransitionProperty, value);
		}
	}

	public string TransitionTimingFunction
	{
		get
		{
			return GetPropertyValue(PropertyNames.TransitionTimingFunction);
		}
		set
		{
			SetPropertyValue(PropertyNames.TransitionTimingFunction, value);
		}
	}

	public string UnicodeBidirectional
	{
		get
		{
			return GetPropertyValue(PropertyNames.UnicodeBidirectional);
		}
		set
		{
			SetPropertyValue(PropertyNames.UnicodeBidirectional, value);
		}
	}

	public string VerticalAlign
	{
		get
		{
			return GetPropertyValue(PropertyNames.VerticalAlign);
		}
		set
		{
			SetPropertyValue(PropertyNames.VerticalAlign, value);
		}
	}

	public string Visibility
	{
		get
		{
			return GetPropertyValue(PropertyNames.Visibility);
		}
		set
		{
			SetPropertyValue(PropertyNames.Visibility, value);
		}
	}

	public string WhiteSpace
	{
		get
		{
			return GetPropertyValue(PropertyNames.WhiteSpace);
		}
		set
		{
			SetPropertyValue(PropertyNames.WhiteSpace, value);
		}
	}

	public string Widows
	{
		get
		{
			return GetPropertyValue(PropertyNames.Widows);
		}
		set
		{
			SetPropertyValue(PropertyNames.Widows, value);
		}
	}

	public string Width
	{
		get
		{
			return GetPropertyValue(PropertyNames.Width);
		}
		set
		{
			SetPropertyValue(PropertyNames.Width, value);
		}
	}

	public string WordBreak
	{
		get
		{
			return GetPropertyValue(PropertyNames.WordBreak);
		}
		set
		{
			SetPropertyValue(PropertyNames.WordBreak, value);
		}
	}

	public string WordSpacing
	{
		get
		{
			return GetPropertyValue(PropertyNames.WordSpacing);
		}
		set
		{
			SetPropertyValue(PropertyNames.WordSpacing, value);
		}
	}

	public string WritingMode
	{
		get
		{
			return GetPropertyValue(PropertyNames.WritingMode);
		}
		set
		{
			SetPropertyValue(PropertyNames.WritingMode, value);
		}
	}

	public string ZIndex
	{
		get
		{
			return GetPropertyValue(PropertyNames.ZIndex);
		}
		set
		{
			SetPropertyValue(PropertyNames.ZIndex, value);
		}
	}

	public string Zoom
	{
		get
		{
			return GetPropertyValue(PropertyNames.Zoom);
		}
		set
		{
			SetPropertyValue(PropertyNames.Zoom, value);
		}
	}

	public event Action<string> Changed;

	private StyleDeclaration(Rule parent, StylesheetParser parser)
	{
		_parent = parent;
		_parser = parser;
	}

	internal StyleDeclaration(StylesheetParser parser)
		: this(null, parser)
	{
	}

	internal StyleDeclaration()
		: this(null, null)
	{
	}

	internal StyleDeclaration(Rule parent)
		: this(parent, parent.Parser)
	{
	}

	public void Update(string value)
	{
		Clear();
		if (!string.IsNullOrEmpty(value))
		{
			_parser.AppendDeclarations(this, value);
		}
	}

	public override void ToCss(TextWriter writer, IStyleFormatter formatter)
	{
		List<string> list = new List<string>();
		List<string> serialized = new List<string>();
		foreach (Property declaration in Declarations)
		{
			string name = declaration.Name;
			if (IsStrictMode)
			{
				if (serialized.Contains(name))
				{
					continue;
				}
				List<string> source = PropertyFactory.Instance.GetShorthands(name).ToList();
				if (source.Any())
				{
					List<Property> list2 = Declarations.Where((Property m) => !serialized.Contains(m.Name)).ToList();
					foreach (string item in source.OrderByDescending((string m) => PropertyFactory.Instance.GetLonghands(m).Length))
					{
						ShorthandProperty shorthandProperty = PropertyFactory.Instance.CreateShorthand(item);
						string[] properties = PropertyFactory.Instance.GetLonghands(item);
						Property[] array = list2.Where((Property m) => properties.Contains(m.Name)).ToArray();
						if (array.Length == 0)
						{
							continue;
						}
						int num = array.Count((Property m) => m.IsImportant);
						if ((num > 0 && num != array.Length) || properties.Length != array.Length)
						{
							continue;
						}
						string value = shorthandProperty.Stringify(array);
						if (!string.IsNullOrEmpty(value))
						{
							list.Add(CompressedStyleFormatter.Instance.Declaration(item, value, num != 0));
							Property[] array2 = array;
							foreach (Property property in array2)
							{
								serialized.Add(property.Name);
								list2.Remove(property);
							}
						}
					}
				}
				if (serialized.Contains(name))
				{
					continue;
				}
				serialized.Add(name);
			}
			list.Add(declaration.ToCss(formatter));
		}
		writer.Write(formatter.Declarations(list));
	}

	public string RemoveProperty(string propertyName)
	{
		string propertyValue = GetPropertyValue(propertyName);
		RemovePropertyByName(propertyName);
		RaiseChanged();
		return propertyValue;
	}

	private void RemovePropertyByName(string propertyName)
	{
		foreach (Property declaration in Declarations)
		{
			if (declaration.Name.Is(propertyName))
			{
				RemoveChild(declaration);
				break;
			}
		}
		if (IsStrictMode && PropertyFactory.Instance.IsShorthand(propertyName))
		{
			string[] longhands = PropertyFactory.Instance.GetLonghands(propertyName);
			foreach (string propertyName2 in longhands)
			{
				RemovePropertyByName(propertyName2);
			}
		}
	}

	public string GetPropertyPriority(string propertyName)
	{
		Property property = GetProperty(propertyName);
		if (property != null && property.IsImportant)
		{
			return Keywords.Important;
		}
		if (!IsStrictMode || !PropertyFactory.Instance.IsShorthand(propertyName))
		{
			return string.Empty;
		}
		if (!PropertyFactory.Instance.GetLonghands(propertyName).Any((string longhand) => !GetPropertyPriority(longhand).Isi(Keywords.Important)))
		{
			return Keywords.Important;
		}
		return string.Empty;
	}

	public string GetPropertyValue(string propertyName)
	{
		Property property = GetProperty(propertyName);
		if (property != null)
		{
			return property.Value;
		}
		if (!IsStrictMode || !PropertyFactory.Instance.IsShorthand(propertyName))
		{
			return string.Empty;
		}
		ShorthandProperty shorthandProperty = PropertyFactory.Instance.CreateShorthand(propertyName);
		string[] longhands = PropertyFactory.Instance.GetLonghands(propertyName);
		List<Property> list = new List<Property>();
		string[] array = longhands;
		foreach (string name in array)
		{
			property = GetProperty(name);
			if (property == null)
			{
				return string.Empty;
			}
			list.Add(property);
		}
		return shorthandProperty.Stringify(list.ToArray());
	}

	public void SetPropertyValue(string propertyName, string propertyValue)
	{
		SetProperty(propertyName, propertyValue);
	}

	public void SetPropertyPriority(string propertyName, string priority)
	{
		if (!string.IsNullOrEmpty(priority) && !priority.Isi(Keywords.Important))
		{
			return;
		}
		bool isImportant = !string.IsNullOrEmpty(priority);
		IEnumerable<string> enumerable;
		if (!IsStrictMode || !PropertyFactory.Instance.IsShorthand(propertyName))
		{
			enumerable = Enumerable.Repeat(propertyName, 1);
		}
		else
		{
			IEnumerable<string> longhands = PropertyFactory.Instance.GetLonghands(propertyName);
			enumerable = longhands;
		}
		foreach (string item in enumerable)
		{
			Property property = GetProperty(item);
			if (property != null)
			{
				property.IsImportant = isImportant;
			}
		}
	}

	public void SetProperty(string propertyName, string propertyValue, string priority = null)
	{
		if (!string.IsNullOrEmpty(propertyValue))
		{
			if (priority != null && !priority.Isi(Keywords.Important))
			{
				return;
			}
			TokenValue tokenValue = _parser.ParseValue(propertyValue);
			if (tokenValue != null)
			{
				Property property = CreateProperty(propertyName);
				if (property != null && property.TrySetValue(tokenValue))
				{
					property.IsImportant = priority != null;
					SetProperty(property);
					RaiseChanged();
				}
			}
		}
		else
		{
			RemoveProperty(propertyName);
		}
	}

	internal Property CreateProperty(string propertyName)
	{
		Property property = GetProperty(propertyName);
		if (property != null)
		{
			return property;
		}
		property = PropertyFactory.Instance.Create(propertyName);
		if (property != null || IsStrictMode)
		{
			return property;
		}
		return new UnknownProperty(propertyName);
	}

	internal Property GetProperty(string name)
	{
		return Declarations.FirstOrDefault((Property m) => m.Name.Isi(name));
	}

	internal void SetProperty(Property property)
	{
		if (property is ShorthandProperty shorthand)
		{
			SetShorthand(shorthand);
		}
		else
		{
			SetLonghand(property);
		}
	}

	internal void SetDeclarations(IEnumerable<Property> declarations)
	{
		ChangeDeclarations(declarations, (Property _) => false, (Property o, Property n) => !o.IsImportant || n.IsImportant);
	}

	internal void UpdateDeclarations(IEnumerable<Property> declarations)
	{
		ChangeDeclarations(declarations, (Property m) => !m.CanBeInherited, (Property o, Property _) => o.IsInherited);
	}

	private void ChangeDeclarations(IEnumerable<Property> declarations, Predicate<Property> defaultSkip, Func<Property, Property, bool> removeExisting)
	{
		List<Property> list = new List<Property>();
		foreach (Property declaration in declarations)
		{
			bool flag = defaultSkip(declaration);
			foreach (Property declaration2 in Declarations)
			{
				if (declaration2.Name.Is(declaration.Name))
				{
					if (removeExisting(declaration2, declaration))
					{
						RemoveChild(declaration2);
					}
					else
					{
						flag = true;
					}
					break;
				}
			}
			if (!flag)
			{
				list.Add(declaration);
			}
		}
		foreach (Property item in list)
		{
			AppendChild(item);
		}
	}

	private void SetLonghand(Property property)
	{
		if (!_parser.Options.PreserveDuplicateProperties)
		{
			foreach (Property declaration in Declarations)
			{
				if (declaration.Name.Is(property.Name))
				{
					RemoveChild(declaration);
					break;
				}
			}
		}
		AppendChild(property);
	}

	private void SetShorthand(ShorthandProperty shorthand)
	{
		Property[] array = PropertyFactory.Instance.CreateLonghandsFor(shorthand.Name);
		shorthand.Export(array);
		Property[] array2 = array;
		foreach (Property longhand in array2)
		{
			SetLonghand(longhand);
		}
	}

	private void RaiseChanged()
	{
		Changed?.Invoke(CssText);
	}

	public IEnumerator<IProperty> GetEnumerator()
	{
		return Declarations.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
