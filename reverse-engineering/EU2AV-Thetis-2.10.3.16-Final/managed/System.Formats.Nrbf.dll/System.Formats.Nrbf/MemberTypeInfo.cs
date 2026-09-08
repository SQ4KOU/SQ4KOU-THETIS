using System.Collections.Generic;
using System.Formats.Nrbf.Utils;
using System.IO;
using System.Reflection.Metadata;

namespace System.Formats.Nrbf;

internal readonly struct MemberTypeInfo
{
	private readonly IReadOnlyList<(BinaryType BinaryType, object AdditionalInfo)> _infos;

	internal IReadOnlyList<(BinaryType BinaryType, object AdditionalInfo)> Infos => _infos;

	internal MemberTypeInfo(IReadOnlyList<(BinaryType BinaryType, object AdditionalInfo)> infos)
	{
		_infos = infos;
	}

	internal static MemberTypeInfo Decode(BinaryReader reader, int count, PayloadOptions options, RecordMap recordMap)
	{
		List<(BinaryType, object)> list = new List<(BinaryType, object)>();
		for (int i = 0; i < count; i++)
		{
			list.Add((reader.ReadBinaryType(), null));
		}
		for (int j = 0; j < list.Count; j++)
		{
			BinaryType item = list[j].Item1;
			switch (item)
			{
			case BinaryType.Primitive:
			case BinaryType.PrimitiveArray:
				list[j] = (item, reader.ReadPrimitiveType());
				break;
			case BinaryType.SystemClass:
				list[j] = (item, reader.ReadString().ParseSystemRecordTypeName(options));
				break;
			case BinaryType.Class:
				list[j] = (item, ClassTypeInfo.Decode(reader, options, recordMap));
				break;
			default:
				throw new InvalidOperationException();
			case BinaryType.String:
			case BinaryType.Object:
			case BinaryType.ObjectArray:
			case BinaryType.StringArray:
				break;
			}
		}
		return new MemberTypeInfo(list);
	}

	internal (AllowedRecordTypes allowed, PrimitiveType primitiveType) GetNextAllowedRecordType(int currentValuesCount)
	{
		var (binaryType, obj) = Infos[currentValuesCount];
		return binaryType switch
		{
			BinaryType.Primitive => (allowed: AllowedRecordTypes.None, primitiveType: (PrimitiveType)obj), 
			BinaryType.String => (allowed: AllowedRecordTypes.BinaryObjectString | AllowedRecordTypes.MemberReference | AllowedRecordTypes.ObjectNull, primitiveType: (PrimitiveType)0), 
			BinaryType.Object => (allowed: AllowedRecordTypes.AnyObject, primitiveType: (PrimitiveType)0), 
			BinaryType.StringArray => (allowed: AllowedRecordTypes.MemberReference | AllowedRecordTypes.ObjectNull | AllowedRecordTypes.ArraySingleString, primitiveType: (PrimitiveType)0), 
			BinaryType.PrimitiveArray => (allowed: AllowedRecordTypes.MemberReference | AllowedRecordTypes.ObjectNull | AllowedRecordTypes.ArraySinglePrimitive, primitiveType: (PrimitiveType)0), 
			BinaryType.Class => (allowed: AllowedRecordTypes.ClassWithId | AllowedRecordTypes.ClassWithMembersAndTypes | AllowedRecordTypes.MemberReference | AllowedRecordTypes.ObjectNull | AllowedRecordTypes.BinaryLibrary, primitiveType: (PrimitiveType)0), 
			BinaryType.SystemClass => (allowed: AllowedRecordTypes.ClassWithId | AllowedRecordTypes.SystemClassWithMembersAndTypes | AllowedRecordTypes.ClassWithMembersAndTypes | AllowedRecordTypes.BinaryObjectString | AllowedRecordTypes.MemberPrimitiveTyped | AllowedRecordTypes.MemberReference | AllowedRecordTypes.ObjectNull | AllowedRecordTypes.BinaryLibrary, primitiveType: (PrimitiveType)0), 
			BinaryType.ObjectArray => (allowed: AllowedRecordTypes.MemberReference | AllowedRecordTypes.ObjectNull | AllowedRecordTypes.ArraySingleObject, primitiveType: (PrimitiveType)0), 
			_ => throw new InvalidOperationException(), 
		};
	}

	internal TypeName GetArrayTypeName(ArrayInfo arrayInfo)
	{
		(BinaryType BinaryType, object AdditionalInfo) tuple = Infos[0];
		BinaryType item = tuple.BinaryType;
		object item2 = tuple.AdditionalInfo;
		TypeName typeName = item switch
		{
			BinaryType.String => TypeNameHelpers.GetPrimitiveTypeName((PrimitiveType)18), 
			BinaryType.StringArray => TypeNameHelpers.GetPrimitiveSZArrayTypeName((PrimitiveType)18), 
			BinaryType.Primitive => TypeNameHelpers.GetPrimitiveTypeName((PrimitiveType)item2), 
			BinaryType.PrimitiveArray => TypeNameHelpers.GetPrimitiveSZArrayTypeName((PrimitiveType)item2), 
			BinaryType.Object => TypeNameHelpers.GetPrimitiveTypeName((PrimitiveType)19), 
			BinaryType.ObjectArray => TypeNameHelpers.GetPrimitiveSZArrayTypeName((PrimitiveType)19), 
			BinaryType.SystemClass => (TypeName)item2, 
			BinaryType.Class => ((ClassTypeInfo)item2).TypeName, 
			_ => throw new InvalidOperationException(), 
		};
		if (arrayInfo.Rank != 1)
		{
			return typeName.MakeArrayTypeName(arrayInfo.Rank);
		}
		return typeName.MakeSZArrayTypeName();
	}
}
