using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading;

namespace System.Formats.Nrbf.Utils;

internal static class BinaryReaderExtensions
{
	private static object s_baseAmbiguousDstDateTime;

	internal static SerializationRecordType ReadSerializationRecordType(this BinaryReader reader, AllowedRecordTypes allowed)
	{
		byte b = reader.ReadByte();
		if (b > 22 || (b > 17 && b < 21) || ((uint)allowed & (uint)(1 << (int)b)) == 0)
		{
			ThrowHelper.ThrowForUnexpectedRecordType(b);
		}
		return (SerializationRecordType)b;
	}

	internal static BinaryArrayType ReadArrayType(this BinaryReader reader)
	{
		byte b = reader.ReadByte();
		if (b > 2)
		{
			if (b >= 3 && b <= 5)
			{
				throw new NotSupportedException(System.SR.NotSupported_NonZeroOffsets);
			}
			ThrowHelper.ThrowInvalidValue(b);
		}
		return (BinaryArrayType)b;
	}

	internal static BinaryType ReadBinaryType(this BinaryReader reader)
	{
		byte b = reader.ReadByte();
		if (b > 7)
		{
			ThrowHelper.ThrowInvalidValue(b);
		}
		return (BinaryType)b;
	}

	internal static PrimitiveType ReadPrimitiveType(this BinaryReader reader)
	{
		byte b = reader.ReadByte();
		if ((b < 1 || b > 16 || b == 4) ? true : false)
		{
			ThrowHelper.ThrowInvalidValue(b);
		}
		return (PrimitiveType)b;
	}

	internal static object ReadPrimitiveValue(this BinaryReader reader, PrimitiveType primitiveType)
	{
		return primitiveType switch
		{
			PrimitiveType.Boolean => reader.ReadBoolean(), 
			PrimitiveType.Byte => reader.ReadByte(), 
			PrimitiveType.SByte => reader.ReadSByte(), 
			PrimitiveType.Char => reader.ParseChar(), 
			PrimitiveType.Int16 => reader.ReadInt16(), 
			PrimitiveType.UInt16 => reader.ReadUInt16(), 
			PrimitiveType.Int32 => reader.ReadInt32(), 
			PrimitiveType.UInt32 => reader.ReadUInt32(), 
			PrimitiveType.Int64 => reader.ReadInt64(), 
			PrimitiveType.UInt64 => reader.ReadUInt64(), 
			PrimitiveType.Single => reader.ReadSingle(), 
			PrimitiveType.Double => reader.ReadDouble(), 
			PrimitiveType.Decimal => reader.ParseDecimal(), 
			PrimitiveType.DateTime => CreateDateTimeFromData(reader.ReadUInt64()), 
			PrimitiveType.TimeSpan => new TimeSpan(reader.ReadInt64()), 
			_ => throw new InvalidOperationException(), 
		};
	}

	internal static decimal ParseDecimal(this BinaryReader reader)
	{
		if (!decimal.TryParse(reader.ReadString(), NumberStyles.Number, CultureInfo.InvariantCulture, out var result))
		{
			ThrowHelper.ThrowInvalidFormat();
		}
		return result;
	}

	internal static char ParseChar(this BinaryReader reader)
	{
		try
		{
			return reader.ReadChar();
		}
		catch (ArgumentException)
		{
			throw new SerializationException(System.SR.Serialization_SurrogateCharacter);
		}
	}

	internal static char[] ParseChars(this BinaryReader reader, int count)
	{
		char[] array;
		try
		{
			array = reader.ReadChars(count);
		}
		catch (ArgumentException)
		{
			throw new SerializationException(System.SR.Serialization_SurrogateCharacter);
		}
		if (array.Length != count)
		{
			ThrowHelper.ThrowEndOfStreamException();
		}
		return array;
	}

	internal static DateTime CreateDateTimeFromData(ulong dateData)
	{
		ulong ticks = dateData & 0x3FFFFFFFFFFFFFFFL;
		DateTimeKind dateTimeKind = (DateTimeKind)(dateData >> 62);
		try
		{
			return ((uint)dateTimeKind <= 2u) ? new DateTime((long)ticks, dateTimeKind) : CreateFromAmbiguousDst(ticks);
		}
		catch (ArgumentException ex)
		{
			throw new SerializationException(ex.Message, ex);
		}
		[MethodImpl(MethodImplOptions.NoInlining)]
		static DateTime CreateFromAmbiguousDst(ulong value)
		{
			object obj = s_baseAmbiguousDstDateTime;
			DateTime dateTime;
			if (obj is DateTime)
			{
				dateTime = (DateTime)obj;
			}
			else
			{
				SerializationInfo serializationInfo = new SerializationInfo(typeof(DateTime), new FormatterConverter());
				serializationInfo.AddValue("dateData", 13835058055282163712uL);
				dateTime = (DateTime)typeof(DateTime).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[2]
				{
					typeof(SerializationInfo),
					typeof(StreamingContext)
				}, null).Invoke(new object[2]
				{
					serializationInfo,
					new StreamingContext(StreamingContextStates.All)
				});
				Volatile.Write(ref s_baseAmbiguousDstDateTime, dateTime);
			}
			return dateTime.AddTicks((long)value);
		}
	}

	internal static bool? IsDataAvailable(this BinaryReader reader, long requiredBytes)
	{
		if (!reader.BaseStream.CanSeek)
		{
			return null;
		}
		try
		{
			return reader.BaseStream.Length - reader.BaseStream.Position > requiredBytes;
		}
		catch
		{
			return null;
		}
	}
}
