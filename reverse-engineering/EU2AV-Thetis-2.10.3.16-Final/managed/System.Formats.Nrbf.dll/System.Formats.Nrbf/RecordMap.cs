using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace System.Formats.Nrbf;

internal sealed class RecordMap : IReadOnlyDictionary<SerializationRecordId, SerializationRecord>, IReadOnlyCollection<KeyValuePair<SerializationRecordId, SerializationRecord>>, IEnumerable<KeyValuePair<SerializationRecordId, SerializationRecord>>, IEnumerable
{
	private readonly Dictionary<SerializationRecordId, SerializationRecord> _map = new Dictionary<SerializationRecordId, SerializationRecord>();

	public IEnumerable<SerializationRecordId> Keys => _map.Keys;

	public IEnumerable<SerializationRecord> Values => _map.Values;

	public int Count => _map.Count;

	public SerializationRecord this[SerializationRecordId objectId] => _map[objectId];

	public bool ContainsKey(SerializationRecordId key)
	{
		return _map.ContainsKey(key);
	}

	public bool TryGetValue(SerializationRecordId key, [MaybeNullWhen(false)] out SerializationRecord value)
	{
		return _map.TryGetValue(key, out value);
	}

	public IEnumerator<KeyValuePair<SerializationRecordId, SerializationRecord>> GetEnumerator()
	{
		return _map.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return _map.GetEnumerator();
	}

	internal void Add(SerializationRecord record)
	{
		if (record.Id.Equals(SerializationRecordId.NoId))
		{
			return;
		}
		if (record.Id._id < 0)
		{
			_map[record.Id] = record;
			return;
		}
		if (!_map.ContainsKey(record.Id))
		{
			_map.Add(record.Id, record);
			return;
		}
		throw new SerializationException(System.SR.Format(System.SR.Serialization_DuplicateSerializationRecordId, record.Id._id));
	}

	internal SerializationRecord GetRootRecord(SerializedStreamHeaderRecord header)
	{
		return GetRecord(header.RootId);
	}

	internal SerializationRecord GetRecord(SerializationRecordId recordId)
	{
		if (!_map.TryGetValue(recordId, out var value))
		{
			throw new SerializationException(System.SR.Serialization_InvalidReference);
		}
		return value;
	}

	internal T GetRecord<T>(SerializationRecordId recordId) where T : SerializationRecord
	{
		if (!_map.TryGetValue(recordId, out var value) || !(value is T result))
		{
			throw new SerializationException(System.SR.Serialization_InvalidReference);
		}
		return result;
	}
}
