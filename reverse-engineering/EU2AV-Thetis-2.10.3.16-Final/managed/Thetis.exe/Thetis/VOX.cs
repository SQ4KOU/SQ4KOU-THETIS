namespace Thetis;

internal static class VOX
{
	public static void PushVox(int id, int active)
	{
		Audio.VOXActive = active == 1;
	}
}
