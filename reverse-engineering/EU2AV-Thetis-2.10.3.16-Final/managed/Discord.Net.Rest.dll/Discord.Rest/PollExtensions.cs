using System.Collections.Immutable;
using System.Linq;
using Discord.API;
using Discord.API.Rest;

namespace Discord.Rest;

internal static class PollExtensions
{
	public static CreatePollParams ToModel(this PollProperties poll)
	{
		return new CreatePollParams
		{
			AllowMultiselect = poll.AllowMultiselect,
			Duration = poll.Duration,
			LayoutType = poll.LayoutType,
			Answers = poll.Answers.Select((PollMediaProperties x) => new Discord.API.PollAnswer
			{
				PollMedia = new Discord.API.PollMedia
				{
					Emoji = ((x.Emoji != null) ? ((Optional<Discord.API.Emoji>)new Discord.API.Emoji
					{
						Id = ((x.Emoji is Emote emote2) ? new ulong?(emote2.Id) : ((ulong?)null)),
						Name = ((x.Emoji is Emoji emoji2) ? emoji2.Name : null)
					}) : Optional<Discord.API.Emoji>.Unspecified),
					Text = x.Text
				}
			}).ToArray(),
			Question = new Discord.API.PollMedia
			{
				Emoji = ((poll.Question.Emoji != null) ? ((Optional<Discord.API.Emoji>)new Discord.API.Emoji
				{
					Id = ((poll.Question.Emoji is Emote emote) ? new ulong?(emote.Id) : ((ulong?)null)),
					Name = ((poll.Question.Emoji is Emoji emoji) ? emoji.Name : null)
				}) : Optional<Discord.API.Emoji>.Unspecified),
				Text = poll.Question.Text
			}
		};
	}

	public static Poll ToEntity(this Discord.API.Poll poll)
	{
		return new Poll(new PollMedia(poll.Question.Text, poll.Question.Emoji.IsSpecified ? poll.Question.Emoji.Value.ToIEmote() : null), poll.Answers.Select((Discord.API.PollAnswer x) => new PollAnswer(x.AnswerId, new PollMedia(x.PollMedia.Text, x.PollMedia.Emoji.IsSpecified ? x.PollMedia.Emoji.Value.ToIEmote() : null))).ToImmutableArray(), poll.Expiry, poll.AllowMultiselect, poll.LayoutType, poll.PollResults.IsSpecified ? new PollResults?(new PollResults(poll.PollResults.Value.IsFinalized, poll.PollResults.Value.AnswerCounts.Select((PollAnswerCount x) => new PollAnswerCounts(x.Id, x.Count, x.MeVoted)).ToImmutableArray())) : ((PollResults?)null));
	}
}
