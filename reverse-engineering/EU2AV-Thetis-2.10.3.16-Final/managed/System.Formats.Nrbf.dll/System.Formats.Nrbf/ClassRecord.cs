using System.Collections.Generic;
using System.Reflection.Metadata;

namespace System.Formats.Nrbf;

public abstract class ClassRecord : SerializationRecord
{
	public override TypeName TypeName => ClassInfo.TypeName;

	public IEnumerable<string> MemberNames => ClassInfo.MemberNames.Keys;

	public override SerializationRecordId Id => ClassInfo.Id;

	internal ClassInfo ClassInfo { get; }

	internal MemberTypeInfo MemberTypeInfo { get; }

	internal int ExpectedValuesCount => MemberTypeInfo.Infos.Count;

	internal List<object?> MemberValues { get; }

	private protected ClassRecord(ClassInfo classInfo, MemberTypeInfo memberTypeInfo)
	{
		ClassInfo = classInfo;
		MemberTypeInfo = memberTypeInfo;
		MemberValues = new List<object>();
	}

	public bool HasMember(string memberName)
	{
		return ClassInfo.MemberNames.ContainsKey(memberName);
	}

	public ClassRecord? GetClassRecord(string memberName)
	{
		return GetMember<ClassRecord>(memberName);
	}

	public string? GetString(string memberName)
	{
		return GetMember<string>(memberName);
	}

	public bool GetBoolean(string memberName)
	{
		return GetMember<bool>(memberName);
	}

	public byte GetByte(string memberName)
	{
		return GetMember<byte>(memberName);
	}

	public sbyte GetSByte(string memberName)
	{
		return GetMember<sbyte>(memberName);
	}

	public short GetInt16(string memberName)
	{
		return GetMember<short>(memberName);
	}

	public ushort GetUInt16(string memberName)
	{
		return GetMember<ushort>(memberName);
	}

	public char GetChar(string memberName)
	{
		return GetMember<char>(memberName);
	}

	public int GetInt32(string memberName)
	{
		return GetMember<int>(memberName);
	}

	public uint GetUInt32(string memberName)
	{
		return GetMember<uint>(memberName);
	}

	public float GetSingle(string memberName)
	{
		return GetMember<float>(memberName);
	}

	public long GetInt64(string memberName)
	{
		return GetMember<long>(memberName);
	}

	public ulong GetUInt64(string memberName)
	{
		return GetMember<ulong>(memberName);
	}

	public double GetDouble(string memberName)
	{
		return GetMember<double>(memberName);
	}

	public decimal GetDecimal(string memberName)
	{
		return GetMember<decimal>(memberName);
	}

	public TimeSpan GetTimeSpan(string memberName)
	{
		return GetMember<TimeSpan>(memberName);
	}

	public DateTime GetDateTime(string memberName)
	{
		return GetMember<DateTime>(memberName);
	}

	public object? GetRawValue(string memberName)
	{
		return GetMember<object>(memberName);
	}

	public ArrayRecord? GetArrayRecord(string memberName)
	{
		return GetMember<ArrayRecord>(memberName);
	}

	public SerializationRecord? GetSerializationRecord(string memberName)
	{
		object obj = MemberValues[ClassInfo.MemberNames[memberName]];
		if (obj != null && !(obj is NullsRecord))
		{
			if (!(obj is MemberReferenceRecord memberReferenceRecord))
			{
				if (obj is SerializationRecord result)
				{
					return result;
				}
				throw new InvalidOperationException(System.SR.Format(System.SR.Serialization_MemberTypeMismatchException, memberName));
			}
			return memberReferenceRecord.GetReferencedRecord();
		}
		return null;
	}

	private T GetMember<T>(string memberName)
	{
		int index = ClassInfo.MemberNames[memberName];
		object obj = MemberValues[index];
		if (obj is SerializationRecord serializationRecord)
		{
			obj = serializationRecord.GetValue();
		}
		if (obj != null)
		{
			if (obj is T)
			{
				return (T)obj;
			}
			throw new InvalidOperationException(System.SR.Format(System.SR.Serialization_MemberTypeMismatchException, memberName));
		}
		return default(T);
	}

	internal abstract (AllowedRecordTypes allowed, PrimitiveType primitiveType) GetNextAllowedRecordType();

	internal override void HandleNextRecord(SerializationRecord nextRecord, NextInfo info)
	{
		HandleNextValue(nextRecord, info);
	}

	internal override void HandleNextValue(object value, NextInfo info)
	{
		MemberValues.Add(value);
		if (MemberValues.Count < ExpectedValuesCount)
		{
			var (allowed, primitiveType) = GetNextAllowedRecordType();
			info.Stack.Push(info.With(allowed, primitiveType));
		}
	}
}
