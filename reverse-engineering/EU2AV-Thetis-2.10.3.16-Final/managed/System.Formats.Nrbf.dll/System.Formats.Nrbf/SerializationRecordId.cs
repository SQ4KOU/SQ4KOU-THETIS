using System.Diagnostics;
using System.Formats.Nrbf.Utils;
using System.IO;

namespace System.Formats.Nrbf;

[DebuggerDisplay("{_id}")]
public readonly struct SerializationRecordId : IEquatable<SerializationRecordId>
{
	internal static readonly SerializationRecordId NoId;

	internal readonly int _id;

	private SerializationRecordId(int id)
	{
		_id = id;
	}

	internal static SerializationRecordId Decode(BinaryReader reader)
	{
		int num = reader.ReadInt32();
		if (num == 0)
		{
			ThrowHelper.ThrowInvalidValue(num);
		}
		return new SerializationRecordId(num);
	}

	public bool Equals(SerializationRecordId other)
	{
		return _id == other._id;
	}

	public override bool Equals(object? obj)
	{
		if (obj is SerializationRecordId other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(_id);
	}
}
