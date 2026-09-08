using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis.Scripting;

namespace Microsoft.CodeAnalysis;

internal sealed class FusionAssemblyIdentity
{
	[Flags]
	internal enum ASM_DISPLAYF
	{
		VERSION = 1,
		CULTURE = 2,
		PUBLIC_KEY_TOKEN = 4,
		PUBLIC_KEY = 8,
		CUSTOM = 0x10,
		PROCESSORARCHITECTURE = 0x20,
		LANGUAGEID = 0x40,
		RETARGET = 0x80,
		CONFIG_MASK = 0x100,
		MVID = 0x200,
		CONTENT_TYPE = 0x400,
		FULL = VERSION | CULTURE | PUBLIC_KEY_TOKEN | PROCESSORARCHITECTURE | RETARGET | CONTENT_TYPE
	}

	internal enum PropertyId
	{
		PUBLIC_KEY,
		PUBLIC_KEY_TOKEN,
		HASH_VALUE,
		NAME,
		MAJOR_VERSION,
		MINOR_VERSION,
		BUILD_NUMBER,
		REVISION_NUMBER,
		CULTURE,
		PROCESSOR_ID_ARRAY,
		OSINFO_ARRAY,
		HASH_ALGID,
		ALIAS,
		CODEBASE_URL,
		CODEBASE_LASTMOD,
		NULL_PUBLIC_KEY,
		NULL_PUBLIC_KEY_TOKEN,
		CUSTOM,
		NULL_CUSTOM,
		MVID,
		FILE_MAJOR_VERSION,
		FILE_MINOR_VERSION,
		FILE_BUILD_NUMBER,
		FILE_REVISION_NUMBER,
		RETARGET,
		SIGNATURE_BLOB,
		CONFIG_MASK,
		ARCHITECTURE,
		CONTENT_TYPE,
		MAX_PARAMS
	}

	private static class CANOF
	{
		public const uint PARSE_DISPLAY_NAME = 1u;

		public const uint SET_DEFAULT_VALUES = 2u;
	}

	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("CD193BC0-B4BC-11d2-9833-00C04FC31D2E")]
	internal interface IAssemblyName
	{
		unsafe void SetProperty(PropertyId id, void* data, uint size);

		[PreserveSig]
		unsafe int GetProperty(PropertyId id, void* data, ref uint size);

		[PreserveSig]
		int Finalize();

		[PreserveSig]
		unsafe int GetDisplayName(byte* buffer, ref uint characterCount, ASM_DISPLAYF dwDisplayFlags);

		[PreserveSig]
		int __BindToObject();

		[PreserveSig]
		int __GetName();

		[PreserveSig]
		int GetVersion(out uint versionHi, out uint versionLow);

		[PreserveSig]
		int IsEqual(IAssemblyName pName, uint dwCmpFlags);

		[PreserveSig]
		int Clone(out IAssemblyName pName);
	}

	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("7c23ff90-33af-11d3-95da-00a024a85b51")]
	internal interface IApplicationContext
	{
	}

	private static readonly object s_assemblyIdentityGate = new object();

	private const int ERROR_INSUFFICIENT_BUFFER = -2147024774;

	private const int FUSION_E_INVALID_NAME = -2146234297;

	private static int CreateAssemblyNameObject(out IAssemblyName ppEnum, string szAssemblyName, uint dwFlags, IntPtr pvReserved)
	{
		lock (s_assemblyIdentityGate)
		{
			return RealCreateAssemblyNameObject(out ppEnum, szAssemblyName, dwFlags, pvReserved);
		}
	}

	[DllImport("clr", CharSet = CharSet.Unicode, EntryPoint = "CreateAssemblyNameObject")]
	private static extern int RealCreateAssemblyNameObject(out IAssemblyName ppEnum, [MarshalAs(UnmanagedType.LPWStr)] string szAssemblyName, uint dwFlags, IntPtr pvReserved);

	internal unsafe static string GetDisplayName(IAssemblyName nameObject, ASM_DISPLAYF displayFlags)
	{
		uint characterCount = 0u;
		int displayName = nameObject.GetDisplayName(null, ref characterCount, displayFlags);
		switch (displayName)
		{
		case 0:
			return string.Empty;
		default:
			throw Marshal.GetExceptionForHR(displayName);
		case -2147024774:
			fixed (byte* ptr = new byte[characterCount * 2])
			{
				displayName = nameObject.GetDisplayName(ptr, ref characterCount, displayFlags);
				if (displayName != 0)
				{
					throw Marshal.GetExceptionForHR(displayName);
				}
				return Marshal.PtrToStringUni((IntPtr)ptr, (int)(characterCount - 1));
			}
		}
	}

	internal unsafe static byte[] GetPropertyBytes(IAssemblyName nameObject, PropertyId propertyId)
	{
		uint size = 0u;
		int property = nameObject.GetProperty(propertyId, null, ref size);
		switch (property)
		{
		case 0:
			return null;
		default:
			throw Marshal.GetExceptionForHR(property);
		case -2147024774:
		{
			byte[] array = new byte[size];
			fixed (byte* data = array)
			{
				property = nameObject.GetProperty(propertyId, data, ref size);
				if (property != 0)
				{
					throw Marshal.GetExceptionForHR(property);
				}
			}
			return array;
		}
		}
	}

	internal unsafe static string GetPropertyString(IAssemblyName nameObject, PropertyId propertyId)
	{
		byte[] propertyBytes = GetPropertyBytes(nameObject, propertyId);
		if (propertyBytes == null)
		{
			return null;
		}
		fixed (byte* ptr = propertyBytes)
		{
			return Marshal.PtrToStringUni((IntPtr)ptr, propertyBytes.Length / 2 - 1);
		}
	}

	internal unsafe static bool IsKeyOrTokenEmpty(IAssemblyName nameObject, PropertyId propertyId)
	{
		uint size = 0u;
		return nameObject.GetProperty(propertyId, null, ref size) == 0;
	}

	internal static Version GetVersion(IAssemblyName nameObject)
	{
		if (nameObject.GetVersion(out var versionHi, out var versionLow) != 0)
		{
			return null;
		}
		return new Version((int)(versionHi >> 16), (int)(versionHi & 0xFFFF), (int)(versionLow >> 16), (int)(versionLow & 0xFFFF));
	}

	internal static Version GetVersion(IAssemblyName name, out AssemblyIdentityParts parts)
	{
		uint? propertyWord = GetPropertyWord(name, PropertyId.MAJOR_VERSION);
		uint? propertyWord2 = GetPropertyWord(name, PropertyId.MINOR_VERSION);
		uint? propertyWord3 = GetPropertyWord(name, PropertyId.BUILD_NUMBER);
		uint? propertyWord4 = GetPropertyWord(name, PropertyId.REVISION_NUMBER);
		parts = (AssemblyIdentityParts)0;
		if (propertyWord.HasValue)
		{
			parts |= AssemblyIdentityParts.VersionMajor;
		}
		if (propertyWord2.HasValue)
		{
			parts |= AssemblyIdentityParts.VersionMinor;
		}
		if (propertyWord3.HasValue)
		{
			parts |= AssemblyIdentityParts.VersionBuild;
		}
		if (propertyWord4.HasValue)
		{
			parts |= AssemblyIdentityParts.VersionRevision;
		}
		return new Version((int)propertyWord.GetValueOrDefault(), (int)propertyWord2.GetValueOrDefault(), (int)propertyWord3.GetValueOrDefault(), (int)propertyWord4.GetValueOrDefault());
	}

	internal static byte[] GetPublicKeyToken(IAssemblyName nameObject)
	{
		byte[] propertyBytes = GetPropertyBytes(nameObject, PropertyId.PUBLIC_KEY_TOKEN);
		if (propertyBytes != null)
		{
			return propertyBytes;
		}
		if (IsKeyOrTokenEmpty(nameObject, PropertyId.NULL_PUBLIC_KEY_TOKEN))
		{
			return Array.Empty<byte>();
		}
		return null;
	}

	internal static byte[] GetPublicKey(IAssemblyName nameObject)
	{
		byte[] propertyBytes = GetPropertyBytes(nameObject, PropertyId.PUBLIC_KEY);
		if (propertyBytes != null)
		{
			return propertyBytes;
		}
		if (IsKeyOrTokenEmpty(nameObject, PropertyId.NULL_PUBLIC_KEY))
		{
			return Array.Empty<byte>();
		}
		return null;
	}

	internal unsafe static uint? GetPropertyWord(IAssemblyName nameObject, PropertyId propertyId)
	{
		uint size = 4u;
		uint value = default(uint);
		int property = nameObject.GetProperty(propertyId, &value, ref size);
		if (property != 0)
		{
			throw Marshal.GetExceptionForHR(property);
		}
		if (size == 0)
		{
			return null;
		}
		return value;
	}

	internal static string GetName(IAssemblyName nameObject)
	{
		return GetPropertyString(nameObject, PropertyId.NAME);
	}

	internal static string GetCulture(IAssemblyName nameObject)
	{
		return GetPropertyString(nameObject, PropertyId.CULTURE);
	}

	internal static AssemblyContentType GetContentType(IAssemblyName nameObject)
	{
		return (AssemblyContentType)GetPropertyWord(nameObject, PropertyId.CONTENT_TYPE).GetValueOrDefault();
	}

	internal static ProcessorArchitecture GetProcessorArchitecture(IAssemblyName nameObject)
	{
		return (ProcessorArchitecture)GetPropertyWord(nameObject, PropertyId.ARCHITECTURE).GetValueOrDefault();
	}

	internal static AssemblyNameFlags GetFlags(IAssemblyName nameObject)
	{
		AssemblyNameFlags assemblyNameFlags = AssemblyNameFlags.None;
		if (GetPropertyWord(nameObject, PropertyId.RETARGET).GetValueOrDefault() != 0)
		{
			assemblyNameFlags |= AssemblyNameFlags.Retargetable;
		}
		return assemblyNameFlags;
	}

	private unsafe static void SetProperty(IAssemblyName nameObject, PropertyId propertyId, string data)
	{
		if (data == null)
		{
			nameObject.SetProperty(propertyId, null, 0u);
			return;
		}
		fixed (char* data2 = data)
		{
			nameObject.SetProperty(propertyId, data2, (uint)((data.Length + 1) * 2));
		}
	}

	private unsafe static void SetProperty(IAssemblyName nameObject, PropertyId propertyId, byte[] data)
	{
		if (data == null)
		{
			nameObject.SetProperty(propertyId, null, 0u);
			return;
		}
		fixed (byte* data2 = data)
		{
			nameObject.SetProperty(propertyId, data2, (uint)data.Length);
		}
	}

	private unsafe static void SetProperty(IAssemblyName nameObject, PropertyId propertyId, ushort data)
	{
		nameObject.SetProperty(propertyId, &data, 2u);
	}

	private unsafe static void SetProperty(IAssemblyName nameObject, PropertyId propertyId, uint data)
	{
		nameObject.SetProperty(propertyId, &data, 4u);
	}

	private unsafe static void SetPublicKeyToken(IAssemblyName nameObject, byte[] value)
	{
		if (value != null && value.Length == 0)
		{
			nameObject.SetProperty(PropertyId.NULL_PUBLIC_KEY_TOKEN, null, 0u);
		}
		else
		{
			SetProperty(nameObject, PropertyId.PUBLIC_KEY_TOKEN, value);
		}
	}

	internal static AssemblyIdentity ToAssemblyIdentity(IAssemblyName nameObject)
	{
		if (nameObject == null)
		{
			return null;
		}
		AssemblyNameFlags flags = GetFlags(nameObject);
		byte[] publicKey = GetPublicKey(nameObject);
		bool flag = publicKey != null && publicKey.Length != 0;
		AssemblyIdentityParts parts;
		return new AssemblyIdentity(GetName(nameObject), GetVersion(nameObject, out parts), GetCulture(nameObject) ?? "", (flag ? publicKey : GetPublicKeyToken(nameObject)).AsImmutableOrNull(), flag, (flags & AssemblyNameFlags.Retargetable) != 0, GetContentType(nameObject));
	}

	internal static IAssemblyName ToAssemblyNameObject(AssemblyName name)
	{
		if (name == null)
		{
			return null;
		}
		Marshal.ThrowExceptionForHR(CreateAssemblyNameObject(out var ppEnum, null, 0u, IntPtr.Zero));
		string name2 = name.Name;
		if (name2 != null)
		{
			if (name2.IndexOf('\0') >= 0)
			{
				throw new ArgumentException(ScriptingResources.InvalidCharactersInAssemblyName, "name");
			}
			SetProperty(ppEnum, PropertyId.NAME, name2);
		}
		if (name.Version != null)
		{
			SetProperty(ppEnum, PropertyId.MAJOR_VERSION, (ushort)name.Version.Major);
			SetProperty(ppEnum, PropertyId.MINOR_VERSION, (ushort)name.Version.Minor);
			SetProperty(ppEnum, PropertyId.BUILD_NUMBER, (ushort)name.Version.Build);
			SetProperty(ppEnum, PropertyId.REVISION_NUMBER, (ushort)name.Version.Revision);
		}
		string cultureName = name.CultureName;
		if (cultureName != null)
		{
			if (cultureName.IndexOf('\0') >= 0)
			{
				throw new ArgumentException(ScriptingResources.InvalidCharactersInAssemblyName, "name");
			}
			SetProperty(ppEnum, PropertyId.CULTURE, cultureName);
		}
		if (name.Flags == AssemblyNameFlags.Retargetable)
		{
			SetProperty(ppEnum, PropertyId.RETARGET, 1u);
		}
		if (name.ContentType != AssemblyContentType.Default)
		{
			SetProperty(ppEnum, PropertyId.CONTENT_TYPE, (uint)name.ContentType);
		}
		byte[] publicKeyToken = name.GetPublicKeyToken();
		SetPublicKeyToken(ppEnum, publicKeyToken);
		return ppEnum;
	}

	internal static IAssemblyName ToAssemblyNameObject(string displayName)
	{
		if (displayName.IndexOf('\0') >= 0)
		{
			return null;
		}
		if (CreateAssemblyNameObject(out var ppEnum, displayName, 1u, IntPtr.Zero) != 0)
		{
			return null;
		}
		return ppEnum;
	}

	internal static IAssemblyName GetBestMatch(IEnumerable<IAssemblyName> candidates, string preferredCultureOpt)
	{
		IAssemblyName assemblyName = null;
		Version version = null;
		string text = null;
		foreach (IAssemblyName candidate in candidates)
		{
			if (assemblyName != null)
			{
				Version version2 = GetVersion(candidate);
				if (version == null)
				{
					version = GetVersion(assemblyName);
				}
				int num = version.CompareTo(version2);
				if (num == 0)
				{
					if (preferredCultureOpt != null)
					{
						string culture = GetCulture(candidate);
						if (text == null)
						{
							text = GetCulture(candidate);
						}
						if (StringComparer.OrdinalIgnoreCase.Equals(culture, preferredCultureOpt) || (culture.Length == 0 && !StringComparer.OrdinalIgnoreCase.Equals(text, preferredCultureOpt)))
						{
							assemblyName = candidate;
							version = version2;
							text = culture;
						}
					}
				}
				else if (num < 0)
				{
					assemblyName = candidate;
					version = version2;
				}
			}
			else
			{
				assemblyName = candidate;
			}
		}
		return assemblyName;
	}
}
