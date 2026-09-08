using System.Runtime.InteropServices;

namespace System.Text;

internal static class EncodingExtensions
{
	public unsafe static int GetBytes(this Encoding encoding, ReadOnlySpan<char> chars, Span<byte> bytes)
	{
		fixed (char* reference = &MemoryMarshal.GetReference(chars))
		{
			fixed (byte* reference2 = &MemoryMarshal.GetReference(bytes))
			{
				return encoding.GetBytes(reference, chars.Length, reference2, bytes.Length);
			}
		}
	}
}
