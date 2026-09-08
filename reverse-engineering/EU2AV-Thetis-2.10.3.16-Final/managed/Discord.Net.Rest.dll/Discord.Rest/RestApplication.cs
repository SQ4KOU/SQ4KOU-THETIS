using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Discord.API;

namespace Discord.Rest;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class RestApplication : RestEntity<ulong>, IApplication, ISnowflakeEntity, IEntity<ulong>
{
	protected string _iconId;

	public string Name { get; private set; }

	public string Description { get; private set; }

	public IReadOnlyCollection<string> RPCOrigins { get; private set; }

	public ApplicationFlags Flags { get; private set; }

	public bool? IsBotPublic { get; private set; }

	public bool? BotRequiresCodeGrant { get; private set; }

	public ITeam Team { get; private set; }

	public IUser Owner { get; private set; }

	public string TermsOfService { get; private set; }

	public string PrivacyPolicy { get; private set; }

	public string VerifyKey { get; private set; }

	public string CustomInstallUrl { get; private set; }

	public string RoleConnectionsVerificationUrl { get; private set; }

	public DateTimeOffset CreatedAt => SnowflakeUtils.FromSnowflake(base.Id);

	public string IconUrl => CDN.GetApplicationIconUrl(base.Id, _iconId);

	public PartialGuild Guild { get; private set; }

	public int? ApproximateGuildCount { get; private set; }

	public int? ApproximateUserInstallCount { get; private set; }

	public int? ApproximateUserAuthorizationCount { get; private set; }

	public IReadOnlyCollection<string> RedirectUris { get; private set; }

	public string InteractionsEndpointUrl { get; private set; }

	public ApplicationInstallParams InstallParams { get; private set; }

	public ApplicationDiscoverabilityState DiscoverabilityState { get; private set; }

	public DiscoveryEligibilityFlags DiscoveryEligibilityFlags { get; private set; }

	public ApplicationExplicitContentFilterLevel ExplicitContentFilterLevel { get; private set; }

	public bool IsHook { get; private set; }

	public IReadOnlyCollection<string> InteractionEventTypes { get; private set; }

	public ApplicationInteractionsVersion InteractionsVersion { get; private set; }

	public bool IsMonetized { get; private set; }

	public ApplicationMonetizationEligibilityFlags MonetizationEligibilityFlags { get; private set; }

	public ApplicationMonetizationState MonetizationState { get; private set; }

	public ApplicationRpcState RpcState { get; private set; }

	public ApplicationStoreState StoreState { get; private set; }

	public ApplicationVerificationState VerificationState { get; private set; }

	public IReadOnlyCollection<string> Tags { get; private set; }

	public IReadOnlyDictionary<ApplicationIntegrationType, ApplicationInstallParams> IntegrationTypesConfig { get; private set; }

	private string DebuggerDisplay => $"{Name} ({base.Id})";

	internal RestApplication(BaseDiscordClient discord, ulong id)
		: base(discord, id)
	{
	}

	internal static RestApplication Create(BaseDiscordClient discord, Application model)
	{
		RestApplication restApplication = new RestApplication(discord, model.Id);
		restApplication.Update(model);
		return restApplication;
	}

	internal void Update(Application model)
	{
		Description = model.Description;
		RPCOrigins = (model.RPCOrigins.IsSpecified ? ImmutableCollectionsMarshal.AsImmutableArray(model.RPCOrigins.Value.ToArray()) : ImmutableArray<string>.Empty);
		Name = model.Name;
		_iconId = model.Icon;
		IsBotPublic = (model.IsBotPublic.IsSpecified ? new bool?(model.IsBotPublic.Value) : ((bool?)null));
		BotRequiresCodeGrant = (model.BotRequiresCodeGrant.IsSpecified ? new bool?(model.BotRequiresCodeGrant.Value) : ((bool?)null));
		Tags = model.Tags.GetValueOrDefault(null)?.ToImmutableArray() ?? ImmutableArray<string>.Empty;
		PrivacyPolicy = model.PrivacyPolicy;
		TermsOfService = model.TermsOfService;
		InstallParams = (model.InstallParams.IsSpecified ? new ApplicationInstallParams(model.InstallParams.Value.Scopes, model.InstallParams.Value.Permission) : null);
		if (model.Flags.IsSpecified)
		{
			Flags = model.Flags.Value;
		}
		if (model.Owner.IsSpecified)
		{
			Owner = RestUser.Create(base.Discord, model.Owner.Value);
		}
		if (model.Team != null)
		{
			Team = RestTeam.Create(base.Discord, model.Team);
		}
		CustomInstallUrl = (model.CustomInstallUrl.IsSpecified ? model.CustomInstallUrl.Value : null);
		RoleConnectionsVerificationUrl = (model.RoleConnectionsUrl.IsSpecified ? model.RoleConnectionsUrl.Value : null);
		VerifyKey = model.VerifyKey;
		if (model.PartialGuild.IsSpecified)
		{
			Guild = PartialGuildExtensions.Create(model.PartialGuild.Value);
		}
		InteractionsEndpointUrl = (model.InteractionsEndpointUrl.IsSpecified ? model.InteractionsEndpointUrl.Value : null);
		if (model.RedirectUris.IsSpecified)
		{
			RedirectUris = ((IEnumerable<string>)model.RedirectUris.Value).ToImmutableArray();
		}
		ApproximateGuildCount = model.ApproximateGuildCount.ToNullable();
		ApproximateUserInstallCount = model.ApproximateUserInstallCount.ToNullable();
		ApproximateUserAuthorizationCount = model.ApproximateUserAuthorizationCount.ToNullable();
		DiscoverabilityState = model.DiscoverabilityState.GetValueOrDefault(ApplicationDiscoverabilityState.None);
		DiscoveryEligibilityFlags = model.DiscoveryEligibilityFlags.GetValueOrDefault(DiscoveryEligibilityFlags.None);
		ExplicitContentFilterLevel = model.ExplicitContentFilter.GetValueOrDefault(ApplicationExplicitContentFilterLevel.Disabled);
		IsHook = model.IsHook;
		InteractionEventTypes = ((IEnumerable<string>)model.InteractionsEventTypes.GetValueOrDefault(Array.Empty<string>())).ToImmutableArray();
		InteractionsVersion = model.InteractionsVersion.GetValueOrDefault(ApplicationInteractionsVersion.Version1);
		IsMonetized = model.IsMonetized;
		MonetizationEligibilityFlags = model.MonetizationEligibilityFlags.GetValueOrDefault(ApplicationMonetizationEligibilityFlags.None);
		MonetizationState = model.MonetizationState.GetValueOrDefault(ApplicationMonetizationState.None);
		RpcState = model.RpcState.GetValueOrDefault(ApplicationRpcState.Disabled);
		StoreState = model.StoreState.GetValueOrDefault(ApplicationStoreState.None);
		VerificationState = model.VerificationState.GetValueOrDefault(ApplicationVerificationState.Ineligible);
		Dictionary<ApplicationIntegrationType, ApplicationInstallParams> dictionary = new Dictionary<ApplicationIntegrationType, ApplicationInstallParams>();
		if (model.IntegrationTypesConfig.IsSpecified)
		{
			foreach (KeyValuePair<ApplicationIntegrationType, InstallParams> item in model.IntegrationTypesConfig.Value)
			{
				dictionary.Add(item.Key, new ApplicationInstallParams(item.Value.Scopes ?? Array.Empty<string>(), item.Value.Permission));
			}
		}
		IntegrationTypesConfig = dictionary.ToImmutableDictionary();
	}

	public async Task UpdateAsync()
	{
		Application application = await base.Discord.ApiClient.GetMyApplicationAsync().ConfigureAwait(continueOnCapturedContext: false);
		if (application.Id != base.Id)
		{
			throw new InvalidOperationException("Unable to update this object from a different application token.");
		}
		Update(application);
	}

	public override string ToString()
	{
		return Name;
	}
}
