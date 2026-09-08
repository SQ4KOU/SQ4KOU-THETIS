using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Discord.Interactions;

internal sealed class EnumConverter<T> : TypeConverter<T> where T : struct, Enum
{
	public override ApplicationCommandOptionType GetDiscordType()
	{
		return ApplicationCommandOptionType.String;
	}

	public override Task<TypeConverterResult> ReadAsync(IInteractionContext context, IApplicationCommandInteractionDataOption option, IServiceProvider services)
	{
		if (Enum.TryParse<T>((string)option.Value, out var result))
		{
			return Task.FromResult(TypeConverterResult.FromSuccess(result));
		}
		return Task.FromResult(TypeConverterResult.FromError(InteractionCommandError.ConvertFailed, string.Format("Value {0} cannot be converted to {1}", option.Value, "T")));
	}

	public override void Write(ApplicationCommandOptionProperties properties, IParameterInfo parameterInfo)
	{
		IEnumerable<MemberInfo> enumerable = from x in Enum.GetNames(typeof(T)).SelectMany((string x) => typeof(T).GetMember(x))
			where !x.IsDefined(typeof(HideAttribute), inherit: true)
			select x;
		ILocalizationManager localizationManager = parameterInfo.Command.Module.CommandService.LocalizationManager;
		if (enumerable.Count() > 25)
		{
			return;
		}
		List<ApplicationCommandOptionChoiceProperties> list = new List<ApplicationCommandOptionChoiceProperties>();
		foreach (MemberInfo item in enumerable)
		{
			string text = item.GetCustomAttribute<ChoiceDisplayAttribute>()?.Name ?? item.Name;
			list.Add(new ApplicationCommandOptionChoiceProperties
			{
				Name = text,
				Value = item.Name,
				NameLocalizations = (localizationManager?.GetAllNames(parameterInfo.GetChoicePath(new ParameterChoice(text.ToLower(), item.Name)), LocalizationTarget.Choice) ?? ImmutableDictionary<string, string>.Empty)
			});
		}
		properties.Choices = list;
	}
}
