using System;
using System.Linq;

namespace WindowsFirewallHelper.InternalHelpers;

internal class ComClassProgIdAttribute : Attribute
{
	public string ClassProgId { get; }

	public ComClassProgIdAttribute(string classProgId)
	{
		ClassProgId = classProgId;
	}

	public static string GetClassProgId<T>()
	{
		return typeof(T).GetCustomAttributes(typeof(ComClassProgIdAttribute), inherit: true).OfType<ComClassProgIdAttribute>().FirstOrDefault()?.ClassProgId;
	}
}
