using System.Collections.Generic;
using System.Formats.Nrbf;

namespace System.Resources.Extensions.BinaryFormat.Deserializer;

internal abstract class ValueUpdater
{
	internal SerializationRecordId ValueId { get; }

	internal SerializationRecordId ObjectId { get; }

	private protected ValueUpdater(SerializationRecordId objectId, SerializationRecordId valueId)
	{
		ObjectId = objectId;
		ValueId = valueId;
	}

	internal abstract void UpdateValue(IDictionary<SerializationRecordId, object> objects);
}
