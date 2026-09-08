using System.Data;

namespace Midi2Cat.Data;

public class MappedCommand
{
	public int CmdId { get; set; }

	public string Description { get; set; }

	public ControlType ControlType { get; set; }

	public string Controller { get; set; }

	public string ControlName { get; set; }

	public string Remove { get; set; }

	public MappedCommand(DataRow row)
	{
		CmdId = (int)row["CmdId"];
		Description = (string)row["CmdDescription"];
		Controller = "";
		ControlName = "";
		ControlType = ControlType.Unknown;
		Remove = "";
	}

	public MappedCommand(MappedCommand src)
	{
		CmdId = src.CmdId;
		Description = src.Description;
		Controller = src.Controller;
		ControlName = src.ControlName;
		ControlType = src.ControlType;
		Remove = src.Remove;
	}
}
