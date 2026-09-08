using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct2D1;

[Guid("483473d7-cd46-4f9d-9d3a-3112aa80159d")]
public class Properties : ComObject
{
	public bool Cached
	{
		get
		{
			return GetBoolValue(-2147483642);
		}
		set
		{
			SetValue(-2147483642, value);
		}
	}

	public int PropertyCount => GetPropertyCount();

	public unsafe string GetPropertyName(int index)
	{
		int propertyNameLength = GetPropertyNameLength(index);
		if (propertyNameLength == 0)
		{
			return null;
		}
		int num = propertyNameLength + 1;
		char* value = stackalloc char[num];
		GetPropertyName(index, new IntPtr(value), num);
		return new string(value, 0, propertyNameLength);
	}

	public unsafe int GetIntValue(int index)
	{
		int result = default(int);
		GetValue(index, PropertyType.Int32, new IntPtr(&result), 4);
		return result;
	}

	public unsafe uint GetUIntValue(int index)
	{
		uint result = default(uint);
		GetValue(index, PropertyType.UInt32, new IntPtr(&result), 4);
		return result;
	}

	public unsafe float GetFloatValue(int index)
	{
		float result = default(float);
		GetValue(index, PropertyType.Float, new IntPtr(&result), 4);
		return result;
	}

	public unsafe bool GetBoolValue(int index)
	{
		int num = default(int);
		GetValue(index, PropertyType.Bool, new IntPtr(&num), 4);
		return num != 0;
	}

	public unsafe Guid GetGuidValue(int index)
	{
		Guid result = default(Guid);
		GetValue(index, PropertyType.Clsid, new IntPtr(&result), Utilities.SizeOf<Guid>());
		return result;
	}

	public unsafe RawVector2 GetVector2Value(int index)
	{
		RawVector2 result = default(RawVector2);
		GetValue(index, PropertyType.Vector2, new IntPtr(&result), Utilities.SizeOf<RawVector2>());
		return result;
	}

	public unsafe RawVector3 GetVector3Value(int index)
	{
		RawVector3 result = default(RawVector3);
		GetValue(index, PropertyType.Vector3, new IntPtr(&result), Utilities.SizeOf<RawVector3>());
		return result;
	}

	public unsafe RawColor3 GetColor3Value(int index)
	{
		RawColor3 result = default(RawColor3);
		GetValue(index, PropertyType.Vector3, new IntPtr(&result), Utilities.SizeOf<RawColor3>());
		return result;
	}

	public unsafe RawVector4 GetVector4Value(int index)
	{
		RawVector4 result = default(RawVector4);
		GetValue(index, PropertyType.Vector4, new IntPtr(&result), Utilities.SizeOf<RawVector4>());
		return result;
	}

	public unsafe RawRectangleF GetRectangleFValue(int index)
	{
		RawRectangleF result = default(RawRectangleF);
		GetValue(index, PropertyType.Vector4, new IntPtr(&result), Utilities.SizeOf<RawRectangleF>());
		return result;
	}

	public unsafe RawColor4 GetColor4Value(int index)
	{
		RawColor4 result = default(RawColor4);
		GetValue(index, PropertyType.Vector4, new IntPtr(&result), Utilities.SizeOf<RawColor4>());
		return result;
	}

	public unsafe RawMatrix GetMatrixValue(int index)
	{
		RawMatrix result = default(RawMatrix);
		GetValue(index, PropertyType.Matrix4x4, new IntPtr(&result), Utilities.SizeOf<RawMatrix>());
		return result;
	}

	public unsafe RawMatrix3x2 GetMatrix3x2Value(int index)
	{
		RawMatrix3x2 result = default(RawMatrix3x2);
		GetValue(index, PropertyType.Matrix3x2, new IntPtr(&result), Utilities.SizeOf<RawMatrix3x2>());
		return result;
	}

	public unsafe RawMatrix5x4 GetMatrix5x4Value(int index)
	{
		RawMatrix5x4 result = default(RawMatrix5x4);
		GetValue(index, PropertyType.Matrix5x4, new IntPtr(&result), Utilities.SizeOf<RawMatrix5x4>());
		return result;
	}

	public unsafe T GetEnumValue<T>(int index) where T : struct
	{
		if (Utilities.SizeOf<T>() != 4)
		{
			throw new ArgumentException("value", "enum must be sizeof(int)");
		}
		T data = default(T);
		GetValue(index, PropertyType.Enum, (IntPtr)Interop.Cast(ref data), 4);
		return data;
	}

	public unsafe T GetComObjectValue<T>(int index) where T : ComObject
	{
		IntPtr intPtr = default(IntPtr);
		GetValue(index, PropertyType.IUnknown, new IntPtr(&intPtr), Utilities.SizeOf<IntPtr>());
		if (!(intPtr == IntPtr.Zero))
		{
			return ComObject.As<T>(intPtr);
		}
		return null;
	}

	public unsafe T GetValue<T>(int index, PropertyType type) where T : struct
	{
		T data = default(T);
		GetValue(index, type, (IntPtr)Interop.Cast(ref data), Utilities.SizeOf<T>());
		return data;
	}

	public unsafe uint GetUIntValueByName(string name)
	{
		uint result = default(uint);
		GetValueByName(name, PropertyType.UInt32, new IntPtr(&result), 4);
		return result;
	}

	public unsafe float GetFloatValueByName(string name)
	{
		float result = default(float);
		GetValueByName(name, PropertyType.Float, new IntPtr(&result), 4);
		return result;
	}

	public unsafe bool GetBoolValueByName(string name)
	{
		int num = default(int);
		GetValueByName(name, PropertyType.Bool, new IntPtr(&num), 4);
		return num != 0;
	}

	public unsafe Guid GetGuidValueByName(string name)
	{
		Guid result = default(Guid);
		GetValueByName(name, PropertyType.Clsid, new IntPtr(&result), Utilities.SizeOf<Guid>());
		return result;
	}

	public unsafe RawVector2 GetVector2ValueByName(string name)
	{
		RawVector2 result = default(RawVector2);
		GetValueByName(name, PropertyType.Vector2, new IntPtr(&result), Utilities.SizeOf<RawVector2>());
		return result;
	}

	public unsafe RawVector3 GetVector3ValueByName(string name)
	{
		RawVector3 result = default(RawVector3);
		GetValueByName(name, PropertyType.Vector3, new IntPtr(&result), Utilities.SizeOf<RawVector3>());
		return result;
	}

	public unsafe RawColor3 GetColor3ValueByName(string name)
	{
		RawColor3 result = default(RawColor3);
		GetValueByName(name, PropertyType.Vector3, new IntPtr(&result), Utilities.SizeOf<RawColor3>());
		return result;
	}

	public unsafe RawVector4 GetVector4ValueByName(string name)
	{
		RawVector4 result = default(RawVector4);
		GetValueByName(name, PropertyType.Vector4, new IntPtr(&result), Utilities.SizeOf<RawVector4>());
		return result;
	}

	public unsafe RawRectangleF GetRectangleFValueByName(string name)
	{
		RawRectangleF result = default(RawRectangleF);
		GetValueByName(name, PropertyType.Vector4, new IntPtr(&result), Utilities.SizeOf<RawRectangleF>());
		return result;
	}

	public unsafe RawColor4 GetColor4ValueByName(string name)
	{
		RawColor4 result = default(RawColor4);
		GetValueByName(name, PropertyType.Vector4, new IntPtr(&result), Utilities.SizeOf<RawColor4>());
		return result;
	}

	public unsafe RawMatrix GetMatrixValueByName(string name)
	{
		RawMatrix result = default(RawMatrix);
		GetValueByName(name, PropertyType.Matrix4x4, new IntPtr(&result), Utilities.SizeOf<RawMatrix>());
		return result;
	}

	public unsafe RawMatrix3x2 GetMatrix3x2ValueByName(string name)
	{
		RawMatrix3x2 result = default(RawMatrix3x2);
		GetValueByName(name, PropertyType.Matrix3x2, new IntPtr(&result), Utilities.SizeOf<RawMatrix3x2>());
		return result;
	}

	public unsafe RawMatrix5x4 GetMatrix5x4ValueByName(string name)
	{
		RawMatrix5x4 result = default(RawMatrix5x4);
		GetValueByName(name, PropertyType.Matrix5x4, new IntPtr(&result), Utilities.SizeOf<RawMatrix5x4>());
		return result;
	}

	public unsafe T GetEnumValueByName<T>(string name) where T : struct
	{
		if (Utilities.SizeOf<T>() != 4)
		{
			throw new ArgumentException("value", "enum must be sizeof(int)");
		}
		T data = default(T);
		GetValueByName(name, PropertyType.Enum, (IntPtr)Interop.Cast(ref data), 4);
		return data;
	}

	public unsafe T GetComObjectValueByName<T>(string name) where T : ComObject
	{
		IntPtr intPtr = default(IntPtr);
		GetValueByName(name, PropertyType.IUnknown, new IntPtr(&intPtr), Utilities.SizeOf<IntPtr>());
		if (!(intPtr == IntPtr.Zero))
		{
			return ComObject.As<T>(intPtr);
		}
		return null;
	}

	public unsafe T GetValue<T>(string name, PropertyType type) where T : struct
	{
		T data = default(T);
		GetValueByName(name, type, (IntPtr)Interop.Cast(ref data), Utilities.SizeOf<T>());
		return data;
	}

	public unsafe void SetValueByName(string name, int value)
	{
		SetValueByName(name, PropertyType.Int32, new IntPtr(&value), 4);
	}

	public unsafe void SetValueByName(string name, uint value)
	{
		SetValueByName(name, PropertyType.UInt32, new IntPtr(&value), 4);
	}

	public unsafe void SetValueByName(string name, bool value)
	{
		int num = (value ? 1 : 0);
		SetValueByName(name, PropertyType.Bool, new IntPtr(&num), 4);
	}

	public unsafe void SetValueByName(string name, Guid value)
	{
		SetValueByName(name, PropertyType.Clsid, new IntPtr(&value), Utilities.SizeOf<Guid>());
	}

	public unsafe void SetValueByName(string name, float value)
	{
		SetValueByName(name, PropertyType.Float, new IntPtr(&value), 4);
	}

	public unsafe void SetValueByName(string name, RawVector2 value)
	{
		SetValueByName(name, PropertyType.Vector2, new IntPtr(&value), sizeof(RawVector2));
	}

	public unsafe void SetValueByName(string name, RawColor3 value)
	{
		SetValueByName(name, PropertyType.Vector3, new IntPtr(&value), sizeof(RawColor3));
	}

	public unsafe void SetValueByName(string name, RawVector4 value)
	{
		SetValueByName(name, PropertyType.Vector4, new IntPtr(&value), sizeof(RawVector4));
	}

	public unsafe void SetValueByName(string name, RawRectangleF value)
	{
		SetValueByName(name, PropertyType.Vector4, new IntPtr(&value), sizeof(RawRectangleF));
	}

	public unsafe void SetValueByName(string name, RawColor4 value)
	{
		SetValueByName(name, PropertyType.Vector4, new IntPtr(&value), sizeof(RawColor4));
	}

	public unsafe void SetValueByName(string name, RawMatrix3x2 value)
	{
		SetValueByName(name, PropertyType.Matrix3x2, new IntPtr(&value), sizeof(RawMatrix3x2));
	}

	public unsafe void SetValueByName(string name, RawMatrix value)
	{
		SetValueByName(name, PropertyType.Matrix4x4, new IntPtr(&value), sizeof(RawMatrix));
	}

	public unsafe void SetValueByName(string name, RawMatrix5x4 value)
	{
		SetValueByName(name, PropertyType.Matrix5x4, new IntPtr(&value), sizeof(RawMatrix5x4));
	}

	public void SetValueByName(string name, string value)
	{
		IntPtr intPtr = Marshal.StringToHGlobalUni(value);
		SetValueByName(name, PropertyType.String, intPtr, value?.Length ?? 0);
		Marshal.FreeHGlobal(intPtr);
	}

	public unsafe void SetValueByName<T>(string name, T value) where T : ComObject
	{
		IntPtr intPtr = value?.NativePointer ?? IntPtr.Zero;
		SetValueByName(name, PropertyType.IUnknown, new IntPtr(&intPtr), Utilities.SizeOf<IntPtr>());
	}

	public unsafe void SetValueByName<T>(string name, PropertyType type, T value) where T : struct
	{
		SetValueByName(name, type, (IntPtr)Interop.Cast(ref value), Utilities.SizeOf<T>());
	}

	public unsafe void SetValue(int index, int value)
	{
		SetValue(index, PropertyType.Int32, new IntPtr(&value), 4);
	}

	public unsafe void SetValue(int index, uint value)
	{
		SetValue(index, PropertyType.UInt32, new IntPtr(&value), 4);
	}

	public unsafe void SetValue(int index, bool value)
	{
		int num = (value ? 1 : 0);
		SetValue(index, PropertyType.Bool, new IntPtr(&num), 4);
	}

	public unsafe void SetValue(int index, Guid value)
	{
		SetValue(index, PropertyType.Clsid, new IntPtr(&value), Utilities.SizeOf<Guid>());
	}

	public unsafe void SetValue(int index, float value)
	{
		SetValue(index, PropertyType.Float, new IntPtr(&value), 4);
	}

	public unsafe void SetValue(int index, RawVector2 value)
	{
		SetValue(index, PropertyType.Vector2, new IntPtr(&value), sizeof(RawVector2));
	}

	public unsafe void SetValue(int index, RawVector3 value)
	{
		SetValue(index, PropertyType.Vector3, new IntPtr(&value), sizeof(RawVector3));
	}

	public unsafe void SetValue(int index, RawColor3 value)
	{
		SetValue(index, PropertyType.Vector3, new IntPtr(&value), sizeof(RawColor3));
	}

	public unsafe void SetValue(int index, RawVector4 value)
	{
		SetValue(index, PropertyType.Vector4, new IntPtr(&value), sizeof(RawVector4));
	}

	public unsafe void SetValue(int index, RawRectangleF value)
	{
		SetValue(index, PropertyType.Vector4, new IntPtr(&value), sizeof(RawRectangleF));
	}

	public unsafe void SetValue(int index, RawColor4 value)
	{
		SetValue(index, PropertyType.Vector4, new IntPtr(&value), sizeof(RawColor4));
	}

	public unsafe void SetValue(int index, RawMatrix3x2 value)
	{
		SetValue(index, PropertyType.Matrix3x2, new IntPtr(&value), sizeof(RawMatrix3x2));
	}

	public unsafe void SetValue(int index, RawMatrix value)
	{
		SetValue(index, PropertyType.Matrix4x4, new IntPtr(&value), sizeof(RawMatrix));
	}

	public unsafe void SetValue(int index, RawMatrix5x4 value)
	{
		SetValue(index, PropertyType.Matrix5x4, new IntPtr(&value), sizeof(RawMatrix5x4));
	}

	public void SetValue(int index, string value)
	{
		IntPtr intPtr = Marshal.StringToHGlobalUni(value);
		SetValue(index, PropertyType.String, intPtr, value?.Length ?? 0);
		Marshal.FreeHGlobal(intPtr);
	}

	public unsafe void SetEnumValue<T>(int index, T value) where T : struct
	{
		if (Utilities.SizeOf<T>() != 4)
		{
			throw new ArgumentException("value", "enum must be sizeof(int)");
		}
		SetValue(index, PropertyType.Enum, (IntPtr)Interop.Cast(ref value), 4);
	}

	public unsafe void SetValue<T>(int index, T value) where T : ComObject
	{
		IntPtr intPtr = value?.NativePointer ?? IntPtr.Zero;
		SetValue(index, PropertyType.IUnknown, new IntPtr(&intPtr), Utilities.SizeOf<IntPtr>());
	}

	public unsafe void SetValue<T>(int index, PropertyType type, T value) where T : struct
	{
		SetValue(index, type, (IntPtr)Interop.Cast(ref value), Utilities.SizeOf<T>());
	}

	public Properties(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Properties(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new Properties(nativePtr);
		}
		return null;
	}

	internal unsafe int GetPropertyCount()
	{
		return ((delegate* unmanaged[Stdcall]<void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer);
	}

	internal unsafe void GetPropertyName(int index, IntPtr name, int nameCount)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, index, (void*)name, nameCount)).CheckError();
	}

	internal unsafe int GetPropertyNameLength(int index)
	{
		return ((delegate* unmanaged[Stdcall]<void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer, index);
	}

	public unsafe PropertyType GetTypeInfo(int index)
	{
		return ((delegate* unmanaged[Stdcall]<void*, int, PropertyType>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(_nativePointer, index);
	}

	public unsafe int GetPropertyIndex(string name)
	{
		int result;
		fixed (char* ptr = name)
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer, ptr);
		}
		return result;
	}

	public unsafe void SetValueByName(string name, PropertyType type, IntPtr data, int dataSize)
	{
		Result result;
		fixed (char* ptr = name)
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int, void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, ptr, (int)type, (void*)data, dataSize);
		}
		result.CheckError();
	}

	public unsafe void SetValue(int index, PropertyType type, IntPtr data, int dataSize)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, int, void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(_nativePointer, index, (int)type, (void*)data, dataSize)).CheckError();
	}

	public unsafe void GetValueByName(string name, PropertyType type, IntPtr data, int dataSize)
	{
		Result result;
		fixed (char* ptr = name)
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int, void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)10 * (nint)sizeof(void*))))(_nativePointer, ptr, (int)type, (void*)data, dataSize);
		}
		result.CheckError();
	}

	public unsafe void GetValue(int index, PropertyType type, IntPtr data, int dataSize)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, int, void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)11 * (nint)sizeof(void*))))(_nativePointer, index, (int)type, (void*)data, dataSize)).CheckError();
	}

	public unsafe int GetValueSize(int index)
	{
		return ((delegate* unmanaged[Stdcall]<void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)12 * (nint)sizeof(void*))))(_nativePointer, index);
	}

	public unsafe Properties GetSubProperties(int index)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)13 * (nint)sizeof(void*))))(_nativePointer, index, &zero);
		Properties result2 = ((!(zero != IntPtr.Zero)) ? null : new Properties(zero));
		result.CheckError();
		return result2;
	}
}
