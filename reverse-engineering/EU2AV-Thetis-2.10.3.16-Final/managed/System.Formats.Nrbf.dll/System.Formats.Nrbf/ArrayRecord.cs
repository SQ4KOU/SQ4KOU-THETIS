using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Formats.Nrbf.Utils;
using System.Runtime.Serialization;

namespace System.Formats.Nrbf;

public abstract class ArrayRecord : SerializationRecord
{
	private protected Array _arrayNullsAllowed;

	private protected Array _arrayNullsNotAllowed;

	public abstract ReadOnlySpan<int> Lengths { get; }

	public int Rank => ArrayInfo.Rank;

	internal BinaryArrayType ArrayType => ArrayInfo.ArrayType;

	public override SerializationRecordId Id => ArrayInfo.Id;

	internal long ValuesToRead { get; private protected set; }

	internal ArrayInfo ArrayInfo { get; }

	internal bool IsJagged
	{
		get
		{
			if (ArrayInfo.ArrayType != BinaryArrayType.Jagged)
			{
				return TypeName.GetElementType().IsArray;
			}
			return true;
		}
	}

	private protected ArrayRecord(ArrayInfo arrayInfo)
	{
		ArrayInfo = arrayInfo;
		ValuesToRead = arrayInfo.FlattenedLength;
	}

	[RequiresDynamicCode("The code for an array of the specified type might not be available.")]
	public Array GetArray(Type expectedArrayType, bool allowNulls = true)
	{
		System.ExceptionPolyfills.ThrowIfNull(expectedArrayType, "expectedArrayType");
		if (!TypeNameMatches(expectedArrayType))
		{
			throw new InvalidOperationException(System.SR.Format(System.SR.Serialization_TypeMismatch, expectedArrayType.AssemblyQualifiedName, TypeName.AssemblyQualifiedName));
		}
		object obj;
		if (!allowNulls)
		{
			obj = _arrayNullsNotAllowed;
			if (obj == null)
			{
				return _arrayNullsNotAllowed = Deserialize(expectedArrayType, allowNulls: false);
			}
		}
		else
		{
			obj = _arrayNullsAllowed ?? (_arrayNullsAllowed = Deserialize(expectedArrayType, allowNulls: true));
		}
		return (Array)obj;
	}

	[RequiresDynamicCode("May call Array.CreateInstance() and Type.MakeArrayType().")]
	private protected abstract Array Deserialize(Type arrayType, bool allowNulls);

	internal sealed override void HandleNextValue(object value, NextInfo info)
	{
		HandleNext(value, info, 1);
	}

	internal sealed override void HandleNextRecord(SerializationRecord nextRecord, NextInfo info)
	{
		HandleNext(nextRecord, info, (!(nextRecord is NullsRecord nullsRecord)) ? 1 : nullsRecord.NullCount);
	}

	private protected abstract void AddValue(object value);

	private void HandleNext(object value, NextInfo info, int size)
	{
		ValuesToRead -= size;
		if (ValuesToRead < 0)
		{
			ThrowHelper.ThrowUnexpectedNullRecordCount();
		}
		else if (ValuesToRead > 0)
		{
			info.Stack.Push(info);
		}
		AddValue(value);
	}

	internal abstract (AllowedRecordTypes allowed, PrimitiveType primitiveType) GetAllowedRecordType();

	internal static void Populate(List<SerializationRecord> source, Array destination, int[] lengths, AllowedRecordTypes allowedRecordTypes, bool allowNulls)
	{
		int[] array = new int[lengths.Length];
		nuint num = 0u;
		foreach (SerializationRecord item in source)
		{
			object actualValue = GetActualValue(item, allowedRecordTypes, out var repeatCount);
			if (actualValue != null)
			{
				destination.SetValue(actualValue, array);
			}
			else if (!allowNulls)
			{
				ThrowHelper.ThrowArrayContainedNulls();
			}
			while (repeatCount > 0)
			{
				repeatCount--;
				num++;
				int num2;
				for (num2 = array.Length - 1; num2 >= 0; num2--)
				{
					array[num2]++;
					if (array[num2] < lengths[num2])
					{
						break;
					}
					array[num2] = 0;
				}
				if (num2 < 0)
				{
					break;
				}
			}
		}
	}

	private static object GetActualValue(SerializationRecord record, AllowedRecordTypes allowedRecordTypes, out int repeatCount)
	{
		repeatCount = 1;
		if (record is NullsRecord nullsRecord)
		{
			repeatCount = nullsRecord.NullCount;
			return null;
		}
		if (record.RecordType == SerializationRecordType.MemberReference)
		{
			record = ((MemberReferenceRecord)record).GetReferencedRecord();
		}
		return allowedRecordTypes switch
		{
			AllowedRecordTypes.BinaryObjectString => ((record as BinaryObjectStringRecord) ?? throw new SerializationException(System.SR.Serialization_InvalidReference)).Value, 
			AllowedRecordTypes.Arrays => (record as ArrayRecord) ?? throw new SerializationException(System.SR.Serialization_InvalidReference), 
			_ => record, 
		};
	}
}
