using System;

namespace Discord.Interactions;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class ChoiceDisplayAttribute : Attribute
{
	public string Name { get; }

	public ChoiceDisplayAttribute(string name)
	{
		Name = name;
	}
}
