namespace Midi2Cat.IO;

public class ParsedMidiMessage
{
	public int Event;

	public int Channel;

	public int Data1;

	public int Data2;

	public bool Valid;

	public string ErrMsg;
}
