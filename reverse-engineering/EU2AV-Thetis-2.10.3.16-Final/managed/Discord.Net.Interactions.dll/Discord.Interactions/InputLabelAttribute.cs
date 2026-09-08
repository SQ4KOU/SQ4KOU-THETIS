using System;

namespace Discord.Interactions;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class InputLabelAttribute : Attribute
{
	public string Label { get; }

	public InputLabelAttribute(string label)
	{
		Label = label;
	}
}
