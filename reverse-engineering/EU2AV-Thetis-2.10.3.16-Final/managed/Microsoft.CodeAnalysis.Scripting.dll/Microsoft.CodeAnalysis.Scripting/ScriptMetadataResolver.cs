using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Scripting.Hosting;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Scripting;

public sealed class ScriptMetadataResolver : MetadataReferenceResolver, IEquatable<ScriptMetadataResolver>
{
	private readonly RuntimeMetadataReferenceResolver _resolver;

	public static ScriptMetadataResolver Default { get; } = new ScriptMetadataResolver(RuntimeMetadataReferenceResolver.CreateCurrentPlatformResolver(ImmutableArray<string>.Empty));

	public ImmutableArray<string> SearchPaths => _resolver.PathResolver.SearchPaths;

	public string? BaseDirectory => _resolver.PathResolver.BaseDirectory;

	public override bool ResolveMissingAssemblies => _resolver.ResolveMissingAssemblies;

	internal ScriptMetadataResolver(RuntimeMetadataReferenceResolver resolver)
	{
		_resolver = resolver;
	}

	public ScriptMetadataResolver WithSearchPaths(params string[] searchPaths)
	{
		return WithSearchPaths(searchPaths.AsImmutableOrEmpty());
	}

	public ScriptMetadataResolver WithSearchPaths(IEnumerable<string> searchPaths)
	{
		return WithSearchPaths(searchPaths.AsImmutableOrEmpty());
	}

	public ScriptMetadataResolver WithSearchPaths(ImmutableArray<string> searchPaths)
	{
		if (SearchPaths == searchPaths)
		{
			return this;
		}
		return new ScriptMetadataResolver(_resolver.WithRelativePathResolver(_resolver.PathResolver.WithSearchPaths(ParameterValidationHelpers.ToImmutableArrayChecked(searchPaths, "searchPaths"))));
	}

	public ScriptMetadataResolver WithBaseDirectory(string? baseDirectory)
	{
		if (BaseDirectory == baseDirectory)
		{
			return this;
		}
		if (baseDirectory != null)
		{
			CompilerPathUtilities.RequireAbsolutePath(baseDirectory, "baseDirectory");
		}
		return new ScriptMetadataResolver(_resolver.WithRelativePathResolver(_resolver.PathResolver.WithBaseDirectory(baseDirectory)));
	}

	public override PortableExecutableReference? ResolveMissingAssembly(MetadataReference definition, AssemblyIdentity referenceIdentity)
	{
		return _resolver.ResolveMissingAssembly(definition, referenceIdentity);
	}

	public override ImmutableArray<PortableExecutableReference> ResolveReference(string reference, string? baseFilePath, MetadataReferenceProperties properties)
	{
		return _resolver.ResolveReference(reference, baseFilePath, properties);
	}

	public bool Equals(ScriptMetadataResolver? other)
	{
		return _resolver.Equals(other);
	}

	public override bool Equals(object? other)
	{
		return Equals(other as ScriptMetadataResolver);
	}

	public override int GetHashCode()
	{
		return _resolver.GetHashCode();
	}
}
