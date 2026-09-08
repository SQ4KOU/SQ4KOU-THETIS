using System;

namespace Discord.Commands;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class CommandAttribute : Attribute
{
	public string Text { get; }

	public RunMode RunMode { get; set; }

	public bool? IgnoreExtraArgs { get; }

	public string Summary { get; set; }

	public string[] Aliases { get; set; }

	public string Remarks { get; set; }

	public CommandAttribute()
	{
		Text = null;
	}

	public CommandAttribute(string text)
	{
		Text = text;
	}

	public CommandAttribute(string text, bool ignoreExtraArgs)
	{
		Text = text;
		IgnoreExtraArgs = ignoreExtraArgs;
	}

	public CommandAttribute(string text, bool ignoreExtraArgs, string summary = null, string[] aliases = null, string remarks = null)
	{
		Text = text;
		IgnoreExtraArgs = ignoreExtraArgs;
		Summary = summary;
		Aliases = aliases;
		Remarks = remarks;
	}
}
