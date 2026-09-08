using System.Linq;

namespace WindowsFirewallHelper.InternalHelpers;

internal static class PortHelper
{
	public static string PortsToString(ushort[] ports)
	{
		string[] value = (from pair in (from port in ports.Distinct()
				orderby port
				select port).Select((ushort port, int index) => new
			{
				PortNumber = port,
				GroupId = port - index
			})
			group pair by pair.GroupId into groups
			select (groups.Count() < 3) ? string.Join(",", groups.Select(pair => pair.PortNumber.ToString("")).ToArray()) : (groups.First().PortNumber + "-" + groups.Last().PortNumber)).ToArray();
		return string.Join(",", value);
	}

	public static ushort[] StringToPorts(string str)
	{
		if (string.IsNullOrEmpty(str?.Trim()))
		{
			return new ushort[0];
		}
		return (from port in str.Trim().Split(',').SelectMany(delegate(string port)
			{
				string[] array = port.Trim().Split('-');
				if (array.Length == 2 && ushort.TryParse(array[0].Trim(), out var result) && ushort.TryParse(array[1].Trim(), out var result2))
				{
					return from p in Enumerable.Range(result, result2 - result + 1)
						select (ushort)p;
				}
				ushort result3;
				return (array.Length == 1 && ushort.TryParse(port.Trim(), out result3)) ? new ushort[1] { result3 } : new ushort[0];
			})
				.Distinct()
			orderby port
			select port).ToArray();
	}
}
