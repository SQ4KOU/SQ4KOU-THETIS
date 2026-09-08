using System;

namespace Midi2Cat.Data;

[AttributeUsage(AttributeTargets.Field)]
public class CatCommandAttribute : Attribute
{
	public string Desc;

	public ControlType ControlType;

	public CatCmd CatCommandId;

	public bool IsToggled;

	public CatCommandAttribute()
	{
	}

	public CatCommandAttribute(string desc, ControlType ctrlType)
	{
		Desc = desc;
		ControlType = ctrlType;
	}

	public CatCommandAttribute(string desc, ControlType ctrlType, bool isToggled)
	{
		Desc = desc;
		ControlType = ctrlType;
		IsToggled = isToggled;
	}

	public override string ToString()
	{
		return Desc;
	}
}
