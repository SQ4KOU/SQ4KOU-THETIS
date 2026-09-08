using System;
using System.Runtime.InteropServices;
using System.Text;

namespace WindowsFirewallHelper.InternalHelpers;

internal class NativeHelper
{
	public static string ResolveStringResource(string str)
	{
		if (str == null || !str.StartsWith("@"))
		{
			return str;
		}
		try
		{
			StringBuilder stringBuilder = new StringBuilder(8192);
			if (SHLoadIndirectString(Environment.ExpandEnvironmentVariables(str), stringBuilder, stringBuilder.Capacity, IntPtr.Zero) == 0)
			{
				str = stringBuilder.ToString();
			}
			else
			{
				int num = str.LastIndexOf(",", StringComparison.InvariantCulture);
				if (num > 1)
				{
					string s = str.Substring(num + 1);
					string fileName = Environment.ExpandEnvironmentVariables(str.Substring(1, num - 1));
					uint resourceId = (uint)Math.Abs(int.Parse(s));
					IntPtr libraryHandle = LoadLibrary(fileName);
					if (LoadString(libraryHandle, resourceId, stringBuilder, stringBuilder.Capacity) > 0)
					{
						str = stringBuilder.ToString();
					}
					FreeLibrary(libraryHandle);
				}
			}
		}
		catch
		{
		}
		return str;
	}

	[DllImport("kernel32")]
	private static extern int FreeLibrary(IntPtr libraryHandle);

	[DllImport("kernel32", BestFitMapping = false, CharSet = CharSet.Auto, SetLastError = true, ThrowOnUnmappableChar = true)]
	private static extern IntPtr LoadLibrary(string fileName);

	[DllImport("user32", BestFitMapping = false, CharSet = CharSet.Auto, SetLastError = true, ThrowOnUnmappableChar = true)]
	private static extern int LoadString(IntPtr libraryHandle, uint resourceId, StringBuilder buffer, int bufferSize);

	[DllImport("shlwapi", BestFitMapping = false, CharSet = CharSet.Auto, SetLastError = true, ThrowOnUnmappableChar = true)]
	private static extern int SHLoadIndirectString(string resourceString, StringBuilder buffer, int bufferSize, IntPtr reserved);
}
