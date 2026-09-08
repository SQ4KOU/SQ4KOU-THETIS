namespace Discord;

public enum InteractionType : byte
{
	Ping = 1,
	ApplicationCommand,
	MessageComponent,
	ApplicationCommandAutocomplete,
	ModalSubmit
}
