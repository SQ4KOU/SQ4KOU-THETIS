using System;

namespace Thetis;

public class SerialRXEvent : EventArgs
{
	internal string buffer;

	public SerialRXEvent(string buffer)
	{
		this.buffer = buffer;
	}
}
