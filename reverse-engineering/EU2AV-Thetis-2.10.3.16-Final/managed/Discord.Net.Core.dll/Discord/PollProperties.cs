using System.Collections.Generic;

namespace Discord;

public class PollProperties
{
	public PollMediaProperties Question { get; set; }

	public List<PollMediaProperties> Answers { get; set; }

	public uint Duration { get; set; }

	public bool AllowMultiselect { get; set; }

	public PollLayout LayoutType { get; set; }
}
