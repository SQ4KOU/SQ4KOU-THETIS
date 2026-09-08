using System.Collections.Generic;

namespace Discord.Webhook;

public class WebhookMessageProperties
{
	public Optional<string> Content { get; set; }

	public Optional<IEnumerable<Embed>> Embeds { get; set; }

	public Optional<AllowedMentions> AllowedMentions { get; set; }

	public Optional<MessageComponent> Components { get; set; }

	public Optional<IEnumerable<FileAttachment>> Attachments { get; set; }
}
