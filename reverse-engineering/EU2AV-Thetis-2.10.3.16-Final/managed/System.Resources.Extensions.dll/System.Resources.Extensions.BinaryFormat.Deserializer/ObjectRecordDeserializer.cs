using System.Diagnostics.CodeAnalysis;
using System.Formats.Nrbf;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace System.Resources.Extensions.BinaryFormat.Deserializer;

internal abstract class ObjectRecordDeserializer
{
	private protected static object s_missingValueSentinel = new object();

	internal SerializationRecord ObjectRecord { get; }

	internal object Object
	{
		get; [param: AllowNull]
		private protected set;
	}

	private protected IDeserializer Deserializer { get; }

	private protected ObjectRecordDeserializer(SerializationRecord objectRecord, IDeserializer deserializer)
	{
		Deserializer = deserializer;
		ObjectRecord = objectRecord;
	}

	internal abstract SerializationRecordId Continue();

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private protected (object value, SerializationRecordId id) UnwrapMemberValue(object memberValue)
	{
		if (memberValue == null)
		{
			return (value: null, id: default(SerializationRecordId));
		}
		if (!(memberValue is SerializationRecord serializationRecord))
		{
			return (value: memberValue, id: default(SerializationRecordId));
		}
		if (serializationRecord.RecordType == SerializationRecordType.BinaryObjectString)
		{
			PrimitiveTypeRecord<string> primitiveTypeRecord = (PrimitiveTypeRecord<string>)serializationRecord;
			return (value: primitiveTypeRecord.Value, id: primitiveTypeRecord.Id);
		}
		if (serializationRecord.RecordType == SerializationRecordType.MemberPrimitiveTyped)
		{
			return (value: ((PrimitiveTypeRecord)serializationRecord).Value, id: default(SerializationRecordId));
		}
		return TryGetObject(serializationRecord.Id);
		(object value, SerializationRecordId id) TryGetObject(SerializationRecordId id)
		{
			if (!Deserializer.DeserializedObjects.TryGetValue(id, out var value))
			{
				return (value: s_missingValueSentinel, id: id);
			}
			ValidateNewMemberObjectValue(value);
			return (value: value, id: id);
		}
	}

	private protected virtual void ValidateNewMemberObjectValue(object value)
	{
	}

	private protected bool DoesValueNeedUpdated(object value, SerializationRecordId valueRecord)
	{
		if (!valueRecord.Equals(default(SerializationRecordId)))
		{
			if (!(value is IObjectReference))
			{
				if (Deserializer.IncompleteObjects.Contains(valueRecord))
				{
					return value.GetType().IsValueType;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	[RequiresUnreferencedCode("Calls System.Windows.Forms.BinaryFormat.Deserializer.ClassRecordParser.Create(ClassRecord, IDeserializer)")]
	internal static ObjectRecordDeserializer Create(SerializationRecord record, IDeserializer deserializer)
	{
		if (record is ClassRecord classRecord)
		{
			return ClassRecordDeserializer.Create(classRecord, deserializer);
		}
		return new ArrayRecordDeserializer((ArrayRecord)record, deserializer);
	}
}
