using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Scripting.Hosting;

public sealed class InteractiveAssemblyLoader : IDisposable
{
	private class LoadedAssembly
	{
		public string? OriginalPath { get; set; }
	}

	[DebuggerDisplay("{GetDebuggerDisplay(),nq}")]
	private readonly struct AssemblyIdentityAndLocation(AssemblyIdentity identity, string location)
	{
		public readonly AssemblyIdentity Identity = identity;

		public readonly string Location = location;

		private string GetDebuggerDisplay()
		{
			return Identity?.ToString() + " @ " + Location;
		}
	}

	[DebuggerDisplay("{GetDebuggerDisplay(),nq}")]
	private readonly struct LoadedAssemblyInfo(Assembly assembly, AssemblyIdentity identity, string? location)
	{
		public readonly Assembly Assembly = assembly;

		public readonly AssemblyIdentity Identity = identity;

		public readonly string? Location = location;

		public bool IsDefault => Assembly == null;

		private string GetDebuggerDisplay()
		{
			if (!IsDefault)
			{
				return Identity.GetDisplayName() + ((Location != null) ? (" @ " + Location) : "");
			}
			return "uninitialized";
		}
	}

	private readonly AssemblyLoaderImpl _runtimeAssemblyLoader;

	private readonly MetadataShadowCopyProvider? _shadowCopyProvider;

	private readonly object _referencesLock = new object();

	private readonly Dictionary<Assembly, LoadedAssembly> _assembliesLoadedFromLocation;

	private readonly Dictionary<string, AssemblyAndLocation> _assembliesLoadedFromLocationByFullPath;

	private readonly Dictionary<string, List<LoadedAssemblyInfo>> _loadedAssembliesBySimpleName;

	private readonly Dictionary<string, List<AssemblyIdentityAndLocation>> _dependenciesWithLocationBySimpleName;

	public InteractiveAssemblyLoader(MetadataShadowCopyProvider? shadowCopyProvider = null)
	{
		_shadowCopyProvider = shadowCopyProvider;
		_assembliesLoadedFromLocationByFullPath = new Dictionary<string, AssemblyAndLocation>();
		_assembliesLoadedFromLocation = new Dictionary<Assembly, LoadedAssembly>();
		_loadedAssembliesBySimpleName = new Dictionary<string, List<LoadedAssemblyInfo>>(AssemblyIdentityComparer.SimpleNameComparer);
		_dependenciesWithLocationBySimpleName = new Dictionary<string, List<AssemblyIdentityAndLocation>>();
		_runtimeAssemblyLoader = AssemblyLoaderImpl.Create(this);
	}

	public void Dispose()
	{
		_runtimeAssemblyLoader.Dispose();
	}

	internal Assembly LoadAssemblyFromStream(Stream peStream, Stream pdbStream)
	{
		Assembly assembly = _runtimeAssemblyLoader.LoadFromStream(peStream, pdbStream);
		RegisterDependency(assembly);
		return assembly;
	}

	private AssemblyAndLocation Load(string reference)
	{
		MetadataShadowCopy metadataShadowCopy = null;
		try
		{
			if (_shadowCopyProvider != null)
			{
				metadataShadowCopy = _shadowCopyProvider.GetMetadataShadowCopy(reference, MetadataImageKind.Assembly);
			}
			AssemblyAndLocation result = _runtimeAssemblyLoader.LoadFromPath((metadataShadowCopy != null) ? metadataShadowCopy.PrimaryModule.FullPath : reference);
			if (_shadowCopyProvider != null && result.GlobalAssemblyCache)
			{
				_shadowCopyProvider.SuppressShadowCopy(reference);
			}
			return result;
		}
		catch (FileNotFoundException)
		{
			return default(AssemblyAndLocation);
		}
		finally
		{
			metadataShadowCopy?.DisposeFileHandles();
		}
	}

	public void RegisterDependency(AssemblyIdentity dependency, string path)
	{
		if (dependency == null)
		{
			throw new ArgumentNullException("dependency");
		}
		if (!PathUtilities.IsAbsolute(path))
		{
			throw new ArgumentException(ScriptingResources.AbsolutePathExpected, "path");
		}
		lock (_referencesLock)
		{
			RegisterDependencyNoLock(new AssemblyIdentityAndLocation(dependency, path));
		}
	}

	public void RegisterDependency(Assembly dependency)
	{
		if (dependency == null)
		{
			throw new ArgumentNullException("dependency");
		}
		lock (_referencesLock)
		{
			RegisterLoadedAssemblySimpleNameNoLock(dependency, null);
		}
	}

	private void RegisterLoadedAssemblySimpleNameNoLock(Assembly assembly, string? location)
	{
		AssemblyIdentity assemblyIdentity = AssemblyIdentity.FromAssemblyDefinition(assembly);
		LoadedAssemblyInfo item = new LoadedAssemblyInfo(assembly, assemblyIdentity, location);
		if (_loadedAssembliesBySimpleName.TryGetValue(assemblyIdentity.Name, out List<LoadedAssemblyInfo> value))
		{
			value.Add(item);
			return;
		}
		_loadedAssembliesBySimpleName.Add(assemblyIdentity.Name, new List<LoadedAssemblyInfo> { item });
	}

	private void RegisterDependencyNoLock(AssemblyIdentityAndLocation dependency)
	{
		string name = dependency.Identity.Name;
		if (_dependenciesWithLocationBySimpleName.TryGetValue(name, out List<AssemblyIdentityAndLocation> value))
		{
			value.Add(dependency);
			return;
		}
		_dependenciesWithLocationBySimpleName.Add(name, new List<AssemblyIdentityAndLocation> { dependency });
	}

	internal Assembly? ResolveAssembly(string assemblyDisplayName, Assembly? requestingAssembly)
	{
		if (!AssemblyIdentity.TryParseDisplayName(assemblyDisplayName, out AssemblyIdentity identity))
		{
			return null;
		}
		string loadDirectory;
		lock (_referencesLock)
		{
			loadDirectory = ((!(requestingAssembly != null) || !_assembliesLoadedFromLocation.TryGetValue(requestingAssembly, out LoadedAssembly value)) ? null : Path.GetDirectoryName(value.OriginalPath));
		}
		return ResolveAssembly(identity, loadDirectory);
	}

	internal Assembly? ResolveAssembly(AssemblyIdentity identity, string? loadDirectory)
	{
		if (loadDirectory != null)
		{
			LoadedAssemblyInfo loadedAssemblyInfo = default(LoadedAssemblyInfo);
			LoadedAssemblyInfo loadedAssemblyInfo2 = default(LoadedAssemblyInfo);
			lock (_referencesLock)
			{
				Assembly assembly = TryGetAssemblyLoadedFromPath(identity, loadDirectory);
				if (assembly != null)
				{
					return assembly;
				}
				if (_loadedAssembliesBySimpleName.TryGetValue(identity.Name, out List<LoadedAssemblyInfo> value))
				{
					if (!identity.IsStrongName)
					{
						loadedAssemblyInfo = value.FirstOrDefault((LoadedAssemblyInfo info) => !info.Identity.IsStrongName);
					}
					loadedAssemblyInfo2 = value.FirstOrDefault((LoadedAssemblyInfo info) => AssemblyIdentityComparer.SimpleNameComparer.Equals(info.Identity.Name, identity.Name) && info.Identity.Version == identity.Version);
				}
			}
			string text = FindExistingAssemblyFile(identity.Name, loadDirectory);
			if (text != null)
			{
				if (loadedAssemblyInfo2.Assembly != null)
				{
					if (TryReadMvid(text, out var mvid) && loadedAssemblyInfo2.Assembly.ManifestModule.ModuleVersionId == mvid)
					{
						return loadedAssemblyInfo2.Assembly;
					}
					throw new InteractiveAssemblyLoaderException(string.Format(null, ScriptingResources.AssemblyAlreadyLoaded, identity.Name, identity.Version, loadedAssemblyInfo2.Location, text));
				}
				if (!loadedAssemblyInfo.IsDefault)
				{
					throw new InteractiveAssemblyLoaderException(string.Format(null, ScriptingResources.AssemblyAlreadyLoadedNotSigned, identity.Name, loadedAssemblyInfo.Location, text));
				}
				Assembly assembly = ShadowCopyAndLoadDependency(text).Assembly;
				if (assembly != null)
				{
					return assembly;
				}
			}
		}
		return GetOrLoadKnownAssembly(identity);
	}

	private static string? FindExistingAssemblyFile(string simpleName, string directory)
	{
		string text = Path.Combine(directory, simpleName);
		foreach (string assemblyExtension in RuntimeMetadataReferenceResolver.AssemblyExtensions)
		{
			string text2 = text + assemblyExtension;
			if (File.Exists(text2))
			{
				return text2;
			}
		}
		return null;
	}

	private Assembly? TryGetAssemblyLoadedFromPath(AssemblyIdentity identity, string directory)
	{
		string text = Path.Combine(directory, identity.Name);
		foreach (string assemblyExtension in RuntimeMetadataReferenceResolver.AssemblyExtensions)
		{
			if (_assembliesLoadedFromLocationByFullPath.TryGetValue(text + assemblyExtension, out var value) && identity.Equals(AssemblyIdentity.FromAssemblyDefinition(value.Assembly)))
			{
				return value.Assembly;
			}
		}
		return null;
	}

	private static bool TryReadMvid(string filePath, out Guid mvid)
	{
		try
		{
			using FileStream peStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
			using PEReader peReader = new PEReader(peStream);
			MetadataReader metadataReader = peReader.GetMetadataReader();
			mvid = metadataReader.GetGuid(metadataReader.GetModuleDefinition().Mvid);
			return true;
		}
		catch
		{
			mvid = default(Guid);
			return false;
		}
	}

	private Assembly? GetOrLoadKnownAssembly(AssemblyIdentity identity)
	{
		Assembly assembly = null;
		string text = null;
		lock (_referencesLock)
		{
			if (_loadedAssembliesBySimpleName.TryGetValue(identity.Name, out List<LoadedAssemblyInfo> value))
			{
				assembly = FindHighestVersionOrFirstMatchingIdentity(identity, value);
				if (assembly != null)
				{
					return assembly;
				}
			}
			if (_dependenciesWithLocationBySimpleName.TryGetValue(identity.Name, out List<AssemblyIdentityAndLocation> value2))
			{
				AssemblyIdentityAndLocation assemblyIdentityAndLocation = FindHighestVersionOrFirstMatchingIdentity(identity, value2);
				if (assemblyIdentityAndLocation.Identity != null)
				{
					text = assemblyIdentityAndLocation.Location;
					if (_assembliesLoadedFromLocationByFullPath.TryGetValue(text, out var value3))
					{
						return value3.Assembly;
					}
				}
			}
		}
		if (text != null)
		{
			assembly = ShadowCopyAndLoadDependency(text).Assembly;
		}
		return assembly;
	}

	private AssemblyAndLocation ShadowCopyAndLoadDependency(string originalPath)
	{
		AssemblyAndLocation assemblyAndLocation = Load(originalPath);
		if (assemblyAndLocation.IsDefault)
		{
			return default(AssemblyAndLocation);
		}
		lock (_referencesLock)
		{
			_assembliesLoadedFromLocationByFullPath[originalPath] = assemblyAndLocation;
			if (_assembliesLoadedFromLocation.TryGetValue(assemblyAndLocation.Assembly, out LoadedAssembly _))
			{
				return assemblyAndLocation;
			}
			_assembliesLoadedFromLocation.Add(assemblyAndLocation.Assembly, new LoadedAssembly
			{
				OriginalPath = (assemblyAndLocation.GlobalAssemblyCache ? assemblyAndLocation.Location : originalPath)
			});
			RegisterLoadedAssemblySimpleNameNoLock(assemblyAndLocation.Assembly, assemblyAndLocation.Location);
			return assemblyAndLocation;
		}
	}

	private static Assembly? FindHighestVersionOrFirstMatchingIdentity(AssemblyIdentity identity, IEnumerable<LoadedAssemblyInfo> infos)
	{
		Assembly assembly = null;
		Version version = null;
		foreach (LoadedAssemblyInfo info in infos)
		{
			if (DesktopAssemblyIdentityComparer.Default.ReferenceMatchesDefinition(identity, info.Identity) && (assembly == null || version < info.Identity.Version))
			{
				assembly = info.Assembly;
				version = info.Identity.Version;
			}
		}
		return assembly;
	}

	private static AssemblyIdentityAndLocation FindHighestVersionOrFirstMatchingIdentity(AssemblyIdentity identity, IEnumerable<AssemblyIdentityAndLocation> assemblies)
	{
		AssemblyIdentityAndLocation result = default(AssemblyIdentityAndLocation);
		foreach (AssemblyIdentityAndLocation assembly in assemblies)
		{
			if (DesktopAssemblyIdentityComparer.Default.ReferenceMatchesDefinition(identity, assembly.Identity) && (result.Identity == null || result.Identity.Version < assembly.Identity.Version))
			{
				result = assembly;
			}
		}
		return result;
	}
}
