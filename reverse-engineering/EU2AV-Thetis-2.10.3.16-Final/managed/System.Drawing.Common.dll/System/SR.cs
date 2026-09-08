using System.Resources;
using FxResources.System.Drawing.Common;

namespace System;

internal static class SR
{
	private static readonly bool s_usingResourceKeys = AppContext.TryGetSwitch("System.Resources.UseSystemResourceKeys", out var isEnabled) & isEnabled;

	private static ResourceManager s_resourceManager;

	internal static ResourceManager ResourceManager => s_resourceManager ?? (s_resourceManager = new ResourceManager(typeof(FxResources.System.Drawing.Common.SR)));

	internal static string CantTellPrinterName => GetResourceString("CantTellPrinterName");

	internal static string CantChangeImmutableObjects => GetResourceString("CantChangeImmutableObjects");

	internal static string CantMakeIconTransparent => GetResourceString("CantMakeIconTransparent");

	internal static string ColorNotSystemColor => GetResourceString("ColorNotSystemColor");

	internal static string GdiplusAborted => GetResourceString("GdiplusAborted");

	internal static string GdiplusAccessDenied => GetResourceString("GdiplusAccessDenied");

	internal static string GdiplusCannotCreateGraphicsFromIndexedPixelFormat => GetResourceString("GdiplusCannotCreateGraphicsFromIndexedPixelFormat");

	internal static string GdiplusCannotSetPixelFromIndexedPixelFormat => GetResourceString("GdiplusCannotSetPixelFromIndexedPixelFormat");

	internal static string GdiplusDestPointsInvalidParallelogram => GetResourceString("GdiplusDestPointsInvalidParallelogram");

	internal static string GdiplusDestPointsInvalidLength => GetResourceString("GdiplusDestPointsInvalidLength");

	internal static string GdiplusFileNotFound => GetResourceString("GdiplusFileNotFound");

	internal static string GdiplusFontFamilyNotFound => GetResourceString("GdiplusFontFamilyNotFound");

	internal static string GdiplusFontStyleNotFound => GetResourceString("GdiplusFontStyleNotFound");

	internal static string GdiplusGenericError => GetResourceString("GdiplusGenericError");

	internal static string GdiplusInsufficientBuffer => GetResourceString("GdiplusInsufficientBuffer");

	internal static string GdiplusInvalidParameter => GetResourceString("GdiplusInvalidParameter");

	internal static string GdiplusInvalidRectangle => GetResourceString("GdiplusInvalidRectangle");

	internal static string GdiplusInvalidSize => GetResourceString("GdiplusInvalidSize");

	internal static string GdiplusNotImplemented => GetResourceString("GdiplusNotImplemented");

	internal static string GdiplusNotInitialized => GetResourceString("GdiplusNotInitialized");

	internal static string GdiplusNotTrueTypeFont => GetResourceString("GdiplusNotTrueTypeFont");

	internal static string GdiplusNotTrueTypeFont_NoName => GetResourceString("GdiplusNotTrueTypeFont_NoName");

	internal static string GdiplusObjectBusy => GetResourceString("GdiplusObjectBusy");

	internal static string GdiplusOverflow => GetResourceString("GdiplusOverflow");

	internal static string GdiplusPropertyNotFoundError => GetResourceString("GdiplusPropertyNotFoundError");

	internal static string GdiplusPropertyNotSupportedError => GetResourceString("GdiplusPropertyNotSupportedError");

	internal static string GdiplusUnknown => GetResourceString("GdiplusUnknown");

	internal static string GdiplusUnknownImageFormat => GetResourceString("GdiplusUnknownImageFormat");

	internal static string GdiplusUnsupportedGdiplusVersion => GetResourceString("GdiplusUnsupportedGdiplusVersion");

	internal static string GdiplusWrongState => GetResourceString("GdiplusWrongState");

	internal static string GlobalAssemblyCache => GetResourceString("GlobalAssemblyCache");

	internal static string GraphicsBufferCurrentlyBusy => GetResourceString("GraphicsBufferCurrentlyBusy");

	internal static string GraphicsBufferQueryFail => GetResourceString("GraphicsBufferQueryFail");

	internal static string IconInvalidMaskLength => GetResourceString("IconInvalidMaskLength");

	internal static string IllegalState => GetResourceString("IllegalState");

	internal static string InterpolationColorsColorBlendNotSet => GetResourceString("InterpolationColorsColorBlendNotSet");

	internal static string InterpolationColorsCommon => GetResourceString("InterpolationColorsCommon");

	internal static string InterpolationColorsInvalidColorBlendObject => GetResourceString("InterpolationColorsInvalidColorBlendObject");

	internal static string InterpolationColorsInvalidStartPosition => GetResourceString("InterpolationColorsInvalidStartPosition");

	internal static string InterpolationColorsInvalidEndPosition => GetResourceString("InterpolationColorsInvalidEndPosition");

	internal static string InterpolationColorsLength => GetResourceString("InterpolationColorsLength");

	internal static string InterpolationColorsLengthsDiffer => GetResourceString("InterpolationColorsLengthsDiffer");

	internal static string InvalidArgumentValue => GetResourceString("InvalidArgumentValue");

	internal static string InvalidArgumentValueFontConverter => GetResourceString("InvalidArgumentValueFontConverter");

	internal static string InvalidBoundArgument => GetResourceString("InvalidBoundArgument");

	internal static string InvalidColor => GetResourceString("InvalidColor");

	internal static string InvalidDashPattern => GetResourceString("InvalidDashPattern");

	internal static string InvalidEx2BoundArgument => GetResourceString("InvalidEx2BoundArgument");

	internal static string InvalidGDIHandle => GetResourceString("InvalidGDIHandle");

	internal static string InvalidImage => GetResourceString("InvalidImage");

	internal static string InvalidLowBoundArgumentEx => GetResourceString("InvalidLowBoundArgumentEx");

	internal static string InvalidPermissionState => GetResourceString("InvalidPermissionState");

	internal static string InvalidPictureType => GetResourceString("InvalidPictureType");

	internal static string InvalidPrinterException_InvalidPrinter => GetResourceString("InvalidPrinterException_InvalidPrinter");

	internal static string InvalidPrinterException_NoDefaultPrinter => GetResourceString("InvalidPrinterException_NoDefaultPrinter");

	internal static string InvalidPrinterHandle => GetResourceString("InvalidPrinterHandle");

	internal static string ValidRangeX => GetResourceString("ValidRangeX");

	internal static string ValidRangeY => GetResourceString("ValidRangeY");

	internal static string NativeHandle0 => GetResourceString("NativeHandle0");

	internal static string NoDefaultPrinter => GetResourceString("NoDefaultPrinter");

	internal static string NotImplemented => GetResourceString("NotImplemented");

	internal static string PDOCbeginPrintDescr => GetResourceString("PDOCbeginPrintDescr");

	internal static string PDOCdocumentNameDescr => GetResourceString("PDOCdocumentNameDescr");

	internal static string PDOCdocumentPageSettingsDescr => GetResourceString("PDOCdocumentPageSettingsDescr");

	internal static string PDOCendPrintDescr => GetResourceString("PDOCendPrintDescr");

	internal static string PDOCoriginAtMarginsDescr => GetResourceString("PDOCoriginAtMarginsDescr");

	internal static string PDOCprintControllerDescr => GetResourceString("PDOCprintControllerDescr");

	internal static string PDOCprintPageDescr => GetResourceString("PDOCprintPageDescr");

	internal static string PDOCprinterSettingsDescr => GetResourceString("PDOCprinterSettingsDescr");

	internal static string PDOCqueryPageSettingsDescr => GetResourceString("PDOCqueryPageSettingsDescr");

	internal static string PrintDocumentDesc => GetResourceString("PrintDocumentDesc");

	internal static string PropertyValueInvalidEntry => GetResourceString("PropertyValueInvalidEntry");

	internal static string PSizeNotCustom => GetResourceString("PSizeNotCustom");

	internal static string ResourceNotFound => GetResourceString("ResourceNotFound");

	internal static string TextParseFailedFormat => GetResourceString("TextParseFailedFormat");

	internal static string TriStateCompareError => GetResourceString("TriStateCompareError");

	internal static string toStringIcon => GetResourceString("toStringIcon");

	internal static string toStringNone => GetResourceString("toStringNone");

	internal static string DCTypeInvalid => GetResourceString("DCTypeInvalid");

	internal static string InvalidEnumArgument => GetResourceString("InvalidEnumArgument");

	internal static string ConvertInvalidPrimitive => GetResourceString("ConvertInvalidPrimitive");

	internal static string BlendObjectMustHaveTwoElements => GetResourceString("BlendObjectMustHaveTwoElements");

	internal static string BlendObjectFirstElementInvalid => GetResourceString("BlendObjectFirstElementInvalid");

	internal static string BlendObjectLastElementInvalid => GetResourceString("BlendObjectLastElementInvalid");

	internal static string AvailableOnlyOnWMF => GetResourceString("AvailableOnlyOnWMF");

	internal static string CannotCreateGraphics => GetResourceString("CannotCreateGraphics");

	internal static string CouldNotOpenDisplay => GetResourceString("CouldNotOpenDisplay");

	internal static string CouldntFindSpecifiedFile => GetResourceString("CouldntFindSpecifiedFile");

	internal static string IconInstanceWasDisposed => GetResourceString("IconInstanceWasDisposed");

	internal static string InvalidGraphicsUnit => GetResourceString("InvalidGraphicsUnit");

	internal static string InvalidThumbnailSize => GetResourceString("InvalidThumbnailSize");

	internal static string NoCodecAvailableForFormat => GetResourceString("NoCodecAvailableForFormat");

	internal static string NotImplementedUnderX11 => GetResourceString("NotImplementedUnderX11");

	internal static string none => GetResourceString("none");

	internal static string NoValidIconImageFound => GetResourceString("NoValidIconImageFound");

	internal static string NullOrEmptyPath => GetResourceString("NullOrEmptyPath");

	internal static string NumberOfPointsAndTypesMustBeSame => GetResourceString("NumberOfPointsAndTypesMustBeSame");

	internal static string ObjectDisposed => GetResourceString("ObjectDisposed");

	internal static string ValueLessThenZero => GetResourceString("ValueLessThenZero");

	internal static string ValueNotOneOfValues => GetResourceString("ValueNotOneOfValues");

	internal static string TargetDirectoryDoesNotExist => GetResourceString("TargetDirectoryDoesNotExist");

	internal static string SystemDrawingCommon_PlatformNotSupported => GetResourceString("SystemDrawingCommon_PlatformNotSupported");

	internal static string IconCouldNotBeExtracted => GetResourceString("IconCouldNotBeExtracted");

	internal static bool UsingResourceKeys()
	{
		return s_usingResourceKeys;
	}

	internal static string GetResourceString(string resourceKey)
	{
		if (UsingResourceKeys())
		{
			return resourceKey;
		}
		string result = null;
		try
		{
			result = ResourceManager.GetString(resourceKey);
		}
		catch (MissingManifestResourceException)
		{
		}
		return result;
	}

	internal static string GetResourceString(string resourceKey, string defaultString)
	{
		string resourceString = GetResourceString(resourceKey);
		if (!(resourceKey == resourceString) && resourceString != null)
		{
			return resourceString;
		}
		return defaultString;
	}

	internal static string Format(string resourceFormat, object? p1)
	{
		if (!UsingResourceKeys())
		{
			return string.Format(resourceFormat, p1);
		}
		return string.Join(", ", resourceFormat, p1);
	}

	internal static string Format(string resourceFormat, object? p1, object? p2)
	{
		if (!UsingResourceKeys())
		{
			return string.Format(resourceFormat, p1, p2);
		}
		return string.Join(", ", resourceFormat, p1, p2);
	}

	internal static string Format(string resourceFormat, object? p1, object? p2, object? p3)
	{
		if (!UsingResourceKeys())
		{
			return string.Format(resourceFormat, p1, p2, p3);
		}
		return string.Join(", ", resourceFormat, p1, p2, p3);
	}

	internal static string Format(string resourceFormat, params object?[]? args)
	{
		if (args == null)
		{
			return resourceFormat;
		}
		if (!UsingResourceKeys())
		{
			return string.Format(resourceFormat, args);
		}
		return resourceFormat + ", " + string.Join(", ", args);
	}

	internal static string Format(IFormatProvider? provider, string resourceFormat, object? p1)
	{
		if (!UsingResourceKeys())
		{
			return string.Format(provider, resourceFormat, p1);
		}
		return string.Join(", ", resourceFormat, p1);
	}

	internal static string Format(IFormatProvider? provider, string resourceFormat, object? p1, object? p2)
	{
		if (!UsingResourceKeys())
		{
			return string.Format(provider, resourceFormat, p1, p2);
		}
		return string.Join(", ", resourceFormat, p1, p2);
	}

	internal static string Format(IFormatProvider? provider, string resourceFormat, object? p1, object? p2, object? p3)
	{
		if (!UsingResourceKeys())
		{
			return string.Format(provider, resourceFormat, p1, p2, p3);
		}
		return string.Join(", ", resourceFormat, p1, p2, p3);
	}

	internal static string Format(IFormatProvider? provider, string resourceFormat, params object?[]? args)
	{
		if (args == null)
		{
			return resourceFormat;
		}
		if (!UsingResourceKeys())
		{
			return string.Format(provider, resourceFormat, args);
		}
		return resourceFormat + ", " + string.Join(", ", args);
	}
}
