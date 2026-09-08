using System.Runtime.InteropServices;

namespace SharpDX.DirectWrite;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct FontFeature(FontFeatureTag nameTag, int parameter)
{
	public FontFeatureTag NameTag = nameTag;

	public int Parameter = parameter;
}
