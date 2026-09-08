using System.Collections.Generic;
using System.Formats.Nrbf.Utils;
using System.IO;
using System.Reflection.Metadata;

namespace System.Formats.Nrbf;

internal sealed class SystemClassWithMembersAndTypesRecord : ClassRecord
{
	public override SerializationRecordType RecordType => SerializationRecordType.SystemClassWithMembersAndTypes;

	private SystemClassWithMembersAndTypesRecord(ClassInfo classInfo, MemberTypeInfo memberTypeInfo)
		: base(classInfo, memberTypeInfo)
	{
	}

	internal static SerializationRecord Decode(BinaryReader reader, RecordMap recordMap, PayloadOptions options)
	{
		ClassInfo classInfo = System.Formats.Nrbf.ClassInfo.Decode(reader);
		MemberTypeInfo memberTypeInfo = MemberTypeInfo.Decode(reader, classInfo.MemberNames.Count, options, recordMap);
		classInfo.LoadTypeName(options);
		TypeName typeName = classInfo.TypeName;
		if (!classInfo.TypeName.IsSimple || classInfo.MemberNames.Count == 0 || memberTypeInfo.Infos[0].BinaryType != BinaryType.Primitive)
		{
			return new SystemClassWithMembersAndTypesRecord(classInfo, memberTypeInfo);
		}
		if (classInfo.MemberNames.Count == 1)
		{
			PrimitiveType primitiveType = (PrimitiveType)memberTypeInfo.Infos[0].AdditionalInfo;
			Dictionary<string, int>.Enumerator enumerator = classInfo.MemberNames.GetEnumerator();
			enumerator.MoveNext();
			string key = enumerator.Current.Key;
			string fullName = typeName.FullName;
			switch (primitiveType)
			{
			case PrimitiveType.Boolean:
				if (!(fullName == "System.Boolean") || !(key == "m_value"))
				{
					break;
				}
				return Create<bool>(reader.ReadBoolean());
			case PrimitiveType.Byte:
				if (!(fullName == "System.Byte") || !(key == "m_value"))
				{
					break;
				}
				return Create<byte>(reader.ReadByte());
			case PrimitiveType.SByte:
				if (!(fullName == "System.SByte") || !(key == "m_value"))
				{
					break;
				}
				return Create<sbyte>(reader.ReadSByte());
			case PrimitiveType.Char:
				if (!(fullName == "System.Char") || !(key == "m_value"))
				{
					break;
				}
				return Create<char>(reader.ParseChar());
			case PrimitiveType.Int16:
				if (!(fullName == "System.Int16") || !(key == "m_value"))
				{
					break;
				}
				return Create<short>(reader.ReadInt16());
			case PrimitiveType.UInt16:
				if (!(fullName == "System.UInt16") || !(key == "m_value"))
				{
					break;
				}
				return Create<ushort>(reader.ReadUInt16());
			case PrimitiveType.Int32:
				if (!(fullName == "System.Int32") || !(key == "m_value"))
				{
					break;
				}
				return Create<int>(reader.ReadInt32());
			case PrimitiveType.UInt32:
				if (!(fullName == "System.UInt32") || !(key == "m_value"))
				{
					break;
				}
				return Create<uint>(reader.ReadUInt32());
			case PrimitiveType.Int64:
				switch (fullName)
				{
				case "System.Int64":
					if (!(key == "m_value"))
					{
						break;
					}
					return Create<long>(reader.ReadInt64());
				case "System.IntPtr":
					if (!(key == "value"))
					{
						break;
					}
					return Create<IntPtr>(new IntPtr(reader.ReadInt64()));
				case "System.TimeSpan":
					if (!(key == "_ticks"))
					{
						break;
					}
					return Create<TimeSpan>(new TimeSpan(reader.ReadInt64()));
				}
				break;
			case PrimitiveType.UInt64:
				if (!(fullName == "System.UInt64"))
				{
					if (fullName == "System.UIntPtr" && key == "value")
					{
						return Create<UIntPtr>(new UIntPtr(reader.ReadUInt64()));
					}
				}
				else if (key == "m_value")
				{
					return Create<ulong>(reader.ReadUInt64());
				}
				break;
			case PrimitiveType.Single:
				if (!(fullName == "System.Single") || !(key == "m_value"))
				{
					break;
				}
				return Create<float>(reader.ReadSingle());
			case PrimitiveType.Double:
				if (!(fullName == "System.Double") || !(key == "m_value"))
				{
					break;
				}
				return Create<double>(reader.ReadDouble());
			}
			return new SystemClassWithMembersAndTypesRecord(classInfo, memberTypeInfo);
		}
		if (classInfo.MemberNames.Count == 2 && typeName.FullName == "System.DateTime" && HasMember("ticks", 0, PrimitiveType.Int64) && HasMember("dateData", 1, PrimitiveType.UInt64))
		{
			return DecodeDateTime(reader, classInfo.Id);
		}
		if (classInfo.MemberNames.Count == 4 && typeName.FullName == "System.Decimal" && HasMember("flags", 0, PrimitiveType.Int32) && HasMember("hi", 1, PrimitiveType.Int32) && HasMember("lo", 2, PrimitiveType.Int32) && HasMember("mid", 3, PrimitiveType.Int32))
		{
			return DecodeDecimal(reader, classInfo.Id);
		}
		return new SystemClassWithMembersAndTypesRecord(classInfo, memberTypeInfo);
		SerializationRecord Create<T>(T value) where T : unmanaged
		{
			return new MemberPrimitiveTypedRecord<T>(value, classInfo.Id);
		}
		bool HasMember(string name, int order, PrimitiveType primitiveType2)
		{
			if (classInfo.MemberNames.TryGetValue(name, out var value) && value == order)
			{
				return (PrimitiveType)memberTypeInfo.Infos[order].AdditionalInfo == primitiveType2;
			}
			return false;
		}
	}

	internal override (AllowedRecordTypes allowed, PrimitiveType primitiveType) GetNextAllowedRecordType()
	{
		return base.MemberTypeInfo.GetNextAllowedRecordType(base.MemberValues.Count);
	}

	internal static MemberPrimitiveTypedRecord<DateTime> DecodeDateTime(BinaryReader reader, SerializationRecordId id)
	{
		reader.ReadInt64();
		return new MemberPrimitiveTypedRecord<DateTime>(BinaryReaderExtensions.CreateDateTimeFromData(reader.ReadUInt64()), id);
	}

	internal static MemberPrimitiveTypedRecord<decimal> DecodeDecimal(BinaryReader reader, SerializationRecordId id)
	{
		int num = reader.ReadInt32();
		int num2 = reader.ReadInt32();
		int num3 = reader.ReadInt32();
		int num4 = reader.ReadInt32();
		return new MemberPrimitiveTypedRecord<decimal>(new decimal(new int[4] { num3, num4, num2, num }), id);
	}
}
