using System;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

internal sealed class ExternalFileLocation : Location, IEquatable<ExternalFileLocation?>
{
	private readonly TextSpan _sourceSpan;

	private readonly FileLinePositionSpan _lineSpan;

	private readonly FileLinePositionSpan _mappedLineSpan;

	public override TextSpan SourceSpan => _sourceSpan;

	public override LocationKind Kind => LocationKind.ExternalFile;

	internal ExternalFileLocation(string filePath, TextSpan sourceSpan, LinePositionSpan lineSpan)
	{
		_sourceSpan = sourceSpan;
		_lineSpan = new FileLinePositionSpan(filePath, lineSpan);
		_mappedLineSpan = _lineSpan;
	}

	internal ExternalFileLocation(string filePath, TextSpan sourceSpan, LinePositionSpan lineSpan, string mappedFilePath, LinePositionSpan mappedLineSpan)
	{
		_sourceSpan = sourceSpan;
		_lineSpan = new FileLinePositionSpan(filePath, lineSpan);
		_mappedLineSpan = new FileLinePositionSpan(mappedFilePath, mappedLineSpan, hasMappedPath: true);
	}

	public override FileLinePositionSpan GetLineSpan()
	{
		return _lineSpan;
	}

	public override FileLinePositionSpan GetMappedLineSpan()
	{
		return _mappedLineSpan;
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as ExternalFileLocation);
	}

	public bool Equals(ExternalFileLocation? obj)
	{
		if ((object)obj == this)
		{
			return true;
		}
		if (obj != null && _sourceSpan == obj._sourceSpan && _lineSpan.Equals(obj._lineSpan))
		{
			return _mappedLineSpan.Equals(obj._mappedLineSpan);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Hash.Combine(_lineSpan.GetHashCode(), Hash.Combine(_mappedLineSpan.GetHashCode(), _sourceSpan.GetHashCode()));
	}
}
