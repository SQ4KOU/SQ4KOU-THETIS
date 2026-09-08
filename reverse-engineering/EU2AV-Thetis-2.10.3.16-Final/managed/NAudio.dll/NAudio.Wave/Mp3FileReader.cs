using System.IO;

namespace NAudio.Wave;

public class Mp3FileReader : Mp3FileReaderBase
{
	public Mp3FileReader(string mp3FileName)
		: base(File.OpenRead(mp3FileName), CreateAcmFrameDecompressor, ownInputStream: true)
	{
	}

	public Mp3FileReader(Stream inputStream)
		: base(inputStream, CreateAcmFrameDecompressor, ownInputStream: false)
	{
	}

	public static IMp3FrameDecompressor CreateAcmFrameDecompressor(WaveFormat mp3Format)
	{
		return new AcmMp3FrameDecompressor(mp3Format);
	}
}
