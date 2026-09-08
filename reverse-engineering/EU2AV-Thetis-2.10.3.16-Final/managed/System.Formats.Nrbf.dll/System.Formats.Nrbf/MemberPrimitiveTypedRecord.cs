namespace System.Formats.Nrbf;

internal sealed class MemberPrimitiveTypedRecord<T> : PrimitiveTypeRecord<T> where T : unmanaged
{
	public override SerializationRecordType RecordType => SerializationRecordType.MemberPrimitiveTyped;

	public override SerializationRecordId Id { get; }

	internal MemberPrimitiveTypedRecord(T value)
		: base(value)
	{
		Id = default(SerializationRecordId);
	}

	internal MemberPrimitiveTypedRecord(T value, SerializationRecordId id)
		: base(value)
	{
		Id = id;
	}
}
