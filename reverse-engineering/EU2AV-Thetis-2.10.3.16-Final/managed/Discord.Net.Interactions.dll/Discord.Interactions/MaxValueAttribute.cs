using System;

namespace Discord.Interactions;

[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public sealed class MaxValueAttribute : Attribute
{
	public double Value { get; }

	public MaxValueAttribute(double value)
	{
		Value = value;
	}
}
