namespace Discord;

public class MessageRoleSubscriptionData
{
	public ulong Id { get; }

	public string TierName { get; }

	public int MonthsSubscribed { get; }

	public bool IsRenewal { get; }

	internal MessageRoleSubscriptionData(ulong id, string tierName, int monthsSubscribed, bool isRenewal)
	{
		Id = id;
		TierName = tierName;
		MonthsSubscribed = monthsSubscribed;
		IsRenewal = isRenewal;
	}
}
