using System.Collections.Generic;

namespace Discord;

public interface IPresence
{
	UserStatus Status { get; }

	IReadOnlyCollection<ClientType> ActiveClients { get; }

	IReadOnlyCollection<IActivity> Activities { get; }
}
