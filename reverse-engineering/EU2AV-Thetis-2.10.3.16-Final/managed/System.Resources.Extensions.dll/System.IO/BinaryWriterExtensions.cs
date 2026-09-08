namespace System.IO;

internal static class BinaryWriterExtensions
{
	public static void Write7BitEncodedInt(this BinaryWriter writer, int value)
	{
		uint num;
		for (num = (uint)value; num >= 128; num >>= 7)
		{
			writer.Write((byte)(num | 0x80));
		}
		writer.Write((byte)num);
	}
}
