using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis;

internal sealed class MonoGlobalAssemblyCache : GlobalAssemblyCache
{
	private static readonly string s_corlibDirectory;

	private static readonly string s_gacDirectory;

	static MonoGlobalAssemblyCache()
	{
		s_corlibDirectory = Path.GetDirectoryName(typeof(object).Assembly.Location);
		s_gacDirectory = Directory.GetParent(Path.GetDirectoryName(typeof(Uri).Assembly.Location)).Parent.FullName;
	}

	private static AssemblyName CreateAssemblyNameFromFile(string path)
	{
		return AssemblyName.GetAssemblyName(path);
	}

	private static IEnumerable<string> GetGacAssemblyPaths(string gacPath, string name, Version version, byte[] publicKeyTokenBytes)
	{
		string fileName = name + ".dll";
		string text = Path.Combine(s_corlibDirectory, fileName);
		if (!File.Exists(text))
		{
			text = Path.Combine(s_corlibDirectory, "Facades", fileName);
		}
		if (File.Exists(text))
		{
			yield return text;
			yield break;
		}
		string publicKeyToken = ToHexString(publicKeyTokenBytes);
		if (version != null && publicKeyToken != null)
		{
			yield return Path.Combine(gacPath, name, version?.ToString() + "__" + publicKeyToken, fileName);
			yield break;
		}
		DirectoryInfo directoryInfo = new DirectoryInfo(Path.Combine(gacPath, name));
		if (!directoryInfo.Exists)
		{
			yield break;
		}
		DirectoryInfo[] directories = directoryInfo.GetDirectories();
		foreach (DirectoryInfo directoryInfo2 in directories)
		{
			if ((!(version != null) || directoryInfo2.Name.StartsWith(version.ToString(), StringComparison.Ordinal)) && (publicKeyToken == null || directoryInfo2.Name.EndsWith(publicKeyToken, StringComparison.Ordinal)))
			{
				string text2 = Path.Combine(directoryInfo2.ToString(), fileName);
				if (File.Exists(text2))
				{
					yield return text2;
				}
			}
		}
	}

	private static IEnumerable<(AssemblyIdentity Identity, string Path)> GetAssemblyIdentitiesAndPaths(AssemblyName name, ImmutableArray<ProcessorArchitecture> architectureFilter)
	{
		if (name == null)
		{
			return GetAssemblyIdentitiesAndPaths(null, null, null, architectureFilter);
		}
		return GetAssemblyIdentitiesAndPaths(name.Name, name.Version, name.GetPublicKeyToken(), architectureFilter);
	}

	private static IEnumerable<(AssemblyIdentity Identity, string Path)> GetAssemblyIdentitiesAndPaths(string name, Version version, byte[] publicKeyToken, ImmutableArray<ProcessorArchitecture> architectureFilter)
	{
		IEnumerable<string> gacAssemblyPaths = GetGacAssemblyPaths(s_gacDirectory, name, version, publicKeyToken);
		foreach (string item2 in gacAssemblyPaths)
		{
			if (File.Exists(item2))
			{
				AssemblyName assemblyName = CreateAssemblyNameFromFile(item2);
				if (assemblyName.ProcessorArchitecture == ProcessorArchitecture.None || !(architectureFilter != default(ImmutableArray<ProcessorArchitecture>)) || architectureFilter.Length <= 0 || architectureFilter.Contains(assemblyName.ProcessorArchitecture))
				{
					AssemblyIdentity item = new AssemblyIdentity(assemblyName.Name, assemblyName.Version, assemblyName.CultureName, ImmutableArray.Create(assemblyName.GetPublicKeyToken()));
					yield return (Identity: item, Path: item2);
				}
			}
		}
	}

	public override IEnumerable<AssemblyIdentity> GetAssemblyIdentities(AssemblyName partialName, ImmutableArray<ProcessorArchitecture> architectureFilter = default(ImmutableArray<ProcessorArchitecture>))
	{
		return from identityAndPath in GetAssemblyIdentitiesAndPaths(partialName, architectureFilter)
			select identityAndPath.Identity;
	}

	public override IEnumerable<AssemblyIdentity> GetAssemblyIdentities(string partialName = null, ImmutableArray<ProcessorArchitecture> architectureFilter = default(ImmutableArray<ProcessorArchitecture>))
	{
		AssemblyName partialName2;
		try
		{
			partialName2 = ((partialName == null) ? null : new AssemblyName(partialName));
		}
		catch
		{
			return SpecializedCollections.EmptyEnumerable<AssemblyIdentity>();
		}
		return GetAssemblyIdentities(partialName2, architectureFilter);
	}

	public override IEnumerable<string> GetAssemblySimpleNames(ImmutableArray<ProcessorArchitecture> architectureFilter = default(ImmutableArray<ProcessorArchitecture>))
	{
		return (from identityAndPath in GetAssemblyIdentitiesAndPaths(null, null, null, architectureFilter)
			select identityAndPath.Identity.Name).Distinct();
	}

	public override AssemblyIdentity ResolvePartialName(string displayName, out string location, ImmutableArray<ProcessorArchitecture> architectureFilter, CultureInfo preferredCulture)
	{
		if (displayName == null)
		{
			throw new ArgumentNullException("displayName");
		}
		string text = ((preferredCulture != null && !preferredCulture.IsNeutralCulture) ? preferredCulture.Name : null);
		AssemblyName name = new AssemblyName(displayName);
		AssemblyIdentity result = null;
		location = null;
		bool flag = false;
		foreach (var assemblyIdentitiesAndPath in GetAssemblyIdentitiesAndPaths(name, architectureFilter))
		{
			string item = assemblyIdentitiesAndPath.Path;
			if (File.Exists(item))
			{
				AssemblyName assemblyName = CreateAssemblyNameFromFile(item);
				flag = text == null || assemblyName.CultureName == text;
				if ((location == null) | flag)
				{
					location = item;
					(result, _) = assemblyIdentitiesAndPath;
				}
				if (flag)
				{
					break;
				}
			}
		}
		return result;
	}

	private static string ToHexString(byte[] bytes)
	{
		if (bytes == null)
		{
			return null;
		}
		PooledStringBuilder instance = PooledStringBuilder.GetInstance();
		foreach (byte b in bytes)
		{
			instance.Builder.Append(b.ToString("x2"));
		}
		return instance.ToStringAndFree();
	}
}
