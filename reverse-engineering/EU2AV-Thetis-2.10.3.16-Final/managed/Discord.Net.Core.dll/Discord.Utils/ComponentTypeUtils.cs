namespace Discord.Utils;

public static class ComponentTypeUtils
{
	public static bool IsSelectType(this ComponentType type)
	{
		if (type == ComponentType.SelectMenu || (uint)(type - 5) <= 3u)
		{
			return true;
		}
		return false;
	}
}
