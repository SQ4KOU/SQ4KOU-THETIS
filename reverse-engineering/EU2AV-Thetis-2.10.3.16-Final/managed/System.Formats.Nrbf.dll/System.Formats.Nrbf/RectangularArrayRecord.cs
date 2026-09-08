using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection.Metadata;

namespace System.Formats.Nrbf;

internal sealed class RectangularArrayRecord : ArrayRecord
{
	private readonly Type _elementType;

	private readonly int[] _lengths;

	private readonly List<SerializationRecord> _records;

	private readonly AllowedRecordTypes _allowedRecordTypes;

	private readonly MemberTypeInfo _memberTypeInfo;

	private TypeName _typeName;

	public override SerializationRecordType RecordType => SerializationRecordType.BinaryArray;

	public override ReadOnlySpan<int> Lengths => MemoryExtensions.AsSpan(_lengths);

	public override TypeName TypeName => _typeName ?? (_typeName = _memberTypeInfo.GetArrayTypeName(base.ArrayInfo));

	internal RectangularArrayRecord(Type elementType, ArrayInfo arrayInfo, MemberTypeInfo memberTypeInfo, int[] lengths)
		: base(arrayInfo)
	{
		_elementType = elementType;
		_lengths = lengths;
		_memberTypeInfo = memberTypeInfo;
		_records = new List<SerializationRecord>(Math.Min(4, arrayInfo.GetSZArrayLength()));
		_allowedRecordTypes = memberTypeInfo.GetNextAllowedRecordType(0).allowed;
	}

	[RequiresDynamicCode("May call Array.CreateInstance() and Type.MakeArrayType().")]
	private protected override Array Deserialize(Type arrayType, bool allowNulls)
	{
		bool num = _elementType == typeof(string);
		Array array = Array.CreateInstance(_elementType, _lengths);
		AllowedRecordTypes allowedRecordTypes = (num ? AllowedRecordTypes.BinaryObjectString : AllowedRecordTypes.AnyObject);
		ArrayRecord.Populate(_records, array, _lengths, allowedRecordTypes, allowNulls);
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
