using System.Collections.Generic;
using System.Formats.Nrbf;
using System.Reflection;

namespace System.Resources.Extensions.BinaryFormat.Deserializer;

internal sealed class FieldValueUpdater : ValueUpdater
{
	private readonly FieldInfo _field;

	internal FieldValueUpdater(SerializationRecordId objectId, SerializationRecordId valueId, FieldInfo field)
		: base(objectId, valueId)
	{
		_field = field;
	}

	internal override void UpdateValue(IDictionary<SerializationRecordId, object> objects)
	{
		object value = objects[base.ValueId];
		_field.SetValue(objects[base.ObjectId], value);
	}
}
