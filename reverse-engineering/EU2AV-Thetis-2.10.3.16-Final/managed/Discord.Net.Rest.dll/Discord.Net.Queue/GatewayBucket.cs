using System.Collections.Immutable;

namespace Discord.Net.Queue;

internal struct GatewayBucket
{
	private static readonly ImmutableDictionary<GatewayBucketType, GatewayBucket> DefsByType;

	private static readonly ImmutableDictionary<BucketId, GatewayBucket> DefsById;

	public GatewayBucketType Type { get; }

	public BucketId Id { get; }

	public int WindowCount { get; set; }

	public int WindowSeconds { get; set; }

	static GatewayBucket()
	{
		GatewayBucket[] array = new GatewayBucket[3]
		{
			new GatewayBucket(GatewayBucketType.Unbucketed, BucketId.Create(null, "<gateway-unbucketed>", null), 117, 60),
			new GatewayBucket(GatewayBucketType.Identify, BucketId.Create(null, "<gateway-identify>", null), 1, 5),
			new GatewayBucket(GatewayBucketType.PresenceUpdate, BucketId.Create(null, "<gateway-presenceupdate>", null), 5, 60)
		};
		ImmutableDictionary<GatewayBucketType, GatewayBucket>.Builder builder = ImmutableDictionary.CreateBuilder<GatewayBucketType, GatewayBucket>();
		GatewayBucket[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			GatewayBucket value = array2[i];
			builder.Add(value.Type, value);
		}
		DefsByType = builder.ToImmutable();
		ImmutableDictionary<BucketId, GatewayBucket>.Builder builder2 = ImmutableDictionary.CreateBuilder<BucketId, GatewayBucket>();
		array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			GatewayBucket value2 = array2[i];
			builder2.Add(value2.Id, value2);
		}
		DefsById = builder2.ToImmutable();
	}

	public static GatewayBucket Get(GatewayBucketType type)
	{
		return DefsByType[type];
	}

	public static GatewayBucket Get(BucketId id)
	{
		return DefsById[id];
	}

	public GatewayBucket(GatewayBucketType type, BucketId id, int count, int seconds)
	{
		Type = type;
		Id = id;
		WindowCount = count;
		WindowSeconds = seconds;
	}
}
