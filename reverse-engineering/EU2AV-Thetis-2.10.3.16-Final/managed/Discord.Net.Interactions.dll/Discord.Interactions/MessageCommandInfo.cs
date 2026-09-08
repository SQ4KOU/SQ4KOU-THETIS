using System;
using System.Threading.Tasks;
using Discord.Interactions.Builders;

namespace Discord.Interactions;

public class MessageCommandInfo : ContextCommandInfo
{
	internal MessageCommandInfo(ContextCommandBuilder builder, ModuleInfo module, InteractionService commandService)
		: base(builder, module, commandService)
	{
	}

	public override Task<IResult> ExecuteAsync(IInteractionContext context, IServiceProvider services)
	{
		if (!(context.Interaction is IMessageCommandInteraction))
		{
			return Task.FromResult((IResult)ExecuteResult.FromError(InteractionCommandError.ParseFailed, "Provided IInteractionContext doesn't belong to a Message Command Interation"));
		}
		return base.ExecuteAsync(context, services);
	}

	protected override Task<IResult> ParseArgumentsAsync(IInteractionContext context, IServiceProvider services)
	{
		try
		{
			return Task.FromResult((IResult)ParseResult.FromSuccess(new object[1] { (context.Interaction as IMessageCommandInteraction).Data.Message }));
		}
		catch (Exception exception)
		{
			return Task.FromResult((IResult)ParseResult.FromError(exception));
		}
	}

	protected override string GetLogString(IInteractionContext context)
	{
		if (context.Guild != null)
		{
			return $"Message Command: \"{ToString()}\" for {context.User} in {context.Guild}/{context.Channel}";
		}
		return $"Message Command: \"{ToString()}\" for {context.User} in {context.Channel}";
	}
}
