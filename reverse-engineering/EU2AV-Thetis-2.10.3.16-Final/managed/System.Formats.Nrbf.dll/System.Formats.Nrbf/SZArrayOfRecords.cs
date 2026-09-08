using System.Collections.Generic;
using System.Formats.Nrbf.Utils;
using System.Reflection.Metadata;

namespace System.Formats.Nrbf;

internal sealed class SZArrayOfRecords : SZArrayRecord<SerializationRecord>
{
	private TypeName _typeName;

	public override SerializationRecordType RecordType => SerializationRecordType.BinaryArray;

	internal List<SerializationRecord> Records { get; }

	private MemberTypeInfo MemberTypeInfo { get; }

	public override TypeName TypeName => _typeName ?? (_typeName = MemberTypeInfo.GetArrayTypeName(base.ArrayInfo));

	internal SZArrayOfRecords(ArrayInfo arrayInfo, MemberTypeInfo memberTypeInfo)
		: base(arrayInfo)
	{
		MemberTypeInfo = memberTypeInfo;
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
		foreach (SerializationRecord record in Records)
		{
			SerializationRecord serializationRecord = ((record is MemberReferenceRecord memberReferenceRecord) ? memberReferenceRecord.GetReferencedRecord() : record);
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

	private protected override void AddValue(object value)
	{
		Records.Add((SerializationRecord)value);
	}

	internal override (AllowedRecordTypes allowed, PrimitiveType primitiveType) GetAllowedRecordType()
	{
		var (allowedRecordTypes, item) = MemberTypeInfo.GetNextAllowedRecordType(0);
		if (allowedRecordTypes != AllowedRecordTypes.None)
		{
			return (allowed: allowedRecordTypes | AllowedRecordTypes.Nulls, primitiveType: item);
		}
		return (allowed: allowedRecordTypes, primitiveType: item);
	}
}
