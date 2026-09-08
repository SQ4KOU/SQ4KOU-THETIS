using System;
using System.IO;

namespace SkiaSharp;

public class SKTypeface : SKObject, ISKReferenceCounted
{
	private sealed class SKTypefaceStatic : SKTypeface
	{
		internal SKTypefaceStatic(IntPtr x)
			: base(x, owns: false)
		{
		}

		protected override void Dispose(bool disposing)
		{
		}
	}

	private static readonly SKTypeface defaultTypeface;

	private SKFont font;

	public static SKTypeface Default => defaultTypeface;

	public string FamilyName => (string)SKString.GetObject(SkiaApi.sk_typeface_get_family_name(Handle));

	public SKFontStyle FontStyle => SKFontStyle.GetObject(SkiaApi.sk_typeface_get_fontstyle(Handle));

	public int FontWeight => SkiaApi.sk_typeface_get_font_weight(Handle);

	public int FontWidth => SkiaApi.sk_typeface_get_font_width(Handle);

	public SKFontStyleSlant FontSlant => SkiaApi.sk_typeface_get_font_slant(Handle);

	public bool IsBold => FontStyle.Weight >= 600;

	public bool IsItalic => FontStyle.Slant != SKFontStyleSlant.Upright;

	public bool IsFixedPitch => SkiaApi.sk_typeface_is_fixed_pitch(Handle);

	public int UnitsPerEm => SkiaApi.sk_typeface_get_units_per_em(Handle);

	public int GlyphCount => SkiaApi.sk_typeface_count_glyphs(Handle);

	public string PostScriptName => (string)SKString.GetObject(SkiaApi.sk_typeface_get_post_script_name(Handle));

	public int TableCount => SkiaApi.sk_typeface_count_tables(Handle);

	public unsafe bool HasGetKerningPairAdjustments => SkiaApi.sk_typeface_get_kerning_pair_adjustments(Handle, null, 0, null);

	static SKTypeface()
	{
		defaultTypeface = new SKTypefaceStatic(SkiaApi.sk_typeface_ref_default());
	}

	internal static void EnsureStaticInstanceAreInitialized()
	{
	}

	internal SKTypeface(IntPtr handle, bool owns)
		: base(handle, owns)
	{
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
	}

	public static SKTypeface CreateDefault()
	{
		return GetObject(SkiaApi.sk_typeface_create_default());
	}

	public static SKTypeface FromFamilyName(string familyName, int weight, int width, SKFontStyleSlant slant)
	{
		return FromFamilyName(familyName, new SKFontStyle(weight, width, slant));
	}

	public static SKTypeface FromFamilyName(string familyName)
	{
		return FromFamilyName(familyName, SKFontStyle.Normal);
	}

	public unsafe static SKTypeface FromFamilyName(string familyName, SKFontStyle style)
	{
		if (style == null)
		{
			throw new ArgumentNullException("style");
		}
		fixed (byte* encodedText = StringUtilities.GetEncodedText(familyName, SKTextEncoding.Utf8, addNull: true))
		{
			SKTypeface sKTypeface = GetObject(SkiaApi.sk_typeface_create_from_name(new IntPtr(encodedText), style.Handle));
			sKTypeface?.PreventPublicDisposal();
			return sKTypeface;
		}
	}

	public static SKTypeface FromFamilyName(string familyName, SKFontStyleWeight weight, SKFontStyleWidth width, SKFontStyleSlant slant)
	{
		return FromFamilyName(familyName, (int)weight, (int)width, slant);
	}

	public unsafe static SKTypeface FromFile(string path, int index = 0)
	{
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		fixed (byte* encodedText = StringUtilities.GetEncodedText(path, SKTextEncoding.Utf8, addNull: true))
		{
			return GetObject(SkiaApi.sk_typeface_create_from_file(encodedText, index));
		}
	}

	public static SKTypeface FromStream(Stream stream, int index = 0)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		return FromStream(new SKManagedStream(stream, disposeManagedStream: true), index);
	}

	public static SKTypeface FromStream(SKStreamAsset stream, int index = 0)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		if (stream is SKManagedStream sKManagedStream)
		{
			stream = sKManagedStream.ToMemoryStream();
			sKManagedStream.Dispose();
		}
		SKTypeface sKTypeface = GetObject(SkiaApi.sk_typeface_create_from_stream(stream.Handle, index));
		stream.RevokeOwnership(sKTypeface);
		return sKTypeface;
	}

	public static SKTypeface FromData(SKData data, int index = 0)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		return GetObject(SkiaApi.sk_typeface_create_from_data(data.Handle, index));
	}

	public uint[] GetTableTags()
	{
		if (!TryGetTableTags(out var tags))
		{
			throw new Exception("Unable to read the tables for the file.");
		}
		return tags;
	}

	public unsafe bool TryGetTableTags(out uint[] tags)
	{
		uint[] array = new uint[TableCount];
		fixed (uint* tags2 = array)
		{
			if (SkiaApi.sk_typeface_get_table_tags(Handle, tags2) == 0)
			{
				tags = null;
				return false;
			}
		}
		tags = array;
		return true;
	}

	public int GetTableSize(uint tag)
	{
		return (int)SkiaApi.sk_typeface_get_table_size(Handle, tag);
	}

	public byte[] GetTableData(uint tag)
	{
		if (!TryGetTableData(tag, out var tableData))
		{
			throw new Exception("Unable to read the data table.");
		}
		return tableData;
	}

	public unsafe bool TryGetTableData(uint tag, out byte[] tableData)
	{
		int tableSize = GetTableSize(tag);
		byte[] array = new byte[tableSize];
		fixed (byte* ptr = array)
		{
			if (!TryGetTableData(tag, 0, tableSize, (IntPtr)ptr))
			{
				tableData = null;
				return false;
			}
		}
		tableData = array;
		return true;
	}

	public unsafe bool TryGetTableData(uint tag, int offset, int length, IntPtr tableData)
	{
		IntPtr intPtr = SkiaApi.sk_typeface_get_table_data(Handle, tag, (IntPtr)offset, (IntPtr)length, (void*)tableData);
		return intPtr != IntPtr.Zero;
	}

	public int CountGlyphs(string str)
	{
		return GetFont().CountGlyphs(str);
	}

	public int CountGlyphs(ReadOnlySpan<char> str)
	{
		return GetFont().CountGlyphs(str);
	}

	public int CountGlyphs(byte[] str, SKTextEncoding encoding)
	{
		return GetFont().CountGlyphs(str, encoding);
	}

	public int CountGlyphs(ReadOnlySpan<byte> str, SKTextEncoding encoding)
	{
		return GetFont().CountGlyphs(str, encoding);
	}

	public int CountGlyphs(IntPtr str, int strLen, SKTextEncoding encoding)
	{
		return GetFont().CountGlyphs(str, strLen * encoding.GetCharacterByteSize(), encoding);
	}

	public ushort GetGlyph(int codepoint)
	{
		return GetFont().GetGlyph(codepoint);
	}

	public ushort[] GetGlyphs(ReadOnlySpan<int> codepoints)
	{
		return GetFont().GetGlyphs(codepoints);
	}

	public ushort[] GetGlyphs(string text)
	{
		return GetGlyphs(MemoryExtensions.AsSpan(text));
	}

	public ushort[] GetGlyphs(ReadOnlySpan<char> text)
	{
		using SKFont sKFont = ToFont();
		return sKFont.GetGlyphs(text);
	}

	public ushort[] GetGlyphs(ReadOnlySpan<byte> text, SKTextEncoding encoding)
	{
		using SKFont sKFont = ToFont();
		return sKFont.GetGlyphs(text, encoding);
	}

	public ushort[] GetGlyphs(IntPtr text, int length, SKTextEncoding encoding)
	{
		using SKFont sKFont = ToFont();
		return sKFont.GetGlyphs(text, length * encoding.GetCharacterByteSize(), encoding);
	}

	public bool ContainsGlyph(int codepoint)
	{
		return GetFont().ContainsGlyph(codepoint);
	}

	public bool ContainsGlyphs(ReadOnlySpan<int> codepoints)
	{
		return GetFont().ContainsGlyphs(codepoints);
	}

	public bool ContainsGlyphs(string text)
	{
		return GetFont().ContainsGlyphs(text);
	}

	public bool ContainsGlyphs(ReadOnlySpan<char> text)
	{
		return GetFont().ContainsGlyphs(text);
	}

	public bool ContainsGlyphs(ReadOnlySpan<byte> text, SKTextEncoding encoding)
	{
		return ContainsGlyphs(text, encoding);
	}

	public bool ContainsGlyphs(IntPtr text, int length, SKTextEncoding encoding)
	{
		return GetFont().ContainsGlyphs(text, length * encoding.GetCharacterByteSize(), encoding);
	}

	internal SKFont GetFont()
	{
		return font ?? (font = SKObject.OwnedBy(new SKFont(this), this));
	}

	public SKFont ToFont()
	{
		return new SKFont(this);
	}

	public SKFont ToFont(float size, float scaleX = 1f, float skewX = 0f)
	{
		return new SKFont(this, size, scaleX, skewX);
	}

	public SKStreamAsset OpenStream()
	{
		int ttcIndex;
		return OpenStream(out ttcIndex);
	}

	public unsafe SKStreamAsset OpenStream(out int ttcIndex)
	{
		fixed (int* ttcIndex2 = &ttcIndex)
		{
			return SKStreamAsset.GetObject(SkiaApi.sk_typeface_open_stream(Handle, ttcIndex2));
		}
	}

	public int[] GetKerningPairAdjustments(ReadOnlySpan<ushort> glyphs)
	{
		int[] array = new int[glyphs.Length];
		GetKerningPairAdjustments(glyphs, array);
		return array;
	}

	public unsafe bool GetKerningPairAdjustments(ReadOnlySpan<ushort> glyphs, Span<int> adjustments)
	{
		if (adjustments.Length < glyphs.Length - 1)
		{
			throw new ArgumentException("Length of adjustments must be large enough to hold one adjustment per pair of glyphs (or, glyphs.Length - 1).");
		}
		bool flag;
		fixed (ushort* glyphs2 = glyphs)
		{
			fixed (int* adjustments2 = adjustments)
			{
				flag = SkiaApi.sk_typeface_get_kerning_pair_adjustments(Handle, glyphs2, glyphs.Length, adjustments2);
			}
		}
		if (!flag && glyphs.Length > 1)
		{
			adjustments.Slice(0, glyphs.Length - 1).Clear();
		}
		return flag;
	}

	internal static SKTypeface GetObject(IntPtr handle)
	{
		return SKObject.GetOrAddObject(handle, (IntPtr h, bool o) => new SKTypeface(h, o));
	}
}
