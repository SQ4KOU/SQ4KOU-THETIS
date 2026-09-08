using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord.Interactions;

internal sealed class DefaultArrayComponentConverter<T> : ComponentTypeConverter<T>
{
	private readonly TypeReader _typeReader;

	private readonly Type _underlyingType;

	public DefaultArrayComponentConverter(InteractionService interactionService)
	{
		if (!typeof(T).IsArray)
		{
			throw new InvalidOperationException("DefaultArrayComponentConverter cannot be used to convert a non-array type.");
		}
		_underlyingType = typeof(T).GetElementType();
		_typeReader = ((!typeof(IUser).IsAssignableFrom(_underlyingType) && !typeof(IChannel).IsAssignableFrom(_underlyingType) && !typeof(IMentionable).IsAssignableFrom(_underlyingType) && !typeof(IRole).IsAssignableFrom(_underlyingType)) ? interactionService.GetTypeReader(_underlyingType) : null);
	}

	public override async Task<TypeConverterResult> ReadAsync(IInteractionContext context, IComponentInteractionData option, IServiceProvider services)
	{
		List<object> objs = new List<object>();
		if (_typeReader != null && option.Values.Count > 0)
		{
			foreach (string value in option.Values)
			{
				TypeConverterResult result = await _typeReader.ReadAsync(context, value, services).ConfigureAwait(continueOnCapturedContext: false);
				if (!result.IsSuccess)
				{
					return result;
				}
				objs.Add(result.Value);
			}
		}
		else
		{
			Dictionary<ulong, IUser> dictionary = new Dictionary<ulong, IUser>();
			if (option.Users != null)
			{
				foreach (IUser user in option.Users)
				{
					dictionary[user.Id] = user;
				}
			}
			if (option.Members != null)
			{
				foreach (IGuildUser member in option.Members)
				{
					dictionary[member.Id] = member;
				}
			}
			objs.AddRange(dictionary.Values);
			if (option.Roles != null)
			{
				objs.AddRange(option.Roles);
			}
			if (option.Channels != null)
			{
				objs.AddRange(option.Channels);
			}
		}
		Array array = Array.CreateInstance(_underlyingType, objs.Count);
		for (int i = 0; i < objs.Count; i++)
		{
			array.SetValue(objs[i], i);
		}
		return TypeConverterResult.FromSuccess(array);
	}
}
