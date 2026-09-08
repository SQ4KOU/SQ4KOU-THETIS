using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Xml;
using ExCSS;
using Svg.FilterEffects;

namespace Svg;

[ElementFactory]
internal class SvgElementFactory
{
	[DebuggerDisplay("{ElementName}, {ElementType}")]
	internal sealed class ElementInfo
	{
		public string ElementName { get; set; }

		public Type ElementType { get; set; }

		public Func<SvgElement> CreateInstance { get; set; }

		public ElementInfo(string elementName, Type elementType)
		{
			ElementName = elementName;
			ElementType = elementType;
		}

		public ElementInfo()
		{
		}
	}

	private readonly StylesheetParser stylesheetParser = new StylesheetParser(includeUnknownRules: true, includeUnknownDeclarations: true, tolerateInvalidSelectors: false, tolerateInvalidValues: true);

	private static readonly List<ElementInfo> availableElements = new List<ElementInfo>
	{
		new ElementInfo
		{
			ElementName = "a",
			ElementType = typeof(SvgAnchor),
			CreateInstance = () => new SvgAnchor()
		},
		new ElementInfo
		{
			ElementName = "circle",
			ElementType = typeof(SvgCircle),
			CreateInstance = () => new SvgCircle()
		},
		new ElementInfo
		{
			ElementName = "clipPath",
			ElementType = typeof(SvgClipPath),
			CreateInstance = () => new SvgClipPath()
		},
		new ElementInfo
		{
			ElementName = "defs",
			ElementType = typeof(SvgDefinitionList),
			CreateInstance = () => new SvgDefinitionList()
		},
		new ElementInfo
		{
			ElementName = "desc",
			ElementType = typeof(SvgDescription),
			CreateInstance = () => new SvgDescription()
		},
		new ElementInfo
		{
			ElementName = "ellipse",
			ElementType = typeof(SvgEllipse),
			CreateInstance = () => new SvgEllipse()
		},
		new ElementInfo
		{
			ElementName = "feBlend",
			ElementType = typeof(SvgBlend),
			CreateInstance = () => new SvgBlend()
		},
		new ElementInfo
		{
			ElementName = "feColorMatrix",
			ElementType = typeof(SvgColourMatrix),
			CreateInstance = () => new SvgColourMatrix()
		},
		new ElementInfo
		{
			ElementName = "feComponentTransfer",
			ElementType = typeof(SvgComponentTransfer),
			CreateInstance = () => new SvgComponentTransfer()
		},
		new ElementInfo
		{
			ElementName = "feComposite",
			ElementType = typeof(SvgComposite),
			CreateInstance = () => new SvgComposite()
		},
		new ElementInfo
		{
			ElementName = "feConvolveMatrix",
			ElementType = typeof(SvgConvolveMatrix),
			CreateInstance = () => new SvgConvolveMatrix()
		},
		new ElementInfo
		{
			ElementName = "feDiffuseLighting",
			ElementType = typeof(SvgDiffuseLighting),
			CreateInstance = () => new SvgDiffuseLighting()
		},
		new ElementInfo
		{
			ElementName = "feDisplacementMap",
			ElementType = typeof(SvgDisplacementMap),
			CreateInstance = () => new SvgDisplacementMap()
		},
		new ElementInfo
		{
			ElementName = "feDistantLight",
			ElementType = typeof(SvgDistantLight),
			CreateInstance = () => new SvgDistantLight()
		},
		new ElementInfo
		{
			ElementName = "feFlood",
			ElementType = typeof(SvgFlood),
			CreateInstance = () => new SvgFlood()
		},
		new ElementInfo
		{
			ElementName = "feFuncA",
			ElementType = typeof(SvgFuncA),
			CreateInstance = () => new SvgFuncA()
		},
		new ElementInfo
		{
			ElementName = "feFuncB",
			ElementType = typeof(SvgFuncB),
			CreateInstance = () => new SvgFuncB()
		},
		new ElementInfo
		{
			ElementName = "feFuncG",
			ElementType = typeof(SvgFuncG),
			CreateInstance = () => new SvgFuncG()
		},
		new ElementInfo
		{
			ElementName = "feFuncR",
			ElementType = typeof(SvgFuncR),
			CreateInstance = () => new SvgFuncR()
		},
		new ElementInfo
		{
			ElementName = "feGaussianBlur",
			ElementType = typeof(SvgGaussianBlur),
			CreateInstance = () => new SvgGaussianBlur()
		},
		new ElementInfo
		{
			ElementName = "feImage",
			ElementType = typeof(Svg.FilterEffects.SvgImage),
			CreateInstance = () => new Svg.FilterEffects.SvgImage()
		},
		new ElementInfo
		{
			ElementName = "feMerge",
			ElementType = typeof(SvgMerge),
			CreateInstance = () => new SvgMerge()
		},
		new ElementInfo
		{
			ElementName = "feMergeNode",
			ElementType = typeof(SvgMergeNode),
			CreateInstance = () => new SvgMergeNode()
		},
		new ElementInfo
		{
			ElementName = "feMorphology",
			ElementType = typeof(SvgMorphology),
			CreateInstance = () => new SvgMorphology()
		},
		new ElementInfo
		{
			ElementName = "feOffset",
			ElementType = typeof(SvgOffset),
			CreateInstance = () => new SvgOffset()
		},
		new ElementInfo
		{
			ElementName = "fePointLight",
			ElementType = typeof(SvgPointLight),
			CreateInstance = () => new SvgPointLight()
		},
		new ElementInfo
		{
			ElementName = "feSpecularLighting",
			ElementType = typeof(SvgSpecularLighting),
			CreateInstance = () => new SvgSpecularLighting()
		},
		new ElementInfo
		{
			ElementName = "feSpotLight",
			ElementType = typeof(SvgSpotLight),
			CreateInstance = () => new SvgSpotLight()
		},
		new ElementInfo
		{
			ElementName = "feTile",
			ElementType = typeof(SvgTile),
			CreateInstance = () => new SvgTile()
		},
		new ElementInfo
		{
			ElementName = "feTurbulence",
			ElementType = typeof(SvgTurbulence),
			CreateInstance = () => new SvgTurbulence()
		},
		new ElementInfo
		{
			ElementName = "filter",
			ElementType = typeof(SvgFilter),
			CreateInstance = () => new SvgFilter()
		},
		new ElementInfo
		{
			ElementName = "font",
			ElementType = typeof(SvgFont),
			CreateInstance = () => new SvgFont()
		},
		new ElementInfo
		{
			ElementName = "font-face",
			ElementType = typeof(SvgFontFace),
			CreateInstance = () => new SvgFontFace()
		},
		new ElementInfo
		{
			ElementName = "font-face-src",
			ElementType = typeof(SvgFontFaceSrc),
			CreateInstance = () => new SvgFontFaceSrc()
		},
		new ElementInfo
		{
			ElementName = "font-face-uri",
			ElementType = typeof(SvgFontFaceUri),
			CreateInstance = () => new SvgFontFaceUri()
		},
		new ElementInfo
		{
			ElementName = "foreignObject",
			ElementType = typeof(SvgForeignObject),
			CreateInstance = () => new SvgForeignObject()
		},
		new ElementInfo
		{
			ElementName = "g",
			ElementType = typeof(SvgGroup),
			CreateInstance = () => new SvgGroup()
		},
		new ElementInfo
		{
			ElementName = "glyph",
			ElementType = typeof(SvgGlyph),
			CreateInstance = () => new SvgGlyph()
		},
		new ElementInfo
		{
			ElementName = "hkern",
			ElementType = typeof(SvgHorizontalKern),
			CreateInstance = () => new SvgHorizontalKern()
		},
		new ElementInfo
		{
			ElementName = "image",
			ElementType = typeof(SvgImage),
			CreateInstance = () => new SvgImage()
		},
		new ElementInfo
		{
			ElementName = "line",
			ElementType = typeof(SvgLine),
			CreateInstance = () => new SvgLine()
		},
		new ElementInfo
		{
			ElementName = "linearGradient",
			ElementType = typeof(SvgLinearGradientServer),
			CreateInstance = () => new SvgLinearGradientServer()
		},
		new ElementInfo
		{
			ElementName = "marker",
			ElementType = typeof(SvgMarker),
			CreateInstance = () => new SvgMarker()
		},
		new ElementInfo
		{
			ElementName = "mask",
			ElementType = typeof(SvgMask),
			CreateInstance = () => new SvgMask()
		},
		new ElementInfo
		{
			ElementName = "metadata",
			ElementType = typeof(SvgDocumentMetadata),
			CreateInstance = () => new SvgDocumentMetadata()
		},
		new ElementInfo
		{
			ElementName = "missing-glyph",
			ElementType = typeof(SvgMissingGlyph),
			CreateInstance = () => new SvgMissingGlyph()
		},
		new ElementInfo
		{
			ElementName = "path",
			ElementType = typeof(SvgPath),
			CreateInstance = () => new SvgPath()
		},
		new ElementInfo
		{
			ElementName = "pattern",
			ElementType = typeof(SvgPatternServer),
			CreateInstance = () => new SvgPatternServer()
		},
		new ElementInfo
		{
			ElementName = "polygon",
			ElementType = typeof(SvgPolygon),
			CreateInstance = () => new SvgPolygon()
		},
		new ElementInfo
		{
			ElementName = "polyline",
			ElementType = typeof(SvgPolyline),
			CreateInstance = () => new SvgPolyline()
		},
		new ElementInfo
		{
			ElementName = "radialGradient",
			ElementType = typeof(SvgRadialGradientServer),
			CreateInstance = () => new SvgRadialGradientServer()
		},
		new ElementInfo
		{
			ElementName = "rect",
			ElementType = typeof(SvgRectangle),
			CreateInstance = () => new SvgRectangle()
		},
		new ElementInfo
		{
			ElementName = "script",
			ElementType = typeof(SvgScript),
			CreateInstance = () => new SvgScript()
		},
		new ElementInfo
		{
			ElementName = "stop",
			ElementType = typeof(SvgGradientStop),
			CreateInstance = () => new SvgGradientStop()
		},
		new ElementInfo
		{
			ElementName = "svg",
			ElementType = typeof(SvgFragment),
			CreateInstance = () => new SvgFragment()
		},
		new ElementInfo
		{
			ElementName = "switch",
			ElementType = typeof(SvgSwitch),
			CreateInstance = () => new SvgSwitch()
		},
		new ElementInfo
		{
			ElementName = "symbol",
			ElementType = typeof(SvgSymbol),
			CreateInstance = () => new SvgSymbol()
		},
		new ElementInfo
		{
			ElementName = "text",
			ElementType = typeof(SvgText),
			CreateInstance = () => new SvgText()
		},
		new ElementInfo
		{
			ElementName = "textPath",
			ElementType = typeof(SvgTextPath),
			CreateInstance = () => new SvgTextPath()
		},
		new ElementInfo
		{
			ElementName = "title",
			ElementType = typeof(SvgTitle),
			CreateInstance = () => new SvgTitle()
		},
		new ElementInfo
		{
			ElementName = "tref",
			ElementType = typeof(SvgTextRef),
			CreateInstance = () => new SvgTextRef()
		},
		new ElementInfo
		{
			ElementName = "tspan",
			ElementType = typeof(SvgTextSpan),
			CreateInstance = () => new SvgTextSpan()
		},
		new ElementInfo
		{
			ElementName = "use",
			ElementType = typeof(SvgUse),
			CreateInstance = () => new SvgUse()
		},
		new ElementInfo
		{
			ElementName = "vkern",
			ElementType = typeof(SvgVerticalKern),
			CreateInstance = () => new SvgVerticalKern()
		}
	};

	private static readonly Dictionary<string, ElementInfo> availableElementsWithoutSvg = new Dictionary<string, ElementInfo>
	{
		["a"] = new ElementInfo
		{
			ElementName = "a",
			ElementType = typeof(SvgAnchor),
			CreateInstance = () => new SvgAnchor()
		},
		["circle"] = new ElementInfo
		{
			ElementName = "circle",
			ElementType = typeof(SvgCircle),
			CreateInstance = () => new SvgCircle()
		},
		["clipPath"] = new ElementInfo
		{
			ElementName = "clipPath",
			ElementType = typeof(SvgClipPath),
			CreateInstance = () => new SvgClipPath()
		},
		["defs"] = new ElementInfo
		{
			ElementName = "defs",
			ElementType = typeof(SvgDefinitionList),
			CreateInstance = () => new SvgDefinitionList()
		},
		["desc"] = new ElementInfo
		{
			ElementName = "desc",
			ElementType = typeof(SvgDescription),
			CreateInstance = () => new SvgDescription()
		},
		["ellipse"] = new ElementInfo
		{
			ElementName = "ellipse",
			ElementType = typeof(SvgEllipse),
			CreateInstance = () => new SvgEllipse()
		},
		["feBlend"] = new ElementInfo
		{
			ElementName = "feBlend",
			ElementType = typeof(SvgBlend),
			CreateInstance = () => new SvgBlend()
		},
		["feColorMatrix"] = new ElementInfo
		{
			ElementName = "feColorMatrix",
			ElementType = typeof(SvgColourMatrix),
			CreateInstance = () => new SvgColourMatrix()
		},
		["feComponentTransfer"] = new ElementInfo
		{
			ElementName = "feComponentTransfer",
			ElementType = typeof(SvgComponentTransfer),
			CreateInstance = () => new SvgComponentTransfer()
		},
		["feComposite"] = new ElementInfo
		{
			ElementName = "feComposite",
			ElementType = typeof(SvgComposite),
			CreateInstance = () => new SvgComposite()
		},
		["feConvolveMatrix"] = new ElementInfo
		{
			ElementName = "feConvolveMatrix",
			ElementType = typeof(SvgConvolveMatrix),
			CreateInstance = () => new SvgConvolveMatrix()
		},
		["feDiffuseLighting"] = new ElementInfo
		{
			ElementName = "feDiffuseLighting",
			ElementType = typeof(SvgDiffuseLighting),
			CreateInstance = () => new SvgDiffuseLighting()
		},
		["feDisplacementMap"] = new ElementInfo
		{
			ElementName = "feDisplacementMap",
			ElementType = typeof(SvgDisplacementMap),
			CreateInstance = () => new SvgDisplacementMap()
		},
		["feDistantLight"] = new ElementInfo
		{
			ElementName = "feDistantLight",
			ElementType = typeof(SvgDistantLight),
			CreateInstance = () => new SvgDistantLight()
		},
		["feFlood"] = new ElementInfo
		{
			ElementName = "feFlood",
			ElementType = typeof(SvgFlood),
			CreateInstance = () => new SvgFlood()
		},
		["feFuncA"] = new ElementInfo
		{
			ElementName = "feFuncA",
			ElementType = typeof(SvgFuncA),
			CreateInstance = () => new SvgFuncA()
		},
		["feFuncB"] = new ElementInfo
		{
			ElementName = "feFuncB",
			ElementType = typeof(SvgFuncB),
			CreateInstance = () => new SvgFuncB()
		},
		["feFuncG"] = new ElementInfo
		{
			ElementName = "feFuncG",
			ElementType = typeof(SvgFuncG),
			CreateInstance = () => new SvgFuncG()
		},
		["feFuncR"] = new ElementInfo
		{
			ElementName = "feFuncR",
			ElementType = typeof(SvgFuncR),
			CreateInstance = () => new SvgFuncR()
		},
		["feGaussianBlur"] = new ElementInfo
		{
			ElementName = "feGaussianBlur",
			ElementType = typeof(SvgGaussianBlur),
			CreateInstance = () => new SvgGaussianBlur()
		},
		["feImage"] = new ElementInfo
		{
			ElementName = "feImage",
			ElementType = typeof(Svg.FilterEffects.SvgImage),
			CreateInstance = () => new Svg.FilterEffects.SvgImage()
		},
		["feMerge"] = new ElementInfo
		{
			ElementName = "feMerge",
			ElementType = typeof(SvgMerge),
			CreateInstance = () => new SvgMerge()
		},
		["feMergeNode"] = new ElementInfo
		{
			ElementName = "feMergeNode",
			ElementType = typeof(SvgMergeNode),
			CreateInstance = () => new SvgMergeNode()
		},
		["feMorphology"] = new ElementInfo
		{
			ElementName = "feMorphology",
			ElementType = typeof(SvgMorphology),
			CreateInstance = () => new SvgMorphology()
		},
		["feOffset"] = new ElementInfo
		{
			ElementName = "feOffset",
			ElementType = typeof(SvgOffset),
			CreateInstance = () => new SvgOffset()
		},
		["fePointLight"] = new ElementInfo
		{
			ElementName = "fePointLight",
			ElementType = typeof(SvgPointLight),
			CreateInstance = () => new SvgPointLight()
		},
		["feSpecularLighting"] = new ElementInfo
		{
			ElementName = "feSpecularLighting",
			ElementType = typeof(SvgSpecularLighting),
			CreateInstance = () => new SvgSpecularLighting()
		},
		["feSpotLight"] = new ElementInfo
		{
			ElementName = "feSpotLight",
			ElementType = typeof(SvgSpotLight),
			CreateInstance = () => new SvgSpotLight()
		},
		["feTile"] = new ElementInfo
		{
			ElementName = "feTile",
			ElementType = typeof(SvgTile),
			CreateInstance = () => new SvgTile()
		},
		["feTurbulence"] = new ElementInfo
		{
			ElementName = "feTurbulence",
			ElementType = typeof(SvgTurbulence),
			CreateInstance = () => new SvgTurbulence()
		},
		["filter"] = new ElementInfo
		{
			ElementName = "filter",
			ElementType = typeof(SvgFilter),
			CreateInstance = () => new SvgFilter()
		},
		["font"] = new ElementInfo
		{
			ElementName = "font",
			ElementType = typeof(SvgFont),
			CreateInstance = () => new SvgFont()
		},
		["font-face"] = new ElementInfo
		{
			ElementName = "font-face",
			ElementType = typeof(SvgFontFace),
			CreateInstance = () => new SvgFontFace()
		},
		["font-face-src"] = new ElementInfo
		{
			ElementName = "font-face-src",
			ElementType = typeof(SvgFontFaceSrc),
			CreateInstance = () => new SvgFontFaceSrc()
		},
		["font-face-uri"] = new ElementInfo
		{
			ElementName = "font-face-uri",
			ElementType = typeof(SvgFontFaceUri),
			CreateInstance = () => new SvgFontFaceUri()
		},
		["foreignObject"] = new ElementInfo
		{
			ElementName = "foreignObject",
			ElementType = typeof(SvgForeignObject),
			CreateInstance = () => new SvgForeignObject()
		},
		["g"] = new ElementInfo
		{
			ElementName = "g",
			ElementType = typeof(SvgGroup),
			CreateInstance = () => new SvgGroup()
		},
		["glyph"] = new ElementInfo
		{
			ElementName = "glyph",
			ElementType = typeof(SvgGlyph),
			CreateInstance = () => new SvgGlyph()
		},
		["hkern"] = new ElementInfo
		{
			ElementName = "hkern",
			ElementType = typeof(SvgHorizontalKern),
			CreateInstance = () => new SvgHorizontalKern()
		},
		["image"] = new ElementInfo
		{
			ElementName = "image",
			ElementType = typeof(SvgImage),
			CreateInstance = () => new SvgImage()
		},
		["line"] = new ElementInfo
		{
			ElementName = "line",
			ElementType = typeof(SvgLine),
			CreateInstance = () => new SvgLine()
		},
		["linearGradient"] = new ElementInfo
		{
			ElementName = "linearGradient",
			ElementType = typeof(SvgLinearGradientServer),
			CreateInstance = () => new SvgLinearGradientServer()
		},
		["marker"] = new ElementInfo
		{
			ElementName = "marker",
			ElementType = typeof(SvgMarker),
			CreateInstance = () => new SvgMarker()
		},
		["mask"] = new ElementInfo
		{
			ElementName = "mask",
			ElementType = typeof(SvgMask),
			CreateInstance = () => new SvgMask()
		},
		["metadata"] = new ElementInfo
		{
			ElementName = "metadata",
			ElementType = typeof(SvgDocumentMetadata),
			CreateInstance = () => new SvgDocumentMetadata()
		},
		["missing-glyph"] = new ElementInfo
		{
			ElementName = "missing-glyph",
			ElementType = typeof(SvgMissingGlyph),
			CreateInstance = () => new SvgMissingGlyph()
		},
		["path"] = new ElementInfo
		{
			ElementName = "path",
			ElementType = typeof(SvgPath),
			CreateInstance = () => new SvgPath()
		},
		["pattern"] = new ElementInfo
		{
			ElementName = "pattern",
			ElementType = typeof(SvgPatternServer),
			CreateInstance = () => new SvgPatternServer()
		},
		["polygon"] = new ElementInfo
		{
			ElementName = "polygon",
			ElementType = typeof(SvgPolygon),
			CreateInstance = () => new SvgPolygon()
		},
		["polyline"] = new ElementInfo
		{
			ElementName = "polyline",
			ElementType = typeof(SvgPolyline),
			CreateInstance = () => new SvgPolyline()
		},
		["radialGradient"] = new ElementInfo
		{
			ElementName = "radialGradient",
			ElementType = typeof(SvgRadialGradientServer),
			CreateInstance = () => new SvgRadialGradientServer()
		},
		["rect"] = new ElementInfo
		{
			ElementName = "rect",
			ElementType = typeof(SvgRectangle),
			CreateInstance = () => new SvgRectangle()
		},
		["script"] = new ElementInfo
		{
			ElementName = "script",
			ElementType = typeof(SvgScript),
			CreateInstance = () => new SvgScript()
		},
		["stop"] = new ElementInfo
		{
			ElementName = "stop",
			ElementType = typeof(SvgGradientStop),
			CreateInstance = () => new SvgGradientStop()
		},
		["switch"] = new ElementInfo
		{
			ElementName = "switch",
			ElementType = typeof(SvgSwitch),
			CreateInstance = () => new SvgSwitch()
		},
		["symbol"] = new ElementInfo
		{
			ElementName = "symbol",
			ElementType = typeof(SvgSymbol),
			CreateInstance = () => new SvgSymbol()
		},
		["text"] = new ElementInfo
		{
			ElementName = "text",
			ElementType = typeof(SvgText),
			CreateInstance = () => new SvgText()
		},
		["textPath"] = new ElementInfo
		{
			ElementName = "textPath",
			ElementType = typeof(SvgTextPath),
			CreateInstance = () => new SvgTextPath()
		},
		["title"] = new ElementInfo
		{
			ElementName = "title",
			ElementType = typeof(SvgTitle),
			CreateInstance = () => new SvgTitle()
		},
		["tref"] = new ElementInfo
		{
			ElementName = "tref",
			ElementType = typeof(SvgTextRef),
			CreateInstance = () => new SvgTextRef()
		},
		["tspan"] = new ElementInfo
		{
			ElementName = "tspan",
			ElementType = typeof(SvgTextSpan),
			CreateInstance = () => new SvgTextSpan()
		},
		["use"] = new ElementInfo
		{
			ElementName = "use",
			ElementType = typeof(SvgUse),
			CreateInstance = () => new SvgUse()
		},
		["vkern"] = new ElementInfo
		{
			ElementName = "vkern",
			ElementType = typeof(SvgVerticalKern),
			CreateInstance = () => new SvgVerticalKern()
		}
	};

	private static readonly Dictionary<string, List<Type>> availableElementsDictionary = new Dictionary<string, List<Type>>
	{
		["a"] = new List<Type> { typeof(SvgAnchor) },
		["circle"] = new List<Type> { typeof(SvgCircle) },
		["clipPath"] = new List<Type> { typeof(SvgClipPath) },
		["defs"] = new List<Type> { typeof(SvgDefinitionList) },
		["desc"] = new List<Type> { typeof(SvgDescription) },
		["ellipse"] = new List<Type> { typeof(SvgEllipse) },
		["feBlend"] = new List<Type> { typeof(SvgBlend) },
		["feColorMatrix"] = new List<Type> { typeof(SvgColourMatrix) },
		["feComponentTransfer"] = new List<Type> { typeof(SvgComponentTransfer) },
		["feComposite"] = new List<Type> { typeof(SvgComposite) },
		["feConvolveMatrix"] = new List<Type> { typeof(SvgConvolveMatrix) },
		["feDiffuseLighting"] = new List<Type> { typeof(SvgDiffuseLighting) },
		["feDisplacementMap"] = new List<Type> { typeof(SvgDisplacementMap) },
		["feDistantLight"] = new List<Type> { typeof(SvgDistantLight) },
		["feFlood"] = new List<Type> { typeof(SvgFlood) },
		["feFuncA"] = new List<Type> { typeof(SvgFuncA) },
		["feFuncB"] = new List<Type> { typeof(SvgFuncB) },
		["feFuncG"] = new List<Type> { typeof(SvgFuncG) },
		["feFuncR"] = new List<Type> { typeof(SvgFuncR) },
		["feGaussianBlur"] = new List<Type> { typeof(SvgGaussianBlur) },
		["feImage"] = new List<Type> { typeof(Svg.FilterEffects.SvgImage) },
		["feMerge"] = new List<Type> { typeof(SvgMerge) },
		["feMergeNode"] = new List<Type> { typeof(SvgMergeNode) },
		["feMorphology"] = new List<Type> { typeof(SvgMorphology) },
		["feOffset"] = new List<Type> { typeof(SvgOffset) },
		["fePointLight"] = new List<Type> { typeof(SvgPointLight) },
		["feSpecularLighting"] = new List<Type> { typeof(SvgSpecularLighting) },
		["feSpotLight"] = new List<Type> { typeof(SvgSpotLight) },
		["feTile"] = new List<Type> { typeof(SvgTile) },
		["feTurbulence"] = new List<Type> { typeof(SvgTurbulence) },
		["filter"] = new List<Type> { typeof(SvgFilter) },
		["font"] = new List<Type> { typeof(SvgFont) },
		["font-face"] = new List<Type> { typeof(SvgFontFace) },
		["font-face-src"] = new List<Type> { typeof(SvgFontFaceSrc) },
		["font-face-uri"] = new List<Type> { typeof(SvgFontFaceUri) },
		["foreignObject"] = new List<Type> { typeof(SvgForeignObject) },
		["g"] = new List<Type> { typeof(SvgGroup) },
		["glyph"] = new List<Type> { typeof(SvgGlyph) },
		["hkern"] = new List<Type> { typeof(SvgHorizontalKern) },
		["image"] = new List<Type> { typeof(SvgImage) },
		["line"] = new List<Type> { typeof(SvgLine) },
		["linearGradient"] = new List<Type> { typeof(SvgLinearGradientServer) },
		["marker"] = new List<Type> { typeof(SvgMarker) },
		["mask"] = new List<Type> { typeof(SvgMask) },
		["metadata"] = new List<Type> { typeof(SvgDocumentMetadata) },
		["missing-glyph"] = new List<Type> { typeof(SvgMissingGlyph) },
		["path"] = new List<Type> { typeof(SvgPath) },
		["pattern"] = new List<Type> { typeof(SvgPatternServer) },
		["polygon"] = new List<Type> { typeof(SvgPolygon) },
		["polyline"] = new List<Type> { typeof(SvgPolyline) },
		["radialGradient"] = new List<Type> { typeof(SvgRadialGradientServer) },
		["rect"] = new List<Type> { typeof(SvgRectangle) },
		["script"] = new List<Type> { typeof(SvgScript) },
		["stop"] = new List<Type> { typeof(SvgGradientStop) },
		["svg"] = new List<Type> { typeof(SvgFragment) },
		["switch"] = new List<Type> { typeof(SvgSwitch) },
		["symbol"] = new List<Type> { typeof(SvgSymbol) },
		["text"] = new List<Type> { typeof(SvgText) },
		["textPath"] = new List<Type> { typeof(SvgTextPath) },
		["title"] = new List<Type> { typeof(SvgTitle) },
		["tref"] = new List<Type> { typeof(SvgTextRef) },
		["tspan"] = new List<Type> { typeof(SvgTextSpan) },
		["use"] = new List<Type> { typeof(SvgUse) },
		["vkern"] = new List<Type> { typeof(SvgVerticalKern) }
	};

	public List<ElementInfo> AvailableElements => availableElements;

	internal Dictionary<string, List<Type>> AvailableElementsDictionary => availableElementsDictionary;

	public T CreateDocument<T>(XmlReader reader) where T : SvgDocument, new()
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		if (reader.LocalName != "svg")
		{
			throw new InvalidOperationException("The CreateDocument method can only be used to parse root <svg> elements.");
		}
		return (T)CreateElement<T>(reader, fragmentIsDocument: true, null);
	}

	public SvgElement CreateElement(XmlReader reader, SvgDocument document)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		return CreateElement<SvgDocument>(reader, fragmentIsDocument: false, document);
	}

	private SvgElement CreateElement<T>(XmlReader reader, bool fragmentIsDocument, SvgDocument document) where T : SvgDocument, new()
	{
		SvgElement svgElement = null;
		string localName = reader.LocalName;
		string namespaceURI = reader.NamespaceURI;
		if (namespaceURI == "http://www.w3.org/2000/svg" || string.IsNullOrEmpty(namespaceURI))
		{
			svgElement = ((localName == "svg") ? (fragmentIsDocument ? new T() : new SvgFragment()) : ((!availableElementsWithoutSvg.TryGetValue(localName, out var value)) ? new SvgUnknownElement(localName) : value.CreateInstance()));
			if (svgElement != null)
			{
				SetAttributes(svgElement, reader, document);
			}
		}
		else
		{
			svgElement = new NonSvgElement(localName, namespaceURI);
			SetAttributes(svgElement, reader, document);
		}
		return svgElement;
	}

	private void SetAttributes(SvgElement element, XmlReader reader, SvgDocument document)
	{
		while (reader.MoveToNextAttribute())
		{
			string prefix = reader.Prefix;
			string localName = reader.LocalName;
			if (!reader.ReadAttributeValue())
			{
				continue;
			}
			if (prefix.Length == 0)
			{
				if (localName.Equals("xmlns"))
				{
					element.Namespaces[string.Empty] = reader.Value;
					continue;
				}
				if (localName.Equals("version"))
				{
					continue;
				}
			}
			else if (prefix.Equals("xmlns"))
			{
				element.Namespaces[localName] = reader.Value;
				continue;
			}
			if (localName.Equals("style") && !(element is NonSvgElement))
			{
				foreach (IStyleRule styleRule in stylesheetParser.Parse("#a{" + reader.Value + "}").StyleRules)
				{
					foreach (IProperty item in styleRule.Style)
					{
						element.AddStyle(item.Name, item.Original, 65536);
					}
				}
			}
			else if (prefix.Length == 0 && IsStyleAttribute(localName))
			{
				element.AddStyle(localName, reader.Value, 0);
			}
			else
			{
				string ns = ((prefix.Length == 0) ? string.Empty : reader.LookupNamespace(prefix));
				SetPropertyValue(element, ns, localName, reader.Value, document);
			}
		}
	}

	private static bool IsStyleAttribute(string name)
	{
		switch (name)
		{
		case "baseline-shift":
		case "letter-spacing":
		case "lighting-color":
		case "pointer-events":
		case "stroke-linecap":
		case "stroke-opacity":
		case "text-rendering":
		case "text-transform":
		case "clip":
		case "font":
		case "mask":
		case "fill":
		case "font-size":
		case "direction":
		case "clip-path":
		case "clip-rule":
		case "fill-rule":
		case "color-profile":
		case "flood-opacity":
		case "image-rendering":
		case "color-rendering":
		case "stroke-linejoin":
		case "text-decoration":
		case "shape-rendering":
		case "marker":
		case "stroke":
		case "cursor":
		case "filter":
		case "display":
		case "kerning":
		case "opacity":
		case "stroke-miterlimit":
		case "dominant-baseline":
		case "enable-background":
		case "stroke-dashoffset":
		case "fill-opacity":
		case "font-stretch":
		case "font-variant":
		case "writing-mode":
		case "marker-start":
		case "stop-opacity":
		case "stroke-width":
		case "unicode-bidi":
		case "word-spacing":
		case "font-family":
		case "font-weight":
		case "flood-color":
		case "text-anchor":
		case "font-size-adjust":
		case "stroke-dasharray":
		case "font-style":
		case "marker-end":
		case "marker-mid":
		case "stop-color":
		case "visibility":
		case "alignment-baseline":
		case "color":
		case "color-interpolation":
		case "color-interpolation-filters":
		case "glyph-orientation-horizontal":
		case "glyph-orientation-vertical":
		case "overflow":
			return true;
		default:
			return false;
		}
	}

	internal static bool SetPropertyValue(SvgElement element, string ns, string attributeName, string attributeValue, SvgDocument document, bool isStyle = false)
	{
		if (attributeName == "opacity" && attributeValue == "undefined")
		{
			attributeValue = "1";
		}
		if (element.SetValue(attributeName, document, CultureInfo.InvariantCulture, attributeValue))
		{
			return true;
		}
		if (isStyle)
		{
			return false;
		}
		element.CustomAttributes[(ns.Length == 0) ? attributeName : (ns + ":" + attributeName)] = attributeValue;
		return true;
	}
}
