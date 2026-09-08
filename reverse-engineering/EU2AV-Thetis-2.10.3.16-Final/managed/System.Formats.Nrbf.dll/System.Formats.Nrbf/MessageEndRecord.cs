using System.Reflection.Metadata;

namespace System.Formats.Nrbf;

internal sealed class MessageEndRecord : SerializationRecord
{
	internal static MessageEndRecord Singleton { get; } = new MessageEndRecord();

	public override SerializationRecordType RecordType => SerializationRecordType.MessageEnd;

	public override SerializationRecordId Id => SerializationRecordId.NoId;

	public override TypeName TypeName => TypeName.Parse(MemoryExtensions.AsSpan("MessageEndRecord"));

	private MessageEndRecord()
	{
	}
}
