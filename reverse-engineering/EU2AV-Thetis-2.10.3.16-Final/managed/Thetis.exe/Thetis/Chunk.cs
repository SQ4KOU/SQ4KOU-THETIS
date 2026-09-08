using System.IO;

namespace Thetis;

public class Chunk
{
	public int chunk_id;

	public static Chunk ReadChunk(ref BinaryReader reader)
	{
		int num = reader.ReadInt32();
		switch (num)
		{
		case 1179011410:
			return new RIFFChunk
			{
				chunk_id = num,
				file_size = reader.ReadInt32(),
				riff_type = reader.ReadInt32()
			};
		case 544501094:
		{
			fmtChunk obj = new fmtChunk
			{
				chunk_id = num,
				chunk_size = reader.ReadInt32(),
				format = reader.ReadInt16(),
				channels = reader.ReadInt16(),
				sample_rate = reader.ReadInt32(),
				bytes_per_sec = reader.ReadInt32(),
				block_align = reader.ReadInt16(),
				bits_per_sample = reader.ReadInt16()
			};
			long num2 = obj.chunk_size - 16;
			if (num2 > 0)
			{
				long num3 = reader.BaseStream.Position + num2;
				if (num3 > reader.BaseStream.Length)
				{
					num3 = reader.BaseStream.Length;
				}
				reader.BaseStream.Position = num3;
			}
			return obj;
		}
		case 1635017060:
			return new dataChunk
			{
				chunk_id = num,
				chunk_size = reader.ReadInt32()
			};
		default:
			return new Chunk
			{
				chunk_id = num
			};
		}
	}
}
