using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.Scripting;

namespace Microsoft.CodeAnalysis;

internal sealed class ClrGlobalAssemblyCache : GlobalAssemblyCache
{
	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("21b8916c-f28e-11d2-a473-00c04f8ef448")]
	private interface IAssemblyEnum
	{
		[PreserveSig]
		int GetNextAssembly(out FusionAssemblyIdentity.IApplicationContext ppAppCtx, out FusionAssemblyIdentity.IAssemblyName ppName, uint dwFlags);

		[PreserveSig]
		int Reset();

		[PreserveSig]
		int Clone(out IAssemblyEnum ppEnum);
	}

	[ComImport]
	[Guid("e707dcde-d1cd-11d2-bab9-00c04f8eceae")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	private interface IAssemblyCache
	{
		void UninstallAssembly();

		void QueryAssemblyInfo(uint dwFlags, [MarshalAs(UnmanagedType.LPWStr)] string pszAssemblyName, ref ASSEMBLY_INFO pAsmInfo);

		void CreateAssemblyCacheItem();

		void CreateAssemblyScavenger();

		void InstallAssembly();
	}

	private struct ASSEMBLY_INFO
	{
		public uint cbAssemblyInfo;

		public readonly uint dwAssemblyFlags;

		public readonly ulong uliAssemblySizeInKB;

		public unsafe char* pszCurrentAssemblyPathBuf;

		public uint cchBuf;
	}

	private const int MAX_PATH = 260;

	private const int S_OK = 0;

	private const int S_FALSE = 1;

	[DllImport("clr")]
	private static extern int CreateAssemblyEnum(out IAssemblyEnum ppEnum, FusionAssemblyIdentity.IApplicationContext pAppCtx, FusionAssemblyIdentity.IAssemblyName pName, GlobalAssemblyCacheLocation.ASM_CACHE dwFlags, IntPtr pvReserved);

	[DllImport("clr", PreserveSig = false)]
	private static extern void CreateAssemblyCache(out IAssemblyCache ppAsmCache, uint dwReserved);

	public override IEnumerable<AssemblyIdentity> GetAssemblyIdentities(AssemblyName partialName, ImmutableArray<ProcessorArchitecture> architectureFilter = default(ImmutableArray<ProcessorArchitecture>))
	{
		return GetAssemblyIdentities(FusionAssemblyIdentity.ToAssemblyNameObject(partialName), architectureFilter);
	}

	public override IEnumerable<AssemblyIdentity> GetAssemblyIdentities(string partialName = null, ImmutableArray<ProcessorArchitecture> architectureFilter = default(ImmutableArray<ProcessorArchitecture>))
	{
		FusionAssemblyIdentity.IAssemblyName assemblyName;
		if (partialName != null)
		{
			assemblyName = FusionAssemblyIdentity.ToAssemblyNameObject(partialName);
			if (assemblyName == null)
			{
				return SpecializedCollections.EmptyEnumerable<AssemblyIdentity>();
			}
		}
		else
		{
			assemblyName = null;
		}
		return GetAssemblyIdentities(assemblyName, architectureFilter);
	}

	public override IEnumerable<string> GetAssemblySimpleNames(ImmutableArray<ProcessorArchitecture> architectureFilter = default(ImmutableArray<ProcessorArchitecture>))
	{
		return (from nameObject in GetAssemblyObjects(null, architectureFilter)
			select FusionAssemblyIdentity.GetName(nameObject)).Distinct();
	}

	private static IEnumerable<AssemblyIdentity> GetAssemblyIdentities(FusionAssemblyIdentity.IAssemblyName partialName, ImmutableArray<ProcessorArchitecture> architectureFilter)
	{
		return from nameObject in GetAssemblyObjects(partialName, architectureFilter)
			select FusionAssemblyIdentity.ToAssemblyIdentity(nameObject);
	}

	internal static IEnumerable<FusionAssemblyIdentity.IAssemblyName> GetAssemblyObjects(FusionAssemblyIdentity.IAssemblyName partialNameFilter, ImmutableArray<ProcessorArchitecture> architectureFilter)
	{
		FusionAssemblyIdentity.IApplicationContext ppAppCtx = null;
		int num = CreateAssemblyEnum(out var enumerator, ppAppCtx, partialNameFilter, GlobalAssemblyCacheLocation.ASM_CACHE.GAC, IntPtr.Zero);
		switch (num)
		{
		case 1:
			yield break;
		default:
		{
			Exception exceptionForHR = Marshal.GetExceptionForHR(num);
			if (exceptionForHR is FileNotFoundException || exceptionForHR is DirectoryNotFoundException)
			{
				yield break;
			}
			if (exceptionForHR != null)
			{
				throw exceptionForHR;
			}
			throw new ArgumentException(ScriptingResources.InvalidAssemblyName);
		}
		case 0:
			break;
		}
		while (true)
		{
			num = enumerator.GetNextAssembly(out ppAppCtx, out var ppName, 0u);
			if (num != 0)
			{
				break;
			}
			if (!architectureFilter.IsDefault)
			{
				ProcessorArchitecture processorArchitecture = FusionAssemblyIdentity.GetProcessorArchitecture(ppName);
				if (!architectureFilter.Contains(processorArchitecture))
				{
					continue;
				}
			}
			yield return ppName;
		}
		if (num < 0)
		{
			Marshal.ThrowExceptionForHR(num);
		}
	}

	public override AssemblyIdentity ResolvePartialName(string displayName, out string location, ImmutableArray<ProcessorArchitecture> architectureFilter, CultureInfo preferredCulture)
	{
		if (displayName == null)
		{
			throw new ArgumentNullException("displayName");
		}
		location = null;
		FusionAssemblyIdentity.IAssemblyName assemblyName = FusionAssemblyIdentity.ToAssemblyNameObject(displayName);
		if (assemblyName == null)
		{
			return null;
		}
		IEnumerable<FusionAssemblyIdentity.IAssemblyName> assemblyObjects = GetAssemblyObjects(assemblyName, architectureFilter);
		string preferredCultureOpt = ((preferredCulture != null && !preferredCulture.IsNeutralCulture) ? preferredCulture.Name : null);
		FusionAssemblyIdentity.IAssemblyName bestMatch = FusionAssemblyIdentity.GetBestMatch(assemblyObjects, preferredCultureOpt);
		if (bestMatch == null)
		{
			return null;
		}
		location = GetAssemblyLocation(bestMatch);
		return FusionAssemblyIdentity.ToAssemblyIdentity(bestMatch);
	}

	internal unsafe static string GetAssemblyLocation(FusionAssemblyIdentity.IAssemblyName nameObject)
	{
		string displayName = FusionAssemblyIdentity.GetDisplayName(nameObject, FusionAssemblyIdentity.ASM_DISPLAYF.FULL);
		fixed (char* pszCurrentAssemblyPathBuf = new char[260])
		{
			ASSEMBLY_INFO pAsmInfo = new ASSEMBLY_INFO
			{
				cbAssemblyInfo = (uint)Marshal.SizeOf<ASSEMBLY_INFO>(),
				pszCurrentAssemblyPathBuf = pszCurrentAssemblyPathBuf,
				cchBuf = 260u
			};
			CreateAssemblyCache(out var ppAsmCache, 0u);
			ppAsmCache.QueryAssemblyInfo(0u, displayName, ref pAsmInfo);
			return Marshal.PtrToStringUni((IntPtr)pAsmInfo.pszCurrentAssemblyPathBuf, (int)(pAsmInfo.cchBuf - 1));
		}
	}
}
