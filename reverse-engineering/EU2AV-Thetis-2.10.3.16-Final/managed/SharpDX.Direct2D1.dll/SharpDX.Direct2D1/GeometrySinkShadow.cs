using System;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct2D1;

internal class GeometrySinkShadow : SimplifiedGeometrySinkShadow
{
	private class GeometrySinkVtbl : SimplifiedGeometrySinkVtbl
	{
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void AddLineDelegate(IntPtr thisPtr, RawVector2 point);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void AddBezierDelegate(IntPtr thisPtr, IntPtr bezier);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void AddQuadraticBezierDelegate(IntPtr thisPtr, IntPtr bezier);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void AddQuadraticBeziersDelegate(IntPtr thisPtr, IntPtr beziers, int beziersCount);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void AddArcDelegate(IntPtr thisPtr, IntPtr arc);

		public GeometrySinkVtbl()
			: base(5)
		{
			AddMethod(new AddLineDelegate(AddLineImpl));
			AddMethod(new AddBezierDelegate(AddBezierImpl));
			AddMethod(new AddQuadraticBezierDelegate(AddQuadraticBezierImpl));
			AddMethod(new AddQuadraticBeziersDelegate(AddQuadraticBeziersImpl));
			AddMethod(new AddArcDelegate(AddArcImpl));
		}

		private static void AddLineImpl(IntPtr thisPtr, RawVector2 point)
		{
			((GeometrySink)CppObjectShadow.ToShadow<GeometrySinkShadow>(thisPtr).Callback).AddLine(point);
		}

		private unsafe static void AddBezierImpl(IntPtr thisPtr, IntPtr bezier)
		{
			((GeometrySink)CppObjectShadow.ToShadow<GeometrySinkShadow>(thisPtr).Callback).AddBezier(*(BezierSegment*)(void*)bezier);
		}

		private unsafe static void AddQuadraticBezierImpl(IntPtr thisPtr, IntPtr bezier)
		{
			((GeometrySink)CppObjectShadow.ToShadow<GeometrySinkShadow>(thisPtr).Callback).AddQuadraticBezier(*(QuadraticBezierSegment*)(void*)bezier);
		}

		private static void AddQuadraticBeziersImpl(IntPtr thisPtr, IntPtr beziers, int beziersCount)
		{
			GeometrySink obj = (GeometrySink)CppObjectShadow.ToShadow<GeometrySinkShadow>(thisPtr).Callback;
			QuadraticBezierSegment[] array = new QuadraticBezierSegment[beziersCount];
			Utilities.Read(beziers, array, 0, beziersCount);
			obj.AddQuadraticBeziers(array);
		}

		private unsafe static void AddArcImpl(IntPtr thisPtr, IntPtr arc)
		{
			((GeometrySink)CppObjectShadow.ToShadow<GeometrySinkShadow>(thisPtr).Callback).AddArc(*(ArcSegment*)(void*)arc);
		}
	}

	private static readonly GeometrySinkVtbl Vtbl = new GeometrySinkVtbl();

	protected override CppObjectVtbl GetVtbl => Vtbl;

	public static IntPtr ToIntPtr(GeometrySink geometrySink)
	{
		return CppObject.ToCallbackPtr<GeometrySink>(geometrySink);
	}
}
