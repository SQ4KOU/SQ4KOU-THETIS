using System;

namespace Discord.Interactions;

[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public class MinLengthAttribute : Attribute
{
	public int Length { get; }

	public MinLengthAttribute(int length)
	{
		Length = length;
	}
}
