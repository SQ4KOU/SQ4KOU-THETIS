using System;

namespace Discord;

public static class PermissionUtils
{
	public static int GetHieararchy(object target)
	{
		if (!(target is IRole { Position: var position }))
		{
			if (!(target is IGuildUser { Hierarchy: var hierarchy }))
			{
				if (target is IUser)
				{
					return int.MinValue;
				}
				throw new ArgumentOutOfRangeException("target", "Cannot determine hierarchy for the provided target.");
			}
			return hierarchy;
		}
		return position;
	}
}
