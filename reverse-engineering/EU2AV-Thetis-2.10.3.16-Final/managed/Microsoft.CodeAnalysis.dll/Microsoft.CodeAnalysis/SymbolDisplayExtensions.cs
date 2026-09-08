using System;
using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis;

public static class SymbolDisplayExtensions
{
	public static string ToDisplayString(this ImmutableArray<SymbolDisplayPart> parts)
	{
		if (parts.IsDefault)
		{
			throw new ArgumentException("parts");
		}
		if (parts.Length == 0)
		{
			return string.Empty;
		}
		if (parts.Length == 1)
		{
			return parts[0].ToString();
		}
		PooledStringBuilder instance = PooledStringBuilder.GetInstance();
		StringBuilder builder = instance.Builder;
		foreach (SymbolDisplayPart item in parts)
		{
			builder.Append(item.ToString());
		}
		return instance.ToStringAndFree();
	}

	internal static string ToDisplayString(this ArrayBuilder<SymbolDisplayPart> parts)
	{
		if (parts == null)
		{
			throw new ArgumentException("parts");
		}
		if (parts.Count == 0)
		{
			return string.Empty;
		}
		if (parts.Count == 1)
		{
			return parts[0].ToString();
		}
		PooledStringBuilder instance = PooledStringBuilder.GetInstance();
		StringBuilder builder = instance.Builder;
		foreach (SymbolDisplayPart part in parts)
		{
			builder.Append(part.ToString());
		}
		return instance.ToStringAndFree();
	}

	internal static bool IncludesOption(this SymbolDisplayCompilerInternalOptions options, SymbolDisplayCompilerInternalOptions flag)
	{
		return (options & flag) == flag;
	}

	internal static bool IncludesOption(this SymbolDisplayGenericsOptions options, SymbolDisplayGenericsOptions flag)
	{
		return (options & flag) == flag;
	}

	internal static bool IncludesOption(this SymbolDisplayMemberOptions options, SymbolDisplayMemberOptions flag)
	{
		return (options & flag) == flag;
	}

	internal static bool IncludesOption(this SymbolDisplayMiscellaneousOptions options, SymbolDisplayMiscellaneousOptions flag)
	{
		return (options & flag) == flag;
	}

	internal static bool IncludesOption(this SymbolDisplayParameterOptions options, SymbolDisplayParameterOptions flag)
	{
		return (options & flag) == flag;
	}

	internal static bool IncludesOption(this SymbolDisplayKindOptions options, SymbolDisplayKindOptions flag)
	{
		return (options & flag) == flag;
	}

	internal static bool IncludesOption(this SymbolDisplayLocalOptions options, SymbolDisplayLocalOptions flag)
	{
		return (options & flag) == flag;
	}
}
