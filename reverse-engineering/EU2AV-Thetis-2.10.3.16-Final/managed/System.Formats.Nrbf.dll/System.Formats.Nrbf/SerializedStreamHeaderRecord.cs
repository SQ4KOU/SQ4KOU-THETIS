using System.Formats.Nrbf.Utils;
using System.IO;
using System.Reflection.Metadata;

namespace System.Formats.Nrbf;

internal sealed class SerializedStreamHeaderRecord : SerializationRecord
{
	internal const int Size = 17;

	internal const int MajorVersion = 1;

	internal const int MinorVersion = 0;

	public override SerializationRecordType RecordType => SerializationRecordType.SerializedStreamHeader;

	public override TypeName TypeName => TypeName.Parse(MemoryExtensions.AsSpan("SerializedStreamHeaderRecord"));

	public override SerializationRecordId Id => SerializationRecordId.NoId;

	internal SerializationRecordId RootId { get; }

	internal SerializedStreamHeaderRecord(SerializationRecordId rootId)
	{
		RootId = rootId;
	}

	internal static SerializedStreamHeaderRecord Decode(BinaryReader reader)
	{
		SerializationRecordId rootId = SerializationRecordId.Decode(reader);
		reader.ReadInt32();
		int num = reader.ReadInt32();
		int num2 = reader.ReadInt32();
		if (num != 1)
		{
			ThrowHelper.ThrowInvalidValue(num);
		}
		else if (num2 != 0)
		{
			ThrowHelper.ThrowInvalidValue(num2);
		}
		return new SerializedStreamHeaderRecord(rootId);
	}
}
