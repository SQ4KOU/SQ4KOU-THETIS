using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using SharpDX.Win32;

namespace SharpDX.WIC;

[Guid("30989668-E1C9-4597-B395-458EEDB808DF")]
public class MetadataQueryReader : ComObject
{
	public IEnumerable<string> Enumerator => new ComStringEnumerator(GetEnumerator());

	public IEnumerable<string> QueryPaths
	{
		get
		{
			foreach (string name in Enumerator)
			{
				if (TryGetMetadataByName(name, out var value).Success)
				{
					if (!(value is MetadataQueryReader metadataQueryReader))
					{
						yield return name;
						continue;
					}
					foreach (string queryPath in metadataQueryReader.QueryPaths)
					{
						yield return name + queryPath;
					}
				}
				else
				{
					yield return name;
				}
			}
		}
	}

	public unsafe string Location
	{
		get
		{
			int cchActualLengthRef = 0;
			GetLocation(0, IntPtr.Zero, out cchActualLengthRef);
			if (cchActualLengthRef == 0)
			{
				return null;
			}
			char* ptr = stackalloc char[cchActualLengthRef];
			GetLocation(cchActualLengthRef, (IntPtr)ptr, out cchActualLengthRef);
			return new string(ptr, 0, cchActualLengthRef - 1);
		}
	}

	public Guid ContainerFormat
	{
		get
		{
			GetContainerFormat(out var guidContainerFormatRef);
			return guidContainerFormatRef;
		}
	}

	public unsafe Result TryGetMetadataByName(string name, out object value)
	{
		value = null;
		byte* ptr = stackalloc byte[512];
		Result metadataByName = GetMetadataByName(name, (IntPtr)ptr);
		if (metadataByName.Success)
		{
			Variant* ptr2 = (Variant*)ptr;
			value = ptr2->Value;
			if (value is ComObject comObject)
			{
				value = comObject.QueryInterfaceOrNull<MetadataQueryReader>();
			}
		}
		return metadataByName;
	}

	public object TryGetMetadataByName(string name)
	{
		Result result = TryGetMetadataByName(name, out var value);
		if (ResultCode.Propertynotfound != result && ResultCode.Propertynotsupported != result)
		{
			result.CheckError();
		}
		return value;
	}

	public object GetMetadataByName(string name)
	{
		TryGetMetadataByName(name, out var value).CheckError();
		return value;
	}

	public void Dump(TextWriter writer, int level = 0)
	{
		foreach (string item in Enumerator)
		{
			object metadataByName = GetMetadataByName(item);
			for (int i = 0; i < level; i++)
			{
				writer.Write("    ");
			}
			string arg = ((metadataByName is MetadataQueryReader) ? "..." : string.Concat((metadataByName is Array) ? Utilities.Join(",", ((Array)metadataByName).GetEnumerator()) : ((metadataByName is IntPtr) ? $"0x{metadataByName:X}" : metadataByName)));
			writer.WriteLine("{0} = {1}", item, arg);
			if (metadataByName is MetadataQueryReader)
			{
				((MetadataQueryReader)metadataByName).Dump(writer, level + 1);
			}
		}
	}

	public MetadataQueryReader(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator MetadataQueryReader(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new MetadataQueryReader(nativePtr);
		}
		return null;
	}

	internal unsafe void GetContainerFormat(out Guid guidContainerFormatRef)
	{
		guidContainerFormatRef = default(Guid);
		Result result;
		fixed (Guid* ptr = &guidContainerFormatRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer, ptr2);
		}
		result.CheckError();
	}

	internal unsafe void GetLocation(int cchMaxLength, IntPtr @namespace, out int cchActualLengthRef)
	{
		Result result;
		fixed (int* ptr = &cchActualLengthRef)
		{
			void* ptr2 = ptr;
			result = ((delegate* unmanaged[Stdcall]<void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, cchMaxLength, (void*)@namespace, ptr2);
		}
		result.CheckError();
	}

	internal unsafe Result GetMetadataByName(string name, IntPtr varValueRef)
	{
		Result result;
		fixed (char* ptr = name)
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer, ptr, (void*)varValueRef);
		}
		return result;
	}

	internal unsafe IntPtr GetEnumerator()
	{
		IntPtr result = default(IntPtr);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(_nativePointer, &result)).CheckError();
		return result;
	}
}
