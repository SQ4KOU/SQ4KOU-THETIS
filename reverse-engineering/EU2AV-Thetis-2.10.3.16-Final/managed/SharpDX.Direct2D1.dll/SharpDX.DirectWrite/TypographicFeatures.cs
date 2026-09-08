using System;
using System.Runtime.InteropServices;

namespace SharpDX.DirectWrite;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
internal struct TypographicFeatures
{
	public IntPtr Features;

	public int FeatureCount;
}
