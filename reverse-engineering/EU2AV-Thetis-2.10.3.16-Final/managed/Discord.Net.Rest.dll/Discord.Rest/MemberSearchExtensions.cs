using System.Collections.Generic;
using Discord.API.Rest;

namespace Discord.Rest;

internal static class MemberSearchExtensions
{
	internal static SearchQueryProperties ToModel(this IMemberSearchQuery props)
	{
		SearchQueryProperties searchQueryProperties = new SearchQueryProperties();
		SearchRangeProperties searchRangeProperties = props.Range?.ToModel();
		searchQueryProperties.Range = ((searchRangeProperties != null) ? ((Optional<SearchRangeProperties>)searchRangeProperties) : Optional<SearchRangeProperties>.Unspecified);
		searchQueryProperties.AndQuery = ((props.AndQuery != null) ? new Optional<IEnumerable<object>>(props.AndQuery) : Optional<IEnumerable<object>>.Unspecified);
		searchQueryProperties.OrQuery = ((props.OrQuery != null) ? new Optional<IEnumerable<object>>(props.OrQuery) : Optional<IEnumerable<object>>.Unspecified);
		return searchQueryProperties;
	}

	internal static SafetySignalsProperties ToModel(this MemberSearchV2SafetySignalsProperties props)
	{
		SafetySignalsProperties obj = new SafetySignalsProperties
		{
			AutomodQuarantinedUsername = (((Optional<bool>?)props.AutomodQuarantinedUsername) ?? Optional<bool>.Unspecified),
			UnusualAccountActivity = (((Optional<bool>?)props.UnusualAccountActivity) ?? Optional<bool>.Unspecified)
		};
		MemberSearchIntQuery? communicationDisabledUntil = props.CommunicationDisabledUntil;
		SearchQueryProperties searchQueryProperties = (communicationDisabledUntil.HasValue ? communicationDisabledUntil.GetValueOrDefault().ToModel() : null);
		obj.CommunicationDisabledUntil = ((searchQueryProperties != null) ? ((Optional<SearchQueryProperties>)searchQueryProperties) : Optional<SearchQueryProperties>.Unspecified);
		communicationDisabledUntil = props.UnusualDmActivityUntil;
		searchQueryProperties = (communicationDisabledUntil.HasValue ? communicationDisabledUntil.GetValueOrDefault().ToModel() : null);
		obj.UnusualDMActivityUntil = ((searchQueryProperties != null) ? ((Optional<SearchQueryProperties>)searchQueryProperties) : Optional<SearchQueryProperties>.Unspecified);
		return obj;
	}

	internal static Discord.API.Rest.MemberSearchFilter ToModel(this MemberSearchFilter props)
	{
		Discord.API.Rest.MemberSearchFilter obj = new Discord.API.Rest.MemberSearchFilter
		{
			DidRejoin = (((Optional<bool>?)props.DidRejoin) ?? Optional<bool>.Unspecified),
			IsPending = (((Optional<bool>?)props.IsPending) ?? Optional<bool>.Unspecified)
		};
		MemberSearchIntQuery? joinSourceType = props.JoinSourceType;
		SearchQueryProperties searchQueryProperties = (joinSourceType.HasValue ? joinSourceType.GetValueOrDefault().ToModel() : null);
		obj.JoinSourceType = ((searchQueryProperties != null) ? ((Optional<SearchQueryProperties>)searchQueryProperties) : Optional<SearchQueryProperties>.Unspecified);
		joinSourceType = props.GuildJoinedAt;
		searchQueryProperties = (joinSourceType.HasValue ? joinSourceType.GetValueOrDefault().ToModel() : null);
		obj.GuildJoinedAt = ((searchQueryProperties != null) ? ((Optional<SearchQueryProperties>)searchQueryProperties) : Optional<SearchQueryProperties>.Unspecified);
		MemberSearchSnowflakeQuery? roleIds = props.RoleIds;
		searchQueryProperties = (roleIds.HasValue ? roleIds.GetValueOrDefault().ToModel() : null);
		obj.RoleIds = ((searchQueryProperties != null) ? ((Optional<SearchQueryProperties>)searchQueryProperties) : Optional<SearchQueryProperties>.Unspecified);
		MemberSearchStringQuery? sourceInviteCode = props.SourceInviteCode;
		searchQueryProperties = (sourceInviteCode.HasValue ? sourceInviteCode.GetValueOrDefault().ToModel() : null);
		obj.SourceInviteCode = ((searchQueryProperties != null) ? ((Optional<SearchQueryProperties>)searchQueryProperties) : Optional<SearchQueryProperties>.Unspecified);
		SafetySignalsProperties safetySignalsProperties = props.SafetySignals?.ToModel();
		obj.SafetySignals = ((safetySignalsProperties != null) ? ((Optional<SafetySignalsProperties>)safetySignalsProperties) : Optional<SafetySignalsProperties>.Unspecified);
		roleIds = props.UserId;
		searchQueryProperties = (roleIds.HasValue ? roleIds.GetValueOrDefault().ToModel() : null);
		obj.UserId = ((searchQueryProperties != null) ? ((Optional<SearchQueryProperties>)searchQueryProperties) : Optional<SearchQueryProperties>.Unspecified);
		sourceInviteCode = props.Usernames;
		searchQueryProperties = (sourceInviteCode.HasValue ? sourceInviteCode.GetValueOrDefault().ToModel() : null);
		obj.Usernames = ((searchQueryProperties != null) ? ((Optional<SearchQueryProperties>)searchQueryProperties) : Optional<SearchQueryProperties>.Unspecified);
		return obj;
	}

	internal static SearchRangeProperties ToModel(this MemberSearchV2Range props)
	{
		return new SearchRangeProperties
		{
			GreaterThanOrEqual = (((Optional<long>?)props.GreaterThanOrEqual) ?? Optional<long>.Unspecified),
			LessThanOrEqual = (((Optional<long>?)props.LessThanOrEqual) ?? Optional<long>.Unspecified)
		};
	}

	internal static Discord.API.Rest.MemberSearchPaginationFilter ToModel(this MemberSearchPaginationFilter props)
	{
		return new Discord.API.Rest.MemberSearchPaginationFilter
		{
			UserId = props.UserId,
			GuildJoinedAt = props.GuildJoinedAt
		};
	}
}
