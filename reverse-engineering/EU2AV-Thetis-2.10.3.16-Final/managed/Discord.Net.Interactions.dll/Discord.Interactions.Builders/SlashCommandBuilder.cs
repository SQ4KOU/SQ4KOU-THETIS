using System;
using System.Collections.Generic;

namespace Discord.Interactions.Builders;

public sealed class SlashCommandBuilder : CommandBuilder<SlashCommandInfo, SlashCommandBuilder, SlashCommandParameterBuilder>
{
	protected override SlashCommandBuilder Instance => this;

	public string Description { get; set; }

	[Obsolete("To be deprecated soon, use IsEnabledInDm and DefaultMemberPermissions instead.")]
	public bool DefaultPermission { get; set; } = true;

	public bool IsEnabledInDm { get; set; } = true;

	public bool IsNsfw { get; set; }

	public GuildPermission? DefaultMemberPermissions { get; set; }

	public HashSet<ApplicationIntegrationType> IntegrationTypes { get; set; }

	public HashSet<InteractionContextType> ContextTypes { get; set; }

	internal SlashCommandBuilder(ModuleBuilder module)
		: base(module)
	{
		IntegrationTypes = module.IntegrationTypes;
		ContextTypes = module.ContextTypes;
		IsEnabledInDm = module.IsEnabledInDm;
	}

	public SlashCommandBuilder(ModuleBuilder module, string name, ExecuteCallback callback)
		: base(module, name, callback)
	{
	}

	public SlashCommandBuilder WithDescription(string description)
	{
		Description = description;
		return this;
	}

	[Obsolete("To be deprecated soon, use SetEnabledInDm and WithDefaultMemberPermissions instead.")]
	public SlashCommandBuilder WithDefaultPermission(bool permission)
	{
		DefaultPermission = permission;
		return Instance;
	}

	public override SlashCommandBuilder AddParameter(Action<SlashCommandParameterBuilder> configure)
	{
		SlashCommandParameterBuilder slashCommandParameterBuilder = new SlashCommandParameterBuilder(this);
		configure(slashCommandParameterBuilder);
		AddParameters(slashCommandParameterBuilder);
		return this;
	}

	public SlashCommandBuilder SetEnabledInDm(bool isEnabled)
	{
		IsEnabledInDm = isEnabled;
		return this;
	}

	public SlashCommandBuilder SetNsfw(bool isNsfw)
	{
		IsNsfw = isNsfw;
		return this;
	}

	public SlashCommandBuilder WithDefaultMemberPermissions(GuildPermission permissions)
	{
		DefaultMemberPermissions = permissions;
		return this;
	}

	public SlashCommandBuilder WithIntegrationTypes(params ApplicationIntegrationType[] integrationTypes)
	{
		IntegrationTypes = ((integrationTypes != null) ? new HashSet<ApplicationIntegrationType>(integrationTypes) : null);
		return this;
	}

	public SlashCommandBuilder WithContextTypes(params InteractionContextType[] contextTypes)
	{
		ContextTypes = ((contextTypes != null) ? new HashSet<InteractionContextType>(contextTypes) : null);
		return this;
	}

	internal override SlashCommandInfo Build(ModuleInfo module, InteractionService commandService)
	{
		return new SlashCommandInfo(this, module, commandService);
	}
}
