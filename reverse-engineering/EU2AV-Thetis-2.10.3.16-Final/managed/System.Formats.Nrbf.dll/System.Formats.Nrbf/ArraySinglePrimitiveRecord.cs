using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Formats.Nrbf.Utils;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Formats.Nrbf;

internal sealed class ArraySinglePrimitiveRecord<T> : SZArrayRecord<T> where T : unmanaged
{
	public override SerializationRecordType RecordType => SerializationRecordType.ArraySinglePrimitive;

	public override TypeName TypeName => TypeNameHelpers.GetPrimitiveSZArrayTypeName(TypeNameHelpers.GetPrimitiveType<T>());

	internal IReadOnlyList<T> Values { get; }

	internal ArraySinglePrimitiveRecord(ArrayInfo arrayInfo, IReadOnlyList<T> values)
		: base(arrayInfo)
	{
		Values = values;
		base.ValuesToRead = 0L;
	}

	public override T[] GetArray(bool allowNulls = true)
	{
		return (T[])(_arrayNullsNotAllowed ?? (_arrayNullsNotAllowed = ((Values is T[] array) ? array : Values.ToArray())));
	}

	internal override (AllowedRecordTypes allowed, PrimitiveType primitiveType) GetAllowedRecordType()
	{
		throw new InvalidOperationException();
	}

	private protected override void AddValue(object value)
	{
		throw new InvalidOperationException();
	}

	internal static IReadOnlyList<T> DecodePrimitiveTypes(BinaryReader reader, int count)
	{
		if (count == 0)
		{
			return Array.Empty<T>();
		}
		if (typeof(T) == typeof(decimal))
		{
			return (List<T>)(object)DecodeDecimals(reader, count);
		}
		int num = ((typeof(T) == typeof(DateTime) || typeof(T) == typeof(TimeSpan)) ? 8 : ((!(typeof(T) != typeof(char))) ? 1 : Unsafe.SizeOf<T>()));
		long num2 = (long)count * (long)num;
		bool? flag = reader.IsDataAvailable(num2);
		if (!flag.HasValue)
		{
			return DecodeFromNonSeekableStream(reader, count);
		}
		if (!flag.Value)
		{
			ThrowHelper.ThrowEndOfStreamException();
		}
		if (typeof(T) == typeof(byte))
		{
			return (T[])(object)reader.ReadBytes(count);
		}
		if (typeof(T) == typeof(char))
		{
			return (T[])(object)reader.ParseChars(count);
		}
		if (typeof(T) == typeof(TimeSpan) || typeof(T) == typeof(DateTime))
		{
			return DecodeTime(reader, count);
		}
		T[] array = new T[count];
		int val = int.MaxValue / num;
		byte[] array2 = ArrayPool<byte>.Shared.Rent((int)Math.Min(num2, 256000L));
		Span<T> span = MemoryExtensions.AsSpan(array);
		while (!span.IsEmpty)
		{
			int num3 = Math.Min(span.Length, val);
			Span<byte> destination = MemoryMarshal.AsBytes(span.Slice(0, num3));
			while (!destination.IsEmpty)
			{
				int num4 = reader.Read(array2, 0, Math.Min(destination.Length, array2.Length));
				if (num4 <= 0)
				{
					ArrayPool<byte>.Shared.Return(array2);
					ThrowHelper.ThrowEndOfStreamException();
				}
				MemoryExtensions.AsSpan(array2, 0, num4).CopyTo(destination);
				destination = destination.Slice(num4);
			}
			span = span.Slice(num3);
		}
		ArrayPool<byte>.Shared.Return(array2);
		if (!BitConverter.IsLittleEndian)
		{
			if (typeof(T) == typeof(short) || typeof(T) == typeof(ushort))
			{
				Span<short> span2 = MemoryMarshal.Cast<T, short>(MemoryExtensions.AsSpan(array));
				for (int i = 0; i < span2.Length; i++)
				{
					span2[i] = BinaryPrimitives.ReverseEndianness(span2[i]);
				}
			}
			else if (typeof(T) == typeof(int) || typeof(T) == typeof(uint) || typeof(T) == typeof(float))
			{
				Span<int> span3 = MemoryMarshal.Cast<T, int>(MemoryExtensions.AsSpan(array));
				for (int j = 0; j < span3.Length; j++)
				{
					span3[j] = BinaryPrimitives.ReverseEndianness(span3[j]);
				}
			}
			else if (typeof(T) == typeof(long) || typeof(T) == typeof(ulong) || typeof(T) == typeof(double))
			{
				Span<long> span4 = MemoryMarshal.Cast<T, long>(MemoryExtensions.AsSpan(array));
				for (int k = 0; k < span4.Length; k++)
				{
					span4[k] = BinaryPrimitives.ReverseEndianness(span4[k]);
				}
			}
		}
		if (typeof(T) == typeof(bool))
		{
			bool[] array3 = (bool[])(object)array;
			Span<byte> span5 = MemoryMarshal.AsBytes(MemoryExtensions.AsSpan(array));
			for (int l = 0; l < array3.Length; l++)
			{
				if (span5[l] != 0)
				{
					array3[l] = true;
				}
			}
		}
		return array;
	}

	private static List<decimal> DecodeDecimals(BinaryReader reader, int count)
	{
		List<decimal> list = new List<decimal>();
		for (int i = 0; i < count; i++)
		{
			list.Add(reader.ParseDecimal());
		}
		return list;
	}

	private static T[] DecodeTime(BinaryReader reader, int count)
	{
		T[] array = new T[count];
		for (int i = 0; i < array.Length; i++)
		{
			if (typeof(T) == typeof(DateTime))
			{
				array[i] = (T)(object)BinaryReaderExtensions.CreateDateTimeFromData(reader.ReadUInt64());
				continue;
			}
			if (typeof(T) == typeof(TimeSpan))
			{
				array[i] = (T)(object)new TimeSpan(reader.ReadInt64());
				continue;
			}
			throw new InvalidOperationException();
		}
		return array;
	}

	private static List<T> DecodeFromNonSeekableStream(BinaryReader reader, int count)
	{
		List<T> list = new List<T>(Math.Min(count, 4));
		for (int i = 0; i < count; i++)
		{
			if (typeof(T) == typeof(byte))
			{
				list.Add((T)(object)reader.ReadByte());
				continue;
			}
			if (typeof(T) == typeof(bool))
			{
				list.Add((T)(object)reader.ReadBoolean());
				continue;
			}
			if (typeof(T) == typeof(sbyte))
			{
				list.Add((T)(object)reader.ReadSByte());
				continue;
			}
			if (typeof(T) == typeof(char))
			{
				list.Add((T)(object)reader.ParseChar());
				continue;
			}
			if (typeof(T) == typeof(short))
			{
				list.Add((T)(object)reader.ReadInt16());
				continue;
			}
			if (typeof(T) == typeof(ushort))
			{
				list.Add((T)(object)reader.ReadUInt16());
				continue;
			}
			if (typeof(T) == typeof(int))
			{
				list.Add((T)(object)reader.ReadInt32());
				continue;
			}
			if (typeof(T) == typeof(uint))
			{
				list.Add((T)(object)reader.ReadUInt32());
				continue;
			}
			if (typeof(T) == typeof(long))
			{
				list.Add((T)(object)reader.ReadInt64());
				continue;
			}
			if (typeof(T) == typeof(ulong))
			{
				list.Add((T)(object)reader.ReadUInt64());
				continue;
			}
			if (typeof(T) == typeof(float))
			{
				list.Add((T)(object)reader.ReadSingle());
				continue;
			}
			if (typeof(T) == typeof(double))
			{
				list.Add((T)(object)reader.ReadDouble());
				continue;
			}
			if (typeof(T) == typeof(DateTime))
			{
				list.Add((T)(object)BinaryReaderExtensions.CreateDateTimeFromData(reader.ReadUInt64()));
				continue;
			}
			if (typeof(T) == typeof(TimeSpan))
			{
				list.Add((T)(object)new TimeSpan(reader.ReadInt64()));
				continue;
			}
			throw new InvalidOperationException();
		}
		return list;
	}
}
