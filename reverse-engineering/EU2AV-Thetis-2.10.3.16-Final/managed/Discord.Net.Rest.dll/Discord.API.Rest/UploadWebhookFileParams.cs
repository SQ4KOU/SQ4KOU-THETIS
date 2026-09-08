using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Discord.Net.Converters;
using Discord.Net.Rest;
using Newtonsoft.Json;

namespace Discord.API.Rest;

internal class UploadWebhookFileParams
{
	private static JsonSerializer _serializer = new JsonSerializer
	{
		ContractResolver = new DiscordContractResolver()
	};

	public FileAttachment[] Files { get; }

	public Optional<string> Content { get; set; }

	public Optional<string> Nonce { get; set; }

	public Optional<bool> IsTTS { get; set; }

	public Optional<string> Username { get; set; }

	public Optional<string> AvatarUrl { get; set; }

	public Optional<Embed[]> Embeds { get; set; }

	public Optional<AllowedMentions> AllowedMentions { get; set; }

	public Optional<IMessageComponent[]> MessageComponents { get; set; }

	public Optional<MessageFlags> Flags { get; set; }

	public Optional<string> ThreadName { get; set; }

	public Optional<ulong[]> AppliedTags { get; set; }

	public Optional<CreatePollParams> Poll { get; set; }

	public UploadWebhookFileParams(params FileAttachment[] files)
	{
		Files = files;
	}

	public IReadOnlyDictionary<string, object> ToDictionary()
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		MessageFlags messageFlags = MessageFlags.None;
		if (Files.Any((FileAttachment x) => x.Waveform != null && x.DurationSeconds.HasValue))
		{
			messageFlags |= MessageFlags.VoiceMessage;
		}
		Dictionary<string, object> dictionary2 = new Dictionary<string, object>();
		if (Content.IsSpecified)
		{
			dictionary2["content"] = Content.Value;
		}
		if (IsTTS.IsSpecified)
		{
			dictionary2["tts"] = IsTTS.Value;
		}
		if (Nonce.IsSpecified)
		{
			dictionary2["nonce"] = Nonce.Value;
		}
		if (Username.IsSpecified)
		{
			dictionary2["username"] = Username.Value;
		}
		if (AvatarUrl.IsSpecified)
		{
			dictionary2["avatar_url"] = AvatarUrl.Value;
		}
		if (Embeds.IsSpecified)
		{
			dictionary2["embeds"] = Embeds.Value;
		}
		if (AllowedMentions.IsSpecified)
		{
			dictionary2["allowed_mentions"] = AllowedMentions.Value;
		}
		if (MessageComponents.IsSpecified)
		{
			dictionary2["components"] = MessageComponents.Value;
			if (MessageComponents.Value.Any((IMessageComponent x) => x.Type != ComponentType.ActionRow))
			{
				messageFlags |= MessageFlags.ComponentsV2;
			}
		}
		dictionary2["flags"] = Flags.GetValueOrDefault(MessageFlags.None) | messageFlags;
		if (ThreadName.IsSpecified)
		{
			dictionary2["thread_name"] = ThreadName.Value;
		}
		if (AppliedTags.IsSpecified)
		{
			dictionary2["applied_tags"] = AppliedTags.Value;
		}
		if (Poll.IsSpecified)
		{
			dictionary2["poll"] = Poll.Value;
		}
		List<object> list = new List<object>();
		for (int num = 0; num != Files.Length; num++)
		{
			FileAttachment fileAttachment = Files[num];
			string text = fileAttachment.FileName ?? "unknown.dat";
			if (fileAttachment.IsSpoiler && !text.StartsWith("SPOILER_"))
			{
				text = text.Insert(0, "SPOILER_");
			}
			dictionary[$"files[{num}]"] = new MultipartFile(fileAttachment.Stream, text);
			long id = num;
			string filename = text;
			string description = fileAttachment.Description;
			list.Add(new
			{
				id = (ulong)id,
				filename = filename,
				description = ((description != null) ? ((Optional<string>)description) : Optional<string>.Unspecified),
				duration_secs = (((Optional<double>?)fileAttachment.DurationSeconds) ?? Optional<double>.Unspecified),
				waveform = ((fileAttachment.Waveform == null) ? Optional<string>.Unspecified : ((Optional<string>)Convert.ToBase64String(fileAttachment.Waveform)))
			});
		}
		dictionary2["attachments"] = list;
		StringBuilder stringBuilder = new StringBuilder();
		using (StringWriter textWriter = new StringWriter(stringBuilder))
		{
			using JsonTextWriter jsonWriter = new JsonTextWriter(textWriter);
			_serializer.Serialize(jsonWriter, dictionary2);
		}
		dictionary["payload_json"] = stringBuilder.ToString();
		return dictionary;
	}
}
