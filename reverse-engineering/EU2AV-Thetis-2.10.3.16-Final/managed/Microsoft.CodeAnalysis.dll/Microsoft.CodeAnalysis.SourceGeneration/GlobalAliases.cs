using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.SourceGeneration;

internal sealed class GlobalAliases : IEquatable<GlobalAliases>
{
	public static readonly GlobalAliases Empty = new GlobalAliases(ImmutableArray<(string, string)>.Empty);

	public readonly ImmutableArray<(string aliasName, string symbolName)> AliasAndSymbolNames;

	private int _hashCode;

	private GlobalAliases(ImmutableArray<(string aliasName, string symbolName)> aliasAndSymbolNames)
	{
		AliasAndSymbolNames = aliasAndSymbolNames;
	}

	public static GlobalAliases Create(ImmutableArray<(string aliasName, string symbolName)> aliasAndSymbolNames)
	{
		if (!aliasAndSymbolNames.IsEmpty)
		{
			return new GlobalAliases(aliasAndSymbolNames);
		}
		return Empty;
	}

	public static GlobalAliases Create(ImmutableArray<GlobalAliases> aliasesArray)
	{
		if (aliasesArray.Length == 0)
		{
			return Empty;
		}
		if (aliasesArray.Length == 1)
		{
			return aliasesArray[0];
		}
		ArrayBuilder<(string, string)> instance = ArrayBuilder<(string, string)>.GetInstance(aliasesArray.Sum((GlobalAliases a) => a.AliasAndSymbolNames.Length));
		foreach (GlobalAliases item in aliasesArray)
		{
			instance.AddRange(item.AliasAndSymbolNames);
		}
		return Create(instance.ToImmutableAndFree());
	}

	public static GlobalAliases Concat(GlobalAliases ga1, GlobalAliases ga2)
	{
		if (ga1.AliasAndSymbolNames.Length == 0)
		{
			return ga2;
		}
		if (ga2.AliasAndSymbolNames.Length == 0)
		{
			return ga1;
		}
		return new GlobalAliases(ga1.AliasAndSymbolNames.Concat(ga2.AliasAndSymbolNames));
	}

	public override int GetHashCode()
	{
		if (_hashCode == 0)
		{
			int num = 0;
			foreach (var aliasAndSymbolName in AliasAndSymbolNames)
			{
				num = Hash.Combine(aliasAndSymbolName.GetHashCode(), num);
			}
			_hashCode = ((num == 0) ? 1 : num);
		}
		return _hashCode;
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as GlobalAliases);
	}

	public bool Equals(GlobalAliases? aliases)
	{
		if (aliases == null)
		{
			return false;
		}
		if (this == aliases)
		{
			return true;
		}
		if (AliasAndSymbolNames == aliases.AliasAndSymbolNames)
		{
			return true;
		}
		return AliasAndSymbolNames.AsSpan().SequenceEqual(aliases.AliasAndSymbolNames.AsSpan());
	}
}
