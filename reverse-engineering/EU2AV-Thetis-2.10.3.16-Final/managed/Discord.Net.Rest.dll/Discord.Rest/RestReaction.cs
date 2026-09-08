using System.Collections.Generic;
using Discord.API;

namespace Discord.Rest;

public class RestReaction : IReaction
{
	public IEmote Emote { get; }

	public int Count { get; }

	public bool Me { get; }

	public bool MeBurst { get; }

	public int BurstCount { get; }

	public int NormalCount { get; }

	public IReadOnlyCollection<Color> BurstColors { get; }

	internal RestReaction(IEmote emote, int count, bool me, int burst, int normal, IReadOnlyCollection<Color> colors, bool meBurst)
	{
		Emote = emote;
		Count = count;
		Me = me;
		BurstCount = burst;
		NormalCount = normal;
		BurstColors = colors;
		MeBurst = meBurst;
	}

	internal static RestReaction Create(Reaction model)
	{
		IEmote emote = ((!model.Emoji.Id.HasValue) ? ((IEmote)new Emoji(model.Emoji.Name)) : ((IEmote)new Emote(model.Emoji.Id.Value, model.Emoji.Name, model.Emoji.Animated.GetValueOrDefault())));
		return new RestReaction(emote, model.Count, model.Me, model.CountDetails.BurstCount, model.CountDetails.NormalCount, model.Colors.ToReadOnlyCollection(), model.MeBurst);
	}
}
