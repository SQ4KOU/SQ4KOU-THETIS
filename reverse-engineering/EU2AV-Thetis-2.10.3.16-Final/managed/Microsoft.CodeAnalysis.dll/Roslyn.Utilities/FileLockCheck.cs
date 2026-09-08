using System;
using System.Collections.Immutable;
using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Roslyn.Utilities;

internal static class FileLockCheck
{
	private struct FILETIME
	{
		public uint dwLowDateTime;

		public uint dwHighDateTime;
	}

	private struct RM_UNIQUE_PROCESS
	{
		public uint dwProcessId;

		public FILETIME ProcessStartTime;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	private struct RM_PROCESS_INFO
	{
		private const int CCH_RM_MAX_APP_NAME = 255;

		private const int CCH_RM_MAX_SVC_NAME = 63;

		internal RM_UNIQUE_PROCESS Process;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
		public string strAppName;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
		public string strServiceShortName;

		internal int ApplicationType;

		public uint AppStatus;

		public uint TSSessionId;

		[MarshalAs(UnmanagedType.Bool)]
		public bool bRestartable;
	}

	private const string RestartManagerDll = "rstrtmgr.dll";

	[DllImport("rstrtmgr.dll", CharSet = CharSet.Unicode)]
	private static extern int RmRegisterResources(uint pSessionHandle, uint nFiles, string[] rgsFilenames, uint nApplications, [In] RM_UNIQUE_PROCESS[]? rgApplications, uint nServices, string[]? rgsServiceNames);

	[DllImport("rstrtmgr.dll", CharSet = CharSet.Unicode)]
	private unsafe static extern int RmStartSession(out uint pSessionHandle, int dwSessionFlags, char* strSessionKey);

	[DllImport("rstrtmgr.dll")]
	private static extern int RmEndSession(uint pSessionHandle);

	[DllImport("rstrtmgr.dll", CharSet = CharSet.Unicode)]
	private static extern int RmGetList(uint dwSessionHandle, out uint pnProcInfoNeeded, ref uint pnProcInfo, [In][Out] RM_PROCESS_INFO[]? rgAffectedApps, ref uint lpdwRebootReasons);

	public static ImmutableArray<(int processId, string applicationName)> TryGetLockingProcessInfos(string path)
	{
		if (!PlatformInformation.IsWindows)
		{
			return ImmutableArray<(int, string)>.Empty;
		}
		try
		{
			return GetLockingProcessInfosImpl(new string[1] { path });
		}
		catch
		{
			return ImmutableArray<(int, string)>.Empty;
		}
	}

	private unsafe static ImmutableArray<(int processId, string applicationName)> GetLockingProcessInfosImpl(string[] paths)
	{
		char* strSessionKey = stackalloc char[sizeof(Guid) * 2 + 1];
		if (RmStartSession(out var pSessionHandle, 0, strSessionKey) != 0)
		{
			return ImmutableArray<(int, string)>.Empty;
		}
		try
		{
			if (RmRegisterResources(pSessionHandle, (uint)paths.Length, paths, 0u, null, 0u, null) != 0)
			{
				return ImmutableArray<(int, string)>.Empty;
			}
			uint pnProcInfo = 0u;
			RM_PROCESS_INFO[] array = null;
			int num = 0;
			while (true)
			{
				uint lpdwRebootReasons = 0u;
				int num2 = RmGetList(pSessionHandle, out var pnProcInfoNeeded, ref pnProcInfo, array, ref lpdwRebootReasons);
				switch (num2)
				{
				case 0:
				{
					if (pnProcInfo == 0)
					{
						return ImmutableArray<(int, string)>.Empty;
					}
					ArrayBuilder<(int, string)> instance = ArrayBuilder<(int, string)>.GetInstance((int)pnProcInfo);
					for (int i = 0; i < pnProcInfo; i++)
					{
						instance.Add(((int)array[i].Process.dwProcessId, array[i].strAppName));
					}
					return instance.ToImmutableAndFree();
				}
				default:
					return ImmutableArray<(int, string)>.Empty;
				case 234:
					pnProcInfo = pnProcInfoNeeded;
					array = new RM_PROCESS_INFO[pnProcInfo];
					if (num2 == 234 && num++ < 6)
					{
						continue;
					}
					break;
				}
				break;
			}
		}
		finally
		{
			RmEndSession(pSessionHandle);
		}
		return ImmutableArray<(int, string)>.Empty;
	}
}
