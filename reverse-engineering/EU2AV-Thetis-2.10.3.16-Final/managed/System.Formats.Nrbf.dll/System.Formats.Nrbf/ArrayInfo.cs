using System.Diagnostics;
using System.Formats.Nrbf.Utils;
using System.IO;

namespace System.Formats.Nrbf;

[DebuggerDisplay("{ArrayType}, rank={Rank}")]
internal readonly struct ArrayInfo
{
	internal const int MaxArrayLength = 2147483591;

	internal SerializationRecordId Id { get; }

	internal long FlattenedLength { get; }

	internal BinaryArrayType ArrayType { get; }

	internal int Rank { get; }

	internal ArrayInfo(SerializationRecordId id, long totalElementsCount, BinaryArrayType arrayType = BinaryArrayType.Single, int rank = 1)
	{
		Id = id;
		FlattenedLength = totalElementsCount;
		ArrayType = arrayType;
		Rank = rank;
	}

	internal int GetSZArrayLength()
	{
		return (int)FlattenedLength;
	}

	internal static ArrayInfo Decode(BinaryReader reader)
	{
		return new ArrayInfo(SerializationRecordId.Decode(reader), ParseValidArrayLength(reader));
	}

	internal static int ParseValidArrayLength(BinaryReader reader)
	{
		int num = reader.ReadInt32();
		if (num < 0 || num > 2147483591)
		{
			ThrowHelper.ThrowInvalidValue(num);
		}
		return num;
	}
}
