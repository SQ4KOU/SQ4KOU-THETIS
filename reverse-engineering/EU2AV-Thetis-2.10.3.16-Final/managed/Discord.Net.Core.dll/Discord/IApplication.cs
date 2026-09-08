using System.Collections.Generic;

namespace Discord;

public interface IApplication : ISnowflakeEntity, IEntity<ulong>
{
	string Name { get; }

	string Description { get; }

	IReadOnlyCollection<string> RPCOrigins { get; }

	ApplicationFlags Flags { get; }

	ApplicationInstallParams InstallParams { get; }

	IReadOnlyCollection<string> Tags { get; }

	string IconUrl { get; }

	bool? IsBotPublic { get; }

	bool? BotRequiresCodeGrant { get; }

	ITeam Team { get; }

	IUser Owner { get; }

	string TermsOfService { get; }

	string PrivacyPolicy { get; }

	string CustomInstallUrl { get; }

	string RoleConnectionsVerificationUrl { get; }

	string VerifyKey { get; }

	PartialGuild Guild { get; }

	IReadOnlyCollection<string> RedirectUris { get; }

	string InteractionsEndpointUrl { get; }

	int? ApproximateGuildCount { get; }

	int? ApproximateUserInstallCount { get; }

	int? ApproximateUserAuthorizationCount { get; }

	ApplicationDiscoverabilityState DiscoverabilityState { get; }

	DiscoveryEligibilityFlags DiscoveryEligibilityFlags { get; }

	ApplicationExplicitContentFilterLevel ExplicitContentFilterLevel { get; }

	bool IsHook { get; }

	IReadOnlyCollection<string> InteractionEventTypes { get; }

	ApplicationInteractionsVersion InteractionsVersion { get; }

	bool IsMonetized { get; }

	ApplicationMonetizationEligibilityFlags MonetizationEligibilityFlags { get; }

	ApplicationMonetizationState MonetizationState { get; }

	ApplicationRpcState RpcState { get; }

	ApplicationStoreState StoreState { get; }

	ApplicationVerificationState VerificationState { get; }

	IReadOnlyDictionary<ApplicationIntegrationType, ApplicationInstallParams> IntegrationTypesConfig { get; }
}
