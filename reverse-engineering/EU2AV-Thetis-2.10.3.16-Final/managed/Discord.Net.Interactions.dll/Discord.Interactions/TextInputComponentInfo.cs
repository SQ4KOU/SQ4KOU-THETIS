using System;
using Discord.Interactions.Builders;

namespace Discord.Interactions;

public class TextInputComponentInfo : InputComponentInfo
{
	private readonly Lazy<bool> _typeOverridesToString;

	internal bool TypeOverridesToString => _typeOverridesToString.Value;

	public TextInputStyle Style { get; }

	public string Placeholder { get; }

	public int MinLength { get; }

	public int MaxLength { get; }

	public string InitialValue { get; }

	internal TextInputComponentInfo(TextInputComponentBuilder builder, ModalInfo modal)
		: base(builder, modal)
	{
		Style = builder.Style;
		Placeholder = builder.Placeholder;
		MinLength = builder.MinLength;
		MaxLength = builder.MaxLength;
		InitialValue = builder.InitialValue;
		_typeOverridesToString = new Lazy<bool>(() => ReflectionUtils<object>.OverridesToString(base.Type));
	}
}
