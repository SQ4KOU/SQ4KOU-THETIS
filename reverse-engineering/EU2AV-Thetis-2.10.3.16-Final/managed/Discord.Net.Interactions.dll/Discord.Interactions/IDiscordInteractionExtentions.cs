using System;
using System.Threading.Tasks;

namespace Discord.Interactions;

public static class IDiscordInteractionExtentions
{
	public static Task RespondWithModalAsync<T>(this IDiscordInteraction interaction, string customId, RequestOptions options = null, Action<ModalBuilder> modifyModal = null) where T : class, IModal
	{
		if (!ModalUtils.TryGet<T>(out var modalInfo))
		{
			throw new ArgumentException($"{typeof(T).FullName} isn't referenced by any registered Modal Interaction Command and doesn't have a cached {typeof(ModalInfo)}");
		}
		return SendModalResponseAsync(interaction, customId, modalInfo, options, modifyModal);
	}

	public static Task RespondWithModalAsync<T>(this IDiscordInteraction interaction, string customId, InteractionService interactionService, RequestOptions options = null, Action<ModalBuilder> modifyModal = null) where T : class, IModal
	{
		ModalInfo orAdd = ModalUtils.GetOrAdd<T>(interactionService);
		return SendModalResponseAsync(interaction, customId, orAdd, options, modifyModal);
	}

	public static Task RespondWithModalAsync<T>(this IDiscordInteraction interaction, string customId, T modal, RequestOptions options = null, Action<ModalBuilder> modifyModal = null) where T : class, IModal
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
				object obj = textInputComponentInfo.Getter(modal);
				string value = (textInputComponentInfo.TypeOverridesToString ? obj?.ToString() : (obj as string));
				modalBuilder.AddTextInput(textInputComponentInfo.Label, textInputComponentInfo.CustomId, textInputComponentInfo.Style, textInputComponentInfo.Placeholder, textInputComponentInfo.IsRequired ? new int?(textInputComponentInfo.MinLength) : ((int?)null), textInputComponentInfo.MaxLength, textInputComponentInfo.IsRequired, value);
				continue;
			}
			throw new InvalidOperationException(component.GetType().FullName + " isn't a valid component info class");
		}
		modifyModal?.Invoke(modalBuilder);
		return interaction.RespondWithModalAsync(modalBuilder.Build(), options);
	}

	private static Task SendModalResponseAsync(IDiscordInteraction interaction, string customId, ModalInfo modalInfo, RequestOptions options = null, Action<ModalBuilder> modifyModal = null)
	{
		Modal modal = modalInfo.ToModal(customId, modifyModal);
		return interaction.RespondWithModalAsync(modal, options);
	}
}
