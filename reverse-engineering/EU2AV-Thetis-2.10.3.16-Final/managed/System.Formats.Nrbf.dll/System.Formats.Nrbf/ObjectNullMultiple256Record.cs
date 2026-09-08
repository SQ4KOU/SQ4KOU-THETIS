using System.Formats.Nrbf.Utils;
using System.IO;

namespace System.Formats.Nrbf;

internal sealed class ObjectNullMultiple256Record : NullsRecord
{
	public override SerializationRecordType RecordType => SerializationRecordType.ObjectNullMultiple256;

	internal override int NullCount { get; }

	private ObjectNullMultiple256Record(byte count)
	{
		NullCount = count;
	}

	internal static ObjectNullMultiple256Record Decode(BinaryReader reader)
	{
		byte b = reader.ReadByte();
		if (b == 0)
		{
			ThrowHelper.ThrowInvalidValue(b);
		}
		return new ObjectNullMultiple256Record(b);
	}
}
