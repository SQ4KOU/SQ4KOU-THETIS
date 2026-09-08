using System;

namespace Discord.Interactions;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class NsfwCommandAttribute : Attribute
{
	public bool IsNsfw { get; }

	public NsfwCommandAttribute(bool isNsfw)
	{
		IsNsfw = isNsfw;
	}
}
