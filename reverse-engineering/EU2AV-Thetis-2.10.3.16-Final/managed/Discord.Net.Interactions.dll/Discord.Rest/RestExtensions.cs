using System;
using Discord.Interactions;

namespace Discord.Rest;

public static class RestExtensions
{
	public static string RespondWithModal<T>(this RestInteraction interaction, string customId, RequestOptions options = null, Action<ModalBuilder> modifyModal = null) where T : class, IModal
	{
		if (!ModalUtils.TryGet<T>(out var modalInfo))
		{
			throw new ArgumentException($"{typeof(T).FullName} isn't referenced by any registered Modal Interaction Command and doesn't have a cached {typeof(ModalInfo)}");
		}
		Modal modal = modalInfo.ToModal(customId, modifyModal);
		return interaction.RespondWithModal(modal, options);
	}

	public static string RespondWithModal<T>(this RestInteraction interaction, string customId, T modal, RequestOptions options = null, Action<ModalBuilder> modifyModal = null) where T : class, IModal
	{
		if (!ModalUtils.TryGet<T>(out var modalInfo))
		{
			throw new ArgumentException($"{typeof(T).FullName} isn't referenced by any registered Modal Interaction Command and doesn't have a cached {typeof(ModalInfo)}");
		}
		ModalBuilder modalBuilder = new ModalBuilder(modal.Title, customId);
		foreach (InputComponentInfo component in modalInfo.Components)
		{
			if (component is TextInputComponentInfo textInputComponentInfo)
			{
				modalBuilder.AddTextInput(textInputComponentInfo.Label, textInputComponentInfo.CustomId, textInputComponentInfo.Style, textInputComponentInfo.Placeholder, textInputComponentInfo.IsRequired ? new int?(textInputComponentInfo.MinLength) : ((int?)null), textInputComponentInfo.MaxLength, textInputComponentInfo.IsRequired, textInputComponentInfo.Getter(modal) as string);
				continue;
			}
			throw new InvalidOperationException(component.GetType().FullName + " isn't a valid component info class");
		}
		modifyModal?.Invoke(modalBuilder);
		return interaction.RespondWithModal(modalBuilder.Build(), options);
	}
}
