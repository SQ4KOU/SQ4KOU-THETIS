using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Discord.Net.Converters;
using Discord.Net.Rest;
using Newtonsoft.Json;

namespace Discord.API.Rest;

internal class UploadInteractionFileParams
{
	private static JsonSerializer _serializer = new JsonSerializer
	{
		ContractResolver = new DiscordContractResolver()
	};

	public FileAttachment[] Files { get; }

	public InteractionResponseType Type { get; set; }

	public Optional<string> Content { get; set; }

	public Optional<bool> IsTTS { get; set; }

	public Optional<Embed[]> Embeds { get; set; }

	public Optional<AllowedMentions> AllowedMentions { get; set; }

	public Optional<IMessageComponent[]> MessageComponents { get; set; }

	public Optional<MessageFlags> Flags { get; set; }

	public Optional<CreatePollParams> Poll { get; set; }

	public bool HasData
	{
		get
		{
			if (!Content.IsSpecified && !IsTTS.IsSpecified && !Embeds.IsSpecified && !AllowedMentions.IsSpecified && !MessageComponents.IsSpecified && !Flags.IsSpecified && !Files.Any())
			{
				return Poll.IsSpecified;
			}
			return true;
		}
	}

	public UploadInteractionFileParams(params FileAttachment[] files)
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
		dictionary2["type"] = Type;
		Dictionary<string, object> dictionary3 = new Dictionary<string, object>();
		if (Content.IsSpecified)
		{
			dictionary3["content"] = Content.Value;
		}
		if (IsTTS.IsSpecified)
		{
			dictionary3["tts"] = IsTTS.Value;
		}
		if (Embeds.IsSpecified)
		{
			dictionary3["embeds"] = Embeds.Value;
		}
		if (AllowedMentions.IsSpecified)
		{
			dictionary3["allowed_mentions"] = AllowedMentions.Value;
		}
		if (MessageComponents.IsSpecified)
		{
			dictionary3["components"] = MessageComponents.Value;
			if (MessageComponents.Value.Any((IMessageComponent x) => x.Type != ComponentType.ActionRow))
			{
				messageFlags |= MessageFlags.ComponentsV2;
			}
		}
		dictionary3["flags"] = Flags.GetValueOrDefault(MessageFlags.None) | messageFlags;
		if (Poll.IsSpecified)
		{
			dictionary3["poll"] = Poll.Value;
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
		dictionary3["attachments"] = list;
		dictionary2["data"] = dictionary3;
		if (dictionary3.Any())
		{
			StringBuilder stringBuilder = new StringBuilder();
			using (StringWriter textWriter = new StringWriter(stringBuilder))
			{
				using JsonTextWriter jsonWriter = new JsonTextWriter(textWriter);
				_serializer.Serialize(jsonWriter, dictionary2);
			}
			dictionary["payload_json"] = stringBuilder.ToString();
		}
		return dictionary;
	}
}
