using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

internal class Interop
{
	internal unsafe static void GetRandomBytes(byte* buffer, int length)
	{
		if (!System.LocalAppContextSwitches.UseNonRandomizedHashSeed)
		{
			using (RandomNumberGenerator randomNumberGenerator = RandomNumberGenerator.Create())
			{
				byte[] array = new byte[length];
				randomNumberGenerator.GetBytes(array);
				Marshal.Copy(array, 0, (IntPtr)buffer, length);
			}
		}
	}
}
