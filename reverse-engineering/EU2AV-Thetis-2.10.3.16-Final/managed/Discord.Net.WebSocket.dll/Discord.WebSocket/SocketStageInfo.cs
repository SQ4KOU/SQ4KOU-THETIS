namespace Discord.WebSocket;

public class SocketStageInfo
{
	public string Topic { get; }

	public StagePrivacyLevel? PrivacyLevel { get; }

	internal SocketStageInfo(StagePrivacyLevel? level, string topic)
	{
		Topic = topic;
		PrivacyLevel = level;
	}
}
