using System.Collections.Generic;

namespace CatAtonic;

public class ScriptCommand
{
	public ScriptCommandType type;

	public string text;

	public int wait_ms;

	public string variable_name;

	public List<string> guard_true;

	public List<string> guard_false;

	public string ID;

	public int macro;

	public int button_index;
}
