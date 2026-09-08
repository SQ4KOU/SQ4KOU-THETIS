using System;
using System.Threading.Tasks;

namespace Discord;

public struct Cacheable<TEntity, TId> where TEntity : IEntity<TId> where TId : IEquatable<TId>
{
	public bool HasValue { get; }

	public TId Id { get; }

	public TEntity Value { get; }

	private Func<Task<TEntity>> DownloadFunc { get; }

	internal Cacheable(TEntity value, TId id, bool hasValue, Func<Task<TEntity>> downloadFunc)
	{
		Value = value;
		Id = id;
		HasValue = hasValue;
		DownloadFunc = downloadFunc;
	}

	public Task<TEntity> DownloadAsync()
	{
		return DownloadFunc();
	}

	public async Task<TEntity> GetOrDownloadAsync()
	{
		return (!HasValue) ? (await DownloadAsync().ConfigureAwait(continueOnCapturedContext: false)) : Value;
	}
}
public struct Cacheable<TCachedEntity, TDownloadableEntity, TRelationship, TId> where TCachedEntity : IEntity<TId>, TRelationship where TDownloadableEntity : IEntity<TId>, TRelationship where TId : IEquatable<TId>
{
	public bool HasValue { get; }

	public TId Id { get; }

	public TCachedEntity Value { get; }

	private Func<Task<TDownloadableEntity>> DownloadFunc { get; }

	internal Cacheable(TCachedEntity value, TId id, bool hasValue, Func<Task<TDownloadableEntity>> downloadFunc)
	{
		Value = value;
		Id = id;
		HasValue = hasValue;
		DownloadFunc = downloadFunc;
	}

	public Task<TDownloadableEntity> DownloadAsync()
	{
		return DownloadFunc();
	}

	public async Task<TRelationship> GetOrDownloadAsync()
	{
		return (!HasValue) ? ((TRelationship)(object)(await DownloadAsync().ConfigureAwait(continueOnCapturedContext: false))) : ((TRelationship)(object)Value);
	}
}
