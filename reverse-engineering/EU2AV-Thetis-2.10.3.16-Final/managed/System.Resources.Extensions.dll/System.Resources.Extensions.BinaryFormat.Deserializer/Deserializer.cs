using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Formats.Nrbf;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace System.Resources.Extensions.BinaryFormat.Deserializer;

internal sealed class Deserializer : IDeserializer
{
	private readonly IReadOnlyDictionary<SerializationRecordId, SerializationRecord> _recordMap;

	private readonly BinaryFormattedObject.ITypeResolver _typeResolver;

	private readonly Dictionary<SerializationRecordId, object> _deserializedObjects = new Dictionary<SerializationRecordId, object>();

	private readonly Dictionary<Type, ISerializationSurrogate> _surrogates;

	private Queue<PendingSerializationInfo> _pendingSerializationInfo;

	private HashSet<SerializationRecordId> _pendingSerializationInfoIds;

	private readonly Stack<ObjectRecordDeserializer> _parserStack = new Stack<ObjectRecordDeserializer>();

	private readonly HashSet<SerializationRecordId> _incompleteObjects = new HashSet<SerializationRecordId>();

	private Dictionary<SerializationRecordId, HashSet<SerializationRecordId>> _incompleteDependencies;

	private HashSet<ValueUpdater> _pendingUpdates;

	private readonly Queue<SerializationRecordId> _pendingCompletions = new Queue<SerializationRecordId>();

	private readonly SerializationRecordId _rootId;

	BinaryFormattedObject.ITypeResolver IDeserializer.TypeResolver => _typeResolver;

	private BinaryFormattedObject.Options Options { get; }

	BinaryFormattedObject.Options IDeserializer.Options => Options;

	IDictionary<SerializationRecordId, object> IDeserializer.DeserializedObjects => _deserializedObjects;

	public HashSet<SerializationRecordId> IncompleteObjects => _incompleteObjects;

	private event Action<object> OnDeserialization;

	private event Action<StreamingContext> OnDeserialized;

	private Deserializer(SerializationRecordId rootId, IReadOnlyDictionary<SerializationRecordId, SerializationRecord> recordMap, BinaryFormattedObject.ITypeResolver typeResolver, BinaryFormattedObject.Options options)
	{
		_rootId = rootId;
		_recordMap = recordMap;
		_typeResolver = typeResolver;
		Options = options;
		if (Options.SurrogateSelector != null)
		{
			_surrogates = new Dictionary<Type, ISerializationSurrogate>();
		}
	}

	[RequiresUnreferencedCode("Calls System.Windows.Forms.BinaryFormat.Deserializer.Deserializer.Deserialize()")]
	internal static object Deserialize(SerializationRecordId rootId, IReadOnlyDictionary<SerializationRecordId, SerializationRecord> recordMap, BinaryFormattedObject.ITypeResolver typeResolver, BinaryFormattedObject.Options options)
	{
		return new Deserializer(rootId, recordMap, typeResolver, options).Deserialize();
	}

	[RequiresUnreferencedCode("Calls System.Windows.Forms.BinaryFormat.Deserializer.Deserializer.DeserializeRoot(SerializationRecordId)")]
	private object Deserialize()
	{
		DeserializeRoot(_rootId);
		int num = _pendingSerializationInfo?.Count ?? 0;
		while (_pendingSerializationInfo != null && _pendingSerializationInfo.Count > 0)
		{
			PendingSerializationInfo pendingSerializationInfo = _pendingSerializationInfo.Dequeue();
			if (--num >= 0 && _pendingSerializationInfo.Count != 0 && _incompleteDependencies != null && _incompleteDependencies.TryGetValue(pendingSerializationInfo.ObjectId, out var value) && value.Count > 0)
			{
				_pendingSerializationInfo.Enqueue(pendingSerializationInfo);
				continue;
			}
			pendingSerializationInfo.Populate(_deserializedObjects, Options.StreamingContext);
			_pendingSerializationInfoIds?.Remove(pendingSerializationInfo.ObjectId);
			((IDeserializer)this).CompleteObject(pendingSerializationInfo.ObjectId);
		}
		if (_incompleteObjects.Count > 0 || (_pendingUpdates != null && _pendingUpdates.Count > 0))
		{
			throw new SerializationException(System.SR.Serialization_Incomplete);
		}
		OnDeserialized?.Invoke(Options.StreamingContext);
		OnDeserialization?.Invoke(null);
		return _deserializedObjects[_rootId];
	}

	[RequiresUnreferencedCode("Calls DeserializeNew(SerializationRecordId)")]
	private void DeserializeRoot(SerializationRecordId rootId)
	{
		if (!(DeserializeNew(rootId) is ObjectRecordDeserializer item))
		{
			return;
		}
		_parserStack.Push(item);
		while (_parserStack.Count > 0)
		{
			ObjectRecordDeserializer objectRecordDeserializer = _parserStack.Pop();
			while (true)
			{
				SerializationRecordId id;
				SerializationRecordId serializationRecordId = (id = objectRecordDeserializer.Continue());
				if (serializationRecordId.Equals(default(SerializationRecordId)))
				{
					break;
				}
				if (DeserializeNew(id) is ObjectRecordDeserializer item2)
				{
					_parserStack.Push(objectRecordDeserializer);
					_parserStack.Push(item2);
					break;
				}
			}
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[RequiresUnreferencedCode("Calls System.Windows.Forms.BinaryFormat.Deserializer.ObjectRecordParser.Create(SerializationRecordId, IRecord, IDeserializer)")]
		object DeserializeNew(SerializationRecordId serializationRecordId2)
		{
			SerializationRecord serializationRecord = _recordMap[serializationRecordId2];
			object obj = serializationRecord.RecordType switch
			{
				SerializationRecordType.BinaryObjectString => ((PrimitiveTypeRecord<string>)serializationRecord).Value, 
				SerializationRecordType.MemberPrimitiveTyped => ((PrimitiveTypeRecord)serializationRecord).Value, 
				SerializationRecordType.ArraySingleString => ((SZArrayRecord<string>)serializationRecord).GetArray(), 
				SerializationRecordType.ArraySinglePrimitive => ArrayRecordDeserializer.GetArraySinglePrimitive(serializationRecord), 
				SerializationRecordType.BinaryArray => ArrayRecordDeserializer.GetRectangularArrayOfPrimitives((ArrayRecord)serializationRecord, _typeResolver), 
				_ => null, 
			};
			if (obj != null)
			{
				_deserializedObjects.Add(serializationRecord.Id, obj);
				return obj;
			}
			if (!_incompleteObjects.Add(serializationRecordId2))
			{
				throw new SerializationException(System.SR.Serialization_Cycle);
			}
			ObjectRecordDeserializer objectRecordDeserializer2 = ObjectRecordDeserializer.Create(serializationRecord, this);
			_deserializedObjects.Add(serializationRecordId2, objectRecordDeserializer2.Object);
			return objectRecordDeserializer2;
		}
	}

	ISerializationSurrogate IDeserializer.GetSurrogate(Type type)
	{
		if (_surrogates == null)
		{
			return null;
		}
		if (!_surrogates.TryGetValue(type, out var value))
		{
			value = Options.SurrogateSelector.GetSurrogate(type, Options.StreamingContext, out var _);
			_surrogates[type] = value;
		}
		return value;
	}

	void IDeserializer.PendSerializationInfo(PendingSerializationInfo pending)
	{
		if (_pendingSerializationInfo == null)
		{
			_pendingSerializationInfo = new Queue<PendingSerializationInfo>();
		}
		_pendingSerializationInfo.Enqueue(pending);
		if (_pendingSerializationInfoIds == null)
		{
			_pendingSerializationInfoIds = new HashSet<SerializationRecordId>();
		}
		_pendingSerializationInfoIds.Add(pending.ObjectId);
	}

	void IDeserializer.PendValueUpdater(ValueUpdater updater)
	{
		if (_pendingUpdates == null)
		{
			_pendingUpdates = new HashSet<ValueUpdater>();
		}
		_pendingUpdates.Add(updater);
		if (_incompleteDependencies == null)
		{
			_incompleteDependencies = new Dictionary<SerializationRecordId, HashSet<SerializationRecordId>>();
		}
		if (_incompleteDependencies.TryGetValue(updater.ObjectId, out var value))
		{
			value.Add(updater.ValueId);
			return;
		}
		_incompleteDependencies.Add(updater.ObjectId, new HashSet<SerializationRecordId> { updater.ValueId });
	}

	[UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "The type is already in the cache of the TypeResolver, no need to mark this one again.")]
	void IDeserializer.CompleteObject(SerializationRecordId id)
	{
		_pendingCompletions.Enqueue(id);
		SerializationRecordId serializationRecordId = default(SerializationRecordId);
		while (_pendingCompletions.Count > 0)
		{
			SerializationRecordId completedId = _pendingCompletions.Dequeue();
			_incompleteObjects.Remove(completedId);
			if (!serializationRecordId.Equals(default(SerializationRecordId)))
			{
				_incompleteDependencies?.Remove(serializationRecordId);
				if (_pendingSerializationInfoIds != null && _pendingSerializationInfoIds.Contains(serializationRecordId))
				{
					continue;
				}
				serializationRecordId = default(SerializationRecordId);
			}
			if (_recordMap[completedId] is ClassRecord classRecord && (_incompleteDependencies == null || !_incompleteDependencies.ContainsKey(completedId)))
			{
				Type type = _typeResolver.GetType(classRecord.TypeName);
				object obj = _deserializedObjects[completedId];
				OnDeserialized += SerializationEvents.GetOnDeserializedForType(type, obj);
				if (obj is IDeserializationCallback deserializationCallback)
				{
					OnDeserialization += deserializationCallback.OnDeserialization;
				}
				if (obj is IObjectReference objectReference)
				{
					_deserializedObjects[completedId] = objectReference.GetRealObject(Options.StreamingContext);
				}
			}
			if (_incompleteDependencies == null)
			{
				continue;
			}
			foreach (KeyValuePair<SerializationRecordId, HashSet<SerializationRecordId>> incompleteDependency in _incompleteDependencies)
			{
				SerializationRecordId key = incompleteDependency.Key;
				HashSet<SerializationRecordId> value = incompleteDependency.Value;
				if (!value.Remove(completedId))
				{
					continue;
				}
				_pendingUpdates.RemoveWhere(delegate(ValueUpdater updater)
				{
					if (!updater.ValueId.Equals(completedId))
					{
						return false;
					}
					updater.UpdateValue(_deserializedObjects);
					return true;
				});
				if (value.Count == 0)
				{
					serializationRecordId = key;
					_pendingCompletions.Enqueue(key);
				}
			}
		}
	}
}
