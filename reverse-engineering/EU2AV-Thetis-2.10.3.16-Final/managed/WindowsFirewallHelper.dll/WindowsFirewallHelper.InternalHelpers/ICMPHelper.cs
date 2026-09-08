using System.Collections.Generic;

namespace WindowsFirewallHelper.InternalHelpers;

internal static class ICMPHelper
{
	public static string ICMToString(FirewallWASInternetControlMessage[] internetControlMessages)
	{
		string[] array = new string[internetControlMessages.Length];
		for (int i = 0; i < internetControlMessages.Length; i++)
		{
			array[i] = internetControlMessages[i].ToString();
		}
		if (array.Length != 0)
		{
			return string.Join(",", array);
		}
		return null;
	}

	public static FirewallWASInternetControlMessage[] StringToICM(string str)
	{
		if (string.IsNullOrEmpty(str?.Trim()))
		{
			return new FirewallWASInternetControlMessage[0];
		}
		List<FirewallWASInternetControlMessage> list = new List<FirewallWASInternetControlMessage>();
		string[] array = str.Split(',');
		for (int i = 0; i < array.Length; i++)
		{
			if (FirewallWASInternetControlMessage.TryParse(array[i], out var icm))
			{
				list.Add(icm);
			}
		}
		return list.ToArray();
	}
}
