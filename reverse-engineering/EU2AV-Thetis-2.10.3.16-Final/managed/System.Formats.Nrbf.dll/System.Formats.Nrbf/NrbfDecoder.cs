using System.Collections.Generic;
using System.Formats.Nrbf.Utils;
using System.IO;
using System.Reflection.Metadata;
using System.Runtime.Serialization;
using System.Text;

namespace System.Formats.Nrbf;

public static class NrbfDecoder
{
	private static UTF8Encoding ThrowOnInvalidUtf8Encoding { get; } = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

	private static ReadOnlySpan<byte> HeaderSuffix => new byte[8] { 1, 0, 0, 0, 0, 0, 0, 0 };

	public static bool StartsWithPayloadHeader(ReadOnlySpan<byte> bytes)
	{
		if (bytes.Length >= 17 && bytes[0] == 0)
		{
			return bytes.Slice(17 - HeaderSuffix.Length, HeaderSuffix.Length).SequenceEqual(HeaderSuffix);
		}
		return false;
	}

	public static bool StartsWithPayloadHeader(Stream stream)
	{
		System.ExceptionPolyfills.ThrowIfNull(stream, "stream");
		if (!stream.CanSeek)
		{
			throw new ArgumentException(System.SR.Argument_NonSeekableStream, "stream");
		}
		long position = stream.Position;
		if (stream.Length - position <= 17)
		{
			return false;
		}
		byte[] array = new byte[17];
		int num;
		for (int i = 0; i < array.Length; i += num)
		{
			num = stream.Read(array, i, array.Length - i);
			if (num == 0)
			{
				stream.Position = position;
				return false;
			}
		}
		bool result = StartsWithPayloadHeader(array);
		stream.Position = position;
		return result;
	}

	public static SerializationRecord Decode(Stream payload, PayloadOptions? options = null, bool leaveOpen = false)
	{
		IReadOnlyDictionary<SerializationRecordId, SerializationRecord> recordMap;
		return Decode(payload, out recordMap, options, leaveOpen);
	}

	public static SerializationRecord Decode(Stream payload, out IReadOnlyDictionary<SerializationRecordId, SerializationRecord> recordMap, PayloadOptions? options = null, bool leaveOpen = false)
	{
		System.ExceptionPolyfills.ThrowIfNull(payload, "payload");
		using BinaryReader reader = new BinaryReader(payload, ThrowOnInvalidUtf8Encoding, leaveOpen);
		try
		{
			return Decode(reader, options ?? new PayloadOptions(), out recordMap);
		}
		catch (FormatException)
		{
			throw new SerializationException(System.SR.Serialization_InvalidFormat);
		}
	}

	public static ClassRecord DecodeClassRecord(Stream payload, PayloadOptions? options = null, bool leaveOpen = false)
	{
		return (ClassRecord)Decode(payload, options, leaveOpen);
	}

	private static SerializationRecord Decode(BinaryReader reader, PayloadOptions options, out IReadOnlyDictionary<SerializationRecordId, SerializationRecord> readOnlyRecordMap)
	{
		Stack<NextInfo> stack = new Stack<NextInfo>();
		RecordMap recordMap = new RecordMap();
		SerializedStreamHeaderRecord header = (SerializedStreamHeaderRecord)DecodeNext(reader, recordMap, AllowedRecordTypes.SerializedStreamHeader, options, out var recordType);
		while (true)
		{
			if (stack.Count > 0)
			{
				NextInfo info = stack.Pop();
				if (info.Allowed != AllowedRecordTypes.None)
				{
					SerializationRecord serializationRecord;
					do
					{
						serializationRecord = DecodeNext(reader, recordMap, info.Allowed, options, out recordType);
					}
					while (serializationRecord is BinaryLibraryRecord);
					info.Parent.HandleNextRecord(serializationRecord, info);
					PushFirstNestedRecordInfo(serializationRecord, stack);
				}
				else
				{
					object value = reader.ReadPrimitiveValue(info.PrimitiveType);
					info.Parent.HandleNextValue(value, info);
				}
			}
			else
			{
				SerializationRecord serializationRecord = DecodeNext(reader, recordMap, AllowedRecordTypes.AnyObject | AllowedRecordTypes.MessageEnd | AllowedRecordTypes.BinaryLibrary, options, out var recordType2);
				PushFirstNestedRecordInfo(serializationRecord, stack);
				if (recordType2 == SerializationRecordType.MessageEnd)
				{
					break;
				}
			}
		}
		readOnlyRecordMap = recordMap;
		return recordMap.GetRootRecord(header);
	}

	private static SerializationRecord DecodeNext(BinaryReader reader, RecordMap recordMap, AllowedRecordTypes allowed, PayloadOptions options, out SerializationRecordType recordType)
	{
		recordType = reader.ReadSerializationRecordType(allowed);
		SerializationRecord serializationRecord = recordType switch
		{
			SerializationRecordType.ArraySingleObject => ArraySingleObjectRecord.Decode(reader), 
			SerializationRecordType.ArraySinglePrimitive => DecodeArraySinglePrimitiveRecord(reader), 
			SerializationRecordType.ArraySingleString => ArraySingleStringRecord.Decode(reader), 
			SerializationRecordType.BinaryArray => DecodeBinaryArrayRecord(reader, recordMap, options), 
			SerializationRecordType.BinaryLibrary => BinaryLibraryRecord.Decode(reader, options), 
			SerializationRecordType.BinaryObjectString => BinaryObjectStringRecord.Decode(reader), 
			SerializationRecordType.ClassWithId => ClassWithIdRecord.Decode(reader, recordMap), 
			SerializationRecordType.ClassWithMembersAndTypes => ClassWithMembersAndTypesRecord.Decode(reader, recordMap, options), 
			SerializationRecordType.MemberPrimitiveTyped => DecodeMemberPrimitiveTypedRecord(reader), 
			SerializationRecordType.MemberReference => MemberReferenceRecord.Decode(reader, recordMap), 
			SerializationRecordType.MessageEnd => MessageEndRecord.Singleton, 
			SerializationRecordType.ObjectNull => ObjectNullRecord.Instance, 
			SerializationRecordType.ObjectNullMultiple => ObjectNullMultipleRecord.Decode(reader), 
			SerializationRecordType.ObjectNullMultiple256 => ObjectNullMultiple256Record.Decode(reader), 
			SerializationRecordType.SerializedStreamHeader => SerializedStreamHeaderRecord.Decode(reader), 
			SerializationRecordType.SystemClassWithMembersAndTypes => SystemClassWithMembersAndTypesRecord.Decode(reader, recordMap, options), 
			_ => throw new InvalidOperationException(), 
		};
		recordMap.Add(serializationRecord);
		return serializationRecord;
	}

	private static SerializationRecord DecodeMemberPrimitiveTypedRecord(BinaryReader reader)
	{
		return reader.ReadPrimitiveType() switch
		{
			PrimitiveType.Boolean => new MemberPrimitiveTypedRecord<bool>(reader.ReadBoolean()), 
			PrimitiveType.Byte => new MemberPrimitiveTypedRecord<byte>(reader.ReadByte()), 
			PrimitiveType.SByte => new MemberPrimitiveTypedRecord<sbyte>(reader.ReadSByte()), 
			PrimitiveType.Char => new MemberPrimitiveTypedRecord<char>(reader.ParseChar()), 
			PrimitiveType.Int16 => new MemberPrimitiveTypedRecord<short>(reader.ReadInt16()), 
			PrimitiveType.UInt16 => new MemberPrimitiveTypedRecord<ushort>(reader.ReadUInt16()), 
			PrimitiveType.Int32 => new MemberPrimitiveTypedRecord<int>(reader.ReadInt32()), 
			PrimitiveType.UInt32 => new MemberPrimitiveTypedRecord<uint>(reader.ReadUInt32()), 
			PrimitiveType.Int64 => new MemberPrimitiveTypedRecord<long>(reader.ReadInt64()), 
			PrimitiveType.UInt64 => new MemberPrimitiveTypedRecord<ulong>(reader.ReadUInt64()), 
			PrimitiveType.Single => new MemberPrimitiveTypedRecord<float>(reader.ReadSingle()), 
			PrimitiveType.Double => new MemberPrimitiveTypedRecord<double>(reader.ReadDouble()), 
			PrimitiveType.Decimal => new MemberPrimitiveTypedRecord<decimal>(reader.ParseDecimal()), 
			PrimitiveType.DateTime => new MemberPrimitiveTypedRecord<DateTime>(BinaryReaderExtensions.CreateDateTimeFromData(reader.ReadUInt64())), 
			PrimitiveType.TimeSpan => new MemberPrimitiveTypedRecord<TimeSpan>(new TimeSpan(reader.ReadInt64())), 
			_ => throw new InvalidOperationException(), 
		};
	}

	private static ArrayRecord DecodeArraySinglePrimitiveRecord(BinaryReader reader)
	{
		ArrayInfo info = ArrayInfo.Decode(reader);
		PrimitiveType primitiveType = reader.ReadPrimitiveType();
		return DecodeArraySinglePrimitiveRecord(reader, info, primitiveType);
	}

	private static ArrayRecord DecodeArraySinglePrimitiveRecord(BinaryReader reader, ArrayInfo info, PrimitiveType primitiveType)
	{
		return primitiveType switch
		{
			PrimitiveType.Boolean => Decode<bool>(info, reader), 
			PrimitiveType.Byte => Decode<byte>(info, reader), 
			PrimitiveType.SByte => Decode<sbyte>(info, reader), 
			PrimitiveType.Char => Decode<char>(info, reader), 
			PrimitiveType.Int16 => Decode<short>(info, reader), 
			PrimitiveType.UInt16 => Decode<ushort>(info, reader), 
			PrimitiveType.Int32 => Decode<int>(info, reader), 
			PrimitiveType.UInt32 => Decode<uint>(info, reader), 
			PrimitiveType.Int64 => Decode<long>(info, reader), 
			PrimitiveType.UInt64 => Decode<ulong>(info, reader), 
			PrimitiveType.Single => Decode<float>(info, reader), 
			PrimitiveType.Double => Decode<double>(info, reader), 
			PrimitiveType.Decimal => Decode<decimal>(info, reader), 
			PrimitiveType.DateTime => Decode<DateTime>(info, reader), 
			PrimitiveType.TimeSpan => Decode<TimeSpan>(info, reader), 
			_ => throw new InvalidOperationException(), 
		};
		static ArrayRecord Decode<T>(ArrayInfo arrayInfo, BinaryReader reader2) where T : unmanaged
		{
			return new ArraySinglePrimitiveRecord<T>(arrayInfo, ArraySinglePrimitiveRecord<T>.DecodePrimitiveTypes(reader2, arrayInfo.GetSZArrayLength()));
		}
	}

	private static ArrayRecord DecodeArrayRectangularPrimitiveRecord(PrimitiveType primitiveType, ArrayInfo info, int[] lengths, BinaryReader reader)
	{
		return primitiveType switch
		{
			PrimitiveType.Boolean => Decode<bool>(info, lengths, reader), 
			PrimitiveType.Byte => Decode<byte>(info, lengths, reader), 
			PrimitiveType.SByte => Decode<sbyte>(info, lengths, reader), 
			PrimitiveType.Char => Decode<char>(info, lengths, reader), 
			PrimitiveType.Int16 => Decode<short>(info, lengths, reader), 
			PrimitiveType.UInt16 => Decode<ushort>(info, lengths, reader), 
			PrimitiveType.Int32 => Decode<int>(info, lengths, reader), 
			PrimitiveType.UInt32 => Decode<uint>(info, lengths, reader), 
			PrimitiveType.Int64 => Decode<long>(info, lengths, reader), 
			PrimitiveType.UInt64 => Decode<ulong>(info, lengths, reader), 
			PrimitiveType.Single => Decode<float>(info, lengths, reader), 
			PrimitiveType.Double => Decode<double>(info, lengths, reader), 
			PrimitiveType.Decimal => Decode<decimal>(info, lengths, reader), 
			PrimitiveType.DateTime => Decode<DateTime>(info, lengths, reader), 
			PrimitiveType.TimeSpan => Decode<TimeSpan>(info, lengths, reader), 
			_ => throw new InvalidOperationException(), 
		};
		static ArrayRecord Decode<T>(ArrayInfo arrayInfo, int[] lengths2, BinaryReader reader2) where T : unmanaged
		{
			IReadOnlyList<T> values = ArraySinglePrimitiveRecord<T>.DecodePrimitiveTypes(reader2, arrayInfo.GetSZArrayLength());
			return new ArrayRectangularPrimitiveRecord<T>(arrayInfo, lengths2, values);
		}
	}

	private static ArrayRecord DecodeBinaryArrayRecord(BinaryReader reader, RecordMap recordMap, PayloadOptions options)
	{
		SerializationRecordId id = SerializationRecordId.Decode(reader);
		BinaryArrayType binaryArrayType = reader.ReadArrayType();
		int num = reader.ReadInt32();
		bool flag = binaryArrayType == BinaryArrayType.Rectangular;
		if (num < 1 || num > 32 || (num != 1 && !flag) || ((num == 1) & flag))
		{
			ThrowHelper.ThrowInvalidValue(num);
		}
		int[] array = new int[num];
		long num2 = 1L;
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = ArrayInfo.ParseValidArrayLength(reader);
			num2 *= array[i];
			if (num2 > 2147483591)
			{
				ThrowHelper.ThrowInvalidValue(array[i]);
			}
		}
		MemberTypeInfo memberTypeInfo = MemberTypeInfo.Decode(reader, 1, options, recordMap);
		ArrayInfo arrayInfo = new ArrayInfo(id, num2, binaryArrayType, num);
		var (binaryType, obj) = memberTypeInfo.Infos[0];
		switch (binaryArrayType)
		{
		case BinaryArrayType.Rectangular:
		{
			bool flag2;
			switch (binaryType)
			{
			case BinaryType.Primitive:
				return DecodeArrayRectangularPrimitiveRecord((PrimitiveType)obj, arrayInfo, array, reader);
			case BinaryType.String:
				return new RectangularArrayRecord(typeof(string), arrayInfo, memberTypeInfo, array);
			case BinaryType.Object:
				return new RectangularArrayRecord(typeof(SerializationRecord), arrayInfo, memberTypeInfo, array);
			case BinaryType.SystemClass:
			case BinaryType.Class:
				flag2 = true;
				break;
			default:
				flag2 = false;
				break;
			}
			if (flag2)
			{
				if (!((binaryType == BinaryType.SystemClass) ? ((TypeName)obj) : ((ClassTypeInfo)obj).TypeName).IsArray)
				{
					return new RectangularArrayRecord(typeof(SerializationRecord), arrayInfo, memberTypeInfo, array);
				}
				return new JaggedArrayRecord(arrayInfo, memberTypeInfo, array);
			}
			if (binaryType - 5 <= BinaryType.Object)
			{
				return new JaggedArrayRecord(arrayInfo, memberTypeInfo, array);
			}
			break;
		}
		case BinaryArrayType.Single:
		{
			if (binaryType - 3 <= BinaryType.String)
			{
				if (!((binaryType == BinaryType.SystemClass) ? ((TypeName)obj) : ((ClassTypeInfo)obj).TypeName).IsArray)
				{
					return new SZArrayOfRecords(arrayInfo, memberTypeInfo);
				}
				return new JaggedArrayRecord(arrayInfo, memberTypeInfo, array);
			}
			bool flag2;
			switch (binaryType)
			{
			case BinaryType.String:
				return new ArraySingleStringRecord(arrayInfo);
			case BinaryType.Primitive:
				return DecodeArraySinglePrimitiveRecord(reader, arrayInfo, (PrimitiveType)obj);
			case BinaryType.Object:
				return new ArraySingleObjectRecord(arrayInfo);
			case BinaryType.ObjectArray:
			case BinaryType.StringArray:
			case BinaryType.PrimitiveArray:
				flag2 = true;
				break;
			default:
				flag2 = false;
				break;
			}
			if (flag2)
			{
				return new JaggedArrayRecord(arrayInfo, memberTypeInfo, array);
			}
			break;
		}
		case BinaryArrayType.Jagged:
			if (binaryType - 5 <= BinaryType.Object)
			{
				return new JaggedArrayRecord(arrayInfo, memberTypeInfo, array);
			}
			if (binaryType == BinaryType.SystemClass && ((TypeName)obj).IsArray)
			{
				return new JaggedArrayRecord(arrayInfo, memberTypeInfo, array);
			}
			if (binaryType == BinaryType.Class && ((ClassTypeInfo)obj).TypeName.IsArray)
			{
				return new JaggedArrayRecord(arrayInfo, memberTypeInfo, array);
			}
			throw new SerializationException(System.SR.Format(System.SR.Serialization_InvalidValue, binaryType));
		}
		throw new InvalidOperationException();
	}

	private static void PushFirstNestedRecordInfo(SerializationRecord record, Stack<NextInfo> readStack)
	{
		if (record is ClassRecord classRecord)
		{
			if (classRecord.ExpectedValuesCount > 0)
			{
				var (allowed, primitiveType) = classRecord.GetNextAllowedRecordType();
				readStack.Push(new NextInfo(allowed, classRecord, readStack, primitiveType));
			}
		}
		else if (record is ArrayRecord arrayRecord && arrayRecord.ValuesToRead > 0)
		{
			var (allowed2, primitiveType2) = arrayRecord.GetAllowedRecordType();
			readStack.Push(new NextInfo(allowed2, arrayRecord, readStack, primitiveType2));
		}
	}
}
