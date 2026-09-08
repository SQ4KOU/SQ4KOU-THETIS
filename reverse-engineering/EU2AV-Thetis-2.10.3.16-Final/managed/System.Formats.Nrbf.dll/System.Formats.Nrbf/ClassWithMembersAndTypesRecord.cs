using System.IO;

namespace System.Formats.Nrbf;

internal sealed class ClassWithMembersAndTypesRecord : ClassRecord
{
	public override SerializationRecordType RecordType => SerializationRecordType.ClassWithMembersAndTypes;

	private ClassWithMembersAndTypesRecord(ClassInfo classInfo, MemberTypeInfo memberTypeInfo)
		: base(classInfo, memberTypeInfo)
	{
	}

	internal static ClassWithMembersAndTypesRecord Decode(BinaryReader reader, RecordMap recordMap, PayloadOptions options)
	{
		ClassInfo classInfo = System.Formats.Nrbf.ClassInfo.Decode(reader);
		MemberTypeInfo memberTypeInfo = MemberTypeInfo.Decode(reader, classInfo.MemberNames.Count, options, recordMap);
		SerializationRecordId recordId = SerializationRecordId.Decode(reader);
		BinaryLibraryRecord record = recordMap.GetRecord<BinaryLibraryRecord>(recordId);
		classInfo.LoadTypeName(record, options);
		return new ClassWithMembersAndTypesRecord(classInfo, memberTypeInfo);
	}

	internal override (AllowedRecordTypes allowed, PrimitiveType primitiveType) GetNextAllowedRecordType()
	{
		return base.MemberTypeInfo.GetNextAllowedRecordType(base.MemberValues.Count);
	}
}
