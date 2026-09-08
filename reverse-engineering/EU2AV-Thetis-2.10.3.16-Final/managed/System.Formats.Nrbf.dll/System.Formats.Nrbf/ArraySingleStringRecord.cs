using System.Collections.Generic;
using System.Formats.Nrbf.Utils;
using System.IO;
using System.Reflection.Metadata;

namespace System.Formats.Nrbf;

internal sealed class ArraySingleStringRecord : SZArrayRecord<string>
{
	public override SerializationRecordType RecordType => SerializationRecordType.ArraySingleString;

	public override TypeName TypeName => TypeNameHelpers.GetPrimitiveSZArrayTypeName((PrimitiveType)18);

	private List<SerializationRecord> Records { get; }

	internal ArraySingleStringRecord(ArrayInfo arrayInfo)
		: base(arrayInfo)
	{
		Records = new List<SerializationRecord>();
	}

	internal static ArraySingleStringRecord Decode(BinaryReader reader)
	{
		return new ArraySingleStringRecord(ArrayInfo.Decode(reader));
	}

	internal override (AllowedRecordTypes allowed, PrimitiveType primitiveType) GetAllowedRecordType()
	{
		return (allowed: AllowedRecordTypes.Nulls | AllowedRecordTypes.BinaryObjectString | AllowedRecordTypes.MemberReference, primitiveType: (PrimitiveType)0);
	}

	private protected override void AddValue(object value)
	{
		Records.Add((SerializationRecord)value);
	}

	public override string[] GetArray(bool allowNulls = true)
	{
		return (string[])(allowNulls ? (_arrayNullsAllowed ?? (_arrayNullsAllowed = ToArray(allowNulls: true))) : (_arrayNullsNotAllowed ?? (_arrayNullsNotAllowed = ToArray(allowNulls: false))));
	}

	private string[] ToArray(bool allowNulls)
	{
		string[] array = new string[base.Length];
		int num = 0;
		for (int i = 0; i < Records.Count; i++)
		{
			SerializationRecord serializationRecord = Records[i];
			if (serializationRecord is MemberReferenceRecord memberReferenceRecord)
			{
				serializationRecord = memberReferenceRecord.GetReferencedRecord();
				if (!(serializationRecord is BinaryObjectStringRecord))
				{
					ThrowHelper.ThrowInvalidReference();
				}
			}
			if (serializationRecord is BinaryObjectStringRecord binaryObjectStringRecord)
			{
				array[num++] = binaryObjectStringRecord.Value;
				continue;
			}
			if (!allowNulls)
			{
				ThrowHelper.ThrowArrayContainedNulls();
			}
			int num2 = ((NullsRecord)serializationRecord).NullCount;
			do
			{
				array[num++] = null;
				num2--;
			}
			while (num2 > 0);
		}
		return array;
	}
}
