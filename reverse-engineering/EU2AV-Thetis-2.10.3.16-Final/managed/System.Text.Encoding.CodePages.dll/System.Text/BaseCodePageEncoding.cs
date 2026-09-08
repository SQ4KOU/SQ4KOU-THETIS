using System.Buffers.Binary;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using Microsoft.Win32.SafeHandles;

namespace System.Text;

internal abstract class BaseCodePageEncoding : System.Text.EncodingNLS, ISerializable
{
	[StructLayout(LayoutKind.Explicit)]
	internal struct CodePageDataFileHeader
	{
		[FieldOffset(0)]
		internal char TableName;

		[FieldOffset(32)]
		internal ushort Version;

		[FieldOffset(40)]
		internal short CodePageCount;

		[FieldOffset(42)]
		internal short unused1;
	}

	[StructLayout(LayoutKind.Explicit, Pack = 2)]
	internal struct CodePageIndex
	{
		[FieldOffset(0)]
		internal char CodePageName;

		[FieldOffset(32)]
		internal short CodePage;

		[FieldOffset(34)]
		internal short ByteCount;

		[FieldOffset(36)]
		internal int Offset;
	}

	[StructLayout(LayoutKind.Explicit)]
	internal struct CodePageHeader
	{
		[FieldOffset(0)]
		internal char CodePageName;

		[FieldOffset(32)]
		internal ushort VersionMajor;

		[FieldOffset(34)]
		internal ushort VersionMinor;

		[FieldOffset(36)]
		internal ushort VersionRevision;

		[FieldOffset(38)]
		internal ushort VersionBuild;

		[FieldOffset(40)]
		internal short CodePage;

		[FieldOffset(42)]
		internal short ByteCount;

		[FieldOffset(44)]
		internal char UnicodeReplace;

		[FieldOffset(46)]
		internal ushort ByteReplace;
	}

	internal const string CODE_PAGE_DATA_FILE_NAME = "codepages.nlp";

	protected int dataTableCodePage;

	protected int iExtraBytes;

	protected char[] arrayUnicodeBestFit;

	protected char[] arrayBytesBestFit;

	private const int CODEPAGE_DATA_FILE_HEADER_SIZE = 44;

	private const int CODEPAGE_HEADER_SIZE = 48;

	private static readonly byte[] s_codePagesDataHeader = new byte[44];

	protected static Stream s_codePagesEncodingDataStream = GetEncodingDataStream("codepages.nlp");

	protected static readonly object s_streamLock = new object();

	protected byte[] m_codePageHeader = new byte[48];

	protected int m_firstDataWordOffset;

	protected int m_dataSize;

	protected SafeAllocHHandle safeNativeMemoryHandle;

	internal BaseCodePageEncoding(int codepage)
		: this(codepage, codepage)
	{
	}

	internal BaseCodePageEncoding(int codepage, int dataCodePage)
		: base(codepage, new System.Text.InternalEncoderBestFitFallback(null), new System.Text.InternalDecoderBestFitFallback(null))
	{
		((System.Text.InternalEncoderBestFitFallback)base.EncoderFallback).encoding = this;
		((System.Text.InternalDecoderBestFitFallback)base.DecoderFallback).encoding = this;
		dataTableCodePage = dataCodePage;
		LoadCodePageTables();
	}

	internal BaseCodePageEncoding(int codepage, int dataCodePage, EncoderFallback enc, DecoderFallback dec)
		: base(codepage, enc, dec)
	{
		dataTableCodePage = dataCodePage;
		LoadCodePageTables();
	}

	void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
	{
		throw new PlatformNotSupportedException();
	}

	private unsafe static void ReadCodePageDataFileHeader(Stream stream, byte[] codePageDataFileHeader)
	{
		stream.Read(codePageDataFileHeader, 0, codePageDataFileHeader.Length);
		if (BitConverter.IsLittleEndian)
		{
			return;
		}
		fixed (byte* ptr = &codePageDataFileHeader[0])
		{
			CodePageDataFileHeader* ptr2 = (CodePageDataFileHeader*)ptr;
			char* ptr3 = &ptr2->TableName;
			for (int i = 0; i < 16; i++)
			{
				ptr3[i] = (char)BinaryPrimitives.ReverseEndianness(ptr3[i]);
			}
			ushort* ptr4 = &ptr2->Version;
			for (int j = 0; j < 4; j++)
			{
				ptr4[j] = BinaryPrimitives.ReverseEndianness(ptr4[j]);
			}
			ptr2->CodePageCount = BinaryPrimitives.ReverseEndianness(ptr2->CodePageCount);
		}
	}

	private unsafe static void ReadCodePageIndex(Stream stream, byte[] codePageIndex)
	{
		stream.Read(codePageIndex, 0, codePageIndex.Length);
		if (BitConverter.IsLittleEndian)
		{
			return;
		}
		fixed (byte* ptr = &codePageIndex[0])
		{
			CodePageIndex* ptr2 = (CodePageIndex*)ptr;
			char* ptr3 = &ptr2->CodePageName;
			for (int i = 0; i < 16; i++)
			{
				ptr3[i] = (char)BinaryPrimitives.ReverseEndianness(ptr3[i]);
			}
			ptr2->CodePage = BinaryPrimitives.ReverseEndianness(ptr2->CodePage);
			ptr2->ByteCount = BinaryPrimitives.ReverseEndianness(ptr2->ByteCount);
			ptr2->Offset = BinaryPrimitives.ReverseEndianness(ptr2->Offset);
		}
	}

	private unsafe static void ReadCodePageHeader(Stream stream, byte[] codePageHeader)
	{
		stream.Read(codePageHeader, 0, codePageHeader.Length);
		if (BitConverter.IsLittleEndian)
		{
			return;
		}
		fixed (byte* ptr = &codePageHeader[0])
		{
			CodePageHeader* ptr2 = (CodePageHeader*)ptr;
			char* ptr3 = &ptr2->CodePageName;
			for (int i = 0; i < 16; i++)
			{
				ptr3[i] = (char)BinaryPrimitives.ReverseEndianness(ptr3[i]);
			}
			ptr2->VersionMajor = BinaryPrimitives.ReverseEndianness(ptr2->VersionMajor);
			ptr2->VersionMinor = BinaryPrimitives.ReverseEndianness(ptr2->VersionMinor);
			ptr2->VersionRevision = BinaryPrimitives.ReverseEndianness(ptr2->VersionRevision);
			ptr2->VersionBuild = BinaryPrimitives.ReverseEndianness(ptr2->VersionBuild);
			ptr2->CodePage = BinaryPrimitives.ReverseEndianness(ptr2->CodePage);
			ptr2->ByteCount = BinaryPrimitives.ReverseEndianness(ptr2->ByteCount);
			ptr2->UnicodeReplace = (char)BinaryPrimitives.ReverseEndianness(ptr2->UnicodeReplace);
			ptr2->ByteReplace = BinaryPrimitives.ReverseEndianness(ptr2->ByteReplace);
		}
	}

	internal static Stream GetEncodingDataStream(string tableName)
	{
		Stream obj = typeof(CodePagesEncodingProvider).Assembly.GetManifestResourceStream(tableName) ?? throw new InvalidOperationException();
		ReadCodePageDataFileHeader(obj, s_codePagesDataHeader);
		return obj;
	}

	private void LoadCodePageTables()
	{
		if (!FindCodePage(dataTableCodePage))
		{
			throw new NotSupportedException(System.SR.Format(System.SR.NotSupported_NoCodepageData, CodePage));
		}
		LoadManagedCodePage();
	}

	private unsafe bool FindCodePage(int codePage)
	{
		byte[] array = new byte[sizeof(CodePageIndex)];
		lock (s_streamLock)
		{
			s_codePagesEncodingDataStream.Seek(44L, SeekOrigin.Begin);
			int codePageCount;
			fixed (byte* ptr = &s_codePagesDataHeader[0])
			{
				CodePageDataFileHeader* ptr2 = (CodePageDataFileHeader*)ptr;
				codePageCount = ptr2->CodePageCount;
			}
			fixed (byte* ptr = &array[0])
			{
				CodePageIndex* ptr3 = (CodePageIndex*)ptr;
				for (int i = 0; i < codePageCount; i++)
				{
					ReadCodePageIndex(s_codePagesEncodingDataStream, array);
					if (ptr3->CodePage == codePage)
					{
						long position = s_codePagesEncodingDataStream.Position;
						s_codePagesEncodingDataStream.Seek(ptr3->Offset, SeekOrigin.Begin);
						ReadCodePageHeader(s_codePagesEncodingDataStream, m_codePageHeader);
						m_firstDataWordOffset = (int)s_codePagesEncodingDataStream.Position;
						if (i == codePageCount - 1)
						{
							m_dataSize = (int)(s_codePagesEncodingDataStream.Length - ptr3->Offset - m_codePageHeader.Length);
						}
						else
						{
							s_codePagesEncodingDataStream.Seek(position, SeekOrigin.Begin);
							int offset = ptr3->Offset;
							ReadCodePageIndex(s_codePagesEncodingDataStream, array);
							m_dataSize = ptr3->Offset - offset - m_codePageHeader.Length;
						}
						return true;
					}
				}
			}
		}
		return false;
	}

	internal unsafe static int GetCodePageByteSize(int codePage)
	{
		byte[] array = new byte[sizeof(CodePageIndex)];
		lock (s_streamLock)
		{
			s_codePagesEncodingDataStream.Seek(44L, SeekOrigin.Begin);
			int codePageCount;
			fixed (byte* ptr = &s_codePagesDataHeader[0])
			{
				CodePageDataFileHeader* ptr2 = (CodePageDataFileHeader*)ptr;
				codePageCount = ptr2->CodePageCount;
			}
			fixed (byte* ptr = &array[0])
			{
				CodePageIndex* ptr3 = (CodePageIndex*)ptr;
				for (int i = 0; i < codePageCount; i++)
				{
					ReadCodePageIndex(s_codePagesEncodingDataStream, array);
					if (ptr3->CodePage == codePage)
					{
						return ptr3->ByteCount;
					}
				}
			}
		}
		return 0;
	}

	protected abstract void LoadManagedCodePage();

	protected unsafe byte* GetNativeMemory(int iSize)
	{
		if (safeNativeMemoryHandle == null)
		{
			byte* ptr = (byte*)(void*)Marshal.AllocHGlobal(iSize);
			safeNativeMemoryHandle = new SafeAllocHHandle((IntPtr)ptr);
		}
		return (byte*)(void*)safeNativeMemoryHandle.DangerousGetHandle();
	}

	protected abstract void ReadBestFitTable();

	internal char[] GetBestFitUnicodeToBytesData()
	{
		if (arrayUnicodeBestFit == null)
		{
			ReadBestFitTable();
		}
		return arrayUnicodeBestFit;
	}

	internal char[] GetBestFitBytesToUnicodeData()
	{
		if (arrayBytesBestFit == null)
		{
			ReadBestFitTable();
		}
		return arrayBytesBestFit;
	}

	internal void CheckMemorySection()
	{
		if (safeNativeMemoryHandle != null && safeNativeMemoryHandle.DangerousGetHandle() == IntPtr.Zero)
		{
			LoadManagedCodePage();
		}
	}
}
