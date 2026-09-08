using System.Collections.Generic;
using System.Data;

namespace Midi2Cat.Data;

public class MappedCommands : List<MappedCommand>
{
	public MappedCommands(DataTable Cmds, DataTable controllerDT)
	{
		foreach (DataRow row in Cmds.Rows)
		{
			if ((CatCmd)row["CmdId"] != CatCmd.None)
			{
				MappedCommand mappedCommand = new MappedCommand(row);
				if (GetDeviceMappings(mappedCommand, controllerDT) <= 0)
				{
					mappedCommand.ControlType = (ControlType)row["ControlType"];
					Add(mappedCommand);
				}
			}
		}
	}

	private int GetDeviceMappings(MappedCommand mappedCmd, DataTable controllerDT)
	{
		int num = 0;
		foreach (DataRow row in controllerDT.Rows)
		{
			if ((int)row["CatCmdId"] == mappedCmd.CmdId)
			{
				MappedCommand mappedCommand = new MappedCommand(mappedCmd);
				mappedCommand.Controller = controllerDT.TableName;
				mappedCommand.ControlName = (string)row["MidiControlName"];
				mappedCommand.ControlType = FixUp.FixControlType((int)row["MidiControlType"]);
				mappedCommand.Remove = "Unmap";
				Add(mappedCommand);
				num++;
			}
		}
		return num;
	}
}
