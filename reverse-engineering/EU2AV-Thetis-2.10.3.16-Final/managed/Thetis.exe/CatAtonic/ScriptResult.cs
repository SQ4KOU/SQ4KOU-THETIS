using System.Collections.Generic;

namespace CatAtonic;

public class ScriptResult
{
	public List<ScriptCommand> commands;

	public bool is_valid;

	public string error_message;

	public ScriptResult()
	{
		commands = new List<ScriptCommand>();
		is_valid = true;
		error_message = string.Empty;
	}
}
