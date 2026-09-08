using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Reflection.PortableExecutable;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

public abstract class MetadataReference
{
	public MetadataReferenceProperties Properties { get; }

	public virtual string? Display => null;

	internal virtual bool IsUnresolved => false;

	protected MetadataReference(MetadataReferenceProperties properties)
	{
		Properties = properties;
	}

	public MetadataReference WithAliases(IEnumerable<string> aliases)
	{
		return WithAliases(ImmutableArray.CreateRange(aliases));
	}

	public MetadataReference WithEmbedInteropTypes(bool value)
	{
		return WithProperties(Properties.WithEmbedInteropTypes(value));
	}

	public MetadataReference WithAliases(ImmutableArray<string> aliases)
	{
		return WithProperties(Properties.WithAliases(aliases));
	}

	public MetadataReference WithProperties(MetadataReferenceProperties properties)
	{
		if (properties == Properties)
		{
			return this;
		}
		return WithPropertiesImplReturningMetadataReference(properties);
	}

	internal abstract MetadataReference WithPropertiesImplReturningMetadataReference(MetadataReferenceProperties properties);

	public static PortableExecutableReference CreateFromImage(ImmutableArray<byte> peImage, MetadataReferenceProperties properties = default(MetadataReferenceProperties), DocumentationProvider? documentation = null, string? filePath = null)
	{
		Metadata metadata = ((properties.Kind != MetadataImageKind.Module) ? ((Metadata)AssemblyMetadata.CreateFromImage(peImage)) : ((Metadata)ModuleMetadata.CreateFromImage(peImage)));
		return new MetadataImageReference(metadata, properties, documentation, filePath, null);
	}

	public static PortableExecutableReference CreateFromImage(IEnumerable<byte> peImage, MetadataReferenceProperties properties = default(MetadataReferenceProperties), DocumentationProvider? documentation = null, string? filePath = null)
	{
		Metadata metadata = ((properties.Kind != MetadataImageKind.Module) ? ((Metadata)AssemblyMetadata.CreateFromImage(peImage)) : ((Metadata)ModuleMetadata.CreateFromImage(peImage)));
		return new MetadataImageReference(metadata, properties, documentation, filePath, null);
	}

	public static PortableExecutableReference CreateFromStream(Stream peStream, MetadataReferenceProperties properties = default(MetadataReferenceProperties), DocumentationProvider? documentation = null, string? filePath = null)
	{
		Metadata metadata = ((properties.Kind != MetadataImageKind.Module) ? ((Metadata)AssemblyMetadata.CreateFromStream(peStream, PEStreamOptions.PrefetchEntireImage)) : ((Metadata)ModuleMetadata.CreateFromStream(peStream, PEStreamOptions.PrefetchEntireImage)));
		return new MetadataImageReference(metadata, properties, documentation, filePath, null);
	}

	public static PortableExecutableReference CreateFromFile(string path, MetadataReferenceProperties properties = default(MetadataReferenceProperties), DocumentationProvider? documentation = null)
	{
		return CreateFromFile(StandardFileSystem.Instance.OpenFileWithNormalizedException(path, FileMode.Open, FileAccess.Read, FileShare.Read), path, PEStreamOptions.PrefetchEntireImage, properties, documentation);
	}

	internal static MetadataImageReference CreateFromFile(string path, PEStreamOptions options, MetadataReferenceProperties properties, DocumentationProvider? documentation = null)
	{
		return CreateFromFile(StandardFileSystem.Instance.OpenFileWithNormalizedException(path, FileMode.Open, FileAccess.Read, FileShare.Read), path, options, properties, documentation);
	}

	internal static MetadataImageReference CreateFromFile(Stream peStream, string path, PEStreamOptions options, MetadataReferenceProperties properties, DocumentationProvider? documentation = null)
	{
		ModuleMetadata moduleMetadata = ModuleMetadata.CreateFromStream(peStream, options);
		if (properties.Kind == MetadataImageKind.Module)
		{
			return new MetadataImageReference(moduleMetadata, properties, documentation, path, null);
		}
		return new MetadataImageReference(AssemblyMetadata.CreateFromFile(moduleMetadata, path), properties, documentation, path, null);
	}

	[Obsolete("Use CreateFromFile(assembly.Location) instead", true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static MetadataReference CreateFromAssembly(Assembly assembly)
	{
		return CreateFromAssemblyInternal(assembly);
	}

	internal static MetadataImageReference CreateFromAssemblyInternal(Assembly assembly)
	{
		return CreateFromAssemblyInternal(assembly, default(MetadataReferenceProperties));
	}

	[Obsolete("Use CreateFromFile(assembly.Location) instead", true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static MetadataReference CreateFromAssembly(Assembly assembly, MetadataReferenceProperties properties, DocumentationProvider? documentation = null)
	{
		return CreateFromAssemblyInternal(assembly, properties, documentation);
	}

	internal static string GetAssemblyFilePath(Assembly assembly, MetadataReferenceProperties properties)
	{
		if (assembly == null)
		{
			throw new ArgumentNullException("assembly");
		}
		if (assembly.IsDynamic)
		{
			throw new NotSupportedException(CodeAnalysisResources.CantCreateReferenceToDynamicAssembly);
		}
		if (properties.Kind != MetadataImageKind.Assembly)
		{
			throw new ArgumentException(CodeAnalysisResources.CantCreateModuleReferenceToAssembly, "properties");
		}
		string location = assembly.Location;
		if (string.IsNullOrEmpty(location))
		{
			throw new NotSupportedException(CodeAnalysisResources.CantCreateReferenceToAssemblyWithoutLocation);
		}
		return location;
	}

	internal static MetadataImageReference CreateFromAssemblyInternal(Assembly assembly, MetadataReferenceProperties properties, DocumentationProvider? documentation = null)
	{
		string assemblyFilePath = GetAssemblyFilePath(assembly, properties);
		return CreateFromFile(StandardFileSystem.Instance.OpenFileWithNormalizedException(assemblyFilePath, FileMode.Open, FileAccess.Read, FileShare.Read), assemblyFilePath, PEStreamOptions.Default, properties, documentation);
	}

	internal static bool HasMetadata(Assembly assembly)
	{
		if (!assembly.IsDynamic)
		{
			return !string.IsNullOrEmpty(assembly.Location);
		}
		return false;
	}
}
