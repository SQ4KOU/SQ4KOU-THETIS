using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.DirectWrite;

[Guid("53737037-6d14-410b-9bfe-0b182bb70961")]
public class TextLayout : TextFormat
{
	public float MaxWidth
	{
		get
		{
			return GetMaxWidth();
		}
		set
		{
			SetMaxWidth(value);
		}
	}

	public float MaxHeight
	{
		get
		{
			return GetMaxHeight();
		}
		set
		{
			SetMaxHeight(value);
		}
	}

	public TextMetrics Metrics
	{
		get
		{
			GetMetrics(out var textMetrics);
			return textMetrics;
		}
	}

	public OverhangMetrics OverhangMetrics
	{
		get
		{
			GetOverhangMetrics(out var overhangs);
			return overhangs;
		}
	}

	public TextLayout(Factory factory, string text, TextFormat textFormat, float maxWidth, float maxHeight)
		: base(IntPtr.Zero)
	{
		factory.CreateTextLayout(text, text.Length, textFormat, maxWidth, maxHeight, this);
	}

	public TextLayout(Factory factory, string text, TextFormat textFormat, float layoutWidth, float layoutHeight, float pixelsPerDip, bool useGdiNatural)
		: this(factory, text, textFormat, layoutWidth, layoutHeight, pixelsPerDip, null, useGdiNatural)
	{
	}

	public TextLayout(Factory factory, string text, TextFormat textFormat, float layoutWidth, float layoutHeight, float pixelsPerDip, RawMatrix3x2? transform, bool useGdiNatural)
		: base(IntPtr.Zero)
	{
		factory.CreateGdiCompatibleTextLayout(text, text.Length, textFormat, layoutWidth, layoutHeight, pixelsPerDip, transform, useGdiNatural, this);
	}

	public void Draw(TextRenderer renderer, float originX, float originY)
	{
		Draw(null, renderer, originX, originY);
	}

	public void Draw(object clientDrawingContext, TextRenderer renderer, float originX, float originY)
	{
		GCHandle value = GCHandle.Alloc(clientDrawingContext);
		try
		{
			Draw(GCHandle.ToIntPtr(value), renderer, originX, originY);
		}
		finally
		{
			if (value.IsAllocated)
			{
				value.Free();
			}
		}
	}

	public ClusterMetrics[] GetClusterMetrics()
	{
		ClusterMetrics[] array = new ClusterMetrics[0];
		int maxClusterCount = 0;
		int actualClusterCount = 0;
		GetClusterMetrics(array, maxClusterCount, out actualClusterCount);
		if (actualClusterCount > 0)
		{
			array = new ClusterMetrics[actualClusterCount];
			GetClusterMetrics(array, actualClusterCount, out actualClusterCount);
		}
		return array;
	}

	public void SetDrawingEffect(ComObject drawingEffect, TextRange textRange)
	{
		IntPtr iUnknownForObject = Utilities.GetIUnknownForObject(drawingEffect);
		SetDrawingEffect(iUnknownForObject, textRange);
		if (iUnknownForObject != IntPtr.Zero)
		{
			Marshal.Release(iUnknownForObject);
		}
	}

	public ComObject GetDrawingEffect(int currentPosition)
	{
		TextRange textRange;
		return GetDrawingEffect(currentPosition, out textRange);
	}

	public ComObject GetDrawingEffect(int currentPosition, out TextRange textRange)
	{
		return (ComObject)Utilities.GetObjectForIUnknown(GetDrawingEffect_(currentPosition, out textRange));
	}

	public FontCollection GetFontCollection(int currentPosition)
	{
		TextRange textRange;
		return GetFontCollection(currentPosition, out textRange);
	}

	public string GetFontFamilyName(int currentPosition)
	{
		TextRange textRange;
		return GetFontFamilyName(currentPosition, out textRange);
	}

	public unsafe string GetFontFamilyName(int currentPosition, out TextRange textRange)
	{
		GetFontFamilyNameLength(currentPosition, out var nameLength, out textRange);
		char* value = stackalloc char[nameLength + 1];
		GetFontFamilyName(currentPosition, new IntPtr(value), nameLength + 1, out textRange);
		return new string(value, 0, nameLength);
	}

	public float GetFontSize(int currentPosition)
	{
		TextRange textRange;
		return GetFontSize(currentPosition, out textRange);
	}

	public FontStretch GetFontStretch(int currentPosition)
	{
		TextRange textRange;
		return GetFontStretch(currentPosition, out textRange);
	}

	public FontStyle GetFontStyle(int currentPosition)
	{
		TextRange textRange;
		return GetFontStyle(currentPosition, out textRange);
	}

	public FontWeight GetFontWeight(int currentPosition)
	{
		TextRange textRange;
		return GetFontWeight(currentPosition, out textRange);
	}

	public InlineObject GetInlineObject(int currentPosition)
	{
		TextRange textRange;
		return GetInlineObject(currentPosition, out textRange);
	}

	public LineMetrics[] GetLineMetrics()
	{
		LineMetrics[] array = new LineMetrics[0];
		int maxLineCount = 0;
		int actualLineCount = 0;
		GetLineMetrics(array, maxLineCount, out actualLineCount);
		if (actualLineCount > 0)
		{
			array = new LineMetrics[actualLineCount];
			GetLineMetrics(array, actualLineCount, out actualLineCount);
		}
		return array;
	}

	public string GetLocaleName(int currentPosition)
	{
		TextRange textRange;
		return GetLocaleName(currentPosition, out textRange);
	}

	public unsafe string GetLocaleName(int currentPosition, out TextRange textRange)
	{
		GetLocaleNameLength(currentPosition, out var nameLength, out textRange);
		char* value = stackalloc char[nameLength + 1];
		GetLocaleName(currentPosition, new IntPtr(value), nameLength + 1, out textRange);
		return new string(value, 0, nameLength);
	}

	public bool HasStrikethrough(int currentPosition)
	{
		TextRange textRange;
		return HasStrikethrough(currentPosition, out textRange);
	}

	public Typography GetTypography(int currentPosition)
	{
		TextRange textRange;
		return GetTypography(currentPosition, out textRange);
	}

	public bool HasUnderline(int currentPosition)
	{
		TextRange textRange;
		return HasUnderline(currentPosition, out textRange);
	}

	public HitTestMetrics[] HitTestTextRange(int textPosition, int textLength, float originX, float originY)
	{
		HitTestMetrics[] array = new HitTestMetrics[0];
		int actualHitTestMetricsCount = 0;
		HitTestTextRange(textPosition, textLength, originX, originY, array, 0, out actualHitTestMetricsCount);
		if (actualHitTestMetricsCount > 0)
		{
			array = new HitTestMetrics[actualHitTestMetricsCount];
			HitTestTextRange(textPosition, textLength, originX, originY, array, actualHitTestMetricsCount, out actualHitTestMetricsCount);
		}
		return array;
	}

	public TextLayout(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator TextLayout(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new TextLayout(nativePtr);
		}
		return null;
	}

	internal unsafe void SetMaxWidth(float maxWidth)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, float, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)28 * (nint)sizeof(void*))))(_nativePointer, maxWidth)).CheckError();
	}

	internal unsafe void SetMaxHeight(float maxHeight)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, float, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)29 * (nint)sizeof(void*))))(_nativePointer, maxHeight)).CheckError();
	}

	public unsafe void SetFontCollection(FontCollection fontCollection, TextRange textRange)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<FontCollection>(fontCollection);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, TextRange, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)30 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, textRange)).CheckError();
	}

	public unsafe void SetFontFamilyName(string fontFamilyName, TextRange textRange)
	{
		Result result;
		fixed (char* ptr = fontFamilyName)
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, TextRange, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)31 * (nint)sizeof(void*))))(_nativePointer, ptr, textRange);
		}
		result.CheckError();
	}

	public unsafe void SetFontWeight(FontWeight fontWeight, TextRange textRange)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, TextRange, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)32 * (nint)sizeof(void*))))(_nativePointer, (int)fontWeight, textRange)).CheckError();
	}

	public unsafe void SetFontStyle(FontStyle fontStyle, TextRange textRange)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, TextRange, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)33 * (nint)sizeof(void*))))(_nativePointer, (int)fontStyle, textRange)).CheckError();
	}

	public unsafe void SetFontStretch(FontStretch fontStretch, TextRange textRange)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, TextRange, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)34 * (nint)sizeof(void*))))(_nativePointer, (int)fontStretch, textRange)).CheckError();
	}

	public unsafe void SetFontSize(float fontSize, TextRange textRange)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, float, TextRange, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)35 * (nint)sizeof(void*))))(_nativePointer, fontSize, textRange)).CheckError();
	}

	public unsafe void SetUnderline(RawBool hasUnderline, TextRange textRange)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, RawBool, TextRange, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)36 * (nint)sizeof(void*))))(_nativePointer, hasUnderline, textRange)).CheckError();
	}

	public unsafe void SetStrikethrough(RawBool hasStrikethrough, TextRange textRange)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, RawBool, TextRange, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)37 * (nint)sizeof(void*))))(_nativePointer, hasStrikethrough, textRange)).CheckError();
	}

	public unsafe void SetDrawingEffect(IntPtr drawingEffect, TextRange textRange)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, TextRange, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)38 * (nint)sizeof(void*))))(_nativePointer, (void*)drawingEffect, textRange)).CheckError();
	}

	public unsafe void SetInlineObject(InlineObject inlineObject, TextRange textRange)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<InlineObject>(inlineObject);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, TextRange, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)39 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, textRange)).CheckError();
	}

	public unsafe void SetTypography(Typography typography, TextRange textRange)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Typography>(typography);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, TextRange, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)40 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, textRange)).CheckError();
	}

	public unsafe void SetLocaleName(string localeName, TextRange textRange)
	{
		Result result;
		fixed (char* ptr = localeName)
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, TextRange, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)41 * (nint)sizeof(void*))))(_nativePointer, ptr, textRange);
		}
		result.CheckError();
	}

	internal unsafe float GetMaxWidth()
	{
		return ((delegate* unmanaged[Stdcall]<void*, float>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)42 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe float GetMaxHeight()
	{
		return ((delegate* unmanaged[Stdcall]<void*, float>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)43 * (nint)sizeof(void*))))(_nativePointer);
	}

	public unsafe FontCollection GetFontCollection(int currentPosition, out TextRange textRange)
	{
		IntPtr zero = IntPtr.Zero;
		textRange = default(TextRange);
		Result result;
		fixed (TextRange* ptr = &textRange)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)44 * (nint)sizeof(void*))))(_nativePointer, currentPosition, &zero, ptr2);
		}
		FontCollection result2 = ((!(zero != IntPtr.Zero)) ? null : new FontCollection(zero));
		result.CheckError();
		return result2;
	}

	internal unsafe void GetFontFamilyNameLength(int currentPosition, out int nameLength, out TextRange textRange)
	{
		textRange = default(TextRange);
		Result result;
		fixed (TextRange* ptr = &textRange)
		{
			void* ptr2 = ptr;
			fixed (int* ptr3 = &nameLength)
			{
				void* ptr4 = ptr3;
				result = ((delegate* unmanaged[Stdcall]<void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)45 * (nint)sizeof(void*))))(_nativePointer, currentPosition, ptr4, ptr2);
			}
		}
		result.CheckError();
	}

	internal unsafe void GetFontFamilyName(int currentPosition, IntPtr fontFamilyName, int nameSize, out TextRange textRange)
	{
		textRange = default(TextRange);
		Result result;
		fixed (TextRange* ptr = &textRange)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, int, void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)46 * (nint)sizeof(void*))))(_nativePointer, currentPosition, (void*)fontFamilyName, nameSize, ptr2);
		}
		result.CheckError();
	}

	public unsafe FontWeight GetFontWeight(int currentPosition, out TextRange textRange)
	{
		textRange = default(TextRange);
		Result result;
		FontWeight result2 = default(FontWeight);
		fixed (TextRange* ptr = &textRange)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)47 * (nint)sizeof(void*))))(_nativePointer, currentPosition, &result2, ptr2);
		}
		result.CheckError();
		return result2;
	}

	public unsafe FontStyle GetFontStyle(int currentPosition, out TextRange textRange)
	{
		textRange = default(TextRange);
		Result result;
		FontStyle result2 = default(FontStyle);
		fixed (TextRange* ptr = &textRange)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)48 * (nint)sizeof(void*))))(_nativePointer, currentPosition, &result2, ptr2);
		}
		result.CheckError();
		return result2;
	}

	public unsafe FontStretch GetFontStretch(int currentPosition, out TextRange textRange)
	{
		textRange = default(TextRange);
		Result result;
		FontStretch result2 = default(FontStretch);
		fixed (TextRange* ptr = &textRange)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)49 * (nint)sizeof(void*))))(_nativePointer, currentPosition, &result2, ptr2);
		}
		result.CheckError();
		return result2;
	}

	public unsafe float GetFontSize(int currentPosition, out TextRange textRange)
	{
		textRange = default(TextRange);
		Result result;
		float result2 = default(float);
		fixed (TextRange* ptr = &textRange)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)50 * (nint)sizeof(void*))))(_nativePointer, currentPosition, &result2, ptr2);
		}
		result.CheckError();
		return result2;
	}

	public unsafe RawBool HasUnderline(int currentPosition, out TextRange textRange)
	{
		textRange = default(TextRange);
		Result result;
		RawBool result2 = default(RawBool);
		fixed (TextRange* ptr = &textRange)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)51 * (nint)sizeof(void*))))(_nativePointer, currentPosition, &result2, ptr2);
		}
		result.CheckError();
		return result2;
	}

	public unsafe RawBool HasStrikethrough(int currentPosition, out TextRange textRange)
	{
		textRange = default(TextRange);
		Result result;
		RawBool result2 = default(RawBool);
		fixed (TextRange* ptr = &textRange)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)52 * (nint)sizeof(void*))))(_nativePointer, currentPosition, &result2, ptr2);
		}
		result.CheckError();
		return result2;
	}

	internal unsafe IntPtr GetDrawingEffect_(int currentPosition, out TextRange textRange)
	{
		textRange = default(TextRange);
		Result result;
		IntPtr result2 = default(IntPtr);
		fixed (TextRange* ptr = &textRange)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)53 * (nint)sizeof(void*))))(_nativePointer, currentPosition, &result2, ptr2);
		}
		result.CheckError();
		return result2;
	}

	public unsafe InlineObject GetInlineObject(int currentPosition, out TextRange textRange)
	{
		IntPtr zero = IntPtr.Zero;
		textRange = default(TextRange);
		Result result;
		fixed (TextRange* ptr = &textRange)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)54 * (nint)sizeof(void*))))(_nativePointer, currentPosition, &zero, ptr2);
		}
		InlineObject result2 = ((!(zero != IntPtr.Zero)) ? null : new InlineObjectNative(zero));
		result.CheckError();
		return result2;
	}

	public unsafe Typography GetTypography(int currentPosition, out TextRange textRange)
	{
		IntPtr zero = IntPtr.Zero;
		textRange = default(TextRange);
		Result result;
		fixed (TextRange* ptr = &textRange)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)55 * (nint)sizeof(void*))))(_nativePointer, currentPosition, &zero, ptr2);
		}
		Typography result2 = ((!(zero != IntPtr.Zero)) ? null : new Typography(zero));
		result.CheckError();
		return result2;
	}

	internal unsafe void GetLocaleNameLength(int currentPosition, out int nameLength, out TextRange textRange)
	{
		textRange = default(TextRange);
		Result result;
		fixed (TextRange* ptr = &textRange)
		{
			void* ptr2 = ptr;
			fixed (int* ptr3 = &nameLength)
			{
				void* ptr4 = ptr3;
				result = ((delegate* unmanaged[Stdcall]<void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)56 * (nint)sizeof(void*))))(_nativePointer, currentPosition, ptr4, ptr2);
			}
		}
		result.CheckError();
	}

	internal unsafe void GetLocaleName(int currentPosition, IntPtr localeName, int nameSize, out TextRange textRange)
	{
		textRange = default(TextRange);
		Result result;
		fixed (TextRange* ptr = &textRange)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, int, void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)57 * (nint)sizeof(void*))))(_nativePointer, currentPosition, (void*)localeName, nameSize, ptr2);
		}
		result.CheckError();
	}

	public unsafe void Draw(IntPtr clientDrawingContext, TextRenderer renderer, float originX, float originY)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<TextRenderer>(renderer);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, void*, float, float, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)58 * (nint)sizeof(void*))))(_nativePointer, (void*)clientDrawingContext, (void*)zero, originX, originY)).CheckError();
	}

	internal unsafe Result GetLineMetrics(LineMetrics[] lineMetrics, int maxLineCount, out int actualLineCount)
	{
		Result result;
		fixed (int* ptr = &actualLineCount)
		{
			void* ptr2 = ptr;
			fixed (LineMetrics* ptr3 = lineMetrics)
			{
				void* ptr4 = ptr3;
				result = ((delegate* unmanaged[Stdcall]<void*, void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)59 * (nint)sizeof(void*))))(_nativePointer, ptr4, maxLineCount, ptr2);
			}
		}
		return result;
	}

	internal unsafe void GetMetrics(out TextMetrics textMetrics)
	{
		textMetrics = default(TextMetrics);
		Result result;
		fixed (TextMetrics* ptr = &textMetrics)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)60 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
		result.CheckError();
	}

	internal unsafe void GetOverhangMetrics(out OverhangMetrics overhangs)
	{
		overhangs = default(OverhangMetrics);
		Result result;
		fixed (OverhangMetrics* ptr = &overhangs)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)61 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
		result.CheckError();
	}

	internal unsafe Result GetClusterMetrics(ClusterMetrics[] clusterMetrics, int maxClusterCount, out int actualClusterCount)
	{
		Result result;
		fixed (int* ptr = &actualClusterCount)
		{
			void* ptr2 = ptr;
			fixed (ClusterMetrics* ptr3 = clusterMetrics)
			{
				void* ptr4 = ptr3;
				result = ((delegate* unmanaged[Stdcall]<void*, void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)62 * (nint)sizeof(void*))))(_nativePointer, ptr4, maxClusterCount, ptr2);
			}
		}
		return result;
	}

	public unsafe float DetermineMinWidth()
	{
		float result = default(float);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)63 * (nint)sizeof(void*))))(_nativePointer, &result)).CheckError();
		return result;
	}

	public unsafe HitTestMetrics HitTestPoint(float pointX, float pointY, out RawBool isTrailingHit, out RawBool isInside)
	{
		isTrailingHit = default(RawBool);
		isInside = default(RawBool);
		Result result;
		HitTestMetrics result2 = default(HitTestMetrics);
		fixed (RawBool* ptr = &isInside)
		{
			void* ptr2 = ptr;
			fixed (RawBool* ptr3 = &isTrailingHit)
			{
				void* ptr4 = ptr3;
				result = ((delegate* unmanaged[Stdcall]<void*, float, float, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)64 * (nint)sizeof(void*))))(_nativePointer, pointX, pointY, ptr4, ptr2, &result2);
			}
		}
		result.CheckError();
		return result2;
	}

	public unsafe HitTestMetrics HitTestTextPosition(int textPosition, RawBool isTrailingHit, out float ointXRef, out float ointYRef)
	{
		Result result;
		HitTestMetrics result2 = default(HitTestMetrics);
		fixed (float* ptr = &ointYRef)
		{
			void* ptr2 = ptr;
			fixed (float* ptr3 = &ointXRef)
			{
				void* ptr4 = ptr3;
				result = ((delegate* unmanaged[Stdcall]<void*, int, RawBool, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)65 * (nint)sizeof(void*))))(_nativePointer, textPosition, isTrailingHit, ptr4, ptr2, &result2);
			}
		}
		result.CheckError();
		return result2;
	}

	internal unsafe Result HitTestTextRange(int textPosition, int textLength, float originX, float originY, HitTestMetrics[] hitTestMetrics, int maxHitTestMetricsCount, out int actualHitTestMetricsCount)
	{
		Result result;
		fixed (int* ptr = &actualHitTestMetricsCount)
		{
			void* ptr2 = ptr;
			fixed (HitTestMetrics* ptr3 = hitTestMetrics)
			{
				void* ptr4 = ptr3;
				result = ((delegate* unmanaged[Stdcall]<void*, int, int, float, float, void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)66 * (nint)sizeof(void*))))(_nativePointer, textPosition, textLength, originX, originY, ptr4, maxHitTestMetricsCount, ptr2);
			}
		}
		return result;
	}
}
