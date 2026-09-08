using System;
using System.Runtime.InteropServices;
using SharpDX.DirectWrite;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct2D1;

internal class CommandSinkShadow : ComObjectShadow
{
	public class CommandSinkVtbl : ComObjectVtbl
	{
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate int CallNoParams(IntPtr thisPtr);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate int SetAntialiasModeDelegate(IntPtr thisPtr, AntialiasMode antialiasMode);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate int SetTagsDelegate(IntPtr thisPtr, long tag1, long tag2);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate int SetTextAntialiasModeDelegate(IntPtr thisPtr, TextAntialiasMode textAntialiasMode);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate int SetTextRenderingParamsDelegate(IntPtr thisPtr, IntPtr textRenderingParams);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate int SetTransformDelegate(IntPtr thisPtr, IntPtr transform);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate int SetPrimitiveBlendDelegate(IntPtr thisPtr, PrimitiveBlend primitiveBlend);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate int SetUnitModeDelegate(IntPtr thisPtr, UnitMode unitMode);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate int ClearDelegate(IntPtr thisPtr, IntPtr color);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate int DrawGlyphRunDelegate(IntPtr thisPtr, RawVector2 baselineOrigin, IntPtr glyphRun, IntPtr glyphRunDescriptionPtr, IntPtr foregroundBrush, MeasuringMode measuringMode);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate int DrawLineDelegate(IntPtr thisPtr, RawVector2 point0, RawVector2 point1, IntPtr brush, float strokeWidth, IntPtr strokeStyle);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate int DrawGeometryDelegate(IntPtr thisPtr, IntPtr geometry, IntPtr brush, float strokeWidth, IntPtr strokeStyle);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate int DrawRectangleDelegate(IntPtr thisPtr, IntPtr rect, IntPtr brush, float strokeWidth, IntPtr strokeStyle);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate int DrawBitmapDelegate(IntPtr thisPtr, IntPtr bitmap, IntPtr destinationRectangle, float opacity, InterpolationMode interpolationMode, IntPtr sourceRectangle, IntPtr erspectiveTransformRef);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate int DrawImageDelegate(IntPtr thisPtr, IntPtr image, IntPtr targetOffset, IntPtr imageRectangle, InterpolationMode interpolationMode, CompositeMode compositeMode);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate int DrawGdiMetafileDelegate(IntPtr thisPtr, IntPtr gdiMetafile, IntPtr targetOffset);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate int FillMeshDelegate(IntPtr thisPtr, IntPtr mesh, IntPtr brush);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate int FillOpacityMaskDelegate(IntPtr thisPtr, IntPtr opacityMask, IntPtr brush, IntPtr destinationRectangle, IntPtr sourceRectangle);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate int FillGeometryDelegate(IntPtr thisPtr, IntPtr geometry, IntPtr brush, IntPtr opacityBrush);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate int FillRectangleDelegate(IntPtr thisPtr, IntPtr rect, IntPtr brush);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate int PushAxisAlignedClipDelegate(IntPtr thisPtr, IntPtr clipRect, AntialiasMode antialiasMode);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate int PushLayerDelegate(IntPtr thisPtr, IntPtr layerParameters1, IntPtr layer);

		public CommandSinkVtbl()
			: this(0)
		{
		}

		public CommandSinkVtbl(int numMethods)
			: base(numMethods + 28)
		{
			AddMethod(new CallNoParams(BeginDrawImpl));
			AddMethod(new CallNoParams(EndDrawImpl));
			AddMethod(new SetAntialiasModeDelegate(SetAntialiasModeImpl));
			AddMethod(new SetTagsDelegate(SetTagsImpl));
			AddMethod(new SetTextAntialiasModeDelegate(SetTextAntialiasModeImpl));
			AddMethod(new SetTextRenderingParamsDelegate(SetTextRenderingParamsImpl));
			AddMethod(new SetTransformDelegate(SetTransformImpl));
			AddMethod(new SetPrimitiveBlendDelegate(SetPrimitiveBlendImpl));
			AddMethod(new SetUnitModeDelegate(SetUnitModeImpl));
			AddMethod(new ClearDelegate(ClearImpl));
			AddMethod(new DrawGlyphRunDelegate(DrawGlyphRunImpl));
			AddMethod(new DrawLineDelegate(DrawLineImpl));
			AddMethod(new DrawGeometryDelegate(DrawGeometryImpl));
			AddMethod(new DrawRectangleDelegate(DrawRectangleImpl));
			AddMethod(new DrawBitmapDelegate(DrawBitmapImpl));
			AddMethod(new DrawImageDelegate(DrawImageImpl));
			AddMethod(new DrawGdiMetafileDelegate(DrawGdiMetafileImpl));
			AddMethod(new FillMeshDelegate(FillMeshImpl));
			AddMethod(new FillOpacityMaskDelegate(FillOpacityMaskImpl));
			AddMethod(new FillGeometryDelegate(FillGeometryImpl));
			AddMethod(new FillRectangleDelegate(FillRectangleImpl));
			AddMethod(new PushAxisAlignedClipDelegate(PushAxisAlignedClipImpl));
			AddMethod(new PushLayerDelegate(PushLayerImpl));
			AddMethod(new CallNoParams(PopAxisAlignedClipImpl));
			AddMethod(new CallNoParams(PopLayerImpl));
		}

		private static int BeginDrawImpl(IntPtr thisPtr)
		{
			try
			{
				((CommandSink)CppObjectShadow.ToShadow<CommandSinkShadow>(thisPtr).Callback).BeginDraw();
			}
			catch (Exception ex)
			{
				return (int)Result.GetResultFromException(ex);
			}
			return Result.Ok.Code;
		}

		private static int EndDrawImpl(IntPtr thisPtr)
		{
			try
			{
				((CommandSink)CppObjectShadow.ToShadow<CommandSinkShadow>(thisPtr).Callback).EndDraw();
			}
			catch (Exception ex)
			{
				return (int)Result.GetResultFromException(ex);
			}
			return Result.Ok.Code;
		}

		private static int SetAntialiasModeImpl(IntPtr thisPtr, AntialiasMode antialiasMode)
		{
			try
			{
				((CommandSink)CppObjectShadow.ToShadow<CommandSinkShadow>(thisPtr).Callback).AntialiasMode = antialiasMode;
			}
			catch (Exception ex)
			{
				return (int)Result.GetResultFromException(ex);
			}
			return Result.Ok.Code;
		}

		private static int SetTagsImpl(IntPtr thisPtr, long tag1, long tag2)
		{
			try
			{
				((CommandSink)CppObjectShadow.ToShadow<CommandSinkShadow>(thisPtr).Callback).SetTags(tag1, tag2);
			}
			catch (Exception ex)
			{
				return (int)Result.GetResultFromException(ex);
			}
			return Result.Ok.Code;
		}

		private static int SetTextAntialiasModeImpl(IntPtr thisPtr, TextAntialiasMode textAntialiasMode)
		{
			try
			{
				((CommandSink)CppObjectShadow.ToShadow<CommandSinkShadow>(thisPtr).Callback).TextAntialiasMode = textAntialiasMode;
			}
			catch (Exception ex)
			{
				return (int)Result.GetResultFromException(ex);
			}
			return Result.Ok.Code;
		}

		private static int SetTextRenderingParamsImpl(IntPtr thisPtr, IntPtr textRenderingParams)
		{
			try
			{
				((CommandSink)CppObjectShadow.ToShadow<CommandSinkShadow>(thisPtr).Callback).TextRenderingParams = new RenderingParams(textRenderingParams);
			}
			catch (Exception ex)
			{
				return (int)Result.GetResultFromException(ex);
			}
			return Result.Ok.Code;
		}

		private unsafe static int SetTransformImpl(IntPtr thisPtr, IntPtr transform)
		{
			try
			{
				((CommandSink)CppObjectShadow.ToShadow<CommandSinkShadow>(thisPtr).Callback).Transform = *(RawMatrix3x2*)(void*)transform;
			}
			catch (Exception ex)
			{
				return (int)Result.GetResultFromException(ex);
			}
			return Result.Ok.Code;
		}

		private static int SetPrimitiveBlendImpl(IntPtr thisPtr, PrimitiveBlend primitiveBlend)
		{
			try
			{
				((CommandSink)CppObjectShadow.ToShadow<CommandSinkShadow>(thisPtr).Callback).PrimitiveBlend = primitiveBlend;
			}
			catch (Exception ex)
			{
				return (int)Result.GetResultFromException(ex);
			}
			return Result.Ok.Code;
		}

		private static int SetUnitModeImpl(IntPtr thisPtr, UnitMode unitMode)
		{
			try
			{
				((CommandSink)CppObjectShadow.ToShadow<CommandSinkShadow>(thisPtr).Callback).UnitMode = unitMode;
			}
			catch (Exception ex)
			{
				return (int)Result.GetResultFromException(ex);
			}
			return Result.Ok.Code;
		}

		private unsafe static int ClearImpl(IntPtr thisPtr, IntPtr color)
		{
			try
			{
				((CommandSink)CppObjectShadow.ToShadow<CommandSinkShadow>(thisPtr).Callback).Clear((color == IntPtr.Zero) ? ((RawColor4?)null) : new RawColor4?(*(RawColor4*)(void*)color));
			}
			catch (Exception ex)
			{
				return (int)Result.GetResultFromException(ex);
			}
			return Result.Ok.Code;
		}

		private unsafe static int DrawGlyphRunImpl(IntPtr thisPtr, RawVector2 baselineOrigin, IntPtr glyphRunNative, IntPtr glyphRunDescriptionPtr, IntPtr foregroundBrush, MeasuringMode measuringMode)
		{
			GlyphRun glyphRun = new GlyphRun();
			try
			{
				GlyphRunDescription glyphRunDescription = new GlyphRunDescription();
				glyphRunDescription.__MarshalFrom(ref *(GlyphRunDescription.__Native*)(void*)glyphRunDescriptionPtr);
				glyphRun.__MarshalFrom(ref *(GlyphRun.__Native*)(void*)glyphRunNative);
				((CommandSink)CppObjectShadow.ToShadow<CommandSinkShadow>(thisPtr).Callback).DrawGlyphRun(baselineOrigin, glyphRun, glyphRunDescription, new Brush(foregroundBrush), measuringMode);
			}
			catch (Exception ex)
			{
				return (int)Result.GetResultFromException(ex);
			}
			finally
			{
				glyphRun.__MarshalFree(ref *(GlyphRun.__Native*)(void*)glyphRunNative);
			}
			return Result.Ok.Code;
		}

		private static int DrawLineImpl(IntPtr thisPtr, RawVector2 point0, RawVector2 point1, IntPtr brush, float strokeWidth, IntPtr strokeStyle)
		{
			try
			{
				((CommandSink)CppObjectShadow.ToShadow<CommandSinkShadow>(thisPtr).Callback).DrawLine(point0, point1, new Brush(brush), strokeWidth, new StrokeStyle(strokeStyle));
			}
			catch (Exception ex)
			{
				return (int)Result.GetResultFromException(ex);
			}
			return Result.Ok.Code;
		}

		private static int DrawGeometryImpl(IntPtr thisPtr, IntPtr geometry, IntPtr brush, float strokeWidth, IntPtr strokeStyle)
		{
			try
			{
				((CommandSink)CppObjectShadow.ToShadow<CommandSinkShadow>(thisPtr).Callback).DrawGeometry(new Geometry(geometry), new Brush(brush), strokeWidth, new StrokeStyle(strokeStyle));
			}
			catch (Exception ex)
			{
				return (int)Result.GetResultFromException(ex);
			}
			return Result.Ok.Code;
		}

		private unsafe static int DrawRectangleImpl(IntPtr thisPtr, IntPtr rect, IntPtr brush, float strokeWidth, IntPtr strokeStyle)
		{
			try
			{
				((CommandSink)CppObjectShadow.ToShadow<CommandSinkShadow>(thisPtr).Callback).DrawRectangle(*(RawRectangleF*)(void*)rect, new Brush(brush), strokeWidth, new StrokeStyle(strokeStyle));
			}
			catch (Exception ex)
			{
				return (int)Result.GetResultFromException(ex);
			}
			return Result.Ok.Code;
		}

		private unsafe static int DrawBitmapImpl(IntPtr thisPtr, IntPtr bitmap, IntPtr destinationRectangle, float opacity, InterpolationMode interpolationMode, IntPtr sourceRectangle, IntPtr erspectiveTransformRef)
		{
			try
			{
				((CommandSink)CppObjectShadow.ToShadow<CommandSinkShadow>(thisPtr).Callback).DrawBitmap(new Bitmap(bitmap), (destinationRectangle == IntPtr.Zero) ? ((RawRectangleF?)null) : new RawRectangleF?(*(RawRectangleF*)(void*)destinationRectangle), opacity, interpolationMode, (sourceRectangle == IntPtr.Zero) ? ((RawRectangleF?)null) : new RawRectangleF?(*(RawRectangleF*)(void*)sourceRectangle), (erspectiveTransformRef == IntPtr.Zero) ? ((RawMatrix?)null) : new RawMatrix?(*(RawMatrix*)(void*)erspectiveTransformRef));
			}
			catch (Exception ex)
			{
				return (int)Result.GetResultFromException(ex);
			}
			return Result.Ok.Code;
		}

		private unsafe static int DrawImageImpl(IntPtr thisPtr, IntPtr image, IntPtr targetOffset, IntPtr imageRectangle, InterpolationMode interpolationMode, CompositeMode compositeMode)
		{
			try
			{
				((CommandSink)CppObjectShadow.ToShadow<CommandSinkShadow>(thisPtr).Callback).DrawImage(new Image(image), (targetOffset == IntPtr.Zero) ? ((RawVector2?)null) : new RawVector2?(*(RawVector2*)(void*)targetOffset), (imageRectangle == IntPtr.Zero) ? ((RawRectangleF?)null) : new RawRectangleF?(*(RawRectangleF*)(void*)imageRectangle), interpolationMode, compositeMode);
			}
			catch (Exception ex)
			{
				return (int)Result.GetResultFromException(ex);
			}
			return Result.Ok.Code;
		}

		private unsafe static int DrawGdiMetafileImpl(IntPtr thisPtr, IntPtr gdiMetafile, IntPtr targetOffset)
		{
			try
			{
				((CommandSink)CppObjectShadow.ToShadow<CommandSinkShadow>(thisPtr).Callback).DrawGdiMetafile(new GdiMetafile(gdiMetafile), (targetOffset == IntPtr.Zero) ? ((RawVector2?)null) : new RawVector2?(*(RawVector2*)(void*)targetOffset));
			}
			catch (Exception ex)
			{
				return (int)Result.GetResultFromException(ex);
			}
			return Result.Ok.Code;
		}

		private static int FillMeshImpl(IntPtr thisPtr, IntPtr mesh, IntPtr brush)
		{
			try
			{
				((CommandSink)CppObjectShadow.ToShadow<CommandSinkShadow>(thisPtr).Callback).FillMesh(new Mesh(mesh), new Brush(brush));
			}
			catch (Exception ex)
			{
				return (int)Result.GetResultFromException(ex);
			}
			return Result.Ok.Code;
		}

		private unsafe static int FillOpacityMaskImpl(IntPtr thisPtr, IntPtr opacityMask, IntPtr brush, IntPtr destinationRectangle, IntPtr sourceRectangle)
		{
			try
			{
				((CommandSink)CppObjectShadow.ToShadow<CommandSinkShadow>(thisPtr).Callback).FillOpacityMask(new Bitmap(opacityMask), new Brush(brush), (destinationRectangle == IntPtr.Zero) ? ((RawRectangleF?)null) : new RawRectangleF?(*(RawRectangleF*)(void*)destinationRectangle), (sourceRectangle == IntPtr.Zero) ? ((RawRectangleF?)null) : new RawRectangleF?(*(RawRectangleF*)(void*)sourceRectangle));
			}
			catch (Exception ex)
			{
				return (int)Result.GetResultFromException(ex);
			}
			return Result.Ok.Code;
		}

		private static int FillGeometryImpl(IntPtr thisPtr, IntPtr geometry, IntPtr brush, IntPtr opacityBrush)
		{
			try
			{
				((CommandSink)CppObjectShadow.ToShadow<CommandSinkShadow>(thisPtr).Callback).FillGeometry(new Geometry(geometry), new Brush(brush), new Brush(opacityBrush));
			}
			catch (Exception ex)
			{
				return (int)Result.GetResultFromException(ex);
			}
			return Result.Ok.Code;
		}

		private unsafe static int FillRectangleImpl(IntPtr thisPtr, IntPtr rect, IntPtr brush)
		{
			try
			{
				((CommandSink)CppObjectShadow.ToShadow<CommandSinkShadow>(thisPtr).Callback).FillRectangle(*(RawRectangleF*)(void*)rect, new Brush(brush));
			}
			catch (Exception ex)
			{
				return (int)Result.GetResultFromException(ex);
			}
			return Result.Ok.Code;
		}

		private unsafe static int PushAxisAlignedClipImpl(IntPtr thisPtr, IntPtr clipRect, AntialiasMode antialiasMode)
		{
			try
			{
				((CommandSink)CppObjectShadow.ToShadow<CommandSinkShadow>(thisPtr).Callback).PushAxisAlignedClip(*(RawRectangleF*)(void*)clipRect, antialiasMode);
			}
			catch (Exception ex)
			{
				return (int)Result.GetResultFromException(ex);
			}
			return Result.Ok.Code;
		}

		private unsafe static int PushLayerImpl(IntPtr thisPtr, IntPtr layerParameters1, IntPtr layer)
		{
			try
			{
				CommandSink obj = (CommandSink)CppObjectShadow.ToShadow<CommandSinkShadow>(thisPtr).Callback;
				LayerParameters1 layerParameters2 = default(LayerParameters1);
				layerParameters2.__MarshalFrom(ref *(LayerParameters1.__Native*)(void*)layerParameters1);
				obj.PushLayer(ref layerParameters2, (layer == IntPtr.Zero) ? null : new Layer(layer));
			}
			catch (Exception ex)
			{
				return (int)Result.GetResultFromException(ex);
			}
			return Result.Ok.Code;
		}

		private static int PopAxisAlignedClipImpl(IntPtr thisPtr)
		{
			try
			{
				((CommandSink)CppObjectShadow.ToShadow<CommandSinkShadow>(thisPtr).Callback).PopAxisAlignedClip();
			}
			catch (Exception ex)
			{
				return (int)Result.GetResultFromException(ex);
			}
			return Result.Ok.Code;
		}

		private static int PopLayerImpl(IntPtr thisPtr)
		{
			try
			{
				((CommandSink)CppObjectShadow.ToShadow<CommandSinkShadow>(thisPtr).Callback).PopLayer();
			}
			catch (Exception ex)
			{
				return (int)Result.GetResultFromException(ex);
			}
			return Result.Ok.Code;
		}
	}

	private static readonly CommandSinkVtbl Vtbl = new CommandSinkVtbl();

	protected override CppObjectVtbl GetVtbl => Vtbl;

	public static IntPtr ToIntPtr(CommandSink callback)
	{
		return CppObject.ToCallbackPtr<CommandSink>(callback);
	}
}
