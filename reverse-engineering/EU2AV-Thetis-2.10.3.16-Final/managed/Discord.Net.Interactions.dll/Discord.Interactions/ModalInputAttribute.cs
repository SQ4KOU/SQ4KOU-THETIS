using System;

namespace Discord.Interactions;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public abstract class ModalInputAttribute : Attribute
{
	public string CustomId { get; }

	public abstract ComponentType ComponentType { get; }

	protected ModalInputAttribute(string customId)
	{
		CustomId = customId;
	}
}
