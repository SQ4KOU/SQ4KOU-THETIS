using System;
using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

internal sealed class AdditionalSourcesCollection
{
	private readonly ArrayBuilder<GeneratedSourceText> _sourcesAdded;

	private readonly string _fileExtension;

	private const StringComparison _hintNameComparison = StringComparison.OrdinalIgnoreCase;

	private static readonly StringComparer s_hintNameComparer = StringComparer.OrdinalIgnoreCase;

	private static readonly Regex s_invalidSegmentPattern = new Regex("(\\.{1,2}|/|^| )/", RegexOptions.Compiled);

	internal AdditionalSourcesCollection(string fileExtension)
	{
		_sourcesAdded = ArrayBuilder<GeneratedSourceText>.GetInstance();
		_fileExtension = fileExtension;
	}

	public void Add(string hintName, SourceText source)
	{
		if (string.IsNullOrWhiteSpace(hintName))
		{
			throw new ArgumentNullException("hintName");
		}
		for (int i = 0; i < hintName.Length; i++)
		{
			char c = hintName[i];
			if (!UnicodeCharacterUtilities.IsIdentifierPartCharacter(c) && c != '.' && c != ',' && c != '-' && c != '+' && c != '`' && c != '_' && c != ' ' && c != '(' && c != ')' && c != '[' && c != ']' && c != '{' && c != '}' && c != '/' && c != '\\')
			{
				throw new ArgumentException(string.Format(CodeAnalysisResources.HintNameInvalidChar, hintName, c, i), "hintName");
			}
		}
		hintName = hintName.Replace('\\', '/');
		Match match = s_invalidSegmentPattern.Match(hintName);
		if (match != null && match.Success)
		{
			throw new ArgumentException(string.Format(CodeAnalysisResources.HintNameInvalidSegment, hintName, match.Value, match.Index), "hintName");
		}
		hintName = AppendExtensionIfRequired(hintName);
		if (Contains(hintName))
		{
			throw new ArgumentException(string.Format(CodeAnalysisResources.HintNameUniquePerGenerator, hintName), "hintName");
		}
		if (source.Encoding == null)
		{
			throw new ArgumentException(string.Format(CodeAnalysisResources.SourceTextRequiresEncoding, hintName), "source");
		}
		_sourcesAdded.Add(new GeneratedSourceText(hintName, source));
	}

	public void RemoveSource(string hintName)
	{
		hintName = AppendExtensionIfRequired(hintName);
		for (int i = 0; i < _sourcesAdded.Count; i++)
		{
			if (s_hintNameComparer.Equals(_sourcesAdded[i].HintName, hintName))
			{
				_sourcesAdded.RemoveAt(i);
				break;
			}
		}
	}

	public bool Contains(string hintName)
	{
		hintName = AppendExtensionIfRequired(hintName);
		for (int i = 0; i < _sourcesAdded.Count; i++)
		{
			if (s_hintNameComparer.Equals(_sourcesAdded[i].HintName, hintName))
			{
				return true;
			}
		}
		return false;
	}

	public void CopyTo(AdditionalSourcesCollection asc)
	{
		if (asc._sourcesAdded.Count == 0)
		{
			asc._sourcesAdded.AddRange(_sourcesAdded);
			return;
		}
		foreach (GeneratedSourceText item in _sourcesAdded)
		{
			if (asc.Contains(item.HintName))
			{
				throw new ArgumentException(string.Format(CodeAnalysisResources.HintNameUniquePerGenerator, item.HintName), "hintName");
			}
			asc._sourcesAdded.Add(item);
		}
	}

	internal ImmutableArray<GeneratedSourceText> ToImmutableAndFree()
	{
		return _sourcesAdded.ToImmutableAndFree();
	}

	internal ImmutableArray<GeneratedSourceText> ToImmutable()
	{
		return _sourcesAdded.ToImmutable();
	}

	internal void Free()
	{
		_sourcesAdded.Free();
	}

	private string AppendExtensionIfRequired(string hintName)
	{
		if (!hintName.EndsWith(_fileExtension, StringComparison.OrdinalIgnoreCase))
		{
			hintName += _fileExtension;
		}
		return hintName;
	}
}
