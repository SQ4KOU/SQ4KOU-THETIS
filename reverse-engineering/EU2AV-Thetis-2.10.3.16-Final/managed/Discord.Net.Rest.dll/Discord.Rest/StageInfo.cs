namespace Discord.Rest;

public class StageInfo
{
	public string Topic { get; }

	public StagePrivacyLevel? PrivacyLevel { get; }

	public IUser User { get; }

	internal StageInfo(IUser user, StagePrivacyLevel? level, string topic)
	{
		Topic = topic;
		PrivacyLevel = level;
		User = user;
	}
}
