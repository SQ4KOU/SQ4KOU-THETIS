using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Discord.API;

namespace Discord.Rest;

public class RestMessageComponentData : IComponentInteractionData, IDiscordInteractionData
{
	public string CustomId { get; }

	public ComponentType Type { get; }

	public IReadOnlyCollection<string> Values { get; }

	public IReadOnlyCollection<RestChannel> Channels { get; }

	public IReadOnlyCollection<RestUser> Users { get; }

	public IReadOnlyCollection<RestRole> Roles { get; }

	public IReadOnlyCollection<RestGuildUser> Members { get; }

	IReadOnlyCollection<IChannel> IComponentInteractionData.Channels => Channels;

	IReadOnlyCollection<IUser> IComponentInteractionData.Users => Users;

	IReadOnlyCollection<IRole> IComponentInteractionData.Roles => Roles;

	IReadOnlyCollection<IGuildUser> IComponentInteractionData.Members => Members;

	public string Value { get; }

	internal RestMessageComponentData(MessageComponentInteractionData model, BaseDiscordClient discord, IGuild guild)
	{
		CustomId = model.CustomId;
		Type = model.ComponentType;
		Values = model.Values.GetValueOrDefault();
		Value = model.Value.GetValueOrDefault();
		if (!model.Resolved.IsSpecified)
		{
			return;
		}
		IReadOnlyCollection<RestUser> readOnlyCollection2;
		if (!model.Resolved.Value.Users.IsSpecified)
		{
			IReadOnlyCollection<RestUser> readOnlyCollection = Array.Empty<RestUser>();
			readOnlyCollection2 = readOnlyCollection;
		}
		else
		{
			IReadOnlyCollection<RestUser> readOnlyCollection = model.Resolved.Value.Users.Value.Select((KeyValuePair<string, User> user) => RestUser.Create(discord, user.Value)).ToImmutableArray();
			readOnlyCollection2 = readOnlyCollection;
		}
		Users = readOnlyCollection2;
		object obj;
		if (!model.Resolved.Value.Members.IsSpecified)
		{
			obj = null;
		}
		else
		{
			IReadOnlyCollection<RestGuildUser> readOnlyCollection3 = model.Resolved.Value.Members.Value.Select(delegate(KeyValuePair<string, GuildMember> member)
			{
				member.Value.User = model.Resolved.Value.Users.Value.First((KeyValuePair<string, User> u) => u.Key == member.Key).Value;
				return RestGuildUser.Create(discord, guild, member.Value);
			}).ToImmutableArray();
			obj = readOnlyCollection3;
		}
		Members = (IReadOnlyCollection<RestGuildUser>)obj;
		IReadOnlyCollection<RestChannel> readOnlyCollection5;
		if (!model.Resolved.Value.Channels.IsSpecified)
		{
			IReadOnlyCollection<RestChannel> readOnlyCollection4 = Array.Empty<RestChannel>();
			readOnlyCollection5 = readOnlyCollection4;
		}
		else
		{
			IReadOnlyCollection<RestChannel> readOnlyCollection4 = model.Resolved.Value.Channels.Value.Select((KeyValuePair<string, Channel> channel) => (channel.Value.Type == ChannelType.DM) ? RestDMChannel.Create(discord, channel.Value) : RestChannel.Create(discord, channel.Value)).ToImmutableArray();
			readOnlyCollection5 = readOnlyCollection4;
		}
		Channels = readOnlyCollection5;
		IReadOnlyCollection<RestRole> readOnlyCollection7;
		if (!model.Resolved.Value.Roles.IsSpecified)
		{
			IReadOnlyCollection<RestRole> readOnlyCollection6 = Array.Empty<RestRole>();
			readOnlyCollection7 = readOnlyCollection6;
		}
		else
		{
			IReadOnlyCollection<RestRole> readOnlyCollection6 = model.Resolved.Value.Roles.Value.Select((KeyValuePair<string, Role> role) => RestRole.Create(discord, guild, role.Value)).ToImmutableArray();
			readOnlyCollection7 = readOnlyCollection6;
		}
		Roles = readOnlyCollection7;
	}

	internal RestMessageComponentData(IInteractableComponent component, BaseDiscordClient discord, IGuild guild)
	{
		CustomId = component.CustomId;
		Type = component.Type;
		if (component is Discord.API.TextInputComponent textInputComponent)
		{
			Value = textInputComponent.Value.Value;
		}
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
			IReadOnlyCollection<RestUser> readOnlyCollection = select.Resolved.Value.Users.Value.Select((KeyValuePair<string, User> user) => RestUser.Create(discord, user.Value)).ToImmutableArray();
			obj = readOnlyCollection;
		}
		Users = (IReadOnlyCollection<RestUser>)obj;
		object obj2;
		if (!select.Resolved.Value.Members.IsSpecified)
		{
			obj2 = null;
		}
		else
		{
			IReadOnlyCollection<RestGuildUser> readOnlyCollection2 = select.Resolved.Value.Members.Value.Select(delegate(KeyValuePair<string, GuildMember> member)
			{
				member.Value.User = select.Resolved.Value.Users.Value.First((KeyValuePair<string, User> u) => u.Key == member.Key).Value;
				return RestGuildUser.Create(discord, guild, member.Value);
			}).ToImmutableArray();
			obj2 = readOnlyCollection2;
		}
		Members = (IReadOnlyCollection<RestGuildUser>)obj2;
		object obj3;
		if (!select.Resolved.Value.Channels.IsSpecified)
		{
			obj3 = null;
		}
		else
		{
			IReadOnlyCollection<RestChannel> readOnlyCollection3 = select.Resolved.Value.Channels.Value.Select((KeyValuePair<string, Channel> channel) => RestChannel.Create(discord, channel.Value)).ToImmutableArray();
			obj3 = readOnlyCollection3;
		}
		Channels = (IReadOnlyCollection<RestChannel>)obj3;
		object obj4;
		if (!select.Resolved.Value.Roles.IsSpecified)
		{
			obj4 = null;
		}
		else
		{
			IReadOnlyCollection<RestRole> readOnlyCollection4 = select.Resolved.Value.Roles.Value.Select((KeyValuePair<string, Role> role) => RestRole.Create(discord, guild, role.Value)).ToImmutableArray();
			obj4 = readOnlyCollection4;
		}
		Roles = (IReadOnlyCollection<RestRole>)obj4;
	}
}
