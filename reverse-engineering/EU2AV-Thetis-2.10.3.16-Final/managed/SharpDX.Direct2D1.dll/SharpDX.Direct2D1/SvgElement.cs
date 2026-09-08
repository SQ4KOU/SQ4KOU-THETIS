using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct2D1;

[Guid("ac7b67a6-183e-49c1-a823-0ebe40b0db29")]
public class SvgElement : Resource
{
	public SvgElement[] Children
	{
		get
		{
			if (!HasChildren())
			{
				return new SvgElement[0];
			}
			SvgElement referenceChild = FirstChild;
			int num = 0;
			SvgElement nextChild;
			do
			{
				GetNextChild(referenceChild, out nextChild);
				referenceChild = nextChild;
				num++;
			}
			while (nextChild != null);
			SvgElement[] array = new SvgElement[num];
			referenceChild = FirstChild;
			for (int i = 0; i < num; i++)
			{
				array[i] = referenceChild;
				GetNextChild(referenceChild, out referenceChild);
			}
			return array;
		}
	}

	public unsafe string TagName
	{
		get
		{
			int tagNameLength = GetTagNameLength();
			sbyte* ptr = stackalloc sbyte[(int)(uint)tagNameLength];
			GetTagName(new IntPtr(ptr), tagNameLength + 1);
			return Marshal.PtrToStringUni((IntPtr)ptr, tagNameLength);
		}
	}

	public SvgDocument Document
	{
		get
		{
			GetDocument(out var document);
			return document;
		}
	}

	public int TagNameLength => GetTagNameLength();

	public RawBool IsTextContent => IsTextContent_();

	public SvgElement Parent
	{
		get
		{
			GetParent(out var arentRef);
			return arentRef;
		}
	}

	public SvgElement FirstChild
	{
		get
		{
			GetFirstChild(out var child);
			return child;
		}
	}

	public SvgElement LastChild
	{
		get
		{
			GetLastChild(out var child);
			return child;
		}
	}

	public int SpecifiedAttributeCount => GetSpecifiedAttributeCount();

	public int TextValueLength => GetTextValueLength();

	public unsafe void SetAttributeValue(string name, float value)
	{
		SetAttributeValue(name, SvgAttributePodType.Float, new IntPtr(&value), 4);
	}

	public unsafe void GetAttributeValue(string name, out float value)
	{
		fixed (float* value2 = &value)
		{
			GetAttributeValue(name, SvgAttributePodType.Float, new IntPtr(value2), 4);
		}
	}

	public unsafe void SetAttributeValue(string name, RawColor4 color)
	{
		SetAttributeValue(name, SvgAttributePodType.Color, new IntPtr(&color), sizeof(RawColor4));
	}

	public unsafe void GetAttributeValue(string name, out RawColor4 color)
	{
		fixed (RawColor4* value = &color)
		{
			GetAttributeValue(name, SvgAttributePodType.Color, new IntPtr(value), sizeof(RawColor4));
		}
	}

	public unsafe void SetAttributeValue(string name, FillMode fillMode)
	{
		SetAttributeValue(name, SvgAttributePodType.FillMode, new IntPtr(&fillMode), 4);
	}

	public unsafe void GetAttributeValue(string name, out FillMode fillMode)
	{
		fixed (FillMode* value = &fillMode)
		{
			GetAttributeValue(name, SvgAttributePodType.FillMode, new IntPtr(value), 4);
		}
	}

	public unsafe void SetAttributeValue(string name, SvgDisplay display)
	{
		SetAttributeValue(name, SvgAttributePodType.Display, new IntPtr(&display), 4);
	}

	public unsafe void GetAttributeValue(string name, out SvgDisplay display)
	{
		fixed (SvgDisplay* value = &display)
		{
			GetAttributeValue(name, SvgAttributePodType.Display, new IntPtr(value), 4);
		}
	}

	public unsafe void SetAttributeValue(string name, SvgOverflow overflow)
	{
		SetAttributeValue(name, SvgAttributePodType.Overflow, new IntPtr(&overflow), 4);
	}

	public unsafe void GetAttributeValue(string name, out SvgOverflow overflow)
	{
		fixed (SvgOverflow* value = &overflow)
		{
			GetAttributeValue(name, SvgAttributePodType.Overflow, new IntPtr(value), 4);
		}
	}

	public unsafe void SetAttributeValue(string name, SvgLineJoin lineJoin)
	{
		SetAttributeValue(name, SvgAttributePodType.LineJoin, new IntPtr(&lineJoin), 4);
	}

	public unsafe void GetAttributeValue(string name, out SvgLineJoin lineJoin)
	{
		fixed (SvgLineJoin* value = &lineJoin)
		{
			GetAttributeValue(name, SvgAttributePodType.LineJoin, new IntPtr(value), 4);
		}
	}

	public unsafe void SetAttributeValue(string name, SvgLineCap lineCap)
	{
		SetAttributeValue(name, SvgAttributePodType.LineCap, new IntPtr(&lineCap), 4);
	}

	public unsafe void GetAttributeValue(string name, out SvgLineCap lineCap)
	{
		fixed (SvgLineCap* value = &lineCap)
		{
			GetAttributeValue(name, SvgAttributePodType.LineCap, new IntPtr(value), 4);
		}
	}

	public unsafe void SetAttributeValue(string name, SvgVisibility visibility)
	{
		SetAttributeValue(name, SvgAttributePodType.Visibility, new IntPtr(&visibility), 4);
	}

	public unsafe void GetAttributeValue(string name, out SvgVisibility visibility)
	{
		fixed (SvgVisibility* value = &visibility)
		{
			GetAttributeValue(name, SvgAttributePodType.Visibility, new IntPtr(value), 4);
		}
	}

	public unsafe void SetAttributeValue(string name, RawMatrix3x2 matrix)
	{
		SetAttributeValue(name, SvgAttributePodType.Matrix, new IntPtr(&matrix), sizeof(RawMatrix3x2));
	}

	public unsafe void GetAttributeValue(string name, out RawMatrix3x2 matrix)
	{
		fixed (RawMatrix3x2* value = &matrix)
		{
			GetAttributeValue(name, SvgAttributePodType.Matrix, new IntPtr(value), sizeof(RawMatrix3x2));
		}
	}

	public unsafe void SetAttributeValue(string name, SvgUnitType unitType)
	{
		SetAttributeValue(name, SvgAttributePodType.UnitType, new IntPtr(&unitType), 4);
	}

	public unsafe void GetAttributeValue(string name, out SvgUnitType unitType)
	{
		fixed (SvgUnitType* value = &unitType)
		{
			GetAttributeValue(name, SvgAttributePodType.UnitType, new IntPtr(value), 4);
		}
	}

	public unsafe void SetAttributeValue(string name, ExtendMode extendMode)
	{
		SetAttributeValue(name, SvgAttributePodType.ExtendMode, new IntPtr(&extendMode), 4);
	}

	public unsafe void GetAttributeValue(string name, out ExtendMode extendMode)
	{
		fixed (ExtendMode* value = &extendMode)
		{
			GetAttributeValue(name, SvgAttributePodType.ExtendMode, new IntPtr(value), 4);
		}
	}

	public unsafe void SetAttributeValue(string name, SvgPreserveAspectRatio preserveAspectRatio)
	{
		SetAttributeValue(name, SvgAttributePodType.PreserveAspectRatio, new IntPtr(&preserveAspectRatio), sizeof(SvgPreserveAspectRatio));
	}

	public unsafe void GetAttributeValue(string name, out SvgPreserveAspectRatio preserveAspectRatio)
	{
		fixed (SvgPreserveAspectRatio* value = &preserveAspectRatio)
		{
			GetAttributeValue(name, SvgAttributePodType.PreserveAspectRatio, new IntPtr(value), sizeof(SvgPreserveAspectRatio));
		}
	}

	public unsafe void SetAttributeValue(string name, SvgLength length)
	{
		SetAttributeValue(name, SvgAttributePodType.Length, new IntPtr(&length), sizeof(SvgLength));
	}

	public unsafe void GetAttributeValue(string name, out SvgLength length)
	{
		fixed (SvgLength* value = &length)
		{
			GetAttributeValue(name, SvgAttributePodType.Length, new IntPtr(value), sizeof(SvgLength));
		}
	}

	public T GetAttributeValue<T>(string name) where T : SvgAttribute
	{
		GetAttributeValue(name, Utilities.GetGuidFromType(typeof(T)), out var value);
		return CppObject.FromPointer<T>(value);
	}

	public SvgElement(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator SvgElement(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new SvgElement(nativePtr);
		}
		return null;
	}

	internal unsafe void GetDocument(out SvgDocument document)
	{
		IntPtr zero = IntPtr.Zero;
		((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, &zero);
		if (zero != IntPtr.Zero)
		{
			document = new SvgDocument(zero);
		}
		else
		{
			document = null;
		}
	}

	public unsafe void GetTagName(IntPtr name, int nameCount)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer, (void*)name, nameCount)).CheckError();
	}

	internal unsafe int GetTagNameLength()
	{
		return ((delegate* unmanaged[Stdcall]<void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe RawBool IsTextContent_()
	{
		return ((delegate* unmanaged[Stdcall]<void*, RawBool>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe void GetParent(out SvgElement arentRef)
	{
		IntPtr zero = IntPtr.Zero;
		((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, &zero);
		if (zero != IntPtr.Zero)
		{
			arentRef = new SvgElement(zero);
		}
		else
		{
			arentRef = null;
		}
	}

	public unsafe RawBool HasChildren()
	{
		return ((delegate* unmanaged[Stdcall]<void*, RawBool>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe void GetFirstChild(out SvgElement child)
	{
		IntPtr zero = IntPtr.Zero;
		((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)10 * (nint)sizeof(void*))))(_nativePointer, &zero);
		if (zero != IntPtr.Zero)
		{
			child = new SvgElement(zero);
		}
		else
		{
			child = null;
		}
	}

	internal unsafe void GetLastChild(out SvgElement child)
	{
		IntPtr zero = IntPtr.Zero;
		((delegate* unmanaged[Stdcall]<void*, void*, void>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)11 * (nint)sizeof(void*))))(_nativePointer, &zero);
		if (zero != IntPtr.Zero)
		{
			child = new SvgElement(zero);
		}
		else
		{
			child = null;
		}
	}

	public unsafe void GetPreviousChild(SvgElement referenceChild, out SvgElement reviousChildRef)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<SvgElement>(referenceChild);
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)12 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, &zero2);
		if (zero2 != IntPtr.Zero)
		{
			reviousChildRef = new SvgElement(zero2);
		}
		else
		{
			reviousChildRef = null;
		}
		result.CheckError();
	}

	public unsafe void GetNextChild(SvgElement referenceChild, out SvgElement nextChild)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<SvgElement>(referenceChild);
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)13 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, &zero2);
		if (zero2 != IntPtr.Zero)
		{
			nextChild = new SvgElement(zero2);
		}
		else
		{
			nextChild = null;
		}
		result.CheckError();
	}

	public unsafe void InsertChildBefore(SvgElement newChild, SvgElement referenceChild)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<SvgElement>(newChild);
		zero2 = CppObject.ToCallbackPtr<SvgElement>(referenceChild);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)14 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, (void*)zero2)).CheckError();
	}

	public unsafe void AppendChild(SvgElement newChild)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<SvgElement>(newChild);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)15 * (nint)sizeof(void*))))(_nativePointer, (void*)zero)).CheckError();
	}

	public unsafe void ReplaceChild(SvgElement newChild, SvgElement oldChild)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<SvgElement>(newChild);
		zero2 = CppObject.ToCallbackPtr<SvgElement>(oldChild);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)16 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, (void*)zero2)).CheckError();
	}

	public unsafe void RemoveChild(SvgElement oldChild)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<SvgElement>(oldChild);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)17 * (nint)sizeof(void*))))(_nativePointer, (void*)zero)).CheckError();
	}

	public unsafe void CreateChild(string tagName, out SvgElement newChild)
	{
		IntPtr zero = IntPtr.Zero;
		Result result;
		fixed (char* ptr = tagName)
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)18 * (nint)sizeof(void*))))(_nativePointer, ptr, &zero);
		}
		if (zero != IntPtr.Zero)
		{
			newChild = new SvgElement(zero);
		}
		else
		{
			newChild = null;
		}
		result.CheckError();
	}

	public unsafe RawBool IsAttributeSpecified(string name, out RawBool inherited)
	{
		inherited = default(RawBool);
		RawBool result;
		fixed (RawBool* ptr = &inherited)
		{
			void* ptr2 = ptr;
			fixed (char* ptr3 = name)
			{
				result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, RawBool>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)19 * (nint)sizeof(void*))))(_nativePointer, ptr3, ptr2);
			}
		}
		return result;
	}

	internal unsafe int GetSpecifiedAttributeCount()
	{
		return ((delegate* unmanaged[Stdcall]<void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)20 * (nint)sizeof(void*))))(_nativePointer);
	}

	public unsafe void GetSpecifiedAttributeName(int index, IntPtr name, int nameCount, out RawBool inherited)
	{
		inherited = default(RawBool);
		Result result;
		fixed (RawBool* ptr = &inherited)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, int, void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)21 * (nint)sizeof(void*))))(_nativePointer, index, (void*)name, nameCount, ptr2);
		}
		result.CheckError();
	}

	public unsafe void GetSpecifiedAttributeNameLength(int index, out int nameLength, out RawBool inherited)
	{
		inherited = default(RawBool);
		Result result;
		fixed (RawBool* ptr = &inherited)
		{
			void* ptr2 = ptr;
			fixed (int* ptr3 = &nameLength)
			{
				void* ptr4 = ptr3;
				result = ((delegate* unmanaged[Stdcall]<void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)22 * (nint)sizeof(void*))))(_nativePointer, index, ptr4, ptr2);
			}
		}
		result.CheckError();
	}

	public unsafe void RemoveAttribute(string name)
	{
		Result result;
		fixed (char* ptr = name)
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)23 * (nint)sizeof(void*))))(_nativePointer, ptr);
		}
		result.CheckError();
	}

	public unsafe void SetTextValue(string name, int nameCount)
	{
		Result result;
		fixed (char* ptr = name)
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)24 * (nint)sizeof(void*))))(_nativePointer, ptr, nameCount);
		}
		result.CheckError();
	}

	public unsafe void GetTextValue(IntPtr name, int nameCount)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)25 * (nint)sizeof(void*))))(_nativePointer, (void*)name, nameCount)).CheckError();
	}

	internal unsafe int GetTextValueLength()
	{
		return ((delegate* unmanaged[Stdcall]<void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)26 * (nint)sizeof(void*))))(_nativePointer);
	}

	public unsafe void SetAttributeValue(string name, SvgAttribute value)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<SvgAttribute>(value);
		Result result;
		fixed (char* ptr = name)
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)27 * (nint)sizeof(void*))))(_nativePointer, ptr, (void*)zero);
		}
		result.CheckError();
	}

	public unsafe void SetAttributeValue(string name, SvgAttributePodType type, IntPtr value, int valueSizeInBytes)
	{
		Result result;
		fixed (char* ptr = name)
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int, void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)28 * (nint)sizeof(void*))))(_nativePointer, ptr, (int)type, (void*)value, valueSizeInBytes);
		}
		result.CheckError();
	}

	public unsafe void SetAttributeValue(string name, SvgAttributeStringType type, string value)
	{
		Result result;
		fixed (char* ptr = value)
		{
			fixed (char* ptr2 = name)
			{
				result = ((delegate* unmanaged[Stdcall]<void*, void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)29 * (nint)sizeof(void*))))(_nativePointer, ptr2, (int)type, ptr);
			}
		}
		result.CheckError();
	}

	public unsafe void GetAttributeValue(string name, Guid riid, out IntPtr value)
	{
		Result result;
		fixed (IntPtr* ptr = &value)
		{
			void* ptr2 = ptr;
			fixed (char* ptr3 = name)
			{
				result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)30 * (nint)sizeof(void*))))(_nativePointer, ptr3, &riid, ptr2);
			}
		}
		result.CheckError();
	}

	public unsafe void GetAttributeValue(string name, SvgAttributePodType type, IntPtr value, int valueSizeInBytes)
	{
		Result result;
		fixed (char* ptr = name)
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int, void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)31 * (nint)sizeof(void*))))(_nativePointer, ptr, (int)type, (void*)value, valueSizeInBytes);
		}
		result.CheckError();
	}

	public unsafe void GetAttributeValue(string name, SvgAttributeStringType type, IntPtr value, int valueCount)
	{
		Result result;
		fixed (char* ptr = name)
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int, void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)32 * (nint)sizeof(void*))))(_nativePointer, ptr, (int)type, (void*)value, valueCount);
		}
		result.CheckError();
	}

	public unsafe void GetAttributeValueLength(string name, SvgAttributeStringType type, out int valueLength)
	{
		Result result;
		fixed (int* ptr = &valueLength)
		{
			void* ptr2 = ptr;
			fixed (char* ptr3 = name)
			{
				result = ((delegate* unmanaged[Stdcall]<void*, void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)33 * (nint)sizeof(void*))))(_nativePointer, ptr3, (int)type, ptr2);
			}
		}
		result.CheckError();
	}
}
