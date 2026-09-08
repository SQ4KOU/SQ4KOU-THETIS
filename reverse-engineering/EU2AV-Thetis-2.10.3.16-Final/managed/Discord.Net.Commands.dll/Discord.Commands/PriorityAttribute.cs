using System;

namespace Discord.Commands;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class PriorityAttribute : Attribute
{
	public int Priority { get; }

	public PriorityAttribute(int priority)
	{
		Priority = priority;
	}
}
