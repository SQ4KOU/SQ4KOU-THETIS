using System;
using System.Reflection;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Direct2D1;

internal class PropertyBinding
{
	public abstract class NativeGetSet
	{
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		protected delegate int NativeSetFunctionDelegate(IntPtr thisPtr, IntPtr dataPtr, int dataSize);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		protected delegate int NativeGetFunctionDelegate(IntPtr thisPtr, IntPtr dataPtr, int dataSize, out int actualSize);

		protected NativeGetFunctionDelegate getterNative;

		protected NativeSetFunctionDelegate setterNative;

		protected Type customEffectType;

		protected PropertyInfo propertyInfo;

		public IntPtr GetFunctionPtr { get; protected set; }

		public IntPtr SetFunctionPtr { get; protected set; }

		public NativeGetSet(Type customEffectType, PropertyInfo propertyInfo)
		{
			this.customEffectType = customEffectType;
			this.propertyInfo = propertyInfo;
		}
	}

	public class NativeGetSetValue<T> : NativeGetSet where T : struct
	{
		private readonly GetValueFastDelegate<T> getter;

		private readonly SetValueFastDelegate<T> setter;

		private static readonly int ValueSize;

		static NativeGetSetValue()
		{
			ValueSize = Utilities.SizeOf<T>();
		}

		public NativeGetSetValue(Type customEffectType, PropertyInfo propertyInfo)
			: base(customEffectType, propertyInfo)
		{
			if (propertyInfo.CanRead)
			{
				getter = Utilities.BuildPropertyGetter<T>(customEffectType, propertyInfo);
				getterNative = NativeGetInt;
				base.GetFunctionPtr = Marshal.GetFunctionPointerForDelegate((Delegate)getterNative);
			}
			if (propertyInfo.CanWrite)
			{
				setter = Utilities.BuildPropertySetter<T>(customEffectType, propertyInfo);
				setterNative = NativeSetInt;
				base.SetFunctionPtr = Marshal.GetFunctionPointerForDelegate((Delegate)setterNative);
			}
		}

		protected void SetValue(IntPtr sourceValue, out T destValue)
		{
			Utilities.ReadOut<T>(sourceValue, out destValue);
		}

		protected void GetValue(IntPtr destValue, ref T sourceValue)
		{
			Utilities.Write(destValue, ref sourceValue);
		}

		private int NativeSetInt(IntPtr thisPtr, IntPtr dataPtr, int dataSize)
		{
			try
			{
				if (dataPtr == IntPtr.Zero)
				{
					return Result.Ok.Code;
				}
				if (dataSize != ValueSize)
				{
					return Result.InvalidArg.Code;
				}
				CustomEffect obj = (CustomEffect)CppObjectShadow.ToShadow<CustomEffectShadow>(thisPtr).Callback;
				SetValue(dataPtr, out var destValue);
				setter(obj, ref destValue);
			}
			catch (Exception ex)
			{
				return (int)Result.GetResultFromException(ex);
			}
			return Result.Ok.Code;
		}

		private int NativeGetInt(IntPtr thisPtr, IntPtr dataPtr, int dataSize, out int actualSize)
		{
			actualSize = ValueSize;
			try
			{
				if (dataPtr == IntPtr.Zero)
				{
					return Result.Ok.Code;
				}
				CustomEffect obj = (CustomEffect)CppObjectShadow.ToShadow<CustomEffectShadow>(thisPtr).Callback;
				actualSize = ValueSize;
				getter(obj, out var value);
				GetValue(dataPtr, ref value);
			}
			catch (Exception ex)
			{
				return (int)Result.GetResultFromException(ex);
			}
			return Result.Ok.Code;
		}
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct __Native
	{
		public IntPtr PropertyName;

		public IntPtr SetFunction;

		public IntPtr GetFunction;
	}

	private NativeGetSet nativeGetSet;

	public string PropertyName;

	internal IntPtr SetFunction;

	internal IntPtr GetFunction;

	public string TypeName => PropertyTypeHelper.ConvertToString(Attribute.Type);

	public PropertyBindingAttribute Attribute { get; private set; }

	protected PropertyBinding()
	{
	}

	public static PropertyBinding Get(Type customEffectType, PropertyInfo propertyInfo)
	{
		PropertyBindingAttribute customAttribute = Utilities.GetCustomAttribute<PropertyBindingAttribute>(propertyInfo, inherited: true);
		if (customAttribute == null)
		{
			return null;
		}
		PropertyBinding propertyBinding = new PropertyBinding
		{
			Attribute = customAttribute
		};
		Type propertyType = propertyInfo.PropertyType;
		PropertyType propertyType2 = customAttribute.BindingType;
		if (propertyType2 == PropertyType.Unknown)
		{
			string name = propertyType.Name;
			if (propertyType == typeof(int))
			{
				propertyBinding.nativeGetSet = new NativeGetSetValue<int>(customEffectType, propertyInfo);
				propertyType2 = PropertyType.Int32;
			}
			else if (propertyType == typeof(float))
			{
				propertyBinding.nativeGetSet = new NativeGetSetValue<float>(customEffectType, propertyInfo);
				propertyType2 = PropertyType.Float;
			}
			else if (propertyType == typeof(uint))
			{
				propertyBinding.nativeGetSet = new NativeGetSetValue<uint>(customEffectType, propertyInfo);
				propertyType2 = PropertyType.UInt32;
			}
			else if (propertyType == typeof(bool))
			{
				propertyBinding.nativeGetSet = new NativeGetSetValue<int>(customEffectType, propertyInfo);
				propertyType2 = PropertyType.Bool;
			}
			else if (name.Contains("Vector2"))
			{
				propertyBinding.nativeGetSet = new NativeGetSetValue<RawVector2>(customEffectType, propertyInfo);
				propertyType2 = PropertyType.Vector2;
			}
			else if (name.Contains("Vector3"))
			{
				propertyBinding.nativeGetSet = new NativeGetSetValue<RawVector3>(customEffectType, propertyInfo);
				propertyType2 = PropertyType.Vector3;
			}
			else if (name.Contains("RectangleF"))
			{
				propertyBinding.nativeGetSet = new NativeGetSetValue<RawRectangleF>(customEffectType, propertyInfo);
				propertyType2 = PropertyType.Vector4;
			}
			else if (name.Contains("Vector4"))
			{
				propertyBinding.nativeGetSet = new NativeGetSetValue<RawVector4>(customEffectType, propertyInfo);
				propertyType2 = PropertyType.Vector4;
			}
			else if (name.Contains("Color3"))
			{
				propertyBinding.nativeGetSet = new NativeGetSetValue<RawColor3>(customEffectType, propertyInfo);
				propertyType2 = PropertyType.Vector3;
			}
			else if (name.Contains("Color4"))
			{
				propertyBinding.nativeGetSet = new NativeGetSetValue<RawColor4>(customEffectType, propertyInfo);
				propertyType2 = PropertyType.Vector4;
			}
			else if (name.Contains("Matrix3x2"))
			{
				propertyBinding.nativeGetSet = new NativeGetSetValue<RawMatrix3x2>(customEffectType, propertyInfo);
				propertyType2 = PropertyType.Matrix3x2;
			}
			else if (name.Contains("Matrix5x4"))
			{
				propertyBinding.nativeGetSet = new NativeGetSetValue<RawMatrix5x4>(customEffectType, propertyInfo);
				propertyType2 = PropertyType.Matrix5x4;
			}
			else if (name.Contains("Matrix"))
			{
				propertyBinding.nativeGetSet = new NativeGetSetValue<RawMatrix>(customEffectType, propertyInfo);
				propertyType2 = PropertyType.Matrix4x4;
			}
			else if (Utilities.IsEnum(propertyType))
			{
				propertyBinding.nativeGetSet = new NativeGetSetValue<int>(customEffectType, propertyInfo);
				propertyType2 = PropertyType.Enum;
			}
			else
			{
				if (!Utilities.IsAssignableFrom(typeof(ComObject), propertyType))
				{
					throw new SharpDXException($"Unsupported property type [{propertyType}] with binding [{propertyType2}] for custom effect [{customEffectType}]");
				}
				propertyBinding.nativeGetSet = new NativeGetSetValue<IntPtr>(customEffectType, propertyInfo);
				propertyType2 = PropertyType.IUnknown;
			}
		}
		customAttribute.Type = propertyType2;
		propertyBinding.PropertyName = customAttribute.DisplayName ?? propertyInfo.Name;
		propertyBinding.GetFunction = propertyBinding.nativeGetSet.GetFunctionPtr;
		propertyBinding.SetFunction = propertyBinding.nativeGetSet.SetFunctionPtr;
		return propertyBinding;
	}

	internal void __MarshalFree(ref __Native @ref)
	{
		Marshal.FreeHGlobal(@ref.PropertyName);
	}

	internal void __MarshalFrom(ref __Native @ref)
	{
		PropertyName = Marshal.PtrToStringUni(@ref.PropertyName);
		SetFunction = @ref.SetFunction;
		GetFunction = @ref.GetFunction;
	}

	internal void __MarshalTo(ref __Native @ref)
	{
		@ref.PropertyName = Marshal.StringToHGlobalUni(PropertyName);
		@ref.SetFunction = SetFunction;
		@ref.GetFunction = GetFunction;
	}
}
