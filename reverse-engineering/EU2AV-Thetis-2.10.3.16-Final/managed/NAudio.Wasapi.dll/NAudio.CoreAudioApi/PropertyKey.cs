using System;

namespace NAudio.CoreAudioApi;

public struct PropertyKey(Guid formatId, int propertyId)
{
	public Guid formatId = formatId;

	public int propertyId = propertyId;
}
