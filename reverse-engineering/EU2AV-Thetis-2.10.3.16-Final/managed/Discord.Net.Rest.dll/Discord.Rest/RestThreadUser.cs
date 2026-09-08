using System;
using System.Threading.Tasks;
using Discord.API;

namespace Discord.Rest;

public class RestThreadUser : RestEntity<ulong>, IThreadUser, IMentionable
{
	public IThreadChannel Thread { get; }

	public DateTimeOffset ThreadJoinedAt { get; private set; }

	public IGuild Guild { get; }

	public RestGuildUser GuildUser { get; private set; }

	public string Mention => MentionUtils.MentionUser(base.Id);

	IGuildUser IThreadUser.GuildUser => GuildUser;

	internal RestThreadUser(BaseDiscordClient discord, IGuild guild, IThreadChannel channel, ulong id)
		: base(discord, id)
	{
		Guild = guild;
		Thread = channel;
	}

	internal static RestThreadUser Create(BaseDiscordClient client, IGuild guild, ThreadMember model, IThreadChannel channel)
	{
		RestThreadUser restThreadUser = new RestThreadUser(client, guild, channel, model.UserId.Value);
		restThreadUser.Update(model);
		return restThreadUser;
	}

	internal void Update(ThreadMember model)
	{
		ThreadJoinedAt = model.JoinTimestamp;
		if (model.GuildMember.IsSpecified)
		{
			GuildUser = RestGuildUser.Create(base.Discord, Guild, model.GuildMember.Value);
		}
	}

	public Task<IGuildUser> GetGuildUser()
	{
		return Guild.GetUserAsync(base.Id);
	}
}
