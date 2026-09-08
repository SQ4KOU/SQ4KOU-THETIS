using System;
using Discord.Utils;

namespace Discord;

public class ButtonBuilder : IInteractableComponentBuilder, IMessageComponentBuilder
{
	public const int MaxButtonLabelLength = 80;

	private string _label;

	private string _customId;

	public ComponentType Type => ComponentType.Button;

	public string Label
	{
		get
		{
			return _label;
		}
		set
		{
			if (value != null)
			{
				Preconditions.AtLeast(value.Length, 1, "Label");
				Preconditions.AtMost(value.Length, 80, "Label");
			}
			_label = value;
		}
	}

	public string CustomId
	{
		get
		{
			return _customId;
		}
		set
		{
			if (value != null)
			{
				Preconditions.AtLeast(value.Length, 1, "CustomId");
				Preconditions.AtMost(value.Length, 100, "CustomId");
			}
			_customId = value;
		}
	}

	public ButtonStyle Style { get; set; }

	public IEmote Emote { get; set; }

	public string Url { get; set; }

	public bool IsDisabled { get; set; }

	public ulong? SkuId { get; set; }

	public int? Id { get; set; }

	public ButtonBuilder()
	{
	}

	public ButtonBuilder(string label = null, string customId = null, ButtonStyle style = ButtonStyle.Primary, string url = null, IEmote emote = null, bool isDisabled = false, ulong? skuId = null, int? id = null)
	{
		CustomId = customId;
		Style = style;
		Url = url;
		Label = label;
		IsDisabled = isDisabled;
		Emote = emote;
		SkuId = skuId;
		Id = id;
	}

	public ButtonBuilder(ButtonComponent button)
	{
		CustomId = button.CustomId;
		Style = button.Style;
		Url = button.Url;
		Label = button.Label;
		IsDisabled = button.IsDisabled;
		Emote = button.Emote;
		SkuId = button.SkuId;
		Id = button.Id;
	}

	public static ButtonBuilder CreateLinkButton(string label, string url, IEmote emote = null)
	{
		return new ButtonBuilder(label, null, ButtonStyle.Link, url, emote);
	}

	public static ButtonBuilder CreateDangerButton(string label, string customId, IEmote emote = null)
	{
		return new ButtonBuilder(label, customId, ButtonStyle.Danger, null, emote);
	}

	public static ButtonBuilder CreatePrimaryButton(string label, string customId, IEmote emote = null)
	{
		return new ButtonBuilder(label, customId, ButtonStyle.Primary, null, emote);
	}

	public static ButtonBuilder CreateSecondaryButton(string label, string customId, IEmote emote = null)
	{
		return new ButtonBuilder(label, customId, ButtonStyle.Secondary, null, emote);
	}

	public static ButtonBuilder CreateSuccessButton(string label, string customId, IEmote emote = null)
	{
		return new ButtonBuilder(label, customId, ButtonStyle.Success, null, emote);
	}

	public static ButtonBuilder CreatePremiumButton(string label, ulong skuId, IEmote emote = null)
	{
		return new ButtonBuilder(label, null, ButtonStyle.Success, null, emote, isDisabled: false, skuId);
	}

	public ButtonBuilder WithLabel(string label)
	{
		Label = label;
		return this;
	}

	public ButtonBuilder WithStyle(ButtonStyle style)
	{
		Style = style;
		return this;
	}

	public ButtonBuilder WithEmote(IEmote emote)
	{
		Emote = emote;
		return this;
	}

	public ButtonBuilder WithUrl(string url)
	{
		Url = url;
		return this;
	}

	public ButtonBuilder WithCustomId(string id)
	{
		CustomId = id;
		return this;
	}

	public ButtonBuilder WithDisabled(bool isDisabled)
	{
		IsDisabled = isDisabled;
		return this;
	}

	public ButtonBuilder WithSkuId(ulong? skuId)
	{
		SkuId = skuId;
		return this;
	}

	public ButtonComponent Build()
	{
		int num = 0;
		if (!string.IsNullOrWhiteSpace(Url))
		{
			num++;
		}
		if (!string.IsNullOrWhiteSpace(CustomId))
		{
			num++;
		}
		if (SkuId.HasValue)
		{
			num++;
		}
		if ((num > 1 || num == 0) ? true : false)
		{
			throw new InvalidOperationException("A button must contain either a URL, CustomId or SkuId, but not multiple of them!");
		}
		switch (Style)
		{
		case (ButtonStyle)0:
			throw new ArgumentException("A button must have a style.", "Style");
		case ButtonStyle.Primary:
		case ButtonStyle.Secondary:
		case ButtonStyle.Success:
		case ButtonStyle.Danger:
			if (string.IsNullOrWhiteSpace(Label) && Emote == null)
			{
				throw new InvalidOperationException("A button must have an Emote or a label!");
			}
			if (string.IsNullOrWhiteSpace(CustomId))
			{
				throw new InvalidOperationException("Non-link and non-premium buttons must have a custom id associated with them");
			}
			break;
		case ButtonStyle.Link:
			if (string.IsNullOrWhiteSpace(Label) && Emote == null)
			{
				throw new InvalidOperationException("A button must have an Emote or a label!");
			}
			if (string.IsNullOrWhiteSpace(Url))
			{
				throw new InvalidOperationException("Link buttons must have a link associated with them");
			}
			UrlValidation.ValidateButton(Url);
			break;
		case ButtonStyle.Premium:
			if (!SkuId.HasValue)
			{
				throw new InvalidOperationException("Premium buttons must have a sku id associated with them");
			}
			break;
		}
		return new ButtonComponent(Style, Label, Emote, CustomId, Url, IsDisabled, SkuId, Id);
	}

	IMessageComponent IMessageComponentBuilder.Build()
	{
		return Build();
	}
}
