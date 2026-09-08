using System.Collections.Generic;
using System.Formats.Nrbf;

namespace System.Resources.Extensions.BinaryFormat.Deserializer;

internal sealed class ArrayUpdater : ValueUpdater
{
	private readonly int[] _indices;

	internal ArrayUpdater(SerializationRecordId objectId, SerializationRecordId valueId, int[] indices)
		: base(objectId, valueId)
	{
		_indices = indices;
	}

	internal override void UpdateValue(IDictionary<SerializationRecordId, object> objects)
	{
		object value = objects[base.ValueId];
		((Array)objects[base.ObjectId]).SetValue(value, _indices);
	}
}
