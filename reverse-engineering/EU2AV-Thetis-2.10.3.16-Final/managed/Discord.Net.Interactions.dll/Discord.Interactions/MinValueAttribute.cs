using System;

namespace Discord.Interactions;

[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public sealed class MinValueAttribute : Attribute
{
	public double Value { get; }

	public MinValueAttribute(double value)
	{
		Value = value;
	}
}
