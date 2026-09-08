using System;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Scripting.Hosting;

internal sealed class RuntimeMetadataReferenceResolver : MetadataReferenceResolver, IEquatable<RuntimeMetadataReferenceResolver>
{
	private static readonly MetadataReferenceProperties s_resolvedMissingAssemblyReferenceProperties = MetadataReferenceProperties.Assembly.WithAliases(ImmutableArray.Create("<implicit>"));

	internal ImmutableDictionary<string, string> TrustedPlatformAssemblies;

	internal readonly RelativePathResolver PathResolver;

	internal readonly NuGetPackageResolver? PackageResolver;

	internal readonly GacFileResolver? GacFileResolver;

	private readonly Func<string, MetadataReferenceProperties, PortableExecutableReference> _createFromFileFunc;

	internal static ImmutableArray<string> AssemblyExtensions = ImmutableArray.Create(".dll", ".exe");

	private static readonly char[] s_directorySeparators = new char[2]
	{
		PathUtilities.DirectorySeparatorChar,
		'/'
	};

	public override bool ResolveMissingAssemblies => true;

	internal static string? GetDesktopFrameworkDirectory()
	{
		if (!Microsoft.CodeAnalysis.Scripting.Hosting.GacFileResolver.IsAvailable)
		{
			return null;
		}
		return PathUtilities.GetDirectoryName(typeof(object).GetTypeInfo().Assembly.ManifestModule.FullyQualifiedName);
	}

	internal static RuntimeMetadataReferenceResolver CreateCurrentPlatformResolver(ImmutableArray<string> searchPaths = default(ImmutableArray<string>), string? baseDirectory = null, Func<string, MetadataReferenceProperties, PortableExecutableReference>? createFromFileFunc = null)
	{
		object gacFileResolver;
		if (!Microsoft.CodeAnalysis.Scripting.Hosting.GacFileResolver.IsAvailable)
		{
			gacFileResolver = null;
		}
		else
		{
			CultureInfo currentCulture = CultureInfo.CurrentCulture;
			gacFileResolver = new GacFileResolver(default(ImmutableArray<ProcessorArchitecture>), currentCulture);
		}
		return new RuntimeMetadataReferenceResolver(searchPaths, baseDirectory, null, (GacFileResolver?)gacFileResolver, GetTrustedPlatformAssemblyPaths(), createFromFileFunc);
	}

	internal RuntimeMetadataReferenceResolver(ImmutableArray<string> searchPaths = default(ImmutableArray<string>), string? baseDirectory = null, NuGetPackageResolver? packageResolver = null, GacFileResolver? gacFileResolver = null, ImmutableArray<string> platformAssemblyPaths = default(ImmutableArray<string>), Func<string, MetadataReferenceProperties, PortableExecutableReference>? createFromFileFunc = null)
		: this(new RelativePathResolver(searchPaths.NullToEmpty(), baseDirectory), packageResolver, gacFileResolver, GetTrustedPlatformAssemblies(platformAssemblyPaths.NullToEmpty()), createFromFileFunc)
	{
	}

	internal RuntimeMetadataReferenceResolver(RelativePathResolver pathResolver, NuGetPackageResolver? packageResolver, GacFileResolver? gacFileResolver, ImmutableDictionary<string, string> trustedPlatformAssemblies, Func<string, MetadataReferenceProperties, PortableExecutableReference>? createFromfileFunc = null)
	{
		PathResolver = pathResolver;
		PackageResolver = packageResolver;
		GacFileResolver = gacFileResolver;
		_createFromFileFunc = createFromfileFunc ?? ((Func<string, MetadataReferenceProperties, PortableExecutableReference>)((string path, MetadataReferenceProperties properties) => Script.CreateFromFile(path, PEStreamOptions.PrefetchEntireImage, properties)));
		TrustedPlatformAssemblies = trustedPlatformAssemblies;
	}

	public override PortableExecutableReference? ResolveMissingAssembly(MetadataReference definition, AssemblyIdentity referenceIdentity)
	{
		if (GacFileResolver != null && referenceIdentity.IsStrongName)
		{
			string text = GacFileResolver.Resolve(referenceIdentity.GetDisplayName());
			if (text != null)
			{
				return CreateResolvedMissingReference(text);
			}
		}
		if (!TrustedPlatformAssemblies.IsEmpty)
		{
			PortableExecutableReference portableExecutableReference = ResolveTrustedPlatformAssembly(referenceIdentity.Name, s_resolvedMissingAssemblyReferenceProperties);
			if (portableExecutableReference != null)
			{
				return portableExecutableReference;
			}
		}
		string directoryName = PathUtilities.GetDirectoryName((definition as PortableExecutableReference)?.FilePath);
		if (directoryName != null)
		{
			string text2 = PathUtilities.CombinePathsUnchecked(directoryName, referenceIdentity.Name);
			foreach (string assemblyExtension in AssemblyExtensions)
			{
				string text3 = text2 + assemblyExtension;
				if (File.Exists(text3))
				{
					return CreateResolvedMissingReference(text3);
				}
			}
		}
		return null;
	}

	private PortableExecutableReference CreateFromFile(string filePath, MetadataReferenceProperties properties)
	{
		return _createFromFileFunc(filePath, properties);
	}

	private PortableExecutableReference CreateResolvedMissingReference(string fullPath)
	{
		return _createFromFileFunc(fullPath, s_resolvedMissingAssemblyReferenceProperties);
	}

	public override ImmutableArray<PortableExecutableReference> ResolveReference(string reference, string? baseFilePath, MetadataReferenceProperties properties)
	{
		if (NuGetPackageResolver.TryParsePackageReference(reference, out var name, out var version))
		{
			if (PackageResolver != null)
			{
				return PackageResolver.ResolveNuGetPackage(name, version).SelectAsArray((string path) => CreateFromFile(path, properties));
			}
		}
		else if (PathUtilities.IsFilePath(reference))
		{
			if (!TrustedPlatformAssemblies.IsEmpty && reference.IndexOfAny(s_directorySeparators) < 0)
			{
				PortableExecutableReference portableExecutableReference = ResolveTrustedPlatformAssembly(PathUtilities.GetFileName(reference, includeExtension: false), properties);
				if (portableExecutableReference != null)
				{
					return ImmutableArray.Create(portableExecutableReference);
				}
			}
			if (PathResolver != null)
			{
				string text = PathResolver.ResolvePath(reference, baseFilePath);
				if (text != null)
				{
					return ImmutableArray.Create(CreateFromFile(text, properties));
				}
			}
		}
		else
		{
			if (GacFileResolver != null)
			{
				string text2 = GacFileResolver.Resolve(reference);
				if (text2 != null)
				{
					return ImmutableArray.Create(CreateFromFile(text2, properties));
				}
			}
			if (!TrustedPlatformAssemblies.IsEmpty && AssemblyIdentity.TryParseDisplayName(reference, out AssemblyIdentity identity, out AssemblyIdentityParts _))
			{
				PortableExecutableReference portableExecutableReference2 = ResolveTrustedPlatformAssembly(identity.Name, properties);
				if (portableExecutableReference2 != null)
				{
					return ImmutableArray.Create(portableExecutableReference2);
				}
			}
		}
		return ImmutableArray<PortableExecutableReference>.Empty;
	}

	private PortableExecutableReference? ResolveTrustedPlatformAssembly(string name, MetadataReferenceProperties properties)
	{
		if (!TrustedPlatformAssemblies.TryGetValue(name, out string value) || !File.Exists(value))
		{
			return null;
		}
		return CreateFromFile(value, properties);
	}

	internal static ImmutableArray<string> GetTrustedPlatformAssemblyPaths()
	{
		return ((AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string)?.Split(new char[1] { Path.PathSeparator })).ToImmutableArrayOrEmpty();
	}

	internal static ImmutableDictionary<string, string> GetTrustedPlatformAssemblies(ImmutableArray<string> paths)
	{
		if (paths.IsEmpty)
		{
			return ImmutableDictionary<string, string>.Empty;
		}
		ImmutableDictionary<string, string>.Builder builder = ImmutableDictionary.CreateBuilder<string, string>(StringComparer.OrdinalIgnoreCase);
		foreach (string item in paths)
		{
			if (PathUtilities.GetExtension(item) == ".dll")
			{
				string text = PathUtilities.GetFileName(item, includeExtension: false);
				if (text.EndsWith(".ni", StringComparison.OrdinalIgnoreCase))
				{
					string text2 = text;
					int length = ".ni".Length;
					text = text2.Substring(0, text2.Length - length);
				}
				builder[text] = item;
			}
		}
		return builder.ToImmutable();
	}

	public override int GetHashCode()
	{
		return Hash.Combine(PathResolver, Hash.Combine(PackageResolver, Hash.Combine(GacFileResolver, RuntimeHelpers.GetHashCode(TrustedPlatformAssemblies))));
	}

	public bool Equals(RuntimeMetadataReferenceResolver? other)
	{
		if (this != other)
		{
			if (other != null && object.Equals(PathResolver, other.PathResolver) && object.Equals(PackageResolver, other.PackageResolver) && object.Equals(GacFileResolver, other.GacFileResolver))
			{
				return TrustedPlatformAssemblies == other.TrustedPlatformAssemblies;
			}
			return false;
		}
		return true;
	}

	public override bool Equals(object? other)
	{
		return Equals(other as RuntimeMetadataReferenceResolver);
	}

	internal RuntimeMetadataReferenceResolver WithRelativePathResolver(RelativePathResolver resolver)
	{
		if (!object.Equals(resolver, PathResolver))
		{
			return new RuntimeMetadataReferenceResolver(resolver, PackageResolver, GacFileResolver, TrustedPlatformAssemblies, _createFromFileFunc);
		}
		return this;
	}
}
