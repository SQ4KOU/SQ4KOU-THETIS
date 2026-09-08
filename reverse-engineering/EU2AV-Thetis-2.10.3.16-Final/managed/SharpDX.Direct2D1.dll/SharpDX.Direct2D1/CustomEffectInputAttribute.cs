using System;

namespace SharpDX.Direct2D1;

[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
public class CustomEffectInputAttribute : Attribute
{
	private string input;

	public string Input => input;

	public CustomEffectInputAttribute(string input)
	{
		this.input = input;
	}
}
