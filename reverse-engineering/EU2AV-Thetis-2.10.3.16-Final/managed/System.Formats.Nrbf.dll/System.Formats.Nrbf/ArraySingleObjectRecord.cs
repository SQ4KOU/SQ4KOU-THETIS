using System.Collections.Generic;
using System.Formats.Nrbf.Utils;
using System.IO;
using System.Reflection.Metadata;

namespace System.Formats.Nrbf;

internal sealed class ArraySingleObjectRecord : SZArrayRecord<SerializationRecord>
{
	public override SerializationRecordType RecordType => SerializationRecordType.ArraySingleObject;

	public override TypeName TypeName => TypeNameHelpers.GetPrimitiveSZArrayTypeName((PrimitiveType)19);

	private List<SerializationRecord> Records { get; }

	internal ArraySingleObjectRecord(ArrayInfo arrayInfo)
		: base(arrayInfo)
	{
		Records = new List<SerializationRecord>();
	}

	public override SerializationRecord[] GetArray(bool allowNulls = true)
	{
		return (SerializationRecord[])(allowNulls ? (_arrayNullsAllowed ?? (_arrayNullsAllowed = ToArray(allowNulls: true))) : (_arrayNullsNotAllowed ?? (_arrayNullsNotAllowed = ToArray(allowNulls: false))));
	}

	private SerializationRecord[] ToArray(bool allowNulls)
	{
		SerializationRecord[] array = new SerializationRecord[base.Length];
		int num = 0;
		for (int i = 0; i < Records.Count; i++)
		{
			SerializationRecord serializationRecord = Records[i];
			if (serializationRecord is MemberReferenceRecord memberReferenceRecord)
			{
				serializationRecord = memberReferenceRecord.GetReferencedRecord();
			}
			if (!(serializationRecord is NullsRecord nullsRecord))
			{
				array[num++] = serializationRecord;
				continue;
			}
			if (!allowNulls)
			{
				ThrowHelper.ThrowArrayContainedNulls();
			}
			int num2 = nullsRecord.NullCount;
			do
			{
				array[num++] = null;
				num2--;
			}
			while (num2 > 0);
		}
		return array;
	}

	internal static ArraySingleObjectRecord Decode(BinaryReader reader)
	{
		return new ArraySingleObjectRecord(ArrayInfo.Decode(reader));
	}

	internal override (AllowedRecordTypes allowed, PrimitiveType primitiveType) GetAllowedRecordType()
	{
		return (allowed: AllowedRecordTypes.AnyObject | AllowedRecordTypes.ObjectNullMultiple256 | AllowedRecordTypes.ObjectNullMultiple, primitiveType: (PrimitiveType)0);
	}

	private protected override void AddValue(object value)
	{
		Records.Add((SerializationRecord)value);
	}
}
