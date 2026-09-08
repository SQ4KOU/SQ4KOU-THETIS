using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Discord.API;
using Discord.Rest;

namespace Discord.WebSocket;

public class SocketMessageComponentData : IComponentInteractionData, IDiscordInteractionData
{
	public string CustomId { get; }

	public ComponentType Type { get; }

	public IReadOnlyCollection<string> Values { get; }

	public IReadOnlyCollection<SocketChannel> Channels { get; }

	public IReadOnlyCollection<IUser> Users { get; }

	public IReadOnlyCollection<SocketRole> Roles { get; }

	public IReadOnlyCollection<SocketGuildUser> Members { get; }

	IReadOnlyCollection<IChannel> IComponentInteractionData.Channels => Channels;

	IReadOnlyCollection<IUser> IComponentInteractionData.Users => Users;

	IReadOnlyCollection<IRole> IComponentInteractionData.Roles => Roles;

	IReadOnlyCollection<IGuildUser> IComponentInteractionData.Members => Members;

	public string Value { get; }

	internal SocketMessageComponentData(MessageComponentInteractionData model, DiscordSocketClient discord, ClientState state, SocketGuild guild, User dmUser)
	{
		CustomId = model.CustomId;
		Type = model.ComponentType;
		Values = model.Values.GetValueOrDefault();
		Value = model.Value.GetValueOrDefault();
		if (!model.Resolved.IsSpecified)
		{
			return;
		}
		object obj;
		if (!model.Resolved.Value.Users.IsSpecified)
		{
			obj = null;
		}
		else
		{
			IReadOnlyCollection<IUser> readOnlyCollection = model.Resolved.Value.Users.Value.Select(delegate(KeyValuePair<string, User> user)
			{
				IUser user2 = state.GetUser(user.Value.Id);
				return user2 ?? RestUser.Create(discord, user.Value);
			}).ToImmutableArray();
			obj = readOnlyCollection;
		}
		Users = (IReadOnlyCollection<IUser>)obj;
		object obj2;
		if (!model.Resolved.Value.Members.IsSpecified)
		{
			obj2 = null;
		}
		else
		{
			IReadOnlyCollection<SocketGuildUser> readOnlyCollection2 = model.Resolved.Value.Members.Value.Select(delegate(KeyValuePair<string, GuildMember> member)
			{
				member.Value.User = model.Resolved.Value.Users.Value.First((KeyValuePair<string, User> u) => u.Key == member.Key).Value;
				return SocketGuildUser.Create(guild, state, member.Value);
			}).ToImmutableArray();
			obj2 = readOnlyCollection2;
		}
		Members = (IReadOnlyCollection<SocketGuildUser>)obj2;
		object obj3;
		if (!model.Resolved.Value.Channels.IsSpecified)
		{
			obj3 = null;
		}
		else
		{
			IReadOnlyCollection<SocketChannel> readOnlyCollection3 = model.Resolved.Value.Channels.Value.Select((KeyValuePair<string, Channel> channel) => (channel.Value.Type == ChannelType.DM) ? ((SocketChannel)SocketDMChannel.Create(discord, state, channel.Value.Id, dmUser)) : ((SocketChannel)SocketGuildChannel.Create(guild, state, channel.Value))).ToImmutableArray();
			obj3 = readOnlyCollection3;
		}
		Channels = (IReadOnlyCollection<SocketChannel>)obj3;
		object obj4;
		if (!model.Resolved.Value.Roles.IsSpecified)
		{
			obj4 = null;
		}
		else
		{
			IReadOnlyCollection<SocketRole> readOnlyCollection4 = model.Resolved.Value.Roles.Value.Select((KeyValuePair<string, Role> role) => SocketRole.Create(guild, state, role.Value)).ToImmutableArray();
			obj4 = readOnlyCollection4;
		}
		Roles = (IReadOnlyCollection<SocketRole>)obj4;
	}

	internal SocketMessageComponentData(IInteractableComponent component, DiscordSocketClient discord, ClientState state, SocketGuild guild, User dmUser)
	{
		CustomId = component.CustomId;
		Type = component.Type;
		Value = ((component.Type == ComponentType.TextInput) ? ((TextInputComponent)component).Value : null);
		Discord.API.SelectMenuComponent select = component as Discord.API.SelectMenuComponent;
		if (select == null)
		{
			return;
		}
		Values = select.Values.GetValueOrDefault(null);
		if (!select.Resolved.IsSpecified)
		{
			return;
		}
		object obj;
		if (!select.Resolved.Value.Users.IsSpecified)
		{
			obj = null;
		}
		else
		{
			IReadOnlyCollection<IUser> readOnlyCollection = select.Resolved.Value.Users.Value.Select(delegate(KeyValuePair<string, User> user)
			{
				IUser user2 = state.GetUser(user.Value.Id);
				return user2 ?? RestUser.Create(discord, user.Value);
			}).ToImmutableArray();
			obj = readOnlyCollection;
		}
		Users = (IReadOnlyCollection<IUser>)obj;
		object obj2;
		if (!select.Resolved.Value.Members.IsSpecified)
		{
			obj2 = null;
		}
		else
		{
			IReadOnlyCollection<SocketGuildUser> readOnlyCollection2 = select.Resolved.Value.Members.Value.Select(delegate(KeyValuePair<string, GuildMember> member)
			{
				member.Value.User = select.Resolved.Value.Users.Value.First((KeyValuePair<string, User> u) => u.Key == member.Key).Value;
				return SocketGuildUser.Create(guild, state, member.Value);
			}).ToImmutableArray();
			obj2 = readOnlyCollection2;
		}
		Members = (IReadOnlyCollection<SocketGuildUser>)obj2;
		object obj3;
		if (!select.Resolved.Value.Channels.IsSpecified)
		{
			obj3 = null;
		}
		else
		{
			IReadOnlyCollection<SocketChannel> readOnlyCollection3 = select.Resolved.Value.Channels.Value.Select((KeyValuePair<string, Channel> channel) => (channel.Value.Type == ChannelType.DM) ? ((SocketChannel)SocketDMChannel.Create(discord, state, channel.Value.Id, dmUser)) : ((SocketChannel)SocketGuildChannel.Create(guild, state, channel.Value))).ToImmutableArray();
			obj3 = readOnlyCollection3;
		}
		Channels = (IReadOnlyCollection<SocketChannel>)obj3;
		object obj4;
		if (!select.Resolved.Value.Roles.IsSpecified)
		{
			obj4 = null;
		}
		else
		{
			IReadOnlyCollection<SocketRole> readOnlyCollection4 = select.Resolved.Value.Roles.Value.Select((KeyValuePair<string, Role> role) => SocketRole.Create(guild, state, role.Value)).ToImmutableArray();
			obj4 = readOnlyCollection4;
		}
		Roles = (IReadOnlyCollection<SocketRole>)obj4;
	}
}
