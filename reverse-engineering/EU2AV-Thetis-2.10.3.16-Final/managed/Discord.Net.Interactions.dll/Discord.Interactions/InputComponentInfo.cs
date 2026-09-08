using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using Discord.Interactions.Builders;

namespace Discord.Interactions;

public abstract class InputComponentInfo
{
	private Lazy<Func<object, object>> _getter;

	internal Func<object, object> Getter => _getter.Value;

	public ModalInfo Modal { get; }

	public string CustomId { get; }

	public string Label { get; }

	public bool IsRequired { get; }

	public ComponentType ComponentType { get; }

	public Type Type { get; }

	public PropertyInfo PropertyInfo { get; }

	public ComponentTypeConverter TypeConverter { get; }

	public object DefaultValue { get; }

	public IReadOnlyCollection<Attribute> Attributes { get; }

	protected InputComponentInfo(IInputComponentBuilder builder, ModalInfo modal)
	{
		Modal = modal;
		CustomId = builder.CustomId;
		Label = builder.Label;
		IsRequired = builder.IsRequired;
		ComponentType = builder.ComponentType;
		Type = builder.Type;
		PropertyInfo = builder.PropertyInfo;
		TypeConverter = builder.TypeConverter;
		DefaultValue = builder.DefaultValue;
		Attributes = builder.Attributes.ToImmutableArray();
		_getter = new Lazy<Func<object, object>>(() => ReflectionUtils<object>.CreateLambdaPropertyGetter(Modal.Type, PropertyInfo));
	}
}
