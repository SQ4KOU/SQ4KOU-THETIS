using System.ComponentModel;

namespace Midi2Cat.Data;

public class MidiDiagList : BindingList<MidiDiagItem>
{
	private int seqnum;

	public new void Add(MidiDiagItem item)
	{
		item.SeqNum = seqnum++;
		base.Add(item);
	}
}
