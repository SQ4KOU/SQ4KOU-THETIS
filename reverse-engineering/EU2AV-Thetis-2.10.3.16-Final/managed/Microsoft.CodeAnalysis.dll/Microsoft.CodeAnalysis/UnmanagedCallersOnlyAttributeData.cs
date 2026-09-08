using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.Symbols;

namespace Microsoft.CodeAnalysis;

internal sealed class UnmanagedCallersOnlyAttributeData
{
	internal static readonly UnmanagedCallersOnlyAttributeData Uninitialized = new UnmanagedCallersOnlyAttributeData(ImmutableHashSet<INamedTypeSymbolInternal>.Empty);

	internal static readonly UnmanagedCallersOnlyAttributeData AttributePresentDataNotBound = new UnmanagedCallersOnlyAttributeData(ImmutableHashSet<INamedTypeSymbolInternal>.Empty);

	private static readonly UnmanagedCallersOnlyAttributeData PlatformDefault = new UnmanagedCallersOnlyAttributeData(ImmutableHashSet<INamedTypeSymbolInternal>.Empty);

	public const string CallConvsPropertyName = "CallConvs";

	public readonly ImmutableHashSet<INamedTypeSymbolInternal> CallingConventionTypes;

	internal static UnmanagedCallersOnlyAttributeData Create(ImmutableHashSet<INamedTypeSymbolInternal>? callingConventionTypes)
	{
		if (callingConventionTypes == null || callingConventionTypes.IsEmpty)
		{
			return PlatformDefault;
		}
		return new UnmanagedCallersOnlyAttributeData(callingConventionTypes);
	}

	private UnmanagedCallersOnlyAttributeData(ImmutableHashSet<INamedTypeSymbolInternal> callingConventionTypes)
	{
		CallingConventionTypes = callingConventionTypes;
	}

	internal static bool IsCallConvsTypedConstant(string key, bool isField, in TypedConstant value)
	{
		if (isField && key == "CallConvs" && value.Kind == TypedConstantKind.Array)
		{
			if (!value.Values.IsDefaultOrEmpty)
			{
				return value.Values.All((TypedConstant v) => v.Kind == TypedConstantKind.Type);
			}
			return true;
		}
		return false;
	}
}
