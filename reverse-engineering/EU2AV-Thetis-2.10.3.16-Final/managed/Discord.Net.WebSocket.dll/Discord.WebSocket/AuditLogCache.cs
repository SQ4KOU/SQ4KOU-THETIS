using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Discord.WebSocket;

internal class AuditLogCache
{
	private readonly ConcurrentDictionary<ulong, SocketAuditLogEntry> _entries;

	private readonly ConcurrentQueue<ulong> _orderedEntries;

	private readonly int _size;

	public IReadOnlyCollection<SocketAuditLogEntry> AuditLogs => _entries.ToReadOnlyCollection();

	public AuditLogCache(DiscordSocketClient client)
	{
		_size = client.AuditLogCacheSize;
		int size = _size;
		_entries = new ConcurrentDictionary<ulong, SocketAuditLogEntry>(ConcurrentHashSet.DefaultConcurrencyLevel, (!((double)size * 1.05 > 2147483647.0)) ? ((int)((double)size * 1.05)) : int.MaxValue);
		_orderedEntries = new ConcurrentQueue<ulong>();
	}

	public void Add(SocketAuditLogEntry entry)
	{
		if (_entries.TryAdd(entry.Id, entry))
		{
			_orderedEntries.Enqueue(entry.Id);
			ulong result;
			while (_orderedEntries.Count > _size && _orderedEntries.TryDequeue(out result))
			{
				_entries.TryRemove(result, out var _);
			}
		}
	}

	public SocketAuditLogEntry Remove(ulong id)
	{
		_entries.TryRemove(id, out var value);
		return value;
	}

	public SocketAuditLogEntry Get(ulong id)
	{
		if (!_entries.TryGetValue(id, out var value))
		{
			return null;
		}
		return value;
	}

	public IReadOnlyCollection<SocketAuditLogEntry> GetMany(ulong? fromEntryId, Direction dir, int limit = 100, ActionType? action = null)
	{
		if (limit < 0)
		{
			throw new ArgumentOutOfRangeException("limit");
		}
		if (limit == 0)
		{
			return ImmutableArray<SocketAuditLogEntry>.Empty;
		}
		IEnumerable<ulong> source;
		if (!fromEntryId.HasValue)
		{
			source = _orderedEntries;
		}
		else
		{
			switch (dir)
			{
			case Direction.Before:
				source = _orderedEntries.Where((ulong x) => x < fromEntryId.Value);
				break;
			case Direction.After:
				source = _orderedEntries.Where((ulong x) => x > fromEntryId.Value);
				break;
			default:
			{
				if (!_entries.TryGetValue(fromEntryId.Value, out var value))
				{
					return ImmutableArray<SocketAuditLogEntry>.Empty;
				}
				int limit2 = limit / 2;
				IReadOnlyCollection<SocketAuditLogEntry> many = GetMany(fromEntryId, Direction.Before, limit2, action);
				return GetMany(fromEntryId, Direction.After, limit2, action).Reverse().Concat(new SocketAuditLogEntry[1] { value }).Concat(many)
					.ToImmutableArray();
			}
			}
		}
		if (dir == Direction.Before)
		{
			source = source.Reverse();
		}
		if (dir == Direction.Around)
		{
			limit = limit / 2 + 1;
		}
		return (from x in source
			select (!_entries.TryGetValue(x, out var value2)) ? null : value2 into x
			where x != null && (!action.HasValue || x.Action == action)
			select x).Take(limit).ToImmutableArray();
	}
}
