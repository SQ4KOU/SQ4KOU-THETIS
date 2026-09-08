namespace Midi2Cat.Data;

public class ControllerMapping
{
	public ProcessMidiMessageHandler onProcessMidiMessage;

	public int MidiControlId { get; set; }

	public string MidiControlName { get; set; }

	public ControlType MidiControlType { get; set; }

	public int MinValue { get; set; }

	public int MaxValue { get; set; }

	public CatCmd CatCmdId { get; set; }

	public string MidiOutCmdDown { get; set; }

	public string MidiOutCmdUp { get; set; }

	public string MidiOutCmdSetValue { get; set; }

	public CatCommandAttribute CatCmd { get; set; }

	public override string ToString()
	{
		return MidiControlName + " -> " + CatCmd.Desc;
	}
}
