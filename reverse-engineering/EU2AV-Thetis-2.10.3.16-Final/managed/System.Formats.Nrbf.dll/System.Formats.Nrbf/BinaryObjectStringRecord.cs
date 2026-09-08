using System.Diagnostics;
using System.IO;

namespace System.Formats.Nrbf;

[DebuggerDisplay("{Value}, {Id}")]
internal sealed class BinaryObjectStringRecord : PrimitiveTypeRecord<string>
{
	public override SerializationRecordType RecordType => SerializationRecordType.BinaryObjectString;

	public override SerializationRecordId Id { get; }

	private BinaryObjectStringRecord(SerializationRecordId id, string value)
		: base(value)
	{
		Id = id;
	}

	internal static BinaryObjectStringRecord Decode(BinaryReader reader)
	{
		return new BinaryObjectStringRecord(SerializationRecordId.Decode(reader), reader.ReadString());
	}
}
