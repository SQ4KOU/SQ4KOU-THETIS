using System.Collections.Generic;
using System.Formats.Nrbf;
using System.Runtime.Serialization;

namespace System.Resources.Extensions.BinaryFormat.Deserializer;

internal sealed class ClassRecordSerializationInfoDeserializer : ClassRecordDeserializer
{
	private readonly ClassRecord _classRecord;

	private readonly SerializationInfo _serializationInfo;

	private readonly ISerializationSurrogate _surrogate;

	private readonly IEnumerator<string> _memberNamesIterator;

	private bool _canIterate;

	internal ClassRecordSerializationInfoDeserializer(ClassRecord classRecord, object @object, Type type, ISerializationSurrogate surrogate, IDeserializer deserializer)
		: base(classRecord, @object, deserializer)
	{
		_classRecord = classRecord;
		_surrogate = surrogate;
		_serializationInfo = new SerializationInfo(type, BinaryFormattedObject.DefaultConverter);
		_memberNamesIterator = _classRecord.MemberNames.GetEnumerator();
		_canIterate = _memberNamesIterator.MoveNext();
	}

	internal override SerializationRecordId Continue()
	{
		if (_canIterate)
		{
			do
			{
				string current = _memberNamesIterator.Current;
				var (obj, serializationRecordId) = UnwrapMemberValue(_classRecord.GetRawValue(current));
				if (ObjectRecordDeserializer.s_missingValueSentinel == obj)
				{
					return serializationRecordId;
				}
				if (obj != null && DoesValueNeedUpdated(obj, serializationRecordId))
				{
					base.Deserializer.PendValueUpdater(new SerializationInfoValueUpdater(_classRecord.Id, serializationRecordId, _serializationInfo, current));
				}
				_serializationInfo.AddValue(current, obj);
			}
			while (_memberNamesIterator.MoveNext());
			_canIterate = false;
		}
		PendingSerializationInfo pending = new PendingSerializationInfo(_classRecord.Id, _serializationInfo, _surrogate);
		base.Deserializer.PendSerializationInfo(pending);
		return default(SerializationRecordId);
	}
}
