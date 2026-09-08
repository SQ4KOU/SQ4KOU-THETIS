using System;

namespace Discord.Interactions;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class ComponentInteractionAttribute : Attribute
{
	public string CustomId { get; }

	public bool IgnoreGroupNames { get; }

	public RunMode RunMode { get; }

	public bool TreatAsRegex { get; set; }

	public ComponentInteractionAttribute(string customId, bool ignoreGroupNames = false, RunMode runMode = RunMode.Default)
	{
		CustomId = customId;
		IgnoreGroupNames = ignoreGroupNames;
		RunMode = runMode;
	}
}
