using System.Formats.Nrbf.Utils;
using System.IO;

namespace System.Formats.Nrbf;

internal sealed class ObjectNullMultipleRecord : NullsRecord
{
	public override SerializationRecordType RecordType => SerializationRecordType.ObjectNullMultiple;

	internal override int NullCount { get; }

	private ObjectNullMultipleRecord(int count)
	{
		NullCount = count;
	}

	internal static ObjectNullMultipleRecord Decode(BinaryReader reader)
	{
		int num = reader.ReadInt32();
		if (num <= 0)
		{
			ThrowHelper.ThrowInvalidValue(num);
		}
		return new ObjectNullMultipleRecord(num);
	}
}
