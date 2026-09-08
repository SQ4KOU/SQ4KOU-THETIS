using System;
using System.Threading.Tasks;
using Discord.API;

namespace Discord.Rest;

public class RestGlobalCommand : RestApplicationCommand
{
	internal RestGlobalCommand(BaseDiscordClient client, ulong id)
		: base(client, id)
	{
	}

	internal static RestGlobalCommand Create(BaseDiscordClient client, ApplicationCommand model)
	{
		RestGlobalCommand restGlobalCommand = new RestGlobalCommand(client, model.Id);
		restGlobalCommand.Update(model);
		return restGlobalCommand;
	}

	public override Task DeleteAsync(RequestOptions options = null)
	{
		return InteractionHelper.DeleteGlobalCommandAsync(base.Discord, this);
	}

	public override async Task ModifyAsync<TArg>(Action<TArg> func, RequestOptions options = null)
	{
		Update(await InteractionHelper.ModifyGlobalCommandAsync(base.Discord, this, func, options).ConfigureAwait(continueOnCapturedContext: false));
	}
}
