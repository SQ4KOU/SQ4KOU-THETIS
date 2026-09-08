using System;

namespace Discord.Commands;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class RemarksAttribute : Attribute
{
	public string Text { get; }

	public RemarksAttribute(string text)
	{
		Text = text;
	}
}
