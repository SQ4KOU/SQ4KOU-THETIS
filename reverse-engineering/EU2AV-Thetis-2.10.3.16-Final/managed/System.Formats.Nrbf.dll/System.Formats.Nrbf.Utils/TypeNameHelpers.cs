using System.Buffers;
using System.Reflection.Metadata;
using System.Runtime.Serialization;

namespace System.Formats.Nrbf.Utils;

internal static class TypeNameHelpers
{
	internal const PrimitiveType StringPrimitiveType = (PrimitiveType)18;

	internal const PrimitiveType ObjectPrimitiveType = (PrimitiveType)19;

	internal const PrimitiveType IntPtrPrimitiveType = (PrimitiveType)20;

	internal const PrimitiveType UIntPtrPrimitiveType = (PrimitiveType)21;

	private static readonly TypeName[] s_primitiveTypeNames = new TypeName[22];

	private static readonly TypeName[] s_primitiveSZArrayTypeNames = new TypeName[22];

	private static AssemblyNameInfo s_coreLibAssemblyName;

	internal static TypeName GetPrimitiveTypeName(PrimitiveType primitiveType)
	{
		TypeName typeName = s_primitiveTypeNames[(uint)primitiveType];
		if (typeName == null)
		{
			string text = primitiveType switch
			{
				PrimitiveType.Boolean => "System.Boolean", 
				PrimitiveType.Byte => "System.Byte", 
				PrimitiveType.SByte => "System.SByte", 
				PrimitiveType.Char => "System.Char", 
				PrimitiveType.Int16 => "System.Int16", 
				PrimitiveType.UInt16 => "System.UInt16", 
				PrimitiveType.Int32 => "System.Int32", 
				PrimitiveType.UInt32 => "System.UInt32", 
				PrimitiveType.Int64 => "System.Int64", 
				PrimitiveType.UInt64 => "System.UInt64", 
				PrimitiveType.Single => "System.Single", 
				PrimitiveType.Double => "System.Double", 
				PrimitiveType.Decimal => "System.Decimal", 
				PrimitiveType.TimeSpan => "System.TimeSpan", 
				PrimitiveType.DateTime => "System.DateTime", 
				(PrimitiveType)18 => "System.String", 
				(PrimitiveType)19 => "System.Object", 
				(PrimitiveType)20 => "System.IntPtr", 
				(PrimitiveType)21 => "System.UIntPtr", 
				_ => throw new InvalidOperationException(), 
			};
			typeName = (s_primitiveTypeNames[(uint)primitiveType] = TypeName.Parse(MemoryExtensions.AsSpan(text)).WithCoreLibAssemblyName());
		}
		return typeName;
	}

	internal static TypeName GetPrimitiveSZArrayTypeName(PrimitiveType primitiveType)
	{
		TypeName typeName = s_primitiveSZArrayTypeNames[(uint)primitiveType];
		if (typeName == null)
		{
			typeName = (s_primitiveSZArrayTypeNames[(uint)primitiveType] = GetPrimitiveTypeName(primitiveType).MakeSZArrayTypeName());
		}
		return typeName;
	}

	internal static PrimitiveType GetPrimitiveType<T>()
	{
		if (typeof(T) == typeof(bool))
		{
			return PrimitiveType.Boolean;
		}
		if (typeof(T) == typeof(byte))
		{
			return PrimitiveType.Byte;
		}
		if (typeof(T) == typeof(sbyte))
		{
			return PrimitiveType.SByte;
		}
		if (typeof(T) == typeof(char))
		{
			return PrimitiveType.Char;
		}
		if (typeof(T) == typeof(short))
		{
			return PrimitiveType.Int16;
		}
		if (typeof(T) == typeof(ushort))
		{
			return PrimitiveType.UInt16;
		}
		if (typeof(T) == typeof(int))
		{
			return PrimitiveType.Int32;
		}
		if (typeof(T) == typeof(uint))
		{
			return PrimitiveType.UInt32;
		}
		if (typeof(T) == typeof(long))
		{
			return PrimitiveType.Int64;
		}
		if (typeof(T) == typeof(ulong))
		{
			return PrimitiveType.UInt64;
		}
		if (typeof(T) == typeof(float))
		{
			return PrimitiveType.Single;
		}
		if (typeof(T) == typeof(double))
		{
			return PrimitiveType.Double;
		}
		if (typeof(T) == typeof(decimal))
		{
			return PrimitiveType.Decimal;
		}
		if (typeof(T) == typeof(DateTime))
		{
			return PrimitiveType.DateTime;
		}
		if (typeof(T) == typeof(TimeSpan))
		{
			return PrimitiveType.TimeSpan;
		}
		if (typeof(T) == typeof(string))
		{
			return (PrimitiveType)18;
		}
		if (typeof(T) == typeof(IntPtr))
		{
			return (PrimitiveType)20;
		}
		if (typeof(T) == typeof(UIntPtr))
		{
			return (PrimitiveType)21;
		}
		throw new InvalidOperationException();
	}

	internal static TypeName ParseNonSystemClassRecordTypeName(this string rawName, BinaryLibraryRecord libraryRecord, PayloadOptions payloadOptions)
	{
		if (libraryRecord.LibraryName != null)
		{
			return ParseWithoutAssemblyName(rawName, payloadOptions).With(libraryRecord.LibraryName);
		}
		ArraySegment<char> segment = RentAssemblyQualifiedName(rawName, libraryRecord.RawLibraryName);
		TypeName.TryParse(segment.AsSpan(), out TypeName result, payloadOptions.TypeNameParseOptions);
		ArrayPool<char>.Shared.Return(segment.Array);
		if (result == null)
		{
			throw new SerializationException(System.SR.Serialization_InvalidTypeOrAssemblyName);
		}
		if (result.AssemblyName == null)
		{
			return result.WithCoreLibAssemblyName();
		}
		return result;
	}

	internal static TypeName ParseSystemRecordTypeName(this string rawName, PayloadOptions payloadOptions)
	{
		return ParseWithoutAssemblyName(rawName, payloadOptions).WithCoreLibAssemblyName();
	}

	internal static TypeName WithCoreLibAssemblyName(this TypeName systemType)
	{
		return systemType.With(s_coreLibAssemblyName ?? (s_coreLibAssemblyName = AssemblyNameInfo.Parse(MemoryExtensions.AsSpan("mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"))));
	}

	private static TypeName With(this TypeName typeName, AssemblyNameInfo assemblyName)
	{
		if (!typeName.IsSimple)
		{
			if (typeName.IsArray)
			{
				TypeName typeName2 = typeName.GetElementType().With(assemblyName);
				if (!typeName.IsSZArray)
				{
					return typeName2.MakeArrayTypeName(typeName.GetArrayRank());
				}
				return typeName2.MakeSZArrayTypeName();
			}
			if (typeName.IsConstructedGenericType)
			{
				return typeName.GetGenericTypeDefinition().With(assemblyName).MakeGenericTypeName(typeName.GetGenericArguments());
			}
			ThrowHelper.ThrowInvalidTypeName();
		}
		return typeName.WithAssemblyName(assemblyName);
	}

	private static TypeName ParseWithoutAssemblyName(string rawName, PayloadOptions payloadOptions)
	{
		if (!TypeName.TryParse(MemoryExtensions.AsSpan(rawName), out TypeName result, payloadOptions.TypeNameParseOptions) || result.AssemblyName != null)
		{
			throw new SerializationException(System.SR.Format(System.SR.Serialization_InvalidTypeName, rawName));
		}
		return result;
	}

	private static ArraySegment<char> RentAssemblyQualifiedName(string typeName, string libraryName)
	{
		int num = typeName.Length + 1 + libraryName.Length;
		char[] array = ArrayPool<char>.Shared.Rent(num);
		MemoryExtensions.AsSpan(typeName).CopyTo(array);
		array[typeName.Length] = ',';
		MemoryExtensions.AsSpan(libraryName).CopyTo(MemoryExtensions.AsSpan(array, typeName.Length + 1));
		return new ArraySegment<char>(array, 0, num);
	}
}
