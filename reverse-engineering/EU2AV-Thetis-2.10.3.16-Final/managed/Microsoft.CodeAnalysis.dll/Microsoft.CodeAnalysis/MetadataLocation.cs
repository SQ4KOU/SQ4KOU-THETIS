using System;
using Microsoft.CodeAnalysis.Symbols;

namespace Microsoft.CodeAnalysis;

internal sealed class MetadataLocation : Location, IEquatable<MetadataLocation?>
{
	private readonly IModuleSymbolInternal _module;

	public override LocationKind Kind => LocationKind.MetadataFile;

	internal override IModuleSymbolInternal MetadataModuleInternal => _module;

	internal MetadataLocation(IModuleSymbolInternal module)
	{
		_module = module;
	}

	public override int GetHashCode()
	{
		return _module.GetHashCode();
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as MetadataLocation);
	}

	public bool Equals(MetadataLocation? other)
	{
		if ((object)other != null)
		{
			return other._module == _module;
		}
		return false;
	}
}
