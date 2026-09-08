using System;

namespace Discord;

public interface IEntity<TId> where TId : IEquatable<TId>
{
	TId Id { get; }
}
