using System.Formats.Nrbf.Utils;
using System.IO;
using System.Reflection.Metadata;

namespace System.Formats.Nrbf;

internal sealed class BinaryLibraryRecord : SerializationRecord
{
	public override SerializationRecordType RecordType => SerializationRecordType.BinaryLibrary;

	public override TypeName TypeName => TypeName.Parse(MemoryExtensions.AsSpan("BinaryLibraryRecord"));

	internal string RawLibraryName { get; }

	internal AssemblyNameInfo LibraryName { get; }

	public override SerializationRecordId Id { get; }

	private BinaryLibraryRecord(SerializationRecordId libraryId, string rawLibraryName)
	{
		Id = libraryId;
		RawLibraryName = rawLibraryName;
	}

	private BinaryLibraryRecord(SerializationRecordId libraryId, AssemblyNameInfo libraryName)
	{
		Id = libraryId;
		LibraryName = libraryName;
	}

	internal static BinaryLibraryRecord Decode(BinaryReader reader, PayloadOptions options)
	{
		SerializationRecordId libraryId = SerializationRecordId.Decode(reader);
		string text = reader.ReadString();
		if (AssemblyNameInfo.TryParse(MemoryExtensions.AsSpan(text), out AssemblyNameInfo result))
		{
			return new BinaryLibraryRecord(libraryId, result);
		}
		if (!options.UndoTruncatedTypeNames)
		{
			ThrowHelper.ThrowInvalidAssemblyName();
		}
		return new BinaryLibraryRecord(libraryId, text);
	}
}
