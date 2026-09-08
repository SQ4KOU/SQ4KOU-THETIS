using System;
using System.Collections.Generic;
using System.Globalization;

namespace CatAtonic;

public class CATScriptInterpreter
{
	private struct if_ctx
	{
		public List<string> conds_seen;

		public string branch_cond;

		public bool seen_else;

		public HashSet<string> used_conds;
	}

	public ScriptResult run(string script)
	{
		ScriptResult scriptResult = new ScriptResult();
		Tokeniser tokeniser = new Tokeniser(script);
		List<if_ctx> list = new List<if_ctx>();
		bool flag = false;
		string text = null;
		while (true)
		{
			Token token = tokeniser.next();
			if (token.type == TokenType.Eof)
			{
				break;
			}
			if (token.type == TokenType.Error)
			{
				scriptResult.is_valid = false;
				scriptResult.error_message = token.text;
				return scriptResult;
			}
			if (token.type == TokenType.Bracket)
			{
				if (flag)
				{
					scriptResult.is_valid = false;
					scriptResult.error_message = "STATE must be followed by a CAT command";
					return scriptResult;
				}
				string text2 = token.text.Trim();
				string text3 = text2.ToUpperInvariant();
				if (text3 == "ELSE")
				{
					if (list.Count == 0)
					{
						scriptResult.is_valid = false;
						scriptResult.error_message = "ELSE without IF";
						return scriptResult;
					}
					if_ctx value = list[list.Count - 1];
					if (value.seen_else)
					{
						scriptResult.is_valid = false;
						scriptResult.error_message = "multiple ELSE in IF";
						return scriptResult;
					}
					value.seen_else = true;
					value.branch_cond = null;
					list[list.Count - 1] = value;
					continue;
				}
				if (text3.StartsWith("ELSE_IF_", StringComparison.Ordinal) || text3.StartsWith("ELSEIF_", StringComparison.Ordinal))
				{
					if (list.Count == 0)
					{
						scriptResult.is_valid = false;
						scriptResult.error_message = "ELSE_IF without IF";
						return scriptResult;
					}
					if_ctx value2 = list[list.Count - 1];
					if (value2.seen_else)
					{
						scriptResult.is_valid = false;
						scriptResult.error_message = "ELSE_IF after ELSE";
						return scriptResult;
					}
					int startIndex = (text3.StartsWith("ELSEIF_", StringComparison.Ordinal) ? 7 : 8);
					string text4 = text2.Substring(startIndex);
					string item = text4.ToUpperInvariant();
					if (value2.used_conds != null && value2.used_conds.Contains(item))
					{
						scriptResult.is_valid = false;
						scriptResult.error_message = "duplicate condition in IF chain: " + text4;
						return scriptResult;
					}
					if (value2.used_conds == null)
					{
						value2.used_conds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
					}
					value2.used_conds.Add(item);
					if (value2.conds_seen == null)
					{
						value2.conds_seen = new List<string>();
					}
					value2.conds_seen.Add(text4);
					value2.branch_cond = text4;
					list[list.Count - 1] = value2;
					continue;
				}
				switch (text3)
				{
				case "END":
				case "ENDIF":
					if (list.Count == 0)
					{
						scriptResult.is_valid = false;
						scriptResult.error_message = "END without IF";
						return scriptResult;
					}
					list.RemoveAt(list.Count - 1);
					continue;
				case "STATE":
					flag = true;
					continue;
				}
				if (text3.StartsWith("VAR_", StringComparison.Ordinal))
				{
					if (text != null)
					{
						scriptResult.is_valid = false;
						scriptResult.error_message = "multiple VAR before CAT";
						return scriptResult;
					}
					text = text2.Substring(4);
					if (text.Length != 0)
					{
						continue;
					}
					scriptResult.is_valid = false;
					scriptResult.error_message = "empty VAR name";
					return scriptResult;
				}
				if (text3 == "WAIT")
				{
					ScriptCommand scriptCommand = new ScriptCommand();
					scriptCommand.type = ScriptCommandType.Wait;
					scriptCommand.text = "[WAIT]";
					scriptCommand.wait_ms = 100;
					scriptCommand.variable_name = null;
					set_guard_from_stack(scriptCommand, list);
					scriptResult.commands.Add(scriptCommand);
					continue;
				}
				if (text3.StartsWith("WAIT", StringComparison.Ordinal))
				{
					string s = text2.Substring(4);
					int result = 0;
					if (!int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out result) || result < 0)
					{
						scriptResult.is_valid = false;
						scriptResult.error_message = "invalid WAIT value";
						return scriptResult;
					}
					ScriptCommand scriptCommand2 = new ScriptCommand();
					scriptCommand2.type = ScriptCommandType.Wait;
					scriptCommand2.text = "[WAIT" + result.ToString(CultureInfo.InvariantCulture) + "]";
					scriptCommand2.wait_ms = result;
					scriptCommand2.variable_name = null;
					set_guard_from_stack(scriptCommand2, list);
					scriptResult.commands.Add(scriptCommand2);
					continue;
				}
				if (text3.StartsWith("IF_", StringComparison.Ordinal))
				{
					string text5 = text2.Substring(3);
					string item2 = text5.ToUpperInvariant();
					if_ctx item3 = default(if_ctx);
					item3.conds_seen = new List<string>();
					item3.conds_seen.Add(text5);
					item3.branch_cond = text5;
					item3.seen_else = false;
					item3.used_conds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
					item3.used_conds.Add(item2);
					list.Add(item3);
					continue;
				}
				scriptResult.is_valid = false;
				scriptResult.error_message = "unknown command in []: " + text2;
				return scriptResult;
			}
			if (token.type == TokenType.Cat)
			{
				ScriptCommand scriptCommand3 = new ScriptCommand();
				if (flag)
				{
					scriptCommand3.type = ScriptCommandType.CatState;
					scriptCommand3.text = token.text;
					scriptCommand3.wait_ms = 0;
					scriptCommand3.variable_name = text;
					text = null;
					flag = false;
				}
				else if (text != null)
				{
					scriptCommand3.type = ScriptCommandType.CatMessageVar;
					scriptCommand3.text = token.text;
					scriptCommand3.wait_ms = 0;
					scriptCommand3.variable_name = text;
					text = null;
				}
				else
				{
					scriptCommand3.type = ScriptCommandType.CatMessage;
					scriptCommand3.text = token.text;
					scriptCommand3.wait_ms = 0;
					scriptCommand3.variable_name = null;
				}
				set_guard_from_stack(scriptCommand3, list);
				scriptResult.commands.Add(scriptCommand3);
			}
		}
		if (list.Count != 0)
		{
			scriptResult.is_valid = false;
			scriptResult.error_message = "missing [END] to a starting [IF]";
			return scriptResult;
		}
		if (flag)
		{
			scriptResult.is_valid = false;
			scriptResult.error_message = "STATE without following CAT command";
			return scriptResult;
		}
		if (text != null)
		{
			scriptResult.is_valid = false;
			scriptResult.error_message = "VAR without following CAT command";
			return scriptResult;
		}
		return scriptResult;
	}

	public List<ScriptCommand> filter_now(ScriptResult r, Func<string, bool> eval)
	{
		List<ScriptCommand> list = new List<ScriptCommand>();
		if (r == null || !r.is_valid)
		{
			return list;
		}
		int i = 0;
		int count = r.commands.Count;
		bool flag = false;
		for (; i < count; i++)
		{
			ScriptCommand scriptCommand = r.commands[i];
			if (!guard_holds(scriptCommand, eval))
			{
				continue;
			}
			if (scriptCommand.type == ScriptCommandType.CatState)
			{
				if (flag)
				{
					return new List<ScriptCommand>();
				}
				flag = true;
			}
			list.Add(scriptCommand);
		}
		return list;
	}

	public int total_wait_milliseconds_now(ScriptResult r, Func<string, bool> eval)
	{
		if (r == null || !r.is_valid)
		{
			return 0;
		}
		int num = 0;
		int i = 0;
		for (int count = r.commands.Count; i < count; i++)
		{
			ScriptCommand scriptCommand = r.commands[i];
			if (scriptCommand.type == ScriptCommandType.Wait && guard_holds(scriptCommand, eval))
			{
				num += scriptCommand.wait_ms;
			}
		}
		return num;
	}

	public string cat_state_command_now(ScriptResult r, Func<string, bool> eval)
	{
		if (r == null || !r.is_valid)
		{
			return string.Empty;
		}
		int i = 0;
		for (int count = r.commands.Count; i < count; i++)
		{
			ScriptCommand scriptCommand = r.commands[i];
			if (scriptCommand.type == ScriptCommandType.CatState && guard_holds(scriptCommand, eval))
			{
				return scriptCommand.text;
			}
		}
		return string.Empty;
	}

	private static bool guard_holds(ScriptCommand c, Func<string, bool> eval)
	{
		if (c.guard_true != null)
		{
			int i = 0;
			for (int count = c.guard_true.Count; i < count; i++)
			{
				string arg = c.guard_true[i];
				if (!eval(arg))
				{
					return false;
				}
			}
		}
		if (c.guard_false != null)
		{
			int j = 0;
			for (int count2 = c.guard_false.Count; j < count2; j++)
			{
				string arg2 = c.guard_false[j];
				if (eval(arg2))
				{
					return false;
				}
			}
		}
		return true;
	}

	private static void set_guard_from_stack(ScriptCommand cmd, List<if_ctx> stack)
	{
		List<string> list = null;
		List<string> list2 = null;
		int i = 0;
		for (int count = stack.Count; i < count; i++)
		{
			if_ctx if_ctx2 = stack[i];
			if (if_ctx2.branch_cond == null)
			{
				if (if_ctx2.conds_seen == null)
				{
					continue;
				}
				int j = 0;
				for (int count2 = if_ctx2.conds_seen.Count; j < count2; j++)
				{
					if (list2 == null)
					{
						list2 = new List<string>();
					}
					if (!contains_string(list2, if_ctx2.conds_seen[j]))
					{
						list2.Add(if_ctx2.conds_seen[j]);
					}
				}
				continue;
			}
			if (list == null)
			{
				list = new List<string>();
			}
			if (!contains_string(list, if_ctx2.branch_cond))
			{
				list.Add(if_ctx2.branch_cond);
			}
			if (if_ctx2.conds_seen == null)
			{
				continue;
			}
			int k = 0;
			for (int num = if_ctx2.conds_seen.Count - 1; k < num; k++)
			{
				if (list2 == null)
				{
					list2 = new List<string>();
				}
				if (!contains_string(list2, if_ctx2.conds_seen[k]))
				{
					list2.Add(if_ctx2.conds_seen[k]);
				}
			}
		}
		cmd.guard_true = list ?? new List<string>();
		cmd.guard_false = list2 ?? new List<string>();
	}

	private static bool contains_string(List<string> list, string s)
	{
		int i = 0;
		for (int count = list.Count; i < count; i++)
		{
			if (string.Equals(list[i], s, StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
		}
		return false;
	}
}
