using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection.Metadata;

namespace System.Formats.Nrbf;

internal sealed class JaggedArrayRecord : ArrayRecord
{
	private readonly MemberTypeInfo _memberTypeInfo;

	private readonly int[] _lengths;

	private readonly List<SerializationRecord> _records;

	private readonly AllowedRecordTypes _allowedRecordTypes;

	private TypeName _typeName;

	public override SerializationRecordType RecordType => SerializationRecordType.BinaryArray;

	public override ReadOnlySpan<int> Lengths => _lengths;

	public override TypeName TypeName => _typeName ?? (_typeName = _memberTypeInfo.GetArrayTypeName(base.ArrayInfo));

	internal JaggedArrayRecord(ArrayInfo arrayInfo, MemberTypeInfo memberTypeInfo, int[] lengths)
		: base(arrayInfo)
	{
		_memberTypeInfo = memberTypeInfo;
		_lengths = lengths;
		_records = new List<SerializationRecord>();
		_allowedRecordTypes = memberTypeInfo.GetNextAllowedRecordType(0).allowed;
	}

	[RequiresDynamicCode("May call Array.CreateInstance().")]
	private protected override Array Deserialize(Type arrayType, bool allowNulls)
	{
		Array array = _lengths.Length switch
		{
			1 => new ArrayRecord[_lengths[0]], 
			2 => new ArrayRecord[_lengths[0], _lengths[1]], 
			_ => Array.CreateInstance(typeof(ArrayRecord), _lengths), 
		};
		ArrayRecord.Populate(_records, array, _lengths, AllowedRecordTypes.Arrays, allowNulls);
		return array;
	}

	private protected override void AddValue(object value)
	{
		_records.Add((SerializationRecord)value);
	}

	internal override (AllowedRecordTypes allowed, PrimitiveType primitiveType) GetAllowedRecordType()
	{
		return (allowed: _allowedRecordTypes, primitiveType: (PrimitiveType)0);
	}
}
