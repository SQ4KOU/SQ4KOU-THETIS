using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace System.Text;

public sealed class CodePagesEncodingProvider : EncodingProvider
{
	private static readonly EncodingProvider s_singleton = new CodePagesEncodingProvider();

	private readonly Dictionary<int, Encoding> _encodings = new Dictionary<int, Encoding>();

	private readonly ReaderWriterLockSlim _cacheLock = new ReaderWriterLockSlim();

	private const int ISCIIAssemese = 57006;

	private const int ISCIIBengali = 57003;

	private const int ISCIIDevanagari = 57002;

	private const int ISCIIGujarathi = 57010;

	private const int ISCIIKannada = 57008;

	private const int ISCIIMalayalam = 57009;

	private const int ISCIIOriya = 57007;

	private const int ISCIIPanjabi = 57011;

	private const int ISCIITamil = 57004;

	private const int ISCIITelugu = 57005;

	private const int ISOKorean = 50225;

	private const int ChineseHZ = 52936;

	private const int ISO2022JP = 50220;

	private const int ISO2022JPESC = 50221;

	private const int ISO2022JPSISO = 50222;

	private const int ISOSimplifiedCN = 50227;

	private const int EUCJP = 51932;

	private const int CodePageMacGB2312 = 10008;

	private const int CodePageMacKorean = 10003;

	private const int CodePageGB2312 = 20936;

	private const int CodePageDLLKorean = 20949;

	private const int GB18030 = 54936;

	private const int DuplicateEUCCN = 51936;

	private const int EUCKR = 51949;

	private const int EUCCN = 936;

	private const int ISO_8859_8I = 38598;

	private const int ISO_8859_8_Visual = 28598;

	public static EncodingProvider Instance => s_singleton;

	private static int SystemDefaultCodePage
	{
		get
		{
			if (!global::Interop.Kernel32.TryGetACPCodePage(out var codePage))
			{
				return 0;
			}
			return codePage;
		}
	}

	internal CodePagesEncodingProvider()
	{
	}

	public override Encoding? GetEncoding(int codepage)
	{
		if (codepage < 0 || codepage > 65535)
		{
			return null;
		}
		if (codepage == 0)
		{
			int systemDefaultCodePage = SystemDefaultCodePage;
			if (systemDefaultCodePage == 0)
			{
				return null;
			}
			return GetEncoding(systemDefaultCodePage);
		}
		Encoding value = null;
		_cacheLock.EnterUpgradeableReadLock();
		try
		{
			if (_encodings.TryGetValue(codepage, out value))
			{
				return value;
			}
			switch (System.Text.BaseCodePageEncoding.GetCodePageByteSize(codepage))
			{
			case 1:
				value = new System.Text.SBCSCodePageEncoding(codepage);
				break;
			case 2:
				value = new System.Text.DBCSCodePageEncoding(codepage);
				break;
			default:
				value = GetEncodingRare(codepage);
				if (value == null)
				{
					return null;
				}
				break;
			}
			_cacheLock.EnterWriteLock();
			try
			{
				if (_encodings.TryGetValue(codepage, out var value2))
				{
					return value2;
				}
				_encodings.Add(codepage, value);
				return value;
			}
			finally
			{
				_cacheLock.ExitWriteLock();
			}
		}
		finally
		{
			_cacheLock.ExitUpgradeableReadLock();
		}
	}

	public override Encoding? GetEncoding(string name)
	{
		int codePageFromName = System.Text.EncodingTable.GetCodePageFromName(name);
		if (codePageFromName == 0)
		{
			return null;
		}
		return GetEncoding(codePageFromName);
	}

	private static Encoding GetEncodingRare(int codepage)
	{
		Encoding result = null;
		switch (codepage)
		{
		case 57002:
		case 57003:
		case 57004:
		case 57005:
		case 57006:
		case 57007:
		case 57008:
		case 57009:
		case 57010:
		case 57011:
			result = new System.Text.ISCIIEncoding(codepage);
			break;
		case 10008:
			result = new System.Text.DBCSCodePageEncoding(10008, 20936);
			break;
		case 10003:
			result = new System.Text.DBCSCodePageEncoding(10003, 20949);
			break;
		case 54936:
			result = new System.Text.GB18030Encoding();
			break;
		case 50220:
		case 50221:
		case 50222:
		case 50225:
		case 52936:
			result = new System.Text.ISO2022Encoding(codepage);
			break;
		case 50227:
		case 51936:
			result = new System.Text.DBCSCodePageEncoding(codepage, 936);
			break;
		case 51932:
			result = new System.Text.EUCJPEncoding();
			break;
		case 51949:
			result = new System.Text.DBCSCodePageEncoding(codepage, 20949);
			break;
		case 38598:
			result = new System.Text.SBCSCodePageEncoding(codepage, 28598);
			break;
		}
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal unsafe static ref T GetNonNullPinnableReference<T>(T[] array) where T : struct
	{
		if (array.Length == 0)
		{
			return ref Unsafe.AsRef<T>((void*)1);
		}
		return ref array[0];
	}
}
