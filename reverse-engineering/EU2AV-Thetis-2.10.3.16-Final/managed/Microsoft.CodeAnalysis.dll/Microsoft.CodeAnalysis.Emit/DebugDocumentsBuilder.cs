using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Cci;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Emit;

internal sealed class DebugDocumentsBuilder
{
	private readonly ConcurrentDictionary<string, DebugSourceDocument> _debugDocuments;

	private readonly ConcurrentCache<(string, string?), string> _normalizedPathsCache;

	private readonly SourceReferenceResolver? _resolver;

	internal int DebugDocumentCount => _debugDocuments.Count;

	internal IReadOnlyDictionary<string, DebugSourceDocument> DebugDocuments => _debugDocuments;

	public DebugDocumentsBuilder(SourceReferenceResolver? resolver, bool isDocumentNameCaseSensitive)
	{
		_resolver = resolver;
		_debugDocuments = new ConcurrentDictionary<string, DebugSourceDocument>(isDocumentNameCaseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase);
		_normalizedPathsCache = new ConcurrentCache<(string, string), string>(16);
	}

	internal void AddDebugDocument(DebugSourceDocument document)
	{
		_debugDocuments.Add(document.Location, document);
	}

	internal DebugSourceDocument? TryGetDebugDocument(string path, string? basePath)
	{
		return TryGetDebugDocumentForNormalizedPath(NormalizeDebugDocumentPath(path, basePath));
	}

	internal DebugSourceDocument? TryGetDebugDocumentForNormalizedPath(string normalizedPath)
	{
		_debugDocuments.TryGetValue(normalizedPath, out DebugSourceDocument value);
		return value;
	}

	internal DebugSourceDocument GetOrAddDebugDocument(string path, string basePath, Func<string, DebugSourceDocument> factory)
	{
		return _debugDocuments.GetOrAdd(NormalizeDebugDocumentPath(path, basePath), factory);
	}

	internal string NormalizeDebugDocumentPath(string path, string? basePath)
	{
		if (_resolver == null)
		{
			return path;
		}
		(string, string) key = (path, basePath);
		if (!_normalizedPathsCache.TryGetValue(key, out string value))
		{
			value = _resolver.NormalizePath(path, basePath) ?? path;
			_normalizedPathsCache.TryAdd(key, value);
		}
		return value;
	}
}
