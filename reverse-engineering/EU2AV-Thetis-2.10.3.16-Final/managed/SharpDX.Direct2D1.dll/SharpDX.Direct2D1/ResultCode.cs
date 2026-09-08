namespace SharpDX.Direct2D1;

public class ResultCode
{
	public static readonly ResultDescriptor WrongState = new ResultDescriptor(-2003238911, "SharpDX.Direct2D1", "D2DERR_WRONG_STATE", "WrongState");

	public static readonly ResultDescriptor NotInitializeD = new ResultDescriptor(-2003238910, "SharpDX.Direct2D1", "D2DERR_NOT_INITIALIZED", "NotInitializeD");

	public static readonly ResultDescriptor UnsupportedOperation = new ResultDescriptor(-2003238909, "SharpDX.Direct2D1", "D2DERR_UNSUPPORTED_OPERATION", "UnsupportedOperation");

	public static readonly ResultDescriptor ScannerFailed = new ResultDescriptor(-2003238908, "SharpDX.Direct2D1", "D2DERR_SCANNER_FAILED", "ScannerFailed");

	public static readonly ResultDescriptor ScreenAccessDenied = new ResultDescriptor(-2003238907, "SharpDX.Direct2D1", "D2DERR_SCREEN_ACCESS_DENIED", "ScreenAccessDenied");

	public static readonly ResultDescriptor DisplayStateInvalid = new ResultDescriptor(-2003238906, "SharpDX.Direct2D1", "D2DERR_DISPLAY_STATE_INVALID", "DisplayStateInvalid");

	public static readonly ResultDescriptor ZeroVector = new ResultDescriptor(-2003238905, "SharpDX.Direct2D1", "D2DERR_ZERO_VECTOR", "ZeroVector");

	public static readonly ResultDescriptor InternalError = new ResultDescriptor(-2003238904, "SharpDX.Direct2D1", "D2DERR_INTERNAL_ERROR", "InternalError");

	public static readonly ResultDescriptor DisplayFormatNotSupported = new ResultDescriptor(-2003238903, "SharpDX.Direct2D1", "D2DERR_DISPLAY_FORMAT_NOT_SUPPORTED", "DisplayFormatNotSupported");

	public static readonly ResultDescriptor InvalidCall = new ResultDescriptor(-2003238902, "SharpDX.Direct2D1", "D2DERR_INVALID_CALL", "InvalidCall");

	public static readonly ResultDescriptor NoHardwareDevice = new ResultDescriptor(-2003238901, "SharpDX.Direct2D1", "D2DERR_NO_HARDWARE_DEVICE", "NoHardwareDevice");

	public static readonly ResultDescriptor RecreateTarget = new ResultDescriptor(-2003238900, "SharpDX.Direct2D1", "D2DERR_RECREATE_TARGET", "RecreateTarget");

	public static readonly ResultDescriptor TooManyShaderElements = new ResultDescriptor(-2003238899, "SharpDX.Direct2D1", "D2DERR_TOO_MANY_SHADER_ELEMENTS", "TooManyShaderElements");

	public static readonly ResultDescriptor ShaderCompileFailed = new ResultDescriptor(-2003238898, "SharpDX.Direct2D1", "D2DERR_SHADER_COMPILE_FAILED", "ShaderCompileFailed");

	public static readonly ResultDescriptor MaximumTextureSizeExceeded = new ResultDescriptor(-2003238897, "SharpDX.Direct2D1", "D2DERR_MAX_TEXTURE_SIZE_EXCEEDED", "MaximumTextureSizeExceeded");

	public static readonly ResultDescriptor UnsupportedVersion = new ResultDescriptor(-2003238896, "SharpDX.Direct2D1", "D2DERR_UNSUPPORTED_VERSION", "UnsupportedVersion");

	public static readonly ResultDescriptor BadNumber = new ResultDescriptor(-2003238895, "SharpDX.Direct2D1", "D2DERR_BAD_NUMBER", "BadNumber");

	public static readonly ResultDescriptor WrongFactory = new ResultDescriptor(-2003238894, "SharpDX.Direct2D1", "D2DERR_WRONG_FACTORY", "WrongFactory");

	public static readonly ResultDescriptor LayerAlreadyInUse = new ResultDescriptor(-2003238893, "SharpDX.Direct2D1", "D2DERR_LAYER_ALREADY_IN_USE", "LayerAlreadyInUse");

	public static readonly ResultDescriptor PopCallDidNotMatchPush = new ResultDescriptor(-2003238892, "SharpDX.Direct2D1", "D2DERR_POP_CALL_DID_NOT_MATCH_PUSH", "PopCallDidNotMatchPush");

	public static readonly ResultDescriptor WrongResourceDomain = new ResultDescriptor(-2003238891, "SharpDX.Direct2D1", "D2DERR_WRONG_RESOURCE_DOMAIN", "WrongResourceDomain");

	public static readonly ResultDescriptor PushPopUnbalanced = new ResultDescriptor(-2003238890, "SharpDX.Direct2D1", "D2DERR_PUSH_POP_UNBALANCED", "PushPopUnbalanced");

	public static readonly ResultDescriptor RenderTargetHasLayerOrCliprect = new ResultDescriptor(-2003238889, "SharpDX.Direct2D1", "D2DERR_RENDER_TARGET_HAS_LAYER_OR_CLIPRECT", "RenderTargetHasLayerOrCliprect");

	public static readonly ResultDescriptor IncompatibleBrushTypes = new ResultDescriptor(-2003238888, "SharpDX.Direct2D1", "D2DERR_INCOMPATIBLE_BRUSH_TYPES", "IncompatibleBrushTypes");

	public static readonly ResultDescriptor Win32Error = new ResultDescriptor(-2003238887, "SharpDX.Direct2D1", "D2DERR_WIN32_ERROR", "Win32Error");

	public static readonly ResultDescriptor TargetNotGdiCompatible = new ResultDescriptor(-2003238886, "SharpDX.Direct2D1", "D2DERR_TARGET_NOT_GDI_COMPATIBLE", "TargetNotGdiCompatible");

	public static readonly ResultDescriptor TextEffectIsWrongType = new ResultDescriptor(-2003238885, "SharpDX.Direct2D1", "D2DERR_TEXT_EFFECT_IS_WRONG_TYPE", "TextEffectIsWrongType");

	public static readonly ResultDescriptor TextRendererNotReleased = new ResultDescriptor(-2003238884, "SharpDX.Direct2D1", "D2DERR_TEXT_RENDERER_NOT_RELEASED", "TextRendererNotReleased");

	public static readonly ResultDescriptor ExceedsMaximumBitmapSize = new ResultDescriptor(-2003238883, "SharpDX.Direct2D1", "D2DERR_EXCEEDS_MAX_BITMAP_SIZE", "ExceedsMaximumBitmapSize");

	public static readonly ResultDescriptor InvalidGraphConfiguration = new ResultDescriptor(-2003238882, "SharpDX.Direct2D1", "D2DERR_INVALID_GRAPH_CONFIGURATION", "InvalidGraphConfiguration");

	public static readonly ResultDescriptor InvalidInternalGraphConfiguration = new ResultDescriptor(-2003238881, "SharpDX.Direct2D1", "D2DERR_INVALID_INTERNAL_GRAPH_CONFIGURATION", "InvalidInternalGraphConfiguration");

	public static readonly ResultDescriptor CyclicGraph = new ResultDescriptor(-2003238880, "SharpDX.Direct2D1", "D2DERR_CYCLIC_GRAPH", "CyclicGraph");

	public static readonly ResultDescriptor BitmapCannotDraw = new ResultDescriptor(-2003238879, "SharpDX.Direct2D1", "D2DERR_BITMAP_CANNOT_DRAW", "BitmapCannotDraw");

	public static readonly ResultDescriptor OutstandingBitmapReferences = new ResultDescriptor(-2003238878, "SharpDX.Direct2D1", "D2DERR_OUTSTANDING_BITMAP_REFERENCES", "OutstandingBitmapReferences");

	public static readonly ResultDescriptor OriginalTargetNotBound = new ResultDescriptor(-2003238877, "SharpDX.Direct2D1", "D2DERR_ORIGINAL_TARGET_NOT_BOUND", "OriginalTargetNotBound");

	public static readonly ResultDescriptor InvalidTarget = new ResultDescriptor(-2003238876, "SharpDX.Direct2D1", "D2DERR_INVALID_TARGET", "InvalidTarget");

	public static readonly ResultDescriptor BitmapBoundAsTarget = new ResultDescriptor(-2003238875, "SharpDX.Direct2D1", "D2DERR_BITMAP_BOUND_AS_TARGET", "BitmapBoundAsTarget");

	public static readonly ResultDescriptor InsufficientDeviceCapabilities = new ResultDescriptor(-2003238874, "SharpDX.Direct2D1", "D2DERR_INSUFFICIENT_DEVICE_CAPABILITIES", "InsufficientDeviceCapabilities");

	public static readonly ResultDescriptor IntermediateTooLarge = new ResultDescriptor(-2003238873, "SharpDX.Direct2D1", "D2DERR_INTERMEDIATE_TOO_LARGE", "IntermediateTooLarge");

	public static readonly ResultDescriptor EffectIsNotRegistered = new ResultDescriptor(-2003238872, "SharpDX.Direct2D1", "D2DERR_EFFECT_IS_NOT_REGISTERED", "EffectIsNotRegistered");

	public static readonly ResultDescriptor InvalidProperty = new ResultDescriptor(-2003238871, "SharpDX.Direct2D1", "D2DERR_INVALID_PROPERTY", "InvalidProperty");

	public static readonly ResultDescriptor NoSubProperties = new ResultDescriptor(-2003238870, "SharpDX.Direct2D1", "D2DERR_NO_SUBPROPERTIES", "NoSubProperties");

	public static readonly ResultDescriptor PrintJobClosed = new ResultDescriptor(-2003238869, "SharpDX.Direct2D1", "D2DERR_PRINT_JOB_CLOSED", "PrintJobClosed");

	public static readonly ResultDescriptor PrintFormatNotSupported = new ResultDescriptor(-2003238868, "SharpDX.Direct2D1", "D2DERR_PRINT_FORMAT_NOT_SUPPORTED", "PrintFormatNotSupported");

	public static readonly ResultDescriptor TooManyTransformInputs = new ResultDescriptor(-2003238867, "SharpDX.Direct2D1", "D2DERR_TOO_MANY_TRANSFORM_INPUTS", "TooManyTransformInputs");

	public static readonly ResultDescriptor InvalidGlyphImage = new ResultDescriptor(-2003238866, "SharpDX.Direct2D1", "D2DERR_INVALID_GLYPH_IMAGE", "InvalidGlyphImage");

	public static readonly ResultDescriptor UnsupportedPixelFormat = new ResultDescriptor(-2003292288, "SharpDX.Direct2D1", "D2DERR_UNSUPPORTED_PIXEL_FORMAT", "UnsupportedPixelFormat");

	public static readonly ResultDescriptor InsufficientBuffer = new ResultDescriptor(122, "SharpDX.Direct2D1", "D2DERR_INSUFFICIENT_BUFFER", "InsufficientBuffer");

	public static readonly ResultDescriptor FileNotFound = new ResultDescriptor(2, "SharpDX.Direct2D1", "D2DERR_FILE_NOT_FOUND", "FileNotFound");
}
