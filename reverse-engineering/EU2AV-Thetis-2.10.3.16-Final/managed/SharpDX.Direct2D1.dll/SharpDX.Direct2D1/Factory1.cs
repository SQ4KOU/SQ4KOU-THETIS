using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using SharpDX.DXGI;
using SharpDX.DirectWrite;
using SharpDX.Win32;

namespace SharpDX.Direct2D1;

[Guid("bb12d362-daee-4b9a-aa1d-14ba401cfa1f")]
public class Factory1 : Factory
{
	private Dictionary<Guid, CustomEffectFactory> registeredEffects = new Dictionary<Guid, CustomEffectFactory>();

	public Guid[] RegisteredEffects
	{
		get
		{
			GetRegisteredEffects(null, 0, out var effectsReturned, out var effectsRegistered);
			Guid[] array = new Guid[effectsRegistered];
			GetRegisteredEffects(array, array.Length, out effectsReturned, out effectsRegistered);
			return array;
		}
	}

	public Factory1()
		: this(FactoryType.SingleThreaded)
	{
	}

	public Factory1(FactoryType factoryType)
		: this(factoryType, DebugLevel.None)
	{
	}

	public Factory1(FactoryType factoryType, DebugLevel debugLevel)
		: base(factoryType, debugLevel)
	{
	}

	public void RegisterEffect<T>(Func<T> effectFactory) where T : CustomEffect
	{
		Guid guidFromType = Utilities.GetGuidFromType(typeof(T));
		RegisterEffect(effectFactory, guidFromType);
	}

	public void RegisterEffect<T>(Func<T> effectFactory, Guid effectId) where T : CustomEffect
	{
		CustomEffectFactory customEffectFactory;
		lock (registeredEffects)
		{
			if (registeredEffects.ContainsKey(effectId))
			{
				throw new ArgumentException("An effect is already registered with this GUID", "effectFactory");
			}
			customEffectFactory = new CustomEffectFactory(() => effectFactory(), typeof(T), effectId);
			registeredEffects.Add(effectId, customEffectFactory);
		}
		RegisterEffectFromString(effectId, customEffectFactory.ToXml(), customEffectFactory.Bindings, customEffectFactory.Bindings.Length, customEffectFactory.NativePointer);
	}

	public void RegisterEffect<T>() where T : CustomEffect, new()
	{
		RegisterEffect<T>(Utilities.GetGuidFromType(typeof(T)));
	}

	public void RegisterEffect<T>(Guid effectId) where T : CustomEffect, new()
	{
		CustomEffectFactory customEffectFactory;
		lock (registeredEffects)
		{
			if (registeredEffects.ContainsKey(effectId))
			{
				throw new ArgumentException("An effect is already registered with this GUID", "effectFactory");
			}
			customEffectFactory = new CustomEffectFactory(() => new T(), typeof(T), effectId);
			registeredEffects.Add(effectId, customEffectFactory);
		}
		RegisterEffectFromString(effectId, customEffectFactory.ToXml(), customEffectFactory.Bindings, customEffectFactory.Bindings.Length, customEffectFactory.NativePointer);
	}

	public void UnRegisterEffect<T>() where T : CustomEffect
	{
		Guid guidFromType = Utilities.GetGuidFromType(typeof(T));
		lock (registeredEffects)
		{
			if (registeredEffects.TryGetValue(guidFromType, out var _))
			{
				registeredEffects.Remove(guidFromType);
			}
		}
		UnregisterEffect(guidFromType);
	}

	public Factory1(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Factory1(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new Factory1(nativePtr);
		}
		return null;
	}

	internal unsafe void CreateDevice(SharpDX.DXGI.Device dxgiDevice, Device d2dDevice)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<SharpDX.DXGI.Device>(dxgiDevice);
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)17 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, &zero2);
		d2dDevice.NativePointer = zero2;
		result.CheckError();
	}

	internal unsafe void CreateStrokeStyle(ref StrokeStyleProperties1 strokeStyleProperties, float[] dashes, int dashesCount, StrokeStyle1 strokeStyle)
	{
		IntPtr zero = IntPtr.Zero;
		Result result;
		fixed (float* ptr = dashes)
		{
			void* ptr2 = ptr;
			fixed (StrokeStyleProperties1* ptr3 = &strokeStyleProperties)
			{
				void* ptr4 = ptr3;
				result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)18 * (nint)sizeof(void*))))(_nativePointer, ptr4, ptr2, dashesCount, &zero);
			}
		}
		strokeStyle.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreatePathGeometry(PathGeometry1 athGeometryRef)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)19 * (nint)sizeof(void*))))(_nativePointer, &zero);
		athGeometryRef.NativePointer = zero;
		result.CheckError();
	}

	internal unsafe void CreateDrawingStateBlock(DrawingStateDescription1? drawingStateDescription, RenderingParams textRenderingParams, DrawingStateBlock1 drawingStateBlock)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		DrawingStateDescription1 value = default(DrawingStateDescription1);
		if (drawingStateDescription.HasValue)
		{
			value = drawingStateDescription.Value;
		}
		zero = CppObject.ToCallbackPtr<RenderingParams>(textRenderingParams);
		void* nativePointer = _nativePointer;
		DrawingStateDescription1* intPtr = ((!drawingStateDescription.HasValue) ? null : (&value));
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)20 * (nint)sizeof(void*))))(nativePointer, intPtr, (void*)zero, &zero2);
		drawingStateBlock.NativePointer = zero2;
		result.CheckError();
	}

	internal unsafe void CreateGdiMetafile(IStream metafileStream, out GdiMetafile metafile)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<IStream>(metafileStream);
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)21 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, &zero2);
		if (zero2 != IntPtr.Zero)
		{
			metafile = new GdiMetafile(zero2);
		}
		else
		{
			metafile = null;
		}
		result.CheckError();
	}

	internal unsafe void RegisterEffectFromStream(Guid classId, IStream ropertyXmlRef, PropertyBinding[] bindings, int bindingsCount, FunctionCallback effectFactory)
	{
		IntPtr zero = IntPtr.Zero;
		PropertyBinding.__Native[] array = ((bindings == null) ? null : new PropertyBinding.__Native[bindings.Length]);
		zero = CppObject.ToCallbackPtr<IStream>(ropertyXmlRef);
		if (bindings != null)
		{
			for (int i = 0; i < bindings.Length; i++)
			{
				if (bindings != null)
				{
					bindings[i].__MarshalTo(ref array[i]);
				}
			}
		}
		Result result;
		fixed (PropertyBinding.__Native* ptr = array)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)22 * (nint)sizeof(void*))))(_nativePointer, &classId, (void*)zero, ptr2, bindingsCount, effectFactory);
		}
		if (bindings != null)
		{
			for (int j = 0; j < bindings.Length; j++)
			{
				if (bindings != null)
				{
					bindings[j].__MarshalFree(ref array[j]);
				}
			}
		}
		result.CheckError();
	}

	internal unsafe void RegisterEffectFromString(Guid classId, string ropertyXmlRef, PropertyBinding[] bindings, int bindingsCount, FunctionCallback effectFactory)
	{
		PropertyBinding.__Native[] array = ((bindings == null) ? null : new PropertyBinding.__Native[bindings.Length]);
		if (bindings != null)
		{
			for (int i = 0; i < bindings.Length; i++)
			{
				if (bindings != null)
				{
					bindings[i].__MarshalTo(ref array[i]);
				}
			}
		}
		Result result;
		fixed (PropertyBinding.__Native* ptr = array)
		{
			void* ptr2 = ptr;
			fixed (char* ptr3 = ropertyXmlRef)
			{
				result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)23 * (nint)sizeof(void*))))(_nativePointer, &classId, ptr3, ptr2, bindingsCount, effectFactory);
			}
		}
		if (bindings != null)
		{
			for (int j = 0; j < bindings.Length; j++)
			{
				if (bindings != null)
				{
					bindings[j].__MarshalFree(ref array[j]);
				}
			}
		}
		result.CheckError();
	}

	internal unsafe void UnregisterEffect(Guid classId)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)24 * (nint)sizeof(void*))))(_nativePointer, &classId)).CheckError();
	}

	internal unsafe void GetRegisteredEffects(Guid[] effects, int effectsCount, out int effectsReturned, out int effectsRegistered)
	{
		Result result;
		fixed (int* ptr = &effectsRegistered)
		{
			void* ptr2 = ptr;
			fixed (int* ptr3 = &effectsReturned)
			{
				void* ptr4 = ptr3;
				fixed (Guid* ptr5 = effects)
				{
					void* ptr6 = ptr5;
					result = ((delegate* unmanaged[Stdcall]<void*, void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)25 * (nint)sizeof(void*))))(_nativePointer, ptr6, effectsCount, ptr4, ptr2);
				}
			}
		}
		result.CheckError();
	}

	public unsafe Properties GetEffectProperties(Guid effectId)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)26 * (nint)sizeof(void*))))(_nativePointer, &effectId, &zero);
		Properties result2 = ((!(zero != IntPtr.Zero)) ? null : new Properties(zero));
		result.CheckError();
		return result2;
	}
}
