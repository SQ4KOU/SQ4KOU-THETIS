using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Runtime.InteropServices;

namespace Svg;

public class SvgFontManager : IDisposable
{
	private static readonly string[][] defaultLocalizedFamilyNames = new string[3][]
	{
		new string[2] { "Meiryo", "メイリオ" },
		new string[2] { "MS Gothic", "ＭＳ ゴシック" },
		new string[2] { "MS Mincho", "ＭＳ 明朝" }
	};

	private readonly List<FontFamily> families = new List<FontFamily>();

	private readonly List<string[]> localizedFamilyNames = new List<string[]>();

	public static List<string[]> LocalizedFamilyNames { get; private set; } = new List<string[]>();

	public static List<string> PrivateFontPathList { get; private set; } = new List<string>();

	public static List<byte[]> PrivateFontDataList { get; private set; } = new List<byte[]>();

	internal SvgFontManager()
	{
		families.AddRange(FontFamily.Families);
		using (PrivateFontCollection privateFontCollection = new PrivateFontCollection())
		{
			foreach (string privateFontPath in PrivateFontPathList)
			{
				privateFontCollection.AddFontFile(privateFontPath);
			}
			foreach (byte[] privateFontData in PrivateFontDataList)
			{
				IntPtr intPtr = IntPtr.Zero;
				try
				{
					intPtr = Marshal.AllocCoTaskMem(privateFontData.Length);
					Marshal.Copy(privateFontData, 0, intPtr, privateFontData.Length);
					privateFontCollection.AddMemoryFont(intPtr, privateFontData.Length);
				}
				finally
				{
					if (intPtr != IntPtr.Zero)
					{
						Marshal.FreeCoTaskMem(intPtr);
					}
				}
			}
			families.AddRange(privateFontCollection.Families);
		}
		localizedFamilyNames.AddRange(LocalizedFamilyNames);
		localizedFamilyNames.AddRange(defaultLocalizedFamilyNames);
	}

	public FontFamily FindFont(string name)
	{
		if (name == null)
		{
			return null;
		}
		IEnumerable<string> enumerable = localizedFamilyNames.Find((string[] f) => f.Contains(name, StringComparer.CurrentCultureIgnoreCase));
		foreach (string familyName in enumerable ?? Enumerable.Repeat(name, 1))
		{
			FontFamily fontFamily = families.Find((FontFamily f) => f.Name.Equals(familyName, StringComparison.CurrentCultureIgnoreCase));
			if (fontFamily != null)
			{
				return fontFamily;
			}
		}
		return name.ToLower() switch
		{
			"serif" => FontFamily.GenericSerif, 
			"sans-serif" => FontFamily.GenericSansSerif, 
			"monospace" => FontFamily.GenericMonospace, 
			_ => null, 
		};
	}

	public void Dispose()
	{
		families.ForEach(delegate(FontFamily f)
		{
			f.Dispose();
		});
	}
}
