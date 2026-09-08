using System.IO;
using System.Reflection.Metadata;

namespace System.Formats.Nrbf;

internal sealed class MemberReferenceRecord : SerializationRecord
{
	public override SerializationRecordType RecordType => SerializationRecordType.MemberReference;

	internal SerializationRecordId Reference { get; }

	private RecordMap RecordMap { get; }

	public override SerializationRecordId Id => SerializationRecordId.NoId;

	public override TypeName TypeName => GetReferencedRecord().TypeName;

	private MemberReferenceRecord(SerializationRecordId reference, RecordMap recordMap)
	{
		Reference = reference;
		RecordMap = recordMap;
	}

	internal override object GetValue()
	{
		return GetReferencedRecord().GetValue();
	}

	internal static MemberReferenceRecord Decode(BinaryReader reader, RecordMap recordMap)
	{
		return new MemberReferenceRecord(SerializationRecordId.Decode(reader), recordMap);
	}

	internal SerializationRecord GetReferencedRecord()
	{
		return RecordMap.GetRecord(Reference);
	}
}
