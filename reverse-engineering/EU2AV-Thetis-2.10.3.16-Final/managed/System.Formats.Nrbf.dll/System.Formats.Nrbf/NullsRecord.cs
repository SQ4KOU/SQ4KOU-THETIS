using System.Reflection.Metadata;

namespace System.Formats.Nrbf;

internal abstract class NullsRecord : SerializationRecord
{
	internal abstract int NullCount { get; }

	public override SerializationRecordId Id => SerializationRecordId.NoId;

	public override TypeName TypeName => TypeName.Parse(MemoryExtensions.AsSpan(GetType().Name));
}
