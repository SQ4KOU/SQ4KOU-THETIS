using System.Diagnostics;
using System.Formats.Nrbf.Utils;
using System.IO;
using System.Reflection.Metadata;

namespace System.Formats.Nrbf;

[DebuggerDisplay("{TypeName}")]
internal sealed class ClassTypeInfo
{
	internal TypeName TypeName { get; }

	internal ClassTypeInfo(TypeName typeName)
	{
		TypeName = typeName;
	}

	internal static ClassTypeInfo Decode(BinaryReader reader, PayloadOptions options, RecordMap recordMap)
	{
		string rawName = reader.ReadString();
		SerializationRecordId recordId = SerializationRecordId.Decode(reader);
		BinaryLibraryRecord record = recordMap.GetRecord<BinaryLibraryRecord>(recordId);
		return new ClassTypeInfo(rawName.ParseNonSystemClassRecordTypeName(record, options));
	}
}
