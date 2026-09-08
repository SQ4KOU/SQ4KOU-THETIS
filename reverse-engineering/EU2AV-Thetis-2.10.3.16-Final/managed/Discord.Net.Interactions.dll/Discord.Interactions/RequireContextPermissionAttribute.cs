using System;
using System.Threading.Tasks;

namespace Discord.Interactions;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public class RequireContextPermissionAttribute : PreconditionAttribute
{
	public GuildPermission? GuildPermission { get; }

	public ChannelPermission? ChannelPermission { get; }

	public string NotAGuildErrorMessage { get; set; }

	public RequireContextPermissionAttribute(GuildPermission permission)
	{
		GuildPermission = permission;
		ChannelPermission = null;
	}

	public RequireContextPermissionAttribute(ChannelPermission permission)
	{
		ChannelPermission = permission;
		GuildPermission = null;
	}

	public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo command, IServiceProvider services)
	{
		if (GuildPermission.HasValue)
		{
			if (!context.Interaction.GuildId.HasValue)
			{
				return Task.FromResult(PreconditionResult.FromError(NotAGuildErrorMessage ?? "Command must be used in a guild channel."));
			}
			if (!context.Interaction.Permissions.Has(GuildPermission.Value))
			{
				return Task.FromResult(PreconditionResult.FromError(ErrorMessage ?? $"Bot requires guild permission {GuildPermission.Value}."));
			}
		}
		if (ChannelPermission.HasValue)
		{
			ChannelPermissions channelPermissions = new ChannelPermissions(context.Interaction.Permissions.RawValue);
			if (!channelPermissions.Has(ChannelPermission.Value))
			{
				return Task.FromResult(PreconditionResult.FromError(ErrorMessage ?? $"Bot requires channel permission {ChannelPermission.Value}."));
			}
		}
		return Task.FromResult(PreconditionResult.FromSuccess());
	}
}
