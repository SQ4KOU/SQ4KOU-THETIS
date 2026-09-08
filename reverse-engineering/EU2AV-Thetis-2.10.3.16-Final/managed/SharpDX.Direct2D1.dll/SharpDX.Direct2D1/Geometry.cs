using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct2D1;

[Guid("2cd906a1-12e2-11dc-9fed-001143a055f9")]
public class Geometry : Resource
{
	private float _flatteningTolerance = 0.25f;

	public const float DefaultFlatteningTolerance = 0.25f;

	public float FlatteningTolerance
	{
		get
		{
			return _flatteningTolerance;
		}
		set
		{
			_flatteningTolerance = value;
		}
	}

	public void Combine(Geometry inputGeometry, CombineMode combineMode, GeometrySink geometrySink)
	{
		Combine(inputGeometry, combineMode, null, FlatteningTolerance, geometrySink);
	}

	public void Combine(Geometry inputGeometry, CombineMode combineMode, float flatteningTolerance, GeometrySink geometrySink)
	{
		Combine(inputGeometry, combineMode, null, flatteningTolerance, geometrySink);
	}

	public GeometryRelation Compare(Geometry inputGeometry)
	{
		return Compare(inputGeometry, null, FlatteningTolerance);
	}

	public GeometryRelation Compare(Geometry inputGeometry, float flatteningTolerance)
	{
		return Compare(inputGeometry, null, flatteningTolerance);
	}

	public float ComputeArea()
	{
		return ComputeArea(null, FlatteningTolerance);
	}

	public float ComputeArea(float flatteningTolerance)
	{
		return ComputeArea(null, flatteningTolerance);
	}

	public float ComputeLength()
	{
		return ComputeLength(null, FlatteningTolerance);
	}

	public float ComputeLength(float flatteningTolerance)
	{
		return ComputeLength(null, flatteningTolerance);
	}

	public RawVector2 ComputePointAtLength(float length, out RawVector2 unitTangentVector)
	{
		return ComputePointAtLength(length, null, FlatteningTolerance, out unitTangentVector);
	}

	public RawVector2 ComputePointAtLength(float length, float flatteningTolerance, out RawVector2 unitTangentVector)
	{
		return ComputePointAtLength(length, null, flatteningTolerance, out unitTangentVector);
	}

	public bool FillContainsPoint(RawPoint point)
	{
		return FillContainsPoint(new RawVector2
		{
			X = point.X,
			Y = point.Y
		}, null, FlatteningTolerance);
	}

	public bool FillContainsPoint(RawVector2 point)
	{
		return FillContainsPoint(point, null, FlatteningTolerance);
	}

	public bool FillContainsPoint(RawPoint point, float flatteningTolerance)
	{
		return FillContainsPoint(new RawVector2
		{
			X = point.X,
			Y = point.Y
		}, null, flatteningTolerance);
	}

	public bool FillContainsPoint(RawVector2 point, float flatteningTolerance)
	{
		return FillContainsPoint(point, null, flatteningTolerance);
	}

	public bool FillContainsPoint(RawPoint point, RawMatrix3x2 worldTransform, float flatteningTolerance)
	{
		return FillContainsPoint(new RawVector2
		{
			X = point.X,
			Y = point.Y
		}, worldTransform, flatteningTolerance);
	}

	public RawRectangleF GetBounds()
	{
		return GetBounds(null);
	}

	public RawRectangleF GetWidenedBounds(float strokeWidth)
	{
		return GetWidenedBounds(strokeWidth, null, null, FlatteningTolerance);
	}

	public RawRectangleF GetWidenedBounds(float strokeWidth, float flatteningTolerance)
	{
		return GetWidenedBounds(strokeWidth, null, null, flatteningTolerance);
	}

	public RawRectangleF GetWidenedBounds(float strokeWidth, StrokeStyle strokeStyle, float flatteningTolerance)
	{
		return GetWidenedBounds(strokeWidth, strokeStyle, null, flatteningTolerance);
	}

	public void Outline(GeometrySink geometrySink)
	{
		Outline(null, FlatteningTolerance, geometrySink);
	}

	public void Outline(float flatteningTolerance, GeometrySink geometrySink)
	{
		Outline(null, flatteningTolerance, geometrySink);
	}

	public void Simplify(GeometrySimplificationOption simplificationOption, SimplifiedGeometrySink geometrySink)
	{
		Simplify(simplificationOption, null, FlatteningTolerance, geometrySink);
	}

	public void Simplify(GeometrySimplificationOption simplificationOption, float flatteningTolerance, SimplifiedGeometrySink geometrySink)
	{
		Simplify(simplificationOption, null, flatteningTolerance, geometrySink);
	}

	public bool StrokeContainsPoint(RawPoint point, float strokeWidth)
	{
		return StrokeContainsPoint(point, strokeWidth, null);
	}

	public bool StrokeContainsPoint(RawVector2 point, float strokeWidth)
	{
		return StrokeContainsPoint(point, strokeWidth, null);
	}

	public bool StrokeContainsPoint(RawPoint point, float strokeWidth, StrokeStyle strokeStyle)
	{
		return StrokeContainsPoint(new RawVector2
		{
			X = point.X,
			Y = point.Y
		}, strokeWidth, strokeStyle);
	}

	public bool StrokeContainsPoint(RawVector2 point, float strokeWidth, StrokeStyle strokeStyle)
	{
		return StrokeContainsPoint(point, strokeWidth, strokeStyle, null, FlatteningTolerance);
	}

	public bool StrokeContainsPoint(RawPoint point, float strokeWidth, StrokeStyle strokeStyle, RawMatrix3x2 transform)
	{
		return StrokeContainsPoint(point, strokeWidth, strokeStyle, transform, FlatteningTolerance);
	}

	public bool StrokeContainsPoint(RawVector2 point, float strokeWidth, StrokeStyle strokeStyle, RawMatrix3x2 transform)
	{
		return StrokeContainsPoint(point, strokeWidth, strokeStyle, transform, FlatteningTolerance);
	}

	public bool StrokeContainsPoint(RawPoint point, float strokeWidth, StrokeStyle strokeStyle, RawMatrix3x2 transform, float flatteningTolerance)
	{
		return StrokeContainsPoint(new RawVector2
		{
			X = point.X,
			Y = point.Y
		}, strokeWidth, strokeStyle, transform, flatteningTolerance);
	}

	public void Tessellate(TessellationSink tessellationSink)
	{
		Tessellate(null, FlatteningTolerance, tessellationSink);
	}

	public void Tessellate(float flatteningTolerance, TessellationSink tessellationSink)
	{
		Tessellate(null, flatteningTolerance, tessellationSink);
	}

	public void Widen(float strokeWidth, GeometrySink geometrySink)
	{
		Widen(strokeWidth, null, null, FlatteningTolerance, geometrySink);
	}

	public void Widen(float strokeWidth, float flatteningTolerance, GeometrySink geometrySink)
	{
		Widen(strokeWidth, null, null, flatteningTolerance, geometrySink);
	}

	public void Widen(float strokeWidth, StrokeStyle strokeStyle, float flatteningTolerance, GeometrySink geometrySink)
	{
		Widen(strokeWidth, strokeStyle, null, flatteningTolerance, geometrySink);
	}

	public Geometry(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Geometry(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new Geometry(nativePtr);
		}
		return null;
	}

	public unsafe RawRectangleF GetBounds(RawMatrix3x2? worldTransform)
	{
		RawMatrix3x2 value = default(RawMatrix3x2);
		if (worldTransform.HasValue)
		{
			value = worldTransform.Value;
		}
		void* nativePointer = _nativePointer;
		RawMatrix3x2* intPtr = ((!worldTransform.HasValue) ? null : (&value));
		RawRectangleF result = default(RawRectangleF);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(nativePointer, intPtr, &result)).CheckError();
		return result;
	}

	public unsafe RawRectangleF GetWidenedBounds(float strokeWidth, StrokeStyle strokeStyle, RawMatrix3x2? worldTransform, float flatteningTolerance)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<StrokeStyle>(strokeStyle);
		RawMatrix3x2 value = default(RawMatrix3x2);
		if (worldTransform.HasValue)
		{
			value = worldTransform.Value;
		}
		void* nativePointer = _nativePointer;
		void* intPtr = (void*)zero;
		RawMatrix3x2* intPtr2 = ((!worldTransform.HasValue) ? null : (&value));
		RawRectangleF result = default(RawRectangleF);
		((Result)((delegate* unmanaged[Stdcall]<void*, float, void*, void*, float, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(nativePointer, strokeWidth, intPtr, intPtr2, flatteningTolerance, &result)).CheckError();
		return result;
	}

	public unsafe RawBool StrokeContainsPoint(RawVector2 point, float strokeWidth, StrokeStyle strokeStyle, RawMatrix3x2? worldTransform, float flatteningTolerance)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<StrokeStyle>(strokeStyle);
		RawMatrix3x2 value = default(RawMatrix3x2);
		if (worldTransform.HasValue)
		{
			value = worldTransform.Value;
		}
		void* nativePointer = _nativePointer;
		void* intPtr = (void*)zero;
		RawMatrix3x2* intPtr2 = ((!worldTransform.HasValue) ? null : (&value));
		RawBool result = default(RawBool);
		((Result)((delegate* unmanaged[Stdcall]<void*, RawVector2, float, void*, void*, float, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(nativePointer, point, strokeWidth, intPtr, intPtr2, flatteningTolerance, &result)).CheckError();
		return result;
	}

	public unsafe RawBool FillContainsPoint(RawVector2 point, RawMatrix3x2? worldTransform, float flatteningTolerance)
	{
		RawMatrix3x2 value = default(RawMatrix3x2);
		if (worldTransform.HasValue)
		{
			value = worldTransform.Value;
		}
		void* nativePointer = _nativePointer;
		RawMatrix3x2* intPtr = ((!worldTransform.HasValue) ? null : (&value));
		RawBool result = default(RawBool);
		((Result)((delegate* unmanaged[Stdcall]<void*, RawVector2, void*, float, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(nativePointer, point, intPtr, flatteningTolerance, &result)).CheckError();
		return result;
	}

	public unsafe GeometryRelation Compare(Geometry inputGeometry, RawMatrix3x2? inputGeometryTransform, float flatteningTolerance)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Geometry>(inputGeometry);
		RawMatrix3x2 value = default(RawMatrix3x2);
		if (inputGeometryTransform.HasValue)
		{
			value = inputGeometryTransform.Value;
		}
		void* nativePointer = _nativePointer;
		void* intPtr = (void*)zero;
		RawMatrix3x2* intPtr2 = ((!inputGeometryTransform.HasValue) ? null : (&value));
		GeometryRelation result = default(GeometryRelation);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, void*, float, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(nativePointer, intPtr, intPtr2, flatteningTolerance, &result)).CheckError();
		return result;
	}

	public unsafe void Simplify(GeometrySimplificationOption simplificationOption, RawMatrix3x2? worldTransform, float flatteningTolerance, SimplifiedGeometrySink geometrySink)
	{
		IntPtr zero = IntPtr.Zero;
		RawMatrix3x2 value = default(RawMatrix3x2);
		if (worldTransform.HasValue)
		{
			value = worldTransform.Value;
		}
		zero = CppObject.ToCallbackPtr<SimplifiedGeometrySink>(geometrySink);
		void* nativePointer = _nativePointer;
		RawMatrix3x2* intPtr = ((!worldTransform.HasValue) ? null : (&value));
		((Result)((delegate* unmanaged[Stdcall]<void*, int, void*, float, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(nativePointer, (int)simplificationOption, intPtr, flatteningTolerance, (void*)zero)).CheckError();
	}

	public unsafe void Tessellate(RawMatrix3x2? worldTransform, float flatteningTolerance, TessellationSink tessellationSink)
	{
		IntPtr zero = IntPtr.Zero;
		RawMatrix3x2 value = default(RawMatrix3x2);
		if (worldTransform.HasValue)
		{
			value = worldTransform.Value;
		}
		zero = CppObject.ToCallbackPtr<TessellationSink>(tessellationSink);
		void* nativePointer = _nativePointer;
		RawMatrix3x2* intPtr = ((!worldTransform.HasValue) ? null : (&value));
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, float, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)10 * (nint)sizeof(void*))))(nativePointer, intPtr, flatteningTolerance, (void*)zero)).CheckError();
	}

	public unsafe void Combine(Geometry inputGeometry, CombineMode combineMode, RawMatrix3x2? inputGeometryTransform, float flatteningTolerance, SimplifiedGeometrySink geometrySink)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<Geometry>(inputGeometry);
		RawMatrix3x2 value = default(RawMatrix3x2);
		if (inputGeometryTransform.HasValue)
		{
			value = inputGeometryTransform.Value;
		}
		zero2 = CppObject.ToCallbackPtr<SimplifiedGeometrySink>(geometrySink);
		void* nativePointer = _nativePointer;
		void* intPtr = (void*)zero;
		RawMatrix3x2* intPtr2 = ((!inputGeometryTransform.HasValue) ? null : (&value));
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int, void*, float, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)11 * (nint)sizeof(void*))))(nativePointer, intPtr, (int)combineMode, intPtr2, flatteningTolerance, (void*)zero2)).CheckError();
	}

	public unsafe void Outline(RawMatrix3x2? worldTransform, float flatteningTolerance, SimplifiedGeometrySink geometrySink)
	{
		IntPtr zero = IntPtr.Zero;
		RawMatrix3x2 value = default(RawMatrix3x2);
		if (worldTransform.HasValue)
		{
			value = worldTransform.Value;
		}
		zero = CppObject.ToCallbackPtr<SimplifiedGeometrySink>(geometrySink);
		void* nativePointer = _nativePointer;
		RawMatrix3x2* intPtr = ((!worldTransform.HasValue) ? null : (&value));
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, float, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)12 * (nint)sizeof(void*))))(nativePointer, intPtr, flatteningTolerance, (void*)zero)).CheckError();
	}

	public unsafe float ComputeArea(RawMatrix3x2? worldTransform, float flatteningTolerance)
	{
		RawMatrix3x2 value = default(RawMatrix3x2);
		if (worldTransform.HasValue)
		{
			value = worldTransform.Value;
		}
		void* nativePointer = _nativePointer;
		RawMatrix3x2* intPtr = ((!worldTransform.HasValue) ? null : (&value));
		float result = default(float);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, float, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)13 * (nint)sizeof(void*))))(nativePointer, intPtr, flatteningTolerance, &result)).CheckError();
		return result;
	}

	public unsafe float ComputeLength(RawMatrix3x2? worldTransform, float flatteningTolerance)
	{
		RawMatrix3x2 value = default(RawMatrix3x2);
		if (worldTransform.HasValue)
		{
			value = worldTransform.Value;
		}
		void* nativePointer = _nativePointer;
		RawMatrix3x2* intPtr = ((!worldTransform.HasValue) ? null : (&value));
		float result = default(float);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, float, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)14 * (nint)sizeof(void*))))(nativePointer, intPtr, flatteningTolerance, &result)).CheckError();
		return result;
	}

	public unsafe RawVector2 ComputePointAtLength(float length, RawMatrix3x2? worldTransform, float flatteningTolerance, out RawVector2 unitTangentVector)
	{
		unitTangentVector = default(RawVector2);
		RawMatrix3x2 value = default(RawMatrix3x2);
		if (worldTransform.HasValue)
		{
			value = worldTransform.Value;
		}
		Result result;
		RawVector2 result2 = default(RawVector2);
		fixed (RawVector2* ptr = &unitTangentVector)
		{
			void* ptr2 = ptr;
			void* nativePointer = _nativePointer;
			RawMatrix3x2* intPtr = ((!worldTransform.HasValue) ? null : (&value));
			result = ((delegate* unmanaged[Stdcall]<void*, float, void*, float, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)15 * (nint)sizeof(void*))))(nativePointer, length, intPtr, flatteningTolerance, &result2, ptr2);
		}
		result.CheckError();
		return result2;
	}

	public unsafe void Widen(float strokeWidth, StrokeStyle strokeStyle, RawMatrix3x2? worldTransform, float flatteningTolerance, SimplifiedGeometrySink geometrySink)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<StrokeStyle>(strokeStyle);
		RawMatrix3x2 value = default(RawMatrix3x2);
		if (worldTransform.HasValue)
		{
			value = worldTransform.Value;
		}
		zero2 = CppObject.ToCallbackPtr<SimplifiedGeometrySink>(geometrySink);
		void* nativePointer = _nativePointer;
		void* intPtr = (void*)zero;
		RawMatrix3x2* intPtr2 = ((!worldTransform.HasValue) ? null : (&value));
		((Result)((delegate* unmanaged[Stdcall]<void*, float, void*, void*, float, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)16 * (nint)sizeof(void*))))(nativePointer, strokeWidth, intPtr, intPtr2, flatteningTolerance, (void*)zero2)).CheckError();
	}
}
