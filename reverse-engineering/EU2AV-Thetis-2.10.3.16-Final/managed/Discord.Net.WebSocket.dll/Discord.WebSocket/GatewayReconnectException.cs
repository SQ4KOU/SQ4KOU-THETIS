using System;

namespace Discord.WebSocket;

public class GatewayReconnectException : Exception
{
	public GatewayReconnectException(string message)
		: base(message)
	{
	}
}
