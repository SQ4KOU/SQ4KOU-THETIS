using System.Collections.Generic;
using System.Formats.Nrbf;
using System.Runtime.Serialization;

namespace System.Resources.Extensions.BinaryFormat.Deserializer;

internal interface IDeserializer
{
	BinaryFormattedObject.Options Options { get; }

	HashSet<SerializationRecordId> IncompleteObjects { get; }

	IDictionary<SerializationRecordId, object> DeserializedObjects { get; }

	BinaryFormattedObject.ITypeResolver TypeResolver { get; }

	void PendValueUpdater(ValueUpdater updater);

	void PendSerializationInfo(PendingSerializationInfo pending);

	void CompleteObject(SerializationRecordId id);

	ISerializationSurrogate GetSurrogate(Type type);
}
