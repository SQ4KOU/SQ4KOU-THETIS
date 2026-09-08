using System.Collections.Immutable;
using System.IO;
using System.Reflection.Internal;

namespace System.Reflection.PortableExecutable;

public sealed class PEHeaders
{
	private readonly CoffHeader _coffHeader;

	private readonly PEHeader _peHeader;

	private readonly ImmutableArray<SectionHeader> _sectionHeaders;

	private readonly CorHeader _corHeader;

	private readonly bool _isLoadedImage;

	private readonly int _metadataStartOffset = -1;

	private readonly int _metadataSize;

	private readonly int _coffHeaderStartOffset = -1;

	private readonly int _corHeaderStartOffset = -1;

	private readonly int _peHeaderStartOffset = -1;

	internal const ushort DosSignature = 23117;

	internal const int PESignatureOffsetLocation = 60;

	internal const uint PESignature = 17744u;

	internal const int PESignatureSize = 4;

	public int MetadataStartOffset => _metadataStartOffset;

	public int MetadataSize => _metadataSize;

	public CoffHeader CoffHeader => _coffHeader;

	public int CoffHeaderStartOffset => _coffHeaderStartOffset;

	public bool IsCoffOnly => _peHeader == null;

	public PEHeader? PEHeader => _peHeader;

	public int PEHeaderStartOffset => _peHeaderStartOffset;

	public ImmutableArray<SectionHeader> SectionHeaders => _sectionHeaders;

	public CorHeader? CorHeader => _corHeader;

	public int CorHeaderStartOffset => _corHeaderStartOffset;

	public bool IsConsoleApplication
	{
		get
		{
			if (_peHeader != null)
			{
				return _peHeader.Subsystem == Subsystem.WindowsCui;
			}
			return false;
		}
	}

	public bool IsDll => (_coffHeader.Characteristics & Characteristics.Dll) != 0;

	public bool IsExe => (_coffHeader.Characteristics & Characteristics.Dll) == 0;

	public PEHeaders(Stream peStream)
		: this(peStream, 0)
	{
	}

	public PEHeaders(Stream peStream, int size)
		: this(peStream, size, isLoadedImage: false)
	{
	}

	public PEHeaders(Stream peStream, int size, bool isLoadedImage)
	{
		if (peStream == null)
		{
			System.Reflection.Throw.ArgumentNull("peStream");
		}
		if (!peStream.CanRead || !peStream.CanSeek)
		{
			throw new ArgumentException(System.SR.StreamMustSupportReadAndSeek, "peStream");
		}
		_isLoadedImage = isLoadedImage;
		int andValidateSize = System.Reflection.Internal.StreamExtensions.GetAndValidateSize(peStream, size, "peStream");
		System.Reflection.PortableExecutable.PEBinaryReader reader = new System.Reflection.PortableExecutable.PEBinaryReader(peStream, andValidateSize);
		SkipDosHeader(ref reader, out var isCOFFOnly);
		if (isCOFFOnly & isLoadedImage)
		{
			throw new BadImageFormatException(System.SR.InvalidPESignature);
		}
		_coffHeaderStartOffset = reader.Offset;
		_coffHeader = new CoffHeader(ref reader);
		if (isCOFFOnly)
		{
			if (_coffHeader.SizeOfOptionalHeader != 0)
			{
				throw new BadImageFormatException(System.SR.UnknownFileFormat);
			}
		}
		else
		{
			_peHeaderStartOffset = reader.Offset;
			_peHeader = new PEHeader(ref reader);
		}
		_sectionHeaders = ReadSectionHeaders(ref reader, andValidateSize);
		if (!isCOFFOnly && TryCalculateCorHeaderOffset(out var startOffset))
		{
			_corHeaderStartOffset = startOffset;
			reader.Offset = startOffset;
			_corHeader = new CorHeader(ref reader);
		}
		CalculateMetadataLocation(andValidateSize, out _metadataStartOffset, out _metadataSize);
	}

	private bool TryCalculateCorHeaderOffset(out int startOffset)
	{
		if (!TryGetDirectoryOffset(_peHeader.CorHeaderTableDirectory, out startOffset, canCrossSectionBoundary: false))
		{
			startOffset = -1;
			return false;
		}
		if (_peHeader.CorHeaderTableDirectory.Size < 72)
		{
			throw new BadImageFormatException(System.SR.InvalidCorHeaderSize);
		}
		return true;
	}

	private static void SkipDosHeader(ref System.Reflection.PortableExecutable.PEBinaryReader reader, out bool isCOFFOnly)
	{
		ushort num = reader.ReadUInt16();
		if (num != 23117)
		{
			if (num == 0 && reader.ReadUInt16() == ushort.MaxValue)
			{
				throw new BadImageFormatException(System.SR.UnknownFileFormat);
			}
			isCOFFOnly = true;
			reader.Offset = 0;
		}
		else
		{
			isCOFFOnly = false;
		}
		if (!isCOFFOnly)
		{
			reader.Offset = 60;
			int offset = reader.ReadInt32();
			reader.Offset = offset;
			if (reader.ReadUInt32() != 17744)
			{
				throw new BadImageFormatException(System.SR.InvalidPESignature);
			}
		}
	}

	private ImmutableArray<SectionHeader> ReadSectionHeaders(ref System.Reflection.PortableExecutable.PEBinaryReader reader, int size)
	{
		int numberOfSections = _coffHeader.NumberOfSections;
		if (numberOfSections < 0 || numberOfSections * 40 > size - reader.Offset)
		{
			throw new BadImageFormatException(System.SR.InvalidNumberOfSections);
		}
		ImmutableArray<SectionHeader>.Builder builder = ImmutableArray.CreateBuilder<SectionHeader>(numberOfSections);
		for (int i = 0; i < numberOfSections; i++)
		{
			builder.Add(new SectionHeader(ref reader));
		}
		return builder.MoveToImmutable();
	}

	public bool TryGetDirectoryOffset(DirectoryEntry directory, out int offset)
	{
		return TryGetDirectoryOffset(directory, out offset, canCrossSectionBoundary: true);
	}

	internal bool TryGetDirectoryOffset(DirectoryEntry directory, out int offset, bool canCrossSectionBoundary)
	{
		int containingSectionIndex = GetContainingSectionIndex(directory.RelativeVirtualAddress);
		if (containingSectionIndex < 0)
		{
			offset = -1;
			return false;
		}
		int num = directory.RelativeVirtualAddress - _sectionHeaders[containingSectionIndex].VirtualAddress;
		if (!canCrossSectionBoundary && directory.Size > _sectionHeaders[containingSectionIndex].VirtualSize - num)
		{
			throw new BadImageFormatException(System.SR.SectionTooSmall);
		}
		offset = (_isLoadedImage ? directory.RelativeVirtualAddress : (_sectionHeaders[containingSectionIndex].PointerToRawData + num));
		return true;
	}

	public int GetContainingSectionIndex(int relativeVirtualAddress)
	{
		for (int i = 0; i < _sectionHeaders.Length; i++)
		{
			if (_sectionHeaders[i].VirtualAddress <= relativeVirtualAddress && relativeVirtualAddress < _sectionHeaders[i].VirtualAddress + _sectionHeaders[i].VirtualSize)
			{
				return i;
			}
		}
		return -1;
	}

	internal int IndexOfSection(string name)
	{
		for (int i = 0; i < SectionHeaders.Length; i++)
		{
			if (SectionHeaders[i].Name.Equals(name, StringComparison.Ordinal))
			{
				return i;
			}
		}
		return -1;
	}

	private void CalculateMetadataLocation(long peImageSize, out int start, out int size)
	{
		if (IsCoffOnly)
		{
			int num = IndexOfSection(".cormeta");
			if (num == -1)
			{
				start = -1;
				size = 0;
				return;
			}
			start = SectionHeaders[num].PointerToRawData;
			size = SectionHeaders[num].SizeOfRawData;
		}
		else
		{
			if (_corHeader == null)
			{
				start = 0;
				size = 0;
				return;
			}
			if (!TryGetDirectoryOffset(_corHeader.MetadataDirectory, out start, canCrossSectionBoundary: false))
			{
				throw new BadImageFormatException(System.SR.MissingDataDirectory);
			}
			size = _corHeader.MetadataDirectory.Size;
		}
		if (start < 0 || start >= peImageSize || size <= 0 || start > peImageSize - size)
		{
			throw new BadImageFormatException(System.SR.InvalidMetadataSectionSpan);
		}
	}
}
