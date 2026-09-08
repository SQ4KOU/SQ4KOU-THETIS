using System.Collections.Generic;
using System.Diagnostics;
using System.Formats.Nrbf.Utils;
using System.IO;
using System.Reflection.Metadata;

namespace System.Formats.Nrbf;

[DebuggerDisplay("{TypeName}")]
internal sealed class ClassInfo
{
	private readonly string _rawName;

	private TypeName _typeName;

	internal SerializationRecordId Id { get; }

	internal TypeName TypeName => _typeName;

	internal Dictionary<string, int> MemberNames { get; }

	private ClassInfo(SerializationRecordId id, string rawName, Dictionary<string, int> memberNames)
	{
		Id = id;
		_rawName = rawName;
		MemberNames = memberNames;
	}

	internal static ClassInfo Decode(BinaryReader reader)
	{
		SerializationRecordId id = SerializationRecordId.Decode(reader);
		string rawName = reader.ReadString();
		int num = reader.ReadInt32();
		Dictionary<string, int> dictionary = new Dictionary<string, int>(StringComparer.Ordinal);
		for (int i = 0; i < num; i++)
		{
			string key = reader.ReadString();
			if (!dictionary.ContainsKey(key))
			{
				dictionary.Add(key, i);
			}
			else
			{
				ThrowHelper.ThrowDuplicateMemberName();
			}
		}
		return new ClassInfo(id, rawName, dictionary);
	}

	internal void LoadTypeName(BinaryLibraryRecord libraryRecord, PayloadOptions payloadOptions)
	{
		_typeName = _rawName.ParseNonSystemClassRecordTypeName(libraryRecord, payloadOptions);
	}

	internal void LoadTypeName(PayloadOptions payloadOptions)
	{
		_typeName = _rawName.ParseSystemRecordTypeName(payloadOptions);
	}
}
