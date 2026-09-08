using System;

namespace SharpDX.Direct2D1;

[AttributeUsage(AttributeTargets.Property, Inherited = true)]
public class PropertyBindingAttribute : Attribute
{
	private readonly PropertyType bindingType;

	private readonly int order;

	private readonly string min;

	private readonly string max;

	private readonly string defaultValue;

	public PropertyType BindingType => bindingType;

	public int Order => order;

	public string DisplayName { get; set; }

	public PropertyType Type { get; set; }

	public string Min => min;

	public string Max => max;

	public string Default => defaultValue;

	public PropertyBindingAttribute(int order, string min, string max, string defaultValue)
		: this(PropertyType.Unknown, order, min, max, defaultValue)
	{
	}

	public PropertyBindingAttribute(PropertyType bindingType, int order, string min, string max, string defaultValue)
	{
		this.bindingType = bindingType;
		this.order = order;
		this.min = min;
		this.max = max;
		this.defaultValue = defaultValue;
	}
}
